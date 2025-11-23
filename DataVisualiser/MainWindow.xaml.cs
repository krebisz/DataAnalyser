using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DataVisualiser.Class;
using DataVisualiser.Helper;
using System.Xml.Linq;

namespace DataVisualiser
{
    public partial class MainWindow : Window
    {
        private readonly string _connectionString;
        private List<HealthMetricData> _currentData = new();
        private List<HealthMetricData> _currentData2 = new();
        private bool _isLoadingMetricTypes = false;
        private bool _isLoadingSubtypes = false;

        // --- Hover / coordinated-line state ---
        private Popup? _sharedHoverPopup;
        private TextBlock? _hoverTimestampText;
        private TextBlock? _hoverMainText;
        private TextBlock? _hoverDiffText;
        private TextBlock? _hoverRatioText;

        // Keep axis vertical line sections so we can remove/move them
        private AxisSection? _verticalLineMain;
        private AxisSection? _verticalLineDiff;
        private AxisSection? _verticalLineRatio;

        // Map charts to their X-axis timestamp lists (index -> DateTime)
        private readonly Dictionary<CartesianChart, List<DateTime>> _chartTimestamps = new();

        public MainWindow()
        {
            InitializeComponent();

            _connectionString = ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";

            FromDate.SelectedDate = DateTime.UtcNow.AddDays(-30);
            ToDate.SelectedDate = DateTime.UtcNow;

            // Initialize resolution dropdown
            ResolutionCombo.Items.Add("All");
            ResolutionCombo.Items.Add("Hourly");
            ResolutionCombo.Items.Add("Daily");
            ResolutionCombo.Items.Add("Weekly");
            ResolutionCombo.Items.Add("Monthly");
            ResolutionCombo.Items.Add("Yearly");
            ResolutionCombo.SelectedItem = "All"; // Default selection

            LoadMetricTypes();

            ChartMain.Zoom = ZoomingOptions.X;
            ChartMain.Pan = PanningOptions.X;

            // Initialize ChartDiff and ChartRatio so zoom/pan work and chart properties are consistent
            ChartDiff.Zoom = ZoomingOptions.X;
            ChartDiff.Pan = PanningOptions.X;

            ChartRatio.Zoom = ZoomingOptions.X;
            ChartRatio.Pan = PanningOptions.X;

            // Ensure titles reflect initial selections (may be empty until types/subtypes loaded)
            UpdateChartTitlesFromCombos();

            // Create the shared hover popup and wire events
            CreateSharedHoverPopup();

            // Wire hover events on charts (LiveCharts DataHover provides ChartPoint)
            ChartMain.DataHover += OnChartDataHover;
            ChartDiff.DataHover += OnChartDataHover;
            ChartRatio.DataHover += OnChartDataHover;

            ChartMain.MouseLeave += OnChartMouseLeave;
            ChartDiff.MouseLeave += OnChartMouseLeave;
            ChartRatio.MouseLeave += OnChartMouseLeave;
        }

        // Create a small popup to act as the shared toolbar showing timest amp + values
        private void CreateSharedHoverPopup()
        {
            _hoverTimestampText = ChartHelper.SetHoverText(true);
            _hoverMainText = ChartHelper.SetHoverText();
            _hoverDiffText = ChartHelper.SetHoverText();
            _hoverRatioText = ChartHelper.SetHoverText();

            StackPanel stack = CreateStackPanel();
            Border border = ChartHelper.CreateBorder(stack);
            _sharedHoverPopup = CreatePopUp(border);
        }

        public Popup CreatePopUp(Border border)
        {
            var popup = new Popup
            {
                Child = border,
                Placement = PlacementMode.Mouse,
                StaysOpen = true,
                AllowsTransparency = true,
                PlacementTarget = this // Attach to the window
            };

            return popup;
        }


        public StackPanel CreateStackPanel()
        {
            _hoverTimestampText = ChartHelper.SetHoverText(true);
            _hoverMainText = ChartHelper.SetHoverText();
            _hoverDiffText = ChartHelper.SetHoverText();
            _hoverRatioText = ChartHelper.SetHoverText();

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Children =
                {
                    _hoverTimestampText,
                    _hoverMainText,
                    _hoverDiffText,
                    _hoverRatioText
                }
            };

            return stack;
        }

        // Called when LiveCharts reports DataHover on any chart
        private void OnChartDataHover(ChartPoint chartPoint)
        {
            if (chartPoint == null) return;

            // Round X to nearest integer index used by our charts
            int index = (int)Math.Round(chartPoint.X);

            // Determine and format timestamp (prefer ChartMain timestamps if available)
            string timestampText = GetTimestampTextForIndex(index);

            // Gather values from each chart at the same index
            string mainValues = ChartHelper.GetChartValuesAtIndex(ChartMain, index);
            string diffValues = ChartHelper.GetChartValuesAtIndex(ChartDiff, index);
            string ratioValues = ChartHelper.GetChartValuesAtIndex(ChartRatio, index);

            if (_hoverTimestampText != null) _hoverTimestampText.Text = timestampText;
            if (_hoverMainText != null) _hoverMainText.Text = $"Main: {mainValues}";
            if (_hoverDiffText != null) _hoverDiffText.Text = $"Diff: {diffValues}";
            if (_hoverRatioText != null) _hoverRatioText.Text = $"Ratio: {ratioValues}";

            if (_sharedHoverPopup != null)
            {
                if (!_sharedHoverPopup.IsOpen) _sharedHoverPopup.IsOpen = true;
                // Force popup to update position when mouse moves
                _sharedHoverPopup.HorizontalOffset = 0;
                _sharedHoverPopup.VerticalOffset = 0;
                _sharedHoverPopup.HorizontalOffset = 10; // Small offset to avoid cursor
                _sharedHoverPopup.VerticalOffset = 10;
            }

            // Add/move vertical black line for each chart at same index
            ChartHelper.UpdateVerticalLineForChart(ref ChartMain, index, ref _verticalLineMain);
            ChartHelper.UpdateVerticalLineForChart(ref ChartDiff, index, ref _verticalLineDiff);
            ChartHelper.UpdateVerticalLineForChart(ref ChartRatio, index, ref _verticalLineRatio);
        }

        private void OnChartMouseLeave(object? sender, System.Windows.Input.MouseEventArgs e)
        {
            ClearHoverVisuals();
        }

        // Clear popup and remove vertical lines
        private void ClearHoverVisuals()
        {
            if (_sharedHoverPopup != null && _sharedHoverPopup.IsOpen) _sharedHoverPopup.IsOpen = false;

            ChartHelper.RemoveAxisSection(ref ChartMain, _verticalLineMain);
            ChartHelper.RemoveAxisSection(ref ChartDiff, _verticalLineDiff);
            ChartHelper.RemoveAxisSection(ref ChartRatio, _verticalLineRatio);
        }

        private string GetTimestampTextForIndex(int index)
        {
            DateTime? ts = null;
            if (_chartTimestamps.TryGetValue(ChartMain, out var listMain) && index >= 0 && index < listMain.Count)
                ts = listMain[index];
            else if (_chartTimestamps.TryGetValue(ChartDiff, out var listDiff) && index >= 0 && index < listDiff.Count)
                ts = listDiff[index];
            else if (_chartTimestamps.TryGetValue(ChartRatio, out var listRatio) && index >= 0 && index < listRatio.Count)
                ts = listRatio[index];

            if (ts.HasValue)
            {
                // Show a human friendly timestamp including date + time
                return ts.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return "Timestamp: N/A";
        }

        /// <summary>
        /// Event handler for Resolution selection change - reloads MetricTypes from the selected table.
        /// </summary>
        private void OnResolutionSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ResolutionCombo.SelectedItem == null)
                return;

