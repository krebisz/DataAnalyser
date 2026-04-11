using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public interface IDistributionChartOrchestrationPipeline
{
    Task RenderAsync(DistributionChartOrchestrationRequest request, CartesianChart chart);
}

public interface IDistributionChartPreparationStage
{
    DistributionChartPreparedData Prepare(DistributionChartOrchestrationRequest request);
}

public interface IDistributionChartRenderInvocationStage
{
    Task RenderAsync(DistributionChartPreparedData preparedData, CartesianChart chart);
}
