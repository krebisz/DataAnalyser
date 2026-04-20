using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Workspace.Coordination;

namespace DataVisualiser.UI.Syncfusion;

public sealed class SyncfusionChartsViewLoadCoordinator
{
    private readonly WorkspaceLoadCoordinator _workspaceLoadCoordinator = new();

    public sealed record LoadValidationInput(string? SelectedMetricType, DateTime FromDate, DateTime ToDate);

    public sealed record ValidationActions(
        Func<IDisposable> BeginSelectionStateBatch,
        Action<string?> SetSelectedMetricType,
        Action UpdateSelectedSubtypesInViewModel,
        Action<DateTime, DateTime> SetDateRange,
        Func<(bool IsValid, string? ErrorMessage)> ValidateDataLoadRequirements,
        Action<string, string> ShowWarning);

    public sealed record LoadExecutionActions(
        Func<Task<bool>> LoadMetricDataIntoLastContextAsync,
        Action ResetLastContext,
        Action PublishLastContextAndRequestChartUpdate,
        Action<string, string> ShowError);

    public sealed record ClearActions(
        Action ClearEvidence,
        Action<IReadOnlyList<MetricSeriesSelection>> SetSelectedSeries,
        Action ResetLastContext,
        Action ClearManagedChart,
        Action UpdateToggleEnabled,
        Action<string> ResetForResolutionChange,
        Action<string> SelectResolution);

    public bool ValidateAndPrepareLoad(LoadValidationInput input, ValidationActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        return _workspaceLoadCoordinator.ValidateAndPrepareLoad(
            new WorkspaceLoadCoordinator.LoadValidationInput(input.SelectedMetricType, input.FromDate, input.ToDate),
            new WorkspaceLoadCoordinator.ValidationActions(
                actions.BeginSelectionStateBatch,
                actions.SetSelectedMetricType,
                actions.UpdateSelectedSubtypesInViewModel,
                actions.SetDateRange,
                null,
                actions.ValidateDataLoadRequirements,
                actions.ShowWarning));
    }

    public async Task ExecuteLoadAsync(LoadExecutionActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        await _workspaceLoadCoordinator.ExecuteLoadAsync(
            new WorkspaceLoadCoordinator.LoadExecutionActions(
                null,
                actions.LoadMetricDataIntoLastContextAsync,
                actions.ResetLastContext,
                actions.PublishLastContextAndRequestChartUpdate,
                actions.ShowError));
    }

    public void ClearSelection(string defaultResolution, bool isDefaultResolutionSelected, ClearActions actions)
    {
        ArgumentNullException.ThrowIfNull(defaultResolution);
        ArgumentNullException.ThrowIfNull(actions);

        _workspaceLoadCoordinator.ClearSelection(
            defaultResolution,
            isDefaultResolutionSelected,
            new WorkspaceLoadCoordinator.ClearActions(
                () =>
                {
                    actions.ClearEvidence();
                    actions.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
                    actions.ResetLastContext();
                    actions.ClearManagedChart();
                    actions.UpdateToggleEnabled();
                },
                actions.ResetForResolutionChange,
                actions.SelectResolution));
    }
}
