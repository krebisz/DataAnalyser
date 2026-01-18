using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the normalized chart panel.
/// </summary>
public partial class NormalizedChartController : UserControl
{
    private readonly LegendToggleManager _legendManager;
    private readonly Dictionary<string, bool> _legendVisibility = new(StringComparer.OrdinalIgnoreCase);

    public NormalizedChartController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.NormalizedChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        NormZeroToOneRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);
        NormPercentOfMaxRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);
        NormRelativeToMaxRadioControl.Checked += (s, e) => NormalizationModeChanged?.Invoke(this, EventArgs.Empty);
        NormalizedPrimarySubtypeComboControl.SelectionChanged += (s, e) => PrimarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        NormalizedSecondarySubtypeComboControl.SelectionChanged += (s, e) => SecondarySubtypeChanged?.Invoke(this, EventArgs.Empty);

        _legendManager = new LegendToggleManager(ChartNormControl, _legendVisibility);
        _legendManager.AttachItemsControl(NormalizedLegendItemsControl);

        RootGrid.Children.Remove(BehavioralControlsPanel);
        RootGrid.Children.Remove(ChartContentPanelRoot);
        PanelController.SetBehavioralControls(BehavioralControlsPanel);
        PanelController.SetChartContent(ChartContentPanelRoot);
    }

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public RadioButton NormZeroToOneRadio => NormZeroToOneRadioControl;

    public RadioButton NormPercentOfMaxRadio => NormPercentOfMaxRadioControl;

    public RadioButton NormRelativeToMaxRadio => NormRelativeToMaxRadioControl;

    public WpfCartesianChart Chart => ChartNormControl;

    public event EventHandler? ToggleRequested;

    public event EventHandler? NormalizationModeChanged;

    public ComboBox NormalizedPrimarySubtypeCombo => NormalizedPrimarySubtypeComboControl;

    public ComboBox NormalizedSecondarySubtypeCombo => NormalizedSecondarySubtypeComboControl;

    public StackPanel NormalizedSecondarySubtypePanel => NormalizedSecondarySubtypePanelControl;

    public event EventHandler? PrimarySubtypeChanged;

    public event EventHandler? SecondarySubtypeChanged;

    private void OnLegendItemToggle(object sender, RoutedEventArgs e)
    {
        LegendToggleManager.HandleToggle(sender);
    }
}
