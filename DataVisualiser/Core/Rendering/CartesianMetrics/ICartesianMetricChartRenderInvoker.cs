namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public interface ICartesianMetricChartRenderInvoker
{
    Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host);
}
