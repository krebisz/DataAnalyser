namespace DataVisualiser.UI.Charts.Infrastructure;

internal static class RenderingHostLifecycleAdapterHelper
{
    public static RenderingHostTarget<TRoute, THost> CreateTarget<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost)
    {
        return new RenderingHostTarget<TRoute, THost>(resolveRoute(), createHost());
    }

    public static void Clear<THost>(Func<THost> createHost, Action<THost> clear)
    {
        clear(createHost());
    }

    public static void ResetView<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost, Action<TRoute, THost> resetView)
    {
        var target = CreateTarget(resolveRoute, createHost);
        resetView(target.Route, target.Host);
    }

    public static bool HasRenderableContent<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost, Func<TRoute, THost, bool> hasRenderableContent)
    {
        var target = CreateTarget(resolveRoute, createHost);
        return hasRenderableContent(target.Route, target.Host);
    }
}
