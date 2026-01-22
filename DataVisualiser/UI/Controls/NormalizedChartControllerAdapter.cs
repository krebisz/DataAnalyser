using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Controls;

public sealed class NormalizedChartControllerAdapter : IChartController, IChartSubtypeOptionsController, IChartCacheController, IChartSeriesAvailability, ICartesianChartSurface
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
    private readonly NormalizedChartController _controller;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _subtypeCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ICanonicalMetricSeries?> _subtypeCmsCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombos;

    public NormalizedChartControllerAdapter(NormalizedChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator, ChartUpdateCoordinator chartUpdateCoordinator, Func<IStrategyCutOverService?> getStrategyCutOverService)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
    }

    public CartesianChart Chart => _controller.Chart;

    public void ClearCache()
    {
        _subtypeCache.Clear();
        _subtypeCmsCache.Clear();
    }

    public string Key => "Norm";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => true;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        return RenderNormalizedAsync(context);
    }

    public void Clear(ChartState state)
    {
        ChartHelper.ClearChart(_controller.Chart, state.ChartTimestamps);
    }

    public void ResetZoom()
    {
        ChartHelper.ResetZoom(_controller.Chart);
    }

    public bool HasSeries(ChartState state)
    {
        return HasSeriesInternal(_controller.Chart.Series);
    }

    public void UpdateSubtypeOptions()
    {
        if (!CanUpdateNormalizedSubtypeOptions())
            return;

        _isUpdatingSubtypeCombos = true;
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
            _isUpdatingSubtypeCombos = false;
        }
    }

    private static bool HasSeriesInternal(IEnumerable? series)
    {
        if (series == null)
            return false;

        return series.Cast<object>().Any();
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleNorm();
    }

    public async void OnNormalizationModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        try
        {
            if (_controller.NormZeroToOneRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.ZeroToOne);
            else if (_controller.NormPercentOfMaxRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.PercentageOfMax);
            else if (_controller.NormRelativeToMaxRadio.IsChecked == true)
                _viewModel.SetNormalizationMode(NormalizationMode.RelativeToMax);

            if (_viewModel.ChartState.IsNormalizedVisible && _viewModel.ChartState.LastContext?.Data1 != null && _viewModel.ChartState.LastContext.Data2 != null)
            {
                using var _ = _beginUiBusyScope();
                var ctx = _viewModel.ChartState.LastContext;
                if (ctx == null)
                    return;

                var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
                if (primaryData == null || secondaryData == null)
                    return;

                var normalizedStrategy = CreateNormalizedStrategy(normalizedContext, primaryData, secondaryData, normalizedContext.DisplayName1, normalizedContext.DisplayName2, normalizedContext.From, normalizedContext.To, _viewModel.ChartState.SelectedNormalizationMode);
                UpdateNormalizedPanelTitle(normalizedContext);
                await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(_controller.Chart, normalizedStrategy, $"{normalizedContext.DisplayName1} ~ {normalizedContext.DisplayName2}", minHeight: 400, metricType: normalizedContext.PrimaryMetricType ?? normalizedContext.MetricType, primarySubtype: normalizedContext.PrimarySubtype, secondarySubtype: normalizedContext.SecondarySubtype, operationType: "~", isOperationChart: true, secondaryMetricType: normalizedContext.SecondaryMetricType, displayPrimaryMetricType: normalizedContext.DisplayPrimaryMetricType, displaySecondaryMetricType: normalizedContext.DisplaySecondaryMetricType, displayPrimarySubtype: normalizedContext.DisplayPrimarySubtype, displaySecondarySubtype: normalizedContext.DisplaySecondarySubtype);
            }
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
        }
    }

    public async void OnPrimarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.NormalizedPrimarySubtypeCombo);
        _viewModel.SetNormalizedPrimarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    public async void OnSecondarySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombos)
            return;

        var selection = GetSeriesSelectionFromCombo(_controller.NormalizedSecondarySubtypeCombo);
        _viewModel.SetNormalizedSecondarySeries(selection);

        await RenderNormalizedFromSelectionAsync();
    }

    private async Task RenderNormalizedFromSelectionAsync()
    {
        if (!_viewModel.ChartState.IsNormalizedVisible || _viewModel.ChartState.LastContext == null)
            return;

        var ctx = _viewModel.ChartState.LastContext;
        await RenderNormalizedAsync(ctx);
    }

    private async Task RenderNormalizedAsync(ChartDataContext ctx)
    {
        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator == null)
            return;

        var (primaryData, secondaryData, normalizedContext) = await ResolveNormalizedDataAsync(ctx);
        if (primaryData == null || secondaryData == null)
            return;

        UpdateNormalizedPanelTitle(normalizedContext);
        await orchestrator.RenderNormalizedChartAsync(normalizedContext, _controller.Chart, _viewModel.ChartState);
    }

    private async Task<(IReadOnlyList<MetricData>? Primary, IReadOnlyList<MetricData>? Secondary, ChartDataContext Context)> ResolveNormalizedDataAsync(ChartDataContext ctx)
    {
        var primarySelection = ResolveSelectedNormalizedPrimarySeries(ctx);
        var secondarySelection = ResolveSelectedNormalizedSecondarySeries(ctx);

        var (primaryData, primaryCms) = await ResolveNormalizedSeriesAsync(ctx, primarySelection);
        IReadOnlyList<MetricData>? secondaryData = null;
        ICanonicalMetricSeries? secondaryCms = null;
        if (secondarySelection != null)
        {
            var resolvedSecondary = await ResolveNormalizedSeriesAsync(ctx, secondarySelection);
            secondaryData = resolvedSecondary.Data;
            secondaryCms = resolvedSecondary.Cms;
        }

        var displayName1 = ResolveNormalizedDisplayName(ctx, primarySelection);
        var displayName2 = ResolveNormalizedDisplayName(ctx, secondarySelection);

        var normalizedContext = new ChartDataContext
        {
                Data1 = primaryData,
                Data2 = secondaryData,
                PrimaryCms = primaryCms,
                SecondaryCms = secondaryCms,
                DisplayName1 = displayName1,
                DisplayName2 = displayName2,
                MetricType = primarySelection?.MetricType ?? ctx.MetricType,
                PrimaryMetricType = primarySelection?.MetricType ?? ctx.PrimaryMetricType,
                PrimarySubtype = primarySelection?.Subtype,
                SecondaryMetricType = secondarySelection?.MetricType ?? ctx.SecondaryMetricType,
                SecondarySubtype = secondarySelection?.Subtype,
                DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
                DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
                DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? ctx.DisplaySecondaryMetricType,
                DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? ctx.DisplaySecondarySubtype,
                From = ctx.From,
                To = ctx.To
        };

        return (primaryData, secondaryData, normalizedContext);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveNormalizedSeriesAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return (null, null);

        if (ctx.Data1 != null && IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (ctx.Data2 != null && IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = BuildNormalizedCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_subtypeCache.TryGetValue(cacheKey, out var cached) && _subtypeCmsCache.TryGetValue(cacheKey, out var cachedCms))
            return (cached, cachedCms);

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selectedSeries, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _subtypeCache[cacheKey] = data;
        _subtypeCmsCache[cacheKey] = primaryCms;
        return (data, primaryCms);
    }

    private MetricSeriesSelection? ResolveSelectedNormalizedPrimarySeries(ChartDataContext ctx)
    {
        if (!_isUpdatingSubtypeCombos && _controller.NormalizedPrimarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.NormalizedPrimarySubtypeCombo);
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
        if (!_isUpdatingSubtypeCombos && _controller.NormalizedSecondarySubtypeCombo != null)
        {
            var selection = GetSeriesSelectionFromCombo(_controller.NormalizedSecondarySubtypeCombo);
            if (selection != null)
                return selection;
        }

        if (_viewModel.ChartState.SelectedNormalizedSecondarySeries != null)
            return _viewModel.ChartState.SelectedNormalizedSecondarySeries;

        var metricType = ctx.SecondaryMetricType;
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

    private void UpdateNormalizedPanelTitle(ChartDataContext ctx)
    {
        var leftName = ctx.DisplayName1 ?? string.Empty;
        var rightName = ctx.DisplayName2 ?? string.Empty;
        _controller.Panel.Title = $"{leftName} ~ {rightName}";
    }

    private IChartComputationStrategy CreateNormalizedStrategy(ChartDataContext ctx, IEnumerable<MetricData> data1, IEnumerable<MetricData> data2, string label1, string label2, DateTime from, DateTime to, NormalizationMode normalizationMode)
    {
        var strategyCutOverService = _getStrategyCutOverService();
        if (strategyCutOverService == null)
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

        return strategyCutOverService.CreateStrategy(StrategyType.Normalized, ctx, parameters);
    }

    private bool CanUpdateNormalizedSubtypeOptions()
    {
        return _controller.NormalizedPrimarySubtypeCombo != null && _controller.NormalizedSecondarySubtypeCombo != null;
    }

    private void ClearNormalizedSubtypeCombos()
    {
        _controller.NormalizedPrimarySubtypeCombo.Items.Clear();
        _controller.NormalizedSecondarySubtypeCombo.Items.Clear();
    }

    private void HandleNoSelectedNormalizedSeries()
    {
        _controller.NormalizedPrimarySubtypeCombo.IsEnabled = false;
        _controller.NormalizedSecondarySubtypeCombo.IsEnabled = false;
        _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        _viewModel.ChartState.SelectedNormalizedPrimarySeries = null;
        _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;

        _controller.NormalizedPrimarySubtypeCombo.SelectedItem = null;
        _controller.NormalizedSecondarySubtypeCombo.SelectedItem = null;
    }

    private void PopulateNormalizedSubtypeCombos(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        foreach (var selection in selectedSeries)
        {
            _controller.NormalizedPrimarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
            _controller.NormalizedSecondarySubtypeCombo.Items.Add(BuildSeriesComboItem(selection));
        }

        _controller.NormalizedPrimarySubtypeCombo.IsEnabled = true;
    }

    private void UpdatePrimaryNormalizedSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        var primaryCurrent = _viewModel.ChartState.SelectedNormalizedPrimarySeries;
        var primarySelection = primaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, primaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? primaryCurrent : selectedSeries[0];

        var primaryItem = FindSeriesComboItem(_controller.NormalizedPrimarySubtypeCombo, primarySelection) ?? _controller.NormalizedPrimarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

        _controller.NormalizedPrimarySubtypeCombo.SelectedItem = primaryItem;
        _viewModel.ChartState.SelectedNormalizedPrimarySeries = primarySelection;
    }

    private void UpdateSecondaryNormalizedSubtype(IReadOnlyList<MetricSeriesSelection> selectedSeries)
    {
        if (selectedSeries.Count > 1)
        {
            _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Visible;
            _controller.NormalizedSecondarySubtypeCombo.IsEnabled = true;

            var secondaryCurrent = _viewModel.ChartState.SelectedNormalizedSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? secondaryCurrent : selectedSeries[1];

            var secondaryItem = FindSeriesComboItem(_controller.NormalizedSecondarySubtypeCombo, secondarySelection) ?? _controller.NormalizedSecondarySubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();

            _controller.NormalizedSecondarySubtypeCombo.SelectedItem = secondaryItem;
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = secondarySelection;
        }
        else
        {
            _controller.NormalizedSecondarySubtypePanel.Visibility = Visibility.Collapsed;
            _controller.NormalizedSecondarySubtypeCombo.IsEnabled = false;
            _controller.NormalizedSecondarySubtypeCombo.SelectedItem = null;
            _viewModel.ChartState.SelectedNormalizedSecondarySeries = null;
        }
    }

    private static ComboBoxItem BuildSeriesComboItem(MetricSeriesSelection selection)
    {
        return new ComboBoxItem
        {
                Content = selection.DisplayName,
                Tag = selection
        };
    }

    private static ComboBoxItem? FindSeriesComboItem(ComboBox combo, MetricSeriesSelection selection)
    {
        return combo.Items.OfType<ComboBoxItem>().FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
    }

    private static MetricSeriesSelection? GetSeriesSelectionFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is MetricSeriesSelection selection)
            return selection;

        return combo.SelectedItem as MetricSeriesSelection;
    }

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}