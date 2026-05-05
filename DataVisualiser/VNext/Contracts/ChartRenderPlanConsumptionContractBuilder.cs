using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public static class ChartRenderPlanConsumptionContractBuilder
{
    public static VNextUiConsumptionContract Build(
        ChartRenderPlan plan,
        IAnalyticalCapabilityContract capabilityContract,
        ConsumerProviderContract provider,
        IReadOnlyDictionary<string, string>? metadata = null,
        string? ownerName = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(capabilityContract);
        ArgumentNullException.ThrowIfNull(provider);

        if (plan.ProgramKind != capabilityContract.ProgramRequest.Kind)
            throw new ArgumentException("Render plan program kind must match the capability contract program kind.", nameof(plan));

        return new VNextUiConsumptionContract(
            capabilityContract.ProgramRequest.Kind,
            capabilityContract.Capability.CapabilityKind,
            capabilityContract.Capability.CompositionKind,
            capabilityContract.Delivery,
            provider,
            plan.SourceSignature,
            ReadRequiredMetadata(plan, ChartRenderPlanMetadataKeys.IntentSignature, ownerName),
            ReadRequiredMetadata(plan, ChartRenderPlanMetadataKeys.ProvenanceSignature, ownerName),
            ConsumerSurfaceModel.FromRenderPlan(plan),
            metadata: metadata);
    }

    private static string ReadRequiredMetadata(ChartRenderPlan plan, string key, string? ownerName)
    {
        if (plan.Metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
            return value;

        var owner = string.IsNullOrWhiteSpace(ownerName) ? plan.ProgramKind.ToString() : ownerName;
        throw new InvalidOperationException($"{owner} render plan is missing required metadata '{key}'.");
    }
}
