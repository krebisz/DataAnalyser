using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the data transform panel.
/// </summary>
public partial class TransformDataPanelController : UserControl
{
    public TransformDataPanelController()
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.TransformChartTitle;
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        TransformOperationComboControl.SelectionChanged += (s, e) => OperationChanged?.Invoke(this, EventArgs.Empty);
        TransformPrimarySubtypeComboControl.SelectionChanged += (s, e) => PrimarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        TransformSecondarySubtypeComboControl.SelectionChanged += (s, e) => SecondarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        TransformComputeButtonControl.Click += (s, e) => ComputeRequested?.Invoke(this, EventArgs.Empty);

        RootGrid.Children.Remove(TransformContentRootPanel);
        PanelController.SetChartContent(TransformContentRootPanel);
    }

    public ChartPanelController Panel => PanelController;

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public ComboBox TransformPrimarySubtypeCombo => TransformPrimarySubtypeComboControl;

    public ComboBox TransformSecondarySubtypeCombo => TransformSecondarySubtypeComboControl;

    public ComboBox TransformOperationCombo => TransformOperationComboControl;

    public Button TransformComputeButton => TransformComputeButtonControl;

    public StackPanel TransformSecondarySubtypePanel => TransformSecondarySubtypePanelControl;

    public StackPanel TransformGrid2Panel => TransformGrid2PanelControl;

    public StackPanel TransformGrid3Panel => TransformGrid3PanelControl;

    public StackPanel TransformChartContentPanel => TransformChartContentPanelControl;

    public Grid TransformChartContainer => TransformChartContainerControl;

    public DataGrid TransformGrid1 => TransformGrid1Control;

    public DataGrid TransformGrid2 => TransformGrid2Control;

    public DataGrid TransformGrid3 => TransformGrid3Control;

    public TextBlock TransformGrid1Title => TransformGrid1TitleControl;

    public TextBlock TransformGrid2Title => TransformGrid2TitleControl;

    public TextBlock TransformGrid3Title => TransformGrid3TitleControl;

    public CartesianChart ChartTransformResult => ChartTransformResultControl;

    public CartesianChart Chart => ChartTransformResultControl;

    public event EventHandler? ToggleRequested;

    public event EventHandler? OperationChanged;

    public event EventHandler? PrimarySubtypeChanged;

    public event EventHandler? SecondarySubtypeChanged;

    public event EventHandler? ComputeRequested;
}