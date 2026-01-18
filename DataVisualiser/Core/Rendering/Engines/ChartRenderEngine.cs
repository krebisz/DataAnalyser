using System.Diagnostics;
using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Shared.Helpers;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Engines;

public sealed class ChartRenderEngine
{
    /// <summary>
    ///     Renders chart series based on the provided model.
    ///     IMPORTANT: Ensures consistent data ordering:
    ///     - PrimaryRaw/PrimarySmoothed = first selected metric subtype
    ///     - SecondaryRaw/SecondarySmoothed = second selected metric subtype (when applicable)
    ///     This ordering is maintained across all charts and strategies.
    /// </summary>
    public void Render(CartesianChart targetChart, ChartRenderModel model, double minHeight = 400.0)
    {
        ValidateInputs(targetChart, model);
        PrepareChart(targetChart);

        LogRenderStart(targetChart, model);

        var isStacked = ShouldStackSeries(model);
        var seriesMode = isStacked ? ResolveStackedSeriesMode() : model.SeriesMode;

        if (HasMultiSeriesMode(model))
            RenderMultiSeriesMode(targetChart, model, isStacked, seriesMode);
        else
            RenderLegacyMode(targetChart, model, isStacked, seriesMode);

        LogRenderEnd(targetChart);

        ConfigureAxes(targetChart, model);
    }

    private static void PrepareChart(CartesianChart chart)
    {
        ChartHelper.ClearChart(chart);
    }

    private static void ConfigureAxes(CartesianChart chart, ChartRenderModel model)
    {
        ConfigureXAxis(chart, model);
        ConfigureYAxis(chart);
    }

    private static void LogRenderStart(CartesianChart chart, ChartRenderModel model)
    {
        Debug.WriteLine($"[TransformChart] Render: chart={chart.Name}, " + $"Timestamps={model.Timestamps?.Count ?? 0}, " + $"PrimaryRaw={model.PrimaryRaw?.Count ?? 0}, " + $"PrimarySmoothed={model.PrimarySmoothed?.Count ?? 0}, " + $"NormalizedIntervals={model.NormalizedIntervals?.Count ?? 0}");
    }

    private static void LogRenderEnd(CartesianChart chart)
    {
        Debug.WriteLine($"[TransformChart] After Render: chart={chart.Name}, " + $"SeriesCount={chart.Series?.Count ?? 0}");
    }

    /// <summary>
    ///     Validates that required inputs are not null.
    /// </summary>
    private static void ValidateInputs(CartesianChart targetChart, ChartRenderModel model)
    {
        if (targetChart == null)
            throw new ArgumentNullException(nameof(targetChart));
        if (model == null)
            throw new ArgumentNullException(nameof(model));
    }

    /// <summary>
    ///     Clears all series from the target chart.
    /// </summary>
    private static void ClearChart(CartesianChart targetChart)
    {
        targetChart.Series.Clear();
    }

    /// <summary>
    ///     Determines if the model uses multi-series mode (Series array is present and non-empty).
    /// </summary>
    private static bool HasMultiSeriesMode(ChartRenderModel model)
    {
        return model.Series != null && model.Series.Count > 0;
    }

    /// <summary>
    ///     Renders chart in multi-series mode, where multiple series are rendered from the Series array.
    ///     Each series is aligned to a main timeline and rendered according to the SeriesMode setting.
    /// </summary>
    private void RenderMultiSeriesMode(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        // Reset color palette for this chart to ensure consistent color assignment
        ColourPalette.Reset(targetChart);

        var mainTimeline = GetMainTimeline(model);

        if (model.Series == null)
            return;

        if (isStacked)
        {
            RenderStackedMultiSeries(targetChart, model.Series, mainTimeline);
            return;
        }

        foreach (var seriesResult in model.Series)
            RenderSingleSeries(targetChart, seriesResult, mainTimeline, seriesMode, isStacked);
    }

    /// <summary>
    ///     Gets the main timeline for series alignment, using model.Timestamps or falling back to union of all series
    ///     timestamps.
    /// </summary>
    private static List<DateTime> GetMainTimeline(ChartRenderModel model)
    {
        var mainTimeline = model.Timestamps;
        if (mainTimeline == null || mainTimeline.Count == 0)
                // Fallback: use union of all series timestamps
            if (model.Series != null)
                mainTimeline = model.Series.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();

        return mainTimeline ?? new List<DateTime>();
    }

