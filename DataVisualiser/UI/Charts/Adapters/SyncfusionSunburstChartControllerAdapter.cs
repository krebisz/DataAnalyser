using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Infrastructure;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.Syncfusion;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Adapters;

public sealed class SyncfusionSunburstChartControllerAdapter : ChartControllerAdapterBase
{
    private readonly ISyncfusionSunburstChartController _controller;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MainWindowViewModel _viewModel;

    public SyncfusionSunburstChartControllerAdapter(
        ISyncfusionSunburstChartController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public override string Key => ChartControllerKeys.SyncfusionSunburst;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;

    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsSyncfusionSunburstVisible)
        {
            _controller.ItemsSource = Array.Empty<SunburstItem>();
            return Task.CompletedTask;
        }

        if (context == null)
        {
            _controller.ItemsSource = Array.Empty<SunburstItem>();
            return Task.CompletedTask;
        }

        return RenderSunburstAsync();
    }

    public override void Clear(ChartState state)
    {
        _controller.ItemsSource = Array.Empty<SunburstItem>();
    }

    public override void ResetZoom()
    {
        // Syncfusion Sunburst does not expose zoom reset in this POC.
    }

    public override bool HasSeries(ChartState state)
    {
        if (_controller.ItemsSource is IEnumerable<SunburstItem> items)
            return items.Any();

        return false;
    }

    public override void UpdateSubtypeOptions()
    {
    }

    private async Task RenderSunburstAsync()
    {
        var items = await BuildItemsFromSelectionsAsync();
        _controller.ItemsSource = items;
    }

    private async Task<IReadOnlyList<SunburstItem>> BuildItemsFromSelectionsAsync()
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return Array.Empty<SunburstItem>();

        if (!TryResolveDateRange(out var from, out var to))
            return Array.Empty<SunburstItem>();

        var bucketCount = ResolveBucketCount(from, to);
        var bucketPlan = BuildBucketPlan(from, to, bucketCount);
        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);

        if (seriesTotals.Count == 0)
            return Array.Empty<SunburstItem>();

        var items = new List<SunburstItem>();
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

                items.Add(new SunburstItem(bucket.Label, label, value.Value));
            }
        }

        return items;
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

    private static BucketPlan BuildBucketPlan(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<Bucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1 ? to.Ticks : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new Bucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new BucketPlan(from, to, bucketTicks, buckets);
    }

    private async Task<IReadOnlyList<SeriesTotals>> LoadSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BucketPlan plan)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<SeriesTotals?> LoadSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new SeriesTotals(selection, BuildBucketTotals(primaryCms, plan));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new SeriesTotals(selection, BuildBucketTotals(fallbackLegacy, plan));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new SeriesTotals(selection, BuildBucketTotals(dataList, plan));
        }
        catch
        {
            return null;
        }
    }

    private static double?[] BuildBucketTotals(IReadOnlyList<MetricData> data, BucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var point in data)
        {
            if (!point.Value.HasValue)
                continue;

            var index = ResolveBucketIndex(point.NormalizedTimestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)point.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static double?[] BuildBucketTotals(ICanonicalMetricSeries series, BucketPlan plan)
    {
        var totals = new double?[plan.Buckets.Count];
        var sums = new double[plan.Buckets.Count];
        var counts = new int[plan.Buckets.Count];

        foreach (var sample in series.Samples)
        {
            if (!sample.Value.HasValue)
                continue;

            var timestamp = sample.Timestamp.LocalDateTime;
            var index = ResolveBucketIndex(timestamp, plan);
            if (index < 0 || index >= sums.Length)
                continue;

            sums[index] += (double)sample.Value.Value;
            counts[index] += 1;
        }

        for (var i = 0; i < sums.Length; i++)
            totals[i] = counts[i] > 0 ? sums[i] / counts[i] : null;

        return totals;
    }

    private static int ResolveBucketIndex(DateTime timestamp, BucketPlan plan)
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

    private sealed record SeriesTotals(MetricSeriesSelection Selection, double?[] Totals);

    private sealed record Bucket(int Index, DateTime Start, DateTime End, string Label);

    private sealed record BucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<Bucket> Buckets);
}
