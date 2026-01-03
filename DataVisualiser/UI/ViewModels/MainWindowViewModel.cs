using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DataVisualiser.Core.Orchestration.Builders;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels.Commands;
using DataVisualiser.UI.ViewModels.Events;

namespace DataVisualiser.UI.ViewModels;

public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ChartUpdateCoordinator _chartCoordinator;

    // ======================
    // SERVICES (injected)
    // ======================

    private readonly MetricSelectionService    _metricService;
    private readonly WeeklyDistributionService _weeklyDistService;

    private bool _isInitializing = true;

    public MainWindowViewModel(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService, ChartUpdateCoordinator chartCoordinator, WeeklyDistributionService weeklyDistService)
    {
        ChartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        MetricState = metricState ?? throw new ArgumentNullException(nameof(metricState));
        UiState = uiState ?? throw new ArgumentNullException(nameof(uiState));

        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _chartCoordinator = chartCoordinator ?? throw new ArgumentNullException(nameof(chartCoordinator));
        _weeklyDistService = weeklyDistService ?? throw new ArgumentNullException(nameof(weeklyDistService));

        LoadMetricsCommand = new RelayCommand(_ => LoadMetrics());
        LoadSubtypesCommand = new RelayCommand(_ => LoadSubtypes());
        LoadDataCommand = new RelayCommand(_ => LoadData(), _ => CanLoadData());
        ToggleNormCommand = new RelayCommand(_ => ToggleNorm());
        ToggleDiffRatioCommand = new RelayCommand(_ => ToggleDiffRatio());
        ToggleDiffRatioOperationCommand = new RelayCommand(_ => ToggleDiffRatioOperation());
        ToggleWeeklyCommand = new RelayCommand(_ => ToggleWeekly());
    }

    // ======================
    // STATE OBJECTS
    // ======================

    public ChartState  ChartState  { get; }
    public MetricState MetricState { get; }
    public UiState     UiState     { get; }

    // ======================
    // COMMANDS
    // ======================

    public ICommand LoadMetricsCommand              { get; }
    public ICommand LoadSubtypesCommand             { get; }
    public ICommand LoadDataCommand                 { get; }
    public ICommand ToggleNormCommand               { get; }
    public ICommand ToggleDiffRatioCommand          { get; }
    public ICommand ToggleDiffRatioOperationCommand { get; }
    public ICommand ToggleWeeklyCommand             { get; }

    // ======================
    // INotifyPropertyChanged
    // ======================

    public event PropertyChangedEventHandler? PropertyChanged;
    // ======================
    // UI → VM Event Surface
    // ======================

    public event EventHandler<MetricTypesLoadedEventArgs>?      MetricTypesLoaded;
    public event EventHandler<SubtypesLoadedEventArgs>?         SubtypesLoaded;
    public event EventHandler<DateRangeLoadedEventArgs>?        DateRangeLoaded;
    public event EventHandler<DataLoadedEventArgs>?             DataLoaded;
    public event EventHandler<ChartVisibilityChangedEventArgs>? ChartVisibilityChanged;
    public event EventHandler<ErrorEventArgs>?                  ErrorOccured;
    public event EventHandler<ChartUpdateRequestedEventArgs>?   ChartUpdateRequested;

    // Secondary string-based error channel (kept for compatibility)
    public event EventHandler<string>? ErrorOccurred;

    // ======================
    // COMMAND TARGET METHODS
    // ======================

    private async void LoadMetrics()
    {
        if (UiState.IsLoadingMetricTypes)
            return;

        try
        {
            UiState.IsLoadingMetricTypes = true;

            // Resolution table name must be set by the UI before this point.
            var tableName = MetricState.ResolutionTableName;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                RaiseError("Resolution is not selected. Please select a resolution before loading metric types.");
                return;
            }

            var metricTypes = await _metricService.LoadMetricTypesAsync(tableName);

            MetricTypesLoaded?.Invoke(this, new MetricTypesLoadedEventArgs
            {
                    MetricTypes = metricTypes
            });
        }
        catch (Exception ex)
        {
            RaiseError(FormatDatabaseError(ex));
        }
        finally
        {
            UiState.IsLoadingMetricTypes = false;
        }
    }

    private async void LoadSubtypes()
    {
        if (UiState.IsLoadingSubtypes)
            return;

        try
        {
            UiState.IsLoadingSubtypes = true;

            if (!ValidateMetricTypeSelected(out var metricError))
            {
                RaiseError(metricError);
                return;
            }

            var metricType = MetricState.SelectedMetricType!;
            var tableName = MetricState.ResolutionTableName;

            if (string.IsNullOrWhiteSpace(tableName))
            {
                RaiseError("Resolution is not selected. Please select a resolution before loading subtypes.");
                return;
            }

            var subtypes = await _metricService.LoadSubtypesAsync(metricType, tableName);

            SubtypesLoaded?.Invoke(this, new SubtypesLoadedEventArgs
            {
                    Subtypes = subtypes
            });
        }
        catch (Exception ex)
        {
            RaiseError(FormatDatabaseError(ex));
        }
        finally
        {
            UiState.IsLoadingSubtypes = false;
        }
    }

    /// <summary>
    ///     Phase 6 – Step 2.1
    ///     Centralised data load for the currently selected metric + subtypes + date range.
    ///     Builds ChartState.LastContext from the DB and returns true if charts can be rendered.
    /// </summary>
    /// <summary>
    ///     Phase 6 – Step 2.1
    ///     Centralised data load for the currently selected metric + subtypes + date range.
    ///     Builds ChartState.LastContext from the DB and returns true if charts can be rendered.
    /// </summary>
    public async Task<bool> LoadMetricDataAsync()
    {
        // Basic validation first
        if (!ValidateDataLoadPrerequisites(out var validationError))
            return false;

        var metricType = MetricState.SelectedMetricType!;
        var tableName = MetricState.ResolutionTableName ?? "HealthMetrics";

        // IMPORTANT: Ensure consistent ordering - first selected subtype is always primary (data1),
        // second selected subtype is always secondary (data2). This ordering is maintained
        // across all charts and strategies.
        var (primarySubtype, secondarySubtype) = ExtractPrimaryAndSecondarySubtypes();

        try
        {
            UiState.IsLoadingData = true;

            var dataLoaded = await LoadAndValidateMetricDataAsync(metricType, primarySubtype, secondarySubtype, tableName);

            if (!dataLoaded)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            ErrorOccured?.Invoke(this, new ErrorEventArgs
            {
                    Message = FormatDatabaseError(ex)
            });
            ChartState.LastContext = null;
            return false;
        }
        finally
        {
            UiState.IsLoadingData = false;
        }
    }


    /// <summary>
    ///     Validates all prerequisites for loading metric data.
    ///     Returns false and raises error event if validation fails.
    /// </summary>
    private bool ValidateDataLoadPrerequisites(out string? validationError)
    {
        validationError = null;

        if (!ValidateDataLoadRequirements(out validationError))
        {
            ErrorOccured?.Invoke(this, new ErrorEventArgs
            {
                    Message = validationError
            });
            return false;
        }

        if (MetricState.FromDate == null || MetricState.ToDate == null)
        {
            validationError = "Please select a valid date range before loading data.";
            ErrorOccured?.Invoke(this, new ErrorEventArgs
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
        var primarySubtype = MetricState.SelectedSubtypes.Count > 0 ? MetricState.SelectedSubtypes[0] // First selected subtype = primary
                : null;

        var secondarySubtype = MetricState.SelectedSubtypes.Count > 1 ? MetricState.SelectedSubtypes[1] // Second selected subtype = secondary
                : null;

        return (primarySubtype, secondarySubtype);
    }

    /// <summary>
    ///     Loads and validates metric data, then builds the chart data context.
    ///     Returns false if data loading or validation fails.
    /// </summary>
    private async Task<bool> LoadAndValidateMetricDataAsync(string metricType, string? primarySubtype, string? secondarySubtype, string tableName)
    {
        // Load data: data1 = first selected subtype (primary), data2 = second selected subtype (secondary)
        var (primaryCms, secondaryCms, data1, data2) = await _metricService.LoadMetricDataWithCmsAsync(metricType, primarySubtype, secondarySubtype, MetricState.FromDate!.Value, MetricState.ToDate!.Value, tableName);

        // When only one subtype is selected, ensure data2 is empty to prevent mixing with all subtypes
        // GetHealthMetricsDataByBaseType with null subtype returns ALL subtypes, which corrupts the chart
        if (secondarySubtype == null)
            data2 = Enumerable.Empty<HealthMetricData>();

        // Validate primary data
        if (!MetricDataValidationHelper.ValidatePrimaryData(metricType, primarySubtype, primaryCms, data1, ErrorOccured))
        {
            ChartState.LastContext = null;
            return false;
        }

        // Validate secondary data (only if secondary subtype is selected)
        if (!MetricDataValidationHelper.ValidateSecondaryData(metricType, secondarySubtype, secondaryCms, data2, ErrorOccured))
        {
            ChartState.LastContext = null;
            return false;
        }

        // NEW: delegate context construction to the builder
        var ctxBuilder = new ChartDataContextBuilder();

        ChartState.LastContext = ctxBuilder.Build(metricType, primarySubtype, secondarySubtype, data1, data2, MetricState.FromDate.Value, MetricState.ToDate.Value, primaryCms, secondaryCms);

        Debug.WriteLine($"[CTX] SemanticMetricCount={ChartState.LastContext.SemanticMetricCount}, " + $"PrimaryCms={(ChartState.LastContext.PrimaryCms == null ? "NULL" : "SET")}");

        return true;
    }

    private async Task LoadDateRangeForSelectedMetricAsync()
    {
        try
        {
            // Basic guard: we must have a selected metric type
            // But don't show error if we're in a transitional state (e.g., resolution change)
            if (!ValidateMetricTypeSelected())
                    // Silently return without showing error - this prevents popups during resolution changes
                return;

            var metricType = MetricState.SelectedMetricType!;
            var tableName = MetricState.ResolutionTableName!;
            if (string.IsNullOrWhiteSpace(tableName))
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs
                {
                        Message = "Resolution table name is missing – cannot load date range."
                });
                return;
            }

            // Use the *primary* selected subtype (if any) for the date range.
            // If the first subtype is "(All)" or empty, we pass null to mean "all subtypes".
            string? primarySubtype = null;
            if (MetricState.SelectedSubtypes.Any())
            {
                var first = MetricState.SelectedSubtypes.First();
                if (!string.IsNullOrWhiteSpace(first) && first != "(All)")
                    primarySubtype = first;
            }

            var dateRange = await _metricService.LoadDateRangeAsync(metricType, primarySubtype, tableName);

            if (!dateRange.HasValue)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs
                {
                        Message = "No date range could be determined for the current selection."
                });
                return;
            }

            // Update state
            MetricState.FromDate = dateRange.Value.MinDate;
            MetricState.ToDate = dateRange.Value.MaxDate;

            // Notify the view so it can update the DatePicker controls
            DateRangeLoaded?.Invoke(this, new DateRangeLoadedEventArgs
            {
                    MinDate = dateRange.Value.MinDate,
                    MaxDate = dateRange.Value.MaxDate
            });
        }
        catch (Exception ex)
        {
            ErrorOccured?.Invoke(this, new ErrorEventArgs
            {
                    Message = FormatDatabaseError(ex)
            });
        }
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

            // VM owns validation – UI just forwards the command.
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

            DataLoaded?.Invoke(this, new DataLoadedEventArgs
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

    // ======================
    // CHART VISIBILITY TOGGLES
    // ======================

    private void ToggleChartVisibility(string chartName, Func<bool> getVisibility, Action<bool> setVisibility)
    {
        var newVisibility = !getVisibility();
        setVisibility(newVisibility);
        OnPropertyChanged(nameof(ChartState));

        ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs
        {
                ChartName = chartName,
                IsVisible = newVisibility
        });

        RequestChartUpdate(true, chartName);
    }

    public void ToggleMain()
    {
        ToggleChartVisibility("Main", () => ChartState.IsMainVisible, v => ChartState.IsMainVisible = v);
    }

    public void ToggleNorm()
    {
        ToggleChartVisibility("Norm", () => ChartState.IsNormalizedVisible, v => ChartState.IsNormalizedVisible = v);
    }

    public void ToggleDiffRatio()
    {
        ToggleChartVisibility("DiffRatio", () => ChartState.IsDiffRatioVisible, v => ChartState.IsDiffRatioVisible = v);
    }

    public void ToggleDiffRatioOperation()
    {
        ChartState.IsDiffRatioDifferenceMode = !ChartState.IsDiffRatioDifferenceMode;
        OnPropertyChanged(nameof(ChartState));

        // Re-render if visible
        if (ChartState.IsDiffRatioVisible && ChartState.LastContext != null)
            RequestChartUpdate();
    }

    public void ToggleWeekly()
    {
        ToggleChartVisibility("Weekly", () => ChartState.IsWeeklyVisible, v => ChartState.IsWeeklyVisible = v);
    }

    public void ToggleWeeklyTrend()
    {
        ToggleChartVisibility("WeeklyTrend", () => ChartState.IsWeeklyTrendVisible, v => ChartState.IsWeeklyTrendVisible = v);
    }

    public void ToggleWeekdayTrendChartType()
    {
        ChartState.IsWeekdayTrendPolarMode = !ChartState.IsWeekdayTrendPolarMode;
        OnPropertyChanged(nameof(ChartState));
    }

    public void ToggleTransformPanel()
    {
        ChartState.IsTransformPanelVisible = !ChartState.IsTransformPanelVisible;
        OnPropertyChanged(nameof(ChartState));
        RequestChartUpdate(true, "Transform");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // ======================
    // STATE SETTERS (UI → VM)
    // ======================

    public void SetMainVisible(bool value)
    {
        ChartState.IsMainVisible = value;
        RequestChartUpdate();
    }

    public void SetNormalizedVisible(bool value)
    {
        ChartState.IsNormalizedVisible = value;
        RequestChartUpdate();
    }

    public void SetDiffRatioVisible(bool value)
    {
        ChartState.IsDiffRatioVisible = value;
        RequestChartUpdate();
    }

    public void SetWeeklyVisible(bool value)
    {
        ChartState.IsWeeklyVisible = value;
        RequestChartUpdate();
    }

    public void SetWeeklyTrendMondayVisible(bool value)
    {
        ChartState.ShowMonday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendTuesdayVisible(bool value)
    {
        ChartState.ShowTuesday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendWednesdayVisible(bool value)
    {
        ChartState.ShowWednesday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendThursdayVisible(bool value)
    {
        ChartState.ShowThursday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendFridayVisible(bool value)
    {
        ChartState.ShowFriday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendSaturdayVisible(bool value)
    {
        ChartState.ShowSaturday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetWeeklyTrendSundayVisible(bool value)
    {
        ChartState.ShowSunday = value;
        RequestChartUpdate(false, "WeeklyTrend");
    }

    public void SetNormalizationMode(NormalizationMode mode)
    {
        ChartState.SelectedNormalizationMode = mode;
    }

    public void SetWeeklyFrequencyShading(bool useFrequencyShading)
    {
        ChartState.UseFrequencyShading = useFrequencyShading;
    }

    public void SetWeeklyIntervalCount(int intervalCount)
    {
        ChartState.WeeklyIntervalCount = intervalCount;
    }

    public void SetSelectedMetricType(string? metric)
    {
        MetricState.SelectedMetricType = metric;
    }

    public void SetSelectedSubtypes(IEnumerable<string?> subtypes)
    {
        MetricState.SetSubtypes(subtypes);
    }

    public void SetDateRange(DateTime? from, DateTime? to)
    {
        MetricState.FromDate = from;
        MetricState.ToDate = to;
    }

    public void SetLoadingMetricTypes(bool isLoading)
    {
        UiState.IsLoadingMetricTypes = isLoading;
    }

    public void SetLoadingSubtypes(bool isLoading)
    {
        UiState.IsLoadingSubtypes = isLoading;
    }

    public void SetLoadingData(bool isLoading)
    {
        UiState.IsLoadingData = isLoading;
    }

    // ======================
    // VALIDATION
    // ======================

    public bool ValidateMetricTypeSelected()
    {
        return !string.IsNullOrWhiteSpace(MetricState.SelectedMetricType);
    }

    private bool ValidateMetricTypeSelected(out string message)
    {
        if (!ValidateMetricTypeSelected())
        {
            message = "Please select a Metric Type before loading data.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public bool ValidateDateRange()
    {
        if (!MetricState.FromDate.HasValue || !MetricState.ToDate.HasValue)
            return false;

        return MetricState.FromDate.Value <= MetricState.ToDate.Value;
    }

    private bool ValidateDateRange(out string message)
    {
        if (!MetricState.FromDate.HasValue || !MetricState.ToDate.HasValue)
        {
            message = "Please select both From and To dates before loading data.";
            return false;
        }

        if (MetricState.FromDate > MetricState.ToDate)
        {
            message = "From date must be before To date.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public(bool IsValid, string? ErrorMessage) ValidateDataLoadRequirements()
    {
        if (!ValidateDataLoadRequirements(out var message))
            return (false, message);

        return (true, null);
    }

    private bool ValidateDataLoadRequirements(out string message)
    {
        if (!ValidateMetricTypeSelected(out message))
            return false;

        if (!ValidateDateRange(out message))
            return false;

        message = string.Empty;
        return true;
    }

    // ======================
    // ERROR HANDLING
    // ======================

    public void RaiseError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return;

        // Primary, structured error path
        ErrorOccured?.Invoke(this, new ErrorEventArgs
        {
                Message = errorMessage
        });

        // Secondary, string-only path retained for compatibility
        ErrorOccurred?.Invoke(this, errorMessage);
    }

    /// <summary>
    ///     Formats a user-friendly error message for database-related failures.
    /// </summary>
    public string FormatDatabaseError(Exception ex)
    {
        return $"An error occurred while loading data: {ex.Message}";
    }

    public void RequestChartUpdate(bool isVisibilityOnlyToggle = false, string? toggledChartName = null)
    {
        if (_isInitializing)
            return;

        ChartUpdateRequested?.Invoke(this, new ChartUpdateRequestedEventArgs
        {
                ShowMain = ChartState.IsMainVisible,
                ShowNormalized = ChartState.IsNormalizedVisible,
                ShowDiffRatio = ChartState.IsDiffRatioVisible,
                ShowWeekly = ChartState.IsWeeklyVisible,
                ShowWeeklyTrend = ChartState.IsWeeklyTrendVisible,
                ShowTransformPanel = ChartState.IsTransformPanelVisible,
                ShouldRenderCharts = ChartState.LastContext != null,
                IsVisibilityOnlyToggle = isVisibilityOnlyToggle,
                ToggledChartName = toggledChartName
        });
    }

    public void CompleteInitialization()
    {
        _isInitializing = false;
        RequestChartUpdate();
    }
}