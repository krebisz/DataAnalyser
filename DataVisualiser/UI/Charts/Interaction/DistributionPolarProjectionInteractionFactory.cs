using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Interaction;

public sealed class DistributionPolarProjectionInteractionFactory : IDistributionPolarProjectionInteractionFactory
{
    public IDistributionPolarProjectionInteraction Create(
        CartesianChart chart,
        DistributionModeDefinition definition,
        DistributionRangeResult rangeResult)
    {
        return new DistributionPolarProjectionTooltip(chart, definition, rangeResult);
    }
}
