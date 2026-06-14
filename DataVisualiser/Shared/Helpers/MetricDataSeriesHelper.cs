using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

internal static class MetricDataSeriesHelper
{
    public static List<MetricData> FilterValuedAndOrder(IEnumerable<MetricData>? source, DateTime? from = null, DateTime? to = null)
    {
        if (source == null)
            return new List<MetricData>();

        return CollapseDuplicateTimestamps(source.Where(data => IsIncluded(data, from, to))!);
    }

    public static Dictionary<DateTime, double> CreateTimestampValueDictionary(IEnumerable<MetricData> orderedData)
    {
        return CollapseDuplicateTimestamps(orderedData)
            .ToDictionary(data => data.NormalizedTimestamp, data => (double)data.Value!.Value);
    }

    public static List<MetricData> CollapseDuplicateTimestamps(IEnumerable<MetricData>? source)
    {
        if (source == null)
            return new List<MetricData>();

        return source
            .Where(data => data != null && data.Value.HasValue)
            .GroupBy(data => data.NormalizedTimestamp)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var values = group.Select(data => data.Value!.Value).ToArray();
                var first = group.First();
                return new MetricData
                {
                    NormalizedTimestamp = group.Key,
                    Value = values.Average(),
                    Unit = group.Select(data => data.Unit).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? first.Unit,
                    Provider = group.Select(data => data.Provider).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? first.Provider
                };
            })
            .ToList();
    }

    public static string? GetPreferredUnit(IReadOnlyList<MetricData> primary, IReadOnlyList<MetricData> secondary)
    {
        return primary.FirstOrDefault()?.Unit ?? secondary.FirstOrDefault()?.Unit;
    }

    private static bool IsIncluded(MetricData? data, DateTime? from, DateTime? to)
    {
        if (data == null || !data.Value.HasValue)
            return false;

        if (from.HasValue && data.NormalizedTimestamp < from.Value)
            return false;

        if (to.HasValue && data.NormalizedTimestamp > to.Value)
            return false;

        return true;
    }
}
