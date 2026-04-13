using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewToggleStateCoordinator
{
    public sealed record Actions(
        Func<string, IChartController> ResolveController,
        Action<string> ClearChart,
        Action SetNormalizedHidden,
        Action SetDiffRatioHidden);

    public void UpdatePrimaryChartToggles(ChartDataContext? context, int selectedSubtypeCount, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var canToggle = MainChartsViewToggleStateEvaluator.CanTogglePrimaryCharts(context);

        var mainController = actions.ResolveController(ChartControllerKeys.Main);
        mainController.SetToggleEnabled(canToggle);
        UpdateMainChartStackedAvailability(context, selectedSubtypeCount, mainController);
        actions.ResolveController(ChartControllerKeys.WeeklyTrend).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.Distribution).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.Transform).SetToggleEnabled(canToggle);
        actions.ResolveController(ChartControllerKeys.BarPie).SetToggleEnabled(canToggle);
    }

    public void UpdateSecondaryChartToggles(ChartDataContext? context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        var hasSecondaryData = MainChartsViewToggleStateEvaluator.HasLoadedSecondaryData(context);
        var canToggle = MainChartsViewToggleStateEvaluator.CanToggleSecondaryCharts(context);

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

    private static void UpdateMainChartStackedAvailability(ChartDataContext? context, int selectedSubtypeCount, IChartController controller)
    {
        var canStack = MainChartsViewToggleStateEvaluator.CanUseStackedDisplay(context, selectedSubtypeCount);
        if (controller is IMainChartControllerExtras extras)
            extras.SetStackedAvailability(canStack);
    }
}
