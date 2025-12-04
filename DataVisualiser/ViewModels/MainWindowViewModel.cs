using DataVisualiser.Class;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.ViewModels.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DataVisualiser.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // STATE OBJECTS
        public ChartState ChartState { get; }
        public MetricState MetricState { get; }
        public UiState UiState { get; }

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

            // Initialize commands (we wire up real methods later)
            LoadMetricsCommand = new RelayCommand(_ => LoadMetrics());
            LoadSubtypesCommand = new RelayCommand(_ => LoadSubtypes());
            LoadDataCommand = new RelayCommand(_ => LoadData());
            ToggleNormCommand = new RelayCommand(_ => ToggleNorm());
            ToggleRatioCommand = new RelayCommand(_ => ToggleRatio());
            ToggleDiffCommand = new RelayCommand(_ => ToggleDiff());
            ToggleWeeklyCommand = new RelayCommand(_ => ToggleWeekly());
        }

        // ======================
        // COMMAND TARGET METHODS
        // ======================

        private void LoadMetrics()
        {
            // The logic moves from MainWindow into here incrementally
        }

        private void LoadSubtypes()
        {
        }

        private void LoadData()
        {
        }

        private void ToggleNorm()
        {
            ChartState.IsNormalizedVisible = !ChartState.IsNormalizedVisible;
            OnPropertyChanged(nameof(ChartState));
        }

        private void ToggleRatio()
        {
            ChartState.IsRatioVisible = !ChartState.IsRatioVisible;
            OnPropertyChanged(nameof(ChartState));
        }

        private void ToggleDiff()
        {
            ChartState.IsDifferenceVisible = !ChartState.IsDifferenceVisible;
            OnPropertyChanged(nameof(ChartState));
        }

        private void ToggleWeekly()
        {
            ChartState.IsWeeklyVisible = !ChartState.IsWeeklyVisible;
            OnPropertyChanged(nameof(ChartState));
        }

        // Notify UI of changes
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        public void SetNormalizedVisible(bool isVisible)
        {
            ChartState.IsNormalizedVisible = isVisible;
        }

        public void SetDifferenceVisible(bool isVisible)
        {
            ChartState.IsDifferenceVisible = isVisible;
        }

        public void SetRatioVisible(bool isVisible)
        {
            ChartState.IsRatioVisible = isVisible;
        }

        public void SetWeeklyVisible(bool isVisible)
        {
            ChartState.IsWeeklyVisible = isVisible;
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

    }
}
