namespace DataVisualiser.VNext.Rendering;

public sealed record ChartBackendCapabilities(
    string BackendKey,
    string DisplayName,
    IReadOnlySet<ChartRenderPlanKind> SupportedPlanKinds,
    bool SupportsZoom,
    bool SupportsPan,
    bool SupportsTooltips,
    bool SupportsSelection,
    bool SupportsViewportRefinement)
{
    public bool Supports(ChartRenderPlanKind planKind) => SupportedPlanKinds.Contains(planKind);

    public static ChartBackendCapabilities LiveChartsWpf { get; } = new(
        "LiveChartsWpf",
        "LiveCharts WPF",
        new HashSet<ChartRenderPlanKind>
        {
            ChartRenderPlanKind.Cartesian,
            ChartRenderPlanKind.Faceted
        },
        SupportsZoom: true,
        SupportsPan: true,
        SupportsTooltips: true,
        SupportsSelection: true,
        SupportsViewportRefinement: false);

    public static ChartBackendCapabilities SyncfusionSunburst { get; } = new(
        "SyncfusionSunburst",
        "Syncfusion Sunburst",
        new HashSet<ChartRenderPlanKind>
        {
            ChartRenderPlanKind.Hierarchy
        },
        SupportsZoom: false,
        SupportsPan: false,
        SupportsTooltips: true,
        SupportsSelection: true,
        SupportsViewportRefinement: false);
}
