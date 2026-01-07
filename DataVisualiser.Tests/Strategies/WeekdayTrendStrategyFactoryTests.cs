using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Factories;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class WeekdayTrendStrategyFactoryTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To   = new(2024, 01, 08);

    [Fact]
    public void CreateLegacyStrategy_ShouldReturnComputationStrategy()
    {
        var factory = new WeekdayTrendStrategyFactory();
        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = new List<MetricData>
            {
                new()
                {
                    NormalizedTimestamp = From,
                    Value = 10m,
                    Unit = "kg"
                }
            },
            Label1 = "Test",
            From = From,
            To = To
        };

        var strategy = factory.CreateLegacyStrategy(parameters);
        strategy.Compute();

        var provider = Assert.IsAssignableFrom<IWeekdayTrendResultProvider>(strategy);
        Assert.NotNull(provider.ExtendedResult);
    }

    [Fact]
    public void CreateCmsStrategy_ShouldReturnComputationStrategy()
    {
        var cms = TestDataBuilders.CanonicalMetricSeries().
                                   WithMetricId("metric.test.weekday").
                                   WithUnit("kg").
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
            Label1 = "Test",
            From = From,
            To = To
        };

        var factory = new WeekdayTrendStrategyFactory();
        var strategy = factory.CreateCmsStrategy(ctx, parameters);
        strategy.Compute();

        var provider = Assert.IsAssignableFrom<IWeekdayTrendResultProvider>(strategy);
        Assert.NotNull(provider.ExtendedResult);
    }
}
