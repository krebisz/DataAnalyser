using System;
using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.Services.Implementations;
using UnitResolutionService = DataVisualiser.Services.Implementations.UnitResolutionService;

namespace DataVisualiser.Charts.Strategies
{
    public sealed class CmsWeeklyDistributionStrategy : IChartComputationStrategy
    {
        private readonly ICanonicalMetricSeries _series;
        private readonly DateTime _from;
        private readonly DateTime _to;
        private readonly string _label;
        private readonly IUnitResolutionService _unitResolutionService;

        public IReadOnlyList<object> Bins { get; private set; } = Array.Empty<object>();

        public Dictionary<int, Dictionary<int, int>> FrequenciesPerDay { get; private set; } = new();

        public Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerDay { get; private set; } = new();


        public CmsWeeklyDistributionStrategy(
            ICanonicalMetricSeries series,
            DateTime from,
            DateTime to,
            string label,
            IUnitResolutionService? unitResolutionService = null)
        {
            _series = series ?? throw new ArgumentNullException(nameof(series));
            _from = from;
            _to = to;
            _label = label ?? string.Empty;
            _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
        }

        private void ComputeFrequencies(
    Dictionary<int, List<double>> dayValues,
    double globalMin,
    double globalMax,
    double binSize)
        {
            var binCount = (int)Math.Ceiling((globalMax - globalMin) / binSize);

            Bins = Enumerable.Range(0, binCount)
                .Select(_ => new object())
                .ToList();

            FrequenciesPerDay.Clear();
            NormalizedFrequenciesPerDay.Clear();

            for (int day = 0; day < 7; day++)
            {
                var freq = new Dictionary<int, int>();
                var values = dayValues[day];

                foreach (var value in values)
                {
                    var index = (int)Math.Floor((value - globalMin) / binSize);
                    index = Math.Clamp(index, 0, binCount - 1);

                    freq[index] = freq.TryGetValue(index, out var c) ? c + 1 : 1;
                }

                FrequenciesPerDay[day] = freq;

                var max = freq.Values.DefaultIfEmpty(0).Max();
                NormalizedFrequenciesPerDay[day] = freq.ToDictionary(
                    kvp => kvp.Key,
                    kvp => max == 0 ? 0d : (double)kvp.Value / max);
            }
        }

        public string PrimaryLabel => _label;
        public string SecondaryLabel => string.Empty;
        public string? Unit => _unitResolutionService.ResolveUnit(_series);
        public WeeklyDistributionResult? ExtendedResult { get; private set; }


        public ChartComputationResult? Compute()
        {
            // Phase 2
            var materialized = MaterializeSeries();

            // Phase 3
            var filteredSamples = ApplyRangeFilter(materialized);

            // Phase 4
            var dayValues = BucketByWeekday(filteredSamples.Select(x => (x.Timestamp, x.Value)));

            // Phase 5
            ComputePerDayStatistics(dayValues, out var mins, out var maxs, out var ranges, out var counts);

            // Phase 6
            ComputeGlobalBounds(mins, maxs, out var globalMin, out var globalMax);

            // Phase 7
            var bins = new List<(double Min, double Max)>();
            var binSize = 0d;
            var freqs = new Dictionary<int, Dictionary<int, int>>();
            var norm = new Dictionary<int, Dictionary<int, double>>();

            if (!double.IsNaN(globalMin) &&
                !double.IsNaN(globalMax) &&
                globalMax > globalMin)
            {
                (bins, binSize, freqs, norm) =
                    DataVisualiser.Services.WeeklyFrequencyRenderer
                        .PrepareBinsAndFrequencies(dayValues, globalMin, globalMax);
            }

            // Phase 8 — fully populate ExtendedResult (legal surface)
            ExtendedResult = new WeeklyDistributionResult
            {
                Mins = mins.ToList(),
                Maxs = maxs.ToList(),
                Ranges = ranges.ToList(),
                Counts = counts.ToList(),
                DayValues = dayValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                GlobalMin = globalMin,
                GlobalMax = globalMax,
                BinSize = binSize,
                Bins = bins,
                FrequenciesPerDay = freqs,
                NormalizedFrequenciesPerDay = norm,
                Unit = _unitResolutionService.ResolveUnit(_series)
            };

            // Minimal ChartComputationResult (matches legacy weekly distribution convention)
            var resolvedUnit = _unitResolutionService.ResolveUnit(_series);
            return new ChartComputationResult
            {
                PrimaryRawValues = mins.ToList(),
                PrimarySmoothed = ranges.ToList(),
                SecondaryRawValues = null,
                SecondarySmoothed = null,
                Timestamps = new List<DateTime>(),
                IntervalIndices = new List<int>(),
                NormalizedIntervals = new List<DateTime>(),
                TickInterval = TickInterval.Day,
                DateRange = _to - _from,
                Unit = resolvedUnit
            };
        }


