using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public interface IMainChartRenderInvocationStage
{
    Task RenderAsync(MainChartStrategyPlan plan, CartesianChart chart);
}
