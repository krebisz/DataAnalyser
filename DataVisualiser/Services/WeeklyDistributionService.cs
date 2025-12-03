using DataVisualiser.Class;
using DataVisualiser.Helper;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Media;

namespace DataVisualiser.Services
{
    public class WeeklyDistributionService
    {
        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;

        public WeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
        {
            _chartTimestamps = chartTimestamps;
        }

        /// <summary>
        /// Builds a Monday->Sunday min-max stacked column visualization:
        ///  - baseline (transparent) = min per day
        ///  - range column (blue) = max - min per day (stacked)
        /// </summary>
        public async Task UpdateWeeklyDistributionChartAsync(
            CartesianChart targetChart,
            IEnumerable<HealthMetricData> data,
            string displayName,
            DateTime from,
            DateTime to,
            double minHeight = 400.0)
        {
            if (data == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // compute using strategy inside Task.Run to avoid blocking UI (consistent with your pattern)
            var computeTask = Task.Run(() =>
            {
                var strat = new DataVisualiser.Charts.Strategies.WeeklyDistributionStrategy(data, displayName, from, to);
                var res = strat.Compute();
                return res;
            });

            var result = await computeTask;

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            try
            {
                // Clear previous series
                targetChart.Series.Clear();

                // Monday->Sunday names already set in XAML labels, but we'll keep an ordered array if needed
                var dayLabels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                // PrimaryRawValues = mins; PrimarySmoothed = ranges (max - min)
                var mins = result.PrimaryRawValues;
                var ranges = result.PrimarySmoothed;

                // If mins or ranges are missing or lengths differ, bail gracefully
                if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
                {
                    ChartHelper.ClearChart(targetChart, _chartTimestamps);
                    return;
                }

                // Baseline series: invisible columns used to set the bottom of each stacked column
                var baselineSeries = new LiveCharts.Wpf.StackedColumnSeries
                {
                    Title = $"{displayName} baseline",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    StrokeThickness = 0,
                    MaxColumnWidth = 40
                };

                // Range series: visible blue stacked columns placed on top of baseline
                var rangeSeries = new LiveCharts.Wpf.StackedColumnSeries
                {
                    Title = $"{displayName} range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)), // light blue
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = 40
                };

                // Populate values Monday -> Sunday
                for (int i = 0; i < 7; i++)
                {
                    var minVal = mins[i];
                    var rangeVal = ranges[i];

                    // Replace NaN with 0 so columns show nothing for empty days
                    if (double.IsNaN(minVal)) minVal = 0.0;
                    if (double.IsNaN(rangeVal) || rangeVal < 0) rangeVal = 0.0;

                    baselineSeries.Values.Add(minVal);
                    rangeSeries.Values.Add(rangeVal);
                }

                // Add baseline first, then range (stacked)
                targetChart.Series.Add(baselineSeries);
                targetChart.Series.Add(rangeSeries);

                //// --- ensure categorical alignment for Monday(0)..Sunday(6) ---
                //if (targetChart.AxisX.Count > 0)
                //{
                //    var xAxis = targetChart.AxisX[0];

                //    // enforce labels and ordering (defensive)
                //    xAxis.Labels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                //    // lock tick step to 1 so integer ticks align with columns
                //    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = 1 };

                //    // Give the axis half-unit padding so integer-indexed columns (0..6) are centered on ticks.
                //    // This ensures stable alignment while zooming/panning.
                //    xAxis.MinValue = -0.5;
                //    xAxis.MaxValue = 6.5;

                //    // Optional: keep labels visible and let the chart show them
                //    xAxis.ShowLabels = true;
                //}

                // Configure axes: Y axis floor/ceiling from underlying raw data
                var allValues = new List<double>();
                for (int i = 0; i < 7; i++)
                {
                    if (!double.IsNaN(mins[i])) allValues.Add(mins[i]);
                    if (!double.IsNaN(mins[i]) && !double.IsNaN(ranges[i])) allValues.Add(mins[i] + ranges[i]);
                }

                if (allValues.Count > 0)
                {
                    var min = Math.Floor(allValues.Min() / 5.0) * 5.0; // round down to nearest 5
                    var max = Math.Ceiling(allValues.Max() / 5.0) * 5.0; // round up to nearest 5

                    // small padding
                    var pad = Math.Max(5, (max - min) * 0.05);
                    var yMin = Math.Max(0, min - pad);
                    var yMax = max + pad;

                    if (targetChart.AxisY.Count > 0)
                    {
                        var yAxis = targetChart.AxisY[0];
                        yAxis.MinValue = yMin;
                        yAxis.MaxValue = yMax;

                        // Set a sensible step
                        var step = MathHelper.RoundToThreeSignificantDigits((yMax - yMin) / 8.0);
                        if (step > 0 && !double.IsNaN(step) && !double.IsInfinity(step))
                            yAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };

                        yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                    }
                }

                // Keep consistent UI patterns (tooltip, timestamps)
                _chartTimestamps[targetChart] = new List<DateTime>(); // not used for categorical chart

                ChartHelper.InitializeChartTooltip(targetChart);

                ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weekly distribution chart error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating weekly distribution chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
