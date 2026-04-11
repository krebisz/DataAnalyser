using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.MainChart;

public interface IMainChartOrchestrationPipeline
{
    Task<MainChartPreparedData> RenderAsync(MainChartRenderRequest request, CartesianChart chart);
}

public interface IMainChartPreparationStage
{
    Task<MainChartPreparedData> PrepareAsync(MainChartRenderRequest request);
}

public interface IMainChartStrategySelectionStage
{
    MainChartStrategyPlan Select(MainChartPreparedData preparedData);
}

public interface IMainChartRenderInvocationStage
{
    Task RenderAsync(MainChartStrategyPlan plan, CartesianChart chart);
}
