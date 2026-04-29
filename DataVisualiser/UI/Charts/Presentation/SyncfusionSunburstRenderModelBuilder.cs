using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class SyncfusionSunburstRenderModelBuilder
{
    private readonly MainWindowViewModel _viewModel;
    private readonly MetricSelectionService _metricSelectionService;

    public SyncfusionSunburstRenderModelBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public async Task<SyncfusionSunburstRenderModel> BuildAsync()
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), 0, 0, null, null);

        if (!TryResolveDateRange(out var from, out var to))
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), 0, selections.Count, null, null);

        var bucketCount = ResolveBucketCount(from, to);
        var bucketPlan = BuildBucketPlan(from, to, bucketCount);
        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);

        if (seriesTotals.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), bucketCount, selections.Count, from, to);

        var items = new List<SyncfusionSunburstItem>();
        foreach (var bucket in bucketPlan.Buckets)
        {
            foreach (var series in seriesTotals)
            {
                var value = series.Totals[bucket.Index];
                if (!value.HasValue)
                    continue;

                var label = string.IsNullOrWhiteSpace(series.Selection.DisplayName)
                    ? series.Selection.DisplayKey
                    : series.Selection.DisplayName;

                items.Add(new SyncfusionSunburstItem(bucket.Label, label, value.Value));
            }
        }

        return new SyncfusionSunburstRenderModel(items, bucketCount, selections.Count, from, to);
    }

    private List<MetricSeriesSelection> GetDistinctSelectedSeries()
    {
        return _viewModel.MetricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private bool TryResolveDateRange(out DateTime from, out DateTime to)
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

    private int ResolveBucketCount(DateTime from, DateTime to)
    {
        const int bucketMax = 20;

        if (to <= from)
            return 1;

        var bucketCount = _viewModel.ChartState.BarPieBucketCount;
        return Math.Max(1, Math.Min(bucketCount, bucketMax));
    }

    private static SunburstBucketPlan BuildBucketPlan(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<SunburstBucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1 ? to.Ticks : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new SunburstBucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new SunburstBucketPlan(from, to, bucketTicks, buckets);
    }

    private async Task<IReadOnlyList<SunburstSeriesTotals>> LoadSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, SunburstBucketPlan plan)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<SunburstSeriesTotals?> LoadSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, SunburstBucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new SunburstSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(primaryCms, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new SunburstSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(fallbackLegacy, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new SunburstSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(dataList, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record SunburstSeriesTotals(MetricSeriesSelection Selection, double?[] Totals);

internal sealed record SunburstBucket(int Index, DateTime Start, DateTime End, string Label);

internal sealed record SunburstBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<SunburstBucket> Buckets);

internal sealed record SyncfusionSunburstRenderModel(
    IReadOnlyList<SyncfusionSunburstItem> Items,
    int BucketCount,
    int SelectionCount,
    DateTime? From,
    DateTime? To);
