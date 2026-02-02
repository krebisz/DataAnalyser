using System.Linq;
using System.Windows;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class MainChartControllerAdapter : ChartControllerAdapterBase, IMainChartControllerExtras, ICartesianChartSurface, IWpfCartesianChartHost
{
    private readonly IMainChartController _controller;
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public MainChartControllerAdapter(IMainChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator, MetricSelectionService metricSelectionService)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public CartesianChart Chart => _controller.Chart;

    public override string Key => "Main";
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;
    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsMainVisible)
            return Task.CompletedTask;

        return RenderMainChartAsync(context);
    }

    public override void Clear(ChartState state)
    {
        ChartSurfaceHelper.ClearCartesian(_controller.Chart, state);
    }

    public override void ResetZoom()
    {
        ChartSurfaceHelper.ResetZoom(_controller.Chart);
    }

    public override bool HasSeries(ChartState state)
    {
        return ChartSurfaceHelper.HasSeries(_controller.Chart);
    }

    public override void UpdateSubtypeOptions()
    {
        if (_controller.OverlaySubtypeCombo == null)
            return;

        var selections = GetStackedSelections();
        _isUpdatingSubtypeCombo = true;
        try
        {
            if (selections.Count == 0)
            {
                ChartSubtypeComboHelper.DisableCombo(_controller.OverlaySubtypeCombo);
                _viewModel.ChartState.SelectedStackedOverlaySeries = null;
                UpdateOverlayControlsVisibility(selections);
                return;
            }

            ChartSubtypeComboHelper.PopulateCombo(_controller.OverlaySubtypeCombo, selections);
            var selection = ChartSubtypeComboHelper.ResolveSelection(selections, _viewModel.ChartState.SelectedStackedOverlaySeries) ?? selections[0];
            ChartSubtypeComboHelper.SelectComboItem(_controller.OverlaySubtypeCombo, selection);

            if (_isInitializing())
                _viewModel.ChartState.SelectedStackedOverlaySeries = selection;
            else
                _viewModel.SetStackedOverlaySeries(selection);
        }
        finally
        {
            _isUpdatingSubtypeCombo = false;
        }

        UpdateOverlayControlsVisibility(selections);
    }

    public override void ClearCache()
    {
    }

    public void SyncDisplayModeSelection()
    {
        var mode = _viewModel.ChartState.MainChartDisplayMode;
        _controller.DisplayRegularRadio.IsChecked = mode == MainChartDisplayMode.Regular;
        _controller.DisplaySummedRadio.IsChecked = mode == MainChartDisplayMode.Summed;
        _controller.DisplayStackedRadio.IsChecked = mode == MainChartDisplayMode.Stacked;
        UpdateOverlayControlsVisibility(GetStackedSelections());
    }


    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleMain();
    }

    public void OnDisplayModeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing())
            return;

        var mode = _controller.DisplayStackedRadio.IsChecked == true ? MainChartDisplayMode.Stacked : _controller.DisplaySummedRadio.IsChecked == true ? MainChartDisplayMode.Summed : MainChartDisplayMode.Regular;

        _viewModel.SetMainChartDisplayMode(mode);
        UpdateOverlayControlsVisibility(GetStackedSelections());
    }

    public void OnOverlaySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.OverlaySubtypeCombo);
        _viewModel.SetStackedOverlaySeries(selection);
    }

    private async Task RenderMainChartAsync(ChartDataContext ctx)
    {
        if (ctx.Data1 == null)
            return;

        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator == null)
            return;

        var mode = _viewModel.ChartState.MainChartDisplayMode;
        var selections = GetStackedSelections();
        var canStack = mode == MainChartDisplayMode.Stacked && selections.Count >= 2;
        var isCumulative = mode == MainChartDisplayMode.Summed;
        var overlaySeries = canStack ? await BuildOverlaySeriesAsync(ctx, selections) : null;

        await orchestrator.RenderPrimaryChartAsync(ctx, _controller.Chart, ctx.Data1, ctx.Data2, ctx.DisplayName1 ?? string.Empty, ctx.DisplayName2 ?? string.Empty, ctx.From, ctx.To, ctx.MetricType, _viewModel.MetricState.SelectedSeries, _viewModel.MetricState.ResolutionTableName, canStack, isCumulative, overlaySeries);
    }

    private List<MetricSeriesSelection> GetStackedSelections()
    {
        return _viewModel.MetricState.SelectedSeries
            .Where(selection => selection.QuerySubtype != null)
            .ToList();
    }

    private void UpdateOverlayControlsVisibility(IReadOnlyList<MetricSeriesSelection> selections)
    {
        if (_controller.OverlaySubtypePanel == null || _controller.OverlaySubtypeCombo == null)
            return;

        var isStacked = _viewModel.ChartState.MainChartDisplayMode == MainChartDisplayMode.Stacked;
        var canStack = isStacked && selections.Count >= 2;
        _controller.OverlaySubtypePanel.Visibility = canStack ? Visibility.Visible : Visibility.Collapsed;
        _controller.OverlaySubtypeCombo.IsEnabled = canStack;
    }

    private async Task<IReadOnlyList<SeriesResult>?> BuildOverlaySeriesAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selections)
    {
        var selection = ResolveOverlaySelection(selections);
        if (selection == null)
            return null;

        var data = ResolveContextSeries(ctx, selection);
        if (data == null)
        {
            if (string.IsNullOrWhiteSpace(selection.MetricType) || selection.QuerySubtype == null)
                return null;

            var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
            var loaded = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
            data = loaded.Primary.ToList();
        }

        var orderedData = StrategyComputationHelper.FilterAndOrderByRange(data, ctx.From, ctx.To);
        if (orderedData.Count == 0)
            return null;

        var rawTimestamps = orderedData.Select(d => d.NormalizedTimestamp).ToList();
        var rawValues = orderedData.Select(d => d.Value.HasValue ? (double)d.Value.Value : double.NaN).ToList();
        var smoothingService = new SmoothingService();
        var smoothedValues = smoothingService.SmoothSeries(orderedData, rawTimestamps, ctx.From, ctx.To).ToList();
        var displayName = $"{selection.DisplayName} (overlay)";

        return new[]
        {
                new SeriesResult
                {
                        SeriesId = "overlay_0",
                        DisplayName = displayName,
                        Timestamps = rawTimestamps,
                        RawValues = rawValues,
                        Smoothed = smoothedValues
                }
        };
    }

    private MetricSeriesSelection? ResolveOverlaySelection(IReadOnlyList<MetricSeriesSelection> selections)
    {
        if (selections == null || selections.Count == 0)
            return null;

        var current = _viewModel.ChartState.SelectedStackedOverlaySeries;
        if (current != null && selections.Any(selection => string.Equals(selection.DisplayKey, current.DisplayKey, StringComparison.OrdinalIgnoreCase)))
            return current;

        return selections[0];
    }

    private static IEnumerable<MetricData>? ResolveContextSeries(ChartDataContext ctx, MetricSeriesSelection selection)
    {
        if (IsMatchingSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (IsMatchingSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        return null;
    }

    private static bool IsMatchingSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(selection.MetricType))
            return false;

        if (!string.Equals(metricType, selection.MetricType, StringComparison.OrdinalIgnoreCase))
            return false;

        var selectionSubtype = selection.Subtype ?? string.Empty;
        var ctxSubtype = subtype ?? string.Empty;

        return string.Equals(selectionSubtype, ctxSubtype, StringComparison.OrdinalIgnoreCase);
    }
}
