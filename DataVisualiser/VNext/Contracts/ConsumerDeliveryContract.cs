namespace DataVisualiser.VNext.Contracts;

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

    public static ConsumerDeliveryContract Export(ChartProgramKind programKind, string deliveryTarget = "EvidenceExport") =>
        new(ConsumerKind.Export, programKind, deliveryTarget, requiresRenderPlan: false);

    public static ConsumerDeliveryContract Api(ChartProgramKind programKind, string deliveryTarget = "ApiResponse") =>
        new(ConsumerKind.Api, programKind, deliveryTarget, requiresRenderPlan: false);
}
