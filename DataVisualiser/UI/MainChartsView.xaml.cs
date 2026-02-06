using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using CartesianChart = LiveCharts.Wpf.CartesianChart;
using ErrorEventArgs = DataVisualiser.UI.Events.ErrorEventArgs;

namespace DataVisualiser.UI;

public partial class MainChartsView : UserControl
{
    private readonly IChartControllerFactory _chartControllerFactory = new ChartControllerFactory();

    private ChartState _chartState = null!;
    private readonly IChartRendererResolver _chartRendererResolver = new ChartRendererResolver();
    private readonly IChartSurfaceFactory _chartSurfaceFactory;

    // Placeholder instance since DiffRatioChartController is commented out in XAML.
    private MetricState _metricState = null!;
    private UiState _uiState = null!;
    private BarPieChartControllerAdapter _barPieAdapter = null!;
    private ChartComputationEngine _chartComputationEngine = null!;
    private IChartControllerRegistry? _chartControllerRegistry;
    private ChartRenderEngine _chartRenderEngine = null!;
    private ChartRenderingOrchestrator? _chartRenderingOrchestrator;
    private ChartUpdateCoordinator _chartUpdateCoordinator = null!;

    private string _connectionString = null!;
    private DiffRatioChartControllerAdapter _diffRatioAdapter = null!;
    private DistributionChartControllerAdapter _distributionAdapter = null!;
    private DistributionPolarRenderingService _distributionPolarRenderingService = null!;
    private ToolTip _distributionPolarTooltip = null!;
    private IDistributionService _hourlyDistributionService = null!;
    private bool _isChangingResolution;

    private bool _isInitializing = true;
    private bool _isMetricTypeChangePending;
    private bool _isApplyingSelectionSync;
    private MainChartControllerAdapter _mainAdapter = null!;

    private MetricSelectionService _metricSelectionService = null!;
    private NormalizedChartControllerAdapter _normalizedAdapter = null!;
    private SubtypeSelectorManager _selectorManager = null!;
    private IStrategyCutOverService? _strategyCutOverService;
    private List<MetricNameOption>? _subtypeList;
    private ChartTooltipManager? _tooltipManager;
    private TransformDataPanelControllerAdapter _transformAdapter = null!;
    private int _uiBusyDepth;
    private MainWindowViewModel _viewModel = null!;
    private WeekdayTrendChartControllerAdapter _weekdayTrendAdapter = null!;
    private WeekdayTrendChartUpdateCoordinator _weekdayTrendChartUpdateCoordinator = null!;
    private IDistributionService _weeklyDistributionService = null!;

    public MainChartsView()
    {
        _chartSurfaceFactory = new ChartSurfaceFactory(_chartRendererResolver);
        InitializeComponent();

        InitializeInfrastructure();
        InitializeChartPipeline();
        InitializeViewModel();
        InitializeUiBindings();

        ExecuteStartupSequence();

        Unloaded += OnUnloaded;
    }

    private DiffRatioChartController DiffRatioChartController { get; } = new();

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

    private void UpdateSelectedSubtypesInViewModel()
    {
        var distinctSeries = GetDistinctSelectedSeries();
        var selectedSubtypeCount = CountSelectedSubtypes(distinctSeries);

        _viewModel.SetSelectedSeries(distinctSeries);
        UpdateSubtypeOptions(ChartControllerKeys.Normalized);
        UpdateSubtypeOptions(ChartControllerKeys.DiffRatio);
        UpdateSubtypeOptions(ChartControllerKeys.Distribution);
        UpdateSubtypeOptions(ChartControllerKeys.WeeklyTrend);
        UpdateSubtypeOptions(ChartControllerKeys.Main);

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

    private bool ShouldRefreshDateRangeForCurrentSelection()
    {
        return !string.IsNullOrWhiteSpace(_viewModel.MetricState.SelectedMetricType);
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
                ClearChart(ChartControllerKeys.Normalized);
                _viewModel.SetNormalizedVisible(false);
            }

            if (_viewModel.ChartState.IsDiffRatioVisible)
            {
                ClearChart(ChartControllerKeys.DiffRatio);
                _viewModel.SetDiffRatioVisible(false);
            }
        }

