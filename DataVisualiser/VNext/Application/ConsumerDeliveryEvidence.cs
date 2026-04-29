using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed record ConsumerDeliveryEvidence
{
    private ConsumerDeliveryEvidence(
        ChartProgramKind programKind,
        string sourceSignature,
        string intentSignature,
        string? executionSignature,
        ConsumerKind consumerKind,
        string deliveryTarget,
        bool requiresRenderPlan,
        AnalyticalCapabilityKind capabilityKind,
        CompositionKind compositionKind,
        string provenanceSignature,
        string providerKey,
        string providerDisplayName,
        string providerSignature,
        IReadOnlyDictionary<string, string> metadata)
    {
        ProgramKind = programKind;
        SourceSignature = sourceSignature;
        IntentSignature = intentSignature;
        ExecutionSignature = executionSignature;
        ConsumerKind = consumerKind;
        DeliveryTarget = deliveryTarget;
        RequiresRenderPlan = requiresRenderPlan;
        CapabilityKind = capabilityKind;
        CompositionKind = compositionKind;
        ProvenanceSignature = provenanceSignature;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        ProviderSignature = providerSignature;
        Metadata = metadata;
    }

    public ChartProgramKind ProgramKind { get; }
    public string SourceSignature { get; }
    public string IntentSignature { get; }
    public string? ExecutionSignature { get; }
    public ConsumerKind ConsumerKind { get; }
    public string DeliveryTarget { get; }
    public bool RequiresRenderPlan { get; }
    public AnalyticalCapabilityKind CapabilityKind { get; }
    public CompositionKind CompositionKind { get; }
    public string ProvenanceSignature { get; }
    public string ProviderKey { get; }
    public string ProviderDisplayName { get; }
    public string ProviderSignature { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }

    public static ConsumerDeliveryEvidence FromIntent(
        AnalyticalIntent intent,
        ConsumerProviderContract provider,
        string? executionSignature = null)
    {
        ArgumentNullException.ThrowIfNull(intent);
        ArgumentNullException.ThrowIfNull(provider);

        if (!provider.Supports(intent.Delivery))
            throw new ArgumentException("Provider must support the intent delivery contract.", nameof(provider));

        var metadata = new Dictionary<string, string>(intent.Delivery.Metadata, StringComparer.OrdinalIgnoreCase);
        foreach (var pair in provider.Metadata)
            metadata[$"Provider.{pair.Key}"] = pair.Value;

        return new ConsumerDeliveryEvidence(
            intent.ProgramRequest.Kind,
            intent.Selection.Signature,
            intent.Signature,
            executionSignature,
            intent.Delivery.ConsumerKind,
            intent.Delivery.DeliveryTarget,
            intent.Delivery.RequiresRenderPlan,
            intent.Capability.CapabilityKind,
            intent.Capability.CompositionKind,
            intent.Provenance.Signature,
            provider.ProviderKey,
            provider.DisplayName,
            provider.Signature,
            metadata);
    }

    public static ConsumerDeliveryEvidence ForExport(
        MetricSelectionRequest selection,
        ChartProgramKind programKind,
        ConsumerProviderRegistry? providerRegistry = null)
    {
        ArgumentNullException.ThrowIfNull(selection);

        providerRegistry ??= ConsumerProviderRegistry.BuiltIn;
        var programRequest = CreateProgramRequest(programKind);
        var delivery = ConsumerDeliveryContract.Export(programKind);
        var intent = AnalyticalIntent.FromRequests(selection, programRequest, delivery);
        var provider = providerRegistry.Resolve(delivery);
        return FromIntent(intent, provider);
    }

    private static ChartProgramRequest CreateProgramRequest(ChartProgramKind kind)
    {
        return kind switch
        {
            ChartProgramKind.Main => ChartProgramRequest.MainProgram(),
            ChartProgramKind.Normalized => ChartProgramRequest.Normalized(),
            ChartProgramKind.Difference => ChartProgramRequest.Difference(),
            ChartProgramKind.Ratio => ChartProgramRequest.Ratio(),
            ChartProgramKind.Transform => ChartProgramRequest.Transform("Transform", Array.Empty<SeriesOperationRequest>()),
            ChartProgramKind.Distribution => ChartProgramRequest.Distribution(),
            ChartProgramKind.WeekdayTrend => ChartProgramRequest.WeekdayTrend(),
            ChartProgramKind.BarPie => ChartProgramRequest.BarPie(),
            ChartProgramKind.SyncfusionSunburst => ChartProgramRequest.SyncfusionSunburst(),
            _ => new ChartProgramRequest(kind)
        };
    }
}

public sealed record AnalyticalConsumerDeliveryResult(
    AnalyticalExecutionResult Execution,
    ConsumerDeliveryEvidence Evidence);
