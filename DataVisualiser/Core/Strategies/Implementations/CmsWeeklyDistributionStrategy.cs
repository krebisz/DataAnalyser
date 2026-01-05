using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using UnitResolutionService = DataVisualiser.Core.Services.UnitResolutionService;

namespace DataVisualiser.Core.Strategies.Implementations;

public sealed class CmsWeeklyDistributionStrategy : IChartComputationStrategy
{
    private static readonly int                    BucketCount = 7;
    private readonly        DateTime               _from;
    private readonly        ICanonicalMetricSeries _series;
    private readonly        DateTime               _to;
    private readonly        IUnitResolutionService _unitResolutionService;


    public CmsWeeklyDistributionStrategy(ICanonicalMetricSeries series, DateTime from, DateTime to, string label, IUnitResolutionService? unitResolutionService = null)
    {
        _series = series ?? throw new ArgumentNullException(nameof(series));
        _from = from;
        _to = to;
        PrimaryLabel = label ?? string.Empty;
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    public IReadOnlyList<object> Bins { get; private set; } = Array.Empty<object>();

    public Dictionary<int, Dictionary<int, int>> FrequenciesPerBucket { get; } = new();

    public Dictionary<int, Dictionary<int, double>> NormalizedFrequenciesPerBucket { get; } = new();
    public WeeklyDistributionResult?                ExtendedResult              { get; private set; }

    public string PrimaryLabel { get; }

    public string  SecondaryLabel => string.Empty;
    public string? Unit           => _unitResolutionService.ResolveUnit(_series);


    public ChartComputationResult? Compute()
    {
        var materialized = MaterializeSeries();
        var filteredSamples = ApplyRangeFilter(materialized);

        var bucketValues = BucketByType(filteredSamples.Select(x => (x.Timestamp, x.Value)));

        ComputePerBucketStatistics(bucketValues, out var mins, out var maxs, out var ranges, out var counts);

        ComputeGlobalBounds(mins, maxs, out var globalMin, out var globalMax);

        var (bins, binSize, freqs, norm) = PrepareFrequencyData(bucketValues, globalMin, globalMax);

        ExtendedResult = BuildExtendedResult(mins, maxs, ranges, counts, bucketValues, globalMin, globalMax, binSize, bins, freqs, norm);

        return BuildChartComputationResult(mins, ranges);
    }

    private void ComputeFrequencies(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax, double binSize)
    {
        var binCount = (int)Math.Ceiling((globalMax - globalMin) / binSize);

        Bins = Enumerable.Range(0, binCount).
                          Select(_ => new object()).
                          ToList();

        FrequenciesPerBucket.Clear();
        NormalizedFrequenciesPerBucket.Clear();

        for (var bucket = 0; bucket < BucketCount; bucket++)
        {
            var freq = new Dictionary<int, int>();
            var values = bucketValues[bucket];

            foreach (var value in values)
            {
                var index = (int)Math.Floor((value - globalMin) / binSize);
                index = Math.Clamp(index, 0, binCount - 1);

                freq[index] = freq.TryGetValue(index, out var c) ? c + 1 : 1;
            }

            FrequenciesPerBucket[bucket] = freq;

            var max = freq.Values.DefaultIfEmpty(0).
                           Max();
            NormalizedFrequenciesPerBucket[bucket] = freq.ToDictionary(kvp => kvp.Key, kvp => max == 0 ? 0d : (double)kvp.Value / max);
        }
    }

    private static(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> Normalized) PrepareFrequencyData(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        if (double.IsNaN(globalMin) || double.IsNaN(globalMax) || globalMax <= globalMin)
            return (new List<(double, double)>(), 0d, new Dictionary<int, Dictionary<int, int>>(), new Dictionary<int, Dictionary<int, double>>());

        return WeeklyFrequencyRenderer.PrepareBinsAndFrequencies(bucketValues, globalMin, globalMax);
    }

    private WeeklyDistributionResult BuildExtendedResult(double[] mins, double[] maxs, double[] ranges, int[] counts, Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax, double binSize, List<(double Min, double Max)> bins, Dictionary<int, Dictionary<int, int>> freqs, Dictionary<int, Dictionary<int, double>> norm)
    {
        return new WeeklyDistributionResult
        {
                Mins = mins.ToList(),
                Maxs = maxs.ToList(),
                Ranges = ranges.ToList(),
                Counts = counts.ToList(),
                BucketValues = bucketValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                GlobalMin = globalMin,
                GlobalMax = globalMax,
                BinSize = binSize,
                Bins = bins,
                FrequenciesPerBucket = freqs,
                NormalizedFrequenciesPerBucket = norm,
                Unit = _unitResolutionService.ResolveUnit(_series)
        };
    }

    private ChartComputationResult BuildChartComputationResult(double[] mins, double[] ranges)
    {
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

            list.Add((sample.Timestamp.LocalDateTime, // local time
                      (double)sample.Value.Value, _series.Unit.Symbol));
        }

        return list;
    }

