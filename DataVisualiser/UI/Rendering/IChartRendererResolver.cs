namespace DataVisualiser.UI.Rendering;

public interface IChartRendererResolver
{
    ChartRendererKind ResolveKind(string chartKey);
    IChartRenderer ResolveRenderer(string chartKey);
}
