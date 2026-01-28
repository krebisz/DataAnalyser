using System;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

/// <summary>
///     Controller for the data transform panel (V2 layout).
/// </summary>
public partial class TransformDataPanelControllerV2 : UserControl
{
    private const double CollapsedHandleWidth = 16;
    private const double DefaultExpandedRailWidth = 700;
    private readonly LegendToggleManager _legendManager;
    private readonly Dictionary<string, bool> _legendVisibility = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLeftRailCollapsed;

    public TransformDataPanelControllerV2()
        : this(new DefaultTransformOperationProvider())
    {
    }

    public TransformDataPanelControllerV2(ITransformOperationProvider operationProvider)
    {
        InitializeComponent();

        PanelController.Title = ChartUiDefaults.TransformChartTitle;
        TransformOperationOptions.Populate(TransformOperationComboControl, operationProvider);
        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
        TransformOperationComboControl.SelectionChanged += (s, e) => OperationChanged?.Invoke(this, EventArgs.Empty);
        TransformPrimarySubtypeComboControl.SelectionChanged += (s, e) => PrimarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        TransformSecondarySubtypeComboControl.SelectionChanged += (s, e) => SecondarySubtypeChanged?.Invoke(this, EventArgs.Empty);
        TransformComputeButtonControl.Click += (s, e) => ComputeRequested?.Invoke(this, EventArgs.Empty);
        TransformLeftRailToggleButton.Checked += (_, _) => CollapseLeftRail();
        TransformLeftRailToggleButton.Unchecked += (_, _) => ExpandLeftRail();

        _legendManager = new LegendToggleManager(ChartTransformResultControl, _legendVisibility);
        _legendManager.AttachItemsControl(TransformLegendItemsControl);

        RootGrid.Children.Remove(TransformContentRootPanel);
        PanelController.SetChartContent(TransformContentRootPanel);

        ExpandLeftRail();
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

    private void CollapseLeftRail()
    {
        if (_isLeftRailCollapsed)
            return;

        _isLeftRailCollapsed = true;
        TransformLeftRailScrollViewer.Visibility = Visibility.Collapsed;
        LeftRailColumn.Width = new GridLength(CollapsedHandleWidth);
        TransformLeftRailToggleButton.Content = ">";
    }

    private void ExpandLeftRail()
    {
        if (!_isLeftRailCollapsed)
        {
            TransformLeftRailScrollViewer.Visibility = Visibility.Visible;
            LeftRailColumn.Width = new GridLength(DefaultExpandedRailWidth);
            TransformLeftRailToggleButton.Content = "<";
            return;
        }

        _isLeftRailCollapsed = false;
        TransformLeftRailScrollViewer.Visibility = Visibility.Visible;
        LeftRailColumn.Width = new GridLength(DefaultExpandedRailWidth);
        TransformLeftRailToggleButton.Content = "<";
    }

    private void OnLegendItemToggle(object sender, RoutedEventArgs e)
    {
        LegendToggleManager.HandleToggle(sender);
    }
}
