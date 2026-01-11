using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the Difference / Ratio chart panel.
/// </summary>
public partial class DiffRatioChartController : UserControl
{
    private readonly CartesianChart _chart;

    public DiffRatioChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.DiffRatioChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        var headerControls = BuildHeaderControls();
        PanelController.SetHeaderControls(headerControls);

        var chartContent = BuildChartContent(out _chart);
        PanelController.SetChartContent(chartContent);
    }

    public CartesianChart Chart => _chart;

    public ChartPanelController Panel => PanelController;

    public Button OperationToggleButton { get; private set; } = null!;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public event EventHandler? ToggleRequested;

    public event EventHandler? OperationToggleRequested;

    private UIElement BuildHeaderControls()
    {
        OperationToggleButton = new Button
        {
                Content = ChartUiDefaults.OperationToggleContent,
                Margin = ChartUiDefaults.ToggleButtonMargin,
                Padding = ChartUiDefaults.ToggleButtonPadding,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = ChartUiDefaults.OperationToggleToolTip
        };
        OperationToggleButton.Click += (s, e) => OperationToggleRequested?.Invoke(this, EventArgs.Empty);

        return OperationToggleButton;
    }

    private static UIElement BuildChartContent(out CartesianChart chart)
    {
        chart = new CartesianChart
        {
                LegendLocation = ChartUiDefaults.DefaultLegendLocation,
                Zoom = ChartUiDefaults.DefaultZoom,
                Pan = ChartUiDefaults.DefaultPan,
                Hoverable = ChartUiDefaults.DefaultHoverable,
                Margin = ChartUiDefaults.ChartContentMargin,
                MinHeight = ChartUiDefaults.ChartMinHeight
        };
        chart.AxisX.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleTime
        });
        chart.AxisY.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleDifference,
                ShowLabels = true
        });

        return chart;
    }
}

