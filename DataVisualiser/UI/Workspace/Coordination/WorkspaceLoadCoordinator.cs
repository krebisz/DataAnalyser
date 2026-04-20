namespace DataVisualiser.UI.Workspace.Coordination;

public sealed class WorkspaceLoadCoordinator
{
    public sealed record LoadValidationInput(string? SelectedMetricType, DateTime FromDate, DateTime ToDate);

    public sealed record ValidationActions(
        Func<IDisposable> BeginSelectionStateBatch,
        Action<string?> SetSelectedMetricType,
        Action UpdateSelectedSubtypesInViewModel,
        Action<DateTime, DateTime> SetDateRange,
        Action? AfterSelectionPrepared,
        Func<(bool IsValid, string? ErrorMessage)> ValidateDataLoadRequirements,
        Action<string, string> ShowWarning);

    public sealed record LoadExecutionActions(
        Action? BeforeLoad,
        Func<Task<bool>> LoadMetricDataIntoLastContextAsync,
        Action OnLoadReturnedFalse,
        Action OnLoadSucceeded,
        Action<string, string> ShowError);

    public sealed record ClearActions(
        Action ResetSelectionState,
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

        actions.AfterSelectionPrepared?.Invoke();

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
            actions.BeforeLoad?.Invoke();

            var dataLoaded = await actions.LoadMetricDataIntoLastContextAsync();
            if (!dataLoaded)
            {
                actions.OnLoadReturnedFalse();
                return;
            }

            actions.OnLoadSucceeded();
        }
        catch (Exception ex)
        {
            actions.ShowError("Error", $"Error loading data: {ex.Message}");
            actions.OnLoadReturnedFalse();
        }
    }

    public void ClearSelection(string defaultResolution, bool isDefaultResolutionSelected, ClearActions actions)
    {
        ArgumentNullException.ThrowIfNull(defaultResolution);
        ArgumentNullException.ThrowIfNull(actions);

        actions.ResetSelectionState();

        if (isDefaultResolutionSelected)
        {
            actions.ResetForResolutionChange(defaultResolution);
            return;
        }

        actions.SelectResolution(defaultResolution);
    }
}
