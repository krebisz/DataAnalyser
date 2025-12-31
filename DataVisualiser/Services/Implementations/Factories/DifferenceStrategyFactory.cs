using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;

namespace DataVisualiser.Services.Implementations.Factories
{
    /// <summary>
    /// Factory for creating Difference strategies.
    /// </summary>
    public sealed class DifferenceStrategyFactory : IStrategyFactory
    {
        public IChartComputationStrategy CreateCmsStrategy(
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            // TODO: Implement CMS Difference strategy
            return CreateLegacyStrategy(parameters);
        }

        public IChartComputationStrategy CreateLegacyStrategy(
            StrategyCreationParameters parameters)
        {
            return new DifferenceStrategy(
                parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(),
                parameters.Label1,
                parameters.Label2,
                parameters.From,
                parameters.To);
        }
    }
}

