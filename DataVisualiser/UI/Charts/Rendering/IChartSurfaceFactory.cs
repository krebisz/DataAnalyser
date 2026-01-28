namespace DataVisualiser.UI.Charts.Rendering;

public interface IChartSurfaceFactory
{
    IChartSurface Create(string chartKey, IChartPanelHost panelHost);
}
