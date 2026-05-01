using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Core.Rendering;

public sealed class CartesianMetricChartRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_ContainsQualifiedEntriesForMainNormalizedAndDiffRatio()
    {
        var contract = new CartesianMetricChartRenderingContract(new StubRenderInvoker());
        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == CartesianMetricBackendKey.LiveChartsWpfMain
            && entry.Route == CartesianMetricChartRoute.Main
            && entry.Qualification == CartesianMetricRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == CartesianMetricBackendKey.LiveChartsWpfNormalized
            && entry.Route == CartesianMetricChartRoute.Normalized
            && entry.Qualification == CartesianMetricRenderingQualification.Qualified);
        Assert.Contains(entries, entry => entry.BackendKey == CartesianMetricBackendKey.LiveChartsWpfDiffRatio
            && entry.Route == CartesianMetricChartRoute.DiffRatio
            && entry.Qualification == CartesianMetricRenderingQualification.Qualified);
    }

    [Fact]
    public void HasRenderableContent_ReflectsChartSeriesState()
    {
        StaTestHelper.Run(() =>
        {
            var contract = new CartesianMetricChartRenderingContract(new StubRenderInvoker());
            var chartState = new ChartState();
            var chart = new CartesianChart();
            var host = new CartesianMetricChartRenderHost(chart, chartState);

            Assert.False(contract.HasRenderableContent(CartesianMetricChartRoute.Main, host));

            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> { 1d }
                }
            };

            Assert.True(contract.HasRenderableContent(CartesianMetricChartRoute.Main, host));
        });
    }

    [Theory]
    [InlineData(ChartProgramKind.Main, "MainChart")]
    [InlineData(ChartProgramKind.Normalized, "NormalizedChart")]
    [InlineData(ChartProgramKind.Difference, "DiffRatioChart")]
    [InlineData(ChartProgramKind.Ratio, "DiffRatioChart")]
    public void CartesianMetricCapabilityContract_Create_ShouldBindCorrectDeliveryTarget(ChartProgramKind kind, string expectedTarget)
    {
        var contract = CartesianMetricCapabilityContract.Create(kind);

        Assert.Equal(kind, contract.ProgramRequest.Kind);
        Assert.Equal(kind, contract.Delivery.ProgramKind);
        Assert.Equal(expectedTarget, contract.Delivery.DeliveryTarget);
    }

    [Fact]
    public void CartesianMetricCapabilityContract_ShouldRejectInvalidKind()
    {
        var programRequest = ChartProgramRequest.BarPie();

        Assert.Throws<ArgumentException>(() => new CartesianMetricCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(ChartProgramKind.BarPie)));
    }

    [Fact]
    public void CartesianMetricCapabilityContract_ShouldRejectDeliveryKindMismatch()
    {
        var programRequest = ChartProgramRequest.MainProgram();

        Assert.Throws<ArgumentException>(() => new CartesianMetricCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ChartProgramDeliveryTargetResolver.CreateDelivery(ChartProgramKind.Normalized)));
    }

    [Fact]
    public void CartesianMetricVNextConsumptionContractBuilder_ShouldWrapMainRenderPlanAndPreserveMetadata()
    {
        var plan = CreateRenderPlan(ChartProgramKind.Main, ChartDisplayMode.Stacked);

        var contract = CartesianMetricVNextConsumptionContractBuilder.Build(
            plan,
            ConsumerDeliveryContract.Chart(ChartProgramKind.Main, "MainChart"),
            new Dictionary<string, string>
            {
                ["CartesianMetric.Route"] = "Main",
                ["CartesianMetric.IsStacked"] = "True",
                ["CartesianMetric.IsCumulative"] = "False"
            });
        var qualifiedPlan = ChartRenderPlanConsumptionContractMetadata.Attach(plan, contract);

        Assert.Equal(ChartProgramKind.Main, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.Identity, contract.CapabilityKind);
        Assert.Equal(CompositionKind.MultiSeries, contract.CompositionKind);
        Assert.Equal(ConsumerKind.Chart, contract.Delivery.ConsumerKind);
        Assert.Equal("MainChart", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(plan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal("Main", contract.Metadata["CartesianMetric.Route"]);
        Assert.Equal("True", contract.Metadata["CartesianMetric.IsStacked"]);
        Assert.Equal("False", contract.Metadata["CartesianMetric.IsCumulative"]);
        Assert.Equal(contract.Signature, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.ConsumptionContractSignatureKey]);
        Assert.Equal("ChartRenderPlan", qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceKindKey]);
        Assert.Equal(plan.Id, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceIdKey]);
    }

    [Fact]
    public void CartesianMetricVNextConsumptionContractBuilder_ShouldRejectNonCartesianMetricProgramKind()
    {
        var plan = CreateRenderPlan(ChartProgramKind.BarPie, ChartDisplayMode.Regular);

        Assert.Throws<ArgumentException>(() => CartesianMetricVNextConsumptionContractBuilder.Build(plan));
    }

    private sealed class StubRenderInvoker : ICartesianMetricChartRenderInvoker
    {
        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            return Task.CompletedTask;
        }
    }

    private static ChartRenderPlan CreateRenderPlan(ChartProgramKind kind, ChartDisplayMode displayMode)
    {
        var sourceSignature = $"{kind}-source";
        var programRequest = new ChartProgramRequest(kind, displayMode);
        var capability = CapabilityRequest.FromProgramRequest(programRequest);
        var delivery = ChartProgramDeliveryTargetResolver.CreateDelivery(kind);
        var metadata = new Dictionary<string, string>
        {
            [ChartRenderPlanMetadataKeys.IntentSignature] = $"{kind}-intent",
            [ChartRenderPlanMetadataKeys.ProvenanceSignature] = $"{kind}-provenance",
            [ChartRenderPlanMetadataKeys.ConsumerKind] = ConsumerKind.Chart.ToString(),
            [ChartRenderPlanMetadataKeys.DeliveryTarget] = delivery.DeliveryTarget,
            [ChartRenderPlanMetadataKeys.CapabilityKind] = capability.CapabilityKind.ToString(),
            [ChartRenderPlanMetadataKeys.CompositionKind] = capability.CompositionKind.ToString()
        };

        return new ChartRenderPlan(
            $"{kind}-plan",
            kind,
            ChartRenderPlanKind.Cartesian,
            displayMode,
            kind.ToString(),
            new DateTime(2024, 1, 1),
            new DateTime(2024, 1, 2),
            sourceSignature,
            Array.Empty<ChartSeriesPlan>(),
            Array.Empty<ChartHierarchyNodePlan>(),
            RenderDensityPlan.FullFidelity(0),
            new ChartInteractionPlan(
                SupportsZoom: true,
                SupportsPan: true,
                SupportsTooltips: true,
                SupportsSelection: false,
                SupportsViewportRefinement: false),
            metadata);
    }
}
