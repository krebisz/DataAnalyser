using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Difference strategies.
/// </summary>
public sealed class DifferenceStrategyFactory : StrategyFactoryBase
{
    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p) =>
        new DifferenceStrategy(
            p.LegacyData1 ?? Array.Empty<HealthMetricData>(),
            p.LegacyData2 ?? Array.Empty<HealthMetricData>(),
            p.Label1,
            p.Label2,
            p.From,
            p.To);

    public DifferenceStrategyFactory()
        : base(
            cmsFactory: (ctx, p) => CreateLegacy(p), // TODO: Implement CMS Difference strategy
            legacyFactory: CreateLegacy)
    {
    }
}
