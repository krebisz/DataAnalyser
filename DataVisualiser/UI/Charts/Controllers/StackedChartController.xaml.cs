using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.UI.Defaults;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Controllers;

/// <summary>
///     Controller for the dedicated stacked chart panel.
/// </summary>
public partial class StackedChartController : UserControl, IStackedChartController
{
    private readonly LegendToggleManager _legendManager;
    private readonly ComboBox _overlaySubtypeCombo;
    private readonly Dictionary<string, bool> _legendVisibility = new(StringComparer.OrdinalIgnoreCase);

    public StackedChartController()
    {
        InitializeComponent();

        PanelController.Title = "Metrics: Stacked";
        PanelController.IsChartVisible = false;

        var behavioralControls = BuildBehavioralControls(out _overlaySubtypeCombo);
        PanelController.SetBehavioralControls(behavioralControls);

        Chart = new CartesianChart
        {
                LegendLocation = LegendLocation.None,
                Zoom = ChartUiDefaults.DefaultZoom,
                Pan = ChartUiDefaults.DefaultPan,
                Hoverable = ChartUiDefaults.DefaultHoverable,
                Margin = ChartUiDefaults.ChartContentMargin,
                MinHeight = ChartUiDefaults.MainChartMinHeight
        };

        Chart.AxisX.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleTime
        });
        Chart.AxisY.Add(new Axis
        {
                Title = ChartUiDefaults.AxisTitleValue,
                ShowLabels = true
        });

        var legendItems = LegendToggleManager.CreateLegendItemsControl(OnLegendItemToggle);
        var legendContainer = LegendToggleManager.CreateLegendContainer(legendItems);
        var chartGrid = new Grid();
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = new GridLength(1, GridUnitType.Star)
        });
        chartGrid.ColumnDefinitions.Add(new ColumnDefinition
        {
                Width = GridLength.Auto
        });
        Grid.SetColumn(Chart, 0);
        Grid.SetColumn(legendContainer, 1);
        chartGrid.Children.Add(Chart);
        chartGrid.Children.Add(legendContainer);

        _legendManager = new LegendToggleManager(Chart, _legendVisibility);
        _legendManager.AttachItemsControl(legendItems);

        PanelController.SetChartContent(chartGrid);

        PanelController.ToggleRequested += (s, e) => ToggleRequested?.Invoke(this, e);
    }

    public CartesianChart Chart { get; }

    public Button ToggleButton => PanelController.ToggleButtonControl;

    public ChartPanelController Panel => PanelController;

    public ComboBox OverlaySubtypeCombo => _overlaySubtypeCombo;

    public event EventHandler? ToggleRequested;
    public event EventHandler? OverlaySubtypeChanged;

    private void OnLegendItemToggle(object sender, RoutedEventArgs e)
    {
        LegendToggleManager.HandleToggle(sender);
    }

    private UIElement BuildBehavioralControls(out ComboBox subtypeCombo)
    {
        var panel = new StackPanel
        {
                Orientation = Orientation.Horizontal,
                Margin = ChartUiDefaults.BehavioralControlsMargin
        };

        var subtypePanel = new StackPanel
        {
                Orientation = Orientation.Vertical,
                Margin = ChartUiDefaults.TransformPanelRightMargin,
                MinWidth = ChartUiDefaults.TransformPrimaryPanelMinWidth
        };

        subtypePanel.Children.Add(new TextBlock
        {
                Text = "Overlay Subtype:",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 5)
        });

        subtypeCombo = new ComboBox
        {
                Width = ChartUiDefaults.SubtypeComboWidth,
                IsEnabled = false
        };
        subtypeCombo.SelectionChanged += (s, e) => OverlaySubtypeChanged?.Invoke(this, EventArgs.Empty);
        subtypePanel.Children.Add(subtypeCombo);

        panel.Children.Add(subtypePanel);

        return panel;
    }
}
