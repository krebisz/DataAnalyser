using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IStackedChartController : IChartPanelControllerHost
{
    CartesianChart Chart { get; }

    ComboBox OverlaySubtypeCombo { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? OverlaySubtypeChanged;
}
