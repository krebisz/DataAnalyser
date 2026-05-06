using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.State;

public sealed record LoadedChartDataSnapshot(
    bool Present,
    string? LoadRequestSignature,
    string? MetricType,
    string? PrimaryMetricType,
    string? SecondaryMetricType,
    string? PrimarySubtype,
    string? SecondarySubtype,
    string? DisplayName1,
    string? DisplayName2,
    int ActualSeriesCount,
    int Data1Count,
    int Data2Count,
    int CmsSeriesCount,
    DateTime? From,
    DateTime? To,
    string? ContextSignature)
{
    public static LoadedChartDataSnapshot Empty { get; } = new(
        Present: false,
        LoadRequestSignature: null,
        MetricType: null,
        PrimaryMetricType: null,
        SecondaryMetricType: null,
        PrimarySubtype: null,
        SecondarySubtype: null,
        DisplayName1: null,
        DisplayName2: null,
        ActualSeriesCount: 0,
        Data1Count: 0,
        Data2Count: 0,
        CmsSeriesCount: 0,
        From: null,
        To: null,
        ContextSignature: null);

    public static LoadedChartDataSnapshot FromContext(ChartDataContext? context)
    {
        if (context == null)
            return Empty;

        return new LoadedChartDataSnapshot(
            Present: true,
            LoadRequestSignature: context.LoadRequestSignature,
            MetricType: context.MetricType,
            PrimaryMetricType: context.PrimaryMetricType,
            SecondaryMetricType: context.SecondaryMetricType,
            PrimarySubtype: context.PrimarySubtype,
            SecondarySubtype: context.SecondarySubtype,
            DisplayName1: context.DisplayName1,
            DisplayName2: context.DisplayName2,
            ActualSeriesCount: context.ActualSeriesCount,
            Data1Count: context.Data1?.Count ?? 0,
            Data2Count: context.Data2?.Count ?? 0,
            CmsSeriesCount: context.CmsSeries?.Count ?? 0,
            From: context.From,
            To: context.To,
            ContextSignature: BuildContextSignature(context));
    }

    private static string BuildContextSignature(ChartDataContext context)
    {
        var metricType = context.PrimaryMetricType ?? context.MetricType ?? "<none>";
        var secondaryMetricType = context.SecondaryMetricType ?? "<none>";
        var primarySubtype = context.PrimarySubtype ?? "<none>";
        var secondarySubtype = context.SecondarySubtype ?? "<none>";

        return $"{metricType}:{primarySubtype}|{secondaryMetricType}:{secondarySubtype}::{context.From:O}->{context.To:O}::series={context.ActualSeriesCount}";
    }
}
