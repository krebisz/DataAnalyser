using DataVisualiser.Charts;
using DataVisualiser.Class;
using DataVisualiser.Helper;
using DataVisualiser.Services;
using DataVisualiser.State;
using DataVisualiser.UI.SubtypeSelectors;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DataVisualiser
{
    public partial class MainWindow : Window
    {
        private readonly ChartComputationEngine _chartComputationEngine;
        private readonly ChartRenderEngine _chartRenderEngine;

        private SubtypeComboBoxManager _comboManager;

        private readonly string _connectionString;
        //private bool _isLoadingMetricTypes = false;
        //private bool _isLoadingSubtypes = false;

        private ChartTooltipManager? _tooltipManager;

        //private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps = new();
        List<string>? subtypeList;

        // Chart visibility tracking
        //private bool _isChartNormVisible = false;
        //private bool _isChartDiffVisible = false;
        //private bool _isChartRatioVisible = false;
        //private bool _isChartWeeklyVisible = false;

        private MetricSelectionService _metricSelectionService;
        private ChartUpdateCoordinator _chartUpdateCoordinator;
        private WeeklyDistributionService _weeklyDistributionService;
        private SubtypeSelectorManager _selectorManager;

        //private ChartDataContext? _lastChartDataContext;

        private readonly ChartState _chartState = new();
        private readonly MetricState _metricState = new();
        private readonly UiState _uiState = new();

        // ============================================================
        // Normalization mode state (defaults to PercentageOfMax)
        // This maps to the radio buttons added in MainWindow.xaml
        // ============================================================
        //private NormalizationMode _selectedNormalizationMode = NormalizationMode.PercentageOfMax;
        // ============================================================

        public MainWindow()
        {
            InitializeComponent();

            _uiState.IsLoadingMetricTypes = false;
            _uiState.IsLoadingSubtypes = false;
            _chartState.IsNormalizedVisible = false;
            _chartState.IsDifferenceVisible = false;
            _chartState.IsRatioVisible = false;
            _chartState.IsWeeklyVisible = false;
            _chartState.SelectedNormalizationMode = NormalizationMode.PercentageOfMax;
            _chartState.LastContext = new ChartDataContext();

            _chartComputationEngine = new ChartComputationEngine();
            _chartRenderEngine = new ChartRenderEngine(normalizeYAxisDelegate: (axis, rawData, smoothed) => ChartHelper.NormalizeYAxis(axis, rawData, smoothed), adjustHeightDelegate: (chart, minHeight) => ChartHelper.AdjustChartHeightBasedOnYAxis(chart, minHeight));

            _selectorManager = new SubtypeSelectorManager(MetricSubtypePanel, SubtypeCombo);

            _selectorManager.SubtypeSelectionChanged += (s, e) =>
            {
                UpdateChartTitlesFromCombos();
                OnAnySubtypeSelectionChanged(s, null);
            };


            _comboManager = new SubtypeComboBoxManager(MetricSubtypePanel);

            //_chartComputationEngine = new ChartComputationEngine();
            //_chartRenderEngine = new ChartRenderEngine(
            //    normalizeYAxisDelegate: (axis, rawData, smoothed) => ChartHelper.NormalizeYAxis(axis, rawData, smoothed),
            //    adjustHeightDelegate: (chart, minHeight) => ChartHelper.AdjustChartHeightBasedOnYAxis(chart, minHeight)
            //);

            _connectionString = ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
            _metricSelectionService = new MetricSelectionService(_connectionString);


            _metricState.FromDate = DateTime.UtcNow.AddDays(-30);
            ToDate.SelectedDate = DateTime.UtcNow;

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

            // Initialize tooltip manager and attach all charts
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




            //public ChartTooltipManager(Window parentWindow, Dictionary<CartesianChart, string>? chartLabels = null)


            // Hide ChartDiff and ChartRatio initially
            ChartNormPanel.Visibility = Visibility.Collapsed;
            ChartDiffPanel.Visibility = Visibility.Collapsed;
            ChartRatioPanel.Visibility = Visibility.Collapsed;
            ChartNormToggleButton.Content = "Show";
            ChartDiffToggleButton.Content = "Show";
            ChartRatioToggleButton.Content = "Show";
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

            _comboManager.ClearDynamic(keepFirstCount: 1);

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
                //_isLoadingMetricTypes = true;
                _uiState.IsLoadingMetricTypes = true;

                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                //var baseMetricTypes = await dataFetcher.GetBaseMetricTypes(tableName);
                var baseMetricTypes = await _metricSelectionService.LoadMetricTypesAsync(tableName);

                TablesCombo.Items.Clear();

                foreach (var baseType in baseMetricTypes)
                {
                    TablesCombo.Items.Add(baseType);
                }

                if (TablesCombo.Items.Count > 0)
                {
                    TablesCombo.SelectedIndex = 0;
                    //_isLoadingMetricTypes = false;
                    _uiState.IsLoadingMetricTypes = false;
                    await LoadSubtypesForSelectedMetricType();
                    await LoadDateRangeForSelectedMetric();
                }
                else
                {
                    SubtypeCombo.Items.Clear();
                    SubtypeCombo.IsEnabled = false;
                    _comboManager.ClearDynamic(keepFirstCount: 1);
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
                //_isLoadingMetricTypes = false;
                _uiState.IsLoadingMetricTypes = false;
            }
        }

        /// <summary>
        /// Event handler for MetricType selection change - loads subtypes and updates date range.
        /// </summary>
        private async void OnMetricTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (_isLoadingMetricTypes)
            if (_uiState.IsLoadingMetricTypes)
                return;

            await LoadSubtypesForSelectedMetricType();
            await LoadDateRangeForSelectedMetric();

            UpdateChartTitlesFromCombos();
        }

        /// <summary>
        /// Generalized event handler for any MetricSubtype ComboBox selection change - updates date range to match data availability.
        /// This handler is used by all subtype ComboBoxes (both static and dynamically added).
        /// </summary>
        private async void OnAnySubtypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            await LoadDateRangeForSelectedMetrics();
        }

        private async Task LoadDateRangeForSelectedMetrics()
        {
            //if (_isLoadingSubtypes || _isLoadingMetricTypes)
            if (_uiState.IsLoadingSubtypes || _uiState.IsLoadingMetricTypes)
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
                _comboManager.ClearDynamic(keepFirstCount: 1);
                return;
            }

            _metricState.SelectedMetricType = TablesCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(_metricState.SelectedMetricType))
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                _comboManager.ClearDynamic(keepFirstCount: 1);
                return;
            }

            //List<ComboBox> active = _comboManager.GetActiveComboBoxes();
            var active = _selectorManager.GetActiveCombos();

            try
            {
                //_isLoadingSubtypes = true;
                _uiState.IsLoadingSubtypes = true;
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                //IEnumerable<string> subtypes = await dataFetcher.GetSubtypesForBaseType(selectedMetricType, tableName);
                var subtypes = await _metricSelectionService.LoadSubtypesAsync(_metricState.SelectedMetricType, tableName);

                SubtypeCombo.Items.Clear();
                _comboManager.ClearDynamic(keepFirstCount: 1);
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
                System.Diagnostics.Debug.WriteLine($"Error loading subtypes for {_metricState.SelectedMetricType}: {ex.Message}");
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
                //_isLoadingSubtypes = false;
                _uiState.IsLoadingSubtypes = false;
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

            _metricState.SelectedMetricType = TablesCombo.SelectedItem.ToString();

            if (string.IsNullOrEmpty(_metricState.SelectedMetricType))
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
                //var dateRange = await dataFetcher.GetBaseTypeDateRange(selectedMetricType, selectedSubtype, tableName);
                var dateRange = await _metricSelectionService.LoadDateRangeAsync(_metricState.SelectedMetricType, selectedSubtype, tableName);


                if (dateRange.HasValue)
                {
                    FromDate.SelectedDate = dateRange.Value.MinDate;
                    ToDate.SelectedDate = dateRange.Value.MaxDate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading date range for {_metricState.SelectedMetricType}: {ex.Message}");
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
            if (TablesCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a MetricType", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _metricState.SelectedMetricType = TablesCombo.SelectedItem.ToString();

            if (string.IsNullOrEmpty(_metricState.SelectedMetricType))
            {
                MessageBox.Show("Please select a valid MetricType", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSubtype = _selectorManager.GetPrimarySubtype();
            var selectedSubtype2 = _selectorManager.GetSecondarySubtype();

            var display1 = !string.IsNullOrEmpty(selectedSubtype) ? selectedSubtype : _metricState.SelectedMetricType;
            var display2 = !string.IsNullOrEmpty(selectedSubtype2) ? selectedSubtype2 : _metricState.SelectedMetricType;
            SetChartTitles(display1, display2);

            _metricState.FromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
            _metricState.ToDate = ToDate.SelectedDate ?? DateTime.UtcNow;

            if (_metricState.FromDate > _metricState.ToDate)
            {
                MessageBox.Show("From date must be before To date", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                DataFetcher dataFetcher = new DataFetcher(_connectionString);

                var dataTask1 = dataFetcher.GetHealthMetricsDataByBaseType(_metricState.SelectedMetricType, selectedSubtype, _metricState.FromDate, _metricState.ToDate, tableName);
                var dataTask2 = dataFetcher.GetHealthMetricsDataByBaseType(_metricState.SelectedMetricType, selectedSubtype2, _metricState.FromDate, _metricState.ToDate, tableName);

                await Task.WhenAll(dataTask1, dataTask2);

                var data1 = dataTask1.Result;
                var data2 = dataTask2.Result;

                if (data1 == null || !data1.Any())
                {
                    var subtypeText = !string.IsNullOrEmpty(selectedSubtype) ? $" and Subtype '{selectedSubtype}'" : "";
                    MessageBox.Show($"No data found for MetricType '{_metricState.SelectedMetricType}'{subtypeText} in the selected date range.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if ((data1 != null && data1.Any()) && (data2 != null && data2.Any()))
                {
                    var displayName1 = !string.IsNullOrEmpty(selectedSubtype) ? $"{_metricState.SelectedMetricType} - {selectedSubtype}" : _metricState.SelectedMetricType;
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{_metricState.SelectedMetricType} - {selectedSubtype2}" : _metricState.SelectedMetricType;

                    // Store data context for potential reload when charts are toggled visible
                    _chartState.LastContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = data2,
                        DisplayName1 = displayName1,
                        DisplayName2 = displayName2,
                        From = (DateTime)_metricState.FromDate,
                        To = (DateTime)_metricState.ToDate
                    };

                    //await UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, from, to), displayName1, displayName2);
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, _chartState.LastContext.From, _chartState.LastContext.To), displayName1, displayName2);


                    // Only update visible charts
                    //if (_isChartNormVisible)
                    if (_chartState.IsNormalizedVisible)
                    {
                        // Pass normalization mode into NormalizedStrategy
                        //await UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(data1, data2, displayName1, displayName2, from, to, _selectedNormalizationMode), $"{displayName1} ~ {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(data1, data2, displayName1, displayName2, _chartState.LastContext.From, _chartState.LastContext.To, _chartState.SelectedNormalizationMode), $"{displayName1} ~ {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartNorm, _chartState.ChartTimestamps);
                    }
                    // Insert after ChartNorm update (and before ChartDiff)
                    //if (_isChartWeeklyVisible)
                    if (_chartState.IsWeeklyVisible)
                    {
                        //await UpdateWeeklyDistributionChartAsync(ChartWeekly, data1, displayName1, from, to, minHeight: 400);
                        await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data1, displayName1, _chartState.LastContext.From, _chartState.LastContext.To, minHeight: 400);

                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartWeekly, _chartState.ChartTimestamps);
                    }
                    //if (_isChartDiffVisible)
                    if (_chartState.IsDifferenceVisible)
                    {
                        //await UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} - {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(data1, data2, displayName1, displayName2, _chartState.LastContext.From, _chartState.LastContext.To), $"{displayName1} - {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartDiff, _chartState.ChartTimestamps);
                    }

                    //if (_isChartRatioVisible)
                    if (_chartState.IsRatioVisible)
                    {
                        //await UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} / {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(data1, data2, displayName1, displayName2, _chartState.LastContext.From, _chartState.LastContext.To), $"{displayName1} / {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartRatio, _chartState.ChartTimestamps);
                    }
                }
                else if (data2 == null || !data2.Any())
                {
                    var subtypeText2 = !string.IsNullOrEmpty(selectedSubtype2) ? $" and Subtype '{selectedSubtype2}'" : "";
                    MessageBox.Show($"No data found for MetricType '{_metricState.SelectedMetricType}'{subtypeText2} in the selected date range (Chart 2).", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChartHelper.ClearChart(ChartMain, _chartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartNorm, _chartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _chartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _chartState.ChartTimestamps);
                    _chartState.LastContext = null; // Clear stored context when no data
                }
                else
                {
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{_metricState.SelectedMetricType} - {selectedSubtype2}" : _metricState.SelectedMetricType;

                    // Store data context for potential reload when charts are toggled visible
                    _chartState.LastContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = null,
                        DisplayName1 = displayName2,
                        DisplayName2 = string.Empty,
                        From = (DateTime)_metricState.FromDate,
                        To = (DateTime)_metricState.ToDate
                    };

                    //await UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.SingleMetricStrategy(data1 ?? Enumerable.Empty<HealthMetricData>(), displayName2, from, to), displayName2);
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.SingleMetricStrategy(data1 ?? Enumerable.Empty<HealthMetricData>(), displayName2, _chartState.LastContext.From, _chartState.LastContext.To), displayName2, minHeight: 400);

                    // Clear hidden charts
                    ChartHelper.ClearChart(ChartNorm, _chartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _chartState.ChartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _chartState.ChartTimestamps);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
        {
            var newCombo = _selectorManager.AddSubtypeCombo(subtypeList);

            _selectorManager.SubtypeSelectionChanged += (s, e) =>
            {
                UpdateChartTitlesFromCombos();
                OnAnySubtypeSelectionChanged(s, null);
            };


            newCombo.SelectedIndex = 0;
            newCombo.IsEnabled = true;

            // 🚀 NEW LINE — ensure second subtype label appears
            UpdateChartTitlesFromCombos();
        }


        #endregion

        #region Chart Visibility Toggle Handlers

        /// <summary>
        /// Toggles ChartNorm visibility and reloads data if available.
        /// </summary>
        private async void OnChartNormToggle(object sender, RoutedEventArgs e)
        {
            //_isChartNormVisible = !_isChartNormVisible;
            _chartState.IsNormalizedVisible = !_chartState.IsNormalizedVisible;


            //if (_isChartNormVisible)
            if (_chartState.IsNormalizedVisible)
            {
                ChartNormPanel.Visibility = Visibility.Visible;
                ChartNormToggleButton.Content = "Hide";

                // Reload data if available
                if (_chartState.LastContext != null && _chartState.LastContext.Data1 != null && _chartState.LastContext.Data2 != null)
                {
                    //await UpdateChartUsingStrategyAsync(
                    //    ChartNorm,
                    //    new DataVisualiser.Charts.Strategies.NormalizedStrategy(
                    //        _lastChartDataContext.Data1,
                    //        _lastChartDataContext.Data2,
                    //        _lastChartDataContext.DisplayName1,
                    //        _lastChartDataContext.DisplayName2,
                    //        _lastChartDataContext.From,
                    //        _lastChartDataContext.To,
                    //        _selectedNormalizationMode),

                    //    $"{_lastChartDataContext.DisplayName1} ~ {_lastChartDataContext.DisplayName2}",
                    //    minHeight: 400);

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_chartState.LastContext.Data1, _chartState.LastContext.Data2, _chartState.LastContext.DisplayName1, _chartState.LastContext.DisplayName2, _chartState.LastContext.From, _chartState.LastContext.To, _chartState.SelectedNormalizationMode), $"{_chartState.LastContext.DisplayName1} ~ {_chartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartNormPanel.Visibility = Visibility.Collapsed;
                ChartNormToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartNorm, _chartState.ChartTimestamps);
            }
        }

        /// <summary>
        /// Toggles Weekly Distribution chart visibility and reloads data if available.
        /// Default state is collapsed — toggle shows it.
        /// </summary>
        private async void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            //_isChartWeeklyVisible = !_isChartWeeklyVisible;
            _chartState.IsWeeklyVisible = !_chartState.IsWeeklyVisible;

            //if (_isChartWeeklyVisible)
            if (_chartState.IsWeeklyVisible)
            {
                ChartWeeklyPanel.Visibility = Visibility.Visible;
                ChartWeeklyToggleButton.Content = "Hide";

                // reload if we have a stored data context (single metric series required)
                if (_chartState.LastContext != null && _chartState.LastContext.Data1 != null)
                {
                    // we use Data1 for this chart (single-series distribution)
                    var data = _chartState.LastContext.Data1;

                    //await UpdateWeeklyDistributionChartAsync(ChartWeekly, data, _lastChartDataContext.DisplayName1, _lastChartDataContext.From, _lastChartDataContext.To, minHeight: 400);
                    await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data, _chartState.LastContext.DisplayName1, _chartState.LastContext.From, _chartState.LastContext.To, minHeight: 400);
                }
            }
            else
            {
                ChartWeeklyPanel.Visibility = Visibility.Collapsed;
                ChartWeeklyToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartWeekly, _chartState.ChartTimestamps);
            }
        }


        /// <summary>
        /// Toggles ChartDiff visibility and reloads data if available.
        /// </summary>
        private async void OnChartDiffToggle(object sender, RoutedEventArgs e)
        {
            //_isChartDiffVisible = !_isChartDiffVisible;
            _chartState.IsDifferenceVisible = !_chartState.IsDifferenceVisible;

                //if (_isChartDiffVisible)
                if (_chartState.IsDifferenceVisible)
                {
                ChartDiffPanel.Visibility = Visibility.Visible;
                ChartDiffToggleButton.Content = "Hide";

                // Reload data if available
                if (_chartState.LastContext != null && _chartState.LastContext.Data1 != null && _chartState.LastContext.Data2 != null)
                {
                    //await UpdateChartUsingStrategyAsync(
                    //    ChartDiff,
                    //    new DataVisualiser.Charts.Strategies.DifferenceStrategy(
                    //        _lastChartDataContext.Data1,
                    //        _lastChartDataContext.Data2,
                    //        _lastChartDataContext.DisplayName1,
                    //        _lastChartDataContext.DisplayName2,
                    //        _lastChartDataContext.From,
                    //        _lastChartDataContext.To),
                    //    $"{_lastChartDataContext.DisplayName1} - {_lastChartDataContext.DisplayName2}",
                    //    minHeight: 400);

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(_chartState.LastContext.Data1, _chartState.LastContext.Data2, _chartState.LastContext.DisplayName1, _chartState.LastContext.DisplayName2, _chartState.LastContext.From, _chartState.LastContext.To), $"{_chartState.LastContext.DisplayName1} - {_chartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartDiffPanel.Visibility = Visibility.Collapsed;
                ChartDiffToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartDiff, _chartState.ChartTimestamps);
            }
        }

        /// <summary>
        /// Toggles ChartRatio visibility and reloads data if available.
        /// </summary>
        private async void OnChartRatioToggle(object sender, RoutedEventArgs e)
        {
            //_isChartRatioVisible = !_isChartRatioVisible;
            _chartState.IsRatioVisible = !_chartState.IsRatioVisible;

            //if (_isChartRatioVisible)
            if (_chartState.IsRatioVisible)
            {
                ChartRatioPanel.Visibility = Visibility.Visible;
                ChartRatioToggleButton.Content = "Hide";

                // Reload data if available
                if (_chartState.LastContext != null && _chartState.LastContext.Data1 != null && _chartState.LastContext.Data2 != null)
                {
                    //await UpdateChartUsingStrategyAsync(
                    //    ChartRatio,
                    //    new DataVisualiser.Charts.Strategies.RatioStrategy(
                    //        _lastChartDataContext.Data1,
                    //        _lastChartDataContext.Data2,
                    //        _lastChartDataContext.DisplayName1,
                    //        _lastChartDataContext.DisplayName2,
                    //        _lastChartDataContext.From,
                    //        _lastChartDataContext.To),
                    //    $"{_lastChartDataContext.DisplayName1} / {_lastChartDataContext.DisplayName2}",
                    //    minHeight: 400);

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(_chartState.LastContext.Data1, _chartState.LastContext.Data2, _chartState.LastContext.DisplayName1, _chartState.LastContext.DisplayName2, _chartState.LastContext.From, _chartState.LastContext.To), $"{_chartState.LastContext.DisplayName1} / {_chartState.LastContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartRatioPanel.Visibility = Visibility.Collapsed;
                ChartRatioToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartRatio, _chartState.ChartTimestamps);
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

            _chartState.LeftTitle = leftName;
            _chartState.RightTitle = rightName;


            ChartMainTitle.Text = $"{leftName} vs. {rightName}";
            ChartNormTitle.Text = $"{leftName} ~ {rightName}";
            ChartDiffTitle.Text = $"{leftName} - {rightName}";
            ChartRatioTitle.Text = $"{leftName} / {rightName}";
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

            System.Diagnostics.Debug.WriteLine(
    $"[DEBUG] subtype1='{_selectorManager.GetPrimarySubtype()}', subtype2='{_selectorManager.GetSecondarySubtype()}'");


            SetChartTitles(display1, display2);

        }

        #endregion

        #region Normalization mode UI handling

        /// <summary>
        /// Handler for the normalization-mode radio buttons (wired in XAML).
        /// Refreshes the normalized chart if visible and data exists.
        /// </summary>
        private async void OnNormalizationModeChanged(object sender, RoutedEventArgs e)
        {
            // Radio button names taken from your XAML:
            // NormZeroToOneRadio, NormPercentOfMaxRadio, NormRelativeToMaxRadio
            try
            {
                if (NormZeroToOneRadio.IsChecked == true)
                    _chartState.SelectedNormalizationMode = NormalizationMode.ZeroToOne;
                else if (NormPercentOfMaxRadio.IsChecked == true)
                    _chartState.SelectedNormalizationMode = NormalizationMode.PercentageOfMax;
                else if (NormRelativeToMaxRadio.IsChecked == true)
                    _chartState.SelectedNormalizationMode = NormalizationMode.RelativeToMax;

                // If Norm chart visible and we have a stored data context, refresh only Normalized chart
                //if (_isChartNormVisible && _lastChartDataContext != null && _lastChartDataContext.Data1 != null && _lastChartDataContext.Data2 != null)
                if (_chartState.IsNormalizedVisible && _chartState.LastContext != null && _chartState.LastContext.Data1 != null && _chartState.LastContext.Data2 != null)
                {
                    //await UpdateChartUsingStrategyAsync(
                    //    ChartNorm,
                    //    new DataVisualiser.Charts.Strategies.NormalizedStrategy(
                    //        _lastChartDataContext.Data1,
                    //        _lastChartDataContext.Data2,
                    //        _lastChartDataContext.DisplayName1,
                    //        _lastChartDataContext.DisplayName2,
                    //        _lastChartDataContext.From,
                    //        _lastChartDataContext.To,
                    //        _selectedNormalizationMode // pass selected mode
                    //    ),
                    //    $"{_lastChartDataContext.DisplayName1} ~ {_lastChartDataContext.DisplayName2}",
                    //    minHeight: 400);


                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_chartState.LastContext.Data1, _chartState.LastContext.Data2, _chartState.LastContext.DisplayName1, _chartState.LastContext.DisplayName2, _chartState.LastContext.From, _chartState.LastContext.To, _chartState.SelectedNormalizationMode), $"{_chartState.LastContext.DisplayName1} ~ {_chartState.LastContext.DisplayName2}", minHeight: 400);
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Normalization mode change error: {ex.Message}");
            }
        }

        #endregion
    }
}
