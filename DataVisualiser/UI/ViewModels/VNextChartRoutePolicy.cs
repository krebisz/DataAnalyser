using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

internal static class VNextChartRoutePolicy
{
    public static bool ShouldUseMainFamilyPath(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        return chartState.IsMainVisible &&
               !chartState.IsDistributionVisible &&
               !chartState.IsWeeklyTrendVisible &&
               !chartState.IsTransformPanelVisible &&
               !chartState.IsBarPieVisible;
    }

    public static bool SupportsOnlyMainChart(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        return chartState.IsMainVisible &&
               !chartState.IsNormalizedVisible &&
               !chartState.IsDiffRatioVisible;
    }
}
