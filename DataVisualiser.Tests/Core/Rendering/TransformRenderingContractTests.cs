using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Core.Rendering;

public sealed class TransformRenderingContractTests
{
    [Fact]
    public void GetBackendQualificationMatrix_ContainsQualifiedTransformEntry()
    {
        var contract = new TransformRenderingContract(new StubRenderInvoker());
        var entries = contract.GetBackendQualificationMatrix();

        Assert.Contains(entries, entry => entry.BackendKey == TransformBackendKey.LiveChartsWpfResultCartesian
            && entry.Route == TransformRenderingRoute.ResultCartesian
            && entry.Qualification == TransformRenderingQualification.Qualified);
    }

    [Fact]
    public void HasRenderableContent_ReflectsChartSeriesState()
    {
        StaTestHelper.Run(() =>
        {
            var contract = new TransformRenderingContract(new StubRenderInvoker());
            var chartState = new ChartState();
            var chart = new CartesianChart();
            var host = new TransformChartRenderHost(chart, chartState);

            Assert.False(contract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, host));

            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<double> { 1d }
                }
            };

            Assert.True(contract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, host));
        });
    }

    [Fact]
    public void Clear_InvokesAuxiliaryVisualReset()
    {
        StaTestHelper.Run(() =>
        {
            var contract = new TransformRenderingContract(new StubRenderInvoker());
            var chartState = new ChartState();
            var chart = new CartesianChart
            {
                Series = new SeriesCollection
                {
                    new LineSeries
                    {
                        Values = new ChartValues<double> { 1d }
                    }
                }
            };
            var resetCalled = false;
            var host = new TransformChartRenderHost(chart, chartState, () => resetCalled = true);

            contract.Clear(TransformRenderingRoute.ResultCartesian, host);

            Assert.True(resetCalled);
            Assert.False(contract.HasRenderableContent(TransformRenderingRoute.ResultCartesian, host));
        });
    }

    [Fact]
    public async Task RenderAsync_ShouldForwardTransformCapabilityContract()
    {
        await StaTestHelper.RunAsync(async () =>
        {
            var invoker = new StubRenderInvoker();
            var contract = new TransformRenderingContract(invoker);
            var programRequest = ChartProgramRequest.Transform(
                "Weight - Fat (mass) / Weight - Skeletal Muscle (mass)",
                [SeriesOperationRequest.Ratio(0, 1, "Weight - Fat (mass) / Weight - Skeletal Muscle (mass)")]);
            var capabilityContract = new TransformCapabilityContract(
                programRequest,
                CapabilityRequest.FromProgramRequest(programRequest),
                ConsumerDeliveryContract.Chart(ChartProgramKind.Transform, "TransformChart"));
            var request = new TransformChartRenderRequest(
                TransformRenderingRoute.ResultCartesian,
                new ChartDataContext(),
                new StubStrategy(),
                "Transform Result",
                "/",
                true,
                CapabilityContract: capabilityContract);

            await contract.RenderAsync(request, new TransformChartRenderHost(new CartesianChart(), new ChartState()));

            Assert.Same(capabilityContract, invoker.LastRequest?.CapabilityContract);
            Assert.Equal(AnalyticalCapabilityKind.Transform, invoker.LastRequest?.CapabilityContract?.Capability.CapabilityKind);
            Assert.Equal(CompositionKind.DerivedSeries, invoker.LastRequest?.CapabilityContract?.Capability.CompositionKind);
        });
    }

    [Fact]
    public void TransformVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata()
    {
        var request = new TransformChartRenderRequest(
            TransformRenderingRoute.ResultCartesian,
            new ChartDataContext(),
            new StubStrategy(),
            "Transform Result",
            "log",
            false,
            CapabilityContract: TransformCapabilityContract.Create(
                "Transform Result",
                [SeriesOperationRequest.Logarithm(0, "transform::log", "Log(Weight - Fat (mass))")]));
        var plan = CreateRenderPlan();

        var contract = TransformVNextConsumptionContractBuilder.Build(request, plan);
        var qualifiedPlan = ChartRenderPlanConsumptionContractMetadata.Attach(plan, contract);

        Assert.Equal(ChartProgramKind.Transform, contract.ProgramKind);
        Assert.Equal(AnalyticalCapabilityKind.Transform, contract.CapabilityKind);
        Assert.Equal(CompositionKind.DerivedSeries, contract.CompositionKind);
        Assert.Equal(ConsumerKind.Chart, contract.Delivery.ConsumerKind);
        Assert.Equal("TransformChart", contract.Delivery.DeliveryTarget);
        Assert.Equal(ConsumerSurfaceModelKind.ChartRenderPlan, contract.SurfaceModel.Kind);
        Assert.Equal(plan.Id, contract.SurfaceModel.SurfaceId);
        Assert.Equal("ResultCartesian", contract.Metadata["Transform.Route"]);
        Assert.Equal("Transform Result", contract.Metadata["Transform.PrimaryLabel"]);
        Assert.Equal("log", contract.Metadata["Transform.OperationType"]);
        Assert.Equal("False", contract.Metadata["Transform.IsOperationChart"]);
        Assert.Equal(contract.Signature, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.ConsumptionContractSignatureKey]);
        Assert.Equal("ChartRenderPlan", qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceKindKey]);
        Assert.Equal(plan.Id, qualifiedPlan.Metadata[ChartRenderPlanConsumptionContractMetadata.SurfaceIdKey]);
    }

    [Fact]
    public void TransformCapabilityContract_ShouldRejectProgramKindDrift()
    {
        var programRequest = ChartProgramRequest.WeekdayTrend();

        Assert.Throws<ArgumentException>(() => new TransformCapabilityContract(
            programRequest,
            CapabilityRequest.FromProgramRequest(programRequest),
            ConsumerDeliveryContract.Chart(ChartProgramKind.Transform, "TransformChart")));
    }

    private sealed class StubRenderInvoker : ITransformChartRenderInvoker
    {
        public TransformChartRenderRequest? LastRequest { get; private set; }

        public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
        {
            LastRequest = request;
            return Task.CompletedTask;
        }
    }

    private sealed class StubStrategy : IChartComputationStrategy
    {
        public string PrimaryLabel => "Transform Result";
        public string SecondaryLabel => string.Empty;
        public string? Unit => null;

        public ChartComputationResult? Compute()
        {
            return null;
        }
    }

    private static ChartRenderPlan CreateRenderPlan()
    {
        var sourceSignature = "transform-source";
        var metadata = new Dictionary<string, string>
        {
            [ChartRenderPlanMetadataKeys.IntentSignature] = "intent-transform",
            [ChartRenderPlanMetadataKeys.ProvenanceSignature] = "provenance-transform",
            [ChartRenderPlanMetadataKeys.ConsumerKind] = ConsumerKind.Chart.ToString(),
            [ChartRenderPlanMetadataKeys.DeliveryTarget] = "TransformChart",
            [ChartRenderPlanMetadataKeys.CapabilityKind] = AnalyticalCapabilityKind.Transform.ToString(),
            [ChartRenderPlanMetadataKeys.CompositionKind] = CompositionKind.DerivedSeries.ToString()
        };

        return new ChartRenderPlan(
            "transform-plan",
            ChartProgramKind.Transform,
            ChartRenderPlanKind.Cartesian,
            ChartDisplayMode.Regular,
            "Transform Result",
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
