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
    private readonly ConsumerProviderRegistry _providerRegistry;

    public AnalyticalRenderPlanPipeline(
        IReasoningEngine engine,
        RenderDensityPolicy? densityPolicy = null,
        ChartRenderPlanProjector? projector = null,
        ConsumerProviderRegistry? providerRegistry = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _densityPolicy = densityPolicy ?? new RenderDensityPolicy();
        _projector = projector ?? new ChartRenderPlanProjector();
        _providerRegistry = providerRegistry ?? ConsumerProviderRegistry.BuiltIn;
    }

    public async Task<AnalyticalExecutionResult> ExecuteAsync(
        AnalyticalIntent intent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        ValidateNonRenderingProvider(intent);

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
        var provider = _providerRegistry.Resolve(intent.Delivery, ChartRenderPlanKind.Cartesian);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
        var renderPlan = AttachProviderMetadata(_projector.ProjectCartesian(execution, density), provider);
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
        var provider = _providerRegistry.Resolve(intent.Delivery, ChartRenderPlanKind.Hierarchy);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var renderPlan = AttachProviderMetadata(_projector.ProjectHierarchy(execution, roots), provider);
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

        var providers = intentSet.Intents
            .Select(intent => _providerRegistry.Resolve(intent.Delivery, ChartRenderPlanKind.Cartesian))
            .ToArray();

        var executionSet = await _engine.ExecuteAsync(intentSet, cancellationToken);
        var renderPlans = executionSet.Results
            .Select((execution, index) =>
            {
                var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
                return AttachProviderMetadata(_projector.ProjectCartesian(execution, density), providers[index]);
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

    private void ValidateNonRenderingProvider(AnalyticalIntent intent)
    {
        if (!intent.Delivery.RequiresRenderPlan)
            _providerRegistry.Resolve(intent.Delivery);
    }

    private static ChartRenderPlan AttachProviderMetadata(
        ChartRenderPlan renderPlan,
        ConsumerProviderContract provider)
    {
        var metadata = new Dictionary<string, string>(renderPlan.Metadata);
        ChartRenderPlanProviderMetadata.AddProvider(metadata, provider);

        return renderPlan with { Metadata = metadata };
    }
}
