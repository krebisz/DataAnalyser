using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI.Controls;

public interface IChartPanelScaffold
{
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
    string Title { get; set; }
    bool IsChartVisible { get; set; }
    IChartRenderingContext? RenderingContext { get; set; }
    event EventHandler? ToggleRequested;

    void SetHeaderControls(UIElement? controls);
    void SetBehavioralControls(UIElement? controls);
    void SetChartContent(UIElement? chart);
}