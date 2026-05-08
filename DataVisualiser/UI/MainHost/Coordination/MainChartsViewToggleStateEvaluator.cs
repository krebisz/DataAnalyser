using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost.Coordination;

public static class MainChartsViewToggleStateEvaluator
{
    public static bool HasRenderableLoadedData(LoadedChartDataSnapshot snapshot)
    {
        return snapshot.Present && snapshot.Data1Count > 0;
    }

    public static bool HasLoadedSecondaryData(LoadedChartDataSnapshot snapshot)
    {
        return snapshot.Present && snapshot.Data2Count > 0;
    }

    public static bool CanTogglePrimaryCharts(LoadedChartDataSnapshot snapshot)
    {
        return HasRenderableLoadedData(snapshot);
    }

    public static bool CanToggleSecondaryCharts(LoadedChartDataSnapshot snapshot)
    {
        return HasLoadedSecondaryData(snapshot);
    }

    public static bool CanUseStackedDisplay(LoadedChartDataSnapshot snapshot, int selectedSubtypeCount)
    {
        return HasRenderableLoadedData(snapshot)
            ? HasLoadedSecondaryData(snapshot)
            : selectedSubtypeCount >= 2;
    }
}
