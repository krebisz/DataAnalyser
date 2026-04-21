using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.Syncfusion;

public interface ISyncfusionSunburstRenderingContract
{
    Task<ChartRenderAdapterResult> RenderAsync(SyncfusionSunburstChartRenderRequest request, SyncfusionSunburstChartRenderHost host);

    void Clear(SyncfusionSunburstChartRenderHost host);

    void ResetView(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host);

    bool HasRenderableContent(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host);
}
