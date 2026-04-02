namespace DataVisualiser.UI.Charts.Presentation.Rendering;

public interface IChartRendererResolver
{
    ChartRendererKind ResolveKind(string chartKey);
    IChartRenderer ResolveRenderer(string chartKey);
}
