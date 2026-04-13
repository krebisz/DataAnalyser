using System.Diagnostics;
using System.Windows.Media;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Helpers;
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
        ArgumentNullException.ThrowIfNull(targetChart);
        ArgumentNullException.ThrowIfNull(model);

        ChartHelper.ClearChart(targetChart);

        Debug.WriteLine($"[ChartRenderEngine] Render: chart={targetChart.Name}, Timestamps={model.Timestamps?.Count ?? 0}, PrimaryRaw={model.PrimaryRaw?.Count ?? 0}, PrimarySmoothed={model.PrimarySmoothed?.Count ?? 0}, NormalizedIntervals={model.NormalizedIntervals?.Count ?? 0}");

        var isStacked = ShouldStackSeries(model);
        var seriesMode = isStacked ? ChartSeriesMode.SmoothedOnly : model.SeriesMode;

        if (HasMultiSeriesMode(model))
            RenderMultiSeriesMode(targetChart, model, isStacked, seriesMode);
        else
            RenderLegacyMode(targetChart, model, isStacked, seriesMode);

        RenderOverlaySeries(targetChart, model);
        BringOverlaySeriesToFront(targetChart, model);

        Debug.WriteLine($"[ChartRenderEngine] After Render: chart={targetChart.Name}, SeriesCount={targetChart.Series?.Count ?? 0}");

        ConfigureXAxis(targetChart, model);
        ConfigureYAxis(targetChart);
    }

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

    private static List<DateTime> GetMainTimeline(ChartRenderModel model)
    {
        var mainTimeline = model.Timestamps;
        if (mainTimeline == null || mainTimeline.Count == 0)
            if (model.Series != null)
                mainTimeline = model.Series.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();

        return mainTimeline ?? new List<DateTime>();
    }

    private void RenderSingleSeries(CartesianChart targetChart, SeriesResult seriesResult, List<DateTime> mainTimeline, ChartSeriesMode seriesMode, bool isStacked)
    {
        var seriesColor = ColourPalette.Next(targetChart);

        var alignedRaw = SeriesAlignmentHelper.AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.RawValues, mainTimeline);
        var alignedSmoothed = seriesResult.Smoothed != null ? SeriesAlignmentHelper.AlignSeriesToTimeline(seriesResult.Timestamps, seriesResult.Smoothed, mainTimeline) : null;

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
        var series = ChartSeriesMaterializer.CreateAndPopulateSeries(title, isSmoothed ? ChartRenderDefaults.SmoothedPointSize : ChartRenderDefaults.RawPointSize, isSmoothed ? ChartRenderDefaults.SmoothedLineThickness : ChartRenderDefaults.RawLineThickness, color, values, isStacked);
        chart.Series.Add(series);
    }

    /// <summary>
    ///     Renders chart in legacy mode, using PrimaryRaw/PrimarySmoothed and optionally SecondaryRaw/SecondarySmoothed.
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
            var values = ChartSeriesMaterializer.ResolveStackedSeriesValues(seriesResult, mainTimeline, out var usedSmoothed);
            var stats = ChartSeriesMaterializer.GetValueStats(values);
            Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={seriesResult.DisplayName}, usedSmoothed={usedSmoothed}, rawCount={seriesResult.RawValues?.Count ?? 0}, smoothCount={seriesResult.Smoothed?.Count ?? 0}, alignedCount={values.Count}, valid={stats.Valid}, NaN={stats.NaN}");

            if (!ChartSeriesMaterializer.HasAnyValidValue(values))
            {
                Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={seriesResult.DisplayName} skipped (no valid values).");
                continue;
            }

            var title = $"{seriesResult.DisplayName} ({(usedSmoothed ? "smooth" : "raw")})";
            var color = ColourPalette.Next(targetChart);
            var series = ChartSeriesMaterializer.CreateAndPopulateSeries(title, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, color, values, true);
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

        var title = ChartSeriesLabelFormatter.FormatSeriesLabel(model, isPrimary, useSmoothed);
        var color = isPrimary ? model.PrimaryColor : model.SecondaryColor;
        var stats = ChartSeriesMaterializer.GetValueStats(values);
        Debug.WriteLine($"[StackedRender] chart={targetChart.Name}, series={title}, usedSmoothed={useSmoothed}, count={values.Count}, valid={stats.Valid}, NaN={stats.NaN}");
        var series = ChartSeriesMaterializer.CreateAndPopulateSeries(title, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, color, values, true);
        targetChart.Series.Add(series);
    }

    private void RenderOverlaySeries(CartesianChart targetChart, ChartRenderModel model)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return;

        var mainTimeline = GetMainTimeline(model);

        foreach (var seriesResult in model.OverlaySeries)
        {
            var values = ChartSeriesMaterializer.ResolveStackedSeriesValues(seriesResult, mainTimeline, out var usedSmoothed);
            if (!ChartSeriesMaterializer.HasAnyValidValue(values))
                continue;

            var title = $"{seriesResult.DisplayName} ({(usedSmoothed ? "smooth" : "raw")})";
            var color = Colors.Black;
            var pointSize = usedSmoothed ? ChartRenderDefaults.SmoothedPointSize : ChartRenderDefaults.RawPointSize;
            var lineThickness = usedSmoothed ? ChartRenderDefaults.SmoothedLineThickness : ChartRenderDefaults.RawLineThickness;

            var series = ChartSeriesMaterializer.CreateAndPopulateSeries(title, pointSize, lineThickness, color, values, false);
            targetChart.Series.Add(series);
        }
    }

    private static void BringOverlaySeriesToFront(CartesianChart targetChart, ChartRenderModel model)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return;

        var overlayNames = new HashSet<string>(model.OverlaySeries
            .Select(series => ChartStackingTooltipState.NormalizeOverlayName(series.DisplayName)),
            StringComparer.OrdinalIgnoreCase);

        if (overlayNames.Count == 0)
            return;

        var overlaySeries = targetChart.Series
            .OfType<Series>()
            .Where(series =>
            {
                var title = series.Title ?? string.Empty;
                var baseName = ChartStackingTooltipState.NormalizeOverlayName(title);
                return overlayNames.Contains(baseName);
            })
            .ToList();

        if (overlaySeries.Count == 0)
            return;

        foreach (var series in overlaySeries)
        {
            targetChart.Series.Remove(series);
            targetChart.Series.Add(series);
        }
    }

    private void RenderPrimarySeries(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        var dataTimestamps = model.Timestamps ?? new List<DateTime>();

        Debug.WriteLine($"[ChartRenderEngine] RenderPrimarySeries: chart={targetChart.Name}, dataTimestamps={dataTimestamps.Count}, PrimaryRaw={model.PrimaryRaw?.Count ?? 0}, PrimarySmoothed={model.PrimarySmoothed?.Count ?? 0}");

        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.PrimarySmoothed != null && model.PrimarySmoothed.Count > 0)
            {
                var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, true, true);
                var series = ChartSeriesMaterializer.CreateAndPopulateSeries(label, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.PrimaryColor, model.PrimarySmoothed.ToList(), isStacked);
                targetChart.Series.Add(series);
            }

        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.RawOnly)
            if (model.PrimaryRaw != null && model.PrimaryRaw.Count > 0)
            {
                var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, true, false);
                var series = ChartSeriesMaterializer.CreateAndPopulateSeries(label, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.PrimaryRaw.ToList(), isStacked);
                targetChart.Series.Add(series);
            }
    }

    private void RenderSecondarySeries(CartesianChart targetChart, ChartRenderModel model, bool isStacked, ChartSeriesMode seriesMode)
    {
        if (model.SecondarySmoothed == null || model.SecondaryRaw == null)
            return;

        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.SmoothedOnly)
            if (model.SecondarySmoothed.Count > 0)
            {
                var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, false, true);
                var series = ChartSeriesMaterializer.CreateAndPopulateSeries(label, ChartRenderDefaults.SmoothedPointSize, ChartRenderDefaults.SmoothedLineThickness, model.SecondaryColor, model.SecondarySmoothed.ToList(), isStacked);
                targetChart.Series.Add(series);
            }

        if (seriesMode == ChartSeriesMode.RawAndSmoothed || seriesMode == ChartSeriesMode.RawOnly)
            if (model.SecondaryRaw.Count > 0)
            {
                var label = ChartSeriesLabelFormatter.FormatSeriesLabel(model, false, false);
                var series = ChartSeriesMaterializer.CreateAndPopulateSeries(label, ChartRenderDefaults.RawPointSize, ChartRenderDefaults.RawLineThickness, Colors.DarkGray, model.SecondaryRaw.ToList(), isStacked);
                targetChart.Series.Add(series);
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

    private static void ConfigureXAxis(CartesianChart targetChart, ChartRenderModel model)
    {
        if (targetChart.AxisX.Count == 0)
            return;

        var xAxis = targetChart.AxisX[0];
        xAxis.Title = ChartRenderDefaults.AxisTitleTime;
        xAxis.ShowLabels = true;

        var dataTimestamps = model.Timestamps ?? new List<DateTime>();
        var normalizedIntervals = model.NormalizedIntervals ?? new List<DateTime>();
        var total = dataTimestamps.Count > 0 ? dataTimestamps.Count : normalizedIntervals.Count;

        Debug.WriteLine($"[ChartRenderEngine] ConfigureXAxis: dataTimestamps={dataTimestamps.Count}, normalizedIntervals={normalizedIntervals.Count}, total={total}, chart={targetChart.Name}");

        xAxis.LabelFormatter = indexAsDouble =>
        {
            var idx = (int)indexAsDouble;
            if (idx < 0 || idx >= total)
                return string.Empty;

            if (idx < dataTimestamps.Count)
                return ChartSeriesLabelFormatter.FormatDateTimeLabel(dataTimestamps[idx], model.TickInterval);

            if (idx < normalizedIntervals.Count)
                return ChartSeriesLabelFormatter.FormatDateTimeLabel(normalizedIntervals[idx], model.TickInterval);

            return string.Empty;
        };

        var desiredTicks = ChartRenderDefaults.DesiredXAxisTickCount;
        var step = Math.Max(1, total / (double)desiredTicks);
        step = MathHelper.RoundToThreeSignificantDigits(step);

        xAxis.Separator = new Separator
        {
                Step = step
        };

        xAxis.MinValue = 0;
        xAxis.MaxValue = total - 1;
        xAxis.Labels = null;
    }

    private static void ConfigureYAxis(CartesianChart targetChart)
    {
        if (targetChart.AxisY.Count > 0)
            targetChart.AxisY[0].ShowLabels = true;
    }
}
