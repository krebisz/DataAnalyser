using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI;
using LiveCharts;
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

        // Wire up toggle event
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
    }

    /// <summary>
    ///     Gets the chart control for external access (e.g., for tooltip management, rendering).
    /// </summary>
    public CartesianChart Chart { get; }

    public Button ToggleButton => PanelController.ToggleButtonControl;

    /// <summary>
    ///     Gets the panel controller for external access.
    /// </summary>
    public ChartPanelController Panel => PanelController;

    /// <summary>
    ///     Event raised when the toggle button is clicked.
    /// </summary>
    public event EventHandler? ToggleRequested;
}

