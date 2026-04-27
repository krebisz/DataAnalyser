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
        {
            var providerDescription = TryReadMetadata(plan, ChartRenderPlanMetadataKeys.ProviderKey, out var providerKey)
                ? $" for provider '{providerKey}'"
                : string.Empty;
            var backendDescription = TryReadMetadata(plan, ChartRenderPlanMetadataKeys.BackendKey, out var backendKey)
                ? $" and backend '{backendKey}'"
                : string.Empty;
            throw new InvalidOperationException(
                $"No chart render adapter supports render plan kind '{plan.PlanKind}'{providerDescription}{backendDescription}.");
        }

        return await adapter.ApplyAsync(surface, plan, cancellationToken);
    }

    private static bool TryReadMetadata(ChartRenderPlan plan, string key, out string value)
    {
        if (plan.Metadata.TryGetValue(key, out var candidate) &&
            !string.IsNullOrWhiteSpace(candidate))
        {
            value = candidate;
            return true;
        }

        value = string.Empty;
        return false;
    }
}
