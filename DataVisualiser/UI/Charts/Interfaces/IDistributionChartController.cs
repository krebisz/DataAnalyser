using System;
using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.WPF;
using WpfCartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IDistributionChartController : ICartesianChartControllerHost
{
    ComboBox ModeCombo { get; }
    ComboBox SubtypeCombo { get; }
    RadioButton FrequencyShadingRadio { get; }
    RadioButton SimpleRangeRadio { get; }
    ComboBox IntervalCountCombo { get; }
    Button ChartTypeToggleButton { get; }
    PolarChart PolarChart { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? ChartTypeToggleRequested;
    event EventHandler? ModeChanged;
    event EventHandler? SubtypeChanged;
    event EventHandler? DisplayModeChanged;
    event EventHandler? IntervalCountChanged;
}
