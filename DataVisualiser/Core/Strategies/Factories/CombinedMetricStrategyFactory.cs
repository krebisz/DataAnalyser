using DataFileReader.Canonical;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating CombinedMetric strategies.
///     Uses unified CombinedMetricStrategy for both CMS and Legacy data.
/// </summary>
public sealed class CombinedMetricStrategyFactory : StrategyFactoryBase
{
    public CombinedMetricStrategyFactory() : base((ctx, p) => new CombinedMetricStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), ctx.SecondaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("SecondaryCms is null"), p.Label1, p.Label2, p.From, p.To), p => new CombinedMetricStrategy(p.LegacyData1 ?? Array.Empty<MetricData>(), p.LegacyData2 ?? Array.Empty<MetricData>(), p.Label1, p.Label2, p.From, p.To))
    {
    }
}