using DataVisualiser.UI.Charts.Presentation;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.UI.MainHost;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class WeekdayTrendChartControllerAdapter : CartesianChartControllerAdapterBase<IWeekdayTrendChartController>, IWeekdayTrendChartControllerExtras
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly IWeekdayTrendChartController _controller;
    private readonly WeekdayTrendComputationInvoker _computationInvoker;
    private readonly Func<bool> _isInitializing;
    private readonly IWeekdayTrendRenderingContract _weekdayTrendRenderingContract;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public WeekdayTrendChartControllerAdapter(IWeekdayTrendChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, Func<IStrategyCutOverService?> getStrategyCutOverService, IWeekdayTrendRenderingContract weekdayTrendRenderingContract, VNextSeriesLoadCoordinator? vnextCoordinator = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        ArgumentNullException.ThrowIfNull(getStrategyCutOverService);
        _weekdayTrendRenderingContract = weekdayTrendRenderingContract ?? throw new ArgumentNullException(nameof(weekdayTrendRenderingContract));
        _computationInvoker = new WeekdayTrendComputationInvoker(viewModel, metricSelectionService, getStrategyCutOverService, vnextCoordinator);
    }

    public CartesianChart PolarChart => _controller.PolarChart;

    public override void ClearCache()
    {
        _computationInvoker.ClearCache();
    }

    public override string Key => ChartControllerKeys.WeeklyTrend;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderWeekdayTrendAsync(context);
    }

    public override void Clear(ChartState state)
    {
        RenderingHostLifecycleAdapterHelper.Clear(CreateRenderHost, _weekdayTrendRenderingContract.Clear);
    }

    public override void ResetZoom()
    {
        RenderingHostLifecycleAdapterHelper.ResetView(ResolveRenderingRoute, CreateRenderHost, _weekdayTrendRenderingContract.ResetView);
    }

    public override bool HasSeries(ChartState state)
    {
        return RenderingHostLifecycleAdapterHelper.HasRenderableContent(ResolveRenderingRoute, CreateRenderHost, _weekdayTrendRenderingContract.HasRenderableContent);
    }

    public override void UpdateSubtypeOptions()
    {
        var combo = _controller.SubtypeCombo;
        if (combo == null)
            return;

        _isUpdatingSubtypeCombo = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            var seriesSelection = MetricSeriesSelectionAdapterHelper.PopulateSubtypeCombo(combo, selectedSeries, _viewModel.ChartState.SelectedWeekdayTrendSeries);

            if (seriesSelection == null)
            {
                _viewModel.ChartState.SelectedWeekdayTrendSeries = null;
            }
            else if (_isInitializing())
                _viewModel.ChartState.SelectedWeekdayTrendSeries = seriesSelection;
            else
                _viewModel.SetWeekdayTrendSeries(seriesSelection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }
    }

    public void InitializeControls()
    {
        ChartComboItemHelper.Populate(_controller.AverageWindowCombo, new[]
        {
                ("Running Mean", (object)WeekdayTrendAverageWindow.RunningMean),
                ("Weekly", (object)WeekdayTrendAverageWindow.Weekly),
                ("Monthly", (object)WeekdayTrendAverageWindow.Monthly)
        });

        SelectWeekdayTrendAverageWindow(_viewModel.ChartState.WeekdayTrendAverageWindow);
    }

    public void UpdateChartTypeVisibility()
    {
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            return;
        }

        var mode = _viewModel.ChartState.WeekdayTrendChartMode;
        if (mode == WeekdayTrendChartMode.Polar)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Visible;
            _controller.ChartTypeToggleButton.Content = "Scatter";
        }
        else
        {
            _controller.Chart.Visibility = Visibility.Visible;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            _controller.ChartTypeToggleButton.Content = mode == WeekdayTrendChartMode.Scatter ? "Cartesian" : "Polar";
        }
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleWeeklyTrend();
    }

    public void OnDayToggled(object? sender, WeekdayTrendDayToggleEventArgs e)
    {
        switch (e.Day)
        {
            case DayOfWeek.Monday:
                _viewModel.SetWeeklyTrendMondayVisible(e.IsChecked);
                break;
            case DayOfWeek.Tuesday:
                _viewModel.SetWeeklyTrendTuesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Wednesday:
                _viewModel.SetWeeklyTrendWednesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Thursday:
                _viewModel.SetWeeklyTrendThursdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Friday:
                _viewModel.SetWeeklyTrendFridayVisible(e.IsChecked);
                break;
            case DayOfWeek.Saturday:
                _viewModel.SetWeeklyTrendSaturdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Sunday:
                _viewModel.SetWeeklyTrendSundayVisible(e.IsChecked);
                break;
        }
    }

    public void OnAverageToggled(object? sender, WeekdayTrendAverageToggleEventArgs e)
    {
        _viewModel.SetWeeklyTrendAverageVisible(e.IsChecked);
    }

    public void OnAverageWindowChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        if (_controller.AverageWindowCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is WeekdayTrendAverageWindow window)
            _viewModel.SetWeeklyTrendAverageWindow(window);
    }

    public void OnSubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
        _viewModel.SetWeekdayTrendSeries(selection);
    }

    public async void OnChartTypeToggleRequested(object? sender, EventArgs e)
    {
        using var _ = _beginUiBusyScope();
        _viewModel.ToggleWeekdayTrendChartType();
        UpdateChartTypeVisibility();

        if (_viewModel.ChartState.IsWeeklyTrendVisible && _viewModel.ChartState.LastContext != null)
        {
            var result = _computationInvoker.ComputeFromContext(_viewModel.ChartState.LastContext);
            if (result != null)
            {
                var renderResult = RenderWeekdayTrendChart(result);
                _viewModel.ChartState.SetRenderPlanDiagnostics(
                    ChartProgramKind.WeekdayTrend,
                    renderResult);
            }
        }
    }

    private void SelectWeekdayTrendAverageWindow(WeekdayTrendAverageWindow window)
    {
        _ = ChartComboItemHelper.TrySelectByTag(_controller.AverageWindowCombo, tag => tag is WeekdayTrendAverageWindow option && option == window);
    }

    private async Task RenderWeekdayTrendAsync(ChartDataContext ctx)
    {
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
            return;

        var selectedSeries = ResolveSelectedWeekdayTrendSeries(ctx);
        var displayName = ResolveWeekdayTrendDisplayName(ctx, selectedSeries);
        var result = await _computationInvoker.ComputeAsync(ctx, selectedSeries, displayName);
        if (result != null)
        {
            var renderResult = RenderWeekdayTrendChart(result);
            _viewModel.ChartState.SetRenderPlanDiagnostics(
                ChartProgramKind.WeekdayTrend,
                renderResult);
        }
    }

    private MetricSeriesSelection? ResolveSelectedWeekdayTrendSeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(!_isUpdatingSubtypeCombo, _controller.SubtypeCombo, _viewModel.ChartState.SelectedWeekdayTrendSeries, ctx);
    }

    private static string ResolveWeekdayTrendDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    private ChartRenderAdapterResult RenderWeekdayTrendChart(WeekdayTrendResult result)
    {
        var renderTarget = RenderingHostLifecycleAdapterHelper.CreateTarget(ResolveRenderingRoute, CreateRenderHost);
        return _weekdayTrendRenderingContract.Render(
            new WeekdayTrendChartRenderRequest(
                renderTarget.Route,
                result,
                _viewModel.ChartState,
                _viewModel.ChartState.SelectedWeekdayTrendSeries?.DisplayKey ?? "<none>",
                WeekdayTrendCapabilityContract.Create()),
            renderTarget.Host);
    }

    private WeekdayTrendRenderingRoute ResolveRenderingRoute()
    {
        return WeekdayTrendRenderingRouteResolver.Resolve(_viewModel.ChartState.WeekdayTrendChartMode);
    }

    private WeekdayTrendChartRenderHost CreateRenderHost()
    {
        return new WeekdayTrendChartRenderHost(_controller.Chart, _controller.PolarChart, _viewModel.ChartState);
    }

}