        // Update button enabled states (this is UI-only, not part of the rendering pipeline)
        ResolveController(ChartControllerKeys.Normalized).SetToggleEnabled(canToggle);
        ResolveController(ChartControllerKeys.DiffRatio).SetToggleEnabled(canToggle);
    }

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require at least one subtype.
    ///     These buttons are disabled when no subtypes are selected.
    /// </summary>
    private void UpdatePrimaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        var hasPrimaryData = selectedSubtypeCount >= 1;
        var canToggle = hasPrimaryData && HasLoadedData();

        ResolveController(ChartControllerKeys.Main).SetToggleEnabled(HasLoadedData());
        UpdateMainChartStackedAvailability(selectedSubtypeCount);
        ResolveController(ChartControllerKeys.WeeklyTrend).SetToggleEnabled(canToggle);
        ResolveController(ChartControllerKeys.Distribution).SetToggleEnabled(canToggle);
        ResolveController(ChartControllerKeys.Transform).SetToggleEnabled(canToggle);
        ResolveController(ChartControllerKeys.BarPie).SetToggleEnabled(canToggle);
    }

    private void UpdateMainChartStackedAvailability(int selectedSubtypeCount)
    {
        var canStack = selectedSubtypeCount >= 2;
        MainChartController.DisplayStackedRadio.IsEnabled = canStack;

        if (!canStack && MainChartController.DisplayStackedRadio.IsChecked == true)
        {
            MainChartController.DisplayRegularRadio.IsChecked = true;
            _viewModel.SetMainChartDisplayMode(MainChartDisplayMode.Regular);
        }
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
                _selectorManager.UpdateLastDynamicComboItems(subtypeListLocal, selectedMetricType);
            else
                RefreshPrimarySubtypeCombo(subtypeListLocal, true, selectedMetricType);

            _isMetricTypeChangePending = false;
            UpdateSelectedSubtypesInViewModel();

            return;
        }

        RefreshPrimarySubtypeCombo(subtypeListLocal, false, selectedMetricType);

        BuildDynamicSubtypeControls(subtypeListLocal);
        UpdateSelectedSubtypesInViewModel();
        if (!HasLoadedData() && ShouldRefreshDateRangeForCurrentSelection())
            _ = LoadDateRangeForSelectedMetrics();

        if (!_isInitializing && _viewModel.MetricState.SelectedSeries.Count > 0)
            ApplySelectionStateToUi();
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
            ApplyBucketCountFromState();
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

        var selections = _viewModel.MetricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        if (selections.Count == 0)
            return;

        var metricType = GetSelectedMetricOption(TablesCombo);
        _selectorManager.ClearDynamic();
        _selectorManager.SetPrimaryMetricType(metricType);

        SetComboSelectionByValue(SubtypeCombo, selections[0].QuerySubtype);

        for (var i = 1; i < selections.Count; i++)
        {
            var combo = _selectorManager.AddSubtypeCombo(_subtypeList, metricType);
            SetComboSelectionByValue(combo, selections[i].QuerySubtype);
        }
    }

    private void ApplyBucketCountFromState()
    {
        var target = _viewModel.ChartState.BarPieBucketCount;
        var combo = BarPieChartController.BucketCountCombo;

        if (combo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is int current && current == target)
            return;

        var match = combo.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Tag is int tag && tag == target);

        if (match != null)
            combo.SelectedItem = match;
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

    private void BuildDynamicSubtypeControls(IEnumerable<MetricNameOption> subtypes)
    {
        _selectorManager.ClearDynamic();
        UpdateSelectedSubtypesInViewModel();
    }

    private void RefreshPrimarySubtypeCombo(IEnumerable<MetricNameOption> subtypes, bool preserveSelection, MetricNameOption? selectedMetricType)
    {
        var previousSelection = GetSelectedMetricValue(SubtypeCombo);

        SubtypeCombo.Items.Clear();

        foreach (var st in subtypes)
            SubtypeCombo.Items.Add(st);

        SubtypeCombo.IsEnabled = subtypes.Any();
        _selectorManager.SetPrimaryMetricType(selectedMetricType);

        if (preserveSelection && !string.IsNullOrWhiteSpace(previousSelection) && SubtypeCombo.Items.OfType<MetricNameOption>().Any(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase)))
        {
            SubtypeCombo.SelectedItem = SubtypeCombo.Items.OfType<MetricNameOption>().First(item => string.Equals(item.Value, previousSelection, StringComparison.OrdinalIgnoreCase));
            return;
        }

        if (SubtypeCombo.Items.Count > 0)
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

        CompleteTransformSelectionsPendingLoad();
        UpdateSubtypeOptions(ChartControllerKeys.Normalized);
        UpdateSubtypeOptions(ChartControllerKeys.DiffRatio);
        UpdateSubtypeOptions(ChartControllerKeys.Main);
        UpdateTransformSubtypeOptions();
        UpdateTransformComputeButtonState();
        var selectedSubtypeCount = CountSelectedSubtypes(_viewModel.MetricState.SelectedSeries);
        UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);

        await RenderChartAsync(ChartControllerKeys.BarPie, ctx);

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
            case ChartControllerKeys.Main:
                SetChartVisibility(ChartControllerKeys.Main, e.ShowMain);
                break;
            case ChartControllerKeys.Normalized:
                SetChartVisibility(ChartControllerKeys.Normalized, e.ShowNormalized);
                break;
            case ChartControllerKeys.DiffRatio:
                SetChartVisibility(ChartControllerKeys.DiffRatio, e.ShowDiffRatio);
                break;
            case ChartControllerKeys.Distribution:
                SetChartVisibility(ChartControllerKeys.Distribution, e.ShowDistribution);
                UpdateDistributionChartTypeVisibility();
                break;
            case ChartControllerKeys.WeeklyTrend:
                SetChartVisibility(ChartControllerKeys.WeeklyTrend, e.ShowWeeklyTrend);
                UpdateWeekdayTrendChartTypeVisibility();
                break;
            case ChartControllerKeys.Transform:
                SetChartVisibility(ChartControllerKeys.Transform, e.ShowTransformPanel);
                break;
            case ChartControllerKeys.BarPie:
                SetChartVisibility(ChartControllerKeys.BarPie, e.ShowBarPie);
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

        if (!IsKnownChartKey(e.ToggledChartName))
        {
            Debug.WriteLine($"[ChartRegistry] Ignoring visibility-only toggle for unknown chart '{e.ToggledChartName}'.");
            return true;
        }

        UpdateChartVisibilityForToggle(e);

        if (e.ToggledChartName == ChartControllerKeys.Transform)
        {
            HandleTransformVisibilityOnlyToggle(_viewModel.ChartState.LastContext);
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
                ChartControllerKeys.Main => _viewModel.ChartState.IsMainVisible,
                ChartControllerKeys.Normalized => _viewModel.ChartState.IsNormalizedVisible,
                ChartControllerKeys.DiffRatio => _viewModel.ChartState.IsDiffRatioVisible,
                ChartControllerKeys.Distribution => _viewModel.ChartState.IsDistributionVisible,
                ChartControllerKeys.WeeklyTrend => _viewModel.ChartState.IsWeeklyTrendVisible,
                ChartControllerKeys.Transform => _viewModel.ChartState.IsTransformPanelVisible,
                ChartControllerKeys.BarPie => _viewModel.ChartState.IsBarPieVisible,
                _ => false
        };
    }

    private bool HasChartData(string chartName)
    {
        if (!IsKnownChartKey(chartName))
            return false;

        return ResolveController(chartName).HasSeries(_viewModel.ChartState);
    }

    private void UpdateAllChartVisibilities(ChartUpdateRequestedEventArgs e)
    {
        SetChartVisibility(ChartControllerKeys.Main, e.ShowMain);
        SetChartVisibility(ChartControllerKeys.Normalized, e.ShowNormalized);
        SetChartVisibility(ChartControllerKeys.DiffRatio, e.ShowDiffRatio);
        SetChartVisibility(ChartControllerKeys.Distribution, e.ShowDistribution);
        UpdateDistributionChartTypeVisibility();

        SetChartVisibility(ChartControllerKeys.WeeklyTrend, e.ShowWeeklyTrend);
        UpdateWeekdayTrendChartTypeVisibility();

        SetChartVisibility(ChartControllerKeys.Transform, _viewModel.ChartState.IsTransformPanelVisible);
        SetChartVisibility(ChartControllerKeys.BarPie, e.ShowBarPie);
    }

    private void SetChartVisibility(string key, bool isVisible)
    {
        ResolveController(key).SetVisible(isVisible);
    }

    private static bool IsKnownChartKey(string key)
    {
        return ChartControllerKeys.All.Any(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
    }

    private void UpdateSubtypeOptions(string key)
    {
        ResolveController(key).UpdateSubtypeOptions();
    }

    private void ClearChartCache(string key)
    {
        ResolveController(key).ClearCache();
    }

    private void InitializeDistributionControlsFromRegistry()
    {
        if (ResolveController(ChartControllerKeys.Distribution) is IDistributionChartControllerExtras controller)
            controller.InitializeControls();
    }

    private void InitializeWeekdayTrendControls()
    {
        if (ResolveController(ChartControllerKeys.WeeklyTrend) is IWeekdayTrendChartControllerExtras controller)
            controller.InitializeControls();
    }

    private void UpdateDistributionChartTypeVisibility()
    {
        if (ResolveController(ChartControllerKeys.Distribution) is IDistributionChartControllerExtras controller)
            controller.UpdateChartTypeVisibility();
    }

    private void UpdateWeekdayTrendChartTypeVisibility()
    {
        if (ResolveController(ChartControllerKeys.WeeklyTrend) is IWeekdayTrendChartControllerExtras controller)
            controller.UpdateChartTypeVisibility();
    }

    private void CompleteTransformSelectionsPendingLoad()
    {
        if (ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras controller)
            controller.CompleteSelectionsPendingLoad();
    }

    private void ResetTransformSelectionsPendingLoad()
    {
        if (ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras controller)
            controller.ResetSelectionsPendingLoad();
    }

    private void HandleTransformVisibilityOnlyToggle(ChartDataContext? context)
    {
        if (ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras controller)
            controller.HandleVisibilityOnlyToggle(context);
    }

    private void UpdateTransformSubtypeOptions()
    {
        if (ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras controller)
            controller.UpdateTransformSubtypeOptions();
    }

    private void UpdateTransformComputeButtonState()
    {
        if (ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras controller)
            controller.UpdateTransformComputeButtonState();
    }

    private void UpdateDiffRatioOperationButton()
    {
        if (ResolveController(ChartControllerKeys.DiffRatio) is IDiffRatioChartControllerExtras controller)
            controller.UpdateOperationButton();
    }

    private void SyncMainDisplayModeSelection()
    {
        if (ResolveController(ChartControllerKeys.Main) is IMainChartControllerExtras controller)
            controller.SyncDisplayModeSelection();
    }

    private async Task HandleSingleChartUpdate(ChartUpdateRequestedEventArgs e)
    {
        // Transform panel visibility toggle - just update visibility, don't reload charts
        if (e.ToggledChartName == "Transform" && e.IsVisibilityOnlyToggle)
        {
            HandleTransformVisibilityOnlyToggle(_viewModel.ChartState.LastContext);
            return;
        }

        if (string.IsNullOrWhiteSpace(e.ToggledChartName))
            return;

        var ctx = _viewModel.ChartState.LastContext;

        if (ctx != null && ShouldRenderCharts(ctx))
            await RenderSingleChart(e.ToggledChartName, ctx);
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

        // Only render charts that are visible - skip computation entirely for hidden charts
        if (_viewModel.ChartState.IsMainVisible)
            await RenderChartAsync(ChartControllerKeys.Main, safeCtx);

        // Charts that require secondary data - only render if visible AND secondary data exists
        if (hasSecondaryData)
        {
            if (_viewModel.ChartState.IsNormalizedVisible)
                await RenderChartAsync(ChartControllerKeys.Normalized, safeCtx);

            if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                await RenderChartAsync(ChartControllerKeys.DiffRatio, safeCtx);
        }
        else
        {
            // Clear charts that require secondary data when no secondary data exists
            ClearChart(ChartControllerKeys.Normalized);
            ClearChart(ChartControllerKeys.DiffRatio);
        }

        // Charts that don't require secondary data - only render if visible
        if (_viewModel.ChartState.IsDistributionVisible)
            await RenderChartAsync(ChartControllerKeys.Distribution, safeCtx);

        if (_viewModel.ChartState.IsWeeklyTrendVisible)
            await RenderChartAsync(ChartControllerKeys.WeeklyTrend, safeCtx);

        // Populate transform panel grids if visible
        if (_viewModel.ChartState.IsTransformPanelVisible)
            await RenderChartAsync(ChartControllerKeys.Transform, safeCtx);

        if (_viewModel.ChartState.IsBarPieVisible)
            await RenderChartAsync(ChartControllerKeys.BarPie, safeCtx);
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
    ///     Renders only a specific chart (used for visibility-only toggles to avoid re-rendering all charts).
    /// </summary>
    private async Task RenderSingleChart(string chartName, ChartDataContext ctx)
    {
        using var busyScope = BeginUiBusyScope();
        var hasSecondaryData = HasSecondaryData(ctx);

        switch (chartName)
        {
            case ChartControllerKeys.Main:
                if (_viewModel.ChartState.IsMainVisible)
                    await RenderChartAsync(ChartControllerKeys.Main, ctx);
                break;

            case ChartControllerKeys.Normalized:
                if (_viewModel.ChartState.IsNormalizedVisible && hasSecondaryData)
                    await RenderChartAsync(ChartControllerKeys.Normalized, ctx);
                break;

            case ChartControllerKeys.DiffRatio:
                if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                    await RenderChartAsync(ChartControllerKeys.DiffRatio, ctx);
                break;

            case ChartControllerKeys.Distribution:
                if (_viewModel.ChartState.IsDistributionVisible)
                    await RenderChartAsync(ChartControllerKeys.Distribution, ctx);
                break;

            case ChartControllerKeys.WeeklyTrend:
                if (_viewModel.ChartState.IsWeeklyTrendVisible)
                    await RenderChartAsync(ChartControllerKeys.WeeklyTrend, ctx);
                break;
            case ChartControllerKeys.BarPie:
                if (_viewModel.ChartState.IsBarPieVisible)
                    await RenderChartAsync(ChartControllerKeys.BarPie, ctx);
                break;
            default:
                Debug.WriteLine($"[ChartRegistry] RenderSingleChart called with unknown key '{chartName}'.");
                break;
        }
    }

    /// <summary>
    ///     Renders all secondary charts (normalized, distribution, weekday trend, difference, ratio).
    /// </summary>
    private async Task RenderSecondaryCharts(ChartDataContext ctx)
    {
        await RenderChartAsync(ChartControllerKeys.Normalized, ctx);
        await RenderChartAsync(ChartControllerKeys.Distribution, ctx);
        await RenderChartAsync(ChartControllerKeys.WeeklyTrend, ctx);
        await RenderChartAsync(ChartControllerKeys.DiffRatio, ctx);
    }

    private void ClearSecondaryChartsAndReturn()
    {
        ClearChart(ChartControllerKeys.Normalized);
        ClearChart(ChartControllerKeys.DiffRatio);
        ClearChart(ChartControllerKeys.Distribution);
        // NOTE: WeekdayTrend intentionally not cleared here to preserve current behavior (tied to secondary presence).
        // Cartesian, Polar, and Scatter modes are handled by the adapter render check.
    }

    private Task RenderChartAsync(string key, ChartDataContext ctx)
    {
        return ResolveController(key).RenderAsync(ctx);
    }

    private void ClearChart(string key)
    {
        ResolveController(key).Clear(_viewModel.ChartState);
    }

    private Panel? GetChartPanel(string chartName)
    {
        try
        {
            return ResolveController(chartName) is IWpfChartPanelHost host ? host.ChartContentPanel : null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
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
        ClearRegisteredCharts();
        _viewModel.ChartState.LastContext = null;
    }

    #region Chart Visibility Toggle Handlers

    private void OnChartMainToggle(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleMain();
    }

    #endregion

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
        var shared = SharedMainWindowViewModelProvider.GetOrCreate(_connectionString);
        _chartState = shared.ChartState;
        _metricState = shared.MetricState;
        _uiState = shared.UiState;
        _metricSelectionService = shared.MetricSelectionService;
        _viewModel = shared.ViewModel;

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
        if (_viewModel == null)
            throw new InvalidOperationException("Shared MainWindowViewModel was not initialized.");
        DataContext = _viewModel;
    }

    private void InitializeUiBindings()
    {
        WireViewModelEvents();

        var factoryResult = _chartControllerFactory.Create(
            new ChartControllerFactoryContext(
                MainChartController,
                NormalizedChartController,
                DiffRatioChartController,
                DistributionChartController,
                WeekdayTrendChartController,
                TransformDataPanelController,
                BarPieChartController,
                _viewModel,
                () => _isInitializing,
                BeginUiBusyScope,
                _metricSelectionService,
                () => _chartRenderingOrchestrator,
                _chartUpdateCoordinator,
                () => _strategyCutOverService,
                _weekdayTrendChartUpdateCoordinator,
                _weeklyDistributionService,
                _hourlyDistributionService,
                _distributionPolarRenderingService,
                () => _distributionPolarTooltip,
                () => _tooltipManager,
                _chartRendererResolver,
                _chartSurfaceFactory));

        _mainAdapter = factoryResult.Main;
        _normalizedAdapter = factoryResult.Normalized;
        _diffRatioAdapter = factoryResult.DiffRatio;
        _distributionAdapter = factoryResult.Distribution;
        _weekdayTrendAdapter = factoryResult.WeekdayTrend;
        _transformAdapter = factoryResult.Transform;
        _barPieAdapter = factoryResult.BarPie;
        _chartControllerRegistry = factoryResult.Registry;
        ValidateChartControllerRegistry(_chartControllerRegistry);

        // Wire up MainChartController events
        MainChartController.ToggleRequested += _mainAdapter.OnToggleRequested;
        MainChartController.DisplayModeChanged += _mainAdapter.OnDisplayModeChanged;
        MainChartController.OverlaySubtypeChanged += _mainAdapter.OnOverlaySubtypeChanged;
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
        BarPieChartController.ToggleRequested += _barPieAdapter.OnToggleRequested;
        BarPieChartController.DisplayModeChanged += _barPieAdapter.OnDisplayModeChanged;
        BarPieChartController.BucketCountChanged += _barPieAdapter.OnBucketCountChanged;
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

    private static void ValidateChartControllerRegistry(IChartControllerRegistry registry)
    {
        foreach (var key in ChartControllerKeys.All)
        {
            var controller = registry.Get(key);
            if (controller == null)
                throw new InvalidOperationException($"Chart controller registry returned null for key '{key}'.");

            if (controller is IWpfChartPanelHost host && host.ChartContentPanel == null)
                throw new InvalidOperationException($"Chart controller '{key}' returned a null ChartContentPanel.");
        }
    }

    private IChartController ResolveController(string key)
    {
        if (_chartControllerRegistry != null)
            return _chartControllerRegistry.Get(key);

        Debug.WriteLine($"[ChartRegistry] Fallback resolve for '{key}' (registry not available).");
        return key switch
        {
                ChartControllerKeys.Main => _mainAdapter,
                ChartControllerKeys.Normalized => _normalizedAdapter,
                ChartControllerKeys.DiffRatio => _diffRatioAdapter,
                ChartControllerKeys.Distribution => _distributionAdapter,
                ChartControllerKeys.WeeklyTrend => _weekdayTrendAdapter,
                ChartControllerKeys.Transform => _transformAdapter,
                ChartControllerKeys.BarPie => _barPieAdapter,
                _ => throw new KeyNotFoundException($"Chart controller not found for key '{key}'.")
        };
    }

    private CartesianChart GetWpfCartesianChart(string key)
    {
        if (_chartControllerRegistry == null && _mainAdapter == null)
            return key switch
            {
                    ChartControllerKeys.Main => MainChartController.Chart,
                    ChartControllerKeys.Normalized => NormalizedChartController.Chart,
                    ChartControllerKeys.DiffRatio => DiffRatioChartController.Chart,
                    ChartControllerKeys.Distribution => DistributionChartController.Chart,
                    ChartControllerKeys.Transform => TransformDataPanelController.Chart,
                    _ => throw new KeyNotFoundException($"Chart controller not found for key '{key}'.")
            };

        if (ResolveController(key) is IWpfCartesianChartHost host)
            return host.Chart;

        throw new InvalidOperationException($"Chart controller '{key}' does not expose a Cartesian chart.");
    }

    private PolarChart GetPolarChart(string key)
    {
        if (_chartControllerRegistry == null && _distributionAdapter == null)
            return key switch
            {
                    ChartControllerKeys.Distribution => DistributionChartController.PolarChart,
                    _ => throw new KeyNotFoundException($"Chart controller not found for key '{key}'.")
            };

        if (ResolveController(key) is IPolarChartSurface polar)
            return polar.PolarChart;

        throw new InvalidOperationException($"Chart controller '{key}' does not expose a polar chart.");
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

        if (_viewModel.ChartState.IsBarPieVisible)
            _ = RenderChartAsync(ChartControllerKeys.BarPie, _viewModel.ChartState.LastContext ?? new ChartDataContext());
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

        if (_viewModel.ChartState.IsBarPieVisible)
            _ = RenderChartAsync(ChartControllerKeys.BarPie, _viewModel.ChartState.LastContext ?? new ChartDataContext());
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

    private IDistributionService CreateWeeklyDistributionService()
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);

        return new WeeklyDistributionService(_chartState.ChartTimestamps, strategyCutOverService);
    }

    private IDistributionService CreateHourlyDistributionService()
    {
        var dataPreparationService = new DataPreparationService();
        var strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);

        return new HourlyDistributionService(_chartState.ChartTimestamps, strategyCutOverService);
    }

    private ChartRenderingOrchestrator CreateChartRenderingOrchestrator()
    {
        var dataPreparationService = new DataPreparationService();
        _strategyCutOverService = new StrategyCutOverService(dataPreparationService, StrategyReachabilityStoreProbe.Default);

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

        _viewModel.SetResolutionTableName(ChartUiHelper.GetTableNameFromResolution(ResolutionCombo));

        _viewModel.LoadMetricsCommand.Execute(null);
    }

    private void InitializeCharts()
    {
        InitializeBarPieControlsFromRegistry();
        InitializeDistributionControlsFromRegistry();
        InitializeWeekdayTrendControls();
        InitializeChartBehavior();
        ClearChartsOnStartup();
        DisableAxisLabelsWhenNoData();
        SetDefaultChartTitles();
        UpdateDistributionChartTypeVisibility();
        InitializeDistributionPolarTooltip();
    }

    private void InitializeBarPieControlsFromRegistry()
    {
        if (ResolveController(ChartControllerKeys.BarPie) is IBarPieChartControllerExtras controller)
            controller.InitializeControls();
    }

    private void SyncInitialButtonStates()
    {
        var selectedSubtypeCount = CountSelectedSubtypes(_viewModel.MetricState.SelectedSeries);
        UpdatePrimaryDataRequiredButtonStates(selectedSubtypeCount);
        UpdateSecondaryDataRequiredButtonStates(selectedSubtypeCount);

        // Sync main chart button text with initial state
        SetChartVisibility(ChartControllerKeys.Main, _viewModel.ChartState.IsMainVisible);
        SetChartVisibility(ChartControllerKeys.BarPie, _viewModel.ChartState.IsBarPieVisible);

        SetChartVisibility(ChartControllerKeys.Transform, _viewModel.ChartState.IsTransformPanelVisible);
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
                { GetWpfCartesianChart(ChartControllerKeys.Main), ChartControllerKeys.Main },
                { GetWpfCartesianChart(ChartControllerKeys.Normalized), ChartControllerKeys.Normalized },
                { GetWpfCartesianChart(ChartControllerKeys.DiffRatio), ChartControllerKeys.DiffRatio },
                { GetWpfCartesianChart(ChartControllerKeys.Transform), ChartControllerKeys.Transform }
        };

        _tooltipManager = new ChartTooltipManager(parentWindow, chartLabels);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Main), ChartControllerKeys.Main);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Normalized), ChartControllerKeys.Normalized);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.DiffRatio), ChartControllerKeys.DiffRatio);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Transform), ChartControllerKeys.Transform);
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
        _viewModel.SelectionStateChanged += OnSelectionStateChanged;
    }

    private void InitializeDefaultUiState()
    {
        _viewModel.SetLoadingMetricTypes(false);
        _viewModel.SetLoadingSubtypes(false);
        _viewModel.SetMainVisible(true); // Default to visible (Show on startup)
        _viewModel.SetNormalizedVisible(false);
        _viewModel.SetDiffRatioVisible(false);
        _viewModel.SetDistributionVisible(false);
        _viewModel.ChartState.IsBarPieVisible = false;
        _viewModel.CompleteInitialization();

        _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
        _viewModel.ChartState.LastContext = new ChartDataContext();

        SyncMainDisplayModeSelection();

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
        InitializeDistributionControlsFromRegistry();
    }

    private void InitializeChartBehavior()
    {
        ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Main));
        InitializeDistributionChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Distribution));
        ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Normalized));
        ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.DiffRatio));
    }

    private void InitializeDistributionPolarTooltip()
    {
        _distributionPolarTooltip = new ToolTip
        {
                Placement = PlacementMode.Mouse,
                StaysOpen = true
        };
        var polarChart = GetPolarChart(ChartControllerKeys.Distribution);
        ToolTipService.SetToolTip(polarChart, _distributionPolarTooltip);
        polarChart.HoveredPointsChanged += OnDistributionPolarHoveredPointsChanged;
    }

    /// <summary>
    ///     Common method to initialize distribution chart behavior.
    /// </summary>
    private void InitializeDistributionChartBehavior(CartesianChart chart)
    {
        ChartUiHelper.InitializeChartBehavior(chart);
    }

    private void ClearChartsOnStartup()
    {
        // Clear charts on startup to prevent gibberish tick labels
        ClearRegisteredCharts();
    }

    private void ClearRegisteredCharts()
    {
        if (_chartControllerRegistry == null)
        {
            foreach (var key in ChartControllerKeys.All)
                ResolveController(key).Clear(_viewModel.ChartState);
            return;
        }

        foreach (var controller in _chartControllerRegistry.All())
            controller.Clear(_viewModel.ChartState);
    }

    private void ResetRegisteredChartsZoom()
    {
        if (_chartControllerRegistry == null)
        {
            foreach (var key in ChartControllerKeys.All)
                ResolveController(key).ResetZoom();
            return;
        }

        foreach (var controller in _chartControllerRegistry.All())
            controller.ResetZoom();
    }

    private void DisableAxisLabelsWhenNoData()
    {
        DisableAxisLabels(GetWpfCartesianChart(ChartControllerKeys.Main));
        DisableAxisLabels(GetWpfCartesianChart(ChartControllerKeys.Normalized));
        DisableAxisLabels(GetWpfCartesianChart(ChartControllerKeys.DiffRatio));
        DisableDistributionAxisLabels(GetWpfCartesianChart(ChartControllerKeys.Distribution));
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
        var polarChart = GetPolarChart(ChartControllerKeys.Distribution);
        polarChart.AngleAxes = Array.Empty<PolarAxis>();
        polarChart.RadiusAxes = Array.Empty<PolarAxis>();
        polarChart.Tag = null;
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
        ResolveController(ChartControllerKeys.Main).SetTitle("Metrics: Total");
        ResolveController(ChartControllerKeys.Normalized).SetTitle("Metrics: Normalized");
        ResolveController(ChartControllerKeys.DiffRatio).SetTitle("Difference / Ratio");
        UpdateDiffRatioOperationButton(); // Initialize button state
    }

    #endregion

    #region Data Loading and Selection Event Handlers

    /// <summary>
    ///     Event handler for Resolution selection change - reloads MetricTypes from the selected table.
    /// </summary>
    private void OnResolutionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isApplyingSelectionSync)
            return;

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

        _viewModel.SetResolutionTableName(ChartUiHelper.GetTableNameFromResolution(ResolutionCombo));
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
        if (_isInitializing || _isApplyingSelectionSync)
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
        if (_isApplyingSelectionSync)
            return;

        UpdateSelectedSubtypesInViewModel();

        if (HasLoadedData())
            return;

        if (ShouldRefreshDateRangeForCurrentSelection())
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
            ClearChartCache(ChartControllerKeys.Distribution);
            ClearChartCache(ChartControllerKeys.WeeklyTrend);
            ClearChartCache(ChartControllerKeys.Normalized);
            ClearChartCache(ChartControllerKeys.DiffRatio);
            ClearChartCache(ChartControllerKeys.Transform);
            ResetTransformSelectionsPendingLoad();
            var dataLoaded = await _viewModel.LoadMetricDataAsync();
            if (!dataLoaded)
            {
                ClearAllCharts();
                return;
            }

            ClearHiddenChartsAfterLoad();
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

    #region Chart Configuration and Helper Methods

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
        ResetRegisteredChartsZoom();
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

    private async void OnExportReachability(object sender, RoutedEventArgs e)
    {
        var records = StrategyReachabilityStoreProbe.Default.Snapshot();
        var selectedSeries = _viewModel.MetricState.SelectedSeries.ToList();
        var distinctSeries = GetDistinctSelectedSeries();
        var paritySnapshot = await BuildDistributionParitySnapshotAsync(_viewModel.ChartState.LastContext);
        var combinedParitySnapshot = BuildCombinedMetricParitySnapshot(_viewModel.ChartState.LastContext);
        var singleParitySnapshot = BuildSingleMetricParitySnapshot(_viewModel.ChartState.LastContext);
        var multiParitySnapshot = BuildMultiMetricParitySnapshot(_viewModel.ChartState.LastContext);
        var normalizedParitySnapshot = BuildNormalizedParitySnapshot(_viewModel.ChartState.LastContext);
        var weekdayTrendParitySnapshot = BuildWeekdayTrendParitySnapshot(_viewModel.ChartState.LastContext);
        var transformParitySnapshot = await BuildTransformParitySnapshotAsync(_viewModel.ChartState.LastContext);
        var paritySummary = BuildParitySummary(paritySnapshot, combinedParitySnapshot, singleParitySnapshot, multiParitySnapshot, normalizedParitySnapshot, weekdayTrendParitySnapshot, transformParitySnapshot);
        var parityWarnings = BuildParityWarnings(paritySnapshot, combinedParitySnapshot, singleParitySnapshot, multiParitySnapshot, normalizedParitySnapshot, weekdayTrendParitySnapshot, transformParitySnapshot, selectedSeries.Count);

        if (records.Count == 0)
            MessageBox.Show("No reachability records captured yet. Export will include parity data if available.", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Information);

        var selectedDistributionSettings = _viewModel.ChartState.GetDistributionSettings(_viewModel.ChartState.SelectedDistributionMode);
        var exportPayload = new
        {
                ExportedAtUtc = DateTime.UtcNow,
                MetricState = new
                {
                        _viewModel.MetricState.SelectedMetricType,
                        _viewModel.MetricState.FromDate,
                        _viewModel.MetricState.ToDate,
                        _viewModel.MetricState.ResolutionTableName,
                        SelectedSeriesCount = selectedSeries.Count,
                        DistinctSelectedSeriesCount = distinctSeries.Count,
                        SelectedSeries = selectedSeries.Select(series => new
                        {
                                series.MetricType,
                                series.Subtype,
                                series.DisplayMetricType,
                                series.DisplaySubtype,
                                series.DisplayName,
                                series.DisplayKey
                        })
                },
                ChartState = new
                {
                        _viewModel.ChartState.IsMainVisible,
                        _viewModel.ChartState.IsNormalizedVisible,
                        _viewModel.ChartState.IsDiffRatioVisible,
                        _viewModel.ChartState.IsDistributionVisible,
                        _viewModel.ChartState.IsWeeklyTrendVisible,
                        _viewModel.ChartState.IsTransformPanelVisible,
                        _viewModel.ChartState.IsBarPieVisible,
                        _viewModel.ChartState.MainChartDisplayMode,
                        _viewModel.ChartState.IsDiffRatioDifferenceMode,
                        _viewModel.ChartState.SelectedDistributionMode,
                        SelectedDistributionSettings = new
                        {
                                selectedDistributionSettings.UseFrequencyShading,
                                selectedDistributionSettings.IntervalCount
                        },
                        _viewModel.ChartState.IsDistributionPolarMode,
                        _viewModel.ChartState.WeekdayTrendChartMode,
                        _viewModel.ChartState.WeekdayTrendAverageWindow,
                        _viewModel.ChartState.ShowMonday,
                        _viewModel.ChartState.ShowTuesday,
                        _viewModel.ChartState.ShowWednesday,
                        _viewModel.ChartState.ShowThursday,
                        _viewModel.ChartState.ShowFriday,
                        _viewModel.ChartState.ShowSaturday,
                        _viewModel.ChartState.ShowSunday,
                        _viewModel.ChartState.ShowAverage,
                        _viewModel.ChartState.BarPieBucketCount,
                        BarPieMode = BarPieChartController.PieModeRadio.IsChecked == true ? "Pie" : "Bar"
                },
                CmsConfiguration = new
                {
                        CmsConfiguration.UseCmsData,
                        CmsConfiguration.UseCmsForSingleMetric,
                        CmsConfiguration.UseCmsForMultiMetric,
                        CmsConfiguration.UseCmsForCombinedMetric,
                        CmsConfiguration.UseCmsForDifference,
                        CmsConfiguration.UseCmsForRatio,
                        CmsConfiguration.UseCmsForNormalized,
                        CmsConfiguration.UseCmsForWeeklyDistribution,
                        CmsConfiguration.UseCmsForWeekdayTrend,
                        CmsConfiguration.UseCmsForHourlyDistribution,
                        CmsConfiguration.UseCmsForBarPie
                },
                ReachabilityRecords = records,
                DistributionParity = paritySnapshot,
                CombinedMetricParity = combinedParitySnapshot,
                SingleMetricParity = singleParitySnapshot,
                MultiMetricParity = multiParitySnapshot,
                NormalizedParity = normalizedParitySnapshot,
                WeekdayTrendParity = weekdayTrendParitySnapshot,
                TransformParity = transformParitySnapshot,
                ParitySummary = paritySummary,
                ParityWarnings = parityWarnings
        };

        if (parityWarnings.Count > 0)
            MessageBox.Show($"Export will include parity warnings:\n- {string.Join("\n- ", parityWarnings)}", "Parity Warnings", MessageBoxButton.OK, MessageBoxImage.Warning);

        var targetDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "documents");
        Directory.CreateDirectory(targetDir);
        var fileName = $"reachability-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var filePath = Path.Combine(targetDir, fileName);

        var options = new JsonSerializerOptions
        {
                WriteIndented = true
        };

        try
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize(exportPayload, options));
            if (!File.Exists(filePath))
                throw new IOException("Export completed without creating the output file.");

            MessageBox.Show($"Reachability snapshot exported to:\n{filePath}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to export reachability snapshot:\n{ex.Message}", "Reachability Export", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private sealed class DistributionParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public object? Selection { get; set; }
        public string? DataSource { get; set; }
        public int LegacySamples { get; set; }
        public int CmsSamplesTotal { get; set; }
        public int CmsSamplesInRange { get; set; }
        public ParityResultSnapshot? Weekly { get; set; }
        public ParityResultSnapshot? Hourly { get; set; }
    }

    private sealed class ParityResultSnapshot
    {
        public bool Passed { get; set; }
        public string? Message { get; set; }
        public object? Details { get; set; }
        public string? Error { get; set; }
    }

    private sealed class ParitySummarySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public bool? OverallPassed { get; set; }
        public bool? WeeklyPassed { get; set; }
        public bool? HourlyPassed { get; set; }
        public bool? CombinedMetricPassed { get; set; }
        public bool? SingleMetricPassed { get; set; }
        public bool? MultiMetricPassed { get; set; }
        public bool? NormalizedPassed { get; set; }
        public bool? WeekdayTrendPassed { get; set; }
        public bool? TransformPassed { get; set; }
        public string[] StrategiesEvaluated { get; set; } = Array.Empty<string>();
    }

    private static ParitySummarySnapshot BuildParitySummary(DistributionParitySnapshot distributionSnapshot, CombinedMetricParitySnapshot combinedSnapshot, SimpleParitySnapshot singleSnapshot, SimpleParitySnapshot multiSnapshot, SimpleParitySnapshot normalizedSnapshot, SimpleParitySnapshot weekdayTrendSnapshot, TransformParitySnapshot transformSnapshot)
    {
        var weeklyPassed = distributionSnapshot.Weekly?.Passed;
        var hourlyPassed = distributionSnapshot.Hourly?.Passed;
        var combinedPassed = combinedSnapshot.Result?.Passed;
        var singlePassed = singleSnapshot.Result?.Passed;
        var multiPassed = multiSnapshot.Result?.Passed;
        var normalizedPassed = normalizedSnapshot.Result?.Passed;
        var weekdayTrendPassed = weekdayTrendSnapshot.Result?.Passed;
        var transformPassed = transformSnapshot.Result?.Passed;
        var completed = string.Equals(distributionSnapshot.Status, "Completed", StringComparison.OrdinalIgnoreCase);

        return new ParitySummarySnapshot
        {
                Status = distributionSnapshot.Status,
                WeeklyPassed = weeklyPassed,
                HourlyPassed = hourlyPassed,
                CombinedMetricPassed = combinedPassed,
                SingleMetricPassed = singlePassed,
                MultiMetricPassed = multiPassed,
                NormalizedPassed = normalizedPassed,
                WeekdayTrendPassed = weekdayTrendPassed,
                TransformPassed = transformPassed,
                OverallPassed = completed && weeklyPassed == true && hourlyPassed == true && combinedPassed != false && singlePassed != false && multiPassed != false && normalizedPassed != false && weekdayTrendPassed != false && transformPassed != false,
                StrategiesEvaluated = new[]
                {
                        "WeeklyDistribution",
                        "HourlyDistribution",
                        "CombinedMetric",
                        "SingleMetric",
                        "MultiMetric",
                        "Normalized",
                        "WeekdayTrend",
                        "Transform"
                }
        };
    }

    private static IReadOnlyList<string> BuildParityWarnings(DistributionParitySnapshot distributionSnapshot, CombinedMetricParitySnapshot combinedSnapshot, SimpleParitySnapshot singleSnapshot, SimpleParitySnapshot multiSnapshot, SimpleParitySnapshot normalizedSnapshot, SimpleParitySnapshot weekdayTrendSnapshot, TransformParitySnapshot transformSnapshot, int selectedSeriesCount)
    {
        var warnings = new List<string>();

        AddWarningIfUnavailable(warnings, "WeeklyDistribution", distributionSnapshot.Status, distributionSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "CombinedMetric", combinedSnapshot.Status, combinedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "SingleMetric", singleSnapshot.Status, singleSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "MultiMetric", multiSnapshot.Status, multiSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Normalized", normalizedSnapshot.Status, normalizedSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "WeekdayTrend", weekdayTrendSnapshot.Status, weekdayTrendSnapshot.Reason);
        AddWarningIfUnavailable(warnings, "Transform", transformSnapshot.Status, transformSnapshot.Reason);

        if (selectedSeriesCount < 2)
            warnings.Add("Multiple series required for CombinedMetric/Normalized/Transform parity; select at least two series.");

        return warnings;
    }

    private static void AddWarningIfUnavailable(List<string> warnings, string label, string status, string? reason)
    {
        if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
            return;

        var detail = string.IsNullOrWhiteSpace(reason) ? "Unavailable" : reason;
        warnings.Add($"{label} parity not completed: {detail}");
    }

    private async Task<DistributionParitySnapshot> BuildDistributionParitySnapshotAsync(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.From == default || ctx.To == default)
        {
            var snapshot = new DistributionParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No chart context available"
            };
            Debug.WriteLine($"[ParityExport] {snapshot.Status}: {snapshot.Reason}");
            return snapshot;
        }

        var selection = ResolveDistributionSelection(ctx);
        if (selection == null)
        {
            var snapshot = new DistributionParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No distribution series selected"
            };
            Debug.WriteLine($"[ParityExport] {snapshot.Status}: {snapshot.Reason}");
            return snapshot;
        }

        var resolutionTableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (legacyData, cmsSeries, dataSource) = await ResolveDistributionParityDataAsync(ctx, selection, resolutionTableName);
        if (legacyData == null || legacyData.Count == 0)
        {
            var snapshot = new DistributionParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No legacy distribution data available",
                    DataSource = dataSource
            };
            Debug.WriteLine($"[ParityExport] {snapshot.Status}: {snapshot.Reason} (Source={dataSource})");
            return snapshot;
        }

        var cmsSampleTotal = CountCmsSamples(cmsSeries);
        var cmsSampleInRange = CountCmsSamples(cmsSeries, ctx.From, ctx.To);

        if (cmsSeries == null)
        {
            var snapshot = new DistributionParitySnapshot
            {
                    Status = "CmsUnavailable",
                    DataSource = dataSource,
                    CmsSamplesTotal = cmsSampleTotal,
                    CmsSamplesInRange = cmsSampleInRange
            };
            Debug.WriteLine($"[ParityExport] {snapshot.Status}: CMS series unavailable (Source={dataSource})");
            return snapshot;
        }

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);

        var displayName = selection.DisplayName ?? ctx.DisplayName1 ?? string.Empty;
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = legacyData,
                Label1 = displayName,
                From = ctx.From,
                To = ctx.To
        };

        var parityContext = new ChartDataContext
        {
                PrimaryCms = cmsSeries,
                Data1 = legacyData,
                DisplayName1 = displayName,
                MetricType = selection.MetricType ?? ctx.MetricType,
                PrimaryMetricType = selection.MetricType ?? ctx.PrimaryMetricType,
                PrimarySubtype = selection.Subtype,
                DisplayPrimaryMetricType = selection.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplayPrimarySubtype = selection.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
                From = ctx.From,
                To = ctx.To
        };

        var weeklyParity = ExecuteParitySafe(strategyCutOverService, StrategyType.WeeklyDistribution, parityContext, parameters);
        var hourlyParity = ExecuteParitySafe(strategyCutOverService, StrategyType.HourlyDistribution, parityContext, parameters);

        return new DistributionParitySnapshot
        {
                Status = "Completed",
                Selection = new
                {
                        selection.MetricType,
                        selection.Subtype,
                        selection.DisplayMetricType,
                        selection.DisplaySubtype,
                        selection.DisplayName,
                        selection.DisplayKey
                },
                DataSource = dataSource,
                LegacySamples = legacyData.Count,
                CmsSamplesTotal = cmsSampleTotal,
                CmsSamplesInRange = cmsSampleInRange,
                Weekly = weeklyParity,
                Hourly = hourlyParity
        };
    }

    private sealed class CombinedMetricParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    private sealed class SimpleParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    private CombinedMetricParitySnapshot BuildCombinedMetricParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null || ctx.Data2 == null)
            return new CombinedMetricParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "Combined metric requires primary and secondary data"
            };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries || ctx.SecondaryCms is not ICanonicalMetricSeries)
            return new CombinedMetricParitySnapshot
            {
                    Status = "CmsUnavailable",
                    Reason = "Combined metric CMS series missing"
            };

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1,
                LegacyData2 = ctx.Data2,
                Label1 = ctx.DisplayName1,
                Label2 = ctx.DisplayName2,
                From = ctx.From,
                To = ctx.To
        };

        var result = ExecuteParitySafe(strategyCutOverService, StrategyType.CombinedMetric, ctx, parameters);

        return new CombinedMetricParitySnapshot
        {
                Status = "Completed",
                Result = result
        };
    }

    private SimpleParitySnapshot BuildSingleMetricParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "Primary series required"
            };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot
            {
                    Status = "CmsUnavailable",
                    Reason = "Primary CMS series missing"
            };

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1,
                Label1 = ctx.DisplayName1,
                From = ctx.From,
                To = ctx.To
        };

        var result = ExecuteParitySafe(strategyCutOverService, StrategyType.SingleMetric, ctx, parameters);

        return new SimpleParitySnapshot
        {
                Status = "Completed",
                Result = result
        };
    }

    private SimpleParitySnapshot BuildMultiMetricParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "Primary series required"
            };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot
            {
                    Status = "CmsUnavailable",
                    Reason = "Primary CMS series missing"
            };

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var parameters = new StrategyCreationParameters
        {
                LegacySeries = new List<IEnumerable<MetricData>>
                {
                        ctx.Data1,
                        ctx.Data2 ?? Array.Empty<MetricData>()
                },
                Labels = new List<string>
                {
                        ctx.DisplayName1,
                        ctx.DisplayName2
                },
                From = ctx.From,
                To = ctx.To
        };

        var result = ExecuteParitySafe(strategyCutOverService, StrategyType.MultiMetric, ctx, parameters);

        return new SimpleParitySnapshot
        {
                Status = "Completed",
                Result = result
        };
    }

    private SimpleParitySnapshot BuildNormalizedParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null || ctx.Data2 == null)
            return new SimpleParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "Primary and secondary series required"
            };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries || ctx.SecondaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot
            {
                    Status = "CmsUnavailable",
                    Reason = "CMS series missing"
            };

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1,
                LegacyData2 = ctx.Data2,
                Label1 = ctx.DisplayName1,
                Label2 = ctx.DisplayName2,
                From = ctx.From,
                To = ctx.To,
                NormalizationMode = _viewModel.ChartState.SelectedNormalizationMode
        };

        var result = ExecuteParitySafe(strategyCutOverService, StrategyType.Normalized, ctx, parameters);

        return new SimpleParitySnapshot
        {
                Status = "Completed",
                Result = result
        };
    }

    private SimpleParitySnapshot BuildWeekdayTrendParitySnapshot(ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "Primary series required"
            };

        if (ctx.PrimaryCms is not ICanonicalMetricSeries)
            return new SimpleParitySnapshot
            {
                    Status = "CmsUnavailable",
                    Reason = "Primary CMS series missing"
            };

        var strategyCutOverService = _strategyCutOverService ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
        var parameters = new StrategyCreationParameters
        {
                LegacyData1 = ctx.Data1,
                Label1 = ctx.DisplayName1,
                From = ctx.From,
                To = ctx.To
        };

        var result = ExecuteParitySafe(strategyCutOverService, StrategyType.WeekdayTrend, ctx, parameters);

        return new SimpleParitySnapshot
        {
                Status = "Completed",
                Result = result
        };
    }

    private sealed class TransformParitySnapshot
    {
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? Operation { get; set; }
        public bool IsUnary { get; set; }
        public bool ExpressionAvailable { get; set; }
        public int LegacySamples { get; set; }
        public int NewSamples { get; set; }
        public ParityResultSnapshot? Result { get; set; }
    }

    private async Task<TransformParitySnapshot> BuildTransformParitySnapshotAsync(ChartDataContext? ctx)
    {
        if (ctx == null)
            return new TransformParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No chart context available"
            };

        var operation = GetSelectedTransformOperation();
        if (string.IsNullOrWhiteSpace(operation))
            return new TransformParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No transform operation selected"
            };

        var (primarySelection, secondarySelection) = ResolveTransformSelections(ctx);
        var primaryData = await ResolveTransformParityDataAsync(ctx, primarySelection);
        if (primaryData == null || primaryData.Count == 0)
            return new TransformParitySnapshot
            {
                    Status = "Unavailable",
                    Reason = "No primary data available for transform"
            };

        var isUnary = IsUnaryTransform(operation);
        IReadOnlyList<MetricData>? secondaryData = null;
        if (!isUnary)
        {
            secondaryData = await ResolveTransformParityDataAsync(ctx, secondarySelection);
            if (secondaryData == null || secondaryData.Count == 0)
                return new TransformParitySnapshot
                {
                        Status = "Unavailable",
                        Reason = "No secondary data available for binary transform"
                };
        }

        var result = isUnary ? ComputeUnaryTransformParity(primaryData, operation) : ComputeBinaryTransformParity(primaryData, secondaryData!, operation);

        return new TransformParitySnapshot
        {
                Status = "Completed",
                Operation = operation,
                IsUnary = isUnary,
                ExpressionAvailable = result.ExpressionAvailable,
                LegacySamples = result.LegacySamples,
                NewSamples = result.NewSamples,
                Result = result.Result
        };
    }

    private string? GetSelectedTransformOperation()
    {
        if (TransformDataPanelController.TransformOperationCombo.SelectedItem is ComboBoxItem item)
            return item.Tag?.ToString();

        return null;
    }

    private static bool IsUnaryTransform(string operation)
    {
        return string.Equals(operation, "Log", StringComparison.OrdinalIgnoreCase) || string.Equals(operation, "Sqrt", StringComparison.OrdinalIgnoreCase);
    }

    private(MetricSeriesSelection? Primary, MetricSeriesSelection? Secondary) ResolveTransformSelections(ChartDataContext ctx)
    {
        var primary = _viewModel.ChartState.SelectedTransformPrimarySeries;
        var secondary = _viewModel.ChartState.SelectedTransformSecondarySeries;

        if (primary != null || secondary != null)
            return (primary, secondary);

        var primaryMetricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        var primarySelection = string.IsNullOrWhiteSpace(primaryMetricType) ? null : new MetricSeriesSelection(primaryMetricType, ctx.PrimarySubtype);

        MetricSeriesSelection? secondarySelection = null;
        if (!string.IsNullOrWhiteSpace(ctx.SecondaryMetricType))
            secondarySelection = new MetricSeriesSelection(ctx.SecondaryMetricType, ctx.SecondarySubtype);

        return (primarySelection, secondarySelection);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformParityDataAsync(ChartDataContext ctx, MetricSeriesSelection? selection)
    {
        if (selection == null)
            return null;

        if (ctx.Data1 != null && IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (ctx.Data2 != null && IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }

    private static(ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeUnaryTransformParity(IReadOnlyList<MetricData> data, string operation)
    {
        var prepared = data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        if (prepared.Count == 0)
            return (new ParityResultSnapshot
            {
                    Passed = false,
                    Error = "No valid data points"
            }, 0, 0, false);

        var values = prepared.Select(d => (double)d.Value!.Value).ToList();
        var legacyOp = operation switch
        {
                "Log" => UnaryOperators.Logarithm,
                "Sqrt" => UnaryOperators.SquareRoot,
                _ => x => x
        };

        var legacy = MathHelper.ApplyUnaryOperation(values, legacyOp);

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0);
        var expressionAvailable = expression != null;
        var modern = expression != null ?
                TransformExpressionEvaluator.Evaluate(expression,
                        new List<IReadOnlyList<MetricData>>
                        {
                                prepared
                        }) :
                legacy;

        var parity = CompareTransformResults(legacy, modern, prepared.Count);

        return (parity, legacy.Count, modern.Count, expressionAvailable);
    }

    private static(ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeBinaryTransformParity(IReadOnlyList<MetricData> data1, IReadOnlyList<MetricData> data2, string operation)
    {
        var prepared1 = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
        var prepared2 = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

        var (aligned1, aligned2) = TransformExpressionEvaluator.AlignMetricsByTimestamp(prepared1, prepared2);
        if (aligned1.Count == 0 || aligned2.Count == 0)
            return (new ParityResultSnapshot
            {
                    Passed = false,
                    Error = "No aligned data points"
            }, 0, 0, false);

        var values1 = aligned1.Select(d => (double)d.Value!.Value).ToList();
        var values2 = aligned2.Select(d => (double)d.Value!.Value).ToList();

        var legacyOp = operation switch
        {
                "Add" => BinaryOperators.Sum,
                "Subtract" => BinaryOperators.Difference,
                _ => (a, b) => a
        };

        var legacy = MathHelper.ApplyBinaryOperation(values1, values2, legacyOp);

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
        var expressionAvailable = expression != null;
        var modern = expression != null ?
                TransformExpressionEvaluator.Evaluate(expression,
                        new List<IReadOnlyList<MetricData>>
                        {
                                aligned1,
                                aligned2
                        }) :
                legacy;

        var parity = CompareTransformResults(legacy, modern, legacy.Count);

        return (parity, legacy.Count, modern.Count, expressionAvailable);
    }

    private static ParityResultSnapshot CompareTransformResults(IReadOnlyList<double> legacy, IReadOnlyList<double> modern, int expectedCount)
    {
        if (legacy.Count != modern.Count)
            return new ParityResultSnapshot
            {
                    Passed = false,
                    Error = $"Result count mismatch: legacy={legacy.Count}, new={modern.Count}"
            };

        var epsilon = 1e-6;
        for (var i = 0; i < legacy.Count; i++)
        {
            var l = legacy[i];
            var n = modern[i];
            if (double.IsNaN(l) && double.IsNaN(n))
                continue;

            if (Math.Abs(l - n) > epsilon)
                return new ParityResultSnapshot
                {
                        Passed = false,
                        Error = $"Value mismatch at index {i}: legacy={l}, new={n}"
                };
        }

        return new ParityResultSnapshot
        {
                Passed = true,
                Message = "Transform parity validation passed"
        };
    }

    private MetricSeriesSelection? ResolveDistributionSelection(ChartDataContext ctx)
    {
        if (_viewModel.ChartState.SelectedDistributionSeries != null)
            return _viewModel.ChartState.SelectedDistributionSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms, string DataSource)> ResolveDistributionParityDataAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        if (ctx.Data1 != null && IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Primary");

        if (ctx.Data2 != null && IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries, "ChartContext.Secondary");

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Fallback");

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms, "MetricSelectionService");
    }

    private static ParityResultSnapshot ExecuteParitySafe(IStrategyCutOverService strategyCutOverService, StrategyType strategyType, ChartDataContext ctx, StrategyCreationParameters parameters)
    {
        try
        {
            var legacy = strategyCutOverService.CreateLegacyStrategy(strategyType, parameters);
            var cms = strategyCutOverService.CreateCmsStrategy(strategyType, ctx, parameters);
            var result = strategyCutOverService.ValidateParity(legacy, cms);

            return new ParityResultSnapshot
            {
                    Passed = result.Passed,
                    Message = result.Message,
                    Details = result.Details
            };
        }
        catch (Exception ex)
        {
            return new ParityResultSnapshot
            {
                    Passed = false,
                    Error = ex.Message
            };
        }
    }

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static int CountCmsSamples(ICanonicalMetricSeries? series, DateTime? from = null, DateTime? to = null)
    {
        if (series?.Samples == null)
            return 0;

        if (!from.HasValue || !to.HasValue)
            return series.Samples.Count(s => s.Value.HasValue);

        var toEndOfDay = to.Value.Date.AddDays(1).AddTicks(-1);
        var fromStartOfDay = from.Value.Date;

        return series.Samples.Count(s => s.Value.HasValue && s.Timestamp.LocalDateTime >= fromStartOfDay && s.Timestamp.LocalDateTime <= toEndOfDay);
    }

    /// <summary>
    ///     Handles hover updates for the distribution polar tooltip.
    /// </summary>
    private void OnDistributionPolarHoveredPointsChanged(IChartView chart, IEnumerable<ChartPoint>? newPoints, IEnumerable<ChartPoint>? oldPoints)
    {
        if (_distributionPolarTooltip == null)
            return;

        var state = GetPolarChart(ChartControllerKeys.Distribution).Tag as DistributionPolarTooltipState;
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

        ResolveController(ChartControllerKeys.Main).SetTitle($"{leftName} vs. {rightName}");
        ResolveController(ChartControllerKeys.Normalized).SetTitle($"{leftName} ~ {rightName}");
        ResolveController(ChartControllerKeys.DiffRatio).SetTitle($"{leftName} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {rightName}");
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
        _tooltipManager.UpdateChartLabel(GetWpfCartesianChart(ChartControllerKeys.Main), chartMainLabel);

        var chartDiffRatioLabel = !string.IsNullOrEmpty(label2) ? $"{label1} {(_viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/")} {label2}" : label1;
        _tooltipManager.UpdateChartLabel(GetWpfCartesianChart(ChartControllerKeys.DiffRatio), chartDiffRatioLabel);
    }

    private void UpdateChartTitlesFromCombos()
    {
        var selections = GetDistinctSelectedSeries();
        var display1 = selections.Count > 0 ? selections[0].DisplayName : string.Empty;
        var display2 = selections.Count > 1 ? selections[1].DisplayName : string.Empty;

        SetChartTitles(display1, display2);
        UpdateChartLabels();
    }

    private void ClearHiddenChartsAfterLoad()
    {
        foreach (var key in ChartVisibilityHelper.GetHiddenChartKeys(_viewModel.ChartState))
            ClearChart(key);
    }

    #endregion
}
