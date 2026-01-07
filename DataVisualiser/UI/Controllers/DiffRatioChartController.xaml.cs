using System;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI;

/// <summary>
///     Controller for the Difference / Ratio chart panel.
/// </summary>
public partial class DiffRatioChartController : UserControl
{
    private readonly CartesianChart _chart;
    private Button _operationToggleButton = null!;

    public DiffRatioChartController()
    {
        InitializeComponent();

        PanelController.Title = "Difference / Ratio";
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        var headerControls = BuildHeaderControls();
        PanelController.SetHeaderControls(headerControls);

        var chartContent = BuildChartContent(out _chart);
        PanelController.SetChartContent(chartContent);
    }

    public CartesianChart Chart => _chart;

    public ChartPanelController Panel => PanelController;

    public Button OperationToggleButton => _operationToggleButton;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public event EventHandler? ToggleRequested;

    public event EventHandler? OperationToggleRequested;

    private UIElement BuildHeaderControls()
    {
        _operationToggleButton = new Button
        {
            Content = "/",
            Margin = new Thickness(20, 0, 0, 0),
            Padding = new Thickness(10, 3, 10, 3),
            VerticalAlignment = VerticalAlignment.Center,
            ToolTip = "Toggle between Difference (-) and Ratio (/)"
        };
        _operationToggleButton.Click += (s, e) => OperationToggleRequested?.Invoke(this, EventArgs.Empty);

        return _operationToggleButton;
    }

    private static UIElement BuildChartContent(out CartesianChart chart)
    {
        chart = new CartesianChart
        {
            LegendLocation = LegendLocation.Right,
            Zoom = ZoomingOptions.X,
            Pan = PanningOptions.X,
            Hoverable = true,
            Margin = new Thickness(20, 5, 10, 20),
            MinHeight = 400
        };
        chart.AxisX.Add(new Axis
        {
            Title = "Time"
        });
        chart.AxisY.Add(new Axis
        {
            Title = "Difference",
            ShowLabels = true
        });

        return chart;
    }
}
