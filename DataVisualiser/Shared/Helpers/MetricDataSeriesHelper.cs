using DataVisualiser.Shared.Models;

namespace DataVisualiser.Shared.Helpers;

internal static class MetricDataSeriesHelper
{
    public static List<MetricData> FilterValuedAndOrder(IEnumerable<MetricData>? source, DateTime? from = null, DateTime? to = null)
    {
        if (source == null)
            return new List<MetricData>();

        return source.Where(data => IsIncluded(data, from, to)).OrderBy(data => data!.NormalizedTimestamp).ToList()!;
    }

    public static Dictionary<DateTime, double> CreateTimestampValueDictionary(IEnumerable<MetricData> orderedData)
    {
        return orderedData.GroupBy(data => data.NormalizedTimestamp).ToDictionary(group => group.Key, group => (double)group.First().Value!.Value);
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
