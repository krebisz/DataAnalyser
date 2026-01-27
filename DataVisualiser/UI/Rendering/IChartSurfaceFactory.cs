namespace DataVisualiser.UI.Rendering;

public interface IChartSurfaceFactory
{
    IChartSurface Create(string chartKey, IChartPanelHost panelHost);
}
