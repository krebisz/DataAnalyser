using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformMetricSelectionRequestFactory
{
    public static MetricSelectionRequest Create(
        IReadOnlyList<MetricSeriesRequest> series,
        DateTime from,
        DateTime to,
        string resolutionTableName)
    {
        if (series == null || series.Count == 0)
            throw new ArgumentException("At least one transform input series is required.", nameof(series));

        return new MetricSelectionRequest(
            ResolveSelectionMetricType(series),
            series,
            from,
            to,
            resolutionTableName);
    }

    public static string ResolveSelectionMetricType(IReadOnlyList<MetricSeriesRequest> series)
    {
        var distinct = series
            .Select(item => item.MetricType)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return distinct.Length == 1 ? distinct[0] : "Mixed";
    }
}
