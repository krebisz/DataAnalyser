namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public interface IDistributionChartPreparationStage
{
    DistributionChartPreparedData Prepare(DistributionChartOrchestrationRequest request);
}
