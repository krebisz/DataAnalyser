using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.SyncfusionViews;

public sealed class SyncfusionChartsViewLoadCoordinator
{
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
            var dataLoaded = await actions.LoadMetricDataIntoLastContextAsync();
            if (!dataLoaded)
            {
                actions.ResetLastContext();
                return;
            }

            actions.PublishLastContextAndRequestChartUpdate();
        }
        catch (Exception ex)
        {
            actions.ShowError("Error", $"Error loading data: {ex.Message}");
            actions.ResetLastContext();
        }
    }

    public void ClearSelection(string defaultResolution, bool isDefaultResolutionSelected, ClearActions actions)
    {
        ArgumentNullException.ThrowIfNull(defaultResolution);
        ArgumentNullException.ThrowIfNull(actions);

        actions.ClearEvidence();
        actions.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
        actions.ResetLastContext();
        actions.ClearManagedChart();
        actions.UpdateToggleEnabled();

        if (isDefaultResolutionSelected)
        {
            actions.ResetForResolutionChange(defaultResolution);
            return;
        }

        actions.SelectResolution(defaultResolution);
    }
}
