using System.Windows;
using DataVisualiser.UI.Controls;

namespace DataVisualiser.UI.Rendering;

public sealed class ChartPanelSurface : IChartSurface
{
    private readonly ChartPanelController _panel;

    public ChartPanelSurface(ChartPanelController panel)
    {
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
    }

    public void SetTitle(string? title)
    {
        _panel.Title = title ?? string.Empty;
    }

    public void SetIsVisible(bool isVisible)
    {
        _panel.IsChartVisible = isVisible;
    }

    public void SetHeader(UIElement? header)
    {
        _panel.SetHeaderControls(header);
    }

    public void SetBehavioralControls(UIElement? controls)
    {
        _panel.SetBehavioralControls(controls);
    }

    public void SetChartContent(UIElement? content)
    {
        _panel.SetChartContent(content);
    }
}