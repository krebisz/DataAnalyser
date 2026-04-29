using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class CartesianMetricOverlaySeriesBuilder
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MetricSelectionService _metricSelectionService;

    public CartesianMetricOverlaySeriesBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public async Task<IReadOnlyList<SeriesResult>?> BuildAsync(ChartDataContext ctx, IReadOnlyList<MetricSeriesSelection> selections)
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
