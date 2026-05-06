using DataVisualiser.UI.Charts.Presentation;
using System.Windows;
using System.Windows.Controls;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Distribution;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Services.Abstractions;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using Axis = LiveCharts.Wpf.Axis;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class DistributionChartControllerAdapter : CartesianChartControllerAdapterBase<IDistributionChartController>, IDistributionChartControllerExtras, IPolarChartSurface
{
    private readonly Func<IDisposable> _beginUiBusyScope;
    private readonly IDistributionChartController _controller;
    private readonly IDistributionRenderingContract _distributionRenderingContract;
    private readonly DistributionRenderInputBuilder _renderInputBuilder;
    private readonly Func<ToolTip?> _getPolarTooltip;
    private readonly Func<bool> _isInitializing;
    private readonly DistributionSessionMilestoneRecorder _milestoneRecorder;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public DistributionChartControllerAdapter(IDistributionChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<IDisposable> beginUiBusyScope, MetricSelectionService metricSelectionService, IDistributionRenderingContract distributionRenderingContract, Func<ToolTip?> getPolarTooltip, VNextSeriesLoadCoordinator? vnextCoordinator = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _beginUiBusyScope = beginUiBusyScope ?? throw new ArgumentNullException(nameof(beginUiBusyScope));
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        _distributionRenderingContract = distributionRenderingContract ?? throw new ArgumentNullException(nameof(distributionRenderingContract));
        _getPolarTooltip = getPolarTooltip ?? throw new ArgumentNullException(nameof(getPolarTooltip));
        _renderInputBuilder = new DistributionRenderInputBuilder(viewModel, metricSelectionService, vnextCoordinator);
        _milestoneRecorder = new DistributionSessionMilestoneRecorder(viewModel);
    }

    public override void ClearCache()
    {
        _renderInputBuilder.ClearCache();
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
            _milestoneRecorder.RecordFrequencyShadingToggle(mode, useFrequencyShading);
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
            _milestoneRecorder.RecordIntervalCountChange(mode, intervalCount);
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
        _milestoneRecorder.RecordChartTypeToggle(_viewModel.ChartState.IsDistributionPolarMode);
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
        _milestoneRecorder.RecordModeChange(mode);
        ApplyModeDefinition(mode);
        ApplySettingsToUi(mode);
    }

    public void OnSubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.SubtypeCombo);
        _viewModel.SetDistributionSeries(selection);
        _milestoneRecorder.RecordSubtypeChange(selection);
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

        var selectedSeries = ResolveSelectedDistributionSeries(ctx);
        var renderInput = await _renderInputBuilder.BuildAsync(ctx, selectedSeries);
        if (renderInput == null)
            return;

        var renderTarget = RenderingHostLifecycleAdapterHelper.CreateTarget(ResolveRenderingRoute, CreateRenderHost);
        var request = new DistributionChartRenderRequest(
            renderTarget.Route,
            mode,
            _viewModel.ChartState.GetDistributionSettings(mode),
            renderInput.Data,
            renderInput.DisplayName,
            renderInput.From,
            renderInput.To,
            renderInput.CmsSeries,
            DistributionRenderInputBuilder.BuildDistributionContext(ctx, renderInput),
            _viewModel.ChartState,
            renderInput.SelectedSeries?.DisplayKey ?? "<none>",
            DistributionCapabilityContract.Create());

        var renderResult = await _distributionRenderingContract.RenderAsync(request, renderTarget.Host);
        _viewModel.ChartState.SetRenderPlanDiagnostics(
            ChartProgramKind.Distribution,
            renderResult);
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

    private MetricSeriesSelection? ResolveSelectedDistributionSeries(ChartDataContext ctx)
    {
        return MetricSeriesSelectionAdapterHelper.ResolveSelectedSeries(!_isUpdatingSubtypeCombo, _controller.SubtypeCombo, _viewModel.ChartState.SelectedDistributionSeries, ctx);
    }

    private DistributionRenderingRoute ResolveRenderingRoute()
    {
        return DistributionRenderingRouteResolver.Resolve(_viewModel.ChartState.IsDistributionPolarMode);
    }

    private DistributionChartRenderHost CreateRenderHost()
    {
        return new DistributionChartRenderHost(_controller.Chart, _controller.PolarChart, _viewModel.ChartState, _getPolarTooltip);
    }
}
