using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.Shared;
using DataVisualiser.UI.Events;
using DataVisualiser.UI;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Coordination;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.MainHost.Export;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Syncfusion;

public partial class SyncfusionChartsView : UserControl
{
    private readonly SyncfusionChartsViewCoordinator _coordinator = new();
    private readonly ChartHostMetricSelectionCoordinator _metricSelectionCoordinator = new();
    private readonly ChartHostDateRangeCoordinator _dateRangeCoordinator = new();
    private readonly SyncfusionChartsViewLoadCoordinator _loadCoordinator = new();
    private readonly MainChartsViewStateSyncCoordinator _stateSyncCoordinator = new();
    private readonly MetricSelectionPanelEventBinder _selectionPanelEventBinder = new();
    private MainChartsEvidenceExportService _evidenceExportService = null!;
    private MainChartsViewThemeCoordinator _themeCoordinator = null!;
    private ChartState _chartState = null!;
    private MetricState _metricState = null!;
    private UiState _uiState = null!;
    private IChartControllerRegistry _chartControllerRegistry = null!;
    private string _connectionString = null!;
    private bool _isChangingResolution;
    private bool _isInitializing = true;
    private bool _isMetricTypeChangePending;
    private bool _isApplyingSelectionSync;
    private MetricSelectionService _metricSelectionService = null!;
    private SubtypeSelectorManager _selectorManager = null!;
    private List<MetricNameOption>? _subtypeList;
    private int _uiBusyDepth;
    private MainWindowViewModel _viewModel = null!;

    // Forwarding properties — bridge shared MetricSelectionPanel controls to existing code-behind references
    private MetricSelectionPanel SelectionPanel => ChartTabHost.SelectionSurface;
    private ComboBox TablesCombo => SelectionPanel.MetricTypeCombo;
    private ComboBox SubtypeCombo => SelectionPanel.PrimarySubtypeCombo;
    private DatePicker FromDate => SelectionPanel.FromDatePicker;
    private DatePicker ToDate => SelectionPanel.ToDatePicker;
    private ComboBox ResolutionCombo => SelectionPanel.ResolutionSelector;
    private StackPanel TopControlMetricSubtypePanel => SelectionPanel.SubtypePanel;
    private Button ThemeToggleButton => SelectionPanel.ThemeToggle;
    private CheckBox CmsEnableCheckBox => SelectionPanel.CmsEnable;
    private CheckBox CmsSingleCheckBox => SelectionPanel.CmsSingle;
    private CheckBox CmsCombinedCheckBox => SelectionPanel.CmsCombined;
    private CheckBox CmsMultiCheckBox => SelectionPanel.CmsMulti;
    private CheckBox CmsNormalizedCheckBox => SelectionPanel.CmsNormalized;
    private CheckBox CmsWeeklyCheckBox => SelectionPanel.CmsWeekly;
    private CheckBox CmsWeekdayTrendCheckBox => SelectionPanel.CmsWeekdayTrend;
    private CheckBox CmsHourlyCheckBox => SelectionPanel.CmsHourly;
    private CheckBox CmsBarPieCheckBox => SelectionPanel.CmsBarPie;

    public SyncfusionChartsView()
    {
        InitializeComponent();
        WireSelectionPanelEvents();

        InitializeInfrastructure();
        InitializeViewModel();
        InitializeUiBindings();
        ExecuteStartupSequence();

        IsVisibleChanged += OnViewVisibilityChanged;
    }

    private void WireSelectionPanelEvents()
    {
        _selectionPanelEventBinder.Bind(
            SelectionPanel,
            new MetricSelectionPanelEventBinder.Actions(
                () => OnLoadData(this, new RoutedEventArgs()),
                () => OnResetZoom(this, new RoutedEventArgs()),
                () => OnClear(this, new RoutedEventArgs()),
                () => OnExportReachability(this, new RoutedEventArgs()),
                () => OnToggleTheme(this, new RoutedEventArgs()),
                () => AddSubtypeComboBox(this, new RoutedEventArgs()),
                (s, e) => OnResolutionSelectionChanged(s!, e),
                (s, e) => OnMetricTypeSelectionChanged(s!, e),
                (s, e) => OnFromDateChanged(s!, e),
                (s, e) => OnToDateChanged(s!, e),
                (s, e) => OnCmsToggleChanged(s!, e),
                (s, e) => OnCmsStrategyToggled(s!, e)));
    }

