using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Evidence;

internal sealed class EvidenceTransformParityDataResolver
{
    private readonly MetricSelectionService _metricSelectionService;

    internal EvidenceTransformParityDataResolver(MetricSelectionService metricSelectionService)
    {
        _metricSelectionService = metricSelectionService ?? throw new ArgumentNullException(nameof(metricSelectionService));
    }

    internal static (MetricSeriesSelection? Primary, MetricSeriesSelection? Secondary) ResolveSelections(ChartState chartState, ChartDataContext ctx)
    {
        var primary = chartState.SelectedTransformPrimarySeries;
        var secondary = chartState.SelectedTransformSecondarySeries;
        if (primary != null || secondary != null)
            return (primary, secondary);

        var primaryMetricType = ctx.PrimaryMetricType ?? ctx.MetricType;
        var primarySelection = string.IsNullOrWhiteSpace(primaryMetricType) ? null : new MetricSeriesSelection(primaryMetricType, ctx.PrimarySubtype);
        MetricSeriesSelection? secondarySelection = null;
        if (!string.IsNullOrWhiteSpace(ctx.SecondaryMetricType))
            secondarySelection = new MetricSeriesSelection(ctx.SecondaryMetricType, ctx.SecondarySubtype);

        return (primarySelection, secondarySelection);
    }

    internal async Task<IReadOnlyList<MetricData>?> ResolveAsync(MetricState? metricState, ChartDataContext ctx, MetricSeriesSelection? selection)
    {
        if (selection == null)
            return null;

        var fromContext = EvidenceDataResolutionHelper.TryResolveFromContext(ctx, selection);
        if (fromContext.Data != null)
            return fromContext.Data;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var tableName = metricState?.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }
}
