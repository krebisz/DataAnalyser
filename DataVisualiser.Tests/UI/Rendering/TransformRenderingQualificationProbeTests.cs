using System.Windows;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class TransformRenderingQualificationProbeTests
{
    [Fact]
    public async Task Probe_ForTransformRoute_PassesLifecycleStages()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartState = new ChartState();
            var contract = new TransformRenderingContract(new StubRenderInvoker());
            var probe = new TransformRenderingQualificationProbe();
            var host = CreateHost(chartState);
            var request = new TransformChartRenderRequest(
                TransformRenderingRoute.ResultCartesian,
                CreateContext(),
                new StubStrategy(),
                "Transform Result",
                "+",
                true);

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

    private static TransformChartRenderHost CreateHost(ChartState chartState)
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
        return new TransformChartRenderHost(chart, chartState);
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
            DisplayName1 = "Transform Input",
            From = new DateTime(2026, 1, 1),
            To = new DateTime(2026, 1, 7)
        };
    }

    private sealed class StubRenderInvoker : ITransformChartRenderInvoker
    {
        public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
        {
            host.Chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = request.PrimaryLabel,
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

    private sealed class StubStrategy : IChartComputationStrategy
    {
        public string PrimaryLabel => "Transform Result";

        public string SecondaryLabel => string.Empty;

        public string? Unit => "kg";

        public DataVisualiser.Core.Computation.Results.ChartComputationResult? Compute()
        {
            return null;
        }
    }
}
