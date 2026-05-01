using DataVisualiser.Core.Rendering.BarPie;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Core.Rendering;

public sealed class BarPieRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_IncludesQualifiedLiveChartsAndPlaceholderDebt()
    {
        var contract = new BarPieRenderingContract();

        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.LiveChartsWpfColumn
            && entry.RendererKind == ChartRendererKind.LiveCharts
            && entry.Route == BarPieRenderingRoute.Column
            && entry.Qualification == BarPieRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.LiveChartsWpfPieFacet
            && entry.RendererKind == ChartRendererKind.LiveCharts
            && entry.Route == BarPieRenderingRoute.PieFacet
            && entry.Qualification == BarPieRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == BarPieBackendKey.EChartsPlaceholderColumn
            && entry.RendererKind == ChartRendererKind.ECharts
            && entry.Route == BarPieRenderingRoute.Column
            && entry.Qualification == BarPieRenderingQualification.UnqualifiedDebt);
    }

    [Fact]
    public void GetCapabilities_ForLiveChartsPieRoute_ReportsQualifiedWithoutReset()
    {
        var contract = new BarPieRenderingContract();

        var capabilities = contract.GetCapabilities(ChartRendererKind.LiveCharts, BarPieRenderingRoute.PieFacet);

        Assert.Equal("BarPie.PieFacet.LiveChartsWpf", capabilities.PathKey);
        Assert.Equal(BarPieRenderingQualification.Qualified, capabilities.Qualification);
        Assert.True(capabilities.SupportsRender);
        Assert.False(capabilities.SupportsResetView);
    }

    [Theory]
    [InlineData(false, BarPieRenderingRoute.Column)]
    [InlineData(true, BarPieRenderingRoute.PieFacet)]
    public void ResolveRoute_ReturnsExpectedRoute(bool isPieMode, BarPieRenderingRoute expectedRoute)
    {
        var route = BarPieRenderingRouteResolver.Resolve(isPieMode);

        Assert.Equal(expectedRoute, route);
    }

    [Fact]
    public void ResetView_ForPieFacetRoute_IsNoOp()
    {
        var contract = new BarPieRenderingContract();
        var host = new BarPieChartRenderHost(new StubTrackedSurface(), new StubRenderer(), ChartRendererKind.LiveCharts, true);

        contract.ResetView(BarPieRenderingRoute.PieFacet, host);
    }

    [Fact]
    public void HasRenderableContent_UsesTrackedSurfaceState()
    {
        var contract = new BarPieRenderingContract();
        var surface = new StubTrackedSurface
        {
            HasRenderedContentValue = true
        };
        var host = new BarPieChartRenderHost(surface, new StubRenderer(), ChartRendererKind.LiveCharts, true);

        Assert.True(contract.HasRenderableContent(BarPieRenderingRoute.PieFacet, host));

        surface.SetHasRenderedContent(false);

        Assert.False(contract.HasRenderableContent(BarPieRenderingRoute.PieFacet, host));
    }

    [Fact]
    public async Task RenderAsync_ShouldPreserveVocabularyMetadata()
    {
        var contract = new BarPieRenderingContract();
        var model = new UiChartRenderModel
        {
            ChartName = "BarPie",
            Title = "Buckets",
            IsVisible = true,
            Series =
            [
                new ChartSeriesModel
                {
                    Name = "Weight",
                    Values = [1, 2]
                }
            ]
        };
        var host = new BarPieChartRenderHost(new StubTrackedSurface(), new StubRenderer(), ChartRendererKind.LiveCharts, true);

        var result = await contract.RenderAsync(new BarPieChartRenderRequest(BarPieRenderingRoute.Column, model), host);

        Assert.Equal("Chart", result.Metadata[ChartRenderPlanMetadataKeys.ConsumerKind]);
        Assert.Equal("BarPieChart", result.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal("Identity", result.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal("MultiSeries", result.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.True(result.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.IntentSignature));
        Assert.True(result.Metadata.ContainsKey(ChartRenderPlanMetadataKeys.ProvenanceSignature));
        Assert.Equal("ChartRenderPlan", result.Metadata[BarPieVNextConsumptionContractBuilder.SurfaceKindKey]);
        Assert.True(result.Metadata.ContainsKey(BarPieVNextConsumptionContractBuilder.ConsumptionContractSignatureKey));
    }

    [Fact]
    public void BarPieRenderPlanBuilder_ShouldUseRuntimeCapabilityContract()
    {
        var programRequest = ChartProgramRequest.BarPie();
        var request = new BarPieChartRenderRequest(
            BarPieRenderingRoute.Column,
            new UiChartRenderModel
            {
                ChartName = "BarPie",
                Title = "Buckets",
                Series = [new ChartSeriesModel { Name = "Weight", Values = [1, 2] }]
            },
            new BarPieCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.Chart(ChartProgramKind.BarPie, "BarPieDiagnosticSurface")));

        var plan = BarPieRenderPlanBuilder.Build(request, ChartRendererKind.LiveCharts);

        Assert.Equal(ChartProgramKind.BarPie, plan.ProgramKind);
        Assert.Equal(BarPieBackendKey.LiveChartsWpfColumn, plan.Metadata[ChartRenderPlanMetadataKeys.BackendKey]);
        Assert.Equal("Column", plan.Metadata["Route"]);
        Assert.Equal("BarPieDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.DeliveryTarget]);
        Assert.Equal(AnalyticalCapabilityKind.Identity.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CapabilityKind]);
        Assert.Equal(CompositionKind.MultiSeries.ToString(), plan.Metadata[ChartRenderPlanMetadataKeys.CompositionKind]);
        Assert.Contains("Chart:BarPie:BarPieDiagnosticSurface", plan.Metadata[ChartRenderPlanMetadataKeys.IntentSignature], StringComparison.Ordinal);
    }

    [Fact]
    public void BarPieVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata()
    {
        var request = new BarPieChartRenderRequest(
            BarPieRenderingRoute.PieFacet,
            new UiChartRenderModel
            {
                ChartName = "BarPie",
                Title = "Buckets",
                Series = [new ChartSeriesModel { Name = "Weight", Values = [1, 2] }],
                Facets =
                [
                    new ChartFacetModel
                    {
                        Title = "Weight",
                        Series = [new ChartSeriesModel { Name = "Fat", Values = [1] }]
                    }
                ]
            },
            BarPieCapabilityContract.Create());
        var plan = BarPieRenderPlanBuilder.Build(request, ChartRendererKind.LiveCharts);

        var contract = BarPieVNextConsumptionContractBuilder.Build(request, ChartRendererKind.LiveCharts, plan);
        var qualifiedPlan = BarPieVNextConsumptionContractBuilder.AttachMetadata(plan, contract);

        Assert.Equal(ChartProgramKind.BarPie, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.Identity, contract.CapabilityKind);
        Assert.Equal(CompositionKind.MultiSeries, contract.CompositionKind);
        Assert.Equal(ConsumerKind.Chart, contract.Delivery.ConsumerKind);
        Assert.Equal("BarPieChart", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(plan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal("PieFacet", contract.Metadata["BarPie.Route"]);
        Assert.Equal("LiveCharts", contract.Metadata["BarPie.RendererKind"]);
        Assert.Equal("BarPie", contract.Metadata["BarPie.ChartName"]);
        Assert.Equal("Buckets", contract.Metadata["BarPie.Title"]);
        Assert.Equal(contract.Signature, qualifiedPlan.Metadata[BarPieVNextConsumptionContractBuilder.ConsumptionContractSignatureKey]);
        Assert.Equal("ChartRenderPlan", qualifiedPlan.Metadata[BarPieVNextConsumptionContractBuilder.SurfaceKindKey]);
        Assert.Equal(plan.Id, qualifiedPlan.Metadata[BarPieVNextConsumptionContractBuilder.SurfaceIdKey]);
    }

    [Fact]
    public void BarPieCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.Distribution();

        Assert.Throws<ArgumentException>(() => new BarPieCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.Chart(ChartProgramKind.BarPie, "BarPieChart")));
    }

    private sealed class StubTrackedSurface : IChartSurface, ITrackedChartContentSurface, ITrackedCartesianChartSurface
    {
        public bool HasRenderedContentValue { get; set; }

        public bool HasRenderedContent => HasRenderedContentValue;

        public CartesianChart? RenderedCartesianChart { get; private set; }

        public void SetRenderedCartesianChart(CartesianChart? chart)
        {
            RenderedCartesianChart = chart;
        }

        public void SetHasRenderedContent(bool hasRenderedContent)
        {
            HasRenderedContentValue = hasRenderedContent;
        }

        public void SetTitle(string? title) { }
        public void SetIsVisible(bool isVisible) { }
        public void SetHeader(System.Windows.UIElement? header) { }
        public void SetBehavioralControls(System.Windows.UIElement? controls) { }
        public void SetChartContent(System.Windows.UIElement? content) { }
    }

    private sealed class StubRenderer : IChartRenderer
    {
        public Task ApplyAsync(IChartSurface surface, UiChartRenderModel model, CancellationToken cancellationToken = default)
        {
            if (surface is ITrackedChartContentSurface tracked)
                tracked.SetHasRenderedContent(model.HasRenderableContent);

            return Task.CompletedTask;
        }
    }
}
