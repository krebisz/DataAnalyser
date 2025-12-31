using DataVisualiser.Charts;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;

namespace DataVisualiser.Services.Implementations.Factories
{
    /// <summary>
    /// Factory for creating WeekdayTrend strategies.
    /// </summary>
    public sealed class WeekdayTrendStrategyFactory : IStrategyFactory
    {
        public IChartComputationStrategy CreateCmsStrategy(
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            // TODO: Implement CMS WeekdayTrend strategy
            return CreateLegacyStrategy(parameters);
        }

        public IChartComputationStrategy CreateLegacyStrategy(
            StrategyCreationParameters parameters)
        {
            // TODO: Implement WeekdayTrend strategy creation
            throw new NotSupportedException("WeekdayTrend strategy is not yet implemented");
        }
    }
}

