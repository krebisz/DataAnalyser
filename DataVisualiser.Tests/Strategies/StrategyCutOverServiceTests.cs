using System.Reflection;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Validation.Parity;
using DataVisualiser.Tests.Helpers;
using Moq;

namespace DataVisualiser.Tests.Strategies;

public sealed class StrategyCutOverServiceTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_WhenGlobalDisabled()
    {
        var service = CreateService(useCmsData: false, useCmsForSingleMetric: true);
        var ctx = new ChartDataContext
        {
                PrimaryCms = TestDataBuilders.CanonicalMetricSeries().Build()
        };

        var result = service.ShouldUseCms(StrategyType.SingleMetric, ctx);

        Assert.False(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnTrue_WhenEnabledAndCmsPresent()
    {
        var service = CreateService(useCmsData: true, useCmsForSingleMetric: true);
        var from = new DateTime(2024, 01, 01);
        var to = from.AddDays(1);
        var ctx = new ChartDataContext
        {
                PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(from, TimeSpan.Zero)).Build(),
                From = from,
                To = to
        };

        var result = service.ShouldUseCms(StrategyType.SingleMetric, ctx);

        Assert.True(result);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForSingleMetric: true);
        var cms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(5).Build();

        var ctx = new ChartDataContext
        {
                PrimaryCms = cms,
                From = From,
                To = To
        };

        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = TestDataBuilders.HealthMetricData().WithTimestamp(From).BuildSeries(2, TimeSpan.FromDays(1)),
                Label1 = "Test",
                From = From,
                To = To
        };

        var strategy = service.CreateStrategy(StrategyType.SingleMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<SingleMetricStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferLegacy_WhenGlobalDisabled()
    {
        var service = CreateService(useCmsData: false, useCmsForSingleMetric: true);
        var cms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(5).Build();

        var legacy = TestDataBuilders.HealthMetricData().WithTimestamp(From).BuildSeries(2, TimeSpan.FromDays(1));

        var ctx = new ChartDataContext
        {
                PrimaryCms = cms,
                From = From,
                To = To
        };

        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = legacy,
                Label1 = "Test",
                From = From,
                To = To
        };

        var strategy = service.CreateStrategy(StrategyType.SingleMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<SingleMetricStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(legacy.Count, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_WhenCombinedMetricFlagDisabled()
    {
        var service = CreateService(useCmsData: true, useCmsForSingleMetric: true, useCmsForCombinedMetric: false);

        var ctx = new ChartDataContext
        {
                PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
                SecondaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
                From = From,
                To = To
        };

        var result = service.ShouldUseCms(StrategyType.CombinedMetric, ctx);

        Assert.False(result);
    }

    [Theory]
    [InlineData(StrategyType.Normalized)]
    [InlineData(StrategyType.Difference)]
    [InlineData(StrategyType.Ratio)]
    public void ShouldUseCms_ShouldReturnTrue_ForBinaryStrategies_WhenEnabledAndCmsPresent(StrategyType strategyType)
    {
        var service = CreateService(useCmsData: true, useCmsForNormalized: true, useCmsForDifference: true, useCmsForRatio: true);

        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            SecondaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(strategyType, ctx);

        Assert.True(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_ForNormalized_WhenFlagDisabled()
    {
        var service = CreateService(useCmsData: true, useCmsForNormalized: false);

        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            SecondaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.Normalized, ctx);

        Assert.False(result);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForNormalized_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForNormalized: true);
        var primaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build();
        var secondaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build();

        var ctx = new ChartDataContext
        {
            PrimaryCms = primaryCms,
            SecondaryCms = secondaryCms,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
            LegacyData2 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1)),
            Label1 = "Left",
            Label2 = "Right",
            From = From,
            To = To,
            NormalizationMode = Shared.Models.NormalizationMode.ZeroToOne
        };

        var strategy = service.CreateStrategy(StrategyType.Normalized, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<NormalizedStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void GetParityHarness_ShouldReturnChartComputationHarness_ForCoreStrategies()
    {
        var service = CreateService();
        var method = typeof(StrategyCutOverService).GetMethod("GetParityHarness", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var types = new[]
        {
                StrategyType.SingleMetric,
                StrategyType.MultiMetric,
                StrategyType.Normalized,
                StrategyType.WeekdayTrend
        };

        foreach (var strategyType in types)
        {
            var harness = method!.Invoke(service,
                    new object?[]
                    {
                            strategyType
                    });
            Assert.IsType<ChartComputationParityHarness>(harness);
        }
    }

    private static StrategyCutOverService CreateService(
        bool useCmsData = true,
        bool useCmsForSingleMetric = true,
        bool useCmsForCombinedMetric = true,
        bool useCmsForDifference = true,
        bool useCmsForRatio = true,
        bool useCmsForNormalized = true)
    {
        var dataPreparation = new Mock<IDataPreparationService>();
        return new StrategyCutOverService(dataPreparation.Object, cmsRuntimeConfiguration: new TestCmsRuntimeConfiguration
        {
            UseCmsData = useCmsData,
            UseCmsForSingleMetric = useCmsForSingleMetric,
            UseCmsForCombinedMetric = useCmsForCombinedMetric,
            UseCmsForDifference = useCmsForDifference,
            UseCmsForRatio = useCmsForRatio,
            UseCmsForNormalized = useCmsForNormalized
        });
    }

    private sealed class TestCmsRuntimeConfiguration : ICmsRuntimeConfiguration
    {
        public bool UseCmsData { get; set; } = true;
        public bool UseCmsForSingleMetric { get; set; } = true;
        public bool UseCmsForMultiMetric { get; set; } = true;
        public bool UseCmsForCombinedMetric { get; set; } = true;
        public bool UseCmsForDifference { get; set; } = true;
        public bool UseCmsForRatio { get; set; } = true;
        public bool UseCmsForNormalized { get; set; } = true;
        public bool UseCmsForWeeklyDistribution { get; set; } = true;
        public bool UseCmsForWeekdayTrend { get; set; } = true;
        public bool UseCmsForHourlyDistribution { get; set; } = true;
        public bool UseCmsForBarPie { get; set; } = true;

        public bool ShouldUseCms(string strategyType)
        {
            if (!UseCmsData)
                return false;

            return strategyType switch
            {
                    "SingleMetricStrategy" => UseCmsForSingleMetric,
                    "MultiMetricStrategy" => UseCmsForMultiMetric,
                    "CombinedMetricStrategy" => UseCmsForCombinedMetric,
                    "DifferenceStrategy" => UseCmsForDifference,
                    "RatioStrategy" => UseCmsForRatio,
                    "NormalizedStrategy" => UseCmsForNormalized,
                    "WeeklyDistributionStrategy" => UseCmsForWeeklyDistribution,
                    "WeekdayTrendStrategy" => UseCmsForWeekdayTrend,
                    "HourlyDistributionStrategy" => UseCmsForHourlyDistribution,
                    "BarPieStrategy" => UseCmsForBarPie,
                    _ => false
            };
        }
    }
}
