namespace DataVisualiser.Core.Orchestration.MainChart;

public interface IMainChartPreparationStage
{
    Task<MainChartPreparedData> PrepareAsync(MainChartRenderRequest request);
}
