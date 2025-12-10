using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using DataVisualiser.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataVisualiser.Charts.Strategies
{
    /// <summary>
    /// Computes per-day-of-week min, max, counts, and frequency bins for a single metric series.
    /// Monday -> Sunday ordering.
    /// </summary>
    public sealed class WeeklyDistributionStrategy : IChartComputationStrategy
    {
        private readonly IEnumerable<HealthMetricData> _data;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;

        public WeeklyDistributionStrategy(IEnumerable<HealthMetricData> data, string label, DateTime from, DateTime to)
        {
            _data = data ?? Array.Empty<HealthMetricData>();
            _label = label ?? "Metric";
            _from = from;
            _to = to;
        }

        // friendly name for chart title/legend (not used as series name here)
        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit { get; private set; }

        /// <summary>
        /// Extended result containing frequency binning data.
        /// </summary>
        public WeeklyDistributionResult? ExtendedResult { get; private set; }

        /// <summary>
        /// Result contains arrays for mins, maxes and counts in Monday->Sunday order.
        /// Uses ChartComputationResult.PrimaryRawValues = mins
        /// and PrimarySmoothed = ranges (max - min).
        /// </summary>
        public ChartComputationResult? Compute()
        {
            if (_data == null) return null;

            // Validate date range
            if (_from > _to) return null;

            // Prepare 7 buckets Monday (1) -> Sunday (7)
            var buckets = Enumerable.Range(0, 7)
                .Select(i => new List<double>())
                .ToList();

            var ordered = _data.Where(d => d.Value.HasValue)
                               .Where(d => d.NormalizedTimestamp >= _from && d.NormalizedTimestamp <= _to)
                               .ToList();

            if (!ordered.Any()) return null;

            foreach (var d in ordered)
            {
                var dow = d.NormalizedTimestamp.DayOfWeek;
                // Map DayOfWeek to Monday=0..Sunday=6
                int idx = dow == DayOfWeek.Sunday ? 6 : ((int)dow - 1);
                if (idx < 0 || idx > 6) idx = 0; // fallback
                buckets[idx].Add((double)d.Value!.Value);
            }

            var mins = new List<double>();
            var maxs = new List<double>();
            var ranges = new List<double>();
            var counts = new List<int>();

            double globalMin = double.NaN;
            double globalMax = double.NaN;

            for (int i = 0; i < 7; i++)
            {
                var items = buckets[i];
                if (items.Count == 0)
                {
                    mins.Add(double.NaN);
                    maxs.Add(double.NaN);
                    ranges.Add(double.NaN);
                    counts.Add(0);
                }
                else
                {
                    double min = items.Min();
                    double max = items.Max();
                    mins.Add(min);
                    maxs.Add(max);
                    ranges.Add(max - min);
                    counts.Add(items.Count);

                    if (double.IsNaN(globalMin) || min < globalMin) globalMin = min;
                    if (double.IsNaN(globalMax) || max > globalMax) globalMax = max;
                }
            }

            // Set Unit from first non-empty item if possible
            Unit = ordered.FirstOrDefault()?.Unit;

            // ============================================================
            // FREQUENCY BINNING (NEW) - Using separated concerns
            // ============================================================
            // Only create bins if we have valid min/max values
            List<(double Min, double Max)> bins = new();
            double binSize = 1.0;
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay = new();
            Dictionary<int, Dictionary<int, double>> normalizedFrequenciesPerDay = new();

            // Convert buckets to dictionary format (used for both binning and storing)
            var dayValuesDict = new Dictionary<int, List<double>>();
            for (int i = 0; i < 7; i++)
            {
                dayValuesDict[i] = buckets[i];
            }

            if (!double.IsNaN(globalMin) && !double.IsNaN(globalMax) && globalMax > globalMin)
            {
                // Use separated renderer to prepare bins and frequencies
                (bins, binSize, frequenciesPerDay, normalizedFrequenciesPerDay) =
                    WeeklyFrequencyRenderer.PrepareBinsAndFrequencies(dayValuesDict, globalMin, globalMax);
            }

            // Create extended result with frequency data
            ExtendedResult = new WeeklyDistributionResult
            {
                Mins = mins,
                Maxs = maxs,
                Ranges = ranges,
                Counts = counts,
                DayValues = dayValuesDict, // Store raw values per day
                GlobalMin = double.IsNaN(globalMin) ? 0.0 : globalMin,
                GlobalMax = double.IsNaN(globalMax) ? 1.0 : globalMax,
                BinSize = binSize,
                Bins = bins,
                FrequenciesPerDay = frequenciesPerDay,
                NormalizedFrequenciesPerDay = normalizedFrequenciesPerDay,
                Unit = Unit
            };

            // Return standard result for compatibility
            var result = new ChartComputationResult
            {
                // For integration with your rendering pipeline:
                // PrimaryRawValues => mins (baseline)
                // PrimarySmoothed   => ranges (max - min) (we will stack this on top of baseline)
                PrimaryRawValues = mins,
                PrimarySmoothed = ranges,
                SecondaryRawValues = null,
                SecondarySmoothed = null,
                Timestamps = new List<DateTime>(), // not used for categorical chart
                IntervalIndices = new List<int>(),
                NormalizedIntervals = new List<DateTime>(),
                TickInterval = TickInterval.Day,
                DateRange = _to - _from,
                Unit = Unit
            };

            return result;
        }
    }
}
