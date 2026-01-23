using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Helpers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using Axis = LiveCharts.Wpf.Axis;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Controls;

public sealed class DistributionChartControllerAdapter : IChartController, IDistributionChartControllerExtras, ICartesianChartSurface, IPolarChartSurface
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly DistributionChartController _controller;
    private readonly DistributionPolarRenderingService _distributionPolarRenderingService;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly Func<ToolTip?> _getPolarTooltip;
    private readonly IDistributionService _hourlyDistributionService;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private readonly IDistributionService _weeklyDistributionService;
    private bool _isUpdatingSubtypeCombo;

    public DistributionChartControllerAdapter(DistributionChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator, IDistributionService weeklyDistributionService, IDistributionService hourlyDistributionService, DistributionPolarRenderingService distributionPolarRenderingService, Func<ToolTip?> getPolarTooltip)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _weeklyDistributionService = weeklyDistributionService ?? throw new ArgumentNullException(nameof(weeklyDistributionService));
        _hourlyDistributionService = hourlyDistributionService ?? throw new ArgumentNullException(nameof(hourlyDistributionService));
        _distributionPolarRenderingService = distributionPolarRenderingService ?? throw new ArgumentNullException(nameof(distributionPolarRenderingService));
        _getPolarTooltip = getPolarTooltip ?? throw new ArgumentNullException(nameof(getPolarTooltip));
    }

    public CartesianChart Chart => _controller.Chart;

    public void ClearCache()
    {
        _selectionCache.Clear();
    }

    public string Key => "Distribution";
    public bool RequiresPrimaryData => true;
    public bool RequiresSecondaryData => false;
    public ChartPanelController Panel => _controller.Panel;
    public ButtonBase ToggleButton => _controller.ToggleButton;

    public void Initialize()
    {
    }

    public Task RenderAsync(ChartDataContext context)
    {
        return RenderDistributionChartAsync(context, _viewModel.ChartState.SelectedDistributionMode);
    }

    public void Clear(ChartState state)
    {
        ChartHelper.ClearChart(_controller.Chart, state.ChartTimestamps);
        ClearDistributionPolarChart();
    }

    public void ResetZoom()
    {
        ChartUiHelper.ResetZoom(_controller.Chart);
        _controller.PolarChart.FitToBounds = true;
    }

    public bool HasSeries(ChartState state)
    {
        return state.IsDistributionPolarMode ? ChartSeriesHelper.HasSeries(_controller.PolarChart.Series) : ChartSeriesHelper.HasSeries(_controller.Chart.Series);
    }

    public void UpdateSubtypeOptions()
    {
        if (_controller.SubtypeCombo == null)
            return;

        _isUpdatingSubtypeCombo = true;
        try
        {
            _controller.SubtypeCombo.Items.Clear();

            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                _controller.SubtypeCombo.IsEnabled = false;
                _viewModel.ChartState.SelectedDistributionSeries = null;
                _controller.SubtypeCombo.SelectedItem = null;
                return;
            }

            foreach (var selection in selectedSeries)
                _controller.SubtypeCombo.Items.Add(MetricSeriesSelectionCache.BuildSeriesComboItem(selection));

            _controller.SubtypeCombo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedDistributionSeries;
            var seriesSelection = current != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase)) ? current : selectedSeries[0];
            var distributionItem = MetricSeriesSelectionCache.FindSeriesComboItem(_controller.SubtypeCombo, seriesSelection) ?? _controller.SubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();
            _controller.SubtypeCombo.SelectedItem = distributionItem;

            if (_isInitializing())
                _viewModel.ChartState.SelectedDistributionSeries = seriesSelection;
            else
                _viewModel.SetDistributionSeries(seriesSelection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }
    }

    public void InitializeControls()
    {
        _controller.ModeCombo.Items.Clear();
        foreach (var definition in DistributionModeCatalog.All)
            _controller.ModeCombo.Items.Add(new ComboBoxItem
            {
                    Content = definition.DisplayName,
                    Tag = definition.Mode
            });

        _controller.IntervalCountCombo.Items.Clear();
        foreach (var intervalCount in DistributionModeCatalog.IntervalCounts)
            _controller.IntervalCountCombo.Items.Add(new ComboBoxItem
            {
                    Content = intervalCount.ToString(),
                    Tag = intervalCount
            });

        var initialMode = _viewModel.ChartState.SelectedDistributionMode;
        SelectDistributionMode(initialMode);
        ApplyModeDefinition(initialMode);
        ApplySettingsToUi(initialMode);
    }

    public void UpdateChartTypeVisibility()
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            var tooltip = _getPolarTooltip();
            if (tooltip != null)
                tooltip.IsOpen = false;
            return;
        }

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            _controller.Chart.Visibility = Visibility.Collapsed;
            _controller.PolarChart.Visibility = Visibility.Visible;
            _controller.ChartTypeToggleButton.Content = "Cartesian";
        }
        else
        {
            _controller.Chart.Visibility = Visibility.Visible;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
            _controller.ChartTypeToggleButton.Content = "Polar";
            var tooltip = _getPolarTooltip();
            if (tooltip != null)
                tooltip.IsOpen = false;
        }
    }

    public PolarChart PolarChart => _controller.PolarChart;

    public void ApplyModeDefinition(DistributionMode mode)
    {
        var definition = DistributionModeCatalog.Get(mode);
        _controller.Panel.Title = definition.Title;

        if (_controller.Chart.AxisX.Count == 0)
            _controller.Chart.AxisX.Add(new Axis());

        var axis = _controller.Chart.AxisX[0];
        axis.Title = definition.XAxisTitle;
        axis.Labels = definition.XAxisLabels.ToArray();
    }

    public void ApplySettingsToUi(DistributionMode mode, DistributionModeSettings settings)
    {
        _controller.FrequencyShadingRadio.IsChecked = settings.UseFrequencyShading;
        _controller.SimpleRangeRadio.IsChecked = !settings.UseFrequencyShading;
        SelectDistributionIntervalCount(settings.IntervalCount);
    }

    public async Task HandleDisplayModeChangedAsync(DistributionMode mode, bool useFrequencyShading)
    {
        if (_isInitializing())
            return;

        try
        {
            _viewModel.SetDistributionFrequencyShading(mode, useFrequencyShading);

            var isVisible = _viewModel.ChartState.IsDistributionVisible;
            if (isVisible && _viewModel.ChartState.LastContext?.Data1 != null)
            {
                using var _ = _beginUiBusyScope();
                var ctx = _viewModel.ChartState.LastContext;
                if (ctx != null)
                    await RenderDistributionChartAsync(ctx, mode);
            }
        }
        catch
        {
            // intentional: mode change shouldn't hard-fail the UI
        }
    }

    public async Task HandleIntervalCountChangedAsync(DistributionMode mode, int intervalCount)
    {
        if (_isInitializing())
            return;

        try
        {
            _viewModel.SetDistributionIntervalCount(mode, intervalCount);
            var isVisible = _viewModel.ChartState.IsDistributionVisible;

            if (isVisible && _viewModel.ChartState.LastContext?.Data1 != null)
            {
                using var _ = _beginUiBusyScope();
                var ctx = _viewModel.ChartState.LastContext;
                if (ctx != null)
                    await RenderDistributionChartAsync(ctx, mode);
            }
        }
        catch
        {
            // intentional: interval change shouldn't hard-fail the UI
        }
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleDistribution();
    }

    public async void OnChartTypeToggleRequested(object? sender, EventArgs e)
    {
        using var _ = _beginUiBusyScope();
        _viewModel.ToggleDistributionChartType();
        UpdateChartTypeVisibility();

        if (_viewModel.ChartState.IsDistributionVisible && _viewModel.ChartState.LastContext != null)
            await RenderDistributionChartAsync(_viewModel.ChartState.LastContext, _viewModel.ChartState.SelectedDistributionMode);
    }

    public async void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        var useFrequencyShading = _controller.FrequencyShadingRadio.IsChecked == true;
        await HandleDisplayModeChangedAsync(GetSelectedDistributionMode(), useFrequencyShading);
    }

    public async void OnIntervalCountChanged(object? sender, EventArgs e)
    {
        if (_controller.IntervalCountCombo.SelectedItem is ComboBoxItem selectedItem && TryGetIntervalCount(selectedItem.Tag, out var intervalCount))
            await HandleIntervalCountChangedAsync(GetSelectedDistributionMode(), intervalCount);
    }

    public void OnModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        var mode = GetSelectedDistributionMode();
        _viewModel.SetDistributionMode(mode);
        ApplyModeDefinition(mode);
        ApplySettingsToUi(mode);
    }

    public void OnSubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
        _viewModel.SetDistributionSeries(selection);
    }

    private void SelectDistributionMode(DistributionMode mode)
    {
        foreach (var item in _controller.ModeCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is DistributionMode taggedMode && taggedMode == mode)
            {
                _controller.ModeCombo.SelectedItem = item;
                return;
            }
    }

    private void SelectDistributionIntervalCount(int intervalCount)
    {
        foreach (var item in _controller.IntervalCountCombo.Items.OfType<ComboBoxItem>())
            if (item.Tag is int taggedInterval && taggedInterval == intervalCount)
            {
                _controller.IntervalCountCombo.SelectedItem = item;
                return;
            }
    }

    private void ApplySettingsToUi(DistributionMode mode)
    {
        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        ApplySettingsToUi(mode, settings);
    }

    private DistributionMode GetSelectedDistributionMode()
    {
        if (_controller.ModeCombo.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is DistributionMode mode)
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

    private async Task RenderDistributionChartAsync(ChartDataContext ctx, DistributionMode mode)
    {
        if (!_viewModel.ChartState.IsDistributionVisible)
            return;

        var selectedSeries = ResolveSelectedDistributionSeries(ctx);
        var (data, cmsSeries) = await ResolveDistributionDataAsync(ctx, selectedSeries);
        if (data == null || (data.Count == 0 && cmsSeries == null))
            return;

        var displayName = ResolveDistributionDisplayName(ctx, selectedSeries);

        if (_viewModel.ChartState.IsDistributionPolarMode)
        {
            await RenderDistributionPolarChartAsync(ctx, mode, data, displayName, cmsSeries);
            return;
        }

        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        var chart = _controller.Chart;
        var orchestrator = _getChartRenderingOrchestrator();

        if (orchestrator != null)
        {
            var distributionContext = new ChartDataContext
            {
                    PrimaryCms = cmsSeries,
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
            await orchestrator.RenderDistributionChartAsync(distributionContext, chart, _viewModel.ChartState, mode);
            return;
        }

        var service = GetDistributionService(mode);
        await service.UpdateDistributionChartAsync(chart, data, displayName, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount, cmsSeries);
    }

    private async Task RenderDistributionPolarChartAsync(ChartDataContext ctx, DistributionMode mode, IReadOnlyList<MetricData> data, string displayName, ICanonicalMetricSeries? cmsSeries)
    {
        var service = GetDistributionService(mode);
        var rangeResult = await service.ComputeSimpleRangeAsync(data, displayName, ctx.From, ctx.To, cmsSeries);
        if (rangeResult == null)
            return;

        var definition = DistributionModeCatalog.Get(mode);
        _distributionPolarRenderingService.RenderPolarChart(rangeResult, definition, _controller.PolarChart);
        _controller.PolarChart.Tag = new DistributionPolarTooltipState(definition, rangeResult);
        _controller.PolarChart.UpdateLayout();
        _controller.PolarChart.InvalidateVisual();
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveDistributionDataAsync(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (ctx.Data1 == null)
            return (null, null);

        if (selectedSeries == null)
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2 ?? ctx.Data1, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (_selectionCache.TryGetDataWithCms(cacheKey, out var cached, out var cachedCms))
            return (cached, cachedCms);

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selectedSeries, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _selectionCache.SetDataWithCms(cacheKey, data, primaryCms);
        return (data, primaryCms);
    }

    private MetricSeriesSelection? ResolveSelectedDistributionSeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionCache.ResolveSelection(!_isUpdatingSubtypeCombo, _controller.SubtypeCombo, _viewModel.ChartState.SelectedDistributionSeries,
                () =>
                {
                    var metricType = ctx.PrimaryMetricType ?? ctx.MetricType;
                    if (string.IsNullOrWhiteSpace(metricType))
                        return null;

                    return new MetricSeriesSelection(metricType, ctx.PrimarySubtype);
                });
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        if (selectedSeries == null)
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.DisplayName1;

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.DisplayName2;

        return selectedSeries.DisplayName;
    }

    private IDistributionService GetDistributionService(DistributionMode mode)
    {
        return mode switch
        {
                DistributionMode.Weekly => _weeklyDistributionService,
                DistributionMode.Hourly => _hourlyDistributionService,
                _ => _weeklyDistributionService
        };
    }

    private void ClearDistributionPolarChart()
    {
        _controller.PolarChart.Series = Array.Empty<ISeries>();
        _controller.PolarChart.AngleAxes = Array.Empty<PolarAxis>();
        _controller.PolarChart.RadiusAxes = Array.Empty<PolarAxis>();
        _controller.PolarChart.Tag = null;
        var tooltip = _getPolarTooltip();
        if (tooltip != null)
            tooltip.IsOpen = false;
    }
}
