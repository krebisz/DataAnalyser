namespace DataVisualiser.UI.MainHost;

public sealed record MainChartsViewStartupActions(
    Action InitializeDateRange,
    Action InitializeDefaultUiState,
    Action InitializeSubtypeSelector,
    Action InitializeResolution,
    Action InitializeCharts,
    Action RequestChartUpdate,
    Action SyncCmsToggleStates,
    Action MarkInitializationComplete,
    Action SyncInitialButtonStates);
