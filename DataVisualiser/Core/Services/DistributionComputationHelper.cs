using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

internal static class DistributionComputationHelper
{
    public static Dictionary<int, List<double>> GetBucketValues(BucketDistributionResult? frequencyData, int bucketCount)
    {
        var bucketValues = new Dictionary<int, List<double>>(bucketCount);

        for (var i = 0; i < bucketCount; i++)
            bucketValues[i] = frequencyData?.BucketValues?.TryGetValue(i, out var values) == true ? values : [];

        return bucketValues;
    }

    public static (double Min, double Max) CalculateGlobalMinMax(List<double> mins, List<double> ranges)
    {
        var min = mins.Where(m => !double.IsNaN(m)).DefaultIfEmpty(0).Min();
        var max = mins
            .Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r)
            .Where(v => !double.IsNaN(v))
            .DefaultIfEmpty(min + 1)
            .Max();

        if (max <= min)
            max = min + 1;

        return (min, max);
    }

    public static Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateTooltipData(
        ChartComputationResult result,
        BucketDistributionResult? frequencyData,
        int bucketCount,
        int intervalCount)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || frequencyData == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != bucketCount || ranges.Count != bucketCount)
            return tooltipData;

        var bucketValues = GetBucketValues(frequencyData, bucketCount);
        var globalMin = mins.Where(m => !double.IsNaN(m)).DefaultIfEmpty(0).Min();
        var globalMax = mins.Zip(ranges, (m, r) => double.IsNaN(m) || double.IsNaN(r) ? double.NaN : m + r).Where(v => !double.IsNaN(v)).DefaultIfEmpty(1).Max();

        if (globalMax <= globalMin)
            globalMax = globalMin + 1;

        var intervals = FrequencyBinningHelper.CreateUniformIntervals(globalMin, globalMax, intervalCount);
        var frequenciesPerBucket = FrequencyBinningHelper.CountFrequenciesPerBucket(bucketValues, intervals, bucketCount);

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var bucketIntervals = new List<(double Min, double Max, int Count, double Percentage)>();
            var totalCount = bucketValues.TryGetValue(bucketIndex, out var values) ? values.Count : 0;
            var bucketMin = double.IsNaN(mins[bucketIndex]) ? 0.0 : mins[bucketIndex];
            var bucketMax = bucketMin + (double.IsNaN(ranges[bucketIndex]) ? 0.0 : ranges[bucketIndex]);

            for (var intervalIndex = 0; intervalIndex < intervals.Count; intervalIndex++)
            {
                var interval = intervals[intervalIndex];
                var overlaps = interval.Min < bucketMax && interval.Max > bucketMin;

                if (!overlaps || ranges[bucketIndex] <= 0 || double.IsNaN(ranges[bucketIndex]))
                    continue;

                var count = 0;
                if (frequenciesPerBucket.TryGetValue(bucketIndex, out var bucketFreqs) && bucketFreqs.TryGetValue(intervalIndex, out var freq))
                    count = freq;

                var percentage = totalCount > 0 ? (double)count / totalCount * 100.0 : 0.0;
                bucketIntervals.Add((interval.Min, interval.Max, count, percentage));
            }

            tooltipData[bucketIndex] = bucketIntervals;
        }

        return tooltipData;
    }

    public static Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>> CalculateSimpleRangeTooltipData(
        ChartComputationResult result,
        BucketDistributionResult? extendedResult,
        int bucketCount)
    {
        var tooltipData = new Dictionary<int, List<(double Min, double Max, int Count, double Percentage)>>();

        if (result == null || extendedResult == null)
            return tooltipData;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;

        if (mins == null || ranges == null || mins.Count != bucketCount || ranges.Count != bucketCount)
            return tooltipData;

        for (var bucketIndex = 0; bucketIndex < bucketCount; bucketIndex++)
        {
            var bucketIntervals = new List<(double Min, double Max, int Count, double Percentage)>();

            if (double.IsNaN(mins[bucketIndex]))
                continue;

            var bucketMin = mins[bucketIndex];
            var bucketRange = double.IsNaN(ranges[bucketIndex]) ? 0.0 : ranges[bucketIndex];
            var bucketMax = bucketMin + bucketRange;
            var count = bucketIndex < extendedResult.Counts.Count ? extendedResult.Counts[bucketIndex] : 0;

            if (count > 0)
                bucketIntervals.Add((bucketMin, bucketMax, count, 100.0));

            if (bucketIntervals.Count > 0)
                tooltipData[bucketIndex] = bucketIntervals;
        }

        return tooltipData;
    }

    public static Dictionary<int, double> CalculateBucketAverages(BucketDistributionResult? extendedResult, int bucketCount)
    {
        var bucketValues = GetBucketValues(extendedResult, bucketCount);
        var averages = new Dictionary<int, double>(bucketCount);

        for (var i = 0; i < bucketCount; i++)
        {
            var values = bucketValues.TryGetValue(i, out var bucket) ? bucket : [];
            var validValues = values.Where(v => !double.IsNaN(v)).ToList();
            averages[i] = validValues.Count > 0 ? validValues.Average() : double.NaN;
        }

        return averages;
    }
}
