using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Workspace.Coordination;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewLoadCoordinator
{
    private readonly WorkspaceLoadCoordinator _workspaceLoadCoordinator = new();

    public sealed record LoadValidationInput(string? SelectedMetricType, DateTime FromDate, DateTime ToDate);

    public sealed record ValidationActions(
        Func<IDisposable> BeginSelectionStateBatch,
        Action<string?> SetSelectedMetricType,
        Action UpdateSelectedSubtypesInViewModel,
        Action<DateTime, DateTime> SetDateRange,
        Action UpdateChartTitlesFromSelections,
        Func<(bool IsValid, string? ErrorMessage)> ValidateDataLoadRequirements,
        Action<string, string> ShowWarning);

    public sealed record LoadExecutionActions(
        Action<string> ClearChartCache,
        Action ResetTransformSelectionsPendingLoad,
        Func<Task<bool>> LoadMetricDataIntoLastContextAsync,
        Action ClearAllCharts,
        Action ClearHiddenCharts,
        Action PublishLastContextAndRequestChartUpdate,
        Action<string, string> ShowError);

    public sealed record ClearActions(
        Action<string, string, string> RecordSessionMilestone,
        Action ClearEvidence,
        Action<IReadOnlyList<MetricSeriesSelection>> SetSelectedSeries,
        Action ResetLastContext,
        Action<int> UpdatePrimaryDataRequiredButtonStates,
        Action<int> UpdateSecondaryDataRequiredButtonStates,
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
                actions.UpdateChartTitlesFromSelections,
                actions.ValidateDataLoadRequirements,
                actions.ShowWarning));
    }

    public async Task ExecuteLoadAsync(LoadExecutionActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        await _workspaceLoadCoordinator.ExecuteLoadAsync(
            new WorkspaceLoadCoordinator.LoadExecutionActions(
                () =>
                {
                    actions.ClearChartCache(ChartControllerKeys.Distribution);
                    actions.ClearChartCache(ChartControllerKeys.WeeklyTrend);
                    actions.ClearChartCache(ChartControllerKeys.Normalized);
                    actions.ClearChartCache(ChartControllerKeys.DiffRatio);
                    actions.ClearChartCache(ChartControllerKeys.Transform);
                    actions.ResetTransformSelectionsPendingLoad();
                },
                actions.LoadMetricDataIntoLastContextAsync,
                actions.ClearAllCharts,
                () =>
                {
                    actions.ClearHiddenCharts();
                    actions.PublishLastContextAndRequestChartUpdate();
                },
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
                    actions.RecordSessionMilestone("ClearInvoked", "Info", "User cleared current selection and chart state.");
                    actions.ClearEvidence();
                    actions.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
                    actions.ResetLastContext();
                    actions.UpdatePrimaryDataRequiredButtonStates(0);
                    actions.UpdateSecondaryDataRequiredButtonStates(0);
                },
                actions.ResetForResolutionChange,
                actions.SelectResolution));
    }
}
