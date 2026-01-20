using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Controls;

public sealed class DistributionChartControllerAdapter : IChartController
{
    private readonly DistributionChartController _controller;
    private readonly MainWindowViewModel _viewModel;
    private readonly Func<bool> _isInitializing;
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly WeeklyDistributionService _weeklyDistributionService;
    private readonly HourlyDistributionService _hourlyDistributionService;
    private readonly DistributionPolarRenderingService _distributionPolarRenderingService;
    private readonly Func<ToolTip?> _getPolarTooltip;
    private bool _isUpdatingSubtypeCombo;
    private readonly Dictionary<string, IReadOnlyList<MetricData>> _subtypeCache = new(StringComparer.OrdinalIgnoreCase);

    public DistributionChartControllerAdapter(
        DistributionChartController controller,
        MainWindowViewModel viewModel,
        Func<bool> isInitializing,
        Func<IDisposable> beginUiBusyScope,
        MetricSelectionService metricSelectionService,
        Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator,
        WeeklyDistributionService weeklyDistributionService,
        HourlyDistributionService hourlyDistributionService,
        DistributionPolarRenderingService distributionPolarRenderingService,
        Func<ToolTip?> getPolarTooltip)
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
        ChartHelper.ResetZoom(_controller.Chart);
        _controller.PolarChart.FitToBounds = true;
    }

    public void ClearCache()
    {
        _subtypeCache.Clear();
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
                _controller.SubtypeCombo.Items.Add(BuildSeriesComboItem(selection));

            _controller.SubtypeCombo.IsEnabled = true;

            var current = _viewModel.ChartState.SelectedDistributionSeries;
            var seriesSelection = current != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase))
                ? current
                : selectedSeries[0];
            var distributionItem = FindSeriesComboItem(_controller.SubtypeCombo, seriesSelection) ?? _controller.SubtypeCombo.Items.OfType<ComboBoxItem>().FirstOrDefault();
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

    public void ApplyModeDefinition(DistributionMode mode)
    {
        var definition = DistributionModeCatalog.Get(mode);
        _controller.Panel.Title = definition.Title;

        if (_controller.Chart.AxisX.Count == 0)
            _controller.Chart.AxisX.Add(new LiveCharts.Wpf.Axis());

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

        var selection = GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
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
        return combo.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Tag is MetricSeriesSelection candidate && string.Equals(candidate.DisplayKey, selection.DisplayKey, StringComparison.OrdinalIgnoreCase));
    }

    private static MetricSeriesSelection? GetSeriesSelectionFromCombo(ComboBox combo)
    {
        if (combo.SelectedItem is ComboBoxItem item && item.Tag is MetricSeriesSelection selection)
            return selection;

        return combo.SelectedItem as MetricSeriesSelection;
    }

    private async Task RenderDistributionChartAsync(ChartDataContext ctx, DistributionMode mode)
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
            await RenderDistributionPolarChartAsync(ctx, mode, data, displayName);
            return;
        }

        var settings = _viewModel.ChartState.GetDistributionSettings(mode);
        var chart = _controller.Chart;
        var orchestrator = _getChartRenderingOrchestrator();

        if (orchestrator != null)
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
            await orchestrator.RenderDistributionChartAsync(distributionContext, chart, _viewModel.ChartState, mode);
            return;
        }

        var service = GetDistributionService(mode);
        await service.UpdateDistributionChartAsync(chart, data, displayName, ctx.From, ctx.To, 400, settings.UseFrequencyShading, settings.IntervalCount);
    }

    private async Task RenderDistributionPolarChartAsync(ChartDataContext ctx, DistributionMode mode, IReadOnlyList<MetricData> data, string displayName)
    {
        var service = GetDistributionService(mode);
        var rangeResult = await service.ComputeSimpleRangeAsync(data, displayName, ctx.From, ctx.To);
        if (rangeResult == null)
            return;

        var definition = DistributionModeCatalog.Get(mode);
        _distributionPolarRenderingService.RenderPolarChart(rangeResult, definition, _controller.PolarChart);
        _controller.PolarChart.Tag = new DistributionPolarTooltipState(definition, rangeResult);
        _controller.PolarChart.UpdateLayout();
        _controller.PolarChart.InvalidateVisual();
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
        if (_subtypeCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selectedSeries.MetricType, selectedSeries.QuerySubtype, null, ctx.From, ctx.To, tableName);
        var data = primaryData.ToList();
        _subtypeCache[cacheKey] = data;
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

    private BaseDistributionService GetDistributionService(DistributionMode mode)
    {
        return mode switch
        {
            DistributionMode.Weekly => _weeklyDistributionService,
            DistributionMode.Hourly => _hourlyDistributionService,
            _ => _weeklyDistributionService
        };
    }

    private static bool IsSameSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (!string.Equals(selection.MetricType, metricType ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            return false;

        var normalizedSubtype = string.IsNullOrWhiteSpace(subtype) || subtype == "(All)" ? null : subtype;
        var selectionSubtype = selection.QuerySubtype;

        return string.Equals(selectionSubtype ?? string.Empty, normalizedSubtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private void ClearDistributionPolarChart()
    {
        _controller.PolarChart.Series = Array.Empty<LiveChartsCore.ISeries>();
        _controller.PolarChart.AngleAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        _controller.PolarChart.RadiusAxes = Array.Empty<LiveChartsCore.SkiaSharpView.PolarAxis>();
        _controller.PolarChart.Tag = null;
        var tooltip = _getPolarTooltip();
        if (tooltip != null)
            tooltip.IsOpen = false;
    }
}
