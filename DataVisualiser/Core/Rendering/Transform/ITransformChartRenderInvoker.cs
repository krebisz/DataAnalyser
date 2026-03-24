namespace DataVisualiser.Core.Rendering.Transform;

public interface ITransformChartRenderInvoker
{
    Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host);
}
