using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Shared.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipStackedFormatter
{
    public static string Build(CartesianChart chart, int index)
    {
        var parts = new List<string>();
        var totalsBySeries = new Dictionary<string, (double? Smoothed, double? Raw)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? "Series";
            var (baseName, isRaw, isSmoothed) = ChartTooltipSeriesTitleParser.Parse(title);
            var valueText = ChartTooltipValueFormatter.FormatSeriesValue(series, index);
            if (valueText == "N/A")
                continue;

            var suffix = isSmoothed ? "smooth" : isRaw ? "Raw" : "value";
            parts.Add($"{baseName} {suffix}: {valueText}");

            if (!ChartTooltipValueFormatter.TryExtractNumericValue(series, index, out var numericValue) ||
                ChartTooltipSeriesFilter.IsOverlaySeries(baseName, state))
            {
                continue;
            }

            if (!totalsBySeries.TryGetValue(baseName, out var entry))
                entry = (null, null);

            if (isSmoothed)
                entry.Smoothed = numericValue;
            else if (isRaw)
                entry.Raw = numericValue;
            else
                entry.Smoothed = numericValue;

            totalsBySeries[baseName] = entry;
        }

        var total = GetStackedTotalFromSeries(totalsBySeries) ?? GetStackedTotalAtIndex(chart, index);
        if (total.HasValue)
            parts.Add($"Total: {MathHelper.FormatDisplayedValue(total.Value)}");

        return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
    }

    public static double? GetStackedTotalAtIndex(CartesianChart chart, int index)
    {
        if (IsCumulativeStack(chart))
            return ChartTooltipCumulativeFormatter.GetCumulativeTotalAtIndex(chart, index);

        var totalsBySeries = new Dictionary<string, (double? Smoothed, double? Raw)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? "Series";
            var (baseName, isRaw, isSmoothed) = ChartTooltipSeriesTitleParser.Parse(title);
            if (string.IsNullOrWhiteSpace(baseName) || ChartTooltipSeriesFilter.IsOverlaySeries(baseName, state))
                continue;

            if (!ChartTooltipValueFormatter.TryExtractNumericValue(series, index, out var value))
                continue;

            if (!totalsBySeries.TryGetValue(baseName, out var entry))
                entry = (null, null);

            if (isSmoothed)
                entry.Smoothed = value;
            else if (isRaw)
                entry.Raw = value;
            else
                entry.Smoothed = value;

            totalsBySeries[baseName] = entry;
        }

        return GetStackedTotalFromSeries(totalsBySeries);
    }

    private static double? GetStackedTotalFromSeries(Dictionary<string, (double? Smoothed, double? Raw)> totalsBySeries)
    {
        double total = 0;
        var found = false;

        foreach (var entry in totalsBySeries.Values)
        {
            var value = entry.Smoothed ?? entry.Raw;
            if (!value.HasValue || double.IsNaN(value.Value) || double.IsInfinity(value.Value))
                continue;

            total += value.Value;
            found = true;
        }

        return found ? total : null;
    }

    private static bool IsCumulativeStack(CartesianChart chart)
    {
        return chart.Tag is ChartStackingTooltipState state && state.IsCumulative;
    }
}
