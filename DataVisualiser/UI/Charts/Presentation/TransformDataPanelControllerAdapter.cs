using DataVisualiser.UI.Charts.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Transforms;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class TransformDataPanelControllerAdapter : CartesianChartControllerAdapterBase<ITransformDataPanelController>, ITransformPanelControllerExtras
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly ITransformDataPanelController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly ITransformRenderingContract _transformRenderingContract;
    private readonly TransformComputationService _transformComputationService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isTransformSelectionPendingLoad;
    private bool _isUpdatingTransformSubtypeCombos;

    public TransformDataPanelControllerAdapter(ITransformDataPanelController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, ITransformRenderingContract transformRenderingContract, TransformComputationService? transformComputationService = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _transformRenderingContract = transformRenderingContract ?? throw new ArgumentNullException(nameof(transformRenderingContract));
        _transformComputationService = transformComputationService ?? new TransformComputationService();
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.Transform;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override async Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return;

        UpdateTransformSubtypeOptions();
        var (_, _, transformContext) = await ResolveTransformDataAsync(context);
        PopulateTransformGrids(transformContext);
        UpdateTransformComputeButtonState();
    }

    public override void Clear(ChartState state)
    {
        ClearTransformGrids(state);
        _transformRenderingContract.Clear(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _transformRenderingContract.ResetView(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _transformRenderingContract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, CreateRenderHost());
    }

    public override void UpdateSubtypeOptions()
    {
        UpdateTransformSubtypeOptions();
    }

    public void CompleteSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = false;
    }

    public void ResetSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = true;
        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

        if (_controller.TransformPrimarySubtypeCombo == null || _controller.TransformSecondarySubtypeCombo == null)
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformSubtypeSelectionCoordinator.ResetSelectionControls(_controller);
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void HandleVisibilityOnlyToggle(ChartDataContext? context)
    {
        if (!_viewModel.ChartState.IsTransformPanelVisible)
            return;

        if (context != null && ShouldRenderCharts(context))
            PopulateTransformGrids(context, false);

        UpdateTransformSubtypeOptions();
    }

    public void UpdateTransformSubtypeOptions()
    {
        if (!TransformSubtypeSelectionCoordinator.CanUpdateSubtypeOptions(_controller, _isTransformSelectionPendingLoad))
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformSubtypeSelectionCoordinator.ApplySubtypeOptions(
                _controller,
                _viewModel.ChartState,
                _viewModel.MetricState.SelectedSeries,
                SetBinaryTransformOperationsEnabled);
            UpdateTransformComputeButtonState();
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    public void UpdateTransformComputeButtonState()
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (_isTransformSelectionPendingLoad || ctx == null)
        {
            _controller.TransformComputeButton.IsEnabled = false;
            return;
        }

        if (!TryGetSelectedOperation(out var operationTag))
        {
            _controller.TransformComputeButton.IsEnabled = CanRenderPrimarySelection(ctx);
            return;
        }

        _controller.TransformComputeButton.IsEnabled = CanComputeTransformOperation(ctx, operationTag);
    }

    public string? GetSelectedOperationTag()
    {
        return _controller.TransformOperationCombo.SelectedItem is ComboBoxItem item ? item.Tag?.ToString() : null;
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleTransformPanel();
    }

    public void OnOperationChanged(object? sender, EventArgs e)
    {
        UpdateTransformComputeButtonState();
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
        _viewModel.SetTransformPrimarySeries(selection);

        UpdateTransformComputeButtonState();
        await RefreshTransformGridsFromSelectionAsync();
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
        _viewModel.SetTransformSecondarySeries(selection);

        UpdateTransformComputeButtonState();
        await RefreshTransformGridsFromSelectionAsync();
    }

    public async void OnComputeRequested(object? sender, EventArgs e)
    {
        if (_isTransformSelectionPendingLoad)
            return;

        if (_viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        using var _ = _beginUiBusyScope();
        if (!TryGetSelectedOperation(out var operationTag))
        {
            await RenderPrimarySelectionAsResult(ctx);
            return;
        }

        await ExecuteTransformOperation(ctx, operationTag);
    }

    private void PopulateTransformGrids(ChartDataContext ctx, bool resetResults = true)
    {
        var hasSecondary = TransformGridPresentationCoordinator.HasSecondaryData(ctx) && !string.IsNullOrEmpty(ctx.SecondarySubtype) && ctx.Data2 != null;
        var hasAvailableSecondaryInput = hasSecondary || ResolveSelectedTransformSecondarySeries(ctx) != null;

        TransformGridPresentationCoordinator.PopulateInputGrids(
            _controller,
            ctx,
            hasAvailableSecondaryInput,
            SetBinaryTransformOperationsEnabled,
            resetResults);
    }

    private void SetBinaryTransformOperationsEnabled(bool enabled)
    {
        TransformGridPresentationCoordinator.SetBinaryTransformOperationsEnabled(_controller, enabled);
    }

    private bool TryGetSelectedOperation(out string operationTag)
    {
        operationTag = string.Empty;
        if (_controller.TransformOperationCombo.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string tag)
            return false;

        operationTag = tag;
        return true;
    }

    private async Task ExecuteTransformOperation(ChartDataContext ctx, string operationTag)
    {
        var isUnary = IsUnaryTransformOperation(operationTag);
        var isBinary = IsBinaryTransformOperation(operationTag);

        var (primaryData, secondaryData, transformContext) = await ResolveTransformDataAsync(ctx);
        if (primaryData == null)
            return;

        if (isUnary)
            await ComputeUnaryTransform(primaryData, operationTag, transformContext);
        else if (isBinary && secondaryData != null && secondaryData.Any())
            await ComputeBinaryTransform(primaryData, secondaryData, operationTag, transformContext);
    }

    private async Task ComputeUnaryTransform(IEnumerable<MetricData> data, string operation, ChartDataContext transformContext)
    {
        var computation = _transformComputationService.ComputeUnaryTransform(data, operation);
        if (!computation.IsSuccess || computation.DataList.Count == 0)
            return;

        await RenderTransformResults(computation.DataList, computation.ComputedResults, operation, computation.MetricsList, transformContext);
    }

    private async Task RenderTransformResults(List<MetricData> dataList, List<double> results, string operation, List<IReadOnlyList<MetricData>> metrics, ChartDataContext transformContext, string? overrideLabel = null)
    {
        var resultData = TransformExpressionEvaluator.CreateTransformResultData(dataList, results);
        TransformGridPresentationCoordinator.PopulateResultGrid(_controller, resultData);

        if (resultData.Count == 0)
            return;

        TransformGridPresentationCoordinator.ShowResultPanels(_controller);
        await TransformChartPresentationCoordinator.RenderResultsAsync(
            _controller,
            _transformRenderingContract,
            CreateRenderHost(),
            dataList,
            results,
            operation,
            metrics,
            transformContext,
            overrideLabel);

        if (_controller is DataVisualiser.UI.Charts.Controllers.TransformDataPanelControllerV2 v2)
            v2.UpdateMinMaxLines();
    }

    private async Task ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation, ChartDataContext transformContext)
    {
        var computation = _transformComputationService.ComputeBinaryTransform(data1, data2, operation);
        if (!computation.IsSuccess || computation.DataList.Count == 0)
            return;

        await RenderTransformResults(computation.DataList, computation.ComputedResults, operation, computation.MetricsList, transformContext);
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveTransformDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedTransformPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedTransformSecondarySeries(ctx);

        var primaryData = await ResolveTransformDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (secondarySelection != null)
            secondaryData = await ResolveTransformDataAsync(ctx, secondarySelection);

        var transformContext = BuildTransformContext(ctx, primarySelection, secondarySelection, primaryData, secondaryData);

        return (primaryData, secondaryData, transformContext);
    }

    private async Task RenderPrimarySelectionAsResult(ChartDataContext ctx)
    {
        var (primaryData, _, transformContext) = await ResolveTransformDataAsync(ctx);
        if (primaryData == null)
            return;

        var dataList = primaryData.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        if (dataList.Count == 0)
            return;

        var results = dataList.Select(d => (double)d.Value!.Value).ToList();
        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                dataList
        };

        var label = string.IsNullOrWhiteSpace(transformContext.DisplayName1) ? "Primary Data" : transformContext.DisplayName1;
        await RenderTransformResults(dataList, results, string.Empty, metricsList, transformContext, label);
    }

    private async Task RefreshTransformGridsFromSelectionAsync()
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (ctx == null)
            return;

        var (_, _, transformContext) = await ResolveTransformDataAsync(ctx);
        PopulateTransformGrids(transformContext);
        UpdateTransformComputeButtonState();
    }

    private bool CanComputeTransformOperation(ChartDataContext ctx, string operationTag)
    {
        if (IsUnaryTransformOperation(operationTag))
            return CanRenderPrimarySelection(ctx);

        return IsBinaryTransformOperation(operationTag) && ResolveSelectedTransformSecondarySeries(ctx) != null;
    }

    private static bool CanRenderPrimarySelection(ChartDataContext ctx)
    {
        return ctx.Data1 != null && ctx.Data1.Any();
    }

    private static bool IsUnaryTransformOperation(string operationTag)
    {
        return operationTag == "Log" || operationTag == "Sqrt";
    }

    private static bool IsBinaryTransformOperation(string operationTag)
    {
        return operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";
    }

    private static ChartDataContext BuildTransformContext(ChartDataContext ctx, MetricSeriesSelection? primarySelection, MetricSeriesSelection? secondarySelection, IReadOnlyList<MetricData>? primaryData, IReadOnlyList<MetricData>? secondaryData)
    {
        return new ChartDataContext
        {
                Data1 = primaryData,
                Data2 = secondaryData,
                DisplayName1 = ResolveTransformDisplayName(ctx, primarySelection),
                DisplayName2 = ResolveTransformDisplayName(ctx, secondarySelection),
                MetricType = primarySelection?.MetricType ?? ctx.MetricType,
                PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
                SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
                PrimarySubtype = primarySelection?.Subtype,
                SecondarySubtype = secondarySelection?.Subtype,
                DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
                DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
                DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
                From = ctx.From,
                To = ctx.To
        };
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_selectionCache.TryGetData(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _selectionCache.SetData(cacheKey, data);
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedTransformPrimarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && _controller.TransformPrimarySubtypeCombo != null)
        {
            var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformPrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformPrimarySeries != null)
            return _viewModel.ChartState.SelectedTransformPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedTransformSecondarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && _controller.TransformSecondarySubtypeCombo != null)
        {
            var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.TransformSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformSecondarySeries != null)
            return _viewModel.ChartState.SelectedTransformSecondarySeries;

        // If no secondary data is loaded and no explicit secondary selection exists,
        // treat the transform as single-input rather than fabricating a second series.
        if (!HasSecondaryData(ctx))
            return null;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveTransformDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }


    private void ClearTransformGrids(ChartState state)
    {
        TransformGridPresentationCoordinator.ClearAllGrids(_controller);
    }

    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return TransformGridPresentationCoordinator.ShouldRenderCharts(ctx);
    }

    private static bool HasSecondaryData(ChartDataContext ctx)
    {
        return TransformGridPresentationCoordinator.HasSecondaryData(ctx);
    }

    private TransformChartRenderHost CreateRenderHost()
    {
        return new TransformChartRenderHost(
            _controller.ChartTransformResult,
            _viewModel.ChartState,
            ResetTransformAuxiliaryVisuals);
    }

    private void ResetTransformAuxiliaryVisuals()
    {
        if (_controller is DataVisualiser.UI.Charts.Controllers.TransformDataPanelControllerV2 v2)
            v2.ResetMinMaxLines();
    }

}
