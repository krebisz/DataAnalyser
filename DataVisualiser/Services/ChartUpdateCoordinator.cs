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
            var model = new ChartRenderModel
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
                IsOperationChart = isOperationChart
            };

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
                    var yAxis = targetChart.AxisY[0];

                    var syntheticRawData = new List<HealthMetricData>();
                    var timestamps = model.Timestamps;

                    // Use both primary and secondary raw values (if present) so the Y-axis
                    // reflects the full visible range across all series.
                    var primaryRaw = model.PrimaryRaw ?? new List<double>();
                    var secondaryRaw = model.SecondaryRaw;

                    for (int i = 0; i < timestamps.Count && i < primaryRaw.Count; i++)
                    {
                        var val = primaryRaw[i];
                        syntheticRawData.Add(new HealthMetricData
                        {
                            NormalizedTimestamp = timestamps[i],
                            Value = double.IsNaN(val) ? (decimal?)null : (decimal?)val,
                            Unit = model.Unit
                        });

                        if (secondaryRaw != null && i < secondaryRaw.Count)
                        {
                            var secVal = secondaryRaw[i];
                            syntheticRawData.Add(new HealthMetricData
                            {
                                NormalizedTimestamp = timestamps[i],
                                Value = double.IsNaN(secVal) ? (decimal?)null : (decimal?)secVal,
                                Unit = model.Unit
                            });
                        }
                    }

                    var smoothedList = new List<double>();
                    if (model.PrimarySmoothed != null)
                        smoothedList.AddRange(model.PrimarySmoothed);
                    if (model.SecondarySmoothed != null)
                        smoothedList.AddRange(model.SecondarySmoothed);

                    ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);
                    ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
                    ChartHelper.InitializeChartTooltip(targetChart);
                }
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
    }
}
