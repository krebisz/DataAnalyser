using System.Windows.Controls;
using DataVisualiser.Core.Services;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Adapters;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Rendering;
using DataVisualiser.UI.State;
using DataVisualiser.UI.ViewModels;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class BarPieChartControllerAdapterResetZoomTests
{
    [Fact]
    public void ResetZoom_WhenCartesianChartPresent_ClearsAxisBounds()
    {
        StaTestHelper.Run(() =>
        {
            var adapter = CreateAdapter(out var controller);

            var chart = new CartesianChart();
            chart.AxisX.Add(new Axis
            {
                MinValue = 0,
                MaxValue = 10
            });

            var grid = new Grid();
            grid.Children.Add(chart);
            controller.SetChartContent(grid);
            controller.IsChartVisible = true;

            adapter.ResetZoom();

            var axis = chart.AxisX[0];
            Assert.True(double.IsNaN(axis.MinValue));
            Assert.True(double.IsNaN(axis.MaxValue));
        });
    }

    [Fact]
    public void ResetZoom_WhenNoCartesianChart_DoesNotThrow()
    {
        StaTestHelper.Run(() =>
        {
            var adapter = CreateAdapter(out var controller);

            controller.SetChartContent(new PieChart());

            adapter.ResetZoom();
        });
    }

    private static BarPieChartControllerAdapter CreateAdapter(out BarPieChartController controller)
    {
        var chartState = new ChartState
        {
            IsBarPieVisible = true
        };
        var metricState = new MetricState();
        var uiState = new UiState();
        var metricService = new MetricSelectionService("Data Source=(local);Initial Catalog=Health;Integrated Security=SSPI;TrustServerCertificate=True");
        var viewModel = new MainWindowViewModel(chartState, metricState, uiState, metricService);
        controller = new BarPieChartController();
        var rendererResolver = new ChartRendererResolver();
        var surfaceFactory = new ChartSurfaceFactory(rendererResolver);

        return new BarPieChartControllerAdapter(controller, viewModel, () => false, metricService, rendererResolver, surfaceFactory);
    }
}
