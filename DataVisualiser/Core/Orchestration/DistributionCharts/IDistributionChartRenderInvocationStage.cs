using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public interface IDistributionChartRenderInvocationStage
{
    Task RenderAsync(DistributionChartPreparedData preparedData, CartesianChart chart);
}
