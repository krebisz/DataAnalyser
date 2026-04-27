namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartRendererResolver
{
    ChartRendererKind ResolveKind(string chartKey);
    IChartRenderer ResolveRenderer(string chartKey);
}
