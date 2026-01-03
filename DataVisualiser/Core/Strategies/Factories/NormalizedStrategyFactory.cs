using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Normalized strategies.
/// </summary>
public sealed class NormalizedStrategyFactory : StrategyFactoryBase
{
    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p) =>
        new NormalizedStrategy(
            p.LegacyData1 ?? Array.Empty<HealthMetricData>(),
            p.LegacyData2 ?? Array.Empty<HealthMetricData>(),
            p.Label1,
            p.Label2,
            p.From,
            p.To,
            p.NormalizationMode ?? NormalizationMode.PercentageOfMax);

    public NormalizedStrategyFactory()
        : base(
            cmsFactory: (ctx, p) => CreateLegacy(p), // TODO: Implement CMS Normalized strategy
            legacyFactory: CreateLegacy)
    {
    }
}
