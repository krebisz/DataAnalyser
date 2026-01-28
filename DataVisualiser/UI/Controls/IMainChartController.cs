using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public interface IMainChartController : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
    RadioButton DisplayRegularRadio { get; }
    RadioButton DisplaySummedRadio { get; }
    RadioButton DisplayStackedRadio { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? DisplayModeChanged;
}
