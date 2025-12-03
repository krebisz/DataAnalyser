using DataVisualiser.Charts;
using DataVisualiser.Class;
using DataVisualiser.Helper;
using DataVisualiser.Services;
using LiveCharts;
using LiveCharts.Wpf;
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
        private bool _isLoadingMetricTypes = false;
        private bool _isLoadingSubtypes = false;

        private ChartTooltipManager? _tooltipManager;

        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps = new();
        List<string>? subtypeList;

        // Chart visibility tracking
        private bool _isChartNormVisible = false;
        private bool _isChartDiffVisible = false;
        private bool _isChartRatioVisible = false;
        private bool _isChartWeeklyVisible = false;

        private MetricSelectionService _metricSelectionService;
        private ChartUpdateCoordinator _chartUpdateCoordinator;
        private WeeklyDistributionService _weeklyDistributionService;

        // Store last loaded data for charts so they can be reloaded when toggled visible
        private class ChartDataContext
        {
            public IEnumerable<HealthMetricData>? Data1 { get; set; }
            public IEnumerable<HealthMetricData>? Data2 { get; set; }
            public string DisplayName1 { get; set; } = string.Empty;
            public string DisplayName2 { get; set; } = string.Empty;
            public DateTime From { get; set; }
            public DateTime To { get; set; }
        }

        private ChartDataContext? _lastChartDataContext;

        // ============================================================
        // Normalization mode state (defaults to PercentageOfMax)
        // This maps to the radio buttons added in MainWindow.xaml
        // ============================================================
        private NormalizationMode _selectedNormalizationMode = NormalizationMode.PercentageOfMax;
        // ============================================================

        public MainWindow()
        {
            InitializeComponent();

            _chartComputationEngine = new ChartComputationEngine();
            _chartRenderEngine = new ChartRenderEngine(normalizeYAxisDelegate: (axis, rawData, smoothed) => ChartHelper.NormalizeYAxis(axis, rawData, smoothed), adjustHeightDelegate: (chart, minHeight) => ChartHelper.AdjustChartHeightBasedOnYAxis(chart, minHeight));


            _comboManager = new SubtypeComboBoxManager(MetricSubtypePanel);

            //_chartComputationEngine = new ChartComputationEngine();
            //_chartRenderEngine = new ChartRenderEngine(
            //    normalizeYAxisDelegate: (axis, rawData, smoothed) => ChartHelper.NormalizeYAxis(axis, rawData, smoothed),
            //    adjustHeightDelegate: (chart, minHeight) => ChartHelper.AdjustChartHeightBasedOnYAxis(chart, minHeight)
            //);

            _connectionString = ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
            _metricSelectionService = new MetricSelectionService(_connectionString);


            FromDate.SelectedDate = DateTime.UtcNow.AddDays(-30);
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

            _chartUpdateCoordinator = new ChartUpdateCoordinator(_chartComputationEngine, _chartRenderEngine, _tooltipManager, _chartTimestamps);
            _weeklyDistributionService = new WeeklyDistributionService(_chartTimestamps);




            //public ChartTooltipManager(Window parentWindow, Dictionary<CartesianChart, string>? chartLabels = null)


            // Hide ChartDiff and ChartRatio initially
            ChartNormPanel.Visibility = Visibility.Collapsed;
            ChartDiffPanel.Visibility = Visibility.Collapsed;
            ChartRatioPanel.Visibility = Visibility.Collapsed;
            ChartNormToggleButton.Content = "Show";
            ChartDiffToggleButton.Content = "Show";
            ChartRatioToggleButton.Content = "Show";
        }

        #region Chart Update Methods

        private async Task UpdateChartUsingStrategyAsync(CartesianChart targetChart, IChartComputationStrategy strategy, string primaryLabel, string? secondaryLabel = null, double minHeight = 400.0)
        {
            var result = await _chartComputationEngine.ComputeAsync(strategy);

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            var model = new ChartRenderModel
            {
                PrimarySeriesName = strategy.PrimaryLabel ?? primaryLabel,
                SecondarySeriesName = strategy.SecondaryLabel ?? secondaryLabel ?? string.Empty,
                PrimaryRaw = result.PrimaryRawValues,
                PrimarySmoothed = result.PrimarySmoothed,
                SecondaryRaw = result.SecondaryRawValues,
                SecondarySmoothed = result.SecondarySmoothed,
                PrimaryColor = ColourPalette.Next(targetChart),
                SecondaryColor = result.SecondaryRawValues != null && result.SecondarySmoothed != null
                    ? ColourPalette.Next(targetChart)
                    : Colors.Red,
                Unit = result.Unit,
                Timestamps = result.Timestamps,
                IntervalIndices = result.IntervalIndices,
                NormalizedIntervals = result.NormalizedIntervals,
                TickInterval = result.TickInterval
            };

            try
            {
                _chartRenderEngine.Render(targetChart, model, minHeight);
                _chartTimestamps[targetChart] = model.Timestamps;

                // Update tooltip manager with new timestamps
                _tooltipManager?.UpdateChartTimestamps(targetChart, model.Timestamps);

                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    var syntheticRawData = new List<HealthMetricData>();
                    var timestamps = model.Timestamps;
                    var primaryRaw = model.PrimaryRaw;

                    for (int i = 0; i < timestamps.Count; i++)
                    {
                        var val = primaryRaw[i];
                        syntheticRawData.Add(new HealthMetricData
                        {
                            NormalizedTimestamp = timestamps[i],
                            Value = double.IsNaN(val) ? (decimal?)null : (decimal?)val,
                            Unit = model.Unit
                        });
                    }

                    var smoothedList = model.PrimarySmoothed.ToList();
                    if (model.SecondarySmoothed != null)
                        smoothedList.AddRange(model.SecondarySmoothed);

                    ChartHelper.NormalizeYAxis(yAxis, syntheticRawData, smoothedList);
                    ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
                    ChartHelper.InitializeChartTooltip(targetChart);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart update/render error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region UI Initialization
        // Tooltip initialization is now handled by ChartTooltipManager
        #endregion

        #region Chart Interaction (Hover/Mouse Events)
        // Tooltip and hover interactions are now handled by ChartTooltipManager
        #endregion

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
                _isLoadingMetricTypes = true;

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
                    _isLoadingMetricTypes = false;
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
                _isLoadingMetricTypes = false;
            }
        }

        /// <summary>
        /// Event handler for MetricType selection change - loads subtypes and updates date range.
        /// </summary>
        private async void OnMetricTypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isLoadingMetricTypes)
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
            if (_isLoadingSubtypes || _isLoadingMetricTypes)
                return;

            await LoadDateRangeForSelectedMetric();

            UpdateChartTitlesFromCombos();
        }

        /// <summary>
        /// Builds a Monday->Sunday min-max stacked column visualization:
        ///  - baseline (transparent) = min per day
        ///  - range column (blue) = max - min per day (stacked)
        /// </summary>
        private async Task UpdateWeeklyDistributionChartAsync(CartesianChart targetChart, IEnumerable<HealthMetricData> data, string displayName, DateTime from, DateTime to, double minHeight = 400.0)
        {
            if (data == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            // compute using strategy inside Task.Run to avoid blocking UI (consistent with your pattern)
            var computeTask = Task.Run(() =>
            {
                var strat = new DataVisualiser.Charts.Strategies.WeeklyDistributionStrategy(data, displayName, from, to);
                var res = strat.Compute();
                return res;
            });

            var result = await computeTask;

            if (result == null)
            {
                ChartHelper.ClearChart(targetChart, _chartTimestamps);
                return;
            }

            try
            {
                // Clear previous series
                targetChart.Series.Clear();

                // Monday->Sunday names already set in XAML labels, but we'll keep an ordered array if needed
                var dayLabels = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

                // PrimaryRawValues = mins; PrimarySmoothed = ranges (max - min)
                var mins = result.PrimaryRawValues;
                var ranges = result.PrimarySmoothed;

                // If mins or ranges are missing or lengths differ, bail gracefully
                if (mins == null || ranges == null || mins.Count != 7 || ranges.Count != 7)
                {
                    ChartHelper.ClearChart(targetChart, _chartTimestamps);
                    return;
                }

                // Baseline series: invisible columns used to set the bottom of each stacked column
                var baselineSeries = new LiveCharts.Wpf.StackedColumnSeries
                {
                    Title = $"{displayName} baseline",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                    StrokeThickness = 0,
                    MaxColumnWidth = 40
                };

                // Range series: visible blue stacked columns placed on top of baseline
                var rangeSeries = new LiveCharts.Wpf.StackedColumnSeries
                {
                    Title = $"{displayName} range",
                    Values = new LiveCharts.ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(173, 216, 230)), // light blue
                    Stroke = new SolidColorBrush(Color.FromRgb(60, 120, 200)),
                    StrokeThickness = 1,
                    MaxColumnWidth = 40
                };

                // Populate values Monday -> Sunday
                for (int i = 0; i < 7; i++)
                {
                    var minVal = mins[i];
                    var rangeVal = ranges[i];

                    // Replace NaN with 0 so columns show nothing for empty days
                    if (double.IsNaN(minVal)) minVal = 0.0;
                    if (double.IsNaN(rangeVal) || rangeVal < 0) rangeVal = 0.0;

                    baselineSeries.Values.Add(minVal);
                    rangeSeries.Values.Add(rangeVal);
                }

                // Add baseline first, then range (stacked)
                targetChart.Series.Add(baselineSeries);
                targetChart.Series.Add(rangeSeries);

                //// --- ensure categorical alignment for Monday(0)..Sunday(6) ---
                //if (targetChart.AxisX.Count > 0)
                //{
                //    var xAxis = targetChart.AxisX[0];

                //    // enforce labels and ordering (defensive)
                //    xAxis.Labels = new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };

                //    // lock tick step to 1 so integer ticks align with columns
                //    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = 1 };

                //    // Give the axis half-unit padding so integer-indexed columns (0..6) are centered on ticks.
                //    // This ensures stable alignment while zooming/panning.
                //    xAxis.MinValue = -0.5;
                //    xAxis.MaxValue = 6.5;

                //    // Optional: keep labels visible and let the chart show them
                //    xAxis.ShowLabels = true;
                //}

                // Configure axes: Y axis floor/ceiling from underlying raw data
                var allValues = new List<double>();
                for (int i = 0; i < 7; i++)
                {
                    if (!double.IsNaN(mins[i])) allValues.Add(mins[i]);
                    if (!double.IsNaN(mins[i]) && !double.IsNaN(ranges[i])) allValues.Add(mins[i] + ranges[i]);
                }

                if (allValues.Count > 0)
                {
                    var min = Math.Floor(allValues.Min() / 5.0) * 5.0; // round down to nearest 5
                    var max = Math.Ceiling(allValues.Max() / 5.0) * 5.0; // round up to nearest 5

                    // small padding
                    var pad = Math.Max(5, (max - min) * 0.05);
                    var yMin = Math.Max(0, min - pad);
                    var yMax = max + pad;

                    if (targetChart.AxisY.Count > 0)
                    {
                        var yAxis = targetChart.AxisY[0];
                        yAxis.MinValue = yMin;
                        yAxis.MaxValue = yMax;

                        // Set a sensible step
                        var step = MathHelper.RoundToThreeSignificantDigits((yMax - yMin) / 8.0);
                        if (step > 0 && !double.IsNaN(step) && !double.IsInfinity(step))
                            yAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };

                        yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                    }
                }

                // Keep consistent UI patterns (tooltip, timestamps)
                _chartTimestamps[targetChart] = new List<DateTime>(); // not used for categorical chart

                ChartHelper.InitializeChartTooltip(targetChart);

                ChartHelper.AdjustChartHeightBasedOnYAxis(targetChart, minHeight);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Weekly distribution chart error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating weekly distribution chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            var selectedMetricType = TablesCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectedMetricType))
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                _comboManager.ClearDynamic(keepFirstCount: 1);
                return;
            }

            List<ComboBox> active = _comboManager.GetActiveComboBoxes();

            try
            {
                _isLoadingSubtypes = true;
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                //IEnumerable<string> subtypes = await dataFetcher.GetSubtypesForBaseType(selectedMetricType, tableName);
                var subtypes = await _metricSelectionService.LoadSubtypesAsync(selectedMetricType, tableName);

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
                System.Diagnostics.Debug.WriteLine($"Error loading subtypes for {selectedMetricType}: {ex.Message}");
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
                _isLoadingSubtypes = false;
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

            var selectedMetricType = TablesCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectedMetricType))
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
                var dateRange = await _metricSelectionService.LoadDateRangeAsync(selectedMetricType, selectedSubtype, tableName);


                if (dateRange.HasValue)
                {
                    FromDate.SelectedDate = dateRange.Value.MinDate;
                    ToDate.SelectedDate = dateRange.Value.MaxDate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading date range for {selectedMetricType}: {ex.Message}");
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

            string? selectedMetricType = TablesCombo.SelectedItem.ToString();

            if (string.IsNullOrEmpty(selectedMetricType))
            {
                MessageBox.Show("Please select a valid MetricType", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            List<ComboBox> active = _comboManager.GetActiveComboBoxes();

            string? selectedSubtype = ChartHelper.GetSubMetricType(SubtypeCombo);
            var dynamicCombo = active.FirstOrDefault(cb => cb != SubtypeCombo);
            string? selectedSubtype2 = dynamicCombo != null ? ChartHelper.GetSubMetricType(dynamicCombo) : null;

            var display1 = !string.IsNullOrEmpty(selectedSubtype) ? selectedSubtype : selectedMetricType;
            var display2 = !string.IsNullOrEmpty(selectedSubtype2) ? selectedSubtype2 : selectedMetricType;
            SetChartTitles(display1, display2);

            DateTime from = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
            DateTime to = ToDate.SelectedDate ?? DateTime.UtcNow;

            if (from > to)
            {
                MessageBox.Show("From date must be before To date", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                DataFetcher dataFetcher = new DataFetcher(_connectionString);

                var dataTask1 = dataFetcher.GetHealthMetricsDataByBaseType(selectedMetricType, selectedSubtype, from, to, tableName);
                var dataTask2 = dataFetcher.GetHealthMetricsDataByBaseType(selectedMetricType, selectedSubtype2, from, to, tableName);

                await Task.WhenAll(dataTask1, dataTask2);

                var data1 = dataTask1.Result;
                var data2 = dataTask2.Result;

                if (data1 == null || !data1.Any())
                {
                    var subtypeText = !string.IsNullOrEmpty(selectedSubtype) ? $" and Subtype '{selectedSubtype}'" : "";
                    MessageBox.Show($"No data found for MetricType '{selectedMetricType}'{subtypeText} in the selected date range.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if ((data1 != null && data1.Any()) && (data2 != null && data2.Any()))
                {
                    var displayName1 = !string.IsNullOrEmpty(selectedSubtype) ? $"{selectedMetricType} - {selectedSubtype}" : selectedMetricType;
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{selectedMetricType} - {selectedSubtype2}" : selectedMetricType;

                    // Store data context for potential reload when charts are toggled visible
                    _lastChartDataContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = data2,
                        DisplayName1 = displayName1,
                        DisplayName2 = displayName2,
                        From = from,
                        To = to
                    };

                    //await UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, from, to), displayName1, displayName2);
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.CombinedMetricStrategy(data1, data2, displayName1, displayName2, from, to), displayName1, displayName2);


                    // Only update visible charts
                    if (_isChartNormVisible)
                    {
                        // Pass normalization mode into NormalizedStrategy
                        //await UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(data1, data2, displayName1, displayName2, from, to, _selectedNormalizationMode), $"{displayName1} ~ {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(data1, data2, displayName1, displayName2, from, to, _selectedNormalizationMode), $"{displayName1} ~ {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartNorm, _chartTimestamps);
                    }
                    // Insert after ChartNorm update (and before ChartDiff)
                    if (_isChartWeeklyVisible)
                    {
                        //await UpdateWeeklyDistributionChartAsync(ChartWeekly, data1, displayName1, from, to, minHeight: 400);
                        await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data1, displayName1, from, to, minHeight: 400);

                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartWeekly, _chartTimestamps);
                    }
                    if (_isChartDiffVisible)
                    {
                        //await UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} - {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} - {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartDiff, _chartTimestamps);
                    }

                    if (_isChartRatioVisible)
                    {
                        //await UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} / {displayName2}", minHeight: 400);
                        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(data1, data2, displayName1, displayName2, from, to), $"{displayName1} / {displayName2}", minHeight: 400);
                    }
                    else
                    {
                        ChartHelper.ClearChart(ChartRatio, _chartTimestamps);
                    }
                }
                else if (data2 == null || !data2.Any())
                {
                    var subtypeText2 = !string.IsNullOrEmpty(selectedSubtype2) ? $" and Subtype '{selectedSubtype2}'" : "";
                    MessageBox.Show($"No data found for MetricType '{selectedMetricType}'{subtypeText2} in the selected date range (Chart 2).", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChartHelper.ClearChart(ChartMain, _chartTimestamps);
                    ChartHelper.ClearChart(ChartNorm, _chartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _chartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _chartTimestamps);
                    _lastChartDataContext = null; // Clear stored context when no data
                }
                else
                {
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{selectedMetricType} - {selectedSubtype2}" : selectedMetricType;

                    // Store data context for potential reload when charts are toggled visible
                    _lastChartDataContext = new ChartDataContext
                    {
                        Data1 = data1,
                        Data2 = null,
                        DisplayName1 = displayName2,
                        DisplayName2 = string.Empty,
                        From = from,
                        To = to
                    };

                    //await UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.SingleMetricStrategy(data1 ?? Enumerable.Empty<HealthMetricData>(), displayName2, from, to), displayName2);
                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartMain, new DataVisualiser.Charts.Strategies.SingleMetricStrategy(data1 ?? Enumerable.Empty<HealthMetricData>(), displayName2, from, to), displayName2, minHeight: 400);

                    // Clear hidden charts
                    ChartHelper.ClearChart(ChartNorm, _chartTimestamps);
                    ChartHelper.ClearChart(ChartDiff, _chartTimestamps);
                    ChartHelper.ClearChart(ChartRatio, _chartTimestamps);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
        {
            ComboBox newCombo = _comboManager.AddSubtypeComboBox();

            if (subtypeList != null)
            {
                foreach (var subtype in subtypeList)
                {
                    newCombo.Items.Add(subtype);
                }
            }

            newCombo.SelectionChanged += OnAnySubtypeSelectionChanged;
            newCombo.SelectedIndex = 0;
            newCombo.IsEnabled = true;
        }

        #endregion

        #region Chart Visibility Toggle Handlers

        /// <summary>
        /// Toggles ChartNorm visibility and reloads data if available.
        /// </summary>
        private async void OnChartNormToggle(object sender, RoutedEventArgs e)
        {
            _isChartNormVisible = !_isChartNormVisible;

            if (_isChartNormVisible)
            {
                ChartNormPanel.Visibility = Visibility.Visible;
                ChartNormToggleButton.Content = "Hide";

                // Reload data if available
                if (_lastChartDataContext != null && _lastChartDataContext.Data1 != null && _lastChartDataContext.Data2 != null)
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

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_lastChartDataContext.Data1, _lastChartDataContext.Data2, _lastChartDataContext.DisplayName1, _lastChartDataContext.DisplayName2, _lastChartDataContext.From, _lastChartDataContext.To, _selectedNormalizationMode), $"{_lastChartDataContext.DisplayName1} ~ {_lastChartDataContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartNormPanel.Visibility = Visibility.Collapsed;
                ChartNormToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartNorm, _chartTimestamps);
            }
        }

        /// <summary>
        /// Toggles Weekly Distribution chart visibility and reloads data if available.
        /// Default state is collapsed — toggle shows it.
        /// </summary>
        private async void OnChartWeeklyToggle(object sender, RoutedEventArgs e)
        {
            _isChartWeeklyVisible = !_isChartWeeklyVisible;

            if (_isChartWeeklyVisible)
            {
                ChartWeeklyPanel.Visibility = Visibility.Visible;
                ChartWeeklyToggleButton.Content = "Hide";

                // reload if we have a stored data context (single metric series required)
                if (_lastChartDataContext != null && _lastChartDataContext.Data1 != null)
                {
                    // we use Data1 for this chart (single-series distribution)
                    var data = _lastChartDataContext.Data1;

                    //await UpdateWeeklyDistributionChartAsync(ChartWeekly, data, _lastChartDataContext.DisplayName1, _lastChartDataContext.From, _lastChartDataContext.To, minHeight: 400);
                    await _weeklyDistributionService.UpdateWeeklyDistributionChartAsync(ChartWeekly, data, _lastChartDataContext.DisplayName1, _lastChartDataContext.From, _lastChartDataContext.To, minHeight: 400);
                }
            }
            else
            {
                ChartWeeklyPanel.Visibility = Visibility.Collapsed;
                ChartWeeklyToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartWeekly, _chartTimestamps);
            }
        }


        /// <summary>
        /// Toggles ChartDiff visibility and reloads data if available.
        /// </summary>
        private async void OnChartDiffToggle(object sender, RoutedEventArgs e)
        {
            _isChartDiffVisible = !_isChartDiffVisible;

            if (_isChartDiffVisible)
            {
                ChartDiffPanel.Visibility = Visibility.Visible;
                ChartDiffToggleButton.Content = "Hide";

                // Reload data if available
                if (_lastChartDataContext != null && _lastChartDataContext.Data1 != null && _lastChartDataContext.Data2 != null)
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

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartDiff, new DataVisualiser.Charts.Strategies.DifferenceStrategy(_lastChartDataContext.Data1, _lastChartDataContext.Data2, _lastChartDataContext.DisplayName1, _lastChartDataContext.DisplayName2, _lastChartDataContext.From, _lastChartDataContext.To), $"{_lastChartDataContext.DisplayName1} - {_lastChartDataContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartDiffPanel.Visibility = Visibility.Collapsed;
                ChartDiffToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartDiff, _chartTimestamps);
            }
        }

        /// <summary>
        /// Toggles ChartRatio visibility and reloads data if available.
        /// </summary>
        private async void OnChartRatioToggle(object sender, RoutedEventArgs e)
        {
            _isChartRatioVisible = !_isChartRatioVisible;

            if (_isChartRatioVisible)
            {
                ChartRatioPanel.Visibility = Visibility.Visible;
                ChartRatioToggleButton.Content = "Hide";

                // Reload data if available
                if (_lastChartDataContext != null && _lastChartDataContext.Data1 != null && _lastChartDataContext.Data2 != null)
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

                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartRatio, new DataVisualiser.Charts.Strategies.RatioStrategy(_lastChartDataContext.Data1, _lastChartDataContext.Data2, _lastChartDataContext.DisplayName1, _lastChartDataContext.DisplayName2, _lastChartDataContext.From, _lastChartDataContext.To), $"{_lastChartDataContext.DisplayName1} / {_lastChartDataContext.DisplayName2}", minHeight: 400);
                }
            }
            else
            {
                ChartRatioPanel.Visibility = Visibility.Collapsed;
                ChartRatioToggleButton.Content = "Show";
                ChartHelper.ClearChart(ChartRatio, _chartTimestamps);
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
            List<ComboBox> active = _comboManager.GetActiveComboBoxes();
            // Get the first dynamic combo (excluding the static SubtypeCombo)
            var SubtypeCombo2 = active.FirstOrDefault(cb => cb != SubtypeCombo);

            string[] titles = ChartHelper.GetChartTitlesFromCombos(TablesCombo, SubtypeCombo, SubtypeCombo2);
            string display1 = titles.Length > 0 ? titles[0] : string.Empty;
            string display2 = titles.Length > 1 ? titles[1] : string.Empty;

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
                    _selectedNormalizationMode = NormalizationMode.ZeroToOne;
                else if (NormPercentOfMaxRadio.IsChecked == true)
                    _selectedNormalizationMode = NormalizationMode.PercentageOfMax;
                else if (NormRelativeToMaxRadio.IsChecked == true)
                    _selectedNormalizationMode = NormalizationMode.RelativeToMax;

                // If Norm chart visible and we have a stored data context, refresh only Normalized chart
                if (_isChartNormVisible && _lastChartDataContext != null && _lastChartDataContext.Data1 != null && _lastChartDataContext.Data2 != null)
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


                    await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, new DataVisualiser.Charts.Strategies.NormalizedStrategy(_lastChartDataContext.Data1, _lastChartDataContext.Data2, _lastChartDataContext.DisplayName1, _lastChartDataContext.DisplayName2, _lastChartDataContext.From, _lastChartDataContext.To), $"{_lastChartDataContext.DisplayName1} ~ {_lastChartDataContext.DisplayName2}", minHeight: 400);
            }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Normalization mode change error: {ex.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Handles window closing to dispose of resources.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _tooltipManager?.Dispose();
            base.OnClosed(e);
        }
    }
}
