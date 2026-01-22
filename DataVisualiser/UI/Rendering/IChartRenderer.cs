namespace DataVisualiser.UI.Rendering;

public interface IChartRenderer
{
    void Apply(IChartSurface surface, ChartRenderModel model);
}