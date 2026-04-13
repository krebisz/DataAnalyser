using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Helpers;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class EvidenceMultiMetricParityEvaluator
{
    private readonly Func<IStrategyCutOverService?> _getStrategyCutOverService;
    private readonly MetricSelectionService _metricSelectionService;

    internal EvidenceMultiMetricParityEvaluator(
        MetricSelectionService metricSelectionService,
        Func<IStrategyCutOverService?> getStrategyCutOverService)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
        _getStrategyCutOverService = getStrategyCutOverService ?? throw new ArgumentNullException(nameof(getStrategyCutOverService));
    }

    internal async Task<SimpleParitySnapshot> BuildAsync(MetricState metricState, ChartDataContext? ctx)
    {
        if (ctx == null || ctx.Data1 == null)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Primary series required" };

        var selectedSeries = metricState.SelectedSeries
            .GroupBy(series => series.DisplayKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        if (selectedSeries.Count < 3)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "At least three series required" };

        var tableName = metricState.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var resolved = await ResolveInputsAsync(ctx, selectedSeries, tableName);
        if (resolved.LegacySeries.Count < 3)
            return new SimpleParitySnapshot { Status = "Unavailable", Reason = "Insufficient multi-series legacy data available" };

        if (resolved.CmsSeries.Count != resolved.LegacySeries.Count)
            return new SimpleParitySnapshot { Status = "CmsUnavailable", Reason = "CMS multi-series data missing" };

        var parityContext = new ChartDataContext
        {
            PrimaryCms = resolved.CmsSeries.FirstOrDefault(),
            SecondaryCms = resolved.CmsSeries.Count > 1 ? resolved.CmsSeries[1] : null,
            CmsSeries = resolved.CmsSeries,
            Data1 = resolved.LegacySeries[0].ToList(),
            Data2 = resolved.LegacySeries.Count > 1 ? resolved.LegacySeries[1].ToList() : Array.Empty<MetricData>(),
            DisplayName1 = resolved.Labels[0],
            DisplayName2 = resolved.Labels.Count > 1 ? resolved.Labels[1] : string.Empty,
            ActualSeriesCount = resolved.LegacySeries.Count,
            From = ctx.From,
            To = ctx.To
        };
        var parameters = new StrategyCreationParameters
        {
            LegacySeries = resolved.LegacySeries,
            CmsSeries = resolved.CmsSeries,
            Labels = resolved.Labels,
            From = ctx.From,
            To = ctx.To
        };

        return new SimpleParitySnapshot
        {
            Status = "Completed",
            Result = EvidenceStrategyParityExecutor.ExecuteSafe(ResolveStrategyCutOverService(), StrategyType.MultiMetric, parityContext, parameters)
        };
    }

    private IStrategyCutOverService ResolveStrategyCutOverService()
    {
        return EvidenceDataResolutionHelper.ResolveStrategyCutOverService(_getStrategyCutOverService);
    }

    private async Task<(List<IEnumerable<MetricData>> LegacySeries, List<ICanonicalMetricSeries> CmsSeries, List<string> Labels)> ResolveInputsAsync(
        ChartDataContext ctx,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        string tableName)
    {
        var legacySeries = new List<IEnumerable<MetricData>>();
        var cmsSeries = new List<ICanonicalMetricSeries>();
        var labels = new List<string>();

        foreach (var selection in selectedSeries)
        {
            var label = string.IsNullOrWhiteSpace(selection.DisplayName) ? selection.DisplayKey : selection.DisplayName;
            var (legacyData, cmsData) = await ResolveSeriesAsync(ctx, selection, tableName);
            if (legacyData == null || legacyData.Count == 0)
                continue;

            legacySeries.Add(legacyData);
            labels.Add(label);
            if (cmsData != null)
                cmsSeries.Add(cmsData);
        }

        return (legacySeries, cmsSeries, labels);
    }

    private async Task<(IReadOnlyList<MetricData>? Data, ICanonicalMetricSeries? Cms)> ResolveSeriesAsync(
        ChartDataContext ctx,
        MetricSeriesSelection selection,
        string tableName)
    {
        var fromContext = EvidenceDataResolutionHelper.TryResolveFromContext(ctx, selection);
        if (fromContext.Data != null)
            return (fromContext.Data, fromContext.Cms);

        if (ctx.CmsSeries != null)
        {
            var targetMetricId = CanonicalMetricMapping.FromLegacyFields(selection.MetricType, selection.QuerySubtype);
            var matchingCms = ctx.CmsSeries.FirstOrDefault(series => string.Equals(series.MetricId?.Value, targetMetricId, StringComparison.OrdinalIgnoreCase));
            if (matchingCms != null)
            {
                var legacyData = await ResolveLegacyDataForSelectionAsync(ctx, selection, tableName);
                return (legacyData, matchingCms);
            }
        }

        var (primaryCms, _, primaryData, _) = await _metricSelectionService.LoadMetricDataWithCmsAsync(selection, null, ctx.From, ctx.To, tableName);
        return (primaryData.ToList(), primaryCms);
    }

    private async Task<IReadOnlyList<MetricData>?> ResolveLegacyDataForSelectionAsync(ChartDataContext ctx, MetricSeriesSelection selection, string tableName)
    {
        var fromContext = EvidenceDataResolutionHelper.TryResolveFromContext(ctx, selection);
        if (fromContext.Data != null)
            return fromContext.Data;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }
}
