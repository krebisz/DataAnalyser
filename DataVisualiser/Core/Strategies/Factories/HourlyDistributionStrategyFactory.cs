using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating HourlyDistribution strategies.
/// </summary>
public sealed class HourlyDistributionStrategyFactory : StrategyFactoryBase
{
    public HourlyDistributionStrategyFactory() : base((ctx, p) => new CmsHourlyDistributionStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), p.From, p.To, p.Label1), p => new HourlyDistributionStrategy(p.LegacyData1 ?? Array.Empty<MetricData>(), p.Label1, p.From, p.To))
    {
    }
}