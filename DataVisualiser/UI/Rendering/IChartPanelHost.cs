using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI.Rendering;

/// <summary>
///     Minimal host contract for chart panel surfaces to target without depending
///     on the concrete ChartPanelController type.
/// </summary>
public interface IChartPanelHost
{
    void SetTitle(string? title);
    void SetIsVisible(bool isVisible);
    void SetHeader(UIElement? header);
    void SetBehavioralControls(UIElement? controls);
    void SetChartContent(UIElement? content);
    Panel ChartContentPanel { get; }
}
