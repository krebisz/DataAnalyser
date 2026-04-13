using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

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

        if (ctx.Data1 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.PrimaryMetricType ?? ctx.MetricType, ctx.PrimarySubtype))
            return ctx.Data1;

        if (ctx.Data2 != null && EvidenceDiagnosticsBuilder.IsSameSelection(selection, ctx.SecondaryMetricType, ctx.SecondarySubtype))
            return ctx.Data2;

        if (string.IsNullOrWhiteSpace(selection.MetricType))
            return ctx.Data1;

        var tableName = metricState?.ResolutionTableName ?? DataAccessDefaults.DefaultTableName;
        var (primaryData, _) = await _metricSelectionService.LoadMetricDataAsync(selection.MetricType, selection.QuerySubtype, null, ctx.From, ctx.To, tableName);
        return primaryData.ToList();
    }
}
