using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Data.Repositories;
using DataVisualiser.Models;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.UI.SubtypeSelectors;
using DataVisualiser.ViewModels;
using DataVisualiser.ViewModels.Events;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Configuration;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ChartHelper = DataVisualiser.Charts.Helpers.ChartHelper;
using ParityValidationService = DataVisualiser.Services.ParityValidationService;

namespace DataVisualiser
{
    public partial class MainWindow : Window
    {
        private readonly ChartComputationEngine _chartComputationEngine;
        private readonly ChartRenderEngine _chartRenderEngine;
        private ChartTooltipManager? _tooltipManager;

        private readonly string _connectionString;
        private List<string>? subtypeList;

        private MetricSelectionService _metricSelectionService;
        private ChartUpdateCoordinator _chartUpdateCoordinator;
        private WeeklyDistributionService _weeklyDistributionService;
        private SubtypeSelectorManager _selectorManager;

        private readonly ChartState _chartState = new();
        private readonly MetricState _metricState = new();
        private readonly UiState _uiState = new();
        private readonly MainWindowViewModel _viewModel;

        private bool _isInitializing = true;
        private bool _isChangingResolution = false;
        private const bool ENABLE_COMBINED_CMS_PARITY = false;
        private const bool EnableCombinedMetricParity = false;
        private const bool ENABLE_COMBINED_METRIC_PARITY = false;

        public MainWindow()
        {
            InitializeComponent();

            // Pin window to top-left corner
            Left = 0;
            Top = 0;

            _connectionString =
                ConfigurationManager.AppSettings["HealthDB"]
                ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";

            _metricSelectionService = new MetricSelectionService(_connectionString);

            _chartComputationEngine = new ChartComputationEngine();
            _chartRenderEngine = new ChartRenderEngine();

            InitializeTooltips();

            _chartUpdateCoordinator = new ChartUpdateCoordinator(
                _chartComputationEngine,
                _chartRenderEngine,
                _tooltipManager,
                _chartState.ChartTimestamps);

            _chartUpdateCoordinator.SeriesMode = ChartSeriesMode.RawAndSmoothed;
            _weeklyDistributionService = new WeeklyDistributionService(_chartState.ChartTimestamps);

            _viewModel = new MainWindowViewModel(
                _chartState,
                _metricState,
                _uiState,
                _metricSelectionService,
                _chartUpdateCoordinator,
                _weeklyDistributionService);

            DataContext = _viewModel;

            WireViewModelEvents();

            // Initialize date range through viewModel
            var initialFromDate = DateTime.UtcNow.AddDays(-30);
            var initialToDate = DateTime.UtcNow;
            _viewModel.SetDateRange(initialFromDate, initialToDate);
            FromDate.SelectedDate = _viewModel.MetricState.FromDate;
            ToDate.SelectedDate = _viewModel.MetricState.ToDate;

            InitializeDefaultUiState();
            InitializeSubtypeSelector();

            InitializeResolutionCombo();

            // Set initial resolution selection
            ResolutionCombo.SelectedItem = "All";

            _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
            _viewModel.LoadMetricsCommand.Execute(null);

            InitializeChartBehavior();
            ClearChartsOnStartup();
            DisableAxisLabelsWhenNoData();
            SetDefaultChartTitles();

            _viewModel.RequestChartUpdate();

            Closing += MainWindow_Closing;

            // Mark initialization as complete - event handlers can now safely use _viewModel
            _isInitializing = false;

            // Initialize button states based on current subtype selection
            UpdateSecondaryDataRequiredButtonStates(_viewModel.MetricState.SelectedSubtypes.Count);
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose tooltip manager to prevent memory leaks
            _tooltipManager?.Dispose();
            _tooltipManager = null;
        }

        #region Initialization

        private void InitializeTooltips()
        {
            var chartLabels = new Dictionary<CartesianChart, string>
            {
                { ChartMain, "Main" },
                { ChartNorm, "Norm" },
                { ChartDiff, "Diff" },
                { ChartRatio, "Ratio" }
            };

            _tooltipManager = new ChartTooltipManager(this, chartLabels);
            _tooltipManager.AttachChart(ChartMain, "Main");
            _tooltipManager.AttachChart(ChartNorm, "Norm");
            _tooltipManager.AttachChart(ChartDiff, "Diff");
            _tooltipManager.AttachChart(ChartRatio, "Ratio");
        }

        private void WireViewModelEvents()
        {
            _viewModel.ChartVisibilityChanged += OnChartVisibilityChanged;
            _viewModel.ErrorOccured += OnErrorOccured;
            _viewModel.MetricTypesLoaded += OnMetricTypesLoaded;
            _viewModel.SubtypesLoaded += OnSubtypesLoaded;
            _viewModel.DateRangeLoaded += OnDateRangeLoaded;
            _viewModel.DataLoaded += OnDataLoaded;
            _viewModel.ChartUpdateRequested += OnChartUpdateRequested;
        }

        private void InitializeDefaultUiState()
        {
            _viewModel.SetLoadingMetricTypes(false);
            _viewModel.SetLoadingSubtypes(false);
            _viewModel.SetNormalizedVisible(false);
            _viewModel.SetDifferenceVisible(false);
            _viewModel.SetRatioVisible(false);
            _viewModel.SetWeeklyVisible(false);
            _viewModel.CompleteInitialization();

            _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
            _viewModel.ChartState.LastContext = new ChartDataContext();
        }

