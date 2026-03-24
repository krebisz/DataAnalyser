using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Ratio strategies.
/// </summary>
public sealed class RatioStrategyFactory : StrategyFactoryBase
{
    public RatioStrategyFactory() : base((ctx, p) => CreateCms(ctx, p),
            CreateLegacy)
    {
    }

    private static IChartComputationStrategy CreateCms(ChartDataContext ctx, StrategyCreationParameters p)
    {
        return new RatioStrategy(
            ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
            ctx.SecondaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("SecondaryCms is null"),
            p.Label1,
            p.Label2,
            p.From,
            p.To);
    }

    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p)
    {
        return new RatioStrategy(p.LegacyData1 ?? Array.Empty<MetricData>(), p.LegacyData2 ?? Array.Empty<MetricData>(), p.Label1, p.Label2, p.From, p.To);
    }
}
