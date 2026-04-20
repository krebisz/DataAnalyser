using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class NormalizedChartControllerAdapter : CartesianChartControllerAdapterBase<INormalizedChartController>
{
    private const CartesianMetricChartRoute RenderingRoute = CartesianMetricChartRoute.Normalized;
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly INormalizedChartController _controller;
    private readonly ICartesianMetricChartRenderingContract _renderingContract;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombos;

    public NormalizedChartControllerAdapter(INormalizedChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, ICartesianMetricChartRenderingContract renderingContract)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _renderingContract = renderingContract ?? throw new ArgumentNullException(nameof(renderingContract));
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.Normalized;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => true;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderNormalizedAsync(context);
    }

    public override void UpdateSubtypeOptions()
    {
        if (!CanUpdateNormalizedSubtypeOptions())
            return;

        _isUpdatingSubtypeCombos = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            MetricSeriesSelectionAdapterHelper.PopulatePrimarySecondarySubtypeCombos(
                _controller.NormalizedPrimarySubtypeCombo,
                _controller.NormalizedSecondarySubtypeCombo,
                _controller.NormalizedSecondarySubtypePanel,
                selectedSeries,
                _viewModel.ChartState.SelectedNormalizedPrimarySeries,
                _viewModel.ChartState.SelectedNormalizedSecondarySeries,
                selection => _viewModel.ChartState.SelectedNormalizedPrimarySeries = selection,
                selection => _viewModel.ChartState.SelectedNormalizedSecondarySeries = selection);
        }
        finally
        {
            _isUpdatingSubtypeCombos = false;
        }
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleNorm();
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

    public async void OnNormalizationModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        try
        {
            ApplySelectedNormalizationMode();
            await RerenderNormalizedIfVisibleAsync();
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
        }
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        await MetricSeriesSelectionAdapterHelper.HandleSubtypeSelectionChangeAsync(
            _isInitializing(),
            _isUpdatingSubtypeCombos,
            _controller.NormalizedPrimarySubtypeCombo,
            _viewModel.SetNormalizedPrimarySeries,
            RenderNormalizedFromSelectionAsync);
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        await MetricSeriesSelectionAdapterHelper.HandleSubtypeSelectionChangeAsync(
            _isInitializing(),
            _isUpdatingSubtypeCombos,
            _controller.NormalizedSecondarySubtypeCombo,
            _viewModel.SetNormalizedSecondarySeries,
            RenderNormalizedFromSelectionAsync);
    }

    private async Task RenderNormalizedFromSelectionAsync()
    {
        await BinaryMetricChartContextHelper.RerenderIfVisibleAsync(
            _viewModel.ChartState.IsNormalizedVisible,
            _viewModel.ChartState.LastContext,
            RenderNormalizedAsync);
    }

    private async Task RenderNormalizedAsync(ChartDataContext ctx)
    {
        var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateNormalizedPanelTitle(normalizedContext);
        await _renderingContract.RenderAsync(
            new CartesianMetricChartRenderRequest(RenderingRoute, normalizedContext),
            CreateRenderHost());
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveNormalizedDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedNormalizedPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedNormalizedSecondarySeries(ctx);

        var (primaryData, primaryCms) = await ResolveNormalizedSeriesAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;
        ICanonicalMetricSeries? secondaryCms = null;
        if (secondarySelection != null)
        {
            var resolvedSecondary = await ResolveNormalizedSeriesAsync(ctx, secondarySelection);
            secondaryData = resolvedSecondary.Data;
            secondaryCms = resolvedSecondary.Cms;
        }

        var normalizedContext = BuildNormalizedContext(ctx, primarySelection, secondarySelection, primaryData, secondaryData, primaryCms, secondaryCms);

        return (primaryData, secondaryData, normalizedContext);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveNormalizedSeriesAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return await MetricSeriesSelectionAdapterHelper.ResolveSeriesAsync(
            ctx,
            selectedSeries,
            _selectionCache,
            _metricSelectionService,
            _viewModel.MetricState.ResolutionTableName,
            static _ => (null, null),
            static (currentContext, selection) =>
            {
                if (currentContext.Data1 != null && MetricSeriesSelectionCache.IsSameSelection(selection, currentContext.PrimaryMetricType ?? currentContext.MetricType, currentContext.PrimarySubtype))
                    return (true, currentContext.Data1, currentContext.PrimaryCms as ICanonicalMetricSeries);

                if (currentContext.Data2 != null && MetricSeriesSelectionCache.IsSameSelection(selection, currentContext.SecondaryMetricType, currentContext.SecondarySubtype))
                    return (true, currentContext.Data2, currentContext.SecondaryCms as ICanonicalMetricSeries);

                return (false, null, null);
            },
            static currentContext => (currentContext.Data1, currentContext.PrimaryCms as ICanonicalMetricSeries));
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedPrimarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(
            !_isUpdatingSubtypeCombos,
            _controller.NormalizedPrimarySubtypeCombo,
            _viewModel.ChartState.SelectedNormalizedPrimarySeries,
            ctx.PrimaryMetricType ?? ctx.MetricType,
            ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedSecondarySeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(
            !_isUpdatingSubtypeCombos,
            _controller.NormalizedSecondarySubtypeCombo,
            _viewModel.ChartState.SelectedNormalizedSecondarySeries,
            ctx.SecondaryMetricType,
            ctx.SecondarySubtype);
    }

    private static string ResolveNormalizedDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    private void ApplySelectedNormalizationMode()
    {
        if (_controller.NormZeroToOneRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
        else if (_controller.NormPercentOfMaxRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
        else if (_controller.NormRelativeToMaxRadio.IsChecked == true)
            _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);
    }

    private async Task RerenderNormalizedIfVisibleAsync()
    {
        using var _ = _beginUiBusyScope();
        await MetricSeriesSelectionAdapterHelper.RerenderIfVisibleAsync(
            _viewModel.ChartState.IsNormalizedVisible,
            _viewModel.ChartState.LastContext,
            static ctx => ctx.Data1 != null && ctx.Data2 != null,
            RenderNormalizedAsync);
    }

    private static ChartDataContext BuildNormalizedContext(ChartDataContext ctx, MetricSeriesSelection? primarySelection, MetricSeriesSelection? secondarySelection, IReadOnlyList<MetricData>? primaryData, IReadOnlyList<MetricData>? secondaryData, ICanonicalMetricSeries? primaryCms, ICanonicalMetricSeries? secondaryCms)
    {
        return BinaryMetricChartContextHelper.BuildContext(
            ctx,
            primarySelection,
            secondarySelection,
            primaryData,
            secondaryData,
            primaryCms,
            secondaryCms,
            ResolveNormalizedDisplayName(ctx, primarySelection),
            ResolveNormalizedDisplayName(ctx, secondarySelection));
    }

    private void UpdateNormalizedPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        _controller.Panel.Title = $"{leftName} ~ {rightName}";
    }

    private bool CanUpdateNormalizedSubtypeOptions()
    {
        return _controller.NormalizedPrimarySubtypeCombo != null && _controller.NormalizedSecondarySubtypeCombo != null;
    }

    private CartesianMetricChartRenderHost CreateRenderHost()
    {
        return new CartesianMetricChartRenderHost(_controller.Chart, _viewModel.ChartState);
    }
}
