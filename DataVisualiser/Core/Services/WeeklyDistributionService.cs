using DataFileReader.Canonical;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Builds a Monday->Sunday min/max stacked column chart for a single metric.
///     Baseline (transparent) = min per day, range column = (max - min) per day.
/// </summary>
public class WeeklyDistributionService : BaseDistributionService
{
    public WeeklyDistributionService(Dictionary<CartesianChart, List<DateTime>> chartTimestamps, IStrategyCutOverService strategyCutOverService, IIntervalShadingStrategy? shadingStrategy = null)
        : base(new WeeklyDistributionConfiguration(), chartTimestamps, strategyCutOverService, shadingStrategy)
    {
    }

    protected override async Task<(ChartComputationResult? Result, BucketDistributionResult? ExtendedResult)> ComputeDistributionAsync(
        IEnumerable<MetricData> data,
        ICanonicalMetricSeries? cmsSeries,
        string displayName,
        DateTime from,
        DateTime to,
        bool useCmsStrategy,
        bool enableParity)
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

        var strategy = _strategyCutOverService.CreateStrategy(Configuration.StrategyType, ctx, parameters);

        // Compute result
        var result = strategy.Compute();

        // Extract ExtendedResult (WeeklyDistribution strategies have this property)
        BucketDistributionResult? extendedResult = null;
        if (strategy is WeeklyDistributionStrategy legacyStrategy)
            extendedResult = legacyStrategy.ExtendedResult;
        else if (strategy is CmsWeeklyDistributionStrategy cmsStrategy)
            extendedResult = cmsStrategy.ExtendedResult;

        return await Task.FromResult((result, extendedResult));
    }

    protected override Dictionary<int, List<double>> GetBucketValuesFromResult(BucketDistributionResult? frequencyData)
    {
        var bucketValues = new Dictionary<int, List<double>>(Configuration.BucketCount);

        for (var i = 0; i < Configuration.BucketCount; i++)
            bucketValues[i] = frequencyData?.BucketValues?.TryGetValue(i, out var values) == true ? values : new List<double>();

        return bucketValues;
    }

    protected override void SetupTooltip(
        CartesianChart targetChart,
        ChartComputationResult result,
        BucketDistributionResult extendedResult,
        bool useFrequencyShading,
        int intervalCount)
    {
        Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> tooltipData;
        if (useFrequencyShading)
            tooltipData = CalculateTooltipData(result, extendedResult, intervalCount);
        else
            tooltipData = CalculateSimpleRangeTooltipData(result, extendedResult);

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

    protected override IIntervalRenderer CreateIntervalRenderer()
    {
        return new WeeklyIntervalRenderer();
    }

    private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateSimpleRangeTooltipData(ChartComputationResult result, BucketDistributionResult? extendedResult)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || extendedResult == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != Configuration.BucketCount || ranges.Count != Configuration.BucketCount)
            return tooltipData;

        // For each bucket, create a single "interval" representing the entire bucket's range
        for (var bucketIndex = 0; bucketIndex < Configuration.BucketCount; bucketIndex++)
        {
            var bucketIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

            // Check if we have valid min value (not NaN)
            if (double.IsNaN(mins[bucketIndex]))
                continue; // Skip buckets with invalid min values

            var bucketMin = mins[bucketIndex];
            var bucketRange = double.IsNaN(ranges[bucketIndex]) ? 0.0 : ranges[bucketIndex];
            var bucketMax = bucketMin + bucketRange;

            // Get count for this bucket
            var count = 0;
            if (bucketIndex < extendedResult.Counts.Count)
                count = extendedResult.Counts[bucketIndex];

            // Add interval if there's valid data (count > 0 and valid min)
            // Note: bucketRange can be 0 (all values for the bucket are the same), which is valid
            if (count > 0)
                    // Single interval representing the entire bucket's range
                    // Percentage is 100% since this is the only interval for the bucket
                bucketIntervals.Add((bucketMin, bucketMax, count, 100.0));

            if (bucketIntervals.Count > 0)
                tooltipData[bucketIndex] = bucketIntervals;
        }

        return tooltipData;
    }

    /// <summary>
    ///     Calculates tooltip data with interval breakdown, percentages, and counts for each bucket.
    /// </summary>
    private Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateTooltipData(ChartComputationResult result, BucketDistributionResult? frequencyData, int intervalCount = 25)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || frequencyData == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != Configuration.BucketCount || ranges.Count != Configuration.BucketCount)
            return tooltipData;

        // Get bucket values
        var bucketValues = GetBucketValuesFromResult(frequencyData);

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

        // Count frequencies per interval per bucket
        var frequenciesPerBucket = _frequencyShadingCalculator.CountFrequenciesPerInterval(bucketValues, intervals);

        // Calculate percentages for each bucket
        for (var bucketIndex = 0; bucketIndex < Configuration.BucketCount; bucketIndex++)
        {
            var bucketIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

            // Get total count for this bucket
            var totalCount = 0;
            if (bucketValues.TryGetValue(bucketIndex, out var values))
                totalCount = values.Count;

            // Calculate bucket min/max to determine which intervals are within the bucket's range
            var bucketMin = double.IsNaN(mins[bucketIndex]) ? 0.0 : mins[bucketIndex];
            var bucketMax = bucketMin + (double.IsNaN(ranges[bucketIndex]) ? 0.0 : ranges[bucketIndex]);

            // For each interval, calculate count and percentage
            for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var interval = intervals[intervalIndex];

                // Check if interval overlaps with bucket's range
                var intervalOverlapsBucketRange = interval.Min < bucketMax && interval.Max > bucketMin;

                if (intervalOverlapsBucketRange && ranges[bucketIndex] > 0 && !double.IsNaN(ranges[bucketIndex]))
                {
                    // Get frequency for this interval
                    var count = 0;
                    if (frequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(intervalIndex, out var freq))
                        count = freq;

                    // Calculate percentage (percentage of total values for this bucket)
                    var percentage = totalCount > 0 ? (double)count / totalCount * 100.0 : 0.0;

                    bucketIntervals.Add((interval.Min, interval.Max, count, percentage));
                }
            }

            tooltipData[bucketIndex] = bucketIntervals;
        }

        return tooltipData;
    }
}
