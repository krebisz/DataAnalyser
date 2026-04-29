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
    public void SyncfusionSunburstCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.BarPie();

        Assert.Throws<ArgumentException>(() => new SyncfusionSunburstCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst, "SyncfusionSunburst")));
    }
}
