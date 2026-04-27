namespace DataVisualiser.VNext.Rendering;

public sealed class ChartBackendSelector
{
    public ChartBackendCapabilities Select(ChartRenderPlan plan, IReadOnlyList<ChartBackendCapabilities> candidates)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(candidates);

        try
        {
            return new ChartBackendCandidateSet(candidates)
                .Select(plan.PlanKind, ResolveProviderKey(plan));
        }
        catch (InvalidOperationException)
        {
            var providerDescription = plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.ProviderKey, out var providerKey) &&
                                      !string.IsNullOrWhiteSpace(providerKey)
                ? $" for provider '{providerKey}'"
                : string.Empty;
            throw new InvalidOperationException(
                $"No chart backend supports render plan kind '{plan.PlanKind}'{providerDescription}.");
        }
    }

    private static string? ResolveProviderKey(ChartRenderPlan plan)
    {
        return plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.ProviderKey, out var providerKey) &&
               !string.IsNullOrWhiteSpace(providerKey)
            ? providerKey
            : null;
    }
}
