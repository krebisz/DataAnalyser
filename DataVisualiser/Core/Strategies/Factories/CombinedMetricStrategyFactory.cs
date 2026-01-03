using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating CombinedMetric strategies.
///     Uses unified CombinedMetricStrategy for both CMS and Legacy data.
/// </summary>
public sealed class CombinedMetricStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        return new CombinedMetricStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), ctx.SecondaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("SecondaryCms is null"), parameters.Label1, parameters.Label2, parameters.From, parameters.To);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new CombinedMetricStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.LegacyData2 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.Label2, parameters.From, parameters.To);
    }
}
