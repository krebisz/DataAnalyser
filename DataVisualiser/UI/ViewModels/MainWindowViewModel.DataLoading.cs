namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel
{
    private Task LoadMetricsAsync()
    {
        return _metricLoadCoordinator.LoadMetricsAsync(args => MetricTypesLoaded?.Invoke(this, args), RaiseError);
    }

    private Task LoadSubtypesAsync()
    {
        return _metricLoadCoordinator.LoadSubtypesAsync(args => SubtypesLoaded?.Invoke(this, args), RaiseError);
    }

    /// <summary>
    ///     Phase 6 - Step 2.1
    ///     Centralized data load for the currently selected metric + subtypes + date range.
    ///     Builds ChartState.LastContext from the DB and returns true if charts can be rendered.
    /// </summary>
    public async Task<bool> LoadMetricDataAsync()
    {
        return await _metricLoadCoordinator.LoadMetricDataAsync(args => ErrorOccured?.Invoke(this, args));
    }

    private async Task LoadDateRangeForSelectedMetricAsync()
    {
        await _metricLoadCoordinator.LoadDateRangeForSelectedMetricAsync(args => DateRangeLoaded?.Invoke(this, args), args => ErrorOccured?.Invoke(this, args));
    }

    public Task RefreshDateRangeForCurrentSelectionAsync()
    {
        return LoadDateRangeForSelectedMetricAsync();
    }

    private void LoadData()
    {
        if (UiState.IsLoadingData)
            return;

        try
        {
            UiState.IsLoadingData = true;

            // VM owns validation - UI just forwards the command.
            if (!ValidateDataLoadRequirements(out var errorMessage))
            {
                RaiseError(errorMessage);
                return;
            }

            // At this point MainWindow should already have populated LastContext.
            var ctx = ChartState.LastContext;
            if (ctx == null || ctx.Data1 == null || !ctx.Data1.Any())
            {
                RaiseError("No data is available to render charts. Please load data first.");
                return;
            }

            DataLoaded?.Invoke(this,
                    new DataLoadedEventArgs
                    {
                            DataContext = ctx
                    });

            // After data is confirmed, request charts to update.
            RequestChartUpdate();
        }
        catch (Exception ex)
        {
            RaiseError(FormatDatabaseError(ex));
        }
        finally
        {
            UiState.IsLoadingData = false;
        }
    }

    private bool CanLoadData()
    {
        // Minimal guard so the button isn't enabled when obviously invalid.
        return ValidateMetricTypeSelected() && MetricState.FromDate.HasValue && MetricState.ToDate.HasValue;
    }
}