using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface ICartesianChartControllerHost : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
}
