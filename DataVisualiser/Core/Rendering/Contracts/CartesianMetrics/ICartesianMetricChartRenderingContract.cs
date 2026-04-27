namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public interface ICartesianMetricChartRenderingContract
{
    IReadOnlyList<CartesianMetricBackendQualification> GetBackendQualificationMatrix();
    CartesianMetricRenderingCapabilities GetCapabilities(CartesianMetricChartRoute route);
    Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host);
    void Clear(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host);
    void ResetView(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host);
    bool HasRenderableContent(CartesianMetricChartRoute route, CartesianMetricChartRenderHost host);
}
