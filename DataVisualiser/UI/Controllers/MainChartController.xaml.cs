using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser.UI;

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
        PanelController.Title = "ChartMain";
        PanelController.IsChartVisible = true;

        // Create the chart
        Chart = new CartesianChart
        {
            LegendLocation = LegendLocation.Right,
            Zoom = ZoomingOptions.X,
            Pan = PanningOptions.X,
            Hoverable = true,
            Margin = new Thickness(20, 5, 10, 20),
            MinHeight = 500
        };
        Chart.AxisX.Add(new Axis
        {
            Title = "Time"
        });
        Chart.AxisY.Add(new Axis
        {
            Title = "Value",
            ShowLabels = true
        });

        // Set the chart content
        PanelController.SetChartContent(Chart);

        // Wire up toggle event
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
    }

    /// <summary>
    ///     Gets the chart control for external access (e.g., for tooltip management, rendering).
    /// </summary>
    public CartesianChart Chart { get; }

    /// <summary>
    ///     Gets the panel controller for external access.
    /// </summary>
    public ChartPanelController Panel => PanelController;

    /// <summary>
    ///     Event raised when the toggle button is clicked.
    /// </summary>
    public event EventHandler? ToggleRequested;
}