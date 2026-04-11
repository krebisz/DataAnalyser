using System.Windows;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.WeekdayTrend;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class WeekdayTrendRenderingQualificationProbeTests
{
    [Theory]
    [InlineData(WeekdayTrendChartMode.Cartesian, WeekdayTrendRenderingRoute.Cartesian)]
    [InlineData(WeekdayTrendChartMode.Polar, WeekdayTrendRenderingRoute.Polar)]
    [InlineData(WeekdayTrendChartMode.Scatter, WeekdayTrendRenderingRoute.Scatter)]
    public void Probe_ForActiveRoute_PassesLifecycleStages(WeekdayTrendChartMode mode, WeekdayTrendRenderingRoute route)
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                WeekdayTrendChartMode = mode
            };
            var contract = CreateContract(chartState);
            var probe = new WeekdayTrendRenderingQualificationProbe();
            var host = CreateHost(chartState);
            var request = new WeekdayTrendChartRenderRequest(route, CreateResult(), chartState);

            var result = probe.Probe(contract, host, request);

            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Failures));
            Assert.True(result.InitialRenderPassed);
            Assert.True(result.RepeatedUpdatePassed);
            Assert.True(result.VisibilityTransitionPassed);
            Assert.True(result.OffscreenTransitionPassed);
            Assert.True(result.ResetViewPassed);
            Assert.True(result.ClearPassed);
        });
    }

    private static WeekdayTrendRenderingContract CreateContract(ChartState chartState)
    {
        return new WeekdayTrendRenderingContract(
            new WeekdayTrendChartUpdateCoordinator(
                new WeekdayTrendRenderingService(),
                chartState.ChartTimestamps));
    }

    private static WeekdayTrendChartRenderHost CreateHost(ChartState chartState)
    {
        var cartesian = CreateChart();
        var polar = CreateChart();
        return new WeekdayTrendChartRenderHost(cartesian, polar, chartState);
    }

    private static CartesianChart CreateChart()
    {
        var chart = new CartesianChart
        {
            Width = 800,
            Height = 400,
            Visibility = Visibility.Visible
        };
        chart.Measure(new Size(800, 400));
        chart.Arrange(new Rect(0, 0, 800, 400));
        chart.UpdateLayout();
        return chart;
    }

    private static WeekdayTrendResult CreateResult()
    {
        var result = new WeekdayTrendResult
        {
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7),
            GlobalMin = 1d,
            GlobalMax = 5d,
            Unit = "kg"
        };

        result.SeriesByDay[0] = new WeekdayTrendSeries
        {
            Day = DayOfWeek.Monday,
            Points =
            [
                new WeekdayTrendPoint
                {
                    Date = new DateTime(2026, 1, 5),
                    Value = 2d
                }
            ]
        };

        result.SeriesByDay[1] = new WeekdayTrendSeries
        {
            Day = DayOfWeek.Tuesday,
            Points =
            [
                new WeekdayTrendPoint
                {
                    Date = new DateTime(2026, 1, 6),
                    Value = 4d
                }
            ]
        };

        return result;
    }
}