    /// <summary>
    ///     Renders a single series result, including both raw and smoothed series based on SeriesMode.
    /// </summary>
    private void RenderSingleSeries(CartesianChart targetChart, SeriesResult seriesResult, List<DateTime> mainTimeline, ChartSeriesMode seriesMode, bool isStacked)
    {
        var seriesColor = ColourPalette.Next(targetChart);

        var alignedRaw = AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);

        var alignedSmoothed = seriesResult.Smoothed != null ? AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline) : null;

        TryRenderSeries(targetChart, seriesResult, alignedSmoothed, seriesColor, seriesMode, true, isStacked);

        TryRenderSeries(targetChart, seriesResult, alignedRaw, Colors.DarkGray, seriesMode, false, isStacked);
    }

    private void TryRenderSeries(CartesianChart chart, SeriesResult result, IList<double>? values, Color color, ChartSeriesMode mode, bool isSmoothed, bool isStacked)
    {
        if (values == null || values.Count == 0)
            return;

        if (isSmoothed && mode == ChartSeriesMode.RawOnly)
            return;

        if (!isSmoothed && mode == ChartSeriesMode.SmoothedOnly)
            return;

        var title = $"{result.DisplayName} ({(isSmoothed ? "smooth" : "raw")})";

        var series = CreateAndPopulateSeries(title, isSmoothed ? ChartRenderDefaults.SmoothedPointSize : ChartRenderDefaults.RawPointSize, isSmoothed ? ChartRenderDefaults.SmoothedLineThickness : ChartRenderDefaults.RawLineThickness, color, values, isStacked);

        chart.Series.Add(series);
    }

    /// <summary>
    ///     Creates a line series and populates it with the provided values.
    /// </summary>
    private static LineSeries CreateAndPopulateSeries(string title, int pointSize, int lineThickness, Color color, IList<double> values, bool isStacked)
    {
        var series = CreateSeries(title, pointSize, lineThickness, color, isStacked);

        var validCount = 0;
        var nanCount = 0;
        double? firstValue = null;
        double? lastValue = null;

        foreach (var value in values)
        {
            var normalized = isStacked && (double.IsNaN(value) || double.IsInfinity(value)) ? 0.0 : value;
            series.Values.Add(normalized);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                nanCount++;
            }
            else
            {
                validCount++;
                if (firstValue == null)
                    firstValue = normalized;
                lastValue = normalized;
            }
        }

        Debug.WriteLine($"[TransformChart] CreateAndPopulateSeries: title={title}, total={values.Count}, valid={validCount}, NaN={nanCount}, first={firstValue}, last={lastValue}");

        return series;
    }

    /// <summary>
    ///     Renders chart in legacy mode, using PrimaryRaw/PrimarySmoothed and optionally SecondaryRaw/SecondarySmoothed.
    ///     Values are aligned to NormalizedIntervals for proper X-axis rendering.
    /// </summary>
    private void RenderLegacyMode(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        if (isStacked)
        {
            RenderStackedLegacyMode(targetChart, model);
            return;
        }

        RenderPrimarySeries(targetChart, model, isStacked, seriesMode);
        RenderSecondarySeries(targetChart, model, isStacked, seriesMode);
    }

    private void RenderStackedMultiSeries(CartesianChart targetChart, IReadOnlyList<SeriesResult> seriesResults, List<DateTime> mainTimeline)
    {
        foreach (var seriesResult in seriesResults)
        {
            var values = ResolveStackedSeriesValues(seriesResult, mainTimeline, out var usedSmoothed);
            var stats = GetValueStats(values);
            Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={seriesResult.DisplayName}, usedSmoothed={usedSmoothed}, rawCount={seriesResult.RawValues?.Count ?? 0}, smoothCount={seriesResult.Smoothed?.Count ?? 0}, alignedCount={values.Count}, valid={stats.Valid}, NaN={stats.NaN}");

            if (!HasAnyValidValue(values))
            {
                Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={seriesResult.DisplayName} skipped (no valid values).");
                continue;
            }

            var title = $"{seriesResult.DisplayName} ({(usedSmoothed ? "smooth" : "raw")})";
            var color = ColourPalette.Next(targetChart);
            var series = CreateAndPopulateSeries(title, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, color, values, true);
            targetChart.Series.Add(series);
        }
    }

    private void RenderStackedLegacyMode(CartesianChart targetChart, ChartRenderModel model)
    {
        RenderStackedLegacySeries(targetChart, model, true);
        RenderStackedLegacySeries(targetChart, model, false);
    }

    private void RenderStackedLegacySeries(CartesianChart targetChart, ChartRenderModel model, bool isPrimary)
    {
        var smoothed = isPrimary ? model.PrimarySmoothed : model.SecondarySmoothed;
        var raw = isPrimary ? model.PrimaryRaw : model.SecondaryRaw;
        var useSmoothed = smoothed != null && smoothed.Count > 0;
        var values = useSmoothed ? smoothed : raw;

        if (values == null || values.Count == 0)
            return;

        var title = FormatSeriesLabel(model, isPrimary, useSmoothed);
        var color = isPrimary ? model.PrimaryColor : model.SecondaryColor;
        var stats = GetValueStats(values);
        Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={title}, usedSmoothed={useSmoothed}, count={values.Count}, valid={stats.Valid}, NaN={stats.NaN}");
        var series = CreateAndPopulateSeries(title, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, color, values, true);
        targetChart.Series.Add(series);
    }

    /// <summary>
    ///     Renders the primary series (raw and/or smoothed) in legacy mode, respecting SeriesMode.
    ///     Values are aligned to actual data timestamps - X-axis formatter maps positions to normalized intervals for display.
    /// </summary>
    private void RenderPrimarySeries(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        // Values must be aligned to actual data timestamps (not normalized intervals)
        // The X-axis range uses data timestamps count, and formatter maps to normalized intervals for labels
        var dataTimestamps = model.Timestamps ?? new List<DateTime>();

        Debug.WriteLine($"[TransformChart] RenderPrimarySeries: chart={targetChart.Name}, dataTimestamps={dataTimestamps.Count}, PrimaryRaw={model.PrimaryRaw?.Count ?? 0}, PrimarySmoothed={model.PrimarySmoothed?.Count ?? 0}");

        // Render smoothed series if available and enabled
        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.PrimarySmoothed != null && model.PrimarySmoothed.Count > 0)
            {
                var primarySmoothedLabel = FormatSeriesLabel(model, true, true);
                // Values should already be aligned to dataTimestamps from the strategy
                var smoothedPrimary = CreateAndPopulateSeries(primarySmoothedLabel, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.PrimaryColor, model.PrimarySmoothed.ToList(), isStacked);
                targetChart.Series.Add(smoothedPrimary);
                Debug.WriteLine($"[TransformChart] Added smoothed series: {primarySmoothedLabel}, values={model.PrimarySmoothed.Count}");
            }

        // Render raw series if enabled
        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.RawOnly)
            if (model.PrimaryRaw != null && model.PrimaryRaw.Count > 0)
            {
                var primaryRawLabel = FormatSeriesLabel(model, true, false);
                // Values should already be aligned to dataTimestamps from the strategy
                var rawPrimary = CreateAndPopulateSeries(primaryRawLabel, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.PrimaryRaw.ToList(), isStacked);
                targetChart.Series.Add(rawPrimary);
                Debug.WriteLine($"[TransformChart] Added raw series: {primaryRawLabel}, values={model.PrimaryRaw.Count}");
            }
    }

    /// <summary>
    ///     Renders the secondary series (raw and/or smoothed) in legacy mode, if available, respecting SeriesMode.
    ///     Values are aligned to actual data timestamps - X-axis formatter maps positions to normalized intervals for display.
    /// </summary>
    private void RenderSecondarySeries(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        if (model.SecondarySmoothed == null || model.SecondaryRaw == null)
            return;

        // Values should already be aligned to dataTimestamps from the strategy
        // Render smoothed series if available and enabled
        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.SecondarySmoothed.Count > 0)
            {
                var secondarySmoothedLabel = FormatSeriesLabel(model, false, true);
                var smoothedSecondary = CreateAndPopulateSeries(secondarySmoothedLabel, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.SecondaryColor, model.SecondarySmoothed.ToList(), isStacked);
                targetChart.Series.Add(smoothedSecondary);
            }

        // Render raw series if enabled
        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.RawOnly)
            if (model.SecondaryRaw.Count > 0)
            {
                var secondaryRawLabel = FormatSeriesLabel(model, false, false);
                var rawSecondary = CreateAndPopulateSeries(secondaryRawLabel, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.SecondaryRaw.ToList(), isStacked);
                targetChart.Series.Add(rawSecondary);
            }
    }

    private static bool ShouldStackSeries(ChartRenderModel model)
    {
        if (!model.IsStacked)
            return false;

        if (model.Series != null && model.Series.Count > 1)
            return true;

        return model.SecondaryRaw != null || model.SecondarySmoothed != null;
    }

    private static ChartSeriesMode ResolveStackedSeriesMode()
    {
        return ChartSeriesMode.SmoothedOnly;
    }

    private static IList<double> ResolveStackedSeriesValues(SeriesResult seriesResult, List<DateTime> mainTimeline, out bool usedSmoothed)
    {
        usedSmoothed = false;

        if (seriesResult.Smoothed != null && seriesResult.Smoothed.Count > 0)
        {
            var alignedSmoothed = AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline);
            if (HasAnyValidValue(alignedSmoothed))
            {
                usedSmoothed = true;
                return alignedSmoothed;
            }
        }

        return AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);
    }

    private static bool HasAnyValidValue(IList<double> values)
    {
        return values.Any(value => !double.IsNaN(value) && !double.IsInfinity(value));
    }

    private static (int Valid, int NaN) GetValueStats(IList<double> values)
    {
        var valid = 0;
        var nan = 0;

        foreach (var value in values)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                nan++;
            else
                valid++;
        }

        return (valid, nan);
    }

    private static LineSeries CreateSeries(string title, int pointSize, int lineThickness, Color color, bool isStacked)
    {
        var series = isStacked ? new StackedAreaSeries() : new LineSeries();

        series.Title = title;
        series.Values = new ChartValues<double>();
        series.PointGeometrySize = pointSize;
        series.StrokeThickness = lineThickness;
        series.Stroke = new SolidColorBrush(color);
        series.DataLabels = false;
        series.Fill = isStacked
                ? new SolidColorBrush(Color.FromArgb(110, color.R, color.G, color.B))
                : Brushes.Transparent;
        series.LineSmoothness = 0;

        if (series is StackedAreaSeries stackedArea)
            stackedArea.StackMode = StackMode.Values;

        return series;
    }

    /// <summary>
    ///     Configures the X-axis with uniform spacing, label formatting, and tick intervals.
    ///     Forces every data point to live at x = 0,1,2,... so tick spacing is uniform.
    ///     The formatter retrieves proper datetime labels from ChartRenderModel.
    /// </summary>
    private static void ConfigureXAxis(CartesianChart targetChart, ChartRenderModel model)
    {
        if (targetChart.AxisX.Count == 0)
            return;

        var xAxis = targetChart.AxisX[0];
        xAxis.Title = ChartRenderDefaults.AxisTitleTime;
        xAxis.ShowLabels = true; // Re-enable labels when rendering data

        // Use actual data timestamps for axis range (this determines how many values we have)
        var dataTimestamps = model.Timestamps ?? new List<DateTime>();
        var normalizedIntervals = model.NormalizedIntervals ?? new List<DateTime>();

        // The axis range must match the number of data points (values array length)
        var total = dataTimestamps.Count > 0 ? dataTimestamps.Count : normalizedIntervals.Count;

        Debug.WriteLine($"[TransformChart] ConfigureXAxis: dataTimestamps={dataTimestamps.Count}, normalizedIntervals={normalizedIntervals.Count}, total={total}, chart={targetChart.Name}");

        // Use normalized intervals for label formatting, but map indices from data timestamps
        var timestampsForLabels = normalizedIntervals.Count > 0 ? normalizedIntervals : dataTimestamps;

        // === LABEL FORMATTER ===
        // Map data point index to timestamp for label display
        // Use data timestamps directly (they're what the values are aligned to)
        xAxis.LabelFormatter = indexAsDouble =>
        {
            var idx = (int)indexAsDouble;
            if (idx < 0 || idx >= total)
                return string.Empty;

            // Use data timestamp directly if available (this is what the values are aligned to)
            if (idx < dataTimestamps.Count)
                return ChartHelper.FormatDateTimeLabel(dataTimestamps[idx], model.TickInterval);

            // Fallback to normalized interval if data timestamps not available
            if (idx < normalizedIntervals.Count)
                return ChartHelper.FormatDateTimeLabel(normalizedIntervals[idx], model.TickInterval);

            return string.Empty;
        };

        // === UNIFORM STEP ===
        // Roughly 10 visible ticks, auto-adjusted
        var desiredTicks = ChartRenderDefaults.DesiredXAxisTickCount;
        var step = Math.Max(1, total / (double)desiredTicks);

        // Cleaner numbers
        step = MathHelper.RoundToThreeSignificantDigits(step);

        xAxis.Separator = new Separator
        {
                Step = step
        };

        // These three settings ensure LiveCharts does NOT compress or stretch based on time
        xAxis.MinValue = 0;
        xAxis.MaxValue = total - 1;
        xAxis.Labels = null; // Force formatter rather than static label array
    }

    /// <summary>
    ///     Configures the Y-axis by re-enabling labels when rendering data.
    ///     Y-axis is already uniform by definition (numeric axis automatically spaces evenly).
    /// </summary>
    private static void ConfigureYAxis(CartesianChart targetChart)
    {
        if (targetChart.AxisY.Count > 0)
            targetChart.AxisY[0].ShowLabels = true;
    }

    /// <summary>
    ///     Formats series labels according to the new requirements:
    ///     - For operation charts: "MetricType : subtype (operation) MetricType : subtype (smooth/raw)"
    ///     - For independent charts: "MetricType : subtype (smooth/raw)"
    /// </summary>
    private static string FormatSeriesLabel(ChartRenderModel model, bool isPrimary, bool isSmoothed)
    {
        var smoothRaw = isSmoothed ? "smooth" : "raw";

        // If we have metric type and subtype information, use the new format
        var primaryMetricType = model.DisplayPrimaryMetricType ?? model.PrimaryMetricType ?? model.MetricType;
        var secondaryMetricType = model.DisplaySecondaryMetricType ?? model.SecondaryMetricType ?? primaryMetricType;
        var primarySubtype = model.DisplayPrimarySubtype ?? model.PrimarySubtype;
        var secondarySubtype = model.DisplaySecondarySubtype ?? model.SecondarySubtype;

        if (!string.IsNullOrEmpty(primaryMetricType))
        {
            if (model.IsOperationChart && !string.IsNullOrEmpty(model.OperationType))
            {
                // Operation chart format: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth)"
                var operation = model.OperationType;
                var primaryLabel = FormatMetricLabel(primaryMetricType, primarySubtype);
                var secondaryLabel = FormatMetricLabel(secondaryMetricType, secondarySubtype);

                if (isPrimary)
                        // Primary series shows: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth/raw)"
                    return $"{primaryLabel} ({operation}) {secondaryLabel} ({smoothRaw})";

                // Secondary series (shouldn't happen for operation charts, but handle it)
                return $"{secondaryLabel} ({smoothRaw})";
            }

            // Independent chart format: "Weight:fat_free_mass (smooth/raw)"
            var subtype = isPrimary ? primarySubtype ?? string.Empty : secondarySubtype ?? string.Empty;
            var metricType = isPrimary ? primaryMetricType : secondaryMetricType;

            var metricLabel = FormatMetricLabel(metricType, subtype);

            // Fallback if no subtype
            return $"{metricLabel} ({smoothRaw})";
        }

        // Fallback to old format if metric type info is not available
        var seriesName = isPrimary ? model.PrimarySeriesName : model.SecondarySeriesName;
        return $"{seriesName} ({smoothRaw})";
    }

    private static string FormatMetricLabel(string metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype) || subtype == "(All)")
            return metricType;

        return $"{metricType} : {subtype}";
    }

    /// <summary>
    ///     Aligns a series' values to the main timeline by interpolating/mapping values.
    ///     Uses forward-fill for missing values (carries last known value forward).
    /// </summary>
    private static List<double> AlignSeriesToTimeline(List<DateTime> seriesTimestamps, List<double> seriesValues, List<DateTime> mainTimeline)
    {
        if (seriesTimestamps.Count == 0 || seriesValues.Count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        var count = Math.Min(seriesTimestamps.Count, seriesValues.Count);
        if (count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        // Create a dictionary for quick lookup
        var valueMap = new Dictionary<DateTime, double>();
        for (var i = 0; i < count; i++)
        {
            var ts = seriesTimestamps[i];
            var val = seriesValues[i];
            // Use the latest value if there are duplicate timestamps
            valueMap[ts] = val;
        }

        var aligned = new List<double>(mainTimeline.Count);
        var lastValue = double.NaN;

        foreach (var timestamp in mainTimeline)
                // Try exact match first
            if (valueMap.TryGetValue(timestamp, out var exactValue))
            {
                aligned.Add(exactValue);
                lastValue = exactValue;
            }
            else
            {
                // Try to find nearest timestamp (within same day for simplicity)
                var day = timestamp.Date;
                var dayMatch = valueMap.Keys.FirstOrDefault(ts => ts.Date == day);

                if (dayMatch != default && valueMap.TryGetValue(dayMatch, out var dayValue))
                {
                    aligned.Add(dayValue);
                    lastValue = dayValue;
                }
                else if (!double.IsNaN(lastValue))
                {
                    // Forward fill: use last known value
                    aligned.Add(lastValue);
                }
                else
                {
                    // No value available
                    aligned.Add(double.NaN);
                }
            }

        return aligned;
    }
}
