using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class DistributionRenderingQualificationProbeTests
{
    [Fact]
    public async Task ProbeAsync_ForCartesianRoute_PassesLifecycleStages()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var service = new ProbeDistributionService();
            var contract = new DistributionRenderingContract(() => null, service, service, new DistributionPolarRenderingService());
            var probe = new DistributionRenderingQualificationProbe();
            var chartState = new ChartState();
            var tooltip = new ToolTip
            {
                    IsOpen = true
            };
            var host = CreateHost(chartState, () => tooltip);

            var result = await probe.ProbeAsync(
                contract,
                host,
                CreateRequest(DistributionRenderingRoute.Cartesian, DistributionMode.Weekly),
                CreateRequest(DistributionRenderingRoute.Cartesian, DistributionMode.Hourly));

            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Failures));
            Assert.True(result.InitialRenderPassed);
            Assert.True(result.RepeatedUpdatePassed);
            Assert.True(result.VisibilityTransitionPassed);
            Assert.True(result.OffscreenTransitionPassed);
            Assert.True(result.ResetViewPassed);
            Assert.True(result.ClearPassed);
            Assert.True(result.DisposalPassed);
            Assert.True(service.DisposedInteractionCount >= 2);
        });
    }

    [Fact]
    public async Task ProbeAsync_ForPolarFallbackRoute_PassesLifecycleStages()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var service = new ProbeDistributionService();
            var contract = new DistributionRenderingContract(() => null, service, service, new DistributionPolarRenderingService());
            var probe = new DistributionRenderingQualificationProbe();
            var chartState = new ChartState
            {
                    IsDistributionPolarMode = true
            };
            var tooltip = new ToolTip
            {
                    IsOpen = true
            };
            var host = CreateHost(chartState, () => tooltip);

            var result = await probe.ProbeAsync(
                contract,
                host,
                CreateRequest(DistributionRenderingRoute.PolarFallback, DistributionMode.Weekly),
                CreateRequest(DistributionRenderingRoute.PolarFallback, DistributionMode.Hourly));

            Assert.True(result.Passed, string.Join(Environment.NewLine, result.Failures));
            Assert.True(result.InitialRenderPassed);
            Assert.True(result.RepeatedUpdatePassed);
            Assert.True(result.VisibilityTransitionPassed);
            Assert.True(result.OffscreenTransitionPassed);
            Assert.True(result.ResetViewPassed);
            Assert.True(result.ClearPassed);
            Assert.True(result.DisposalPassed);
            Assert.False(tooltip.IsOpen);
            Assert.Null(host.CartesianChart.Tag);
        });
    }

    private static DistributionChartRenderHost CreateHost(ChartState chartState, Func<ToolTip?> getPolarTooltip)
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

        var polar = new PolarChart
        {
                Visibility = Visibility.Collapsed
        };

        return new DistributionChartRenderHost(chart, polar, chartState, getPolarTooltip);
    }

    private static DistributionChartRenderRequest CreateRequest(DistributionRenderingRoute route, DistributionMode mode)
    {
        var data = new List<MetricData>
        {
            new()
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1, 0, 0, 0),
                Value = 10m,
                Unit = "kg"
            },
            new()
            {
                NormalizedTimestamp = new DateTime(2026, 1, 1, 1, 0, 0),
                Value = 12m,
                Unit = "kg"
            }
        };

        return new DistributionChartRenderRequest(
            route,
            mode,
            new DistributionModeSettings(true, DistributionModeCatalog.Get(mode).DefaultIntervalCount),
            data,
            "Weight",
            data[0].NormalizedTimestamp,
            data[^1].NormalizedTimestamp,
            CmsSeries: null,
            new ChartDataContext
            {
                Data1 = data,
                DisplayName1 = "Weight",
                From = data[0].NormalizedTimestamp,
                To = data[^1].NormalizedTimestamp
            },
            new ChartState());
    }

    private sealed class ProbeDistributionService : IDistributionService
    {
        public int DisposedInteractionCount { get; private set; }

        public Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400, bool useFrequencyShading = true, int intervalCount = 10, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            if (targetChart.AxisX.Count == 0)
                targetChart.AxisX.Add(new Axis());

            targetChart.Series.Clear();
            targetChart.Series.Add(new LineSeries
            {
                    Title = displayName,
                    Values = new ChartValues<double>
                    {
                            1,
                            2,
                            3
                    }
            });
            targetChart.Tag = new TrackingDisposable(this);
            targetChart.Update(true, true);
            return Task.CompletedTask;
        }

        public Task<DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
        {
            return Task.FromResult<DistributionRangeResult?>(new DistributionRangeResult(
                [1, 2, 3, 4, 5, 6, 7],
                [2, 3, 4, 5, 6, 7, 8],
                [1.5, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5],
                1,
                8,
                "kg"));
        }

        public void SetShadingStrategy(IIntervalShadingStrategy strategy)
        {
        }

        private sealed class TrackingDisposable : IDisposable
        {
            private readonly ProbeDistributionService _owner;
            private bool _disposed;

            public TrackingDisposable(ProbeDistributionService owner)
            {
                _owner = owner;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                _owner.DisposedInteractionCount++;
            }
        }
    }
}
