using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.Coordinator;

/// <summary>
///     Coordinates turning a computation strategy into a rendered chart.
/// </summary>
public class ChartUpdateCoordinator
{
    private readonly ChartComputationEngine _chartComputationEngine;
    private readonly ChartRenderEngine _chartRenderEngine;

    private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    private readonly ChartRenderGate _renderGate = new();
    private readonly ChartTooltipManager _tooltipManager;

    public ChartUpdateCoordinator(ChartComputationEngine computationEngine, ChartRenderEngine renderEngine, ChartTooltipManager tooltipManager, Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
    {
        _chartComputationEngine = computationEngine ?? throw new ArgumentNullException(nameof(computationEngine));
        _chartRenderEngine = renderEngine ?? throw new ArgumentNullException(nameof(renderEngine));
        _tooltipManager = tooltipManager ?? throw new ArgumentNullException(nameof(tooltipManager));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
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
    public async Task UpdateChartUsingStrategyAsync(CartesianChart targetChart, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel = null, double minHeight = 400.0, string? metricType = null, string? primarySubtype = null, string? secondarySubtype = null, string? operationType = null, bool isOperationChart = false, string? secondaryMetricType = null, string? displayPrimaryMetricType = null, string? displaySecondaryMetricType = null, string? displayPrimarySubtype = null, string? displaySecondarySubtype = null, bool isStacked = false, bool isCumulative = false)
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
        var cumulativeBundle = isCumulative ? BuildCumulativeSeries(result, strategy, primaryLabel, secondaryLabel) : (RenderSeries: (List<SeriesResult>?)null, OriginalSeries: (List<SeriesResult>?)null);
        var renderSeries = cumulativeBundle.RenderSeries;

        var model = BuildChartRenderModel(strategy, result, targetChart, primaryLabel, secondaryLabel, metricType, primarySubtype, secondarySubtype, operationType, isOperationChart, secondaryMetricType, displayPrimaryMetricType, displaySecondaryMetricType, displayPrimarySubtype, displaySecondarySubtype, isStacked, renderSeries);

        if (isStacked || isCumulative)
        {
            targetChart.Tag = new ChartStackingTooltipState(true, isCumulative, isCumulative ? cumulativeBundle.OriginalSeries : null);
        }
        else if (targetChart.Tag is ChartStackingTooltipState)
        {
            targetChart.Tag = null;
        }

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
                        MessageBox.Show($"Error rendering chart: {ex.Message}", "Chart Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        ChartHelper.ClearChart(targetChart, _chartTimestamps);
                    }
                });
    }


    /// <summary>
    ///     Builds a ChartRenderModel from a computation result and strategy metadata.
    /// </summary>
    private ChartRenderModel BuildChartRenderModel(IChartComputationStrategy strategy, ChartComputationResult result, CartesianChart targetChart, string primaryLabel, string? secondaryLabel, string? metricType, string? primarySubtype, string? secondarySubtype, string? operationType, bool isOperationChart, string? secondaryMetricType, string? displayPrimaryMetricType, string? displaySecondaryMetricType, string? displayPrimarySubtype, string? displaySecondarySubtype, bool isStacked, List<SeriesResult>? overrideSeries)
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
                Series = overrideSeries ?? result.Series
        };
    }

    private (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeries(ChartComputationResult result, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel)
    {
        if (result.Series != null && result.Series.Count > 0)
            return BuildCumulativeSeriesFromMulti(result.Series);

        return BuildCumulativeSeriesFromLegacy(result, strategy, primaryLabel, secondaryLabel);
    }

    private (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeriesFromMulti(IReadOnlyList<SeriesResult> seriesResults)
    {
        var mainTimeline = seriesResults.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();
        if (mainTimeline.Count == 0)
            return (null, null);

        var cumulativeRaw = new double[mainTimeline.Count];
        var cumulativeSmoothed = new double[mainTimeline.Count];
        var cumulativeSeries = new List<SeriesResult>();
        var originalSeries = new List<SeriesResult>();

        foreach (var series in seriesResults)
        {
            var alignedRaw = AlignSeriesToTimeline(series.Timestamps, series.RawValues, mainTimeline);
            var alignedSmooth = AlignSeriesToTimeline(series.Timestamps, series.Smoothed ?? series.RawValues, mainTimeline);

            originalSeries.Add(new SeriesResult
            {
                    SeriesId = series.SeriesId,
                    DisplayName = series.DisplayName,
                    Timestamps = mainTimeline.ToList(),
                    RawValues = alignedRaw.ToList(),
                    Smoothed = alignedSmooth.ToList()
            });

            AddIntoCumulative(cumulativeRaw, alignedRaw);
            AddIntoCumulative(cumulativeSmoothed, alignedSmooth);

            cumulativeSeries.Add(new SeriesResult
            {
                    SeriesId = series.SeriesId,
                    DisplayName = series.DisplayName,
                    Timestamps = mainTimeline.ToList(),
                    RawValues = cumulativeRaw.ToList(),
                    Smoothed = cumulativeSmoothed.ToList()
            });
        }

        return (cumulativeSeries, originalSeries);
    }

    private (List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeriesFromLegacy(ChartComputationResult result, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel)
    {
        var timeline = result.Timestamps ?? new List<DateTime>();
        if (timeline.Count == 0)
            return (null, null);

        var cumulativeRaw = new double[timeline.Count];
        var cumulativeSmoothed = new double[timeline.Count];
        var cumulativeSeries = new List<SeriesResult>();
        var originalSeries = new List<SeriesResult>();

        AddIntoCumulative(cumulativeRaw, result.PrimaryRawValues);
        AddIntoCumulative(cumulativeSmoothed, result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed : result.PrimaryRawValues);

        originalSeries.Add(new SeriesResult
        {
                SeriesId = "primary-original",
                DisplayName = strategy.PrimaryLabel ?? primaryLabel,
                Timestamps = timeline.ToList(),
                RawValues = result.PrimaryRawValues.ToList(),
                Smoothed = result.PrimarySmoothed.Count > 0 ? result.PrimarySmoothed.ToList() : result.PrimaryRawValues.ToList()
        });

        cumulativeSeries.Add(new SeriesResult
        {
                SeriesId = "primary",
                DisplayName = strategy.PrimaryLabel ?? primaryLabel,
                Timestamps = timeline.ToList(),
                RawValues = cumulativeRaw.ToList(),
                Smoothed = cumulativeSmoothed.ToList()
        });

        if (result.SecondaryRawValues != null && result.SecondaryRawValues.Count > 0)
        {
            AddIntoCumulative(cumulativeRaw, result.SecondaryRawValues);
            var secondarySmooth = result.SecondarySmoothed != null && result.SecondarySmoothed.Count > 0 ? result.SecondarySmoothed : result.SecondaryRawValues;
            AddIntoCumulative(cumulativeSmoothed, secondarySmooth);

            originalSeries.Add(new SeriesResult
            {
                    SeriesId = "secondary-original",
                    DisplayName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                    Timestamps = timeline.ToList(),
                    RawValues = result.SecondaryRawValues.ToList(),
                    Smoothed = secondarySmooth.ToList()
            });

            cumulativeSeries.Add(new SeriesResult
            {
                    SeriesId = "secondary",
                    DisplayName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                    Timestamps = timeline.ToList(),
                    RawValues = cumulativeRaw.ToList(),
                    Smoothed = cumulativeSmoothed.ToList()
            });
        }

        return (cumulativeSeries, originalSeries);
    }

    private static void AddIntoCumulative(double[] cumulative, IList<double> values)
    {
        var count = Math.Min(cumulative.Length, values.Count);
        for (var i = 0; i < count; i++)
        {
            var value = values[i];
            if (double.IsNaN(value) || double.IsInfinity(value))
                continue;

            cumulative[i] += value;
        }
    }

    private static bool ShouldUseStackedTotals(ChartRenderModel model)
    {
        if (!model.IsStacked)
            return false;

        if (model.Series != null && model.Series.Count > 1)
            return true;

        return model.SecondaryRaw != null || model.SecondarySmoothed != null;
    }

    /// <summary>
    ///     Builds synthetic MetricData from the render model for Y-axis normalization.
    ///     Handles both multi-series mode and legacy primary/secondary mode.
    /// </summary>
    private List<MetricData> BuildSyntheticRawData(ChartRenderModel model)
    {
        if (ShouldUseStackedTotals(model))
            return BuildStackedRawData(model);

        return EnumerateRawPoints(model).Select(p => CreateMetric(p.Timestamp, p.Value, model.Unit)).ToList();
    }

    private List<MetricData> BuildStackedRawData(ChartRenderModel model)
    {
        var timestamps = GetStackTimeline(model);
        var totals = new double[timestamps.Count];
        var hasValue = new bool[timestamps.Count];

        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                var aligned = AlignSeriesToTimeline(series.Timestamps, series.RawValues, timestamps);
                for (var i = 0; i < aligned.Count; i++)
                {
                    var value = aligned[i];
                    if (double.IsNaN(value) || double.IsInfinity(value))
                        continue;

                    totals[i] += value;
                    hasValue[i] = true;
                }
            }
        }
        else
        {
            var primaryRaw = model.PrimaryRaw ?? new List<double>();
            var secondaryRaw = model.SecondaryRaw;
            var count = timestamps.Count;

            for (var i = 0; i < count; i++)
            {
                var sum = 0.0;
                var found = false;

                if (i < primaryRaw.Count && !double.IsNaN(primaryRaw[i]) && !double.IsInfinity(primaryRaw[i]))
                {
                    sum += primaryRaw[i];
                    found = true;
                }

                if (secondaryRaw != null && i < secondaryRaw.Count && !double.IsNaN(secondaryRaw[i]) && !double.IsInfinity(secondaryRaw[i]))
                {
                    sum += secondaryRaw[i];
                    found = true;
                }

                if (found)
                {
                    totals[i] = sum;
                    hasValue[i] = true;
                }
            }
        }

        var metrics = new List<MetricData>(timestamps.Count);
        for (var i = 0; i < timestamps.Count; i++)
        {
            var value = hasValue[i] ? (decimal)totals[i] : (decimal?)null;
            metrics.Add(new MetricData
            {
                    NormalizedTimestamp = timestamps[i],
                    Value = value,
                    Unit = model.Unit
            });
        }

        return metrics;
    }

    private static MetricData CreateMetric(DateTime timestamp, double value, string? unit)
    {
        return new MetricData
        {
                NormalizedTimestamp = timestamp,
                Value = double.IsNaN(value) ? null : (decimal)value,
                Unit = unit
        };
    }

    private IEnumerable<(DateTime Timestamp, double Value)> EnumerateRawPoints(ChartRenderModel model)
    {
        // Multi-series mode
        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                var count = Math.Min(series.Timestamps.Count, series.RawValues.Count);

                for (var i = 0; i < count; i++)
                    yield return (series.Timestamps[i], series.RawValues[i]);
            }

            yield break;
        }

        // Legacy primary/secondary mode
        var timestamps = model.Timestamps;
        var primaryRaw = model.PrimaryRaw ?? new List<double>();
        var secondaryRaw = model.SecondaryRaw;

        var primaryCount = Math.Min(timestamps.Count, primaryRaw.Count);

        for (var i = 0; i < primaryCount; i++)
        {
            yield return (timestamps[i], primaryRaw[i]);

            if (secondaryRaw != null && i < secondaryRaw.Count)
                yield return (timestamps[i], secondaryRaw[i]);
        }
    }

    /// <summary>
    ///     Collects all smoothed values from the render model for Y-axis normalization.
    ///     Handles both multi-series mode and legacy primary/secondary mode.
    /// </summary>
    private List<double> CollectSmoothedValues(ChartRenderModel model)
    {
        if (ShouldUseStackedTotals(model))
            return BuildStackedSmoothedValues(model);

        var smoothedList = new List<double>();

        if (model.Series != null && model.Series.Count > 0)
        {
            // Collect smoothed values from all series
            foreach (var series in model.Series)
                if (series.Smoothed != null)
                    smoothedList.AddRange(series.Smoothed);
        }
        else
        {
            // Fallback to primary/secondary for backward compatibility
            if (model.PrimarySmoothed != null)
                smoothedList.AddRange(model.PrimarySmoothed);
            if (model.SecondarySmoothed != null)
                smoothedList.AddRange(model.SecondarySmoothed);
        }

        return smoothedList;
    }

    private List<double> BuildStackedSmoothedValues(ChartRenderModel model)
    {
        var timestamps = GetStackTimeline(model);
        var totals = new double[timestamps.Count];
        var hasValue = new bool[timestamps.Count];

        if (model.Series != null && model.Series.Count > 0)
        {
            foreach (var series in model.Series)
            {
                if (series.Smoothed == null)
                    continue;

                var aligned = AlignSeriesToTimeline(series.Timestamps, series.Smoothed, timestamps);
                for (var i = 0; i < aligned.Count; i++)
                {
                    var value = aligned[i];
                    if (double.IsNaN(value) || double.IsInfinity(value))
                        continue;

                    totals[i] += value;
                    hasValue[i] = true;
                }
            }
        }
        else
        {
            var primarySmoothed = model.PrimarySmoothed ?? new List<double>();
            var secondarySmoothed = model.SecondarySmoothed;
            var count = timestamps.Count;

            for (var i = 0; i < count; i++)
            {
                var sum = 0.0;
                var found = false;

                if (i < primarySmoothed.Count && !double.IsNaN(primarySmoothed[i]) && !double.IsInfinity(primarySmoothed[i]))
                {
                    sum += primarySmoothed[i];
                    found = true;
                }

                if (secondarySmoothed != null && i < secondarySmoothed.Count && !double.IsNaN(secondarySmoothed[i]) && !double.IsInfinity(secondarySmoothed[i]))
                {
                    sum += secondarySmoothed[i];
                    found = true;
                }

                if (found)
                {
                    totals[i] = sum;
                    hasValue[i] = true;
                }
            }
        }

        var values = new List<double>(timestamps.Count);
        for (var i = 0; i < timestamps.Count; i++)
            values.Add(hasValue[i] ? totals[i] : double.NaN);

        return values;
    }

    private static List<DateTime> GetStackTimeline(ChartRenderModel model)
    {
        var timeline = model.Timestamps;
        if (timeline == null || timeline.Count == 0)
        {
            if (model.Series != null)
                timeline = model.Series.SelectMany(s => s.Timestamps).Distinct().OrderBy(t => t).ToList();
        }

        return timeline ?? new List<DateTime>();
    }

    private static List<double> AlignSeriesToTimeline(List<DateTime> seriesTimestamps, List<double> seriesValues, List<DateTime> mainTimeline)
    {
        if (seriesTimestamps.Count == 0 || seriesValues.Count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        var count = Math.Min(seriesTimestamps.Count, seriesValues.Count);
        if (count == 0)
            return mainTimeline.Select(_ => double.NaN).ToList();

        var valueMap = new Dictionary<DateTime, double>();
        for (var i = 0; i < count; i++)
            valueMap[seriesTimestamps[i]] = seriesValues[i];

        var aligned = new List<double>(mainTimeline.Count);
        var lastValue = double.NaN;

        foreach (var timestamp in mainTimeline)
            if (valueMap.TryGetValue(timestamp, out var exactValue))
            {
                aligned.Add(exactValue);
                lastValue = exactValue;
            }
            else
            {
                var day = timestamp.Date;
                var dayMatch = valueMap.Keys.FirstOrDefault(ts => ts.Date == day);

                if (dayMatch != default && valueMap.TryGetValue(dayMatch, out var dayValue))
                {
                    aligned.Add(dayValue);
                    lastValue = dayValue;
                }
                else if (!double.IsNaN(lastValue))
                {
                    aligned.Add(lastValue);
                }
                else
                {
                    aligned.Add(double.NaN);
                }
            }

        return aligned;
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

        Debug.WriteLine($"[TransformChart] NormalizeYAxisForChart: chart={targetChart.Name}, syntheticRawData={syntheticRawData.Count}, smoothedList={smoothedList.Count}");

        ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);

        Debug.WriteLine($"[TransformChart] After NormalizeYAxis: chart={targetChart.Name}, YMin={yAxis.MinValue}, YMax={yAxis.MaxValue}, ShowLabels={yAxis.ShowLabels}");

        ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
        ChartHelper.InitializeChartTooltip(targetChart);
    }
}
