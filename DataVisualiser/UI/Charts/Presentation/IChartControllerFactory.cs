using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.Charts.Presentation;

public interface IChartControllerFactory
{
    ChartControllerFactoryResult Create(ChartControllerFactoryContext context);
}
