using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Tests.Services;

public sealed class TimeBucketAggregationHelperTests
{
    [Fact]
    public void BuildAverageTotals_ForMetricData_ShouldAveragePerBucket()
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0);
        var to = from.AddHours(4);
        var bucketTicks = (to - from).Ticks / 2d;
        var data = new[]
        {
            new MetricData { NormalizedTimestamp = from.AddMinutes(10), Value = 10m },
            new MetricData { NormalizedTimestamp = from.AddMinutes(50), Value = 20m },
            new MetricData { NormalizedTimestamp = from.AddHours(2).AddMinutes(10), Value = 30m },
            new MetricData { NormalizedTimestamp = from.AddHours(2).AddMinutes(20), Value = 50m }
        };

        var totals = TimeBucketAggregationHelper.BuildAverageTotals(data, from, to, bucketTicks, 2);

        Assert.Equal(15d, totals[0]);
        Assert.Equal(40d, totals[1]);
    }

    [Theory]
    [InlineData(-1, 2, -1)]
    [InlineData(0, 2, 0)]
    [InlineData(1, 2, 1)]
    [InlineData(2, 2, 1)]
    public void ResolveBucketIndex_ShouldClampToExpectedBucket(int hoursOffset, int bucketCount, int expectedIndex)
    {
        var from = new DateTime(2026, 1, 1, 0, 0, 0);
        var to = from.AddHours(2);
        var bucketTicks = (to - from).Ticks / (double)bucketCount;
        var timestamp = from.AddHours(hoursOffset);

        var index = TimeBucketAggregationHelper.ResolveBucketIndex(timestamp, from, to, bucketTicks, bucketCount);

        Assert.Equal(expectedIndex, index);
    }
}
