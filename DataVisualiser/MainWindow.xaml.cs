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
            _chartRenderEngine = new ChartRenderEngine(normalizeYAxisDelegate: (axis, rawData, smoothed) => ChartHelper.NormalizeYAxis(axis, rawData, smoothed), adjustHeightDelegate: (chart, minHeight) => ChartHelper.AdjustChartHeightBasedOnYAxis(chart, minHeight));

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
            _weeklyDistributionService = new WeeklyDistributionService(_chartState.ChartTimestamps);

            _viewModel = new MainWindowViewModel(_chartState, _metricState, _uiState, _metricSelectionService, _chartUpdateCoordinator, _weeklyDistributionService);
            DataContext = _viewModel;
            _viewModel.ChartVisibilityChanged += OnChartVisibilityChanged;
            _viewModel.ErrorOccured += OnErrorOccured;
            _viewModel.MetricTypesLoaded += OnMetricTypesLoaded;
            _viewModel.SubtypesLoaded += OnSubtypesLoaded;
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
        private async void OnMetricTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (_viewModel.UiState.IsLoadingMetricTypes)
                return;

            await LoadSubtypesForSelectedMetricType();
            await LoadDateRangeForSelectedMetric();

            UpdateChartTitlesFromCombos();
        }

        /// <summary>
        /// Generalized event handler for any MetricSubtype ComboBox selection change - updates date range to match data availability.
        /// This handler is used by all subtype ComboBoxes (both static and dynamically added).
        /// </summary>
        private async void OnAnySubtypeSelectionChanged(object? sender, System.Windows.Controls.SelectionChangedEventArgs? e)
        {
            UpdateSelectedSubtypesInViewModel();

            await LoadDateRangeForSelectedMetrics();
        }

        private async Task LoadDateRangeForSelectedMetrics()
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (_viewModel.UiState.IsLoadingSubtypes || _viewModel.UiState.IsLoadingMetricTypes)
                return;

            await LoadDateRangeForSelectedMetric();

            UpdateChartTitlesFromCombos();
        }

        /// <summary>
        /// Loads subtypes for the currently selected metric type into the subtype combo boxes.
        /// </summary>
        private async Task LoadSubtypesForSelectedMetricType()
        {
            if (TablesCombo.SelectedItem == null)
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                _selectorManager.ClearDynamic();
                return;
            }

            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());
            if (string.IsNullOrEmpty(_viewModel.MetricState.SelectedMetricType))
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                _selectorManager.ClearDynamic();
                return;
            }

            var active = _selectorManager.GetActiveCombos();

            try
            {
                //_uiState.IsLoadingSubtypes = true;
                _viewModel.SetLoadingSubtypes(true);
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                var subtypes = await _metricSelectionService.LoadSubtypesAsync(_viewModel.MetricState.SelectedMetricType, tableName);

                SubtypeCombo.Items.Clear();
                _selectorManager.ClearDynamic();
                subtypeList = subtypes.ToList();

                if (subtypeList.Count > 0)
                {
                    SubtypeCombo.Items.Add("(All)");

                    foreach (var subtype in subtypeList)
                    {
                        SubtypeCombo.Items.Add(subtype);
                    }

                    SubtypeCombo.IsEnabled = true;
                    SubtypeCombo.SelectedIndex = 0;
                    UpdateSelectedSubtypesInViewModel();
                }
                else
                {
                    SubtypeCombo.IsEnabled = false;

                    foreach (var combo in active)
                    {
                        combo.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;

                foreach (var combo in active)
                {
                    combo.IsEnabled = false;
                    combo.Items.Clear();
                }
            }
            finally
            {
                _viewModel.SetLoadingSubtypes(false);

            }
        }

        /// <summary>
        /// Loads and sets the date range for the currently selected metric type and subtype
        /// </summary>
        private async Task LoadDateRangeForSelectedMetric()
        {
            if (TablesCombo.SelectedItem == null)
            {
                return;
            }

            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());

            if (string.IsNullOrEmpty(_viewModel.MetricState.SelectedMetricType))
            {
                return;
            }

            string? selectedSubtype = null;
            if (SubtypeCombo.IsEnabled && SubtypeCombo.SelectedItem != null)
            {
                var subtypeValue = SubtypeCombo.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(subtypeValue) && subtypeValue != "(All)")
                {
                    selectedSubtype = subtypeValue;
                }
            }

            try
            {
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                DataFetcher dataFetcher = new DataFetcher(_connectionString);
                var dateRange = await _metricSelectionService.LoadDateRangeAsync(_viewModel.MetricState.SelectedMetricType, selectedSubtype, tableName);


                if (dateRange.HasValue)
                {
                    FromDate.SelectedDate = dateRange.Value.MinDate;
                    ToDate.SelectedDate = dateRange.Value.MaxDate;
                }
            }
            catch (Exception ex)
            {
                // Error handled silently - date range will use default values
            }
        }

        /// <summary>
        /// Loads data from HealthMetrics table based on selected MetricType, MetricSubtype(s), and date range.
        /// Updates charts using the selected metric type and subtypes.
        /// </summary>
        private async void OnLoadData(object sender, RoutedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (TablesCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a Metric Type", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());

            if (string.IsNullOrEmpty(_viewModel.MetricState.SelectedMetricType))
            {
                MessageBox.Show("Please select a valid Metric Type", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSubtype = _selectorManager.GetPrimarySubtype();
            var selectedSubtype2 = _selectorManager.GetSecondarySubtype();

            string baseType = _viewModel.MetricState.SelectedMetricType;
            string display1 = !string.IsNullOrEmpty(selectedSubtype) && selectedSubtype != "(All)"
                ? selectedSubtype
                : baseType;
            string display2 = !string.IsNullOrEmpty(selectedSubtype2)
                ? selectedSubtype2
                : "";

            SetChartTitles(display1, display2);
            UpdateChartLabels(selectedSubtype ?? string.Empty, selectedSubtype2 ?? string.Empty);

            var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
            var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;
            _viewModel.SetDateRange(fromDate, toDate);

            if (_viewModel.MetricState.FromDate > _viewModel.MetricState.ToDate)
            {
                MessageBox.Show("From date must be before To date", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                DataFetcher dataFetcher = new DataFetcher(_connectionString);

                var dataTask1 = dataFetcher.GetHealthMetricsDataByBaseType(
                    _viewModel.MetricState.SelectedMetricType,
                    selectedSubtype,
                    _viewModel.MetricState.FromDate,
                    _viewModel.MetricState.ToDate,
                    tableName);

                var dataTask2 = dataFetcher.GetHealthMetricsDataByBaseType(
                    _viewModel.MetricState.SelectedMetricType,
                    selectedSubtype2,
                    _viewModel.MetricState.FromDate,
                    _viewModel.MetricState.ToDate,
                    tableName);

                await Task.WhenAll(dataTask1, dataTask2);

                var data1 = dataTask1.Result;
                var data2 = dataTask2.Result;

                // Handle "no data" scenarios here and DO NOT call into the viewmodel for rendering
                if (data1 == null || !data1.Any())
                {
                    var subtypeText = !string.IsNullOrEmpty(selectedSubtype) ? $" and Subtype '{selectedSubtype}'" : "";
                    MessageBox.Show(
                        $"No data found for MetricType '{_viewModel.MetricState.SelectedMetricType}'{subtypeText} in the selected date range.",
                        "No Data",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    ClearAllCharts();
                    return;
                }

                if (data2 == null || !data2.Any())
                {
                    var subtypeText2 = !string.IsNullOrEmpty(selectedSubtype2) ? $" and Subtype '{selectedSubtype2}'" : "";
                    MessageBox.Show(
                        $"No data found for MetricType '{_viewModel.MetricState.SelectedMetricType}'{subtypeText2} in the selected date range (Chart 2).",
                        "No Data",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    ClearAllCharts();
                    return;
                }

                var displayName1 = !string.IsNullOrEmpty(selectedSubtype)
                    ? $"{_viewModel.MetricState.SelectedMetricType} - {selectedSubtype}"
                    : _viewModel.MetricState.SelectedMetricType;

                var displayName2 = !string.IsNullOrEmpty(selectedSubtype2)
                    ? $"{_viewModel.MetricState.SelectedMetricType} - {selectedSubtype2}"
                    : _viewModel.MetricState.SelectedMetricType;

                _viewModel.ChartState.LastContext = new ChartDataContext
                {
                    Data1 = data1,
                    Data2 = data2,
                    DisplayName1 = displayName1,
                    DisplayName2 = displayName2,
                    From = (DateTime)_viewModel.MetricState.FromDate,
                    To = (DateTime)_viewModel.MetricState.ToDate
                };

                _viewModel.LoadDataCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Primary (static) subtype combo
            if (SubtypeCombo.IsEnabled && SubtypeCombo.SelectedItem != null)
            {
                selectedSubtypes.Add(SubtypeCombo.SelectedItem.ToString());
            }

            // Dynamic subtypes managed by SubtypeSelectorManager
            var activeCombos = _selectorManager.GetActiveCombos();
            foreach (var combo in activeCombos)
            {
                if (combo.SelectedItem != null)
                {
                    selectedSubtypes.Add(combo.SelectedItem.ToString());
                }
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

            var data1 = ctx.Data1;
            var data2 = ctx.Data2;
            var displayName1 = ctx.DisplayName1 ?? string.Empty;
            var displayName2 = ctx.DisplayName2 ?? string.Empty;

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
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
