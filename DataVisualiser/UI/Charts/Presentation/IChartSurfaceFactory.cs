namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartSurfaceFactory
{
    IChartSurface Create(string chartKey, IChartPanelHost panelHost);
}
