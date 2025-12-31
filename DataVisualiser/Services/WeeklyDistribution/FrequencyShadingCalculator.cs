using DataVisualiser.Models;
using DataVisualiser.Services.Shading;
using System.Diagnostics;

namespace DataVisualiser.Services.WeeklyDistribution
{
    /// <summary>
    /// Calculates frequency shading data for weekly distribution charts.
    /// Extracted from WeeklyDistributionService to reduce complexity.
    /// </summary>
    public sealed class FrequencyShadingCalculator
    {
        private readonly IIntervalShadingStrategy _shadingStrategy;

        public FrequencyShadingCalculator(IIntervalShadingStrategy shadingStrategy)
        {
            _shadingStrategy = shadingStrategy ?? throw new ArgumentNullException(nameof(shadingStrategy));
        }

        /// <summary>
        /// Builds frequency shading data from day values and global min/max.
        /// </summary>
        public FrequencyShadingData BuildFrequencyShadingData(
            Dictionary<int, List<double>> dayValues,
            double globalMin,
            double globalMax,
            int intervalCount)
        {
            var intervals = CreateUniformIntervals(globalMin, globalMax, intervalCount);
            var frequencies = CountFrequenciesPerInterval(dayValues, intervals);

            var context = new IntervalShadingContext
            {
                Intervals = intervals,
                FrequenciesPerDay = frequencies,
                DayValues = dayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
            };

            var colorMap = _shadingStrategy.CalculateColorMap(context);

            return new FrequencyShadingData(intervals, frequencies, colorMap, dayValues);
        }

        /// <summary>
        /// Creates uniform partitions/intervals for the y-axis.
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

            double intervalSize = (globalMax - globalMin) / intervalCount;

            Debug.WriteLine($"=== WeeklyDistribution: Creating Intervals ===");
            Debug.WriteLine($"Interval Count: {intervalCount}, Interval Size: {intervalSize:F6}");

            for (int i = 0; i < intervalCount; i++)
            {
                double min = globalMin + i * intervalSize;
                // Last interval includes the max value to ensure we cover the full range
                double max = (i == intervalCount - 1) ? globalMax : min + intervalSize;
                intervals.Add((min, max));

                // Log first 3, last 3, and every 5th interval
                if (i < 3 || i >= intervalCount - 3 || i % 5 == 0)
                {
                    string intervalType = i == intervalCount - 1 ? "[inclusive]" : "[half-open)";
                    Debug.WriteLine($"  Interval {i}: [{min:F6}, {max:F6}{intervalType}");
                }
            }

            Debug.WriteLine($"Total intervals created: {intervals.Count}");

            return intervals;
        }

        /// <summary>
        /// Counts the number of values that fall in each interval, for each separate day.
        /// </summary>
        public Dictionary<int, Dictionary<int, int>> CountFrequenciesPerInterval(
            Dictionary<int, List<double>> dayValues,
            List<(double Min, double Max)> intervals)
        {
            var frequenciesPerDay = new Dictionary<int, Dictionary<int, int>>();

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var frequencies = new Dictionary<int, int>();

                // Initialize all intervals to 0
                for (int i = 0; i < intervals.Count; i++)
                {
                    frequencies[i] = 0;
                }

                // Count values in each interval
                if (dayValues.TryGetValue(dayIndex, out var values))
                {
                    foreach (var value in values)
                    {
                        if (double.IsNaN(value) || double.IsInfinity(value))
                            continue;

                        // Find which interval this value belongs to
                        for (int i = 0; i < intervals.Count; i++)
                        {
                            var interval = intervals[i];
                            // Check if value is in [Min, Max) for all intervals except the last, which is [Min, Max]
                            if (i < intervals.Count - 1)
                            {
                                if (value >= interval.Min && value < interval.Max)
                                {
                                    frequencies[i]++;
                                    break;
                                }
                            }
                            else
                            {
                                // Last interval is inclusive on both ends
                                if (value >= interval.Min && value <= interval.Max)
                                {
                                    frequencies[i]++;
                                    break;
                                }
                            }
                        }
                    }
                }

                frequenciesPerDay[dayIndex] = frequencies;

                // Log frequency summary for each day
                int totalValues = frequencies.Values.Sum();
                int nonZeroIntervals = frequencies.Values.Count(f => f > 0);
                int maxFreq = frequencies.Values.DefaultIfEmpty(0).Max();
                Debug.WriteLine($"Day {dayIndex} frequencies: Total values={totalValues}, Non-zero intervals={nonZeroIntervals}, Max frequency={maxFreq}");

                // Log frequencies for first few and last few intervals
                if (frequencies.Count > 0)
                {
                    var sortedIntervals = frequencies.OrderBy(kvp => kvp.Key).ToList();
                    var firstFew = sortedIntervals.Take(3).Select(kvp => $"I{kvp.Key}={kvp.Value}").ToList();
                    var lastFew = sortedIntervals.Skip(Math.Max(0, sortedIntervals.Count - 3)).Select(kvp => $"I{kvp.Key}={kvp.Value}").ToList();
                    Debug.WriteLine($"  First intervals: {string.Join(", ", firstFew)}");
                    Debug.WriteLine($"  Last intervals: {string.Join(", ", lastFew)}");
                }
            }

            return frequenciesPerDay;
        }
    }

    /// <summary>
    /// Data structure for frequency shading calculations.
    /// </summary>
    public sealed record FrequencyShadingData(
        List<(double Min, double Max)> Intervals,
        Dictionary<int, Dictionary<int, int>> FrequenciesPerDay,
        Dictionary<int, Dictionary<int, System.Windows.Media.Color>> ColorMap,
        Dictionary<int, List<double>> DayValues)
    {
        public static readonly FrequencyShadingData Empty =
            new(new(), new(), new(), new());
    }
}

