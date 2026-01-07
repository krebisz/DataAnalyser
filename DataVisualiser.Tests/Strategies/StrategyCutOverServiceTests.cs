using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using Moq;

namespace DataVisualiser.Tests.Strategies;

public sealed class StrategyCutOverServiceTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To   = new(2024, 01, 10);

    [Fact]
    public void ShouldUseCms_ShouldReturnFalse_WhenGlobalDisabled()
    {
        using var _ = new CmsConfigurationScope(useCmsData: false, useCmsForSingleMetric: true);
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
        using var _ = new CmsConfigurationScope(useCmsData: true, useCmsForSingleMetric: true);
        var service = CreateService();
        var ctx = new ChartDataContext
        {
            PrimaryCms = TestDataBuilders.CanonicalMetricSeries().Build()
        };

        var result = service.ShouldUseCms(StrategyType.SingleMetric, ctx);

        Assert.True(result);
    }

    [Fact]
    public void CreateStrategy_ShouldPreferCms_WhenEnabled()
    {
        using var _ = new CmsConfigurationScope(useCmsData: true, useCmsForSingleMetric: true);
        var service = CreateService();
        var cms = TestDataBuilders.CanonicalMetricSeries().
                                   WithMetricId("metric.test").
                                   WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).
                                   WithInterval(TimeSpan.FromDays(1)).
                                   WithSampleCount(5).
                                   Build();

        var ctx = new ChartDataContext
        {
            PrimaryCms = cms,
            From = From,
            To = To
        };

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = TestDataBuilders.HealthMetricData().
                                         WithTimestamp(From).
                                         BuildSeries(2, TimeSpan.FromDays(1)),
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
        using var _ = new CmsConfigurationScope(useCmsData: false, useCmsForSingleMetric: true);
        var service = CreateService();
        var cms = TestDataBuilders.CanonicalMetricSeries().
                                   WithMetricId("metric.test").
                                   WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).
                                   WithInterval(TimeSpan.FromDays(1)).
                                   WithSampleCount(5).
                                   Build();

        var legacy = TestDataBuilders.HealthMetricData().
                                     WithTimestamp(From).
                                     BuildSeries(2, TimeSpan.FromDays(1));

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

    private static StrategyCutOverService CreateService()
    {
        var dataPreparation = new Mock<IDataPreparationService>();
        return new StrategyCutOverService(dataPreparation.Object);
    }

    private sealed class CmsConfigurationScope : IDisposable
    {
        private readonly bool _useCmsData;
        private readonly bool _useCmsForSingleMetric;

        public CmsConfigurationScope(bool useCmsData, bool useCmsForSingleMetric)
        {
            _useCmsData = CmsConfiguration.UseCmsData;
            _useCmsForSingleMetric = CmsConfiguration.UseCmsForSingleMetric;

            CmsConfiguration.UseCmsData = useCmsData;
            CmsConfiguration.UseCmsForSingleMetric = useCmsForSingleMetric;
        }

        public void Dispose()
        {
            CmsConfiguration.UseCmsData = _useCmsData;
            CmsConfiguration.UseCmsForSingleMetric = _useCmsForSingleMetric;
        }
    }
}
