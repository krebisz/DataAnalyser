using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Core.Transforms;
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
using ErrorEventArgs = DataVisualiser.Shared.ErrorEventArgs;

namespace DataVisualiser.UI;

public partial class MainChartsView : UserControl
{
    private readonly IChartControllerFactory _chartControllerFactory = new ChartControllerFactory();
    private readonly MainChartsViewChartPipelineFactory _chartPipelineFactory = new();
    private readonly MainChartsViewChartPresentationCoordinator _chartPresentationCoordinator = new();
    private readonly MainChartsViewChartUpdateCoordinator _chartUpdateCoordinatorHost = new();
    private readonly MainChartsViewDataLoadedCoordinator _dataLoadedCoordinator = new();
    private readonly ChartHostMetricSelectionCoordinator _metricSelectionCoordinator = new();
    private readonly ChartHostDateRangeCoordinator _dateRangeCoordinator = new();
    private readonly MainChartsViewEvidenceExportCoordinator _evidenceExportCoordinator = new();
    private readonly MainChartsViewLoadCoordinator _loadCoordinator = new();
    private readonly MainChartsViewControllerExtrasCoordinator _controllerExtrasCoordinator = new();
    private readonly MainChartsViewRegistryCoordinator _registryCoordinator = new();
    private readonly MainChartsViewSurfaceCoordinator _surfaceCoordinator = new();
    private readonly MainChartsViewResolutionResetCoordinator _resolutionResetCoordinator = new();
    private readonly MainChartsViewStateSyncCoordinator _stateSyncCoordinator = new();
    private readonly MainChartsViewCmsToggleCoordinator _cmsToggleCoordinator = new();
    private readonly MainChartsViewZoomResetCoordinator _zoomResetCoordinator = new();
    private readonly MainChartsViewSelectionCoordinator _selectionCoordinator = new();
    private readonly MainChartsViewToggleStateCoordinator _toggleStateCoordinator = new();
    private MainChartsSessionDiagnosticsRecorder _sessionDiagnosticsRecorder = null!;
    private MainChartsUiSurfaceDiagnosticsReader _uiSurfaceDiagnosticsReader = null!;
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
        _selectionCoordinator.UpdateSelectedSubtypes(
            GetSelectedSeriesFromUi(),
            CreateSelectionActions());
    }

    private List<MetricSeriesSelection> GetSelectedSeriesFromUi()
    {
        return _selectorManager.GetSelectedSeries().ToList();
    }

    private static int CountSelectedSubtypes(IEnumerable<MetricSeriesSelection> selections)
    {
        return selections.Count(selection => selection.QuerySubtype != null);
    }

    private bool HasLoadedData()
    {
        return ChartContextSelectionGuard.IsCompatibleWithCurrentSelection(
            _viewModel.ChartState.LastContext,
            _viewModel.MetricState.SelectedMetricType,
            _viewModel.MetricState.SelectedSeries,
            _viewModel.MetricState.FromDate,
            _viewModel.MetricState.ToDate,
            _viewModel.MetricState.ResolutionTableName);
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

    private void ShowTrackedMessage(string title, string message, MessageBoxImage image)
    {
        _sessionDiagnosticsRecorder.TrackHostMessage(title, message, image);
        MessageBox.Show(message, title, MessageBoxButton.OK, image);
    }

    private UiSurfaceDiagnosticsSnapshot CaptureEvidenceExportUiSurfaceDiagnostics()
    {
        return _uiSurfaceDiagnosticsReader.Capture(TablesCombo, FromDate, ToDate, TransformDataPanelController);
    }

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require secondary data.
    ///     These buttons are disabled when fewer than 2 subtypes are selected.
    ///     If charts are currently visible when secondary data becomes unavailable, they are cleared and hidden.
    /// </summary>
    private void UpdateSecondaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        _toggleStateCoordinator.UpdateSecondaryChartToggles(_viewModel.ChartState.LastContext, CreateToggleStateActions());
    }

    /// <summary>
    ///     Updates the enabled state of buttons for charts that require at least one subtype.
    ///     These buttons are disabled when no subtypes are selected.
    /// </summary>
    private void UpdatePrimaryDataRequiredButtonStates(int selectedSubtypeCount)
    {
        _toggleStateCoordinator.UpdatePrimaryChartToggles(_viewModel.ChartState.LastContext, selectedSubtypeCount, CreateToggleStateActions());
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
        _metricSelectionCoordinator.HandleMetricTypesLoaded(e.MetricTypes.ToList(), CreateMetricTypesLoadedActions());
        _isChangingResolution = false;
    }

    private void OnSubtypesLoaded(object? sender, SubtypesLoadedEventArgs e)
    {
        var subtypeListLocal = e.Subtypes.ToList();

        _subtypeList = subtypeListLocal;
        var selectedMetricType = GetSelectedMetricOption(TablesCombo);
        var followUp = _metricSelectionCoordinator.HandleSubtypesLoaded(
            new ChartHostMetricSelectionCoordinator.SubtypesLoadedInput(
                subtypeListLocal,
                selectedMetricType,
                _isMetricTypeChangePending,
                HasLoadedData(),
                ShouldRefreshDateRangeForCurrentSelection(),
                _isInitializing,
                _viewModel.MetricState.SelectedSeries.Count),
            CreateSubtypesLoadedActions());

        if (followUp == ChartHostMetricSelectionCoordinator.SubtypesFollowUp.LoadDateRange)
        {
            _ = LoadDateRangeForSelectedMetrics();
            return;
        }

        if (followUp == ChartHostMetricSelectionCoordinator.SubtypesFollowUp.ApplySelectionState)
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
            _stateSyncCoordinator.Apply(_viewModel, TablesCombo.Items.OfType<MetricNameOption>(), CreateStateSyncActions());
        }
        finally
        {
            _isApplyingSelectionSync = false;
        }
    }

    private void BuildDynamicSubtypeControls(IEnumerable<MetricNameOption> subtypes)
    {
        _selectorManager.ClearDynamic();
    }

    private void RefreshPrimarySubtypeCombo(IEnumerable<MetricNameOption> subtypes, bool preserveSelection, MetricNameOption? selectedMetricType)
    {
        _selectorManager.ReplacePrimaryItems(subtypes, selectedMetricType, preserveSelection);
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

            ShowTrackedMessage("DEBUG - LastContext contents", msg, MessageBoxImage.Information);
        }

        var selectedSubtypeCount = CountSelectedSubtypes(_viewModel.MetricState.SelectedSeries);
        await _dataLoadedCoordinator.HandleAsync(ctx, selectedSubtypeCount, CreateDataLoadedActions());
        _sessionDiagnosticsRecorder.RecordSessionMilestone("DataLoaded", "Success");
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
        _controllerExtrasCoordinator.InitializeDistributionControls(CreateControllerExtrasActions());
    }

    private void InitializeWeekdayTrendControls()
    {
        _controllerExtrasCoordinator.InitializeWeekdayTrendControls(CreateControllerExtrasActions());
    }

    private void UpdateDistributionChartTypeVisibility()
    {
        _controllerExtrasCoordinator.UpdateDistributionChartTypeVisibility(CreateControllerExtrasActions());
    }

    private void UpdateWeekdayTrendChartTypeVisibility()
    {
        _controllerExtrasCoordinator.UpdateWeekdayTrendChartTypeVisibility(CreateControllerExtrasActions());
    }

    private void CompleteTransformSelectionsPendingLoad()
    {
        _controllerExtrasCoordinator.CompleteTransformSelectionsPendingLoad(CreateControllerExtrasActions());
    }

    private void ResetTransformSelectionsPendingLoad()
    {
        _controllerExtrasCoordinator.ResetTransformSelectionsPendingLoad(CreateControllerExtrasActions());
    }

    private void HandleTransformVisibilityOnlyToggle(ChartDataContext? context)
    {
        _controllerExtrasCoordinator.HandleTransformVisibilityOnlyToggle(context, CreateControllerExtrasActions());
    }

    private void UpdateTransformSubtypeOptions()
    {
        _controllerExtrasCoordinator.UpdateTransformSubtypeOptions(CreateControllerExtrasActions());
    }

    private void UpdateTransformComputeButtonState()
    {
        _controllerExtrasCoordinator.UpdateTransformComputeButtonState(CreateControllerExtrasActions());
    }

    private void UpdateDiffRatioOperationButton()
    {
        _controllerExtrasCoordinator.UpdateDiffRatioOperationButton(CreateControllerExtrasActions());
    }

    private void SyncMainDisplayModeSelection()
    {
        _controllerExtrasCoordinator.SyncMainDisplayModeSelection(CreateControllerExtrasActions());
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

        ShowTrackedMessage("Error", e.Message, MessageBoxImage.Error);
        ClearAllCharts();
    }

    private void ClearAllCharts()
    {
        ClearRegisteredCharts();
        _viewModel.ChartState.LastContext = null;
        _viewModel.ChartState.LastLoadRuntime = null;
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
            () => ResolveController(ChartControllerKeys.BarPie) is IBarPieChartControllerExtras barPieController ? barPieController.GetDisplayMode() : "Bar",
            CaptureEvidenceExportUiSurfaceDiagnostics);

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
        _sessionDiagnosticsRecorder = new MainChartsSessionDiagnosticsRecorder(_viewModel);
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
        _cmsToggleCoordinator.SyncStates(CreateCmsToggleSyncActions());
    }

    private void UpdateCmsToggleEnablement(bool enabled)
    {
        CmsSingleCheckBox.IsEnabled = enabled;
        CmsCombinedCheckBox.IsEnabled = enabled;
        CmsMultiCheckBox.IsEnabled = enabled;
        CmsNormalizedCheckBox.IsEnabled = enabled;
        CmsWeeklyCheckBox.IsEnabled = enabled;
        CmsWeekdayTrendCheckBox.IsEnabled = enabled;
        CmsHourlyCheckBox.IsEnabled = enabled;
        CmsBarPieCheckBox.IsEnabled = enabled;
    }

    private async void OnCmsToggleChanged(object sender, RoutedEventArgs e)
    {
        await _cmsToggleCoordinator.HandleCmsToggleChangedAsync(
            _isInitializing,
            CmsEnableCheckBox.IsChecked == true,
            _viewModel.ChartState.IsBarPieVisible,
            _viewModel.ChartState.LastContext,
            CreateCmsToggleChangeActions());
    }

    private async void OnCmsStrategyToggled(object sender, RoutedEventArgs e)
    {
        await _cmsToggleCoordinator.HandleStrategyToggleChangedAsync(
            _isInitializing,
            new MainChartsViewCmsToggleCoordinator.StrategyToggleInput(
                CmsSingleCheckBox.IsChecked == true,
                CmsCombinedCheckBox.IsChecked == true,
                CmsMultiCheckBox.IsChecked == true,
                CmsNormalizedCheckBox.IsChecked == true,
                CmsWeeklyCheckBox.IsChecked == true,
                CmsWeekdayTrendCheckBox.IsChecked == true,
                CmsHourlyCheckBox.IsChecked == true,
                CmsBarPieCheckBox.IsChecked == true),
            _viewModel.ChartState.IsBarPieVisible,
            _viewModel.ChartState.LastContext,
            CreateCmsToggleChangeActions());
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
        _dateRangeCoordinator.ApplyDefaultRange(DateTime.UtcNow, CreateDateRangeActions());
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
        UpdateDistributionChartTypeVisibility();
        _surfaceCoordinator.InitializeSurfaces(CreateSurfaceActions());
    }

    private void InitializeBarPieControlsFromRegistry()
    {
        _controllerExtrasCoordinator.InitializeBarPieControls(CreateControllerExtrasActions());
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
        _viewModel.ChartState.LastLoadRuntime = null;

        SyncMainDisplayModeSelection();

        // Initialize weekday trend chart type visibility
        UpdateWeekdayTrendChartTypeVisibility();
    }

    private void InitializeSubtypeSelector()
    {
        _selectorManager = new SubtypeSelectorManager(TopControlMetricSubtypePanel, SubtypeCombo);
        _uiSurfaceDiagnosticsReader = new MainChartsUiSurfaceDiagnosticsReader(_selectorManager, _sessionDiagnosticsRecorder);

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

    private void ClearRegisteredCharts()
    {
        _registryCoordinator.ClearRegisteredCharts(_viewModel.ChartState, CreateRegistryActions());
    }

    private void ResetRegisteredChartsZoom()
    {
        var controllers = _registryCoordinator.ResolveControllers(CreateRegistryActions());

        _zoomResetCoordinator.ResetRegisteredCharts(
            controllers,
            new MainChartsViewZoomResetCoordinator.Actions(_sessionDiagnosticsRecorder.TrackHostMessage));
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
                () =>
                {
                    _viewModel.ChartState.LastContext = new ChartDataContext();
                    _viewModel.ChartState.LastLoadRuntime = null;
                },
                ResetDateRangeToDefault,
                () => TablesCombo.Items.Clear(),
                () => _selectorManager.ClearDynamic(),
                () => SubtypeCombo.Items.Clear(),
                () => SubtypeCombo.IsEnabled = false,
                tableName => _viewModel.SetResolutionTableName(tableName),
                () => _viewModel.LoadMetricsCommand.Execute(null),
                UpdatePrimaryDataRequiredButtonStates,
                UpdateSecondaryDataRequiredButtonStates));
    }

    private void ResetDateRangeToDefault()
    {
        _dateRangeCoordinator.ApplyDefaultRange(DateTime.UtcNow, CreateDateRangeActions());
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

        var selectedMetricType = GetSelectedMetricValue(TablesCombo);
        _metricSelectionCoordinator.HandleMetricTypeSelectionChanged(
            selectedMetricType,
            CreateMetricTypeSelectionChangedActions());
        UpdateChartTitlesFromSelections();
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
        await _selectionCoordinator.HandleSubtypeSelectionChangedAsync(
            HasLoadedData(),
            ShouldRefreshDateRangeForCurrentSelection(),
            CreateSelectionActions());
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
        var fromDate = FromDate.SelectedDate ?? DateTime.UtcNow.AddDays(-30);
        var toDate = ToDate.SelectedDate ?? DateTime.UtcNow;

        var isValid = _loadCoordinator.ValidateAndPrepareLoad(
            new MainChartsViewLoadCoordinator.LoadValidationInput(selectedMetricType, fromDate, toDate),
            CreateLoadValidationActions());

        return Task.FromResult(isValid);
    }

    private async Task LoadMetricData()
    {
        await _loadCoordinator.ExecuteLoadAsync(CreateLoadExecutionActions());
    }

    private void AddSubtypeComboBox(object sender, RoutedEventArgs e)
    {
        if (_subtypeList == null || !_subtypeList.Any())
            return;

        using (var comboSuppression = _selectorManager.SuppressSelectionChanged())
        {
            _selectorManager.EnsurePrimarySelection();
            var metricType = GetSelectedMetricOption(TablesCombo);
            _selectorManager.AddSubtypeCombo(_subtypeList, metricType);
        }

        using var selectionBatch = _viewModel.BeginSelectionStateBatch();
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
        _loadCoordinator.ClearSelection(
            defaultResolution,
            ResolutionCombo.SelectedItem?.ToString() == defaultResolution,
            CreateClearActions());
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
            GetSelectedSeriesFromUi(),
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

    private MainChartsViewDataLoadedCoordinator.Actions CreateDataLoadedActions()
    {
        return new MainChartsViewDataLoadedCoordinator.Actions(
            CompleteTransformSelectionsPendingLoad,
            UpdateSubtypeOptions,
            UpdateTransformSubtypeOptions,
            UpdateTransformComputeButtonState,
            UpdatePrimaryDataRequiredButtonStates,
            UpdateSecondaryDataRequiredButtonStates,
            RenderChartAsync,
            RenderChartsFromLastContext);
    }

    private MainChartsViewEvidenceExportCoordinator.Actions CreateEvidenceExportActions()
    {
        return new MainChartsViewEvidenceExportCoordinator.Actions(
            (chartState, metricState, utcNow) => _evidenceExportService.ExportAsync(chartState, metricState, utcNow),
            () => _evidenceExportService.ClearEvidence(),
            (title, message) => ShowTrackedMessage(title, message, MessageBoxImage.Information),
            (title, message) => ShowTrackedMessage(title, message, MessageBoxImage.Warning),
            (title, message) => ShowTrackedMessage(title, message, MessageBoxImage.Error));
    }

    private MainChartsViewSelectionCoordinator.Actions CreateSelectionActions()
    {
        return new MainChartsViewSelectionCoordinator.Actions(
            selections => _viewModel.SetSelectedSeries(selections),
            UpdateSubtypeOptions,
            UpdateTransformSubtypeOptions,
            UpdatePrimaryDataRequiredButtonStates,
            UpdateSecondaryDataRequiredButtonStates,
            UpdateChartTitlesFromSelections,
            RenderChartsFromLastContext,
            LoadDateRangeForSelectedMetrics);
    }

    private MainChartsViewLoadCoordinator.ValidationActions CreateLoadValidationActions()
    {
        return new MainChartsViewLoadCoordinator.ValidationActions(
            _viewModel.BeginSelectionStateBatch,
            _viewModel.SetSelectedMetricType,
            UpdateSelectedSubtypesInViewModel,
            (from, to) => _viewModel.SetDateRange(from, to),
            UpdateChartTitlesFromSelections,
            _viewModel.ValidateDataLoadRequirements,
            (title, message) => ShowTrackedMessage(title, message, MessageBoxImage.Warning));
    }

    private ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions CreateMetricTypesLoadedActions()
    {
        return new ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions(
            () => TablesCombo.Items.Clear(),
            option => TablesCombo.Items.Add(option),
            () => TablesCombo.Items.Count,
            value => _isApplyingSelectionSync = value,
            _viewModel.BeginSelectionStateBatch,
            index => TablesCombo.SelectedIndex = index,
            () => GetSelectedMetricValue(TablesCombo),
            _viewModel.SetSelectedMetricType,
            () => _viewModel.LoadSubtypesCommand.Execute(null),
            () => SubtypeCombo.Items.Clear(),
            value => SubtypeCombo.IsEnabled = value,
            () => _selectorManager.ClearDynamic());
    }

    private ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions CreateMetricTypeSelectionChangedActions()
    {
        return new ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions(
            value => _isMetricTypeChangePending = value,
            value => _isApplyingSelectionSync = value,
            _viewModel.BeginSelectionStateBatch,
            () =>
            {
                _subtypeList = null;
                ClearAllCharts();
            },
            _viewModel.SetSelectedMetricType,
            () => _selectorManager.ClearAllSubtypeControls(),
            UpdateSelectedSubtypesInViewModel,
            () => _viewModel.LoadSubtypesCommand.Execute(null));
    }

    private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions()
    {
        return new ChartHostMetricSelectionCoordinator.SubtypesLoadedActions(
            value => _isApplyingSelectionSync = value,
            _selectorManager.SuppressSelectionChanged,
            _viewModel.BeginSelectionStateBatch,
            (subtypes, preserveSelection, selectedMetricType) => RefreshPrimarySubtypeCombo(subtypes, preserveSelection, selectedMetricType),
            subtypes => BuildDynamicSubtypeControls(subtypes),
            UpdateSelectedSubtypesInViewModel,
            value => _isMetricTypeChangePending = value);
    }

    private MainChartsViewLoadCoordinator.LoadExecutionActions CreateLoadExecutionActions()
    {
        return new MainChartsViewLoadCoordinator.LoadExecutionActions(
            ClearChartCache,
            ResetTransformSelectionsPendingLoad,
            () => ChartPresentationSpine.LoadMetricDataIntoLastContextAsync(_viewModel),
            ClearAllCharts,
            () => _chartPresentationCoordinator.ClearHiddenCharts(_viewModel.ChartState, CreateChartPresentationActions()),
            () => ChartPresentationSpine.PublishLastContextAndRequestChartUpdate(_viewModel),
            (title, message) => ShowTrackedMessage(title, message, MessageBoxImage.Error));
    }

    private MainChartsViewLoadCoordinator.ClearActions CreateClearActions()
    {
        return new MainChartsViewLoadCoordinator.ClearActions(
            _sessionDiagnosticsRecorder.RecordSessionMilestone,
            () => _evidenceExportCoordinator.ClearEvidence(CreateEvidenceExportActions()),
            selections => _viewModel.SetSelectedSeries(selections),
            () =>
            {
                _viewModel.ChartState.LastContext = new ChartDataContext();
                _viewModel.ChartState.LastLoadRuntime = null;
            },
            UpdatePrimaryDataRequiredButtonStates,
            UpdateSecondaryDataRequiredButtonStates,
            ResetForResolutionChange,
            resolution => ResolutionCombo.SelectedItem = resolution);
    }

    private MainChartsViewStateSyncCoordinator.Actions CreateStateSyncActions()
    {
        return new MainChartsViewStateSyncCoordinator.Actions(
            targetResolution =>
            {
                if (ResolutionCombo.SelectedItem?.ToString() != targetResolution)
                    ResolutionCombo.SelectedItem = targetResolution;
            },
            fromDate =>
            {
                if (FromDate.SelectedDate != fromDate)
                    FromDate.SelectedDate = fromDate;
            },
            toDate =>
            {
                if (ToDate.SelectedDate != toDate)
                    ToDate.SelectedDate = toDate;
            },
            metricType =>
            {
                if (metricType == null)
                    return;

                var existing = TablesCombo.SelectedItem as MetricNameOption;
                if (existing != null && string.Equals(existing.Value, metricType.Value, StringComparison.OrdinalIgnoreCase))
                    return;

                TablesCombo.SelectedItem = metricType;
            },
            (selections, selectedMetricType) =>
            {
                if (_subtypeList == null || _subtypeList.Count == 0)
                    return;

                MainChartsViewStateSyncCoordinator.ApplySubtypeSelections(
                    _selectorManager,
                    SubtypeCombo,
                    _subtypeList,
                    selections,
                    selectedMetricType);
            },
            bucketCount =>
            {
                if (ResolveController(ChartControllerKeys.BarPie) is IBarPieChartControllerExtras controller)
                    controller.SelectBucketCount(bucketCount);
            });
    }

    private MainChartsViewToggleStateCoordinator.Actions CreateToggleStateActions()
    {
        return new MainChartsViewToggleStateCoordinator.Actions(
            ResolveController,
            ClearChart,
            () =>
            {
                if (_viewModel.ChartState.IsNormalizedVisible)
                    _viewModel.SetNormalizedVisible(false);
            },
            () =>
            {
                if (_viewModel.ChartState.IsDiffRatioVisible)
                    _viewModel.SetDiffRatioVisible(false);
            });
    }

    private MainChartsViewCmsToggleCoordinator.SyncActions CreateCmsToggleSyncActions()
    {
        return new MainChartsViewCmsToggleCoordinator.SyncActions(
            value => CmsEnableCheckBox.IsChecked = value,
            value => CmsSingleCheckBox.IsChecked = value,
            value => CmsCombinedCheckBox.IsChecked = value,
            value => CmsMultiCheckBox.IsChecked = value,
            value => CmsNormalizedCheckBox.IsChecked = value,
            value => CmsWeeklyCheckBox.IsChecked = value,
            value => CmsWeekdayTrendCheckBox.IsChecked = value,
            value => CmsHourlyCheckBox.IsChecked = value,
            value => CmsBarPieCheckBox.IsChecked = value,
            value => CmsSingleCheckBox.IsEnabled = value,
            value => CmsCombinedCheckBox.IsEnabled = value,
            value => CmsMultiCheckBox.IsEnabled = value,
            value => CmsNormalizedCheckBox.IsEnabled = value,
            value => CmsWeeklyCheckBox.IsEnabled = value,
            value => CmsWeekdayTrendCheckBox.IsEnabled = value,
            value => CmsHourlyCheckBox.IsEnabled = value,
            value => CmsBarPieCheckBox.IsEnabled = value);
    }

    private MainChartsViewCmsToggleCoordinator.ChangeActions CreateCmsToggleChangeActions()
    {
        return new MainChartsViewCmsToggleCoordinator.ChangeActions(
            UpdateCmsToggleEnablement,
            RenderChartAsync);
    }

    private ChartHostDateRangeCoordinator.Actions CreateDateRangeActions()
    {
        return new ChartHostDateRangeCoordinator.Actions(
            _viewModel.SetDateRange,
            value => FromDate.SelectedDate = value,
            value => ToDate.SelectedDate = value,
            () => _viewModel.MetricState.FromDate,
            () => _viewModel.MetricState.ToDate);
    }

    private MainChartsViewControllerExtrasCoordinator.Actions CreateControllerExtrasActions()
    {
        return new MainChartsViewControllerExtrasCoordinator.Actions(ResolveController);
    }

    private MainChartsViewRegistryCoordinator.Actions CreateRegistryActions()
    {
        return new MainChartsViewRegistryCoordinator.Actions(
            () => _chartControllerRegistry?.All().ToList(),
            ResolveController);
    }

    private MainChartsViewSurfaceCoordinator.Actions CreateSurfaceActions()
    {
        return new MainChartsViewSurfaceCoordinator.Actions(
            () => GetWpfCartesianChart(ChartControllerKeys.Main),
            () => GetWpfCartesianChart(ChartControllerKeys.Normalized),
            () => GetWpfCartesianChart(ChartControllerKeys.DiffRatio),
            () => GetWpfCartesianChart(ChartControllerKeys.Distribution),
            () =>
            {
                ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Main));
                ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Normalized));
                ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.DiffRatio));
            },
            () => ChartUiHelper.InitializeChartBehavior(GetWpfCartesianChart(ChartControllerKeys.Distribution)),
            ClearRegisteredCharts,
            () =>
            {
                _chartPresentationCoordinator.ApplyDefaultTitles(CreateChartPresentationActions());
                UpdateDiffRatioOperationButton();
            },
            tooltip => _distributionPolarTooltip = tooltip);
    }

    #endregion
}

