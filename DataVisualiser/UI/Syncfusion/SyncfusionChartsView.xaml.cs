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
using DataVisualiser.Shared.Events;
using DataVisualiser.UI.Events;
using DataVisualiser.UI;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.SyncfusionViews;

public partial class SyncfusionChartsView : UserControl
{
    private readonly SyncfusionChartsViewCoordinator _coordinator = new();
    private SyncfusionEvidenceExportService _evidenceExportService = null!;
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

    public SyncfusionChartsView()
    {
        InitializeComponent();

        InitializeInfrastructure();
        InitializeViewModel();
        InitializeUiBindings();
        ExecuteStartupSequence();

        IsVisibleChanged += OnViewVisibilityChanged;
    }

    private IDisposable BeginUiBusyScope()
    {
        _uiBusyDepth++;
        if (_uiBusyDepth == 1)
            _uiState.IsUiBusy = true;

        return new UiBusyScope(this);
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
        _evidenceExportService = new SyncfusionEvidenceExportService(
            new ReachabilityExportWriter(),
            new ReachabilityExportPathResolver(),
            new StrategyReachabilityEvidenceStore());
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
        SunburstPanel.ToggleRequested += OnSunburstToggleRequested;
        SunburstPanel.BucketCountChanged += OnSunburstBucketCountChanged;
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
        var initialFromDate = DateTime.UtcNow.AddDays(-30);
        var initialToDate = DateTime.UtcNow;

        _viewModel.SetDateRange(initialFromDate, initialToDate);

        FromDate.SelectedDate = _viewModel.MetricState.FromDate;
        ToDate.SelectedDate = _viewModel.MetricState.ToDate;
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

        _isMetricTypeChangePending = true;
        _isApplyingSelectionSync = true;
        try
        {
            using var selectionBatch = _viewModel.BeginSelectionStateBatch();
            _subtypeList = null;
            ClearChart(SyncfusionChartsViewCoordinator.ManagedChartKey);
            _viewModel.ChartState.LastContext = null;
            _viewModel.SetSelectedMetricType(GetSelectedMetricValue(TablesCombo));
            _selectorManager.ClearAllSubtypeControls();
            UpdateSelectedSubtypesInViewModel();
        }
        finally
        {
            _isApplyingSelectionSync = false;
        }

        _viewModel.LoadSubtypesCommand.Execute(null);
    }

    private static string? GetSelectedMetricValue(ComboBox combo)
    {
        if (combo.SelectedItem is MetricNameOption option)
            return option.Value;

        return combo.SelectedValue?.ToString() ?? combo.SelectedItem?.ToString();
    }

    private static MetricNameOption? GetSelectedMetricOption(ComboBox combo)
    {
        return combo.SelectedItem as MetricNameOption;
    }

    private void OnMetricTypesLoaded(object? sender, MetricTypesLoadedEventArgs e)
    {
        _isMetricTypeChangePending = false;
        TablesCombo.Items.Clear();

        var addedAllMetricType = !e.MetricTypes.Any(type => string.Equals(type.Value, "(All)", StringComparison.OrdinalIgnoreCase));

        if (addedAllMetricType)
            TablesCombo.Items.Add(new MetricNameOption("(All)", "(All)"));

        foreach (var type in e.MetricTypes)
            TablesCombo.Items.Add(type);

        if (TablesCombo.Items.Count > 0)
        {
            _isApplyingSelectionSync = true;
            try
            {
                using var selectionBatch = _viewModel.BeginSelectionStateBatch();
                TablesCombo.SelectedIndex = addedAllMetricType && TablesCombo.Items.Count > 1 ? 1 : 0;
                _viewModel.SetSelectedMetricType(GetSelectedMetricValue(TablesCombo));
            }
            finally
            {
                _isApplyingSelectionSync = false;
            }

            _viewModel.LoadSubtypesCommand.Execute(null);
        }
        else
        {
            SubtypeCombo.Items.Clear();
            SubtypeCombo.IsEnabled = false;
            _selectorManager.ClearDynamic();
        }

        _isChangingResolution = false;
    }

