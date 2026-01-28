using System;
using System.Windows.Controls;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IBarPieChartController : IChartPanelControllerHost
{
    RadioButton BarModeRadio { get; }
    RadioButton PieModeRadio { get; }
    ComboBox BucketCountCombo { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? DisplayModeChanged;
    event EventHandler? BucketCountChanged;
}
