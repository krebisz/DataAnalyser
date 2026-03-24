using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

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

    private sealed class StubRenderInvoker : ITransformChartRenderInvoker
    {
        public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
        {
            return Task.CompletedTask;
        }
    }
}
