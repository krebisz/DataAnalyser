using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Computation.BucketedSeries;
using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class SyncfusionSunburstRenderModelBuilder
{
    private readonly MainWindowViewModel _viewModel;
    private readonly BucketedSeriesLegacyTotalsLoader _legacyTotalsLoader;

    public SyncfusionSunburstRenderModelBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _legacyTotalsLoader = new BucketedSeriesLegacyTotalsLoader(metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService)));
    }

    public async Task<SyncfusionSunburstRenderModel> BuildAsync()
    {
        var selections = BucketedSeriesInputPlanner.GetDistinctSelectedSeries(_viewModel);
        if (selections.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), 0, 0, null, null);

        if (!BucketedSeriesInputPlanner.TryResolveDateRange(_viewModel, out var from, out var to))
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), 0, selections.Count, null, null);

        var bucketPlan = BucketedSeriesInputPlanner.BuildBucketPlan(_viewModel, from, to);
        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);

        if (seriesTotals.Count == 0)
            return new SyncfusionSunburstRenderModel(Array.Empty<SyncfusionSunburstItem>(), bucketPlan.Buckets.Count, selections.Count, from, to);

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

        return new SyncfusionSunburstRenderModel(items, bucketPlan.Buckets.Count, selections.Count, from, to);
    }

    private async Task<IReadOnlyList<BucketedSeriesTotals>> LoadSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BucketedSeriesBucketPlan plan)
    {
        var tableName = BucketedSeriesInputPlanner.ResolveTableName(_viewModel);
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => _legacyTotalsLoader.LoadAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }
}

internal sealed record SyncfusionSunburstRenderModel(
    IReadOnlyList<SyncfusionSunburstItem> Items,
    int BucketCount,
    int SelectionCount,
    DateTime? From,
    DateTime? To);