        private void InitializeSubtypeSelector()
        {
            _selectorManager = new SubtypeSelectorManager(MetricSubtypePanel, SubtypeCombo);

            _selectorManager.SubtypeSelectionChanged += (s, e) =>
            {
                UpdateChartTitlesFromCombos();
                OnAnySubtypeSelectionChanged(s ?? this, null);
            };
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

        private void InitializeChartBehavior()
        {
            ChartHelper.InitializeChartBehavior(ChartMain);
            ChartHelper.InitializeChartBehavior(ChartWeekly);
            ChartHelper.InitializeChartBehavior(ChartNorm);
            ChartHelper.InitializeChartBehavior(ChartDiff);
            ChartHelper.InitializeChartBehavior(ChartRatio);
        }

        private void ClearChartsOnStartup()
        {
            // Clear charts on startup to prevent gibberish tick labels
            ChartHelper.ClearChart(ChartMain, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
        }

        private void DisableAxisLabelsWhenNoData()
        {
            DisableAxisLabels(ChartMain);
            DisableAxisLabels(ChartNorm);
            DisableAxisLabels(ChartDiff);
            DisableAxisLabels(ChartRatio);
            DisableAxisLabels(ChartWeekly);
        }

        private static void DisableAxisLabels(CartesianChart chart)
        {
            if (chart.AxisX.Count > 0) chart.AxisX[0].ShowLabels = false;
            if (chart.AxisY.Count > 0) chart.AxisY[0].ShowLabels = false;
        }

        private void SetDefaultChartTitles()
        {
            ChartMainTitle.Text = "Metrics: Total";
            ChartNormTitle.Text = "Metrics: Normalized";
            ChartDiffTitle.Text = "Metric Difference";
            ChartRatioTitle.Text = "Metrics: Ratio";
        }

        #endregion

        #region Data Loading and Selection Event Handlers

        /// <summary>
        /// Event handler for Resolution selection change - reloads MetricTypes from the selected table.
        /// </summary>
        private void OnResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionCombo.SelectedItem == null)
                return;

            // Store the selected resolution to retain it
            var selectedResolution = ResolutionCombo.SelectedItem.ToString();

            // Set flag to suppress error popups during resolution change
            _isChangingResolution = true;

            // Clear all charts when resolution changes
            ClearAllCharts();

            // Prevent error popups during resolution change by temporarily suppressing validation
            _viewModel.MetricState.SelectedMetricType = null; // Clear to prevent validation errors

            TablesCombo.Items.Clear();

            _selectorManager.ClearDynamic();
            SubtypeCombo.Items.Clear();
            SubtypeCombo.IsEnabled = false;

            _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
            _viewModel.LoadMetricsCommand.Execute(null);

            // Restore the resolution selection (in case it was changed)
            if (ResolutionCombo.SelectedItem?.ToString() != selectedResolution)
            {
                ResolutionCombo.SelectedItem = selectedResolution;
            }

            // Update button states since subtypes are cleared when resolution changes
            UpdateSecondaryDataRequiredButtonStates(0);
        }

        /// <summary>
        /// Event handler for MetricType selection change - loads subtypes and updates date range.
        /// </summary>
        private void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            if (_viewModel.UiState.IsLoadingMetricTypes)
                return;

            // Clear all charts when metric type changes
            ClearAllCharts();

            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());
            _viewModel.LoadSubtypesCommand.Execute(null);

            UpdateChartTitlesFromCombos();

