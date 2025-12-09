using DataVisualiser.Charts;
using DataVisualiser.Charts.Computation;
using DataVisualiser.Charts.Helpers;
using ChartHelper = DataVisualiser.Charts.Helpers.ChartHelper;
using DataVisualiser.Charts.Rendering;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Data.Repositories;
using DataVisualiser.Models;
using DataVisualiser.Helper;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.UI.SubtypeSelectors;
using DataVisualiser.ViewModels;
using DataVisualiser.ViewModels.Events;
using LiveCharts.Wpf;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace DataVisualiser
{
    public partial class MainWindow : Window
    {
        private readonly ChartComputationEngine _chartComputationEngine;
        private readonly ChartRenderEngine _chartRenderEngine;
        private ChartTooltipManager? _tooltipManager;

        private readonly string _connectionString;
        List<string>? subtypeList;

        private MetricSelectionService _metricSelectionService;
        private ChartUpdateCoordinator _chartUpdateCoordinator;
        private WeeklyDistributionService _weeklyDistributionService;
        private SubtypeSelectorManager _selectorManager;

        private readonly ChartState _chartState = new();
        private readonly MetricState _metricState = new();
        private readonly UiState _uiState = new();
        private readonly MainWindowViewModel _viewModel;
        private bool _isInitializing = true;


        public MainWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
            _metricSelectionService = new MetricSelectionService(_connectionString);

            _chartComputationEngine = new ChartComputationEngine();
            _chartRenderEngine = new ChartRenderEngine();

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


            _chartUpdateCoordinator = new ChartUpdateCoordinator(_chartComputationEngine, _chartRenderEngine, _tooltipManager, _chartState.ChartTimestamps);
            _chartUpdateCoordinator.SeriesMode = ChartSeriesMode.RawAndSmoothed;
            _weeklyDistributionService = new WeeklyDistributionService(_chartState.ChartTimestamps);

            _viewModel = new MainWindowViewModel(_chartState, _metricState, _uiState, _metricSelectionService, _chartUpdateCoordinator, _weeklyDistributionService);
            DataContext = _viewModel;
            _viewModel.ChartVisibilityChanged += OnChartVisibilityChanged;
            _viewModel.ErrorOccured += OnErrorOccured;
            _viewModel.MetricTypesLoaded += OnMetricTypesLoaded;
            _viewModel.SubtypesLoaded += OnSubtypesLoaded;
            _viewModel.DateRangeLoaded += OnDateRangeLoaded; // NEW
            _viewModel.DataLoaded += OnDataLoaded;
            _viewModel.ChartUpdateRequested += OnChartUpdateRequested;


            // Initialize date range through viewModel
            var initialFromDate = DateTime.UtcNow.AddDays(-30);
            var initialToDate = DateTime.UtcNow;
            _viewModel.SetDateRange(initialFromDate, initialToDate);
            FromDate.SelectedDate = _viewModel.MetricState.FromDate;
            ToDate.SelectedDate = _viewModel.MetricState.ToDate;


            _viewModel.SetLoadingMetricTypes(false);
            _viewModel.SetLoadingSubtypes(false);
            _viewModel.SetNormalizedVisible(false);
            _viewModel.SetDifferenceVisible(false);
            _viewModel.SetRatioVisible(false);
            _viewModel.SetWeeklyVisible(false);
            _viewModel.CompleteInitialization();

            _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
            _viewModel.ChartState.LastContext = new ChartDataContext();
            _selectorManager = new SubtypeSelectorManager(MetricSubtypePanel, SubtypeCombo);

            _selectorManager.SubtypeSelectionChanged += (s, e) =>
            {
                UpdateChartTitlesFromCombos();
                OnAnySubtypeSelectionChanged(s ?? this, null);
            };

            ResolutionCombo.Items.Add("All");
            ResolutionCombo.Items.Add("Hourly");
            ResolutionCombo.Items.Add("Daily");
            ResolutionCombo.Items.Add("Weekly");
            ResolutionCombo.Items.Add("Monthly");
            ResolutionCombo.Items.Add("Yearly");
            ResolutionCombo.SelectedItem = "All";

            _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
            _viewModel.LoadMetricsCommand.Execute(null);

            ChartHelper.InitializeChartBehavior(ChartMain);
            ChartHelper.InitializeChartBehavior(ChartWeekly); //NEW
            ChartHelper.InitializeChartBehavior(ChartNorm);
            ChartHelper.InitializeChartBehavior(ChartDiff);
            ChartHelper.InitializeChartBehavior(ChartRatio);

            UpdateChartTitlesFromCombos();
            _viewModel.RequestChartUpdate();


            // Handle window closing to dispose resources
            this.Closing += MainWindow_Closing;

            // Mark initialization as complete - event handlers can now safely use _viewModel
            _isInitializing = false;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Dispose tooltip manager to prevent memory leaks
            _tooltipManager?.Dispose();
            _tooltipManager = null;
        }

        #region Data Loading and Selection Event Handlers

        /// <summary>
        /// Event handler for Resolution selection change - reloads MetricTypes from the selected table.
        /// </summary>
        private void OnResolutionSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ResolutionCombo.SelectedItem == null)
                return;

            TablesCombo.Items.Clear();

            _selectorManager.ClearDynamic();
            SubtypeCombo.Items.Clear();
            SubtypeCombo.IsEnabled = false;

            _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
            _viewModel.LoadMetricsCommand.Execute(null);
        }

        /// <summary>
        /// Event handler for MetricType selection change - loads subtypes and updates date range.
        /// </summary>
        private void OnMetricTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (_viewModel.UiState.IsLoadingMetricTypes)
                return;

            // Push selection into VM
            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());

            // Ask the VM to load subtypes; when finished, it will raise SubtypesLoaded
            _viewModel.LoadSubtypesCommand.Execute(null);

            // Titles should respond immediately to combo state
            UpdateChartTitlesFromCombos();
        }


        /// <summary>
        /// Generalized event handler for any MetricSubtype ComboBox selection change - updates date range to match data availability.
        /// This handler is used by all subtype ComboBoxes (both static and dynamically added).
        /// </summary>
        private async void OnAnySubtypeSelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs? e)
        {
            // Push all selected subtypes (primary + dynamic) into the VM state
            UpdateSelectedSubtypesInViewModel();

            // Ask the VM to recompute the date range for the current metric + subtype selection
            await LoadDateRangeForSelectedMetrics();
        }


        private async Task LoadDateRangeForSelectedMetrics()
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (_viewModel.UiState.IsLoadingSubtypes || _viewModel.UiState.IsLoadingMetricTypes)
                return;

            // Delegate date-range computation to the ViewModel (Pattern A)
            await _viewModel.RefreshDateRangeForCurrentSelectionAsync();

            // Titles / labels in the UI stay in sync
            UpdateChartTitlesFromCombos();
        }

        /// <summary>
        /// Loads data from HealthMetrics table based on selected MetricType, MetricSubtype(s), and date range.
        /// Updates charts using the selected metric type and subtypes.
        /// </summary>
        /// <summary>
        /// UI entry point for loading data.
        /// Pushes current UI selections into the ViewModel and delegates
        /// validation + data fetching to the VM. Rendering is driven by
        /// DataLoaded / ChartUpdateRequested events.
        /// </summary>
        private async void OnLoadData(object sender, RoutedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            // Ensure a metric type is selected in the UI
            if (TablesCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a Metric Type", "No Selection",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1) Push current UI selections into the VM
            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());
            UpdateSelectedSubtypesInViewModel();

            var selectedSubtype = _selectorManager.GetPrimarySubtype();
            var selectedSubtype2 = _selectorManager.GetSecondarySubtype();

            string baseType = _viewModel.MetricState.SelectedMetricType!;
            string display1 = !string.IsNullOrEmpty(selectedSubtype) && selectedSubtype != "(All)"
                ? selectedSubtype
                : baseType;
            string display2 = !string.IsNullOrEmpty(selectedSubtype2)
                ? selectedSubtype2
                : string.Empty;

            SetChartTitles(display1, display2);
            UpdateChartLabels(selectedSubtype ?? string.Empty, selectedSubtype2 ?? string.Empty);

            var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;
            _viewModel.SetDateRange(fromDate, toDate);

            // 2) Let the ViewModel validate the request
            var (isValid, errorMessage) = _viewModel.ValidateDataLoadRequirements();
            if (!isValid)
            {
                MessageBox.Show(errorMessage ?? "The current selection is not valid.",
                    "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 3) Ask the VM to load data and build ChartState.LastContext
                var loaded = await _viewModel.LoadMetricDataAsync();

                if (!loaded)
                {
                    // VM already raised ErrorOccured – treat this as "no data"
                    ClearAllCharts();
                    return;
                }

                // 4) Let the VM raise DataLoaded + ChartUpdateRequested
                _viewModel.LoadDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        /// <summary>
        /// Toggles ChartNorm visibility and reloads data if available.
        /// </summary>
        private void OnChartNormToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleNorm();
            _viewModel.RequestChartUpdate();
        }

        /// <summary>
        /// Toggles Weekly Distribution chart visibility and reloads data if available.
        /// </summary>
        private void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleWeekly();
            _viewModel.RequestChartUpdate();
        }

        /// <summary>
        /// Toggles ChartDiff visibility and reloads data if available.
        /// </summary>
        private void OnChartDiffToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleDiff();
            _viewModel.RequestChartUpdate();
        }

        /// <summary>
        /// Toggles ChartRatio visibility and reloads data if available.
        /// </summary>
        private void OnChartRatioToggle(object sender, RoutedEventArgs e)
        {
            _viewModel.ToggleRatio();
            _viewModel.RequestChartUpdate();
        }

        #endregion

        #region Chart Configuration and Helper Methods

        /// <summary>
        /// Resets zoom and pan to show all data for all charts
        /// </summary>
        private void OnResetZoom(object sender, RoutedEventArgs e)
        {
            ChartHelper.ResetZoom(ref ChartMain);
            ChartHelper.ResetZoom(ref ChartNorm);
            ChartHelper.ResetZoom(ref ChartDiff);
            ChartHelper.ResetZoom(ref ChartRatio);
            ChartHelper.ResetZoom(ref ChartWeekly);
        }

        /// <summary>
        /// Sets the three chart title TextBlocks based on provided display names.
        /// </summary>
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

        /// <summary>
        /// Updates the chart labels in the tooltip manager based on the selected metric subtypes.
        /// Format: (metric subtype 1) + operator + (metric subtype 2)
        /// </summary>
        private void UpdateChartLabels(string subtype1, string subtype2)
        {
            if (_tooltipManager == null) return;

            subtype1 ??= string.Empty;
            subtype2 ??= string.Empty;

            // Get base type as fallback if subtypes are empty
            string baseType = TablesCombo.SelectedItem?.ToString() ?? "";

            // Use subtype1 if available and not "(All)", otherwise use base type
            string label1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)"
                ? subtype1
                : baseType;

            // Use subtype2 if available, otherwise use empty string (don't fall back to base type)
            string label2 = !string.IsNullOrEmpty(subtype2)
                ? subtype2
                : string.Empty;

            // ChartMain: subtype1 vs subtype2 (or just subtype1 if subtype2 is empty)
            string chartMainLabel = !string.IsNullOrEmpty(label2)
                ? $"{label1} vs {label2}"
                : label1;
            _tooltipManager.UpdateChartLabel(ChartMain, chartMainLabel);

            // ChartDiff: subtype1 - subtype2
            string chartDiffLabel = !string.IsNullOrEmpty(label2)
                ? $"{label1} - {label2}"
                : label1;
            _tooltipManager.UpdateChartLabel(ChartDiff, chartDiffLabel);

            // ChartRatio: subtype1 / subtype2
            string chartRatioLabel = !string.IsNullOrEmpty(label2)
                ? $"{label1} / {label2}"
                : label1;
            _tooltipManager.UpdateChartLabel(ChartRatio, chartRatioLabel);
        }

        /// <summary>
        /// Reads selection state of the combo boxes and updates titles accordingly.
        /// Called when selections change so titles stay in sync even before loading data.
        /// </summary>
        private void UpdateChartTitlesFromCombos()
        {
            string subtype1 = _selectorManager.GetPrimarySubtype() ?? "";
            string subtype2 = _selectorManager.GetSecondarySubtype() ?? "";

            string baseType = TablesCombo.SelectedItem?.ToString() ?? "";

            string display1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)"
                ? subtype1
                : baseType;

            string display2 = !string.IsNullOrEmpty(subtype2)
                ? subtype2
                : "";

            SetChartTitles(display1, display2);
            UpdateChartLabels(subtype1, subtype2);

        }

        #endregion

        #region Normalization mode UI handling

        /// <summary>
        /// Handler for the normalization-mode radio buttons (wired in XAML).
        /// Refreshes the normalized chart if visible and data exists.
        /// </summary>
        private async void OnNormalizationModeChanged(object sender, RoutedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
            {
                return;
            }

            try
            {
                if (NormZeroToOneRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
                else if (NormPercentOfMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
                else if (NormRelativeToMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);

                if (_viewModel.ChartState.IsNormalizedVisible && _viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
                {
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_viewModel.ChartState.LastContext.Data1, _viewModel.ChartState.LastContext.Data2, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.DisplayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, _viewModel.ChartState.SelectedNormalizationMode), $"{_viewModel.ChartState.LastContext.DisplayName1} ~ {_viewModel.ChartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            catch (Exception ex)
            {
                // Error handled silently - normalization mode change failed
            }
        }

        #endregion

        /// <summary>
        /// Reads all selected subtypes (primary + dynamic) and pushes them into the ViewModel.
        /// </summary>
        private void UpdateSelectedSubtypesInViewModel()
        {
            if (_viewModel == null) return;

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
                if (selectedSubtypes.Contains(st)) continue; // avoid duplicates

                selectedSubtypes.Add(st);
            }

            _viewModel.SetSelectedSubtypes(selectedSubtypes);
        }


        /// <summary>
        /// Keeps ViewModel date range in sync when the From date changes.
        /// </summary>
        private void OnFromDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializing || _viewModel == null) return;

            _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
        }

        /// <summary>
        /// Keeps ViewModel date range in sync when the To date changes.
        /// </summary>
        private void OnToDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isInitializing || _viewModel == null) return;

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
        }


        private void OnSubtypesLoaded(object? sender, SubtypesLoadedEventArgs e)
        {
            // Materialize list once
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
            if (_viewModel == null)
                return;

            var ctx = e.DataContext ?? _viewModel.ChartState.LastContext;
            if (ctx == null || ctx.Data1 == null || !ctx.Data1.Any())
                return;

            var data1 = ctx.Data1.ToList();
            var data2 = ctx.Data2?.ToList() ?? new List<HealthMetricData>();

            // TEMP: debug to see what we actually have
            var msg =
                $"Data1 count: {data1.Count}\n" +
                $"Data2 count: {data2.Count}\n" +
                $"First 3 timestamps (Data1):\n" +
                string.Join("\n", data1.Take(3).Select(d => d.NormalizedTimestamp)) +
                "\n\nFirst 3 timestamps (Data2):\n" +
                string.Join("\n", data2.Take(3).Select(d => d.NormalizedTimestamp));

            MessageBox.Show(msg, "DEBUG – LastContext contents");

            await RenderChartsFromLastContext();
        }

        /// <summary>
        /// Updates chart panel visibility and toggle button content.
        /// </summary>
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

            if (e.ShouldRenderCharts)
            {
                await RenderChartsFromLastContext();
            }
        }

        /// <summary>
        /// Renders the main chart based on available data (single or combined).
        /// </summary>
        private async Task RenderMainChart(
            IEnumerable<HealthMetricData> data1,
            IEnumerable<HealthMetricData>? data2,
            string displayName1,
            string displayName2,
            DateTime from,
            DateTime to)
        {
            if (data2 != null && data2.Any())
            {
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                    ChartMain,
                    new Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, from, to),
                    displayName1,
                    displayName2);
            }
            else
            {
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                    ChartMain,
                    new Charts.Strategies.SingleMetricStrategy(data1, displayName1, from, to),
                    displayName1,
                    minHeight: 400);
            }
        }

        /// <summary>
        /// Renders or clears a chart based on visibility state.
        /// </summary>
        private async Task RenderOrClearChart(
            CartesianChart chart,
            bool isVisible,
            Charts.IChartComputationStrategy? strategy,
            string title,
            double minHeight = 400)
        {
            if (isVisible && strategy != null)
            {
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chart, strategy, title, minHeight: minHeight);
            }
            else
            {
                ChartHelper.ClearChart(chart, _viewModel.ChartState.ChartTimestamps);
            }
        }

        /// <summary>
        /// Renders all charts based on the current context and visibility settings.
        /// </summary>
        private async Task RenderChartsFromLastContext()
        {
            var ctx = _viewModel.ChartState.LastContext;
            if (ctx == null || ctx.Data1 == null || !ctx.Data1.Any())
                return;

            var data1 = ctx.Data1;
            var data2 = ctx.Data2;
            var displayName1 = ctx.DisplayName1 ?? string.Empty;
            var displayName2 = ctx.DisplayName2 ?? string.Empty;
            var hasSecondaryData = data2 != null && data2.Any();

            // Render main chart
            await RenderMainChart(data1, data2, displayName1, displayName2, ctx.From, ctx.To);

            // If no secondary data, clear secondary charts and return
            if (!hasSecondaryData)
            {
                ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
                ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
                ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
                ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
                return;
            }

            // Render or clear charts based on visibility
            await RenderOrClearChart(
                ChartNorm,
                _viewModel.ChartState.IsNormalizedVisible,
                new Charts.Strategies.NormalizedStrategy(data1, data2!, displayName1, displayName2, ctx.From, ctx.To, _viewModel.ChartState.SelectedNormalizationMode),
                $"{displayName1} ~ {displayName2}");

            if (_viewModel.ChartState.IsWeeklyVisible)
            {
                await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(
                    ChartWeekly, data1, displayName1, ctx.From, ctx.To, minHeight: 400);
            }
            else
            {
                ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            }

            await RenderOrClearChart(
                ChartDiff,
                _viewModel.ChartState.IsDifferenceVisible,
                new Charts.Strategies.DifferenceStrategy(data1, data2!, displayName1, displayName2, ctx.From, ctx.To),
                $"{displayName1} - {displayName2}");

            await RenderOrClearChart(
                ChartRatio,
                _viewModel.ChartState.IsRatioVisible,
                new Charts.Strategies.RatioStrategy(data1, data2!, displayName1, displayName2, ctx.From, ctx.To),
                $"{displayName1} / {displayName2}");
        }


        /// <summary>
        /// Maps chart names to their corresponding panels.
        /// </summary>
        private Panel? GetChartPanel(string chartName)
        {
            return chartName switch
            {
                "Norm" => ChartNormPanel,
                "Diff" => ChartDiffPanel,
                "Ratio" => ChartRatioPanel,
                "Weekly" => ChartWeeklyPanel,
                _ => null
            };
        }

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
            MessageBox.Show(e.Message, "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);

            // Any VM-level error tied to data / charts should leave the UI clean.
            ClearAllCharts();
        }


        /// <summary>
        /// Clears all charts and resets the chart context.
        /// </summary>
        private void ClearAllCharts()
        {
            ChartHelper.ClearChart(ChartMain, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            _viewModel.ChartState.LastContext = null;
        }

    }
}
