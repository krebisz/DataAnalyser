using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Media;

namespace DataVisualiser.Services
{
    public class ChartUpdateCoordinator
    {
        private readonly ChartComputationEngine _chartComputationEngine;
        private readonly ChartRenderEngine _chartRenderEngine;
        private readonly ChartTooltipManager _tooltipManager;

        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;

        public ChartUpdateCoordinator(
            ChartComputationEngine computationEngine,
            ChartRenderEngine renderEngine,
            ChartTooltipManager tooltipManager,
            Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
        {
            _chartComputationEngine = computationEngine;
            _chartRenderEngine = renderEngine;
            _tooltipManager = tooltipManager;
            _chartTimestamps = chartTimestamps;
        }

        public async Task UpdateChartUsingStrategyAsync(
            CartesianChart targetChart,
            IChartComputationStrategy strategy,
            string primaryLabel,
            string? secondaryLabel = null,
            double minHeight = 400.0)
        {
            var result = await _chartComputationEngine.ComputeAsync(strategy);

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            var model = new ChartRenderModel
            {
                PrimarySeriesName = strategy.PrimaryLabel ?? primaryLabel,
                SecondarySeriesName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                PrimaryRaw = result.PrimaryRawValues,
                PrimarySmoothed = result.PrimarySmoothed,
                SecondaryRaw = result.SecondaryRawValues,
                SecondarySmoothed = result.SecondarySmoothed,
                PrimaryColor = ColourPalette.Next(targetChart),
                SecondaryColor = result.SecondaryRawValues != null && result.SecondarySmoothed != null
                    ? ColourPalette.Next(targetChart)
                    : Colors.Red,
                Unit = result.Unit,
                Timestamps = result.Timestamps,
                IntervalIndices = result.IntervalIndices,
                NormalizedIntervals = result.NormalizedIntervals,
                TickInterval = result.TickInterval
            };

            try
            {
                _chartRenderEngine.Render(targetChart, model, minHeight);
                _chartTimestamps[targetChart] = model.Timestamps;

                // Update tooltip manager with new timestamps
                _tooltipManager?.UpdateChartTimestamps(targetChart, model.Timestamps);

                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    var syntheticRawData = new List<HealthMetricData>();
                    var timestamps = model.Timestamps;
                    var primaryRaw = model.PrimaryRaw;

                    for (int i = 0; i < timestamps.Count; i++)
                    {
                        var val = primaryRaw[i];
                        syntheticRawData.Add(new HealthMetricData
                        {
                            NormalizedTimestamp = timestamps[i],
                            Value = double.IsNaN(val) ? (decimal?)null : (decimal?)val,
                            Unit = model.Unit
                        });
                    }

                    var smoothedList = model.PrimarySmoothed.ToList();
                    if (model.SecondarySmoothed != null)
                        smoothedList.AddRange(model.SecondarySmoothed);

                    ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);
                    ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
                    ChartHelper.InitializeChartTooltip(targetChart);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart update/render error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
