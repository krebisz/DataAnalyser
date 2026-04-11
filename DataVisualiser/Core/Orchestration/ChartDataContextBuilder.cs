using DataFileReader.Canonical;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

/// <summary>
///     Builds a fully aligned, fully derived ChartDataContext from raw
///     MetricData series.
/// </summary>
public sealed class ChartDataContextBuilder
{
    private const int SmoothWindow = ComputationDefaults.SmoothingWindow;

    public ChartDataContext Build(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, DateTime from, DateTime to)
    {
        var list1 = data1?.ToList() ?? new List<MetricData>();
        var list2 = data2?.ToList() ?? new List<MetricData>();
        var prepared = ChartDataSeriesPreparationHelper.Prepare(list1, list2, SmoothWindow);
        var (display1, display2) = BuildDisplayNames(primarySelection, secondarySelection);

        return new ChartDataContext
        {
            Data1 = list1,
            Data2 = list2,
            Timestamps = prepared.Timestamps,
            RawValues1 = prepared.RawValues1,
            RawValues2 = prepared.RawValues2,
            SmoothedValues1 = prepared.SmoothedValues1,
            SmoothedValues2 = prepared.SmoothedValues2,
            DifferenceValues = prepared.DifferenceValues,
            RatioValues = prepared.RatioValues,
            NormalizedValues1 = prepared.NormalizedValues1,
            NormalizedValues2 = prepared.NormalizedValues2,
            DisplayName1 = display1,
            DisplayName2 = display2,
            MetricType = primarySelection.MetricType,
            PrimaryMetricType = primarySelection.MetricType,
            SecondaryMetricType = secondarySelection?.MetricType,
            PrimarySubtype = primarySelection.Subtype,
            SecondarySubtype = secondarySelection?.Subtype,
            DisplayPrimaryMetricType = string.IsNullOrWhiteSpace(primarySelection.DisplayMetricType) ? primarySelection.MetricType : primarySelection.DisplayMetricType,
            DisplaySecondaryMetricType = secondarySelection == null ? null : string.IsNullOrWhiteSpace(secondarySelection.DisplayMetricType) ? secondarySelection.MetricType : secondarySelection.DisplayMetricType,
            DisplayPrimarySubtype = string.IsNullOrWhiteSpace(primarySelection.DisplaySubtype) ? primarySelection.Subtype : primarySelection.DisplaySubtype,
            DisplaySecondarySubtype = secondarySelection == null ? null : string.IsNullOrWhiteSpace(secondarySelection.DisplaySubtype) ? secondarySelection.Subtype : secondarySelection.DisplaySubtype,
            From = from,
            To = to,
            ActualSeriesCount = secondarySelection == null ? 1 : 2
        };
    }

    public ChartDataContext Build(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection, IEnumerable<MetricData> data1, IEnumerable<MetricData>? data2, DateTime from, DateTime to, ICanonicalMetricSeries? primaryCms, ICanonicalMetricSeries? secondaryCms)
    {
        var ctx = Build(primarySelection, secondarySelection, data1, data2, from, to);
        ctx.PrimaryCms = primaryCms;
        ctx.SecondaryCms = secondaryCms;
        ctx.CmsSeries = BuildCmsSeries(primaryCms, secondaryCms);
        return ctx;
    }

    private static IReadOnlyList<ICanonicalMetricSeries>? BuildCmsSeries(ICanonicalMetricSeries? primaryCms, ICanonicalMetricSeries? secondaryCms)
    {
        var series = new List<ICanonicalMetricSeries>(2);

        if (primaryCms != null)
            series.Add(primaryCms);

        if (secondaryCms != null)
            series.Add(secondaryCms);

        return series.Count > 0 ? series : null;
    }

    private static (string DisplayName1, string DisplayName2) BuildDisplayNames(MetricSeriesSelection primarySelection, MetricSeriesSelection? secondarySelection)
    {
        var display1 = primarySelection.DisplayName;
        var display2 = secondarySelection?.DisplayName ?? primarySelection.DisplayName;

        return (display1, display2);
    }
}
