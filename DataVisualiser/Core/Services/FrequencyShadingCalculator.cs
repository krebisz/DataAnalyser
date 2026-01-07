using System.Diagnostics;
using DataVisualiser.Core.Rendering.Shading;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Calculates frequency shading data for weekly distribution charts.
///     Extracted from WeeklyDistributionService to reduce complexity.
/// </summary>
public sealed class FrequencyShadingCalculator
{
    private static   int                      _bucketCount;
    private readonly IIntervalShadingStrategy _shadingStrategy;

    public FrequencyShadingCalculator(IIntervalShadingStrategy shadingStrategy, int bucketCount)
    {
        _shadingStrategy = shadingStrategy ?? throw new ArgumentNullException(nameof(shadingStrategy));
        _bucketCount = bucketCount;
    }

    /// <summary>
    ///     Builds frequency shading data from day values and global min/max.
    /// </summary>
    public FrequencyShadingData BuildFrequencyShadingData(Dictionary<int, List<double>> dayValues, double globalMin, double globalMax, int intervalCount)
    {
        var intervals = CreateUniformIntervals(globalMin, globalMax, intervalCount);
        var frequencies = CountFrequenciesPerInterval(dayValues, intervals);

        var context = new IntervalShadingContext
        {
                Intervals = intervals,
                FrequenciesPerBucket = frequencies,
                BucketValues = dayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
        };

        var colorMap = _shadingStrategy.CalculateColorMap(context);

        return new FrequencyShadingData(intervals, frequencies, colorMap, dayValues);
    }

    /// <summary>
    ///     Creates uniform partitions/intervals for the y-axis.
    /// </summary>
    public List<(double Min, double Max)> CreateUniformIntervals(double globalMin, double globalMax, int intervalCount)
    {
        var intervals = new List<(double Min, double Max)>();

        if (globalMax <= globalMin || intervalCount <= 0)
        {
            // Return a single interval if invalid input
            intervals.Add((globalMin, globalMax));
            return intervals;
        }

        var intervalSize = (globalMax - globalMin) / intervalCount;

        Debug.WriteLine("=== WeeklyDistribution: Creating Intervals ===");
        Debug.WriteLine($"Interval Count: {intervalCount}, Interval Size: {intervalSize:F6}");

        for (var i = 0; i < intervalCount; i++)
        {
            var min = globalMin + i * intervalSize;
            // Last interval includes the max value to ensure we cover the full range
            var max = i == intervalCount - 1 ? globalMax : min + intervalSize;
            intervals.Add((min, max));

            // Log first 3, last 3, and every 5th interval
            if (i < 3 || i >= intervalCount - 3 || i % 5 == 0)
            {
                var intervalType = i == intervalCount - 1 ? "[inclusive]" : "[half-open)";
                Debug.WriteLine($"  Interval {i}: [{min:F6}, {max:F6}{intervalType}");
            }
        }

        Debug.WriteLine($"Total intervals created: {intervals.Count}");

        return intervals;
    }

    /// <summary>
    ///     Counts the number of values that fall in each interval, for each separate day.
    /// </summary>
    public Dictionary<int, Dictionary<int, int>> CountFrequenciesPerInterval(Dictionary<int, List<double>> bucketValues, List<(double Min, double Max)> intervals)
    {
        var result = new Dictionary<int, Dictionary<int, int>>();

        foreach (var bucketIndex in EnumerateBuckets(_bucketCount))
        {
            var frequencies = InitializeFrequencies(intervals.Count);

            if (bucketValues.TryGetValue(bucketIndex, out var values))
                CountValuesIntoIntervals(values, intervals, frequencies);

            result[bucketIndex] = frequencies;
            LogDaySummary(bucketIndex, frequencies);
        }

        return result;
    }

    private static IEnumerable<int> EnumerateBuckets(int bucketCount)
    {
        for (var bucket = 0; bucket < bucketCount; bucket++)
            yield return bucket;
    }

    private static Dictionary<int, int> InitializeFrequencies(int intervalCount)
    {
        var frequencies = new Dictionary<int, int>(intervalCount);

        for (var i = 0; i < intervalCount; i++)
            frequencies[i] = 0;

        return frequencies;
    }

    private static void CountValuesIntoIntervals(IEnumerable<double> values, List<(double Min, double Max)> intervals, Dictionary<int, int> frequencies)
    {
        foreach (var value in values)
        {
            if (!IsValidValue(value))
                continue;

            var intervalIndex = FindIntervalIndex(value, intervals);
            if (intervalIndex >= 0)
                frequencies[intervalIndex]++;
        }
    }

    private static bool IsValidValue(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static int FindIntervalIndex(double value, List<(double Min, double Max)> intervals)
    {
        for (var i = 0; i < intervals.Count; i++)
        {
            var interval = intervals[i];
            var isLast = i == intervals.Count - 1;

            if (IsValueInInterval(value, interval, isLast))
                return i;
        }

        return -1;
    }

    private static bool IsValueInInterval(double value, (double Min, double Max) interval, bool inclusiveUpperBound)
    {
        return inclusiveUpperBound ? value >= interval.Min && value <= interval.Max : value >= interval.Min && value < interval.Max;
    }

    private static void LogDaySummary(int bucketIndex, Dictionary<int, int> frequencies)
    {
        var totalValues = frequencies.Values.Sum();
        var nonZeroIntervals = frequencies.Values.Count(f => f > 0);
        var maxFreq = frequencies.Values.DefaultIfEmpty(0).
                                  Max();

        Debug.WriteLine($"Day {bucketIndex} frequencies: " + $"Total values={totalValues}, " + $"Non-zero intervals={nonZeroIntervals}, " + $"Max frequency={maxFreq}");

        if (frequencies.Count == 0)
            return;

        var ordered = frequencies.OrderBy(kvp => kvp.Key).
                                  ToList();

        var firstFew = ordered.Take(3).
                               Select(kvp => $"I{kvp.Key}={kvp.Value}");

        var lastFew = ordered.Skip(Math.Max(0, ordered.Count - 3)).
                              Select(kvp => $"I{kvp.Key}={kvp.Value}");

        Debug.WriteLine($"  First intervals: {string.Join(", ", firstFew)}");
        Debug.WriteLine($"  Last intervals: {string.Join(", ", lastFew)}");
    }
}