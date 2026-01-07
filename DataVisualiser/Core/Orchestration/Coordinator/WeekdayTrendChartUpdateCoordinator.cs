using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.Coordinator;

/// <summary>
///     Coordinates rendering and post-render updates for weekday trend charts.
///     Keeps behavior consistent with other chart pipelines (clear, render, finalize).
/// </summary>
public sealed class WeekdayTrendChartUpdateCoordinator
{
    private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    private readonly ChartRenderGate                            _renderGate = new();
    private readonly WeekdayTrendRenderingService               _renderingService;
    private          CartesianChart?                            _cartesianChart;
    private          ChartState?                                _chartState;
    private          WeekdayTrendResult?                        _lastResult;
    private          CartesianChart?                            _polarChart;

    public WeekdayTrendChartUpdateCoordinator(WeekdayTrendRenderingService renderingService, Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
    {
        _renderingService = renderingService ?? throw new ArgumentNullException(nameof(renderingService));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
    }

    public void UpdateChart(WeekdayTrendResult? result, ChartState chartState, CartesianChart cartesianChart, CartesianChart polarChart)
    {
        _lastResult = result;
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _cartesianChart = cartesianChart ?? throw new ArgumentNullException(nameof(cartesianChart));
        _polarChart = polarChart ?? throw new ArgumentNullException(nameof(polarChart));

        if (result == null)
        {
            ChartHelper.ClearChart(cartesianChart, _chartTimestamps);
            ChartHelper.ClearChart(polarChart, _chartTimestamps);
            return;
        }

        RenderWhenReady();
    }

    private static void FinalizeChart(CartesianChart chart)
    {
        ChartHelper.InitializeChartTooltip(chart);
        chart.Update(true, true);
    }

    private void RenderWhenReady()
    {
        if (_lastResult == null || _chartState == null || _cartesianChart == null || _polarChart == null)
            return;

        var targetChart = _chartState.IsWeekdayTrendPolarMode ? _polarChart : _cartesianChart;

        _renderGate.ExecuteWhenReady(targetChart, () =>
        {
            _renderingService.RenderWeekdayTrendChart(_lastResult, _chartState, _cartesianChart, _polarChart);

            FinalizeChart(_cartesianChart);
            FinalizeChart(_polarChart);
        });
    }
}