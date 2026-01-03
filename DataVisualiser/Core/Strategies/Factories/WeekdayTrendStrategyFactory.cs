using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Strategies.Factories;

/// <summary>
///     Factory for creating WeekdayTrend strategies.
/// </summary>
public sealed class WeekdayTrendStrategyFactory : StrategyFactoryBase
{
    private static IChartComputationStrategy CreateLegacy(StrategyCreationParameters p) =>
        throw new NotSupportedException("WeekdayTrend strategy is not yet implemented"); // TODO: Implement WeekdayTrend strategy creation

    public WeekdayTrendStrategyFactory()
        : base(
            cmsFactory: (ctx, p) => CreateLegacy(p), // TODO: Implement CMS WeekdayTrend strategy
            legacyFactory: CreateLegacy)
    {
    }
}
