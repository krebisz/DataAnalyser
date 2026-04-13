using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformDataResolutionCoordinator
{
    private readonly ITransformDataPanelController _controller;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache;
    private readonly VNextSeriesLoadCoordinator _vnextCoordinator;
    private readonly MainWindowViewModel _viewModel;

    public TransformDataResolutionCoordinator(
        ITransformDataPanelController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        MetricSeriesSelectionCache selectionCache,
        VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _selectionCache = selectionCache ?? throw new ArgumentNullException(nameof(selectionCache));
        _vnextCoordinator = vnextCoordinator ?? new VNextSeriesLoadCoordinator(metricSelectionService);
    }

    public TransformSelectionResolution ResolveSelections(ChartDataContext context, bool isSelectionPendingLoad)
    {
        var primarySelection = ResolvePrimarySelection(context, isSelectionPendingLoad);
        var secondarySelection = ResolveSecondarySelection(context, isSelectionPendingLoad);
        var hasAvailableSecondaryInput = HasSecondaryData(context) || secondarySelection != null;

        return new TransformSelectionResolution(primarySelection, secondarySelection, hasAvailableSecondaryInput);
    }

    public async Task<TransformResolutionResult> ResolveAsync(ChartDataContext context, bool isSelectionPendingLoad)
    {
        var selection = ResolveSelections(context, isSelectionPendingLoad);
        var primaryData = await ResolveDataAsync(context, selection.PrimarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (selection.SecondarySelection != null)
            secondaryData = await ResolveDataAsync(context, selection.SecondarySelection);

        var transformContext = BuildTransformContext(context, selection.PrimarySelection, selection.SecondarySelection, primaryData, secondaryData);
        return new TransformResolutionResult(selection, primaryData, secondaryData, transformContext);
    }

    public static bool CanRenderPrimarySelection(ChartDataContext context)
    {
        return context.Data1 != null && context.Data1.Any();
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDataAsync(ChartDataContext context, MetricSeriesSelection? selectedSeries)
    {
        if (context.Data1 == null)
            return null;

        if (selectedSeries == null)
            return context.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype))
            return context.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, context.SecondaryMetricType, context.SecondarySubtype))
            return context.Data2 ?? context.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return context.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, context.From, context.To, tableName);
        if (_selectionCache.TryGetData(cacheKey, out var cached))
            return cached;

        return await LoadFreshTransformDataAsync(selectedSeries, context.From, context.To, tableName, cacheKey);
    }

    private async Task<IReadOnlyList<MetricData>?> LoadFreshTransformDataAsync(
        MetricSeriesSelection selectedSeries, DateTime from, DateTime to, string tableName, string cacheKey)
    {
        var vnextResult = await _vnextCoordinator.LoadAsync(selectedSeries, from, to, tableName, ChartProgramKind.Transform);
        if (vnextResult.Success && vnextResult.Data != null)
        {
            _viewModel.ChartState.LastTransformLoadRuntime = new LoadRuntimeState(
                EvidenceRuntimePath.VNextTransform,
                vnextResult.RequestSignature ?? string.Empty,
                vnextResult.SnapshotSignature,
                vnextResult.ProgramKind,
                vnextResult.ProgramSourceSignature,
                null, null, false);

            var data = vnextResult.Data is List<MetricData> list ? list : vnextResult.Data.ToList();
            _selectionCache.SetData(cacheKey, data);
            return data;
        }

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, from, to, tableName);
        var legacyData = primaryData.ToList();
        _selectionCache.SetData(cacheKey, legacyData);

        _viewModel.ChartState.LastTransformLoadRuntime = new LoadRuntimeState(
            EvidenceRuntimePath.Legacy,
            vnextResult.RequestSignature ?? string.Empty,
            null, null, null, null,
            vnextResult.FailureReason, false);

        return legacyData;
    }

    private MetricSeriesSelection? ResolvePrimarySelection(ChartDataContext context, bool isSelectionPendingLoad)
    {
        if (!isSelectionPendingLoad && _controller.TransformPrimarySubtypeCombo != null)
        {
            var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformPrimarySeries != null)
            return _viewModel.ChartState.SelectedTransformPrimarySeries;

        var metricType = context.PrimaryMetricType ?? context.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, context.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSecondarySelection(ChartDataContext context, bool isSelectionPendingLoad)
    {
        if (!isSelectionPendingLoad && _controller.TransformSecondarySubtypeCombo != null)
        {
            var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformSecondarySeries != null)
            return _viewModel.ChartState.SelectedTransformSecondarySeries;

        if (!HasSecondaryData(context))
            return null;

        var metricType = context.SecondaryMetricType ?? context.PrimaryMetricType ?? context.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, context.SecondarySubtype);
    }

    private static ChartDataContext BuildTransformContext(
        ChartDataContext context,
        MetricSeriesSelection? primarySelection,
        MetricSeriesSelection? secondarySelection,
        IReadOnlyList<MetricData>? primaryData,
        IReadOnlyList<MetricData>? secondaryData)
    {
        return new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            DisplayName1 = ResolveTransformDisplayName(context, primarySelection),
            DisplayName2 = ResolveTransformDisplayName(context, secondarySelection),
            MetricType = primarySelection?.MetricType ?? context.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? context.PrimaryMetricType,
            SecondaryMetricType = secondarySelection?.MetricType ?? context.SecondaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? context.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? context.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? context.DisplayPrimarySubtype,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? context.DisplaySecondarySubtype,
            From = context.From,
            To = context.To
        };
    }

    private static string ResolveTransformDisplayName(ChartDataContext context, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return context.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype))
            return context.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, context.SecondaryMetricType, context.SecondarySubtype))
            return context.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static bool HasSecondaryData(ChartDataContext context)
    {
        return TransformGridPresentationCoordinator.HasSecondaryData(context);
    }
}
