using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

namespace DataVisualiser.Core.Rendering.Helpers;

public static class ChartHelper
{
    /// <summary>
    ///     Formats DateTime label based on tick interval.
    /// </summary>
    public static string FormatDateTimeLabel(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
                TickInterval.Month => dateTime.ToString("MMM yyyy"),
                TickInterval.Week => dateTime.ToString("MMM dd"),
                TickInterval.Day => dateTime.ToString("MM/dd"),
                TickInterval.Hour => dateTime.ToString("MM/dd HH:mm"),
                _ => dateTime.ToString("MM/dd HH:mm")
        };
    }

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

    /// <summary>
    ///     Returns formatted values for every LineSeries in the chart at the given index.
    /// </summary>
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

    /// <summary>
    ///     Extracts the base name from a series title, removing "(Raw)" or "(smooth)" suffixes.
    /// </summary>
    private static(string BaseName, bool IsRaw, bool IsSmoothed) ParseSeriesTitle(string title)
    {
        if (title.EndsWith(" (Raw)") || title.EndsWith(" (raw)"))
            return (title.Substring(0, title.Length - 6), true, false);

        if (title.EndsWith(" (smooth)"))
            return (title.Substring(0, title.Length - 9), false, true);

        return (title, false, false);
    }

    /// <summary>
    ///     Identifies primary and secondary series names from chart series.
    /// </summary>
    private static(string? Primary, string? Secondary) IdentifySeriesNames(CartesianChart chart)
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

    /// <summary>
    ///     Extracts a formatted value from a line series at the specified index.
    /// </summary>
    private static string ExtractFormattedValue(Series series, int index)
    {
        if (series.Values == null || index < 0 || index >= series.Values.Count)
            return "N/A";

        try
        {
            var raw = series.Values[index];
            if (raw == null)
                return "N/A";

            var val = Convert.ToDouble(raw);
            return MathHelper.FormatDisplayedValue(val);
        }
        catch
        {
            return "N/A";
        }
    }

    /// <summary>
    ///     Returns formatted values in the order: Primary smooth, Secondary smooth, Primary Raw, Secondary Raw.
    ///     Format: "{metric subtype} smooth: {value}" or "{metric subtype} Raw: {value}"
    /// </summary>
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


    /// <summary>
    ///     Builds the formatted string in the specified order.
    /// </summary>
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

        var total = GetStackedTotalFromSeries(totalsBySeries);
        if (!total.HasValue)
            total = GetStackedTotalAtIndex(chart, index);

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

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(series.Title ?? "Series");
            if (string.IsNullOrWhiteSpace(baseName))
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

        foreach (var series in chart.Series.OfType<Series>())
        {
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(series.Title ?? "Series");
            if (string.IsNullOrWhiteSpace(baseName))
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
        if (double.IsNaN(value) || double.IsInfinity(value))
            return false;

        return true;
    }

    private static bool ShouldIncludeStackTotal(CartesianChart chart)
    {
        return chart.Tag is ChartStackingTooltipState state && state.IncludeTotal;
    }

    private static bool IsCumulativeStack(CartesianChart chart)
    {
        return chart.Tag is ChartStackingTooltipState state && state.IsCumulative;
    }

    private static double? GetStackedTotalAtIndex(CartesianChart chart, int index)
    {
        if (IsCumulativeStack(chart))
            return GetCumulativeTotalAtIndex(chart, index);

        var totalsBySeries = new Dictionary<string, (double? Smoothed, double? Raw)>(StringComparer.OrdinalIgnoreCase);

        foreach (var series in chart.Series.OfType<Series>())
        {
            var title = series.Title ?? "Series";
            var (baseName, isRaw, isSmoothed) = ParseSeriesTitle(title);
            if (string.IsNullOrWhiteSpace(baseName))
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

    private static double? GetCumulativeTotalAtIndex(CartesianChart chart, int index)
    {
        double? total = null;

        foreach (var series in chart.Series.OfType<Series>())
        {
            if (!TryExtractNumericValue(series, index, out var value))
                continue;

            if (double.IsNaN(value) || double.IsInfinity(value))
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

    /// <summary>
    ///     Safely gets the X-axis from a chart, handling potential disposal issues.
    /// </summary>
    private static Axis? GetXAxisSafely(CartesianChart? chart)
    {
        if (chart == null)
            return null;

        try
        {
            if (chart.AxisX != null && chart.AxisX.Count > 0)
                return chart.AxisX[0];
        }
        catch
        {
            // Axis may have been disposed
        }

        return null;
    }

    /// <summary>
    ///     Draws (or moves) a thin black vertical line by using an AxisSection with zero width.
    /// </summary>
    public static void UpdateVerticalLineForChart(ref CartesianChart chart, int index, ref AxisSection? sectionField)
    {
        if (chart == null || index < 0)
            return;

        var axis = GetXAxisSafely(chart);
        if (axis?.Sections == null)
            return;

        TryRemoveAxisSection(axis, sectionField);

        try
        {
            var line = new AxisSection
            {
                    Value = index,
                    SectionWidth = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
            };

            axis.Sections.Add(line);
            sectionField = line;
        }
        catch
        {
            sectionField = null;
        }
    }

    private static void TryRemoveAxisSection(Axis axis, AxisSection? section)
    {
        if (section == null)
            return;

        try
        {
            axis.Sections.Remove(section);
        }
        catch
        {
            // ignored
        }
    }


    public static Border CreateBorder(StackPanel stack)
    {
        var border = new Border
        {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 30, 30)),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Padding = new Thickness(4),
                Child = stack,
                MaxWidth = 600
        };

        return border;
    }

    /// <summary>
    ///     Creates a text block for hover tooltips with customizable styling.
    /// </summary>
    public static TextBlock SetHoverText(bool isBold = false)
    {
        var _hoverTimestampText = new TextBlock
        {
                Foreground = Brushes.White,
                FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(6, 2, 6, 4)
        };

        return _hoverTimestampText;
    }

    /// <summary>
    ///     Safely removes an axis section from a chart, handling potential disposal issues.
    /// </summary>
    public static void RemoveAxisSection(CartesianChart? chart, AxisSection? axisSection)
    {
        if (axisSection == null)
            return;

        var axis = chart != null ? GetXAxisSafely(chart) : null;
        if (axis?.Sections == null)
            return;

        try
        {
            axis.Sections.Remove(axisSection);
        }
        catch
        {
            // Section or axis may have been disposed - ignore
        }
    }

    /// <summary>
    ///     Clears all series from a chart and removes it from the timestamps dictionary.
    /// </summary>
    public static void ClearChart(CartesianChart? chart, Dictionary<CartesianChart, List<DateTime>>? chartTimestamps = null)
    {
        if (chart == null)
            return;
        chart.Series.Clear();
        chartTimestamps?.Remove(chart);
    }

    /// <summary>
    ///     Initializes the default tooltip for a chart if it doesn't already have one.
    /// </summary>
    public static void InitializeChartTooltip(CartesianChart chart)
    {
        if (chart == null)
            return;
        if (chart.DataTooltip is not SimpleChartTooltip)
            chart.DataTooltip = new SimpleChartTooltip();
    }

    /// <summary>
    ///     Gets the timestamp text for a given index from a chart's timestamp dictionary.
    /// </summary>
    public static string GetTimestampTextForIndex(int index, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, params CartesianChart[] charts)
    {
        foreach (var chart in charts)
            if (chartTimestamps.TryGetValue(chart, out var list) && index >= 0 && index < list.Count)
                return list[index].ToString("yyyy-MM-dd HH:mm:ss");

        return "Timestamp: N/A";
    }

    /// <summary>
    ///     Positions a hover popup with standard offsets.
    /// </summary>
    public static void PositionHoverPopup(Popup popup, double horizontalOffset = RenderingDefaults.HoverPopupOffsetPx, double verticalOffset = RenderingDefaults.HoverPopupOffsetPx)
    {
        if (popup == null)
            return;
        if (!popup.IsOpen)
            popup.IsOpen = true;
        popup.HorizontalOffset = 0;
        popup.VerticalOffset = 0;
        popup.HorizontalOffset = horizontalOffset;
        popup.VerticalOffset = verticalOffset;
    }

    /// <summary>
    ///     Collects all valid numeric values from raw data and smoothed values.
    /// </summary>
    private static List<double> CollectAllValues(List<MetricData> rawData, List<double> smoothedValues)
    {
        var allValues = new List<double>();

        foreach (var point in rawData)
            if (point.Value.HasValue)
                allValues.Add((double)point.Value.Value);

        foreach (var value in smoothedValues)
            if (!double.IsNaN(value) && !double.IsInfinity(value))
                allValues.Add(value);

        return allValues;
    }

    /// <summary>
    ///     Resets Y-axis to invalid state when no valid data is available.
    /// </summary>
    private static void ResetYAxisToInvalid(Axis yAxis)
    {
        yAxis.MinValue = double.NaN;
        yAxis.MaxValue = double.NaN;
        yAxis.Separator = new Separator();
        yAxis.ShowLabels = false;
    }

    /// <summary>
    ///     Applies padding to min/max values based on data range.
    /// </summary>
    private static(double MinValue, double MaxValue) ApplyPadding(double dataMin, double dataMax, double range)
    {
        var minValue = dataMin;
        var maxValue = dataMax;

        if (range <= double.Epsilon)
        {
            var padding = Math.Max(Math.Abs(minValue) * 0.1, 1e-3);
            if (Math.Abs(minValue) < 1e-6)
            {
                minValue = -padding;
                maxValue = padding;
            }
            else
            {
                minValue -= padding;
                maxValue += padding;
                if (dataMin >= 0)
                    minValue = Math.Max(0, minValue);
            }
        }
        else
        {
            var padding = range * 0.05;
            minValue -= padding;
            maxValue += padding;
            if (dataMin >= 0)
                minValue = Math.Max(0, minValue);
        }

        return (minValue, maxValue);
    }

    /// <summary>
    ///     Calculates a "nice" tick interval for the given range.
    /// </summary>
    private static double CalculateNiceInterval(double range, double targetTicks = 10.0)
    {
        var rawTickInterval = range / targetTicks;
        if (rawTickInterval <= 0 || double.IsNaN(rawTickInterval) || double.IsInfinity(rawTickInterval))
            return range / targetTicks;

        double magnitude;
        try
        {
            var logValue = Math.Log10(Math.Abs(rawTickInterval));
            magnitude = Math.Pow(10, Math.Floor(logValue));
        }
        catch
        {
            magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(1e-6, rawTickInterval))));
        }

        var normalizedInterval = rawTickInterval / magnitude;
        var niceInterval = normalizedInterval switch
        {
                <= 1 => 1 * magnitude,
                <= 2 => 2 * magnitude,
                <= 5 => 5 * magnitude,
                _ => 10 * magnitude
        };

        niceInterval = MathHelper.RoundToThreeSignificantDigits(niceInterval);
        return niceInterval <= 0 || double.IsNaN(niceInterval) || double.IsInfinity(niceInterval) ? rawTickInterval : niceInterval;
    }

    /// <summary>
    ///     Calculates nice min/max values aligned to the tick interval.
    /// </summary>
    private static(double NiceMin, double NiceMax) CalculateNiceBounds(double minValue, double maxValue, double niceInterval, double dataMin)
    {
        var niceMin = Math.Floor(minValue / niceInterval) * niceInterval;
        var niceMax = Math.Ceiling(maxValue / niceInterval) * niceInterval;

        if (dataMin >= 0 && niceMin < 0)
            niceMin = 0;

        return (niceMin, niceMax);
    }

    /// <summary>
    ///     Normalizes Y-axis ticks to show uniform intervals (~10 ticks) with rounded bounds.
    /// </summary>
    public static void NormalizeYAxis(Axis yAxis, List<MetricData> rawData, List<double> smoothedValues)
    {
        var allValues = CollectAllValues(rawData, smoothedValues);
        LogInputCounts(rawData, smoothedValues, allValues);

        if (!TryGetValidMinMax(allValues, out var dataMin, out var dataMax))
        {
            ResetYAxisToInvalid(yAxis);
            return;
        }

        var padded = CalculatePaddedRange(dataMin, dataMax);
        LogPaddedRange(padded);

        if (!TryApplyFallbackAxis(yAxis, padded))
            ApplyNiceAxis(yAxis, padded, dataMin);

        FinalizeAxis(yAxis);
    }

    private static void LogInputCounts(List<MetricData> rawData, List<double> smoothedValues, List<double> allValues)
    {
        Debug.WriteLine($"[TransformChart] NormalizeYAxis: rawData={rawData?.Count ?? 0}, " + $"smoothedValues={smoothedValues?.Count ?? 0}, allValues={allValues.Count}");
    }

    private static void LogPaddedRange((double Min, double Max, double Range) padded)
    {
        Debug.WriteLine($"[TransformChart] NormalizeYAxis: After padding - " + $"minValue={padded.Min}, maxValue={padded.Max}, range={padded.Range}");
    }

    private static bool TryGetValidMinMax(List<double> values, out double min, out double max)
    {
        min = max = 0;

        if (!values.Any())
        {
            Debug.WriteLine("[TransformChart] NormalizeYAxis: No values, resetting Y-axis");
            return false;
        }

        min = values.Min();
        max = values.Max();

        Debug.WriteLine($"[TransformChart] NormalizeYAxis: dataMin={min}, dataMax={max}");

        if (double.IsNaN(min) || double.IsNaN(max) || double.IsInfinity(min) || double.IsInfinity(max))
        {
            Debug.WriteLine("[TransformChart] NormalizeYAxis: Invalid min/max, resetting Y-axis");
            return false;
        }

        return true;
    }

    private static(double Min, double Max, double Range) CalculatePaddedRange(double dataMin, double dataMax)
    {
        var range = dataMax - dataMin;
        var (minValue, maxValue) = ApplyPadding(dataMin, dataMax, range);
        return (minValue, maxValue, maxValue - minValue);
    }

    private static bool TryApplyFallbackAxis(Axis yAxis, (double Min, double Max, double Range) padded)
    {
        const double targetTicks = 10.0;
        var rawTickInterval = padded.Range / targetTicks;

        if (rawTickInterval > 0 && !double.IsNaN(rawTickInterval) && !double.IsInfinity(rawTickInterval))
            return false; // fallback not needed

        yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(padded.Min);
        yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(padded.Max);

        var fallbackStep = MathHelper.RoundToThreeSignificantDigits(padded.Range / targetTicks);

        yAxis.Separator = fallbackStep > 0 ?
                new Separator
                {
                        Step = fallbackStep
                } :
                new Separator();

        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);

        yAxis.ShowLabels = true;

        Debug.WriteLine($"[TransformChart] NormalizeYAxis: Using fallback - " + $"YMin={yAxis.MinValue}, YMax={yAxis.MaxValue}");

        return true;
    }

    private static void ApplyNiceAxis(Axis yAxis, (double Min, double Max, double Range) padded, double dataMin)
    {
        const double targetTicks = 10.0;

        var niceInterval = CalculateNiceInterval(padded.Range);

        var (niceMin, niceMax) = CalculateNiceBounds(padded.Min, padded.Max, niceInterval, dataMin);

        yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(niceMin);

        yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(niceMax);

        var step = MathHelper.RoundToThreeSignificantDigits(niceInterval);

        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            step = MathHelper.RoundToThreeSignificantDigits((yAxis.MaxValue - yAxis.MinValue) / targetTicks);

        yAxis.Separator = new Separator
        {
                Step = step
        };

        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);

        yAxis.ShowLabels = true;

        Debug.WriteLine($"[TransformChart] NormalizeYAxis: Final - " + $"YMin={yAxis.MinValue}, YMax={yAxis.MaxValue}, Step={step}");
    }

    private static void FinalizeAxis(Axis yAxis)
    {
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

    // linear gradients are based on SkiaSharp linear gradients
    // for more info please see:
    // https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/shaders/linear-gradient
    private static LinearGradientBrush? CreateYAxisGradientBrush(double minValue, double maxValue)
    {
        var range = maxValue - minValue;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            return null;

        var midValue = minValue < 0 && maxValue > 0 ? 0 : minValue + range / 2.0;
        var midOffset = (midValue - minValue) / range;

        var brush = new LinearGradientBrush
        {
                StartPoint = new Point(0, 1),
                EndPoint = new Point(0, 0),
                SpreadMethod = GradientSpreadMethod.Pad
        };

        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0x2B, 0x6C, 0xB0), 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xE9, 0xC4, 0x6A), midOffset));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(0xD6, 0x28, 0x28), 1.0));

        return brush;
    }

    /// <summary>
    ///     Adjusts chart control Height based on Y-axis tick count to ensure ticks are spaced 20-40px apart.
    ///     Charts live inside a ScrollViewer; if total chart heights exceed window height a scrollbar will appear.
    /// </summary>
    public static void AdjustChartHeightBasedOnYAxis(CartesianChart chart, double minHeight)
    {
        if (chart == null || chart.AxisY.Count == 0)
            return;

        var yAxis = chart.AxisY[0];

        if (double.IsNaN(yAxis.MinValue) || double.IsNaN(yAxis.MaxValue) || double.IsInfinity(yAxis.MinValue) || double.IsInfinity(yAxis.MaxValue))
        {
            chart.Height = minHeight;
            return;
        }

        var minValue = yAxis.MinValue;
        var maxValue = yAxis.MaxValue;
        var step = yAxis.Separator?.Step ?? 0;

        if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
        {
            step = (maxValue - minValue) / 10.0;
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                chart.Height = minHeight;
                return;
            }
        }

        var range = maxValue - minValue;
        var tickCount = (int)Math.Ceiling(range / step) + 1;

        tickCount = Math.Max(2, tickCount);

        const double tickSpacingPx = RenderingDefaults.ChartTickSpacingPx;
        const double paddingPx = RenderingDefaults.ChartPaddingPx;

        var calculatedHeight = tickCount * tickSpacingPx + paddingPx;

        calculatedHeight = Math.Max(minHeight, calculatedHeight);

        const double maxHeight = RenderingDefaults.ChartMaxHeightPx;
        calculatedHeight = Math.Min(maxHeight, calculatedHeight);

        chart.Height = calculatedHeight;
    }
}
