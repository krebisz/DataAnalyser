namespace DataVisualiser.VNext.Contracts;

public sealed record AnalyticalIntent
{
    public AnalyticalIntent(
        MetricSelectionRequest selection,
        ChartProgramRequest programRequest,
        ProvenanceDescriptor provenance,
        ConsumerDeliveryContract delivery,
        CapabilityRequest? capability = null,
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(programRequest);
        ArgumentNullException.ThrowIfNull(provenance);
        ArgumentNullException.ThrowIfNull(delivery);

        if (delivery.ProgramKind != programRequest.Kind)
            throw new ArgumentException("Delivery program kind must match the requested program kind.", nameof(delivery));

        Selection = selection;
        ProgramRequest = programRequest;
        Provenance = provenance;
        Delivery = delivery;
        Capability = capability ?? CapabilityRequest.FromProgramRequest(programRequest);
        Overlays = overlays?.ToArray() ?? [];
        Interactions = interactions?.ToArray() ?? [];
    }

    public MetricSelectionRequest Selection { get; }
    public ChartProgramRequest ProgramRequest { get; }
    public ProvenanceDescriptor Provenance { get; }
    public ConsumerDeliveryContract Delivery { get; }
    public CapabilityRequest Capability { get; }
    public IReadOnlyList<OverlayPlan> Overlays { get; }
    public IReadOnlyList<InteractionRequest> Interactions { get; }

    public string Signature =>
        string.Join("::", new[]
        {
            Selection.Signature,
            ProgramRequest.Kind.ToString(),
            Capability.Signature,
            Delivery.Signature,
            Provenance.Signature,
            string.Join("|", Overlays.Select(overlay => overlay.Signature)),
            string.Join("|", Interactions.Select(interaction => interaction.Signature))
        });

    public static AnalyticalIntent FromRequests(
        MetricSelectionRequest selection,
        ChartProgramRequest programRequest,
        ConsumerDeliveryContract? delivery = null,
        ProvenanceDescriptor? provenance = null,
        CapabilityRequest? capability = null,
        IReadOnlyList<OverlayPlan>? overlays = null,
        IReadOnlyList<InteractionRequest>? interactions = null)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(programRequest);

        return new AnalyticalIntent(
            selection,
            programRequest,
            provenance ?? ProvenanceDescriptor.FromSelection(selection),
            delivery ?? ChartProgramDeliveryTargetResolver.CreateDelivery(programRequest.Kind),
            capability,
            overlays,
            interactions);
    }
}
