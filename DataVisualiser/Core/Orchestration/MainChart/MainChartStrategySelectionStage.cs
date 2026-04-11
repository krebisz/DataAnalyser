using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed class MainChartStrategySelectionStage : IMainChartStrategySelectionStage
{
    private readonly StrategySelectionService _strategySelectionService;

    public MainChartStrategySelectionStage(StrategySelectionService strategySelectionService)
    {
        _strategySelectionService = strategySelectionService ?? throw new ArgumentNullException(nameof(strategySelectionService));
    }

    public MainChartStrategyPlan Select(MainChartPreparedData preparedData)
    {
        if (preparedData == null)
            throw new ArgumentNullException(nameof(preparedData));

        var labels = preparedData.Labels.ToList();
        var series = preparedData.Series.ToList();
        var (strategy, secondaryLabel) = _strategySelectionService.SelectComputationStrategy(
            series,
            labels,
            preparedData.WorkingContext,
            preparedData.WorkingContext.From,
            preparedData.WorkingContext.To);

        return new MainChartStrategyPlan(
            ResolveStrategyType(series.Count),
            preparedData.WorkingContext,
            strategy,
            labels[0],
            secondaryLabel,
            preparedData.IsStacked,
            preparedData.IsCumulative,
            preparedData.OverlaySeries);
    }

    private static StrategyType ResolveStrategyType(int seriesCount)
    {
        return seriesCount switch
        {
            > 2 => StrategyType.MultiMetric,
            2 => StrategyType.CombinedMetric,
            _ => StrategyType.SingleMetric
        };
    }
}
