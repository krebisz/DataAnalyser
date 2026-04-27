using DataVisualiser.Core.Rendering.CartesianMetrics;

namespace DataVisualiser.Core.Orchestration;

public interface ICartesianMetricChartRenderInvoker
{
    Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host);
}
