using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation;

public interface ICartesianChartControllerHost : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
}
