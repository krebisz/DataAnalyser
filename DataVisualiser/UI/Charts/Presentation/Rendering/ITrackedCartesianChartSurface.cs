using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Presentation.Rendering;

/// <summary>
///     Optional surface capability for renderers that produce a WPF CartesianChart
///     and want to expose that instance back to adapter code without tree walking.
/// </summary>
public interface ITrackedCartesianChartSurface
{
    CartesianChart? RenderedCartesianChart { get; }

    void SetRenderedCartesianChart(CartesianChart? chart);
}
