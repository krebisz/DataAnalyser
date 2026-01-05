using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using ChartHelper = DataVisualiser.Core.Rendering.Helpers.ChartHelper;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Builds a (0 - n) min/max stacked column chart for a single metric.
///     Baseline (transparent) = min per bucket, range column = (max - min) per bucket.
/// </summary>
public class HourlyDistributionService
{
    private const double DefaultMinHeight       = 400.0;
    private const double YAxisRoundingStep      = 5.0;
    private const double YAxisPaddingPercentage = 0.05;
    private const double MinYAxisPadding        = 5.0;
    private const double MaxColumnWidth         = 40.0;
    private const int BucketCount            = 24;

    private const    bool                                       UseCmsHourlyDistribution = false;
    private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;

    private readonly IFrequencyShadingRenderer? _frequencyRenderer;
    private readonly IStrategyCutOverService    _strategyCutOverService;
    private          FrequencyShadingCalculator _frequencyShadingCalculator;
    private          IIntervalShadingStrategy   _shadingStrategy;

    public HourlyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService, IIntervalShadingStrategy? shadingStrategy = null)
    {
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _shadingStrategy = shadingStrategy ?? new FrequencyBasedShadingStrategy(BucketCount);
        _frequencyRenderer = new FrequencyShadingRenderer(MaxColumnWidth, BucketCount);
        _frequencyShadingCalculator = new FrequencyShadingCalculator(_shadingStrategy, BucketCount);
    }

    /// <summary>
    ///     Sets the shading strategy to use for interval coloring.
    /// </summary>
    public void SetShadingStrategy(IIntervalShadingStrategy strategy)
    {
        _shadingStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _frequencyShadingCalculator = new FrequencyShadingCalculator(_shadingStrategy, BucketCount);
    }

    /// <summary>
    ///     Updates the weekly distribution chart with the provided data.
    /// </summary>
    /// <param name="targetChart">The chart to update</param>
    /// <param name="data">The health metric data to visualize</param>
    /// <param name="displayName">Display name for the chart</param>
    /// <param name="from">Start date for the data range</param>
    /// <param name="to">End date for the data range</param>
    /// <param name="minHeight">Minimum height for the chart</param>
    /// <param name="useFrequencyShading">Whether to use frequency shading or simple range view</param>
    /// <param name="intervalCount">Number of intervals to divide the value range into. Default is 10.</param>
    public async Task UpdateHourlyDistributionChartAsync(CartesianChart targetChart, IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = DefaultMinHeight, bool useFrequencyShading = true, int intervalCount = 10, ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
    {
        if (targetChart == null)
            throw new ArgumentNullException(nameof(targetChart));

        if (data == null)
        {
            ChartHelper.ClearChart(targetChart, _chartTimestamps);
            return;
        }

        var useCmsStrategy = cmsSeries != null;

        var (result, frequencyResult) = await ComputeHourlyDistributionAsync(data, cmsSeries, displayName, from, to, useCmsStrategy, enableParity);

        if (result == null || frequencyResult == null)
        {
            ChartHelper.ClearChart(targetChart, _chartTimestamps);
            return;
        }

        try
        {
            // --- Render ---
            targetChart.Series.Clear();

            RenderOriginalMinMaxChart(targetChart, result, displayName, minHeight, frequencyResult, useFrequencyShading, intervalCount);

            // --- Tooltip / state ---
            _chartTimestamps[targetChart] = new List<DateTime>();
            targetChart.DataTooltip = null;

            SetupHourlyTooltip(targetChart, result, frequencyResult, useFrequencyShading, intervalCount);

            ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hourly distribution chart error: {ex.Message}\n{ex.StackTrace}");

            MessageBox.Show($"Error updating chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            ChartHelper.ClearChart(targetChart, _chartTimestamps);
        }
    }


    private static void ConfigureYAxis(CartesianChart targetChart, IList<double> mins, IList<double> ranges)
    {
        // Ensure Y-axis exists
        if (targetChart.AxisY.Count == 0)
            targetChart.AxisY.Add(new Axis());

        var allValues = new List<double>();
        for (var i = 0; i < BucketCount; i++)
        {
            if (!double.IsNaN(mins[i]))
                allValues.Add(mins[i]);

            if (!double.IsNaN(mins[i]) && !double.IsNaN(ranges[i]))
                allValues.Add(mins[i] + ranges[i]);
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
            yAxis.Separator = new Separator
            {
                    Step = step
            };

        yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
        yAxis.ShowLabels = true; // Re-enable labels when rendering data
        yAxis.Title = "Value";   // Ensure title is set
    }

    /// <summary>
    ///     Step 1: Render the original working min/max range chart.
    ///     This shows baseline (min) + range (max-min) as stacked columns per day.
    /// </summary>
    private void RenderOriginalMinMaxChart(CartesianChart targetChart, ChartComputationResult result, string displayName, double minHeight, HourlyDistributionResult? frequencyData, bool useFrequencyShading = true, int intervalCount = 25)
    {
        if (!TryExtractMinMax(result, targetChart, out var mins, out var ranges))
            return;

        var hourValues = GetHourValuesFromStrategy(frequencyData);

        var (globalMin, globalMax) = CalculateGlobalMinMax(mins, ranges);

        LogHourlySummary(mins, ranges, hourValues, globalMin, globalMax);

        var shadingData = useFrequencyShading ? BuildFrequencyShadingData(hourValues, globalMin, globalMax, intervalCount) : FrequencyShadingData.Empty;

        AddBaselineAndRangeSeries(targetChart, mins, ranges, globalMin, displayName, useFrequencyShading);

        if (useFrequencyShading)
            ApplyFrequencyShadingViaRenderer(targetChart, mins, ranges, shadingData, globalMin, globalMax);

        ConfigureYAxis(targetChart, mins, ranges);
        ConfigureXAxis(targetChart);
        targetChart.LegendLocation = LegendLocation.None;
    }

    // FrequencyShadingData moved to FrequencyShadingCalculator namespace

    private bool TryExtractMinMax(ChartComputationResult result, CartesianChart chart, out List<double> mins, out List<double> ranges)
    {
        mins = result.PrimaryRawValues;
        ranges = result.PrimarySmoothed;

        var isValid = mins != null && ranges != null && mins.Count == BucketCount && ranges.Count == BucketCount;

        if (!isValid)
            ChartHelper.ClearChart(chart, _chartTimestamps);

        return isValid;
    }


    private(double Min, double Max) CalculateGlobalMinMax(List<double> mins, List<double> ranges)
    {
        var min = mins.Where(m => !double.IsNaN(m)).
                       DefaultIfEmpty(0).
                       Min();

        var max = mins.Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r).
                       Where(v => !double.IsNaN(v)).
                       DefaultIfEmpty(min + 1).
                       Max();

        if (max <= min)
            max = min + 1;

        return (min, max);
    }

    private FrequencyShadingData BuildFrequencyShadingData(Dictionary<int, List<double>> hourValues, double globalMin, double globalMax, int intervalCount)
    {
        return _frequencyShadingCalculator.BuildFrequencyShadingData(hourValues, globalMin, globalMax, intervalCount);
    }

    private void AddBaselineAndRangeSeries(CartesianChart chart, List<double> mins, List<double> ranges, double globalMin, string displayName, bool useFrequencyShading)
    {
        var baseline = CreateBaselineSeries(displayName);
        var range = CreateRangeSeries(displayName);

        for (var i = 0; i < BucketCount; i++)
        {
            var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];

            var baselineVal = useFrequencyShading ? globalMin : double.IsNaN(mins[i]) ? 0.0 : mins[i];

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

    private void ApplyFrequencyShadingViaRenderer(CartesianChart chart, List<double> mins, List<double> ranges, FrequencyShadingData data, double globalMin, double globalMax)
    {
        var context = new IntervalShadingContext
        {
                Intervals = data.Intervals,
                FrequenciesPerBucket = data.FrequenciesPerDay,
                BucketValues = data.DayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
        };

        _frequencyRenderer.Render(chart, mins, ranges, data.Intervals, data.FrequenciesPerDay, data.ColorMap, globalMin, globalMax, context);
    }

    private void ConfigureXAxis(CartesianChart chart)
    {
        if (chart.AxisX.Count == 0)
            chart.AxisX.Add(new Axis());

        var axis = chart.AxisX[0];
        axis.Labels = new[]
        {
                "12AM",
                "1AM",
                "2AM",
                "3AM",
                "4AM",
                "5AM",
                "6AM",
                "7AM",
                "8AM",
                "9AM",
                "10AM",
                "11AM",
                "12PM",
                "1PM",
                "2PM",
                "3PM",
                "4PM",
                "5PM",
                "6PM",
                "7PM",
                "8PM",
                "9PM",
                "10PM",
                "11PM"
        };
        axis.Title = "Hour of Day";
        axis.ShowLabels = true;
        axis.Separator = new Separator
        {
                Step = 1,
                IsEnabled = false
        };
    }

    private void LogHourlySummary(List<double> mins, List<double> ranges, Dictionary<int, List<double>> hourValues, double globalMin, double globalMax)
    {
        Debug.WriteLine("=== HourlyDistribution: Data Summary ===");
        Debug.WriteLine($"Global Min: {globalMin:F4}, Global Max: {globalMax:F4}, Range: {globalMax - globalMin:F4}");
        Debug.WriteLine("Hour Min/Max values:");

        for (var i = 0; i < BucketCount; i++)
        {
            var hourMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
            var hourMax = hourMin + (double.IsNaN(ranges[i]) ? 0.0 : ranges[i]);
            Debug.WriteLine($"  Hour {i}: Min={hourMin:F4}, Max={hourMax:F4}, Range={ranges[i]:F4}");
        }

        // Log sample raw values for first hour with data
        for (var hourIndex = 0; hourIndex < BucketCount; hourIndex++)
            if (hourValues.TryGetValue(hourIndex, out var values) && values.Count > 0)
            {
                Debug.WriteLine($"Hour {hourIndex} raw values (first 10): {string.Join(", ", values.Take(10).Select(v => v.ToString("F4")))}");
                Debug.WriteLine($"Hour {hourIndex} total value count: {values.Count}");
                break;
            }
    }


    /// <summary>
    ///     Gets day values from frequency data. Returns empty lists if not available.
    /// </summary>
    private Dictionary<int, List<double>> GetHourValuesFromStrategy(HourlyDistributionResult? frequencyData)
    {
        var hourValues = new Dictionary<int, List<double>>(BucketCount);

        for (var i = 0; i < BucketCount; i++)
            hourValues[i] = frequencyData?.HourValues?.TryGetValue(i, out var values) == true ? values : new List<double>();

        return hourValues;
    }


    // Frequency shading calculation methods moved to FrequencyShadingCalculator

    private void ApplyFrequencyShading(CartesianChart targetChart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerHour, Dictionary<int, Dictionary<int, Color>> colorMap, double globalMin, double globalMax, IntervalShadingContext shadingContext)
    {
        RemoveExistingRangeSeries(targetChart);

        Debug.WriteLine("=== HourlyDistribution: ApplyFrequencyShading ===");
        Debug.WriteLine($"Intervals count: {intervals.Count}, Frequencies count: {frequenciesPerHour.Count}");

        // Guard: if no intervals or frequencies, restore simple range series.
        if (!CanApplyFrequencyShading(intervals, frequenciesPerHour, mins, ranges))
        {
            RestoreSimpleRangeSeries(targetChart, ranges);
            return;
        }

        // Uniform interval height across all days.
        var uniformIntervalHeight = CalculateUniformIntervalHeight(globalMin, globalMax, intervals.Count);
        Debug.WriteLine($"Uniform interval height: {uniformIntervalHeight:F6}");
        Debug.WriteLine($"Global range: {globalMin:F4} to {globalMax:F4}");

        // Used only for logging / diagnostics, but keep it for parity with original behavior.
        var globalMaxFreq = CalculateGlobalMaxFrequency(frequenciesPerHour);
        Debug.WriteLine($"Global max frequency for normalization: {globalMaxFreq}");

        // Track cumulative stack height per hour (needed because StackedColumnSeries stacks across series).
        var cumulativeStackHeight = InitializeCumulativeStack(globalMin);

        var seriesCreated = RenderIntervals(targetChart, mins, ranges, intervals, frequenciesPerHour, colorMap, uniformIntervalHeight, cumulativeStackHeight, globalMaxFreq);

        // Safety: If nothing was created, restore a visible range series.
        if (seriesCreated == 0)
        {
            RestoreSimpleRangeSeries(targetChart, ranges);
            Debug.WriteLine("HourlyDistribution: No interval series created, using fallback simple range series");
            return;
        }

        Debug.WriteLine("=== HourlyDistribution: Rendering Complete ===");
        Debug.WriteLine($"Total interval series created: {seriesCreated}");
        Debug.WriteLine($"Total series in chart: {targetChart.Series.Count}");

        var seriesTypes = targetChart.Series.GroupBy(s => s.GetType().
                                                            Name).
                                      Select(g => $"{g.Key}:{g.Count()}").
                                      ToList();

        Debug.WriteLine($"Series breakdown: {string.Join(", ", seriesTypes)}");
    }

    #region Core pipeline

    private int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerHour, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
    {
        var renderer = new HourlyIntervalRenderer();
        return renderer.RenderIntervals(chart, mins, ranges, intervals, frequenciesPerHour, colorMap, uniformIntervalHeight, cumulativeStackHeight, globalMaxFreq);
    }

    #endregion

    private async Task<(ChartComputationResult? Result, HourlyDistributionResult? ExtendedResult)> ComputeHourlyDistributionAsync(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to)
    {
        // Backwards-compatible default (legacy-only)
        return await ComputeHourlyDistributionAsync(data, null, displayName, from, to, false, false).
                ConfigureAwait(true);
    }

    private async Task<(ChartComputationResult? Result, HourlyDistributionResult? ExtendedResult)> ComputeHourlyDistributionAsync(IEnumerable<MetricData> data, ICanonicalMetricSeries? cmsSeries, string displayName, DateTime from, DateTime to, bool useCmsStrategy, bool enableParity)
    {
        // Create minimal ChartDataContext for cut-over decision
        var ctx = new ChartDataContext
        {
                PrimaryCms = cmsSeries,
                Data1 = data?.ToList(),
                DisplayName1 = displayName,
                From = from,
                To = to
        };

        // Create strategy using unified cut-over service
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = data,
                Label1 = displayName,
                From = from,
                To = to
        };

        var strategy = _strategyCutOverService.CreateStrategy(StrategyType.HourlyDistribution, ctx, parameters);

        // Compute result
        var result = strategy.Compute();

        // Extract ExtendedResult (HourlyDistribution strategies have this property)
        HourlyDistributionResult? extendedResult = null;
        if (strategy is HourlyDistributionStrategy legacyStrategy)
            extendedResult = legacyStrategy.ExtendedResult;
        else if (strategy is CmsHourlyDistributionStrategy cmsStrategy)
            extendedResult = cmsStrategy.ExtendedResult;

        return (result, extendedResult);
    }


    ///// <summary>
    ///// Calculates simple tooltip data for Simple Range mode (min, max, range, count per day).
    ///// </summary>
    ///// <summary>
    /// <summary>
    ///     Sets up tooltip for weekly distribution chart.
    /// </summary>
    private void SetupHourlyTooltip(CartesianChart targetChart, ChartComputationResult result, HourlyDistributionResult extendedResult, bool useFrequencyShading, int intervalCount)
    {
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData;
        if (useFrequencyShading)
            tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
        else
            tooltipData = CalculateSimpleRangeTooltipData(result, extendedResult);

        if (tooltipData != null && tooltipData.Count > 0)
        {
            var oldTooltip = targetChart.Tag as HourlyDistributionTooltip;
            oldTooltip?.Dispose();

            var tooltip = new HourlyDistributionTooltip(targetChart, tooltipData);
            targetChart.Tag = tooltip;
        }
        else
        {
            var oldTooltip = targetChart.Tag as HourlyDistributionTooltip;
            oldTooltip?.Dispose();
            targetChart.Tag = null;
        }
    }

    private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateSimpleRangeTooltipData(ChartComputationResult result, HourlyDistributionResult? extendedResult)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || extendedResult == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != BucketCount || ranges.Count != BucketCount)
            return tooltipData;

        // For each hour, create a single "interval" representing the entire hour's range
        for (var hourIndex = 0; hourIndex < BucketCount; hourIndex++)
        {
            var dayIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

            // Check if we have valid min value (not NaN)
            if (double.IsNaN(mins[hourIndex]))
                continue; // Skip hours with invalid min values

            var hourMin = mins[hourIndex];
            var hourRange = double.IsNaN(ranges[hourIndex]) ? 0.0 : ranges[hourIndex];
            var hourMax = hourMin + hourRange;

            // Get count for this hour
            var count = 0;
            if (hourIndex < extendedResult.Counts.Count)
                count = extendedResult.Counts[hourIndex];

            // Add interval if there's valid data (count > 0 and valid min)
            // Note: hourRange can be 0 (all values for the hour are the same), which is valid
            if (count > 0)
                    // Single interval representing the entire hour's range
                    // Percentage is 100% since this is the only interval for the hour
                dayIntervals.Add((hourMin, hourMax, count, 100.0));

            if (dayIntervals.Count > 0)
                tooltipData[hourIndex] = dayIntervals;
        }

        return tooltipData;
    }

    /// <summary>
    ///     Calculates tooltip data with interval breakdown, percentages, and counts for each day.
    /// </summary>
    private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateTooltipData(ChartComputationResult result, HourlyDistributionResult? frequencyData, int intervalCount = 25)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || frequencyData == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != BucketCount || ranges.Count != BucketCount)
            return tooltipData;

        // Get hour values
        var hourValues = GetHourValuesFromStrategy(frequencyData);

        // Calculate global min/max
        var globalMin = mins.Where(m => !double.IsNaN(m)).
                             DefaultIfEmpty(0).
                             Min();
        var globalMax = mins.Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r).
                             Where(v => !double.IsNaN(v)).
                             DefaultIfEmpty(1).
                             Max();

        if (globalMax <= globalMin)
            globalMax = globalMin + 1;

        // Create intervals (same as in RenderOriginalMinMaxChart)
        var intervals = _frequencyShadingCalculator.CreateUniformIntervals(globalMin, globalMax, intervalCount);

        // Count frequencies per interval per hour
        var frequenciesPerHour = _frequencyShadingCalculator.CountFrequenciesPerInterval(hourValues, intervals);

        // Calculate percentages for each hour
        for (var hourIndex = 0; hourIndex < BucketCount; hourIndex++)
        {
            var hourIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

            // Get total count for this hour
            var totalCount = 0;
            if (hourValues.TryGetValue(hourIndex, out var values))
                totalCount = values.Count;

            // Calculate hour min/max to determine which intervals are within the hour's range
            var hourMin = double.IsNaN(mins[hourIndex]) ? 0.0 : mins[hourIndex];
            var hourMax = hourMin + (double.IsNaN(ranges[hourIndex]) ? 0.0 : ranges[hourIndex]);

            // For each interval, calculate count and percentage
            for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var interval = intervals[intervalIndex];

                // Check if interval overlaps with hour's range
                var intervalOverlapsHourRange = interval.Min < hourMax && interval.Max > hourMin;

                if (intervalOverlapsHourRange && ranges[hourIndex] > 0 && !double.IsNaN(ranges[hourIndex]))
                {
                    // Get frequency for this interval
                    var count = 0;
                    if (frequenciesPerHour.TryGetValue(hourIndex, out var hourFreqs) && hourFreqs.TryGetValue(intervalIndex, out var freq))
                        count = freq;

                    // Calculate percentage (percentage of total values for this hour)
                    var percentage = totalCount > 0 ? (double)count / totalCount * 100.0 : 0.0;

                    hourIntervals.Add((interval.Min, interval.Max, count, percentage));
                }
            }

            tooltipData[hourIndex] = hourIntervals;
        }

        return tooltipData;
    }

    /// <summary>
    ///     Creates a brush with intensity based on normalized frequency.
    ///     Higher frequency = darker color (closer to black).
    ///     Uses a blue color ramp from light blue to near-black.
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
        var r = (byte)Math.Round(r0 + (r1 - r0) * normalizedFrequency);
        var g = (byte)Math.Round(g0 + (g1 - g0) * normalizedFrequency);
        var b = (byte)Math.Round(b0 + (b1 - b0) * normalizedFrequency);

        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    #region IntervalRenderState (missing piece - now included)

    private sealed class IntervalRenderState
    {
        private readonly double _height;

        public IntervalRenderState(double intervalHeight)
        {
            _height = intervalHeight;
        }

        public ChartValues<double> Baselines      { get; } = new();
        public ChartValues<double> WhiteHeights   { get; } = new();
        public ChartValues<double> ColoredHeights { get; } = new();

        public bool HasData            { get; private set; }
        public bool HasZeroFreqDays    { get; private set; }
        public bool HasNonZeroFreqDays { get; private set; }

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


    #region Fallback + guards

    private void RemoveExistingRangeSeries(CartesianChart chart)
    {
        var seriesToRemove = chart.Series.Where(s => s.Title?.Contains("range") == true).
                                   ToList();
        foreach (var series in seriesToRemove)
            chart.Series.Remove(series);
    }

    private bool CanApplyFrequencyShading(List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerHour, List<double> mins, List<double> ranges)
    {
        if (intervals == null || frequenciesPerHour == null || mins == null || ranges == null)
            return false;

        if (intervals.Count == 0 || frequenciesPerHour.Count == 0)
            return false;

        return HasAnyIntervalOverlap(intervals, mins, ranges);
    }

    private bool HasAnyIntervalOverlap(List<(double Min, double Max)> intervals, List<double> mins, List<double> ranges)
    {
        for (var hourIndex = 0; hourIndex < BucketCount; hourIndex++)
        {
            var hourMin = SafeMin(mins, hourIndex);
            var hourMax = hourMin + SafeRange(ranges, hourIndex);

            for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var iv = intervals[intervalIndex];
                if (iv.Min < hourMax && iv.Max > hourMin && ranges[hourIndex] > 0)
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

        for (var i = 0; i < BucketCount; i++)
            simpleRangeSeries.Values.Add(SafeRange(ranges, i));

        chart.Series.Add(simpleRangeSeries);
    }

    #endregion

    #region Small utilities

    private double CalculateUniformIntervalHeight(double globalMin, double globalMax, int intervalCount)
    {
        return intervalCount > 0 ? (globalMax - globalMin) / intervalCount : 1.0;
    }

    private int CalculateGlobalMaxFrequency(Dictionary<int, Dictionary<int, int>> frequenciesPerHour)
    {
        return frequenciesPerHour.Values.SelectMany(h => h.Values).
                                 DefaultIfEmpty(1).
                                 Max();
    }

    private double[] InitializeCumulativeStack(double globalMin)
    {
        var cumulative = new double[BucketCount];
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
        return double.IsNaN(v) || v < 0 ? 0.0 : v;
    }

    private Color ResolveIntervalColor(Dictionary<int, Dictionary<int, int>> frequenciesPerHour, Dictionary<int, Dictionary<int, Color>> colorMap, int intervalIndex)
    {
        // Pick the hour with the highest frequency for this interval (most representative color).
        var bestHour = -1;
        var bestFreq = 0;

        for (var hourIndex = 0; hourIndex < BucketCount; hourIndex++)
            if (frequenciesPerHour.TryGetValue(hourIndex, out var hourFreqs) && hourFreqs.TryGetValue(intervalIndex, out var freq) && freq > bestFreq)
            {
                bestFreq = freq;
                bestHour = hourIndex;
            }

        if (bestHour >= 0 && colorMap.TryGetValue(bestHour, out var hourColorMap) && hourColorMap.TryGetValue(intervalIndex, out var chosen))
            return chosen;

        return Color.FromRgb(173, 216, 230); // fallback
    }

    private SolidColorBrush Darken(Color c)
    {
        // Darken by ~30%
        var r = (byte)Math.Max(0, c.R - (int)(c.R * 0.3));
        var g = (byte)Math.Max(0, c.G - (int)(c.G * 0.3));
        var b = (byte)Math.Max(0, c.B - (int)(c.B * 0.3));
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    #endregion
}