using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;

namespace DataVisualiser.Tests.Charts.Parity;

public sealed class WeekdayTrendCmsLegacyConsistencyTests
{
    [Fact]
    public void WeekdayTrend_Cms_and_Legacy_ShouldProduceEquivalentExtendedResult()
    {
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(2024, 01, 01, 12, 0, 0));
        var start = new DateTimeOffset(2024, 01, 01, 12, 0, 0, localOffset);
        var interval = TimeSpan.FromDays(1);
        var sampleCount = 10;

        var cms = TestDataBuilders.CanonicalMetricSeries()
            .WithMetricId("metric.weekday")
            .WithStartTime(start)
            .WithInterval(interval)
            .WithValue(100m)
            .WithUnit("kg")
            .WithSampleCount(sampleCount)
            .Build();

        var legacy = new List<MetricData>();
        var current = start;
        for (var i = 0; i < sampleCount; i++)
        {
            legacy.Add(TestDataBuilders.HealthMetricData().WithTimestamp(current.DateTime).WithValue(100m).WithUnit("kg").Build());
            current = current.Add(interval);
        }

        var from = start.DateTime;
        var to = start.AddDays(sampleCount - 1).DateTime;

        var legacyStrategy = new WeekdayTrendComputationStrategy(legacy, "weight", from, to);
        var cmsStrategy = new WeekdayTrendComputationStrategy(cms, "weight", from, to);

        legacyStrategy.Compute();
        cmsStrategy.Compute();

        var legacyResult = AssertExtendedResult(legacyStrategy.ExtendedResult);
        var cmsResult = AssertExtendedResult(cmsStrategy.ExtendedResult);

        Assert.Equal(legacyResult.Unit, cmsResult.Unit);
        Assert.Equal(legacyResult.GlobalMin, cmsResult.GlobalMin);
        Assert.Equal(legacyResult.GlobalMax, cmsResult.GlobalMax);
        Assert.Equal(legacyResult.SeriesByDay.Keys.OrderBy(k => k), cmsResult.SeriesByDay.Keys.OrderBy(k => k));

        foreach (var dayKey in legacyResult.SeriesByDay.Keys.OrderBy(k => k))
        {
            var legacySeries = legacyResult.SeriesByDay[dayKey];
            var cmsSeries = cmsResult.SeriesByDay[dayKey];

            Assert.Equal(legacySeries.Day, cmsSeries.Day);
            Assert.Equal(legacySeries.Points.Count, cmsSeries.Points.Count);

            for (var i = 0; i < legacySeries.Points.Count; i++)
            {
                Assert.Equal(legacySeries.Points[i].Date, cmsSeries.Points[i].Date);
                Assert.Equal(legacySeries.Points[i].Value, cmsSeries.Points[i].Value);
                Assert.Equal(legacySeries.Points[i].SampleCount, cmsSeries.Points[i].SampleCount);
            }
        }
    }

    private static WeekdayTrendResult AssertExtendedResult(WeekdayTrendResult? result)
    {
        Assert.NotNull(result);
        return result!;
    }
}
