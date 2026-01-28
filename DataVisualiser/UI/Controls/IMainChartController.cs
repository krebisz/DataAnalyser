using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public interface IMainChartController
{
    CartesianChart Chart { get; }
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
    RadioButton DisplayRegularRadio { get; }
    RadioButton DisplaySummedRadio { get; }
    RadioButton DisplayStackedRadio { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? DisplayModeChanged;
}
