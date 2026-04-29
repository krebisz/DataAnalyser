using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;
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

    private sealed class StubRenderInvoker : ICartesianMetricChartRenderInvoker
    {
        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            return Task.CompletedTask;
        }
    }
}
