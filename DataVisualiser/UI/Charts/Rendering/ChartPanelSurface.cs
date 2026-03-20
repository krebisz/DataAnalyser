using System.Windows;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Rendering;

public sealed class ChartPanelSurface : IChartSurface, ITrackedCartesianChartSurface
{
    private readonly IChartPanelHost _panel;
    private CartesianChart? _renderedCartesianChart;

    public ChartPanelSurface(IChartPanelHost panel)
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
        _panel.SetChartContent(content);
    }

    public CartesianChart? RenderedCartesianChart => _renderedCartesianChart;

    public void SetRenderedCartesianChart(CartesianChart? chart)
    {
        _renderedCartesianChart = chart;
    }
}
