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
        var result = await ResolveSeriesDataAsync(SeriesResolutionRequest.FromContext(
            ctx,
            selectedSeries,
            cache,
            tableName,
            vnextCoordinator,
            programKind,
            vnextPath,
            setRuntime,
            legacyLoad));

        return (result.Data, result.Cms);
    }

    public static async Task<SeriesResolutionResult> ResolveSeriesDataAsync(SeriesResolutionRequest request)
    {
        if (request.PrimaryData == null)
            return new SeriesResolutionResult(null, null);

        if (request.SelectedSeries == null)
            return new SeriesResolutionResult(request.PrimaryData, request.PrimaryCms);

        if (MetricSeriesSelectionCache.IsSameSelection(request.SelectedSeries, request.PrimaryMetricType ?? request.MetricType, request.PrimarySubtype))
            return new SeriesResolutionResult(request.PrimaryData, request.PrimaryCms);

        if (MetricSeriesSelectionCache.IsSameSelection(request.SelectedSeries, request.SecondaryMetricType, request.SecondarySubtype))
            return new SeriesResolutionResult(request.SecondaryData ?? request.PrimaryData, request.SecondaryCms);

        if (string.IsNullOrWhiteSpace(request.SelectedSeries.MetricType))
            return new SeriesResolutionResult(request.PrimaryData, request.PrimaryCms);

        var cacheKey = MetricSeriesSelectionCache.BuildCacheKey(request.SelectedSeries, request.From, request.To, request.TableName);
        if (request.Cache.TryGetDataWithCms(cacheKey, out var cached, out var cachedCms))
            return new SeriesResolutionResult(cached, cachedCms);

        var vnextResult = await request.VNextCoordinator.LoadAsync(
            request.SelectedSeries,
            request.From,
            request.To,
            request.TableName,
            request.ProgramKind);
        if (vnextResult.Success && vnextResult.Data != null)
        {
            request.SetRuntime(LoadRuntimeState.FromVNextSuccess(
                request.VNextPath, vnextResult.RequestSignature,
                vnextResult.SnapshotSignature, vnextResult.ProgramKind, vnextResult.ProgramSourceSignature));

            var data = vnextResult.Data is List<MetricData> list ? list : vnextResult.Data.ToList();
            request.Cache.SetDataWithCms(cacheKey, data, vnextResult.CmsSeries);
            return new SeriesResolutionResult(data, vnextResult.CmsSeries);
        }

        var (legacyData, legacyCms) = await request.LegacyLoad(
            request.SelectedSeries,
            request.From,
            request.To,
            request.TableName);
        var legacyDataList = legacyData is List<MetricData> legacyList ? legacyList : legacyData.ToList();
        request.Cache.SetDataWithCms(cacheKey, legacyDataList, legacyCms);

        request.SetRuntime(LoadRuntimeState.LegacyFallback(vnextResult.RequestSignature, vnextResult.FailureReason));

        return new SeriesResolutionResult(legacyDataList, legacyCms);
    }
}

internal sealed record SeriesResolutionRequest(
    MetricSeriesSelection? SelectedSeries,
    MetricSeriesSelectionCache Cache,
    string TableName,
    VNextSeriesLoadCoordinator VNextCoordinator,
    ChartProgramKind ProgramKind,
    EvidenceRuntimePath VNextPath,
    Action<LoadRuntimeState> SetRuntime,
    Func<MetricSeriesSelection, DateTime, DateTime, string, Task<(IReadOnlyList<MetricData> Data, ICanonicalMetricSeries? Cms)>> LegacyLoad,
    DateTime From,
    DateTime To,
    IReadOnlyList<MetricData>? PrimaryData,
    IReadOnlyList<MetricData>? SecondaryData,
    ICanonicalMetricSeries? PrimaryCms,
    ICanonicalMetricSeries? SecondaryCms,
    string? MetricType,
    string? PrimaryMetricType,
    string? SecondaryMetricType,
    string? PrimarySubtype,
    string? SecondarySubtype)
{
    public static SeriesResolutionRequest FromContext(
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
        return new SeriesResolutionRequest(
            selectedSeries,
            cache,
            tableName,
            vnextCoordinator,
            programKind,
            vnextPath,
            setRuntime,
            legacyLoad,
            ctx.From,
            ctx.To,
            ctx.Data1,
            ctx.Data2,
            ctx.PrimaryCms as ICanonicalMetricSeries,
            ctx.SecondaryCms as ICanonicalMetricSeries,
            ctx.MetricType,
            ctx.PrimaryMetricType,
            ctx.SecondaryMetricType,
            ctx.PrimarySubtype,
            ctx.SecondarySubtype);
    }
}

internal sealed record SeriesResolutionResult(
    IReadOnlyList<MetricData>? Data,
    ICanonicalMetricSeries? Cms);