    private List<(DateTime Timestamp, double Value, string Unit)> ApplyRangeFilter(List<(DateTime Timestamp, double Value, string Unit)> source)
    {
        return source.Where(x => x.Timestamp >= _from && x.Timestamp <= _to).
                      OrderBy(x => x.Timestamp).
                      ToList();
    }

    private void ComputePerBucketStatistics(Dictionary<int, List<double>> bucketValues, out double[] mins, out double[] maxs, out double[] ranges, out int[] counts)
    {
        mins = new double[BucketCount];
        maxs = new double[BucketCount];
        ranges = new double[BucketCount];
        counts = new int[BucketCount];

        for (var bucket = 0; bucket < BucketCount; bucket++)
        {
            if (!bucketValues.TryGetValue(bucket, out var values) || values.Count == 0)
            {
                mins[bucket] = double.NaN;
                maxs[bucket] = double.NaN;
                ranges[bucket] = double.NaN;
                counts[bucket] = 0;
                continue;
            }

            var min = values.Min();
            var max = values.Max();

            mins[bucket] = min;
            maxs[bucket] = max;
            ranges[bucket] = max - min;

            counts[bucket] = values.Count;
        }
    }


    internal int Debug_GetFilteredCount()
    {
        var materialized = MaterializeSeries();
        var filtered = ApplyRangeFilter(materialized);
        return filtered.Count;
    }

    private static Dictionary<int, List<double>> BucketByType(IEnumerable<(DateTime Timestamp, double Value)> samples)
    {
        // Monday = 0 ... Sunday = 6
        var buckets = new Dictionary<int, List<double>>(BucketCount)
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
            var bucketIndex = ((int)timestamp.DayOfWeek + (BucketCount - 1)) % BucketCount;
            buckets[bucketIndex].
                    Add(value);
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

    internal(double[] Mins, double[] Maxs, double[] Ranges, int[] Counts) Debug_ComputePerBucketStatistics()
    {
        var materialized = MaterializeSeries();
        var filteredSamples = ApplyRangeFilter(materialized);
        var bucketValues = BucketByType(filteredSamples.Select(x => (x.Timestamp, x.Value)));

        ComputePerBucketStatistics(bucketValues, out var mins, out var maxs, out var ranges, out var counts);

        return (mins, maxs, ranges, counts);
    }

    private static void ComputeGlobalBounds(double[] mins, double[] maxs, out double globalMin, out double globalMax)
    {
        globalMin = double.NaN;
        globalMax = double.NaN;

        for (var i = 0; i < BucketCount; i++)
        {
            if (double.IsNaN(mins[i]) || double.IsNaN(maxs[i]))
                continue;

            if (double.IsNaN(globalMin) || mins[i] < globalMin)
                globalMin = mins[i];

            if (double.IsNaN(globalMax) || maxs[i] > globalMax)
                globalMax = maxs[i];
        }
    }

    internal(double GlobalMin, double GlobalMax) Debug_ComputeGlobalBounds()
    {
        var materialized = MaterializeSeries();
        var filteredSamples = ApplyRangeFilter(materialized);

        var bucketValues = BucketByType(filteredSamples.Select(x => (x.Timestamp, x.Value)));

        ComputePerBucketStatistics(bucketValues, out var mins, out var maxs, out _, out _);

        ComputeGlobalBounds(mins, maxs, out var globalMin, out var globalMax);

        return (globalMin, globalMax);
    }
}