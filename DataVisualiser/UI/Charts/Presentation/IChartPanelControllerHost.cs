using System.Windows.Controls;
using DataVisualiser.UI.Charts.Controllers;

namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartPanelControllerHost
{
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
}
