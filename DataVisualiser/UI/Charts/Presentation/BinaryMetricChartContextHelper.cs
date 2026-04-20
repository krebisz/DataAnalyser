using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class BinaryMetricChartContextHelper
{
    public static Task RerenderIfVisibleAsync(
        bool isVisible,
        ChartDataContext? context,
        Func<ChartDataContext, Task> render)
    {
        return MetricSeriesSelectionAdapterHelper.RerenderIfVisibleAsync(isVisible, context, render);
    }

    public static ChartDataContext BuildContext(
        ChartDataContext source,
        MetricSeriesSelection? primarySelection,
        MetricSeriesSelection? secondarySelection,
        IReadOnlyList<MetricData>? primaryData,
        IReadOnlyList<MetricData>? secondaryData,
        ICanonicalMetricSeries? primaryCms,
        ICanonicalMetricSeries? secondaryCms,
        string displayName1,
        string displayName2)
    {
        return new ChartDataContext
        {
            Data1 = primaryData,
            Data2 = secondaryData,
            PrimaryCms = primaryCms,
            SecondaryCms = secondaryCms,
            DisplayName1 = displayName1,
            DisplayName2 = displayName2,
            MetricType = primarySelection?.MetricType ?? source.MetricType,
            PrimaryMetricType = primarySelection?.MetricType ?? source.PrimaryMetricType,
            PrimarySubtype = primarySelection?.Subtype,
            SecondaryMetricType = secondarySelection?.MetricType ?? source.SecondaryMetricType,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = primarySelection?.DisplayMetricType ?? source.DisplayPrimaryMetricType,
            DisplayPrimarySubtype = primarySelection?.DisplaySubtype ?? source.DisplayPrimarySubtype,
            DisplaySecondaryMetricType = secondarySelection?.DisplayMetricType ?? source.DisplaySecondaryMetricType,
            DisplaySecondarySubtype = secondarySelection?.DisplaySubtype ?? source.DisplaySecondarySubtype,
            ActualSeriesCount = secondaryData == null ? 1 : 2,
            From = source.From,
            To = source.To
        };
    }
}
