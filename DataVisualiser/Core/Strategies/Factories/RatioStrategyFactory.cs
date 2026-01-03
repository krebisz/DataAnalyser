using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating Ratio strategies.
/// </summary>
public sealed class RatioStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        // TODO: Implement CMS Ratio strategy
        return CreateLegacyStrategy(parameters);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new RatioStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.Label2, parameters.From, parameters.To);
    }
}
