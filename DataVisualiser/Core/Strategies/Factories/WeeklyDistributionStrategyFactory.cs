using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;
using DataVisualiser.Core.Services.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating WeeklyDistribution strategies.
/// </summary>
public sealed class WeeklyDistributionStrategyFactory : IStrategyFactory
{
    public IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        return new CmsWeeklyDistributionStrategy(ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null"), parameters.From, parameters.To, parameters.Label1);
    }

    public IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new WeeklyDistributionStrategy(parameters.LegacyData1 ?? Array.Empty<HealthMetricData>(), parameters.Label1, parameters.From, parameters.To);
    }
}
