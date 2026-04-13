using DataVisualiser.Core.Rendering;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartSeriesLabelFormatter
{
    internal static string FormatSeriesLabel(ChartRenderModel model, bool isPrimary, bool isSmoothed)
    {
        var smoothRaw = isSmoothed ? "smooth" : "raw";

        var primaryMetricType = model.DisplayPrimaryMetricType ?? model.PrimaryMetricType ?? model.MetricType;
        var secondaryMetricType = model.DisplaySecondaryMetricType ?? model.SecondaryMetricType ?? primaryMetricType;
        var primarySubtype = model.DisplayPrimarySubtype ?? model.PrimarySubtype;
        var secondarySubtype = model.DisplaySecondarySubtype ?? model.SecondarySubtype;

        if (!string.IsNullOrEmpty(primaryMetricType))
        {
            if (model.IsOperationChart && !string.IsNullOrEmpty(model.OperationType))
            {
                var operation = model.OperationType;
                var primaryLabel = FormatMetricLabel(primaryMetricType, primarySubtype);
                var secondaryLabel = FormatMetricLabel(secondaryMetricType, secondarySubtype);

                if (isPrimary)
                    return $"{primaryLabel} ({operation}) {secondaryLabel} ({smoothRaw})";

                return $"{secondaryLabel} ({smoothRaw})";
            }

            var subtype = isPrimary ? primarySubtype ?? string.Empty : secondarySubtype ?? string.Empty;
            var metricType = isPrimary ? primaryMetricType : secondaryMetricType;
            var metricLabel = FormatMetricLabel(metricType, subtype);

            return $"{metricLabel} ({smoothRaw})";
        }

        var seriesName = isPrimary ? model.PrimarySeriesName : model.SecondarySeriesName;
        return $"{seriesName} ({smoothRaw})";
    }

    internal static string FormatDateTimeLabel(DateTime dateTime, TickInterval interval)
    {
        return interval switch
        {
                TickInterval.Month => dateTime.ToString("MMM yyyy"),
                TickInterval.Week => dateTime.ToString("MMM dd"),
                TickInterval.Day => dateTime.ToString("MM/dd"),
                TickInterval.Hour => dateTime.ToString("MM/dd HH:mm"),
                _ => dateTime.ToString("MM/dd HH:mm")
        };
    }

    private static string FormatMetricLabel(string? metricType, string? subtype)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            return string.Empty;

        if (string.IsNullOrWhiteSpace(subtype) || subtype == "(All)")
            return metricType;

        return $"{metricType} : {subtype}";
    }
}
