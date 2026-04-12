using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration;

/// <summary>
///     Coordinates turning a computation strategy into a rendered chart.
/// </summary>
public class ChartUpdateCoordinator
{
    private readonly ChartComputationEngine _chartComputationEngine;
    private readonly ChartRenderEngine _chartRenderEngine;

    private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    private readonly ChartRenderGate _renderGate = new();
    private readonly IUserNotificationService _notificationService;
    private readonly ChartTooltipManager _tooltipManager;

    public ChartUpdateCoordinator(ChartComputationEngine computationEngine, ChartRenderEngine renderEngine, ChartTooltipManager tooltipManager, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IUserNotificationService notificationService)
    {
        _chartComputationEngine = computationEngine ?? throw new ArgumentNullException(nameof(computationEngine));
        _chartRenderEngine = renderEngine ?? throw new ArgumentNullException(nameof(renderEngine));
        _tooltipManager = tooltipManager ?? throw new ArgumentNullException(nameof(tooltipManager));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    /// <summary>
    ///     Global series rendering mode for all charts.
    ///     Can be changed at runtime (e.g., from config or UI).
    /// </summary>
    public ChartSeriesMode SeriesMode { get; set; } = ChartSeriesMode.RawAndSmoothed;

    /// <summary>
    ///     Runs the supplied strategy, then renders the result into the target chart.
    ///     If the strategy returns null, the chart is cleared.
    /// </summary>
    public async Task UpdateChartUsingStrategyAsync(CartesianChart targetChart, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel = null, double minHeight = 400.0, string? metricType = null, string? primarySubtype = null, string? secondarySubtype = null, string? operationType = null, bool isOperationChart = false, string? secondaryMetricType = null, string? displayPrimaryMetricType = null, string? displaySecondaryMetricType = null, string? displayPrimarySubtype = null, string? displaySecondarySubtype = null, bool isStacked = false, bool isCumulative = false, IReadOnlyList<SeriesResult>? overlaySeries = null)
    {
        if (targetChart == null)
            throw new ArgumentNullException(nameof(targetChart));
        if (strategy == null)
            throw new ArgumentNullException(nameof(strategy));

        // -------------------------
        // Phase 1: Compute
        // -------------------------
        var result = await _chartComputationEngine.ComputeAsync(strategy);

        if (result == null)
        {
            ChartHelper.ClearChart(targetChart, _chartTimestamps);
            return;
        }

        // -------------------------
        // Phase 2: Build render model
        // -------------------------
        var cumulativeBundle = isCumulative ? BuildCumulativeSeries(result, strategy, primaryLabel, secondaryLabel) : (RenderSeries: null, OriginalSeries: null);
        var renderSeries = cumulativeBundle.RenderSeries;

        var model = BuildChartRenderModel(strategy, result, targetChart, primaryLabel, secondaryLabel, metricType, primarySubtype, secondarySubtype, operationType, isOperationChart, secondaryMetricType, displayPrimaryMetricType, displaySecondaryMetricType, displayPrimarySubtype, displaySecondarySubtype, isStacked, renderSeries, overlaySeries);

        if (isStacked || isCumulative)
            targetChart.Tag = new ChartStackingTooltipState(true, isCumulative, isCumulative ? cumulativeBundle.OriginalSeries : null, overlaySeries?.Select(series => series.DisplayName).ToList());
        else if (targetChart.Tag is ChartStackingTooltipState)
            targetChart.Tag = null;

        // -------------------------
        // Phase 3: Render + post-render sync
        // -------------------------
        _renderGate.ExecuteWhenReady(targetChart,
                () =>
                {
                    try
                    {
                        // Render series (sync render engine)
                        _chartRenderEngine.Render(targetChart, model, minHeight);

                        // Track timestamps for tooltips / hover sync
                        _chartTimestamps[targetChart] = model.Timestamps;

                        // Keep tooltip manager in sync (timestamps only in this coordinator)
                        _tooltipManager?.UpdateChartTimestamps(targetChart, model.Timestamps);

                        // Normalise Y-axis and adjust chart height based on rendered data
                        if (targetChart.AxisY.Count > 0)
                            NormalizeYAxisForChart(targetChart, model, minHeight);

                        // Force chart update (important if chart was hidden when rendered)
                        targetChart.Update(true, true);
                    }
                    catch (Exception ex)
                    {
                        _notificationService.ShowError("Chart Error", $"Error rendering chart: {ex.Message}");
                        ChartHelper.ClearChart(targetChart, _chartTimestamps);
                    }
                });
    }


    /// <summary>
    ///     Builds a ChartRenderModel from a computation result and strategy metadata.
    /// </summary>
    private ChartRenderModel BuildChartRenderModel(IChartComputationStrategy strategy, ChartComputationResult result, CartesianChart targetChart, string primaryLabel, string? secondaryLabel, string? metricType, string? primarySubtype, string? secondarySubtype, string? operationType, bool isOperationChart, string? secondaryMetricType, string? displayPrimaryMetricType, string? displaySecondaryMetricType, string? displayPrimarySubtype, string? displaySecondarySubtype, bool isStacked, List<SeriesResult>? overrideSeries, IReadOnlyList<SeriesResult>? overlaySeries)
    {
        return new ChartRenderModel
        {
                PrimarySeriesName = strategy.PrimaryLabel ?? primaryLabel,
                SecondarySeriesName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,

                PrimaryRaw = result.PrimaryRawValues ?? new List<double>(),
                PrimarySmoothed = result.PrimarySmoothed ?? new List<double>(),

                SecondaryRaw = result.SecondaryRawValues,
                SecondarySmoothed = result.SecondarySmoothed,

                PrimaryColor = ColourPalette.Next(targetChart),
                SecondaryColor = result.SecondaryRawValues != null && result.SecondarySmoothed != null ? ColourPalette.Next(targetChart) : Colors.Red,

                Unit = result.Unit,
                Timestamps = result.Timestamps ?? new List<DateTime>(),
                IntervalIndices = result.IntervalIndices,
                NormalizedIntervals = result.NormalizedIntervals,
                TickInterval = result.TickInterval,

                // Pass through the coordinator's global mode
                SeriesMode = SeriesMode,
                IsStacked = isStacked,

                // Metric information for label formatting
                MetricType = metricType,
                PrimaryMetricType = metricType,
                SecondaryMetricType = secondaryMetricType,
                PrimarySubtype = primarySubtype,
                SecondarySubtype = secondarySubtype,
                DisplayPrimaryMetricType = displayPrimaryMetricType,
                DisplaySecondaryMetricType = displaySecondaryMetricType,
                DisplayPrimarySubtype = displayPrimarySubtype,
                DisplaySecondarySubtype = displaySecondarySubtype,
                OperationType = operationType,
                IsOperationChart = isOperationChart,

                // NEW: Multi-series support - when Series is present, it takes precedence
                Series = overrideSeries ?? result.Series,
                OverlaySeries = overlaySeries?.ToList()
        };
    }

    private(List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeries(ChartComputationResult result, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel)
    {
        return ChartCumulativeSeriesBuilder.Build(result, strategy, primaryLabel, secondaryLabel);
    }

    private static bool ShouldUseStackedTotals(ChartRenderModel model)
    {
        return model.IsStacked && (
            (model.Series != null && model.Series.Count > 1) ||
            model.SecondaryRaw != null ||
            model.SecondarySmoothed != null);
    }

    /// <summary>
    ///     Builds synthetic MetricData from the render model for Y-axis normalization.
    ///     Handles both multi-series mode and legacy primary/secondary mode.
    /// </summary>
    private List<MetricData> BuildSyntheticRawData(ChartRenderModel model)
    {
        return ChartYAxisDataBuilder.BuildSyntheticRawData(model);
    }

    /// <summary>
    ///     Collects all smoothed values from the render model for Y-axis normalization.
    ///     Handles both multi-series mode and legacy primary/secondary mode.
    /// </summary>
    private List<double> CollectSmoothedValues(ChartRenderModel model)
    {
        return ChartYAxisDataBuilder.CollectSmoothedValues(model);
    }

    /// <summary>
    ///     Normalizes the Y-axis and adjusts chart height based on all rendered data.
    ///     Handles both multi-series mode and legacy primary/secondary mode.
    /// </summary>
    private void NormalizeYAxisForChart(CartesianChart targetChart, ChartRenderModel model, double minHeight)
    {
        var yAxis = targetChart.AxisY[0];
        var syntheticRawData = BuildSyntheticRawData(model);
        var smoothedList = CollectSmoothedValues(model);

        ChartYAxisDataBuilder.EnsureOverlayExtremes(syntheticRawData, model.OverlaySeries, model.Unit);

        if (model.IsStacked)
            ChartYAxisDataBuilder.EnsureStackedBaseline(syntheticRawData, smoothedList, model.Unit);

        Debug.WriteLine($"[TransformChart] NormalizeYAxisForChart: chart={targetChart.Name}, syntheticRawData={syntheticRawData.Count}, smoothedList={smoothedList.Count}");

        ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);
        ChartHelper.ApplyTransformChartGradient(targetChart, yAxis);

        ApplyOverlayRangeIfNeeded(yAxis, model.OverlaySeries);

        Debug.WriteLine($"[TransformChart] After NormalizeYAxis: chart={targetChart.Name}, YMin={yAxis.MinValue}, YMax={yAxis.MaxValue}, ShowLabels={yAxis.ShowLabels}");

        ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
        ChartHelper.InitializeChartTooltip(targetChart);
    }

    private static void ApplyOverlayRangeIfNeeded(Axis yAxis, IReadOnlyList<SeriesResult>? overlaySeries)
    {
        if (overlaySeries == null || overlaySeries.Count == 0 || yAxis == null)
            return;

        var (min, max) = ChartYAxisDataBuilder.GetOverlayMinMax(overlaySeries);
        if (!min.HasValue || !max.HasValue)
            return;

        var currentMin = yAxis.MinValue;
        var currentMax = yAxis.MaxValue;

        if (double.IsNaN(currentMin) || double.IsNaN(currentMax))
        {
            currentMin = min.Value;
            currentMax = max.Value;
        }

        var updatedMin = Math.Min(currentMin, min.Value);
        var updatedMax = Math.Max(currentMax, max.Value);

        if (updatedMin.Equals(currentMin) && updatedMax.Equals(currentMax))
            return;

        var range = updatedMax - updatedMin;
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range))
            range = Math.Max(Math.Abs(updatedMax) * 0.1, 1e-3);

        var paddedMin = updatedMin - range * 0.05;
        var paddedMax = updatedMax + range * 0.05;

        if (updatedMin >= 0 && paddedMin < 0)
            paddedMin = 0;

        yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(paddedMin);
        yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(paddedMax);

        var step = MathHelper.RoundToThreeSignificantDigits((yAxis.MaxValue - yAxis.MinValue) / 10.0);
        yAxis.Separator = step > 0 ? new Separator { Step = step } : new Separator();
        yAxis.LabelFormatter = value => MathHelper.FormatDisplayedValue(value);
        yAxis.ShowLabels = true;
        yAxis.Labels = null;
    }

}
