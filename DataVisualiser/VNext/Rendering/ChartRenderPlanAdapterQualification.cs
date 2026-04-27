namespace DataVisualiser.VNext.Rendering;

public sealed record ChartRenderPlanAdapterQualification(
    bool SupportsPlanKind,
    string? RequiredProviderKey,
    string? RequiredBackendKey,
    bool ProviderMatchesAdapter,
    bool BackendMatchesAdapter)
{
    public bool IsQualified => SupportsPlanKind && ProviderMatchesAdapter && BackendMatchesAdapter;
}

public static class ChartRenderPlanAdapterQualificationRules
{
    public static ChartRenderPlanAdapterQualification Evaluate(
        ChartBackendCapabilities capabilities,
        ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(plan);

        var supportsPlanKind = capabilities.Supports(plan.PlanKind);
        var requiredProviderKey = ResolveProviderKey(plan);
        var requiredBackendKey = ResolveBackendKey(plan);
        var providerMatchesAdapter = requiredProviderKey == null ||
            string.Equals(requiredProviderKey, capabilities.BackendKey, StringComparison.OrdinalIgnoreCase);
        var backendMatchesAdapter = requiredBackendKey == null ||
            BackendMatches(capabilities.BackendKey, requiredBackendKey);

        return new ChartRenderPlanAdapterQualification(
            supportsPlanKind,
            requiredProviderKey,
            requiredBackendKey,
            providerMatchesAdapter,
            backendMatchesAdapter);
    }

    public static bool CanRender(
        ChartBackendCapabilities capabilities,
        ChartRenderPlan plan) =>
        Evaluate(capabilities, plan).IsQualified;

    private static string? ResolveProviderKey(ChartRenderPlan plan)
    {
        return plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.ProviderKey, out var providerKey) &&
               !string.IsNullOrWhiteSpace(providerKey)
            ? providerKey
            : null;
    }

    private static string? ResolveBackendKey(ChartRenderPlan plan)
    {
        return plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.BackendKey, out var backendKey) &&
               !string.IsNullOrWhiteSpace(backendKey)
            ? backendKey
            : null;
    }

    private static bool BackendMatches(string adapterBackendKey, string requiredBackendKey)
    {
        return string.Equals(requiredBackendKey, adapterBackendKey, StringComparison.OrdinalIgnoreCase) ||
               requiredBackendKey.StartsWith($"{adapterBackendKey}.", StringComparison.OrdinalIgnoreCase);
    }
}
