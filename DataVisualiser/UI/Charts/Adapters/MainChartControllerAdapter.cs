using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class MainChartControllerAdapter : ChartControllerAdapterBase, IMainChartControllerExtras, ICartesianChartSurface, IWpfCartesianChartHost
{
    private readonly IMainChartController _controller;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly Func<bool> _isInitializing;
    private readonly MainWindowViewModel _viewModel;

    public MainChartControllerAdapter(IMainChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
    }

    public CartesianChart Chart => _controller.Chart;

    public override string Key => "Main";
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsMainVisible)
            return Task.CompletedTask;

        return RenderMainChartAsync(context);
    }

    public override void Clear(ChartState state)
    {
        ChartSurfaceHelper.ClearCartesian(_controller.Chart, state);
    }

    public override void ResetZoom()
    {
        ChartSurfaceHelper.ResetZoom(_controller.Chart);
    }

    public override bool HasSeries(ChartState state)
    {
        return ChartSurfaceHelper.HasSeries(_controller.Chart);
    }

    public override void UpdateSubtypeOptions()
    {
    }

    public override void ClearCache()
    {
    }

    public void SyncDisplayModeSelection()
    {
        var mode = _viewModel.ChartState.MainChartDisplayMode;
        _controller.DisplayRegularRadio.IsChecked = mode == MainChartDisplayMode.Regular;
        _controller.DisplaySummedRadio.IsChecked = mode == MainChartDisplayMode.Summed;
        _controller.DisplayStackedRadio.IsChecked = mode == MainChartDisplayMode.Stacked;
    }


    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleMain();
    }

    public void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        var mode = _controller.DisplayStackedRadio.IsChecked == true ? MainChartDisplayMode.Stacked : _controller.DisplaySummedRadio.IsChecked == true ? MainChartDisplayMode.Summed : MainChartDisplayMode.Regular;

        _viewModel.SetMainChartDisplayMode(mode);
    }

    private async Task RenderMainChartAsync(ChartDataContext ctx)
    {
        if (ctx.Data1 == null)
            return;

        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator == null)
            return;

        var mode = _viewModel.ChartState.MainChartDisplayMode;
        var isStacked = mode == MainChartDisplayMode.Stacked;
        var isCumulative = mode == MainChartDisplayMode.Summed;

        await orchestrator.RenderPrimaryChartAsync(ctx, _controller.Chart, ctx.Data1, ctx.Data2, ctx.DisplayName1 ?? string.Empty, ctx.DisplayName2 ?? string.Empty, ctx.From, ctx.To, ctx.MetricType, _viewModel.MetricState.SelectedSeries, _viewModel.MetricState.ResolutionTableName, isStacked, isCumulative);
    }
}
