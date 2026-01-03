using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating SingleMetric strategies.
///     Uses unified SingleMetricStrategy for both CMS and Legacy data.
/// </summary>
public sealed class SingleMetricStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        return new SingleMetricStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), parameters.Label1, parameters.From, parameters.To);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new SingleMetricStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.From, parameters.To);
    }
}
