using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewLoadCoordinator
{
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

        if (input.SelectedMetricType == null)
        {
            actions.ShowWarning("No Selection", "Please select a Metric Type");
            return false;
        }

        using (actions.BeginSelectionStateBatch())
        {
            actions.SetSelectedMetricType(input.SelectedMetricType);
            actions.UpdateSelectedSubtypesInViewModel();
            actions.SetDateRange(input.FromDate, input.ToDate);
        }

        actions.UpdateChartTitlesFromSelections();

        var (isValid, errorMessage) = actions.ValidateDataLoadRequirements();
        if (!isValid)
        {
            actions.ShowWarning("Invalid Selection", errorMessage ?? "The current selection is not valid.");
            return false;
        }

        return true;
    }

    public async Task ExecuteLoadAsync(LoadExecutionActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        try
        {
            actions.ClearChartCache(ChartControllerKeys.Distribution);
            actions.ClearChartCache(ChartControllerKeys.WeeklyTrend);
            actions.ClearChartCache(ChartControllerKeys.Normalized);
            actions.ClearChartCache(ChartControllerKeys.DiffRatio);
            actions.ClearChartCache(ChartControllerKeys.Transform);
            actions.ResetTransformSelectionsPendingLoad();

            var dataLoaded = await actions.LoadMetricDataIntoLastContextAsync();
            if (!dataLoaded)
            {
                actions.ClearAllCharts();
                return;
            }

            actions.ClearHiddenCharts();
            actions.PublishLastContextAndRequestChartUpdate();
        }
        catch (Exception ex)
        {
            actions.ShowError("Error", $"Error loading data: {ex.Message}");
            actions.ClearAllCharts();
        }
    }

    public void ClearSelection(string defaultResolution, bool isDefaultResolutionSelected, ClearActions actions)
    {
        ArgumentNullException.ThrowIfNull(defaultResolution);
        ArgumentNullException.ThrowIfNull(actions);

        actions.RecordSessionMilestone("ClearInvoked", "Info", "User cleared current selection and chart state.");
        actions.ClearEvidence();
        actions.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
        actions.ResetLastContext();
        actions.UpdatePrimaryDataRequiredButtonStates(0);
        actions.UpdateSecondaryDataRequiredButtonStates(0);

        if (isDefaultResolutionSelected)
        {
            actions.ResetForResolutionChange(defaultResolution);
            return;
        }

        actions.SelectResolution(defaultResolution);
    }
}
