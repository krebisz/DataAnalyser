using DataVisualiser.Core.Rendering.Helpers;
using DataVisualiser.Tests.Helpers;
using LiveCharts.Wpf;

namespace DataVisualiser.Tests.UI.Rendering;

public sealed class DistributionAxisCoordinatorTests
{
    [Fact]
    public void ConfigureYAxis_ShouldPopulateAxisValuesAndFormatter()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();

            DistributionAxisCoordinator.ConfigureYAxis(chart, [10d, 20d], [2d, 3d], 2);

            Assert.Single(chart.AxisY);
            Assert.True(chart.AxisY[0].ShowLabels);
            Assert.Equal("Value", chart.AxisY[0].Title);
            Assert.True(chart.AxisY[0].MaxValue > chart.AxisY[0].MinValue);
            Assert.NotNull(chart.AxisY[0].LabelFormatter);
        });
    }

    [Fact]
    public void ConfigureXAxis_ShouldPopulateLabelsAndDisableSeparators()
    {
        StaTestHelper.Run(() =>
        {
            var chart = new CartesianChart();

            DistributionAxisCoordinator.ConfigureXAxis(chart, ["A", "B"], "Bucket");

            Assert.Single(chart.AxisX);
            Assert.Equal("Bucket", chart.AxisX[0].Title);
            Assert.Equal(["A", "B"], chart.AxisX[0].Labels);
            Assert.True(chart.AxisX[0].ShowLabels);
            Assert.NotNull(chart.AxisX[0].Separator);
            Assert.False(chart.AxisX[0].Separator.IsEnabled);
        });
    }
}
