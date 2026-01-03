using DataVisualiser.Core.Orchestration;
using DataVisualiser.Validation;

namespace DataVisualiser.Core.Strategies.Abstractions;

/// <summary>
///     Unified cut-over mechanism for all strategies.
///     Single decision point for legacy vs CMS strategy selection.
/// </summary>
public interface IStrategyCutOverService
{
    /// <summary>
    ///     Creates a strategy with unified cut-over logic and parity validation.
    /// </summary>
    IChartComputationStrategy CreateStrategy(StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters);

    /// <summary>
    ///     Determines if CMS should be used for a strategy.
    /// </summary>
    bool ShouldUseCms(StrategyType strategyType, ChartDataContext ctx);

    /// <summary>
    ///     Executes parity validation between legacy and CMS strategies.
    /// </summary>
    ParityResult ValidateParity(IChartComputationStrategy legacyStrategy, IChartComputationStrategy cmsStrategy);
}
