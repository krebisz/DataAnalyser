using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Adapters;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Tooltip;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
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
    private readonly ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface> _renderPlanAdapterDispatcher;
    private readonly ChartRenderPlanProjector _renderPlanProjector;
    private readonly IUserNotificationService _notificationService;
    private readonly ChartTooltipManager _tooltipManager;

    public ChartUpdateCoordinator(
        ChartComputationEngine computationEngine,
        ChartRenderEngine renderEngine,
        ChartTooltipManager tooltipManager,
        Dictionary<CartesianChart, List<DateTime>> chartTimestamps,
        IUserNotificationService notificationService,
        ChartRenderPlanProjector? renderPlanProjector = null,
        ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface>? renderPlanAdapterDispatcher = null)
    {
        _chartComputationEngine = computationEngine ?? throw new ArgumentNullException(nameof(computationEngine));
        _chartRenderEngine = renderEngine ?? throw new ArgumentNullException(nameof(renderEngine));
        _tooltipManager = tooltipManager ?? throw new ArgumentNullException(nameof(tooltipManager));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _renderPlanProjector = renderPlanProjector ?? new ChartRenderPlanProjector();
        _renderPlanAdapterDispatcher = renderPlanAdapterDispatcher
            ?? new ChartRenderPlanAdapterDispatcher<LiveChartsRenderSurface>([new LiveChartsRenderPlanAdapter()]);
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
    public async Task UpdateChartUsingStrategyAsync(CartesianChart targetChart, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel = null, double minHeight = 400.0, string? metricType = null, string? primarySubtype = null, string? secondarySubtype = null, string? operationType = null, bool isOperationChart = false, string? secondaryMetricType = null, string? displayPrimaryMetricType = null, string? displaySecondaryMetricType = null, string? displayPrimarySubtype = null, string? displaySecondarySubtype = null, bool isStacked = false, bool isCumulative = false, IReadOnlyList<SeriesResult>? overlaySeries = null, bool useRenderPlanAdapter = false, ChartProgramKind renderProgramKind = ChartProgramKind.Main)
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
                        LastRenderPlanAdapterResult = null;

                        if (useRenderPlanAdapter)
                        {
                            var renderPlan = BuildChartRenderPlan(model, metricType, isCumulative, renderProgramKind);
                            LastRenderPlanAdapterResult = _renderPlanAdapterDispatcher.ApplyAsync(
                                new LiveChartsRenderSurface(targetChart, _chartRenderEngine, minHeight),
                                renderPlan).AsTask().GetAwaiter().GetResult();
                        }
                        else
                        {
                            // Render series (sync render engine)
                            _chartRenderEngine.Render(targetChart, model, minHeight);
                        }

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

    private ChartRenderPlan BuildChartRenderPlan(ChartRenderModel model, string? title, bool isCumulative, ChartProgramKind programKind)
    {
        var timeline = ResolveRenderPlanTimeline(model);
        var sourceSignature = BuildRenderPlanSignature(model, timeline);
        var program = new ChartProgram(
            programKind,
            ResolveDisplayMode(model, isCumulative),
            title ?? model.MetricType ?? model.PrimarySeriesName,
            timeline.Count > 0 ? timeline[0] : DateTime.MinValue,
            timeline.Count > 0 ? timeline[^1] : DateTime.MinValue,
            timeline,
            BuildChartSeriesPrograms(model),
            sourceSignature);

        var plan = _renderPlanProjector.ProjectCartesian(program);
        return plan with
        {
            Metadata = BuildRenderPlanMetadata(model, programKind, sourceSignature, program.DisplayMode),
            OverlaySeries = BuildOverlayRenderPlanSeries(model, timeline, programKind)
        };
    }

    private static ChartDisplayMode ResolveDisplayMode(ChartRenderModel model, bool isCumulative)
    {
        if (model.IsStacked)
            return ChartDisplayMode.Stacked;

        return isCumulative ? ChartDisplayMode.Summed : ChartDisplayMode.Regular;
    }

    private static IReadOnlyList<ChartSeriesProgram> BuildChartSeriesPrograms(ChartRenderModel model)
    {
        if (model.Series != null && model.Series.Count > 0)
        {
            return model.Series
                .Select(series => new ChartSeriesProgram(
                    series.SeriesId,
                    series.DisplayName,
                    series.RawValues,
                    series.Smoothed ?? series.RawValues))
                .ToArray();
        }

        var programs = new List<ChartSeriesProgram>
        {
            new(
                "primary",
                model.PrimarySeriesName,
                model.PrimaryRaw.ToList(),
                model.PrimarySmoothed.ToList())
        };

        if (model.SecondaryRaw != null && model.SecondaryRaw.Count > 0)
        {
            programs.Add(new ChartSeriesProgram(
                "secondary",
                model.SecondarySeriesName,
                model.SecondaryRaw.ToList(),
                (model.SecondarySmoothed ?? model.SecondaryRaw).ToList()));
        }

        return programs;
    }

    private IReadOnlyList<ChartSeriesPlan> BuildOverlayRenderPlanSeries(ChartRenderModel model, IReadOnlyList<DateTime> timeline, ChartProgramKind programKind)
    {
        if (model.OverlaySeries == null || model.OverlaySeries.Count == 0)
            return Array.Empty<ChartSeriesPlan>();

        var overlayPrograms = model.OverlaySeries
            .Select((series, index) => new ChartSeriesProgram(
                string.IsNullOrWhiteSpace(series.SeriesId) ? $"overlay_{index}" : series.SeriesId,
                series.DisplayName,
                series.RawValues,
                series.Smoothed ?? series.RawValues))
            .ToArray();

        var overlayProgram = new ChartProgram(
            programKind,
            ChartDisplayMode.Regular,
            model.MetricType ?? model.PrimarySeriesName,
            timeline.Count > 0 ? timeline[0] : DateTime.MinValue,
            timeline.Count > 0 ? timeline[^1] : DateTime.MinValue,
            timeline,
            overlayPrograms,
            $"{BuildRenderPlanSignature(model, timeline)}:overlay");

        return _renderPlanProjector.ProjectCartesian(overlayProgram).Series;
    }

    private static List<DateTime> ResolveRenderPlanTimeline(ChartRenderModel model)
    {
        if (model.Timestamps.Count > 0)
            return model.Timestamps.ToList();

        if (model.Series == null || model.Series.Count == 0)
            return [];

        return model.Series
            .SelectMany(series => series.Timestamps)
            .Distinct()
            .OrderBy(timestamp => timestamp)
            .ToList();
    }

    private static string BuildRenderPlanSignature(ChartRenderModel model, IReadOnlyList<DateTime> timeline)
    {
        var from = timeline.Count > 0 ? timeline[0].ToString("O") : string.Empty;
        var to = timeline.Count > 0 ? timeline[^1].ToString("O") : string.Empty;
        return $"legacy-render:{model.MetricType}:{model.PrimarySeriesName}:{model.SecondarySeriesName}:{from}:{to}:{timeline.Count}";
    }

    private static IReadOnlyDictionary<string, string> BuildRenderPlanMetadata(
        ChartRenderModel model,
        ChartProgramKind programKind,
        string sourceSignature,
        ChartDisplayMode displayMode)
    {
        var metadata = new Dictionary<string, string>
        {
            ["Projection"] = "ChartRenderModel",
            ["ProgramKind"] = programKind.ToString(),
            [LiveChartsRenderPlanAdapter.TickIntervalMetadataKey] = model.TickInterval.ToString(),
            [LiveChartsRenderPlanAdapter.SeriesModeMetadataKey] = model.SeriesMode.ToString(),
            [LiveChartsRenderPlanAdapter.IsStackedMetadataKey] = model.IsStacked.ToString(),
            [LiveChartsRenderPlanAdapter.IsOperationChartMetadataKey] = model.IsOperationChart.ToString()
        };

        AddMetadata(metadata, LiveChartsRenderPlanAdapter.UnitMetadataKey, model.Unit);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.MetricTypeMetadataKey, model.MetricType);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.PrimaryMetricTypeMetadataKey, model.PrimaryMetricType);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.SecondaryMetricTypeMetadataKey, model.SecondaryMetricType);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.PrimarySubtypeMetadataKey, model.PrimarySubtype);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.SecondarySubtypeMetadataKey, model.SecondarySubtype);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.DisplayPrimaryMetricTypeMetadataKey, model.DisplayPrimaryMetricType);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.DisplaySecondaryMetricTypeMetadataKey, model.DisplaySecondaryMetricType);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.DisplayPrimarySubtypeMetadataKey, model.DisplayPrimarySubtype);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.DisplaySecondarySubtypeMetadataKey, model.DisplaySecondarySubtype);
        AddMetadata(metadata, LiveChartsRenderPlanAdapter.OperationTypeMetadataKey, model.OperationType);
        ChartRenderPlanVocabularyMetadata.AddTo(
            metadata,
            programKind,
            sourceSignature,
            displayMode,
            overlayCount: model.OverlaySeries?.Count ?? 0);

        return metadata;
    }

    private static void AddMetadata(IDictionary<string, string> metadata, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            metadata[key] = value;
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
