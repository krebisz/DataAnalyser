using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using UnitResolutionService = DataVisualiser.Core.Services.UnitResolutionService;

namespace DataVisualiser.Core.Strategies.Implementations;

/// <summary>
///     Computes per-day-of-week min, max, counts, and frequency bins for a single metric series.
///     Monday -> Sunday ordering.
/// </summary>
public sealed class WeeklyDistributionStrategy : BucketDistributionStrategy
{
    protected override int BucketCount => 7;

    public WeeklyDistributionStrategy(IEnumerable<MetricData> data, string label, DateTime from, DateTime to, IUnitResolutionService? unitResolutionService = null)
        : base(data, label, from, to, unitResolutionService)
    {
    }

    protected override int GetBucketIndex(MetricData data)
    {
        var dow = data.NormalizedTimestamp.DayOfWeek;

        // Monday = 0 … Sunday = 6
        var idx = dow == DayOfWeek.Sunday ? BucketCount - 1 : (int)dow - 1;

        return idx;
    }

    protected override (List<(double Min, double Max)> Bins, double BinSize, Dictionary<int, Dictionary<int, int>> Frequencies, Dictionary<int, Dictionary<int, double>> NormalizedFrequencies) PrepareBinsAndFrequencies(Dictionary<int, List<double>> bucketValues, double globalMin, double globalMax)
    {
        return WeeklyFrequencyRenderer.PrepareBinsAndFrequencies(bucketValues, globalMin, globalMax);
    }
}