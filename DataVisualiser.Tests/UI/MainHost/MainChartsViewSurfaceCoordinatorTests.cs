using DataVisualiser.UI.MainHost;
using DataVisualiser.Tests.Helpers;
using LiveCharts.Wpf;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsViewSurfaceCoordinatorTests
{
    [Fact]
    public void InitializeSurfaces_ShouldInitializeChartsClearStartupStateAndCreatePolarTooltip()
    {
        StaTestHelper.Run(() =>
        {
            var coordinator = new MainChartsViewSurfaceCoordinator();
            var calls = new List<string>();
            var main = CreateChart();
            var normalized = CreateChart();
            var diffRatio = CreateChart();
            var distribution = CreateChart();
            ToolTip? tooltip = null;

            coordinator.InitializeSurfaces(
                new MainChartsViewSurfaceCoordinator.Actions(
                    () => main,
                    () => normalized,
                    () => diffRatio,
                    () => distribution,
                    () => calls.Add("behavior"),
                    () => calls.Add("distribution-behavior"),
                    () => calls.Add("clear"),
                    () => calls.Add("titles"),
                    value => tooltip = value));

            Assert.Equal(["behavior", "distribution-behavior", "clear", "titles"], calls);
            Assert.NotNull(tooltip);
            Assert.Equal(PlacementMode.Mouse, tooltip!.Placement);
            Assert.True(tooltip.StaysOpen);
            Assert.False(main.AxisX[0].ShowLabels);
            Assert.False(main.AxisY[0].ShowLabels);
            Assert.False(normalized.AxisX[0].ShowLabels);
            Assert.False(normalized.AxisY[0].ShowLabels);
            Assert.False(diffRatio.AxisX[0].ShowLabels);
            Assert.False(diffRatio.AxisY[0].ShowLabels);
            Assert.False(distribution.AxisX[0].ShowLabels);
            Assert.False(distribution.AxisY[0].ShowLabels);
        });
    }

    private static CartesianChart CreateChart()
    {
        var chart = new CartesianChart();
        chart.AxisX.Add(new Axis { ShowLabels = true });
        chart.AxisY.Add(new Axis { ShowLabels = true });
        return chart;
    }
}
