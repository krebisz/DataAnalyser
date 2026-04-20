using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public sealed record RenderDensityPolicyOptions
{
    public RenderDensityPolicyOptions(
        int FullFidelityPointThreshold = 5_000,
        int OverviewTargetPointCount = 2_000)
    {
        if (FullFidelityPointThreshold < 1)
            throw new ArgumentOutOfRangeException(nameof(FullFidelityPointThreshold));
        if (OverviewTargetPointCount < 1)
            throw new ArgumentOutOfRangeException(nameof(OverviewTargetPointCount));

        this.FullFidelityPointThreshold = FullFidelityPointThreshold;
        this.OverviewTargetPointCount = OverviewTargetPointCount;
    }

    public int FullFidelityPointThreshold { get; }
    public int OverviewTargetPointCount { get; }
}

public sealed class RenderDensityPolicy
{
    private readonly RenderDensityPolicyOptions _options;

    public RenderDensityPolicy(RenderDensityPolicyOptions? options = null)
    {
        _options = options ?? new RenderDensityPolicyOptions();
    }

    public RenderDensityPlan Resolve(
        ChartProgram program,
        ChartViewport? viewport = null,
        ChartBackendCapabilities? backendCapabilities = null)
    {
        ArgumentNullException.ThrowIfNull(program);

        var sourcePointCount = program.Series.Sum(series => Math.Min(program.Timeline.Count, series.RawValues.Count));
        if (sourcePointCount <= _options.FullFidelityPointThreshold)
            return RenderDensityPlan.FullFidelity(sourcePointCount);

        var mode = viewport != null && backendCapabilities?.SupportsViewportRefinement == true
            ? ChartRenderDensityMode.ViewportRefined
            : ChartRenderDensityMode.AggregatedOverview;
        var renderedPointCount = Math.Min(sourcePointCount, _options.OverviewTargetPointCount * Math.Max(1, program.Series.Count));

        return new RenderDensityPlan(
            mode,
            sourcePointCount,
            renderedPointCount,
            _options.OverviewTargetPointCount,
            viewport);
    }
}
