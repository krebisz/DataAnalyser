using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewResolutionResetCoordinator
{
    public sealed record Actions(
        Action MarkResolutionRefreshStarted,
        Action ClearAllCharts,
        Action ClearSelectedMetricType,
        Action ClearLastContext,
        Action ResetDateRange,
        Action ClearMetricTypeItems,
        Action ClearDynamicSubtypeSelectors,
        Action ClearSubtypeItems,
        Action DisableSubtypeSelector,
        Action<string> SetResolutionTableName,
        Action RequestMetricReload,
        Action<int> UpdatePrimaryButtonStates,
        Action<int> UpdateSecondaryButtonStates);

    public void ExecuteReset(string selectedResolution, Actions actions)
    {
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;
        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        actions.MarkResolutionRefreshStarted();
        actions.ClearAllCharts();
        actions.ClearSelectedMetricType();
        actions.ClearLastContext();
        actions.ResetDateRange();
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
