using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI;
using DataVisualiser.UI.Events;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

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

        PanelController.Title = ChartUiDefaults.WeekdayTrendChartTitle;
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
                Margin = ChartUiDefaults.BehavioralControlsMargin
        };

        panel.Children.Add(new TextBlock
        {
                Text = "Days:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = ChartUiDefaults.LabelMargin
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
                Margin = ChartUiDefaults.SectionLabelMargin
        });

        SubtypeCombo = new ComboBox
        {
                Width = ChartUiDefaults.SubtypeComboWidth,
                VerticalAlignment = VerticalAlignment.Center
        };
        SubtypeCombo.SelectionChanged += (s, e) => SubtypeChanged?.Invoke(this, EventArgs.Empty);
        panel.Children.Add(SubtypeCombo);

        ChartTypeToggleButton = new Button
        {
                Content = ChartUiDefaults.ChartTypeToggleLabel,
                Margin = ChartUiDefaults.ToggleButtonMargin,
                Padding = ChartUiDefaults.ToggleButtonPadding,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = ChartUiDefaults.ChartTypeToggleToolTip
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
                Margin = ChartUiDefaults.DayCheckboxMargin
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
                LegendLocation = ChartUiDefaults.DefaultLegendLocation,
                Zoom = ChartUiDefaults.DefaultZoom,
                Pan = ChartUiDefaults.DefaultPan,
                Hoverable = ChartUiDefaults.DefaultHoverable,
                Margin = ChartUiDefaults.ChartContentMargin,
                MinHeight = ChartUiDefaults.ChartMinHeight
        };
        cartesianChart.AxisX.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleTime
        });
        cartesianChart.AxisY.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleValue,
                ShowLabels = true
        });
        panel.Children.Add(cartesianChart);

        polarChart = new CartesianChart
        {
                LegendLocation = ChartUiDefaults.DefaultLegendLocation,
                Zoom = ChartUiDefaults.DefaultZoom,
                Pan = ChartUiDefaults.DefaultPan,
                Hoverable = ChartUiDefaults.DefaultHoverable,
                Margin = ChartUiDefaults.ChartContentMargin,
                MinHeight = ChartUiDefaults.ChartMinHeight,
                Visibility = Visibility.Collapsed
        };
        polarChart.AxisX.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleDayOfWeek,
                MinValue = ChartUiDefaults.PolarAxisMinValue,
                MaxValue = ChartUiDefaults.PolarAxisMaxValue
        });
        polarChart.AxisY.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleValue,
                ShowLabels = true
        });
        panel.Children.Add(polarChart);

        return panel;
    }
}