            // Clear current selections
            TablesCombo.Items.Clear();
            SubtypeCombo.Items.Clear();
            SubtypeCombo.IsEnabled = false;
            SubtypeCombo2.Items.Clear();
            SubtypeCombo2.IsEnabled = false;

            // Reload MetricTypes from the newly selected table
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
                var baseMetricTypes = await dataFetcher.GetBaseMetricTypes(tableName);

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
                    // Clear subtype dropdown if no metric types
                    SubtypeCombo.Items.Clear();
                    SubtypeCombo.IsEnabled = false;
                    SubtypeCombo2.Items.Clear();
                    SubtypeCombo2.IsEnabled = false;
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
        /// Event handler for MetricSubtype selection change - updates date range to match data availability.
        /// </summary>
        private async void OnSubtypeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadDateRangeForSelectedMetrics();
        }

        /// <summary>
        /// Event handler for second MetricSubtype selection change - updates date range (recalculate) if needed.
        /// </summary>
        private async void OnSubtype2SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadDateRangeForSelectedMetrics();
        }

        private async Task LoadDateRangeForSelectedMetrics()
        {
            if (_isLoadingSubtypes || _isLoadingMetricTypes)
                return;

            // Recalculate date range for primary selection. This keeps behavior predictable.
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
                SubtypeCombo2.Items.Clear();
                SubtypeCombo2.IsEnabled = false;
                return;
            }

            var selectedMetricType = TablesCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectedMetricType))
            {
                SubtypeCombo.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                SubtypeCombo2.Items.Clear();
                SubtypeCombo2.IsEnabled = false;
                return;
            }

            try
            {
                _isLoadingSubtypes = true;
                var tableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
                var dataFetcher = new DataFetcher(_connectionString);
                var subtypes = await dataFetcher.GetSubtypesForBaseType(selectedMetricType, tableName);

                SubtypeCombo.Items.Clear();
                SubtypeCombo2.Items.Clear();
                var subtypeList = subtypes.ToList();

                if (subtypeList.Count > 0)
                {
                    // Add an option for "All" or "None" to show data without subtype filtering
                    SubtypeCombo.Items.Add("(All)");
                    SubtypeCombo2.Items.Add("(All)");
                    foreach (var subtype in subtypeList)
                    {
                        SubtypeCombo.Items.Add(subtype);
                        SubtypeCombo2.Items.Add(subtype);
                    }
                    SubtypeCombo.IsEnabled = true;
                    SubtypeCombo2.IsEnabled = true;
                    SubtypeCombo.SelectedIndex = 0; // Select "(All)" by default
                    SubtypeCombo2.SelectedIndex = 0;
                }
                else
                {
                    // No subtypes available for this metric type
                    SubtypeCombo.IsEnabled = false;
                    SubtypeCombo2.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading subtypes for {selectedMetricType}: {ex.Message}");
                SubtypeCombo.Items.Clear();
                SubtypeCombo2.Items.Clear();
                SubtypeCombo.IsEnabled = false;
                SubtypeCombo2.IsEnabled = false;
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

            // Get selected subtype (if any) - uses first subtype combo as the primary reference
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
                var dateRange = await dataFetcher.GetBaseTypeDateRange(selectedMetricType, selectedSubtype, tableName);

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

            var selectedMetricType = TablesCombo.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectedMetricType))
            {
                MessageBox.Show("Please select a valid MetricType", "Invalid Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected subtype for chart 1 (if any)
            string? selectedSubtype = null;
            if (SubtypeCombo.IsEnabled && SubtypeCombo.SelectedItem != null)
            {
                var subtypeValue = SubtypeCombo.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(subtypeValue) && subtypeValue != "(All)")
                {
                    selectedSubtype = subtypeValue;
                }
            }

            // Get selected subtype for chart 2 (if any)
            string? selectedSubtype2 = null;
            if (SubtypeCombo2.IsEnabled && SubtypeCombo2.SelectedItem != null)
            {
                var subtypeValue2 = SubtypeCombo2.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(subtypeValue2) && subtypeValue2 != "(All)")
                {
                    selectedSubtype2 = subtypeValue2;
                }
            }

            // Update titles immediately based on selection (use base metric when subtype is null)
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

                // Update Chart 1 as before (single dataset view)
                if (data1 == null || !data1.Any())
                {
                    var subtypeText = !string.IsNullOrEmpty(selectedSubtype) ? $" and Subtype '{selectedSubtype}'" : "";
                    MessageBox.Show($"No data found for MetricType '{selectedMetricType}'{subtypeText} in the selected date range.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _currentData = data1.ToList();
                    var displayName = !string.IsNullOrEmpty(selectedSubtype) ? $"{selectedMetricType} - {selectedSubtype}" : selectedMetricType;
                }

                // For ChartMain, ChartDiff and ChartRatio: if both datasets exist, draw combined + diff + ratio; otherwise fallback
                if ((data1 != null && data1.Any()) && (data2 != null && data2.Any()))
                {
                    _currentData2 = data2.ToList();
                    var displayName1 = !string.IsNullOrEmpty(selectedSubtype) ? $"{selectedMetricType} - {selectedSubtype}" : selectedMetricType;
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2) ? $"{selectedMetricType} - {selectedSubtype2}" : selectedMetricType;

                    await UpdateCombinedChartAsync(ChartMain, data1, data2, displayName1, displayName2, from, to);

                    // Update difference chart (raw + smoothed)
                    await UpdateDifferenceChartAsync(ChartDiff, data1, data2, displayName1, displayName2, from, to);

                    // Update ratio chart (raw ratio and smoothed ratio)
                    await UpdateRatioChartAsync(ChartRatio, data1, data2, displayName1, displayName2, from, to);
                }
                else if (data2 == null || !data2.Any())
                {
                    var subtypeText2 = !string.IsNullOrEmpty(selectedSubtype2) ? $" and Subtype '{selectedSubtype2}'" : "";
                    MessageBox.Show($"No data found for MetricType '{selectedMetricType}'{subtypeText2} in the selected date range (Chart 2).", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
                    ChartMain.Series.Clear();
                    ChartDiff.Series.Clear();
                    ChartRatio.Series.Clear();

                    // Remove timestamp mappings for cleared charts
                    _chartTimestamps.Remove(ChartMain);
                    _chartTimestamps.Remove(ChartDiff);
                    _chartTimestamps.Remove(ChartRatio);
                }
                else
                {
                    // Only data2 exists — single dataset rendering for ChartMain; clear diff & ratio
                    _currentData2 = data2.ToList();
                    var displayName2 = !string.IsNullOrEmpty(selectedSubtype2)
                        ? $"{selectedMetricType} - {selectedSubtype2}"
                        : selectedMetricType;

                    await UpdateChartForAsync(ChartMain, _currentData2, displayName2, from, to);
                    ChartDiff.Series.Clear();
                    ChartRatio.Series.Clear();

                    _chartTimestamps.Remove(ChartDiff);
                    _chartTimestamps.Remove(ChartRatio);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Resets zoom and pan to show all data for all charts
        /// </summary>
        private void OnResetZoom(object sender, RoutedEventArgs e)
        {
            // Reset ChartMain X axis
            if (ChartMain != null && ChartMain.AxisX.Count > 0)
            {
                ChartMain.AxisX[0].MinValue = double.NaN;
                ChartMain.AxisX[0].MaxValue = double.NaN;
            }

            // Reset difference chart X axis
            if (ChartDiff != null && ChartDiff.AxisX.Count > 0)
            {
                ChartDiff.AxisX[0].MinValue = double.NaN;
                ChartDiff.AxisX[0].MaxValue = double.NaN;
            }

            // Reset ratio chart X axis
            if (ChartRatio != null && ChartRatio.AxisX.Count > 0)
            {
                ChartRatio.AxisX[0].MinValue = double.NaN;
                ChartRatio.AxisX[0].MaxValue = double.NaN;
            }
        }


        /// <summary>
        /// Updates the given chart with the retrieved data (async version to prevent UI freezing).
        /// This is a copy of existing chart update logic adapted to operate on an arbitrary CartesianChart.
        /// </summary>
        private async Task UpdateChartForAsync(CartesianChart targetChart, IEnumerable<HealthMetricData> data, string metricType, DateTime fromDate, DateTime toDate)
        {
            if (data == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            var loadingTask = Task.Run(() =>
            {
                var orderedData = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

                if (!orderedData.Any())
                    return null;

                var dateRange = toDate - fromDate;
                var tickInterval = MathHelper.DetermineTickInterval(dateRange);
                var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();

                // Generate normalized intervals for X-axis
                var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(fromDate, toDate, tickInterval);

                // Map each data point to its normalized interval index
                var intervalIndices = rawTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

                var smoothedData = MathHelper.CreateSmoothedData(orderedData, fromDate, toDate);
                var smoothedValues = MathHelper.InterpolateSmoothedData(smoothedData, rawTimestamps);

                return new
                {
                    OrderedData = orderedData,
                    RawTimestamps = rawTimestamps,
                    IntervalIndices = intervalIndices,
                    NormalizedIntervals = normalizedIntervals,
                    SmoothedValues = smoothedValues,
                    TickInterval = tickInterval,
                    DateRange = dateRange,
                    MetricType = metricType
                };
            });

            var chartData = await loadingTask;

            if (chartData == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            try
            {
                targetChart.Series.Clear();

                var orderedData = chartData.OrderedData;
                var rawTimestamps = chartData.RawTimestamps;
                var intervalIndices = chartData.IntervalIndices;
                var normalizedIntervals = chartData.NormalizedIntervals;
                var smoothedValues = chartData.SmoothedValues;
                var tickInterval = chartData.TickInterval;
                var dateRange = chartData.DateRange;


                var smoothedSeries = ChartHelper.CreateLineSeries($"{metricType} (Smoothed)", 5, 2, Colors.Blue);

                foreach (var value in smoothedValues)
                {
                    smoothedSeries.Values.Add(value);
                }

                var rawSeries = ChartHelper.CreateLineSeries($"{metricType} (Raw)", 3, 1, Colors.DarkGray);

                foreach (var point in orderedData)
                {
                    rawSeries.Values.Add(point.Value.HasValue ? (double)point.Value.Value : double.NaN);
                }

                AddSeriesToChart(targetChart, rawSeries);
                AddSeriesToChart(targetChart, smoothedSeries);

                // Store timestamps mapping for this chart (index -> DateTime)
                _chartTimestamps[targetChart] = rawTimestamps;

                if (targetChart.AxisX.Count > 0)
                {
                    var xAxis = targetChart.AxisX[0];
                    xAxis.Title = "Time";

                    // Pre-calculate which data point indices should show labels (first occurrence of each interval)
                    var labelDataPointIndices = new HashSet<int>();
                    var seenIntervals = new HashSet<int>();
                    for (int i = 0; i < intervalIndices.Count; i++)
                    {
                        int intervalIndex = intervalIndices[i];
                        if (!seenIntervals.Contains(intervalIndex))
                        {
                            labelDataPointIndices.Add(i);
                            seenIntervals.Add(intervalIndex);
                        }
                    }

                    xAxis.LabelFormatter = value =>
                    {
                        try
                        {
                            double roundedValue = MathHelper.RoundToThreeSignificantDigits(value);
                            int dataPointIndex = (int)Math.Round(roundedValue);
                            if (dataPointIndex >= 0 &&
                                dataPointIndex < intervalIndices.Count &&
                                labelDataPointIndices.Contains(dataPointIndex))
                            {
                                int intervalIndex = intervalIndices[dataPointIndex];
                                if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                                {
                                    return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                                }
                            }
                            return string.Empty;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    };

                    var intervalsToShow = tickInterval switch
                    {
                        TickInterval.Month => Math.Max(6, Math.Min(12, normalizedIntervals.Count)),
                        TickInterval.Week => Math.Max(4, Math.Min(8, normalizedIntervals.Count)),
                        TickInterval.Day => Math.Max(7, Math.Min(14, normalizedIntervals.Count)),
                        TickInterval.Hour => Math.Max(12, Math.Min(24, normalizedIntervals.Count)),
                        _ => Math.Min(10, normalizedIntervals.Count)
                    };

                    var sortedLabelIndices = labelDataPointIndices.OrderBy(x => x).ToList();
                    double step = 1.0;

                    if (sortedLabelIndices.Count > intervalsToShow && sortedLabelIndices.Count > 1)
                    {
                        var totalSpacing = sortedLabelIndices.Last() - sortedLabelIndices.First();
                        var averageSpacing = totalSpacing / (double)(sortedLabelIndices.Count - 1);
                        step = Math.Max(1.0, Math.Ceiling(averageSpacing * (sortedLabelIndices.Count / (double)intervalsToShow)));
                    }
                    else if (sortedLabelIndices.Count > 0)
                    {
                        if (sortedLabelIndices.Count > 1)
                        {
                            var minSpacing = sortedLabelIndices
                                .Zip(sortedLabelIndices.Skip(1), (a, b) => b - a)
                                .Where(s => s > 0)
                                .DefaultIfEmpty(1)
                                .Min();
                            step = Math.Max(1.0, minSpacing);
                        }
                    }

                    step = MathHelper.RoundToThreeSignificantDigits(step);
                    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                    xAxis.Labels = null;
                    xAxis.MinValue = double.NaN;
                    xAxis.MaxValue = double.NaN;
                }

                var unit = orderedData.FirstOrDefault()?.Unit;
                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    yAxis.Title = !string.IsNullOrEmpty(unit) ? $"Value ({unit})" : "Value";

                    try
                    {
                        NormalizeYAxis(yAxis, orderedData, smoothedValues);
                    }
                    catch (Exception yAxisEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Y-axis normalization failed: {yAxisEx.Message}");
                        var allValues = orderedData.Where(d => d.Value.HasValue).Select(d => (double)d.Value!.Value).ToList();

                        if (allValues.Any())
                        {
                            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(allValues.Min());
                            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(allValues.Max());
                            // provide a default step so around 10 ticks are shown
                            var fallbackStep = MathHelper.RoundToThreeSignificantDigits((allValues.Max() - allValues.Min()) / 10.0);
                            if (fallbackStep > 0 && !double.IsNaN(fallbackStep) && !double.IsInfinity(fallbackStep))
                                yAxis.Separator = new LiveCharts.Wpf.Separator { Step = fallbackStep };
                            yAxis.ShowLabels = true;
                            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                        }
                    }

                    // Adjust chart height based on Y-axis tick count
                    double minHeight = targetChart == ChartMain ? 500.0 : 400.0;
                    AdjustChartHeightBasedOnYAxis(targetChart, minHeight);

                    if (targetChart.DataTooltip == null)
                    {
                        targetChart.DataTooltip = new DefaultTooltip();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Chart update error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddSeriesToChart(CartesianChart chart, LineSeries series)
        {
            chart.Series.Add(series);
        }


        /// <summary>
        /// Updates ChartMain with both datasets on a single shared axis.
        /// Both series are aligned to a combined timestamp axis and Y-axis is normalized across both datasets.
        /// </summary>
        private async Task UpdateCombinedChartAsync(CartesianChart targetChart, IEnumerable<HealthMetricData> data1, IEnumerable<HealthMetricData> data2, string label1, string label2, DateTime fromDate, DateTime toDate)
        {
            if (data1 == null || data2 == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            var loadingTask = Task.Run(() =>
            {
                var ordered1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
                var ordered2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

                if (!ordered1.Any() && !ordered2.Any())
                    return null;

                var dateRange = toDate - fromDate;
                var tickInterval = MathHelper.DetermineTickInterval(dateRange);

                // Combined sorted unique timestamps from both datasets
                var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
                    .Concat(ordered2.Select(d => d.NormalizedTimestamp))
                    .Distinct()
                    .OrderBy(dt => dt)
                    .ToList();

                if (!combinedTimestamps.Any())
                    return null;

                var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(fromDate, toDate, tickInterval);

                // Map each combined timestamp to interval index
                var intervalIndices = combinedTimestamps.Select(ts =>
                    MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

                // Smoothed and interpolated values per dataset aligned to combined timestamps
                var smoothed1 = MathHelper.CreateSmoothedData(ordered1, fromDate, toDate);
                var smoothed2 = MathHelper.CreateSmoothedData(ordered2, fromDate, toDate);

                var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
                var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

                // Build raw values aligned to combined timestamps (missing -> NaN)
                // Group by timestamp and take the first value if duplicates exist to avoid ToDictionary exception
                var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);
                var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);

                var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
                var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();

                return new
                {
                    Ordered1 = ordered1,
                    Ordered2 = ordered2,
                    CombinedTimestamps = combinedTimestamps,
                    IntervalIndices = intervalIndices,
                    NormalizedIntervals = normalizedIntervals,
                    InterpSmoothed1 = interpSmoothed1,
                    InterpSmoothed2 = interpSmoothed2,
                    RawValues1 = rawValues1,
                    RawValues2 = rawValues2,
                    TickInterval = tickInterval,
                    DateRange = dateRange,
                    Label1 = label1,
                    Label2 = label2
                };
            });

            var chartData = await loadingTask;

            if (chartData == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            try
            {
                targetChart.Series.Clear();

                var combinedTimestamps = chartData.CombinedTimestamps;
                var intervalIndices = chartData.IntervalIndices;
                var normalizedIntervals = chartData.NormalizedIntervals;
                var interpSmoothed1 = chartData.InterpSmoothed1;
                var interpSmoothed2 = chartData.InterpSmoothed2;
                var rawValues1 = chartData.RawValues1;
                var rawValues2 = chartData.RawValues2;
                var tickInterval = chartData.TickInterval;

                var smoothedSeries1 = ChartHelper.CreateLineSeries($"{chartData.Label1} (Smoothed)", 5, 2, Colors.Blue);

                foreach (var v in interpSmoothed1) smoothedSeries1.Values.Add(v);

                var rawSeries1 = ChartHelper.CreateLineSeries($"{chartData.Label1} (Raw)", 3, 1, Colors.DarkGray);

                foreach (var v in rawValues1) rawSeries1.Values.Add(v);

                var smoothedSeries2 = ChartHelper.CreateLineSeries($"{chartData.Label2} (Smoothed)", 5, 2, Colors.Red);

                foreach (var v in interpSmoothed2) smoothedSeries2.Values.Add(v);

                var rawSeries2 = ChartHelper.CreateLineSeries($"{chartData.Label2} (Raw)", 3, 1, Colors.Orange);

                foreach (var v in rawValues2) rawSeries2.Values.Add(v);

                // Add series to chart in a sensible order (smoothed then raw)
                targetChart.Series.Add(smoothedSeries1);
                targetChart.Series.Add(rawSeries1);
                targetChart.Series.Add(smoothedSeries2);
                targetChart.Series.Add(rawSeries2);

                // Store combined timestamps mapping for this chart
                _chartTimestamps[targetChart] = combinedTimestamps;

                if (targetChart.AxisX.Count > 0)
                {
                    var xAxis = targetChart.AxisX[0];
                    xAxis.Title = "Time";

                    // Pre-calculate which data point indices should show labels (first occurrence of each interval)
                    var labelDataPointIndices = new HashSet<int>();
                    var seenIntervals = new HashSet<int>();
                    for (int i = 0; i < intervalIndices.Count; i++)
                    {
                        int intervalIndex = intervalIndices[i];
                        if (!seenIntervals.Contains(intervalIndex))
                        {
                            labelDataPointIndices.Add(i);
                            seenIntervals.Add(intervalIndex);
                        }
                    }

                    xAxis.LabelFormatter = value =>
                    {
                        try
                        {
                            double roundedValue = MathHelper.RoundToThreeSignificantDigits(value);
                            int dataPointIndex = (int)Math.Round(roundedValue);
                            if (dataPointIndex >= 0 &&
                                dataPointIndex < intervalIndices.Count &&
                                labelDataPointIndices.Contains(dataPointIndex))
                            {
                                int intervalIndex = intervalIndices[dataPointIndex];
                                if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                                {
                                    return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                                }
                            }
                            return string.Empty;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    };

                    var intervalsToShow = tickInterval switch
                    {
                        TickInterval.Month => Math.Max(6, Math.Min(12, normalizedIntervals.Count)),
                        TickInterval.Week => Math.Max(4, Math.Min(8, normalizedIntervals.Count)),
                        TickInterval.Day => Math.Max(7, Math.Min(14, normalizedIntervals.Count)),
                        TickInterval.Hour => Math.Max(12, Math.Min(24, normalizedIntervals.Count)),
                        _ => Math.Min(10, normalizedIntervals.Count)
                    };

                    var sortedLabelIndices = labelDataPointIndices.OrderBy(x => x).ToList();
                    double step = 1.0;

                    if (sortedLabelIndices.Count > intervalsToShow && sortedLabelIndices.Count > 1)
                    {
                        var totalSpacing = sortedLabelIndices.Last() - sortedLabelIndices.First();
                        var averageSpacing = totalSpacing / (double)(sortedLabelIndices.Count - 1);
                        step = Math.Max(1.0, Math.Ceiling(averageSpacing * (sortedLabelIndices.Count / (double)intervalsToShow)));
                    }
                    else if (sortedLabelIndices.Count > 0)
                    {
                        if (sortedLabelIndices.Count > 1)
                        {
                            var minSpacing = sortedLabelIndices
                                .Zip(sortedLabelIndices.Skip(1), (a, b) => b - a)
                                .Where(s => s > 0)
                                .DefaultIfEmpty(1)
                                .Min();
                            step = Math.Max(1.0, minSpacing);
                        }
                    }

                    step = MathHelper.RoundToThreeSignificantDigits(step);
                    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                    xAxis.Labels = null;
                    xAxis.MinValue = double.NaN;
                    xAxis.MaxValue = double.NaN;
                }

                // Normalize Y-axis across both datasets: combine raw points and smoothed values
                var combinedRawData = chartData.Ordered1.Concat(chartData.Ordered2).ToList();
                var combinedSmoothed = interpSmoothed1.Concat(interpSmoothed2).ToList();

                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    var unit = combinedRawData.FirstOrDefault()?.Unit;
                    yAxis.Title = !string.IsNullOrEmpty(unit) ? $"Value ({unit})" : "Value";

                    try
                    {
                        NormalizeYAxis(yAxis, combinedRawData, combinedSmoothed);
                    }
                    catch (Exception yAxisEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Y-axis normalization failed: {yAxisEx.Message}");
                        var allValues = combinedRawData.Where(d => d.Value.HasValue).Select(d => (double)d.Value!.Value).ToList();
                        allValues.AddRange(combinedSmoothed.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)));

                        if (allValues.Any())
                        {
                            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(allValues.Min());
                            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(allValues.Max());
                            var fallbackStep = MathHelper.RoundToThreeSignificantDigits((allValues.Max() - allValues.Min()) / 10.0);
                            if (fallbackStep > 0 && !double.IsNaN(fallbackStep) && !double.IsInfinity(fallbackStep))
                                yAxis.Separator = new LiveCharts.Wpf.Separator { Step = fallbackStep };
                            yAxis.ShowLabels = true;
                            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                        }
                    }

                    // Adjust chart height based on Y-axis tick count
                    double minHeight = targetChart == ChartMain ? 500.0 : 400.0;
                    AdjustChartHeightBasedOnYAxis(targetChart, minHeight);

                    if (targetChart.DataTooltip == null)
                    {
                        targetChart.DataTooltip = new DefaultTooltip();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Combined chart update error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating combined chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates a chart showing the difference between two datasets (data1 - data2).
        /// Produces a smoothed and raw difference series aligned to the combined timestamps.
        /// </summary>
        private async Task UpdateDifferenceChartAsync(CartesianChart targetChart, IEnumerable<HealthMetricData> data1, IEnumerable<HealthMetricData> data2, string label1, string label2, DateTime fromDate, DateTime toDate)
        {
            if (data1 == null || data2 == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            var loadingTask = Task.Run(() =>
            {
                var ordered1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
                var ordered2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

                if (!ordered1.Any() && !ordered2.Any())
                    return null;

                var dateRange = toDate - fromDate;
                var tickInterval = MathHelper.DetermineTickInterval(dateRange);

                // Combined sorted unique timestamps from both datasets
                var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
                    .Concat(ordered2.Select(d => d.NormalizedTimestamp))
                    .Distinct()
                    .OrderBy(dt => dt)
                    .ToList();

                if (!combinedTimestamps.Any())
                    return null;

                var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(fromDate, toDate, tickInterval);

                // Map each combined timestamp to interval index
                var intervalIndices = combinedTimestamps.Select(ts => MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

                // Smoothed and interpolated values per dataset aligned to combined timestamps
                var smoothed1 = MathHelper.CreateSmoothedData(ordered1, fromDate, toDate);
                var smoothed2 = MathHelper.CreateSmoothedData(ordered2, fromDate, toDate);

                var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
                var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

                // Build raw values aligned to combined timestamps (missing -> NaN)
                // Group by timestamp and take the first value if duplicates exist to avoid ToDictionary exception
                var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);
                var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);

                var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
                var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();

                // Differences (raw and smoothed)
                var rawDiffs = MathHelper.ReturnValueDifferences(rawValues1, rawValues2);
                var smoothedDiffs = MathHelper.ReturnValueDifferences(interpSmoothed1, interpSmoothed2);

                return new
                {
                    CombinedTimestamps = combinedTimestamps,
                    IntervalIndices = intervalIndices,
                    NormalizedIntervals = normalizedIntervals,
                    RawDiffs = rawDiffs,
                    SmoothedDiffs = smoothedDiffs,
                    TickInterval = tickInterval,
                    DateRange = dateRange,
                    Unit = ordered1.FirstOrDefault()?.Unit ?? ordered2.FirstOrDefault()?.Unit
                };
            });

            var chartData = await loadingTask;

            if (chartData == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            try
            {
                targetChart.Series.Clear();

                var combinedTimestamps = chartData.CombinedTimestamps;
                var intervalIndices = chartData.IntervalIndices;
                var normalizedIntervals = chartData.NormalizedIntervals;
                var rawDiffs = chartData.RawDiffs;
                var smoothedDiffs = chartData.SmoothedDiffs;
                var tickInterval = chartData.TickInterval;

                var smoothedSeries = ChartHelper.CreateLineSeries($"Difference (Smoothed)", 5, 2, Colors.Purple);

                if (smoothedDiffs != null)
                {
                    foreach (var v in smoothedDiffs) smoothedSeries.Values.Add(v);
                }


                var rawSeries = ChartHelper.CreateLineSeries($"Difference (Raw)", 3, 1, Colors.DarkGray);

                if (rawDiffs != null)
                {
                    foreach (var v in rawDiffs) rawSeries.Values.Add(v);
                }

                targetChart.Series.Add(smoothedSeries);
                targetChart.Series.Add(rawSeries);

                // store timestamps mapping for this chart
                _chartTimestamps[targetChart] = combinedTimestamps;

                if (targetChart.AxisX.Count > 0)
                {
                    var xAxis = targetChart.AxisX[0];
                    xAxis.Title = "Time";

                    // Pre-calculate which data point indices should show labels (first occurrence of each interval)
                    var labelDataPointIndices = new HashSet<int>();
                    var seenIntervals = new HashSet<int>();
                    for (int i = 0; i < intervalIndices.Count; i++)
                    {
                        int intervalIndex = intervalIndices[i];
                        if (!seenIntervals.Contains(intervalIndex))
                        {
                            labelDataPointIndices.Add(i);
                            seenIntervals.Add(intervalIndex);
                        }
                    }

                    xAxis.LabelFormatter = value =>
                    {
                        try
                        {
                            double roundedValue = MathHelper.RoundToThreeSignificantDigits(value);
                            int dataPointIndex = (int)Math.Round(roundedValue);
                            if (dataPointIndex >= 0 &&
                                dataPointIndex < intervalIndices.Count &&
                                labelDataPointIndices.Contains(dataPointIndex))
                            {
                                int intervalIndex = intervalIndices[dataPointIndex];
                                if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                                {
                                    return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                                }
                            }
                            return string.Empty;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    };

                    var intervalsToShow = tickInterval switch
                    {
                        TickInterval.Month => Math.Max(6, Math.Min(12, normalizedIntervals.Count)),
                        TickInterval.Week => Math.Max(4, Math.Min(8, normalizedIntervals.Count)),
                        TickInterval.Day => Math.Max(7, Math.Min(14, normalizedIntervals.Count)),
                        TickInterval.Hour => Math.Max(12, Math.Min(24, normalizedIntervals.Count)),
                        _ => Math.Min(10, normalizedIntervals.Count)
                    };

                    var sortedLabelIndices = labelDataPointIndices.OrderBy(x => x).ToList();
                    double step = 1.0;

                    if (sortedLabelIndices.Count > intervalsToShow && sortedLabelIndices.Count > 1)
                    {
                        var totalSpacing = sortedLabelIndices.Last() - sortedLabelIndices.First();
                        var averageSpacing = totalSpacing / (double)(sortedLabelIndices.Count - 1);
                        step = Math.Max(1.0, Math.Ceiling(averageSpacing * (sortedLabelIndices.Count / (double)intervalsToShow)));
                    }
                    else if (sortedLabelIndices.Count > 0)
                    {
                        if (sortedLabelIndices.Count > 1)
                        {
                            var minSpacing = sortedLabelIndices
                                .Zip(sortedLabelIndices.Skip(1), (a, b) => b - a)
                                .Where(s => s > 0)
                                .DefaultIfEmpty(1)
                                .Min();
                            step = Math.Max(1.0, minSpacing);
                        }
                    }

                    step = MathHelper.RoundToThreeSignificantDigits(step);
                    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                    xAxis.Labels = null;
                    xAxis.MinValue = double.NaN;
                    xAxis.MaxValue = double.NaN;
                }

                // Build synthetic rawData list for axis normalization function
                var syntheticRawData = new List<HealthMetricData>();
                for (int i = 0; i < combinedTimestamps.Count; i++)
                {
                    var val = rawDiffs[i];
                    syntheticRawData.Add(new HealthMetricData
                    {
                        NormalizedTimestamp = combinedTimestamps[i],
                        Value = (decimal?)(double.IsNaN(val) ? (double?)null : val),
                        Unit = chartData.Unit
                    });
                }

                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    var unit = chartData.Unit;
                    yAxis.Title = !string.IsNullOrEmpty(unit) ? $"Difference ({unit})" : "Difference";

                    try
                    {
                        NormalizeYAxis(yAxis, syntheticRawData, smoothedDiffs);
                    }
                    catch (Exception yAxisEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Y-axis normalization failed for diff chart: {yAxisEx.Message}");
                        var allValues = syntheticRawData.Where(d => d.Value.HasValue).Select(d => (double)d.Value!.Value).ToList();
                        allValues.AddRange(smoothedDiffs.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)));

                        if (allValues.Any())
                        {
                            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(allValues.Min());
                            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(allValues.Max());
                            var fallbackStep = MathHelper.RoundToThreeSignificantDigits((allValues.Max() - allValues.Min()) / 10.0);
                            if (fallbackStep > 0 && !double.IsNaN(fallbackStep) && !double.IsInfinity(fallbackStep))
                                yAxis.Separator = new LiveCharts.Wpf.Separator { Step = fallbackStep };
                            yAxis.ShowLabels = true;
                            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                        }
                    }

                    // Adjust chart height based on Y-axis tick count
                    AdjustChartHeightBasedOnYAxis(targetChart, 400.0);

                    if (targetChart.DataTooltip == null)
                    {
                        targetChart.DataTooltip = new DefaultTooltip();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Difference chart update error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating difference chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates a chart showing the ratio between two datasets (data1 / data2).
        /// Produces smoothed and raw ratio series aligned to the combined timestamps.
        /// </summary>
        private async Task UpdateRatioChartAsync(CartesianChart targetChart, IEnumerable<HealthMetricData> data1, IEnumerable<HealthMetricData> data2, string label1, string label2, DateTime fromDate, DateTime toDate)
        {
            if (data1 == null || data2 == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            var loadingTask = Task.Run(() =>
            {
                var ordered1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
                var ordered2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

                if (!ordered1.Any() && !ordered2.Any())
                    return null;

                var dateRange = toDate - fromDate;
                var tickInterval = MathHelper.DetermineTickInterval(dateRange);

                // Combined sorted unique timestamps from both datasets
                var combinedTimestamps = ordered1.Select(d => d.NormalizedTimestamp)
                    .Concat(ordered2.Select(d => d.NormalizedTimestamp))
                    .Distinct()
                    .OrderBy(dt => dt)
                    .ToList();

                if (!combinedTimestamps.Any())
                    return null;

                var normalizedIntervals = MathHelper.GenerateNormalizedIntervals(fromDate, toDate, tickInterval);

                // Map each combined timestamp to interval index
                var intervalIndices = combinedTimestamps.Select(ts =>
                    MathHelper.MapTimestampToIntervalIndex(ts, normalizedIntervals, tickInterval)).ToList();

                // Smoothed and interpolated values per dataset aligned to combined timestamps
                var smoothed1 = MathHelper.CreateSmoothedData(ordered1, fromDate, toDate);
                var smoothed2 = MathHelper.CreateSmoothedData(ordered2, fromDate, toDate);

                var interpSmoothed1 = MathHelper.InterpolateSmoothedData(smoothed1, combinedTimestamps);
                var interpSmoothed2 = MathHelper.InterpolateSmoothedData(smoothed2, combinedTimestamps);

                // Build raw values aligned to combined timestamps (missing -> NaN)
                // Group by timestamp and take the first value if duplicates exist to avoid ToDictionary exception
                var dict1 = ordered1.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);
                var dict2 = ordered2.GroupBy(d => d.NormalizedTimestamp)
                    .ToDictionary(g => g.Key, g => (double)g.First().Value!.Value);

                var rawValues1 = combinedTimestamps.Select(ts => dict1.TryGetValue(ts, out var v1) ? v1 : double.NaN).ToList();
                var rawValues2 = combinedTimestamps.Select(ts => dict2.TryGetValue(ts, out var v2) ? v2 : double.NaN).ToList();

                // Ratios (raw and smoothed). Guard against divide-by-zero and NaN
                var rawRatios = MathHelper.ReturnValueRatios(rawValues1, rawValues2);
                var smoothedRatios = MathHelper.ReturnValueRatios(interpSmoothed1, interpSmoothed2);

                var unit1 = ordered1.FirstOrDefault()?.Unit;
                var unit2 = ordered2.FirstOrDefault()?.Unit;
                var ratioUnit = (!string.IsNullOrEmpty(unit1) && !string.IsNullOrEmpty(unit2)) ? $"{unit1}/{unit2}" : null;

                return new
                {
                    CombinedTimestamps = combinedTimestamps,
                    IntervalIndices = intervalIndices,
                    NormalizedIntervals = normalizedIntervals,
                    RawRatios = rawRatios,
                    SmoothedRatios = smoothedRatios,
                    TickInterval = tickInterval,
                    DateRange = dateRange,
                    Unit = ratioUnit
                };
            });

            var chartData = await loadingTask;

            if (chartData == null)
            {
                targetChart?.Series.Clear();
                if (targetChart != null)
                {
                    _chartTimestamps.Remove(targetChart);
                }
                return;
            }

            try
            {
                targetChart.Series.Clear();

                var combinedTimestamps = chartData.CombinedTimestamps;
                var intervalIndices = chartData.IntervalIndices;
                var normalizedIntervals = chartData.NormalizedIntervals;
                var rawRatios = chartData.RawRatios;
                var smoothedRatios = chartData.SmoothedRatios;
                var tickInterval = chartData.TickInterval;


                var smoothedSeries = ChartHelper.CreateLineSeries($"Ratio (Smoothed)", 5, 2, Colors.DarkGreen);

                foreach (var v in smoothedRatios) smoothedSeries.Values.Add(v);


                var rawSeries = ChartHelper.CreateLineSeries($"Ratio (Raw)", 3, 1, Colors.DarkGray);

                foreach (var v in rawRatios) rawSeries.Values.Add(v);

                targetChart.Series.Add(smoothedSeries);
                targetChart.Series.Add(rawSeries);

                // store timestamps mapping for this chart
                _chartTimestamps[targetChart] = combinedTimestamps;

                if (targetChart.AxisX.Count > 0)
                {
                    var xAxis = targetChart.AxisX[0];
                    xAxis.Title = "Time";

                    var labelDataPointIndices = new HashSet<int>();
                    var seenIntervals = new HashSet<int>();
                    for (int i = 0; i < intervalIndices.Count; i++)
                    {
                        int intervalIndex = intervalIndices[i];
                        if (!seenIntervals.Contains(intervalIndex))
                        {
                            labelDataPointIndices.Add(i);
                            seenIntervals.Add(intervalIndex);
                        }
                    }

                    xAxis.LabelFormatter = value =>
                    {
                        try
                        {
                            double roundedValue = MathHelper.RoundToThreeSignificantDigits(value);
                            int dataPointIndex = (int)Math.Round(roundedValue);
                            if (dataPointIndex >= 0 &&
                                dataPointIndex < intervalIndices.Count &&
                                labelDataPointIndices.Contains(dataPointIndex))
                            {
                                int intervalIndex = intervalIndices[dataPointIndex];
                                if (intervalIndex >= 0 && intervalIndex < normalizedIntervals.Count)
                                {
                                    return ChartHelper.FormatDateTimeLabel(normalizedIntervals[intervalIndex], tickInterval);
                                }
                            }
                            return string.Empty;
                        }
                        catch
                        {
                            return string.Empty;
                        }
                    };

                    var intervalsToShow = tickInterval switch
                    {
                        TickInterval.Month => Math.Max(6, Math.Min(12, normalizedIntervals.Count)),
                        TickInterval.Week => Math.Max(4, Math.Min(8, normalizedIntervals.Count)),
                        TickInterval.Day => Math.Max(7, Math.Min(14, normalizedIntervals.Count)),
                        TickInterval.Hour => Math.Max(12, Math.Min(24, normalizedIntervals.Count)),
                        _ => Math.Min(10, normalizedIntervals.Count)
                    };

                    var sortedLabelIndices = labelDataPointIndices.OrderBy(x => x).ToList();
                    double step = 1.0;

                    if (sortedLabelIndices.Count > intervalsToShow && sortedLabelIndices.Count > 1)
                    {
                        var totalSpacing = sortedLabelIndices.Last() - sortedLabelIndices.First();
                        var averageSpacing = totalSpacing / (double)(sortedLabelIndices.Count - 1);
                        step = Math.Max(1.0, Math.Ceiling(averageSpacing * (sortedLabelIndices.Count / (double)intervalsToShow)));
                    }
                    else if (sortedLabelIndices.Count > 0)
                    {
                        if (sortedLabelIndices.Count > 1)
                        {
                            var minSpacing = sortedLabelIndices
                                .Zip(sortedLabelIndices.Skip(1), (a, b) => b - a)
                                .Where(s => s > 0)
                                .DefaultIfEmpty(1)
                                .Min();
                            step = Math.Max(1.0, minSpacing);
                        }
                    }

                    step = MathHelper.RoundToThreeSignificantDigits(step);
                    xAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
                    xAxis.Labels = null;
                    xAxis.MinValue = double.NaN;
                    xAxis.MaxValue = double.NaN;
                }

                // Build synthetic rawData list for axis normalization function
                var syntheticRawData = new List<HealthMetricData>();
                for (int i = 0; i < combinedTimestamps.Count; i++)
                {
                    var val = rawRatios[i];
                    syntheticRawData.Add(new HealthMetricData
                    {
                        NormalizedTimestamp = combinedTimestamps[i],
                        Value = (decimal?)(double.IsNaN(val) ? (double?)null : val),
                        Unit = chartData.Unit
                    });
                }

                if (targetChart.AxisY.Count > 0)
                {
                    var yAxis = targetChart.AxisY[0];
                    var unit = chartData.Unit;
                    yAxis.Title = !string.IsNullOrEmpty(unit) ? $"Ratio ({unit})" : "Ratio";

                    try
                    {
                        NormalizeYAxis(yAxis, syntheticRawData, smoothedRatios);
                    }
                    catch (Exception yAxisEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Y-axis normalization failed for ratio chart: {yAxisEx.Message}");
                        var allValues = syntheticRawData.Where(d => d.Value.HasValue).Select(d => (double)d.Value!.Value).ToList();
                        allValues.AddRange(smoothedRatios.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)));

                        if (allValues.Any())
                        {
                            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(allValues.Min());
                            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(allValues.Max());
                            var fallbackStep = MathHelper.RoundToThreeSignificantDigits((allValues.Max() - allValues.Min()) / 10.0);
                            if (fallbackStep > 0 && !double.IsNaN(fallbackStep) && !double.IsInfinity(fallbackStep))
                                yAxis.Separator = new LiveCharts.Wpf.Separator { Step = fallbackStep };
                            yAxis.ShowLabels = true;
                            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                        }
                    }

                    // Adjust chart height based on Y-axis tick count
                    AdjustChartHeightBasedOnYAxis(targetChart, 400.0);

                    if (targetChart.DataTooltip == null)
                    {
                        targetChart.DataTooltip = new DefaultTooltip();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ratio chart update error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error updating ratio chart: {ex.Message}\n\nSee debug output for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Normalizes Y-axis ticks to show uniform intervals (~10 ticks) with rounded bounds.
        /// </summary>
        private void NormalizeYAxis(Axis yAxis, List<HealthMetricData> rawData, List<double> smoothedValues)
        {
            var allValues = new List<double>();

            foreach (var point in rawData)
            {
                if (point.Value.HasValue)
                {
                    allValues.Add((double)point.Value.Value);
                }
            }

            foreach (var value in smoothedValues)
            {
                if (!double.IsNaN(value) && !double.IsInfinity(value))
                {
                    allValues.Add(value);
                }
            }

            if (!allValues.Any())
            {
                // Leave axis defaults if there's no data
                yAxis.MinValue = double.NaN;
                yAxis.MaxValue = double.NaN;
                yAxis.Separator = new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = false;
                return;
            }

            double dataMin = allValues.Min();
            double dataMax = allValues.Max();

            if (double.IsNaN(dataMin) || double.IsNaN(dataMax) || double.IsInfinity(dataMin) || double.IsInfinity(dataMax))
            {
                yAxis.MinValue = double.NaN;
                yAxis.MaxValue = double.NaN;
                yAxis.Separator = new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = false;
                return;
            }

            double minValue = dataMin;
            double maxValue = dataMax;
            double range = maxValue - minValue;

            // If all values are equal, provide a small symmetric padding (or absolute padding for zero)
            if (range <= double.Epsilon)
            {
                double padding = Math.Max(Math.Abs(minValue) * 0.1, 1e-3); // 0.1x value or small absolute
                // If value is zero, choose a sensible default window
                if (Math.Abs(minValue) < 1e-6)
                {
                    minValue = -padding;
                    maxValue = padding;
                }
                else
                {
                    minValue = minValue - padding;
                    maxValue = maxValue + padding;
                    if (dataMin >= 0)
                    {
                        // keep floor at zero if all data non-negative but allow a small gap for readability
                        minValue = Math.Max(0, minValue);
                    }
                }

                range = maxValue - minValue;
            }
            else
            {
                // Add small proportional padding for readability (5%)
                double padding = range * 0.05;
                minValue -= padding;
                maxValue += padding;

                // Avoid tiny negatives when data are strictly positive: keep min >= 0 if it was non-negative originally but allow small gap
                if (dataMin >= 0)
                {
                    minValue = Math.Max(0, minValue);
                }

                range = maxValue - minValue;
            }

            // Target ~10 ticks
            const double targetTicks = 10.0;
            double rawTickInterval = range / targetTicks;
            if (rawTickInterval <= 0 || double.IsNaN(rawTickInterval) || double.IsInfinity(rawTickInterval))
            {
                yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(minValue);
                yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(maxValue);
                var fallbackStep = MathHelper.RoundToThreeSignificantDigits((maxValue - minValue) / targetTicks);
                yAxis.Separator = fallbackStep > 0 ? new LiveCharts.Wpf.Separator { Step = fallbackStep } : new LiveCharts.Wpf.Separator();
                yAxis.ShowLabels = true;
                yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
                return;
            }

            // Choose a "nice" interval (1,2,5 * magnitude)
            double magnitude;
            try
            {
                var logValue = Math.Log10(Math.Abs(rawTickInterval));
                magnitude = Math.Pow(10, Math.Floor(logValue));
            }
            catch
            {
                magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Max(1e-6, rawTickInterval))));
            }

            var normalizedInterval = rawTickInterval / magnitude;
            double niceInterval = normalizedInterval switch
            {
                <= 1 => 1 * magnitude,
                <= 2 => 2 * magnitude,
                <= 5 => 5 * magnitude,
                _ => 10 * magnitude
            };

            niceInterval = MathHelper.RoundToThreeSignificantDigits(niceInterval);
            if (niceInterval <= 0 || double.IsNaN(niceInterval) || double.IsInfinity(niceInterval))
                niceInterval = rawTickInterval;

            // Round axis bounds to multiples of the interval and add one extra interval of padding
            var niceMin = Math.Floor(minValue / niceInterval) * niceInterval;
            var niceMax = Math.Ceiling(maxValue / niceInterval) * niceInterval;

            // Small extra margin so highest/lowest points aren't pinned to edge
            niceMin -= niceInterval * 0.0;
            niceMax += niceInterval * 0.0;

            // If original data are all non-negative, avoid negative axis minimum
            if (dataMin >= 0 && niceMin < 0)
            {
                niceMin = 0;
            }

            yAxis.MinValue = MathHelper.RoundToThreeSignificantDigits(niceMin);
            yAxis.MaxValue = MathHelper.RoundToThreeSignificantDigits(niceMax);

            // Ensure separator step approximately yields targetTicks
            double step = MathHelper.RoundToThreeSignificantDigits(niceInterval);
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                step = MathHelper.RoundToThreeSignificantDigits((yAxis.MaxValue - yAxis.MinValue) / targetTicks);
            }

            yAxis.Separator = new LiveCharts.Wpf.Separator { Step = step };
            yAxis.LabelFormatter = value => MathHelper.FormatToThreeSignificantDigits(value);
            yAxis.ShowLabels = true;
            yAxis.Labels = null;
        }

        /// <summary>
        /// Adjusts chart control Height based on Y-axis tick count to ensure ticks are spaced 20-40px apart.
        /// Charts live inside a ScrollViewer; if total chart heights exceed window height a scrollbar will appear.
        /// </summary>
        private void AdjustChartHeightBasedOnYAxis(CartesianChart chart, double minHeight)
        {
            if (chart == null || chart.AxisY.Count == 0) return;

            var yAxis = chart.AxisY[0];

            // Check if Y-axis has valid values (MinValue/MaxValue are double, can be NaN for auto)
            if (double.IsNaN(yAxis.MinValue) || double.IsNaN(yAxis.MaxValue) ||
                double.IsInfinity(yAxis.MinValue) || double.IsInfinity(yAxis.MaxValue))
            {
                // Use minimum height if axis is not properly configured
                chart.Height = minHeight;
                return;
            }

            double minValue = yAxis.MinValue;
            double maxValue = yAxis.MaxValue;
            double step = yAxis.Separator?.Step ?? 0;

            // If step is invalid, try to calculate from min/max
            if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
            {
                // Fallback: assume ~10 ticks
                step = (maxValue - minValue) / 10.0;
                if (step <= 0 || double.IsNaN(step) || double.IsInfinity(step))
                {
                    chart.Height = minHeight;
                    return;
                }
            }

            // Calculate number of ticks
            // Number of intervals = (max - min) / step, number of ticks = intervals + 1
            double range = maxValue - minValue;
            int tickCount = (int)Math.Ceiling(range / step) + 1;

            // Ensure at least 2 ticks
            tickCount = Math.Max(2, tickCount);

            // Calculate height based on tick spacing (target 30px per tick, range 20-40px)
            // Use 30px as average spacing for calculation
            const double tickSpacingPx = 30.0;
            const double paddingPx = 100.0; // Padding for margins, axis labels, legend, etc.

            double calculatedHeight = (tickCount * tickSpacingPx) + paddingPx;

            // Ensure height is at least the minimum
            calculatedHeight = Math.Max(minHeight, calculatedHeight);

            // Set a reasonable maximum to prevent runaway growth (e.g., 2000px)
            const double maxHeight = 2000.0;
            calculatedHeight = Math.Min(maxHeight, calculatedHeight);

            chart.Height = calculatedHeight;
        }

        /// <summary>
        /// Sets the three chart title TextBlocks based on provided display names.
        /// </summary>
        private void SetChartTitles(string leftName, string rightName)
        {
            // Defensive null checks
            leftName ??= string.Empty;
            rightName ??= string.Empty;

            ChartMainTitle.Text = $"{leftName} vs. {rightName}";
            ChartRatioTitle.Text = $"{leftName} / {rightName}";
            ChartDiffTitle.Text = $"{leftName} - {rightName}";
        }

        /// <summary>
        /// Reads selection state of the combo boxes and updates titles accordingly.
        /// Called when selections change so titles stay in sync even before loading data.
        /// </summary>
        private void UpdateChartTitlesFromCombos()
        {
            // Fix: Use array assignment to unpack the result of UpdateChartTitlesFromCombos
            string[] titles = ChartHelper.GetChartTitlesFromCombos(TablesCombo, SubtypeCombo, SubtypeCombo2);
            string display1 = titles.Length > 0 ? titles[0] : string.Empty;
            string display2 = titles.Length > 1 ? titles[1] : string.Empty;

            SetChartTitles(display1, display2);
        }

        private void OnChartDataHover(object? sender, ChartPoint chartPoint)
        {
            // Delegate to the existing handler so both possible DataHoverHandler signatures are supported.
            OnChartDataHover(chartPoint);
        }
    }
}