using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed record CartesianMetricChartRenderHost(
    CartesianChart Chart,
    ChartState ChartState);
