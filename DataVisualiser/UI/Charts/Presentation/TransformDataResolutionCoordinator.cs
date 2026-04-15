using DataFileReader.Canonical;
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
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (data, _) = await VNextDataResolutionHelper.ResolveSeriesDataAsync(
            context, selectedSeries, _selectionCache, tableName, _vnextCoordinator,
            ChartProgramKind.Transform, EvidenceRuntimePath.VNextTransform,
            runtime => _viewModel.ChartState.SetFamilyRuntime(ChartProgramKind.Transform, runtime),
            async (sel, from, to, table) =>
            {
                var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(sel.MetricType, sel.QuerySubtype, null, from, to, table);
                return ((IReadOnlyList<MetricData>)primary.ToList(), (ICanonicalMetricSeries?)null);
            });
        return data;
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
