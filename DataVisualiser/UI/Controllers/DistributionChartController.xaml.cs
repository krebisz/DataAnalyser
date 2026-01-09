using System;
using System.Windows;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Controllers;

/// <summary>
///     Controller for the distribution chart panel.
/// </summary>
public partial class DistributionChartController : UserControl
{
    public DistributionChartController()
    {
        InitializeComponent();

        PanelController.Title = "Distribution";
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        RootGrid.Children.Remove(BehavioralControlsPanel);
        RootGrid.Children.Remove(ChartContentPanelRoot);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);
        PanelController.SetChartContent(ChartContentPanelRoot);
    }

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public ComboBox ModeCombo => DistributionModeComboControl;

    public ComboBox SubtypeCombo => DistributionSubtypeComboControl;

    public RadioButton FrequencyShadingRadio => DistributionFrequencyShadingRadioControl;

    public RadioButton SimpleRangeRadio => DistributionSimpleRangeRadioControl;

    public ComboBox IntervalCountCombo => DistributionIntervalCountComboControl;

    public Button ChartTypeToggleButton => DistributionChartTypeToggleButtonControl;

    public WpfCartesianChart Chart => ChartDistributionControl;

    public PolarChart PolarChart => ChartDistributionPolarControl;

    public event EventHandler? ToggleRequested;
}
