using DataVisualiser.UI.Charts.Infrastructure;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface IChartControllerFactory
{
    ChartControllerFactoryResult Create(ChartControllerFactoryContext context);
}
