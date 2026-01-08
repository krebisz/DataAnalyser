using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Strategies;

public sealed class WeekdayTrendComputationStrategyTests
{
    private static readonly DateTime From = new(2024, 01, 01);
    private static readonly DateTime To = new(2024, 01, 08);

    [Fact]
    public void Compute_ShouldPopulateExtendedResult_ForLegacyData()
    {
        var data = new List<MetricData>
        {
                new()
                {
                        NormalizedTimestamp = From,
                        Value = 10m,
                        Unit = "kg"
                },
                new()
                {
                        NormalizedTimestamp = From.AddDays(1),
                        Value = 20m,
                        Unit = "kg"
                }
        };

        var strategy = new WeekdayTrendComputationStrategy(data, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(strategy.ExtendedResult);
        Assert.True(strategy.ExtendedResult!.SeriesByDay.Count > 0);
        Assert.Equal("kg", strategy.Unit);
    }

    [Fact]
    public void Compute_ShouldPopulateExtendedResult_ForCmsData()
    {
        var cms = TestDataBuilders.CanonicalMetricSeries().WithMetricId("metric.test.weekday").WithUnit("kg").WithStartTime(new DateTimeOffset(From, TimeSpan.Zero)).WithInterval(TimeSpan.FromDays(1)).WithSampleCount(5).Build();

        var strategy = new WeekdayTrendComputationStrategy(cms, "Test", From, To);

        var result = strategy.Compute();

        Assert.NotNull(result);
        Assert.NotNull(strategy.ExtendedResult);
        Assert.Equal("kg", strategy.Unit);
        Assert.True(strategy.ExtendedResult!.SeriesByDay.Count > 0);
    }

    [Fact]
    public void Compute_ShouldExposeExtendedResultViaInterface()
    {
        var data = TestDataBuilders.HealthMetricData().WithTimestamp(From).WithUnit("kg").WithValue(5m).BuildSeries(3, TimeSpan.FromDays(1));

        IChartComputationStrategy strategy = new WeekdayTrendComputationStrategy(data, "Test", From, To);

        strategy.Compute();

        var provider = Assert.IsAssignableFrom<IWeekdayTrendResultProvider>(strategy);
        Assert.NotNull(provider.ExtendedResult);
    }
}