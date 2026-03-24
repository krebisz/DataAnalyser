namespace DataVisualiser.Core.Rendering.Transform;

public interface ITransformRenderingContract
{
    IReadOnlyList<TransformBackendQualification> GetBackendQualificationMatrix();
    TransformRenderingCapabilities GetCapabilities(TransformRenderingRoute route);
    Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host);
    void Clear(TransformRenderingRoute route, TransformChartRenderHost host);
    void ResetView(TransformRenderingRoute route, TransformChartRenderHost host);
    bool HasRenderableContent(TransformRenderingRoute route, TransformChartRenderHost host);
}
