using DataVisualiser.Charts;
using DataVisualiser.Charts.Strategies;
using DataVisualiser.Class;
using DataVisualiser.Helper;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.UI.SubtypeSelectors;
using DataVisualiser.ViewModels;
using DataVisualiser.ViewModels.Events;
using LiveCharts.Wpf;
using System.Configuration;
using System.Windows;

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

        // Replace existing fields
        private readonly MainWindowViewModel _viewModel;
        private bool _isInitializing = true; // Flag to prevent event handlers from running during initialization


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

            // Create services using _chartState.ChartTimestamps (same reference as _viewModel.ChartState.ChartTimestamps)
            _chartUpdateCoordinator = new ChartUpdateCoordinator(_chartComputationEngine, _chartRenderEngine, _tooltipManager, _chartState.ChartTimestamps);
            _weeklyDistributionService = new WeeklyDistributionService(_chartState.ChartTimestamps);

            // Now create _viewModel with all dependencies
            _viewModel = new MainWindowViewModel(_chartState, _metricState, _uiState, _metricSelectionService, _chartUpdateCoordinator, _weeklyDistributionService);
            DataContext = _viewModel;
            // Subscribe to ViewModel events (Phase 5C – event wiring, no behaviour change yet)
            _viewModel.ChartVisibilityChanged += OnChartVisibilityChanged;
            _viewModel.ErrorOccured += OnErrorOccured;

            // Initialize date range through viewModel
            var initialFromDate = DateTime.UtcNow.AddDays(-30);
            var initialToDate = DateTime.UtcNow;
            _viewModel.SetDateRange(initialFromDate, initialToDate);
            FromDate.SelectedDate = _viewModel.MetricState.FromDate;
            ToDate.SelectedDate = _viewModel.MetricState.ToDate;

            //_uiState.IsLoadingMetricTypes = false;
            _viewModel.SetLoadingMetricTypes(false);
            //_uiState.IsLoadingSubtypes = false;
            _viewModel.SetLoadingSubtypes(false);
            //_chartState.IsNormalizedVisible = false;
            _viewModel.SetNormalizedVisible(false);
            //_chartState.IsDifferenceVisible = false;
            _viewModel.SetDifferenceVisible(false);
            //_chartState.IsRatioVisible = false;
            _viewModel.SetRatioVisible(false);
            //_chartState.IsWeeklyVisible = false;
            _viewModel.SetWeeklyVisible(false);

            _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
            _viewModel.ChartState.LastContext = new ChartDataContext();
            _selectorManager = new SubtypeSelectorManager(MetricSubtypePanel, SubtypeCombo);

            _selectorManager.SubtypeSelectionChanged += (s, e) =>
            {
                UpdateChartTitlesFromCombos();
                // Pass null for SelectionChangedEventArgs since SubtypeSelectionChanged uses EventArgs
                OnAnySubtypeSelectionChanged(s ?? this, null);
            };

            ResolutionCombo.Items.Add("All");
            ResolutionCombo.Items.Add("Hourly");
            ResolutionCombo.Items.Add("Daily");
            ResolutionCombo.Items.Add("Weekly");
            ResolutionCombo.Items.Add("Monthly");
            ResolutionCombo.Items.Add("Yearly");
            ResolutionCombo.SelectedItem = "All";

            LoadMetricTypes();

            ChartHelper.InitializeChartBehavior(ChartMain);
            ChartHelper.InitializeChartBehavior(ChartNorm);
            ChartHelper.InitializeChartBehavior(ChartDiff);
            ChartHelper.InitializeChartBehavior(ChartRatio);

            UpdateChartTitlesFromCombos();

            ChartNormPanel.Visibility = Visibility.Collapsed;
            ChartDiffPanel.Visibility = Visibility.Collapsed;
            ChartRatioPanel.Visibility = Visibility.Collapsed;
            ChartNormToggleButton.Content = "Show";
            ChartDiffToggleButton.Content = "Show";
            ChartRatioToggleButton.Content = "Show";

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

            LoadMetricTypes();
        }

        /// <summary>
        /// Loads distinct base MetricType values from the selected resolution table into the combo box.
        /// Only includes metric types that have data with valid date ranges.
        /// </summary>
        private async void LoadMetricTypes()
        {
            try
            {
                //_uiState.IsLoadingMetricTypes = true;
                _viewModel.SetLoadingMetricTypes(true);

                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                var baseMetricTypes = await _metricSelectionService.LoadMetricTypesAsync(tableName);

                TablesCombo.Items.Clear();

                foreach (var baseType in baseMetricTypes)
                {
                    TablesCombo.Items.Add(baseType);
                }

                if (TablesCombo.Items.Count > 0)
                {
                    TablesCombo.SelectedIndex = 0;
                    //_uiState.IsLoadingMetricTypes = false;
                    _viewModel.SetLoadingMetricTypes(false);
                    await LoadSubtypesForSelectedMetricType();
                    await LoadDateRangeForSelectedMetric();
                }
                else
                {
                    SubtypeCombo.Items.Clear();
                    SubtypeCombo.IsEnabled = false;
                    _selectorManager.ClearDynamic();
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex is Microsoft.Data.SqlClient.SqlException sqlEx
                    ? $"Database connection error: {sqlEx.Message}\n\nPlease check:\n1. SQL Server is running\n2. Database 'Health' exists\n3. Connection string in App.config is correct"
                    : $"Error loading metric types: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //_uiState.IsLoadingMetricTypes = false;
                _viewModel.SetLoadingMetricTypes(false);
            }
        }

        /// <summary>
        /// Event handler for MetricType selection change - loads subtypes and updates date range.
        /// </summary>
        private async void OnMetricTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            //if (_uiState.IsLoadingMetricTypes)
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
            // NEW: keep VM in sync with all selected subtypes
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
                _viewModel.SetLoadingMetricTypes(true);
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

                    // NEW: propagate this default selection into the VM
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
                System.Diagnostics.Debug.WriteLine($"Error loading subtypes for {_viewModel.MetricState.SelectedMetricType}: {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Error loading date range for {_viewModel.MetricState.SelectedMetricType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads data from HealthMetrics table based on selected MetricType, MetricSubtype(s), and date range
        /// Updates both Chart and ChartMain using the same MetricType but their respective subtypes.
        /// ChartMain will show both series on a single shared axis when both datasets exist.
        /// ChartDiff shows the difference (data1 - data2) aligned to ChartMain's X axis.
        /// </summary>
        private async void OnLoadData(object sender, RoutedEventArgs e)
        {
            // Guard against null reference during initialization
            if (_isInitializing || _viewModel == null)
                return;

            if (TablesCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a MetricType", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());

            if (string.IsNullOrEmpty(_viewModel.MetricState.SelectedMetricType))
            {
                MessageBox.Show("Please select a valid MetricType", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSubtype = _selectorManager.GetPrimarySubtype();
            var selectedSubtype2 = _selectorManager.GetSecondarySubtype();

            // Use the same logic as UpdateChartTitlesFromCombos for consistency
            string baseType = _viewModel.MetricState.SelectedMetricType;
            string display1 = !string.IsNullOrEmpty(selectedSubtype) && selectedSubtype != "(All)"
                ? selectedSubtype
                : baseType;
            string display2 = !string.IsNullOrEmpty(selectedSubtype2)
                ? selectedSubtype2
                : "";

            SetChartTitles(display1, display2);
            // Update chart labels using actual subtype values, not display values that fall back to base type
            UpdateChartLabels(selectedSubtype ?? "", selectedSubtype2 ?? "");

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

                var dataTask1 = dataFetcher.GetHealthMetricsDataByBaseType(_viewModel.MetricState.SelectedMetricType, selectedSubtype, _viewModel.MetricState.FromDate, _viewModel.MetricState.ToDate, tableName);
                var dataTask2 = dataFetcher.GetHealthMetricsDataByBaseType(_viewModel.MetricState.SelectedMetricType, selectedSubtype2, _viewModel.MetricState.FromDate, _viewModel.MetricState.ToDate, tableName);

                await Task.WhenAll(dataTask1, dataTask2);

                var data1 = dataTask1.Result;
                var data2 = dataTask2.Result;

                if (data1 == null || !data1.Any())
                {
                    var subtypeText = !string.IsNullOrEmpty(selectedSubtype) ? $" and Subtype '{selectedSubtype}'" : "";
                    MessageBox.Show($"No data found for MetricType '{_viewModel.MetricState.SelectedMetricType}'{subtypeText} in the selected date range.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if ((data1 != null && data1.Any()) && (data2 != null && data2.Any()))
                {
                    var displayName1 = !string.IsNullOrEmpty(selectedSubtype) ? $"{_viewModel.MetricState.SelectedMetricType} - {selectedSubtype}" : _viewModel.MetricState.SelectedMetricType;
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{_viewModel.MetricState.SelectedMetricType} - {selectedSubtype2}" : _viewModel.MetricState.SelectedMetricType;

                    _viewModel.ChartState.LastContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = data2,
                        DisplayName1 = displayName1,
                        DisplayName2 = displayName2,
                        From = (DateTime)_viewModel.MetricState.FromDate,
                        To = (DateTime)_viewModel.MetricState.ToDate
                    };

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), displayName1, displayName2);


                    // Only update visible charts
                    if (_viewModel.ChartState.IsNormalizedVisible)
                    {
                        // Pass normalization mode into NormalizedStrategy
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(data1, data2, displayName1, displayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, _viewModel.ChartState.SelectedNormalizationMode), $"{displayName1} ~ {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
                    }
                    // Insert after ChartNorm update (and before ChartDiff)
                    if (_viewModel.ChartState.IsWeeklyVisible)
                    {
                        await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data1, displayName1, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, minHeight: 400);

                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
                    }
                    if (_viewModel.ChartState.IsDifferenceVisible)
                    {
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(data1, data2, displayName1, displayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), $"{displayName1} - {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
                    }

                    if (_viewModel.ChartState.IsRatioVisible)
                    {
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(data1, data2, displayName1, displayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), $"{displayName1} / {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
                    }
                }
                else if (data2 == null || !data2.Any())
                {
                    var subtypeText2 = !string.IsNullOrEmpty(selectedSubtype2) ? $" and Subtype '{selectedSubtype2}'" : "";
                    MessageBox.Show($"No data found for MetricType '{_viewModel.MetricState.SelectedMetricType}'{subtypeText2} in the selected date range (Chart 2).", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChartHelper.ClearChart(ChartMain, _viewModel.ChartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
                    _viewModel.ChartState.LastContext = null; // Clear stored context when no data
                }
                else
                {
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{_viewModel.MetricState.SelectedMetricType} - {selectedSubtype2}" : _viewModel.MetricState.SelectedMetricType;

                    // Store data context for potential reload when charts are toggled visible
                    _viewModel.ChartState.LastContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = null,
                        DisplayName1 = displayName2,
                        DisplayName2 = string.Empty,
                        From = (DateTime)_viewModel.MetricState.FromDate,
                        To = (DateTime)_viewModel.MetricState.ToDate
                    };

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.SingleMetricStrategy(data1 ?? Enumerable.Empty<HealthMetricData>(), displayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), displayName2, minHeight: 400);

                    // Clear hidden charts
                    ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
        {
            if (subtypeList == null) return;
            var newCombo = _selectorManager.AddSubtypeCombo(subtypeList);
            newCombo.SelectedIndex = 0;
            newCombo.IsEnabled = true;

            UpdateChartTitlesFromCombos();

            // NEW: update VM with new collection of subtypes
            UpdateSelectedSubtypesInViewModel();
        }


        #endregion

        #region Chart Visibility Toggle Handlers

        /// <summary>
        /// Toggles ChartNorm visibility and reloads data if available.
        /// </summary>
        private async void OnChartNormToggle(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.ChartState.IsNormalizedVisible = !_viewModel.ChartState.IsNormalizedVisible;

            if (_viewModel.ChartState.IsNormalizedVisible)
            {
                ChartNormPanel.Visibility = Visibility.Visible;
                ChartNormToggleButton.Content = "Hide";

                // Reload data if available
                if (_viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
                {
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_viewModel.ChartState.LastContext.Data1, _viewModel.ChartState.LastContext.Data2, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.DisplayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, _viewModel.ChartState.SelectedNormalizationMode), $"{_viewModel.ChartState.LastContext.DisplayName1} ~ {_viewModel.ChartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartNormPanel.Visibility = Visibility.Collapsed;
                ChartNormToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            }
        }

        /// <summary>
        /// Toggles Weekly Distribution chart visibility and reloads data if available.
        /// Default state is collapsed — toggle shows it.
        /// </summary>
        private async void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.ChartState.IsWeeklyVisible = !_viewModel.ChartState.IsWeeklyVisible;

            if (_viewModel.ChartState.IsWeeklyVisible)
            {
                ChartWeeklyPanel.Visibility = Visibility.Visible;
                ChartWeeklyToggleButton.Content = "Hide";

                // reload if we have a stored data context (single metric series required)
                if (_viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null)
                {
                    // we use Data1 for this chart (single-series distribution)
                    var data = _viewModel.ChartState.LastContext.Data1;
                    await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, minHeight: 400);
                }
            }
            else
            {
                ChartWeeklyPanel.Visibility = Visibility.Collapsed;
                ChartWeeklyToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartWeekly, _viewModel.ChartState.ChartTimestamps);
            }
        }


        /// <summary>
        /// Toggles ChartDiff visibility and reloads data if available.
        /// </summary>
        private async void OnChartDiffToggle(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.ChartState.IsDifferenceVisible = !_viewModel.ChartState.IsDifferenceVisible;

            if (_viewModel.ChartState.IsDifferenceVisible)
            {
                ChartDiffPanel.Visibility = Visibility.Visible;
                ChartDiffToggleButton.Content = "Hide";

                // Reload data if available
                if (_viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
                {
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(_viewModel.ChartState.LastContext.Data1, _viewModel.ChartState.LastContext.Data2, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.DisplayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), $"{_viewModel.ChartState.LastContext.DisplayName1} - {_viewModel.ChartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartDiffPanel.Visibility = Visibility.Collapsed;
                ChartDiffToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartDiff, _viewModel.ChartState.ChartTimestamps);
            }
        }

        /// <summary>
        /// Toggles ChartRatio visibility and reloads data if available.
        /// </summary>
        private async void OnChartRatioToggle(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) return;

            _viewModel.ChartState.IsRatioVisible = !_viewModel.ChartState.IsRatioVisible;

            if (_viewModel.ChartState.IsRatioVisible)
            {
                ChartRatioPanel.Visibility = Visibility.Visible;
                ChartRatioToggleButton.Content = "Hide";

                // Reload data if available
                if (_viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
                {
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(_viewModel.ChartState.LastContext.Data1, _viewModel.ChartState.LastContext.Data2, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.DisplayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To), $"{_viewModel.ChartState.LastContext.DisplayName1} / {_viewModel.ChartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartRatioPanel.Visibility = Visibility.Collapsed;
                ChartRatioToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartRatio, _viewModel.ChartState.ChartTimestamps);
            }
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

            System.Diagnostics.Debug.WriteLine($"[DEBUG] subtype1='{subtype1}', subtype2='{subtype2}'");

            SetChartTitles(display1, display2);
            // Pass raw subtype values to UpdateChartLabels, not display values
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
            // This can happen if a radio button has IsChecked="True" in XAML,
            // which fires the Checked event during InitializeComponent() before _viewModel is initialized
            if (_isInitializing || _viewModel == null)
            {
                return;
            }

            // Radio button names taken from your XAML:
            // NormZeroToOneRadio, NormPercentOfMaxRadio, NormRelativeToMaxRadio
            try
            {
                if (NormZeroToOneRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
                else if (NormPercentOfMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
                else if (NormRelativeToMaxRadio.IsChecked == true)
                    _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);

                // If Norm chart visible and we have a stored data context, refresh only Normalized chart
                if (_viewModel.ChartState.IsNormalizedVisible && _viewModel.ChartState.LastContext != null && _viewModel.ChartState.LastContext.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
                {
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_viewModel.ChartState.LastContext.Data1, _viewModel.ChartState.LastContext.Data2, _viewModel.ChartState.LastContext.DisplayName1, _viewModel.ChartState.LastContext.DisplayName2, _viewModel.ChartState.LastContext.From, _viewModel.ChartState.LastContext.To, _viewModel.ChartState.SelectedNormalizationMode), $"{_viewModel.ChartState.LastContext.DisplayName1} ~ {_viewModel.ChartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Normalization mode change error: {ex.Message}");
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
                TablesCombo.SelectedIndex = 0;
        }

        private void OnSubtypesLoaded(object? sender, SubtypesLoadedEventArgs e)
        {
            SubtypeCombo.Items.Clear();
            foreach (var sub in e.Subtypes)
                SubtypeCombo.Items.Add(sub);

            SubtypeCombo.IsEnabled = e.Subtypes.Any();
        }

        private void OnDateRangeLoaded(object? sender, DateRangeLoadedEventArgs e)
        {
            FromDate.SelectedDate = e.MinDate;
            ToDate.SelectedDate = e.MaxDate;
        }

        private async void OnDataLoaded(object? sender, DataLoadedEventArgs e)
        {
            var ctx = e.DataContext;

            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
                ChartMain,
                new CombinedMetricStrategy(ctx.Data1!, ctx.Data2!, ctx.DisplayName1!, ctx.DisplayName2!, ctx.From, ctx.To),
                ctx.DisplayName1!,
                ctx.DisplayName2!
            );
        }

        private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
        {
            switch (e.ChartName)
            {       
                case "Norm":
                    ChartNormPanel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "Diff":
                    ChartDiffPanel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "Ratio":
                    ChartRatioPanel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
                case "Weekly":
                    ChartWeeklyPanel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    break;
            }
        }

        private void OnErrorOccured(object? sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
