using DataVisualiser.Core.Computation.BucketedSeries;

namespace DataVisualiser.Tests.Core.Computation;

public sealed class BucketedSeriesBucketPlanBuilderTests
{
    [Fact]
    public void ResolveBucketCount_WhenDateRangeIsNotForward_ReturnsOne()
    {
        var from = new DateTime(2026, 1, 2);
        var to = new DateTime(2026, 1, 1);

        var count = BucketedSeriesBucketPlanBuilder.ResolveBucketCount(from, to, requestedBucketCount: 10);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ResolveBucketCount_ClampsRequestedCountToConfiguredBounds()
    {
        var from = new DateTime(2026, 1, 1);
        var to = new DateTime(2026, 1, 31);

        Assert.Equal(1, BucketedSeriesBucketPlanBuilder.ResolveBucketCount(from, to, requestedBucketCount: 0));
        Assert.Equal(20, BucketedSeriesBucketPlanBuilder.ResolveBucketCount(from, to, requestedBucketCount: 40));
        Assert.Equal(12, BucketedSeriesBucketPlanBuilder.ResolveBucketCount(from, to, requestedBucketCount: 12));
    }

    [Fact]
    public void Build_WhenDateRangeIsReversed_NormalizesRangeAndBuildsStableLabels()
    {
        var plan = BucketedSeriesBucketPlanBuilder.Build(
            new DateTime(2026, 1, 3),
            new DateTime(2026, 1, 1),
            bucketCount: 2);

        Assert.Equal(new DateTime(2026, 1, 1), plan.From);
        Assert.Equal(new DateTime(2026, 1, 3), plan.To);
        Assert.Equal(2, plan.Buckets.Count);
        Assert.Equal(0, plan.Buckets[0].Index);
        Assert.Equal("2026-01-01 - 2026-01-02", plan.Buckets[0].Label);
        Assert.Equal("2026-01-02 - 2026-01-03", plan.Buckets[1].Label);
    }
}
