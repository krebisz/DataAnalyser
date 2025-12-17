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
            if (_data == null)
                return null;

            if (_from > _to)
                return null;

            var ordered = FilterData(_data, _from, _to);
            if (ordered.Count == 0)
                return null;

            var buckets = BucketByWeekday(ordered);

            var stats = ComputeDailyStatistics(buckets);

            Unit = ordered.FirstOrDefault()?.Unit;

            var frequencyData =
                ComputeFrequencyDistributions(
                    buckets,
                    stats.GlobalMin,
                    stats.GlobalMax);

            ExtendedResult = BuildExtendedResult(
                stats,
                buckets,
                frequencyData,
                Unit);

            return BuildCompatibilityResult(
                stats,
                Unit,
                _from,
                _to);
        }

        private static List<HealthMetricData> FilterData(
            IEnumerable<HealthMetricData> data,
            DateTime from,
            DateTime to)
        {
            return StrategyComputationHelper.FilterAndOrderByRange(data, from, to);
        }

        private static List<List<double>> BucketByWeekday(
            IEnumerable<HealthMetricData> ordered)
        {
            var buckets = Enumerable.Range(0, 7)
                .Select(_ => new List<double>())
                .ToList();

            foreach (var d in ordered)
            {
                var dow = d.NormalizedTimestamp.DayOfWeek;

                // Monday = 0 … Sunday = 6
                int idx = dow == DayOfWeek.Sunday
                    ? 6
                    : ((int)dow - 1);

                if (idx < 0 || idx > 6)
                    idx = 0;

                buckets[idx].Add((double)d.Value!.Value);
            }

            return buckets;
        }

        private static (
            List<double> Mins,
            List<double> Maxs,
            List<double> Ranges,
            List<int> Counts,
            double GlobalMin,
            double GlobalMax)
        ComputeDailyStatistics(
            List<List<double>> buckets)
        {
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
                    continue;
                }

                var min = items.Min();
                var max = items.Max();

                mins.Add(min);
                maxs.Add(max);
                ranges.Add(max - min);
                counts.Add(items.Count);

                if (double.IsNaN(globalMin) || min < globalMin)
                    globalMin = min;

                if (double.IsNaN(globalMax) || max > globalMax)
                    globalMax = max;
            }

            return (mins, maxs, ranges, counts, globalMin, globalMax);
        }

        private static (
            List<(double Min, double Max)> Bins,
            double BinSize,
            Dictionary<int, Dictionary<int, int>> Frequencies,
            Dictionary<int, Dictionary<int, double>> NormalizedFrequencies)
        ComputeFrequencyDistributions(
            List<List<double>> buckets,
            double globalMin,
            double globalMax)
        {
            var bins = new List<(double Min, double Max)>();
            double binSize = 1.0;

            var frequencies = new Dictionary<int, Dictionary<int, int>>();
            var normalized = new Dictionary<int, Dictionary<int, double>>();

            var dayValuesDict = new Dictionary<int, List<double>>();
            for (int i = 0; i < 7; i++)
            {
                dayValuesDict[i] = buckets[i];
            }

            if (!double.IsNaN(globalMin) &&
                !double.IsNaN(globalMax) &&
                globalMax > globalMin)
            {
                (bins, binSize, frequencies, normalized) =
                    WeeklyFrequencyRenderer
                        .PrepareBinsAndFrequencies(
                            dayValuesDict,
                            globalMin,
                            globalMax);
            }

            return (bins, binSize, frequencies, normalized);
        }

        private static WeeklyDistributionResult BuildExtendedResult(
            (
                List<double> Mins,
                List<double> Maxs,
                List<double> Ranges,
                List<int> Counts,
                double GlobalMin,
                double GlobalMax
            ) stats,
            List<List<double>> buckets,
            (
                List<(double Min, double Max)> Bins,
                double BinSize,
                Dictionary<int, Dictionary<int, int>> Frequencies,
                Dictionary<int, Dictionary<int, double>> NormalizedFrequencies
            ) frequencyData,
            string? unit)
        {
            var dayValuesDict = new Dictionary<int, List<double>>();
            for (int i = 0; i < 7; i++)
            {
                dayValuesDict[i] = buckets[i];
            }

            return new WeeklyDistributionResult
            {
                Mins = stats.Mins,
                Maxs = stats.Maxs,
                Ranges = stats.Ranges,
                Counts = stats.Counts,
                DayValues = dayValuesDict,
                GlobalMin = double.IsNaN(stats.GlobalMin) ? 0.0 : stats.GlobalMin,
                GlobalMax = double.IsNaN(stats.GlobalMax) ? 1.0 : stats.GlobalMax,
                BinSize = frequencyData.BinSize,
                Bins = frequencyData.Bins,
                FrequenciesPerDay = frequencyData.Frequencies,
                NormalizedFrequenciesPerDay = frequencyData.NormalizedFrequencies,
                Unit = unit
            };
        }

        private static ChartComputationResult BuildCompatibilityResult(
            (
                List<double> Mins,
                List<double> Maxs,
                List<double> Ranges,
                List<int> Counts,
                double GlobalMin,
                double GlobalMax
            ) stats,
            string? unit,
            DateTime from,
            DateTime to)
        {
            return new ChartComputationResult
            {
                PrimaryRawValues = stats.Mins,
                PrimarySmoothed = stats.Ranges,
                SecondaryRawValues = null,
                SecondarySmoothed = null,
                Timestamps = new List<DateTime>(),
                IntervalIndices = new List<int>(),
                NormalizedIntervals = new List<DateTime>(),
                TickInterval = TickInterval.Day,
                DateRange = to - from,
                Unit = unit
            };
        }
    }
}
