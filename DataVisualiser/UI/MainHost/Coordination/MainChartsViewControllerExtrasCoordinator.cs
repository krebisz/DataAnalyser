using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewControllerExtrasCoordinator
{
    public sealed record Actions(Func<string, IChartController> ResolveController);

    public void InitializeBarPieControls(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IBarPieChartControllerExtras>(ChartControllerKeys.BarPie, actions)?.InitializeControls();
    }

    public void InitializeDistributionControls(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IDistributionChartControllerExtras>(ChartControllerKeys.Distribution, actions)?.InitializeControls();
    }

    public void InitializeWeekdayTrendControls(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IWeekdayTrendChartControllerExtras>(ChartControllerKeys.WeeklyTrend, actions)?.InitializeControls();
    }

    public void UpdateDistributionChartTypeVisibility(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IDistributionChartControllerExtras>(ChartControllerKeys.Distribution, actions)?.UpdateChartTypeVisibility();
    }

    public void UpdateWeekdayTrendChartTypeVisibility(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IWeekdayTrendChartControllerExtras>(ChartControllerKeys.WeeklyTrend, actions)?.UpdateChartTypeVisibility();
    }

    public void CompleteTransformSelectionsPendingLoad(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<ITransformPanelControllerExtras>(ChartControllerKeys.Transform, actions)?.CompleteSelectionsPendingLoad();
    }

    public void ResetTransformSelectionsPendingLoad(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<ITransformPanelControllerExtras>(ChartControllerKeys.Transform, actions)?.ResetSelectionsPendingLoad();
    }

    public void HandleTransformVisibilityOnlyToggle(ChartDataContext? context, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<ITransformPanelControllerExtras>(ChartControllerKeys.Transform, actions)?.HandleVisibilityOnlyToggle(context);
    }

    public void UpdateTransformSubtypeOptions(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<ITransformPanelControllerExtras>(ChartControllerKeys.Transform, actions)?.UpdateTransformSubtypeOptions();
    }

    public void UpdateTransformComputeButtonState(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<ITransformPanelControllerExtras>(ChartControllerKeys.Transform, actions)?.UpdateTransformComputeButtonState();
    }

    public void UpdateDiffRatioOperationButton(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IDiffRatioChartControllerExtras>(ChartControllerKeys.DiffRatio, actions)?.UpdateOperationButton();
    }

    public void SyncMainDisplayModeSelection(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ResolveExtras<IMainChartControllerExtras>(ChartControllerKeys.Main, actions)?.SyncDisplayModeSelection();
    }

    private static TExtras? ResolveExtras<TExtras>(string key, Actions actions)
        where TExtras : class
    {
        return actions.ResolveController(key) as TExtras;
    }
}
