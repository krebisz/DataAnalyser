using System.Diagnostics;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Validation.DataLoad;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.Shared;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
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
    private readonly VNextMainChartIntegrationCoordinator _vnextMainChartIntegrationCoordinator;

    private MetricLoadCoordinator(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService, DataLoadValidator validator, Func<Exception, string> formatError, VNextMainChartIntegrationCoordinator? vnextMainChartIntegrationCoordinator = null)
    {
        _chartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        _metricState = metricState ?? throw new ArgumentNullException(nameof(metricState));
        _uiState = uiState ?? throw new ArgumentNullException(nameof(uiState));
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _formatError = formatError ?? throw new ArgumentNullException(nameof(formatError));
        _vnextMainChartIntegrationCoordinator = vnextMainChartIntegrationCoordinator ?? new VNextMainChartIntegrationCoordinator(metricService);
    }

    public static MetricLoadCoordinator CreateInstance(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService, DataLoadValidator validator, Func<Exception, string> formatError)
    {
        return new MetricLoadCoordinator(chartState, metricState, uiState, metricService, validator, formatError);
    }

    internal static MetricLoadCoordinator CreateInstance(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService, DataLoadValidator validator, Func<Exception, string> formatError, VNextMainChartIntegrationCoordinator vnextMainChartIntegrationCoordinator)
    {
        return new MetricLoadCoordinator(chartState, metricState, uiState, metricService, validator, formatError, vnextMainChartIntegrationCoordinator);
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

            // Mark subtype loading as complete before raising the loaded callback so
            // host follow-up work (for example date-range refresh for the new selection)
            // does not immediately trip the IsLoadingSubtypes guard and get dropped.
            _uiState.IsLoadingSubtypes = false;

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

    public async Task<bool> LoadMetricDataAsync(MetricLoadRequest request, Action<ErrorEventArgs> onError)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!ValidateDataLoadPrerequisites(request, out var validationError, onError))
            return false;

        if (request.SelectedSeries.Count == 0)
        {
            onError(new ErrorEventArgs
            {
                    Message = "Please select at least one Metric Subtype before loading data."
            });
            return false;
        }

        // IMPORTANT: Ensure consistent ordering - first selected series is always primary (data1),
        // second selected series is always secondary (data2). This ordering is maintained
        // across all charts and strategies.
        var (primarySelection, secondarySelection) = ExtractPrimaryAndSecondarySelections(request);

        try
        {
            _uiState.IsLoadingData = true;

            if (ShouldUseVNextMainFamilyPath())
            {
                var vnextResult = await _vnextMainChartIntegrationCoordinator.LoadMainChartAsync(request, _chartState.MainChartDisplayMode);
                if (vnextResult.Success && vnextResult.ProjectedContext != null)
                {
                    var supportsOnlyMainChart = SupportsOnlyMainChartRoute();
                    _chartState.LastContext = vnextResult.ProjectedContext;
                    _chartState.LastLoadRuntime = new LoadRuntimeState(
                        EvidenceRuntimePath.VNextMain,
                        vnextResult.RequestSignature ?? request.Signature,
                        vnextResult.SnapshotSignature,
                        vnextResult.ProgramKind,
                        vnextResult.ProgramSourceSignature,
                        vnextResult.ProjectedContextSignature,
                        null,
                        supportsOnlyMainChart);
                    return true;
                }

                _chartState.LastLoadRuntime = new LoadRuntimeState(
                    EvidenceRuntimePath.VNextMain,
                    vnextResult.RequestSignature ?? request.Signature,
                    vnextResult.SnapshotSignature,
                    vnextResult.ProgramKind,
                    vnextResult.ProgramSourceSignature,
                    vnextResult.ProjectedContextSignature,
                    vnextResult.FailureReason,
                    false);
            }

            var dataLoaded = await LoadAndValidateMetricDataAsync(request, primarySelection, secondarySelection, onError);

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
            if (_chartState.LastLoadRuntime?.RuntimePath != EvidenceRuntimePath.VNextMain)
                _chartState.LastLoadRuntime = null;
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

            var tableName = _metricState.ResolutionTableName!;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                onError(new ErrorEventArgs
                {
                        Message = "Resolution table name is missing - cannot load date range."
                });
                return;
            }

            var metricType = _metricState.SelectedMetricType;
            if (string.IsNullOrWhiteSpace(metricType))
                return;

            var selections = _metricState.SelectedSeries;
            var dateRange = await _metricService.LoadDateRangeForSelectionsAsync(metricType, selections, tableName);

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
    private bool ValidateDataLoadPrerequisites(MetricLoadRequest request, out string? validationError, Action<ErrorEventArgs> onError)
    {
        validationError = null;

        if (string.IsNullOrWhiteSpace(request.MetricType))
        {
            validationError = "Please select a Metric Type before loading data.";
            onError(new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        if (request.From > request.To)
        {
            validationError = "From date must be before To date.";
            onError(new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ResolutionTableName))
        {
            validationError = "Resolution is not selected. Please select a resolution before loading data.";
            onError(new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        if (request.SelectedSeries.Count == 0)
        {
            validationError = "Please select at least one Metric Subtype before loading data.";
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
    private static(MetricSeriesSelection Primary, MetricSeriesSelection? Secondary) ExtractPrimaryAndSecondarySelections(MetricLoadRequest request)
    {
        var primary = request.SelectedSeries[0];
        var secondary = request.SelectedSeries.Count > 1 ? request.SelectedSeries[1] : null;

        return (primary, secondary);
    }

    /// <summary>
    ///     Loads and validates metric data, then builds the chart data context.
    ///     Returns false if data loading or validation fails.
    /// </summary>
    private async Task<bool> LoadAndValidateMetricDataAsync(MetricLoadRequest request, MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, Action<ErrorEventArgs> onError)
    {
        // Load data: data1 = first selected subtype (primary), data2 = second selected subtype (secondary)
        var (primaryCms, secondaryCms, data1, data2) = await _metricService.LoadMetricDataWithCmsAsync(primarySelection, secondarySelection, request.From, request.To, request.ResolutionTableName);

        // When only one subtype is selected, ensure data2 is empty to prevent mixing with all subtypes
        // GetHealthMetricsDataByBaseType with null subtype returns ALL subtypes, which corrupts the chart
        if (secondarySelection == null)
            data2 = Enumerable.Empty<MetricData>();

        EventHandler<ErrorEventArgs> errorHandler = (_, args) => onError(args);

        // Validate primary data
        if (!MetricDataValidationHelper.ValidatePrimaryData(primarySelection.MetricType, primarySelection.QuerySubtype, primaryCms, data1, errorHandler))
        {
            _chartState.LastContext = null;
            if (_chartState.LastLoadRuntime?.RuntimePath != EvidenceRuntimePath.VNextMain)
                _chartState.LastLoadRuntime = null;
            return false;
        }

        // Validate secondary data (only if secondary subtype is selected)
        if (secondarySelection != null && !MetricDataValidationHelper.ValidateSecondaryData(secondarySelection.MetricType, secondarySelection.QuerySubtype, secondaryCms, data2, errorHandler))
        {
            _chartState.LastContext = null;
            if (_chartState.LastLoadRuntime?.RuntimePath != EvidenceRuntimePath.VNextMain)
                _chartState.LastLoadRuntime = null;
            return false;
        }

        // NEW: delegate context construction to the builder
        var ctxBuilder = new ChartDataContextBuilder();

        _chartState.LastContext = ctxBuilder.Build(primarySelection, secondarySelection, data1, data2, request.From, request.To, primaryCms, secondaryCms);
        _chartState.LastContext.LoadRequestSignature = request.Signature;
        _chartState.LastLoadRuntime = new LoadRuntimeState(
            EvidenceRuntimePath.Legacy,
            request.Signature,
            null,
            null,
            null,
            EvidenceDiagnosticsBuilder.BuildContextSignature(_chartState.LastContext),
            null,
            false);

        Debug.WriteLine($"[CTX] ActualSeriesCount={_chartState.LastContext.ActualSeriesCount}, " + $"PrimaryCms={(_chartState.LastContext.PrimaryCms == null ? "NULL" : "SET")}");

        return true;
    }

    private bool ShouldUseVNextMainFamilyPath()
    {
        return _chartState.IsMainVisible &&
               !_chartState.IsDistributionVisible &&
               !_chartState.IsWeeklyTrendVisible &&
               !_chartState.IsTransformPanelVisible &&
               !_chartState.IsBarPieVisible;
    }

    private bool SupportsOnlyMainChartRoute()
    {
        return _chartState.IsMainVisible &&
               !_chartState.IsNormalizedVisible &&
               !_chartState.IsDiffRatioVisible;
    }
}
