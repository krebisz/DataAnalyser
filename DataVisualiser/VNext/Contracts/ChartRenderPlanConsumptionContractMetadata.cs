using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.VNext.Contracts;

public static class ChartRenderPlanConsumptionContractMetadata
{
    public const string ConsumptionContractSignatureKey = "ConsumptionContractSignature";
    public const string SurfaceKindKey = "SurfaceKind";
    public const string SurfaceIdKey = "SurfaceId";

    public static ChartRenderPlan Attach(
        ChartRenderPlan plan,
        VNextUiConsumptionContract consumptionContract)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(consumptionContract);

        var metadata = new Dictionary<string, string>(plan.Metadata)
        {
            [ConsumptionContractSignatureKey] = consumptionContract.Signature,
            [SurfaceKindKey] = consumptionContract.SurfaceModel.Kind.ToString(),
            [SurfaceIdKey] = consumptionContract.SurfaceModel.SurfaceId
        };

        return plan with { Metadata = metadata };
    }
}
