using DataVisualiser.UI.Charts.Presentation.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public sealed record BarPieChartRenderHost(
    IChartSurface Surface,
    IChartRenderer Renderer,
    ChartRendererKind RendererKind,
    bool IsVisible);
