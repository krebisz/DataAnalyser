using DataVisualiser.Charts;

namespace DataVisualiser.Services.Abstractions;

/// <summary>
///     Factory interface for creating chart computation strategies.
/// </summary>
public interface IStrategyFactory
{
    /// <summary>
    ///     Creates a CMS strategy.
    /// </summary>
    IChartComputationStrategy CreateCmsStrategy(ChartDataContext ctx, StrategyCreationParameters parameters);

    /// <summary>
    ///     Creates a legacy strategy.
    /// </summary>
    IChartComputationStrategy CreateLegacyStrategy(StrategyCreationParameters parameters);
}