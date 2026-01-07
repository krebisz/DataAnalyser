using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating WeekdayTrend strategies.
/// </summary>
public sealed class WeekdayTrendStrategyFactory : StrategyFactoryBase
{
    public override IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        var cms = ctx.PrimaryCms as ICanonicalMetricSeries ?? throw new InvalidOperationException("PrimaryCms is null");
        return new WeekdayTrendComputationStrategy(cms, parameters.Label1, parameters.From, parameters.To);
    }

    public override IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters)
    {
        return new WeekdayTrendComputationStrategy(parameters.LegacyData1 ?? Array.Empty<MetricData>(), parameters.Label1, parameters.From, parameters.To);
    }
}
