using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using DataVisualiser.UI.Syncfusion;
using DataVisualiser.UI.ViewModels;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.UI.Charts.Presentation;

public sealed class SyncfusionSunburstChartControllerAdapter : ChartControllerAdapterBase
{
    private readonly ISyncfusionSunburstChartController _controller;
    private readonly ISyncfusionSunburstRenderingContract _renderingContract;
    private readonly MetricSelectionService _metricSelectionService;
    private readonly MainWindowViewModel _viewModel;

    public SyncfusionSunburstChartControllerAdapter(
        ISyncfusionSunburstChartController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        ISyncfusionSunburstRenderingContract? renderingContract = null)
        : base(controller)
    {
        _controller = controller ?? throw new ArgumentNullException(nameof(controller));
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _renderingContract = renderingContract ?? new SyncfusionSunburstRenderingContract();
    }

    public override string Key => ChartControllerKeys.SyncfusionSunburst;
    public override bool RequiresPrimaryData => true;
    public override bool RequiresSecondaryData => false;

    public override Task RenderAsync(ChartDataContext context)
    {
        if (!_viewModel.ChartState.IsSyncfusionSunburstVisible)
        {
            _renderingContract.Clear(CreateRenderHost());
            return Task.CompletedTask;
        }

        if (context == null)
        {
            _renderingContract.Clear(CreateRenderHost());
            return Task.CompletedTask;
        }

        return RenderSunburstAsync();
    }

    public override void Clear(ChartState state)
    {
        _renderingContract.Clear(CreateRenderHost());
    }

    public override void ResetZoom()
    {
        _renderingContract.ResetView(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override bool HasSeries(ChartState state)
    {
        return _renderingContract.HasRenderableContent(ResolveRenderingRoute(), CreateRenderHost());
    }

    public override void UpdateSubtypeOptions()
    {
    }

    private async Task RenderSunburstAsync()
    {
        var model = await BuildRenderModelFromSelectionsAsync();
        var route = ResolveRenderingRoute();
        var request = new SyncfusionSunburstChartRenderRequest(
            route,
            model.Items,
            model.BucketCount,
            model.SelectionCount,
            model.From,
            model.To);

        await _renderingContract.RenderAsync(request, CreateRenderHost());
        _viewModel.ChartState.SetRenderPlanDiagnostics(
            ChartProgramKind.SyncfusionSunburst,
            BuildRenderPlanDiagnostics(request));
    }

    private async Task<SyncfusionSunburstRenderModel> BuildRenderModelFromSelectionsAsync()
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SunburstItem>(), 0, 0, null, null);

        if (!TryResolveDateRange(out var from, out var to))
            return new SyncfusionSunburstRenderModel(Array.Empty<SunburstItem>(), 0, selections.Count, null, null);

        var bucketCount = ResolveBucketCount(from, to);
        var bucketPlan = BuildBucketPlan(from, to, bucketCount);
        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);

        if (seriesTotals.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SunburstItem>(), bucketCount, selections.Count, from, to);

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

        return new SyncfusionSunburstRenderModel(items, bucketCount, selections.Count, from, to);
    }

    private SyncfusionSunburstRenderingRoute ResolveRenderingRoute()
    {
        return SyncfusionSunburstRenderingRoute.Hierarchy;
    }

    private SyncfusionSunburstChartRenderHost CreateRenderHost()
    {
        return new SyncfusionSunburstChartRenderHost(_controller, _viewModel.ChartState.IsSyncfusionSunburstVisible);
    }

    private static ChartRenderAdapterResult BuildRenderPlanDiagnostics(SyncfusionSunburstChartRenderRequest request)
    {
        var bucketCount = request.Items
            .Select(item => item.Bucket)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var submetricCount = request.Items
            .Select(item => item.Submetric)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
        var renderedNodeCount = bucketCount + request.Items.Count;

        return new ChartRenderAdapterResult(
            SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy,
            $"{SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy}:{request.Route}:{request.BucketCount}:{request.SelectionCount}:{request.From:O}:{request.To:O}:{request.Items.Count}",
            ChartRenderPlanKind.Hierarchy,
            ChartRenderDensityMode.FullFidelity,
            submetricCount,
            renderedNodeCount,
            request.Items.Count,
            new Dictionary<string, string>
            {
                ["Adapter"] = nameof(SyncfusionSunburstChartControllerAdapter),
                ["ProgramKind"] = ChartProgramKind.SyncfusionSunburst.ToString(),
                ["Route"] = request.Route.ToString(),
                ["BucketCount"] = request.BucketCount.ToString(),
                ["SelectionCount"] = request.SelectionCount.ToString()
            });
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
                    return new SeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(primaryCms, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new SeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(fallbackLegacy, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new SeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(dataList, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
        }
        catch
        {
            return null;
        }
    }

    private sealed record SeriesTotals(MetricSeriesSelection Selection, double?[] Totals);

    private sealed record Bucket(int Index, DateTime Start, DateTime End, string Label);

    private sealed record BucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<Bucket> Buckets);

    private sealed record SyncfusionSunburstRenderModel(
        IReadOnlyList<SunburstItem> Items,
        int BucketCount,
        int SelectionCount,
        DateTime? From,
        DateTime? To);
}
