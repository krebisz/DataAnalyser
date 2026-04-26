using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Application;

public sealed record AnalyticalRenderPlanResult(
    AnalyticalExecutionResult Execution,
    ChartRenderPlan RenderPlan);

public sealed record AnalyticalRenderPlanSetResult(
    AnalyticalResultSet ExecutionSet,
    IReadOnlyList<ChartRenderPlan> RenderPlans);

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

    public async Task<AnalyticalExecutionResult> ExecuteAsync(
        AnalyticalIntent intent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        return await _engine.ExecuteAsync(intent, cancellationToken);
    }

    public async Task<AnalyticalResultSet> ExecuteAsync(
        AnalyticalIntentSet intentSet,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);

        return await _engine.ExecuteAsync(intentSet, cancellationToken);
    }

    public async Task<AnalyticalRenderPlanResult> BuildCartesianAsync(
        AnalyticalIntent intent,
        ChartViewport? viewport = null,
        ChartBackendCapabilities? backendCapabilities = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        EnsureRenderPlanRequested(intent);

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
        EnsureRenderPlanRequested(intent);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var renderPlan = _projector.ProjectHierarchy(execution, roots);
        return new AnalyticalRenderPlanResult(execution, renderPlan);
    }

    public async Task<AnalyticalRenderPlanSetResult> BuildCartesianSetAsync(
        IReadOnlyList<AnalyticalIntent> intents,
        ChartViewport? viewport = null,
        ChartBackendCapabilities? backendCapabilities = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intents);

        return await BuildCartesianSetAsync(
            AnalyticalIntentSet.FromIntents(intents),
            viewport,
            backendCapabilities,
            cancellationToken);
    }

    public async Task<AnalyticalRenderPlanSetResult> BuildCartesianSetAsync(
        AnalyticalIntentSet intentSet,
        ChartViewport? viewport = null,
        ChartBackendCapabilities? backendCapabilities = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);

        foreach (var intent in intentSet.Intents)
            EnsureRenderPlanRequested(intent);

        var executionSet = await _engine.ExecuteAsync(intentSet, cancellationToken);
        var renderPlans = executionSet.Results
            .Select(execution =>
            {
                var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
                return _projector.ProjectCartesian(execution, density);
            })
            .ToArray();

        return new AnalyticalRenderPlanSetResult(
            executionSet,
            renderPlans);
    }

    private static void EnsureRenderPlanRequested(AnalyticalIntent intent)
    {
        if (!intent.Delivery.RequiresRenderPlan)
        {
            throw new InvalidOperationException(
                $"Consumer '{intent.Delivery.ConsumerKind}' does not require a render plan. Use ExecuteAsync for non-rendering consumers.");
        }
    }
}
