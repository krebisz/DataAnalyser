namespace DataVisualiser.VNext.Rendering;

public sealed record ChartRenderAdapterResult(
    string BackendKey,
    string PlanId,
    ChartRenderPlanKind PlanKind,
    ChartRenderDensityMode DensityMode,
    int RenderedSeriesCount,
    int RenderedHierarchyNodeCount,
    int RenderedPointCount,
    IReadOnlyDictionary<string, string> Metadata);

public interface IChartRenderPlanAdapter<TSurface>
{
    ChartBackendCapabilities Capabilities { get; }

    bool CanRender(ChartRenderPlan plan);

    ValueTask<ChartRenderAdapterResult> ApplyAsync(
        TSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default);
}

public sealed class ChartRenderPlanAdapterDispatcher<TSurface>
{
    private readonly IReadOnlyList<IChartRenderPlanAdapter<TSurface>> _adapters;

    public ChartRenderPlanAdapterDispatcher(IReadOnlyList<IChartRenderPlanAdapter<TSurface>> adapters)
    {
        _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
    }

    public async ValueTask<ChartRenderAdapterResult> ApplyAsync(
        TSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);

        var adapter = _adapters.FirstOrDefault(candidate => candidate.CanRender(plan));
        if (adapter == null)
            throw new InvalidOperationException($"No chart render adapter supports render plan kind '{plan.PlanKind}'.");

        return await adapter.ApplyAsync(surface, plan, cancellationToken);
    }
}
