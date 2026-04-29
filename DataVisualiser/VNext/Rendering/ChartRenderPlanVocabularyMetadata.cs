using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Rendering;

public static class ChartRenderPlanVocabularyMetadata
{
    public static IReadOnlyDictionary<string, string> Build(
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery,
        string sourceSignature,
        int overlayCount = 0,
        int interactionCount = 0)
    {
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(delivery);

        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("Delivery program kind must match the render-plan program request.", nameof(delivery));

        if (string.IsNullOrWhiteSpace(sourceSignature))
            sourceSignature = "<unknown>";

        var provenance = ProvenanceDescriptor.Projected(sourceSignature, AnalyticalAuthority.Legacy);
        var intentSignature = string.Join("::",
            sourceSignature,
            programRequest.Kind,
            capability.Signature,
            delivery.Signature,
            provenance.Signature);

        var metadata = new Dictionary<string, string>
        {
            [ChartRenderPlanMetadataKeys.IntentSignature] = intentSignature,
            [ChartRenderPlanMetadataKeys.ProvenanceSignature] = provenance.Signature,
            [ChartRenderPlanMetadataKeys.ConsumerKind] = delivery.ConsumerKind.ToString(),
            [ChartRenderPlanMetadataKeys.DeliveryTarget] = delivery.DeliveryTarget,
            [ChartRenderPlanMetadataKeys.CapabilityKind] = capability.CapabilityKind.ToString(),
            [ChartRenderPlanMetadataKeys.CompositionKind] = capability.CompositionKind.ToString(),
            [ChartRenderPlanMetadataKeys.OverlayCount] = overlayCount.ToString(),
            [ChartRenderPlanMetadataKeys.InteractionCount] = interactionCount.ToString()
        };

        ChartRenderPlanProviderMetadata.TryAddBuiltInProvider(metadata, delivery);
        return metadata;
    }

    public static void AddTo(
        IDictionary<string, string> metadata,
        ChartProgramRequest programRequest,
        CapabilityRequest capability,
        ConsumerDeliveryContract delivery,
        string sourceSignature,
        int overlayCount = 0,
        int interactionCount = 0)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        foreach (var pair in Build(programRequest, capability, delivery, sourceSignature, overlayCount, interactionCount))
            metadata[pair.Key] = pair.Value;
    }
}
