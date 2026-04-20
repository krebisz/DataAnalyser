using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public enum ChartRenderPlanKind
{
    Cartesian = 0,
    Hierarchy = 1,
    Faceted = 2
}

public enum ChartRenderDensityMode
{
    FullFidelity = 0,
    AggregatedOverview = 1,
    ViewportRefined = 2
}

public sealed record ChartViewport(DateTime? From, DateTime? To);

public sealed record RenderDensityPlan(
    ChartRenderDensityMode Mode,
    int SourcePointCount,
    int RenderedPointCount,
    int? BucketCount = null,
    ChartViewport? Viewport = null)
{
    public static RenderDensityPlan FullFidelity(int pointCount) =>
        new(ChartRenderDensityMode.FullFidelity, pointCount, pointCount);
}

public sealed record ChartInteractionPlan(
    bool SupportsZoom,
    bool SupportsPan,
    bool SupportsTooltips,
    bool SupportsSelection,
    bool SupportsViewportRefinement);

public sealed record RenderDataPoint(
    DateTime X,
    double? Y,
    double? Minimum,
    double? Maximum,
    int SourcePointCount);

public sealed record RenderDataBuffer(
    string SeriesId,
    string Label,
    IReadOnlyList<RenderDataPoint> Points,
    int SourcePointCount,
    int RenderedPointCount,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ChartSeriesPlan(
    string Id,
    string Label,
    IReadOnlyList<DateTime> Timeline,
    IReadOnlyList<double> RawValues,
    IReadOnlyList<double> SmoothedValues,
    RenderDataBuffer RenderBuffer,
    int SourcePointCount,
    int RenderedPointCount,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ChartHierarchyNodePlan(
    string Id,
    string Label,
    double Value,
    IReadOnlyList<ChartHierarchyNodePlan> Children,
    IReadOnlyDictionary<string, string> Metadata);

public sealed record ChartRenderPlan(
    string Id,
    ChartProgramKind ProgramKind,
    ChartRenderPlanKind PlanKind,
    ChartDisplayMode DisplayMode,
    string Title,
    DateTime From,
    DateTime To,
    string SourceSignature,
    IReadOnlyList<ChartSeriesPlan> Series,
    IReadOnlyList<ChartHierarchyNodePlan> HierarchyRoots,
    RenderDensityPlan Density,
    ChartInteractionPlan Interaction,
    IReadOnlyDictionary<string, string> Metadata);
