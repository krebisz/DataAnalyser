using System;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface ITransformDataPanelController : IChartPanelControllerHost
{
    ComboBox TransformPrimarySubtypeCombo { get; }
    ComboBox TransformSecondarySubtypeCombo { get; }
    ComboBox TransformOperationCombo { get; }
    Button TransformComputeButton { get; }
    StackPanel TransformSecondarySubtypePanel { get; }
    StackPanel TransformGrid2Panel { get; }
    StackPanel TransformGrid3Panel { get; }
    StackPanel TransformChartContentPanel { get; }
    Grid TransformChartContainer { get; }
    DataGrid TransformGrid1 { get; }
    DataGrid TransformGrid2 { get; }
    DataGrid TransformGrid3 { get; }
    TextBlock TransformGrid1Title { get; }
    TextBlock TransformGrid2Title { get; }
    TextBlock TransformGrid3Title { get; }
    CartesianChart ChartTransformResult { get; }
    CartesianChart Chart { get; }
    Dispatcher Dispatcher { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? OperationChanged;
    event EventHandler? PrimarySubtypeChanged;
    event EventHandler? SecondarySubtypeChanged;
    event EventHandler? ComputeRequested;
}
