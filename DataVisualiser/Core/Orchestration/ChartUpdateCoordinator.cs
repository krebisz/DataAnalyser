using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Adapters;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;
using Separator = LiveCharts.Wpf.Separator;

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
    private readonly ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface> _renderPlanAdapterDispatcher;
    private readonly ChartRenderPlanProjector _renderPlanProjector;
    private readonly IUserNotificationService _notificationService;
    private readonly IChartTimestampSink? _tooltipManager;
    private readonly Func<UserControl>? _tooltipFactory;

    public ChartUpdateCoordinator(
        ChartComputationEngine computationEngine,
        ChartRenderEngine renderEngine,
        IChartTimestampSink? tooltipManager,
        Dictionary<CartesianChart, List<DateTime>> chartTimestamps,
        IUserNotificationService notificationService,
        ChartRenderPlanProjector? renderPlanProjector = null,
        ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface>? renderPlanAdapterDispatcher = null,
        Func<UserControl>? tooltipFactory = null)
    {
        _chartComputationEngine = computationEngine ?? throw new ArgumentNullException(nameof(computationEngine));
        _chartRenderEngine = renderEngine ?? throw new ArgumentNullException(nameof(renderEngine));
        _tooltipManager = tooltipManager;
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _renderPlanProjector = renderPlanProjector ?? new ChartRenderPlanProjector();
        _renderPlanAdapterDispatcher = renderPlanAdapterDispatcher
            ?? new ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface>([new LiveChartsRenderPlanAdapter()]);
        _tooltipFactory = tooltipFactory;
    }

    /// <summary>
    ///     Global series rendering mode for all charts.
    ///     Can be changed at runtime (e.g., from config or UI).
    /// </summary>
    public ChartSeriesMode SeriesMode { get; set; } = ChartSeriesMode.RawAndSmoothed;

    public ChartRenderAdapterResult? LastRenderPlanAdapterResult { get; private set; }

    /// <summary>
    ///     Runs the supplied strategy, then renders the result into the target chart.
    ///     If the strategy returns null, the chart is cleared.
    /// </summary>
    public async Task UpdateChartUsingStrategyAsync(CartesianChart targetChart, IChartComputationStrategy strategy, ChartUpdateRequest request)
    {
        if (targetChart == null)
            throw new ArgumentNullException(nameof(targetChart));
        if (strategy == null)
            throw new ArgumentNullException(nameof(strategy));
        if (request == null)
            throw new ArgumentNullException(nameof(request));

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
        var cumulativeBundle = request.IsCumulative ? BuildCumulativeSeries(result, strategy, request.PrimaryLabel, request.SecondaryLabel) : (RenderSeries: null, OriginalSeries: null);
        var renderSeries = cumulativeBundle.RenderSeries;

        var model = BuildChartRenderModel(strategy, result, targetChart, request.PrimaryLabel, request.SecondaryLabel, request.MetricType, request.PrimarySubtype, request.SecondarySubtype, request.OperationType, request.IsOperationChart, request.SecondaryMetricType, request.DisplayPrimaryMetricType, request.DisplaySecondaryMetricType, request.DisplayPrimarySubtype, request.DisplaySecondarySubtype, request.IsStacked, renderSeries, request.OverlaySeries);

        if (request.IsStacked || request.IsCumulative)
            targetChart.Tag = new ChartStackingTooltipState(true, request.IsCumulative, request.IsCumulative ? cumulativeBundle.OriginalSeries : null, request.OverlaySeries?.Select(series => series.DisplayName).ToList());
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
                        LastRenderPlanAdapterResult = null;

                        var renderPlan = CartesianMetricRenderPlanBuilder.Build(model, request.MetricType, request.IsCumulative, request.RenderProgramKind, _renderPlanProjector, request.RenderProgramRequest, request.RenderCapability, request.RenderDelivery);
                        LastRenderPlanAdapterResult = _renderPlanAdapterDispatcher.ApplyAsync(
                            new LiveChartsRenderSurface(targetChart, _chartRenderEngine, request.MinHeight),
                            renderPlan).AsTask().GetAwaiter().GetResult();

                        // Track timestamps for tooltips / hover sync
                        _chartTimestamps[targetChart] = model.Timestamps;

                        // Keep tooltip manager in sync (timestamps only in this coordinator)
                        _tooltipManager?.UpdateChartTimestamps(targetChart, model.Timestamps);

                        // Normalise Y-axis and adjust chart height based on rendered data
                        if (targetChart.AxisY.Count > 0)
                            NormalizeYAxisForChart(targetChart, model, request.MinHeight);

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
        ChartHelper.InitializeChartTooltip(targetChart, _tooltipFactory);
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
