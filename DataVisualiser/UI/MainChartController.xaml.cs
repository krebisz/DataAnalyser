using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI
{
    /// <summary>
    /// Controller for the Main chart panel.
    /// This is the simplest chart panel - just title, toggle, and chart.
    /// </summary>
    public partial class MainChartController : UserControl
    {
        private readonly CartesianChart _chart;

        public MainChartController()
        {
            InitializeComponent();

            // Set up the panel controller
            PanelController.Title = "ChartMain";
            PanelController.IsChartVisible = true;

            // Create the chart
            _chart = new CartesianChart
            {
                LegendLocation = LegendLocation.Right,
                Zoom = ZoomingOptions.X,
                Pan = PanningOptions.X,
                Hoverable = true,
                Margin = new Thickness(20, 5, 10, 20),
                MinHeight = 500
            };
            _chart.AxisX.Add(new Axis { Title = "Time" });
            _chart.AxisY.Add(new Axis { Title = "Value", ShowLabels = true });

            // Set the chart content
            PanelController.SetChartContent(_chart);

            // Wire up toggle event
            PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        }

        /// <summary>
        /// Gets the chart control for external access (e.g., for tooltip management, rendering).
        /// </summary>
        public CartesianChart Chart => _chart;

        /// <summary>
        /// Gets the panel controller for external access.
        /// </summary>
        public ChartPanelController Panel => PanelController;

        /// <summary>
        /// Event raised when the toggle button is clicked.
        /// </summary>
        public event System.EventHandler? ToggleRequested;
    }
}

