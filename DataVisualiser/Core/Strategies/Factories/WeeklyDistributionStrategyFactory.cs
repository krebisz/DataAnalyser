using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating WeeklyDistribution strategies.
/// </summary>
public sealed class WeeklyDistributionStrategyFactory : StrategyFactoryBase
{
    public WeeklyDistributionStrategyFactory() : base((ctx, p) => new CmsWeeklyDistributionStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), p.From, p.To, p.Label1), p => new WeeklyDistributionStrategy(p.LegacyData1 ?? Array.Empty<HealthMetricData>(), p.Label1, p.From, p.To))
    {
    }
}