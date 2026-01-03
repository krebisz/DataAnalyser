using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Ratio strategies.
/// </summary>
public sealed class RatioStrategyFactory : StrategyFactoryBase
{
    public RatioStrategyFactory() : base((ctx, p) => CreateLegacy(p), // TODO: Implement CMS Ratio strategy
            CreateLegacy)
    {
    }

    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p)
    {
        return new RatioStrategy(p.LegacyData1 ?? Array.Empty<HealthMetricData>(), p.LegacyData2 ?? Array.Empty<HealthMetricData>(), p.Label1, p.Label2, p.From, p.To);
    }
}