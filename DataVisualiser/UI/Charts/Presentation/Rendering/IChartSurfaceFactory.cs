namespace DataVisualiser.UI.Charts.Presentation.Rendering;

public interface IChartSurfaceFactory
{
    IChartSurface Create(string chartKey, IChartPanelHost panelHost);
}
