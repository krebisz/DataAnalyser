using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

public static class ChartContextSelectionGuard
{
    public static bool IsCompatibleWithCurrentSelection(
        ChartDataContext? context,
        string? selectedMetricType,
        IReadOnlyList<MetricSeriesSelection>? selectedSeries,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? resolutionTableName = null)
    {
        if (context?.Data1 == null || !context.Data1.Any())
            return false;

        if (selectedSeries == null || selectedSeries.Count == 0)
            return false;

        var effectiveSelections = selectedSeries
            .Where(selection => !string.IsNullOrWhiteSpace(selection.QuerySubtype))
            .ToList();
        if (effectiveSelections.Count == 0)
            return false;

        if (!string.IsNullOrWhiteSpace(context.LoadRequestSignature) &&
            fromDate.HasValue &&
            toDate.HasValue &&
            !string.IsNullOrWhiteSpace(resolutionTableName))
        {
            return string.Equals(
                context.LoadRequestSignature,
                BuildSelectionSignature(selectedMetricType, effectiveSelections, fromDate.Value, toDate.Value, resolutionTableName),
                StringComparison.Ordinal);
        }

        var effectiveMetricType = string.IsNullOrWhiteSpace(selectedMetricType)
            ? effectiveSelections[0].MetricType
            : selectedMetricType;

        if (string.IsNullOrWhiteSpace(effectiveMetricType))
            return false;

        var contextMetricType = context.PrimaryMetricType ?? context.MetricType;
        if (!string.Equals(effectiveMetricType, contextMetricType, StringComparison.OrdinalIgnoreCase))
            return false;

        if (effectiveSelections.Count == 1)
            return IsMatchingSelection(effectiveSelections[0], context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype);

        if (effectiveSelections.Count == 2)
        {
            return IsMatchingSelection(effectiveSelections[0], context.PrimaryMetricType ?? context.MetricType, context.PrimarySubtype) &&
                   IsMatchingSelection(effectiveSelections[1], context.SecondaryMetricType, context.SecondarySubtype);
        }

        return false;
    }

    private static bool IsMatchingSelection(MetricSeriesSelection selection, string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(selection.MetricType))
            return false;

        if (!string.Equals(selection.MetricType, metricType, StringComparison.OrdinalIgnoreCase))
            return false;

        return string.Equals(selection.QuerySubtype ?? string.Empty, subtype ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildSelectionSignature(
        string? selectedMetricType,
        IReadOnlyList<MetricSeriesSelection> selectedSeries,
        DateTime fromDate,
        DateTime toDate,
        string resolutionTableName)
    {
        var metricType = string.IsNullOrWhiteSpace(selectedMetricType)
            ? selectedSeries[0].MetricType
            : selectedMetricType;
        var orderedSeries = string.Join(
            "|",
            selectedSeries.Select(series => $"{series.MetricType}:{series.QuerySubtype ?? "<none>"}"));

        return $"{metricType ?? "<none>"}::{resolutionTableName}::{fromDate:O}->{toDate:O}::{orderedSeries}";
    }

    public static bool HasRenderableContext(ChartDataContext? context, string? selectedMetricType)
    {
        if (context?.Data1 == null || context.Data1.Count == 0)
            return false;

        var contextMetric = context.PrimaryMetricType ?? context.MetricType;
        return string.Equals(contextMetric, selectedMetricType, StringComparison.OrdinalIgnoreCase);
    }
}
