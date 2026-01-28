using System.Windows.Controls;

namespace DataVisualiser.UI.Charts.Interfaces;

/// <summary>
///     WPF-only escape hatch for view code that still needs access to a chart's
///     content panel. Kept separate from the main controller contract.
/// </summary>
public interface IWpfChartPanelHost
{
    Panel ChartContentPanel { get; }
}
