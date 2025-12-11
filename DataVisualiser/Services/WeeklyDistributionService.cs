using DataVisualiser.Charts.Helpers;
using ChartHelper = DataVisualiser.Charts.Helpers.ChartHelper;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using DataVisualiser.Charts.Computation;
using LiveCharts.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;

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
        /// Calculates the optimal number of intervals based on the value range size of the loaded data.
        /// This method is a placeholder for dynamic interval calculation.
        /// TODO: Implement the calculation logic based on value range size criteria.
        /// </summary>
        /// <param name="globalMin">The minimum value across all data</param>
        /// <param name="globalMax">The maximum value across all data</param>
        /// <param name="dataCount">Total number of data points</param>
        /// <returns>The calculated number of intervals. Currently returns default of 25.</returns>
        private static int CalculateOptimalIntervalCount(double globalMin, double globalMax, int dataCount)
        {
            // TODO: Implement dynamic calculation based on value range size
            // Example considerations:
            // - Range size: globalMax - globalMin
            // - Data density: dataCount / range
            // - Minimum intervals: e.g., 10
            // - Maximum intervals: e.g., 50
            // - Optimal granularity based on range size

            // For now, return the default value
            return 25;
        }

        /// <summary>
        /// Updates the weekly distribution chart with the provided data.
        /// </summary>
        /// <param name="targetChart">The chart to update</param>
        /// <param name="data">The health metric data to visualize</param>
        /// <param name="displayName">Display name for the chart</param>
        /// <param name="from">Start date for the data range</param>
        /// <param name="to">End date for the data range</param>
        /// <param name="minHeight">Minimum height for the chart</param>
        /// <param name="useFrequencyShading">Whether to use frequency shading or simple range view</param>
        /// <param name="intervalCount">Number of intervals to divide the value range into. Default is 25. 
        /// TODO: Calculate dynamically based on value range size of loaded data.</param>
        public async Task UpdateWeeklyDistributionChartAsync(
            CartesianChart targetChart,
            IEnumerable<HealthMetricData> data,
            string displayName,
            DateTime from,
            DateTime to,
            double minHeight = DefaultMinHeight,
            bool useFrequencyShading = true,
            int intervalCount = 25)
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
            WeeklyDistributionResult? extendedResult = null;
            var result = await Task.Run(() =>
            {
                var strategy = new DataVisualiser.Charts.Strategies.WeeklyDistributionStrategy(
                    data,
                    displayName,
                    from,
                    to);

                var computeResult = strategy.Compute();
                extendedResult = strategy.ExtendedResult;
                return computeResult;
            }).ConfigureAwait(true);

            if (result == null || extendedResult == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // TODO: Calculate intervalCount dynamically based on value range size
            // Example: Calculate global min/max from data, then call CalculateOptimalIntervalCount
            // var dataList = data.ToList();
            // var globalMin = dataList.Min(d => d.Value);
            // var globalMax = dataList.Max(d => d.Value);
            // intervalCount = CalculateOptimalIntervalCount(globalMin, globalMax, dataList.Count);

            try
            {
                // Clear previous series
                targetChart.Series.Clear();

                // Step 1: Render the original min/max range visualization (baseline + range columns)
                // This was the working implementation before
                RenderOriginalMinMaxChart(targetChart, result, displayName, minHeight, extendedResult, useFrequencyShading, intervalCount);

                // Weekly chart has no per-point timestamps, but we keep the dictionary consistent
                _chartTimestamps[targetChart] = new List<DateTime>();

                // Explicitly disable default LiveCharts tooltip for weekly chart
                // We use our custom tooltip instead
                targetChart.DataTooltip = null;

                // Set up tooltip based on frequency shading mode
                if (useFrequencyShading)
                {
                    // Calculate interval data with percentages for tooltip
                    var tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
                    if (tooltipData != null && tooltipData.Count > 0)
                    {
                        // Create tooltip manager (it will attach itself to the chart via events)
                        // Store it in chart's Tag for potential cleanup later
                        var oldTooltip = targetChart.Tag as WeeklyDistributionTooltip;
                        oldTooltip?.Dispose();

                        var tooltip = new WeeklyDistributionTooltip(targetChart, tooltipData);
                        targetChart.Tag = tooltip;
                    }
                    else
                    {
                        var oldTooltip = targetChart.Tag as WeeklyDistributionTooltip;
                        oldTooltip?.Dispose();
                        targetChart.Tag = null;
                    }
                }
                else
                {
                    // Disable tooltip for simple range chart
                    var oldTooltip = targetChart.Tag as WeeklyDistributionTooltip;
                    oldTooltip?.Dispose();
                    targetChart.Tag = null;
                }

                // Height handling consistent with other charts
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
            // Ensure Y-axis exists
            if (targetChart.AxisY.Count == 0)
            {
                targetChart.AxisY.Add(new LiveCharts.Wpf.Axis());
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
                // Set default range if no values
                var defaultYAxis = targetChart.AxisY[0];
                defaultYAxis.MinValue = 0;
                defaultYAxis.MaxValue = 100;
                defaultYAxis.ShowLabels = true;
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
            yAxis.ShowLabels = true; // Re-enable labels when rendering data
            yAxis.Title = "Value"; // Ensure title is set
        }

        /// <summary>
        /// Step 1: Render the original working min/max range chart.
        /// This shows baseline (min) + range (max-min) as stacked columns per day.
        /// </summary>
        private void RenderOriginalMinMaxChart(
            CartesianChart targetChart,
            ChartComputationResult result,
            string displayName,
            double minHeight,
            WeeklyDistributionResult? frequencyData,
            bool useFrequencyShading = true,
            int intervalCount = 25)
        {
            // PrimaryRawValues = mins; PrimarySmoothed = ranges (max - min)
            var mins = result.PrimaryRawValues;
            var ranges = result.PrimarySmoothed;

            // If mins or ranges are missing or lengths differ, bail gracefully
            if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // Get the actual data values per day from the strategy for frequency counting
            // We need the raw values, not just min/max
            var dayValues = GetDayValuesFromStrategy(frequencyData);

            // Step 2: Create uniform partitions/intervals for the y-axis (20-30 intervals)
            double globalMin = mins.Where(m => !double.IsNaN(m)).DefaultIfEmpty(0).Min();
            double globalMax = mins.Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r)
                                  .Where(v => !double.IsNaN(v)).DefaultIfEmpty(1).Max();

            if (globalMax <= globalMin)
            {
                globalMax = globalMin + 1;
            }

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Data Summary ===");
            System.Diagnostics.Debug.WriteLine($"Global Min: {globalMin:F4}, Global Max: {globalMax:F4}, Range: {globalMax - globalMin:F4}");
            System.Diagnostics.Debug.WriteLine($"Day Min/Max values:");
            for (int i = 0; i < 7; i++)
            {
                double dayMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
                double dayMax = dayMin + (double.IsNaN(ranges[i]) ? 0.0 : ranges[i]);
                System.Diagnostics.Debug.WriteLine($"  Day {i}: Min={dayMin:F4}, Max={dayMax:F4}, Range={ranges[i]:F4}");
            }

            // Log sample raw values for first day with data
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                if (dayValues.TryGetValue(dayIndex, out var values) && values.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Day {dayIndex} raw values (first 10): {string.Join(", ", values.Take(10).Select(v => v.ToString("F4")))}");
                    System.Diagnostics.Debug.WriteLine($"Day {dayIndex} total value count: {values.Count}");
                    break; // Only log first day with data
                }
            }

            // Step 2-4: Only compute intervals, frequencies, and color maps if frequency shading is enabled
            List<(double Min, double Max)> intervals = new();
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay = new();
            Dictionary<int, Dictionary<int, Color>> colorMap = new();

            if (useFrequencyShading)
            {
                // Step 2: Create uniform intervals based on parameterized interval count
                intervals = CreateUniformIntervals(globalMin, globalMax, intervalCount);

                // Step 3: Count frequencies per interval per day
                frequenciesPerDay = CountFrequenciesPerInterval(dayValues, intervals);

                // Step 4: Map frequencies to color shades
                colorMap = MapFrequenciesToColors(frequenciesPerDay);
            }

            // Step 1: Render original baseline + range columns with frequency-based shading
            // We need a global baseline series to start the stacking from globalMin
            // Each interval will then be positioned independently at its Y value (interval.Min)
            var baselineSeries = new StackedColumnSeries
            {
                Title = $"{displayName} baseline",
                Values = new LiveCharts.ChartValues<double>(),
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                StrokeThickness = 0,
                MaxColumnWidth = MaxColumnWidth
            };

            // Range series: will be replaced by frequency-shaded intervals
            // We keep it temporarily for fallback, but it will be removed in ApplyFrequencyShading
            var rangeSeries = new StackedColumnSeries
            {
                Title = $"{displayName} range",
                Values = new LiveCharts.ChartValues<double>(),
                Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)), // Default light blue
                Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                StrokeThickness = 1,
                MaxColumnWidth = MaxColumnWidth
            };

            // Populate values Monday -> Sunday
            // For simple range chart: baseline is at each day's min, range is (max - min)
            // For frequency shading: baseline is at globalMin (will be replaced by interval baselines)
            for (int i = 0; i < 7; i++)
            {
                var rangeVal = ranges[i];
                // Replace NaN with 0 so columns show nothing for empty days
                if (double.IsNaN(rangeVal) || rangeVal < 0) rangeVal = 0.0;

                if (useFrequencyShading)
                {
                    // For frequency shading, baseline is at globalMin (intervals will position themselves)
                    baselineSeries.Values.Add(globalMin);
                }
                else
                {
                    // For simple range chart, baseline is at each day's min value
                    var dayMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
                    baselineSeries.Values.Add(dayMin);
                }

                rangeSeries.Values.Add(rangeVal);
            }

            // Add baseline first, then range (stacked)
            targetChart.Series.Add(baselineSeries);
            targetChart.Series.Add(rangeSeries);

            // Step 5: Apply frequency-based shading to the range columns (if enabled)
            // This replaces the simple range series with frequency-shaded interval segments
            // Each interval has uniform height, and zero-frequency intervals are shaded white
            System.Diagnostics.Debug.WriteLine($"WeeklyDistribution: useFrequencyShading={useFrequencyShading}, Series count before ApplyFrequencyShading: {targetChart.Series.Count}");

            if (useFrequencyShading)
            {
                System.Diagnostics.Debug.WriteLine("WeeklyDistribution: Applying frequency shading...");
                ApplyFrequencyShading(targetChart, mins, ranges, intervals, frequenciesPerDay, colorMap, globalMin, globalMax);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("WeeklyDistribution: Frequency shading disabled - keeping simple range series");
                // Ensure the range series is visible and not removed
                // The baseline and range series should already be added above
            }

            System.Diagnostics.Debug.WriteLine($"WeeklyDistribution: Final series count: {targetChart.Series.Count}");

            // Configure Y axis based on data range
            ConfigureYAxis(targetChart, mins, ranges);

            // Configure X axis (days of week) - ensure labels are set
            if (targetChart.AxisX.Count == 0)
            {
                targetChart.AxisX.Add(new LiveCharts.Wpf.Axis());
            }

            var xAxis = targetChart.AxisX[0];
            xAxis.Labels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            xAxis.Title = "Day of Week";
            xAxis.ShowLabels = true;
            xAxis.Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false };

            // Hide legend (user doesn't want keys/legend to the right)
            targetChart.LegendLocation = LiveCharts.LegendLocation.None;
        }

        /// <summary>
        /// Gets day values from frequency data. Returns empty lists if not available.
        /// </summary>
        private Dictionary<int, List<double>> GetDayValuesFromStrategy(WeeklyDistributionResult? frequencyData)
        {
            var dayValues = new Dictionary<int, List<double>>();
            for (int i = 0; i < 7; i++)
            {
                dayValues[i] = frequencyData?.DayValues?.TryGetValue(i, out var values) == true
                    ? values
                    : new List<double>();
            }
            return dayValues;
        }

        /// <summary>
        /// Step 2: Create uniform partitions/intervals for the y-axis (20-30 intervals).
        /// These intervals are shared across all days and represent the stratification of the y-axis.
        /// </summary>
        private List<(double Min, double Max)> CreateUniformIntervals(double globalMin, double globalMax, int intervalCount)
        {
            var intervals = new List<(double Min, double Max)>();

            if (globalMax <= globalMin || intervalCount <= 0)
            {
                // Return a single interval if invalid input
                intervals.Add((globalMin, globalMax));
                return intervals;
            }

            double intervalSize = (globalMax - globalMin) / intervalCount;

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Creating Intervals ===");
            System.Diagnostics.Debug.WriteLine($"Interval Count: {intervalCount}, Interval Size: {intervalSize:F6}");

            for (int i = 0; i < intervalCount; i++)
            {
                double min = globalMin + i * intervalSize;
                // Last interval includes the max value to ensure we cover the full range
                double max = (i == intervalCount - 1) ? globalMax : min + intervalSize;
                intervals.Add((min, max));

                // Log first 3, last 3, and every 5th interval
                if (i < 3 || i >= intervalCount - 3 || i % 5 == 0)
                {
                    string intervalType = i == intervalCount - 1 ? "[inclusive]" : "[half-open)";
                    System.Diagnostics.Debug.WriteLine($"  Interval {i}: [{min:F6}, {max:F6}{intervalType}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"Total intervals created: {intervals.Count}");

            return intervals;
        }

        /// <summary>
        /// Step 3: Count the number of values that fall in each interval, for each separate day.
        /// </summary>
        private Dictionary<int, Dictionary<int, int>> CountFrequenciesPerInterval(
            Dictionary<int, List<double>> dayValues,
            List<(double Min, double Max)> intervals)
        {
            var frequenciesPerDay = new Dictionary<int, Dictionary<int, int>>();

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var frequencies = new Dictionary<int, int>();

                // Initialize all intervals to 0
                for (int i = 0; i < intervals.Count; i++)
                {
                    frequencies[i] = 0;
                }

                // Count values in each interval
                if (dayValues.TryGetValue(dayIndex, out var values))
                {
                    foreach (var value in values)
                    {
                        if (double.IsNaN(value) || double.IsInfinity(value))
                            continue;

                        // Find which interval this value belongs to
                        for (int i = 0; i < intervals.Count; i++)
                        {
                            var interval = intervals[i];
                            // Check if value is in [Min, Max) for all intervals except the last, which is [Min, Max]
                            if (i < intervals.Count - 1)
                            {
                                if (value >= interval.Min && value < interval.Max)
                                {
                                    frequencies[i]++;
                                    break;
                                }
                            }
                            else
                            {
                                // Last interval is inclusive on both ends
                                if (value >= interval.Min && value <= interval.Max)
                                {
                                    frequencies[i]++;
                                    break;
                                }
                            }
                        }
                    }
                }

                frequenciesPerDay[dayIndex] = frequencies;

                // Log frequency summary for each day
                int totalValues = frequencies.Values.Sum();
                int nonZeroIntervals = frequencies.Values.Count(f => f > 0);
                int maxFreq = frequencies.Values.DefaultIfEmpty(0).Max();
                System.Diagnostics.Debug.WriteLine($"Day {dayIndex} frequencies: Total values={totalValues}, Non-zero intervals={nonZeroIntervals}, Max frequency={maxFreq}");

                // Log frequencies for first few and last few intervals
                if (frequencies.Count > 0)
                {
                    var sortedIntervals = frequencies.OrderBy(kvp => kvp.Key).ToList();
                    var firstFew = sortedIntervals.Take(3).Select(kvp => $"I{kvp.Key}={kvp.Value}").ToList();
                    var lastFew = sortedIntervals.Skip(Math.Max(0, sortedIntervals.Count - 3)).Select(kvp => $"I{kvp.Key}={kvp.Value}").ToList();
                    System.Diagnostics.Debug.WriteLine($"  First intervals: {string.Join(", ", firstFew)}");
                    System.Diagnostics.Debug.WriteLine($"  Last intervals: {string.Join(", ", lastFew)}");
                }
            }

            return frequenciesPerDay;
        }

        /// <summary>
        /// Step 4: Map frequencies to color shades.
        /// Non-zero frequencies map to light blue (low) to dark blue/near-black (high).
        /// Zero frequencies are handled separately in ApplyFrequencyShading (white only if within day's range).
        /// </summary>
        private Dictionary<int, Dictionary<int, Color>> MapFrequenciesToColors(
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay)
        {
            // Find global maximum frequency across all days and intervals
            int maxFreq = 0;
            foreach (var dayFreqs in frequenciesPerDay.Values)
            {
                foreach (var freq in dayFreqs.Values)
                {
                    if (freq > maxFreq)
                        maxFreq = freq;
                }
            }

            if (maxFreq == 0)
                maxFreq = 1; // Avoid division by zero

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Color Mapping ===");
            System.Diagnostics.Debug.WriteLine($"Global max frequency: {maxFreq}");

            // Map each non-zero frequency to a color (light blue to dark blue/near-black)
            var colorMap = new Dictionary<int, Dictionary<int, Color>>();

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var dayColorMap = new Dictionary<int, Color>();

                if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs))
                {
                    foreach (var kvp in dayFreqs)
                    {
                        int intervalIndex = kvp.Key;
                        int frequency = kvp.Value;

                        // Only map non-zero frequencies to colors
                        // Zero frequencies will be handled in ApplyFrequencyShading (white if within day's range)
                        if (frequency > 0)
                        {
                            // Normalize frequency to [0.0, 1.0]
                            double normalizedFreq = (double)frequency / maxFreq;

                            // Map to color (light blue = low frequency, dark blue/near-black = high frequency)
                            Color color = WeeklyFrequencyRenderer.MapFrequencyToColor(normalizedFreq);
                            dayColorMap[intervalIndex] = color;
                        }
                        // Note: frequency = 0 is not added to dayColorMap here
                        // It will be handled in ApplyFrequencyShading based on whether interval overlaps day's range
                    }
                }

                colorMap[dayIndex] = dayColorMap;
            }

            return colorMap;
        }

        /// <summary>
        /// Step 5: Apply frequency-based shading to the existing range columns.
        /// Breaks down each day's range into intervals and shades each interval according to frequency.
        /// </summary>
        private void ApplyFrequencyShading(
            CartesianChart targetChart,
            List<double> mins,
            List<double> ranges,
            List<(double Min, double Max)> intervals,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            Dictionary<int, Dictionary<int, Color>> colorMap,
            double globalMin,
            double globalMax)
        {
            // Remove the original range series and replace with frequency-shaded segments
            // We'll create stacked column segments for each interval that overlaps with each day's range
            // Note: We keep the baseline series that was added in RenderOriginalMinMaxChart

            var seriesToRemove = targetChart.Series.Where(s => s.Title?.Contains("range") == true).ToList();
            foreach (var series in seriesToRemove)
            {
                targetChart.Series.Remove(series);
            }

            // We no longer use a global baseline series - each interval positions itself independently
            // at its Y value (interval.Min). The baseline check is no longer needed.

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: ApplyFrequencyShading ===");
            System.Diagnostics.Debug.WriteLine($"Intervals count: {intervals.Count}, Frequencies count: {frequenciesPerDay.Count}");

            // If no intervals or frequencies, restore a simple range series to ensure something is visible
            if (intervals.Count == 0 || frequenciesPerDay.Count == 0)
            {
                // Restore simple range series if frequency shading can't be applied
                var simpleRangeSeries = new StackedColumnSeries
                {
                    Title = "range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = MaxColumnWidth
                };

                for (int i = 0; i < 7; i++)
                {
                    var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];
                    simpleRangeSeries.Values.Add(rangeVal);
                }

                targetChart.Series.Add(simpleRangeSeries);
                return;
            }

            // Debug: Check if we have any overlapping intervals
            bool hasAnyOverlaps = false;
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                double dayMin = double.IsNaN(mins[dayIndex]) ? 0.0 : mins[dayIndex];
                double dayMax = dayMin + (double.IsNaN(ranges[dayIndex]) ? 0.0 : ranges[dayIndex]);

                for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
                {
                    var interval = intervals[intervalIndex];
                    if (interval.Min < dayMax && interval.Max > dayMin && ranges[dayIndex] > 0)
                    {
                        hasAnyOverlaps = true;
                        break;
                    }
                }
                if (hasAnyOverlaps) break;
            }

            // If no overlaps at all, restore simple range series
            if (!hasAnyOverlaps)
            {
                var simpleRangeSeries = new StackedColumnSeries
                {
                    Title = "range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = MaxColumnWidth
                };

                for (int i = 0; i < 7; i++)
                {
                    var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];
                    simpleRangeSeries.Values.Add(rangeVal);
                }

                targetChart.Series.Add(simpleRangeSeries);
                return;
            }

            // Track total series created to ensure we have something visible
            int seriesCreated = 0;

            // Calculate uniform interval height - this is the same for ALL intervals across ALL days
            double uniformIntervalHeight = intervals.Count > 0 ? (globalMax - globalMin) / intervals.Count : 1.0;

            System.Diagnostics.Debug.WriteLine($"Uniform interval height: {uniformIntervalHeight:F6}");
            System.Diagnostics.Debug.WriteLine($"Global range: {globalMin:F4} to {globalMax:F4}");

            // Find global max frequency for normalization
            int globalMaxFreq = frequenciesPerDay.Values
                .SelectMany(d => d.Values)
                .DefaultIfEmpty(1)
                .Max();

            System.Diagnostics.Debug.WriteLine($"Global max frequency for normalization: {globalMaxFreq}");

            // Track cumulative stack height per day to position intervals independently
            // Each interval should be positioned at its absolute Y value (interval.Min), not stacked on previous intervals
            var cumulativeStackHeight = new double[7];
            Array.Fill(cumulativeStackHeight, globalMin); // Start at globalMin for all days

            // Debug: Log all intervals that should overlap with Tuesday (dayIndex = 1)
            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Checking intervals for Tuesday (Day 1) ===");
            double tuesdayMin = double.IsNaN(mins[1]) ? 0.0 : mins[1];
            double tuesdayMax = tuesdayMin + (double.IsNaN(ranges[1]) ? 0.0 : ranges[1]);
            System.Diagnostics.Debug.WriteLine($"Tuesday range: [{tuesdayMin:F4}, {tuesdayMax:F4}]");
            int overlappingIntervalsCount = 0;
            for (int i = 0; i < intervals.Count; i++)
            {
                var iv = intervals[i];
                bool overlaps = iv.Min < tuesdayMax && iv.Max > tuesdayMin;
                if (overlaps)
                {
                    overlappingIntervalsCount++;
                    int freq = 0;
                    if (frequenciesPerDay.TryGetValue(1, out var dayFreqs) && dayFreqs.TryGetValue(i, out var f))
                        freq = f;
                    System.Diagnostics.Debug.WriteLine($"  Interval {i} [{iv.Min:F4}, {iv.Max:F4}] overlaps Tuesday, frequency={freq}");
                }
            }
            System.Diagnostics.Debug.WriteLine($"Total intervals overlapping Tuesday: {overlappingIntervalsCount}");

            // For each interval, create series that position the interval at its actual Y value
            // Each interval is rendered independently - it's positioned at interval.Min with height uniformIntervalHeight
            // Since StackedColumnSeries stacks all series, we need to calculate baselines to account for this
            for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var interval = intervals[intervalIndex];
                var intervalBaselineValues = new LiveCharts.ChartValues<double>();
                var intervalHeightValuesWhite = new LiveCharts.ChartValues<double>(); // For zero-frequency days
                var intervalHeightValuesColored = new LiveCharts.ChartValues<double>(); // For non-zero frequency days
                var hasData = false;
                var hasZeroFreqDays = false;
                var hasNonZeroFreqDays = false;

                // Track max frequency for non-zero frequency days
                int maxFreqForInterval = 0;

                for (int dayIndex = 0; dayIndex < 7; dayIndex++)
                {
                    double dayMin = double.IsNaN(mins[dayIndex]) ? 0.0 : mins[dayIndex];
                    double dayMax = dayMin + (double.IsNaN(ranges[dayIndex]) ? 0.0 : ranges[dayIndex]);

                    // Only draw intervals that fall WITHIN the day's actual data range (dayMin to dayMax)
                    // The interval must overlap with the day's range to be drawn
                    // An interval overlaps if: interval.Min < dayMax && interval.Max > dayMin
                    bool intervalOverlapsDayRange = interval.Min < dayMax && interval.Max > dayMin;

                    // Only draw if the interval overlaps AND the day has data
                    if (intervalOverlapsDayRange && ranges[dayIndex] > 0 && !double.IsNaN(ranges[dayIndex]))
                    {
                        // Get frequency for this day/interval
                        int frequency = 0;
                        if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) &&
                            dayFreqs.TryGetValue(intervalIndex, out var freq))
                        {
                            frequency = freq;
                        }

                        // ALWAYS draw intervals that overlap with the day's range
                        // This ensures no gaps in the visualization within the day's value range
                        // White intervals (frequency = 0) will be drawn to fill gaps
                        hasData = true;

                        // Position interval at its actual Y value (interval.Min)
                        // Each interval is rendered independently - it should appear at interval.Min with height uniformIntervalHeight
                        // Since StackedColumnSeries stacks all series, we need to calculate the baseline to position
                        // this interval at interval.Min, accounting for the cumulative stack height so far
                        // The baseline is the difference between where we want to be (interval.Min) and where we currently are (cumulativeStackHeight[dayIndex])
                        double desiredPosition = interval.Min;
                        double currentStackHeight = cumulativeStackHeight[dayIndex];
                        double intervalBaseline = desiredPosition - currentStackHeight;

                        // Ensure baseline is non-negative (interval.Min should be >= current stack height)
                        // If baseline is negative, it means we've already drawn intervals that extend beyond this one
                        // In this case, we still need to draw this interval, but we'll position it at the current stack height
                        if (intervalBaseline < 0)
                        {
                            // This can happen if intervals are not perfectly sequential or if there are gaps
                            // We'll position this interval right after the previous one (at current stack height)
                            intervalBaseline = 0;
                            // Update stack height to include this interval
                            cumulativeStackHeight[dayIndex] = currentStackHeight + uniformIntervalHeight;
                        }
                        else
                        {
                            // Update cumulative stack height for this day after this interval is rendered
                            // The interval will be positioned at currentStackHeight + intervalBaseline = interval.Min
                            // And will have height uniformIntervalHeight, so the new cumulative stack height is:
                            // currentStackHeight + intervalBaseline + uniformIntervalHeight = interval.Min + uniformIntervalHeight
                            cumulativeStackHeight[dayIndex] = interval.Min + uniformIntervalHeight;
                        }

                        intervalBaselineValues.Add(intervalBaseline);

                        // Separate zero-frequency intervals (white) from non-zero frequency intervals (colored)
                        if (frequency == 0)
                        {
                            // Zero frequency = white for this day (only if within day's range)
                            intervalHeightValuesWhite.Add(uniformIntervalHeight);
                            intervalHeightValuesColored.Add(0.0);
                            hasZeroFreqDays = true;
                        }
                        else
                        {
                            // Non-zero frequency = colored for this day
                            intervalHeightValuesWhite.Add(0.0);
                            intervalHeightValuesColored.Add(uniformIntervalHeight);
                            hasNonZeroFreqDays = true;
                            if (frequency > maxFreqForInterval)
                                maxFreqForInterval = frequency;
                        }

                        // Log detailed info for debugging Tuesday (dayIndex = 1) intervals
                        if (dayIndex == 1 && intervalOverlapsDayRange)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Interval {intervalIndex} [{interval.Min:F4}, {interval.Max:F4}] on Tuesday: Baseline={intervalBaseline:F4}, Height={uniformIntervalHeight:F6}, Frequency={frequency}, Color={(frequency == 0 ? "White" : "Colored")}, Overlaps={intervalOverlapsDayRange}");
                        }
                    }
                    else
                    {
                        // No overlap or no data - add zero height and zero baseline
                        // We still need to track that we processed this interval for this day
                        // so the series arrays stay aligned
                        intervalBaselineValues.Add(0.0);
                        intervalHeightValuesWhite.Add(0.0);
                        intervalHeightValuesColored.Add(0.0);
                        // Don't update cumulative stack height - it stays the same when there's no overlap
                    }
                }

                // Debug output for intervals with data
                if (hasData)
                {
                    // Count how many days have white vs colored for this interval
                    int whiteDayCount = 0;
                    int coloredDayCount = 0;
                    for (int d = 0; d < 7; d++)
                    {
                        if (intervalHeightValuesWhite.Count > d && intervalHeightValuesWhite[d] > 0)
                            whiteDayCount++;
                        if (intervalHeightValuesColored.Count > d && intervalHeightValuesColored[d] > 0)
                            coloredDayCount++;
                    }
                    System.Diagnostics.Debug.WriteLine($"WeeklyDistribution: Interval {intervalIndex} ({interval.Min:F2} to {interval.Max:F2}) has overlapping data (ZeroFreqDays={hasZeroFreqDays}, NonZeroFreqDays={hasNonZeroFreqDays}, WhiteDays={whiteDayCount}, ColoredDays={coloredDayCount})");
                }

                // Only create series if there's data
                // Each interval is rendered independently at its Y value (interval.Min), not stacked
                if (hasData)
                {
                    // For each interval, we create a baseline series that positions it at its absolute Y value
                    // Then we add the interval height on top. Since StackedColumnSeries stacks, we need to ensure
                    // each interval is positioned independently by using its absolute baseline position.

                    // Create baseline series for this interval (transparent, positions the interval at interval.Min)
                    // The baseline is the offset from globalMin to interval.Min, positioning this interval independently
                    var intervalBaselineSeries = new StackedColumnSeries
                    {
                        Title = null,
                        Values = intervalBaselineValues,
                        Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                        StrokeThickness = 0,
                        MaxColumnWidth = MaxColumnWidth,
                        DataLabels = false
                    };

                    // Add baseline first - this positions the interval at its Y value
                    targetChart.Series.Add(intervalBaselineSeries);

                    // Create white series for zero-frequency intervals (if any)
                    // Each interval is independent - white segments appear at their interval's Y position
                    if (hasZeroFreqDays)
                    {
                        // Use pure white fill with a border that matches the white color (light gray for visibility)
                        var whiteBrush = new SolidColorBrush(Colors.White);
                        whiteBrush.Freeze();
                        // Use a light gray border that complements the white fill
                        var whiteStrokeBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                        whiteStrokeBrush.Freeze();

                        var whiteIntervalSeries = new StackedColumnSeries
                        {
                            Title = null,
                            Values = intervalHeightValuesWhite,
                            Fill = whiteBrush,
                            Stroke = whiteStrokeBrush,
                            StrokeThickness = 1.0,
                            MaxColumnWidth = MaxColumnWidth,
                            DataLabels = false
                        };

                        // Add white series - it stacks on the baseline, positioning it at interval.Min
                        targetChart.Series.Add(whiteIntervalSeries);
                        seriesCreated++;

                        // Debug: Log white series creation with detailed info
                        int whiteDayCount = intervalHeightValuesWhite.Count(v => v > 0);
                        System.Diagnostics.Debug.WriteLine($"  Interval {intervalIndex}: Created WHITE series (WhiteDays={whiteDayCount}, TotalDays={intervalHeightValuesWhite.Count})");
                        if (intervalIndex < 5 || (hasZeroFreqDays && !hasNonZeroFreqDays))
                        {
                            System.Diagnostics.Debug.WriteLine($"    White values: [{string.Join(", ", intervalHeightValuesWhite.Select(v => v.ToString("F4")))}]");
                            System.Diagnostics.Debug.WriteLine($"    Baseline values: [{string.Join(", ", intervalBaselineValues.Select(v => v.ToString("F4")))}]");
                            System.Diagnostics.Debug.WriteLine($"    Interval Y position: {interval.Min:F4} to {interval.Max:F4}");
                        }
                    }

                    // Create colored series for non-zero frequency intervals (if any)
                    // Each interval is independent - colored segments appear at their interval's Y position
                    if (hasNonZeroFreqDays)
                    {
                        // Use color mapping based on max frequency (light blue to dark blue)
                        double normalizedFreq = globalMaxFreq > 0 ? (double)maxFreqForInterval / globalMaxFreq : 0.0;
                        Color finalColor = WeeklyFrequencyRenderer.MapFrequencyToColor(normalizedFreq);

                        if (intervalIndex < 3)
                        {
                            System.Diagnostics.Debug.WriteLine($"  Interval {intervalIndex}: MaxFreq={maxFreqForInterval}, Normalized={normalizedFreq:F4}, Color=RGB({finalColor.R},{finalColor.G},{finalColor.B})");
                        }

                        var fillBrush = new SolidColorBrush(finalColor);
                        fillBrush.Freeze();

                        // Create a darker version of the fill color for the border to match the shading
                        // Darken by reducing RGB values by 30% (or to a minimum of 0)
                        byte borderR = (byte)Math.Max(0, finalColor.R - (int)(finalColor.R * 0.3));
                        byte borderG = (byte)Math.Max(0, finalColor.G - (int)(finalColor.G * 0.3));
                        byte borderB = (byte)Math.Max(0, finalColor.B - (int)(finalColor.B * 0.3));
                        var strokeBrush = new SolidColorBrush(Color.FromRgb(borderR, borderG, borderB));
                        strokeBrush.Freeze();

                        var coloredIntervalSeries = new StackedColumnSeries
                        {
                            Title = null,
                            Values = intervalHeightValuesColored,
                            Fill = fillBrush,
                            Stroke = strokeBrush,
                            StrokeThickness = 1.0, // Match white interval border thickness for consistency
                            MaxColumnWidth = MaxColumnWidth,
                            DataLabels = false
                        };

                        // Add colored series - it stacks on the baseline, positioning it at interval.Min
                        targetChart.Series.Add(coloredIntervalSeries);
                        seriesCreated++;
                    }
                }
            }

            // Safety check: If no interval series were created, restore simple range series
            if (seriesCreated == 0)
            {
                // Restore simple range series if frequency shading failed
                var simpleRangeSeries = new StackedColumnSeries
                {
                    Title = "range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = MaxColumnWidth
                };

                for (int i = 0; i < 7; i++)
                {
                    var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];
                    simpleRangeSeries.Values.Add(rangeVal);
                }

                targetChart.Series.Add(simpleRangeSeries);
                System.Diagnostics.Debug.WriteLine("WeeklyDistribution: No interval series created, using fallback simple range series");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"=== WeeklyDistribution: Rendering Complete ===");
            System.Diagnostics.Debug.WriteLine($"Total interval series created: {seriesCreated}");
            System.Diagnostics.Debug.WriteLine($"Total series in chart: {targetChart.Series.Count}");

            // Log series summary
            var seriesTypes = targetChart.Series.GroupBy(s => s.GetType().Name).Select(g => $"{g.Key}:{g.Count()}").ToList();
            System.Diagnostics.Debug.WriteLine($"Series breakdown: {string.Join(", ", seriesTypes)}");
        }

        /// <summary>
        /// Calculates tooltip data with interval breakdown, percentages, and counts for each day.
        /// </summary>
        private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateTooltipData(
            ChartComputationResult result,
            WeeklyDistributionResult? frequencyData,
            int intervalCount = 25)
        {
            var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

            if (result == null || frequencyData == null)
                return tooltipData;

            var mins = result.PrimaryRawValues;
            var ranges = result.PrimarySmoothed;

            if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
                return tooltipData;

            // Get day values
            var dayValues = GetDayValuesFromStrategy(frequencyData);

            // Calculate global min/max
            double globalMin = mins.Where(m => !double.IsNaN(m)).DefaultIfEmpty(0).Min();
            double globalMax = mins.Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r)
                                  .Where(v => !double.IsNaN(v)).DefaultIfEmpty(1).Max();

            if (globalMax <= globalMin)
                globalMax = globalMin + 1;

            // Create intervals (same as in RenderOriginalMinMaxChart)
            var intervals = CreateUniformIntervals(globalMin, globalMax, intervalCount);

            // Count frequencies per interval per day
            var frequenciesPerDay = CountFrequenciesPerInterval(dayValues, intervals);

            // Calculate percentages for each day
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var dayIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

                // Get total count for this day
                int totalCount = 0;
                if (dayValues.TryGetValue(dayIndex, out var values))
                {
                    totalCount = values.Count;
                }

                // Calculate day min/max to determine which intervals are within the day's range
                double dayMin = double.IsNaN(mins[dayIndex]) ? 0.0 : mins[dayIndex];
                double dayMax = dayMin + (double.IsNaN(ranges[dayIndex]) ? 0.0 : ranges[dayIndex]);

                // For each interval, calculate count and percentage
                for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
                {
                    var interval = intervals[intervalIndex];

                    // Check if interval overlaps with day's range
                    bool intervalOverlapsDayRange = interval.Min < dayMax && interval.Max > dayMin;

                    if (intervalOverlapsDayRange && ranges[dayIndex] > 0 && !double.IsNaN(ranges[dayIndex]))
                    {
                        // Get frequency for this interval
                        int count = 0;
                        if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) &&
                            dayFreqs.TryGetValue(intervalIndex, out var freq))
                        {
                            count = freq;
                        }

                        // Calculate percentage (percentage of total values for this day)
                        double percentage = totalCount > 0 ? (double)count / totalCount * 100.0 : 0.0;

                        dayIntervals.Add((interval.Min, interval.Max, count, percentage));
                    }
                }

                tooltipData[dayIndex] = dayIntervals;
            }

            return tooltipData;
        }

        /// <summary>
        /// Creates a brush with intensity based on normalized frequency.
        /// Higher frequency = darker color (closer to black).
        /// Uses a blue color ramp from light blue to near-black.
        /// </summary>
        private SolidColorBrush CreateFrequencyBrush(double normalizedFrequency)
        {
            // Clamp to [0.0, 1.0]
            normalizedFrequency = Math.Max(0.0, Math.Min(1.0, normalizedFrequency));

            // Start color: light blue (when frequency = 0)
            byte r0 = 173, g0 = 216, b0 = 230;

            // End color: near-black/dark blue (when frequency = 1.0)
            byte r1 = 8, g1 = 10, b1 = 25;

            // Interpolate based on normalized frequency
            byte r = (byte)Math.Round(r0 + (r1 - r0) * normalizedFrequency);
            byte g = (byte)Math.Round(g0 + (g1 - g0) * normalizedFrequency);
            byte b = (byte)Math.Round(b0 + (b1 - b0) * normalizedFrequency);

            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }
    }
}
