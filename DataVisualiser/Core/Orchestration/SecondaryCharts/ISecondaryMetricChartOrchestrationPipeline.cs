using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public interface ISecondaryMetricChartOrchestrationPipeline
{
    Task RenderAsync(SecondaryMetricChartRenderRequest request, CartesianChart chart);
}

public interface ISecondaryMetricChartStrategySelectionStage
{
    SecondaryMetricChartStrategyPlan Select(SecondaryMetricChartRenderRequest request);
}

public interface ISecondaryMetricChartRenderInvocationStage
{
    Task RenderAsync(SecondaryMetricChartStrategyPlan plan, SecondaryMetricChartRenderRequest request, CartesianChart chart);
}