    private IDisposable BeginUiBusyScope()
    {
        _uiBusyDepth++;
        if (_uiBusyDepth == 1)
            _uiState.IsUiBusy = true;

        return new UiBusyScopeLease(EndUiBusyScope);
    }

    private void EndUiBusyScope()
    {
        if (_uiBusyDepth == 0)
            return;

        _uiBusyDepth--;
        if (_uiBusyDepth == 0)
            _uiState.IsUiBusy = false;
    }

    private void InitializeInfrastructure()
    {
        _connectionString = ResolveConnectionString();
        var shared = SharedMainWindowViewModelProvider.GetOrCreate(_connectionString);
        _chartState = shared.ChartState;
        _metricState = shared.MetricState;
        _uiState = shared.UiState;
        _metricSelectionService = shared.MetricSelectionService;
        _viewModel = shared.ViewModel;
        _evidenceExportService = new MainChartsEvidenceExportService(
            new ReachabilityExportWriter(),
            new ReachabilityExportPathResolver(),
            new StrategyReachabilityEvidenceStore(),
            _metricSelectionService,
            () => null,
            () => "N/A",
            () => "N/A");
    }

    private void InitializeViewModel()
    {
        if (_viewModel == null)
            throw new InvalidOperationException("Shared MainWindowViewModel was not initialized.");
        DataContext = _viewModel;
    }

    private void InitializeUiBindings()
    {
        WireViewModelEvents();
        InitializeSubtypeSelector();
        InitializeChartRegistry();
        InitializeThemeToggle();
        SunburstPanel.ToggleRequested += OnSunburstToggleRequested;
        SunburstPanel.BucketCountChanged += OnSunburstBucketCountChanged;
    }

    private void InitializeThemeToggle()
    {
        _themeCoordinator = new MainChartsViewThemeCoordinator(
            Theming.AppThemeService.Default,
            content =>
            {
                if (ThemeToggleButton != null)
                    ThemeToggleButton.Content = content;
            });
        _themeCoordinator.Attach();
    }

    private void ExecuteStartupSequence()
    {
        InitializeDateRange();
        InitializeResolution();
        SyncCmsToggleStates();
        SyncInitialChartState();
        SunburstPanel.SetBucketCount(_viewModel.ChartState.BarPieBucketCount);

        _viewModel.CompleteInitialization();
        _isInitializing = false;
    }

    private void WireViewModelEvents()
    {
        _viewModel.ErrorOccured += OnErrorOccured;
        _viewModel.MetricTypesLoaded += OnMetricTypesLoaded;
        _viewModel.SubtypesLoaded += OnSubtypesLoaded;
        _viewModel.DateRangeLoaded += OnDateRangeLoaded;
        _viewModel.DataLoaded += OnDataLoaded;
        _viewModel.ChartVisibilityChanged += OnChartVisibilityChanged;
        _viewModel.ChartUpdateRequested += OnChartUpdateRequested;
        _viewModel.SelectionStateChanged += OnSelectionStateChanged;
    }

    private void InitializeDateRange()
    {
        _dateRangeCoordinator.ApplyDefaultRange(DateTime.UtcNow, CreateDateRangeActions());
    }

    private void InitializeResolution()
    {
        InitializeResolutionCombo();
        ResolutionCombo.SelectedItem = "All";
        _viewModel.SetResolutionTableName(ChartUiHelper.GetTableNameFromResolution(ResolutionCombo));
        _viewModel.LoadMetricsCommand.Execute(null);
        UpdateSyncfusionToggleEnabled();
    }

    private void InitializeResolutionCombo()
    {
        ResolutionCombo.Items.Add("All");
        ResolutionCombo.Items.Add("Hourly");
        ResolutionCombo.Items.Add("Daily");
        ResolutionCombo.Items.Add("Weekly");
        ResolutionCombo.Items.Add("Monthly");
        ResolutionCombo.Items.Add("Yearly");
    }

    private void InitializeSubtypeSelector()
    {
        _selectorManager = new SubtypeSelectorManager(TopControlMetricSubtypePanel, SubtypeCombo);
        _selectorManager.SubtypeSelectionChanged += (s, e) => OnAnySubtypeSelectionChanged(s, null);
    }

    private void InitializeChartRegistry()
    {
        var factory = new ChartControllerFactory();
        var result = factory.CreateSyncfusion(SunburstPanel, _viewModel, _metricSelectionService);
        _chartControllerRegistry = result.Registry;
    }

