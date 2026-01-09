using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controllers;

/// <summary>
///     Controller for the weekday trend chart panel, including day filters and chart type toggle.
/// </summary>
public partial class WeekdayTrendChartController : UserControl
{
    private readonly CartesianChart _cartesianChart;
    private readonly CartesianChart _polarChart;

    public WeekdayTrendChartController()
    {
        InitializeComponent();

        PanelController.Title = "Weekday Trends";
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        var behavioralControls = BuildBehavioralControls();
        PanelController.SetBehavioralControls(behavioralControls);

        var chartContent = BuildChartContent(out _cartesianChart, out _polarChart);
        PanelController.SetChartContent(chartContent);
    }

    public CartesianChart Chart => _cartesianChart;

    public CartesianChart PolarChart => _polarChart;

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public Button ChartTypeToggleButton { get; private set; } = null!;
    public ComboBox SubtypeCombo { get; private set; } = null!;

    public event EventHandler? ToggleRequested;

    public event EventHandler? ChartTypeToggleRequested;
    public event EventHandler? SubtypeChanged;

    public event EventHandler<WeekdayTrendDayToggleEventArgs>? DayToggled;

    private UIElement BuildBehavioralControls()
    {
        var panel = new StackPanel
        {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(40, 5, 10, 0)
        };

        panel.Children.Add(new TextBlock
        {
                Text = "Days:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
        });

        AddDayCheckBox(panel, "Mon", DayOfWeek.Monday);
        AddDayCheckBox(panel, "Tue", DayOfWeek.Tuesday);
        AddDayCheckBox(panel, "Wed", DayOfWeek.Wednesday);
        AddDayCheckBox(panel, "Thu", DayOfWeek.Thursday);
        AddDayCheckBox(panel, "Fri", DayOfWeek.Friday);
        AddDayCheckBox(panel, "Sat", DayOfWeek.Saturday);
        AddDayCheckBox(panel, "Sun", DayOfWeek.Sunday);

        panel.Children.Add(new TextBlock
        {
                Text = "Subtype:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 10, 0)
        });

        SubtypeCombo = new ComboBox
        {
                Width = 160,
                VerticalAlignment = VerticalAlignment.Center
        };
        SubtypeCombo.SelectionChanged += (s, e) => SubtypeChanged?.Invoke(this, EventArgs.Empty);
        panel.Children.Add(SubtypeCombo);

        ChartTypeToggleButton = new Button
        {
                Content = "Polar",
                Margin = new Thickness(20, 0, 0, 0),
                Padding = new Thickness(10, 3, 10, 3),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Toggle between Cartesian and Polar chart"
        };
        ChartTypeToggleButton.Click += (s, e) => ChartTypeToggleRequested?.Invoke(this, EventArgs.Empty);
        panel.Children.Add(ChartTypeToggleButton);

        return panel;
    }

    private void AddDayCheckBox(Panel panel, string label, DayOfWeek day)
    {
        var checkbox = new CheckBox
        {
                Content = label,
                IsChecked = true,
                Margin = new Thickness(5, 0, 0, 0)
        };

        checkbox.Click += (s, e) => DayToggled?.Invoke(this, new WeekdayTrendDayToggleEventArgs(day, checkbox.IsChecked == true));
        panel.Children.Add(checkbox);
    }

    private static UIElement BuildChartContent(out CartesianChart cartesianChart, out CartesianChart polarChart)
    {
        var panel = new StackPanel
        {
                Orientation = Orientation.Vertical
        };

        cartesianChart = new CartesianChart
        {
                LegendLocation = LegendLocation.Right,
                Zoom = ZoomingOptions.X,
                Pan = PanningOptions.X,
                Hoverable = true,
                Margin = new Thickness(20, 5, 10, 20),
                MinHeight = 400
        };
        cartesianChart.AxisX.Add(new Axis
        {
                Title = "Time"
        });
        cartesianChart.AxisY.Add(new Axis
        {
                Title = "Value",
                ShowLabels = true
        });
        panel.Children.Add(cartesianChart);

        polarChart = new CartesianChart
        {
                LegendLocation = LegendLocation.Right,
                Zoom = ZoomingOptions.X,
                Pan = PanningOptions.X,
                Hoverable = true,
                Margin = new Thickness(20, 5, 10, 20),
                MinHeight = 400,
                Visibility = Visibility.Collapsed
        };
        polarChart.AxisX.Add(new Axis
        {
                Title = "Day of Week",
                MinValue = 0,
                MaxValue = 360
        });
        polarChart.AxisY.Add(new Axis
        {
                Title = "Value",
                ShowLabels = true
        });
        panel.Children.Add(polarChart);

        return panel;
    }
}
