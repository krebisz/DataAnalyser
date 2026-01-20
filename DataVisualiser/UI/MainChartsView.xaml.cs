using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media;
using DataFileReader.Canonical;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Controls;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.Rendering;
using DataVisualiser.UI.Rendering.LiveCharts;
using UiChartRenderModel = DataVisualiser.UI.Rendering.ChartRenderModel;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;
using LiveChartsCore;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using Axis = LiveCharts.Wpf.Axis;
using ChartHelper = DataVisualiser.Core.Rendering.Helpers.ChartHelper;

namespace DataVisualiser.UI;

public partial class MainChartsView : UserControl
{
    private readonly ChartState _chartState = new();
    private readonly MetricState _metricState = new();
    private readonly UiState _uiState = new();
    private readonly LiveChartsChartRenderer _barPieRenderer = new();
    // Placeholder instance since DiffRatioChartController is commented out in XAML.
    private readonly DiffRatioChartController _diffRatioChartController = new();
    private ChartComputationEngine _chartComputationEngine = null!;
    private ChartRenderEngine _chartRenderEngine = null!;
    private ChartRenderingOrchestrator? _chartRenderingOrchestrator;
    private ChartUpdateCoordinator _chartUpdateCoordinator = null!;

    private string _connectionString = null!;
    private DistributionPolarRenderingService _distributionPolarRenderingService = null!;
    private ToolTip _distributionPolarTooltip = null!;
    private HourlyDistributionService _hourlyDistributionService = null!;
    private bool _isChangingResolution;
    private ChartPanelSurface _barPieSurface = null!;
    private bool _isBarPieVisible;
    private WeekdayTrendChartControllerAdapter _weekdayTrendAdapter = null!;
    private DistributionChartControllerAdapter _distributionAdapter = null!;
    private NormalizedChartControllerAdapter _normalizedAdapter = null!;
    private DiffRatioChartControllerAdapter _diffRatioAdapter = null!;
    private TransformDataPanelControllerAdapter _transformAdapter = null!;

    private bool _isInitializing = true;
    private bool _isMetricTypeChangePending;

    private MetricSelectionService _metricSelectionService = null!;
    private SubtypeSelectorManager _selectorManager = null!;
    private IStrategyCutOverService? _strategyCutOverService;
    private List<MetricNameOption>? _subtypeList;
    private ChartTooltipManager? _tooltipManager;
    private int _uiBusyDepth;
    private MainWindowViewModel _viewModel = null!;
    private WeekdayTrendChartUpdateCoordinator _weekdayTrendChartUpdateCoordinator = null!;
    private WeeklyDistributionService _weeklyDistributionService = null!;
    private DiffRatioChartController DiffRatioChartController => _diffRatioChartController;

    public MainChartsView()
    {
        InitializeComponent();

        InitializeInfrastructure();
        InitializeChartPipeline();
        InitializeViewModel();
        InitializeUiBindings();

        ExecuteStartupSequence();

        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        // Dispose tooltip manager to prevent memory leaks
        _tooltipManager?.Dispose();
        _tooltipManager = null;
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

    private void OnMainChartDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        var mode = MainChartController.DisplayStackedRadio.IsChecked == true
                ? MainChartDisplayMode.Stacked
                : MainChartController.DisplaySummedRadio.IsChecked == true
                        ? MainChartDisplayMode.Summed
                        : MainChartDisplayMode.Regular;

        _viewModel.SetMainChartDisplayMode(mode);
    }

