using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Infrastructure;
using System;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Charts.Helpers;
using LiveCharts.Defaults;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Controllers;

/// <summary>
///     Controller for the data transform panel (V2 layout).
/// </summary>
public partial class TransformDataPanelControllerV2 : UserControl, ITransformDataPanelController
{
    private const double CollapsedHandleWidth = 16;
    private const double DefaultExpandedRailWidth = 700;
    private readonly LegendToggleManager _legendManager;
    private readonly Dictionary<string, bool> _legendVisibility = new(StringComparer.OrdinalIgnoreCase);
    private bool _isLeftRailCollapsed;
    private double _lastKnownLeftRailContentWidth;
    private double _lastKnownGrid1Width;
    private double _lastKnownGrid2Width;
    private AxisSection? _minLineSection;
    private AxisSection? _maxLineSection;

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
        TransformLeftRailHostPanel.SizeChanged += (_, _) => UpdateResultsGridWidth();
        TransformGrid1Panel.SizeChanged += (_, _) => UpdateResultsGridWidth();
        TransformGrid2PanelControl.SizeChanged += (_, _) => UpdateResultsGridWidth();
        TransformGrid3PanelControl.IsVisibleChanged += (_, _) => UpdateResultsGridWidth();
        TransformMainContentGrid.SizeChanged += (_, _) => UpdateChartWidth();
        TransformMinMaxLinesCheckBox.Checked += (_, _) => UpdateMinMaxLines();
        TransformMinMaxLinesCheckBox.Unchecked += (_, _) => ClearMinMaxLines();

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
        UpdateResultsGridWidth();
    }

    private void ExpandLeftRail()
    {
        if (!_isLeftRailCollapsed)
        {
            TransformLeftRailScrollViewer.Visibility = Visibility.Visible;
            LeftRailColumn.Width = new GridLength(DefaultExpandedRailWidth);
            TransformLeftRailToggleButton.Content = "<";
            UpdateResultsGridWidth();
            return;
        }

        _isLeftRailCollapsed = false;
        TransformLeftRailScrollViewer.Visibility = Visibility.Visible;
        LeftRailColumn.Width = new GridLength(DefaultExpandedRailWidth);
        TransformLeftRailToggleButton.Content = "<";
        UpdateResultsGridWidth();
    }

    private void OnLegendItemToggle(object sender, RoutedEventArgs e)
    {
        LegendToggleManager.HandleToggle(sender);
    }

    public void UpdateMinMaxLines()
    {
        if (TransformMinMaxLinesCheckBox.IsChecked != true)
        {
            ClearMinMaxLines();
            return;
        }

        if (!TryGetSeriesMinMax(out var min, out var max))
        {
            ClearMinMaxLines();
            return;
        }

        var chart = ChartTransformResultControl;
        var minLabel = $"min {min:F4}";
        var maxLabel = $"max {max:F4}";

        DataVisualiser.Core.Rendering.Helpers.ChartHelper.UpdateHorizontalLineForChart(
            ref chart,
            min,
            ref _minLineSection,
            ChartUiDefaults.MinMaxLineStroke,
            ChartUiDefaults.MinMaxLineThickness,
            ChartUiDefaults.MinMaxLineDashArray,
            minLabel);
        DataVisualiser.Core.Rendering.Helpers.ChartHelper.UpdateHorizontalLineForChart(
            ref chart,
            max,
            ref _maxLineSection,
            ChartUiDefaults.MinMaxLineStroke,
            ChartUiDefaults.MinMaxLineThickness,
            ChartUiDefaults.MinMaxLineDashArray,
            maxLabel);

        ChartTransformResultControl.Update(true, true);
        ChartTransformResultControl.InvalidateVisual();
    }

    public void ResetMinMaxLines()
    {
        TransformMinMaxLinesCheckBox.IsChecked = false;
        ClearMinMaxLines();
    }

    private void ClearMinMaxLines()
    {
        DataVisualiser.Core.Rendering.Helpers.ChartHelper.RemoveAxisSectionFromYAxis(ChartTransformResultControl, _minLineSection);
        DataVisualiser.Core.Rendering.Helpers.ChartHelper.RemoveAxisSectionFromYAxis(ChartTransformResultControl, _maxLineSection);
        _minLineSection = null;
        _maxLineSection = null;
    }

    private bool TryGetSeriesMinMax(out double min, out double max)
    {
        min = double.PositiveInfinity;
        max = double.NegativeInfinity;

        foreach (var series in ChartTransformResultControl.Series.OfType<Series>())
        {
            if (series.Visibility == Visibility.Collapsed)
                continue;

            if (series.Values == null)
                continue;

            foreach (var value in series.Values)
            {
                if (!TryExtractValue(value, out var numeric))
                    continue;
                if (double.IsNaN(numeric) || double.IsInfinity(numeric))
                    continue;

                if (numeric < min)
                    min = numeric;
                if (numeric > max)
                    max = numeric;
            }
        }

        return min <= max && !double.IsInfinity(min) && !double.IsInfinity(max);
    }

    private static bool TryExtractValue(object value, out double numeric)
    {
        switch (value)
        {
            case double d:
                numeric = d;
                return true;
            case float f:
                numeric = f;
                return true;
            case decimal m:
                numeric = (double)m;
                return true;
            case ObservablePoint point:
                numeric = point.Y;
                return true;
            default:
                numeric = 0;
                return false;
        }
    }

    private void UpdateResultsGridWidth()
    {
        if (TransformLeftRailScrollViewer.Visibility == Visibility.Visible)
        {
            var hostWidth = TransformLeftRailHostPanel.ActualWidth;
            if (hostWidth > 0)
            {
                _lastKnownLeftRailContentWidth = hostWidth;
            }

            var grid1Width = TransformGrid1Panel.ActualWidth;
            if (grid1Width > 0)
                _lastKnownGrid1Width = grid1Width;

            var grid2Width = TransformGrid2PanelControl.IsVisible ? TransformGrid2PanelControl.ActualWidth : 0;
            if (grid2Width > 0)
                _lastKnownGrid2Width = grid2Width;
        }

        var grid1BaseWidth = TransformGrid1Panel.MinWidth > 0 ? TransformGrid1Panel.MinWidth : _lastKnownGrid1Width;
        var grid2BaseWidth = TransformGrid2PanelControl.IsVisible
            ? (TransformGrid2PanelControl.MinWidth > 0 ? TransformGrid2PanelControl.MinWidth : _lastKnownGrid2Width)
            : 0;
        var targetLeftWidth = Math.Max(grid1BaseWidth, grid2BaseWidth);

        if (targetLeftWidth <= 0)
            return;

        var minWidth = TransformGrid3PanelControl.MinWidth;
        var targetWidth = Math.Max(targetLeftWidth, minWidth);
        TransformGrid3PanelControl.Width = targetWidth;
        UpdateChartWidth();
    }

    private void UpdateChartWidth()
    {
        if (TransformChartContainerControl == null || TransformMainContentGrid == null)
            return;

        var mainWidth = TransformMainContentGrid.ActualWidth;
        if (mainWidth <= 0)
            return;

        var grid3Width = TransformGrid3PanelControl.IsVisible ? TransformGrid3PanelControl.ActualWidth : 0;
        var margin = TransformChartContainerControl.Margin;
        var availableWidth = Math.Max(0, mainWidth - grid3Width - margin.Left - margin.Right);
        var minWidth = TransformChartContainerControl.MinWidth;
        var chartWidth = Math.Max(minWidth, availableWidth);
        TransformChartContainerControl.Width = chartWidth;
    }
}
