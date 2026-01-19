using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Media;
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
using DataVisualiser.Core.Transforms.Evaluators;
using DataVisualiser.Core.Transforms.Expressions;
using DataVisualiser.Core.Transforms.Operations;
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
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _diffRatioSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _distributionSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly MetricState _metricState = new();
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _normalizedSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _transformSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly UiState _uiState = new();
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _weekdayTrendSubtypeCache = new(StringComparer.OrdinalIgnoreCase);
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

    private bool _isInitializing = true;
    private bool _isMetricTypeChangePending;
    private bool _isUpdatingDiffRatioSubtypeCombos;
    private bool _isUpdatingNormalizedSubtypeCombos;
    private bool _isTransformSelectionPendingLoad;
    private bool _isUpdatingDistributionSubtypeCombo;
    private bool _isUpdatingTransformSubtypeCombos;
    private bool _isUpdatingWeekdayTrendSubtypeCombo;

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

    #region Normalization mode UI handling

    private async void OnNormalizationModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        try
        {
            if (NormalizedChartController.NormZeroToOneRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
            else if (NormalizedChartController.NormPercentOfMaxRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
            else if (NormalizedChartController.NormRelativeToMaxRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);

            if (_viewModel.ChartState.IsNormalizedVisible && _viewModel.ChartState.LastContext?.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
            {
                using var busyScope = BeginUiBusyScope();
                var ctx = _viewModel.ChartState.LastContext;
                var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
                if (primaryData == null || secondaryData == null)
                    return;

                var normalizedStrategy = CreateNormalizedStrategy(normalizedContext, primaryData, secondaryData, normalizedContext.DisplayName1, normalizedContext.DisplayName2, normalizedContext.From, normalizedContext.To, _viewModel.ChartState.SelectedNormalizationMode);
                UpdateNormalizedPanelTitle(normalizedContext);
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(NormalizedChartController.Chart, normalizedStrategy, $"{normalizedContext.DisplayName1} ~ {normalizedContext.DisplayName2}", minHeight: 400, metricType: normalizedContext.PrimaryMetricType ?? normalizedContext.MetricType, primarySubtype: normalizedContext.PrimarySubtype, secondarySubtype: normalizedContext.SecondarySubtype, operationType: "~", isOperationChart: true, secondaryMetricType: normalizedContext.SecondaryMetricType, displayPrimaryMetricType: normalizedContext.DisplayPrimaryMetricType, displaySecondaryMetricType: normalizedContext.DisplaySecondaryMetricType, displayPrimarySubtype: normalizedContext.DisplayPrimarySubtype, displaySecondarySubtype: normalizedContext.DisplaySecondarySubtype);
            }
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
        }
    }

    #endregion

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
        UpdateNormalizedSubtypeOptions();
        UpdateDiffRatioSubtypeOptions();
        UpdateDistributionSubtypeOptions();
        UpdateWeekdayTrendSubtypeOptions();

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

    private static ComboBoxItem BuildSeriesComboItem(MetricSeriesSelection selection)
    {
        return new ComboBoxItem
        {
            Content = selection.DisplayName,
            Tag = selection
        };
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

    private static MetricSeriesSelection? GetSeriesSelectionFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is MetricSeriesSelection selection)
            return selection;

        return combo.SelectedItem as MetricSeriesSelection;
    }

    private static ComboBoxItem? FindSeriesComboItem(ComboBox combo, MetricSeriesSelection selection)
    {
        return combo.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
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
                ChartHelper.ClearChart(NormalizedChartController.Chart, _viewModel.ChartState.ChartTimestamps);
                _viewModel.SetNormalizedVisible(false);
            }

            if (_viewModel.ChartState.IsDiffRatioVisible)
            {
                ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
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

        _isTransformSelectionPendingLoad = false;
        UpdateNormalizedSubtypeOptions();
        UpdateDiffRatioSubtypeOptions();
        UpdateTransformSubtypeOptions();
        UpdateTransformComputeButtonState();
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
            HandleTransformPanelVisibilityOnlyToggle();
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

    private void HandleTransformPanelVisibilityOnlyToggle()
    {
        if (_viewModel.ChartState.IsTransformPanelVisible)
        {
            var transformCtx = _viewModel.ChartState.LastContext;
            if (transformCtx != null && ShouldRenderCharts(transformCtx))
                PopulateTransformGrids(transformCtx, false);

            UpdateTransformSubtypeOptions();
        }
    }

    private void UpdateAllChartVisibilities(ChartUpdateRequestedEventArgs e)
    {
        MainChartController.Panel.IsChartVisible = e.ShowMain;
        NormalizedChartController.Panel.IsChartVisible = e.ShowNormalized;
        DiffRatioChartController.Panel.IsChartVisible = e.ShowDiffRatio;
        DistributionChartController.Panel.IsChartVisible = e.ShowDistribution;
        UpdateDistributionChartTypeVisibility();

        WeekdayTrendChartController.Panel.IsChartVisible = e.ShowWeeklyTrend;
        UpdateWeekdayTrendChartTypeVisibility();

        TransformDataPanelController.Panel.IsChartVisible = _viewModel.ChartState.IsTransformPanelVisible;
    }

    private async Task HandleSingleChartUpdate(ChartUpdateRequestedEventArgs e)
    {
        // Transform panel visibility toggle - just update visibility, don't reload charts
        if (e.ToggledChartName == "Transform" && e.IsVisibilityOnlyToggle)
        {
            HandleTransformPanelVisibilityOnlyToggle();
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

    private void RenderWeekdayTrendChart(WeekdayTrendResult result)
    {
        _weekdayTrendChartUpdateCoordinator.UpdateChart(result, _viewModel.ChartState, WeekdayTrendChartController.Chart, WeekdayTrendChartController.PolarChart);
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
                await RenderNormalized(safeCtx, metricType, primarySubtype, secondarySubtype);

            if (_viewModel.ChartState.IsDiffRatioVisible && hasSecondaryData)
                await RenderDiffRatio(safeCtx, metricType, primarySubtype, secondarySubtype);
        }
        else
        {
            // Clear charts that require secondary data when no secondary data exists
            ChartHelper.ClearChart(NormalizedChartController.Chart, _viewModel.ChartState.ChartTimestamps);
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

    private void PopulateTransformGrids(ChartDataContext ctx, bool resetResults = true)
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

        if (resetResults)
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
        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

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

    //private void UpdateTransformSubtypeOptions()
    //{
    //    if (TransformDataPanelController.TransformPrimarySubtypeCombo == null || TransformDataPanelController.TransformSecondarySubtypeCombo == null)
    //        return;

    //    if (_isTransformSelectionPendingLoad)
    //        return;

    //    _isUpdatingTransformSubtypeCombos = true;
    //    try
    //    {
    //        TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Clear();
    //        TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Clear();

    //        var selectedSeries = _viewModel.MetricState.SelectedSeries;
    //        if (selectedSeries.Count == 0)
    //        {
    //            TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = false;
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
    //            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
    //            _viewModel.ChartState.SelectedTransformPrimarySeries = null;
    //            _viewModel.ChartState.SelectedTransformSecondarySeries = null;
    //            TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = null;
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
    //            return;
    //        }

    //        foreach (var selection in selectedSeries)
    //        {
    //            TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
    //        }

    //        TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = true;

    //        var primaryCurrent = _viewModel.ChartState.SelectedTransformPrimarySeries;
    //        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];
    //        var primaryItem = FindSeriesComboItem(TransformDataPanelController.TransformPrimarySubtypeCombo, primarySelection) ?? TransformDataPanelController.TransformPrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();
    //        TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = primaryItem;
    //        _viewModel.ChartState.SelectedTransformPrimarySeries = primarySelection;

    //        if (selectedSeries.Count > 1)
    //        {
    //            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = true;

    //            var secondaryCurrent = _viewModel.ChartState.SelectedTransformSecondarySeries;
    //            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];
    //            var secondaryItem = FindSeriesComboItem(TransformDataPanelController.TransformSecondarySubtypeCombo, secondarySelection) ?? TransformDataPanelController.TransformSecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = secondaryItem;
    //            _viewModel.ChartState.SelectedTransformSecondarySeries = secondarySelection;
    //        }
    //        else
    //        {
    //            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
    //            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
    //            _viewModel.ChartState.SelectedTransformSecondarySeries = null;
    //        }

    //        UpdateTransformComputeButtonState();
    //    }
    //    finally
    //    {
    //        _isUpdatingTransformSubtypeCombos = false;
    //    }
    //}


    private void UpdateTransformSubtypeOptions()
    {
        if (!CanUpdateTransformSubtypeOptions())
            return;

        _isUpdatingTransformSubtypeCombos = true;
        try
        {
            ClearTransformSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedSeries();
                return;
            }

            PopulateTransformSubtypeCombos(selectedSeries);
            UpdatePrimaryTransformSubtype(selectedSeries);
            UpdateSecondaryTransformSubtype(selectedSeries);

            UpdateTransformComputeButtonState();
        }
        finally
        {
            _isUpdatingTransformSubtypeCombos = false;
        }
    }

    private void UpdateNormalizedSubtypeOptions()
    {
        if (!CanUpdateNormalizedSubtypeOptions())
            return;

        _isUpdatingNormalizedSubtypeCombos = true;
        try
        {
            ClearNormalizedSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedNormalizedSeries();
                return;
            }

            PopulateNormalizedSubtypeCombos(selectedSeries);
            UpdatePrimaryNormalizedSubtype(selectedSeries);
            UpdateSecondaryNormalizedSubtype(selectedSeries);
        }
        finally
        {
            _isUpdatingNormalizedSubtypeCombos = false;
        }
    }

    private bool CanUpdateNormalizedSubtypeOptions()
    {
        return NormalizedChartController?.NormalizedPrimarySubtypeCombo != null && NormalizedChartController?.NormalizedSecondarySubtypeCombo != null;
    }

    private void ClearNormalizedSubtypeCombos()
    {
        NormalizedChartController.NormalizedPrimarySubtypeCombo.Items.Clear();
        NormalizedChartController.NormalizedSecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedNormalizedSeries()
    {
        NormalizedChartController.NormalizedPrimarySubtypeCombo.IsEnabled = false;
        NormalizedChartController.NormalizedSecondarySubtypeCombo.IsEnabled = false;
        NormalizedChartController.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedNormalizedPrimarySeries = null;
        _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;

        NormalizedChartController.NormalizedPrimarySubtypeCombo.SelectedItem = null;
        NormalizedChartController.NormalizedSecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateNormalizedSubtypeCombos(IReadOnlyList<dynamic> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            NormalizedChartController.NormalizedPrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
            NormalizedChartController.NormalizedSecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        NormalizedChartController.NormalizedPrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryNormalizedSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedNormalizedPrimarySeries;
        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(NormalizedChartController.NormalizedPrimarySubtypeCombo, primarySelection) ?? NormalizedChartController.NormalizedPrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        NormalizedChartController.NormalizedPrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedNormalizedPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryNormalizedSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            NormalizedChartController.NormalizedSecondarySubtypePanel.Visibility = Visibility.Visible;
            NormalizedChartController.NormalizedSecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedNormalizedSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(NormalizedChartController.NormalizedSecondarySubtypeCombo, secondarySelection) ?? NormalizedChartController.NormalizedSecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            NormalizedChartController.NormalizedSecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = secondarySelection;
        }
        else
        {
            NormalizedChartController.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            NormalizedChartController.NormalizedSecondarySubtypeCombo.IsEnabled = false;
            NormalizedChartController.NormalizedSecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;
        }
    }

    private void UpdateDiffRatioSubtypeOptions()
    {
        if (!CanUpdateDiffRatioSubtypeOptions())
            return;

        _isUpdatingDiffRatioSubtypeCombos = true;
        try
        {
            ClearDiffRatioSubtypeCombos();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                HandleNoSelectedDiffRatioSeries();
                return;
            }

            PopulateDiffRatioSubtypeCombos(selectedSeries);
            UpdatePrimaryDiffRatioSubtype(selectedSeries);
            UpdateSecondaryDiffRatioSubtype(selectedSeries);
        }
        finally
        {
            _isUpdatingDiffRatioSubtypeCombos = false;
        }
    }

    private bool CanUpdateDiffRatioSubtypeOptions()
    {
        return DiffRatioChartController?.PrimarySubtypeCombo != null && DiffRatioChartController?.SecondarySubtypeCombo != null;
    }

    private void ClearDiffRatioSubtypeCombos()
    {
        DiffRatioChartController.PrimarySubtypeCombo.Items.Clear();
        DiffRatioChartController.SecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedDiffRatioSeries()
    {
        DiffRatioChartController.PrimarySubtypeCombo.IsEnabled = false;
        DiffRatioChartController.SecondarySubtypeCombo.IsEnabled = false;
        DiffRatioChartController.SecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedDiffRatioPrimarySeries = null;
        _viewModel.ChartState.SelectedDiffRatioSecondarySeries = null;

        DiffRatioChartController.PrimarySubtypeCombo.SelectedItem = null;
        DiffRatioChartController.SecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateDiffRatioSubtypeCombos(IReadOnlyList<dynamic> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            DiffRatioChartController.PrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
            DiffRatioChartController.SecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        DiffRatioChartController.PrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryDiffRatioSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedDiffRatioPrimarySeries;
        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(DiffRatioChartController.PrimarySubtypeCombo, primarySelection) ?? DiffRatioChartController.PrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        DiffRatioChartController.PrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedDiffRatioPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryDiffRatioSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            DiffRatioChartController.SecondarySubtypePanel.Visibility = Visibility.Visible;
            DiffRatioChartController.SecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedDiffRatioSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(DiffRatioChartController.SecondarySubtypeCombo, secondarySelection) ?? DiffRatioChartController.SecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            DiffRatioChartController.SecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedDiffRatioSecondarySeries = secondarySelection;
        }
        else
        {
            DiffRatioChartController.SecondarySubtypePanel.Visibility = Visibility.Collapsed;
            DiffRatioChartController.SecondarySubtypeCombo.IsEnabled = false;
            DiffRatioChartController.SecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedDiffRatioSecondarySeries = null;
        }
    }


    private bool CanUpdateTransformSubtypeOptions()
    {
        if (TransformDataPanelController.TransformPrimarySubtypeCombo == null || TransformDataPanelController.TransformSecondarySubtypeCombo == null)
            return false;

        return !_isTransformSelectionPendingLoad;
    }

    private void ClearTransformSubtypeCombos()
    {
        TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Clear();
        TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedSeries()
    {
        TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = false;
        TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
        TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedTransformPrimarySeries = null;
        _viewModel.ChartState.SelectedTransformSecondarySeries = null;

        TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = null;
        TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateTransformSubtypeCombos(IReadOnlyList<dynamic> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            TransformDataPanelController.TransformPrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));

            TransformDataPanelController.TransformSecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        TransformDataPanelController.TransformPrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryTransformSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedTransformPrimarySeries;

        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(TransformDataPanelController.TransformPrimarySubtypeCombo, primarySelection) ?? TransformDataPanelController.TransformPrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        TransformDataPanelController.TransformPrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedTransformPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryTransformSubtype(IReadOnlyList<dynamic> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedTransformSecondarySeries;

            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(TransformDataPanelController.TransformSecondarySubtypeCombo, secondarySelection) ?? TransformDataPanelController.TransformSecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedTransformSecondarySeries = secondarySelection;
        }
        else
        {
            TransformDataPanelController.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            TransformDataPanelController.TransformSecondarySubtypeCombo.IsEnabled = false;
            TransformDataPanelController.TransformSecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedTransformSecondarySeries = null;
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
        ChartHelper.ClearChart(NormalizedChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(DistributionChartController.Chart);
        // NOTE: WeekdayTrend intentionally not cleared here to preserve current behavior (tied to secondary presence).
        // Cartesian, Polar, and Scatter modes are handled by RenderWeekdayTrendChart which checks visibility.
    }

    private async Task RenderNormalized(ChartDataContext ctx, string? metricType, string? primarySubtype, string? secondarySubtype)
    {
        if (_chartRenderingOrchestrator == null)
            return;

        var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateNormalizedPanelTitle(normalizedContext);
        await _chartRenderingOrchestrator.RenderNormalizedChartAsync(normalizedContext, NormalizedChartController.Chart, _viewModel.ChartState);
    }

    /// <summary>
    ///     Common method to render the distribution chart for the selected mode.
    /// </summary>
    private async Task RenderDistributionChart(ChartDataContext ctx, DistributionMode mode)
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
            return;

        var selectedSeries = ResolveSelectedDistributionSeries(ctx);
        var data = await ResolveDistributionDataAsync(ctx, selectedSeries);
        if (data == null || data.Count == 0)
            return;

        var displayName = ResolveDistributionDisplayName(ctx, selectedSeries);

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            await RenderDistributionPolarChart(ctx, mode, data, displayName);
            return;
        }

        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        var chart = DistributionChartController.Chart;

        if (_chartRenderingOrchestrator != null)
        {
            var distributionContext = new ChartDataContext
            {
                Data1 = data,
                DisplayName1 = displayName,
                MetricType = selectedSeries?.MetricType ?? ctx.MetricType,
                PrimaryMetricType = selectedSeries?.MetricType ?? ctx.PrimaryMetricType,
                PrimarySubtype = selectedSeries?.Subtype,
                DisplayPrimaryMetricType = selectedSeries?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplayPrimarySubtype = selectedSeries?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
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

        var selectedSeries = ResolveSelectedWeekdayTrendSeries(ctx);
        var data = await ResolveWeekdayTrendDataAsync(ctx, selectedSeries);
        if (data == null || data.Count == 0)
            return;

        var displayName = ResolveWeekdayTrendDisplayName(ctx, selectedSeries);
        var trendContext = new ChartDataContext
        {
            Data1 = data,
            DisplayName1 = displayName,
            MetricType = selectedSeries?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = selectedSeries?.MetricType ?? ctx.PrimaryMetricType,
            PrimarySubtype = selectedSeries?.Subtype,
            DisplayPrimaryMetricType = selectedSeries?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = selectedSeries?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
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
        _distributionPolarRenderingService.RenderPolarChart(rangeResult, definition, DistributionChartController.PolarChart);
        DistributionChartController.PolarChart.Tag = new DistributionPolarTooltipState(definition, rangeResult);
        DistributionChartController.PolarChart.UpdateLayout();
        DistributionChartController.PolarChart.InvalidateVisual();
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDistributionDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildDistributionCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_distributionSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _distributionSubtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedDistributionSeries(ChartDataContext ctx)
    {
        if (_viewModel.ChartState.SelectedDistributionSeries != null)
            return _viewModel.ChartState.SelectedDistributionSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildDistributionCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveWeekdayTrendDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildWeekdayTrendCacheKey(selectedSeries, ctx.From, ctx.To, tableName);

        if (_weekdayTrendSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _weekdayTrendSubtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedWeekdayTrendSeries(ChartDataContext ctx)
    {
        if (_viewModel.ChartState.SelectedWeekdayTrendSeries != null)
            return _viewModel.ChartState.SelectedWeekdayTrendSeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private static string ResolveWeekdayTrendDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildWeekdayTrendCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveNormalizedDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedNormalizedPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedNormalizedSecondarySeries(ctx);

        var primaryData = await ResolveNormalizedDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (secondarySelection != null)
            secondaryData = await ResolveNormalizedDataAsync(ctx, secondarySelection);

        var displayName1 = ResolveNormalizedDisplayName(ctx, primarySelection);
        var displayName2 = ResolveNormalizedDisplayName(ctx, secondarySelection);

        var normalizedContext = new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            MetricType = primarySelection?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
            SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
            From = ctx.From,
            To = ctx.To
        };

        return (primaryData, secondaryData, normalizedContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveNormalizedDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildNormalizedCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_normalizedSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _normalizedSubtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedPrimarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingNormalizedSubtypeCombos && NormalizedChartController.NormalizedPrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(NormalizedChartController.NormalizedPrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedNormalizedPrimarySeries != null)
            return _viewModel.ChartState.SelectedNormalizedPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedSecondarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingNormalizedSubtypeCombos && NormalizedChartController.NormalizedSecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(NormalizedChartController.NormalizedSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedNormalizedSecondarySeries != null)
            return _viewModel.ChartState.SelectedNormalizedSecondarySeries;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveNormalizedDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildNormalizedCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveDiffRatioDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedDiffRatioPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedDiffRatioSecondarySeries(ctx);

        var primaryData = await ResolveDiffRatioDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (secondarySelection != null)
            secondaryData = await ResolveDiffRatioDataAsync(ctx, secondarySelection);

        var displayName1 = ResolveDiffRatioDisplayName(ctx, primarySelection);
        var displayName2 = ResolveDiffRatioDisplayName(ctx, secondarySelection);

        var diffRatioContext = new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            MetricType = primarySelection?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
            SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
            From = ctx.From,
            To = ctx.To
        };

        return (primaryData, secondaryData, diffRatioContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveDiffRatioDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildDiffRatioCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_diffRatioSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _diffRatioSubtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioPrimarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingDiffRatioSubtypeCombos && DiffRatioChartController.PrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(DiffRatioChartController.PrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedDiffRatioPrimarySeries != null)
            return _viewModel.ChartState.SelectedDiffRatioPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedDiffRatioSecondarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingDiffRatioSubtypeCombos && DiffRatioChartController.SecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(DiffRatioChartController.SecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedDiffRatioSecondarySeries != null)
            return _viewModel.ChartState.SelectedDiffRatioSecondarySeries;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveDiffRatioDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildDiffRatioCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveTransformDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedTransformPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedTransformSecondarySeries(ctx);

        var primaryData = await ResolveTransformDataAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;

        if (secondarySelection != null)
            secondaryData = await ResolveTransformDataAsync(ctx, secondarySelection);

        var displayName1 = ResolveTransformDisplayName(ctx, primarySelection);
        var displayName2 = ResolveTransformDisplayName(ctx, secondarySelection);

        var transformContext = new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            MetricType = primarySelection?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
            SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
            From = ctx.From,
            To = ctx.To
        };

        return (primaryData, secondaryData, transformContext);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return null;

        if (selectedSeries == null)
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2 ?? ctx.Data1;

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return ctx.Data1;

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildTransformCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_transformSubtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _transformSubtypeCache[cacheKey] = data;
        return data;
    }

    private MetricSeriesSelection? ResolveSelectedTransformPrimarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && TransformDataPanelController.TransformPrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(TransformDataPanelController.TransformPrimarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformPrimarySeries != null)
            return _viewModel.ChartState.SelectedTransformPrimarySeries;

        var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
    }

    private MetricSeriesSelection? ResolveSelectedTransformSecondarySeries(ChartDataContext ctx)
    {
        if (!_isTransformSelectionPendingLoad && TransformDataPanelController.TransformSecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(TransformDataPanelController.TransformSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedTransformSecondarySeries != null)
            return _viewModel.ChartState.SelectedTransformSecondarySeries;

        var metricType = ctx.SecondaryMetricType ?? ctx.PrimaryMetricType ?? ctx.MetricType;
        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        return new MetricSeriesSelection(metricType, ctx.SecondarySubtype);
    }

    private static string ResolveTransformDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private static string BuildTransformCacheKey(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        return $"{selection.DisplayKey}|{from:O}|{to:O}|{tableName}";
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

        var (primaryData, secondaryData, diffRatioContext) = await ResolveDiffRatioDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateDiffRatioPanelTitle(diffRatioContext);
        await _chartRenderingOrchestrator.RenderDiffRatioChartAsync(diffRatioContext, DiffRatioChartController.Chart, _viewModel.ChartState);
    }

    private async Task RenderNormalizedFromSelectionAsync()
    {
        if (!_viewModel.ChartState.IsNormalizedVisible || _viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        await RenderNormalized(ctx, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype);
    }

    private async Task RenderDiffRatioFromSelectionAsync()
    {
        if (!_viewModel.ChartState.IsDiffRatioVisible || _viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        await RenderDiffRatio(ctx, ctx.MetricType, ctx.PrimarySubtype, ctx.SecondarySubtype);
    }

    private void UpdateNormalizedPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        NormalizedChartController.Panel.Title = $"{leftName} ~ {rightName}";
    }

    private void UpdateDiffRatioPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        var operationSymbol = _viewModel.ChartState.IsDiffRatioDifferenceMode ? "-" : "/";

        DiffRatioChartController.Panel.Title = $"{leftName} {operationSymbol} {rightName}";

        if (_tooltipManager != null)
        {
            var label = !string.IsNullOrEmpty(rightName) ? $"{leftName} {operationSymbol} {rightName}" : leftName;
            _tooltipManager.UpdateChartLabel(DiffRatioChartController.Chart, label);
        }
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

    private void OnWeekdayTrendAverageToggled(object? sender, WeekdayTrendAverageToggleEventArgs e)
    {
        _viewModel.SetWeeklyTrendAverageVisible(e.IsChecked);
    }

    private void OnWeekdayTrendAverageWindowChanged(object? sender, EventArgs e)
    {
        if (_isInitializing)
            return;

        if (WeekdayTrendChartController.AverageWindowCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is WeekdayTrendAverageWindow window)
            _viewModel.SetWeeklyTrendAverageWindow(window);
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
        ChartHelper.ClearChart(NormalizedChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(DistributionChartController.Chart);
        ClearDistributionPolarChart();
        ChartHelper.ClearChart(WeekdayTrendChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(WeekdayTrendChartController.PolarChart, _viewModel.ChartState.ChartTimestamps);
        ClearBarPieChart();
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

        // Wire up MainChartController events
        MainChartController.ToggleRequested += OnMainChartToggleRequested;
        MainChartController.DisplayModeChanged += OnMainChartDisplayModeChanged;
        WeekdayTrendChartController.ToggleRequested += OnWeekdayTrendToggleRequested;
        WeekdayTrendChartController.ChartTypeToggleRequested += OnWeekdayTrendChartTypeToggleRequested;
        WeekdayTrendChartController.DayToggled += OnWeekdayTrendDayToggled;
        WeekdayTrendChartController.AverageToggled += OnWeekdayTrendAverageToggled;
        WeekdayTrendChartController.AverageWindowChanged += OnWeekdayTrendAverageWindowChanged;
        WeekdayTrendChartController.SubtypeChanged += OnWeekdayTrendSubtypeChanged;
        DiffRatioChartController.ToggleRequested += OnDiffRatioToggleRequested;
        DiffRatioChartController.OperationToggleRequested += OnDiffRatioOperationToggleRequested;
        DiffRatioChartController.PrimarySubtypeChanged += OnDiffRatioPrimarySubtypeChanged;
        DiffRatioChartController.SecondarySubtypeChanged += OnDiffRatioSecondarySubtypeChanged;
        BarPieChartController.ToggleRequested += OnBarPieToggleRequested;
        BarPieChartController.DisplayModeChanged += OnBarPieDisplayModeChanged;
        BarPieChartController.BucketCountChanged += OnBarPieBucketCountChanged;
        NormalizedChartController.ToggleRequested += OnChartNormToggleRequested;
        NormalizedChartController.NormalizationModeChanged += OnNormalizationModeChanged;
        NormalizedChartController.PrimarySubtypeChanged += OnNormalizedPrimarySubtypeChanged;
        NormalizedChartController.SecondarySubtypeChanged += OnNormalizedSecondarySubtypeChanged;
        DistributionChartController.ToggleRequested += OnDistributionToggleRequested;
        DistributionChartController.ChartTypeToggleRequested += OnDistributionChartTypeToggleRequested;
        DistributionChartController.ModeChanged += OnDistributionModeChanged;
        DistributionChartController.SubtypeChanged += OnDistributionSubtypeChanged;
        DistributionChartController.DisplayModeChanged += OnDistributionDisplayModeChanged;
        DistributionChartController.IntervalCountChanged += OnDistributionIntervalCountChanged;
        TransformDataPanelController.ToggleRequested += OnTransformPanelToggleRequested;
        TransformDataPanelController.OperationChanged += OnTransformOperationChanged;
        TransformDataPanelController.PrimarySubtypeChanged += OnTransformPrimarySubtypeChanged;
        TransformDataPanelController.SecondarySubtypeChanged += OnTransformSecondarySubtypeChanged;
        TransformDataPanelController.ComputeRequested += OnTransformCompute;
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
        InitializeWeekdayTrendControls();
        InitializeChartBehavior();
        ClearChartsOnStartup();
        DisableAxisLabelsWhenNoData();
        SetDefaultChartTitles();
        UpdateDistributionChartTypeVisibility();
        InitializeDistributionPolarTooltip();
    }

    private void InitializeWeekdayTrendControls()
    {
        WeekdayTrendChartController.AverageWindowCombo.Items.Clear();
        AddWeekdayTrendAverageOption("Running Mean", WeekdayTrendAverageWindow.RunningMean);
        AddWeekdayTrendAverageOption("Weekly", WeekdayTrendAverageWindow.Weekly);
        AddWeekdayTrendAverageOption("Monthly", WeekdayTrendAverageWindow.Monthly);

        SelectWeekdayTrendAverageWindow(_viewModel.ChartState.WeekdayTrendAverageWindow);
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
        DistributionChartController.ModeCombo.Items.Clear();
        foreach (var definition in DistributionModeCatalog.All)
            DistributionChartController.ModeCombo.Items.Add(new ComboBoxItem
            {
                Content = definition.DisplayName,
                Tag = definition.Mode
            });

        DistributionChartController.IntervalCountCombo.Items.Clear();
        foreach (var intervalCount in DistributionModeCatalog.IntervalCounts)
            DistributionChartController.IntervalCountCombo.Items.Add(new ComboBoxItem
            {
                Content = intervalCount.ToString(),
                Tag = intervalCount
            });

        var initialMode = _viewModel.ChartState.SelectedDistributionMode;
        SelectDistributionMode(initialMode);
        ApplyDistributionModeDefinition(initialMode);
        ApplyDistributionSettingsToUi(initialMode);
    }

    private void AddWeekdayTrendAverageOption(string label, WeekdayTrendAverageWindow window)
    {
        WeekdayTrendChartController.AverageWindowCombo.Items.Add(new ComboBoxItem
        {
            Content = label,
            Tag = window
        });
    }

    private void SelectWeekdayTrendAverageWindow(WeekdayTrendAverageWindow window)
    {
        foreach (var item in WeekdayTrendChartController.AverageWindowCombo.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag is WeekdayTrendAverageWindow option && option == window)
            {
                WeekdayTrendChartController.AverageWindowCombo.SelectedItem = item;
                break;
            }
        }
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
        ChartHelper.ClearChart(NormalizedChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ChartHelper.ClearChart(DiffRatioChartController.Chart, _viewModel.ChartState.ChartTimestamps);
        ClearDistributionChart(DistributionChartController.Chart);
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
        DistributionChartController.PolarChart.Series = Array.Empty<ISeries>();
        DistributionChartController.PolarChart.AngleAxes = Array.Empty<PolarAxis>();
        DistributionChartController.PolarChart.RadiusAxes = Array.Empty<PolarAxis>();
        DistributionChartController.PolarChart.Tag = null;
        if (_distributionPolarTooltip != null)
            _distributionPolarTooltip.IsOpen = false;
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
            _distributionSubtypeCache.Clear();
            _weekdayTrendSubtypeCache.Clear();
            _normalizedSubtypeCache.Clear();
            _diffRatioSubtypeCache.Clear();
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

    private void OnChartNormToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleNorm();
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

    private async void OnNormalizedPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing || _isUpdatingNormalizedSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(NormalizedChartController.NormalizedPrimarySubtypeCombo);
        _viewModel.SetNormalizedPrimarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    private async void OnNormalizedSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing || _isUpdatingNormalizedSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(NormalizedChartController.NormalizedSecondarySubtypeCombo);
        _viewModel.SetNormalizedSecondarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    /// <summary>
    ///     Common handler for distribution chart toggles.
    /// </summary>
    private void HandleDistributionChartToggle()
    {
        _viewModel.ToggleDistribution();
    }

    private void OnDistributionToggleRequested(object? sender, EventArgs e)
    {
        HandleDistributionChartToggle();
    }

    private void OnWeekdayTrendToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleWeeklyTrend();
    }

    private void OnWeekdayTrendChartTypeToggleRequested(object? sender, EventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
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

    private async void OnDistributionChartTypeToggleRequested(object? sender, EventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
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

        var mode = _viewModel.ChartState.WeekdayTrendChartMode;
        if (mode == WeekdayTrendChartMode.Polar)
        {
            WeekdayTrendChartController.Chart.Visibility = Visibility.Collapsed;
            WeekdayTrendChartController.PolarChart.Visibility = Visibility.Visible;
            WeekdayTrendChartController.ChartTypeToggleButton.Content = "Scatter";
        }
        else
        {
            WeekdayTrendChartController.Chart.Visibility = Visibility.Visible;
            WeekdayTrendChartController.PolarChart.Visibility = Visibility.Collapsed;
            WeekdayTrendChartController.ChartTypeToggleButton.Content = mode == WeekdayTrendChartMode.Scatter ? "Cartesian" : "Polar";
        }
    }

    private void OnDiffRatioToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDiffRatio();
    }

    private async void OnDiffRatioPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing || _isUpdatingDiffRatioSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(DiffRatioChartController.PrimarySubtypeCombo);
        _viewModel.SetDiffRatioPrimarySeries(selection);

        await RenderDiffRatioFromSelectionAsync();
    }

    private async void OnDiffRatioSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing || _isUpdatingDiffRatioSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(DiffRatioChartController.SecondarySubtypeCombo);
        _viewModel.SetDiffRatioSecondarySeries(selection);

        await RenderDiffRatioFromSelectionAsync();
    }

    private async void OnDiffRatioOperationToggleRequested(object? sender, EventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
        _viewModel.ToggleDiffRatioOperation();
        UpdateDiffRatioOperationButton();

        // Re-render the chart with current data if visible
        await RenderDiffRatioFromSelectionAsync();
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

    private void OnTransformOperationChanged(object? sender, EventArgs e)
    {
        UpdateTransformComputeButtonState();
    }

    private void OnTransformPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(TransformDataPanelController.TransformPrimarySubtypeCombo);
        _viewModel.SetTransformPrimarySeries(selection);

        UpdateTransformComputeButtonState();
    }

    private void OnTransformSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingTransformSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(TransformDataPanelController.TransformSecondarySubtypeCombo);
        _viewModel.SetTransformSecondarySeries(selection);

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
        var hasSecondSubtype = ResolveSelectedTransformSecondarySeries(ctx) != null;
        var isUnary = operationTag == "Log" || operationTag == "Sqrt";
        var isBinary = operationTag == "Add" || operationTag == "Subtract" || operationTag == "Divide";

        // Enable compute button if operation matches data availability
        // For binary operations, require both secondary data AND a second subtype selected
        TransformDataPanelController.TransformComputeButton.IsEnabled = (isUnary && ctx.Data1 != null) || (isBinary && hasSecondary && hasSecondSubtype);
    }

    private async void OnTransformCompute(object? sender, EventArgs e)
    {
        if (_isTransformSelectionPendingLoad)
            return;

        if (_viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        if (!TryGetSelectedOperation(out var operationTag))
            return;

        using var busyScope = BeginUiBusyScope();
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
        var hasSecondary = HasSecondaryData(ctx) && ResolveSelectedTransformSecondarySeries(ctx) != null;

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

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(TransformDataPanelController.ChartTransformResult, strategy, label, null, 400, transformContext.PrimaryMetricType ?? transformContext.MetricType, transformContext.PrimarySubtype, transformContext.SecondarySubtype, operationType, isOperationChart, transformContext.SecondaryMetricType, displayPrimaryMetricType: transformContext.DisplayPrimaryMetricType, displaySecondaryMetricType: transformContext.DisplaySecondaryMetricType, displayPrimarySubtype: transformContext.DisplayPrimarySubtype, displaySecondarySubtype: transformContext.DisplaySecondarySubtype);
    }


    //private async Task ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation, ChartDataContext transformContext)
    //{
    //    // Use ALL data for chart computation (proper normalization)
    //    var allData1List = data1.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

    //    var allData2List = data2.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();

    //    if (allData1List.Count == 0 || allData2List.Count == 0)
    //        return;

    //    // Phase 4: Align data by timestamp (required for expression evaluator)
    //    var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);
    //    if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
    //        return;

    //    // Phase 4: Use new transform expression infrastructure
    //    var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);
    //    List<double> binaryComputedResults;
    //    List<IReadOnlyList<MetricData>> binaryMetricsList;

    //    if (expression == null)
    //    {
    //        // Fallback to legacy approach if operation not found in registry
    //        Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");
    //        var op = operation switch
    //        {
    //                "Add" => BinaryOperators.Sum,
    //                "Subtract" => BinaryOperators.Difference,
    //                "Divide" => BinaryOperators.Ratio,
    //                _ => (a, b) => a
    //        };

    //        var allValues1 = alignedData.Item1.Select(d => (double)d.Value!.Value).ToList();
    //        var allValues2 = alignedData.Item2.Select(d => (double)d.Value!.Value).ToList();
    //        binaryComputedResults = MathHelper.ApplyBinaryOperation(allValues1, allValues2, op);
    //        binaryMetricsList = new List<IReadOnlyList<MetricData>>
    //        {
    //                alignedData.Item1,
    //                alignedData.Item2
    //        };
    //        Debug.WriteLine($"[Transform] BINARY - Legacy computation completed: {binaryComputedResults.Count} results");
    //    }
    //    else
    //    {
    //        // Evaluate using new infrastructure
    //        Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}, expression built successfully");
    //        binaryMetricsList = new List<IReadOnlyList<MetricData>>
    //        {
    //                alignedData.Item1,
    //                alignedData.Item2
    //        };
    //        binaryComputedResults = TransformExpressionEvaluator.Evaluate(expression, binaryMetricsList);
    //        Debug.WriteLine($"[Transform] BINARY - Evaluated {binaryComputedResults.Count} results using TransformExpressionEvaluator");
    //    }

    //    // Continue with grid and chart rendering
    //    await RenderTransformResults(alignedData.Item1, binaryComputedResults, operation, binaryMetricsList, transformContext);
    //}
    private async Task ComputeBinaryTransform(IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string operation, ChartDataContext transformContext)
    {
        var allData1List = PrepareMetricData(data1);
        var allData2List = PrepareMetricData(data2);

        if (allData1List.Count == 0 || allData2List.Count == 0)
            return;

        // IMPORTANT: this is a VALUE TUPLE
        var alignedData = TransformExpressionEvaluator.AlignMetricsByTimestamp(allData1List, allData2List);

        if (alignedData.Item1.Count == 0 || alignedData.Item2.Count == 0)
            return;

        var computation = ComputeBinaryResults(alignedData, operation);

        await RenderTransformResults(alignedData.Item1, computation.Results, operation, computation.MetricsList, transformContext);
    }

    private static List<MetricData> PrepareMetricData(IEnumerable<MetricData> data)
    {
        return data.Where(d => d.Value.HasValue).OrderBy(d => d.NormalizedTimestamp).ToList();
    }

    private static (List<double> Results, List<IReadOnlyList<MetricData>> MetricsList) ComputeBinaryResults((List<MetricData> Item1, List<MetricData> Item2) alignedData, string operation)
    {
        var metricsList = new List<IReadOnlyList<MetricData>>
        {
                alignedData.Item1,
                alignedData.Item2
        };

        var expression = TransformExpressionBuilder.BuildFromOperation(operation, 0, 1);

        if (expression == null)
        {
            Debug.WriteLine($"[Transform] BINARY - Using LEGACY approach for operation: {operation}");

            var op = operation switch
            {
                "Add" => BinaryOperators.Sum,
                "Subtract" => BinaryOperators.Difference,
                "Divide" => BinaryOperators.Ratio,
                _ => (a, b) => a
            };

            var values1 = alignedData.Item1.Select(d => (double)d.Value!.Value).ToList();

            var values2 = alignedData.Item2.Select(d => (double)d.Value!.Value).ToList();

            var results = MathHelper.ApplyBinaryOperation(values1, values2, op);

            Debug.WriteLine($"[Transform] BINARY - Legacy computation completed: {results.Count} results");

            return (results, metricsList);
        }

        Debug.WriteLine($"[Transform] BINARY - Using NEW infrastructure for operation: {operation}");

        var computedResults = TransformExpressionEvaluator.Evaluate(expression, metricsList);

        Debug.WriteLine($"[Transform] BINARY - Evaluated {computedResults.Count} results using TransformExpressionEvaluator");

        return (computedResults, metricsList);
    }

    #endregion

    #region Chart Configuration and Helper Methods

    private void OnResetZoom(object sender, RoutedEventArgs e)
    {
        using var busyScope = BeginUiBusyScope();
        var mainChart = MainChartController.Chart;
        ChartHelper.ResetZoom(mainChart);
        ChartHelper.ResetZoom(NormalizedChartController.Chart);
        ChartHelper.ResetZoom(DiffRatioChartController.Chart);
        ResetDistributionChartZoom(DistributionChartController.Chart);
        ResetDistributionPolarZoom();
        ChartHelper.ResetZoom(TransformDataPanelController.ChartTransformResult);
        var weekdayChart = WeekdayTrendChartController.Chart;
        ChartHelper.ResetZoom(ref weekdayChart);
        var weekdayPolarChart = WeekdayTrendChartController.PolarChart;
        ChartHelper.ResetZoom(ref weekdayPolarChart);
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
    ///     Common method to reset zoom for distribution charts.
    /// </summary>
    private void ResetDistributionChartZoom(CartesianChart chart)
    {
        ChartHelper.ResetZoom(chart);
    }

    private void ResetDistributionPolarZoom()
    {
        DistributionChartController.PolarChart.FitToBounds = true;
    }

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

        var seriesData = await LoadBarPieSeriesDataAsync(selections, from, to);
        if (seriesData.Count == 0)
            return CreateEmptyBarPieModel();

        var bucketCount = ResolveBarPieBucketCount(from, to);
        var bucketPlan = BuildBarPieBucketPlan(from, to, bucketCount);

        var paletteKey = BarPieChartController;
        ColourPalette.Reset(paletteKey);

        var coloredSeries = seriesData.Select(data => new BarPieSeriesValues(
            data.Selection,
            BuildBucketTotals(data.Data, bucketPlan),
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

    private async Task<IReadOnlyList<BarPieSeriesData>> LoadBarPieSeriesDataAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var tasks = selections.Select(selection => LoadBarPieSeriesAsync(selection, from, to, tableName)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<BarPieSeriesData?> LoadBarPieSeriesAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new BarPieSeriesData(selection, dataList);
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

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var index = ResolveBucketIndex(point.NormalizedTimestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)point.Value.Value;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = sums[i];

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

    private sealed record BarPieSeriesData(MetricSeriesSelection Selection, IReadOnlyList<MetricData> Data);

    private sealed record BarPieSeriesValues(MetricSeriesSelection Selection, double?[] Totals, Color Color);

    private sealed record BarPieBucket(int Index, DateTime Start, DateTime End, string Label);

    private sealed record BarPieBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<BarPieBucket> Buckets);

    #endregion

    #region Distribution chart display mode UI handling

    private void SelectDistributionMode(DistributionMode mode)
    {
        foreach (var item in DistributionChartController.ModeCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is DistributionMode taggedMode && taggedMode == mode)
            {
                DistributionChartController.ModeCombo.SelectedItem = item;
                return;
            }
    }

    private void UpdateDistributionChartTypeVisibility()
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
        {
            DistributionChartController.Chart.Visibility = Visibility.Collapsed;
            DistributionChartController.PolarChart.Visibility = Visibility.Collapsed;
            if (_distributionPolarTooltip != null)
                _distributionPolarTooltip.IsOpen = false;
            return;
        }

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            DistributionChartController.Chart.Visibility = Visibility.Collapsed;
            DistributionChartController.PolarChart.Visibility = Visibility.Visible;
            DistributionChartController.ChartTypeToggleButton.Content = "Cartesian";
        }
        else
        {
            DistributionChartController.Chart.Visibility = Visibility.Visible;
            DistributionChartController.PolarChart.Visibility = Visibility.Collapsed;
            DistributionChartController.ChartTypeToggleButton.Content = "Polar";
            if (_distributionPolarTooltip != null)
                _distributionPolarTooltip.IsOpen = false;
        }
    }

    private void SelectDistributionIntervalCount(int intervalCount)
    {
        foreach (var item in DistributionChartController.IntervalCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == intervalCount)
            {
                DistributionChartController.IntervalCountCombo.SelectedItem = item;
                return;
            }
    }

    private void SelectBarPieBucketCount(int bucketCount)
    {
        foreach (var item in BarPieChartController.BucketCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == bucketCount)
            {
                BarPieChartController.BucketCountCombo.SelectedItem = item;
                return;
            }
    }

    private void ApplyDistributionModeDefinition(DistributionMode mode)
    {
        var definition = DistributionModeCatalog.Get(mode);
        DistributionChartController.Panel.Title = definition.Title;

        if (DistributionChartController.Chart.AxisX.Count == 0)
            DistributionChartController.Chart.AxisX.Add(new Axis());

        var axis = DistributionChartController.Chart.AxisX[0];
        axis.Title = definition.XAxisTitle;
        axis.Labels = definition.XAxisLabels.ToArray();
    }

    private void ApplyDistributionSettingsToUi(DistributionMode mode)
    {
        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        DistributionChartController.FrequencyShadingRadio.IsChecked = settings.UseFrequencyShading;
        DistributionChartController.SimpleRangeRadio.IsChecked = !settings.UseFrequencyShading;
        SelectDistributionIntervalCount(settings.IntervalCount);
    }

    private void UpdateDistributionSubtypeOptions()
    {
        if (DistributionChartController.SubtypeCombo == null)
            return;

        _isUpdatingDistributionSubtypeCombo = true;
        try
        {
            DistributionChartController.SubtypeCombo.Items.Clear();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                DistributionChartController.SubtypeCombo.IsEnabled = false;
                _viewModel.ChartState.SelectedDistributionSeries = null;
                DistributionChartController.SubtypeCombo.SelectedItem = null;
                return;
            }

            foreach (var selection in selectedSeries)
                DistributionChartController.SubtypeCombo.Items.Add(BuildSeriesComboItem(selection));

            DistributionChartController.SubtypeCombo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedDistributionSeries;
            var seriesSelection = current != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? current : selectedSeries[0];
            var distributionItem = FindSeriesComboItem(DistributionChartController.SubtypeCombo, seriesSelection) ?? DistributionChartController.SubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();
            DistributionChartController.SubtypeCombo.SelectedItem = distributionItem;

            if (_isInitializing)
                _viewModel.ChartState.SelectedDistributionSeries = seriesSelection;
            else
                _viewModel.SetDistributionSeries(seriesSelection);
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

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                combo.IsEnabled = false;
                _viewModel.ChartState.SelectedWeekdayTrendSeries = null;
                combo.SelectedItem = null;
                return;
            }

            foreach (var selection in selectedSeries)
                combo.Items.Add(BuildSeriesComboItem(selection));

            combo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedWeekdayTrendSeries;
            var seriesSelection = current != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? current : selectedSeries[0];
            var weekdayItem = FindSeriesComboItem(combo, seriesSelection) ?? combo.Items.OfType<ComboBoxItem>().FirstOrDefault();
            combo.SelectedItem = weekdayItem;

            if (_isInitializing)
                _viewModel.ChartState.SelectedWeekdayTrendSeries = seriesSelection;
            else
                _viewModel.SetWeekdayTrendSeries(seriesSelection);
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
                using var busyScope = BeginUiBusyScope();
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
                using var busyScope = BeginUiBusyScope();
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

        if (DistributionChartController.ModeCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is DistributionMode mode)
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

    private async void OnDistributionDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        var useFrequencyShading = DistributionChartController.FrequencyShadingRadio.IsChecked == true;
        await HandleDistributionDisplayModeChanged(GetSelectedDistributionMode(), useFrequencyShading);
    }

    private async void OnDistributionIntervalCountChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (DistributionChartController.IntervalCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var intervalCount))
            await HandleDistributionIntervalCountChanged(GetSelectedDistributionMode(), intervalCount);
    }

    private void OnDistributionModeChanged(object? sender, EventArgs e)
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

    private void OnDistributionSubtypeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingDistributionSubtypeCombo)
            return;

        var selection = GetSeriesSelectionFromCombo(DistributionChartController.SubtypeCombo);
        _viewModel.SetDistributionSeries(selection);
    }

    private void OnWeekdayTrendSubtypeChanged(object? sender, EventArgs e)
    {
        if (_viewModel == null)
            return;

        if (_isInitializing || _isUpdatingWeekdayTrendSubtypeCombo)
            return;

        var selection = GetSeriesSelectionFromCombo(WeekdayTrendChartController.SubtypeCombo);
        _viewModel.SetWeekdayTrendSeries(selection);
    }

    #endregion
}
