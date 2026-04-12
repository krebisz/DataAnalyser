using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Services;

internal static class DistributionRangeResultBuilder
{
    public static DistributionRangeResult? Build(ChartComputationResult? result, BucketDistributionResult? extendedResult, int bucketCount)
    {
        if (result?.PrimaryRawValues == null || result.PrimarySmoothed == null)
            return null;

        var mins = result.PrimaryRawValues;
        var ranges = result.PrimarySmoothed;
        if (mins.Count != bucketCount || ranges.Count != bucketCount)
            return null;

        var maxs = mins.Zip(ranges,
                (min, range) =>
                {
                    if (double.IsNaN(min))
                        return double.NaN;

                    if (double.IsNaN(range))
                        range = 0.0;

                    return min + range;
                })
            .ToList();

        var globalMin = mins.Where(m => !double.IsNaN(m)).DefaultIfEmpty(0.0).Min();
        var globalMax = maxs.Where(m => !double.IsNaN(m)).DefaultIfEmpty(globalMin + 1.0).Max();
        if (globalMax <= globalMin)
            globalMax = globalMin + 1.0;

        var averages = BuildAverages(extendedResult, bucketCount);
        return new DistributionRangeResult(mins, maxs, averages, globalMin, globalMax, result.Unit);
    }

    private static List<double> BuildAverages(BucketDistributionResult? extendedResult, int bucketCount)
    {
        var averages = new List<double>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            if (extendedResult?.BucketValues.TryGetValue(i, out var values) == true)
            {
                var validValues = values.Where(v => !double.IsNaN(v)).ToList();
                averages.Add(validValues.Count > 0 ? validValues.Average() : double.NaN);
            }
            else
            {
                averages.Add(double.NaN);
            }
        }

        return averages;
    }
}
