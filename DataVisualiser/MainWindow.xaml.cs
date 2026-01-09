using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Models;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Controllers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.SubtypeSelectors;
using DataVisualiser.UI.ViewModels;
using LiveChartsCore.Kernel;
using LiveCharts.Wpf;
using ChartHelper = DataVisualiser.Core.Rendering.Helpers.ChartHelper;

namespace DataVisualiser;

public partial class MainWindow : Window
{
    private readonly ChartState _chartState = new();
    private readonly MetricState _metricState = new();
    private readonly UiState _uiState = new();
    private ChartComputationEngine _chartComputationEngine = null!;
    private ChartRenderEngine _chartRenderEngine = null!;
    private ChartRenderingOrchestrator? _chartRenderingOrchestrator;
    private ChartUpdateCoordinator _chartUpdateCoordinator = null!;
    private DistributionPolarRenderingService _distributionPolarRenderingService = null!;
    private ToolTip _distributionPolarTooltip = null!;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _distributionSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isUpdatingDistributionSubtypeCombo;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _weekdayTrendSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isUpdatingWeekdayTrendSubtypeCombo;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _transformSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private bool _isUpdatingTransformSubtypeCombos;
    private bool _isTransformSelectionPendingLoad;

    private string _connectionString = null!;
    private HourlyDistributionService _hourlyDistributionService = null!;
    private bool _isChangingResolution;

    private bool _isInitializing = true;

    private MetricSelectionService _metricSelectionService = null!;
    private SubtypeSelectorManager _selectorManager = null!;
    private IStrategyCutOverService? _strategyCutOverService;
    private List<string>? _subtypeList;
    private ChartTooltipManager? _tooltipManager;
    private MainWindowViewModel _viewModel = null!;
    private WeekdayTrendChartUpdateCoordinator _weekdayTrendChartUpdateCoordinator = null!;
    private WeeklyDistributionService _weeklyDistributionService = null!;

