using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Application;

public sealed record AnalyticalRenderPlanResult(
    AnalyticalExecutionResult Execution,
    ChartRenderPlan RenderPlan);

public sealed class AnalyticalRenderPlanPipeline
{
    private readonly IReasoningEngine _engine;
    private readonly RenderDensityPolicy _densityPolicy;
    private readonly ChartRenderPlanProjector _projector;

    public AnalyticalRenderPlanPipeline(
        IReasoningEngine engine,
        RenderDensityPolicy? densityPolicy = null,
        ChartRenderPlanProjector? projector = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _densityPolicy = densityPolicy ?? new RenderDensityPolicy();
        _projector = projector ?? new ChartRenderPlanProjector();
    }

    public async Task<AnalyticalRenderPlanResult> BuildCartesianAsync(
        AnalyticalIntent intent,
        ChartViewport? viewport = null,
        ChartBackendCapabilities? backendCapabilities = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
        var renderPlan = _projector.ProjectCartesian(execution, density);
        return new AnalyticalRenderPlanResult(execution, renderPlan);
    }

    public async Task<AnalyticalRenderPlanResult> BuildHierarchyAsync(
        AnalyticalIntent intent,
        IReadOnlyList<ChartHierarchyNodePlan> roots,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(roots);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var renderPlan = _projector.ProjectHierarchy(execution, roots);
        return new AnalyticalRenderPlanResult(execution, renderPlan);
    }
}
