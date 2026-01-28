using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Events;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the weekday trend chart panel, including day filters and chart type toggle.
/// </summary>
public partial class WeekdayTrendChartController : UserControl, IWeekdayTrendChartController
{
    private readonly CartesianChart _cartesianChart;
    private readonly LegendToggleManager _cartesianLegendManager;
    private readonly CartesianChart _polarChart;
    private readonly LegendToggleManager _polarLegendManager;

    public WeekdayTrendChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.WeekdayTrendChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        var behavioralControls = BuildBehavioralControls();
        PanelController.SetBehavioralControls(behavioralControls);

        var chartContent = BuildChartContent(out _cartesianChart, out _polarChart, out var cartesianLegendItems, out var polarLegendItems);
        _cartesianLegendManager = new LegendToggleManager(_cartesianChart);
        _cartesianLegendManager.AttachItemsControl(cartesianLegendItems);
        _polarLegendManager = new LegendToggleManager(_polarChart);
        _polarLegendManager.AttachItemsControl(polarLegendItems);
        PanelController.SetChartContent(chartContent);
    }

    public CartesianChart Chart => _cartesianChart;

    public CartesianChart PolarChart => _polarChart;

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public Button ChartTypeToggleButton { get; private set; } = null!;
    public ComboBox SubtypeCombo { get; private set; } = null!;
    public ComboBox AverageWindowCombo { get; private set; } = null!;

    public event EventHandler? ToggleRequested;

    public event EventHandler? ChartTypeToggleRequested;
    public event EventHandler? SubtypeChanged;

    public event EventHandler<WeekdayTrendDayToggleEventArgs>? DayToggled;
    public event EventHandler<WeekdayTrendAverageToggleEventArgs>? AverageToggled;
    public event EventHandler? AverageWindowChanged;

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
        AddAverageCheckBox(panel);

        panel.Children.Add(new TextBlock
        {
                Text = "Average:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = ChartUiDefaults.SectionLabelMargin
        });

        AverageWindowCombo = new ComboBox
        {
                Width = ChartUiDefaults.WeekdayTrendAverageComboWidth,
                VerticalAlignment = VerticalAlignment.Center
        };
        AverageWindowCombo.SelectionChanged += (s, e) => AverageWindowChanged?.Invoke(this, EventArgs.Empty);
        panel.Children.Add(AverageWindowCombo);

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
                ToolTip = ChartUiDefaults.WeekdayTrendChartTypeToggleToolTip
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

    private void AddAverageCheckBox(Panel panel)
    {
        var checkbox = new CheckBox
        {
                Content = "Ave",
                IsChecked = true,
                Margin = ChartUiDefaults.DayCheckboxMargin
        };

        checkbox.Click += (s, e) => AverageToggled?.Invoke(this, new WeekdayTrendAverageToggleEventArgs(checkbox.IsChecked == true));
        panel.Children.Add(checkbox);
    }

    private UIElement BuildChartContent(out CartesianChart cartesianChart, out CartesianChart polarChart, out ItemsControl cartesianLegendItems, out ItemsControl polarLegendItems)
    {
        var panel = new StackPanel
        {
                Orientation = Orientation.Vertical
        };

        cartesianChart = new CartesianChart
        {
                LegendLocation = LegendLocation.None,
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
        cartesianLegendItems = LegendToggleManager.CreateLegendItemsControl(OnLegendItemToggle);
        var cartesianLegendContainer = LegendToggleManager.CreateLegendContainer(cartesianLegendItems);
        var cartesianGrid = new Grid();
        cartesianGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = new GridLength(1, GridUnitType.Star)
        });
        cartesianGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = GridLength.Auto
        });
        Grid.SetColumn(cartesianChart, 0);
        Grid.SetColumn(cartesianLegendContainer, 1);
        cartesianGrid.Children.Add(cartesianChart);
        cartesianGrid.Children.Add(cartesianLegendContainer);
        cartesianGrid.SetBinding(VisibilityProperty,
                new Binding(nameof(Visibility))
                {
                        Source = cartesianChart
                });
        panel.Children.Add(cartesianGrid);

        polarChart = new CartesianChart
        {
                LegendLocation = LegendLocation.None,
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
        polarLegendItems = LegendToggleManager.CreateLegendItemsControl(OnLegendItemToggle);
        var polarLegendContainer = LegendToggleManager.CreateLegendContainer(polarLegendItems);
        var polarGrid = new Grid();
        polarGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = new GridLength(1, GridUnitType.Star)
        });
        polarGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = GridLength.Auto
        });
        Grid.SetColumn(polarChart, 0);
        Grid.SetColumn(polarLegendContainer, 1);
        polarGrid.Children.Add(polarChart);
        polarGrid.Children.Add(polarLegendContainer);
        polarGrid.SetBinding(VisibilityProperty,
                new Binding(nameof(Visibility))
                {
                        Source = polarChart
                });
        panel.Children.Add(polarGrid);

        return panel;
    }

    private void OnLegendItemToggle(object sender, RoutedEventArgs e)
    {
        LegendToggleManager.HandleToggle(sender);
    }
}