    private void OnFromDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || _isApplyingSelectionSync)
            return;

        _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
    }

    private void OnToDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || _isApplyingSelectionSync)
            return;

        _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
    }

    private void OnResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isApplyingSelectionSync)
            return;

        if (ResolutionCombo.SelectedItem == null)
            return;

        var selectedResolution = ResolutionCombo.SelectedItem.ToString();
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;

        ResetForResolutionChange(selectedResolution);

        if (ResolutionCombo.SelectedItem?.ToString() != selectedResolution)
            ResolutionCombo.SelectedItem = selectedResolution;
    }

    private void ResetForResolutionChange(string selectedResolution)
    {
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;

        _isChangingResolution = true;

        _viewModel.MetricState.SelectedMetricType = null;
        _viewModel.ChartState.LastContext = new ChartDataContext();
        _viewModel.ChartState.LastLoadRuntime = null;
        ResetDateRangeToDefault();

        TablesCombo.Items.Clear();
        _selectorManager.ClearAllSubtypeControls();

        _viewModel.SetResolutionTableName(ChartUiHelper.GetTableNameFromResolution(ResolutionCombo));
        _viewModel.LoadMetricsCommand.Execute(null);
    }

    private void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing || _isApplyingSelectionSync)
            return;

        if (_viewModel.UiState.IsLoadingMetricTypes)
            return;

        _metricSelectionCoordinator.HandleMetricTypeSelectionChanged(
            GetSelectedMetricValue(TablesCombo),
            CreateMetricTypeSelectionChangedActions());
    }

    private static string? GetSelectedMetricValue(ComboBox combo)
    {
        return MetricSelectionComboReader.GetSelectedMetricValue(combo);
    }

    private static MetricNameOption? GetSelectedMetricOption(ComboBox combo)
    {
        return MetricSelectionComboReader.GetSelectedMetricOption(combo);
    }

    private void OnMetricTypesLoaded(object? sender, MetricTypesLoadedEventArgs e)
    {
        _isMetricTypeChangePending = false;
        _metricSelectionCoordinator.HandleMetricTypesLoaded(e.MetricTypes.ToList(), CreateMetricTypesLoadedActions());
        _isChangingResolution = false;
    }

    private void OnSubtypesLoaded(object? sender, SubtypesLoadedEventArgs e)
    {
        var subtypeListLocal = e.Subtypes.ToList();

        _subtypeList = subtypeListLocal;
        var selectedMetricType = GetSelectedMetricOption(TablesCombo);
        var followUp = _metricSelectionCoordinator.HandleSubtypesLoaded(
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedInput(
                subtypeListLocal,
                selectedMetricType,
                _isMetricTypeChangePending,
                HasLoadedData(),
                ShouldRefreshDateRangeForCurrentSelection(),
                _isInitializing,
                _viewModel.MetricState.SelectedSeries.Count),
            CreateSubtypesLoadedActions());

        if (followUp == ChartHostMetricSelectionCoordinator.SubtypesFollowUp.LoadDateRange)
        {
            _ = LoadDateRangeForSelectedMetrics();
            return;
        }

        if (followUp == ChartHostMetricSelectionCoordinator.SubtypesFollowUp.ApplySelectionState)
            ApplySelectionStateToUi();
    }

    private void RefreshPrimarySubtypeCombo(IEnumerable<MetricNameOption> subtypes, bool preserveSelection, MetricNameOption? selectedMetricType)
    {
        _selectorManager.ReplacePrimaryItems(subtypes, selectedMetricType, preserveSelection);
    }

    private void BuildDynamicSubtypeControls(IEnumerable<MetricNameOption> subtypes)
    {
        _selectorManager.ClearDynamic();
    }

    private void OnDateRangeLoaded(object? sender, DateRangeLoadedEventArgs e)
    {
        FromDate.SelectedDate = e.MinDate;
        ToDate.SelectedDate = e.MaxDate;
    }

    private void OnErrorOccured(object? sender, ErrorEventArgs e)
    {
        if (_isChangingResolution)
            return;

        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        _viewModel.ChartState.LastContext = null;
        _viewModel.ChartState.LastLoadRuntime = null;
    }

    private async void OnAnySubtypeSelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        if (_isApplyingSelectionSync)
            return;

        UpdateSelectedSubtypesInViewModel();

        if (_coordinator.ShouldRenderAfterSubtypeSelectionChange(_isApplyingSelectionSync, HasRenderableContext(), _viewModel.ChartState.LastContext))
        {
            await RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, _viewModel.ChartState.LastContext!);
            return;
        }

        if (ShouldRefreshDateRangeForCurrentSelection())
            await LoadDateRangeForSelectedMetrics();
    }

    private bool HasLoadedData()
    {
        return ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(
            _viewModel.ChartState.LastContext,
            _viewModel.MetricState.SelectedMetricType,
            _viewModel.MetricState.SelectedSeries,
            _viewModel.MetricState.FromDate,
            _viewModel.MetricState.ToDate,
            _viewModel.MetricState.ResolutionTableName);
    }

    private bool HasRenderableContext()
    {
        return ChartContextSelectionGuard.HasRenderableContext(_viewModel.ChartState.LastContext, _viewModel.MetricState.SelectedMetricType);
    }

    private bool ShouldRefreshDateRangeForCurrentSelection()
    {
        return !string.IsNullOrWhiteSpace(_viewModel.MetricState.SelectedMetricType);
    }

    private async Task LoadDateRangeForSelectedMetrics()
    {
        if (_isInitializing)
            return;

        if (_viewModel.UiState.IsLoadingSubtypes || _viewModel.UiState.IsLoadingMetricTypes)
            return;

        if (_isChangingResolution)
            return;

        using var busyScope = BeginUiBusyScope();
        await _viewModel.RefreshDateRangeForCurrentSelectionAsync();
    }

    private void UpdateSelectedSubtypesInViewModel()
    {
        var selectedSeries = GetSelectedSeriesFromUi();
        _viewModel.SetSelectedSeries(selectedSeries);
    }

    private List<MetricSeriesSelection> GetSelectedSeriesFromUi()
    {
        return _selectorManager.GetSelectedSeries().ToList();
    }

    private async void OnLoadData(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
            return;

        RecordSessionMilestone("SyncfusionLoadRequested", "Info", "Syncfusion load requested.");

        var isValid = await LoadDataAndValidate();
        if (!isValid)
        {
            RecordSessionMilestone("SyncfusionLoadValidationFailed", "Warning", "Syncfusion load validation failed.");
            return;
        }

        await LoadMetricData();
    }

    private Task<bool> LoadDataAndValidate()
    {
        var isValid = _loadCoordinator.ValidateAndPrepareLoad(
            new SyncfusionChartsViewLoadCoordinator.LoadValidationInput(
                GetSelectedMetricValue(TablesCombo),
                FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30),
                ToDate.SelectedDate ?? DateTime.UtcNow),
            new SyncfusionChartsViewLoadCoordinator.ValidationActions(
                _viewModel.BeginSelectionStateBatch,
                _viewModel.SetSelectedMetricType,
                UpdateSelectedSubtypesInViewModel,
                (fromDate, toDate) => _viewModel.SetDateRange(fromDate, toDate),
                _viewModel.ValidateDataLoadRequirements,
                (title, message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning)));

        return Task.FromResult(isValid);
    }

    private async Task LoadMetricData()
    {
        await _loadCoordinator.ExecuteLoadAsync(
            new SyncfusionChartsViewLoadCoordinator.LoadExecutionActions(
                () => ChartPresentationSpine.LoadMetricDataIntoLastContextAsync(_viewModel),
                () =>
                {
                    _viewModel.ChartState.LastContext = null;
                    _viewModel.ChartState.LastLoadRuntime = null;
                },
                () => ChartPresentationSpine.PublishLastContextAndRequestChartUpdate(_viewModel),
                (title, message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error)));
    }

    private async void OnDataLoaded(object? sender, DataLoadedEventArgs e)
    {
        var ctx = e.DataContext ?? _viewModel.ChartState.LastContext;
        if (ctx == null)
            return;

        RecordSessionMilestone("SyncfusionDataLoaded", "Success", "Syncfusion data-loaded event received.");
        await RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, ctx);
        UpdateSyncfusionToggleEnabled();
    }

    private void OnSelectionStateChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.Invoke(() => OnSelectionStateChanged(sender, e));
            return;
        }

        ApplySelectionStateToUi();
    }

    private async void AddSubtypeComboBox(object sender, RoutedEventArgs e)
    {
        if (_subtypeList == null || !_subtypeList.Any())
            return;

        using (var comboSuppression = _selectorManager.SuppressSelectionChanged())
        {
            _selectorManager.EnsurePrimarySelection();
            var metricType = GetSelectedMetricOption(TablesCombo);
            _selectorManager.AddSubtypeCombo(_subtypeList, metricType);
        }

        using var selectionBatch = _viewModel.BeginSelectionStateBatch();
        UpdateSelectedSubtypesInViewModel();

        if (_viewModel.ChartState.LastContext != null)
            await RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, _viewModel.ChartState.LastContext);
        else if (ShouldRefreshDateRangeForCurrentSelection())
            await LoadDateRangeForSelectedMetrics();
    }

    private void OnSunburstBucketCountChanged(object? sender, int bucketCount)
    {
        if (_isInitializing || _isApplyingSelectionSync)
            return;

        _viewModel.SetBarPieBucketCount(bucketCount);

        if (_viewModel.ChartState.IsBarPieVisible)
            _viewModel.RequestChartUpdate(false, ChartControllerKeys.BarPie);

        var ctx = _viewModel.ChartState.LastContext;
        if (ctx != null)
            _ = RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, ctx);
    }

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        // Syncfusion Sunburst does not expose a zoom reset in this POC.
        RecordSessionMilestone(
            "SyncfusionZoomResetRequested",
            "Info",
            "Syncfusion Sunburst does not expose a zoom reset in this POC.");
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        _themeCoordinator.ToggleTheme();
        RecordSessionMilestone(
            "ThemeToggled",
            "Success",
            $"Theme set to {Theming.AppThemeService.Default.CurrentTheme}.");
    }

    private void RecordSessionMilestone(string kind, string outcome, string? note = null)
    {
        _viewModel.ChartState.RecordSessionMilestone(new SessionMilestoneSnapshot
        {
            TimestampUtc = DateTime.UtcNow,
            Kind = kind,
            Outcome = outcome,
            MetricType = _viewModel.MetricState.SelectedMetricType,
            SelectedSeriesCount = _viewModel.MetricState.SelectedSeries.Count,
            SelectedDisplayKeys = _viewModel.MetricState.SelectedSeries.Select(series => series.DisplayKey).ToList(),
            RuntimePath = _viewModel.ChartState.LastLoadRuntime?.RuntimePath,
            LoadedSeriesCount = _viewModel.ChartState.LastContext?.ActualSeriesCount ?? 0,
            ContextSignature = EvidenceDiagnosticsBuilder.BuildContextSignature(_viewModel.ChartState.LastContext),
            Note = note
        });
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        const string defaultResolution = "All";

        _loadCoordinator.ClearSelection(
            defaultResolution,
            isDefaultResolutionSelected: ResolutionCombo.SelectedItem?.ToString() == defaultResolution,
            new SyncfusionChartsViewLoadCoordinator.ClearActions(
                _evidenceExportService.ClearEvidence,
                _viewModel.SetSelectedSeries,
                () =>
                {
                    _viewModel.ChartState.LastContext = new ChartDataContext();
                    _viewModel.ChartState.LastLoadRuntime = null;
                },
                () => ClearChart(SyncfusionChartsViewCoordinator.ManagedChartKey),
                UpdateSyncfusionToggleEnabled,
                ResetForResolutionChange,
                resolution => ResolutionCombo.SelectedItem = resolution));
    }

    private void ResetDateRangeToDefault()
    {
        _dateRangeCoordinator.ApplyDefaultRange(DateTime.UtcNow, CreateDateRangeActions());
    }

    private async void OnExportReachability(object sender, RoutedEventArgs e)
    {
        RecordSessionMilestone("SyncfusionExportRequested", "Info", "Syncfusion evidence export requested.");

        try
        {
            var result = await _evidenceExportService.ExportAsync(_viewModel.ChartState, _viewModel.MetricState, DateTime.UtcNow, "Syncfusion");
            RecordSessionMilestone("SyncfusionExportCompleted", "Success", $"Syncfusion evidence exported to {result.FilePath}.");

            if (!result.HadReachabilityRecords)
                MessageBox.Show("No reachability records captured yet. Export will include parity data if available.", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Information);

            if (result.Warnings.Count > 0)
                MessageBox.Show($"Export includes parity warnings:\n- {string.Join("\n- ", result.Warnings)}", "Parity Warnings", MessageBoxButton.OK, MessageBoxImage.Warning);

            MessageBox.Show($"Evidence snapshot exported to:\n{result.FilePath}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            RecordSessionMilestone("SyncfusionExportFailed", "Error", ex.Message);
            MessageBox.Show($"Failed to export evidence snapshot:\n{ex.Message}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSunburstToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleSyncfusionSunburst();
    }

    private void SyncCmsToggleStates()
    {
        CmsEnableCheckBox.IsChecked = CmsConfiguration.UseCmsData;
        CmsSingleCheckBox.IsChecked = CmsConfiguration.UseCmsForSingleMetric;
        CmsCombinedCheckBox.IsChecked = CmsConfiguration.UseCmsForCombinedMetric;
        CmsMultiCheckBox.IsChecked = CmsConfiguration.UseCmsForMultiMetric;
        CmsNormalizedCheckBox.IsChecked = CmsConfiguration.UseCmsForNormalized;
        CmsWeeklyCheckBox.IsChecked = CmsConfiguration.UseCmsForWeeklyDistribution;
        CmsWeekdayTrendCheckBox.IsChecked = CmsConfiguration.UseCmsForWeekdayTrend;
        CmsHourlyCheckBox.IsChecked = CmsConfiguration.UseCmsForHourlyDistribution;
        CmsBarPieCheckBox.IsChecked = CmsConfiguration.UseCmsForBarPie;

        UpdateCmsToggleEnablement();
    }

    private void UpdateCmsToggleEnablement()
    {
        var enabled = CmsEnableCheckBox.IsChecked == true;
        CmsSingleCheckBox.IsEnabled = enabled;
        CmsCombinedCheckBox.IsEnabled = enabled;
        CmsMultiCheckBox.IsEnabled = enabled;
        CmsNormalizedCheckBox.IsEnabled = enabled;
        CmsWeeklyCheckBox.IsEnabled = enabled;
        CmsWeekdayTrendCheckBox.IsEnabled = enabled;
        CmsHourlyCheckBox.IsEnabled = enabled;
        CmsBarPieCheckBox.IsEnabled = enabled;
    }

    private void OnCmsToggleChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
            return;

        CmsConfiguration.UseCmsData = CmsEnableCheckBox.IsChecked == true;
        UpdateCmsToggleEnablement();
    }

    private void OnCmsStrategyToggled(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
            return;

        CmsConfiguration.UseCmsForSingleMetric = CmsSingleCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForCombinedMetric = CmsCombinedCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForMultiMetric = CmsMultiCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForNormalized = CmsNormalizedCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForWeeklyDistribution = CmsWeeklyCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForWeekdayTrend = CmsWeekdayTrendCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForHourlyDistribution = CmsHourlyCheckBox.IsChecked == true;
        CmsConfiguration.UseCmsForBarPie = CmsBarPieCheckBox.IsChecked == true;
    }

    private static string ResolveConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
    }

    private void SyncInitialChartState()
    {
        _viewModel.ChartState.IsSyncfusionSunburstVisible = true;
        ResolveController(SyncfusionChartsViewCoordinator.ManagedChartKey).SetVisible(_viewModel.ChartState.IsSyncfusionSunburstVisible);
        UpdateSyncfusionToggleEnabled();
    }

    private void UpdateSyncfusionToggleEnabled()
    {
        ResolveController(SyncfusionChartsViewCoordinator.ManagedChartKey).SetToggleEnabled(HasLoadedData());
    }

    private IChartController ResolveController(string key)
    {
        return _chartControllerRegistry.Get(key);
    }

    private async Task RenderChartAsync(string key, ChartDataContext ctx)
    {
        try
        {
            await ResolveController(key).RenderAsync(ctx);
            RecordSessionMilestone("SyncfusionRenderCompleted", "Success", $"Rendered {key}.");
        }
        catch (Exception ex)
        {
            RecordSessionMilestone("SyncfusionRenderFailed", "Error", $"Render failed for {key}: {ex.Message}");
            throw;
        }
    }

    private void ClearChart(string key)
    {
        ResolveController(key).Clear(_viewModel.ChartState);
    }

    private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
    {
        if (!SyncfusionChartsViewCoordinator.IsRegisteredKey(e.ChartName))
            return;

        ResolveController(e.ChartName).SetVisible(e.IsVisible);
    }

    private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (_coordinator.ShouldRenderAfterVisibilityOnlyToggle(e, ctx))
            await RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, ctx!);
    }

    private async void OnViewVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (_coordinator.ShouldRenderWhenViewBecomesVisible(
                _isInitializing,
                e.NewValue is bool isVisible && isVisible,
                _viewModel.ChartState.IsSyncfusionSunburstVisible,
                ctx))
            await RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, ctx!);
    }


    private void ApplySelectionStateToUi()
    {
        if (_isApplyingSelectionSync)
            return;

        _isApplyingSelectionSync = true;
        try
        {
            _stateSyncCoordinator.Apply(_viewModel, TablesCombo.Items.OfType<MetricNameOption>(), CreateStateSyncActions());
        }
        finally
        {
            _isApplyingSelectionSync = false;
        }
    }

    private MainChartsViewStateSyncCoordinator.Actions CreateStateSyncActions()
    {
        return new MainChartsViewStateSyncCoordinator.Actions(
            targetResolution =>
            {
                if (ResolutionCombo.SelectedItem?.ToString() != targetResolution)
                    ResolutionCombo.SelectedItem = targetResolution;
            },
            fromDate =>
            {
                if (FromDate.SelectedDate != fromDate)
                    FromDate.SelectedDate = fromDate;
            },
            toDate =>
            {
                if (ToDate.SelectedDate != toDate)
                    ToDate.SelectedDate = toDate;
            },
            metricType =>
            {
                if (metricType == null)
                    return;

                var existing = TablesCombo.SelectedItem as MetricNameOption;
                if (existing != null && string.Equals(existing.Value, metricType.Value, StringComparison.OrdinalIgnoreCase))
                    return;

                TablesCombo.SelectedItem = metricType;
            },
            (selections, selectedMetricType) =>
            {
                if (_subtypeList == null || _subtypeList.Count == 0)
                    return;

                MainChartsViewStateSyncCoordinator.ApplySubtypeSelections(
                    _selectorManager,
                    SubtypeCombo,
                    _subtypeList,
                    selections,
                    selectedMetricType);
            },
            bucketCount => SunburstPanel.SetBucketCount(bucketCount));
    }

    private ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions CreateMetricTypesLoadedActions()
    {
        return new ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions(
            () => TablesCombo.Items.Clear(),
            option => TablesCombo.Items.Add(option),
            () => TablesCombo.Items.Count,
            value => _isApplyingSelectionSync = value,
            _viewModel.BeginSelectionStateBatch,
            index => TablesCombo.SelectedIndex = index,
            () => GetSelectedMetricValue(TablesCombo),
            _viewModel.SetSelectedMetricType,
            () => _viewModel.LoadSubtypesCommand.Execute(null),
            () => SubtypeCombo.Items.Clear(),
            value => SubtypeCombo.IsEnabled = value,
            () => _selectorManager.ClearDynamic());
    }

    private ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions CreateMetricTypeSelectionChangedActions()
    {
        return new ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions(
            value => _isMetricTypeChangePending = value,
            value => _isApplyingSelectionSync = value,
            _viewModel.BeginSelectionStateBatch,
            () =>
            {
                _subtypeList = null;
                ClearChart(SyncfusionChartsViewCoordinator.ManagedChartKey);
                _viewModel.ChartState.LastContext = null;
                _viewModel.ChartState.LastLoadRuntime = null;
            },
            _viewModel.SetSelectedMetricType,
            () => _selectorManager.ClearAllSubtypeControls(),
            UpdateSelectedSubtypesInViewModel,
            () => _viewModel.LoadSubtypesCommand.Execute(null));
    }

    private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions()
    {
        return new ChartHostMetricSelectionCoordinator.SubtypesLoadedActions(
            value => _isApplyingSelectionSync = value,
            _selectorManager.SuppressSelectionChanged,
            _viewModel.BeginSelectionStateBatch,
            (subtypes, preserveSelection, selectedMetricType) => RefreshPrimarySubtypeCombo(subtypes, preserveSelection, selectedMetricType),
            subtypes => BuildDynamicSubtypeControls(subtypes),
            UpdateSelectedSubtypesInViewModel,
            value => _isMetricTypeChangePending = value);
    }

    private ChartHostDateRangeCoordinator.Actions CreateDateRangeActions()
    {
        return new ChartHostDateRangeCoordinator.Actions(
            _viewModel.SetDateRange,
            value => FromDate.SelectedDate = value,
            value => ToDate.SelectedDate = value,
            () => _viewModel.MetricState.FromDate,
            () => _viewModel.MetricState.ToDate);
    }

}
