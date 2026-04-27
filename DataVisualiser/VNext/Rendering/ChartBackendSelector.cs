namespace DataVisualiser.VNext.Rendering;

public sealed class ChartBackendSelector
{
    public ChartBackendCapabilities Select(ChartRenderPlan plan, IReadOnlyList<ChartBackendCapabilities> candidates)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(candidates);

        return new ChartBackendCandidateSet(candidates)
            .Select(plan.PlanKind, ResolveProviderKey(plan));
    }

    private static string? ResolveProviderKey(ChartRenderPlan plan)
    {
        return plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.ProviderKey, out var providerKey) &&
               !string.IsNullOrWhiteSpace(providerKey)
            ? providerKey
            : null;
    }
}
