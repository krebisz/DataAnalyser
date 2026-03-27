namespace DataVisualiser.UI.Charts.Infrastructure;

internal static class RenderingHostLifecycleAdapterHelper
{
    public static RenderingHostTarget<TRoute, THost> CreateTarget<TRoute, THost>(TRoute route, Func<THost> createHost)
    {
        return new RenderingHostTarget<TRoute, THost>(route, createHost());
    }

    public static RenderingHostTarget<TRoute, THost> CreateTarget<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost)
    {
        return CreateTarget(resolveRoute(), createHost);
    }

    public static void Clear<THost>(Func<THost> createHost, Action<THost> clear)
    {
        clear(createHost());
    }

    public static void Clear<TRoute, THost>(TRoute route, Func<THost> createHost, Action<TRoute, THost> clear)
    {
        var target = CreateTarget(route, createHost);
        clear(target.Route, target.Host);
    }

    public static void ResetView<TRoute, THost>(TRoute route, Func<THost> createHost, Action<TRoute, THost> resetView)
    {
        var target = CreateTarget(route, createHost);
        resetView(target.Route, target.Host);
    }

    public static void ResetView<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost, Action<TRoute, THost> resetView)
    {
        ResetView(resolveRoute(), createHost, resetView);
    }

    public static bool HasRenderableContent<TRoute, THost>(TRoute route, Func<THost> createHost, Func<TRoute, THost, bool> hasRenderableContent)
    {
        var target = CreateTarget(route, createHost);
        return hasRenderableContent(target.Route, target.Host);
    }

    public static bool HasRenderableContent<TRoute, THost>(Func<TRoute> resolveRoute, Func<THost> createHost, Func<TRoute, THost, bool> hasRenderableContent)
    {
        return HasRenderableContent(resolveRoute(), createHost, hasRenderableContent);
    }
}
