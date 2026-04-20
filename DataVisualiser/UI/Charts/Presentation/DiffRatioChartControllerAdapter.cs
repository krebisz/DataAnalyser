using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class DiffRatioChartControllerAdapter : CartesianChartControllerAdapterBase<IDiffRatioChartController>, IDiffRatioChartControllerExtras
{
    private const CartesianMetricChartRoute RenderingRoute = CartesianMetricChartRoute.DiffRatio;
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly IDiffRatioChartController _controller;
    private readonly ICartesianMetricChartRenderingContract _renderingContract;
    private readonly Func<ChartTooltipManager?> _getTooltipManager;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombos;

    public DiffRatioChartControllerAdapter(IDiffRatioChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, Func<ChartTooltipManager?> getTooltipManager, ICartesianMetricChartRenderingContract renderingContract)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getTooltipManager = getTooltipManager ?? throw new ArgumentNullException(nameof(getTooltipManager));
        _renderingContract = renderingContract ?? throw new ArgumentNullException(nameof(renderingContract));
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.DiffRatio;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => true;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderDiffRatioAsync(context);
    }

    public override void UpdateSubtypeOptions()
    {
        if (!CanUpdateDiffRatioSubtypeOptions())
            return;

        _isUpdatingSubtypeCombos = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            MetricSeriesSelectionAdapterHelper.PopulatePrimarySecondarySubtypeCombos(
                _controller.PrimarySubtypeCombo,
                _controller.SecondarySubtypeCombo,
                _controller.SecondarySubtypePanel,
                selectedSeries,
                _viewModel.ChartState.SelectedDiffRatioPrimarySeries,
                _viewModel.ChartState.SelectedDiffRatioSecondarySeries,
                selection => _viewModel.ChartState.SelectedDiffRatioPrimarySeries = selection,
                selection => _viewModel.ChartState.SelectedDiffRatioSecondarySeries = selection);
        }
        finally
        {
            _isUpdatingSubtypeCombos = false;
        }
    }

    public void UpdateOperationButton()
    {
        var isDifference = _viewModel.ChartState.IsDiffRatioDifferenceMode;

        var operationButton = _controller.OperationToggleButton;
        operationButton.Content = isDifference ? "/" : "-";
        operationButton.ToolTip = isDifference ? "Switch to Ratio (/)" : "Switch to Difference (-)";

        if (_controller.Chart.AxisY.Count > 0)
            _controller.Chart.AxisY[0].Title = isDifference ? "Difference" : "Ratio";
    }

    public override void Clear(ChartState state)
    {
        RenderingHostLifecycleAdapterHelper.Clear(RenderingRoute, CreateRenderHost, _renderingContract.Clear);
    }

    public override void ResetZoom()
    {
        RenderingHostLifecycleAdapterHelper.ResetView(RenderingRoute, CreateRenderHost, _renderingContract.ResetView);
    }

    public override bool HasSeries(ChartState state)
    {
        return RenderingHostLifecycleAdapterHelper.HasRenderableContent(RenderingRoute, CreateRenderHost, _renderingContract.HasRenderableContent);
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDiffRatio();
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        await MetricSeriesSelectionAdapterHelper.HandleSubtypeSelectionChangeAsync(
            _isInitializing(),
            _isUpdatingSubtypeCombos,
            _controller.PrimarySubtypeCombo,
            _viewModel.SetDiffRatioPrimarySeries,
            RenderDiffRatioFromSelectionAsync);
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        await MetricSeriesSelectionAdapterHelper.HandleSubtypeSelectionChangeAsync(
            _isInitializing(),
            _isUpdatingSubtypeCombos,
            _controller.SecondarySubtypeCombo,
            _viewModel.SetDiffRatioSecondarySeries,
            RenderDiffRatioFromSelectionAsync);
    }

    public async void OnOperationToggleRequested(object? sender, EventArgs e)
    {
        using var _ = _beginUiBusyScope();
        _viewModel.ToggleDiffRatioOperation();
        UpdateOperationButton();

        await RenderDiffRatioFromSelectionAsync();
    }

    private async Task RenderDiffRatioFromSelectionAsync()
    {
        await BinaryMetricChartContextHelper.RerenderIfVisibleAsync(
            _viewModel.ChartState.IsDiffRatioVisible,
            _viewModel.ChartState.LastContext,
            RenderDiffRatioAsync);
    }

    private async Task RenderDiffRatioAsync(ChartDataContext ctx)
    {
        var (primaryData, secondaryData, diffRatioContext) = await ResolveDiffRatioDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateDiffRatioPanelTitle(diffRatioContext);
        await _renderingContract.RenderAsync(
            new CartesianMetricChartRenderRequest(RenderingRoute, diffRatioContext),
            CreateRenderHost());
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveDiffRatioDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedDiffRatioPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedDiffRatioSecondarySeries(ctx);

        var (primaryData, primaryCms) = await ResolveDiffRatioSeriesAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;
        ICanonicalMetricSeries? secondaryCms = null;
        if (secondarySelection != null)
        {
            var resolvedSecondary = await ResolveDiffRatioSeriesAsync(ctx, secondarySelection);
            secondaryData = resolvedSecondary.Data;
            secondaryCms = resolvedSecondary.Cms;
        }

        var displayName1 = ResolveDiffRatioDisplayName(ctx, primarySelection);
        var displayName2 = ResolveDiffRatioDisplayName(ctx, secondarySelection);

        var diffRatioContext = BinaryMetricChartContextHelper.BuildContext(
            ctx,
            primarySelection,
            secondarySelection,
            primaryData,
            secondaryData,
            primaryCms,
            secondaryCms,
            displayName1,
            displayName2);

        return (primaryData, secondaryData, diffRatioContext);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveDiffRatioSeriesAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return (null, null);

        return await MetricSeriesSelectionAdapterHelper.ResolveSeriesAsync(
            ctx,
            selectedSeries,
            _selectionCache,
            _metricSelectionService,
            _viewModel.MetricState.ResolutionTableName,
            static currentContext => (currentContext.Data1, currentContext.PrimaryCms as ICanonicalMetricSeries),
            static (currentContext, selection) =>
            {
                if (MetricSeriesSelectionCache.IsSameSelection(selection, currentContext.PrimaryMetricType ?? currentContext.MetricType, currentContext.PrimarySubtype))
                    return (true, currentContext.Data1, currentContext.PrimaryCms as ICanonicalMetricSeries);

                if (MetricSeriesSelectionCache.IsSameSelection(selection, currentContext.SecondaryMetricType, currentContext.SecondarySubtype))
                    return (true, currentContext.Data2 ?? currentContext.Data1, currentContext.SecondaryCms as ICanonicalMetricSeries ?? currentContext.PrimaryCms as ICanonicalMetricSeries);

                return (false, null, null);
            },
            static currentContext => (currentContext.Data1, currentContext.PrimaryCms as ICanonicalMetricSeries));
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioPrimarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(
            !_isUpdatingSubtypeCombos,
            _controller.PrimarySubtypeCombo,
            _viewModel.ChartState.SelectedDiffRatioPrimarySeries,
            ctx.PrimaryMetricType ?? ctx.MetricType,
            ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioSecondarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(
            !_isUpdatingSubtypeCombos,
            _controller.SecondarySubtypeCombo,
            _viewModel.ChartState.SelectedDiffRatioSecondarySeries,
            ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType,
            ctx.SecondarySubtype);
    }

    private static string ResolveDiffRatioDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    private void UpdateDiffRatioPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        var operationSymbol = _viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/";
        _controller.Panel.Title = $"{leftName} {operationSymbol} {rightName}";

        var tooltipManager = _getTooltipManager();
        if (tooltipManager != null)
        {
            var label = !string.IsNullOrEmpty(rightName) ? $"{leftName} {operationSymbol} {rightName}" : leftName;
            tooltipManager.UpdateChartLabel(_controller.Chart, label);
        }
    }

    private bool CanUpdateDiffRatioSubtypeOptions()
    {
        return _controller.PrimarySubtypeCombo != null && _controller.SecondarySubtypeCombo != null;
    }

    private CartesianMetricChartRenderHost CreateRenderHost()
    {
        return new CartesianMetricChartRenderHost(_controller.Chart, _viewModel.ChartState);
    }
}
