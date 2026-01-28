using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public interface IDiffRatioChartController : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
    Button OperationToggleButton { get; }
    ComboBox PrimarySubtypeCombo { get; }
    ComboBox SecondarySubtypeCombo { get; }
    StackPanel SecondarySubtypePanel { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? OperationToggleRequested;
    event EventHandler? PrimarySubtypeChanged;
    event EventHandler? SecondarySubtypeChanged;
}
