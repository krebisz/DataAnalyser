using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Tests.VNext;

public sealed class VNextUiConsumptionContractTests
{
    [Fact]
    public void FromRenderPlan_ShouldPreserveProgramDeliveryProviderProvenanceAndSurfaceMetadata()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.Distribution(),
            new ConsumerDeliveryContract(
                ConsumerKind.Chart,
                ChartProgramKind.Distribution,
                "DistributionChart",
                requiresRenderPlan: true,
                metadata: new Dictionary<string, string> { ["DeliveryRole"] = "PrimaryChart" }),
            overlays: [new OverlayPlan(OverlayKind.AverageLine, "Average")],
            interactions: [new InteractionRequest(InteractionKind.ResetZoom, "DistributionChart")]);
        var provider = ConsumerProviderContracts.LiveChartsWpf;
        var renderPlan = CreateRenderPlan(intent);

        var contract = VNextUiConsumptionContract.FromRenderPlan(
            intent,
            provider,
            renderPlan,
            new Dictionary<string, string> { ["ContractRole"] = "UiConsumption" });

        Assert.Equal(ChartProgramKind.Distribution, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.Distribution, contract.CapabilityKind);
        Assert.Equal(CompositionKind.SingleSeries, contract.CompositionKind);
        Assert.Equal(intent.Delivery, contract.Delivery);
        Assert.Equal(provider, contract.Provider);
        Assert.Equal(selection.Signature, contract.SourceSignature);
        Assert.Equal(intent.Signature, contract.IntentSignature);
        Assert.Equal(intent.Provenance.Signature, contract.ProvenanceSignature);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(renderPlan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal(ChartRenderPlanKind.Cartesian, contract.SurfaceModel.RenderPlanKind);
        Assert.Single(contract.Overlays);
        Assert.Single(contract.Interactions);
        Assert.Equal("Distribution", contract.Metadata["ProgramKind"]);
        Assert.Equal("Chart", contract.Metadata["ConsumerKind"]);
        Assert.Equal("DistributionChart", contract.Metadata["DeliveryTarget"]);
        Assert.Equal("True", contract.Metadata["RequiresRenderPlan"]);
        Assert.Equal(provider.ProviderKey, contract.Metadata["ProviderKey"]);
        Assert.Equal(provider.Signature, contract.Metadata["ProviderSignature"]);
        Assert.Equal("PrimaryChart", contract.Metadata["Delivery.DeliveryRole"]);
        Assert.Equal("BuiltInChartRenderer", contract.Metadata["Provider.ProviderRole"]);
        Assert.Equal("ChartRenderPlan", contract.Metadata["SurfaceKind"]);
        Assert.Equal(ChartRenderPlanKind.Cartesian.ToString(), contract.Metadata["Surface.PlanKind"]);
        Assert.Equal("UiConsumption", contract.Metadata["ContractRole"]);
    }

    [Fact]
    public void FromIntent_ShouldSupportNonChartConsumersWithoutRenderPlan()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Export(ChartProgramKind.Main));

        var contract = VNextUiConsumptionContract.FromIntent(
            intent,
            ConsumerProviderContracts.EvidenceExport);

        Assert.Equal(ConsumerKind.Export, contract.Delivery.ConsumerKind);
        Assert.False(contract.Delivery.RequiresRenderPlan);
        Assert.Equal(ConsumerSurfaceModelKind.None, contract.SurfaceModel.Kind);
        Assert.Equal("False", contract.Metadata["RequiresRenderPlan"]);
        Assert.Equal(ConsumerProviderContracts.EvidenceExport.ProviderKey, contract.Metadata["ProviderKey"]);
    }

    [Fact]
    public void Constructor_ShouldRejectProviderDeliveryDrift()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.SyncfusionSunburst(),
            ConsumerDeliveryContract.HierarchyChart(ChartProgramKind.SyncfusionSunburst));

        var ex = Assert.Throws<ArgumentException>(() =>
            VNextUiConsumptionContract.FromIntent(
                intent,
                ConsumerProviderContracts.LiveChartsWpf));

        Assert.Contains("Provider must support", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FromRenderPlan_ShouldRejectProviderPlanKindDrift()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.Distribution(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Distribution, "DistributionChart"));
        var renderPlan = CreateRenderPlan(intent);
        var provider = new ConsumerProviderContract(
            "HierarchyOnlyChartProvider",
            "Hierarchy Only Chart Provider",
            ConsumerKind.Chart,
            new HashSet<ChartProgramKind> { ChartProgramKind.Distribution },
            new HashSet<ChartRenderPlanKind> { ChartRenderPlanKind.Hierarchy });

        var ex = Assert.Throws<ArgumentException>(() =>
            VNextUiConsumptionContract.FromRenderPlan(
                intent,
                provider,
                renderPlan));

        Assert.Contains("render-plan kind", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SurfaceModel_ShouldExposeCommonShapeForChartAndDerivedDatasetConsumers()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram());
        var renderPlan = CreateRenderPlan(intent);
        var chartSurface = ConsumerSurfaceModel.FromRenderPlan(renderPlan);
        var derivedSurface = ConsumerSurfaceModel.FromDerivedDatasets(
        [
            new DerivedDataset(
                "sum",
                "Total",
                [selection.From, selection.To],
                [1d, 2d],
                [1d, 2d],
                ["Weight:negative"],
                "Sum:sum:0,1:",
                new Dictionary<string, string> { ["StepIndex"] = "0" })
        ]);

        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, chartSurface.Kind);
        Assert.Equal(renderPlan.Id, chartSurface.SurfaceId);
        Assert.True(chartSurface.RequiresRenderPlan);
        Assert.Equal(ChartRenderPlanKind.Cartesian, chartSurface.RenderPlanKind);
        Assert.Equal("Weight", chartSurface.Metadata["Title"]);

        Assert.Equal(ConsumerSurfaceModelKind.DerivedDataset, derivedSurface.Kind);
        Assert.Equal("sum", derivedSurface.SurfaceId);
        Assert.False(derivedSurface.RequiresRenderPlan);
        Assert.Null(derivedSurface.RenderPlanKind);
        Assert.Equal("1", derivedSurface.Metadata["DatasetCount"]);
        Assert.Equal("sum", derivedSurface.Metadata["DatasetIds"]);
    }

    [Fact]
    public void Constructor_ShouldRejectRenderSurfaceForNonRenderDelivery()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.MainProgram(),
            ConsumerDeliveryContract.Export(ChartProgramKind.Main));
        var renderPlan = CreateRenderPlan(
            AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram()));
        var surface = ConsumerSurfaceModel.FromRenderPlan(renderPlan);

        var ex = Assert.Throws<ArgumentException>(() =>
            VNextUiConsumptionContract.FromIntent(
                intent,
                ConsumerProviderContracts.EvidenceExport,
                surface));

        Assert.Contains("render-plan surface", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void FromRenderPlan_ShouldRejectProgramDrift()
    {
        var selection = CreateSelection();
        var intent = AnalyticalIntent.FromRequests(
            selection,
            ChartProgramRequest.Distribution(),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Distribution, "DistributionChart"));
        var renderPlan = CreateRenderPlan(
            AnalyticalIntent.FromRequests(selection, ChartProgramRequest.MainProgram()));

        var ex = Assert.Throws<ArgumentException>(() =>
            VNextUiConsumptionContract.FromRenderPlan(
                intent,
                ConsumerProviderContracts.LiveChartsWpf,
                renderPlan));

        Assert.Contains("Render plan program kind", ex.Message, StringComparison.Ordinal);
    }

    private static MetricSelectionRequest CreateSelection() =>
        new(
            "Weight",
            [new MetricSeriesRequest("Weight", "negative")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 3),
            "HealthMetrics");

    private static ChartRenderPlan CreateRenderPlan(AnalyticalIntent intent) =>
        new(
            "plan-1",
            intent.ProgramRequest.Kind,
            ChartRenderPlanKind.Cartesian,
            ChartDisplayMode.Regular,
            "Weight",
            intent.Selection.From,
            intent.Selection.To,
            intent.Selection.Signature,
            Array.Empty<ChartSeriesPlan>(),
            Array.Empty<ChartHierarchyNodePlan>(),
            RenderDensityPlan.FullFidelity(0),
            new ChartInteractionPlan(
                SupportsZoom: true,
                SupportsPan: false,
                SupportsTooltips: true,
                SupportsSelection: false,
                SupportsViewportRefinement: false),
            new Dictionary<string, string>
            {
                [ChartRenderPlanMetadataKeys.IntentSignature] = intent.Signature,
                [ChartRenderPlanMetadataKeys.ProvenanceSignature] = intent.Provenance.Signature,
                [ChartRenderPlanMetadataKeys.ConsumerKind] = intent.Delivery.ConsumerKind.ToString(),
                [ChartRenderPlanMetadataKeys.DeliveryTarget] = intent.Delivery.DeliveryTarget,
                [ChartRenderPlanMetadataKeys.CapabilityKind] = intent.Capability.CapabilityKind.ToString(),
                [ChartRenderPlanMetadataKeys.CompositionKind] = intent.Capability.CompositionKind.ToString(),
                [ChartRenderPlanMetadataKeys.ProviderKey] = ConsumerProviderContracts.LiveChartsWpf.ProviderKey,
                [ChartRenderPlanMetadataKeys.ProviderSignature] = ConsumerProviderContracts.LiveChartsWpf.Signature
            });
}
