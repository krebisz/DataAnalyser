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
    public void BuildInitialSeriesList_ShouldIncludeSecondaryWhenPresent()
    {
        var method = GetPrivateStaticMethod("BuildInitialSeriesList");

        var data1 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1));
        var data2 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1));

        var result = method.Invoke(null,
                new object?[]
                {
                        data1,
                        data2,
                        "A",
                        "B"
                })!;
        var typed = ((List<IEnumerable<MetricData>> series, List<string> labels))result;

        Assert.Equal(2, typed.series.Count);
        Assert.Equal(new[]
                {
                        "A",
                        "B"
                },
                typed.labels);
    }

    [Fact]
    public void BuildSeriesAndLabels_ShouldSkipEmptyAdditionalSeries()
    {
        var method = GetPrivateStaticMethod("BuildSeriesAndLabels");

        var ctx = new ChartDataContext
        {
                Data1 = TestDataBuilders.HealthMetricData().BuildSeries(1, TimeSpan.FromDays(1)),
                DisplayName1 = "A"
        };

        var additionalSeries = new List<IEnumerable<MetricData>>
        {
                new List<MetricData>()
        };
        var additionalLabels = new List<string>
        {
                "C"
        };

        var result = method.Invoke(null,
                new object?[]
                {
                        ctx,
                        additionalSeries,
                        additionalLabels
                })!;
        var typed = ((List<IEnumerable<MetricData>> series, List<string> labels))result;

        Assert.Single(typed.series);
        Assert.Equal(new[]
                {
                        "A"
                },
                typed.labels);
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
                    cutOverService.Object);

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

                cutOverService.Verify(service => service.CreateStrategy(
                        StrategyType.MultiMetric,
                        It.Is<ChartDataContext>(ctx => ctx == context),
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

    private static MethodInfo GetPrivateStaticMethod(string name)
    {
        var method = typeof(ChartRenderingOrchestrator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);
        return method!;
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
