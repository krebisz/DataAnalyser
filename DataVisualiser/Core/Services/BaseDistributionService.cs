using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;
using ChartHelper = DataVisualiser.Core.Rendering.Helpers.ChartHelper;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Base class for distribution services (weekly, hourly, etc.)
///     Contains all common functionality shared between distribution chart types
/// </summary>
public abstract class BaseDistributionService
{
    protected const double DefaultMinHeight         = 400.0;
    protected const double YAxisRoundingStep        = 5.0;
    protected const double YAxisPaddingPercentage   = 0.05;
    protected const double MinYAxisPadding          = 5.0;
    protected const double MaxColumnWidth          = 40.0;

    protected readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    protected readonly IFrequencyShadingRenderer?                   _frequencyRenderer;
    protected readonly IStrategyCutOverService                      _strategyCutOverService;
    protected          FrequencyShadingCalculator                   _frequencyShadingCalculator;
    protected          IIntervalShadingStrategy                      _shadingStrategy;

    protected readonly IDistributionConfiguration Configuration;

    protected BaseDistributionService(
        IDistributionConfiguration configuration,
        Dictionary<CartesianChart, List<DateTime>> chartTimestamps,
        IStrategyCutOverService strategyCutOverService,
        IIntervalShadingStrategy? shadingStrategy = null)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _shadingStrategy = shadingStrategy ?? new FrequencyBasedShadingStrategy(Configuration.BucketCount);
        _frequencyRenderer = new FrequencyShadingRenderer(MaxColumnWidth, Configuration.BucketCount);
        _frequencyShadingCalculator = new FrequencyShadingCalculator(_shadingStrategy, Configuration.BucketCount);
    }

    /// <summary>
    ///     Sets the shading strategy to use for interval coloring.
    /// </summary>
    public void SetShadingStrategy(IIntervalShadingStrategy strategy)
    {
        _shadingStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _frequencyShadingCalculator = new FrequencyShadingCalculator(_shadingStrategy, Configuration.BucketCount);
    }

    /// <summary>
    ///     Updates the distribution chart with the provided data.
    /// </summary>
    public async Task UpdateDistributionChartAsync(
        CartesianChart targetChart,
        IEnumerable<MetricData> data,
        string displayName,
        DateTime from,
        DateTime to,
        double minHeight = DefaultMinHeight,
        bool useFrequencyShading = true,
        int intervalCount = 10,
        ICanonicalMetricSeries? cmsSeries = null,
        bool enableParity = false)
    {
        if (targetChart == null)
            throw new ArgumentNullException(nameof(targetChart));

        if (data == null)
        {
            ChartHelper.ClearChart(targetChart, _chartTimestamps);
            return;
        }

        var useCmsStrategy = cmsSeries != null;

        var (result, frequencyResult) = await ComputeDistributionAsync(data, cmsSeries, displayName, from, to, useCmsStrategy, enableParity);

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

            SetupTooltip(targetChart, result, frequencyResult, useFrequencyShading, intervalCount);

            ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{Configuration.LogPrefix}: Chart error: {ex.Message}\n{ex.StackTrace}");

            MessageBox.Show($"Error updating chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            ChartHelper.ClearChart(targetChart, _chartTimestamps);
        }
    }

    // Abstract methods that must be implemented by derived classes
    protected abstract Task<(ChartComputationResult? Result, object? ExtendedResult)> ComputeDistributionAsync(
        IEnumerable<MetricData> data,
        ICanonicalMetricSeries? cmsSeries,
        string displayName,
        DateTime from,
        DateTime to,
        bool useCmsStrategy,
        bool enableParity);

    protected abstract Dictionary<int, List<double>> GetBucketValuesFromResult(object? frequencyData);

    protected abstract void SetupTooltip(
        CartesianChart targetChart,
        ChartComputationResult result,
        object extendedResult,
        bool useFrequencyShading,
        int intervalCount);

    protected abstract IIntervalRenderer CreateIntervalRenderer();

    // Common methods shared by all distribution services
    protected static void ConfigureYAxis(CartesianChart targetChart, IList<double> mins, IList<double> ranges, int bucketCount)
    {
        // Ensure Y-axis exists
        if (targetChart.AxisY.Count == 0)
            targetChart.AxisY.Add(new Axis());

        var allValues = new List<double>();
        for (var i = 0; i < bucketCount; i++)
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

    protected void RenderOriginalMinMaxChart(
        CartesianChart targetChart,
        ChartComputationResult result,
        string displayName,
        double minHeight,
        object? frequencyData,
        bool useFrequencyShading = true,
        int intervalCount = 25)
    {
        if (!TryExtractMinMax(result, targetChart, out var mins, out var ranges))
            return;

        var bucketValues = GetBucketValuesFromResult(frequencyData);

        var (globalMin, globalMax) = CalculateGlobalMinMax(mins, ranges);

        LogSummary(mins, ranges, bucketValues, globalMin, globalMax);

        var shadingData = useFrequencyShading ? BuildFrequencyShadingData(bucketValues, globalMin, globalMax, intervalCount) : FrequencyShadingData.Empty;

        AddBaselineAndRangeSeries(targetChart, mins, ranges, globalMin, displayName, useFrequencyShading);

        if (useFrequencyShading)
            ApplyFrequencyShadingViaRenderer(targetChart, mins, ranges, shadingData, globalMin, globalMax);

        ConfigureYAxis(targetChart, mins, ranges, Configuration.BucketCount);
        ConfigureXAxis(targetChart);
        targetChart.LegendLocation = LegendLocation.None;
    }

    protected bool TryExtractMinMax(ChartComputationResult result, CartesianChart chart, out List<double> mins, out List<double> ranges)
    {
        mins = result.PrimaryRawValues;
        ranges = result.PrimarySmoothed;

        var isValid = mins != null && ranges != null && mins.Count == Configuration.BucketCount && ranges.Count == Configuration.BucketCount;

        if (!isValid)
            ChartHelper.ClearChart(chart, _chartTimestamps);

        return isValid;
    }

    protected (double Min, double Max) CalculateGlobalMinMax(List<double> mins, List<double> ranges)
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

    protected FrequencyShadingData BuildFrequencyShadingData(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax, int intervalCount)
    {
        return _frequencyShadingCalculator.BuildFrequencyShadingData(bucketValues, globalMin, globalMax, intervalCount);
    }

    protected void AddBaselineAndRangeSeries(CartesianChart chart, List<double> mins, List<double> ranges, double globalMin, string displayName, bool useFrequencyShading)
    {
        var baseline = CreateBaselineSeries(displayName);
        var range = CreateRangeSeries(displayName);

        for (var i = 0; i < Configuration.BucketCount; i++)
        {
            var rangeVal = double.IsNaN(ranges[i]) || ranges[i] < 0 ? 0.0 : ranges[i];

            var baselineVal = useFrequencyShading ? globalMin : double.IsNaN(mins[i]) ? 0.0 : mins[i];

            baseline.Values.Add(baselineVal);
            range.Values.Add(rangeVal);
        }

        chart.Series.Add(baseline);
        chart.Series.Add(range);
    }

    protected StackedColumnSeries CreateBaselineSeries(string displayName)
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

    protected StackedColumnSeries CreateRangeSeries(string displayName)
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

    protected void ApplyFrequencyShadingViaRenderer(CartesianChart chart, List<double> mins, List<double> ranges, FrequencyShadingData data, double globalMin, double globalMax)
    {
        var context = new IntervalShadingContext
        {
                Intervals = data.Intervals,
                FrequenciesPerBucket = data.FrequenciesPerBucket,
                BucketValues = data.BucketValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
        };

        _frequencyRenderer.Render(chart, mins, ranges, data.Intervals, data.FrequenciesPerBucket, data.ColorMap, globalMin, globalMax, context);
    }

    protected void ConfigureXAxis(CartesianChart chart)
    {
        if (chart.AxisX.Count == 0)
            chart.AxisX.Add(new Axis());

        var axis = chart.AxisX[0];
        axis.Labels = Configuration.BucketLabels;
        axis.Title = Configuration.XAxisTitle;
        axis.ShowLabels = true;
        axis.Separator = new Separator
        {
                Step = 1,
                IsEnabled = false
        };
    }

    protected void LogSummary(List<double> mins, List<double> ranges, Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        Debug.WriteLine($"=== {Configuration.LogPrefix}: Data Summary ===");
        Debug.WriteLine($"Global Min: {globalMin:F4}, Global Max: {globalMax:F4}, Range: {globalMax - globalMin:F4}");
        Debug.WriteLine($"{Configuration.BucketName} Min/Max values:");

        for (var i = 0; i < Configuration.BucketCount; i++)
        {
            var bucketMin = double.IsNaN(mins[i]) ? 0.0 : mins[i];
            var bucketMax = bucketMin + (double.IsNaN(ranges[i]) ? 0.0 : ranges[i]);
            Debug.WriteLine($"  {Configuration.BucketName} {i}: Min={bucketMin:F4}, Max={bucketMax:F4}, Range={ranges[i]:F4}");
        }

        // Log sample raw values for first bucket with data
        for (var bucketIndex = 0; bucketIndex < Configuration.BucketCount; bucketIndex++)
            if (bucketValues.TryGetValue(bucketIndex, out var values) && values.Count > 0)
            {
                Debug.WriteLine($"{Configuration.BucketName} {bucketIndex} raw values (first 10): {string.Join(", ", values.Take(10).Select(v => v.ToString("F4")))}");
                Debug.WriteLine($"{Configuration.BucketName} {bucketIndex} total value count: {values.Count}");
                break;
            }
    }

    protected int RenderIntervals(
        CartesianChart chart,
        List<double> mins,
        List<double> ranges,
        List<(double Min, double Max)> intervals,
        Dictionary<int, Dictionary<int, int>> frequenciesPerBucket,
        Dictionary<int, Dictionary<int, Color>> colorMap,
        double uniformIntervalHeight,
        double[] cumulativeStackHeight,
        int globalMaxFreq)
    {
        var renderer = CreateIntervalRenderer();
        return renderer.RenderIntervals(chart, mins, ranges, intervals, frequenciesPerBucket, colorMap, uniformIntervalHeight, cumulativeStackHeight, globalMaxFreq);
    }

    protected double CalculateUniformIntervalHeight(double globalMin, double globalMax, int intervalCount)
    {
        return intervalCount > 0 ? (globalMax - globalMin) / intervalCount : 1.0;
    }

    protected int CalculateGlobalMaxFrequency(Dictionary<int, Dictionary<int, int>> frequenciesPerBucket)
    {
        return frequenciesPerBucket.Values.SelectMany(d => d.Values).
                                 DefaultIfEmpty(1).
                                 Max();
    }

    protected double[] InitializeCumulativeStack(double globalMin)
    {
        var cumulative = new double[Configuration.BucketCount];
        Array.Fill(cumulative, globalMin);
        return cumulative;
    }

    protected double SafeMin(List<double> mins, int index)
    {
        var v = mins[index];
        return double.IsNaN(v) ? 0.0 : v;
    }

    protected double SafeRange(List<double> ranges, int index)
    {
        var v = ranges[index];
        return double.IsNaN(v) || v < 0 ? 0.0 : v;
    }
}

/// <summary>
///     Interface for interval renderers (weekly, hourly, etc.)
/// </summary>
public interface IIntervalRenderer
{
    int RenderIntervals(
        CartesianChart chart,
        List<double> mins,
        List<double> ranges,
        List<(double Min, double Max)> intervals,
        Dictionary<int, Dictionary<int, int>> frequenciesPerBucket,
        Dictionary<int, Dictionary<int, Color>> colorMap,
        double uniformIntervalHeight,
        double[] cumulativeStackHeight,
        int globalMaxFreq);
}

