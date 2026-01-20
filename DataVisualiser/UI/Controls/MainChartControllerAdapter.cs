using System;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

public sealed class MainChartControllerAdapter : IChartController, IMainChartControllerExtras, IChartSeriesAvailability
{
    private readonly MainChartController _controller;
    private readonly MainWindowViewModel _viewModel;
    private readonly Func<bool> _isInitializing;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;

    public MainChartControllerAdapter(
        MainChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
    }

    public string Key => "Main";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => false;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsMainVisible)
            return Task.CompletedTask;

        return RenderMainChartAsync(context);
    }

    public void Clear(ChartState state)
    {
        ChartHelper.ClearChart(_controller.Chart, state.ChartTimestamps);
    }

    public void ResetZoom()
    {
        ChartHelper.ResetZoom(_controller.Chart);
    }

    public bool HasSeries(ChartState state)
    {
        return HasSeriesInternal(_controller.Chart.Series);
    }

    private static bool HasSeriesInternal(System.Collections.IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
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

        var mode = _controller.DisplayStackedRadio.IsChecked == true
            ? MainChartDisplayMode.Stacked
            : _controller.DisplaySummedRadio.IsChecked == true
                ? MainChartDisplayMode.Summed
                : MainChartDisplayMode.Regular;

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

        await orchestrator.RenderPrimaryChartAsync(
            ctx,
            _controller.Chart,
            ctx.Data1,
            ctx.Data2,
            ctx.DisplayName1 ?? string.Empty,
            ctx.DisplayName2 ?? string.Empty,
            ctx.From,
            ctx.To,
            ctx.MetricType,
            _viewModel.MetricState.SelectedSeries,
            _viewModel.MetricState.ResolutionTableName,
            isStacked: isStacked,
            isCumulative: isCumulative);
    }
}
