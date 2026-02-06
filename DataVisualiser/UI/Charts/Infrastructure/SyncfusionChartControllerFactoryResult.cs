using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Interfaces;

namespace DataVisualiser.UI.Charts.Infrastructure;

public sealed record SyncfusionChartControllerFactoryResult(
    SyncfusionSunburstChartControllerAdapter Sunburst,
    IChartControllerRegistry Registry);
