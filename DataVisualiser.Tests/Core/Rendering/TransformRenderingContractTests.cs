using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
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
}
