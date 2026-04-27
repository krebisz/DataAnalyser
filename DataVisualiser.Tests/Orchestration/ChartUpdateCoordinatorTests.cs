using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartUpdateCoordinatorTests
{
    [Fact]
    public async Task UpdateChartUsingStrategyAsync_ShouldRenderSeries_AndTrackTimestamps_WhenStrategyReturnsResult()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());

                await coordinator.UpdateChartUsingStrategyAsync(
                    chart,
                    new StubStrategy(CreateSingleSeriesResult()),
                    "Primary");
                await FlushChartAsync(chart);

                Assert.NotEmpty(chart.Series);
                Assert.True(chartTimestamps.TryGetValue(chart, out var timestamps));
                Assert.Equal(2, timestamps!.Count);
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task UpdateChartUsingStrategyAsync_WithRenderPlanAdapter_ShouldRenderSeries_AndTrackTimestamps()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());

                await coordinator.UpdateChartUsingStrategyAsync(
                    chart,
                    new StubStrategy(CreateSingleSeriesResult()),
                    "Primary",
                    metricType: "Weight",
                    useRenderPlanAdapter: true);
                await FlushChartAsync(chart);

                Assert.NotEmpty(chart.Series);
                Assert.True(chartTimestamps.TryGetValue(chart, out var timestamps));
                Assert.Equal(2, timestamps!.Count);
                Assert.Equal(2, chart.Series.Count);
                Assert.NotNull(coordinator.LastRenderPlanAdapterResult);
                var metadata = coordinator.LastRenderPlanAdapterResult!.Metadata;
                Assert.Equal("Main", metadata["ProgramKind"]);
                Assert.Equal("Chart", metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
                Assert.Equal("MainChart", metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
                Assert.Equal("Identity", metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
                Assert.Equal("MultiSeries", metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
                Assert.True(metadata.ContainsKey(ChartRenderPlanMetadataKeys.IntentSignature));
                Assert.True(metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProvenanceSignature));
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task UpdateChartUsingStrategyAsync_WithRenderPlanAdapter_ShouldRenderOverlaySeries()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());

                await coordinator.UpdateChartUsingStrategyAsync(
                    chart,
                    new StubStrategy(CreateSingleSeriesResult()),
                    "Primary",
                    metricType: "Weight",
                    overlaySeries: [CreateOverlaySeriesResult()],
                    useRenderPlanAdapter: true);
                await FlushChartAsync(chart);

                Assert.Contains(chart.Series, series => series.Title.Contains("Overlay", StringComparison.Ordinal));
                Assert.Equal(3, chart.Series.Count);
                Assert.Equal(4, coordinator.LastRenderPlanAdapterResult?.RenderedPointCount);
                Assert.Equal(2, chartTimestamps[chart].Count);
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task UpdateChartUsingStrategyAsync_ShouldClearChart_AndRemoveTimestamps_WhenStrategyReturnsNull()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                chart.Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Existing",
                        Values = new ChartValues<double> { 1d, 2d }
                    }
                };
                chartTimestamps[chart] = [new DateTime(2024, 1, 1)];

                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());

                await coordinator.UpdateChartUsingStrategyAsync(
                    chart,
                    new StubStrategy(null),
                    "Primary");
                await FlushChartAsync(chart);

                Assert.Empty(chart.Series);
                Assert.False(chartTimestamps.ContainsKey(chart));
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task TransformChartRenderInvoker_ShouldUseRenderPlanAdapter_AndCaptureTransformDiagnostics()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());
                var invoker = new TransformChartRenderInvoker(coordinator);
                var chartState = new ChartState();
                var context = new ChartDataContext
                {
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    PrimarySubtype = "body_fat_mass",
                    DisplayPrimaryMetricType = "Weight",
                    DisplayPrimarySubtype = "Fat (mass)",
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                };

                await invoker.RenderAsync(
                    new TransformChartRenderRequest(
                        TransformRenderingRoute.ResultCartesian,
                        context,
                        new StubStrategy(CreateSingleSeriesResult()),
                        "Transform Result",
                        "log",
                        true),
                    new TransformChartRenderHost(chart, chartState));
                await FlushChartAsync(chart);

                Assert.NotEmpty(chart.Series);
                Assert.True(chartTimestamps.ContainsKey(chart));
                Assert.True(chartState.RenderPlanDiagnostics.ContainsKey(ChartProgramKind.Transform));
                Assert.Equal("LiveChartsWpf", chartState.RenderPlanDiagnostics[ChartProgramKind.Transform].BackendKey);
                var metadata = chartState.RenderPlanDiagnostics[ChartProgramKind.Transform].Metadata;
                Assert.Equal("Chart", metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
                Assert.Equal("TransformChart", metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
                Assert.Equal("Transform", metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
                Assert.Equal("MultiSeries", metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
                Assert.True(metadata.ContainsKey(ChartRenderPlanMetadataKeys.IntentSignature));
                Assert.True(metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProvenanceSignature));
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    private static async Task<(Window Window, CartesianChart Chart)> CreateHostedChartAsync()
    {
        var chart = new CartesianChart
        {
            Width = 800,
            Height = 400,
            Visibility = Visibility.Visible
        };
        var window = new Window
        {
            Width = 900,
            Height = 600,
            Content = chart
        };

        window.Show();
        window.UpdateLayout();
        chart.Measure(new Size(800, 400));
        chart.Arrange(new Rect(0, 0, 800, 400));
        chart.UpdateLayout();
        await chart.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

        return (window, chart);
    }

    private static async Task FlushChartAsync(CartesianChart chart)
    {
        chart.UpdateLayout();
        await chart.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static ChartComputationResult CreateSingleSeriesResult()
    {
        return new ChartComputationResult
        {
            Timestamps = [new DateTime(2024, 1, 1), new DateTime(2024, 1, 2)],
            PrimaryRawValues = [10d, 12d],
            PrimarySmoothed = [10d, 12d]
        };
    }

    private static SeriesResult CreateOverlaySeriesResult()
    {
        return new SeriesResult
        {
            SeriesId = "overlay",
            DisplayName = "Overlay",
            Timestamps = [new DateTime(2024, 1, 1), new DateTime(2024, 1, 2)],
            RawValues = [11d, 13d],
            Smoothed = [11d, 13d]
        };
    }

    private sealed class StubStrategy : IChartComputationStrategy
    {
        private readonly ChartComputationResult? _result;

        public StubStrategy(ChartComputationResult? result)
        {
            _result = result;
        }

        public string PrimaryLabel => "Primary";

        public string SecondaryLabel => string.Empty;

        public string? Unit => "kg";

        public ChartComputationResult? Compute()
        {
            return _result;
        }
    }

    private sealed class CapturingNotificationService : IUserNotificationService
    {
        public List<(string Title, string Message)> Errors { get; } = [];

        public void ShowError(string title, string message)
        {
            Errors.Add((title, message));
        }
    }
}