            // Update button states since subtypes will be cleared when metric type changes
            UpdateSecondaryDataRequiredButtonStates(0);
        }

        /// <summary>
        /// Generalized event handler for any MetricSubtype ComboBox selection change - updates date range to match data availability.
        /// This handler is used by all subtype ComboBoxes (both static and dynamically added).
        /// </summary>
        private async void OnAnySubtypeSelectionChanged(object? sender, SelectionChangedEventArgs? e)
        {
            UpdateSelectedSubtypesInViewModel();

            if (!string.IsNullOrWhiteSpace(_viewModel.MetricState.SelectedMetricType))
            {
                await LoadDateRangeForSelectedMetrics();
            }
        }

        private async Task LoadDateRangeForSelectedMetrics()
        {
            if (_isInitializing)
                return;

            if (_viewModel.UiState.IsLoadingSubtypes || _viewModel.UiState.IsLoadingMetricTypes)
                return;

            if (_isChangingResolution)
                return;

            await _viewModel.RefreshDateRangeForCurrentSelectionAsync();

            UpdateChartTitlesFromCombos();
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
            var selectedMetricType = TablesCombo.SelectedItem?.ToString();
            if (selectedMetricType == null)
            {
                MessageBox.Show("Please select a Metric Type", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }

            _viewModel.SetSelectedMetricType(selectedMetricType);
            UpdateSelectedSubtypesInViewModel();

            var primarySubtype = _selectorManager.GetPrimarySubtype();
            var secondarySubtype = _selectorManager.GetSecondarySubtype();

            string baseType = _viewModel.MetricState.SelectedMetricType!;
            string display1 = primarySubtype != null && primarySubtype != "(All)" ? primarySubtype : baseType;
            string display2 = secondarySubtype ?? string.Empty;

            SetChartTitles(display1, display2);
            UpdateChartLabels(primarySubtype ?? string.Empty, secondarySubtype ?? string.Empty);

            var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;
            _viewModel.SetDateRange(fromDate, toDate);

            var (isValid, errorMessage) = _viewModel.ValidateDataLoadRequirements();
            if (!isValid)
            {
                MessageBox.Show(errorMessage ?? "The current selection is not valid.",
                    "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        private async Task LoadMetricData()
        {
            try
            {
                var dataLoaded = await _viewModel.LoadMetricDataAsync();
                if (!dataLoaded)
                {
                    ClearAllCharts();
                    return;
                }

                _viewModel.LoadDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ClearAllCharts();
            }
        }

        private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
        {
            if (subtypeList == null || !subtypeList.Any()) return;

            var newCombo = _selectorManager.AddSubtypeCombo(subtypeList);
            newCombo.SelectedIndex = 0;
            newCombo.IsEnabled = true;

            UpdateChartTitlesFromCombos();
            UpdateSelectedSubtypesInViewModel();
        }

        #endregion

        #region Chart Visibility Toggle Handlers

        private void OnChartNormToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleNorm();
            _viewModel.RequestChartUpdate();
        }

        private void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleWeekly();
            _viewModel.RequestChartUpdate();
        }

        private void OnChartWeekdayTrendToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleWeeklyTrend();
            _viewModel.RequestChartUpdate();
        }

        private void OnChartDiffToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleDiff();
            _viewModel.RequestChartUpdate();
        }

        private void OnChartRatioToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleRatio();
            _viewModel.RequestChartUpdate();
        }

        #endregion

        #region Chart Configuration and Helper Methods

        private void OnResetZoom(object sender, RoutedEventArgs e)
        {
            ChartHelper.ResetZoom(ref ChartMain);
            ChartHelper.ResetZoom(ref ChartNorm);
            ChartHelper.ResetZoom(ref ChartDiff);
            ChartHelper.ResetZoom(ref ChartRatio);
            ChartHelper.ResetZoom(ref ChartWeekly);
        }

        private void SetChartTitles(string leftName, string rightName)
        {
            leftName ??= string.Empty;
            rightName ??= string.Empty;

            _viewModel.ChartState.LeftTitle = leftName;
            _viewModel.ChartState.RightTitle = rightName;

            ChartMainTitle.Text = $"{leftName} vs. {rightName}";
            ChartNormTitle.Text = $"{leftName} ~ {rightName}";
            ChartDiffTitle.Text = $"{leftName} - {rightName}";
            ChartRatioTitle.Text = $"{leftName} / {rightName}";
        }

        private void UpdateChartLabels(string subtype1, string subtype2)
        {
            if (_tooltipManager == null) return;

            subtype1 ??= string.Empty;
            subtype2 ??= string.Empty;

            string baseType = TablesCombo.SelectedItem?.ToString() ?? "";

            string label1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)" ? subtype1 : baseType;
            string label2 = !string.IsNullOrEmpty(subtype2) ? subtype2 : string.Empty;

            string chartMainLabel = !string.IsNullOrEmpty(label2) ? $"{label1} vs {label2}" : label1;
            _tooltipManager.UpdateChartLabel(ChartMain, chartMainLabel);

            string chartDiffLabel = !string.IsNullOrEmpty(label2) ? $"{label1} - {label2}" : label1;
            _tooltipManager.UpdateChartLabel(ChartDiff, chartDiffLabel);

            string chartRatioLabel = !string.IsNullOrEmpty(label2) ? $"{label1} / {label2}" : label1;
            _tooltipManager.UpdateChartLabel(ChartRatio, chartRatioLabel);
        }

        private void UpdateChartTitlesFromCombos()
        {
            string subtype1 = _selectorManager.GetPrimarySubtype() ?? "";
            string subtype2 = _selectorManager.GetSecondarySubtype() ?? "";

            string baseType = TablesCombo.SelectedItem?.ToString() ?? "";

            string display1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)" ? subtype1 : baseType;
            string display2 = !string.IsNullOrEmpty(subtype2) ? subtype2 : "";

            SetChartTitles(display1, display2);
            UpdateChartLabels(subtype1, subtype2);
        }

        #endregion

        #region Normalization mode UI handling

        private async void OnNormalizationModeChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;

            try
            {
                if (NormZeroToOneRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
                else if (NormPercentOfMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
                else if (NormRelativeToMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);

                if (_viewModel.ChartState.IsNormalizedVisible &&
                    _viewModel.ChartState.LastContext?.Data1 != null &&
                    _viewModel.ChartState.LastContext.Data2 != null)
                {
                    var ctx = _viewModel.ChartState.LastContext;

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                        ChartNorm,
                        new NormalizedStrategy(ctx.Data1, ctx.Data2, ctx.DisplayName1, ctx.DisplayName2, ctx.From, ctx.To, _viewModel.ChartState.SelectedNormalizationMode),
                        $"{ctx.DisplayName1} ~ {ctx.DisplayName2}",
                        minHeight: 400);
                }
            }
            catch
            {
                // intentional: mode change shouldn't hard-fail the UI
            }
        }

        #endregion

        #region Weekly distribution display mode UI handling

        private async void OnWeeklyDisplayModeChanged(object sender, RoutedEventArgs e)
        {
            if (_isInitializing)
                return;

            try
            {
                bool newValue = WeeklyFrequencyShadingRadio.IsChecked == true;
                System.Diagnostics.Debug.WriteLine($"OnWeeklyDisplayModeChanged: Setting UseFrequencyShading to {newValue}");

                if (WeeklyFrequencyShadingRadio.IsChecked == true)
                    _viewModel.SetWeeklyFrequencyShading(true);
                else if (WeeklySimpleRangeRadio.IsChecked == true)
                    _viewModel.SetWeeklyFrequencyShading(false);

                System.Diagnostics.Debug.WriteLine($"OnWeeklyDisplayModeChanged: ChartState.UseFrequencyShading = {_viewModel.ChartState.UseFrequencyShading}");

                if (_viewModel.ChartState.IsWeeklyVisible && _viewModel.ChartState.LastContext?.Data1 != null)
                {
                    var ctx = _viewModel.ChartState.LastContext;
                    System.Diagnostics.Debug.WriteLine($"OnWeeklyDisplayModeChanged: Refreshing chart with useFrequencyShading={_viewModel.ChartState.UseFrequencyShading}");

                    await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(
                        ChartWeekly,
                        ctx.Data1,
                        ctx.DisplayName1,
                        ctx.From,
                        ctx.To,
                        minHeight: 400,
                        useFrequencyShading: _viewModel.ChartState.UseFrequencyShading,
                        intervalCount: _viewModel.ChartState.WeeklyIntervalCount);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnWeeklyDisplayModeChanged error: {ex.Message}");
            }
        }

        private async void OnWeeklyIntervalCountChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
                return;

            try
            {
                if (WeeklyIntervalCountCombo.SelectedItem is System.Windows.Controls.ComboBoxItem selectedItem &&
                    selectedItem.Tag is string tagValue &&
                    int.TryParse(tagValue, out int intervalCount))
                {
                    _viewModel.SetWeeklyIntervalCount(intervalCount);

                    if (_viewModel.ChartState.IsWeeklyVisible && _viewModel.ChartState.LastContext?.Data1 != null)
                    {
                        var ctx = _viewModel.ChartState.LastContext;
                        await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(
                            ChartWeekly,
                            ctx.Data1,
                            ctx.DisplayName1,
                            ctx.From,
                            ctx.To,
                            minHeight: 400,
                            useFrequencyShading: _viewModel.ChartState.UseFrequencyShading,
                            intervalCount: _viewModel.ChartState.WeeklyIntervalCount);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnWeeklyIntervalCountChanged error: {ex.Message}");
            }
        }

        #endregion

        private void UpdateSelectedSubtypesInViewModel()
        {
            var selectedSubtypes = new List<string?>();

            string? primary = null;
            if (SubtypeCombo.IsEnabled && SubtypeCombo.SelectedItem != null)
            {
                primary = SubtypeCombo.SelectedItem.ToString();
                if (!string.IsNullOrWhiteSpace(primary) && primary != "(All)")
                    selectedSubtypes.Add(primary);
            }

            var activeCombos = _selectorManager.GetActiveCombos();
            foreach (var combo in activeCombos)
            {
                if (combo.SelectedItem == null) continue;

                var st = combo.SelectedItem.ToString();
                if (string.IsNullOrWhiteSpace(st)) continue;
                if (st == "(All)") continue;
                if (selectedSubtypes.Contains(st)) continue;

                selectedSubtypes.Add(st);
            }

            _viewModel.SetSelectedSubtypes(selectedSubtypes);

            // Update button states based on selected subtype count
            UpdateSecondaryDataRequiredButtonStates(selectedSubtypes.Count);
        }

        /// <summary>
        /// Updates the enabled state of buttons for charts that require secondary data.
        /// These buttons are disabled when fewer than 2 subtypes are selected.
        /// If charts are currently visible when secondary data becomes unavailable, they are cleared and hidden
        /// by leveraging the existing rendering pipeline through state updates.
        /// 
        /// Pipeline flow:
        /// 1. ViewModel.SetXxxVisible(false) -> Sets ChartState.IsXxxVisible = false
        /// 2. RequestChartUpdate() -> Fires ChartUpdateRequested event with updated visibility states
        /// 3. OnChartUpdateRequested() -> Calls UpdateChartVisibility() for each chart
        /// 4. UpdateChartVisibility() -> Updates panel visibility and button text ("Show"/"Hide")
        /// 5. RenderChartsFromLastContext() -> Checks visibility states and clears/renders charts accordingly
        ///    - Uses RenderOrClearChart() which respects visibility state
        ///    - Or directly clears when no secondary data exists
        /// </summary>
        private void UpdateSecondaryDataRequiredButtonStates(int selectedSubtypeCount)
        {
            bool hasSecondaryData = selectedSubtypeCount >= 2;

            // If secondary data is no longer available, use the ViewModel state setters to trigger
            // the full rendering pipeline. This ensures all state updates, UI updates, and chart
            // clearing happen through the established pipeline with proper strategy adoption.
            if (!hasSecondaryData)
            {
                // Update state through ViewModel - this triggers the full pipeline:
                // ViewModel -> RequestChartUpdate -> OnChartUpdateRequested -> UpdateChartVisibility -> RenderChartsFromLastContext
                if (_viewModel.ChartState.IsNormalizedVisible)
                {
                    _viewModel.SetNormalizedVisible(false);
                }

                if (_viewModel.ChartState.IsDifferenceVisible)
                {
                    _viewModel.SetDifferenceVisible(false);
                }

                if (_viewModel.ChartState.IsRatioVisible)
                {
                    _viewModel.SetRatioVisible(false);
                }
            }

            // Update button enabled states (this is UI-only, not part of the rendering pipeline)
            ChartNormToggleButton.IsEnabled = hasSecondaryData;
            ChartDiffToggleButton.IsEnabled = hasSecondaryData;
            ChartRatioToggleButton.IsEnabled = hasSecondaryData;
        }

        private void OnFromDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
        }

        private void OnToDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
        }

        private void OnMetricTypesLoaded(object? sender, MetricTypesLoadedEventArgs e)
        {
            TablesCombo.Items.Clear();
            foreach (var type in e.MetricTypes)
                TablesCombo.Items.Add(type);

            if (TablesCombo.Items.Count > 0)
            {
                TablesCombo.SelectedIndex = 0;
                _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());
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

            SubtypeCombo.Items.Clear();
            SubtypeCombo.Items.Add("(All)");
            foreach (var st in subtypeListLocal)
                SubtypeCombo.Items.Add(st);

            SubtypeCombo.IsEnabled = subtypeListLocal.Any();
            SubtypeCombo.SelectedIndex = 0;

            subtypeList = subtypeListLocal;

            BuildDynamicSubtypeControls(subtypeListLocal);
            UpdateSelectedSubtypesInViewModel();
            _ = LoadDateRangeForSelectedMetrics();
        }

        private void BuildDynamicSubtypeControls(IEnumerable<string> subtypes)
        {
            _selectorManager.ClearDynamic();
            UpdateSelectedSubtypesInViewModel();
        }

        private void OnDateRangeLoaded(object? sender, DateRangeLoadedEventArgs e)
        {
            FromDate.SelectedDate = e.MinDate;
            ToDate.SelectedDate = e.MaxDate;
        }

        private async void OnDataLoaded(object? sender, DataLoadedEventArgs e)
        {
            var ctx = e.DataContext ?? _viewModel.ChartState.LastContext;
            if (ctx == null || ctx.Data1 == null || !ctx.Data1.Any())
                return;

            // Optional debug popup (existing behavior)
            var showDebugPopup = ConfigurationManager.AppSettings["DataVisualiser:ShowDebugPopup"];
            if (bool.TryParse(showDebugPopup, out bool showDebug) && showDebug)
            {
                var data1 = ctx.Data1.ToList();
                var data2 = ctx.Data2?.ToList() ?? new List<HealthMetricData>();

                var msg =
                    $"Data1 count: {data1.Count}\n" +
                    $"Data2 count: {data2.Count}\n" +
                    $"First 3 timestamps (Data1):\n" +
                    string.Join("\n", data1.Take(3).Select(d => d.NormalizedTimestamp)) +
                    "\n\nFirst 3 timestamps (Data2):\n" +
                    string.Join("\n", data2.Take(3).Select(d => d.NormalizedTimestamp));

                MessageBox.Show(msg, "DEBUG - LastContext contents");
            }

            await RenderChartsFromLastContext();
        }

        private void UpdateChartVisibility(Panel panel, System.Windows.Controls.Primitives.ButtonBase toggleButton, bool isVisible)
        {
            panel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            toggleButton.Content = isVisible ? "Hide" : "Show";
        }

        private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
        {
            UpdateChartVisibility(ChartNormPanel, ChartNormToggleButton, e.ShowNormalized);
            UpdateChartVisibility(ChartDiffPanel, ChartDiffToggleButton, e.ShowDifference);
            UpdateChartVisibility(ChartRatioPanel, ChartRatioToggleButton, e.ShowRatio);
            UpdateChartVisibility(ChartWeeklyPanel, ChartWeeklyToggleButton, e.ShowWeekly);
            UpdateChartVisibility(ChartWeekdayTrendPanel, ChartWeekdayTrendToggleButton, e.ShowWeeklyTrend);

            if (e.ShouldRenderCharts)
            {
                await RenderChartsFromLastContext();
            }
        }

        private ChartComputationResult? ComputeMainChart(
            IReadOnlyList<IEnumerable<HealthMetricData>> selectedMetricSeries,
            IReadOnlyList<string> selectedMetricLabels,
            string? unit,
            DateTime from,
            DateTime to)
        {
            IChartComputationStrategy strategy;

            if (selectedMetricSeries.Count > 2)
            {
                strategy = new MultiMetricStrategy(
                    selectedMetricSeries,
                    selectedMetricLabels,
                    from,
                    to,
                    unit);
            }
            else if (selectedMetricSeries.Count == 2)
            {
                strategy = new CombinedMetricStrategy(
                    selectedMetricSeries[0],
                    selectedMetricSeries[1],
                    selectedMetricLabels[0],
                    selectedMetricLabels[1],
                    from,
                    to);
            }
            else if (_viewModel.ChartState.LastContext?.SemanticMetricCount == 1)
            {
                strategy = CreateSingleMetricStrategy(
                    _viewModel.ChartState.LastContext!,
                    selectedMetricSeries[0],
                    selectedMetricLabels[0],
                    from,
                    to);
            }
            else
            {
                return null;
            }

            return strategy.Compute();
        }


        private async Task RenderMainChart(
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            string displayName1,
            string displayName2,
            DateTime from,
            DateTime to,
            string? metricType = null,
            string? primarySubtype = null,
            string? secondarySubtype = null)
        {
            // Build initial series list for multi-metric routing
            var (series, labels) = BuildInitialSeriesList(data1, data2, displayName1, displayName2);

            // Load additional subtypes if more than 2 are selected
            await LoadAdditionalSubtypesAsync(series, labels, metricType, from, to);

            var (strategy, secondaryLabel) = SelectComputationStrategy(series, labels, from, to);

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                ChartMain,
                strategy,
                labels[0],
                secondaryLabel,
                minHeight: 400,
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondaryLabel != null ? secondarySubtype : null,
                isOperationChart: false);
        }

        /// <summary>
        /// Builds the initial series list and labels from primary and secondary data.
        /// </summary>
        private (List<IEnumerable<HealthMetricData>> series, List<string> labels) BuildInitialSeriesList(
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            string displayName1,
            string displayName2)
        {
            var series = new List<IEnumerable<HealthMetricData>> { data1 };
            var labels = new List<string> { displayName1 };

            if (data2 != null && data2.Any())
            {
                series.Add(data2);
                labels.Add(displayName2);
            }

            return (series, labels);
        }

        /// <summary>
        /// Loads additional subtype data (subtypes 3, 4, etc.) and adds them to the series and labels lists.
        /// </summary>
        private async Task LoadAdditionalSubtypesAsync(
            List<IEnumerable<HealthMetricData>> series,
            List<string> labels,
            string? metricType,
            DateTime from,
            DateTime to)
        {
            var selectedSubtypes = _viewModel.MetricState.SelectedSubtypes;
            if (selectedSubtypes.Count <= 2 || string.IsNullOrEmpty(metricType))
                return;

            var dataFetcher = new DataFetcher(_connectionString);
            var tableName = _viewModel.MetricState.ResolutionTableName ?? "HealthMetrics";

            // Load data for subtypes 3, 4, etc.
            for (int i = 2; i < selectedSubtypes.Count; i++)
            {
                var subtype = selectedSubtypes[i];
                if (string.IsNullOrWhiteSpace(subtype))
                    continue;

                try
                {
                    var additionalData = await dataFetcher.GetHealthMetricsDataByBaseType(
                        metricType,
                        subtype,
                        from,
                        to,
                        tableName);

                    if (additionalData != null && additionalData.Any())
                    {
                        series.Add(additionalData);
                        labels.Add($"{metricType}:{subtype}");
                    }
                }
                catch
                {
                    // Skip if loading fails
                }
            }
        }

        /// <summary>
        /// Selects the appropriate computation strategy based on the number of series.
        /// Returns the strategy and secondary label (if applicable).
        /// </summary>
        private (IChartComputationStrategy strategy, string? secondaryLabel)
            SelectComputationStrategy(
                List<IEnumerable<HealthMetricData>> series,
                List<string> labels,
                DateTime from,
                DateTime to)
        {
            string? secondaryLabel = null;
            IChartComputationStrategy strategy;

            var ctx = _viewModel.ChartState.LastContext!;
            // Use actual series count instead of SemanticMetricCount which is hardcoded to max 2
            var actualSeriesCount = series.Count;

            System.Diagnostics.Debug.WriteLine(
                $"[STRATEGY] ActualSeriesCount={actualSeriesCount}, SemanticMetricCount={ctx.SemanticMetricCount}, " +
                $"PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, " +
                $"SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}"
            );

            // ---------- MULTI METRIC ----------
            if (actualSeriesCount > 2)
            {
                strategy = new MultiMetricStrategy(
                    series,
                    labels,
                    from,
                    to,
                    unit: null);
            }
            // ---------- COMBINED METRIC ----------
            else if (actualSeriesCount == 2)
            {
                secondaryLabel = labels[1];

                var leftCms = ctx.PrimaryCms as DataFileReader.Canonical.ICanonicalMetricSeries;
                var rightCms = ctx.SecondaryCms as DataFileReader.Canonical.ICanonicalMetricSeries;

                // 🔒 HARD GATE — parity OFF by default
                const bool ENABLE_COMBINED_METRIC_PARITY = false;

                var parityService = new ParityValidationService();
                strategy = parityService.ExecuteCombinedMetricParityIfEnabled(
                    leftCms,
                    rightCms,
                    series[0],
                    series[1],
                    labels[0],
                    labels[1],
                    from,
                    to,
                    ENABLE_COMBINED_METRIC_PARITY);
            }
            // ---------- SINGLE METRIC ----------
            else
            {
                strategy = CreateSingleMetricStrategy(
                    ctx,
                    series[0],
                    labels[0],
                    from,
                    to);
            }

            System.Diagnostics.Debug.WriteLine(
                $"[StrategySelection] actualSeriesCount={actualSeriesCount}, " +
                $"series.Count={series.Count}, " +
                $"strategy={strategy.GetType().Name}");

            return (strategy, secondaryLabel);
        }







        private async Task RenderOrClearChart(
            CartesianChart chart,
            bool isVisible,
            IChartComputationStrategy? strategy,
            string title,
            double minHeight = 400,
            string? metricType = null,
            string? primarySubtype = null,
            string? secondarySubtype = null,
            string? operationType = null,
            bool isOperationChart = false)
        {
            if (isVisible && strategy != null)
            {
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                    chart,
                    strategy,
                    title,
                    minHeight: minHeight,
                    metricType: metricType,
                    primarySubtype: primarySubtype,
                    secondarySubtype: secondarySubtype,
                    operationType: operationType,
                    isOperationChart: isOperationChart);
            }
            else
            {
                ChartHelper.ClearChart(chart, _viewModel.ChartState.ChartTimestamps);
            }
        }

        private void RenderWeekdayTrendChart(WeekdayTrendResult result)
        {
            var weekdayStrokes = new[]
            {
                System.Windows.Media.Brushes.SteelBlue,
                System.Windows.Media.Brushes.CadetBlue,
                System.Windows.Media.Brushes.SeaGreen,
                System.Windows.Media.Brushes.OliveDrab,
                System.Windows.Media.Brushes.Goldenrod,
                System.Windows.Media.Brushes.OrangeRed,
                System.Windows.Media.Brushes.IndianRed
            };

            ChartWeekdayTrend.Series.Clear();
            ChartWeekdayTrend.AxisX.Clear();
            ChartWeekdayTrend.AxisY.Clear();

            if (result == null || result.SeriesByDay.Count == 0)
                return;

            ChartWeekdayTrend.AxisX.Add(new Axis
            {
                Title = "Time",
                MinValue = result.From.Ticks,
                MaxValue = result.To.Ticks,
                LabelFormatter = v => new DateTime((long)v).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            });

            ChartWeekdayTrend.AxisY.Add(new Axis
            {
                Title = result.Unit ?? "Value",
                MinValue = result.GlobalMin,
                MaxValue = result.GlobalMax
            });

            for (int dayIndex = 0; dayIndex <= 6; dayIndex++)
            {
                if (!result.SeriesByDay.TryGetValue(dayIndex, out var series))
                    continue;

                bool isEnabled = dayIndex switch
                {
                    0 => _viewModel.ChartState.ShowMonday,
                    1 => _viewModel.ChartState.ShowTuesday,
                    2 => _viewModel.ChartState.ShowWednesday,
                    3 => _viewModel.ChartState.ShowThursday,
                    4 => _viewModel.ChartState.ShowFriday,
                    5 => _viewModel.ChartState.ShowSaturday,
                    6 => _viewModel.ChartState.ShowSunday,
                    _ => false
                };

                if (!isEnabled)
                    continue;

                var values = new ChartValues<ObservablePoint>();
                foreach (var point in series.Points)
                {
                    values.Add(new ObservablePoint(point.Date.Ticks, point.Value));
                }

                ChartWeekdayTrend.Series.Add(new LineSeries
                {
                    Title = series.Day.ToString(),
                    Values = values,
                    PointGeometry = null,
                    LineSmoothness = 0.3,
                    Fill = System.Windows.Media.Brushes.Transparent,
                    StrokeThickness = 2,
                    Stroke = weekdayStrokes[dayIndex]
                });
            }
        }

        private async Task RenderChartsFromLastContext()
        {
            var ctx = _viewModel.ChartState.LastContext;
            if (!ShouldRenderCharts(ctx))
                return;

            var hasSecondaryData = HasSecondaryData(ctx);

            await RenderPrimaryChart(ctx);

            // Render charts based on individual visibility states
            var metricType = ctx.MetricType;
            var primarySubtype = ctx.PrimarySubtype;
            var secondarySubtype = ctx.SecondarySubtype;

            // Charts that require secondary data - only render if secondary data exists
            if (hasSecondaryData)
            {
                await RenderNormalized(ctx, metricType, primarySubtype, secondarySubtype);
                await RenderDifference(ctx, metricType, primarySubtype, secondarySubtype);
                await RenderRatio(ctx, metricType, primarySubtype, secondarySubtype);
            }
            else
            {
                // Clear charts that require secondary data when no secondary data exists
                ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
                ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
                ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            }

            // Charts that don't require secondary data - always render based on visibility
            await RenderWeeklyDistribution(ctx);
            RenderWeeklyTrend(ctx);
        }

        /// <summary>
        /// Validates that the chart context is valid and has data to render.
        /// </summary>
        private static bool ShouldRenderCharts(ChartDataContext? ctx)
        {
            return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
        }

        /// <summary>
        /// Determines if secondary data is available for rendering secondary charts.
        /// </summary>
        private static bool HasSecondaryData(ChartDataContext ctx)
        {
            return ctx.Data2 != null && ctx.Data2.Any();
        }

        /// <summary>
        /// Renders the primary (main) chart from the context.
        /// </summary>
        private async Task RenderPrimaryChart(ChartDataContext ctx)
        {
            await RenderMainChart(
                ctx.Data1!,
                ctx.Data2,
                ctx.DisplayName1 ?? string.Empty,
                ctx.DisplayName2 ?? string.Empty,
                ctx.From,
                ctx.To,
                metricType: ctx.MetricType,
                primarySubtype: ctx.PrimarySubtype,
                secondarySubtype: ctx.SecondarySubtype);
        }

        /// <summary>
        /// Renders all secondary charts (normalized, weekly distribution, weekday trend, difference, ratio).
        /// </summary>
        private async Task RenderSecondaryCharts(ChartDataContext ctx)
        {
            var metricType = ctx.MetricType;
            var primarySubtype = ctx.PrimarySubtype;
            var secondarySubtype = ctx.SecondarySubtype;

            await RenderNormalized(ctx, metricType, primarySubtype, secondarySubtype);
            await RenderWeeklyDistribution(ctx);
            RenderWeeklyTrend(ctx);
            await RenderDifference(ctx, metricType, primarySubtype, secondarySubtype);
            await RenderRatio(ctx, metricType, primarySubtype, secondarySubtype);
        }

        private void ClearSecondaryChartsAndReturn()
        {
            ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            // NOTE: WeekdayTrend intentionally not cleared here to preserve current behavior (tied to secondary presence).
        }

        private Task RenderNormalized(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
        {
            return RenderOrClearChart(
                ChartNorm,
                _viewModel.ChartState.IsNormalizedVisible,
                new NormalizedStrategy(ctx.Data1!, ctx.Data2!, ctx.DisplayName1, ctx.DisplayName2, ctx.From, ctx.To, _viewModel.ChartState.SelectedNormalizationMode),
                $"{ctx.DisplayName1} ~ {ctx.DisplayName2}",
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "~",
                isOperationChart: true);
        }

        private async Task RenderWeeklyDistribution(ChartDataContext ctx)
        {
            if (_viewModel.ChartState.IsWeeklyVisible)
            {
                await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(
                    ChartWeekly,
                    ctx.Data1!,
                    ctx.DisplayName1,
                    ctx.From,
                    ctx.To,
                    minHeight: 400,
                    useFrequencyShading: _viewModel.ChartState.UseFrequencyShading,
                    intervalCount: _viewModel.ChartState.WeeklyIntervalCount);
            }
            else
            {
                ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            }
        }

        private void RenderWeeklyTrend(ChartDataContext ctx)
        {
            if (_viewModel.ChartState.IsWeeklyTrendVisible)
            {
                var result = new WeekdayTrendStrategy().Compute(ctx.Data1!, ctx.From, ctx.To);
                RenderWeekdayTrendChart(result);
            }
            else
            {
                ChartHelper.ClearChart(ChartWeekdayTrend, _viewModel.ChartState.ChartTimestamps);
            }
        }

        private Task RenderDifference(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
        {
            return RenderOrClearChart(
                ChartDiff,
                _viewModel.ChartState.IsDifferenceVisible,
                new DifferenceStrategy(ctx.Data1!, ctx.Data2!, ctx.DisplayName1, ctx.DisplayName2, ctx.From, ctx.To),
                $"{ctx.DisplayName1} - {ctx.DisplayName2}",
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "-",
                isOperationChart: true);
        }

        private Task RenderRatio(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
        {
            return RenderOrClearChart(
                ChartRatio,
                _viewModel.ChartState.IsRatioVisible,
                new RatioStrategy(ctx.Data1!, ctx.Data2!, ctx.DisplayName1, ctx.DisplayName2, ctx.From, ctx.To),
                $"{ctx.DisplayName1} / {ctx.DisplayName2}",
                metricType: metricType,
                primarySubtype: primarySubtype,
                secondarySubtype: secondarySubtype,
                operationType: "/",
                isOperationChart: true);
        }

        private Panel? GetChartPanel(string chartName) =>
            chartName switch
            {
                "Norm" => ChartNormPanel,
                "Diff" => ChartDiffPanel,
                "Ratio" => ChartRatioPanel,
                "Weekly" => ChartWeeklyPanel,
                "WeeklyTrend" => ChartWeekdayTrendPanel,
                _ => null
            };

        //private static IChartComputationStrategy CreateSingleMetricStrategy(ChartDataContext ctx, IEnumerable<HealthMetricData> data, string label, DateTime from, DateTime to)
        //{
        //    //if (ctx.PrimaryCms != null)
        //    //{
        //        return new SingleMetricCmsStrategy(
        //            (DataFileReader.Canonical.ICanonicalMetricSeries)ctx.PrimaryCms,
        //            label,
        //            from,
        //            to);
        //    //}

        //    return new SingleMetricLegacyStrategy(
        //        data,
        //        label,
        //        from,
        //        to);
        //}

        private static IChartComputationStrategy CreateSingleMetricStrategy(
    ChartDataContext ctx,
    IEnumerable<HealthMetricData> data,
    string label,
    DateTime from,
    DateTime to)
        {
            if (ctx.PrimaryCms is DataFileReader.Canonical.ICanonicalMetricSeries cms)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[STRATEGY] Using SingleMetricCmsStrategy");

                return new SingleMetricCmsStrategy(
                    cms,
                    label,
                    from,
                    to);
            }

            System.Diagnostics.Debug.WriteLine(
                "[STRATEGY] Using SingleMetricLegacyStrategy");

            return new SingleMetricLegacyStrategy(
                data,
                label,
                from,
                to);
        }



        private void OnWeeklyTrendMondayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendMondayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendTuesdayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendTuesdayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendWednesdayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendWednesdayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendThursdayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendThursdayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendFridayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendFridayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendSaturdayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendSaturdayVisible(((CheckBox)sender).IsChecked == true);

        private void OnWeeklyTrendSundayToggled(object sender, RoutedEventArgs e) =>
            _viewModel.SetWeeklyTrendSundayVisible(((CheckBox)sender).IsChecked == true);

        private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
        {
            var panel = GetChartPanel(e.ChartName);
            if (panel != null)
            {
                panel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void OnErrorOccured(object? sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            ClearAllCharts();
        }

        private void ClearAllCharts()
        {
            ChartHelper.ClearChart(ChartMain, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartWeekdayTrend, _viewModel.ChartState.ChartTimestamps);
            _viewModel.ChartState.LastContext = null;
        }
    }
}
