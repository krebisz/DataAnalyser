using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public interface INormalizedChartController
{
    CartesianChart Chart { get; }
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
    RadioButton NormZeroToOneRadio { get; }
    RadioButton NormPercentOfMaxRadio { get; }
    RadioButton NormRelativeToMaxRadio { get; }
    ComboBox NormalizedPrimarySubtypeCombo { get; }
    ComboBox NormalizedSecondarySubtypeCombo { get; }
    StackPanel NormalizedSecondarySubtypePanel { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? NormalizationModeChanged;
    event EventHandler? PrimarySubtypeChanged;
    event EventHandler? SecondarySubtypeChanged;
}
