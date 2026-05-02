using DataVisualiser.UI.State;

namespace DataVisualiser.UI.ViewModels;

internal static class ChartLoadPolicy
{
    public static bool ShouldUseMainFamilyPath(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        return chartState.IsMainVisible &&
               !chartState.IsTransformPanelVisible;
    }

    public static bool SupportsOnlyMainChart(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        return chartState.IsMainVisible &&
               !chartState.IsNormalizedVisible &&
               !chartState.IsDiffRatioVisible;
    }
}
