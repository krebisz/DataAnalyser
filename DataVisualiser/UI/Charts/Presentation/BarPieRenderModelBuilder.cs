using System.Windows.Media;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation.Rendering;
using DataVisualiser.UI.Defaults;
using DataVisualiser.UI.ViewModels;
using UiChartRenderModel = DataVisualiser.UI.Charts.Presentation.Rendering.UiChartRenderModel;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class BarPieRenderModelBuilder
{
    private readonly MetricSelectionService _metricSelectionService;
    private readonly object _paletteKey;
    private readonly MainWindowViewModel _viewModel;

    public BarPieRenderModelBuilder(MainWindowViewModel viewModel, MetricSelectionService metricSelectionService, object paletteKey)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _paletteKey = paletteKey ?? throw new ArgumentNullException(nameof(paletteKey));
    }

    public async Task<UiChartRenderModel> BuildAsync(bool isPieMode)
    {
        var selections = GetDistinctSelectedSeries();
        if (selections.Count == 0)
            return CreateEmptyModel();

        if (!TryResolveDateRange(out var from, out var to))
            return CreateEmptyModel();

        var bucketCount = ResolveBucketCount(from, to);
        var bucketPlan = BuildBucketPlan(from, to, bucketCount);

        var seriesTotals = await LoadSeriesTotalsAsync(selections, from, to, bucketPlan);
        if (seriesTotals.Count == 0)
            return CreateEmptyModel();

        ColourPalette.Reset(_paletteKey);
        var coloredSeries = seriesTotals.Select(data => new BarPieSeriesValues(data.Selection, data.Totals, ColourPalette.Next(_paletteKey))).ToList();

        return isPieMode
            ? BuildPieModel(bucketPlan, coloredSeries)
            : BuildBarModel(bucketPlan, coloredSeries);
    }

    private UiChartRenderModel BuildPieModel(BarPieBucketPlan bucketPlan, IReadOnlyList<BarPieSeriesValues> coloredSeries)
    {
        var facets = bucketPlan.Buckets.Select(bucket =>
                               {
                                   var series = coloredSeries.Select(item => new ChartSeriesModel
                                   {
                                       Name = item.Selection.DisplayName,
                                       SeriesType = ChartSeriesType.Pie,
                                       Values = [item.Totals[bucket.Index]],
                                       Color = item.Color
                                   }).ToList();

                                   return new ChartFacetModel
                                   {
                                       Title = bucket.Label,
                                       Series = series
                                   };
                               })
                               .ToList();

        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Facets = facets,
            Legend = new ChartLegendModel
            {
                IsVisible = true,
                Placement = ChartLegendPlacement.Right
            },
            Interactions = new ChartInteractionModel
            {
                Hoverable = ChartUiDefaults.DefaultHoverable
            }
        };
    }

    private UiChartRenderModel BuildBarModel(BarPieBucketPlan bucketPlan, IReadOnlyList<BarPieSeriesValues> coloredSeries)
    {
        var barSeries = coloredSeries.Select(item => new ChartSeriesModel
        {
            Name = item.Selection.DisplayName,
            SeriesType = ChartSeriesType.Column,
            Values = item.Totals,
            Color = item.Color
        }).ToList();

        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Series = barSeries,
            AxesX =
            [
                new ChartAxisModel
                {
                    Title = "Interval",
                    Labels = bucketPlan.Buckets.Select(bucket => bucket.Label).ToList()
                }
            ],
            AxesY =
            [
                new ChartAxisModel
                {
                    Title = "Value"
                }
            ],
            Legend = new ChartLegendModel
            {
                IsVisible = true,
                Placement = ChartLegendPlacement.Right
            },
            Interactions = new ChartInteractionModel
            {
                EnableZoomX = true,
                EnablePanX = true,
                Hoverable = ChartUiDefaults.DefaultHoverable
            }
        };
    }

    private UiChartRenderModel CreateEmptyModel()
    {
        return new UiChartRenderModel
        {
            ChartName = RenderingDefaults.BarPieChartName,
            Title = ChartUiDefaults.BarPieChartTitle,
            IsVisible = _viewModel.ChartState.IsBarPieVisible,
            Series = [],
            Facets = []
        };
    }

    private List<MetricSeriesSelection> GetDistinctSelectedSeries()
    {
        return _viewModel.MetricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    private async Task<IReadOnlyList<BarPieSeriesTotals>> LoadSeriesTotalsAsync(IReadOnlyList<MetricSeriesSelection> selections, DateTime from, DateTime to, BarPieBucketPlan plan)
    {
        var tableName = _viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var useCms = CmsConfiguration.ShouldUseCms("BarPieStrategy");
        var tasks = selections.Select(selection => LoadSeriesTotalsAsync(selection, from, to, tableName, plan, useCms)).ToList();
        var results = await Task.WhenAll(tasks);
        return results.Where(result => result != null).Select(result => result!).ToList();
    }

    private async Task<BarPieSeriesTotals?> LoadSeriesTotalsAsync(MetricSeriesSelection selection, DateTime from, DateTime to, string tableName, BarPieBucketPlan plan, bool useCms)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new BarPieSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(primaryCms, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new BarPieSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(fallbackLegacy, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new BarPieSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(dataList, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count));
        }
        catch
        {
            return null;
        }
    }

    private int ResolveBucketCount(DateTime from, DateTime to)
    {
        const int bucketMax = 20;

        if (to <= from)
            return 1;

        var bucketCount = _viewModel.ChartState.BarPieBucketCount;
        return Math.Max(1, Math.Min(bucketCount, bucketMax));
    }

    private static BarPieBucketPlan BuildBucketPlan(DateTime from, DateTime to, int bucketCount)
    {
        if (to < from)
            (from, to) = (to, from);

        bucketCount = Math.Max(1, bucketCount);
        var totalTicks = Math.Max(1, (to - from).Ticks);
        var bucketTicks = totalTicks / (double)bucketCount;

        var buckets = new List<BarPieBucket>(bucketCount);
        for (var i = 0; i < bucketCount; i++)
        {
            var startTicks = from.Ticks + (long)Math.Floor(i * bucketTicks);
            var endTicks = i == bucketCount - 1 ? to.Ticks : from.Ticks + (long)Math.Floor((i + 1) * bucketTicks);

            if (endTicks < startTicks)
                endTicks = startTicks;

            var start = new DateTime(startTicks);
            var end = new DateTime(Math.Min(endTicks, to.Ticks));
            buckets.Add(new BarPieBucket(i, start, end, $"{start:yyyy-MM-dd} - {end:yyyy-MM-dd}"));
        }

        return new BarPieBucketPlan(from, to, bucketTicks, buckets);
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

    private sealed record BarPieSeriesTotals(MetricSeriesSelection Selection, double?[] Totals);
    private sealed record BarPieSeriesValues(MetricSeriesSelection Selection, double?[] Totals, Color Color);
    private sealed record BarPieBucket(int Index, DateTime Start, DateTime End, string Label);
    private sealed record BarPieBucketPlan(DateTime From, DateTime To, double BucketTicks, IReadOnlyList<BarPieBucket> Buckets);
}
