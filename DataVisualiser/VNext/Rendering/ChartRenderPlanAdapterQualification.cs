namespace DataVisualiser.VNext.Rendering;

public sealed record ChartRenderPlanAdapterQualification(
    bool SupportsPlanKind,
    string? RequiredProviderKey,
    bool ProviderMatchesAdapter)
{
    public bool IsQualified => SupportsPlanKind && ProviderMatchesAdapter;
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
        var providerMatchesAdapter = requiredProviderKey == null ||
            string.Equals(requiredProviderKey, capabilities.BackendKey, StringComparison.OrdinalIgnoreCase);

        return new ChartRenderPlanAdapterQualification(
            supportsPlanKind,
            requiredProviderKey,
            providerMatchesAdapter);
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
}
