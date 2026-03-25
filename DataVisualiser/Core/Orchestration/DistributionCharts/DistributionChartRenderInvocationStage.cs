using LiveCharts.Wpf;

namespace DataVisualiser.Core.Orchestration.DistributionCharts;

public sealed class DistributionChartRenderInvocationStage : IDistributionChartRenderInvocationStage
{
    public Task RenderAsync(DistributionChartPreparedData preparedData, CartesianChart chart)
    {
        if (preparedData == null)
            throw new ArgumentNullException(nameof(preparedData));
        if (chart == null)
            throw new ArgumentNullException(nameof(chart));

        return preparedData.DistributionService.UpdateDistributionChartAsync(
            chart,
            preparedData.Data,
            preparedData.DisplayName,
            preparedData.From,
            preparedData.To,
            minHeight: 400,
            useFrequencyShading: preparedData.Settings.UseFrequencyShading,
            intervalCount: preparedData.Settings.IntervalCount,
            cmsSeries: preparedData.CmsSeries);
    }
}
