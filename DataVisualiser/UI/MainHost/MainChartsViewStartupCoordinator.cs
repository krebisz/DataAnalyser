namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewStartupCoordinator
{
    public void Execute(MainChartsViewStartupActions actions)
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
