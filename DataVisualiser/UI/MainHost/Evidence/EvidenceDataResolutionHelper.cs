using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.MainHost.Evidence;

/// <summary>
///     Shared data resolution patterns used by evidence evaluators.
///     Eliminates duplication of the "check Data1, check Data2, fall back to service" pattern.
/// </summary>
internal static class EvidenceDataResolutionHelper
{
    internal static (IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms, string DataSource)
        TryResolveFromContext(ChartDataContext ctx, MetricSeriesSelection selection)
    {
        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Primary");

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return (ctx.Data2, ctx.SecondaryCms as ICanonicalMetricSeries, "ChartContext.Secondary");

        return (null, null, string.Empty);
    }

    internal static async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms, string DataSource)>
        ResolveWithFallbackAsync(
            ChartDataContext ctx,
            MetricSeriesSelection selection,
            MetricSelectionService metricSelectionService,
            string tableName)
    {
        var fromContext = TryResolveFromContext(ctx, selection);
        if (fromContext.Data != null)
            return fromContext;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return (ctx.Data1, ctx.PrimaryCms as ICanonicalMetricSeries, "ChartContext.Fallback");

        var (primaryCms, _, primaryData, _) = await metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms, "MetricSelectionService");
    }

    internal static IStrategyCutOverService ResolveStrategyCutOverService(Func<IStrategyCutOverService?> getStrategyCutOverService)
    {
        return getStrategyCutOverService() ?? new StrategyCutOverService(new DataPreparationService(), StrategyReachabilityStoreProbe.Default);
    }
}
