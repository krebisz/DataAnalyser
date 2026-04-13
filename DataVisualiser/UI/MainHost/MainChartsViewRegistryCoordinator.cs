using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewRegistryCoordinator
{
    public sealed record Actions(
        Func<IReadOnlyList<IChartController>?> GetRegisteredControllers,
        Func<string, IChartController> ResolveController);

    public IReadOnlyList<IChartController> ResolveControllers(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var controllers = actions.GetRegisteredControllers();
        if (controllers is { Count: > 0 })
            return controllers;

        return ChartControllerKeys.All
            .Select(actions.ResolveController)
            .ToList();
    }

    public void ClearRegisteredCharts(ChartState chartState, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        foreach (var controller in ResolveControllers(actions))
            controller.Clear(chartState);
    }
}
