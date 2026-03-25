using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public interface IDistributionChartOrchestrationPipeline
{
    Task RenderAsync(DistributionChartOrchestrationRequest request, CartesianChart chart);
}
