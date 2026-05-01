using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public sealed record VNextUiConsumptionContract
{
    public VNextUiConsumptionContract(
        ChartProgramKind programKind,
        AnalyticalCapabilityKind capabilityKind,
        CompositionKind compositionKind,
        ConsumerDeliveryContract delivery,
        ConsumerProviderContract provider,
        string sourceSignature,
        string intentSignature,
        string provenanceSignature,
        ConsumerSurfaceModel surfaceModel,
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(surfaceModel);

        if (delivery.ProgramKind != programKind)
            throw new ArgumentException("Delivery program kind must match the consumption contract program kind.", nameof(delivery));
        if (!provider.Supports(delivery))
            throw new ArgumentException("Provider must support the delivery contract.", nameof(provider));
        if (surfaceModel.RequiresRenderPlan && !delivery.RequiresRenderPlan)
            throw new ArgumentException("A render-plan surface cannot be attached to a non-render-plan delivery.", nameof(surfaceModel));
        if (string.IsNullOrWhiteSpace(sourceSignature))
            throw new ArgumentException("Source signature cannot be null or empty.", nameof(sourceSignature));
        if (string.IsNullOrWhiteSpace(intentSignature))
            throw new ArgumentException("Intent signature cannot be null or empty.", nameof(intentSignature));
        if (string.IsNullOrWhiteSpace(provenanceSignature))
            throw new ArgumentException("Provenance signature cannot be null or empty.", nameof(provenanceSignature));

        ProgramKind = programKind;
        CapabilityKind = capabilityKind;
        CompositionKind = compositionKind;
        Delivery = delivery;
        Provider = provider;
        SourceSignature = sourceSignature;
        IntentSignature = intentSignature;
        ProvenanceSignature = provenanceSignature;
        SurfaceModel = surfaceModel;
        Overlays = overlays?.ToArray() ?? [];
        Interactions = interactions?.ToArray() ?? [];
        Metadata = BuildMetadata(delivery, provider, surfaceModel, metadata);
    }

    public ChartProgramKind ProgramKind { get; }
    public AnalyticalCapabilityKind CapabilityKind { get; }
    public CompositionKind CompositionKind { get; }
    public ConsumerDeliveryContract Delivery { get; }
    public ConsumerProviderContract Provider { get; }
    public string SourceSignature { get; }
    public string IntentSignature { get; }
    public string ProvenanceSignature { get; }
    public ConsumerSurfaceModel SurfaceModel { get; }
    public IReadOnlyList<OverlayPlan> Overlays { get; }
    public IReadOnlyList<InteractionRequest> Interactions { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature =>
        string.Join("::", new[]
        {
            ProgramKind.ToString(),
            CapabilityKind.ToString(),
            CompositionKind.ToString(),
            Delivery.Signature,
            Provider.Signature,
            SourceSignature,
            IntentSignature,
            ProvenanceSignature,
            SurfaceModel.SurfaceId,
            string.Join("|", Overlays.Select(overlay => overlay.Signature)),
            string.Join("|", Interactions.Select(interaction => interaction.Signature))
        });

    public static VNextUiConsumptionContract FromIntent(
        AnalyticalIntent intent,
        ConsumerProviderContract provider,
        ConsumerSurfaceModel? surfaceModel = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(intent);

        return new VNextUiConsumptionContract(
            intent.ProgramRequest.Kind,
            intent.Capability.CapabilityKind,
            intent.Capability.CompositionKind,
            intent.Delivery,
            provider,
            intent.Selection.Signature,
            intent.Signature,
            intent.Provenance.Signature,
            surfaceModel ?? ConsumerSurfaceModel.None,
            intent.Overlays,
            intent.Interactions,
            metadata);
    }

    public static VNextUiConsumptionContract FromRenderPlan(
        AnalyticalIntent intent,
        ConsumerProviderContract provider,
        ChartRenderPlan renderPlan,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(renderPlan);

        if (renderPlan.ProgramKind != intent.ProgramRequest.Kind)
            throw new ArgumentException("Render plan program kind must match the analytical intent.", nameof(renderPlan));

        return FromIntent(
            intent,
            provider,
            ConsumerSurfaceModel.FromRenderPlan(renderPlan),
            metadata);
    }

    private static IReadOnlyDictionary<string, string> BuildMetadata(
        ConsumerDeliveryContract delivery,
        ConsumerProviderContract provider,
        ConsumerSurfaceModel surfaceModel,
        IReadOnlyDictionary<string, string>? metadata)
    {
        var resolved = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["ProgramKind"] = delivery.ProgramKind.ToString(),
            ["ConsumerKind"] = delivery.ConsumerKind.ToString(),
            ["DeliveryTarget"] = delivery.DeliveryTarget,
            ["RequiresRenderPlan"] = delivery.RequiresRenderPlan.ToString(),
            ["ProviderKey"] = provider.ProviderKey,
            ["ProviderDisplayName"] = provider.DisplayName,
            ["ProviderSignature"] = provider.Signature,
            ["SurfaceKind"] = surfaceModel.Kind.ToString(),
            ["SurfaceId"] = surfaceModel.SurfaceId
        };

        foreach (var pair in delivery.Metadata)
            resolved[$"Delivery.{pair.Key}"] = pair.Value;

        foreach (var pair in provider.Metadata)
            resolved[$"Provider.{pair.Key}"] = pair.Value;

        foreach (var pair in surfaceModel.Metadata)
            resolved[$"Surface.{pair.Key}"] = pair.Value;

        if (metadata != null)
        {
            foreach (var pair in metadata)
                resolved[pair.Key] = pair.Value;
        }

        return resolved;
    }
}
