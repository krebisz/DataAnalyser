using System;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Controls;

public interface IDistributionChartController
{
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
    ComboBox ModeCombo { get; }
    ComboBox SubtypeCombo { get; }
    RadioButton FrequencyShadingRadio { get; }
    RadioButton SimpleRangeRadio { get; }
    ComboBox IntervalCountCombo { get; }
    Button ChartTypeToggleButton { get; }
    WpfCartesianChart Chart { get; }
    PolarChart PolarChart { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? ChartTypeToggleRequested;
    event EventHandler? ModeChanged;
    event EventHandler? SubtypeChanged;
    event EventHandler? DisplayModeChanged;
    event EventHandler? IntervalCountChanged;
}
