using DataFileReader.Canonical;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Implementations;

public sealed class CmsHourlyDistributionStrategy : CmsBucketDistributionStrategy
{
    public CmsHourlyDistributionStrategy(ICanonicalMetricSeries series, DateTime from, DateTime to, string label, IUnitResolutionService? unitResolutionService = null) : base(series, from, to, label, unitResolutionService)
    {
    }

    protected override int BucketCount => 24;

    protected override int GetBucketIndex(DateTime timestamp)
    {
        // 12AM = 0 ... 11PM = 23
        // Map hour (0-23) directly to bucket index (0-23)
        return timestamp.Hour;
    }

    protected override(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> Normalized) PrepareFrequencyData(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        if (double.IsNaN(globalMin) || double.IsNaN(globalMax) || globalMax <= globalMin)
            return (new List<(double, double)>(), 0d, new Dictionary<int, Dictionary<int, int>>(), new Dictionary<int, Dictionary<int, double>>());

        return HourlyFrequencyRenderer.PrepareBinsAndFrequencies(bucketValues, globalMin, globalMax);
    }
}