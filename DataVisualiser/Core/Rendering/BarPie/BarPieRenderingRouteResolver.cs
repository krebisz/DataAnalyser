namespace DataVisualiser.Core.Rendering.BarPie;

public static class BarPieRenderingRouteResolver
{
    public static BarPieRenderingRoute Resolve(bool isPieMode)
    {
        return isPieMode ? BarPieRenderingRoute.PieFacet : BarPieRenderingRoute.Column;
    }
}
