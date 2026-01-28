using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

/// <summary>
///     WPF-only escape hatch for views that still need access to a chart's
///     CartesianChart instance.
/// </summary>
public interface IWpfCartesianChartHost
{
    CartesianChart Chart { get; }
}
