using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.MainHost.Coordination;

public static class MainChartsViewToggleStateEvaluator
{
    public static bool HasRenderableLoadedContext(ChartDataContext? context)
    {
        return context?.Data1 != null && context.Data1.Any();
    }

    public static bool HasLoadedSecondaryData(ChartDataContext? context)
    {
        return context?.Data2 != null && context.Data2.Any();
    }

    public static bool CanTogglePrimaryCharts(ChartDataContext? context)
    {
        return HasRenderableLoadedContext(context);
    }

    public static bool CanToggleSecondaryCharts(ChartDataContext? context)
    {
        return HasLoadedSecondaryData(context);
    }

    public static bool CanUseStackedDisplay(ChartDataContext? context, int selectedSubtypeCount)
    {
        return HasRenderableLoadedContext(context)
            ? HasLoadedSecondaryData(context)
            : selectedSubtypeCount >= 2;
    }
}
