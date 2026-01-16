using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the Difference / Ratio chart panel.
/// </summary>
public partial class DiffRatioChartController : UserControl
{
    private readonly CartesianChart _chart;
    private readonly ComboBox _primarySubtypeCombo;
    private readonly ComboBox _secondarySubtypeCombo;
    private readonly StackPanel _secondarySubtypePanel;

    public DiffRatioChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.DiffRatioChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);

        var headerControls = BuildHeaderControls();
        PanelController.SetHeaderControls(headerControls);

        var behavioralControls = BuildBehavioralControls(out _primarySubtypeCombo, out _secondarySubtypeCombo, out _secondarySubtypePanel);
        PanelController.SetBehavioralControls(behavioralControls);

        var chartContent = BuildChartContent(out _chart);
        PanelController.SetChartContent(chartContent);
    }

    public CartesianChart Chart => _chart;

    public ChartPanelController Panel => PanelController;

    public Button OperationToggleButton { get; private set; } = null!;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public ComboBox PrimarySubtypeCombo => _primarySubtypeCombo;

    public ComboBox SecondarySubtypeCombo => _secondarySubtypeCombo;

    public StackPanel SecondarySubtypePanel => _secondarySubtypePanel;

    public event EventHandler? ToggleRequested;

    public event EventHandler? OperationToggleRequested;

    public event EventHandler? PrimarySubtypeChanged;

    public event EventHandler? SecondarySubtypeChanged;

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

    private UIElement BuildBehavioralControls(out ComboBox primaryCombo, out ComboBox secondaryCombo, out StackPanel secondaryPanel)
    {
        var panel = new StackPanel
        {
                Orientation = Orientation.Horizontal,
                Margin = ChartUiDefaults.BehavioralControlsMargin
        };

        var primaryPanel = new StackPanel
        {
                Orientation = Orientation.Vertical,
                Margin = ChartUiDefaults.TransformPanelRightMargin,
                MinWidth = ChartUiDefaults.TransformPrimaryPanelMinWidth
        };

        primaryPanel.Children.Add(new TextBlock
        {
                Text = "Primary Subtype:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
        });

        primaryCombo = new ComboBox
        {
                Width = ChartUiDefaults.SubtypeComboWidth
        };
        primaryCombo.SelectionChanged += (s, e) => PrimarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        primaryPanel.Children.Add(primaryCombo);

        secondaryPanel = new StackPanel
        {
                Orientation = Orientation.Vertical,
                Margin = ChartUiDefaults.TransformPanelRightMargin,
                MinWidth = ChartUiDefaults.TransformSecondaryPanelMinWidth,
                Visibility = Visibility.Collapsed
        };

        secondaryPanel.Children.Add(new TextBlock
        {
                Text = "Secondary Subtype:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
        });

        secondaryCombo = new ComboBox
        {
                Width = ChartUiDefaults.SubtypeComboWidth
        };
        secondaryCombo.SelectionChanged += (s, e) => SecondarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        secondaryPanel.Children.Add(secondaryCombo);

        panel.Children.Add(primaryPanel);
        panel.Children.Add(secondaryPanel);

        return panel;
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
