using DataFileReader.Canonical;
using DataFileReader.Normalization.Canonical;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Helper;
using DataVisualiser.Models;
using DataVisualiser.Services.Shading;
using LiveCharts;
using LiveCharts.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using ChartHelper = DataVisualiser.Charts.Helpers.ChartHelper;

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

        private const bool UseCmsWeeklyDistribution = false;

        private IFrequencyShadingRenderer? _frequencyRenderer;
        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
        private IIntervalShadingStrategy _shadingStrategy;

        public WeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IIntervalShadingStrategy? shadingStrategy = null)
        {
            _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
            _shadingStrategy = shadingStrategy ?? new FrequencyBasedShadingStrategy();
            _frequencyRenderer = new FrequencyShadingRenderer(MaxColumnWidth);
        }

        /// <summary>
        /// Sets the shading strategy to use for interval coloring.
        /// </summary>
        public void SetShadingStrategy(IIntervalShadingStrategy strategy)
        {
            _shadingStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
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
        /// <param name="intervalCount">Number of intervals to divide the value range into. Default is 10.</param>
        public async Task UpdateWeeklyDistributionChartAsync(
            CartesianChart targetChart,
            IEnumerable<HealthMetricData> data,
            string displayName,
            DateTime from,
            DateTime to,
            double minHeight = DefaultMinHeight,
            bool useFrequencyShading = true,
            int intervalCount = 10,
            DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries = null,
            bool enableParity = false)
        {
            if (targetChart == null)
                throw new ArgumentNullException(nameof(targetChart));

            if (data == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // Toggle: CMS path only when cmsSeries provided
            var useCmsStrategy = (cmsSeries != null);

            var tuple = await ComputeWeeklyDistributionAsync(
                data,
                cmsSeries,
                displayName,
                from,
                to,
                useCmsStrategy: useCmsStrategy,
                enableParity: enableParity);

            var result = tuple.Result;
            var extendedResult = tuple.ExtendedResult;

            if (result == null || extendedResult == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            try
            {
                targetChart.Series.Clear();

                RenderOriginalMinMaxChart(
                    targetChart,
                    result,
                    displayName,
                    minHeight,
                    extendedResult,
                    useFrequencyShading,
                    intervalCount);

                _chartTimestamps[targetChart] = new List<DateTime>();
                targetChart.DataTooltip = null;

                SetupWeeklyTooltip(targetChart, result, extendedResult, useFrequencyShading, intervalCount);

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
            if (!TryExtractMinMax(result, targetChart, out var mins, out var ranges))
                return;

            var dayValues = GetDayValuesFromStrategy(frequencyData);

            var (globalMin, globalMax) = CalculateGlobalMinMax(mins, ranges);

            LogWeeklySummary(mins, ranges, dayValues, globalMin, globalMax);

            var shadingData = useFrequencyShading
                ? BuildFrequencyShadingData(dayValues, globalMin, globalMax, intervalCount)
                : FrequencyShadingData.Empty;

            AddBaselineAndRangeSeries(
                targetChart,
                mins,
                ranges,
                globalMin,
                displayName,
                useFrequencyShading);

            if (useFrequencyShading)
            {
                ApplyFrequencyShadingViaRenderer(
                    targetChart,
                    mins,
                    ranges,
                    shadingData,
                    globalMin,
                    globalMax);
            }

            ConfigureYAxis(targetChart, mins, ranges);
            ConfigureXAxis(targetChart);
            targetChart.LegendLocation = LiveCharts.LegendLocation.None;
        }

        private sealed record FrequencyShadingData(
    List<(double Min, double Max)> Intervals,
    Dictionary<int, Dictionary<int, int>> FrequenciesPerDay,
    Dictionary<int, Dictionary<int, Color>> ColorMap,
    Dictionary<int, List<double>> DayValues)
        {
            public static readonly FrequencyShadingData Empty =
                new(new(), new(), new(), new());
        }

        private bool TryExtractMinMax(
            ChartComputationResult result,
            CartesianChart chart,
            out List<double> mins,
            out List<double> ranges)
        {
            mins = result.PrimaryRawValues;
            ranges = result.PrimarySmoothed;

            if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
            {
                ChartHelper.ClearChart(chart, _chartTimestamps);
                return false;
            }

            return true;
        }

        private (double Min, double Max) CalculateGlobalMinMax(
    List<double> mins,
    List<double> ranges)
        {
            double min = mins.Where(m => !double.IsNaN(m))
                             .DefaultIfEmpty(0)
                             .Min();

            double max = mins.Zip(ranges, (m, r) =>
                            double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r)
                             .Where(v => !double.IsNaN(v))
                             .DefaultIfEmpty(min + 1)
                             .Max();

            if (max <= min)
                max = min + 1;

            return (min, max);
        }

        private FrequencyShadingData BuildFrequencyShadingData(
    Dictionary<int, List<double>> dayValues,
    double globalMin,
    double globalMax,
    int intervalCount)
        {
            var intervals = CreateUniformIntervals(globalMin, globalMax, intervalCount);
            var frequencies = CountFrequenciesPerInterval(dayValues, intervals);

            var context = new IntervalShadingContext
            {
                Intervals = intervals,
                FrequenciesPerDay = frequencies,
                DayValues = dayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
            };

            var colorMap = _shadingStrategy.CalculateColorMap(context);

            return new FrequencyShadingData(intervals, frequencies, colorMap, dayValues);
        }

        private void AddBaselineAndRangeSeries(
    CartesianChart chart,
    List<double> mins,
    List<double> ranges,
    double globalMin,
    string displayName,
    bool useFrequencyShading)
        {
            var baseline = CreateBaselineSeries(displayName);
            var range = CreateRangeSeries(displayName);

            for (int i = 0; i < 7; i++)
            {
                double rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0
                    ? 0.0
                    : ranges[i];

                double baselineVal = useFrequencyShading
                    ? globalMin
                    : (double.IsNaN(mins[i]) ? 0.0 : mins[i]);

                baseline.Values.Add(baselineVal);
                range.Values.Add(rangeVal);
            }

            chart.Series.Add(baseline);
            chart.Series.Add(range);
        }

        private StackedColumnSeries CreateBaselineSeries(string displayName)
        {
            return new StackedColumnSeries
            {
                Title = $"{displayName} baseline",
                Values = new ChartValues<double>(),
                Fill = Brushes.Transparent,
                StrokeThickness = 0,
                MaxColumnWidth = MaxColumnWidth
            };
        }

        private StackedColumnSeries CreateRangeSeries(string displayName)
        {
            return new StackedColumnSeries
            {
                Title = $"{displayName} range",
                Values = new ChartValues<double>(),
                Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                StrokeThickness = 1,
                MaxColumnWidth = MaxColumnWidth
            };
        }

        private void ApplyFrequencyShadingViaRenderer(
    CartesianChart chart,
    List<double> mins,
    List<double> ranges,
    FrequencyShadingData data,
    double globalMin,
    double globalMax)
        {
            var context = new IntervalShadingContext
            {
                Intervals = data.Intervals,
                FrequenciesPerDay = data.FrequenciesPerDay,
                DayValues = data.DayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
            };

            _frequencyRenderer.Render(
                chart,
                mins,
                ranges,
                data.Intervals,
                data.FrequenciesPerDay,
                data.ColorMap,
                globalMin,
                globalMax,
                context);
        }

        private void ConfigureXAxis(CartesianChart chart)
        {
            if (chart.AxisX.Count == 0)
                chart.AxisX.Add(new LiveCharts.Wpf.Axis());

            var axis = chart.AxisX[0];
            axis.Labels = new[]
            {
        "Monday", "Tuesday", "Wednesday",
        "Thursday", "Friday", "Saturday", "Sunday"
    };
            axis.Title = "Day of Week";
            axis.ShowLabels = true;
            axis.Separator = new LiveCharts.Wpf.Separator { Step = 1, IsEnabled = false };
        }

        private void LogWeeklySummary(
    List<double> mins,
    List<double> ranges,
    Dictionary<int, List<double>> dayValues,
    double globalMin,
    double globalMax)
        {
            Debug.WriteLine("=== WeeklyDistribution: Data Summary ===");
            Debug.WriteLine($"Global Min: {globalMin:F4}, Global Max: {globalMax:F4}, Range: {globalMax - globalMin:F4}");
            Debug.WriteLine("Day Min/Max values:");

            for (int i = 0; i < 7; i++)
            {
                double dayMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
                double dayMax = dayMin + (double.IsNaN(ranges[i]) ? 0.0 : ranges[i]);
                Debug.WriteLine($"  Day {i}: Min={dayMin:F4}, Max={dayMax:F4}, Range={ranges[i]:F4}");
            }

            // Log sample raw values for first day with data
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                if (dayValues.TryGetValue(dayIndex, out var values) && values.Count > 0)
                {
                    Debug.WriteLine(
                        $"Day {dayIndex} raw values (first 10): {string.Join(", ", values.Take(10).Select(v => v.ToString("F4")))}");
                    Debug.WriteLine($"Day {dayIndex} total value count: {values.Count}");
                    break;
                }
            }
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

        private void ApplyFrequencyShading(
            CartesianChart targetChart,
            List<double> mins,
            List<double> ranges,
            List<(double Min, double Max)> intervals,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            Dictionary<int, Dictionary<int, Color>> colorMap,
            double globalMin,
            double globalMax,
            IntervalShadingContext shadingContext)
        {
            RemoveExistingRangeSeries(targetChart);

            Debug.WriteLine("=== WeeklyDistribution: ApplyFrequencyShading ===");
            Debug.WriteLine($"Intervals count: {intervals.Count}, Frequencies count: {frequenciesPerDay.Count}");

            // Guard: if no intervals or frequencies, restore simple range series.
            if (!CanApplyFrequencyShading(intervals, frequenciesPerDay, mins, ranges))
            {
                RestoreSimpleRangeSeries(targetChart, ranges);
                return;
            }

            // Uniform interval height across all days.
            double uniformIntervalHeight = CalculateUniformIntervalHeight(globalMin, globalMax, intervals.Count);
            Debug.WriteLine($"Uniform interval height: {uniformIntervalHeight:F6}");
            Debug.WriteLine($"Global range: {globalMin:F4} to {globalMax:F4}");

            // Used only for logging / diagnostics, but keep it for parity with original behavior.
            int globalMaxFreq = CalculateGlobalMaxFrequency(frequenciesPerDay);
            Debug.WriteLine($"Global max frequency for normalization: {globalMaxFreq}");

            // Track cumulative stack height per day (needed because StackedColumnSeries stacks across series).
            var cumulativeStackHeight = InitializeCumulativeStack(globalMin);

            int seriesCreated = RenderIntervals(
                targetChart,
                mins,
                ranges,
                intervals,
                frequenciesPerDay,
                colorMap,
                uniformIntervalHeight,
                cumulativeStackHeight,
                globalMaxFreq);

            // Safety: If nothing was created, restore a visible range series.
            if (seriesCreated == 0)
            {
                RestoreSimpleRangeSeries(targetChart, ranges);
                Debug.WriteLine("WeeklyDistribution: No interval series created, using fallback simple range series");
                return;
            }

            Debug.WriteLine("=== WeeklyDistribution: Rendering Complete ===");
            Debug.WriteLine($"Total interval series created: {seriesCreated}");
            Debug.WriteLine($"Total series in chart: {targetChart.Series.Count}");

            var seriesTypes = targetChart.Series
                .GroupBy(s => s.GetType().Name)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            Debug.WriteLine($"Series breakdown: {string.Join(", ", seriesTypes)}");
        }

        #region Core pipeline

        private int RenderIntervals(
            CartesianChart chart,
            List<double> mins,
            List<double> ranges,
            List<(double Min, double Max)> intervals,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            Dictionary<int, Dictionary<int, Color>> colorMap,
            double uniformIntervalHeight,
            double[] cumulativeStackHeight,
            int globalMaxFreq)
        {
            int seriesCreated = 0;

            for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var interval = intervals[intervalIndex];

                var state = BuildIntervalState(
                    intervalIndex,
                    interval,
                    mins,
                    ranges,
                    frequenciesPerDay,
                    uniformIntervalHeight,
                    cumulativeStackHeight);

                if (!state.HasData)
                    continue;

                // 1) Baseline series (transparent) to position this interval at its absolute Y value.
                AddBaselineSeries(chart, state.Baselines);

                // 2) White fill for zero-frequency segments inside the day's value range.
                if (state.HasZeroFreqDays)
                {
                    AddWhiteSeries(chart, state.WhiteHeights);
                    seriesCreated++;
                }

                // 3) Colored fill for non-zero frequency segments.
                if (state.HasNonZeroFreqDays)
                {
                    var color = ResolveIntervalColor(frequenciesPerDay, colorMap, intervalIndex);
                    AddColoredSeries(chart, state.ColoredHeights, color);
                    seriesCreated++;
                }
            }

            return seriesCreated;
        }

        #endregion

        #region Interval state builder

        private IntervalRenderState BuildIntervalState(
            int intervalIndex,
            (double Min, double Max) interval,
            List<double> mins,
            List<double> ranges,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            double uniformIntervalHeight,
            double[] cumulativeStackHeight)
        {
            var state = new IntervalRenderState(uniformIntervalHeight);

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                double dayMin = SafeMin(mins, dayIndex);
                double dayRange = SafeRange(ranges, dayIndex);
                double dayMax = dayMin + dayRange;

                bool hasDayData = dayRange > 0 && !double.IsNaN(ranges[dayIndex]);
                bool intervalOverlapsDayRange = interval.Min < dayMax && interval.Max > dayMin;

                if (!hasDayData || !intervalOverlapsDayRange)
                {
                    state.AddEmpty();
                    continue;
                }

                // Frequency lookup for this day/interval.
                int frequency = 0;
                if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) &&
                    dayFreqs.TryGetValue(intervalIndex, out var f))
                {
                    frequency = f;
                }

                // Baseline positioning:
                // We want the *top of the current stack* to move to interval.Min, then add intervalHeight.
                // Since series stack, we store "baseline = desiredPosition - currentStackHeight".
                double desiredPosition = interval.Min;
                double currentStack = cumulativeStackHeight[dayIndex];
                double baseline = desiredPosition - currentStack;

                if (baseline < 0)
                {
                    // If already past desired position, clamp baseline and just stack forward.
                    baseline = 0;
                    cumulativeStackHeight[dayIndex] = currentStack + uniformIntervalHeight;
                }
                else
                {
                    cumulativeStackHeight[dayIndex] = interval.Min + uniformIntervalHeight;
                }

                state.Add(baseline, frequency);

                // Optional targeted debug (kept compact).
                if (dayIndex == 1) // Tuesday
                {
                    Debug.WriteLine(
                        $"  Interval {intervalIndex} [{interval.Min:F4}, {interval.Max:F4}] Tue: Baseline={baseline:F4}, Height={uniformIntervalHeight:F6}, Freq={frequency}");
                }
            }

            return state;
        }

        #endregion

        #region Series creation

        private void AddBaselineSeries(CartesianChart chart, ChartValues<double> baselineValues)
        {
            chart.Series.Add(new StackedColumnSeries
            {
                Title = null,
                Values = baselineValues,
                Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                StrokeThickness = 0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
            });
        }

        private void AddWhiteSeries(CartesianChart chart, ChartValues<double> whiteValues)
        {
            var whiteBrush = new SolidColorBrush(Colors.White);
            whiteBrush.Freeze();

            var strokeBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
            strokeBrush.Freeze();

            chart.Series.Add(new StackedColumnSeries
            {
                Title = null,
                Values = whiteValues,
                Fill = whiteBrush,
                Stroke = strokeBrush,
                StrokeThickness = 1.0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
            });
        }

        private void AddColoredSeries(CartesianChart chart, ChartValues<double> coloredValues, Color fillColor)
        {
            var fillBrush = new SolidColorBrush(fillColor);
            fillBrush.Freeze();

            var strokeBrush = Darken(fillColor);
            strokeBrush.Freeze();

            chart.Series.Add(new StackedColumnSeries
            {
                Title = null,
                Values = coloredValues,
                Fill = fillBrush,
                Stroke = strokeBrush,
                StrokeThickness = 1.0,
                MaxColumnWidth = MaxColumnWidth,
                DataLabels = false
            });
        }

        #endregion

        #region Fallback + guards

        private void RemoveExistingRangeSeries(CartesianChart chart)
        {
            var seriesToRemove = chart.Series.Where(s => s.Title?.Contains("range") == true).ToList();
            foreach (var series in seriesToRemove)
                chart.Series.Remove(series);
        }

        private bool CanApplyFrequencyShading(
            List<(double Min, double Max)> intervals,
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            List<double> mins,
            List<double> ranges)
        {
            if (intervals == null || frequenciesPerDay == null || mins == null || ranges == null)
                return false;

            if (intervals.Count == 0 || frequenciesPerDay.Count == 0)
                return false;

            return HasAnyIntervalOverlap(intervals, mins, ranges);
        }

        private bool HasAnyIntervalOverlap(
            List<(double Min, double Max)> intervals,
            List<double> mins,
            List<double> ranges)
        {
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                double dayMin = SafeMin(mins, dayIndex);
                double dayMax = dayMin + SafeRange(ranges, dayIndex);

                for (int intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
                {
                    var iv = intervals[intervalIndex];
                    if (iv.Min < dayMax && iv.Max > dayMin && ranges[dayIndex] > 0)
                        return true;
                }
            }

            return false;
        }

        private void RestoreSimpleRangeSeries(CartesianChart chart, List<double> ranges)
        {
            var simpleRangeSeries = new StackedColumnSeries
            {
                Title = "range",
                Values = new ChartValues<double>(),
                Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)),
                Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                StrokeThickness = 1,
                MaxColumnWidth = MaxColumnWidth
            };

            for (int i = 0; i < 7; i++)
                simpleRangeSeries.Values.Add(SafeRange(ranges, i));

            chart.Series.Add(simpleRangeSeries);
        }

        #endregion

        #region Small utilities

        private double CalculateUniformIntervalHeight(double globalMin, double globalMax, int intervalCount)
        {
            return intervalCount > 0 ? (globalMax - globalMin) / intervalCount : 1.0;
        }

        private int CalculateGlobalMaxFrequency(Dictionary<int, Dictionary<int, int>> frequenciesPerDay)
        {
            return frequenciesPerDay.Values
                .SelectMany(d => d.Values)
                .DefaultIfEmpty(1)
                .Max();
        }

        private double[] InitializeCumulativeStack(double globalMin)
        {
            var cumulative = new double[7];
            Array.Fill(cumulative, globalMin);
            return cumulative;
        }

        private double SafeMin(List<double> mins, int index)
        {
            var v = mins[index];
            return double.IsNaN(v) ? 0.0 : v;
        }

        private double SafeRange(List<double> ranges, int index)
        {
            var v = ranges[index];
            return (double.IsNaN(v) || v < 0) ? 0.0 : v;
        }

        private Color ResolveIntervalColor(
            Dictionary<int, Dictionary<int, int>> frequenciesPerDay,
            Dictionary<int, Dictionary<int, Color>> colorMap,
            int intervalIndex)
        {
            // Pick the day with the highest frequency for this interval (most representative color).
            int bestDay = -1;
            int bestFreq = 0;

            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                if (frequenciesPerDay.TryGetValue(dayIndex, out var dayFreqs) &&
                    dayFreqs.TryGetValue(intervalIndex, out var freq) &&
                    freq > bestFreq)
                {
                    bestFreq = freq;
                    bestDay = dayIndex;
                }
            }

            if (bestDay >= 0 &&
                colorMap.TryGetValue(bestDay, out var dayColorMap) &&
                dayColorMap.TryGetValue(intervalIndex, out var chosen))
            {
                return chosen;
            }

            return Color.FromRgb(173, 216, 230); // fallback
        }

        private SolidColorBrush Darken(Color c)
        {
            // Darken by ~30%
            byte r = (byte)Math.Max(0, c.R - (int)(c.R * 0.3));
            byte g = (byte)Math.Max(0, c.G - (int)(c.G * 0.3));
            byte b = (byte)Math.Max(0, c.B - (int)(c.B * 0.3));
            return new SolidColorBrush(Color.FromRgb(r, g, b));
        }

        #endregion

        #region IntervalRenderState (missing piece - now included)

        private sealed class IntervalRenderState
        {
            public ChartValues<double> Baselines { get; } = new ChartValues<double>();
            public ChartValues<double> WhiteHeights { get; } = new ChartValues<double>();
            public ChartValues<double> ColoredHeights { get; } = new ChartValues<double>();

            public bool HasData { get; private set; }
            public bool HasZeroFreqDays { get; private set; }
            public bool HasNonZeroFreqDays { get; private set; }

            private readonly double _height;

            public IntervalRenderState(double intervalHeight)
            {
                _height = intervalHeight;
            }

            public void Add(double baseline, int frequency)
            {
                HasData = true;

                Baselines.Add(baseline);

                if (frequency == 0)
                {
                    WhiteHeights.Add(_height);
                    ColoredHeights.Add(0.0);
                    HasZeroFreqDays = true;
                }
                else
                {
                    WhiteHeights.Add(0.0);
                    ColoredHeights.Add(_height);
                    HasNonZeroFreqDays = true;
                }
            }

            public void AddEmpty()
            {
                Baselines.Add(0.0);
                WhiteHeights.Add(0.0);
                ColoredHeights.Add(0.0);
            }
        }

        #endregion

        private async Task<(Charts.Computation.ChartComputationResult? Result, Models.WeeklyDistributionResult? ExtendedResult)> ComputeWeeklyDistributionAsync(
    IEnumerable<HealthMetricData> data,
    string displayName,
    DateTime from,
    DateTime to)
        {
            // Backwards-compatible default (legacy-only)
            return await ComputeWeeklyDistributionAsync(
                data,
                cmsSeries: null,
                displayName,
                from,
                to,
                useCmsStrategy: false,
                enableParity: false).ConfigureAwait(true);
        }

        private async Task<(Charts.Computation.ChartComputationResult? Result, Models.WeeklyDistributionResult? ExtendedResult)>
        ComputeWeeklyDistributionAsync(
            IEnumerable<HealthMetricData> data,
            DataFileReader.Canonical.ICanonicalMetricSeries? cmsSeries,
            string displayName,
            DateTime from,
            DateTime to,
            bool useCmsStrategy,
            bool enableParity)
        {
            Charts.Computation.ChartComputationResult? result;
            Models.WeeklyDistributionResult? extendedResult;

            // Legacy strategy (always available)
            var legacyStrategy = new WeeklyDistributionStrategy(
                data,
                displayName,
                from,
                to);

            var legacyResult = legacyStrategy.Compute();
            var legacyExtended = legacyStrategy.ExtendedResult;

            // CMS cut-over: PRIMARY metric only
            if (useCmsStrategy && cmsSeries is not null)
            {
                var cmsStrategy = new CmsWeeklyDistributionStrategy(
                    cmsSeries,
                    from,
                    to,
                    displayName);

                result = cmsStrategy.Compute();
                extendedResult = cmsStrategy.ExtendedResult;
            }
            else
            {
                result = legacyResult;
                extendedResult = legacyExtended;
            }

            return (result, extendedResult);
        }



        ///// <summary>
        ///// Calculates simple tooltip data for Simple Range mode (min, max, range, count per day).
        ///// </summary>
        ///// <summary>
        ///// Computes weekly distribution on a background thread.
        ///// </summary>
        //private async Task<(Charts.Computation.ChartComputationResult? Result, Models.WeeklyDistributionResult? ExtendedResult)> ComputeWeeklyDistributionAsync(
        //    IEnumerable<HealthMetricData> data,
        //    string displayName,
        //    DateTime from,
        //    DateTime to)
        //{
        //    Models.WeeklyDistributionResult? extendedResult = null;
        //    Charts.Computation.ChartComputationResult? result = null;

        //    await Task.Run(() =>
        //    {
        //        var strategy = new Charts.Strategies.WeeklyDistributionStrategy(
        //            data,
        //            displayName,
        //            from,
        //            to);

        //        result = strategy.Compute();
        //        extendedResult = strategy.ExtendedResult;
        //    }).ConfigureAwait(true);

        //    return (result, extendedResult);
        //}

        /// <summary>
        /// Sets up tooltip for weekly distribution chart.
        /// </summary>
        private void SetupWeeklyTooltip(
            CartesianChart targetChart,
            Charts.Computation.ChartComputationResult result,
            Models.WeeklyDistributionResult extendedResult,
            bool useFrequencyShading,
            int intervalCount)
        {
            Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData;
            if (useFrequencyShading)
            {
                tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
            }
            else
            {
                tooltipData = CalculateSimpleRangeTooltipData(result, extendedResult);
            }

            if (tooltipData != null && tooltipData.Count > 0)
            {
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

        private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateSimpleRangeTooltipData(
            ChartComputationResult result,
            WeeklyDistributionResult? extendedResult)
        {
            var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

            if (result == null || extendedResult == null)
                return tooltipData;

            var mins = result.PrimaryRawValues;
            var ranges = result.PrimarySmoothed;

            if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
                return tooltipData;

            // For each day, create a single "interval" representing the entire day's range
            for (int dayIndex = 0; dayIndex < 7; dayIndex++)
            {
                var dayIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

                // Check if we have valid min value (not NaN)
                if (double.IsNaN(mins[dayIndex]))
                {
                    continue; // Skip days with invalid min values
                }

                double dayMin = mins[dayIndex];
                double dayRange = double.IsNaN(ranges[dayIndex]) ? 0.0 : ranges[dayIndex];
                double dayMax = dayMin + dayRange;

                // Get count for this day
                int count = 0;
                if (dayIndex < extendedResult.Counts.Count)
                {
                    count = extendedResult.Counts[dayIndex];
                }

                // Add interval if there's valid data (count > 0 and valid min)
                // Note: dayRange can be 0 (all values for the day are the same), which is valid
                if (count > 0)
                {
                    // Single interval representing the entire day's range
                    // Percentage is 100% since this is the only interval for the day
                    dayIntervals.Add((dayMin, dayMax, count, 100.0));
                }

                if (dayIntervals.Count > 0)
                {
                    tooltipData[dayIndex] = dayIntervals;
                }
            }

            return tooltipData;
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