        private List<(DateTime Timestamp, double Value, string Unit)> MaterializeSeries()
        {
            var list = new List<(DateTime, double, string)>();

            foreach (var sample in _series.Samples)
            {
                if (sample.Value == null)
                    continue;

                list.Add((
                    sample.Timestamp.LocalDateTime,          // local time
                    (double)sample.Value.Value,
                    _series.Unit.Symbol
                ));
            }

            return list;
        }

        private List<(DateTime Timestamp, double Value, string Unit)> ApplyRangeFilter(List<(DateTime Timestamp, double Value, string Unit)> source)
        {
            return source
                .Where(x => x.Timestamp >= _from && x.Timestamp <= _to)
                .OrderBy(x => x.Timestamp)
                .ToList();
        }

        private void ComputePerDayStatistics(
            Dictionary<int, List<double>> dayValues,
            out double[] mins,
            out double[] maxs,
            out double[] ranges,
            out int[] counts)
        {
            mins = new double[7];
            maxs = new double[7];
            ranges = new double[7];
            counts = new int[7];

            for (int day = 0; day < 7; day++)
            {
                if (!dayValues.TryGetValue(day, out var values) || values.Count == 0)
                {
                    mins[day] = double.NaN;
                    maxs[day] = double.NaN;
                    ranges[day] = double.NaN;
                    counts[day] = 0;
                    continue;
                }

                var min = values.Min();
                var max = values.Max();

                mins[day] = min;
                maxs[day] = max;
                ranges[day] = max - min;

                counts[day] = values.Count;
            }
        }



        internal int Debug_GetFilteredCount()
        {
            var materialized = MaterializeSeries();
            var filtered = ApplyRangeFilter(materialized);
            return filtered.Count;
        }

        private static Dictionary<int, List<double>> BucketByWeekday(IEnumerable<(DateTime Timestamp, double Value)> samples)
        {
            // Monday = 0 ... Sunday = 6
            var buckets = new Dictionary<int, List<double>>(7)
            {
                { 0, new List<double>() },
                { 1, new List<double>() },
                { 2, new List<double>() },
                { 3, new List<double>() },
                { 4, new List<double>() },
                { 5, new List<double>() },
                { 6, new List<double>() }
            };

            foreach (var (timestamp, value) in samples)
            {
                var weekdayIndex = ((int)timestamp.DayOfWeek + 6) % 7;
                buckets[weekdayIndex].Add(value);
            }

            return buckets;
        }

        private static List<(DateTime Timestamp, double Value)> FilterSamples(IEnumerable<(DateTime Timestamp, double Value)> samples, DateTime from, DateTime to)
        {
            var filtered = new List<(DateTime Timestamp, double Value)>();

            foreach (var (timestamp, value) in samples)
            {
                if (timestamp < from || timestamp > to)
                    continue;

                filtered.Add((timestamp, value));
            }

            filtered.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
            return filtered;
        }

        internal (double[] Mins, double[] Maxs, double[] Ranges, int[] Counts) Debug_ComputePerDayStatistics()
        {
            var materialized = MaterializeSeries();
            var filteredSamples = ApplyRangeFilter(materialized);
            var dayValues = BucketByWeekday(filteredSamples.Select(x => (x.Timestamp, x.Value)));

            ComputePerDayStatistics(
                dayValues,
                out var mins,
                out var maxs,
                out var ranges,
                out var counts
            );

            return (mins, maxs, ranges, counts);
        }

        private static void ComputeGlobalBounds(double[] mins, double[] maxs, out double globalMin, out double globalMax)
        {
            globalMin = double.NaN;
            globalMax = double.NaN;

            for (int i = 0; i < 7; i++)
            {
                if (double.IsNaN(mins[i]) || double.IsNaN(maxs[i]))
                    continue;

                if (double.IsNaN(globalMin) || mins[i] < globalMin)
                    globalMin = mins[i];

                if (double.IsNaN(globalMax) || maxs[i] > globalMax)
                    globalMax = maxs[i];
            }
        }

        internal (double GlobalMin, double GlobalMax) Debug_ComputeGlobalBounds()
        {
            var materialized = MaterializeSeries();
            var filteredSamples = ApplyRangeFilter(materialized);

            var dayValues = BucketByWeekday(
                filteredSamples.Select(x => (x.Timestamp, x.Value))
            );

            ComputePerDayStatistics(
                dayValues,
                out var mins,
                out var maxs,
                out _,
                out _
            );

            ComputeGlobalBounds(
                mins,
                maxs,
                out var globalMin,
                out var globalMax
            );

            return (globalMin, globalMax);
        }

    }
}
