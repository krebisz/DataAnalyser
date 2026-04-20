using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public sealed class TimeBucketRenderAggregationKernel
{
    public RenderDataBuffer BuildBuffer(
        ChartSeriesProgram series,
        IReadOnlyList<DateTime> timeline,
        RenderDensityPlan density)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(timeline);
        ArgumentNullException.ThrowIfNull(density);

        var pointCount = Math.Min(timeline.Count, series.RawValues.Count);
        var metadata = new Dictionary<string, string>
        {
            ["DensityMode"] = density.Mode.ToString()
        };

        if (density.Mode == ChartRenderDensityMode.FullFidelity || pointCount <= 1)
        {
            var points = Enumerable.Range(0, pointCount)
                .Select(index => CreatePoint(timeline[index], series.RawValues[index], 1))
                .ToArray();

            return new RenderDataBuffer(series.Id, series.Label, points, pointCount, points.Length, metadata);
        }

        var bucketCount = Math.Min(pointCount, Math.Max(1, density.BucketCount ?? density.RenderedPointCount));
        var buckets = new List<RenderDataPoint>(bucketCount);

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var start = bucketIndex * pointCount / bucketCount;
            var end = (bucketIndex + 1) * pointCount / bucketCount;
            if (end <= start)
                continue;

            buckets.Add(CreateBucketPoint(timeline, series.RawValues, start, end));
        }

        return new RenderDataBuffer(series.Id, series.Label, buckets, pointCount, buckets.Count, metadata);
    }

    private static RenderDataPoint CreatePoint(DateTime timestamp, double value, int sourcePointCount)
    {
        var y = double.IsNaN(value) ? (double?)null : value;
        return new RenderDataPoint(timestamp, y, y, y, sourcePointCount);
    }

    private static RenderDataPoint CreateBucketPoint(
        IReadOnlyList<DateTime> timeline,
        IReadOnlyList<double> values,
        int start,
        int end)
    {
        double sum = 0;
        double? minimum = null;
        double? maximum = null;
        var validCount = 0;

        for (var index = start; index < end; index++)
        {
            var value = values[index];
            if (double.IsNaN(value))
                continue;

            sum += value;
            minimum = minimum.HasValue ? Math.Min(minimum.Value, value) : value;
            maximum = maximum.HasValue ? Math.Max(maximum.Value, value) : value;
            validCount++;
        }

        var average = validCount == 0 ? (double?)null : sum / validCount;
        var midpoint = start + ((end - start - 1) / 2);
        return new RenderDataPoint(timeline[midpoint], average, minimum, maximum, end - start);
    }
}
