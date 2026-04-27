using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.Distribution;

public interface IDistributionPolarProjectionInteraction : IDisposable
{
}

public interface IDistributionPolarProjectionInteractionFactory
{
    IDistributionPolarProjectionInteraction Create(
        CartesianChart chart,
        DistributionModeDefinition definition,
        DistributionRangeResult rangeResult);
}
