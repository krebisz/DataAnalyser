using System.Windows;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class CartesianMetricChartRenderingQualificationProbeTests
{
    [Theory]
    [InlineData(CartesianMetricChartRoute.Main)]
    [InlineData(CartesianMetricChartRoute.Normalized)]
    [InlineData(CartesianMetricChartRoute.DiffRatio)]
    public async Task Probe_ForQualifiedRoute_PassesLifecycleStages(CartesianMetricChartRoute route)
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartState = new ChartState();
            var contract = new CartesianMetricChartRenderingContract(new StubRenderInvoker());
            var probe = new CartesianMetricChartRenderingQualificationProbe();
            var host = CreateHost(chartState);
            var request = new CartesianMetricChartRenderRequest(route, CreateContext());

            var result = await probe.ProbeAsync(contract, host, request);

            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Failures));
            Assert.True(result.InitialRenderPassed);
            Assert.True(result.RepeatedUpdatePassed);
            Assert.True(result.VisibilityTransitionPassed);
            Assert.True(result.OffscreenTransitionPassed);
            Assert.True(result.ResetViewPassed);
            Assert.True(result.ClearPassed);
        });
    }

    private static CartesianMetricChartRenderHost CreateHost(ChartState chartState)
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
        return new CartesianMetricChartRenderHost(chart, chartState);
    }

    private static ChartDataContext CreateContext()
    {
        return new ChartDataContext
        {
            Data1 =
            [
                new MetricData
                {
                    NormalizedTimestamp = new DateTime(2026, 1, 1),
                    Value = 10m
                }
            ],
            Data2 =
            [
                new MetricData
                {
                    NormalizedTimestamp = new DateTime(2026, 1, 1),
                    Value = 20m
                }
            ],
            DisplayName1 = "Left",
            DisplayName2 = "Right",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7)
        };
    }

    private sealed class StubRenderInvoker : ICartesianMetricChartRenderInvoker
    {
        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            host.Chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = request.Route.ToString(),
                    Values = new ChartValues<double> { 1d, 2d }
                }
            };

            if (host.Chart.AxisX.Count == 0)
                host.Chart.AxisX.Add(new Axis { Title = "Time" });

            if (host.Chart.AxisY.Count == 0)
                host.Chart.AxisY.Add(new Axis { Title = "Value" });

            return Task.CompletedTask;
        }
    }
}
