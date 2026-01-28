using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the Bar / Pie chart panel.
/// </summary>
public partial class BarPieChartController : UserControl, IChartPanelScaffold, IBarPieChartController
{
    public BarPieChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.BarPieChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        BarModeRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        PieModeRadioControl.Checked += (s, e) => DisplayModeChanged?.Invoke(this, EventArgs.Empty);
        BucketCountComboControl.SelectionChanged += (s, e) => BucketCountChanged?.Invoke(this, EventArgs.Empty);

        RootGrid.Children.Remove(BehavioralControlsPanel);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);
    }

    public RadioButton BarModeRadio => BarModeRadioControl;

    public RadioButton PieModeRadio => PieModeRadioControl;

    public ComboBox BucketCountCombo => BucketCountComboControl;

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public string Title
    {
        get => PanelController.Title;
        set => PanelController.Title = value;
    }

    public bool IsChartVisible
    {
        get => PanelController.IsChartVisible;
        set => PanelController.IsChartVisible = value;
    }

    public IChartRenderingContext? RenderingContext
    {
        get => PanelController.RenderingContext;
        set => PanelController.RenderingContext = value;
    }

    public event EventHandler? ToggleRequested;

    public void SetHeaderControls(UIElement? controls)
    {
        PanelController.SetHeaderControls(controls);
    }

    public void SetBehavioralControls(UIElement? controls)
    {
        PanelController.SetBehavioralControls(controls);
    }

    public void SetChartContent(UIElement? chart)
    {
        PanelController.SetChartContent(chart);
    }

    public event EventHandler? DisplayModeChanged;
    public event EventHandler? BucketCountChanged;
}
