namespace DataVisualiser.UI.MainHost;

public sealed record MainChartsViewResolutionResetActions(
    Action MarkResolutionRefreshStarted,
    Action ClearAllCharts,
    Action ClearSelectedMetricType,
    Action ClearLastContext,
    Action ClearMetricTypeItems,
    Action ClearDynamicSubtypeSelectors,
    Action ClearSubtypeItems,
    Action DisableSubtypeSelector,
    Action<string> SetResolutionTableName,
    Action RequestMetricReload,
    Action<int> UpdatePrimaryButtonStates,
    Action<int> UpdateSecondaryButtonStates);
