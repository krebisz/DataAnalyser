using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating MultiMetric strategies.
/// </summary>
public sealed class MultiMetricStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        // TODO: Implement CMS MultiMetric strategy
        return CreateLegacyStrategy(parameters);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new MultiMetricStrategy(parameters.LegacySeries ?? Array.Empty<IEnumerable<HealthMetricData>>(), parameters.Labels ?? Array.Empty<string>(), parameters.From, parameters.To, parameters.Unit);
    }
}
