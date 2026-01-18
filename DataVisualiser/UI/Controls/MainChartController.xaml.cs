using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the Main chart panel.
///     This is the simplest chart panel - just title, toggle, and chart.
/// </summary>
public partial class MainChartController : UserControl
{
    public MainChartController()
    {
        InitializeComponent();

        // Set up the panel controller
        PanelController.Title = ChartUiDefaults.MainChartTitle;
        PanelController.IsChartVisible = true;

        DisplayRegularRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        DisplaySummedRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        DisplayStackedRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);

        // Create the chart
        Chart = new CartesianChart
        {
                LegendLocation = ChartUiDefaults.DefaultLegendLocation,
                Zoom = ChartUiDefaults.DefaultZoom,
                Pan = ChartUiDefaults.DefaultPan,
                Hoverable = ChartUiDefaults.DefaultHoverable,
                Margin = ChartUiDefaults.ChartContentMargin,
                MinHeight = ChartUiDefaults.MainChartMinHeight
        };
        Chart.AxisX.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleTime
        });
        Chart.AxisY.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleValue,
                ShowLabels = true
        });

        // Set the chart content
        PanelController.SetChartContent(Chart);
        RootGrid.Children.Remove(BehavioralControlsPanel);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);

        // Wire up toggle event
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
    }

    /// <summary>
    ///     Gets the chart control for external access (e.g., for tooltip management, rendering).
    /// </summary>
    public CartesianChart Chart { get; }

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public RadioButton DisplayRegularRadio => DisplayRegularRadioControl;

    public RadioButton DisplaySummedRadio => DisplaySummedRadioControl;

    public RadioButton DisplayStackedRadio => DisplayStackedRadioControl;

    /// <summary>
    ///     Gets the panel controller for external access.
    /// </summary>
    public ChartPanelController Panel => PanelController;

    /// <summary>
    ///     Event raised when the toggle button is clicked.
    /// </summary>
    public event EventHandler? ToggleRequested;

    public event EventHandler? DisplayModeChanged;
}
