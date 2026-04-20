using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public sealed class ChartRenderPlanProjector
{
    private readonly TimeBucketRenderAggregationKernel _aggregationKernel;

    private static readonly ChartInteractionPlan DefaultCartesianInteraction = new(
        SupportsZoom: true,
        SupportsPan: true,
        SupportsTooltips: true,
        SupportsSelection: true,
        SupportsViewportRefinement: false);

    private static readonly ChartInteractionPlan DefaultHierarchyInteraction = new(
        SupportsZoom: false,
        SupportsPan: false,
        SupportsTooltips: true,
        SupportsSelection: true,
        SupportsViewportRefinement: false);

    public ChartRenderPlanProjector(TimeBucketRenderAggregationKernel? aggregationKernel = null)
    {
        _aggregationKernel = aggregationKernel ?? new TimeBucketRenderAggregationKernel();
    }

    public ChartRenderPlan ProjectCartesian(ChartProgram program, RenderDensityPlan? density = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        var resolvedDensity = density ?? RenderDensityPlan.FullFidelity(
            program.Series.Sum(item => Math.Min(program.Timeline.Count, item.RawValues.Count)));
        var series = program.Series
            .Select(item => BuildSeriesPlan(program, item, resolvedDensity))
            .ToArray();

        return new ChartRenderPlan(
            BuildPlanId(program),
            program.Kind,
            ChartRenderPlanKind.Cartesian,
            program.DisplayMode,
            program.Title,
            program.From,
            program.To,
            program.SourceSignature,
            series,
            Array.Empty<ChartHierarchyNodePlan>(),
            resolvedDensity with { RenderedPointCount = series.Sum(item => item.RenderedPointCount) },
            DefaultCartesianInteraction,
            new Dictionary<string, string>
            {
                ["Projection"] = "ChartProgram",
                ["ProgramKind"] = program.Kind.ToString()
            });
    }

    public ChartRenderPlan ProjectHierarchy(
        ChartProgram program,
        IReadOnlyList<ChartHierarchyNodePlan> roots,
        RenderDensityPlan? density = null)
    {
        ArgumentNullException.ThrowIfNull(program);
        ArgumentNullException.ThrowIfNull(roots);

        var sourcePointCount = program.Series.Sum(series => series.RawValues.Count);
        var renderedNodeCount = CountNodes(roots);

        return new ChartRenderPlan(
            BuildPlanId(program),
            program.Kind,
            ChartRenderPlanKind.Hierarchy,
            program.DisplayMode,
            program.Title,
            program.From,
            program.To,
            program.SourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            roots,
            density ?? new RenderDensityPlan(
                ChartRenderDensityMode.FullFidelity,
                sourcePointCount,
                renderedNodeCount),
            DefaultHierarchyInteraction,
            new Dictionary<string, string>
            {
                ["Projection"] = "Hierarchy",
                ["ProgramKind"] = program.Kind.ToString()
            });
    }

    private static string BuildPlanId(ChartProgram program) =>
        $"{program.Kind}:{program.SourceSignature}";

    private ChartSeriesPlan BuildSeriesPlan(
        ChartProgram program,
        ChartSeriesProgram item,
        RenderDensityPlan density)
    {
        var buffer = _aggregationKernel.BuildBuffer(item, program.Timeline, density);
        return new ChartSeriesPlan(
            item.Id,
            item.Label,
            program.Timeline,
            item.RawValues,
            item.SmoothedValues,
            buffer,
            SourcePointCount: buffer.SourcePointCount,
            RenderedPointCount: buffer.RenderedPointCount,
            Metadata: new Dictionary<string, string>
            {
                ["ProgramKind"] = program.Kind.ToString(),
                ["DisplayMode"] = program.DisplayMode.ToString()
            });
    }

    private static int CountNodes(IReadOnlyList<ChartHierarchyNodePlan> nodes)
    {
        var count = 0;
        foreach (var node in nodes)
        {
            count++;
            count += CountNodes(node.Children);
        }

        return count;
    }
}
