using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;

namespace DataVisualiser.Services.Implementations.Factories;

/// <summary>
///     Factory for creating Normalized strategies.
/// </summary>
public sealed class NormalizedStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        // TODO: Implement CMS Normalized strategy
        return CreateLegacyStrategy(parameters);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new NormalizedStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.Label2, parameters.From, parameters.To, parameters.NormalizationMode ?? NormalizationMode.PercentageOfMax);
    }
}