    private void OnSubtypesLoaded(object? sender, SubtypesLoadedEventArgs e)
    {
        var subtypeListLocal = e.Subtypes.ToList();

        _subtypeList = subtypeListLocal;
        var selectedMetricType = GetSelectedMetricOption(TablesCombo);

        _isApplyingSelectionSync = true;
        try
        {
            using var comboSuppression = _selectorManager.SuppressSelectionChanged();
            using var selectionBatch = _viewModel.BeginSelectionStateBatch();

            RefreshPrimarySubtypeCombo(subtypeListLocal, false, selectedMetricType);
            BuildDynamicSubtypeControls(subtypeListLocal);
            UpdateSelectedSubtypesInViewModel();
        }
        finally
        {
            _isApplyingSelectionSync = false;
        }

        if (_isMetricTypeChangePending)
        {
            _isMetricTypeChangePending = false;
            _ = LoadDateRangeForSelectedMetrics();
            return;
        }

        if (!HasLoadedData() && ShouldRefreshDateRangeForCurrentSelection())
            _ = LoadDateRangeForSelectedMetrics();

        if (!_isInitializing && _viewModel.MetricState.SelectedSeries.Count > 0)
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
    }

    private async void OnAnySubtypeSelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        if (_isApplyingSelectionSync)
            return;

        UpdateSelectedSubtypesInViewModel();

        if (_viewModel.ChartState.LastContext != null && !HasLoadedData())
        {
            ClearChart(SyncfusionChartsViewCoordinator.ManagedChartKey);
            _viewModel.ChartState.LastContext = null;
            UpdateSyncfusionToggleEnabled();
        }

