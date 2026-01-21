using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Validation.Parity;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using Moq;

namespace DataVisualiser.Tests.Strategies;

public sealed class StrategyCutOverServiceTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 10);

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_WhenGlobalDisabled()
    {
        using var _ = new CmsConfigurationScope(false, true);
        var service = CreateService();
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
        using var _ = new CmsConfigurationScope(true, true);
        var service = CreateService();
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
        using var _ = new CmsConfigurationScope(true, true);
        var service = CreateService();
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
        using var _ = new CmsConfigurationScope(false, true);
        var service = CreateService();
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
        using var _ = new CmsConfigurationScope(true, true, false);
        var service = CreateService();

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

    [Fact]
    public void GetParityHarness_ShouldReturnChartComputationHarness_ForCoreStrategies()
    {
        var service = CreateService();
        var method = typeof(StrategyCutOverService).GetMethod("GetParityHarness", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
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
            var harness = method!.Invoke(service, new object?[] { strategyType });
            Assert.IsType<ChartComputationParityHarness>(harness);
        }
    }

    private static StrategyCutOverService CreateService()
    {
        var dataPreparation = new Mock<IDataPreparationService>();
        return new StrategyCutOverService(dataPreparation.Object);
    }

    private sealed class CmsConfigurationScope : IDisposable
    {
        private readonly bool _useCmsData;
        private readonly bool _useCmsForSingleMetric;
        private readonly bool _useCmsForCombinedMetric;

        public CmsConfigurationScope(bool useCmsData, bool useCmsForSingleMetric, bool useCmsForCombinedMetric = true)
        {
            _useCmsData = CmsConfiguration.UseCmsData;
            _useCmsForSingleMetric = CmsConfiguration.UseCmsForSingleMetric;
            _useCmsForCombinedMetric = CmsConfiguration.UseCmsForCombinedMetric;

            CmsConfiguration.UseCmsData = useCmsData;
            CmsConfiguration.UseCmsForSingleMetric = useCmsForSingleMetric;
            CmsConfiguration.UseCmsForCombinedMetric = useCmsForCombinedMetric;
        }

        public void Dispose()
        {
            CmsConfiguration.UseCmsData = _useCmsData;
            CmsConfiguration.UseCmsForSingleMetric = _useCmsForSingleMetric;
            CmsConfiguration.UseCmsForCombinedMetric = _useCmsForCombinedMetric;
        }
    }
}
