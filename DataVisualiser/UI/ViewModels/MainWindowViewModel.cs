using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using DataVisualiser.Core.Services;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

public partial class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ChartVisibilityController _chartVisibilityController;
    private readonly DataLoadValidator _dataLoadValidator;
    private readonly MetricLoadCoordinator _metricLoadCoordinator;

    // ======================
    // SERVICES (injected)
    // ======================

    private readonly MetricSelectionService _metricService;
    private bool _isInitializing = true;

    public MainWindowViewModel(ChartState chartState, MetricState metricState, UiState uiState, MetricSelectionService metricService)
    {
        ChartState = chartState ?? throw new ArgumentNullException(nameof(chartState));
        MetricState = metricState ?? throw new ArgumentNullException(nameof(metricState));
        UiState = uiState ?? throw new ArgumentNullException(nameof(uiState));

        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _dataLoadValidator = new DataLoadValidator(MetricState);
        _metricLoadCoordinator = new MetricLoadCoordinator(ChartState, MetricState, UiState, _metricService, _dataLoadValidator, FormatDatabaseError);
        _chartVisibilityController = new ChartVisibilityController(ChartState, () => _isInitializing, OnPropertyChanged);

        LoadMetricsCommand = new AsyncRelayCommand(LoadMetricsAsync);
        LoadSubtypesCommand = new AsyncRelayCommand(LoadSubtypesAsync);
        LoadDataCommand = new RelayCommand(_ => LoadData(), _ => CanLoadData());
        ToggleNormCommand = new RelayCommand(_ => ToggleNorm());
        ToggleDiffRatioCommand = new RelayCommand(_ => ToggleDiffRatio());
        ToggleDiffRatioOperationCommand = new RelayCommand(_ => ToggleDiffRatioOperation());
        ToggleWeeklyCommand = new RelayCommand(_ => ToggleWeekly());
        ToggleHourlyCommand = new RelayCommand(_ => ToggleHourly());
    }

    // ======================
    // STATE OBJECTS
    // ======================

    public ChartState ChartState { get; }
    public MetricState MetricState { get; }
    public UiState UiState { get; }

    // ======================
    // COMMANDS
    // ======================

    public ICommand LoadMetricsCommand { get; }
    public ICommand LoadSubtypesCommand { get; }
    public ICommand LoadDataCommand { get; }
    public ICommand ToggleNormCommand { get; }
    public ICommand ToggleDiffRatioCommand { get; }
    public ICommand ToggleDiffRatioOperationCommand { get; }
    public ICommand ToggleWeeklyCommand { get; }
    public ICommand ToggleHourlyCommand { get; }

    // ======================
    // INotifyPropertyChanged
    // ======================

    public event PropertyChangedEventHandler? PropertyChanged;

    // ======================
    // UI -> VM Event Surface
    // ======================

    public event EventHandler<MetricTypesLoadedEventArgs>? MetricTypesLoaded;
    public event EventHandler<SubtypesLoadedEventArgs>? SubtypesLoaded;
    public event EventHandler<DateRangeLoadedEventArgs>? DateRangeLoaded;
    public event EventHandler<DataLoadedEventArgs>? DataLoaded;
    public event EventHandler<ChartVisibilityChangedEventArgs>? ChartVisibilityChanged;
    public event EventHandler<ErrorEventArgs>? ErrorOccured;
    public event EventHandler<ChartUpdateRequestedEventArgs>? ChartUpdateRequested;

    // Secondary string-based error channel (kept for compatibility)
    public event EventHandler<string>? ErrorOccurred;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}