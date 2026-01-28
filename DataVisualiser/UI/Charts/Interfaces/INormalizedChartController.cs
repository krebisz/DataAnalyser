using System;
using System.Windows.Controls;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface INormalizedChartController : IChartPanelControllerHost
{
    CartesianChart Chart { get; }
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
