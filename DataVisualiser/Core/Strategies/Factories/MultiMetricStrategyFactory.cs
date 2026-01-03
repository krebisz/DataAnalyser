using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating MultiMetric strategies.
/// </summary>
public sealed class MultiMetricStrategyFactory : StrategyFactoryBase
{
    public MultiMetricStrategyFactory() : base((ctx, p) => CreateLegacy(p), // TODO: Implement CMS MultiMetric strategy
            CreateLegacy)
    {
    }

    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p)
    {
        return new MultiMetricStrategy(p.LegacySeries ?? Array.Empty<IEnumerable<HealthMetricData>>(), p.Labels ?? Array.Empty<string>(), p.From, p.To, p.Unit);
    }
}