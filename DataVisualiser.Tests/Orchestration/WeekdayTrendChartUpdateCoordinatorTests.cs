using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using System.Windows;
using System.Windows.Media;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Orchestration;

public sealed class WeekdayTrendChartUpdateCoordinatorTests
{
    [Fact]
    public void TryRefitActiveChart_InPolarMode_ReappliesPolarAxisBounds()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                WeekdayTrendChartMode = WeekdayTrendChartMode.Polar
            };
            var coordinator = new WeekdayTrendChartUpdateCoordinator(new WeekdayTrendRenderingService(), chartState.ChartTimestamps);
            var cartesianChart = new CartesianChart();
            var polarChart = new CartesianChart();

            coordinator.UpdateChart(CreateResult(), chartState, cartesianChart, polarChart);

            var refit = coordinator.TryRefitActiveChart();

            Assert.True(refit);
            Assert.Equal(ChartRenderDefaults.PolarAxisMinValue, polarChart.AxisX[0].MinValue);
            Assert.Equal(ChartRenderDefaults.PolarAxisMaxValue, polarChart.AxisX[0].MaxValue);
            Assert.Equal(1d, polarChart.AxisY[0].MinValue);
            Assert.Equal(5d, polarChart.AxisY[0].MaxValue);
        });
    }

    [Fact]
    public void TryRefitActiveChart_WithoutCachedResult_ReturnsFalse()
    {
        StaTestHelper.Run(() =>
        {
            var coordinator = new WeekdayTrendChartUpdateCoordinator(new WeekdayTrendRenderingService(), new Dictionary<CartesianChart, List<DateTime>>());

            Assert.False(coordinator.TryRefitActiveChart());
        });
    }

    [Fact]
    public void UpdateChart_ShouldBindAverageSeriesStrokeToThemeTextBrush()
    {
        StaTestHelper.Run(() =>
        {
            var chartState = new ChartState
            {
                ShowAverage = true
            };
            var chart = new CartesianChart();
            var initialBrush = Brushes.Black;
            var updatedBrush = Brushes.White;
            var app = Application.Current ?? new Application();
            var resources = app.Resources;
            var hadExisting = resources.Contains("ThemePrimaryTextBrush");
            var existing = hadExisting ? resources["ThemePrimaryTextBrush"] : null;

            try
            {
                resources["ThemePrimaryTextBrush"] = initialBrush;

                new WeekdayTrendRenderingService().RenderWeekdayTrendChart(CreateResult(), chartState, chart, new CartesianChart());

                var averageSeries = chart.Series.OfType<LineSeries>().Single(series => series.Title == "Ave");
                Assert.Same(initialBrush, averageSeries.Stroke);

                resources["ThemePrimaryTextBrush"] = updatedBrush;
                WeekdayTrendRenderingService.ApplyAverageStroke(chart);

                Assert.Same(updatedBrush, averageSeries.Stroke);
            }
            finally
            {
                if (hadExisting)
                    resources["ThemePrimaryTextBrush"] = existing;
                else
                    resources.Remove("ThemePrimaryTextBrush");
            }
        });
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
