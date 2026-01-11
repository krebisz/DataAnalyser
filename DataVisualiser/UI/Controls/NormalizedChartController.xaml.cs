using System;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the normalized chart panel.
/// </summary>
public partial class NormalizedChartController : UserControl
{
    public NormalizedChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.NormalizedChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        NormZeroToOneRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);
        NormPercentOfMaxRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);
        NormRelativeToMaxRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);

        RootGrid.Children.Remove(BehavioralControlsPanel);
        RootGrid.Children.Remove(ChartContentPanelRoot);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);
        PanelController.SetChartContent(ChartContentPanelRoot);
    }

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public RadioButton NormZeroToOneRadio => NormZeroToOneRadioControl;

    public RadioButton NormPercentOfMaxRadio => NormPercentOfMaxRadioControl;

    public RadioButton NormRelativeToMaxRadio => NormRelativeToMaxRadioControl;

    public WpfCartesianChart Chart => ChartNormControl;

    public event EventHandler? ToggleRequested;

    public event EventHandler? NormalizationModeChanged;
}

