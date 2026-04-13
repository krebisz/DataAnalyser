using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Base class for distribution services (weekly, hourly, etc.)
///     Contains all common functionality shared between distribution chart types
/// </summary>
public abstract class BaseDistributionService : IDistributionService
{
    protected readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps;
    protected readonly IFrequencyShadingRenderer _frequencyRenderer;
    protected readonly IUserNotificationService _notificationService;
    protected readonly ChartRenderGate _renderGate = new();
    protected readonly IStrategyCutOverService _strategyCutOverService;

    protected readonly IDistributionConfiguration Configuration;
    protected FrequencyShadingCalculator _frequencyShadingCalculator;
    protected IIntervalShadingStrategy _shadingStrategy;

    protected BaseDistributionService(IDistributionConfiguration configuration, Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService, IUserNotificationService notificationService, IIntervalShadingStrategy? shadingStrategy = null)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _chartTimestamps = chartTimestamps ?? throw new ArgumentNullException(nameof(chartTimestamps));
        _strategyCutOverService = strategyCutOverService ?? throw new ArgumentNullException(nameof(strategyCutOverService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _shadingStrategy = shadingStrategy ?? new FrequencyBasedShadingStrategy(Configuration.BucketCount);
        _frequencyRenderer = new FrequencyShadingRenderer(RenderingDefaults.MaxColumnWidth, Configuration.BucketCount);
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
    public async Task UpdateDistributionChartAsync(CartesianChart targetChart, IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, double minHeight = DistributionDefaults.DefaultMinHeight, bool useFrequencyShading = true, int intervalCount = 10, ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
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

        _renderGate.ExecuteWhenReady(targetChart,
                () =>
                {
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
                        targetChart.Update(true, true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"{Configuration.LogPrefix}: Chart error: {ex.Message}\n{ex.StackTrace}");

                        _notificationService.ShowError("Error", $"Error updating chart: {ex.Message}\n\nSee debug output for details.");
                        ChartHelper.ClearChart(targetChart, _chartTimestamps);
                    }
                });
    }

    /// <summary>
    ///     Computes per-bucket min/max values for simple range rendering.
    /// </summary>
    public async Task<DistributionRangeResult?> ComputeSimpleRangeAsync(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to, ICanonicalMetricSeries? cmsSeries = null, bool enableParity = false)
    {
        if (data == null)
            return null;

        var useCmsStrategy = cmsSeries != null;
        var (result, extendedResult) = await ComputeDistributionAsync(data, cmsSeries, displayName, from, to, useCmsStrategy, enableParity);
        return DistributionRangeResultBuilder.Build(result, extendedResult, Configuration.BucketCount);
    }

    // Abstract methods that must be implemented by derived classes
    protected async Task<(ChartComputationResult? Result, BucketDistributionResult? ExtendedResult)> ComputeDistributionAsync(IEnumerable<MetricData> data, ICanonicalMetricSeries? cmsSeries, string displayName, DateTime from, DateTime to, bool useCmsStrategy, bool enableParity)
    {
        var ctx = BuildContext(data, cmsSeries, displayName, from, to);
        var parameters = BuildParameters(data, displayName, from, to);

        var strategy = CreateStrategy(ctx, parameters);
        var result = strategy.Compute();
        var extendedResult = ExtractExtendedResult(strategy);

        return await Task.FromResult((result, extendedResult));
    }

    protected abstract BucketDistributionResult? ExtractExtendedResult(object strategy);

    private IChartComputationStrategy CreateStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        return _strategyCutOverService.CreateStrategy(Configuration.StrategyType, ctx, parameters);
    }

    private ChartDataContext BuildContext(IEnumerable<MetricData> data, ICanonicalMetricSeries? cmsSeries, string displayName, DateTime from, DateTime to)
    {
        return new ChartDataContext
        {
                PrimaryCms = cmsSeries,
                Data1 = data?.ToList(),
                DisplayName1 = displayName,
                From = from,
                To = to
        };
    }

    private StrategyCreationParameters BuildParameters(IEnumerable<MetricData> data, string displayName, DateTime from, DateTime to)
    {
        return new StrategyCreationParameters
        {
                LegacyData1 = data,
                Label1 = displayName,
                From = from,
                To = to
        };
    }

