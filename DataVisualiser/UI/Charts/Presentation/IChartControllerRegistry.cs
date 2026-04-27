namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartControllerRegistry
{
    void Register(IChartController controller);
    IChartController Get(string key);
    IReadOnlyList<IChartController> All();
}
