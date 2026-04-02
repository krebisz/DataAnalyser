using DataVisualiser.UI.Charts.Presentation.Rendering;

namespace DataVisualiser.Core.Rendering.BarPie;

public interface IBarPieRenderingContract
{
    IReadOnlyList<BarPieBackendQualification> GetBackendQualificationMatrix();

    BarPieRenderingCapabilities GetCapabilities(ChartRendererKind rendererKind, BarPieRenderingRoute route);

    Task RenderAsync(BarPieChartRenderRequest request, BarPieChartRenderHost host);

    Task ClearAsync(BarPieChartRenderHost host);

    void ResetView(BarPieRenderingRoute route, BarPieChartRenderHost host);

    bool HasRenderableContent(BarPieRenderingRoute route, BarPieChartRenderHost host);
}
