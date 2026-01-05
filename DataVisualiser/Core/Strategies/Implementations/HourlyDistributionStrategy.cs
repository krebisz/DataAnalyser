using System.DirectoryServices.ActiveDirectory;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using UnitResolutionService = DataVisualiser.Core.Services.UnitResolutionService;

namespace DataVisualiser.Core.Strategies.Implementations;

/// <summary>
///     Computes per-day-of-hour min, max, counts, and frequency bins for a single metric series.
///     (0 - n) ordering.
/// </summary>
public sealed class HourlyDistributionStrategy : IChartComputationStrategy
{
    private static readonly int                     BucketCount = 24;
    private readonly        IEnumerable<MetricData> _data;
    private readonly        DateTime                _from;
    private readonly        DateTime                _to;
    private readonly        IUnitResolutionService  _unitResolutionService;

    public HourlyDistributionStrategy(IEnumerable<MetricData> data, string label, DateTime from, DateTime to, IUnitResolutionService? unitResolutionService = null)
    {
        _data = data ?? Array.Empty<MetricData>();
        PrimaryLabel = label ?? "Metric";
        _from = from;
        _to = to;
        _unitResolutionService = unitResolutionService ?? new UnitResolutionService();
    }

    /// <summary>
    ///     Extended result containing frequency binning data.
    /// </summary>
    public HourlyDistributionResult? ExtendedResult { get; private set; }

    // friendly name for chart title/legend (not used as series name here)
    public string PrimaryLabel { get; }

    public string  SecondaryLabel => string.Empty;
    public string? Unit           { get; private set; }

    /// <summary>
    ///     Result contains arrays for mins, maxes and counts in (0 - n) order.
    ///     Uses ChartComputationResult.PrimaryRawValues = mins
    ///     and PrimarySmoothed = ranges (max - min).
    /// </summary>
    public ChartComputationResult? Compute()
    {
        if (_data == null || _from > _to)
            return null;

        var ordered = FilterData(_data, _from, _to);
        if (ordered.Count == 0)
            return null;

        var buckets = BucketByHourday(ordered);

        var stats = ComputeDailyStatistics(buckets);

        Unit = _unitResolutionService.ResolveUnit(ordered);

        var frequencyData = ComputeFrequencyDistributions(buckets, stats.GlobalMin, stats.GlobalMax);

        ExtendedResult = BuildExtendedResult(stats, buckets, frequencyData, Unit);

        return BuildCompatibilityResult(stats, Unit, _from, _to);
    }


    private static List<MetricData> FilterData(IEnumerable<MetricData> data, DateTime from, DateTime to)
    {
        return StrategyComputationHelper.FilterAndOrderByRange(data, from, to);
    }

    private static List<List<double>> BucketByHourday(IEnumerable<MetricData> ordered)
    {
        var buckets = Enumerable.Range(0, BucketCount).
                                 Select(_ => new List<double>()).
                                 ToList();

        foreach (var d in ordered)
        {
            var dow = d.NormalizedTimestamp.Hour;

            //12AM = 0 … 11PM = 23
            var idx = (int)dow == (int)HourOfDay.TwentyThree ? (BucketCount - 1) : (int)dow - 1;

            if (idx < 0 || idx > (BucketCount - 1))
                idx = 0;

            buckets[idx].
                    Add((double)d.Value!.Value);
        }

        return buckets;
    }

    private static(List<double> Mins, List<double> Maxs, List<double> Ranges, List<int> Counts, double GlobalMin, double GlobalMax) ComputeDailyStatistics(List<List<double>> buckets)
    {
        var mins = new List<double>();
        var maxs = new List<double>();
        var ranges = new List<double>();
        var counts = new List<int>();

        var globalMin = double.NaN;
        var globalMax = double.NaN;

        for (var i = 0; i < BucketCount; i++)
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

    private static(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> NormalizedFrequencies) ComputeFrequencyDistributions(List<List<double>> buckets, double globalMin, double globalMax)
    {
        var dayValuesDict = BuildDayValuesDictionary(buckets);

        if (double.IsNaN(globalMin) || double.IsNaN(globalMax) || globalMax <= globalMin)
            return (new List<(double, double)>(), 1.0, new Dictionary<int, Dictionary<int, int>>(), new Dictionary<int, Dictionary<int, double>>());

        return HourlyFrequencyRenderer.PrepareBinsAndFrequencies(dayValuesDict, globalMin, globalMax);
    }

    private static Dictionary<int, List<double>> BuildDayValuesDictionary(List<List<double>> buckets)
    {
        var dict = new Dictionary<int, List<double>>(BucketCount);
        for (var i = 0; i < BucketCount; i++)
            dict[i] = buckets[i];
        return dict;
    }

    private static HourlyDistributionResult BuildExtendedResult((List<double> Mins, List<double> Maxs, List<double> Ranges, List<int> Counts, double GlobalMin, double GlobalMax) stats, List<List<double>> buckets, (List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> NormalizedFrequencies) frequencyData, string? unit)
    {
        var dayValuesDict = new Dictionary<int, List<double>>();
        for (var i = 0; i < BucketCount; i++)
            dayValuesDict[i] = buckets[i];

        return new HourlyDistributionResult
        {
                Mins = stats.Mins,
                Maxs = stats.Maxs,
                Ranges = stats.Ranges,
                Counts = stats.Counts,
                HourValues = dayValuesDict,
                GlobalMin = double.IsNaN(stats.GlobalMin) ? 0.0 : stats.GlobalMin,
                GlobalMax = double.IsNaN(stats.GlobalMax) ? 1.0 : stats.GlobalMax,
                BinSize = frequencyData.BinSize,
                Bins = frequencyData.Bins,
                FrequenciesPerHour = frequencyData.Frequencies,
                NormalizedFrequenciesPerHour = frequencyData.NormalizedFrequencies,
                Unit = unit
        };
    }

    private static ChartComputationResult BuildCompatibilityResult((List<double> Mins, List<double> Maxs, List<double> Ranges, List<int> Counts, double GlobalMin, double GlobalMax) stats, string? unit, DateTime from, DateTime to)
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