    private void UpdateSelectedSubtypesInViewModel()
    {
        var distinctSeries = GetDistinctSelectedSeries();
        var selectedSubtypeCount = CountSelectedSubtypes(distinctSeries);

        _viewModel.SetSelectedSeries(distinctSeries);
        _normalizedAdapter.UpdateSubtypeOptions();
        _diffRatioAdapter.UpdateSubtypeOptions();
        _distributionAdapter.UpdateSubtypeOptions();
        _weekdayTrendAdapter.UpdateSubtypeOptions();

        // Update button states based on selected subtype count
        UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);
    }

    private List<MetricSeriesSelection> GetDistinctSelectedSeries()
    {
        return _selectorManager.GetSelectedSeries().GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase).Select(group => group.First()).ToList();
    }

    private static int CountSelectedSubtypes(IEnumerable<MetricSeriesSelection> selections)
    {
        return selections.Count(selection => selection.QuerySubtype != null);
    }

    private bool HasLoadedData()
    {
        return _viewModel.ChartState.LastContext?.Data1 != null && _viewModel.ChartState.LastContext.Data1.Any();
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

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require secondary data.
    ///     These buttons are disabled when fewer than 2 subtypes are selected.
    ///     If charts are currently visible when secondary data becomes unavailable, they are cleared and hidden.
    /// </summary>
    private void UpdateSecondaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        var hasSecondaryData = selectedSubtypeCount >= 2;
        var canToggle = hasSecondaryData && HasLoadedData();

        // If secondary data is no longer available, use the ViewModel state setters to trigger
        // visibility updates while clearing stale chart data.
        if (!hasSecondaryData)
        {
            if (_viewModel.ChartState.IsNormalizedVisible)
            {
                _normalizedAdapter.Clear(_viewModel.ChartState);
                _viewModel.SetNormalizedVisible(false);
            }

            if (_viewModel.ChartState.IsDiffRatioVisible)
            {
                _diffRatioAdapter.Clear(_viewModel.ChartState);
                _viewModel.SetDiffRatioVisible(false);
            }
        }

        // Update button enabled states (this is UI-only, not part of the rendering pipeline)
        NormalizedChartController.ToggleButton.IsEnabled = canToggle;
        DiffRatioChartController.ToggleButton.IsEnabled = canToggle;
    }

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require at least one subtype.
    ///     These buttons are disabled when no subtypes are selected.
    /// </summary>
    private void UpdatePrimaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        var hasPrimaryData = selectedSubtypeCount >= 1;
        var canToggle = hasPrimaryData && HasLoadedData();

        MainChartController.ToggleButton.IsEnabled = HasLoadedData();
        WeekdayTrendChartController.ToggleButton.IsEnabled = canToggle;
        DistributionChartController.ToggleButton.IsEnabled = canToggle;
        TransformDataPanelController.ToggleButton.IsEnabled = canToggle;
        BarPieChartController.ToggleButton.IsEnabled = canToggle;
    }

    private void OnFromDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;
        _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
    }

    private void OnToDateChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;
        _viewModel.SetDateRange(FromDate.SelectedDate, ToDate.SelectedDate);
    }

    private void OnMetricTypesLoaded(object? sender, MetricTypesLoadedEventArgs e)
    {
        _isMetricTypeChangePending = false;
        TablesCombo.Items.Clear();

        var addedAllMetricType = !e.MetricTypes.Any(type => string.Equals(type.Value, "(All)", StringComparison.OrdinalIgnoreCase));

        if (addedAllMetricType)
        {
            TablesCombo.Items.Add(new MetricNameOption("(All)", "(All)"));
        }

        foreach (var type in e.MetricTypes)
        {
            TablesCombo.Items.Add(type);
        }

        if (TablesCombo.Items.Count > 0)
        {
            TablesCombo.SelectedIndex = addedAllMetricType && TablesCombo.Items.Count > 1 ? 1 : 0;
            _viewModel.SetSelectedMetricType(GetSelectedMetricValue(TablesCombo));
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

        if (_isMetricTypeChangePending)
        {
            if (_selectorManager.HasDynamicCombos)
            {
                _selectorManager.UpdateLastDynamicComboItems(subtypeListLocal, selectedMetricType);
            }
            else
            {
                RefreshPrimarySubtypeCombo(subtypeListLocal, true, selectedMetricType);
            }

            _isMetricTypeChangePending = false;
            UpdateSelectedSubtypesInViewModel();

            return;
        }

        RefreshPrimarySubtypeCombo(subtypeListLocal, false, selectedMetricType);

        BuildDynamicSubtypeControls(subtypeListLocal);
        UpdateSelectedSubtypesInViewModel();
        _ = LoadDateRangeForSelectedMetrics();
    }

    private void BuildDynamicSubtypeControls(IEnumerable<MetricNameOption> subtypes)
    {
        _selectorManager.ClearDynamic();
        UpdateSelectedSubtypesInViewModel();
    }

    private void RefreshPrimarySubtypeCombo(IEnumerable<MetricNameOption> subtypes, bool preserveSelection, MetricNameOption? selectedMetricType)
    {
        var previousSelection = GetSelectedMetricValue(SubtypeCombo);

        SubtypeCombo.Items.Clear();
        SubtypeCombo.Items.Add(new MetricNameOption("(All)", "(All)"));

        foreach (var st in subtypes)
        {
            SubtypeCombo.Items.Add(st);
        }

        SubtypeCombo.IsEnabled = subtypes.Any();
        _selectorManager.SetPrimaryMetricType(selectedMetricType);

        if (preserveSelection && !string.IsNullOrWhiteSpace(previousSelection) && SubtypeCombo.Items.OfType<MetricNameOption>().Any(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase)))
        {
            SubtypeCombo.SelectedItem = SubtypeCombo.Items.OfType<MetricNameOption>().First(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase));
            return;
        }

        SubtypeCombo.SelectedIndex = 0;
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
        {
            return;
        }

        // Optional debug popup (existing behavior)
        var showDebugPopup = ConfigurationManager.AppSettings["DataVisualiser:ShowDebugPopup"];

        if (bool.TryParse(showDebugPopup, out var showDebug) && showDebug)
        {
            var data1 = ctx.Data1.ToList();
            var data2 = ctx.Data2?.ToList() ?? new List<MetricData>();

            var msg = $"Data1 count: {data1.Count}\n" + $"Data2 count: {data2.Count}\n" + $"First 3 timestamps (Data1):\n" + string.Join("\n", data1.Take(3).Select(d => d.NormalizedTimestamp)) + "\n\nFirst 3 timestamps (Data2):\n" + string.Join("\n", data2.Take(3).Select(d => d.NormalizedTimestamp));

            MessageBox.Show(msg, "DEBUG - LastContext contents");
        }

        _transformAdapter.CompleteSelectionsPendingLoad();
        _normalizedAdapter.UpdateSubtypeOptions();
        _diffRatioAdapter.UpdateSubtypeOptions();
        _transformAdapter.UpdateTransformSubtypeOptions();
        _transformAdapter.UpdateTransformComputeButtonState();
        var selectedSubtypeCount = CountSelectedSubtypes(_viewModel.MetricState.SelectedSeries);
        UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);

        if (_isBarPieVisible)
            await RenderBarPieChartAsync();

        await RenderChartsFromLastContext();
    }

    private void UpdateChartVisibility(Panel panel, ButtonBase toggleButton, bool isVisible)
    {
        panel.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        toggleButton.Content = isVisible ? "Hide" : "Show";
    }

    private void UpdateChartVisibilityForToggle(ChartUpdateRequestedEventArgs e)
    {
        switch (e.ToggledChartName)
        {
            case "Main":
                MainChartController.Panel.IsChartVisible = e.ShowMain;
                break;
            case "Norm":
                NormalizedChartController.Panel.IsChartVisible = e.ShowNormalized;
                break;
            case "DiffRatio":
                DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
                break;
            case "Distribution":
                DistributionChartController.Panel.IsChartVisible = e.ShowDistribution;
                _distributionAdapter.UpdateChartTypeVisibility();
                break;
            case "WeeklyTrend":
                WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
                _weekdayTrendAdapter.UpdateChartTypeVisibility();
                break;
            case "Transform":
                TransformDataPanelController.Panel.IsChartVisible = e.ShowTransformPanel;
                break;
        }
    }

    //private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
    //{
    //    if (e.IsVisibilityOnlyToggle && !string.IsNullOrEmpty(e.ToggledChartName))
    //    {
    //        UpdateChartVisibilityForToggle(e);

    //        // Transform panel visibility toggle - just update visibility, don't reload charts
    //        if (e.ToggledChartName == "Transform")
    //        {
    //            // Only populate grids if panel is being shown and we have data
    //            if (_viewModel.ChartState.IsTransformPanelVisible)
    //            {
    //                var transformCtx = _viewModel.ChartState.LastContext;
    //                if (transformCtx != null && ShouldRenderCharts(transformCtx))
    //                    PopulateTransformGrids(transformCtx);
    //                UpdateTransformSubtypeOptions();
    //            }

    //            return; // Don't reload other charts
    //        }

    //        var ctx = _viewModel.ChartState.LastContext;
    //        if (ctx != null && ShouldRenderCharts(ctx))
    //            await RenderSingleChart(e.ToggledChartName, ctx);

    //        return;
    //    }

    //    // Update visibility for all charts (just UI state, doesn't clear data)
    //    MainChartController.Panel.IsChartVisible = e.ShowMain;
    //    NormalizedChartController.Panel.IsChartVisible = e.ShowNormalized;
    //    DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
    //    DistributionChartController.Panel.IsChartVisible = e.ShowDistribution;
    //    UpdateDistributionChartTypeVisibility();
    //    WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
    //    UpdateWeekdayTrendChartTypeVisibility();
    //    TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;

    //    // If a specific chart was identified (visibility toggle or chart-specific config change), only render that chart
    //    if (!string.IsNullOrEmpty(e.ToggledChartName))
    //    {
    //        // Transform panel visibility toggle - just update visibility, don't reload charts
    //        if (e.ToggledChartName == "Transform" && e.IsVisibilityOnlyToggle)
    //        {
    //            // Only populate grids if panel is being shown and we have data
    //            if (_viewModel.ChartState.IsTransformPanelVisible)
    //            {
    //                var transformCtx = _viewModel.ChartState.LastContext;
    //                if (transformCtx != null && ShouldRenderCharts(transformCtx))
    //                    PopulateTransformGrids(transformCtx);
    //                UpdateTransformSubtypeOptions();
    //            }

    //            return; // Don't reload other charts
    //        }

    //        var ctx = _viewModel.ChartState.LastContext;
    //        if (ctx != null && ShouldRenderCharts(ctx))
    //            await RenderSingleChart(e.ToggledChartName, ctx);
    //    }
    //    // Otherwise, render all charts (data change scenario)
    //    else if (e.ShouldRenderCharts && !e.IsVisibilityOnlyToggle)
    //    {
    //        await RenderChartsFromLastContext();
    //    }
    //}

    private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
    {
        if (HandleVisibilityOnlyToggle(e))
            return;

        UpdateAllChartVisibilities(e);

        if (!string.IsNullOrEmpty(e.ToggledChartName))
            await HandleSingleChartUpdate(e);
        else if (e.ShouldRenderCharts && !e.IsVisibilityOnlyToggle)
            await RenderChartsFromLastContext();
    }

    private bool HandleVisibilityOnlyToggle(ChartUpdateRequestedEventArgs e)
    {
        if (!e.IsVisibilityOnlyToggle || string.IsNullOrEmpty(e.ToggledChartName))
            return false;

        UpdateChartVisibilityForToggle(e);

        if (e.ToggledChartName == "Transform")
        {
            _transformAdapter.HandleVisibilityOnlyToggle(_viewModel.ChartState.LastContext);
            return true; // Never reload charts
        }

        if (!IsChartVisible(e.ToggledChartName))
            return true;

        var ctx = _viewModel.ChartState.LastContext;
        if (ctx != null && ShouldRenderCharts(ctx) && !HasChartData(e.ToggledChartName))
            _ = RenderSingleChart(e.ToggledChartName, ctx);

        return true;
    }

    private bool IsChartVisible(string chartName)
    {
        return chartName switch
        {
            "Main" => _viewModel.ChartState.IsMainVisible,
            "Norm" => _viewModel.ChartState.IsNormalizedVisible,
            "DiffRatio" => _viewModel.ChartState.IsDiffRatioVisible,
            "Distribution" => _viewModel.ChartState.IsDistributionVisible,
            "WeeklyTrend" => _viewModel.ChartState.IsWeeklyTrendVisible,
            _ => false
        };
    }

    private bool HasChartData(string chartName)
    {
        return chartName switch
        {
            "Main" => HasSeries(MainChartController.Chart.Series),
            "Norm" => HasSeries(NormalizedChartController.Chart.Series),
            "DiffRatio" => HasSeries(DiffRatioChartController.Chart.Series),
            "Distribution" => _viewModel.ChartState.IsDistributionPolarMode
                    ? HasSeries(DistributionChartController.PolarChart.Series)
                    : HasSeries(DistributionChartController.Chart.Series),
            "WeeklyTrend" => _viewModel.ChartState.WeekdayTrendChartMode == WeekdayTrendChartMode.Polar
                    ? HasSeries(WeekdayTrendChartController.PolarChart.Series)
                    : HasSeries(WeekdayTrendChartController.Chart.Series),
            _ => false
        };
    }

    private static bool HasSeries(System.Collections.IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
    }

    private void UpdateAllChartVisibilities(ChartUpdateRequestedEventArgs e)
    {
        MainChartController.Panel.IsChartVisible = e.ShowMain;
        NormalizedChartController.Panel.IsChartVisible = e.ShowNormalized;
        DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
        DistributionChartController.Panel.IsChartVisible = e.ShowDistribution;
        _distributionAdapter.UpdateChartTypeVisibility();

        WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
        _weekdayTrendAdapter.UpdateChartTypeVisibility();

        TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;
    }

    private async Task HandleSingleChartUpdate(ChartUpdateRequestedEventArgs e)
    {
        // Transform panel visibility toggle - just update visibility, don't reload charts
        if (e.ToggledChartName == "Transform" && e.IsVisibilityOnlyToggle)
        {
            _transformAdapter.HandleVisibilityOnlyToggle(_viewModel.ChartState.LastContext);
            return;
        }

        if (string.IsNullOrWhiteSpace(e.ToggledChartName))
            return;

        var ctx = _viewModel.ChartState.LastContext;

        if (ctx != null && ShouldRenderCharts(ctx))
        {
            await RenderSingleChart(e.ToggledChartName, ctx);
        }
    }


    private ChartComputationResult? ComputeMainChart(IReadOnlyList<IEnumerable<MetricData>> selectedMetricSeries, IReadOnlyList<string> selectedMetricLabels, string? unit, DateTime from, DateTime to)
    {
        IChartComputationStrategy strategy;
        var ctx = _viewModel.ChartState.LastContext;

        if (selectedMetricSeries.Count > 2)
            strategy = CreateMultiMetricStrategy(ctx!, selectedMetricSeries.ToList(), selectedMetricLabels.ToList(), from, to, unit);
        else if (selectedMetricSeries.Count == 2)
            strategy = CreateCombinedMetricStrategy(ctx!, selectedMetricSeries[0], selectedMetricSeries[1], selectedMetricLabels[0], selectedMetricLabels[1], from, to);
        else if (ctx?.SemanticMetricCount == 1)
            strategy = CreateSingleMetricStrategy(ctx, selectedMetricSeries[0], selectedMetricLabels[0], from, to);
        else
            return null;

        return strategy.Compute();
    }


    private async Task RenderMainChart(IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, string displayName1, string displayName2, DateTime from, DateTime to, string? metricType = null, string? primarySubtype = null, string? secondarySubtype = null)
    {
        var ctx = _viewModel.ChartState.LastContext;
        if (ctx == null || _chartRenderingOrchestrator == null)
            return;

        var mode = _viewModel.ChartState.MainChartDisplayMode;
        var isStacked = mode == MainChartDisplayMode.Stacked;
        var isCumulative = mode == MainChartDisplayMode.Summed;

        await _chartRenderingOrchestrator.RenderPrimaryChartAsync(ctx, MainChartController.Chart, data1, data2, displayName1, displayName2, from, to, metricType, _viewModel.MetricState.SelectedSeries, _viewModel.MetricState.ResolutionTableName, isStacked: isStacked, isCumulative: isCumulative);
    }


    /// <summary>
    ///     Selects the appropriate computation strategy based on the number of series.
    ///     Returns the strategy and secondary label (if applicable).
    /// </summary>
    private (IChartComputationStrategy strategy, string? secondaryLabel) SelectComputationStrategy(List<IEnumerable<MetricData>> series, List<string> labels, DateTime from, DateTime to)
    {
        string? secondaryLabel = null;
        IChartComputationStrategy strategy;

        var ctx = _viewModel.ChartState.LastContext!;
        // Use actual series count instead of SemanticMetricCount which is hardcoded to max 2
        var actualSeriesCount = series.Count;

        Debug.WriteLine($"[STRATEGY] ActualSeriesCount={actualSeriesCount}, SemanticMetricCount={ctx.SemanticMetricCount}, " + $"PrimaryCms={(ctx.PrimaryCms == null ? "NULL" : "SET")}, " + $"SecondaryCms={(ctx.SecondaryCms == null ? "NULL" : "SET")}");

        // ---------- MULTI METRIC ----------
        if (actualSeriesCount > 2)
        {
            strategy = CreateMultiMetricStrategy(ctx, series, labels, from, to);
        }
        // ---------- COMBINED METRIC ----------
        else if (actualSeriesCount == 2)
        {
            secondaryLabel = labels[1];
            strategy = CreateCombinedMetricStrategy(ctx, series[0], series[1], labels[0], labels[1], from, to);
        }
        // ---------- SINGLE METRIC ----------
        else
        {
            strategy = CreateSingleMetricStrategy(ctx, series[0], labels[0], from, to);
        }

        Debug.WriteLine($"[StrategySelection] actualSeriesCount={actualSeriesCount}, " + $"series.Count={series.Count}, " + $"strategy={strategy.GetType().Name}");

        return (strategy, secondaryLabel);
    }


    private async Task RenderOrClearChart(CartesianChart chart, bool isVisible, IChartComputationStrategy? strategy, string title, double minHeight = 400, string? metricType = null, string? primarySubtype = null, string? secondarySubtype = null, string? operationType = null, bool isOperationChart = false)
    {
        if (isVisible && strategy != null)
        {
            var ctx = _viewModel.ChartState.LastContext;
            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chart, strategy, title, minHeight: minHeight, metricType: metricType, primarySubtype: primarySubtype, secondarySubtype: secondarySubtype, operationType: operationType, isOperationChart: isOperationChart, displayPrimaryMetricType: ctx?.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx?.DisplaySecondaryMetricType, displayPrimarySubtype: ctx?.DisplayPrimarySubtype, displaySecondarySubtype: ctx?.DisplaySecondarySubtype);
        }
        // Note: We don't clear the chart when hiding - just hide the panel to preserve data
        // Charts are only cleared when data changes (e.g., new selection, resolution change, etc.)
    }

    private async Task RenderChartsFromLastContext()
    {
        using var busyScope = BeginUiBusyScope();
        var ctx = _viewModel.ChartState.LastContext;
        if (!ShouldRenderCharts(ctx))
            return;

        var safeCtx = ctx!;
        var hasSecondaryData = HasSecondaryData(safeCtx);
        var metricType = safeCtx.MetricType;
        var primarySubtype = safeCtx.PrimarySubtype;
        var secondarySubtype = safeCtx.SecondarySubtype;

        // Only render charts that are visible - skip computation entirely for hidden charts
        if (_viewModel.ChartState.IsMainVisible)
            await RenderPrimaryChart(safeCtx);

        // Charts that require secondary data - only render if visible AND secondary data exists
        if (hasSecondaryData)
        {
            if (_viewModel.ChartState.IsNormalizedVisible)
                await _normalizedAdapter.RenderAsync(safeCtx);

            if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                await _diffRatioAdapter.RenderAsync(safeCtx);
        }
        else
        {
            // Clear charts that require secondary data when no secondary data exists
            _normalizedAdapter.Clear(_viewModel.ChartState);
            _diffRatioAdapter.Clear(_viewModel.ChartState);
        }

        // Charts that don't require secondary data - only render if visible
        if (_viewModel.ChartState.IsDistributionVisible)
            await _distributionAdapter.RenderAsync(safeCtx);

        if (_viewModel.ChartState.IsWeeklyTrendVisible)
            await _weekdayTrendAdapter.RenderAsync(safeCtx);

        // Populate transform panel grids if visible
        if (_viewModel.ChartState.IsTransformPanelVisible)
            await _transformAdapter.RenderAsync(safeCtx);
    }

    /// <summary>
    ///     Validates that the chart context is valid and has data to render.
    /// </summary>
    private static bool ShouldRenderCharts(ChartDataContext? ctx)
    {
        return ctx != null && ctx.Data1 != null && ctx.Data1.Any();
    }

    /// <summary>
    ///     Determines if secondary data is available for rendering secondary charts.
    /// </summary>
    private static bool HasSecondaryData(ChartDataContext ctx)
    {
        return ctx.Data2 != null && ctx.Data2.Any();
    }

    /// <summary>
    ///     Renders the primary (main) chart from the context.
    /// </summary>
    private async Task RenderPrimaryChart(ChartDataContext ctx)
    {
        if (_viewModel.ChartState.IsMainVisible)
            await RenderMainChart(ctx.Data1!, ctx.Data2, ctx.DisplayName1 ?? string.Empty, ctx.DisplayName2 ?? string.Empty, ctx.From, ctx.To, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype);
        // Note: We don't clear the chart when hiding - just hide the panel to preserve data
    }

    /// <summary>
    ///     Renders only a specific chart (used for visibility-only toggles to avoid re-rendering all charts).
    /// </summary>
    private async Task RenderSingleChart(string chartName, ChartDataContext ctx)
    {
        using var busyScope = BeginUiBusyScope();
        var hasSecondaryData = HasSecondaryData(ctx);
        var metricType = ctx.MetricType;
        var primarySubtype = ctx.PrimarySubtype;
        var secondarySubtype = ctx.SecondarySubtype;

        switch (chartName)
        {
            case "Main":
                if (_viewModel.ChartState.IsMainVisible)
                    await RenderMainChart(ctx.Data1!, ctx.Data2, ctx.DisplayName1 ?? string.Empty, ctx.DisplayName2 ?? string.Empty, ctx.From, ctx.To, metricType, primarySubtype, secondarySubtype);
                break;

            case "Norm":
                if (_viewModel.ChartState.IsNormalizedVisible && hasSecondaryData)
                    await _normalizedAdapter.RenderAsync(ctx);
                break;

            case "DiffRatio":
                if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                    await _diffRatioAdapter.RenderAsync(ctx);
                break;

            case "Distribution":
                if (_viewModel.ChartState.IsDistributionVisible)
                    await _distributionAdapter.RenderAsync(ctx);
                break;

            case "WeeklyTrend":
                if (_viewModel.ChartState.IsWeeklyTrendVisible)
                    await _weekdayTrendAdapter.RenderAsync(ctx);
                break;
        }
    }

    /// <summary>
    ///     Renders all secondary charts (normalized, distribution, weekday trend, difference, ratio).
    /// </summary>
    private async Task RenderSecondaryCharts(ChartDataContext ctx)
    {
        var metricType = ctx.MetricType;
        var primarySubtype = ctx.PrimarySubtype;
        var secondarySubtype = ctx.SecondarySubtype;

        await _normalizedAdapter.RenderAsync(ctx);
        await _distributionAdapter.RenderAsync(ctx);
        await _weekdayTrendAdapter.RenderAsync(ctx);
        await _diffRatioAdapter.RenderAsync(ctx);
    }

    private void ClearSecondaryChartsAndReturn()
    {
        _normalizedAdapter.Clear(_viewModel.ChartState);
        _diffRatioAdapter.Clear(_viewModel.ChartState);
        _distributionAdapter.Clear(_viewModel.ChartState);
        // NOTE: WeekdayTrend intentionally not cleared here to preserve current behavior (tied to secondary presence).
        // Cartesian, Polar, and Scatter modes are handled by the adapter render check.
    }

    private Panel? GetChartPanel(string chartName)
    {
        return chartName switch
        {
            "Norm" => NormalizedChartController.Panel.ChartContentPanel,
            "DiffRatio" => DiffRatioChartController.Panel.ChartContentPanel,
            "Distribution" => DistributionChartController.Panel.ChartContentPanel,
            "WeeklyTrend" => WeekdayTrendChartController.Panel.ChartContentPanel,
            _ => null
        };
    }

    private IChartComputationStrategy CreateSingleMetricStrategy(ChartDataContext ctx, IEnumerable<MetricData> data, string label, DateTime from, DateTime to)
    {
        // Use unified cut-over service
        if (_strategyCutOverService == null)
            throw new InvalidOperationException("StrategyCutOverService is not initialized. Ensure InitializeChartPipeline() is called before using strategies.");

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = data,
            Label1 = label,
            From = from,
            To = to
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.SingleMetric, ctx, parameters);
    }

    private IChartComputationStrategy CreateMultiMetricStrategy(ChartDataContext ctx, List<IEnumerable<MetricData>> series, List<string> labels, DateTime from, DateTime to, string? unit = null)
    {
        // Use unified cut-over service
        if (_strategyCutOverService == null)
            throw new InvalidOperationException("StrategyCutOverService is not initialized. Ensure InitializeChartPipeline() is called before using strategies.");

        var parameters = new StrategyCreationParameters
        {
            LegacySeries = series,
            Labels = labels,
            From = from,
            To = to,
            Unit = unit
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.MultiMetric, ctx, parameters);
    }

    private IChartComputationStrategy CreateCombinedMetricStrategy(ChartDataContext ctx, IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string label1, string label2, DateTime from, DateTime to)
    {
        // Use unified cut-over service
        if (_strategyCutOverService == null)
            throw new InvalidOperationException("StrategyCutOverService is not initialized. Ensure InitializeChartPipeline() is called before using strategies.");

        var parameters = new StrategyCreationParameters
        {
            LegacyData1 = data1,
            LegacyData2 = data2,
            Label1 = label1,
            Label2 = label2,
            From = from,
            To = to
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.CombinedMetric, ctx, parameters);
    }

    private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
    {
        var panel = GetChartPanel(e.ChartName);
        if (panel != null)
            panel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnErrorOccured(object? sender, ErrorEventArgs e)
    {
        if (_isChangingResolution)
        {
            ClearAllCharts();
            return;
        }

        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        ClearAllCharts();
    }

    private void ClearAllCharts()
    {
        ChartHelper.ClearChart(MainChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        _normalizedAdapter.Clear(_viewModel.ChartState);
        _diffRatioAdapter.Clear(_viewModel.ChartState);
        _distributionAdapter.Clear(_viewModel.ChartState);
        _weekdayTrendAdapter.Clear(_viewModel.ChartState);
        ClearBarPieChart();
        _viewModel.ChartState.LastContext = null;
        _transformAdapter.Clear(_viewModel.ChartState);
    }

    private void ClearBarPieChart()
    {
        _barPieSurface ??= new ChartPanelSurface(BarPieChartController.Panel);
        _barPieRenderer.Apply(_barPieSurface, CreateEmptyBarPieModel());
    }

    private sealed class UiBusyScope : IDisposable
    {
        private readonly MainChartsView _owner;
        private bool _disposed;

        public UiBusyScope(MainChartsView owner)
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

    #region Initialization Phases

    private void InitializeInfrastructure()
    {
        _connectionString = ResolveConnectionString();

        _metricSelectionService = new MetricSelectionService(_connectionString);

        _chartComputationEngine = new ChartComputationEngine();
        _chartRenderEngine = new ChartRenderEngine();

        InitializeTooltips();
    }

    private void InitializeChartPipeline()
    {
        _chartUpdateCoordinator = CreateChartUpdateCoordinator();
        _weeklyDistributionService = CreateWeeklyDistributionService();
        _hourlyDistributionService = CreateHourlyDistributionService();
        _distributionPolarRenderingService = CreateDistributionPolarRenderingService();
        _chartRenderingOrchestrator = CreateChartRenderingOrchestrator();
        _weekdayTrendChartUpdateCoordinator = CreateWeekdayTrendChartUpdateCoordinator();
    }

    private void InitializeViewModel()
    {
        _viewModel = new MainWindowViewModel(_chartState, _metricState, _uiState, _metricSelectionService);

        DataContext = _viewModel;
    }

    private void InitializeUiBindings()
    {
        WireViewModelEvents();

        _distributionAdapter = new DistributionChartControllerAdapter(
            DistributionChartController,
            _viewModel,
            () => _isInitializing,
            BeginUiBusyScope,
            _metricSelectionService,
            () => _chartRenderingOrchestrator,
            _weeklyDistributionService,
            _hourlyDistributionService,
            _distributionPolarRenderingService,
            () => _distributionPolarTooltip);

        _weekdayTrendAdapter = new WeekdayTrendChartControllerAdapter(
            WeekdayTrendChartController,
            _viewModel,
            () => _isInitializing,
            BeginUiBusyScope,
            _metricSelectionService,
            () => _strategyCutOverService,
            _weekdayTrendChartUpdateCoordinator);

        _normalizedAdapter = new NormalizedChartControllerAdapter(
            NormalizedChartController,
            _viewModel,
            () => _isInitializing,
            BeginUiBusyScope,
            _metricSelectionService,
            () => _chartRenderingOrchestrator,
            _chartUpdateCoordinator,
            () => _strategyCutOverService);

        _diffRatioAdapter = new DiffRatioChartControllerAdapter(
            DiffRatioChartController,
            _viewModel,
            () => _isInitializing,
            BeginUiBusyScope,
            _metricSelectionService,
            () => _chartRenderingOrchestrator,
            () => _tooltipManager);

        _transformAdapter = new TransformDataPanelControllerAdapter(
            TransformDataPanelController,
            _viewModel,
            () => _isInitializing,
            BeginUiBusyScope,
            _metricSelectionService,
            _chartUpdateCoordinator);

        // Wire up MainChartController events
        MainChartController.ToggleRequested += OnMainChartToggleRequested;
        MainChartController.DisplayModeChanged += OnMainChartDisplayModeChanged;
        WeekdayTrendChartController.ToggleRequested += _weekdayTrendAdapter.OnToggleRequested;
        WeekdayTrendChartController.ChartTypeToggleRequested += _weekdayTrendAdapter.OnChartTypeToggleRequested;
        WeekdayTrendChartController.DayToggled += _weekdayTrendAdapter.OnDayToggled;
        WeekdayTrendChartController.AverageToggled += _weekdayTrendAdapter.OnAverageToggled;
        WeekdayTrendChartController.AverageWindowChanged += _weekdayTrendAdapter.OnAverageWindowChanged;
        WeekdayTrendChartController.SubtypeChanged += _weekdayTrendAdapter.OnSubtypeChanged;
        DiffRatioChartController.ToggleRequested += _diffRatioAdapter.OnToggleRequested;
        DiffRatioChartController.OperationToggleRequested += _diffRatioAdapter.OnOperationToggleRequested;
        DiffRatioChartController.PrimarySubtypeChanged += _diffRatioAdapter.OnPrimarySubtypeChanged;
        DiffRatioChartController.SecondarySubtypeChanged += _diffRatioAdapter.OnSecondarySubtypeChanged;
        BarPieChartController.ToggleRequested += OnBarPieToggleRequested;
        BarPieChartController.DisplayModeChanged += OnBarPieDisplayModeChanged;
        BarPieChartController.BucketCountChanged += OnBarPieBucketCountChanged;
        NormalizedChartController.ToggleRequested += _normalizedAdapter.OnToggleRequested;
        NormalizedChartController.NormalizationModeChanged += _normalizedAdapter.OnNormalizationModeChanged;
        NormalizedChartController.PrimarySubtypeChanged += _normalizedAdapter.OnPrimarySubtypeChanged;
        NormalizedChartController.SecondarySubtypeChanged += _normalizedAdapter.OnSecondarySubtypeChanged;
        DistributionChartController.ToggleRequested += _distributionAdapter.OnToggleRequested;
        DistributionChartController.ChartTypeToggleRequested += _distributionAdapter.OnChartTypeToggleRequested;
        DistributionChartController.ModeChanged += _distributionAdapter.OnModeChanged;
        DistributionChartController.SubtypeChanged += _distributionAdapter.OnSubtypeChanged;
        DistributionChartController.DisplayModeChanged += _distributionAdapter.OnDisplayModeChanged;
        DistributionChartController.IntervalCountChanged += _distributionAdapter.OnIntervalCountChanged;
        TransformDataPanelController.ToggleRequested += _transformAdapter.OnToggleRequested;
        TransformDataPanelController.OperationChanged += _transformAdapter.OnOperationChanged;
        TransformDataPanelController.PrimarySubtypeChanged += _transformAdapter.OnPrimarySubtypeChanged;
        TransformDataPanelController.SecondarySubtypeChanged += _transformAdapter.OnSecondarySubtypeChanged;
        TransformDataPanelController.ComputeRequested += _transformAdapter.OnComputeRequested;
    }

    private void ExecuteStartupSequence()
    {
        InitializeDateRange();
        InitializeDefaultUiState();
        InitializeSubtypeSelector();
        InitializeResolution();
        InitializeCharts();

        _viewModel.RequestChartUpdate();

        // Mark initialization as complete
        SyncCmsToggleStates();
        _isInitializing = false;

        SyncInitialButtonStates();
    }


    #endregion

    #region CMS toggle handling

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
        Debug.WriteLine($"[CMS] Enabled={CmsConfiguration.UseCmsData}");

        if (_isBarPieVisible)
            _ = RenderBarPieChartAsync();
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

        Debug.WriteLine($"[CMS] Enabled={CmsConfiguration.UseCmsData}, Single={CmsConfiguration.UseCmsForSingleMetric}, Combined={CmsConfiguration.UseCmsForCombinedMetric}, Multi={CmsConfiguration.UseCmsForMultiMetric}, Normalized={CmsConfiguration.UseCmsForNormalized}, Weekly={CmsConfiguration.UseCmsForWeeklyDistribution}, WeekdayTrend={CmsConfiguration.UseCmsForWeekdayTrend}, Hourly={CmsConfiguration.UseCmsForHourlyDistribution}, BarPie={CmsConfiguration.UseCmsForBarPie}");

        if (_isBarPieVisible)
            _ = RenderBarPieChartAsync();
    }

    #endregion

    #region Factory / Creation Methods

    private string ResolveConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
    }

    private ChartUpdateCoordinator CreateChartUpdateCoordinator()
    {
        if (_tooltipManager == null)
            throw new InvalidOperationException("Tooltip manager is not initialized. Ensure InitializeInfrastructure() is called before creating the chart update coordinator.");

        var coordinator = new ChartUpdateCoordinator(_chartComputationEngine, _chartRenderEngine, _tooltipManager, _chartState.ChartTimestamps);

        coordinator.SeriesMode = ChartSeriesMode.RawAndSmoothed;

        return coordinator;
    }

    private WeekdayTrendChartUpdateCoordinator CreateWeekdayTrendChartUpdateCoordinator()
    {
        var renderingService = new WeekdayTrendRenderingService();

        return new WeekdayTrendChartUpdateCoordinator(renderingService, _chartState.ChartTimestamps);
    }

    private DistributionPolarRenderingService CreateDistributionPolarRenderingService()
    {
        return new DistributionPolarRenderingService();
    }

    private WeeklyDistributionService CreateWeeklyDistributionService()
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService);

        return new WeeklyDistributionService(_chartState.ChartTimestamps, strategyCutOverService);
    }

    private HourlyDistributionService CreateHourlyDistributionService()
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService);

        return new HourlyDistributionService(_chartState.ChartTimestamps, strategyCutOverService);
    }

    private ChartRenderingOrchestrator CreateChartRenderingOrchestrator()
    {
        var dataPreparationService = new DataPreparationService();
        _strategyCutOverService = new StrategyCutOverService(dataPreparationService);

        var metricSelectionService = new MetricSelectionService(_connectionString);
        return new ChartRenderingOrchestrator(_chartUpdateCoordinator, _weeklyDistributionService, _hourlyDistributionService, _strategyCutOverService, metricSelectionService, _connectionString);
    }

    #endregion

    #region Startup Helpers

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

        // Set initial resolution selection
        ResolutionCombo.SelectedItem = "All";

        _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);

        _viewModel.LoadMetricsCommand.Execute(null);
    }

    private void InitializeCharts()
    {
        _barPieSurface = new ChartPanelSurface(BarPieChartController.Panel);
        InitializeBarPieControls();
        InitializeDistributionControls();
        _weekdayTrendAdapter.InitializeControls();
        InitializeChartBehavior();
        ClearChartsOnStartup();
        DisableAxisLabelsWhenNoData();
        SetDefaultChartTitles();
        _distributionAdapter.UpdateChartTypeVisibility();
        InitializeDistributionPolarTooltip();
    }

    private void InitializeBarPieControls()
    {
        BarPieChartController.BucketCountCombo.Items.Clear();
        for (var i = 1; i <= 20; i++)
            BarPieChartController.BucketCountCombo.Items.Add(new ComboBoxItem
            {
                Content = i.ToString(),
                Tag = i
            });

        SelectBarPieBucketCount(_viewModel.ChartState.BarPieBucketCount);
    }

    private void SyncInitialButtonStates()
    {
        var selectedSubtypeCount = CountSelectedSubtypes(_viewModel.MetricState.SelectedSeries);
        UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);

        // Sync main chart button text with initial state
        MainChartController.Panel.IsChartVisible = _viewModel.ChartState.IsMainVisible;
        BarPieChartController.Panel.IsChartVisible = _isBarPieVisible;

        TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;
    }

    #endregion

    #region Initialization

    private void InitializeTooltips()
    {
        var parentWindow = Application.Current?.MainWindow ?? Window.GetWindow(this);
        if (parentWindow == null)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            throw new InvalidOperationException("Unable to resolve a parent window for tooltips.");
        }

        var chartLabels = new Dictionary<CartesianChart, string>
        {
                { MainChartController.Chart, "Main" },
                { NormalizedChartController.Chart, "Norm" },
                { DiffRatioChartController.Chart, "DiffRatio" },
                { TransformDataPanelController.ChartTransformResult, "Transform" }
        };

        _tooltipManager = new ChartTooltipManager(parentWindow, chartLabels);
        _tooltipManager.AttachChart(MainChartController.Chart, "Main");
        _tooltipManager.AttachChart(NormalizedChartController.Chart, "Norm");
        _tooltipManager.AttachChart(DiffRatioChartController.Chart, "DiffRatio");
        _tooltipManager.AttachChart(TransformDataPanelController.ChartTransformResult, "Transform");
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
        _viewModel.SetMainVisible(true); // Default to visible (Show on startup)
        _viewModel.SetNormalizedVisible(false);
        _viewModel.SetDiffRatioVisible(false);
        _viewModel.SetDistributionVisible(false);
        _isBarPieVisible = false;
        _viewModel.CompleteInitialization();

        _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
        _viewModel.ChartState.LastContext = new ChartDataContext();

        MainChartController.DisplayRegularRadio.IsChecked = _viewModel.ChartState.MainChartDisplayMode == MainChartDisplayMode.Regular;
        MainChartController.DisplaySummedRadio.IsChecked = _viewModel.ChartState.MainChartDisplayMode == MainChartDisplayMode.Summed;
        MainChartController.DisplayStackedRadio.IsChecked = _viewModel.ChartState.MainChartDisplayMode == MainChartDisplayMode.Stacked;

        // Initialize weekday trend chart type visibility
        _weekdayTrendAdapter.UpdateChartTypeVisibility();
    }

    private void InitializeSubtypeSelector()
    {
        _selectorManager = new SubtypeSelectorManager(TopControlMetricSubtypePanel, SubtypeCombo);

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

    private void InitializeDistributionControls()
    {
        _distributionAdapter.InitializeControls();
    }

    private void InitializeChartBehavior()
    {
        ChartHelper.InitializeChartBehavior(MainChartController.Chart);
        InitializeDistributionChartBehavior(DistributionChartController.Chart);
        ChartHelper.InitializeChartBehavior(NormalizedChartController.Chart);
        ChartHelper.InitializeChartBehavior(DiffRatioChartController.Chart);
    }

    private void InitializeDistributionPolarTooltip()
    {
        _distributionPolarTooltip = new ToolTip
        {
            Placement = PlacementMode.Mouse,
            StaysOpen = true
        };
        ToolTipService.SetToolTip(DistributionChartController.PolarChart, _distributionPolarTooltip);
        DistributionChartController.PolarChart.HoveredPointsChanged += OnDistributionPolarHoveredPointsChanged;
    }

    /// <summary>
    ///     Common method to initialize distribution chart behavior.
    /// </summary>
    private void InitializeDistributionChartBehavior(CartesianChart chart)
    {
        ChartHelper.InitializeChartBehavior(chart);
    }

    private void ClearChartsOnStartup()
    {
        // Clear charts on startup to prevent gibberish tick labels
        ChartHelper.ClearChart(MainChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        _normalizedAdapter.Clear(_viewModel.ChartState);
        _diffRatioAdapter.Clear(_viewModel.ChartState);
        _distributionAdapter.Clear(_viewModel.ChartState);
    }

    private void DisableAxisLabelsWhenNoData()
    {
        DisableAxisLabels(MainChartController.Chart);
        DisableAxisLabels(NormalizedChartController.Chart);
        DisableAxisLabels(DiffRatioChartController.Chart);
        DisableDistributionAxisLabels(DistributionChartController.Chart);
        DisableDistributionPolarAxisLabels();
    }

    /// <summary>
    ///     Common method to disable axis labels for distribution charts.
    /// </summary>
    private void DisableDistributionAxisLabels(CartesianChart chart)
    {
        DisableAxisLabels(chart);
    }

    private void DisableDistributionPolarAxisLabels()
    {
        DistributionChartController.PolarChart.AngleAxes = Array.Empty<PolarAxis>();
        DistributionChartController.PolarChart.RadiusAxes = Array.Empty<PolarAxis>();
        DistributionChartController.PolarChart.Tag = null;
        if (_distributionPolarTooltip != null)
            _distributionPolarTooltip.IsOpen = false;
    }

    private static void DisableAxisLabels(CartesianChart chart)
    {
        if (chart.AxisX.Count > 0)
            chart.AxisX[0].ShowLabels = false;
        if (chart.AxisY.Count > 0)
            chart.AxisY[0].ShowLabels = false;
    }

    private void SetDefaultChartTitles()
    {
        MainChartController.Panel.Title = "Metrics: Total";
        NormalizedChartController.Panel.Title = "Metrics: Normalized";
        DiffRatioChartController.Panel.Title = "Difference / Ratio";
        _diffRatioAdapter.UpdateOperationButton(); // Initialize button state
    }

    #endregion

    #region Data Loading and Selection Event Handlers

    /// <summary>
    ///     Event handler for Resolution selection change - reloads MetricTypes from the selected table.
    /// </summary>
    private void OnResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResolutionCombo.SelectedItem == null)
            return;

        // Store the selected resolution to retain it
        var selectedResolution = ResolutionCombo.SelectedItem.ToString();
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;

        ResetForResolutionChange(selectedResolution);

        // Restore the resolution selection (in case it was changed)
        if (ResolutionCombo.SelectedItem?.ToString() != selectedResolution)
            ResolutionCombo.SelectedItem = selectedResolution;
    }

    private void ResetForResolutionChange(string selectedResolution)
    {
        if (string.IsNullOrWhiteSpace(selectedResolution))
            return;

        // Set flag to suppress validation errors while resolution is being refreshed
        _isChangingResolution = true;

        ClearAllCharts();

        _viewModel.MetricState.SelectedMetricType = null;
        _viewModel.ChartState.LastContext = new ChartDataContext();

        TablesCombo.Items.Clear();

        _selectorManager.ClearDynamic();
        SubtypeCombo.Items.Clear();
        SubtypeCombo.IsEnabled = false;

        _viewModel.MetricState.ResolutionTableName = ChartHelper.GetTableNameFromResolution(ResolutionCombo);
        _viewModel.LoadMetricsCommand.Execute(null);

        // Ensure chart buttons reflect the absence of subtypes
        UpdatePrimaryDataRequiredButtonStates(0);
        UpdateSecondaryDataRequiredButtonStates(0);
    }

    /// <summary>
    ///     Event handler for MetricType selection change - loads subtypes and updates date range.
    /// </summary>
    private void OnMetricTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        if (_viewModel.UiState.IsLoadingMetricTypes)
            return;

        _isMetricTypeChangePending = true;
        _viewModel.SetSelectedMetricType(GetSelectedMetricValue(TablesCombo));
        _viewModel.LoadSubtypesCommand.Execute(null);
    }

    /// <summary>
    ///     Generalized event handler for any MetricSubtype ComboBox selection change - updates date range to match data
    ///     availability.
    ///     This handler is used by all subtype ComboBoxes (both static and dynamically added).
    /// </summary>
    private async void OnAnySubtypeSelectionChanged(object? sender, SelectionChangedEventArgs? e)
    {
        UpdateSelectedSubtypesInViewModel();

        if (!string.IsNullOrWhiteSpace(_viewModel.MetricState.SelectedMetricType))
            await LoadDateRangeForSelectedMetrics();
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
        var selectedMetricType = GetSelectedMetricValue(TablesCombo);
        if (selectedMetricType == null)
        {
            MessageBox.Show("Please select a Metric Type", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return Task.FromResult(false);
        }

        _viewModel.SetSelectedMetricType(selectedMetricType);
        UpdateSelectedSubtypesInViewModel();

        var selections = GetDistinctSelectedSeries();
        var display1 = selections.Count > 0 ? selections[0].DisplayName : string.Empty;
        var display2 = selections.Count > 1 ? selections[1].DisplayName : string.Empty;

        SetChartTitles(display1, display2);
        UpdateChartLabels();

        var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;
        _viewModel.SetDateRange(fromDate, toDate);

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
            _distributionAdapter.ClearCache();
            _weekdayTrendAdapter.ClearCache();
            _normalizedAdapter.ClearCache();
            _diffRatioAdapter.ClearCache();
            _transformAdapter.ClearCache();
            _transformAdapter.ResetSelectionsPendingLoad();
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
        if (_subtypeList == null || !_subtypeList.Any())
            return;

        var metricType = GetSelectedMetricOption(TablesCombo);
        var newCombo = _selectorManager.AddSubtypeCombo(_subtypeList, metricType);
        newCombo.SelectedIndex = 0;
        newCombo.IsEnabled = true;

        UpdateChartTitlesFromCombos();
        UpdateSelectedSubtypesInViewModel();
    }

    #endregion

    #region Chart Visibility Toggle Handlers

    private void OnChartMainToggle(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleMain();
    }

    private void OnMainChartToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleMain();
    }

    private async void OnBarPieToggleRequested(object? sender, EventArgs e)
    {
        _isBarPieVisible = !_isBarPieVisible;
        await RenderBarPieChartAsync();
    }

    private async void OnBarPieDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        await RenderBarPieChartAsync();
    }

    private async void OnBarPieBucketCountChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        if (BarPieChartController.BucketCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var bucketCount))
            _viewModel.ChartState.BarPieBucketCount = bucketCount;

        if (_isBarPieVisible)
            await RenderBarPieChartAsync();
    }


    #endregion

    #region Chart Configuration and Helper Methods

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
        var mainChart = MainChartController.Chart;
        ChartHelper.ResetZoom(mainChart);
        _normalizedAdapter.ResetZoom();
        _diffRatioAdapter.ResetZoom();
        _distributionAdapter.ResetZoom();
        _transformAdapter.ResetZoom();
        _weekdayTrendAdapter.ResetZoom();
    }

    private void OnClear(object sender, RoutedEventArgs e)
    {
        const string defaultResolution = "All";

        // Clear selection state and disable chart toggles immediately on reset.
        _viewModel.SetSelectedSeries(Array.Empty<MetricSeriesSelection>());
        _viewModel.ChartState.LastContext = new ChartDataContext();
        UpdatePrimaryDataRequiredButtonStates(0);
        UpdateSecondaryDataRequiredButtonStates(0);

        if (ResolutionCombo.SelectedItem?.ToString() == defaultResolution)
        {
            ResetForResolutionChange(defaultResolution);
            return;
        }

        ResolutionCombo.SelectedItem = defaultResolution;
    }

    /// <summary>
    ///     Handles hover updates for the distribution polar tooltip.
    /// </summary>
    private void OnDistributionPolarHoveredPointsChanged(IChartView chart, IEnumerable<ChartPoint>? newPoints, IEnumerable<ChartPoint>? oldPoints)
    {
        if (_distributionPolarTooltip == null)
            return;

        var state = DistributionChartController.PolarChart.Tag as DistributionPolarTooltipState;
        if (state == null || newPoints == null)
        {
            _distributionPolarTooltip.IsOpen = false;
            return;
        }

        var point = newPoints.FirstOrDefault();
        if (point == null)
        {
            _distributionPolarTooltip.IsOpen = false;
            return;
        }

        var bucketCount = state.Definition.XAxisLabels.Count;
        if (bucketCount <= 0)
        {
            _distributionPolarTooltip.IsOpen = false;
            return;
        }

        var rawIndex = (int)Math.Round(point.Coordinate.SecondaryValue);
        var index = rawIndex % bucketCount;
        if (index < 0)
            index += bucketCount;

        var label = state.Definition.XAxisLabels[index];
        var minRaw = state.RangeResult.Mins[index];
        var maxRaw = state.RangeResult.Maxs[index];
        var minValue = FormatDistributionValue(minRaw, state.RangeResult.Unit);
        var maxValue = FormatDistributionValue(maxRaw, state.RangeResult.Unit);
        var avgValue = FormatDistributionValue(state.RangeResult.Averages[index], state.RangeResult.Unit);
        var diffValue = FormatDistributionValue(maxRaw - minRaw, state.RangeResult.Unit);

        _distributionPolarTooltip.Content = $"{label}\nMin: {minValue}\nMax: {maxValue}\nAvg: {avgValue}\nΔ: {diffValue}";
        _distributionPolarTooltip.IsOpen = true;
    }

    private static string FormatDistributionValue(double value, string? unit)
    {
        if (double.IsNaN(value))
            return "n/a";

        var formatted = MathHelper.FormatDisplayedValue(value);
        if (!string.IsNullOrWhiteSpace(unit))
            formatted = $"{formatted} {unit}";

        return formatted;
    }

    private void SetChartTitles(string leftName, string rightName)
    {
        leftName ??= string.Empty;
        rightName ??= string.Empty;

        _viewModel.ChartState.LeftTitle = leftName;
        _viewModel.ChartState.RightTitle = rightName;

        MainChartController.Panel.Title = $"{leftName} vs. {rightName}";
        NormalizedChartController.Panel.Title = $"{leftName} ~ {rightName}";
        DiffRatioChartController.Panel.Title = $"{leftName} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {rightName}";
    }

    private void UpdateChartLabels()
    {
        if (_tooltipManager == null)
            return;

        var selections = GetDistinctSelectedSeries();
        var primary = selections.Count > 0 ? selections[0] : null;
        var secondary = selections.Count > 1 ? selections[1] : null;

        var label1 = primary?.DisplayName ?? string.Empty;
        var label2 = secondary?.DisplayName ?? string.Empty;

        var chartMainLabel = !string.IsNullOrEmpty(label2) ? $"{label1} vs {label2}" : label1;
        _tooltipManager.UpdateChartLabel(MainChartController.Chart, chartMainLabel);

        var chartDiffRatioLabel = !string.IsNullOrEmpty(label2) ? $"{label1} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {label2}" : label1;
        _tooltipManager.UpdateChartLabel(DiffRatioChartController.Chart, chartDiffRatioLabel);
    }

    private void UpdateChartTitlesFromCombos()
    {
        var selections = GetDistinctSelectedSeries();
        var display1 = selections.Count > 0 ? selections[0].DisplayName : string.Empty;
        var display2 = selections.Count > 1 ? selections[1].DisplayName : string.Empty;

        SetChartTitles(display1, display2);
        UpdateChartLabels();
    }

    private async Task RenderBarPieChartAsync()
    {
        if (_barPieSurface == null)
            _barPieSurface = new ChartPanelSurface(BarPieChartController.Panel);

        if (!_isBarPieVisible)
        {
            _barPieRenderer.Apply(_barPieSurface, CreateEmptyBarPieModel());
            return;
        }

        var isPieMode = BarPieChartController.PieModeRadio.IsChecked == true;
        var model = await BuildBarPieRenderModelAsync(isPieMode);
        _barPieRenderer.Apply(_barPieSurface, model);
    }

    private async Task<UiChartRenderModel> BuildBarPieRenderModelAsync(bool isPieMode)
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return CreateEmptyBarPieModel();

        if (!TryResolveBarPieDateRange(out var from, out var to))
            return CreateEmptyBarPieModel();

        var bucketCount = ResolveBarPieBucketCount(from, to);
        var bucketPlan = BuildBarPieBucketPlan(from, to, bucketCount);

        var seriesTotals = await LoadBarPieSeriesTotalsAsync(selections, from, to, bucketPlan);
        if (seriesTotals.Count == 0)
            return CreateEmptyBarPieModel();

        var paletteKey = BarPieChartController;
        ColourPalette.Reset(paletteKey);

        var coloredSeries = seriesTotals.Select(data => new BarPieSeriesValues(
            data.Selection,
            data.Totals,
            ColourPalette.Next(paletteKey))).ToList();

        if (isPieMode)
        {
            var facets = bucketPlan.Buckets.Select(bucket =>
            {
                var series = coloredSeries.Select(item => new ChartSeriesModel
                {
                    Name = item.Selection.DisplayName,
                    SeriesType = ChartSeriesType.Pie,
                    Values = new double?[] { item.Totals[bucket.Index] },
                    Color = item.Color
                }).ToList();

                return new ChartFacetModel
                {
                    Title = bucket.Label,
                    Series = series
                };
            }).ToList();

            return new UiChartRenderModel
            {
                Title = ChartUiDefaults.BarPieChartTitle,
                IsVisible = _isBarPieVisible,
                Facets = facets,
                Legend = new ChartLegendModel
                {
                    IsVisible = true,
                    Placement = ChartLegendPlacement.Right
                },
                Interactions = new ChartInteractionModel
                {
                    Hoverable = ChartUiDefaults.DefaultHoverable
                }
            };
        }

        var barSeries = coloredSeries.Select(item => new ChartSeriesModel
        {
            Name = item.Selection.DisplayName,
            SeriesType = ChartSeriesType.Column,
            Values = item.Totals,
            Color = item.Color
        }).ToList();

        return new UiChartRenderModel
        {
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _isBarPieVisible,
            Series = barSeries,
            AxesX = new[]
            {
                new ChartAxisModel
                {
                    Title = "Interval",
                    Labels = bucketPlan.Buckets.Select(bucket => bucket.Label).ToList()
                }
            },
            AxesY = new[]
            {
                new ChartAxisModel
                {
                    Title = "Value"
                }
            },
            Legend = new ChartLegendModel
            {
                IsVisible = true,
                Placement = ChartLegendPlacement.Right
            },
            Interactions = new ChartInteractionModel
            {
                EnableZoomX = true,
                EnablePanX = true,
                Hoverable = ChartUiDefaults.DefaultHoverable
            }
        };
    }

    private UiChartRenderModel CreateEmptyBarPieModel()
    {
        return new UiChartRenderModel
        {
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _isBarPieVisible,
            Series = Array.Empty<ChartSeriesModel>(),
            Facets = Array.Empty<ChartFacetModel>()
        };
    }

    private async Task<IReadOnlyList<BarPieSeriesTotals>> LoadBarPieSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BarPieBucketPlan plan)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadBarPieSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<BarPieSeriesTotals?> LoadBarPieSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BarPieBucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new BarPieSeriesTotals(selection, BuildBucketTotals(primaryCms, plan));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new BarPieSeriesTotals(selection, BuildBucketTotals(fallbackLegacy, plan));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new BarPieSeriesTotals(selection, BuildBucketTotals(dataList, plan));
        }
        catch
        {
            return null;
        }
    }

    private int ResolveBarPieBucketCount(DateTime from, DateTime to)
    {
        const int bucketMax = 20;

        if (to <= from)
            return 1;

        var bucketCount = _viewModel.ChartState.BarPieBucketCount;
        return Math.Max(1, Math.Min(bucketCount, bucketMax));
    }

    private static BarPieBucketPlan BuildBarPieBucketPlan(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<BarPieBucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1
                ? to.Ticks
                : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new BarPieBucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new BarPieBucketPlan(from, to, bucketTicks, buckets);
    }

    private static double?[] BuildBucketTotals(IReadOnlyList<MetricData> data, BarPieBucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var index = ResolveBucketIndex(point.NormalizedTimestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)point.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static double?[] BuildBucketTotals(ICanonicalMetricSeries series, BarPieBucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var sample in series.Samples)
        {
            if (!sample.Value.HasValue)
                continue;

            var timestamp = sample.Timestamp.UtcDateTime;
            var index = ResolveBucketIndex(timestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)sample.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static int ResolveBucketIndex(DateTime timestamp, BarPieBucketPlan plan)
    {
        if (timestamp < plan.From || timestamp > plan.To)
            return -1;

        if (plan.BucketTicks <= 0)
            return 0;

        var offsetTicks = timestamp.Ticks - plan.From.Ticks;
        var index = (int)Math.Floor(offsetTicks / plan.BucketTicks);
        if (index < 0)
            return -1;
        if (index >= plan.Buckets.Count)
            return plan.Buckets.Count - 1;

        return index;
    }

    private bool TryResolveBarPieDateRange(out DateTime from, out DateTime to)
    {
        if (_viewModel.MetricState.FromDate.HasValue && _viewModel.MetricState.ToDate.HasValue)
        {
            from = _viewModel.MetricState.FromDate.Value;
            to = _viewModel.MetricState.ToDate.Value;
            return true;
        }

        var context = _viewModel.ChartState.LastContext;
        if (context != null && context.From != default && context.To != default)
        {
            from = context.From;
            to = context.To;
            return true;
        }

        from = default;
        to = default;
        return false;
    }

    private sealed record BarPieSeriesTotals(MetricSeriesSelection Selection, double?[] Totals);

    private sealed record BarPieSeriesValues(MetricSeriesSelection Selection, double?[] Totals, Color Color);

    private sealed record BarPieBucket(int Index, DateTime Start, DateTime End, string Label);

    private sealed record BarPieBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<BarPieBucket> Buckets);

    #endregion

    #region Distribution chart display mode UI handling

    private void SelectBarPieBucketCount(int bucketCount)
    {
        foreach (var item in BarPieChartController.BucketCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == bucketCount)
            {
                BarPieChartController.BucketCountCombo.SelectedItem = item;
                return;
            }
    }

    private static bool TryGetIntervalCount(object? tag, out int intervalCount)
    {
        switch (tag)
        {
            case int direct:
                intervalCount = direct;
                return true;
            case string tagValue when int.TryParse(tagValue, out var parsed):
                intervalCount = parsed;
                return true;
            default:
                intervalCount = 0;
                return false;
        }
    }

    #endregion
}
