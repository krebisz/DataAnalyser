using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.UI.Charts.Interaction;
using DataVisualiser.Tests.Helpers.Infrastructure;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Helpers;

public sealed class ChartHelperInteractionTests
{
    [Fact]
    public void InitializeChartTooltip_WithFactory_AssignsCorrectTooltipType()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();

            ChartHelper.InitializeChartTooltip(chart, () => new SimpleChartTooltip());

            Assert.IsType<SimpleChartTooltip>(chart.DataTooltip);
        });
    }

    [Fact]
    public void InitializeChartTooltip_WithoutFactory_IsNoOp()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();
            var original = chart.DataTooltip;

            ChartHelper.InitializeChartTooltip(chart);

            Assert.Same(original, chart.DataTooltip);
        });
    }
}
