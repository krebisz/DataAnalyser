using DataVisualiser.UI.Charts.Helpers;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewResolutionResetCoordinator
{
    public void ExecuteReset(string selectedResolution, MainChartsViewResolutionResetActions actions)
    {
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;
        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        actions.MarkResolutionRefreshStarted();
        actions.ClearAllCharts();
        actions.ClearSelectedMetricType();
        actions.ClearLastContext();
        actions.ClearMetricTypeItems();
        actions.ClearDynamicSubtypeSelectors();
        actions.ClearSubtypeItems();
        actions.DisableSubtypeSelector();
        actions.SetResolutionTableName(ChartUiHelper.GetTableNameFromResolution(selectedResolution));
        actions.RequestMetricReload();
        actions.UpdatePrimaryButtonStates(0);
        actions.UpdateSecondaryButtonStates(0);
    }

    public void HandleSuppressedError(Action clearResolutionRefreshFlag, Action clearAllCharts)
    {
        clearResolutionRefreshFlag?.Invoke();
        clearAllCharts?.Invoke();
    }
}
