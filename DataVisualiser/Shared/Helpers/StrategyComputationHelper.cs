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
    public static List<MetricData> PrepareOrderedData(IEnumerable<MetricData> source)
    {
        return MetricDataSeriesHelper.FilterValuedAndOrder(source);
    }

    /// <summary>
    ///     Filters out null or missing values and restricts data to the [from, to] range,
    ///     ordered by NormalizedTimestamp.
    /// </summary>
    public static List<MetricData> FilterAndOrderByRange(IEnumerable<MetricData>? source, DateTime from, DateTime to)
    {
        return MetricDataSeriesHelper.FilterValuedAndOrder(source, from, to);
    }

    /// <summary>
    ///     Validates and prepares data for strategy computation.
    /// </summary>
    public static (List<MetricData> Ordered1, List<MetricData> Ordered2, TimeSpan DateRange, TickInterval TickInterval)? PrepareDataForComputation(IEnumerable<MetricData>? left, IEnumerable<MetricData>? right, DateTime from, DateTime to)
    {
        if (left == null && right == null)
            return null;

        var ordered1 = MetricDataSeriesHelper.FilterValuedAndOrder(left);
        var ordered2 = MetricDataSeriesHelper.FilterValuedAndOrder(right);

        if (!ordered1.Any() && !ordered2.Any())
            return null;

        if (from > to)
            return null;

        var dateRange = to - from;
        if (dateRange.TotalMilliseconds <= 0)
            return null;

        var tickInterval = TemporalIntervalHelper.DetermineTickInterval(dateRange);

        return (ordered1, ordered2, dateRange, tickInterval);
    }

    /// <summary>
    ///     Combines timestamps from two data sets, removing duplicates and sorting.
    /// </summary>
    public static List<DateTime> CombineTimestamps(IEnumerable<MetricData> ordered1, IEnumerable<MetricData> ordered2)
    {
        return ordered1.Select(d => d.NormalizedTimestamp).Union(ordered2.Select(d => d.NormalizedTimestamp)).OrderBy(dt => dt).ToList();
    }

    /// <summary>
    ///     Creates dictionaries mapping timestamps to values for efficient lookup.
    /// </summary>
    public static (Dictionary<DateTime, double> Dict1, Dictionary<DateTime, double> Dict2) CreateTimestampValueDictionaries(List<MetricData> ordered1, List<MetricData> ordered2)
    {
        var dict1 = MetricDataSeriesHelper.CreateTimestampValueDictionary(ordered1);
        var dict2 = MetricDataSeriesHelper.CreateTimestampValueDictionary(ordered2);

        return (dict1, dict2);
    }

    /// <summary>
    ///     Extracts raw values aligned to combined timestamps.
    /// </summary>
    public static (List<double> RawValues1, List<double> RawValues2) ExtractAlignedRawValues(List<DateTime> combinedTimestamps, Dictionary<DateTime, double> dict1, Dictionary<DateTime, double> dict2)
    {
        var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
        var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();

        return (rawValues1, rawValues2);
    }

    /// <summary>
    ///     Processes smoothed data for two datasets and interpolates to combined timestamps.
    /// </summary>
    public static (List<double> InterpSmoothed1, List<double> InterpSmoothed2) ProcessSmoothedData(List<MetricData> ordered1, List<MetricData> ordered2, List<DateTime> combinedTimestamps, DateTime from, DateTime to)
    {
        var smoothed1 = TimeSeriesSmoothingHelper.CreateSmoothedData(ordered1, from, to);
        var smoothed2 = TimeSeriesSmoothingHelper.CreateSmoothedData(ordered2, from, to);
        var interpSmoothed1 = TimeSeriesSmoothingHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
        var interpSmoothed2 = TimeSeriesSmoothingHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

        return (interpSmoothed1, interpSmoothed2);
    }

    /// <summary>
    ///     Gets the unit from the first available data point.
    /// </summary>
    public static string? GetUnit(List<MetricData> ordered1, List<MetricData> ordered2)
    {
        return MetricDataSeriesHelper.GetPreferredUnit(ordered1, ordered2);
    }

    /// <summary>
    ///     Aligns two MetricData series by index and extracts timestamps and values.
    ///     Used by strategies that need to align two series by their ordered index.
    /// </summary>
    public static (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) AlignByIndex(IReadOnlyList<MetricData> left, IReadOnlyList<MetricData> right, int count)
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
    public static (List<DateTime> Timestamps, List<double> Primary, List<double> Secondary) AlignByIndex(IReadOnlyList<(DateTime Timestamp, decimal? ValueDecimal)> left, IReadOnlyList<(DateTime Timestamp, decimal? ValueDecimal)> right, int count)
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
