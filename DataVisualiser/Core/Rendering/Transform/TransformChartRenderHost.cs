using DataVisualiser.UI.State;
using LiveCharts.Wpf;

namespace DataVisualiser.Core.Rendering.Transform;

public sealed record TransformChartRenderHost(
    CartesianChart Chart,
    ChartState ChartState,
    Action? ResetAuxiliaryVisuals = null);
