using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating SingleMetric strategies.
///     Uses unified SingleMetricStrategy for both CMS and Legacy data.
/// </summary>
public sealed class SingleMetricStrategyFactory : StrategyFactoryBase
{
    public SingleMetricStrategyFactory()
        : base(
            cmsFactory: (ctx, p) => new SingleMetricStrategy(
                ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
                p.Label1,
                p.From,
                p.To),
            legacyFactory: p => new SingleMetricStrategy(
                p.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                p.Label1,
                p.From,
                p.To))
    {
    }
}
