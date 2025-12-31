using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;

namespace DataVisualiser.Services.Implementations.Factories
{
    /// <summary>
    /// Factory for creating MultiMetric strategies.
    /// </summary>
    public sealed class MultiMetricStrategyFactory : IStrategyFactory
    {
        public IChartComputationStrategy CreateCmsStrategy(
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            // TODO: Implement CMS MultiMetric strategy
            return CreateLegacyStrategy(parameters);
        }

        public IChartComputationStrategy CreateLegacyStrategy(
            StrategyCreationParameters parameters)
        {
            return new MultiMetricStrategy(
                parameters.LegacySeries ?? Array.Empty<IEnumerable<HealthMetricData>>(),
                parameters.Labels ?? Array.Empty<string>(),
                parameters.From,
                parameters.To,
                parameters.Unit);
        }
    }
}

