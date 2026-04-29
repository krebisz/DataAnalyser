using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public static class ConsumerProviderContracts
{
    public static ConsumerProviderContract LiveChartsWpf { get; } = new(
        "LiveChartsWpf",
        "LiveCharts WPF",
        ConsumerKind.Chart,
        new HashSet<ChartProgramKind>
        {
            ChartProgramKind.Main,
            ChartProgramKind.Normalized,
            ChartProgramKind.Difference,
            ChartProgramKind.Ratio,
            ChartProgramKind.Transform,
            ChartProgramKind.Distribution,
            ChartProgramKind.WeekdayTrend,
            ChartProgramKind.BarPie
        },
        new HashSet<ChartRenderPlanKind>
        {
            ChartRenderPlanKind.Cartesian,
            ChartRenderPlanKind.Faceted
        },
        new Dictionary<string, string>
        {
            ["ProviderRole"] = "BuiltInChartRenderer"
        });

    public static ConsumerProviderContract SyncfusionSunburst { get; } = new(
        "SyncfusionSunburst",
        "Syncfusion Sunburst",
        ConsumerKind.HierarchyChart,
        new HashSet<ChartProgramKind>
        {
            ChartProgramKind.SyncfusionSunburst
        },
        new HashSet<ChartRenderPlanKind>
        {
            ChartRenderPlanKind.Hierarchy
        },
        new Dictionary<string, string>
        {
            ["ProviderRole"] = "BuiltInHierarchyRenderer"
        });

    public static ConsumerProviderContract TabularSummaryChart { get; } = new(
        "TabularSummaryChart",
        "Tabular Summary Chart",
        ConsumerKind.Chart,
        new HashSet<ChartProgramKind>
        {
            ChartProgramKind.MovingAverage
        },
        new HashSet<ChartRenderPlanKind>
        {
            ChartRenderPlanKind.Cartesian
        },
        new Dictionary<string, string>
        {
            ["ProviderRole"] = "TabularSummaryConsumer"
        });

    public static ConsumerProviderContract EvidenceExport { get; } = new(
        "EvidenceExport",
        "Evidence Export",
        ConsumerKind.Export,
        Enum.GetValues<ChartProgramKind>().ToHashSet(),
        Enum.GetValues<ChartRenderPlanKind>().ToHashSet(),
        new Dictionary<string, string>
        {
            ["ProviderRole"] = "EvidenceConsumer"
        });

    public static ConsumerProviderContract ApiResponse { get; } = new(
        "ApiResponse",
        "API Response",
        ConsumerKind.Api,
        Enum.GetValues<ChartProgramKind>().ToHashSet(),
        Enum.GetValues<ChartRenderPlanKind>().ToHashSet(),
        new Dictionary<string, string>
        {
            ["ProviderRole"] = "ProgrammaticConsumer"
        });
}
