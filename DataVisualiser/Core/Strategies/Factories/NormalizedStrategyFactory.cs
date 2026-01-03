using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Normalized strategies.
/// </summary>
public sealed class NormalizedStrategyFactory : StrategyFactoryBase
{
    public NormalizedStrategyFactory() : base((ctx, p) => CreateLegacy(p), // TODO: Implement CMS Normalized strategy
            CreateLegacy)
    {
    }

    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p)
    {
        return new NormalizedStrategy(p.LegacyData1 ?? Array.Empty<HealthMetricData>(), p.LegacyData2 ?? Array.Empty<HealthMetricData>(), p.Label1, p.Label2, p.From, p.To, p.NormalizationMode ?? NormalizationMode.PercentageOfMax);
    }
}