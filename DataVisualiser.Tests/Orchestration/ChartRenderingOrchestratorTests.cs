using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using DataFileReader.Canonical;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using Moq;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Orchestration;

public sealed class ChartRenderingOrchestratorTests
{
    [Fact]
    public void ShouldRenderCharts_ShouldReturnFalse_WhenNoPrimaryData()
    {
        var method = GetPrivateStaticMethod("ShouldRenderCharts");

        var result = (bool)method.Invoke(null,
                new object?[]
                {
                        null
                })!;

        Assert.False(result);
    }

    [Fact]
    public void HasSecondaryData_ShouldReturnTrue_WhenSecondaryPresent()
    {
        var method = GetPrivateStaticMethod("HasSecondaryData");

        var ctx = new ChartDataContext
        {
                Data2 = new List<MetricData>
                {
                        TestDataBuilders.HealthMetricData().Build()
                }
        };

        var result = (bool)method.Invoke(null,
                new object?[]
                {
                        ctx
                })!;

        Assert.True(result);
    }

    [Fact]
    public async Task RenderPrimaryChart_ShouldUseMultiMetricCutOver_AndRenderTrackedSeries_WhenThreeSeriesProvided()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var window = new Window
            {
                Width = 900,
                Height = 600
            };
            var chart = new CartesianChart
            {
                Name = "MainChartTest",
                Width = 800,
                Height = 400
            };
            window.Content = chart;
            window.Show();
            window.UpdateLayout();
            chart.Measure(new Size(800, 400));
            chart.Arrange(new Rect(0, 0, 800, 400));
            chart.UpdateLayout();
            await chart.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            var tooltipManager = new ChartTooltipManager(window);
            try
            {
                var strategy = new StubStrategy(CreateMultiSeriesResult());
                var cutOverService = new Mock<IStrategyCutOverService>(MockBehavior.Strict);
                cutOverService
                    .Setup(service => service.CreateStrategy(
                        StrategyType.MultiMetric,
                        It.IsAny<ChartDataContext>(),
                        It.IsAny<StrategyCreationParameters>()))
                    .Returns(strategy);

                var coordinator = new ChartUpdateCoordinator(
                    new ChartComputationEngine(),
                    new ChartRenderEngine(),
                    tooltipManager,
                    chartTimestamps,
                    new CapturingNotificationService());

                var orchestrator = new ChartRenderingOrchestrator(
                    coordinator,
                    Mock.Of<IDistributionService>(),
                    Mock.Of<IDistributionService>(),
                    cutOverService.Object,
                    new CapturingNotificationService());

                var additionalSeries = new List<IEnumerable<MetricData>>
                {
                    TestDataBuilders.HealthMetricData().WithValue(15m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1))
                };
                var additionalLabels = new List<string> { "MetricA:C" };
                var cmsSeries = new List<ICanonicalMetricSeries>
                {
                    TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_weight").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build(),
                    TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build(),
                    TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithDimension(MetricDimension.Mass).WithStartTime(new DateTimeOffset(new DateTime(2024, 1, 1), TimeSpan.Zero)).WithSampleCount(2).Build()
                };
                var context = new ChartDataContext
                {
                    Data1 = TestDataBuilders.HealthMetricData().WithValue(10m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
                    Data2 = TestDataBuilders.HealthMetricData().WithValue(20m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
                    DisplayName1 = "MetricA:A",
                    DisplayName2 = "MetricA:B",
                    CmsSeries = cmsSeries,
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2),
                    ActualSeriesCount = 3
                };

                await orchestrator.RenderPrimaryChart(context, chart, additionalSeries, additionalLabels);
                await FlushChartAsync(chart);

                cutOverService.Verify(service => service.CreateStrategy(
                        StrategyType.MultiMetric,
                        It.Is<ChartDataContext>(ctx =>
                            ctx.ActualSeriesCount == 3 &&
                            ctx.From == context.From &&
                            ctx.To == context.To &&
                            ctx.CmsSeries != null &&
                            ctx.CmsSeries.Count == 3),
                        It.Is<StrategyCreationParameters>(parameters =>
                            parameters.LegacySeries != null &&
                            parameters.LegacySeries.Count == 3 &&
                            parameters.CmsSeries != null &&
                            parameters.CmsSeries.Count == 3 &&
                            parameters.Labels != null &&
                            parameters.Labels.SequenceEqual(new[] { "MetricA:A", "MetricA:B", "MetricA:C" }) &&
                            parameters.From == context.From &&
                            parameters.To == context.To)),
                    Times.Once);

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
    public async Task RenderNormalizedChartAsync_ShouldUseNormalizedCutOver_AndRenderTrackedSeries()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var cutOverService = new Mock<IStrategyCutOverService>(MockBehavior.Strict);
                cutOverService
                    .Setup(service => service.CreateStrategy(
                        StrategyType.Normalized,
                        It.IsAny<ChartDataContext>(),
                        It.IsAny<StrategyCreationParameters>()))
                    .Returns(new StubStrategy(CreateSingleSeriesResult()));

                var orchestrator = CreateOrchestrator(chartTimestamps, tooltipManager, cutOverService, out _);
                var chartState = new ChartState
                {
                    SelectedNormalizationMode = NormalizationMode.PercentageOfMax
                };
                var context = CreateSecondaryContext();

                await orchestrator.RenderNormalizedChartAsync(context, chart, chartState);
                await FlushChartAsync(chart);

                cutOverService.Verify(service => service.CreateStrategy(
                        StrategyType.Normalized,
                        context,
                        It.Is<StrategyCreationParameters>(parameters =>
                            parameters.LegacyData1 == context.Data1 &&
                            parameters.LegacyData2 == context.Data2 &&
                            parameters.NormalizationMode == NormalizationMode.PercentageOfMax)),
                    Times.Once);
                Assert.NotEmpty(chart.Series);
                Assert.True(chartTimestamps.ContainsKey(chart));
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task RenderDiffRatioChartAsync_ShouldUseRatioCutOver_WhenRatioModeSelected()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var cutOverService = new Mock<IStrategyCutOverService>(MockBehavior.Strict);
                cutOverService
                    .Setup(service => service.CreateStrategy(
                        StrategyType.Ratio,
                        It.IsAny<ChartDataContext>(),
                        It.IsAny<StrategyCreationParameters>()))
                    .Returns(new StubStrategy(CreateSingleSeriesResult()));

                var orchestrator = CreateOrchestrator(chartTimestamps, tooltipManager, cutOverService, out _);
                var chartState = new ChartState
                {
                    IsDiffRatioVisible = true,
                    IsDiffRatioDifferenceMode = false
                };
                var context = CreateSecondaryContext();

                await orchestrator.RenderDiffRatioChartAsync(context, chart, chartState);
                await FlushChartAsync(chart);

                cutOverService.Verify(service => service.CreateStrategy(
                        StrategyType.Ratio,
                        context,
                        It.Is<StrategyCreationParameters>(parameters =>
                            parameters.LegacyData1 == context.Data1 &&
                            parameters.LegacyData2 == context.Data2)),
                    Times.Once);
                Assert.NotEmpty(chart.Series);
                Assert.True(chartTimestamps.ContainsKey(chart));
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    [Fact]
    public async Task RenderDistributionChartAsync_ShouldUseHourlyDistributionService_ForHourlyMode()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var chartTimestamps = new Dictionary<CartesianChart, List<DateTime>>();
            var (window, chart) = await CreateHostedChartAsync();
            var tooltipManager = new ChartTooltipManager(window);

            try
            {
                var weeklyService = new Mock<IDistributionService>(MockBehavior.Strict);
                var hourlyService = new Mock<IDistributionService>(MockBehavior.Strict);
                var cutOverService = new Mock<IStrategyCutOverService>(MockBehavior.Loose);
                hourlyService
                    .Setup(service => service.UpdateDistributionChartAsync(
                        It.IsAny<CartesianChart>(),
                        It.IsAny<IEnumerable<MetricData>>(),
                        It.IsAny<string>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<double>(),
                        It.IsAny<bool>(),
                        It.IsAny<int>(),
                        It.IsAny<ICanonicalMetricSeries>(),
                        It.IsAny<bool>()))
                    .Returns(Task.CompletedTask);

                var orchestrator = CreateOrchestrator(chartTimestamps, tooltipManager, cutOverService, out _, weeklyService.Object, hourlyService.Object);
                var chartState = new ChartState();
                var context = new ChartDataContext
                {
                    Data1 = TestDataBuilders.HealthMetricData().BuildSeries(2, TimeSpan.FromDays(1)),
                    DisplayName1 = "Primary",
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                };

                await orchestrator.RenderDistributionChartAsync(context, chart, chartState, DistributionMode.Hourly);

                hourlyService.Verify(service => service.UpdateDistributionChartAsync(
                    chart,
                    context.Data1,
                    context.DisplayName1,
                    context.From,
                    context.To,
                    400,
                    true,
                    chartState.GetDistributionSettings(DistributionMode.Hourly).IntervalCount,
                    null,
                    false), Times.Once);
                weeklyService.VerifyNoOtherCalls();
            }
            finally
            {
                tooltipManager.Dispose();
                window.Close();
            }
        });
    }

    private static MethodInfo GetPrivateStaticMethod(string name)
    {
        var method = typeof(ChartRenderingOrchestrator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!;
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

    private static ChartDataContext CreateSecondaryContext()
    {
        return new ChartDataContext
        {
            Data1 = TestDataBuilders.HealthMetricData().WithValue(10m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            Data2 = TestDataBuilders.HealthMetricData().WithValue(5m).WithTimestamp(new DateTime(2024, 1, 1)).BuildSeries(2, TimeSpan.FromDays(1)),
            DisplayName1 = "MetricA:A",
            DisplayName2 = "MetricA:B",
            MetricType = "MetricA",
            SecondaryMetricType = "MetricA",
            PrimarySubtype = "A",
            SecondarySubtype = "B",
            DisplayPrimaryMetricType = "MetricA",
            DisplaySecondaryMetricType = "MetricA",
            DisplayPrimarySubtype = "A",
            DisplaySecondarySubtype = "B",
            From = new DateTime(2024, 1, 1),
            To = new DateTime(2024, 1, 2)
        };
    }

    private static ChartRenderingOrchestrator CreateOrchestrator(
        Dictionary<CartesianChart, List<DateTime>> chartTimestamps,
        ChartTooltipManager tooltipManager,
        Mock<IStrategyCutOverService> cutOverService,
        out CapturingNotificationService notifications,
        IDistributionService? weeklyDistributionService = null,
        IDistributionService? hourlyDistributionService = null)
    {
        notifications = new CapturingNotificationService();
        var coordinator = new ChartUpdateCoordinator(
            new ChartComputationEngine(),
            new ChartRenderEngine(),
            tooltipManager,
            chartTimestamps,
            notifications);

        return new ChartRenderingOrchestrator(
            coordinator,
            weeklyDistributionService ?? Mock.Of<IDistributionService>(),
            hourlyDistributionService ?? Mock.Of<IDistributionService>(),
            cutOverService.Object,
            notifications);
    }

    private static ChartComputationResult CreateMultiSeriesResult()
    {
        var timestamps = new List<DateTime>
        {
            new(2024, 1, 1),
            new(2024, 1, 2)
        };

        return new ChartComputationResult
        {
            Timestamps = timestamps,
            PrimaryRawValues = [10d, 12d],
            PrimarySmoothed = [10d, 12d],
            Series =
            [
                new SeriesResult
                {
                    SeriesId = "a",
                    DisplayName = "MetricA:A",
                    Timestamps = timestamps,
                    RawValues = [10d, 12d],
                    Smoothed = [10d, 12d]
                },
                new SeriesResult
                {
                    SeriesId = "b",
                    DisplayName = "MetricA:B",
                    Timestamps = timestamps,
                    RawValues = [20d, 24d],
                    Smoothed = [20d, 24d]
                },
                new SeriesResult
                {
                    SeriesId = "c",
                    DisplayName = "MetricA:C",
                    Timestamps = timestamps,
                    RawValues = [15d, 18d],
                    Smoothed = [15d, 18d]
                }
            ]
        };
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
