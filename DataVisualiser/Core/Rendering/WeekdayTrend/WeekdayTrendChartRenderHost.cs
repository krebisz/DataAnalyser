using DataVisualiser.UI.State;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed record WeekdayTrendChartRenderHost(
    CartesianChart CartesianChart,
    CartesianChart PolarChart,
    ChartState ChartState);
