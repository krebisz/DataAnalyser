using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Core.Rendering.Tooltip;
using DataVisualiser.Core.Rendering.Interaction;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.Helpers;

public sealed class ChartHelperInteractionTests
{
    [Fact]
    public void InitializeChartTooltip_AssignsSimpleChartTooltip_AndIsIdempotent()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();

            ChartHelper.InitializeChartTooltip(chart);
            var firstTooltip = chart.DataTooltip;

            ChartHelper.InitializeChartTooltip(chart);

            Assert.NotNull(firstTooltip);
            Assert.IsType<SimpleChartTooltip>(firstTooltip);
            Assert.Same(firstTooltip, chart.DataTooltip);
        });
    }
}