    protected Dictionary<int, List<double>> GetBucketValuesFromResult(BucketDistributionResult? frequencyData)
    {
        return DistributionComputationHelper.GetBucketValues(frequencyData, Configuration.BucketCount);
    }

    protected abstract void SetupTooltip(CartesianChart targetChart, ChartComputationResult result, BucketDistributionResult extendedResult, bool useFrequencyShading, int intervalCount);

    protected abstract IIntervalRenderer CreateIntervalRenderer();

    // Common methods shared by all distribution services
    protected void RenderOriginalMinMaxChart(CartesianChart targetChart, ChartComputationResult result, string displayName, double minHeight, BucketDistributionResult? frequencyData, bool useFrequencyShading = true, int intervalCount = 25)
    {
        if (!TryExtractMinMax(result, targetChart, out var mins, out var ranges))
            return;

        var bucketValues = GetBucketValuesFromResult(frequencyData);

        var (globalMin, globalMax) = CalculateGlobalMinMax(mins, ranges);

        DistributionDebugSummaryLogger.LogSummary(
            Configuration.LogPrefix,
            Configuration.BucketName,
            Configuration.BucketCount,
            mins,
            ranges,
            bucketValues,
            globalMin,
            globalMax);

        var shadingData = useFrequencyShading ? BuildFrequencyShadingData(bucketValues, globalMin, globalMax, intervalCount) : FrequencyShadingData.Empty;

        DistributionSeriesBuilder.AddBaselineAndRangeSeries(targetChart, mins, ranges, globalMin, displayName, useFrequencyShading, Configuration.BucketCount);

        if (!useFrequencyShading)
            DistributionSeriesBuilder.AddAverageSeries(targetChart, bucketValues, displayName, Configuration.BucketCount);

        if (useFrequencyShading)
            ApplyFrequencyShadingViaRenderer(targetChart, mins, ranges, shadingData, globalMin, globalMax);

        DistributionAxisCoordinator.ConfigureYAxis(targetChart, mins, ranges, Configuration.BucketCount);
        DistributionAxisCoordinator.ConfigureXAxis(targetChart, Configuration.BucketLabels, Configuration.XAxisTitle);
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

    protected(double Min, double Max) CalculateGlobalMinMax(List<double> mins, List<double> ranges)
    {
        return DistributionComputationHelper.CalculateGlobalMinMax(mins, ranges);
    }

    protected FrequencyShadingData BuildFrequencyShadingData(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax, int intervalCount)
    {
        return _frequencyShadingCalculator.BuildFrequencyShadingData(bucketValues, globalMin, globalMax, intervalCount);
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

    protected int RenderIntervals(CartesianChart chart, List<double> mins, List<double> ranges, List<(double Min, double Max)> intervals, Dictionary<int, Dictionary<int, int>> frequenciesPerBucket, Dictionary<int, Dictionary<int, Color>> colorMap, double uniformIntervalHeight, double[] cumulativeStackHeight, int globalMaxFreq)
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
        return frequenciesPerBucket.Values.SelectMany(b => b.Values).DefaultIfEmpty(1).Max();
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

    /// <summary>
    ///     Calculates tooltip data with interval breakdown, percentages, and counts for each bucket.
    /// </summary>
    protected Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateTooltipData(ChartComputationResult result, BucketDistributionResult? frequencyData, int intervalCount = 25)
    {
        return DistributionComputationHelper.CalculateTooltipData(result, frequencyData, Configuration.BucketCount, intervalCount);
    }

    protected Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateSimpleRangeTooltipData(ChartComputationResult result, BucketDistributionResult? extendedResult)
    {
        return DistributionComputationHelper.CalculateSimpleRangeTooltipData(result, extendedResult, Configuration.BucketCount);
    }

    protected Dictionary<int, double> CalculateBucketAverages(BucketDistributionResult? extendedResult)
    {
        return DistributionComputationHelper.CalculateBucketAverages(extendedResult, Configuration.BucketCount);
    }
}
