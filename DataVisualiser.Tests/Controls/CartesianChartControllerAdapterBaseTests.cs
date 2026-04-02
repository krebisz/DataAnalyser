using System.Threading.Tasks;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.Charts.Controllers;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;
using LiveCharts;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Controls;

public sealed class CartesianChartControllerAdapterBaseTests
{
    [Fact]
    public void Chart_ReturnsControllerChart()
    {
        StaTestHelper.Run(() =>
        {
            var chart = CreateChart();
            var adapter = new TestCartesianChartControllerAdapter(new TestCartesianChartControllerHost(chart));

            Assert.Same(chart, adapter.Chart);
        });
    }

    [Fact]
    public void Clear_RemovesSeries_AndTimestampRegistration()
    {
        StaTestHelper.Run(() =>
        {
            var chart = CreateChart();
            chart.Series.Add(new LineSeries { Values = new ChartValues<double> { 1d, 2d } });

            var state = new ChartState();
            state.ChartTimestamps[chart] =
            [
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ];

            var adapter = new TestCartesianChartControllerAdapter(new TestCartesianChartControllerHost(chart));

            adapter.Clear(state);

            Assert.Empty(chart.Series);
            Assert.DoesNotContain(chart, state.ChartTimestamps.Keys);
        });
    }

    [Fact]
    public void ResetZoom_ClearsAxisBounds()
    {
        StaTestHelper.Run(() =>
        {
            var chart = CreateChart();
            chart.AxisX[0].MinValue = 10;
            chart.AxisX[0].MaxValue = 20;

            var adapter = new TestCartesianChartControllerAdapter(new TestCartesianChartControllerHost(chart));

            adapter.ResetZoom();

            Assert.True(double.IsNaN(chart.AxisX[0].MinValue));
            Assert.True(double.IsNaN(chart.AxisX[0].MaxValue));
        });
    }

    [Fact]
    public void HasSeries_ReflectsChartSeriesPresence()
    {
        StaTestHelper.Run(() =>
        {
            var chart = CreateChart();
            var adapter = new TestCartesianChartControllerAdapter(new TestCartesianChartControllerHost(chart));
            var state = new ChartState();

            Assert.False(adapter.HasSeries(state));

            chart.Series.Add(new LineSeries { Values = new ChartValues<double> { 1d } });

            Assert.True(adapter.HasSeries(state));
        });
    }

    private static CartesianChart CreateChart()
    {
        var chart = new CartesianChart();
        chart.AxisX.Add(new Axis());
        return chart;
    }

    private sealed class TestCartesianChartControllerAdapter : CartesianChartControllerAdapterBase<TestCartesianChartControllerHost>
    {
        public TestCartesianChartControllerAdapter(TestCartesianChartControllerHost controller)
            : base(controller)
        {
        }

        public override string Key => "Test";
        public override bool RequiresPrimaryData => true;
        public override bool RequiresSecondaryData => false;

        public override Task RenderAsync(ChartDataContext context)
        {
            return Task.CompletedTask;
        }

        public override void UpdateSubtypeOptions()
        {
        }
    }

    private sealed class TestCartesianChartControllerHost : ICartesianChartControllerHost
    {
        public TestCartesianChartControllerHost(CartesianChart chart)
        {
            Chart = chart;
            Panel = new ChartPanelController();
            ToggleButton = Panel.ToggleButtonControl;
        }

        public ChartPanelController Panel { get; }
        public System.Windows.Controls.Button ToggleButton { get; }
        public CartesianChart Chart { get; }
    }
}
