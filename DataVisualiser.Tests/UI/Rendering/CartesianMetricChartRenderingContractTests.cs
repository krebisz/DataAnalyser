using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

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

    private sealed class StubRenderInvoker : ICartesianMetricChartRenderInvoker
    {
        public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
        {
            return Task.CompletedTask;
        }
    }
}
