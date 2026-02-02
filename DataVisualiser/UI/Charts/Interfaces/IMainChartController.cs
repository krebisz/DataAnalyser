using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IMainChartController : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
    RadioButton DisplayRegularRadio { get; }
    RadioButton DisplaySummedRadio { get; }
    RadioButton DisplayStackedRadio { get; }
    ComboBox OverlaySubtypeCombo { get; }
    StackPanel OverlaySubtypePanel { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? DisplayModeChanged;
    event EventHandler? OverlaySubtypeChanged;
}
