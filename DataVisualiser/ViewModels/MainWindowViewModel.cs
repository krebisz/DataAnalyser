using DataVisualiser.Class;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.ViewModels.Commands;
using DataVisualiser.ViewModels.Events;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DataVisualiser.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event EventHandler<MetricTypesLoadedEventArgs>? MetricTypesLoaded;
        public event EventHandler<SubtypesLoadedEventArgs>? SubtypesLoaded;
        public event EventHandler<DateRangeLoadedEventArgs>? DateRangeLoaded;
        public event EventHandler<DataLoadedEventArgs>? DataLoaded;
        public event EventHandler<ChartVisibilityChangedEventArgs>? ChartVisibilityChanged;
        public event EventHandler<ErrorEventArgs>? ErrorOccured;
        public event EventHandler<ChartUpdateRequestedEventArgs>? ChartUpdateRequested;

        // STATE OBJECTS
        public ChartState ChartState { get; }
        public MetricState MetricState { get; }
        public UiState UiState { get; }

        private bool _isInitializing = true;

        // SERVICES (injected)
        private readonly MetricSelectionService _metricService;
        private readonly ChartUpdateCoordinator _chartCoordinator;
        private readonly WeeklyDistributionService _weeklyDistService;

        // COMMANDS
        public ICommand LoadMetricsCommand { get; }
        public ICommand LoadSubtypesCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand ToggleNormCommand { get; }
        public ICommand ToggleRatioCommand { get; }
        public ICommand ToggleDiffCommand { get; }
        public ICommand ToggleWeeklyCommand { get; }

        public MainWindowViewModel(
            ChartState chartState,
            MetricState metricState,
            UiState uiState,
            MetricSelectionService metricService,
            ChartUpdateCoordinator chartCoordinator,
            WeeklyDistributionService weeklyDistService)
        {
            ChartState = chartState;
            MetricState = metricState;
            UiState = uiState;

            _metricService = metricService;
            _chartCoordinator = chartCoordinator;
            _weeklyDistService = weeklyDistService;

            LoadMetricsCommand = new RelayCommand(_ => LoadMetrics());
            LoadSubtypesCommand = new RelayCommand(_ => LoadSubtypes());
            LoadDataCommand = new RelayCommand(_ => LoadData(), _ => CanLoadData());
            ToggleNormCommand = new RelayCommand(_ => ToggleNorm());
            ToggleRatioCommand = new RelayCommand(_ => ToggleRatio());
            ToggleDiffCommand = new RelayCommand(_ => ToggleDiff());
            ToggleWeeklyCommand = new RelayCommand(_ => ToggleWeekly());
        }

        // ======================
        // COMMAND TARGET METHODS
        // ======================

        private async void LoadMetrics()
        {
            try
            {
                UiState.IsLoadingMetricTypes = true;

                if (MetricState == null)
                    return;

                var tableName = MetricState.ResolutionTableName;
                if (string.IsNullOrEmpty(tableName))
                    return;

                var metricTypes = await _metricService.LoadMetricTypesAsync(tableName);

                // Raise event � let UI populate the combo box
                MetricTypesLoaded?.Invoke(this, new MetricTypesLoadedEventArgs
                {
                    MetricTypes = metricTypes
                });
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs
                {
                    Message = FormatDatabaseError(ex)
                });
            }
            finally
            {
                UiState.IsLoadingMetricTypes = false;
            }
        }

        private async void LoadSubtypes()
        {
            try
            {
                UiState.IsLoadingSubtypes = true;

                if (!ValidateMetricTypeSelected())
                {
                    ErrorOccured?.Invoke(this, new ErrorEventArgs
                    {
                        Message = "Please select a Metric Type before loading subtypes."
                    });
                    return;
                }

                var metricType = MetricState.SelectedMetricType!;
                var tableName = MetricState.ResolutionTableName!;

                // Fetch subtype list
                var subtypes = await _metricService.LoadSubtypesAsync(metricType, tableName);

                // Raise event � UI updates SubtypeCombo + dynamic controls
                SubtypesLoaded?.Invoke(this, new SubtypesLoadedEventArgs
                {
                    Subtypes = subtypes
                });
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(this, new ErrorEventArgs
                {
                    Message = FormatDatabaseError(ex)
                });
            }
            finally
            {
                UiState.IsLoadingSubtypes = false;
            }
        }

        private void LoadData()
        {
            try
            {
                // Validate high-level state before we attempt rendering
                if (!ValidateDataLoadRequirements(out var errorMessage))
                {
                    ErrorOccured?.Invoke(this, new ErrorEventArgs
                    {
                        Message = errorMessage
                    });
                    return;
                }

                // Expect that the view (OnLoadData) has already populated ChartState.LastContext
                if (ChartState.LastContext == null ||
                    ChartState.LastContext.Data1 == null ||
                    !ChartState.LastContext.Data1.Any())
                {
                    ErrorOccured?.Invoke(this, new ErrorEventArgs
                    {
                        Message = "No data is available to render charts. Please load data first."
                    });
                    return;
                }

                DataLoaded?.Invoke(this, new DataLoadedEventArgs
                {
                    DataContext = ChartState.LastContext
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


        private bool CanLoadData()
        {
            return !string.IsNullOrWhiteSpace(MetricState.SelectedMetricType);
        }


        #region Chart Visibility Toggles
        public void ToggleNorm()
        {
            ChartState.IsNormalizedVisible = !ChartState.IsNormalizedVisible;
            OnPropertyChanged(nameof(ChartState));

            ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs
            {
                ChartName = "Norm",
                IsVisible = ChartState.IsNormalizedVisible
            });

            RequestChartUpdate();
        }


        public void ToggleRatio()
        {
            ChartState.IsRatioVisible = !ChartState.IsRatioVisible;
            OnPropertyChanged(nameof(ChartState));

            ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs
            {
                ChartName = "Ratio",
                IsVisible = ChartState.IsRatioVisible
            });
            RequestChartUpdate();
        }

        public void ToggleDiff()
        {
            ChartState.IsDifferenceVisible = !ChartState.IsDifferenceVisible;
            OnPropertyChanged(nameof(ChartState));

            ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs
            {
                ChartName = "Diff",
                IsVisible = ChartState.IsDifferenceVisible
            });
            RequestChartUpdate();
        }

        public void ToggleWeekly()
        {
            ChartState.IsWeeklyVisible = !ChartState.IsWeeklyVisible;
            OnPropertyChanged(nameof(ChartState));

            ChartVisibilityChanged?.Invoke(this, new ChartVisibilityChangedEventArgs
            {
                ChartName = "Weekly",
                IsVisible = ChartState.IsWeeklyVisible
            });
            RequestChartUpdate();
        }
        #endregion

        #region Property Change Notification
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region State Setters
        public void SetNormalizedVisible(bool value)
        {
            ChartState.IsNormalizedVisible = value;
            RequestChartUpdate();
        }

        public void SetDifferenceVisible(bool value)
        {
            ChartState.IsDifferenceVisible = value;
            RequestChartUpdate();
        }

        public void SetRatioVisible(bool value)
        {
            ChartState.IsRatioVisible = value;
            RequestChartUpdate();
        }

        public void SetWeeklyVisible(bool value)
        {
            ChartState.IsWeeklyVisible = value;
            RequestChartUpdate();
        }

        public void SetNormalizationMode(NormalizationMode mode)
        {
            ChartState.SelectedNormalizationMode = mode;
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
        #endregion

        #region Validation Methods

        /// <summary>
        /// Validates that a metric type is selected.
        /// </summary>
        public bool ValidateMetricTypeSelected()
        {
            return !string.IsNullOrWhiteSpace(MetricState.SelectedMetricType);
        }
        private bool ValidateMetricTypeSelected(out string message)
        {
            if (string.IsNullOrWhiteSpace(MetricState.SelectedMetricType))
            {
                message = "Please select a Metric Type before loading data.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        /// <summary>
        /// Validates that the date range is valid (from <= to).
        /// </summary>
        public bool ValidateDateRange()
        {
            if (!MetricState.FromDate.HasValue || !MetricState.ToDate.HasValue)
                return false;

            return MetricState.FromDate.Value <= MetricState.ToDate.Value;
        }
        private bool ValidateDateRange(out string message)
        {
            if (MetricState.FromDate == null || MetricState.ToDate == null)
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

        public (bool IsValid, string? ErrorMessage) ValidateDataLoadRequirements()
        {
            if (!ValidateMetricTypeSelected())
            {
                return (false, "Please select a MetricType");
            }

            if (!ValidateDateRange())
            {
                return (false, "From date must be before To date");
            }

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
        #endregion

        #region Error Handling
        public event EventHandler<string>? ErrorOccurred;

        public void RaiseError(string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                ErrorOccurred?.Invoke(this, errorMessage);
            }
        }

        /// <summary>
        /// Handles database exceptions and formats user-friendly error messages.
        /// </summary>
        public string FormatDatabaseError(Exception ex)
        {
            return $"An error occurred while loading data: {ex.Message}";
        }

        public void RequestChartUpdate()
        {
            if (_isInitializing)
                return;

            ChartUpdateRequested?.Invoke(this, new ChartUpdateRequestedEventArgs
            {
                ShowNormalized = ChartState.IsNormalizedVisible,
                ShowDifference = ChartState.IsDifferenceVisible,
                ShowRatio = ChartState.IsRatioVisible,
                ShowWeekly = ChartState.IsWeeklyVisible,
                ShouldRenderCharts = ChartState.LastContext != null
            });
        }
        public void CompleteInitialization()
        {
            _isInitializing = false;
            RequestChartUpdate();
        }
        #endregion
    }
}
