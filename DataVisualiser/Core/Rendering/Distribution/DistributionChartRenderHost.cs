using System.Windows.Controls;
using DataVisualiser.UI.State;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed record DistributionChartRenderHost(
    CartesianChart CartesianChart,
    PolarChart PolarChart,
    ChartState ChartState,
    Func<ToolTip?> GetPolarTooltip);
