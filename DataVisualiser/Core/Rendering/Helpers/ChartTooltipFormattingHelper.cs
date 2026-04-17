using DataVisualiser.Core.Rendering.Interaction;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipFormattingHelper
{
    public static string GetChartValuesAtIndex(CartesianChart chart, int index)
    {
        if (chart?.Series == null || chart.Series.Count == 0)
            return "N/A";

        var parts = new List<string>();
        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title;
            parts.Add($"{title}: {ChartTooltipValueFormatter.FormatSeriesValue(series, index)}");
        }

        return string.Join(" | ", parts);
    }

    public static string GetChartValuesFormattedAtIndex(CartesianChart chart, int index)
    {
        if (chart?.Series == null || chart.Series.Count == 0)
            return "N/A";

        if (chart.Tag is ChartStackingTooltipState state && state.IncludeTotal)
        {
            if (state.IsCumulative)
            {
                if (state.OriginalSeries != null && state.OriginalSeries.Count > 0)
                    return ChartTooltipCumulativeFormatter.BuildOriginal(state.OriginalSeries, index);

                return ChartTooltipCumulativeFormatter.BuildFromSeries(chart, index);
            }

            return ChartTooltipStackedFormatter.Build(chart, index);
        }

        if (chart.Series.OfType<StackedAreaSeries>().Any())
            return ChartTooltipStackedFormatter.Build(chart, index);

        if (ChartTooltipCumulativeFormatter.TryBuildFallback(chart, index, out var fallback))
            return fallback;

        return ChartTooltipPairFormatter.Build(chart, index);
    }

    public static (string BaseName, bool IsRaw, bool IsSmoothed) ParseSeriesTitle(string title)
    {
        return ChartTooltipSeriesTitleParser.Parse(title);
    }
}
