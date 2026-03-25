namespace DataVisualiser.Core.Orchestration.MainChart;

public interface IMainChartStrategySelectionStage
{
    MainChartStrategyPlan Select(MainChartPreparedData preparedData);
}
