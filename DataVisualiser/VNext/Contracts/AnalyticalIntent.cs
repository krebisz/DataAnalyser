namespace DataVisualiser.VNext.Contracts;

public enum ConsumerKind
{
    Chart,
    HierarchyChart,
    Export,
    Api,
    Other
}

public enum OverlayKind
{
    ReferenceLine,
    AverageLine,
    MedianLine,
    Threshold,
    ConfidenceMarker
}

public enum InteractionKind
{
    VisibilityToggle,
    ResetZoom,
    ViewportChange,
    Selection,
    Hover
}

public enum AnalyticalCapabilityKind
{
    Identity,
    Normalization,
    Comparison,
    Transform,
    Distribution,
    TemporalTrend,
    Hierarchy
}

public enum CompositionKind
{
    SingleSeries,
    MultiSeries,
    DerivedSeries,
    Hierarchy
}

public sealed record CapabilityRequest(
    AnalyticalCapabilityKind CapabilityKind,
    CompositionKind CompositionKind,
    IReadOnlyList<SeriesOperationRequest>? Operations = null)
{
    public IReadOnlyList<SeriesOperationRequest> ResolvedOperations { get; } =
        Operations?.ToArray() ?? Array.Empty<SeriesOperationRequest>();

    public string Signature =>
        $"{CapabilityKind}:{CompositionKind}:{string.Join("|", ResolvedOperations.Select(operation => $"{operation.Kind}:{operation.Id}"))}";

    public static CapabilityRequest FromProgramRequest(ChartProgramRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Kind switch
        {
            ChartProgramKind.Main => new CapabilityRequest(
                AnalyticalCapabilityKind.Identity,
                request.DisplayMode == ChartDisplayMode.Summed ? CompositionKind.SingleSeries : CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.Normalized => new CapabilityRequest(
                AnalyticalCapabilityKind.Normalization,
                CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.Difference or ChartProgramKind.Ratio => new CapabilityRequest(
                AnalyticalCapabilityKind.Comparison,
                CompositionKind.DerivedSeries,
                request.SeriesOperations),
            ChartProgramKind.Transform => new CapabilityRequest(
                AnalyticalCapabilityKind.Transform,
                request.SeriesOperations.Count == 0 ? CompositionKind.MultiSeries : CompositionKind.DerivedSeries,
                request.SeriesOperations),
            ChartProgramKind.Distribution => new CapabilityRequest(
                AnalyticalCapabilityKind.Distribution,
                CompositionKind.SingleSeries,
                request.SeriesOperations),
            ChartProgramKind.WeekdayTrend => new CapabilityRequest(
                AnalyticalCapabilityKind.TemporalTrend,
                CompositionKind.SingleSeries,
                request.SeriesOperations),
            ChartProgramKind.BarPie => new CapabilityRequest(
                AnalyticalCapabilityKind.Identity,
                CompositionKind.MultiSeries,
                request.SeriesOperations),
            ChartProgramKind.SyncfusionSunburst => new CapabilityRequest(
                AnalyticalCapabilityKind.Hierarchy,
                CompositionKind.Hierarchy,
                request.SeriesOperations),
            _ => throw new InvalidOperationException($"Unsupported program kind '{request.Kind}'.")
        };
    }
}

public sealed record ProvenanceDescriptor
{
    public ProvenanceDescriptor(
        string sourceSignature,
        string authority = "VNext",
        string trustClass = "Derived",
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(sourceSignature))
            throw new ArgumentException("Source signature cannot be null or empty.", nameof(sourceSignature));
        if (string.IsNullOrWhiteSpace(authority))
            throw new ArgumentException("Authority cannot be null or empty.", nameof(authority));
        if (string.IsNullOrWhiteSpace(trustClass))
            throw new ArgumentException("Trust class cannot be null or empty.", nameof(trustClass));

        SourceSignature = sourceSignature;
        Authority = authority;
        TrustClass = trustClass;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public string SourceSignature { get; }
    public string Authority { get; }
    public string TrustClass { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature => $"{Authority}:{TrustClass}:{SourceSignature}";

    public static ProvenanceDescriptor FromSelection(MetricSelectionRequest selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        return new ProvenanceDescriptor(selection.Signature, trustClass: "Requested");
    }
}

public sealed record ConsumerDeliveryContract
{
    public ConsumerDeliveryContract(
        ConsumerKind consumerKind,
        ChartProgramKind programKind,
        string deliveryTarget,
        bool requiresRenderPlan = true,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(deliveryTarget))
            throw new ArgumentException("Delivery target cannot be null or empty.", nameof(deliveryTarget));

        ConsumerKind = consumerKind;
        ProgramKind = programKind;
        DeliveryTarget = deliveryTarget;
        RequiresRenderPlan = requiresRenderPlan;
        Metadata = metadata == null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(metadata);
    }

    public ConsumerKind ConsumerKind { get; }
    public ChartProgramKind ProgramKind { get; }
    public string DeliveryTarget { get; }
    public bool RequiresRenderPlan { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public string Signature => $"{ConsumerKind}:{ProgramKind}:{DeliveryTarget}:{RequiresRenderPlan}";

    public static ConsumerDeliveryContract Chart(ChartProgramKind programKind, string deliveryTarget = "ChartSurface") =>
        new(ConsumerKind.Chart, programKind, deliveryTarget);

    public static ConsumerDeliveryContract HierarchyChart(ChartProgramKind programKind, string deliveryTarget = "HierarchySurface") =>
        new(ConsumerKind.HierarchyChart, programKind, deliveryTarget);
}

public sealed record OverlayPlan(
    OverlayKind Kind,
    string Label,
    IReadOnlyDictionary<string, string>? Parameters = null)
{
    public IReadOnlyDictionary<string, string> ResolvedParameters { get; } = Parameters == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Parameters);

    public string Signature => $"{Kind}:{Label}:{string.Join("|", ResolvedParameters.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}

public sealed record InteractionRequest(
    InteractionKind Kind,
    string Target,
    IReadOnlyDictionary<string, string>? Parameters = null)
{
    public IReadOnlyDictionary<string, string> ResolvedParameters { get; } = Parameters == null
        ? new Dictionary<string, string>()
        : new Dictionary<string, string>(Parameters);

    public string Signature => $"{Kind}:{Target}:{string.Join("|", ResolvedParameters.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"))}";
}

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
            delivery ?? ConsumerDeliveryContract.Chart(programRequest.Kind),
            capability,
            overlays,
            interactions);
    }
}
