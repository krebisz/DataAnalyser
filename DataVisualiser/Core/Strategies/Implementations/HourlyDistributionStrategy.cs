using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Implementations;

/// <summary>
///     Computes per-hour-of-day min, max, counts, and frequency bins for a single metric series.
///     (0 - 23) ordering (12AM - 11PM).
/// </summary>
public sealed class HourlyDistributionStrategy : BucketDistributionStrategy
{
    public HourlyDistributionStrategy(IEnumerable<MetricData> data, string label, DateTime from, DateTime to, IUnitResolutionService? unitResolutionService = null) : base(data, label, from, to, unitResolutionService)
    {
    }

    protected override int BucketCount => 24;

    protected override int GetBucketIndex(MetricData data)
    {
        var hour = data.NormalizedTimestamp.Hour;

        // 12AM = 0 … 11PM = 23
        // Map hour (0-23) directly to bucket index (0-23)
        return hour;
    }

    protected override(List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> NormalizedFrequencies) PrepareBinsAndFrequencies(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        return HourlyFrequencyRenderer.PrepareBinsAndFrequencies(bucketValues, globalMin, globalMax);
    }
}