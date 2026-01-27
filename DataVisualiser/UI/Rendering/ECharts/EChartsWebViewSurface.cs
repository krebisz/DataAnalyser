using System;
using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI.Rendering.ECharts;

/// <summary>
///     Placeholder surface for a future WebView2-backed ECharts renderer.
///     For now, it behaves like a standard chart panel surface while keeping
///     the surface type distinct for resolver-based routing.
/// </summary>
public sealed class EChartsWebViewSurface : IChartSurface
{
    private readonly IChartPanelHost _panel;

    public EChartsWebViewSurface(IChartPanelHost panel)
    {
        _panel = panel ?? throw new ArgumentNullException(nameof(panel));
    }

    public void SetTitle(string? title)
    {
        _panel.SetTitle(title);
    }

    public void SetIsVisible(bool isVisible)
    {
        _panel.SetIsVisible(isVisible);
    }

    public void SetHeader(UIElement? header)
    {
        _panel.SetHeader(header);
    }

    public void SetBehavioralControls(UIElement? controls)
    {
        _panel.SetBehavioralControls(controls);
    }

    public void SetChartContent(UIElement? content)
    {
        // Keep content assignment centralized at the surface boundary.
        // A future WebView2 host can be mounted here.
        _panel.SetChartContent(content ?? new Grid());
    }
}
