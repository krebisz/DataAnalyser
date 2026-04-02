using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IChartControllerFactory
{
    ChartControllerFactoryResult Create(ChartControllerFactoryContext context);
}
