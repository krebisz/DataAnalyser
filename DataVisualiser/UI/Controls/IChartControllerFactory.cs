namespace DataVisualiser.UI.Controls;

public interface IChartControllerFactory
{
    ChartControllerFactoryResult Create(ChartControllerFactoryContext context);
}