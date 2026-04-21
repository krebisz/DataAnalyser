using DataVisualiser.UI.Syncfusion;

namespace DataVisualiser.Core.Rendering.Syncfusion;

public sealed class SyncfusionSunburstRenderingContract : ISyncfusionSunburstRenderingContract
{
    public Task RenderAsync(SyncfusionSunburstChartRenderRequest request, SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(host);

        host.Controller.ItemsSource = host.IsVisible
            ? request.Items
            : Array.Empty<SunburstItem>();

        return Task.CompletedTask;
    }

    public void Clear(SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        host.Controller.ItemsSource = Array.Empty<SunburstItem>();
    }

    public void ResetView(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (route != SyncfusionSunburstRenderingRoute.Hierarchy)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown Syncfusion Sunburst rendering route.");
    }

    public bool HasRenderableContent(SyncfusionSunburstRenderingRoute route, SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        if (route != SyncfusionSunburstRenderingRoute.Hierarchy)
            throw new ArgumentOutOfRangeException(nameof(route), route, "Unknown Syncfusion Sunburst rendering route.");

        return host.Controller.ItemsSource is IEnumerable<SunburstItem> items && items.Any();
    }
}
