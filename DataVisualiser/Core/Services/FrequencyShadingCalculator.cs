using DataVisualiser.Core.Rendering.Shading;
using DataVisualiser.Shared.Helpers;

namespace DataVisualiser.Core.Services;

/// <summary>
///     Calculates frequency shading data for weekly distribution charts.
///     Extracted from WeeklyDistributionService to reduce complexity.
/// </summary>
public sealed class FrequencyShadingCalculator
{
    private readonly int _bucketCount;
    private readonly IIntervalShadingStrategy _shadingStrategy;

    public FrequencyShadingCalculator(IIntervalShadingStrategy shadingStrategy, int bucketCount)
    {
        _shadingStrategy = shadingStrategy ?? throw new ArgumentNullException(nameof(shadingStrategy));
        _bucketCount = bucketCount;
    }

    /// <summary>
    ///     Builds frequency shading data from day values and global min/max.
    /// </summary>
    public FrequencyShadingData BuildFrequencyShadingData(Dictionary<int, List<double>> dayValues, double globalMin, double globalMax, int intervalCount)
    {
        var intervals = FrequencyBinningHelper.CreateUniformIntervals(globalMin, globalMax, intervalCount);
        var frequencies = FrequencyBinningHelper.CountFrequenciesPerBucket(dayValues, intervals, _bucketCount);

        var context = new IntervalShadingContext
        {
                Intervals = intervals,
                FrequenciesPerBucket = frequencies,
                BucketValues = dayValues,
                GlobalMin = globalMin,
                GlobalMax = globalMax
        };

        var colorMap = _shadingStrategy.CalculateColorMap(context);

        return new FrequencyShadingData(intervals, frequencies, colorMap, dayValues);
    }
}
