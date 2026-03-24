using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Interaction;

public static class ChartTooltipParticipationCalculator
{
    public static IReadOnlyDictionary<string, double> BuildColumnSeriesParticipationLookup(CartesianChart chart, int index)
    {
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        if (index < 0)
            return new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        var valuesBySeries = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        var total = 0d;

        foreach (var series in chart.Series.OfType<Series>())
        {
            if (series.Visibility == System.Windows.Visibility.Collapsed || series is not ColumnSeries)
                continue;

            if (!TryGetSeriesValueAtIndex(series, index, out var value) || value <= 0d)
                continue;

            var title = string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title;
            valuesBySeries[title] = value;
            total += value;
        }

        if (total <= 0d || valuesBySeries.Count == 0)
            return new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        return valuesBySeries.ToDictionary(entry => entry.Key, entry => entry.Value / total, StringComparer.OrdinalIgnoreCase);
    }

    private static bool TryGetSeriesValueAtIndex(Series series, int index, out double value)
    {
        value = double.NaN;

        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return false;

        return SimpleChartTooltip.TryExtractNumeric(series.Values[index], out value) && !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
