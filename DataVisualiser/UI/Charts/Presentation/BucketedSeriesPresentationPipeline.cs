using DataVisualiser.Core.Computation.BucketedSeries;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class BucketedSeriesInputPlanner
{
    public static IReadOnlyList<MetricSeriesSelection> GetDistinctSelectedSeries(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        return viewModel.MetricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    public static bool TryResolveDateRange(MainWindowViewModel viewModel, out DateTime from, out DateTime to)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        if (viewModel.MetricState.FromDate.HasValue && viewModel.MetricState.ToDate.HasValue)
        {
            from = viewModel.MetricState.FromDate.Value;
            to = viewModel.MetricState.ToDate.Value;
            return true;
        }

        var context = viewModel.ChartState.LastContext;
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

    public static string ResolveTableName(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        return viewModel.MetricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
    }

    public static BucketedSeriesBucketPlan BuildBucketPlan(MainWindowViewModel viewModel, DateTime from, DateTime to)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        var bucketCount = BucketedSeriesBucketPlanBuilder.ResolveBucketCount(from, to, viewModel.ChartState.BarPieBucketCount);
        return BucketedSeriesBucketPlanBuilder.Build(from, to, bucketCount);
    }
}

internal sealed record BucketedSeriesTotals(MetricSeriesSelection Selection, double?[] Totals, LoadRuntimeState? Runtime = null);

internal sealed class BucketedSeriesLegacyTotalsLoader
{
    private readonly MetricSelectionService _metricSelectionService;

    public BucketedSeriesLegacyTotalsLoader(MetricSelectionService metricSelectionService)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    public async Task<BucketedSeriesTotals?> LoadAsync(
        MetricSeriesSelection selection,
        DateTime from,
        DateTime to,
        string tableName,
        BucketedSeriesBucketPlan plan,
        bool useCms,
        LoadRuntimeState? runtime = null)
    {
        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return null;

        try
        {
            if (useCms)
            {
                var (primaryCms, _, primaryLegacy, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, from, to, tableName);
                if (primaryCms != null && primaryCms.Samples.Count > 0)
                    return new BucketedSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(primaryCms, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count), runtime);

                var fallbackLegacy = primaryLegacy?.ToList() ?? new List<MetricData>();
                if (fallbackLegacy.Count == 0)
                    return null;

                return new BucketedSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(fallbackLegacy, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count), runtime);
            }

            var (primary, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, from, to, tableName);
            var dataList = primary?.ToList() ?? new List<MetricData>();
            if (dataList.Count == 0)
                return null;

            return new BucketedSeriesTotals(selection, TimeBucketAggregationHelper.BuildAverageTotals(dataList, plan.From, plan.To, plan.BucketTicks, plan.Buckets.Count), runtime);
        }
        catch
        {
            return null;
        }
    }
}
