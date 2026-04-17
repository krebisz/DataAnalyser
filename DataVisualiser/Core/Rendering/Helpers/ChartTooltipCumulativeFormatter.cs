using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Shared.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipCumulativeFormatter
{
    public static string BuildOriginal(IReadOnlyList<SeriesResult> originalSeries, int index)
    {
        var parts = new List<string>();
        var rawTotals = new List<double>();
        var smoothTotals = new List<double>();

        foreach (var series in originalSeries)
        {
            var rawValue = ChartTooltipValueFormatter.TryGetValue(series.RawValues, index, out var raw) ? raw : (double?)null;
            var smoothValue = series.Smoothed != null && ChartTooltipValueFormatter.TryGetValue(series.Smoothed, index, out var smooth) ? smooth : (double?)null;

            if (rawValue.HasValue)
            {
                parts.Add($"{series.DisplayName} Raw: {MathHelper.FormatDisplayedValue(rawValue.Value)}");
                rawTotals.Add(rawValue.Value);
            }
            else if (smoothValue.HasValue)
            {
                parts.Add($"{series.DisplayName} smooth: {MathHelper.FormatDisplayedValue(smoothValue.Value)}");
                smoothTotals.Add(smoothValue.Value);
            }
        }

        if (rawTotals.Count > 0)
            parts.Add($"Total: {MathHelper.FormatDisplayedValue(rawTotals.Sum())}");
        else if (smoothTotals.Count > 0)
            parts.Add($"Total: {MathHelper.FormatDisplayedValue(smoothTotals.Sum())}");

        return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
    }

    public static string BuildFromSeries(CartesianChart chart, int index)
    {
        var (orderedNames, valuesByName) = CollectCumulativeValues(chart, index);
        var parts = new List<string>();
        double? previousCumulative = null;
        double? total = null;

        foreach (var name in orderedNames)
        {
            if (!valuesByName.TryGetValue(name, out var entry))
                continue;

            var chosen = entry.Raw ?? entry.Smooth;
            if (!chosen.HasValue || double.IsNaN(chosen.Value) || double.IsInfinity(chosen.Value))
                continue;

            var originalValue = previousCumulative.HasValue ? chosen.Value - previousCumulative.Value : chosen.Value;
            previousCumulative = chosen.Value;
            total = chosen.Value;

            var suffix = entry.Raw.HasValue ? "Raw" : "smooth";
            parts.Add($"{name} {suffix}: {MathHelper.FormatDisplayedValue(originalValue)}");
        }

        if (total.HasValue)
            parts.Add($"Total: {MathHelper.FormatDisplayedValue(total.Value)}");

        return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
    }

    public static bool TryBuildFallback(CartesianChart chart, int index, out string text)
    {
        text = string.Empty;
        var (orderedNames, valuesByName) = CollectCumulativeValues(chart, index);

        if (orderedNames.Count < 2)
            return false;

        var parts = new List<string>();
        double? previousCumulative = null;
        double? total = null;

        foreach (var name in orderedNames)
        {
            if (!valuesByName.TryGetValue(name, out var entry))
                return false;

            var chosen = entry.Raw ?? entry.Smooth;
            if (!chosen.HasValue || double.IsNaN(chosen.Value) || double.IsInfinity(chosen.Value))
                return false;

            if (previousCumulative.HasValue && chosen.Value < previousCumulative.Value - 1e-6)
                return false;

            var originalValue = previousCumulative.HasValue ? chosen.Value - previousCumulative.Value : chosen.Value;
            previousCumulative = chosen.Value;
            total = chosen.Value;

            var suffix = entry.Raw.HasValue ? "Raw" : "smooth";
            parts.Add($"{name} {suffix}: {MathHelper.FormatDisplayedValue(originalValue)}");
        }

        if (total.HasValue)
            parts.Add($"Total: {MathHelper.FormatDisplayedValue(total.Value)}");

        text = parts.Count > 0 ? string.Join("; ", parts) : "N/A";
        return parts.Count > 0;
    }

    public static double? GetCumulativeTotalAtIndex(CartesianChart chart, int index)
    {
        double? total = null;
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, _, _) = ChartTooltipSeriesTitleParser.Parse(series.Title ?? "Series");
            if (ChartTooltipSeriesFilter.IsOverlaySeries(baseName, state))
                continue;

            if (!ChartTooltipValueFormatter.TryExtractNumericValue(series, index, out var value))
                continue;

            total = total.HasValue ? Math.Max(total.Value, value) : value;
        }

        return total;
    }

    private static (List<string> OrderedNames, Dictionary<string, (double? Raw, double? Smooth)> ValuesByName) CollectCumulativeValues(CartesianChart chart, int index)
    {
        var orderedNames = new List<string>();
        var valuesByName = new Dictionary<string, (double? Raw, double? Smooth)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ChartTooltipSeriesTitleParser.Parse(series.Title ?? "Series");
            if (string.IsNullOrWhiteSpace(baseName) || ChartTooltipSeriesFilter.IsOverlaySeries(baseName, state))
                continue;

            if (!orderedNames.Contains(baseName))
                orderedNames.Add(baseName);

            if (!ChartTooltipValueFormatter.TryExtractNumericValue(series, index, out var value))
                continue;

            if (!valuesByName.TryGetValue(baseName, out var entry))
                entry = (null, null);

            if (isRaw)
                entry.Raw = value;
            else if (isSmoothed)
                entry.Smooth = value;
            else
                entry.Smooth = value;

            valuesByName[baseName] = entry;
        }

        return (orderedNames, valuesByName);
    }
}
