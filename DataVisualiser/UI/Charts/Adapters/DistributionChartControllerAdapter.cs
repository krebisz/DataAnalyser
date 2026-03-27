using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using Axis = LiveCharts.Wpf.Axis;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class DistributionChartControllerAdapter : CartesianChartControllerAdapterBase<IDistributionChartController>, IDistributionChartControllerExtras, IPolarChartSurface
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly IDistributionChartController _controller;
    private readonly IDistributionRenderingContract _distributionRenderingContract;
    private readonly Func<ToolTip?> _getPolarTooltip;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MetricSeriesSelectionCache _selectionCache = new();
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public DistributionChartControllerAdapter(IDistributionChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, IDistributionRenderingContract distributionRenderingContract, Func<ToolTip?> getPolarTooltip)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _distributionRenderingContract = distributionRenderingContract ?? throw new ArgumentNullException(nameof(distributionRenderingContract));
        _getPolarTooltip = getPolarTooltip ?? throw new ArgumentNullException(nameof(getPolarTooltip));
    }

    public override void ClearCache()
    {
        _selectionCache.Clear();
    }

    public override string Key => ChartControllerKeys.Distribution;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        return RenderDistributionChartAsync(context, _viewModel.ChartState.SelectedDistributionMode);
    }

    public override void Clear(ChartState state)
    {
        RenderingHostLifecycleAdapterHelper.Clear(CreateRenderHost, _distributionRenderingContract.Clear);
    }

    public override void ResetZoom()
    {
        RenderingHostLifecycleAdapterHelper.ResetView(ResolveRenderingRoute, CreateRenderHost, _distributionRenderingContract.ResetView);
    }

    public override bool HasSeries(ChartState state)
    {
        return RenderingHostLifecycleAdapterHelper.HasRenderableContent(ResolveRenderingRoute, CreateRenderHost, _distributionRenderingContract.HasRenderableContent);
    }

    public override void UpdateSubtypeOptions()
    {
        if (_controller.SubtypeCombo == null)
            return;

        _isUpdatingSubtypeCombo = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            var seriesSelection = MetricSeriesSelectionAdapterHelper.PopulateSubtypeCombo(_controller.SubtypeCombo, selectedSeries, _viewModel.ChartState.SelectedDistributionSeries);
            if (seriesSelection == null)
                _viewModel.ChartState.SelectedDistributionSeries = null;
            else if (_isInitializing())
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
        ChartComboItemHelper.Populate(_controller.ModeCombo,
                DistributionModeCatalog.All.Select(definition => (definition.DisplayName, (object)definition.Mode)));

        ChartComboItemHelper.Populate(_controller.IntervalCountCombo,
                DistributionModeCatalog.IntervalCounts.Select(intervalCount => (intervalCount.ToString(), (object)intervalCount)));

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
            _controller.Chart.Visibility = Visibility.Visible;
            _controller.PolarChart.Visibility = Visibility.Collapsed;
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
            await RerenderDistributionIfVisibleAsync(mode);
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
            await RerenderDistributionIfVisibleAsync(mode);
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
        _ = ChartComboItemHelper.TrySelectByTag(_controller.ModeCombo, tag => tag is DistributionMode taggedMode && taggedMode == mode);
    }

    private void SelectDistributionIntervalCount(int intervalCount)
    {
        _ = ChartComboItemHelper.TrySelectByTag(_controller.IntervalCountCombo, tag => tag is int taggedInterval && taggedInterval == intervalCount);
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

        var renderInput = await BuildDistributionRenderInputAsync(ctx);
        if (renderInput == null)
            return;

        var renderTarget = RenderingHostLifecycleAdapterHelper.CreateTarget(ResolveRenderingRoute, CreateRenderHost);
        var request = new DistributionChartRenderRequest(
            renderTarget.Route,
            mode,
            _viewModel.ChartState.GetDistributionSettings(mode),
            renderInput.Data,
            renderInput.DisplayName,
            ctx.From,
            ctx.To,
            renderInput.CmsSeries,
            BuildDistributionContext(ctx, renderInput),
            _viewModel.ChartState);

        await _distributionRenderingContract.RenderAsync(request, renderTarget.Host);
    }

    private async Task RerenderDistributionIfVisibleAsync(DistributionMode mode)
    {
        if (!_viewModel.ChartState.IsDistributionVisible || _viewModel.ChartState.LastContext?.Data1 == null)
            return;

        using var _ = _beginUiBusyScope();
        var ctx = _viewModel.ChartState.LastContext;
        if (ctx != null)
            await RenderDistributionChartAsync(ctx, mode);
    }

    private async Task<DistributionRenderInput?> BuildDistributionRenderInputAsync(ChartDataContext ctx)
    {
        var selectedSeries = ResolveSelectedDistributionSeries(ctx);
        var (data, cmsSeries) = await ResolveDistributionDataAsync(ctx, selectedSeries);
        if (data == null || (data.Count == 0 && cmsSeries == null))
            return null;

        var displayName = ResolveDistributionDisplayName(ctx, selectedSeries);
        return new DistributionRenderInput(selectedSeries, data, cmsSeries, displayName);
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
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(!_isUpdatingSubtypeCombo, _controller.SubtypeCombo, _viewModel.ChartState.SelectedDistributionSeries, ctx);
    }

    private static string ResolveDistributionDisplayName(ChartDataContext ctx, MetricSeriesSelection? selectedSeries)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveDisplayName(ctx, selectedSeries);
    }

    private DistributionRenderingRoute ResolveRenderingRoute()
    {
        return DistributionRenderingRouteResolver.Resolve(_viewModel.ChartState.IsDistributionPolarMode);
    }

    private static ChartDataContext BuildDistributionContext(ChartDataContext ctx, DistributionRenderInput renderInput)
    {
        return new ChartDataContext
        {
            PrimaryCms = renderInput.CmsSeries,
            Data1 = renderInput.Data,
            DisplayName1 = renderInput.DisplayName,
            MetricType = renderInput.SelectedSeries?.MetricType ?? ctx.MetricType,
            PrimaryMetricType = renderInput.SelectedSeries?.MetricType ?? ctx.PrimaryMetricType,
            PrimarySubtype = renderInput.SelectedSeries?.Subtype,
            DisplayPrimaryMetricType = renderInput.SelectedSeries?.DisplayMetricType ?? ctx.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = renderInput.SelectedSeries?.DisplaySubtype ?? ctx.DisplayPrimarySubtype,
            From = ctx.From,
            To = ctx.To
        };
    }

    private DistributionChartRenderHost CreateRenderHost()
    {
        return new DistributionChartRenderHost(_controller.Chart, _controller.PolarChart, _viewModel.ChartState, _getPolarTooltip);
    }

    private sealed record DistributionRenderInput(
        MetricSeriesSelection? SelectedSeries,
        IReadOnlyList<MetricData> Data,
        ICanonicalMetricSeries? CmsSeries,
        string DisplayName);
}
