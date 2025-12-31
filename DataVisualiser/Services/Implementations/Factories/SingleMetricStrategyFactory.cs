using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;
using DataVisualiser.State;

namespace DataVisualiser.Services.Implementations.Factories
{
    /// <summary>
    /// Factory for creating SingleMetric strategies.
    /// </summary>
    public sealed class SingleMetricStrategyFactory : IStrategyFactory
    {
        public IChartComputationStrategy CreateCmsStrategy(
            ChartDataContext ctx,
            StrategyCreationParameters parameters)
        {
            return new SingleMetricCmsStrategy(
                ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"),
                parameters.Label1,
                parameters.From,
                parameters.To);
        }

        public IChartComputationStrategy CreateLegacyStrategy(
            StrategyCreationParameters parameters)
        {
            return new SingleMetricLegacyStrategy(
                parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(),
                parameters.Label1,
                parameters.From,
                parameters.To);
        }
    }
}

