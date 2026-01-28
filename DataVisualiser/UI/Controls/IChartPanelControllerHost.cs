using System.Windows.Controls;

namespace DataVisualiser.UI.Controls;

public interface IChartPanelControllerHost
{
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
}
