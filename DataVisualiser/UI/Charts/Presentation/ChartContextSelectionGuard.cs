using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

public static class ChartContextSelectionGuard
{
    public static bool IsCompatibleWithCurrentSelection(
        ChartDataContext? context,
        string? selectedMetricType,
        IReadOnlyList<MetricSeriesSelection>? selectedSeries)
    {
        if (context?.Data1 == null || !context.Data1.Any())
            return false;

        if (selectedSeries == null || selectedSeries.Count == 0)
            return false;

        var effectiveMetricType = string.IsNullOrWhiteSpace(selectedMetricType)
            ? selectedSeries[0].MetricType
            : selectedMetricType;

        if (string.IsNullOrWhiteSpace(effectiveMetricType))
            return false;

        var contextMetricType = context.PrimaryMetricType ?? context.MetricType;
        if (!string.Equals(effectiveMetricType, contextMetricType, StringComparison.OrdinalIgnoreCase))
            return false;

        return selectedSeries.All(selection =>
            !string.IsNullOrWhiteSpace(selection.QuerySubtype) &&
            string.Equals(selection.MetricType, effectiveMetricType, StringComparison.OrdinalIgnoreCase));
    }
}
