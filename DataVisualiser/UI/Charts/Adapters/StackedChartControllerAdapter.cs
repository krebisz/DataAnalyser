using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Orchestration.Coordinator;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Implementations;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Helpers;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts.Wpf;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class StackedChartControllerAdapter : ChartControllerAdapterBase, ICartesianChartSurface, IWpfCartesianChartHost
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;
    private readonly IStackedChartController _controller;
    private readonly Func<bool> _isInitializing;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MainWindowViewModel _viewModel;
    private bool _isUpdatingSubtypeCombo;

    public StackedChartControllerAdapter(IStackedChartController controller, MainWindowViewModel viewModel, Func<bool> isInitializing, MetricSelectionService metricSelectionService, ChartUpdateCoordinator chartUpdateCoordinator)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _isInitializing = isInitializing ?? throw new ArgumentNullException(nameof(isInitializing));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public CartesianChart Chart => _controller.Chart;

    public override string Key => ChartControllerKeys.Stacked;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;

    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsStackedVisible)
            return Task.CompletedTask;

        return RenderStackedChartAsync(context);
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

        _isUpdatingSubtypeCombo = true;
        try
        {
            var selectedSeries = _viewModel.MetricState.SelectedSeries;
            if (selectedSeries.Count == 0)
            {
                ChartSubtypeComboHelper.DisableCombo(_controller.OverlaySubtypeCombo);
                _viewModel.ChartState.SelectedStackedOverlaySeries = null;
                return;
            }

            ChartSubtypeComboHelper.PopulateCombo(_controller.OverlaySubtypeCombo, selectedSeries);
            var selection = ChartSubtypeComboHelper.ResolveSelection(selectedSeries, _viewModel.ChartState.SelectedStackedOverlaySeries) ?? selectedSeries[0];
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
    }

    public override void ClearCache()
    {
    }

    public void OnToggleRequested(object? sender, EventArgs e)
    {
        _viewModel.ToggleStacked();
    }

    public void OnOverlaySubtypeChanged(object? sender, EventArgs e)
    {
        if (_isInitializing() || _isUpdatingSubtypeCombo)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(_controller.OverlaySubtypeCombo);
        _viewModel.SetStackedOverlaySeries(selection);
    }

    private async Task RenderStackedChartAsync(ChartDataContext ctx)
    {
        if (ctx.Data1 == null)
            return;

        var selections = _viewModel.MetricState.SelectedSeries
                .Where(selection => selection.QuerySubtype != null)
                .ToList();

        if (selections.Count < 2)
        {
            ChartSurfaceHelper.ClearCartesian(_controller.Chart, _viewModel.ChartState);
            return;
        }

        var (series, labels) = await BuildSeriesFromSelectionsAsync(ctx, selections);
        if (series.Count < 2)
        {
            ChartSurfaceHelper.ClearCartesian(_controller.Chart, _viewModel.ChartState);
            return;
        }

        var strategy = new MultiMetricStrategy(series, labels, ctx.From, ctx.To);
        var overlaySeries = await BuildOverlaySeriesAsync(ctx, selections);

        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(_controller.Chart, strategy, labels[0], null, 400, ctx.MetricType, ctx.PrimarySubtype, null, isOperationChart: false, secondaryMetricType: ctx.SecondaryMetricType, displayPrimaryMetricType: ctx.DisplayPrimaryMetricType, displaySecondaryMetricType: ctx.DisplaySecondaryMetricType, displayPrimarySubtype: ctx.DisplayPrimarySubtype, displaySecondarySubtype: ctx.DisplaySecondarySubtype, isStacked: true, isCumulative: false, overlaySeries: overlaySeries);
    }

    private async Task<(List<IEnumerable<MetricData>> Series, List<string> Labels)> BuildSeriesFromSelectionsAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selections)
    {
        var series = new List<IEnumerable<MetricData>>();
        var labels = new List<string>();
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;

        foreach (var selection in selections)
        {
            var data = ResolveContextSeries(ctx, selection);
            if (data == null)
            {
                if (string.IsNullOrWhiteSpace(selection.MetricType))
                    continue;

                var loaded = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
                data = loaded.Primary.ToList();
            }

            if (data == null || !data.Any())
                continue;

            series.Add(data);
            labels.Add(selection.DisplayName);
        }

        return (series, labels);
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
