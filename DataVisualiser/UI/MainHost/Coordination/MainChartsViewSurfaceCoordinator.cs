using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CartesianChart = LiveCharts.Wpf.CartesianChart;

namespace DataVisualiser.UI.MainHost.Coordination;

public sealed class MainChartsViewSurfaceCoordinator
{
    public sealed record Actions(
        Func<CartesianChart> GetMainChart,
        Func<CartesianChart> GetNormalizedChart,
        Func<CartesianChart> GetDiffRatioChart,
        Func<CartesianChart> GetDistributionChart,
        Action InitializeChartBehavior,
        Action InitializeDistributionChartBehavior,
        Action ClearChartsOnStartup,
        Action SetDefaultChartTitles,
        Action<ToolTip> SetDistributionPolarTooltip);

    public void InitializeSurfaces(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        actions.InitializeChartBehavior();
        actions.InitializeDistributionChartBehavior();
        actions.ClearChartsOnStartup();
        DisableAxisLabelsWhenNoData(actions);
        actions.SetDefaultChartTitles();
        actions.SetDistributionPolarTooltip(CreateDistributionPolarTooltip());
    }

    private static void DisableAxisLabelsWhenNoData(Actions actions)
    {
        DisableAxisLabels(actions.GetMainChart());
        DisableAxisLabels(actions.GetNormalizedChart());
        DisableAxisLabels(actions.GetDiffRatioChart());
        DisableAxisLabels(actions.GetDistributionChart());
    }

    private static ToolTip CreateDistributionPolarTooltip()
    {
        return new ToolTip
        {
            Placement = PlacementMode.Mouse,
            StaysOpen = true
        };
    }

    private static void DisableAxisLabels(CartesianChart chart)
    {
        if (chart.AxisX.Count > 0)
            chart.AxisX[0].ShowLabels = false;
        if (chart.AxisY.Count > 0)
            chart.AxisY[0].ShowLabels = false;
    }
}
