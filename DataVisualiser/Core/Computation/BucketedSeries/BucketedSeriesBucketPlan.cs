namespace DataVisualiser.Core.Computation.BucketedSeries;

internal sealed record BucketedSeriesBucket(int Index, DateTime Start, DateTime End, string Label);

internal sealed record BucketedSeriesBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<BucketedSeriesBucket> Buckets);

internal static class BucketedSeriesBucketPlanBuilder
{
    public static int ResolveBucketCount(DateTime from, DateTime to, int requestedBucketCount, int bucketMax = 20)
    {
        if (to <= from)
            return 1;

        return Math.Max(1, Math.Min(requestedBucketCount, bucketMax));
    }

    public static BucketedSeriesBucketPlan Build(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<BucketedSeriesBucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1 ? to.Ticks : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new BucketedSeriesBucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new BucketedSeriesBucketPlan(from, to, bucketTicks, buckets);
    }
}
