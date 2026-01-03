using DataFileReader.Canonical;
using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Models;
using DataVisualiser.Services.Abstractions;

namespace DataVisualiser.Services.Implementations.Factories;

/// <summary>
///     Factory for creating CombinedMetric strategies.
/// </summary>
public sealed class CombinedMetricStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        return new CombinedMetricCmsStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), ctx.SecondaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("SecondaryCms is null"), parameters.Label1, parameters.Label2, parameters.From, parameters.To);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new CombinedMetricStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.Label2, parameters.From, parameters.To);
    }
}