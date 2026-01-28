namespace DataVisualiser.UI.Charts.Interfaces;

public interface IChartControllerRegistry
{
    void Register(IChartController controller);
    IChartController Get(string key);
    IReadOnlyList<IChartController> All();
}
