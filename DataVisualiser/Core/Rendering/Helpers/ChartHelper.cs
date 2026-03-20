using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ChartHelper
{
    public static LineSeries? CreateLineSeries(string title, int pointSize, int lineThickness, Color colour, bool dataLabels = false)
    {
        var smoothedSeries = new LineSeries
        {
                Title = title,
                Values = new ChartValues<double>(),
                PointGeometrySize = pointSize,
                StrokeThickness = lineThickness,
                Fill = Brushes.Transparent,
                Stroke = new SolidColorBrush(colour),
                DataLabels = dataLabels
        };

        return smoothedSeries;
    }

    public static string GetChartValuesAtIndex(CartesianChart chart, int index)
    {
        if (chart?.Series == null || chart.Series.Count == 0)
            return "N/A";

        var parts = new List<string>();

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = string.IsNullOrWhiteSpace(series.Title) ? "Series" : series.Title;
            parts.Add($"{title}: {GetFormattedValue(series, index)}");
        }

        return string.Join(" | ", parts);
    }

    private static string GetFormattedValue(Series series, int index)
    {
        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return "N/A";

        try
        {
            var raw = series.Values[index];
            return raw == null ? "N/A" : MathHelper.FormatDisplayedValue(Convert.ToDouble(raw));
        }
        catch
        {
            return "N/A";
        }
    }

    private static (string BaseName, bool IsRaw, bool IsSmoothed) ParseSeriesTitle(string title)
    {
        if (title.EndsWith(" (Raw)") || title.EndsWith(" (raw)"))
            return (title.Substring(0, title.Length - 6), true, false);

        if (title.EndsWith(" (smooth)"))
            return (title.Substring(0, title.Length - 9), false, true);

        return (title, false, false);
    }

    private static (string? Primary, string? Secondary) IdentifySeriesNames(CartesianChart chart)
    {
        var seenBaseNames = new HashSet<string>();
        string? primary = null;
        string? secondary = null;

        foreach (var s in chart.Series.OfType<Series>())
        {
            var title = string.IsNullOrEmpty(s.Title) ? "Series" : s.Title;
            var (baseName, _, _) = ParseSeriesTitle(title);

            if (!seenBaseNames.Contains(baseName))
            {
                seenBaseNames.Add(baseName);
                if (primary == null)
                    primary = baseName;
                else if (secondary == null && baseName != primary)
                    secondary = baseName;
            }
        }

        return (primary, secondary);
    }

    private static string ExtractFormattedValue(Series series, int index)
    {
        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return "N/A";

        try
        {
            var raw = series.Values[index];
            if (raw == null)
                return "N/A";

            return MathHelper.FormatDisplayedValue(Convert.ToDouble(raw));
        }
        catch
        {
            return "N/A";
        }
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
                    return BuildCumulativeOriginalTooltip(state.OriginalSeries, index);

                return BuildCumulativeTooltipFromSeries(chart, index);
            }

            return BuildStackedValuesFormattedString(chart, index);
        }

        if (chart.Series.OfType<StackedAreaSeries>().Any())
            return BuildStackedValuesFormattedString(chart, index);

        if (TryBuildCumulativeTooltipFallback(chart, index, out var fallback))
            return fallback;

        var (primary, secondary) = IdentifySeriesNames(chart);
        var values = new Dictionary<string, string>();

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(series.Title ?? "Series");
            var formattedValue = ExtractFormattedValue(series, index);
            var key = baseName == primary ? isSmoothed ? "PrimarySmoothed" : "PrimaryRaw" : baseName == secondary ? isSmoothed ? "SecondarySmoothed" : "SecondaryRaw" : null;

            if (key != null)
                values[key] = formattedValue;
        }

        return BuildFormattedString(primary, secondary, values);
    }

    private static string BuildFormattedString(string? primaryName, string? secondaryName, Dictionary<string, string> values)
    {
        var parts = new List<string>();

        if (primaryName != null && values.TryGetValue("PrimarySmoothed", out var ps))
            parts.Add($"{primaryName} smooth: {ps}");

        if (secondaryName != null && values.TryGetValue("SecondarySmoothed", out var ss))
            parts.Add($"{secondaryName} smooth: {ss}");

        if (primaryName != null && values.TryGetValue("PrimaryRaw", out var pr))
            parts.Add($"{primaryName} Raw: {pr}");

        if (secondaryName != null && values.TryGetValue("SecondaryRaw", out var sr))
            parts.Add($"{secondaryName} Raw: {sr}");

        return parts.Count > 0 ? string.Join("; ", parts) : "N/A";
    }

    private static string BuildStackedValuesFormattedString(CartesianChart chart, int index)
    {
        var parts = new List<string>();
        var totalsBySeries = new Dictionary<string, (double? Smoothed, double? Raw)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? "Series";
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(title);
            var valueText = ExtractFormattedValue(series, index);

            if (valueText == "N/A")
                continue;

            var suffix = isSmoothed ? "smooth" : isRaw ? "Raw" : "value";
            parts.Add($"{baseName} {suffix}: {valueText}");

            if (!TryExtractNumericValue(series, index, out var numericValue))
                continue;

            if (IsOverlaySeries(baseName, state))
                continue;

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

    private static string BuildCumulativeOriginalTooltip(IReadOnlyList<SeriesResult> originalSeries, int index)
    {
        var parts = new List<string>();
        var rawTotals = new List<double>();
        var smoothTotals = new List<double>();

        foreach (var series in originalSeries)
        {
            var rawValue = TryGetValue(series.RawValues, index, out var raw) ? raw : (double?)null;
            var smoothValue = series.Smoothed != null && TryGetValue(series.Smoothed, index, out var smooth) ? smooth : (double?)null;

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

    private static string BuildCumulativeTooltipFromSeries(CartesianChart chart, int index)
    {
        var orderedNames = new List<string>();
        var valuesByName = new Dictionary<string, (double? Raw, double? Smooth)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(series.Title ?? "Series");
            if (string.IsNullOrWhiteSpace(baseName) || IsOverlaySeries(baseName, state))
                continue;

            if (!orderedNames.Contains(baseName))
                orderedNames.Add(baseName);

            if (!TryExtractNumericValue(series, index, out var value))
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

    private static bool TryBuildCumulativeTooltipFallback(CartesianChart chart, int index, out string text)
    {
        text = string.Empty;

        var orderedNames = new List<string>();
        var valuesByName = new Dictionary<string, (double? Raw, double? Smooth)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(series.Title ?? "Series");
            if (string.IsNullOrWhiteSpace(baseName) || IsOverlaySeries(baseName, state))
                continue;

            if (!orderedNames.Contains(baseName))
                orderedNames.Add(baseName);

            if (!TryExtractNumericValue(series, index, out var value))
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

    private static bool TryGetValue(IList<double> values, int index, out double value)
    {
        value = double.NaN;
        if (values == null || index < 0 || index >= values.Count)
            return false;

        value = values[index];
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    private static bool IsCumulativeStack(CartesianChart chart)
    {
        return chart.Tag is ChartStackingTooltipState state && state.IsCumulative;
    }

    private static bool IsOverlaySeries(string baseName, ChartStackingTooltipState? state)
    {
        if (state?.OverlaySeriesNames == null || string.IsNullOrWhiteSpace(baseName))
            return false;

        return state.OverlaySeriesNames.Contains(baseName, StringComparer.OrdinalIgnoreCase);
    }

    private static double? GetStackedTotalAtIndex(CartesianChart chart, int index)
    {
        if (IsCumulativeStack(chart))
            return GetCumulativeTotalAtIndex(chart, index);

        var totalsBySeries = new Dictionary<string, (double? Smoothed, double? Raw)>(StringComparer.OrdinalIgnoreCase);
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? "Series";
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(title);
            if (string.IsNullOrWhiteSpace(baseName) || IsOverlaySeries(baseName, state))
                continue;

            if (!TryExtractNumericValue(series, index, out var value))
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

    private static double? GetCumulativeTotalAtIndex(CartesianChart chart, int index)
    {
        double? total = null;
        var state = chart.Tag as ChartStackingTooltipState;

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, _, _) = ParseSeriesTitle(series.Title ?? "Series");
            if (IsOverlaySeries(baseName, state))
                continue;

            if (!TryExtractNumericValue(series, index, out var value))
                continue;

            total = total.HasValue ? Math.Max(total.Value, value) : value;
        }

        return total;
    }

    private static bool TryExtractNumericValue(Series series, int index, out double value)
    {
        value = double.NaN;

        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return false;

        try
        {
            var raw = series.Values[index];
            if (raw == null)
                return false;

            value = Convert.ToDouble(raw);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static void ClearChart(CartesianChart? chart, Dictionary<CartesianChart, List<DateTime>>? chartTimestamps = null)
    {
        if (chart == null)
            return;

        chart.Series.Clear();
        chartTimestamps?.Remove(chart);
    }

    public static void InitializeChartTooltip(CartesianChart chart)
    {
        if (chart == null)
            return;

        if (chart.DataTooltip is not SimpleChartTooltip)
            chart.DataTooltip = new SimpleChartTooltip();
    }

    public static string GetTimestampTextForIndex(int index, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, params CartesianChart[] charts)
    {
        foreach (var chart in charts)
            if (chartTimestamps.TryGetValue(chart, out var list) && index >= 0 && index < list.Count)
                return list[index].ToString("yyyy-MM-dd HH:mm:ss");

        return "Timestamp: N/A";
    }

    public static void NormalizeYAxis(Axis yAxis, List<MetricData> rawData, List<double> smoothedValues)
    {
        if (!TransformChartAxisCalculator.TryCreateYAxisLayout(rawData, smoothedValues, out var layout))
        {
            yAxis.MinValue = double.NaN;
            yAxis.MaxValue = double.NaN;
            yAxis.Separator = new Separator();
            yAxis.ShowLabels = false;
            yAxis.Labels = null;
            return;
        }

        yAxis.MinValue = layout.MinValue;
        yAxis.MaxValue = layout.MaxValue;
        yAxis.Separator = layout.Step.HasValue ? new Separator { Step = layout.Step.Value } : new Separator();
        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);
        yAxis.ShowLabels = layout.ShowLabels;
        yAxis.Labels = null;
    }

    public static void ApplyTransformChartGradient(CartesianChart chart, Axis yAxis)
    {
        if (chart == null || yAxis == null)
            return;

        if (!string.Equals(chart.Name, "ChartTransformResultControl", StringComparison.Ordinal))
            return;

        if (double.IsNaN(yAxis.MinValue) || double.IsNaN(yAxis.MaxValue))
            return;

        var gradient = CreateYAxisGradientBrush(yAxis.MinValue, yAxis.MaxValue);
        if (gradient == null)
            return;

        foreach (var series in chart.Series.OfType<LineSeries>())
        {
            var (_, _, isSmoothed) = ParseSeriesTitle(series.Title ?? string.Empty);
            if (isSmoothed)
                continue;

            series.Stroke = gradient;
        }
    }

    private static LinearGradientBrush? CreateYAxisGradientBrush(double minValue, double maxValue)
    {
        var range = maxValue - minValue;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return null;

        var midValue = minValue < 0 && maxValue > 0 ? 0 : minValue + range / 2.0;
        var midOffset = (midValue - minValue) / range;

        var brush = new LinearGradientBrush
        {
                StartPoint = new System.Windows.Point(0, 1),
                EndPoint = new System.Windows.Point(0, 0),
                SpreadMethod = GradientSpreadMethod.Pad
        };

        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x2B, 0x6C, 0xB0), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xE9, 0xC4, 0x6A), midOffset));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xD6, 0x28, 0x28), 1.0));

        return brush;
    }

    public static void AdjustChartHeightBasedOnYAxis(CartesianChart chart, double minHeight)
    {
        if (chart == null || chart.AxisY.Count == 0)
            return;

        var yAxis = chart.AxisY[0];
        chart.Height = TransformChartAxisCalculator.CalculateChartHeight(yAxis.MinValue, yAxis.MaxValue, yAxis.Separator?.Step, minHeight);
    }
}
