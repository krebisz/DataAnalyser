using System;
using System.Windows.Controls;
using DataVisualiser.UI.Events;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public interface IWeekdayTrendChartController
{
    CartesianChart Chart { get; }
    CartesianChart PolarChart { get; }
    ChartPanelController Panel { get; }
    Button ToggleButton { get; }
    Button ChartTypeToggleButton { get; }
    ComboBox SubtypeCombo { get; }
    ComboBox AverageWindowCombo { get; }

    event EventHandler? ToggleRequested;
    event EventHandler? ChartTypeToggleRequested;
    event EventHandler? SubtypeChanged;
    event EventHandler<WeekdayTrendDayToggleEventArgs>? DayToggled;
    event EventHandler<WeekdayTrendAverageToggleEventArgs>? AverageToggled;
    event EventHandler? AverageWindowChanged;
}
