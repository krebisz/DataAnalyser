namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public interface ISecondaryMetricChartStrategySelectionStage
{
    SecondaryMetricChartStrategyPlan Select(SecondaryMetricChartRenderRequest request);
}
