namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewStartupCoordinator
{
    public sealed record Actions(
        Action InitializeDateRange,
        Action InitializeDefaultUiState,
        Action InitializeSubtypeSelector,
        Action InitializeResolution,
        Action InitializeCharts,
        Action RequestChartUpdate,
        Action SyncCmsToggleStates,
        Action MarkInitializationComplete,
        Action SyncInitialButtonStates);

    public void Execute(Actions actions)
    {
        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        actions.InitializeDateRange();
        actions.InitializeDefaultUiState();
        actions.InitializeSubtypeSelector();
        actions.InitializeResolution();
        actions.InitializeCharts();
        actions.RequestChartUpdate();
        actions.SyncCmsToggleStates();
        actions.MarkInitializationComplete();
        actions.SyncInitialButtonStates();
    }
}
