using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
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
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using DataVisualiser.UI.Events;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.Theming;
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
    private readonly MainChartsViewChartPipelineFactory _chartPipelineFactory = new();
    private readonly MainChartsViewChartPresentationCoordinator _chartPresentationCoordinator = new();
    private readonly MainChartsViewChartUpdateCoordinator _chartUpdateCoordinatorHost = new();
    private readonly MainChartsViewEvidenceExportCoordinator _evidenceExportCoordinator = new();
    private readonly MainChartsViewResolutionResetCoordinator _resolutionResetCoordinator = new();
    private MainChartsEvidenceExportService _evidenceExportService = null!;
    private readonly MainChartsViewStartupCoordinator _startupCoordinator = new();

    private ChartState _chartState = null!;
    private readonly IChartRendererResolver _chartRendererResolver = new ChartRendererResolver();
    private readonly IChartSurfaceFactory _chartSurfaceFactory;

    private MetricState _metricState = null!;
    private UiState _uiState = null!;
    private IChartControllerRegistry? _chartControllerRegistry;
    private ChartRenderingOrchestrator? _chartRenderingOrchestrator;
    private ChartUpdateCoordinator _chartUpdateCoordinator = null!;

    private string _connectionString = null!;
    private DistributionPolarRenderingService _distributionPolarRenderingService = null!;
    private ToolTip _distributionPolarTooltip = null!;
    private IDistributionService _hourlyDistributionService = null!;
    private bool _isChangingResolution;

    private bool _isInitializing = true;
    private bool _isMetricTypeChangePending;
    private bool _isApplyingSelectionSync;
    private MainChartsViewThemeCoordinator _themeCoordinator = null!;
    private MainChartsViewEventBinder? _viewModelEventBinder;

    private MetricSelectionService _metricSelectionService = null!;
    private SubtypeSelectorManager _selectorManager = null!;
    private IStrategyCutOverService? _strategyCutOverService;
    private List<MetricNameOption>? _subtypeList;
    private ChartTooltipManager? _tooltipManager;
    private int _uiBusyDepth;
    private MainWindowViewModel _viewModel = null!;
    private WeekdayTrendChartUpdateCoordinator _weekdayTrendChartUpdateCoordinator = null!;
    private IDistributionService _weeklyDistributionService = null!;

    public MainChartsView()
    {
        _chartSurfaceFactory = new ChartSurfaceFactory(_chartRendererResolver);
        InitializeComponent();
        _themeCoordinator = new MainChartsViewThemeCoordinator(
            AppThemeService.Default,
            content =>
            {
                if (ThemeToggleButton != null)
                    ThemeToggleButton.Content = content;
            },
            action =>
            {
                if (Dispatcher.CheckAccess())
                    action();
                else
                    Dispatcher.Invoke(action);
            });

        InitializeInfrastructure();
        InitializeChartPipeline();
        InitializeViewModel();
        InitializeUiBindings();
        InitializeTooltips();
        _themeCoordinator.Attach();

        ExecuteStartupSequence();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private DiffRatioChartController DiffRatioChartController { get; } = new();

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _viewModelEventBinder?.Bind();
        _themeCoordinator.Attach();

        if (_tooltipManager == null && _chartControllerRegistry != null)
        {
            InitializeTooltipManager();
            InitializeTooltips();
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        _viewModelEventBinder?.Unbind();
        _themeCoordinator.Detach();
        // Dispose tooltip manager to prevent memory leaks
        _tooltipManager?.Dispose();
        _tooltipManager = null;
    }

    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        _themeCoordinator.ToggleTheme();
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
        if (ResolveController(ChartControllerKeys.Main) is IMainChartControllerExtras controller)
            controller.SetStackedAvailability(canStack);
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
        if (ResolveController(ChartControllerKeys.BarPie) is IBarPieChartControllerExtras controller)
            controller.SelectBucketCount(target);
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

    private async void OnChartUpdateRequested(object? sender, ChartUpdateRequestedEventArgs e)
    {
        if (await TryHandleVisibilityOnlyToggleAsync(e))
            return;

        _chartUpdateCoordinatorHost.ApplyAllChartVisibilities(e, _viewModel.ChartState, CreateChartUpdateActions());

        if (!string.IsNullOrEmpty(e.ToggledChartName))
            await HandleSingleChartUpdateAsync(e);
        else if (e.ShouldRenderCharts && !e.IsVisibilityOnlyToggle)
            await RenderChartsFromLastContext();
    }

    private Task<bool> TryHandleVisibilityOnlyToggleAsync(ChartUpdateRequestedEventArgs e)
    {
        return _chartUpdateCoordinatorHost.TryHandleVisibilityOnlyToggleAsync(
            e,
            _viewModel.ChartState,
            _viewModel.ChartState.LastContext,
            CreateChartUpdateActions());
    }

    private void SetChartVisibility(string key, bool isVisible)
    {
        ResolveController(key).SetVisible(isVisible);
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

    private Task HandleSingleChartUpdateAsync(ChartUpdateRequestedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(e.ToggledChartName))
            return Task.CompletedTask;

        var ctx = _viewModel.ChartState.LastContext;
        if (!MainChartsViewChartUpdateCoordinator.ShouldRenderCharts(ctx))
            return Task.CompletedTask;

        return RenderSingleChartAsync(e.ToggledChartName, ctx!);
    }

    private async Task RenderChartsFromLastContext()
    {
        using var busyScope = BeginUiBusyScope();
        await _chartUpdateCoordinatorHost.RenderVisibleChartsAsync(
            _viewModel.ChartState,
            _viewModel.ChartState.LastContext,
            CreateChartUpdateActions());
    }

    private async Task RenderSingleChartAsync(string chartName, ChartDataContext ctx)
    {
        using var busyScope = BeginUiBusyScope();
        await _chartUpdateCoordinatorHost.RenderSingleChartAsync(
            _viewModel.ChartState,
            chartName,
            ctx,
            CreateChartUpdateActions());
    }

    private Task RenderChartAsync(string key, ChartDataContext ctx)
    {
        return ResolveController(key).RenderAsync(ctx);
    }

    private void ClearChart(string key)
    {
        ResolveController(key).Clear(_viewModel.ChartState);
    }

    private void OnChartVisibilityChanged(object? sender, ChartVisibilityChangedEventArgs e)
    {
        if (_chartControllerRegistry == null)
            return;

        if (ChartControllerKeys.All.Any(key => string.Equals(key, e.ChartName, StringComparison.OrdinalIgnoreCase)))
            SetChartVisibility(e.ChartName, e.IsVisible);
    }

    private void OnErrorOccured(object? sender, ErrorEventArgs e)
    {
        if (_isChangingResolution)
        {
            _resolutionResetCoordinator.HandleSuppressedError(
                () => _isChangingResolution = false,
                ClearAllCharts);
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
        _evidenceExportService = new MainChartsEvidenceExportService(
            new ReachabilityExportWriter(),
            new ReachabilityExportPathResolver(),
            new StrategyReachabilityEvidenceStore(),
            _metricSelectionService,
            () => _strategyCutOverService,
            () => ResolveController(ChartControllerKeys.Transform) is ITransformPanelControllerExtras transformController ? transformController.GetSelectedOperationTag() : null,
            () => ResolveController(ChartControllerKeys.BarPie) is IBarPieChartControllerExtras barPieController ? barPieController.GetDisplayMode() : "Bar");

        InitializeTooltipManager();
    }

    private void InitializeChartPipeline()
    {
        if (_tooltipManager == null)
            throw new InvalidOperationException("Tooltip manager is not initialized.");

        var pipeline = _chartPipelineFactory.Create(
            new MainChartsViewChartPipelineFactory.Context(
                _chartState.ChartTimestamps,
                _tooltipManager,
                _connectionString));

        _chartUpdateCoordinator = pipeline.ChartUpdateCoordinator;
        _weeklyDistributionService = pipeline.WeeklyDistributionService;
        _hourlyDistributionService = pipeline.HourlyDistributionService;
        _distributionPolarRenderingService = pipeline.DistributionPolarRenderingService;
        _strategyCutOverService = pipeline.StrategyCutOverService;
        _chartRenderingOrchestrator = pipeline.ChartRenderingOrchestrator;
        _weekdayTrendChartUpdateCoordinator = pipeline.WeekdayTrendChartUpdateCoordinator;
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

        _chartControllerRegistry = factoryResult.Registry;
        ValidateChartControllerRegistry(_chartControllerRegistry);
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
        if (_chartControllerRegistry == null)
            throw new InvalidOperationException($"Chart controller registry is not initialized for key '{key}'.");

        return _chartControllerRegistry.Get(key);
    }

    private CartesianChart GetWpfCartesianChart(string key)
    {
        if (ResolveController(key) is IWpfCartesianChartHost host)
            return host.Chart;

        throw new InvalidOperationException($"Chart controller '{key}' does not expose a Cartesian chart.");
    }

    private void ExecuteStartupSequence()
    {
        _startupCoordinator.Execute(
            new MainChartsViewStartupCoordinator.Actions(
                InitializeDateRange,
                InitializeDefaultUiState,
                InitializeSubtypeSelector,
                InitializeResolution,
                InitializeCharts,
                () => _viewModel.RequestChartUpdate(),
                SyncCmsToggleStates,
                () => _isInitializing = false,
                SyncInitialButtonStates));
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

    #region Infrastructure Helpers

    private string ResolveConnectionString()
    {
        return ConfigurationManager.AppSettings["HealthDB"] ?? "Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True";
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

    private void InitializeTooltipManager()
    {
        var parentWindow = Application.Current?.MainWindow ?? Window.GetWindow(this);
        if (parentWindow == null)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            throw new InvalidOperationException("Unable to resolve a parent window for tooltips.");
        }

        _tooltipManager = new ChartTooltipManager(parentWindow);
    }

    private void InitializeTooltips()
    {
        if (_tooltipManager == null)
            throw new InvalidOperationException("Tooltip manager is not initialized.");

        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Main), ChartControllerKeys.Main);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Normalized), ChartControllerKeys.Normalized);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.DiffRatio), ChartControllerKeys.DiffRatio);
        _tooltipManager.AttachChart(GetWpfCartesianChart(ChartControllerKeys.Transform), ChartControllerKeys.Transform);
    }

    private void WireViewModelEvents()
    {
        _viewModelEventBinder = new MainChartsViewEventBinder(
            _viewModel,
            new MainChartsViewEventBinder.Handlers(
                OnChartVisibilityChanged,
                OnErrorOccured,
                OnMetricTypesLoaded,
                OnSubtypesLoaded,
                OnDateRangeLoaded,
                OnDataLoaded,
                OnChartUpdateRequested,
                OnSelectionStateChanged));
        _viewModelEventBinder.Bind();
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
            UpdateChartTitlesFromSelections();
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
        _chartPresentationCoordinator.ApplyDefaultTitles(CreateChartPresentationActions());
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
        _resolutionResetCoordinator.ExecuteReset(
            selectedResolution,
            new MainChartsViewResolutionResetCoordinator.Actions(
                () => _isChangingResolution = true,
                ClearAllCharts,
                () => _viewModel.MetricState.SelectedMetricType = null,
                () => _viewModel.ChartState.LastContext = new ChartDataContext(),
                () => TablesCombo.Items.Clear(),
                () => _selectorManager.ClearDynamic(),
                () => SubtypeCombo.Items.Clear(),
                () => SubtypeCombo.IsEnabled = false,
                tableName => _viewModel.SetResolutionTableName(tableName),
                () => _viewModel.LoadMetricsCommand.Execute(null),
                UpdatePrimaryDataRequiredButtonStates,
                UpdateSecondaryDataRequiredButtonStates));
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
        {
            UpdateChartTitlesFromSelections();
            await RenderChartsFromLastContext();
            return;
        }

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

        UpdateChartTitlesFromSelections();
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

        UpdateChartTitlesFromSelections();

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
            var dataLoaded = await ChartPresentationSpine.LoadMetricDataIntoLastContextAsync(_viewModel);
            if (!dataLoaded)
            {
                ClearAllCharts();
                return;
            }

            _chartPresentationCoordinator.ClearHiddenCharts(_viewModel.ChartState, CreateChartPresentationActions());
            ChartPresentationSpine.PublishLastContextAndRequestChartUpdate(_viewModel);
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

        UpdateChartTitlesFromSelections();
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
        _evidenceExportCoordinator.ClearEvidence(CreateEvidenceExportActions());
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
        await _evidenceExportCoordinator.ExportAsync(
            _viewModel.ChartState,
            _viewModel.MetricState,
            DateTime.UtcNow,
            CreateEvidenceExportActions());
    }



    private void UpdateChartTitlesFromSelections()
    {
        _chartPresentationCoordinator.UpdateTitlesFromSelections(
            GetDistinctSelectedSeries(),
            _viewModel.ChartState.IsDiffRatioDifferenceMode,
            CreateChartPresentationActions());
    }

    private MainChartsViewChartPresentationCoordinator.Actions CreateChartPresentationActions()
    {
        return new MainChartsViewChartPresentationCoordinator.Actions(
            (leftName, rightName) =>
            {
                _viewModel.ChartState.LeftTitle = leftName ?? string.Empty;
                _viewModel.ChartState.RightTitle = rightName ?? string.Empty;
            },
            title => ResolveController(ChartControllerKeys.Main).SetTitle(title),
            title => ResolveController(ChartControllerKeys.Normalized).SetTitle(title),
            title => ResolveController(ChartControllerKeys.DiffRatio).SetTitle(title),
            label =>
            {
                if (_tooltipManager != null)
                    _tooltipManager.UpdateChartLabel(GetWpfCartesianChart(ChartControllerKeys.Main), label);
            },
            label =>
            {
                if (_tooltipManager != null)
                    _tooltipManager.UpdateChartLabel(GetWpfCartesianChart(ChartControllerKeys.DiffRatio), label);
            },
            ClearChart);
    }

    private MainChartsViewChartUpdateCoordinator.Actions CreateChartUpdateActions()
    {
        return new MainChartsViewChartUpdateCoordinator.Actions(
            SetChartVisibility,
            UpdateDistributionChartTypeVisibility,
            UpdateWeekdayTrendChartTypeVisibility,
            HandleTransformVisibilityOnlyToggle,
            RenderChartAsync,
            ClearChart);
    }

    private MainChartsViewEvidenceExportCoordinator.Actions CreateEvidenceExportActions()
    {
        return new MainChartsViewEvidenceExportCoordinator.Actions(
            (chartState, metricState, utcNow) => _evidenceExportService.ExportAsync(chartState, metricState, utcNow),
            () => _evidenceExportService.ClearEvidence(),
            (title, message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information),
            (title, message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning),
            (title, message) => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error));
    }

    #endregion
}

