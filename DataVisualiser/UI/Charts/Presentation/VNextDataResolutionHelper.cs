using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class VNextDataResolutionHelper
{
    public static async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveSeriesDataAsync(
        ChartDataContext ctx,
        MetricSeriesSelection? selectedSeries,
        MetricSeriesSelectionCache cache,
        string tableName,
        VNextSeriesLoadCoordinator vnextCoordinator,
        ChartProgramKind programKind,
        EvidenceRuntimePath vnextPath,
        Action<LoadRuntimeState> setRuntime,
        Func<MetricSeriesSelection, DateTime, DateTime, string, Task<(IReadOnlyList<MetricData> Data, ICanonicalMetricSeries? Cms)>> legacyLoad)
    {
        if (ctx.Data1 == null)
            return (null, null);

        if (selectedSeries == null)
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        if (MetricSeriesSelectionCache.IsSameSelection(selectedSeries, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2 ?? ctx.Data1, ctx.SecondaryCms as ICanonicalMetricSeries);

        if (string.IsNullOrWhiteSpace(selectedSeries.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries);

        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(selectedSeries, ctx.From, ctx.To, tableName);
        if (cache.TryGetDataWithCms(cacheKey, out var cached, out var cachedCms))
            return (cached, cachedCms);

        var vnextResult = await vnextCoordinator.LoadAsync(selectedSeries, ctx.From, ctx.To, tableName, programKind);
        if (vnextResult.Success && vnextResult.Data != null)
        {
            setRuntime(LoadRuntimeState.FromVNextSuccess(
                vnextPath, vnextResult.RequestSignature,
                vnextResult.SnapshotSignature, vnextResult.ProgramKind, vnextResult.ProgramSourceSignature));

            var data = vnextResult.Data is List<MetricData> list ? list : vnextResult.Data.ToList();
            cache.SetDataWithCms(cacheKey, data, vnextResult.CmsSeries);
            return (data, vnextResult.CmsSeries);
        }

        var (legacyData, legacyCms) = await legacyLoad(selectedSeries, ctx.From, ctx.To, tableName);
        var legacyDataList = legacyData is List<MetricData> legacyList ? legacyList : legacyData.ToList();
        cache.SetDataWithCms(cacheKey, legacyDataList, legacyCms);

        setRuntime(LoadRuntimeState.LegacyFallback(vnextResult.RequestSignature, vnextResult.FailureReason));

        return (legacyDataList, legacyCms);
    }
}
