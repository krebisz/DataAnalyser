using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

/// <summary>
///     Provides common computation logic shared across chart computation strategies.
/// </summary>
public static class StrategyComputationHelper
{
    /// <summary>
    ///     Filters out null-valued points and orders data by timestamp.
    ///     Simple version without date range filtering.
    /// </summary>
    public static List<HealthMetricData> PrepareOrderedData(IEnumerable<HealthMetricData> source)
    {
        if (source == null)
            return new List<HealthMetricData>();

        return source.Where(d => d != null && d.Value.HasValue).
                      OrderBy(d => d!.NormalizedTimestamp).
                      ToList();
    }

    /// <summary>
    ///     Filters out null or missing values and restricts data to the [from, to] range,
    ///     ordered by NormalizedTimestamp.
    /// </summary>
    public static List<HealthMetricData> FilterAndOrderByRange(IEnumerable<HealthMetricData>? source, DateTime from, DateTime to)
    {
        if (source == null)
            return new List<HealthMetricData>();

        return source.Where(d => d != null && d.Value.HasValue && d.NormalizedTimestamp >= from && d.NormalizedTimestamp <= to).
                      OrderBy(d => d!.NormalizedTimestamp).
                      ToList();
    }

    /// <summary>
    ///     Validates and prepares data for strategy computation.
    /// </summary>
    public static(List<HealthMetricData> Ordered1, List<HealthMetricData> Ordered2, TimeSpan DateRange, TickInterval TickInterval)? PrepareDataForComputation(IEnumerable<HealthMetricData>? left, IEnumerable<HealthMetricData>? right, DateTime from, DateTime to)
    {
        if (left == null && right == null)
            return null;

        var ordered1 = left?.Where(d => d.Value.HasValue).
                             OrderBy(d => d.NormalizedTimestamp).
                             ToList() ?? new List<HealthMetricData>();
        var ordered2 = right?.Where(d => d.Value.HasValue).
                              OrderBy(d => d.NormalizedTimestamp).
                              ToList() ?? new List<HealthMetricData>();

        if (!ordered1.Any() && !ordered2.Any())
            return null;

        if (from > to)
            return null;

        var dateRange = to - from;
        if (dateRange.TotalMilliseconds <= 0)
            return null;

        var tickInterval = MathHelper.DetermineTickInterval(dateRange);

        return (ordered1, ordered2, dateRange, tickInterval);
    }

    /// <summary>
    ///     Combines timestamps from two data sets, removing duplicates and sorting.
    /// </summary>
    public static List<DateTime> CombineTimestamps(IEnumerable<HealthMetricData> ordered1, IEnumerable<HealthMetricData> ordered2)
    {
        return ordered1.Select(d => d.NormalizedTimestamp).
                        Union(ordered2.Select(d => d.NormalizedTimestamp)).
                        OrderBy(dt => dt).
                        ToList();
    }

    /// <summary>
    ///     Creates dictionaries mapping timestamps to values for efficient lookup.
    /// </summary>
    public static(Dictionary<DateTime, double> Dict1, Dictionary<DateTime, double> Dict2) CreateTimestampValueDictionaries(List<HealthMetricData> ordered1, List<HealthMetricData> ordered2)
    {
        var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp).
                             ToDictionary(g => g.Key, g => (double)g.First().
                                                                     Value!.Value);

        var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp).
                             ToDictionary(g => g.Key, g => (double)g.First().
                                                                     Value!.Value);

        return (dict1, dict2);
    }

    /// <summary>
    ///     Extracts raw values aligned to combined timestamps.
    /// </summary>
    public static(List<double> RawValues1, List<double> RawValues2) ExtractAlignedRawValues(List<DateTime> combinedTimestamps, Dictionary<DateTime, double> dict1, Dictionary<DateTime, double> dict2)
    {
        var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).
                                            ToList();

        var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).
                                            ToList();

        return (rawValues1, rawValues2);
    }

    /// <summary>
    ///     Processes smoothed data for two datasets and interpolates to combined timestamps.
    /// </summary>
    public static(List<double> InterpSmoothed1, List<double> InterpSmoothed2) ProcessSmoothedData(List<HealthMetricData> ordered1, List<HealthMetricData> ordered2, List<DateTime> combinedTimestamps, DateTime from, DateTime to)
    {
        var smoothed1 = MathHelper.CreateSmoothedData(ordered1, from, to);
        var smoothed2 = MathHelper.CreateSmoothedData(ordered2, from, to);
        var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
        var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

        return (interpSmoothed1, interpSmoothed2);
    }

    /// <summary>
    ///     Gets the unit from the first available data point.
    /// </summary>
    public static string? GetUnit(List<HealthMetricData> ordered1, List<HealthMetricData> ordered2)
    {
        return ordered1.FirstOrDefault()?.
                        Unit ?? ordered2.FirstOrDefault()?.
                                         Unit;
    }

    /// <summary>
    ///     Aligns two HealthMetricData series by index and extracts timestamps and values.
    ///     Used by strategies that need to align two series by their ordered index.
    /// </summary>
    public static(List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) AlignByIndex(IReadOnlyList<HealthMetricData> left, IReadOnlyList<HealthMetricData> right, int count)
    {
        var timestamps = new List<DateTime>(count);
        var primary = new List<double>(count);
        var secondary = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var l = left[i];
            var r = right[i];

            timestamps.Add(l.NormalizedTimestamp);
            primary.Add(l.Value.HasValue ? (double)l.Value.Value : double.NaN);
            secondary.Add(r.Value.HasValue ? (double)r.Value.Value : double.NaN);
        }

        return (timestamps, primary, secondary);
    }

    /// <summary>
    ///     Aligns two CmsPoint series by index and extracts timestamps and values.
    ///     Used by CMS strategies that need to align two series by their ordered index.
    /// </summary>
    public static(List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) AlignByIndex(IReadOnlyList<(DateTime Timestamp, decimal? ValueDecimal)> left, IReadOnlyList<(DateTime Timestamp, decimal? ValueDecimal)> right, int count)
    {
        var timestamps = new List<DateTime>(count);
        var primary = new List<double>(count);
        var secondary = new List<double>(count);

        for (var i = 0; i < count; i++)
        {
            var l = left[i];
            var r = right[i];

            timestamps.Add(l.Timestamp);
            primary.Add(l.ValueDecimal.HasValue ? (double)l.ValueDecimal.Value : double.NaN);
            secondary.Add(r.ValueDecimal.HasValue ? (double)r.ValueDecimal.Value : double.NaN);
        }

        return (timestamps, primary, secondary);
    }
}