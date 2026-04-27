using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public sealed record ChartRenderDeliveryBinding(
    ConsumerDeliveryContract Delivery,
    ChartRenderPlanKind PlanKind,
    ConsumerProviderContract Provider,
    ChartBackendCapabilities? Backend)
{
    public void AddTo(IDictionary<string, string> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        ChartRenderPlanProviderMetadata.AddProvider(metadata, Provider);

        if (Backend == null)
            return;

        metadata[ChartRenderPlanMetadataKeys.BackendKey] = Backend.BackendKey;
        metadata[ChartRenderPlanMetadataKeys.BackendDisplayName] = Backend.DisplayName;
    }

    public static ChartRenderDeliveryBinding Resolve(
        ConsumerProviderRegistry providerRegistry,
        ConsumerDeliveryContract delivery,
        ChartRenderPlanKind planKind,
        ChartBackendCandidateSet? backendCandidates = null)
    {
        ArgumentNullException.ThrowIfNull(providerRegistry);
        ArgumentNullException.ThrowIfNull(delivery);

        var provider = providerRegistry.Resolve(delivery, planKind);
        var backend = backendCandidates?.Select(planKind, provider.ProviderKey);

        return new ChartRenderDeliveryBinding(
            delivery,
            planKind,
            provider,
            backend);
    }
}
