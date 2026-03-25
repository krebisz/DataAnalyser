using System.Windows;
using System.Windows.Threading;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Tests.Helpers;
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

    private static ChartComputationResult CreateSingleSeriesResult()
    {
        return new ChartComputationResult
        {
            Timestamps = [new DateTime(2024, 1, 1), new DateTime(2024, 1, 2)],
            PrimaryRawValues = [10d, 12d],
            PrimarySmoothed = [10d, 12d]
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
