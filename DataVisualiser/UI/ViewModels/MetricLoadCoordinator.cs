using System.Diagnostics;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration.Builders;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public sealed class MetricLoadCoordinator
{
    private readonly ChartState _chartState;
    private readonly Func<Exception, string> _formatError;
    private readonly MetricSelectionService _metricService;
    private readonly MetricState _metricState;
    private readonly UiState _uiState;
    private readonly DataLoadValidator _validator;

    public MetricLoadCoordinator(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService, DataLoadValidator validator, Func<Exception, string> formatError)
    {
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _metricState = metricState ?? throw new ArgumentNullException(nameof(metricState));
        _uiState = uiState ?? throw new ArgumentNullException(nameof(uiState));
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _formatError = formatError ?? throw new ArgumentNullException(nameof(formatError));
    }

    public async Task LoadMetricsAsync(Action<MetricTypesLoadedEventArgs> onLoaded, Action<string> onError)
    {
        if (_uiState.IsLoadingMetricTypes)
            return;

        try
        {
            _uiState.IsLoadingMetricTypes = true;

            // Resolution table name must be set by the UI before this point.
            var tableName = _metricState.ResolutionTableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                onError("Resolution is not selected. Please select a resolution before loading metric types.");
                return;
            }

            var metricTypes = await _metricService.LoadMetricTypesAsync(tableName);

            onLoaded(new MetricTypesLoadedEventArgs
            {
                    MetricTypes = metricTypes
            });
        }
        catch (Exception ex)
        {
            onError(_formatError(ex));
        }
        finally
        {
            _uiState.IsLoadingMetricTypes = false;
        }
    }

    public async Task LoadSubtypesAsync(Action<SubtypesLoadedEventArgs> onLoaded, Action<string> onError)
    {
        if (_uiState.IsLoadingSubtypes)
            return;

        try
        {
            _uiState.IsLoadingSubtypes = true;

            if (!_validator.ValidateMetricTypeSelected(out var metricError))
            {
                onError(metricError);
                return;
            }

            var metricType = _metricState.SelectedMetricType!;
            var tableName = _metricState.ResolutionTableName;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                onError("Resolution is not selected. Please select a resolution before loading subtypes.");
                return;
            }

            var subtypes = await _metricService.LoadSubtypesAsync(metricType, tableName);

            onLoaded(new SubtypesLoadedEventArgs
            {
                    Subtypes = subtypes
            });
        }
        catch (Exception ex)
        {
            onError(_formatError(ex));
        }
        finally
        {
            _uiState.IsLoadingSubtypes = false;
        }
    }

    public async Task<bool> LoadMetricDataAsync(Action<ErrorEventArgs> onError)
    {
        if (!ValidateDataLoadPrerequisites(out var validationError, onError))
            return false;

        var metricType = _metricState.SelectedMetricType!;
        var tableName = _metricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;

        // IMPORTANT: Ensure consistent ordering - first selected subtype is always primary (data1),
        // second selected subtype is always secondary (data2). This ordering is maintained
        // across all charts and strategies.
        var (primarySubtype, secondarySubtype) = ExtractPrimaryAndSecondarySubtypes();

        try
        {
            _uiState.IsLoadingData = true;

            var dataLoaded = await LoadAndValidateMetricDataAsync(metricType, primarySubtype, secondarySubtype, tableName, onError);

            if (!dataLoaded)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            onError(new ErrorEventArgs
            {
                    Message = _formatError(ex)
            });
            _chartState.LastContext = null;
            return false;
        }
        finally
        {
            _uiState.IsLoadingData = false;
        }
    }

    public async Task LoadDateRangeForSelectedMetricAsync(Action<DateRangeLoadedEventArgs> onLoaded, Action<ErrorEventArgs> onError)
    {
        try
        {
            // Basic guard: we must have a selected metric type
            // But don't show error if we're in a transitional state (e.g., resolution change)
            if (!_validator.ValidateMetricTypeSelected())
                    // Silently return without showing error - this prevents popups during resolution changes
                return;

            var metricType = _metricState.SelectedMetricType!;
            var tableName = _metricState.ResolutionTableName!;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                onError(new ErrorEventArgs
                {
                        Message = "Resolution table name is missing - cannot load date range."
                });
                return;
            }

            // Use the *primary* selected subtype (if any) for the date range.
            // If the first subtype is "(All)" or empty, we pass null to mean "all subtypes".
            string? primarySubtype = null;
            if (_metricState.SelectedSubtypes.Any())
            {
                var first = _metricState.SelectedSubtypes.First();
                if (!string.IsNullOrWhiteSpace(first) && first != "(All)")
                    primarySubtype = first;
            }

            var dateRange = await _metricService.LoadDateRangeAsync(metricType, primarySubtype, tableName);

            if (!dateRange.HasValue)
            {
                onError(new ErrorEventArgs
                {
                        Message = "No date range could be determined for the current selection."
                });
                return;
            }

            // Update state
            _metricState.FromDate = dateRange.Value.MinDate;
            _metricState.ToDate = dateRange.Value.MaxDate;

            // Notify the view so it can update the DatePicker controls
            onLoaded(new DateRangeLoadedEventArgs
            {
                    MinDate = dateRange.Value.MinDate,
                    MaxDate = dateRange.Value.MaxDate
            });
        }
        catch (Exception ex)
        {
            onError(new ErrorEventArgs
            {
                    Message = _formatError(ex)
            });
        }
    }

    /// <summary>
    ///     Validates all prerequisites for loading metric data.
    ///     Returns false and raises error event if validation fails.
    /// </summary>
    private bool ValidateDataLoadPrerequisites(out string? validationError, Action<ErrorEventArgs> onError)
    {
        validationError = null;

        if (!_validator.ValidateDataLoadRequirements(out validationError))
        {
            onError(new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        if (_metricState.FromDate == null || _metricState.ToDate == null)
        {
            validationError = "Please select a valid date range before loading data.";
            onError(new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Extracts primary and secondary subtypes from the selected subtypes list.
    ///     IMPORTANT: First selected subtype is always primary (data1),
    ///     second selected subtype is always secondary (data2).
    /// </summary>
    private(string? primarySubtype, string? secondarySubtype) ExtractPrimaryAndSecondarySubtypes()
    {
        var primarySubtype = _metricState.SelectedSubtypes.Count > 0 ?
                _metricState.SelectedSubtypes[0] // First selected subtype = primary
                :
                null;

        var secondarySubtype = _metricState.SelectedSubtypes.Count > 1 ?
                _metricState.SelectedSubtypes[1] // Second selected subtype = secondary
                :
                null;

        return (primarySubtype, secondarySubtype);
    }

    /// <summary>
    ///     Loads and validates metric data, then builds the chart data context.
    ///     Returns false if data loading or validation fails.
    /// </summary>
    private async Task<bool> LoadAndValidateMetricDataAsync(string metricType, string? primarySubtype, string? secondarySubtype, string tableName, Action<ErrorEventArgs> onError)
    {
        // Load data: data1 = first selected subtype (primary), data2 = second selected subtype (secondary)
        var (primaryCms, secondaryCms, data1, data2) = await _metricService.LoadMetricDataWithCmsAsync(metricType, primarySubtype, secondarySubtype, _metricState.FromDate!.Value, _metricState.ToDate!.Value, tableName);

        // When only one subtype is selected, ensure data2 is empty to prevent mixing with all subtypes
        // GetHealthMetricsDataByBaseType with null subtype returns ALL subtypes, which corrupts the chart
        if (secondarySubtype == null)
            data2 = Enumerable.Empty<MetricData>();

        EventHandler<ErrorEventArgs> errorHandler = (_, args) => onError(args);

        // Validate primary data
        if (!MetricDataValidationHelper.ValidatePrimaryData(metricType, primarySubtype, primaryCms, data1, errorHandler))
        {
            _chartState.LastContext = null;
            return false;
        }

        // Validate secondary data (only if secondary subtype is selected)
        if (!MetricDataValidationHelper.ValidateSecondaryData(metricType, secondarySubtype, secondaryCms, data2, errorHandler))
        {
            _chartState.LastContext = null;
            return false;
        }

        // NEW: delegate context construction to the builder
        var ctxBuilder = new ChartDataContextBuilder();

        _chartState.LastContext = ctxBuilder.Build(metricType, primarySubtype, secondarySubtype, data1, data2, _metricState.FromDate.Value, _metricState.ToDate.Value, primaryCms, secondaryCms);

        Debug.WriteLine($"[CTX] SemanticMetricCount={_chartState.LastContext.SemanticMetricCount}, " + $"PrimaryCms={(_chartState.LastContext.PrimaryCms == null ? "NULL" : "SET")}");

        return true;
    }
}