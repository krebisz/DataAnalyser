namespace DataVisualiser.Core.Computation.TimeSeries;

public static class RunningAverageCalculator
{
    public static IReadOnlyList<TimeSeriesPoint> Calculate(IEnumerable<TimeSeriesPoint> sourcePoints, TimeSpan? window)
    {
        ArgumentNullException.ThrowIfNull(sourcePoints);

        var points = sourcePoints
            .Where(point => double.IsFinite(point.Value))
            .OrderBy(point => point.Timestamp)
            .ToList();

        if (points.Count == 0)
            return Array.Empty<TimeSeriesPoint>();

        return window == null
            ? CalculateCumulative(points)
            : CalculateWindowed(points, window.Value);
    }

    private static IReadOnlyList<TimeSeriesPoint> CalculateCumulative(IReadOnlyList<TimeSeriesPoint> points)
    {
        var result = new List<TimeSeriesPoint>(points.Count);
        var runningSum = 0.0;
        var runningCount = 0;

        foreach (var point in points)
        {
            runningSum += point.Value;
            runningCount++;
            result.Add(new TimeSeriesPoint(point.Timestamp, runningSum / runningCount));
        }

        return result;
    }

    private static IReadOnlyList<TimeSeriesPoint> CalculateWindowed(IReadOnlyList<TimeSeriesPoint> points, TimeSpan window)
    {
        var result = new List<TimeSeriesPoint>(points.Count);
        var windowQueue = new Queue<TimeSeriesPoint>();
        var windowSum = 0.0;
        var windowTicks = window.Ticks;

        foreach (var point in points)
        {
            windowQueue.Enqueue(point);
            windowSum += point.Value;

            while (windowQueue.Count > 0 && point.Timestamp.Ticks - windowQueue.Peek().Timestamp.Ticks > windowTicks)
            {
                var removed = windowQueue.Dequeue();
                windowSum -= removed.Value;
            }

            result.Add(new TimeSeriesPoint(point.Timestamp, windowSum / windowQueue.Count));
        }

        return result;
    }
}
