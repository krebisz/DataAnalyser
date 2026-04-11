using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

public static class TimeBucketAggregationHelper
{
    public static double?[] BuildAverageTotals(
        IReadOnlyList<MetricData> data,
        DateTime from,
        DateTime to,
        double bucketTicks,
        int bucketCount)
    {
        return BuildAverageTotals(
            data,
            from,
            to,
            bucketTicks,
            bucketCount,
            point => point.NormalizedTimestamp,
            point => point.Value);
    }

    public static double?[] BuildAverageTotals(
        ICanonicalMetricSeries series,
        DateTime from,
        DateTime to,
        double bucketTicks,
        int bucketCount)
    {
        return BuildAverageTotals(
            series.Samples,
            from,
            to,
            bucketTicks,
            bucketCount,
            sample => sample.Timestamp.LocalDateTime,
            sample => sample.Value);
    }

    public static int ResolveBucketIndex(DateTime timestamp, DateTime from, DateTime to, double bucketTicks, int bucketCount)
    {
        if (timestamp < from || timestamp > to)
            return -1;

        if (bucketTicks <= 0)
            return 0;

        var offsetTicks = timestamp.Ticks - from.Ticks;
        var index = (int)Math.Floor(offsetTicks / bucketTicks);
        if (index < 0)
            return -1;
        if (index >= bucketCount)
            return bucketCount - 1;

        return index;
    }

    private static double?[] BuildAverageTotals<T>(
        IEnumerable<T> source,
        DateTime from,
        DateTime to,
        double bucketTicks,
        int bucketCount,
        Func<T, DateTime> timestampSelector,
        Func<T, decimal?> valueSelector)
    {
        var totals = new double?[bucketCount];
        var sums = new double[bucketCount];
        var counts = new int[bucketCount];

        foreach (var item in source)
        {
            var value = valueSelector(item);
            if (!value.HasValue)
                continue;

            var index = ResolveBucketIndex(timestampSelector(item), from, to, bucketTicks, bucketCount);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }
}
