using System.Diagnostics;
using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Shared.Helpers;
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

        if (HasMultiSeriesMode(model))
            RenderMultiSeriesMode(targetChart, model);
        else
            RenderLegacyMode(targetChart, model);

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
    private void RenderMultiSeriesMode(CartesianChart targetChart, ChartRenderModel model)
    {
        // Reset color palette for this chart to ensure consistent color assignment
        ColourPalette.Reset(targetChart);

        var mainTimeline = GetMainTimeline(model);

        if (model.Series != null)
            foreach (var seriesResult in model.Series)
                RenderSingleSeries(targetChart, seriesResult, mainTimeline, model.SeriesMode);
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
    private void RenderSingleSeries(CartesianChart targetChart, SeriesResult seriesResult, List<DateTime> mainTimeline, ChartSeriesMode seriesMode)
    {
        var seriesColor = ColourPalette.Next(targetChart);

        var alignedRaw = AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);

        var alignedSmoothed = seriesResult.Smoothed != null ? AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline) : null;

        TryRenderSeries(targetChart, seriesResult, alignedSmoothed, seriesColor, seriesMode, true);

        TryRenderSeries(targetChart, seriesResult, alignedRaw, Colors.DarkGray, seriesMode, false);
    }

    private void TryRenderSeries(CartesianChart chart, SeriesResult result, IList<double>? values, Color color, ChartSeriesMode mode, bool isSmoothed)
    {
        if (values == null || values.Count == 0)
            return;

        if (isSmoothed && mode == ChartSeriesMode.RawOnly)
            return;

        if (!isSmoothed && mode == ChartSeriesMode.SmoothedOnly)
            return;

        var title = $"{result.DisplayName} ({(isSmoothed ? "smooth" : "raw")})";

        var series = CreateAndPopulateSeries(title, isSmoothed ? ChartRenderDefaults.SmoothedPointSize : ChartRenderDefaults.RawPointSize, isSmoothed ? ChartRenderDefaults.SmoothedLineThickness : ChartRenderDefaults.RawLineThickness, color, values);

        chart.Series.Add(series);
    }

    /// <summary>
    ///     Creates a line series and populates it with the provided values.
    /// </summary>
    private static LineSeries CreateAndPopulateSeries(string title, int pointSize, int lineThickness, Color color, IList<double> values)
    {
        var series = ChartHelper.CreateLineSeries(title, pointSize, lineThickness, color);
        if (series == null)
            throw new InvalidOperationException($"Failed to create line series: {title}");

        var validCount = 0;
        var nanCount = 0;
        double? firstValue = null;
        double? lastValue = null;

        foreach (var value in values)
        {
            series.Values.Add(value);
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                nanCount++;
            }
            else
            {
                validCount++;
                if (firstValue == null)
                    firstValue = value;
                lastValue = value;
            }
        }

        Debug.WriteLine($"[TransformChart] CreateAndPopulateSeries: title={title}, total={values.Count}, valid={validCount}, NaN={nanCount}, first={firstValue}, last={lastValue}");

        return series;
    }

    /// <summary>
    ///     Renders chart in legacy mode, using PrimaryRaw/PrimarySmoothed and optionally SecondaryRaw/SecondarySmoothed.
    ///     Values are aligned to NormalizedIntervals for proper X-axis rendering.
    /// </summary>
    private void RenderLegacyMode(CartesianChart targetChart, ChartRenderModel model)
    {
        RenderPrimarySeries(targetChart, model);
        RenderSecondarySeries(targetChart, model);
    }

    /// <summary>
    ///     Renders the primary series (raw and/or smoothed) in legacy mode, respecting SeriesMode.
    ///     Values are aligned to actual data timestamps - X-axis formatter maps positions to normalized intervals for display.
    /// </summary>
    private void RenderPrimarySeries(CartesianChart targetChart, ChartRenderModel model)
    {
        // Values must be aligned to actual data timestamps (not normalized intervals)
        // The X-axis range uses data timestamps count, and formatter maps to normalized intervals for labels
        var dataTimestamps = model.Timestamps ?? new List<DateTime>();

        Debug.WriteLine($"[TransformChart] RenderPrimarySeries: chart={targetChart.Name}, dataTimestamps={dataTimestamps.Count}, PrimaryRaw={model.PrimaryRaw?.Count ?? 0}, PrimarySmoothed={model.PrimarySmoothed?.Count ?? 0}");

        // Render smoothed series if available and enabled
        if (model.SeriesMode == ChartSeriesMode.RawAndSmoothed || model.SeriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.PrimarySmoothed != null && model.PrimarySmoothed.Count > 0)
            {
                var primarySmoothedLabel = FormatSeriesLabel(model, true, true);
                // Values should already be aligned to dataTimestamps from the strategy
                var smoothedPrimary = CreateAndPopulateSeries(primarySmoothedLabel, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.PrimaryColor, model.PrimarySmoothed.ToList());
                targetChart.Series.Add(smoothedPrimary);
                Debug.WriteLine($"[TransformChart] Added smoothed series: {primarySmoothedLabel}, values={model.PrimarySmoothed.Count}");
            }

        // Render raw series if enabled
        if (model.SeriesMode == ChartSeriesMode.RawAndSmoothed || model.SeriesMode == ChartSeriesMode.RawOnly)
            if (model.PrimaryRaw != null && model.PrimaryRaw.Count > 0)
            {
                var primaryRawLabel = FormatSeriesLabel(model, true, false);
                // Values should already be aligned to dataTimestamps from the strategy
                var rawPrimary = CreateAndPopulateSeries(primaryRawLabel, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.PrimaryRaw.ToList());
                targetChart.Series.Add(rawPrimary);
                Debug.WriteLine($"[TransformChart] Added raw series: {primaryRawLabel}, values={model.PrimaryRaw.Count}");
            }
    }

    /// <summary>
    ///     Renders the secondary series (raw and/or smoothed) in legacy mode, if available, respecting SeriesMode.
    ///     Values are aligned to actual data timestamps - X-axis formatter maps positions to normalized intervals for display.
    /// </summary>
    private void RenderSecondarySeries(CartesianChart targetChart, ChartRenderModel model)
    {
        if (model.SecondarySmoothed == null || model.SecondaryRaw == null)
            return;

        // Values should already be aligned to dataTimestamps from the strategy
        // Render smoothed series if available and enabled
        if (model.SeriesMode == ChartSeriesMode.RawAndSmoothed || model.SeriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.SecondarySmoothed.Count > 0)
            {
                var secondarySmoothedLabel = FormatSeriesLabel(model, false, true);
                var smoothedSecondary = CreateAndPopulateSeries(secondarySmoothedLabel, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.SecondaryColor, model.SecondarySmoothed.ToList());
                targetChart.Series.Add(smoothedSecondary);
            }

        // Render raw series if enabled
        if (model.SeriesMode == ChartSeriesMode.RawAndSmoothed || model.SeriesMode == ChartSeriesMode.RawOnly)
            if (model.SecondaryRaw.Count > 0)
            {
                var secondaryRawLabel = FormatSeriesLabel(model, false, false);
                var rawSecondary = CreateAndPopulateSeries(secondaryRawLabel, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.SecondaryRaw.ToList());
                targetChart.Series.Add(rawSecondary);
            }
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
    ///     - For operation charts: "MetricType:subtype1 (operation) MetricType:subtype2 (smooth/raw)"
    ///     - For independent charts: "MetricType:subtype (smooth/raw)"
    /// </summary>
    private static string FormatSeriesLabel(ChartRenderModel model, bool isPrimary, bool isSmoothed)
    {
        var smoothRaw = isSmoothed ? "smooth" : "raw";

        // If we have metric type and subtype information, use the new format
        var primaryMetricType = model.PrimaryMetricType ?? model.MetricType;
        var secondaryMetricType = model.SecondaryMetricType ?? primaryMetricType;

        if (!string.IsNullOrEmpty(primaryMetricType))
        {
            if (model.IsOperationChart && !string.IsNullOrEmpty(model.OperationType))
            {
                // Operation chart format: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth)"
                var primarySubtype = model.PrimarySubtype ?? string.Empty;
                var secondarySubtype = model.SecondarySubtype ?? string.Empty;
                var operation = model.OperationType;

                if (isPrimary)
                        // Primary series shows: "Weight:fat_free_mass (-) Weight:body_fat_mass (smooth/raw)"
                    return $"{primaryMetricType}:{primarySubtype} ({operation}) {secondaryMetricType}:{secondarySubtype} ({smoothRaw})";

                // Secondary series (shouldn't happen for operation charts, but handle it)
                return $"{secondaryMetricType}:{secondarySubtype} ({smoothRaw})";
            }

            // Independent chart format: "Weight:fat_free_mass (smooth/raw)"
            var subtype = isPrimary ? model.PrimarySubtype ?? string.Empty : model.SecondarySubtype ?? string.Empty;
            var metricType = isPrimary ? primaryMetricType : secondaryMetricType;

            if (!string.IsNullOrEmpty(subtype))
                return $"{metricType}:{subtype} ({smoothRaw})";

            // Fallback if no subtype
            return $"{metricType} ({smoothRaw})";
        }

        // Fallback to old format if metric type info is not available
        var seriesName = isPrimary ? model.PrimarySeriesName : model.SecondarySeriesName;
        return $"{seriesName} ({smoothRaw})";
    }

    /// <summary>
    ///     Aligns a series' values to the main timeline by interpolating/mapping values.
    ///     Uses forward-fill for missing values (carries last known value forward).
    /// </summary>
    private static List<double> AlignSeriesToTimeline(List<DateTime> seriesTimestamps, List<double> seriesValues, List<DateTime> mainTimeline)
    {
        if (seriesTimestamps.Count == 0 || seriesValues.Count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        if (seriesTimestamps.Count != seriesValues.Count)
            return mainTimeline.Select(_ => double.NaN).ToList();

        // Create a dictionary for quick lookup
        var valueMap = new Dictionary<DateTime, double>();
        for (var i = 0; i < seriesTimestamps.Count; i++)
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
