using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Strategies.Reachability;
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
    [InlineData(StrategyType.CombinedMetric)]
    [InlineData(StrategyType.Normalized)]
    [InlineData(StrategyType.Difference)]
    [InlineData(StrategyType.Ratio)]
    public void ShouldUseCms_ShouldReturnFalse_ForBinaryStrategies_WhenSecondaryCmsMissing(StrategyType strategyType)
    {
        var service = CreateService(useCmsData: true, useCmsForCombinedMetric: true, useCmsForNormalized: true, useCmsForDifference: true, useCmsForRatio: true);

        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            SecondaryCms = null,
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(strategyType, ctx);

        Assert.False(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_ForMultiMetric_WhenCmsSeriesCollectionIsMissing()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);

        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            SecondaryCms = TestDataBuilders.CanonicalMetricSeries().WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(2).Build(),
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.MultiMetric, ctx);

        Assert.False(result);
    }

    [Theory]
    [InlineData(StrategyType.Normalized)]
    [InlineData(StrategyType.Difference)]
    [InlineData(StrategyType.Ratio)]
    public void ShouldUseCms_ShouldReturnTrue_ForBinaryStrategies_WhenPrimaryAndSecondaryCmsArePresent(StrategyType strategyType)
    {
        var service = CreateService(useCmsData: true, useCmsForNormalized: true, useCmsForDifference: true, useCmsForRatio: true);

        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(5).Build(),
            SecondaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(5).Build(),
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(strategyType, ctx);

        Assert.True(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnTrue_ForMultiMetric_WhenRealCmsSeriesArePresent()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var ctx = new ChartDataContext
        {
            CmsSeries =
            [
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build()
            ],
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.MultiMetric, ctx);

        Assert.True(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnTrue_ForMultiMetric_WhenCmsSeriesShareADimension()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var ctx = new ChartDataContext
        {
            CmsSeries =
            [
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build()
            ],
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.MultiMetric, ctx);

        Assert.True(result);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_ForMultiMetric_WhenCmsSeriesHaveDifferentDimensions()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var ctx = new ChartDataContext
        {
            CmsSeries =
            [
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_weight").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("sleep.duration").WithUnit("hours").WithDimension(DataFileReader.Canonical.MetricDimension.Duration).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build(),
                TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithSampleCount(5).Build()
            ],
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.MultiMetric, ctx);

        Assert.False(result);
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
    public void CreateStrategy_ShouldPreferCms_ForNormalized_WhenCmsIsEligible()
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
    public void CreateStrategy_ShouldPreferCms_ForDifference_WhenCmsIsEligible()
    {
        var service = CreateService(useCmsData: true, useCmsForDifference: true);
        var primaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build();
        var secondaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(4m).WithSampleCount(5).Build();

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
            LegacyData2 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(4m).BuildSeries(2, TimeSpan.FromDays(1)),
            Label1 = "Left",
            Label2 = "Right",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.Difference, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<DifferenceStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForRatio_WhenCmsIsEligible()
    {
        var service = CreateService(useCmsData: true, useCmsForRatio: true);
        var primaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build();
        var secondaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(2m).WithSampleCount(5).Build();

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
            LegacyData2 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(2m).BuildSeries(2, TimeSpan.FromDays(1)),
            Label1 = "Left",
            Label2 = "Right",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.Ratio, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<RatioStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForMultiMetric_WhenCompleteCmsSeriesAreProvided()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.multi").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacySeries =
            [
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).BuildSeries(2, TimeSpan.FromDays(1))
            ],
            CmsSeries = cmsSeries,
            Labels = ["A", "B", "C"],
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<MultiMetricStrategy>(strategy);
        Assert.NotNull(result);
        Assert.NotNull(result!.Series);
        Assert.Equal(3, result.Series!.Count);
        Assert.Equal(5, result.Series[0].RawValues.Count);
    }

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_WhenCmsSamplesFallOutsideRequestedRange()
    {
        var service = CreateService(useCmsData: true, useCmsForSingleMetric: true);
        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries()
                .WithStartTime(new DateTimeOffset(From.AddDays(-20), TimeSpan.Zero))
                .WithInterval(TimeSpan.FromDays(1))
                .WithSampleCount(3)
                .Build(),
            From = From,
            To = To
        };

        var result = service.ShouldUseCms(StrategyType.SingleMetric, ctx);

        Assert.False(result);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForCombinedMetric_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForCombinedMetric: true);
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
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.CombinedMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.IsType<CombinedMetricStrategy>(strategy);
        Assert.NotNull(result);
        Assert.Equal(5, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferLegacy_ForCombinedMetric_WhenCmsDisabled()
    {
        var service = CreateService(useCmsData: false, useCmsForCombinedMetric: true);
        var primaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.left").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build();
        var secondaryCms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.right").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build();
        var legacyLeft = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1));
        var legacyRight = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1));

        var ctx = new ChartDataContext
        {
            PrimaryCms = primaryCms,
            SecondaryCms = secondaryCms,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = legacyLeft,
            LegacyData2 = legacyRight,
            Label1 = "Left",
            Label2 = "Right",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.CombinedMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.Equal(legacyLeft.Count, result!.PrimaryRawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForWeeklyDistribution_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForWeeklyDistribution: true);
        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries()
                .WithMetricId("metric.weekly")
                .WithStartTime(new DateTimeOffset(From, TimeSpan.Zero))
                .WithInterval(TimeSpan.FromDays(1))
                .WithValue(10m)
                .WithSampleCount(5)
                .Build(),
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
            Label1 = "Weekly",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.WeeklyDistribution, ctx, parameters);

        Assert.IsType<CmsWeeklyDistributionStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForMultiMetric_WhenFourCompatibleMassSeriesAreProvided()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.muscle_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(40m).WithSampleCount(5).Build()
        };

        var legacySeries = new[]
        {
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(40m).BuildSeries(2, TimeSpan.FromDays(1))
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacySeries = legacySeries,
            CmsSeries = cmsSeries,
            Labels = ["Fat", "Water", "Skeletal", "Muscle"],
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.Series);
        Assert.Equal(4, result.Series!.Count);
        Assert.Equal(5, result.Series[0].RawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferLegacy_ForMultiMetric_WhenCmsIsDisabled()
    {
        var service = CreateService(useCmsData: false, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.muscle_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(40m).WithSampleCount(5).Build()
        };
        var legacySeries = new[]
        {
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(40m).BuildSeries(2, TimeSpan.FromDays(1))
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacySeries = legacySeries,
            CmsSeries = cmsSeries,
            Labels = ["Fat", "Water", "Skeletal", "Muscle"],
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.Series);
        Assert.Equal(4, result.Series!.Count);
        Assert.Equal(2, result.Series[0].RawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferLegacy_ForMultiMetric_WhenCmsSeriesHaveDifferentDimensions()
    {
        var service = CreateService(useCmsData: true, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_weight").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("sleep.duration").WithUnit("hours").WithDimension(DataFileReader.Canonical.MetricDimension.Duration).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("body.fat_percentage").WithUnit("%").WithDimension(DataFileReader.Canonical.MetricDimension.Percentage).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };
        var legacySeries = new[]
        {
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).WithUnit("kg").BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).WithUnit("hours").BuildSeries(2, TimeSpan.FromDays(1)),
            TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).WithUnit("%").BuildSeries(2, TimeSpan.FromDays(1))
        };

        var ctx = new ChartDataContext
        {
            CmsSeries = cmsSeries,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacySeries = legacySeries,
            CmsSeries = cmsSeries,
            Labels = ["Weight", "Sleep", "Fat %"],
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(result!.Series);
        Assert.Equal(3, result.Series!.Count);
        Assert.Equal(2, result.Series[0].RawValues.Count);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForHourlyDistribution_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForHourlyDistribution: true);
        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries()
                .WithMetricId("metric.hourly")
                .WithStartTime(new DateTimeOffset(From, TimeSpan.Zero))
                .WithInterval(TimeSpan.FromHours(1))
                .WithValue(10m)
                .WithSampleCount(24)
                .Build(),
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromHours(1)),
            Label1 = "Hourly",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.HourlyDistribution, ctx, parameters);

        Assert.IsType<CmsHourlyDistributionStrategy>(strategy);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_ForWeekdayTrend_WhenEnabled()
    {
        var service = CreateService(useCmsData: true, useCmsForWeekdayTrend: true);
        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries()
                .WithMetricId("metric.weekday")
                .WithStartTime(new DateTimeOffset(From, TimeSpan.Zero))
                .WithInterval(TimeSpan.FromDays(1))
                .WithValue(10m)
                .WithSampleCount(5)
                .Build(),
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
            Label1 = "Weekday",
            From = From,
            To = To
        };

        var strategy = service.CreateStrategy(StrategyType.WeekdayTrend, ctx, parameters);
        strategy.Compute();

        Assert.Equal(5, CountWeekdayTrendPoints(strategy));
    }

    [Fact]
    public void GetParityHarness_ShouldReturnChartComputationHarness_ForCoreStrategies()
    {
        var service = new StrategyParityValidationService();

        var types = new[]
        {
                StrategyType.SingleMetric,
                StrategyType.MultiMetric,
                StrategyType.Normalized,
                StrategyType.WeekdayTrend
        };

        foreach (var strategyType in types)
        {
            var harness = service.GetParityHarness(strategyType);
            Assert.IsType<ChartComputationParityHarness>(harness);
        }
    }

    [Fact]
    public void CreateStrategy_ShouldRecordReachabilityReason_WhenMultiMetricUsesCms()
    {
        var probe = new CapturingReachabilityProbe();
        var service = CreateService(reachabilityProbe: probe, useCmsData: true, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_fat_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.total_body_water").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.skeletal_mass").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };

        var ctx = new ChartDataContext { CmsSeries = cmsSeries, ActualSeriesCount = 3, From = From, To = To };
        var parameters = new StrategyCreationParameters
        {
            LegacySeries =
            [
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).BuildSeries(2, TimeSpan.FromDays(1))
            ],
            CmsSeries = cmsSeries,
            Labels = ["Fat", "Water", "Skeletal"],
            From = From,
            To = To
        };

        service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);

        var record = Assert.Single(probe.Records);
        Assert.True(record.UsedCms);
        Assert.True(record.CmsRequested);
        Assert.True(record.RealCmsSupported);
        Assert.Equal(3, record.CmsSeriesCount);
        Assert.Equal(3, record.ActualSeriesCount);
        Assert.Equal("Compatible CMS multi-series data available", record.DecisionReason);
    }

    [Fact]
    public void CreateStrategy_ShouldRecordReachabilityReason_WhenMultiMetricFallsBackToLegacy()
    {
        var probe = new CapturingReachabilityProbe();
        var service = CreateService(reachabilityProbe: probe, useCmsData: true, useCmsForMultiMetric: true);
        var cmsSeries = new[]
        {
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("weight.body_weight").WithUnit("kg").WithDimension(DataFileReader.Canonical.MetricDimension.Mass).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(10m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("sleep.duration").WithUnit("hours").WithDimension(DataFileReader.Canonical.MetricDimension.Duration).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(20m).WithSampleCount(5).Build(),
            TestDataBuilders.CanonicalMetricSeries().WithMetricId("body.fat_percentage").WithUnit("%").WithDimension(DataFileReader.Canonical.MetricDimension.Percentage).WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithValue(30m).WithSampleCount(5).Build()
        };

        var ctx = new ChartDataContext { CmsSeries = cmsSeries, ActualSeriesCount = 3, From = From, To = To };
        var parameters = new StrategyCreationParameters
        {
            LegacySeries =
            [
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(10m).WithUnit("kg").BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(20m).WithUnit("hours").BuildSeries(2, TimeSpan.FromDays(1)),
                TestDataBuilders.HealthMetricData().WithTimestamp(From).WithValue(30m).WithUnit("%").BuildSeries(2, TimeSpan.FromDays(1))
            ],
            CmsSeries = cmsSeries,
            Labels = ["Weight", "Sleep", "Fat %"],
            From = From,
            To = To
        };

        service.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);

        var record = Assert.Single(probe.Records);
        Assert.False(record.UsedCms);
        Assert.True(record.CmsRequested);
        Assert.Contains("incompatible dimensions", record.DecisionReason, StringComparison.OrdinalIgnoreCase);
    }

    private static StrategyCutOverService CreateService(
        IStrategyReachabilityProbe? reachabilityProbe = null,
        bool useCmsData = true,
        bool useCmsForSingleMetric = true,
        bool useCmsForMultiMetric = true,
        bool useCmsForCombinedMetric = true,
        bool useCmsForDifference = true,
        bool useCmsForRatio = true,
        bool useCmsForNormalized = true,
        bool useCmsForWeeklyDistribution = true,
        bool useCmsForWeekdayTrend = true,
        bool useCmsForHourlyDistribution = true)
    {
        var dataPreparation = new Mock<IDataPreparationService>();
        return new StrategyCutOverService(dataPreparation.Object, reachabilityProbe, new TestCmsRuntimeConfiguration
        {
            UseCmsData = useCmsData,
            UseCmsForSingleMetric = useCmsForSingleMetric,
            UseCmsForMultiMetric = useCmsForMultiMetric,
            UseCmsForCombinedMetric = useCmsForCombinedMetric,
            UseCmsForDifference = useCmsForDifference,
            UseCmsForRatio = useCmsForRatio,
            UseCmsForNormalized = useCmsForNormalized,
            UseCmsForWeeklyDistribution = useCmsForWeeklyDistribution,
            UseCmsForWeekdayTrend = useCmsForWeekdayTrend,
            UseCmsForHourlyDistribution = useCmsForHourlyDistribution
        });
    }

    private static int CountWeekdayTrendPoints(IChartComputationStrategy strategy)
    {
        var provider = Assert.IsAssignableFrom<IWeekdayTrendResultProvider>(strategy);
        Assert.NotNull(provider.ExtendedResult);
        return provider.ExtendedResult!.SeriesByDay.Values.Sum(series => series.Points.Count);
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

    private sealed class CapturingReachabilityProbe : IStrategyReachabilityProbe
    {
        public List<StrategyReachabilityRecord> Records { get; } = new();

        public void Record(StrategyReachabilityRecord record)
        {
            Records.Add(record);
        }
    }
}
