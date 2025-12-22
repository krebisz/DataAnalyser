using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Models;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Media;

namespace DataVisualiser.Services
{
    /// <summary>
    /// Coordinates turning a computation strategy into a rendered chart.
    /// </summary>
    public class ChartUpdateCoordinator
    {
        private readonly ChartComputationEngine _chartComputationEngine;
        private readonly ChartRenderEngine _chartRenderEngine;
        private readonly ChartTooltipManager _tooltipManager;

        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;

        /// <summary>
        /// Global series rendering mode for all charts.
        /// Can be changed at runtime (e.g., from config or UI).
        /// </summary>
        public ChartSeriesMode SeriesMode { get; set; } = ChartSeriesMode.RawAndSmoothed;

        public ChartUpdateCoordinator(
            ChartComputationEngine computationEngine,
            ChartRenderEngine renderEngine,
            ChartTooltipManager tooltipManager,
            Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
        {
            _chartComputationEngine = computationEngine ?? throw new ArgumentNullException(nameof(computationEngine));
            _chartRenderEngine = renderEngine ?? throw new ArgumentNullException(nameof(renderEngine));
            _tooltipManager = tooltipManager ?? throw new ArgumentNullException(nameof(tooltipManager));
            _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        }

        /// <summary>
        /// Runs the supplied strategy, then renders the result into the target chart.
        /// If the strategy returns null, the chart is cleared.
        /// </summary>
        public async Task UpdateChartUsingStrategyAsync(
            CartesianChart targetChart,
            IChartComputationStrategy strategy,
            string primaryLabel,
            string? secondaryLabel = null,
            double minHeight = 400.0,
            string? metricType = null,
            string? primarySubtype = null,
            string? secondarySubtype = null,
            string? operationType = null,
            bool isOperationChart = false)
        {
            if (targetChart == null) throw new ArgumentNullException(nameof(targetChart));
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));

            var result = await _chartComputationEngine.ComputeAsync(strategy);

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // Build render model from computation result
            var model = BuildChartRenderModel(
                strategy,
                result,
                targetChart,
                primaryLabel,
                secondaryLabel,
                metricType,
                primarySubtype,
                secondarySubtype,
                operationType,
                isOperationChart);

            try
            {
                // Render series
                _chartRenderEngine.Render(targetChart, model, minHeight);

                // Track timestamps for tooltips / hover sync
                _chartTimestamps[targetChart] = model.Timestamps;

                // Keep tooltip manager in sync
                _tooltipManager?.UpdateChartTimestamps(targetChart, model.Timestamps);

                // Normalise Y-axis and adjust chart height based on all data we actually rendered
                if (targetChart.AxisY.Count > 0)
                {
                    NormalizeYAxisForChart(targetChart, model, minHeight);
                }

                // Force chart update (especially important if chart was hidden when rendered)
                targetChart.Update(true, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error rendering chart: {ex.Message}",
                    "Chart Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                ChartHelper.ClearChart(targetChart, _chartTimestamps);
            }
        }

        /// <summary>
        /// Builds a ChartRenderModel from a computation result and strategy metadata.
        /// </summary>
        private ChartRenderModel BuildChartRenderModel(
            IChartComputationStrategy strategy,
            ChartComputationResult result,
            CartesianChart targetChart,
            string primaryLabel,
            string? secondaryLabel,
            string? metricType,
            string? primarySubtype,
            string? secondarySubtype,
            string? operationType,
            bool isOperationChart)
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
                SecondaryColor = (result.SecondaryRawValues != null && result.SecondarySmoothed != null)
                    ? ColourPalette.Next(targetChart)
                    : Colors.Red,

                Unit = result.Unit,
                Timestamps = result.Timestamps ?? new List<DateTime>(),
                IntervalIndices = result.IntervalIndices,
                NormalizedIntervals = result.NormalizedIntervals,
                TickInterval = result.TickInterval,

                // Pass through the coordinator's global mode
                SeriesMode = this.SeriesMode,

                // Metric information for label formatting
                MetricType = metricType,
                PrimarySubtype = primarySubtype,
                SecondarySubtype = secondarySubtype,
                OperationType = operationType,
                IsOperationChart = isOperationChart,

                // NEW: Multi-series support - when Series is present, it takes precedence
                Series = result.Series
            };
        }

        /// <summary>
        /// Builds synthetic HealthMetricData from the render model for Y-axis normalization.
        /// Handles both multi-series mode and legacy primary/secondary mode.
        /// </summary>
        private List<HealthMetricData> BuildSyntheticRawData(ChartRenderModel model)
        {
            return EnumerateRawPoints(model)
                .Select(p => CreateMetric(p.Timestamp, p.Value, model.Unit))
                .ToList();
        }

        private static HealthMetricData CreateMetric(
            DateTime timestamp,
            double value,
            string unit)
        {
            return new HealthMetricData
            {
                NormalizedTimestamp = timestamp,
                Value = double.IsNaN(value) ? (decimal?)null : (decimal)value,
                Unit = unit
            };
        }

        private IEnumerable<(DateTime Timestamp, double Value)> EnumerateRawPoints(
            ChartRenderModel model)
        {
            // Multi-series mode
            if (model.Series != null && model.Series.Count > 0)
            {
                foreach (var series in model.Series)
                {
                    var count = Math.Min(series.Timestamps.Count, series.RawValues.Count);

                    for (int i = 0; i < count; i++)
                    {
                        yield return (series.Timestamps[i], series.RawValues[i]);
                    }
                }

                yield break;
            }

            // Legacy primary/secondary mode
            var timestamps = model.Timestamps;
            var primaryRaw = model.PrimaryRaw ?? new List<double>();
            var secondaryRaw = model.SecondaryRaw;

            var primaryCount = Math.Min(timestamps.Count, primaryRaw.Count);

            for (int i = 0; i < primaryCount; i++)
            {
                yield return (timestamps[i], primaryRaw[i]);

                if (secondaryRaw != null && i < secondaryRaw.Count)
                {
                    yield return (timestamps[i], secondaryRaw[i]);
                }
            }
        }

        /// <summary>
        /// Collects all smoothed values from the render model for Y-axis normalization.
        /// Handles both multi-series mode and legacy primary/secondary mode.
        /// </summary>
        private List<double> CollectSmoothedValues(ChartRenderModel model)
        {
            var smoothedList = new List<double>();

            if (model.Series != null && model.Series.Count > 0)
            {
                // Collect smoothed values from all series
                foreach (var series in model.Series)
                {
                    if (series.Smoothed != null)
                        smoothedList.AddRange(series.Smoothed);
                }
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

        /// <summary>
        /// Normalizes the Y-axis and adjusts chart height based on all rendered data.
        /// Handles both multi-series mode and legacy primary/secondary mode.
        /// </summary>
        private void NormalizeYAxisForChart(CartesianChart targetChart, ChartRenderModel model, double minHeight)
        {
            var yAxis = targetChart.AxisY[0];
            var syntheticRawData = BuildSyntheticRawData(model);
            var smoothedList = CollectSmoothedValues(model);

            System.Diagnostics.Debug.WriteLine($"[TransformChart] NormalizeYAxisForChart: chart={targetChart.Name}, syntheticRawData={syntheticRawData.Count}, smoothedList={smoothedList.Count}");

            ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);

            System.Diagnostics.Debug.WriteLine($"[TransformChart] After NormalizeYAxis: chart={targetChart.Name}, YMin={yAxis.MinValue}, YMax={yAxis.MaxValue}, ShowLabels={yAxis.ShowLabels}");

            ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
            ChartHelper.InitializeChartTooltip(targetChart);
        }
    }
}
