using DataVisualiser.Core.Rendering.Interaction;

namespace DataVisualiser.Core.Rendering.Helpers;

internal static class ChartTooltipSeriesFilter
{
    public static bool IsOverlaySeries(string baseName, ChartStackingTooltipState? state)
    {
        if (state?.OverlaySeriesNames == null || string.IsNullOrWhiteSpace(baseName))
            return false;

        return state.OverlaySeriesNames.Contains(baseName, StringComparer.OrdinalIgnoreCase);
    }
}