    public MainWindow()
    {
        InitializeComponent();

        InitializeWindowLayout();

        InitializeInfrastructure();
        InitializeChartPipeline();
        InitializeViewModel();
        InitializeUiBindings();

        ExecuteStartupSequence();

        RegisterLifecycleEvents();
    }


    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        // Dispose tooltip manager to prevent memory leaks
        _tooltipManager?.Dispose();
        _tooltipManager = null;
    }

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

            if (_viewModel.ChartState.IsNormalizedVisible && _viewModel.ChartState.LastContext?.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
            {
                var ctx = _viewModel.ChartState.LastContext;

                var normalizedStrategy = CreateNormalizedStrategy(ctx, ctx.Data1, ctx.Data2, ctx.DisplayName1, ctx.DisplayName2, ctx.From, ctx.To, _viewModel.ChartState.SelectedNormalizationMode);
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(ChartNorm, normalizedStrategy, $"{ctx.DisplayName1} ~ {ctx.DisplayName2}", minHeight: 400);
            }
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
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
            if (combo.SelectedItem == null)
                continue;

            var st = combo.SelectedItem.ToString();
            if (string.IsNullOrWhiteSpace(st))
                continue;
            if (st == "(All)")
                continue;
            if (selectedSubtypes.Contains(st))
                continue;

            selectedSubtypes.Add(st);
        }

        _viewModel.SetSelectedSubtypes(selectedSubtypes);
        UpdateDistributionSubtypeOptions();
        UpdateWeekdayTrendSubtypeOptions();

        // Update button states based on selected subtype count
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypes.Count);
    }

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require secondary data.
    ///     These buttons are disabled when fewer than 2 subtypes are selected.
    ///     If charts are currently visible when secondary data becomes unavailable, they are cleared and hidden
    ///     by leveraging the existing rendering pipeline through state updates.
    ///     Pipeline flow:
    ///     1. ViewModel.SetXxxVisible(false) -> Sets ChartState.IsXxxVisible = false
    ///     2. RequestChartUpdate() -> Fires ChartUpdateRequested event with updated visibility states
    ///     3. OnChartUpdateRequested() -> Calls UpdateChartVisibility() for each chart
    ///     4. UpdateChartVisibility() -> Updates panel visibility and button text ("Show"/"Hide")
    ///     5. RenderChartsFromLastContext() -> Checks visibility states and clears/renders charts accordingly
    ///     - Uses RenderOrClearChart() which respects visibility state
    ///     - Or directly clears when no secondary data exists
    /// </summary>
    private void UpdateSecondaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        var hasSecondaryData = selectedSubtypeCount >= 2;

        // If secondary data is no longer available, use the ViewModel state setters to trigger
        // the full rendering pipeline. This ensures all state updates, UI updates, and chart
        // clearing happen through the established pipeline with proper strategy adoption.
        if (!hasSecondaryData)
        {
            // Update state through ViewModel - this triggers the full pipeline:
            // ViewModel -> RequestChartUpdate -> OnChartUpdateRequested -> UpdateChartVisibility -> RenderChartsFromLastContext
            if (_viewModel.ChartState.IsNormalizedVisible)
                _viewModel.SetNormalizedVisible(false);

            if (_viewModel.ChartState.IsDiffRatioVisible)
                _viewModel.SetDiffRatioVisible(false);
        }

        // Update button enabled states (this is UI-only, not part of the rendering pipeline)
        ChartNormToggleButton.IsEnabled = hasSecondaryData;
        DiffRatioChartController.ToggleButton.IsEnabled = hasSecondaryData;
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

        _subtypeList = subtypeListLocal;

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
        if (bool.TryParse(showDebugPopup, out var showDebug) && showDebug)
        {
            var data1 = ctx.Data1.ToList();
            var data2 = ctx.Data2?.ToList() ?? new List<MetricData>();

            var msg = $"Data1 count: {data1.Count}\n" + $"Data2 count: {data2.Count}\n" + $"First 3 timestamps (Data1):\n" + string.Join("\n", data1.Take(3).Select(d => d.NormalizedTimestamp)) + "\n\nFirst 3 timestamps (Data2):\n" + string.Join("\n", data2.Take(3).Select(d => d.NormalizedTimestamp));

            MessageBox.Show(msg, "DEBUG - LastContext contents");
        }

        _isTransformSelectionPendingLoad = false;
        UpdateTransformSubtypeOptions();
        UpdateTransformComputeButtonState();

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
                UpdateChartVisibility(ChartNormContentPanel, ChartNormToggleButton, e.ShowNormalized);
                break;
            case "DiffRatio":
                DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
                break;
            case "Distribution":
                UpdateChartVisibility(ChartDistributionContentPanel, ChartDistributionToggleButton, e.ShowDistribution);
                UpdateDistributionChartTypeVisibility();
                break;
            case "WeeklyTrend":
                WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
                UpdateWeekdayTrendChartTypeVisibility();
                break;
            case "Transform":
                TransformDataPanelController.Panel.IsChartVisible = e.ShowTransformPanel;
                break;
        }
    }

    private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
    {
        if (e.IsVisibilityOnlyToggle && !string.IsNullOrEmpty(e.ToggledChartName))
        {
            UpdateChartVisibilityForToggle(e);

            // Transform panel visibility toggle - just update visibility, don't reload charts
            if (e.ToggledChartName == "Transform")
            {
                // Only populate grids if panel is being shown and we have data
                if (_viewModel.ChartState.IsTransformPanelVisible)
                {
                    var transformCtx = _viewModel.ChartState.LastContext;
                    if (transformCtx != null && ShouldRenderCharts(transformCtx))
                        PopulateTransformGrids(transformCtx);
                    UpdateTransformSubtypeOptions();
                }

                return; // Don't reload other charts
            }

            var ctx = _viewModel.ChartState.LastContext;
            if (ctx != null && ShouldRenderCharts(ctx))
                await RenderSingleChart(e.ToggledChartName, ctx);

            return;
        }

        // Update visibility for all charts (just UI state, doesn't clear data)
        MainChartController.Panel.IsChartVisible = e.ShowMain;
        UpdateChartVisibility(ChartNormContentPanel, ChartNormToggleButton, e.ShowNormalized);
        DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
        UpdateChartVisibility(ChartDistributionContentPanel, ChartDistributionToggleButton, e.ShowDistribution);
        UpdateDistributionChartTypeVisibility();
        WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
        UpdateWeekdayTrendChartTypeVisibility();
        TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;

        // If a specific chart was identified (visibility toggle or chart-specific config change), only render that chart
        if (!string.IsNullOrEmpty(e.ToggledChartName))
        {
            // Transform panel visibility toggle - just update visibility, don't reload charts
            if (e.ToggledChartName == "Transform" && e.IsVisibilityOnlyToggle)
            {
                // Only populate grids if panel is being shown and we have data
                if (_viewModel.ChartState.IsTransformPanelVisible)
                {
                    var transformCtx = _viewModel.ChartState.LastContext;
                    if (transformCtx != null && ShouldRenderCharts(transformCtx))
                        PopulateTransformGrids(transformCtx);
                    UpdateTransformSubtypeOptions();
                }

                return; // Don't reload other charts
            }

            var ctx = _viewModel.ChartState.LastContext;
            if (ctx != null && ShouldRenderCharts(ctx))
                await RenderSingleChart(e.ToggledChartName, ctx);
        }
        // Otherwise, render all charts (data change scenario)
        else if (e.ShouldRenderCharts && !e.IsVisibilityOnlyToggle)
        {
            await RenderChartsFromLastContext();
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

        await _chartRenderingOrchestrator.RenderPrimaryChartAsync(ctx, MainChartController.Chart, data1, data2, displayName1, displayName2, from, to, metricType, _viewModel.MetricState.SelectedSubtypes, _viewModel.MetricState.ResolutionTableName);
    }


    /// <summary>
    ///     Selects the appropriate computation strategy based on the number of series.
    ///     Returns the strategy and secondary label (if applicable).
    /// </summary>
    private(IChartComputationStrategy strategy, string? secondaryLabel) SelectComputationStrategy(List<IEnumerable<MetricData>> series, List<string> labels, DateTime from, DateTime to)
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
            await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(chart, strategy, title, minHeight: minHeight, metricType: metricType, primarySubtype: primarySubtype, secondarySubtype: secondarySubtype, operationType: operationType, isOperationChart: isOperationChart);
        // Note: We don't clear the chart when hiding - just hide the panel to preserve data
        // Charts are only cleared when data changes (e.g., new selection, resolution change, etc.)
    }

    private void RenderWeekdayTrendChart(WeekdayTrendResult result)
    {
        _weekdayTrendChartUpdateCoordinator.UpdateChart(result, _viewModel.ChartState, WeekdayTrendChartController.Chart, WeekdayTrendChartController.PolarChart);
    }

    private async Task RenderChartsFromLastContext()
    {
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
                await RenderNormalized(safeCtx, metricType, primarySubtype, secondarySubtype);

            if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                await RenderDiffRatio(safeCtx, metricType, primarySubtype, secondarySubtype);
        }
        else
        {
            // Clear charts that require secondary data when no secondary data exists
            ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
            ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        }

        // Charts that don't require secondary data - only render if visible
        if (_viewModel.ChartState.IsDistributionVisible)
            await RenderDistributionChart(safeCtx, _viewModel.ChartState.SelectedDistributionMode);

        if (_viewModel.ChartState.IsWeeklyTrendVisible)
            await RenderWeeklyTrendAsync(safeCtx);

        // Populate transform panel grids if visible
        if (_viewModel.ChartState.IsTransformPanelVisible)
            PopulateTransformGrids(safeCtx);
    }

    private void PopulateTransformGrids(ChartDataContext ctx)
    {
        PopulateTransformGrid(ctx.Data1, TransformDataPanelController.TransformGrid1, TransformDataPanelController.TransformGrid1Title, ctx.DisplayName1 ?? "Primary Data", true);

        var hasSecondary = HasSecondaryData(ctx) && !string.IsNullOrEmpty(ctx.SecondarySubtype) && ctx.Data2 != null;

        if (hasSecondary)
        {
            TransformDataPanelController.TransformGrid2Panel.Visibility = Visibility.Visible;

            PopulateTransformGrid(ctx.Data2, TransformDataPanelController.TransformGrid2, TransformDataPanelController.TransformGrid2Title, ctx.DisplayName2 ?? "Secondary Data", false);

            SetBinaryTransformOperationsEnabled(true);
        }
        else
        {
            TransformDataPanelController.TransformGrid2Panel.Visibility = Visibility.Collapsed;
            TransformDataPanelController.TransformGrid2.ItemsSource = null;
            SetBinaryTransformOperationsEnabled(false);
        }

        ResetTransformResultState();
    }

    private void PopulateTransformGrid(IEnumerable<MetricData>? data, DataGrid grid, TextBlock title, string titleText, bool alwaysVisible)
    {
        if (data == null && !alwaysVisible)
            return;

        var rows = data?.Where(d => d.Value.HasValue)
                       .OrderBy(d => d.NormalizedTimestamp)
                       .Select(d => new
                       {
                               Timestamp = d.NormalizedTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                               Value = d.Value!.Value.ToString("F4")
                       })
                       .ToList();

        grid.ItemsSource = rows;
        title.Text = titleText;

        if (grid.Columns.Count >= 2)
        {
            grid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            grid.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }
    }

    private void SetBinaryTransformOperationsEnabled(bool enabled)
    {
        var binaryItems = TransformDataPanelController.TransformOperationCombo.Items.Cast<ComboBoxItem>().Where(i => i.Tag?.ToString() == "Add" || i.Tag?.ToString() == "Subtract");

        foreach (var item in binaryItems)
            item.IsEnabled = enabled;
    }

    private void ResetTransformResultState()
    {
        TransformDataPanelController.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        TransformDataPanelController.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        TransformDataPanelController.TransformGrid3.ItemsSource = null;
        TransformDataPanelController.TransformComputeButton.IsEnabled = false;
    }

    private void ResetTransformSelectionsPendingLoad()
    {
        _isTransformSelectionPendingLoad = true;
        _viewModel.ChartState.SelectedTransformPrimarySubtype = null;
        _viewModel.ChartState.SelectedTransformSecondarySubtype = null;

        if (TransformDataPanelController.TransformPrimarySubtypeCombo == null || TransformDataPanelController.TransformSecondarySubtypeCombo == null)
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Clear();
            TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Clear();
            TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = false;
            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = null;
            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
            TransformDataPanelController.TransformOperationCombo.SelectedItem = null;
            TransformDataPanelController.TransformComputeButton.IsEnabled = false;
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    private void UpdateTransformSubtypeOptions()
    {
        if (TransformDataPanelController.TransformPrimarySubtypeCombo == null || TransformDataPanelController.TransformSecondarySubtypeCombo == null)
            return;

        if (_isTransformSelectionPendingLoad)
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Clear();
            TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Clear();

            var selectedSubtypes = _viewModel.MetricState.SelectedSubtypes;
            if (selectedSubtypes.Count == 0)
            {
                TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = false;
                TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
                TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
                _viewModel.ChartState.SelectedTransformPrimarySubtype = null;
                _viewModel.ChartState.SelectedTransformSecondarySubtype = null;
                TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = null;
                TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
                return;
            }

            foreach (var subtype in selectedSubtypes)
            {
                TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Add(subtype);
                TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Add(subtype);
            }

            TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = true;

            var primaryCurrent = _viewModel.ChartState.SelectedTransformPrimarySubtype;
            var primarySelection = primaryCurrent != null && selectedSubtypes.Contains(primaryCurrent) ? primaryCurrent : selectedSubtypes[0];
            TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = primarySelection;
            _viewModel.ChartState.SelectedTransformPrimarySubtype = primarySelection;

            if (selectedSubtypes.Count > 1)
            {
                TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
                TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = true;

                var secondaryCurrent = _viewModel.ChartState.SelectedTransformSecondarySubtype;
                var secondarySelection = secondaryCurrent != null && selectedSubtypes.Contains(secondaryCurrent) ? secondaryCurrent : selectedSubtypes[1];
                TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = secondarySelection;
                _viewModel.ChartState.SelectedTransformSecondarySubtype = secondarySelection;
            }
            else
            {
                TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
                TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
                TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
                _viewModel.ChartState.SelectedTransformSecondarySubtype = null;
            }

            UpdateTransformComputeButtonState();
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
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
                    await RenderNormalized(ctx, metricType, primarySubtype, secondarySubtype);
                break;

            case "DiffRatio":
                if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                    await RenderDiffRatio(ctx, metricType, primarySubtype, secondarySubtype);
                break;

            case "Distribution":
                if (_viewModel.ChartState.IsDistributionVisible)
                    await RenderDistributionChart(ctx, _viewModel.ChartState.SelectedDistributionMode);
                break;

            case "WeeklyTrend":
                if (_viewModel.ChartState.IsWeeklyTrendVisible)
                    await RenderWeeklyTrendAsync(ctx);
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

        await RenderNormalized(ctx, metricType, primarySubtype, secondarySubtype);
        await RenderDistributionChart(ctx, _viewModel.ChartState.SelectedDistributionMode);
        await RenderWeeklyTrendAsync(ctx);
        await RenderDiffRatio(ctx, metricType, primarySubtype, secondarySubtype);
    }

    private void ClearSecondaryChartsAndReturn()
    {
        ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(ChartDistribution);
        // NOTE: WeekdayTrend intentionally not cleared here to preserve current behavior (tied to secondary presence).
        // Both Cartesian and Polar versions are handled by RenderWeekdayTrendChart which checks visibility.
    }

    private Task RenderNormalized(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
    {
        if (_chartRenderingOrchestrator == null)
            return Task.CompletedTask;

        return _chartRenderingOrchestrator.RenderNormalizedChartAsync(ctx, ChartNorm, _viewModel.ChartState);
    }

    /// <summary>
    ///     Common method to render the distribution chart for the selected mode.
    /// </summary>
    private async Task RenderDistributionChart(ChartDataContext ctx, DistributionMode mode)
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
            return;

        var selectedSubtype = ResolveSelectedDistributionSubtype(ctx);
        var data = await ResolveDistributionDataAsync(ctx, selectedSubtype);
        if (data == null || data.Count == 0)
            return;

        var displayName = ResolveDistributionDisplayName(ctx, selectedSubtype);

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            await RenderDistributionPolarChart(ctx, mode, data, displayName);
            return;
        }

        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        var chart = ChartDistribution;

        if (_chartRenderingOrchestrator != null)
        {
            var distributionContext = new ChartDataContext
            {
                    Data1 = data,
                    DisplayName1 = displayName,
                    MetricType = ctx.MetricType,
                    PrimarySubtype = selectedSubtype,
                    From = ctx.From,
                    To = ctx.To
            };
            await _chartRenderingOrchestrator.RenderDistributionChartAsync(distributionContext, chart, _viewModel.ChartState, mode);
            return;
        }

        var service = GetDistributionService(mode);
        await service.UpdateDistributionChartAsync(chart, data, displayName, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount);
        // Note: We don't clear the chart when hiding - just hide the panel to preserve data
    }

    private async Task RenderWeeklyTrendAsync(ChartDataContext ctx)
    {
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
            return;

        var selectedSubtype = ResolveSelectedWeekdayTrendSubtype(ctx);
        var data = await ResolveWeekdayTrendDataAsync(ctx, selectedSubtype);
        if (data == null || data.Count == 0)
            return;

        var displayName = ResolveWeekdayTrendDisplayName(ctx, selectedSubtype);
        var trendContext = new ChartDataContext
        {
                Data1 = data,
                DisplayName1 = displayName,
                MetricType = ctx.MetricType,
                PrimarySubtype = selectedSubtype,
                From = ctx.From,
                To = ctx.To
        };

        var result = ComputeWeekdayTrend(trendContext);
        if (result != null)
            RenderWeekdayTrendChart(result);
        // Note: We don't clear the chart when hiding - just hide the panel to preserve data
    }

    private async Task RenderDistributionPolarChart(ChartDataContext ctx, DistributionMode mode, IReadOnlyList<MetricData> data, string displayName)
    {
        var service = GetDistributionService(mode);
        var rangeResult = await service.ComputeSimpleRangeAsync(data, displayName, ctx.From, ctx.To);
        if (rangeResult == null)
            return;

        var definition = DistributionModeCatalog.Get(mode);
        _distributionPolarRenderingService.RenderPolarChart(rangeResult, definition, ChartDistributionPolar);
        ChartDistributionPolar.Tag = new DistributionPolarTooltipState(definition, rangeResult);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDistributionDataAsync(ChartDataContext ctx, string? selectedSubtype)
    {
        if (ctx.Data1 == null)
            return null;

        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data2 ?? ctx.Data1;

        var metricType = ctx.MetricType ?? _viewModel.MetricState.SelectedMetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildDistributionCacheKey(metricType, selectedSubtype, ctx.From, ctx.To, tableName);
        if (_distributionSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(metricType, selectedSubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _distributionSubtypeCache[cacheKey] = data;
        return data;
    }

    private string? ResolveSelectedDistributionSubtype(ChartDataContext ctx)
    {
        var selectedSubtype = _viewModel.ChartState.SelectedDistributionSubtype;
        if (!string.IsNullOrWhiteSpace(selectedSubtype))
            return selectedSubtype;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype))
            return ctx.PrimarySubtype;

        return null;
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, string? selectedSubtype)
    {
        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName2;

        return selectedSubtype;
    }

    private static string BuildDistributionCacheKey(string metricType, string subtype, DateTime from, DateTime to, string tableName)
    {
        return $"{metricType}|{subtype}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveWeekdayTrendDataAsync(ChartDataContext ctx, string? selectedSubtype)
    {
        if (ctx.Data1 == null)
            return null;

        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data2 ?? ctx.Data1;

        var metricType = ctx.MetricType ?? _viewModel.MetricState.SelectedMetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildWeekdayTrendCacheKey(metricType, selectedSubtype, ctx.From, ctx.To, tableName);
        if (_weekdayTrendSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(metricType, selectedSubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _weekdayTrendSubtypeCache[cacheKey] = data;
        return data;
    }

    private string? ResolveSelectedWeekdayTrendSubtype(ChartDataContext ctx)
    {
        var selectedSubtype = _viewModel.ChartState.SelectedWeekdayTrendSubtype;
        if (!string.IsNullOrWhiteSpace(selectedSubtype))
            return selectedSubtype;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype))
            return ctx.PrimarySubtype;

        return null;
    }

    private static string ResolveWeekdayTrendDisplayName(ChartDataContext ctx, string? selectedSubtype)
    {
        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName2;

        return selectedSubtype;
    }

    private static string BuildWeekdayTrendCacheKey(string metricType, string subtype, DateTime from, DateTime to, string tableName)
    {
        return $"{metricType}|{subtype}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveTransformDataAsync(ChartDataContext ctx)
    {
        var primarySubtype = ResolveSelectedTransformPrimarySubtype(ctx);
        var secondarySubtype = ResolveSelectedTransformSecondarySubtype(ctx);

        var primaryData = await ResolveTransformDataAsync(ctx, primarySubtype);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (!string.IsNullOrWhiteSpace(secondarySubtype))
            secondaryData = await ResolveTransformDataAsync(ctx, secondarySubtype);

        var displayName1 = ResolveTransformDisplayName(ctx, primarySubtype);
        var displayName2 = ResolveTransformDisplayName(ctx, secondarySubtype);

        var transformContext = new ChartDataContext
        {
                Data1 = primaryData,
                Data2 = secondaryData,
                DisplayName1 = displayName1,
                DisplayName2 = displayName2,
                MetricType = ctx.MetricType,
                PrimarySubtype = primarySubtype,
                SecondarySubtype = secondarySubtype,
                From = ctx.From,
                To = ctx.To
        };

        return (primaryData, secondaryData, transformContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync(ChartDataContext ctx, string? selectedSubtype)
    {
        if (ctx.Data1 == null)
            return null;

        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.Data2 ?? ctx.Data1;

        var metricType = ctx.MetricType ?? _viewModel.MetricState.SelectedMetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildTransformCacheKey(metricType, selectedSubtype, ctx.From, ctx.To, tableName);
        if (_transformSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(metricType, selectedSubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _transformSubtypeCache[cacheKey] = data;
        return data;
    }

    private string? ResolveSelectedTransformPrimarySubtype(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && TransformDataPanelController.TransformPrimarySubtypeCombo?.SelectedItem is string comboSelection)
            return comboSelection;

        var selectedSubtype = _viewModel.ChartState.SelectedTransformPrimarySubtype;
        if (!string.IsNullOrWhiteSpace(selectedSubtype))
            return selectedSubtype;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype))
            return ctx.PrimarySubtype;

        return null;
    }

    private string? ResolveSelectedTransformSecondarySubtype(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && TransformDataPanelController.TransformSecondarySubtypeCombo?.SelectedItem is string comboSelection)
            return comboSelection;

        var selectedSubtype = _viewModel.ChartState.SelectedTransformSecondarySubtype;
        if (!string.IsNullOrWhiteSpace(selectedSubtype))
            return selectedSubtype;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype))
            return ctx.SecondarySubtype;

        return null;
    }

    private static string ResolveTransformDisplayName(ChartDataContext ctx, string? selectedSubtype)
    {
        if (string.IsNullOrWhiteSpace(selectedSubtype))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.PrimarySubtype) && string.Equals(selectedSubtype, ctx.PrimarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName1;

        if (!string.IsNullOrWhiteSpace(ctx.SecondarySubtype) && string.Equals(selectedSubtype, ctx.SecondarySubtype, StringComparison.OrdinalIgnoreCase))
            return ctx.DisplayName2;

        return selectedSubtype;
    }

    private static string BuildTransformCacheKey(string metricType, string subtype, DateTime from, DateTime to, string tableName)
    {
        return $"{metricType}|{subtype}|{from:O}|{to:O}|{tableName}";
    }

    private BaseDistributionService GetDistributionService(DistributionMode mode)
    {
        return mode switch
        {
                DistributionMode.Weekly => _weeklyDistributionService,
                DistributionMode.Hourly => _hourlyDistributionService,
                _ => _weeklyDistributionService
        };
    }

    private async Task RenderDiffRatio(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
    {
        if (_chartRenderingOrchestrator == null)
            return;

        await _chartRenderingOrchestrator.RenderDiffRatioChartAsync(ctx, DiffRatioChartController.Chart, _viewModel.ChartState);
    }

    private Panel? GetChartPanel(string chartName)
    {
        return chartName switch
        {
                "Norm" => ChartNormContentPanel,
                "DiffRatio" => DiffRatioChartController.Panel.ChartContentPanel,
                "Distribution" => ChartDistributionContentPanel,
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

    private IChartComputationStrategy CreateNormalizedStrategy(ChartDataContext ctx, IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string label1, string label2, DateTime from, DateTime to, NormalizationMode normalizationMode)
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
                To = to,
                NormalizationMode = normalizationMode
        };

        return _strategyCutOverService.CreateStrategy(StrategyType.Normalized, ctx, parameters);
    }

    private WeekdayTrendResult? ComputeWeekdayTrend(ChartDataContext ctx)
    {
        if (_strategyCutOverService == null)
            throw new InvalidOperationException("StrategyCutOverService is not initialized. Ensure InitializeChartPipeline() is called before using strategies.");

        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1 ?? Array.Empty<MetricData>(),
                Label1 = ctx.DisplayName1,
                From = ctx.From,
                To = ctx.To
        };

        var strategy = _strategyCutOverService.CreateStrategy(StrategyType.WeekdayTrend, ctx, parameters);
        strategy.Compute();

        return strategy is IWeekdayTrendResultProvider provider ? provider.ExtendedResult : null;
    }


    private void OnWeekdayTrendDayToggled(object? sender, WeekdayTrendDayToggleEventArgs e)
    {
        switch (e.Day)
        {
            case DayOfWeek.Monday:
                _viewModel.SetWeeklyTrendMondayVisible(e.IsChecked);
                break;
            case DayOfWeek.Tuesday:
                _viewModel.SetWeeklyTrendTuesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Wednesday:
                _viewModel.SetWeeklyTrendWednesdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Thursday:
                _viewModel.SetWeeklyTrendThursdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Friday:
                _viewModel.SetWeeklyTrendFridayVisible(e.IsChecked);
                break;
            case DayOfWeek.Saturday:
                _viewModel.SetWeeklyTrendSaturdayVisible(e.IsChecked);
                break;
            case DayOfWeek.Sunday:
                _viewModel.SetWeeklyTrendSundayVisible(e.IsChecked);
                break;
        }
    }

    private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
    {
        var panel = GetChartPanel(e.ChartName);
        if (panel != null)
            panel.Visibility = e.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnErrorOccured(object? sender, ErrorEventArgs e)
    {
        MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        ClearAllCharts();
    }

    private void ClearAllCharts()
    {
        ChartHelper.ClearChart(MainChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(ChartDistribution);
        ClearDistributionPolarChart();
        ChartHelper.ClearChart(WeekdayTrendChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(WeekdayTrendChartController.PolarChart, _viewModel.ChartState.ChartTimestamps);
        _viewModel.ChartState.LastContext = null;

        // Clear transform panel grids
        ClearTransformGrids();
    }

    private void ClearTransformGrids()
    {
        TransformDataPanelController.TransformGrid1.ItemsSource = null;
        TransformDataPanelController.TransformGrid2.ItemsSource = null;
        TransformDataPanelController.TransformGrid3.ItemsSource = null;
        TransformDataPanelController.TransformGrid2Panel.Visibility = Visibility.Collapsed;
        TransformDataPanelController.TransformGrid3Panel.Visibility = Visibility.Collapsed;
        TransformDataPanelController.TransformChartContentPanel.Visibility = Visibility.Collapsed;
        TransformDataPanelController.TransformComputeButton.IsEnabled = false;
        ChartHelper.ClearChart(TransformDataPanelController.ChartTransformResult, _viewModel.ChartState.ChartTimestamps);
    }

    #region Initialization Phases

    private void InitializeWindowLayout()
    {
        // Pin window to top-left corner
        Left = 0;
        Top = 0;
    }

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

        // Wire up MainChartController events
        MainChartController.ToggleRequested += OnMainChartToggleRequested;
        WeekdayTrendChartController.ToggleRequested += OnWeekdayTrendToggleRequested;
        WeekdayTrendChartController.ChartTypeToggleRequested += OnWeekdayTrendChartTypeToggleRequested;
        WeekdayTrendChartController.DayToggled += OnWeekdayTrendDayToggled;
        WeekdayTrendChartController.SubtypeChanged += OnWeekdayTrendSubtypeChanged;
        DiffRatioChartController.ToggleRequested += OnDiffRatioToggleRequested;
        DiffRatioChartController.OperationToggleRequested += OnDiffRatioOperationToggleRequested;
        TransformDataPanelController.ToggleRequested += OnTransformPanelToggleRequested;
        TransformDataPanelController.TransformOperationCombo.SelectionChanged += OnTransformOperationChanged;
        TransformDataPanelController.TransformPrimarySubtypeCombo.SelectionChanged += OnTransformPrimarySubtypeChanged;
        TransformDataPanelController.TransformSecondarySubtypeCombo.SelectionChanged += OnTransformSecondarySubtypeChanged;
        TransformDataPanelController.TransformComputeButton.Click += OnTransformCompute;
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

    private void RegisterLifecycleEvents()
    {
        Closing += MainWindow_Closing;
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
    }

    private void OnCmsToggleChanged(object sender, RoutedEventArgs e)
    {
        if (_isInitializing)
            return;

        CmsConfiguration.UseCmsData = CmsEnableCheckBox.IsChecked == true;
        UpdateCmsToggleEnablement();
        Debug.WriteLine($"[CMS] Enabled={CmsConfiguration.UseCmsData}");
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

        Debug.WriteLine($"[CMS] Enabled={CmsConfiguration.UseCmsData}, Single={CmsConfiguration.UseCmsForSingleMetric}, Combined={CmsConfiguration.UseCmsForCombinedMetric}, Multi={CmsConfiguration.UseCmsForMultiMetric}, Normalized={CmsConfiguration.UseCmsForNormalized}, Weekly={CmsConfiguration.UseCmsForWeeklyDistribution}, WeekdayTrend={CmsConfiguration.UseCmsForWeekdayTrend}, Hourly={CmsConfiguration.UseCmsForHourlyDistribution}");
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

        return new ChartRenderingOrchestrator(_chartUpdateCoordinator, _weeklyDistributionService, _hourlyDistributionService, _strategyCutOverService, _connectionString);
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
        InitializeDistributionControls();
        InitializeChartBehavior();
        ClearChartsOnStartup();
        DisableAxisLabelsWhenNoData();
        SetDefaultChartTitles();
        UpdateDistributionChartTypeVisibility();
        InitializeDistributionPolarTooltip();
    }

    private void SyncInitialButtonStates()
    {
        UpdateSecondaryDataRequiredButtonStates(_viewModel.MetricState.SelectedSubtypes.Count);

        // Sync main chart button text with initial state
        MainChartController.Panel.IsChartVisible = _viewModel.ChartState.IsMainVisible;

        TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;
    }

    #endregion

    #region Initialization

    private void InitializeTooltips()
    {
        var chartLabels = new Dictionary<CartesianChart, string>
        {
                { MainChartController.Chart, "Main" },
                { ChartNorm, "Norm" },
                { DiffRatioChartController.Chart, "DiffRatio" },
                { TransformDataPanelController.ChartTransformResult, "Transform" }
        };

        _tooltipManager = new ChartTooltipManager(this, chartLabels);
        _tooltipManager.AttachChart(MainChartController.Chart, "Main");
        _tooltipManager.AttachChart(ChartNorm, "Norm");
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
        _viewModel.CompleteInitialization();

        _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
        _viewModel.ChartState.LastContext = new ChartDataContext();

        // Initialize weekday trend chart type visibility
        UpdateWeekdayTrendChartTypeVisibility();
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
        DistributionModeCombo.Items.Clear();
        foreach (var definition in DistributionModeCatalog.All)
        {
            DistributionModeCombo.Items.Add(new ComboBoxItem
            {
                    Content = definition.DisplayName,
                    Tag = definition.Mode
            });
        }

        DistributionIntervalCountCombo.Items.Clear();
        foreach (var intervalCount in DistributionModeCatalog.IntervalCounts)
        {
            DistributionIntervalCountCombo.Items.Add(new ComboBoxItem
            {
                    Content = intervalCount.ToString(),
                    Tag = intervalCount
            });
        }

        var initialMode = _viewModel.ChartState.SelectedDistributionMode;
        SelectDistributionMode(initialMode);
        ApplyDistributionModeDefinition(initialMode);
        ApplyDistributionSettingsToUi(initialMode);
    }

    private void InitializeChartBehavior()
    {
        ChartHelper.InitializeChartBehavior(MainChartController.Chart);
        InitializeDistributionChartBehavior(ChartDistribution);
        ChartHelper.InitializeChartBehavior(ChartNorm);
        ChartHelper.InitializeChartBehavior(DiffRatioChartController.Chart);
    }

    private void InitializeDistributionPolarTooltip()
    {
        _distributionPolarTooltip = new ToolTip
        {
                Placement = PlacementMode.Mouse,
                StaysOpen = true
        };
        ToolTipService.SetToolTip(ChartDistributionPolar, _distributionPolarTooltip);
        ChartDistributionPolar.HoveredPointsChanged += OnDistributionPolarHoveredPointsChanged;
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
        ChartHelper.ClearChart(ChartNorm, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(ChartDistribution);
        ClearDistributionPolarChart();
    }

    /// <summary>
    ///     Common method to clear distribution charts.
    /// </summary>
    private void ClearDistributionChart(CartesianChart chart)
    {
        ChartHelper.ClearChart(chart, _viewModel.ChartState.ChartTimestamps);
    }

    private void ClearDistributionPolarChart()
    {
        ChartDistributionPolar.Series = Array.Empty<LiveChartsCore.ISeries>();
        ChartDistributionPolar.AngleAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        ChartDistributionPolar.RadiusAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        ChartDistributionPolar.Tag = null;
        if (_distributionPolarTooltip != null)
            _distributionPolarTooltip.IsOpen = false;
    }

    private void DisableAxisLabelsWhenNoData()
    {
        DisableAxisLabels(MainChartController.Chart);
        DisableAxisLabels(ChartNorm);
        DisableAxisLabels(DiffRatioChartController.Chart);
        DisableDistributionAxisLabels(ChartDistribution);
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
        ChartDistributionPolar.AngleAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        ChartDistributionPolar.RadiusAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        ChartDistributionPolar.Tag = null;
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
        ChartNormTitle.Text = "Metrics: Normalized";
        DiffRatioChartController.Panel.Title = "Difference / Ratio";
        UpdateDiffRatioOperationButton(); // Initialize button state
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
            ResolutionCombo.SelectedItem = selectedResolution;

        // Update button states since subtypes are cleared when resolution changes
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

        // Clear all charts when metric type changes
        ClearAllCharts();

        _viewModel.SetSelectedMetricType(TablesCombo.SelectedItem?.ToString());
        _viewModel.LoadSubtypesCommand.Execute(null);

        UpdateChartTitlesFromCombos();

        // Update button states since subtypes will be cleared when metric type changes
        UpdateSecondaryDataRequiredButtonStates(0);
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

        var baseType = _viewModel.MetricState.SelectedMetricType!;
        var display1 = primarySubtype != null && primarySubtype != "(All)" ? primarySubtype : baseType;
        var display2 = secondarySubtype ?? string.Empty;

        SetChartTitles(display1, display2);
        UpdateChartLabels(primarySubtype ?? string.Empty, secondarySubtype ?? string.Empty);

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
            _distributionSubtypeCache.Clear();
            _weekdayTrendSubtypeCache.Clear();
            _transformSubtypeCache.Clear();
            ResetTransformSelectionsPendingLoad();
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

        var newCombo = _selectorManager.AddSubtypeCombo(_subtypeList);
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

    private void OnChartNormToggle(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleNorm();
    }

    /// <summary>
    ///     Common handler for distribution chart toggles.
    /// </summary>
    private void HandleDistributionChartToggle()
    {
        _viewModel.ToggleDistribution();
    }

    private void OnChartDistributionToggle(object sender, RoutedEventArgs e)
    {
        HandleDistributionChartToggle();
    }

    private void OnWeekdayTrendToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleWeeklyTrend();
    }

    private void OnWeekdayTrendChartTypeToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleWeekdayTrendChartType();
        UpdateWeekdayTrendChartTypeVisibility();

        // Re-render the chart with current data if visible
        if (_viewModel.ChartState.IsWeeklyTrendVisible && _viewModel.ChartState.LastContext != null)
        {
            var result = ComputeWeekdayTrend(_viewModel.ChartState.LastContext);
            if (result != null)
                RenderWeekdayTrendChart(result);
        }
    }

    private async void OnDistributionChartTypeToggleRequested(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleDistributionChartType();
        UpdateDistributionChartTypeVisibility();

        if (_viewModel.ChartState.IsDistributionVisible && _viewModel.ChartState.LastContext != null)
            await RenderDistributionChart(_viewModel.ChartState.LastContext, _viewModel.ChartState.SelectedDistributionMode);
    }

    private void UpdateWeekdayTrendChartTypeVisibility()
    {
        // Only update chart visibility if the panel itself is visible
        if (!_viewModel.ChartState.IsWeeklyTrendVisible)
        {
            WeekdayTrendChartController.Chart.Visibility = Visibility.Collapsed;
            WeekdayTrendChartController.PolarChart.Visibility = Visibility.Collapsed;
            return;
        }

        if (_viewModel.ChartState.IsWeekdayTrendPolarMode)
        {
            WeekdayTrendChartController.Chart.Visibility = Visibility.Collapsed;
            WeekdayTrendChartController.PolarChart.Visibility = Visibility.Visible;
            WeekdayTrendChartController.ChartTypeToggleButton.Content = "Cartesian";
        }
        else
        {
            WeekdayTrendChartController.Chart.Visibility = Visibility.Visible;
            WeekdayTrendChartController.PolarChart.Visibility = Visibility.Collapsed;
            WeekdayTrendChartController.ChartTypeToggleButton.Content = "Polar";
        }
    }

    private void OnDiffRatioToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDiffRatio();
    }

    private async void OnDiffRatioOperationToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDiffRatioOperation();
        UpdateDiffRatioOperationButton();

        // Re-render the chart with current data if visible
        if (_viewModel.ChartState.IsDiffRatioVisible && _viewModel.ChartState.LastContext != null)
        {
            var ctx = _viewModel.ChartState.LastContext;
            var hasSecondaryData = HasSecondaryData(ctx);
            if (hasSecondaryData)
                await RenderDiffRatio(ctx, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype);
        }
    }

    private void UpdateDiffRatioOperationButton()
    {
        var isDifference = _viewModel.ChartState.IsDiffRatioDifferenceMode;

        var operationButton = DiffRatioChartController.OperationToggleButton;
        operationButton.Content = isDifference ? "/" : "-";
        operationButton.ToolTip = isDifference ? "Switch to Ratio (/)" : "Switch to Difference (-)";

        if (DiffRatioChartController.Chart.AxisY.Count > 0)
            DiffRatioChartController.Chart.AxisY[0].Title = isDifference ? "Difference" : "Ratio";
    }


    private void OnTransformPanelToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleTransformPanel();
    }

    private void OnTransformOperationChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateTransformComputeButtonState();
    }

    private void OnTransformPrimarySubtypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingTransformSubtypeCombos)
            return;

        if (TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem is string selectedSubtype)
            _viewModel.SetTransformPrimarySubtype(selectedSubtype);

        UpdateTransformComputeButtonState();
    }

    private void OnTransformSecondarySubtypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingTransformSubtypeCombos)
            return;

        if (TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem is string selectedSubtype)
            _viewModel.SetTransformSecondarySubtype(selectedSubtype);

        UpdateTransformComputeButtonState();
    }

    private void UpdateTransformComputeButtonState()
    {
        if (_isTransformSelectionPendingLoad)
        {
            TransformDataPanelController.TransformComputeButton.IsEnabled = false;
            return;
        }

        if (TransformDataPanelController.TransformOperationCombo.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string operationTag)
        {
            TransformDataPanelController.TransformComputeButton.IsEnabled = false;
            return;
        }

        var ctx = _viewModel.ChartState.LastContext;
        if (ctx == null)
        {
            TransformDataPanelController.TransformComputeButton.IsEnabled = false;
            return;
        }

        var hasSecondary = HasSecondaryData(ctx);
        var hasSecondSubtype = !string.IsNullOrEmpty(ResolveSelectedTransformSecondarySubtype(ctx));
        var isUnary = operationTag == "Log" || operationTag == "Sqrt";
        var isBinary = operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";

        // Enable compute button if operation matches data availability
        // For binary operations, require both secondary data AND a second subtype selected
        TransformDataPanelController.TransformComputeButton.IsEnabled = (isUnary && ctx.Data1 != null) || (isBinary && hasSecondary && hasSecondSubtype);
    }

    private async void OnTransformCompute(object sender, RoutedEventArgs e)
    {
        if (_isTransformSelectionPendingLoad)
            return;

        if (_viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        if (!TryGetSelectedOperation(out var operationTag))
            return;

        await ExecuteTransformOperation(ctx, operationTag);
    }

    /// <summary>
    ///     Gets the selected transform operation from the combo box.
    /// </summary>
    private bool TryGetSelectedOperation(out string operationTag)
    {
        operationTag = string.Empty;
        if (TransformDataPanelController.TransformOperationCombo.SelectedItem is not ComboBoxItem selectedItem || selectedItem.Tag is not string tag)
            return false;

        operationTag = tag;
        return true;
    }

    /// <summary>
    ///     Executes the appropriate transform operation based on operation type and data availability.
    /// </summary>
    private async Task ExecuteTransformOperation(ChartDataContext ctx, string operationTag)
    {
        var isUnary = operationTag == "Log" || operationTag == "Sqrt";
        var isBinary = operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";
        var hasSecondary = HasSecondaryData(ctx) && !string.IsNullOrEmpty(ResolveSelectedTransformSecondarySubtype(ctx));

        var (primaryData, secondaryData, transformContext) = await ResolveTransformDataAsync(ctx);
        if (primaryData == null)
            return;

        if (isUnary)
            await ComputeUnaryTransform(primaryData, operationTag, transformContext);
        else if (isBinary && hasSecondary && secondaryData != null)
            await ComputeBinaryTransform(primaryData, secondaryData, operationTag, transformContext);
    }

    private async Task ComputeUnaryTransform(IEnumerable<MetricData> data, string operation, ChartDataContext transformContext)
    {
        // Use ALL data for chart computation (proper normalization)
        var allDataList = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        if (allDataList.Count == 0)
            return;

        // Phase 4: Use new transform expression infrastructure
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        List<double> computedResults;
        List<IReadOnlyList<MetricData>> metricsList;

        if (expression == null)
        {
            // Fallback to legacy approach if operation not found in registry
            Debug.WriteLine($"[Transform] UNARY - Using LEGACY approach for operation: {operation}");
            var op = operation switch
            {
                    "Log" => UnaryOperators.Logarithm,
                    "Sqrt" => UnaryOperators.SquareRoot,
                    _ => x => x
            };
            var allValues = allDataList.Select(d => (double)d.Value!.Value).ToList();
            computedResults = MathHelper.ApplyUnaryOperation(allValues, op);
            metricsList = new List<IReadOnlyList<MetricData>>
            {
                    allDataList
            };
        }
        else
        {
            // Evaluate using new infrastructure
            Debug.WriteLine($"[Transform] UNARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
            metricsList = new List<IReadOnlyList<MetricData>>
            {
                    allDataList
            };
            computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);
            Debug.WriteLine($"[Transform] UNARY - Evaluated {computedResults.Count} results using TransformExpressionEvaluator");
        }

        // Continue with grid and chart rendering
        await RenderTransformResults(allDataList, computedResults, operation, metricsList, transformContext);
    }

    /// <summary>
    ///     Phase 4: Shared method to render transform results (grid and chart) using new infrastructure.
    /// </summary>
    private async Task RenderTransformResults(List<MetricData> dataList, List<double> results, string operation, List<IReadOnlyList<MetricData>> metrics, ChartDataContext transformContext)
    {
        var resultData = TransformExpressionEvaluator.CreateTransformResultData(dataList, results);
        PopulateTransformResultGrid(resultData);

        if (resultData.Count == 0)
            return;

        ShowTransformResultPanels();
        await PrepareTransformChartLayout();
        await RenderTransformChart(dataList, results, operation, metrics, transformContext);
        await FinalizeTransformChartRendering();
    }


    /// <summary>
    ///     Populates the transform result grid with data.
    /// </summary>
    private void PopulateTransformResultGrid(List<object> resultData)
    {
        TransformDataPanelController.TransformGrid3.ItemsSource = resultData;
        if (TransformDataPanelController.TransformGrid3.Columns.Count >= 2)
        {
            TransformDataPanelController.TransformGrid3.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
            TransformDataPanelController.TransformGrid3.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
        }
    }

    /// <summary>
    ///     Shows the transform result panels (grid and chart).
    /// </summary>
    private void ShowTransformResultPanels()
    {
        TransformDataPanelController.TransformGrid3Panel.Visibility = Visibility.Visible;
        TransformDataPanelController.TransformChartContentPanel.Visibility = Visibility.Visible;
    }

    /// <summary>
    ///     Prepares the transform chart layout before rendering.
    /// </summary>
    private async Task PrepareTransformChartLayout()
    {
        TransformDataPanelController.TransformChartContentPanel.UpdateLayout();
        await Dispatcher.InvokeAsync(() =>
                {
                },
                DispatcherPriority.Render);
        await CalculateAndSetTransformChartWidth();
        Debug.WriteLine($"[TransformChart] Before render - ActualWidth={TransformDataPanelController.ChartTransformResult.ActualWidth}, ActualHeight={TransformDataPanelController.ChartTransformResult.ActualHeight}, IsVisible={TransformDataPanelController.ChartTransformResult.IsVisible}, PanelVisible={TransformDataPanelController.TransformChartContentPanel.Visibility}");
    }

    /// <summary>
    ///     Finalizes transform chart rendering with forced updates.
    /// </summary>
    private async Task FinalizeTransformChartRendering()
    {
        TransformDataPanelController.ChartTransformResult.Update(true, true);
        TransformDataPanelController.TransformChartContentPanel.UpdateLayout();
        await Dispatcher.InvokeAsync(() =>
                {
                    TransformDataPanelController.ChartTransformResult.InvalidateVisual();
                    TransformDataPanelController.ChartTransformResult.Update(true, true);
                },
                DispatcherPriority.Render);
        Debug.WriteLine($"[TransformChart] After render - ActualWidth={TransformDataPanelController.ChartTransformResult.ActualWidth}, ActualHeight={TransformDataPanelController.ChartTransformResult.ActualHeight}, SeriesCount={TransformDataPanelController.ChartTransformResult.Series?.Count ?? 0}");
    }

    /// <summary>
    ///     Calculates and sets the transform chart container width to fill remaining space.
    /// </summary>
    private async Task CalculateAndSetTransformChartWidth()
    {
        await Dispatcher.InvokeAsync(() =>
                {
                    if (TransformDataPanelController.TransformChartContainer == null)
                        return;

                    var parentStackPanel = TransformDataPanelController.TransformChartContainer.Parent as FrameworkElement;
                    if (parentStackPanel?.Parent is not FrameworkElement parentContainer)
                        return;

                    var usedWidth = CalculateUsedWidthForTransformGrids();
                    usedWidth += 40; // Margins and spacing (20px left + 10px between grids + 10px before chart)

                    var availableWidth = parentContainer.ActualWidth > 0 ? parentContainer.ActualWidth : 1800;
                    var chartWidth = Math.Max(400, availableWidth - usedWidth - 40); // 40px for window padding
                    TransformDataPanelController.TransformChartContainer.Width = chartWidth;

                    Debug.WriteLine($"[TransformChart] Calculated width - parentWidth={parentContainer.ActualWidth}, usedWidth={usedWidth}, chartWidth={chartWidth}");
                },
                DispatcherPriority.Render);
    }

    /// <summary>
    ///     Calculates the total width used by visible transform grids.
    /// </summary>
    private double CalculateUsedWidthForTransformGrids()
    {
        double usedWidth = 0;

        // Grid 1 is always visible
        var grid1StackPanel = TransformDataPanelController.TransformGrid1.Parent as FrameworkElement;
        usedWidth += grid1StackPanel?.ActualWidth > 0 ? grid1StackPanel.ActualWidth : 250;

        // Grid 2 (if visible)
        if (TransformDataPanelController.TransformGrid2Panel.IsVisible)
            usedWidth += TransformDataPanelController.TransformGrid2Panel.ActualWidth > 0 ? TransformDataPanelController.TransformGrid2Panel.ActualWidth : 250;

        // Grid 3 (if visible)
        if (TransformDataPanelController.TransformGrid3Panel.IsVisible)
            usedWidth += TransformDataPanelController.TransformGrid3Panel.ActualWidth > 0 ? TransformDataPanelController.TransformGrid3Panel.ActualWidth : 250;

        return usedWidth;
    }

    /// <summary>
    ///     Phase 4: Renders transform chart using new infrastructure for label generation.
    /// </summary>
    private async Task RenderTransformChart(List<MetricData> dataList, List<double> results, string operation, List<IReadOnlyList<MetricData>> metrics, ChartDataContext transformContext)
    {
        if (dataList.Count == 0 || results.Count == 0)
            return;

        // Get date range from context or data
        var from = transformContext.From != default ? transformContext.From : dataList.Min(d => d.NormalizedTimestamp);
        var to = transformContext.To != default ? transformContext.To : dataList.Max(d => d.NormalizedTimestamp);

        // Phase 4: Generate label using new infrastructure
        var label = TransformExpressionEvaluator.GenerateTransformLabel(operation, metrics, transformContext);

        // Create strategy using existing pipeline
        var strategy = new TransformResultStrategy(dataList, results, label, from, to);

        // Use existing chart rendering pipeline
        // Pass metric type and subtype info for proper label formatting
        var operationTag = TransformDataPanelController.TransformOperationCombo.SelectedItem is ComboBoxItem item ? item.Tag?.ToString() ?? "Transform" : "Transform";
        var operationType = operationTag == "Subtract" ? "-" : operationTag == "Add" ? "+" : operationTag == "Divide" ? "/" : null;
        var isOperationChart = operationTag == "Subtract" || operationTag == "Add" || operationTag == "Divide";

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(TransformDataPanelController.ChartTransformResult, strategy, label, null, 400, transformContext.MetricType, transformContext.PrimarySubtype, transformContext.SecondarySubtype, operationType, isOperationChart);
    }


    private async Task ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation, ChartDataContext transformContext)
    {
        // Use ALL data for chart computation (proper normalization)
        var allData1List = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        var allData2List = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return;

        // Phase 4: Align data by timestamp (required for expression evaluator)
        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);
        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return;

        // Phase 4: Use new transform expression infrastructure
        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        List<double> binaryComputedResults;
        List<IReadOnlyList<MetricData>> binaryMetricsList;

        if (expression == null)
        {
            // Fallback to legacy approach if operation not found in registry
            Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");
            var op = operation switch
            {
                    "Add" => BinaryOperators.Sum,
                    "Subtract" => BinaryOperators.Difference,
                    "Divide" => BinaryOperators.Ratio,
                    _ => (a, b) => a
            };

            var allValues1 = alignedData.Item1.Select(d => (double)d.Value!.Value).ToList();
            var allValues2 = alignedData.Item2.Select(d => (double)d.Value!.Value).ToList();
            binaryComputedResults = MathHelper.ApplyBinaryOperation(allValues1, allValues2, op);
            binaryMetricsList = new List<IReadOnlyList<MetricData>>
            {
                    alignedData.Item1,
                    alignedData.Item2
            };
            Debug.WriteLine($"[Transform] BINARY - Legacy computation completed: {binaryComputedResults.Count} results");
        }
        else
        {
            // Evaluate using new infrastructure
            Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
            binaryMetricsList = new List<IReadOnlyList<MetricData>>
            {
                    alignedData.Item1,
                    alignedData.Item2
            };
            binaryComputedResults = TransformExpressionEvaluator.Evaluate(expression, binaryMetricsList);
            Debug.WriteLine($"[Transform] BINARY - Evaluated {binaryComputedResults.Count} results using TransformExpressionEvaluator");
        }

        // Continue with grid and chart rendering
        await RenderTransformResults(alignedData.Item1, binaryComputedResults, operation, binaryMetricsList, transformContext);
    }

    #endregion

    #region Chart Configuration and Helper Methods

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        var mainChart = MainChartController.Chart;
        ChartHelper.ResetZoom(mainChart);
        ChartHelper.ResetZoom(ChartNorm);
        ChartHelper.ResetZoom(DiffRatioChartController.Chart);
        ResetDistributionChartZoom(ChartDistribution);
        ResetDistributionPolarZoom();
        ChartHelper.ResetZoom(TransformDataPanelController.ChartTransformResult);
        var weekdayChart = WeekdayTrendChartController.Chart;
        ChartHelper.ResetZoom(ref weekdayChart);
        var weekdayPolarChart = WeekdayTrendChartController.PolarChart;
        ChartHelper.ResetZoom(ref weekdayPolarChart);
    }

    /// <summary>
    ///     Common method to reset zoom for distribution charts.
    /// </summary>
    private void ResetDistributionChartZoom(CartesianChart chart)
    {
        ChartHelper.ResetZoom(chart);
    }

    private void ResetDistributionPolarZoom()
    {
        ChartDistributionPolar.FitToBounds = true;
    }

    private void OnDistributionPolarHoveredPointsChanged(LiveChartsCore.Kernel.Sketches.IChartView chart, IEnumerable<ChartPoint>? newPoints, IEnumerable<ChartPoint>? oldPoints)
    {
        if (_distributionPolarTooltip == null)
            return;

        var state = ChartDistributionPolar.Tag as DistributionPolarTooltipState;
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

        var formatted = value.ToString("0.###");
        if (!string.IsNullOrWhiteSpace(unit))
            formatted = $"{formatted} {unit}";

        return formatted;
    }

    private sealed class DistributionPolarTooltipState
    {
        public DistributionPolarTooltipState(DistributionModeDefinition definition, DistributionRangeResult rangeResult)
        {
            Definition = definition;
            RangeResult = rangeResult;
        }

        public DistributionModeDefinition Definition { get; }
        public DistributionRangeResult RangeResult { get; }
    }

    private void SetChartTitles(string leftName, string rightName)
    {
        leftName ??= string.Empty;
        rightName ??= string.Empty;

        _viewModel.ChartState.LeftTitle = leftName;
        _viewModel.ChartState.RightTitle = rightName;

        MainChartController.Panel.Title = $"{leftName} vs. {rightName}";
        ChartNormTitle.Text = $"{leftName} ~ {rightName}";
        DiffRatioChartController.Panel.Title = $"{leftName} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {rightName}";
    }

    private void UpdateChartLabels(string subtype1, string subtype2)
    {
        if (_tooltipManager == null)
            return;

        subtype1 ??= string.Empty;
        subtype2 ??= string.Empty;

        var baseType = TablesCombo.SelectedItem?.ToString() ?? "";

        var label1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)" ? subtype1 : baseType;
        var label2 = !string.IsNullOrEmpty(subtype2) ? subtype2 : string.Empty;

        var chartMainLabel = !string.IsNullOrEmpty(label2) ? $"{label1} vs {label2}" : label1;
        _tooltipManager.UpdateChartLabel(MainChartController.Chart, chartMainLabel);

        var chartDiffRatioLabel = !string.IsNullOrEmpty(label2) ? $"{label1} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {label2}" : label1;
        _tooltipManager.UpdateChartLabel(DiffRatioChartController.Chart, chartDiffRatioLabel);
    }

    private void UpdateChartTitlesFromCombos()
    {
        var subtype1 = _selectorManager.GetPrimarySubtype() ?? "";
        var subtype2 = _selectorManager.GetSecondarySubtype() ?? "";

        var baseType = TablesCombo.SelectedItem?.ToString() ?? "";

        var display1 = !string.IsNullOrEmpty(subtype1) && subtype1 != "(All)" ? subtype1 : baseType;
        var display2 = !string.IsNullOrEmpty(subtype2) ? subtype2 : "";

        SetChartTitles(display1, display2);
        UpdateChartLabels(subtype1, subtype2);
    }

    #endregion

    #region Distribution chart display mode UI handling

    private void SelectDistributionMode(DistributionMode mode)
    {
        foreach (var item in DistributionModeCombo.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is DistributionMode taggedMode && taggedMode == mode)
            {
                DistributionModeCombo.SelectedItem = item;
                return;
            }
        }
    }

    private void UpdateDistributionChartTypeVisibility()
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
        {
            ChartDistribution.Visibility = Visibility.Collapsed;
            ChartDistributionPolar.Visibility = Visibility.Collapsed;
            if (_distributionPolarTooltip != null)
                _distributionPolarTooltip.IsOpen = false;
            return;
        }

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            ChartDistribution.Visibility = Visibility.Collapsed;
            ChartDistributionPolar.Visibility = Visibility.Visible;
            DistributionChartTypeToggleButton.Content = "Cartesian";
        }
        else
        {
            ChartDistribution.Visibility = Visibility.Visible;
            ChartDistributionPolar.Visibility = Visibility.Collapsed;
            DistributionChartTypeToggleButton.Content = "Polar";
            if (_distributionPolarTooltip != null)
                _distributionPolarTooltip.IsOpen = false;
        }
    }

    private void SelectDistributionIntervalCount(int intervalCount)
    {
        foreach (var item in DistributionIntervalCountCombo.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is int taggedInterval && taggedInterval == intervalCount)
            {
                DistributionIntervalCountCombo.SelectedItem = item;
                return;
            }
        }
    }

    private void ApplyDistributionModeDefinition(DistributionMode mode)
    {
        var definition = DistributionModeCatalog.Get(mode);
        ChartDistributionTitle.Text = definition.Title;

        if (ChartDistribution.AxisX.Count == 0)
            ChartDistribution.AxisX.Add(new Axis());

        var axis = ChartDistribution.AxisX[0];
        axis.Title = definition.XAxisTitle;
        axis.Labels = definition.XAxisLabels.ToArray();
    }

    private void ApplyDistributionSettingsToUi(DistributionMode mode)
    {
        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        DistributionFrequencyShadingRadio.IsChecked = settings.UseFrequencyShading;
        DistributionSimpleRangeRadio.IsChecked = !settings.UseFrequencyShading;
        SelectDistributionIntervalCount(settings.IntervalCount);
    }

    private void UpdateDistributionSubtypeOptions()
    {
        if (DistributionSubtypeCombo == null)
            return;

        _isUpdatingDistributionSubtypeCombo = true;
        try
        {
            DistributionSubtypeCombo.Items.Clear();

            var selectedSubtypes = _viewModel.MetricState.SelectedSubtypes;
            if (selectedSubtypes.Count == 0)
            {
                DistributionSubtypeCombo.IsEnabled = false;
                _viewModel.ChartState.SelectedDistributionSubtype = null;
                DistributionSubtypeCombo.SelectedItem = null;
                return;
            }

            foreach (var subtype in selectedSubtypes)
                DistributionSubtypeCombo.Items.Add(subtype);

            DistributionSubtypeCombo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedDistributionSubtype;
            var selection = current != null && selectedSubtypes.Contains(current) ? current : selectedSubtypes[0];
            DistributionSubtypeCombo.SelectedItem = selection;

            if (_isInitializing)
                _viewModel.ChartState.SelectedDistributionSubtype = selection;
            else
                _viewModel.SetDistributionSubtype(selection);
        }
        finally
        {
            _isUpdatingDistributionSubtypeCombo = false;
        }
    }

    private void UpdateWeekdayTrendSubtypeOptions()
    {
        var combo = WeekdayTrendChartController?.SubtypeCombo;
        if (combo == null)
            return;

        _isUpdatingWeekdayTrendSubtypeCombo = true;
        try
        {
            combo.Items.Clear();

            var selectedSubtypes = _viewModel.MetricState.SelectedSubtypes;
            if (selectedSubtypes.Count == 0)
            {
                combo.IsEnabled = false;
                _viewModel.ChartState.SelectedWeekdayTrendSubtype = null;
                combo.SelectedItem = null;
                return;
            }

            foreach (var subtype in selectedSubtypes)
                combo.Items.Add(subtype);

            combo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedWeekdayTrendSubtype;
            var selection = current != null && selectedSubtypes.Contains(current) ? current : selectedSubtypes[0];
            combo.SelectedItem = selection;

            if (_isInitializing)
                _viewModel.ChartState.SelectedWeekdayTrendSubtype = selection;
            else
                _viewModel.SetWeekdayTrendSubtype(selection);
        }
        finally
        {
            _isUpdatingWeekdayTrendSubtypeCombo = false;
        }
    }

    /// <summary>
    ///     Common handler for display mode changes (frequency shading vs simple range).
    /// </summary>
    private async Task HandleDistributionDisplayModeChanged(DistributionMode mode, bool useFrequencyShading)
    {
        if (_isInitializing)
            return;

        try
        {
            var definition = DistributionModeCatalog.Get(mode);
            Debug.WriteLine($"On{definition.DisplayName}DisplayModeChanged: Setting UseFrequencyShading to {useFrequencyShading}");

            _viewModel.SetDistributionFrequencyShading(mode, useFrequencyShading);

            var isVisible = _viewModel.ChartState.IsDistributionVisible;
            var settings = _viewModel.ChartState.GetDistributionSettings(mode);
            var useFrequencyShadingState = settings.UseFrequencyShading;

            Debug.WriteLine($"On{definition.DisplayName}DisplayModeChanged: ChartState.UseFrequencyShading = {useFrequencyShadingState}");

            if (isVisible && _viewModel.ChartState.LastContext?.Data1 != null)
            {
                var ctx = _viewModel.ChartState.LastContext;
                Debug.WriteLine($"On{definition.DisplayName}DisplayModeChanged: Refreshing chart with useFrequencyShading={useFrequencyShadingState}");
                await RenderDistributionChart(ctx, mode);
            }
        }
        catch (Exception ex)
        {
            var definition = DistributionModeCatalog.Get(mode);
            Debug.WriteLine($"On{definition.DisplayName}DisplayModeChanged error: {ex.Message}");
        }
    }

    /// <summary>
    ///     Common handler for interval count changes.
    /// </summary>
    private async Task HandleDistributionIntervalCountChanged(DistributionMode mode, int intervalCount)
    {
        if (_isInitializing)
            return;

        try
        {
            _viewModel.SetDistributionIntervalCount(mode, intervalCount);
            var isVisible = _viewModel.ChartState.IsDistributionVisible;
            var useFrequencyShading = _viewModel.ChartState.GetDistributionSettings(mode).UseFrequencyShading;

            if (isVisible && _viewModel.ChartState.LastContext?.Data1 != null)
            {
                var ctx = _viewModel.ChartState.LastContext;
                await RenderDistributionChart(ctx, mode);
            }
        }
        catch (Exception ex)
        {
            var definition = DistributionModeCatalog.Get(mode);
            Debug.WriteLine($"On{definition.DisplayName}IntervalCountChanged error: {ex.Message}");
        }
    }

    private DistributionMode GetSelectedDistributionMode()
    {
        if (_viewModel == null)
            return DistributionMode.Weekly;

        if (DistributionModeCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is DistributionMode mode)
            return mode;

        return _viewModel.ChartState.SelectedDistributionMode;
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

    private async void OnDistributionDisplayModeChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
            return;

        var useFrequencyShading = DistributionFrequencyShadingRadio.IsChecked == true;
        await HandleDistributionDisplayModeChanged(GetSelectedDistributionMode(), useFrequencyShading);
    }

    private async void OnDistributionIntervalCountChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        if (DistributionIntervalCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var intervalCount))
            await HandleDistributionIntervalCountChanged(GetSelectedDistributionMode(), intervalCount);
    }

    private void OnDistributionModeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing)
            return;

        var mode = GetSelectedDistributionMode();
        _viewModel.SetDistributionMode(mode);
        ApplyDistributionModeDefinition(mode);
        ApplyDistributionSettingsToUi(mode);
    }

    private void OnDistributionSubtypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingDistributionSubtypeCombo)
            return;

        if (DistributionSubtypeCombo.SelectedItem is string selectedSubtype)
            _viewModel.SetDistributionSubtype(selectedSubtype);
    }

    private void OnWeekdayTrendSubtypeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingWeekdayTrendSubtypeCombo)
            return;

        if (WeekdayTrendChartController.SubtypeCombo.SelectedItem is string selectedSubtype)
            _viewModel.SetWeekdayTrendSubtype(selectedSubtype);
    }

    #endregion
}



