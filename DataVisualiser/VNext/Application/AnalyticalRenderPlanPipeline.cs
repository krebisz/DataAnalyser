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
    private readonly AnalyticalInterpretationBuilder _interpretationBuilder;

    public AnalyticalRenderPlanPipeline(
        IReasoningEngine engine,
        RenderDensityPolicy? densityPolicy = null,
        ChartRenderPlanProjector? projector = null,
        ConsumerProviderRegistry? providerRegistry = null,
        AnalyticalInterpretationBuilder? interpretationBuilder = null)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        _densityPolicy = densityPolicy ?? new RenderDensityPolicy();
        _projector = projector ?? new ChartRenderPlanProjector();
        _providerRegistry = providerRegistry ?? ConsumerProviderRegistry.BuiltIn;
        _interpretationBuilder = interpretationBuilder ?? new AnalyticalInterpretationBuilder();
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

        foreach (var intent in intentSet.Intents)
            ValidateNonRenderingProvider(intent);

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
        var binding = ResolveBinding(intent, ChartRenderPlanKind.Cartesian);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
        var renderPlan = AttachBindingMetadata(_projector.ProjectCartesian(execution, density), binding);
        return new AnalyticalRenderPlanResult(execution, renderPlan);
    }

    public async Task<AnalyticalInterpretationResult> InterpretAsync(
        AnalyticalIntent intent,
        bool includeAverageLines = false,
        bool includeMedianLines = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        var execution = await ExecuteAsync(intent, cancellationToken);
        return _interpretationBuilder.Build(
            execution,
            includeAverageLines,
            includeMedianLines);
    }

    public async Task<AnalyticalInterpretationResult> InterpretAsync(
        AnalyticalIntent intent,
        AnalyticalInterpretationOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);

        var execution = await ExecuteAsync(intent, cancellationToken);
        return _interpretationBuilder.Build(execution, options);
    }

    public async Task<AnalyticalInterpretationSetResult> InterpretSetAsync(
        AnalyticalIntentSet intentSet,
        bool includeAverageLines = false,
        bool includeMedianLines = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);

        var executionSet = await ExecuteAsync(intentSet, cancellationToken);
        return _interpretationBuilder.BuildSet(
            executionSet,
            includeAverageLines,
            includeMedianLines);
    }

    public async Task<AnalyticalInterpretationSetResult> InterpretSetAsync(
        AnalyticalIntentSet intentSet,
        AnalyticalInterpretationOptions? options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);

        var executionSet = await ExecuteAsync(intentSet, cancellationToken);
        return _interpretationBuilder.BuildSet(executionSet, options);
    }

    public async Task<AnalyticalRenderPlanResult> BuildCartesianAsync(
        AnalyticalIntent intent,
        ChartBackendCandidateSet backendCandidates,
        ChartViewport? viewport = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(backendCandidates);
        ArgumentNullException.ThrowIfNull(intent);
        EnsureRenderPlanRequested(intent);
        var binding = ResolveBinding(intent, ChartRenderPlanKind.Cartesian, backendCandidates);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var density = _densityPolicy.Resolve(execution.Program, viewport, binding.Backend);
        var renderPlan = AttachBindingMetadata(_projector.ProjectCartesian(execution, density), binding);
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
        var binding = ResolveBinding(intent, ChartRenderPlanKind.Hierarchy);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var renderPlan = AttachBindingMetadata(_projector.ProjectHierarchy(execution, roots), binding);
        return new AnalyticalRenderPlanResult(execution, renderPlan);
    }

    public async Task<AnalyticalRenderPlanResult> BuildHierarchyAsync(
        AnalyticalIntent intent,
        IReadOnlyList<ChartHierarchyNodePlan> roots,
        ChartBackendCandidateSet backendCandidates,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(backendCandidates);
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(roots);
        EnsureRenderPlanRequested(intent);
        var binding = ResolveBinding(intent, ChartRenderPlanKind.Hierarchy, backendCandidates);

        var execution = await _engine.ExecuteAsync(intent, cancellationToken);
        var renderPlan = AttachBindingMetadata(_projector.ProjectHierarchy(execution, roots), binding);
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

        var bindings = intentSet.Intents
            .Select(intent => ResolveBinding(intent, ChartRenderPlanKind.Cartesian))
            .ToArray();

        var executionSet = await _engine.ExecuteAsync(intentSet, cancellationToken);
        var renderPlans = executionSet.Results
            .Select((execution, index) =>
            {
                var density = _densityPolicy.Resolve(execution.Program, viewport, backendCapabilities);
                return AttachBindingMetadata(_projector.ProjectCartesian(execution, density), bindings[index]);
            })
            .ToArray();

        return new AnalyticalRenderPlanSetResult(
            executionSet,
            renderPlans);
    }

    public async Task<AnalyticalRenderPlanSetResult> BuildCartesianSetAsync(
        AnalyticalIntentSet intentSet,
        ChartBackendCandidateSet backendCandidates,
        ChartViewport? viewport = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intentSet);
        ArgumentNullException.ThrowIfNull(backendCandidates);

        foreach (var intent in intentSet.Intents)
            EnsureRenderPlanRequested(intent);

        var bindings = intentSet.Intents
            .Select(intent => ResolveBinding(intent, ChartRenderPlanKind.Cartesian, backendCandidates))
            .ToArray();

        var executionSet = await _engine.ExecuteAsync(intentSet, cancellationToken);
        var renderPlans = executionSet.Results
            .Select((execution, index) =>
            {
                var density = _densityPolicy.Resolve(execution.Program, viewport, bindings[index].Backend);
                return AttachBindingMetadata(_projector.ProjectCartesian(execution, density), bindings[index]);
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

    private ChartRenderDeliveryBinding ResolveBinding(
        AnalyticalIntent intent,
        ChartRenderPlanKind planKind,
        ChartBackendCandidateSet? backendCandidates = null)
    {
        return ChartRenderDeliveryBinding.Resolve(
            _providerRegistry,
            intent.Delivery,
            planKind,
            backendCandidates);
    }

    private static ChartRenderPlan AttachBindingMetadata(
        ChartRenderPlan renderPlan,
        ChartRenderDeliveryBinding binding)
    {
        var metadata = new Dictionary<string, string>(renderPlan.Metadata);
        binding.AddTo(metadata);

        return renderPlan with { Metadata = metadata };
    }
}
