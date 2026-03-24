using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public static class WeekdayTrendRenderingRouteResolver
{
    public static WeekdayTrendRenderingRoute Resolve(WeekdayTrendChartMode mode)
    {
        return mode switch
        {
            WeekdayTrendChartMode.Polar => WeekdayTrendRenderingRoute.Polar,
            WeekdayTrendChartMode.Scatter => WeekdayTrendRenderingRoute.Scatter,
            _ => WeekdayTrendRenderingRoute.Cartesian
        };
    }
}
