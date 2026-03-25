using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public interface ISecondaryMetricChartOrchestrationPipeline
{
    Task RenderAsync(SecondaryMetricChartRenderRequest request, CartesianChart chart);
}
