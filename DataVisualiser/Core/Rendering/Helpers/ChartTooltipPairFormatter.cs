using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipPairFormatter
{
    public static string Build(CartesianChart chart, int index)
    {
        var (primary, secondary) = IdentifySeriesNames(chart);
        var values = new Dictionary<string, string>();

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ChartTooltipSeriesTitleParser.Parse(series.Title ?? "Series");
            var formattedValue = ChartTooltipValueFormatter.FormatSeriesValue(series, index);
            var key = baseName == primary ? isSmoothed ? "PrimarySmoothed" : "PrimaryRaw" : baseName == secondary ? isSmoothed ? "SecondarySmoothed" : "SecondaryRaw" : null;

            if (key != null)
                values[key] = formattedValue;
        }

        return BuildFormattedString(primary, secondary, values);
    }

    private static (string? Primary, string? Secondary) IdentifySeriesNames(CartesianChart chart)
    {
        var seenBaseNames = new HashSet<string>();
        string? primary = null;
        string? secondary = null;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = string.IsNullOrEmpty(series.Title) ? "Series" : series.Title;
            var (baseName, _, _) = ChartTooltipSeriesTitleParser.Parse(title);

            if (seenBaseNames.Contains(baseName))
                continue;

            seenBaseNames.Add(baseName);
            if (primary == null)
                primary = baseName;
            else if (secondary == null && baseName != primary)
                secondary = baseName;
        }

        return (primary, secondary);
    }

    private static string BuildFormattedString(string? primaryName, string? secondaryName, Dictionary<string, string> values)
    {
        var parts = new List<string>();

        if (primaryName != null && values.TryGetValue("PrimarySmoothed", out var primarySmoothed))
            parts.Add($"{primaryName} smooth: {primarySmoothed}");
        if (secondaryName != null && values.TryGetValue("SecondarySmoothed", out var secondarySmoothed))
            parts.Add($"{secondaryName} smooth: {secondarySmoothed}");
        if (primaryName != null && values.TryGetValue("PrimaryRaw", out var primaryRaw))
            parts.Add($"{primaryName} Raw: {primaryRaw}");
        if (secondaryName != null && values.TryGetValue("SecondaryRaw", out var secondaryRaw))
            parts.Add($"{secondaryName} Raw: {secondaryRaw}");

        return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
    }
}
