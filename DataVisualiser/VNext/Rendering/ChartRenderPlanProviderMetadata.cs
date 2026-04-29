using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public static class ChartRenderPlanProviderMetadata
{
    public static bool TryAddBuiltInProvider(
        IDictionary<string, string> metadata,
        ConsumerDeliveryContract delivery)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(delivery);

        var planKind = ResolvePlanKind(delivery.ProgramKind);
        return TryAddBuiltInProvider(metadata, delivery, planKind);
    }

    public static bool TryAddBuiltInProvider(
        IDictionary<string, string> metadata,
        ConsumerDeliveryContract delivery,
        ChartRenderPlanKind planKind)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(delivery);

        try
        {
            var provider = ConsumerProviderRegistry.BuiltIn.Resolve(delivery, planKind);
            AddProvider(metadata, provider);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    public static void AddProvider(
        IDictionary<string, string> metadata,
        ConsumerProviderContract provider)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(provider);

        metadata[ChartRenderPlanMetadataKeys.ProviderKey] = provider.ProviderKey;
        metadata[ChartRenderPlanMetadataKeys.ProviderDisplayName] = provider.DisplayName;
        metadata[ChartRenderPlanMetadataKeys.ProviderSignature] = provider.Signature;
    }

    private static ChartRenderPlanKind ResolvePlanKind(ChartProgramKind programKind)
    {
        return programKind == ChartProgramKind.SyncfusionSunburst
            ? ChartRenderPlanKind.Hierarchy
            : ChartRenderPlanKind.Cartesian;
    }
}