        if (_coordinator.ShouldRenderAfterSubtypeSelectionChange(_isApplyingSelectionSync, HasLoadedData(), _viewModel.ChartState.LastContext))
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
            _viewModel.MetricState.SelectedSeries);
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

        var isValid = await LoadDataAndValidate();
        if (!isValid)
            return;

        await LoadMetricData();
    }

    private Task<bool> LoadDataAndValidate()
    {
        var selectedMetricType = GetSelectedMetricValue(TablesCombo);
        if (selectedMetricType == null)
        {
            MessageBox.Show("Please select a Metric Type", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.FromResult(false);
        }

        var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;
        using (var selectionBatch = _viewModel.BeginSelectionStateBatch())
        {
            _viewModel.SetSelectedMetricType(selectedMetricType);
            UpdateSelectedSubtypesInViewModel();
            _viewModel.SetDateRange(fromDate, toDate);
        }

        var (isValid, errorMessage) = _viewModel.ValidateDataLoadRequirements();
        if (!isValid)
        {
            MessageBox.Show(errorMessage ?? "The current selection is not valid.", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private async Task LoadMetricData()
    {
        try
        {
            var dataLoaded = await ChartPresentationSpine.LoadMetricDataIntoLastContextAsync(_viewModel);
            if (!dataLoaded)
            {
                _viewModel.ChartState.LastContext = null;
                return;
            }

            ChartPresentationSpine.PublishLastContextAndRequestChartUpdate(_viewModel);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _viewModel.ChartState.LastContext = null;
        }
    }

    private async void OnDataLoaded(object? sender, DataLoadedEventArgs e)
    {
        var ctx = e.DataContext ?? _viewModel.ChartState.LastContext;
        if (ctx == null)
            return;

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

    private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
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
    }

    private void OnSunburstBucketCountChanged(object? sender, int bucketCount)
    {
        if (_isInitializing || _isApplyingSelectionSync)
            return;

        _viewModel.SetBarPieBucketCount(bucketCount);

        if (_viewModel.ChartState.IsBarPieVisible)
            _viewModel.RequestChartUpdate(false, ChartControllerKeys.BarPie);

        if (HasLoadedData())
            _ = RenderChartAsync(SyncfusionChartsViewCoordinator.ManagedChartKey, _viewModel.ChartState.LastContext ?? new ChartDataContext());
    }

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        // Syncfusion Sunburst does not expose a zoom reset in this POC.
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        const string defaultResolution = "All";

        _evidenceExportService.ClearEvidence();
        _viewModel.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
        _viewModel.ChartState.LastContext = new ChartDataContext();
        ClearChart(SyncfusionChartsViewCoordinator.ManagedChartKey);
        UpdateSyncfusionToggleEnabled();

        if (ResolutionCombo.SelectedItem?.ToString() == defaultResolution)
        {
            ResetForResolutionChange(defaultResolution);
            return;
        }

        ResolutionCombo.SelectedItem = defaultResolution;
    }

    private void ResetDateRangeToDefault()
    {
        var initialFromDate = DateTime.UtcNow.AddDays(-30);
        var initialToDate = DateTime.UtcNow;

        _viewModel.SetDateRange(initialFromDate, initialToDate);
        FromDate.SelectedDate = _viewModel.MetricState.FromDate;
        ToDate.SelectedDate = _viewModel.MetricState.ToDate;
    }

    private void OnExportReachability(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = _evidenceExportService.Export(_viewModel.ChartState, _viewModel.MetricState, DateTime.UtcNow);
            MessageBox.Show($"Syncfusion evidence snapshot exported to:\n{result.FilePath}\n\nNotes:\n- {string.Join("\n- ", result.Notes)}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to export Syncfusion evidence snapshot:\n{ex.Message}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Error);
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

    private Task RenderChartAsync(string key, ChartDataContext ctx)
    {
        return ResolveController(key).RenderAsync(ctx);
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
            ApplyResolutionFromState();
            ApplyDateRangeFromState();
            ApplyMetricTypeFromState();
            ApplySubtypeSelectionsFromState();
            SunburstPanel.SetBucketCount(_viewModel.ChartState.BarPieBucketCount);
        }
        finally
        {
            _isApplyingSelectionSync = false;
        }
    }

    private void ApplyResolutionFromState()
    {
        var targetResolution = ChartUiHelper.GetResolutionFromTableName(_viewModel.MetricState.ResolutionTableName);
        if (ResolutionCombo.SelectedItem?.ToString() != targetResolution)
            ResolutionCombo.SelectedItem = targetResolution;
    }

    private void ApplyDateRangeFromState()
    {
        if (_viewModel.MetricState.FromDate.HasValue && FromDate.SelectedDate != _viewModel.MetricState.FromDate)
            FromDate.SelectedDate = _viewModel.MetricState.FromDate;

        if (_viewModel.MetricState.ToDate.HasValue && ToDate.SelectedDate != _viewModel.MetricState.ToDate)
            ToDate.SelectedDate = _viewModel.MetricState.ToDate;
    }

    private void ApplyMetricTypeFromState()
    {
        var selectedMetric = _viewModel.MetricState.SelectedMetricType;
        if (string.IsNullOrWhiteSpace(selectedMetric))
            return;

        var existing = TablesCombo.SelectedItem as MetricNameOption;
        if (existing != null && string.Equals(existing.Value, selectedMetric, StringComparison.OrdinalIgnoreCase))
            return;

        var match = TablesCombo.Items.OfType<MetricNameOption>()
            .FirstOrDefault(item => string.Equals(item.Value, selectedMetric, StringComparison.OrdinalIgnoreCase));

        if (match != null)
            TablesCombo.SelectedItem = match;
    }

    private void ApplySubtypeSelectionsFromState()
    {
        if (_subtypeList == null || _subtypeList.Count == 0)
            return;

        var selections = _viewModel.MetricState.SelectedSeries.ToList();

        if (selections.Count == 0)
            return;

        var metricType = GetSelectedMetricOption(TablesCombo);
        using var comboSuppression = _selectorManager.SuppressSelectionChanged();
        _selectorManager.ClearDynamic();
        _selectorManager.SetPrimaryMetricType(metricType);

        SetComboSelectionByValue(SubtypeCombo, selections[0].QuerySubtype);

        for (var i = 1; i < selections.Count; i++)
        {
            var combo = _selectorManager.AddSubtypeCombo(_subtypeList, metricType);
            SetComboSelectionByValue(combo, selections[i].QuerySubtype);
        }
    }

    private static void SetComboSelectionByValue(ComboBox combo, string? value)
    {
        if (combo.Items.Count == 0)
            return;

        if (string.IsNullOrWhiteSpace(value))
        {
            combo.SelectedIndex = 0;
            return;
        }

        var match = combo.Items.OfType<MetricNameOption>()
            .FirstOrDefault(item => string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            combo.SelectedItem = match;
            return;
        }

        combo.SelectedIndex = 0;
    }


    private sealed class UiBusyScope : IDisposable
    {
        private readonly SyncfusionChartsView _owner;
        private bool _disposed;

        public UiBusyScope(SyncfusionChartsView owner)
        {
            _owner = owner;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _owner.EndUiBusyScope();
        }
    }
}
