namespace DataVisualiser.Core.Rendering.Distribution;

public static class DistributionRenderingRouteResolver
{
    public static DistributionRenderingRoute Resolve(bool isPolarMode)
    {
        return isPolarMode
            ? DistributionRenderingRoute.PolarFallback
            : DistributionRenderingRoute.Cartesian;
    }
}
