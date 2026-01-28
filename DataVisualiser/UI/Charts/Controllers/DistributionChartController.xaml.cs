using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Infrastructure;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Controllers;

/// <summary>
///     Controller for the distribution chart panel.
/// </summary>
public partial class DistributionChartController : UserControl, IDistributionChartController
{
    public DistributionChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.DistributionChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        DistributionChartTypeToggleButtonControl.Click += (s, e) => ChartTypeToggleRequested?.Invoke(this, EventArgs.Empty);
        DistributionModeComboControl.SelectionChanged += (s, e) => ModeChanged?.Invoke(this, EventArgs.Empty);
        DistributionSubtypeComboControl.SelectionChanged += (s, e) => SubtypeChanged?.Invoke(this, EventArgs.Empty);
        DistributionFrequencyShadingRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        DistributionSimpleRangeRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        DistributionIntervalCountComboControl.SelectionChanged += (s, e) => IntervalCountChanged?.Invoke(this, EventArgs.Empty);

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

    public event EventHandler? ChartTypeToggleRequested;
    public event EventHandler? ModeChanged;
    public event EventHandler? SubtypeChanged;
    public event EventHandler? DisplayModeChanged;
    public event EventHandler? IntervalCountChanged;
}
