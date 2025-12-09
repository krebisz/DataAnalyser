using DataVisualiser.Charts.Helpers;
using ChartHelper = DataVisualiser.Charts.Helpers.ChartHelper;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using LiveCharts.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace DataVisualiser.Services
{
    /// <summary>
    /// Builds a Monday->Sunday min/max stacked column chart for a single metric.
    /// Baseline (transparent) = min per day, range column = (max - min) per day.
    /// </summary>
    public class WeeklyDistributionService
    {
        private const double DefaultMinHeight = 400.0;
        private const double YAxisRoundingStep = 5.0;
        private const double YAxisPaddingPercentage = 0.05;
        private const double MinYAxisPadding = 5.0;
        private const double MaxColumnWidth = 40.0;

        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;

        public WeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps)
        {
            _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        }

        /// <summary>
        /// Updates the target chart with a weekly distribution (Mon->Sun) of the specified metric.
        /// </summary>
        /// <param name="targetChart">The chart to render into.</param>
        /// <param name="data">Metric data to analyse.</param>
        /// <param name="displayName">Display name for the metric.</param>
        /// <param name="from">Inclusive start of the date range.</param>
        /// <param name="to">Inclusive end of the date range.</param>
        /// <param name="minHeight">Minimum chart height in pixels.</param>
        public async Task UpdateWeeklyDistributionChartAsync(
            CartesianChart targetChart,
            IEnumerable<HealthMetricData> data,
            string displayName,
            DateTime from,
            DateTime to,
            double minHeight = DefaultMinHeight)
        {
            if (targetChart == null)
            {
                throw new ArgumentNullException(nameof(targetChart));
            }

            if (data == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // Compute using the strategy on a background thread (same pattern as other charts).
            var result = await Task.Run(() =>
            {
                var strategy = new DataVisualiser.Charts.Strategies.WeeklyDistributionStrategy(
                    data,
                    displayName,
                    from,
                    to);

                return strategy.Compute();
            }).ConfigureAwait(true);

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            try
            {
                // Clear previous series
                targetChart.Series.Clear();

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
                var baselineSeries = new StackedColumnSeries
                {
                    Title = $"{displayName} baseline",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    StrokeThickness = 0,
                    MaxColumnWidth = MaxColumnWidth
                };

                // Range series: visible stacked columns placed on top of baseline
                var rangeSeries = new StackedColumnSeries
                {
                    Title = $"{displayName} range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)), // visually consistent light blue
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = MaxColumnWidth
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

                // Configure Y axis based on data range
                ConfigureYAxis(targetChart, mins, ranges);

                // Weekly chart has no per-point timestamps, but we keep the dictionary consistent
                _chartTimestamps[targetChart] = new List<DateTime>();

                // Tooltip + height handling consistent with other charts
                ChartHelper.InitializeChartTooltip(targetChart);
                ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Weekly distribution chart error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show(
                    $"Error updating chart: {ex.Message}\n\nSee debug output for details.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                // Ensure chart is left in a clean state
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
            }
        }

        private static void ConfigureYAxis(
            CartesianChart targetChart,
            IList<double> mins,
            IList<double> ranges)
        {
            if (targetChart.AxisY.Count == 0)
            {
                return;
            }

            var allValues = new List<double>();
            for (int i = 0; i < 7; i++)
            {
                if (!double.IsNaN(mins[i]))
                {
                    allValues.Add(mins[i]);
                }

                if (!double.IsNaN(mins[i]) && !double.IsNaN(ranges[i]))
                {
                    allValues.Add(mins[i] + ranges[i]);
                }
            }

            if (allValues.Count == 0)
            {
                return;
            }

            // Round to nearest YAxisRoundingStep and apply padding
            var min = Math.Floor(allValues.Min() / YAxisRoundingStep) * YAxisRoundingStep;
            var max = Math.Ceiling(allValues.Max() / YAxisRoundingStep) * YAxisRoundingStep;

            var rawRange = max - min;
            var pad = Math.Max(MinYAxisPadding, rawRange * YAxisPaddingPercentage);
            var yMin = Math.Max(0, min - pad);
            var yMax = max + pad;

            var yAxis = targetChart.AxisY[0];
            yAxis.MinValue = yMin;
            yAxis.MaxValue = yMax;

            // Set a sensible step
            var step = MathHelper.RoundToThreeSignificantDigits((yMax - yMin) / 8.0);
            if (step > 0 && !double.IsNaN(step) && !double.IsInfinity(step))
            {
                yAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
            }

            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
        }
    }
}
