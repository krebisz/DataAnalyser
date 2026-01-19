using System.Windows;

namespace DataVisualiser.UI.Rendering;

public interface IChartSurface
{
    void SetTitle(string? title);
    void SetIsVisible(bool isVisible);
    void SetHeader(UIElement? header);
    void SetBehavioralControls(UIElement? controls);
    void SetChartContent(UIElement? content);
}
