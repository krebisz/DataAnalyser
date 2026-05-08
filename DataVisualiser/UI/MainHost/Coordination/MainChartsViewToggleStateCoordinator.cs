using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.State;
namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewToggleStateCoordinator
{
    public sealed record Actions(
        Func<string, IChartController> ResolveController,
        Action<string> ClearChart,
        Action SetNormalizedHidden,
        Action SetDiffRatioHidden);

    public void UpdatePrimaryChartToggles(LoadedChartDataSnapshot snapshot, int selectedSubtypeCount, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var canToggle = MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(snapshot);

        var mainController = actions.ResolveController(ChartControllerKeys.Main);
        mainController.SetToggleEnabled(canToggle);
        UpdateMainChartStackedAvailability(snapshot, selectedSubtypeCount, mainController);
        actions.ResolveController(ChartControllerKeys.WeeklyTrend).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.Distribution).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.Transform).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.BarPie).SetToggleEnabled(canToggle);
    }

    public void UpdateSecondaryChartToggles(LoadedChartDataSnapshot snapshot, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var hasSecondaryData = MainChartsViewToggleStateEvaluator.HasLoadedSecondaryData(snapshot);
        var canToggle = MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(snapshot);

        if (!hasSecondaryData)
        {
            actions.ClearChart(ChartControllerKeys.Normalized);
            actions.SetNormalizedHidden();
            actions.ClearChart(ChartControllerKeys.DiffRatio);
            actions.SetDiffRatioHidden();
        }

        actions.ResolveController(ChartControllerKeys.Normalized).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.DiffRatio).SetToggleEnabled(canToggle);
    }

    private static void UpdateMainChartStackedAvailability(LoadedChartDataSnapshot snapshot, int selectedSubtypeCount, IChartController controller)
    {
        var canStack = MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(snapshot, selectedSubtypeCount);
        if (controller is IMainChartControllerExtras extras)
            extras.SetStackedAvailability(canStack);
    }
}
