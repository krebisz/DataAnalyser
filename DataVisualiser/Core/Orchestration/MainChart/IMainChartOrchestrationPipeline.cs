using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public interface IMainChartOrchestrationPipeline
{
    Task RenderAsync(MainChartRenderRequest request, CartesianChart chart);
}
