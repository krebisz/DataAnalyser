using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public interface ISecondaryMetricChartRenderInvocationStage
{
    Task RenderAsync(SecondaryMetricChartStrategyPlan plan, SecondaryMetricChartRenderRequest request, CartesianChart chart);
}
