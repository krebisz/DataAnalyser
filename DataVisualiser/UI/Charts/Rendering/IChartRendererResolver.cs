namespace DataVisualiser.UI.Charts.Rendering;

public interface IChartRendererResolver
{
    ChartRendererKind ResolveKind(string chartKey);
    IChartRenderer ResolveRenderer(string chartKey);
}
