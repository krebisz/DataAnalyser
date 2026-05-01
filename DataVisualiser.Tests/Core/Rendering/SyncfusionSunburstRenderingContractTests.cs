using DataVisualiser.Core.Rendering.Syncfusion;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.Core.Rendering;

public sealed class SyncfusionSunburstRenderingContractTests
{
    [Fact]
    public void SyncfusionSunburstRenderPlanBuilder_ShouldUseRuntimeCapabilityContract()
    {
        var programRequest = ChartProgramRequest.SyncfusionSunburst();
        var request = new SyncfusionSunburstChartRenderRequest(
            SyncfusionSunburstRenderingRoute.Hierarchy,
            Array.Empty<SyncfusionSunburstItem>(),
            BucketCount: 0,
            SelectionCount: 0,
            From: null,
            To: null,
            new SyncfusionSunburstCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst, "SyncfusionSunburstDiagnosticSurface")));

        var plan = SyncfusionSunburstRenderPlanBuilder.Build(request);

        Assert.Equal(ChartProgramKind.SyncfusionSunburst, plan.ProgramKind);
        Assert.Equal("HierarchyChart", plan.Metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
        Assert.Equal("SyncfusionSunburstDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.True(plan.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.IntentSignature));
        Assert.Contains("HierarchyChart:SyncfusionSunburst:SyncfusionSunburstDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionSunburstVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata()
    {
        var request = new SyncfusionSunburstChartRenderRequest(
            SyncfusionSunburstRenderingRoute.Hierarchy,
            [
                new SyncfusionSunburstItem("0-10", "Fat", 1d),
                new SyncfusionSunburstItem("0-10", "Muscle", 2d)
            ],
            BucketCount: 1,
            SelectionCount: 2,
            From: new DateTime(2024, 1, 1),
            To: new DateTime(2024, 1, 2),
            SyncfusionSunburstCapabilityContract.Create());
        var plan = SyncfusionSunburstRenderPlanBuilder.Build(request);

        var contract = SyncfusionSunburstVNextConsumptionContractBuilder.Build(request, plan);
        var qualifiedPlan = ChartRenderPlanConsumptionContractMetadata.Attach(plan, contract);

        Assert.Equal(ChartProgramKind.SyncfusionSunburst, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.Hierarchy, contract.CapabilityKind);
        Assert.Equal(CompositionKind.Hierarchy, contract.CompositionKind);
        Assert.Equal(ConsumerKind.HierarchyChart, contract.Delivery.ConsumerKind);
        Assert.Equal("SyncfusionSunburst", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(plan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal("Hierarchy", contract.Metadata["SyncfusionSunburst.Route"]);
        Assert.Equal("1", contract.Metadata["SyncfusionSunburst.BucketCount"]);
        Assert.Equal("2", contract.Metadata["SyncfusionSunburst.SelectionCount"]);
        Assert.Equal("2", contract.Metadata["SyncfusionSunburst.ItemCount"]);
        Assert.Equal(contract.Signature, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.ConsumptionContractSignatureKey]);
        Assert.Equal("ChartRenderPlan", qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceKindKey]);
        Assert.Equal(plan.Id, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceIdKey]);
    }

    [Fact]
    public async Task RenderAsync_ShouldAttachVNextConsumptionMetadata()
    {
        var contract = new SyncfusionSunburstRenderingContract();
        var target = new StubRenderTarget();
        var request = new SyncfusionSunburstChartRenderRequest(
            SyncfusionSunburstRenderingRoute.Hierarchy,
            [new SyncfusionSunburstItem("0-10", "Fat", 1d)],
            BucketCount: 1,
            SelectionCount: 1,
            From: new DateTime(2024, 1, 1),
            To: new DateTime(2024, 1, 2),
            SyncfusionSunburstCapabilityContract.Create());

        var result = await contract.RenderAsync(
            request,
            new SyncfusionSunburstChartRenderHost(target, IsVisible: true));

        Assert.True(target.HasItems);
        Assert.Equal(SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy, result.BackendKey);
        Assert.Equal("ChartRenderPlan", result.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceKindKey]);
        Assert.True(result.Metadata.ContainsKey(ChartRenderPlanConsumptionContractMetadata.ConsumptionContractSignatureKey));
        Assert.Equal("SyncfusionSunburst", result.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal("Hierarchy", result.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
    }

    [Fact]
    public void SyncfusionSunburstCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.BarPie();

        Assert.Throws<ArgumentException>(() => new SyncfusionSunburstCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst, "SyncfusionSunburst")));
    }

    private sealed class StubRenderTarget : ISyncfusionSunburstRenderTarget
    {
        public bool HasItems { get; private set; }

        public void SetItems(IReadOnlyList<SyncfusionSunburstItem> items)
        {
            HasItems = items.Count > 0;
        }
    }
}
