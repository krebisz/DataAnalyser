using DataVisualiser.Core.Rendering.Transform;

namespace DataVisualiser.Core.Orchestration;

public interface ITransformChartRenderInvoker
{
    Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host);
}
