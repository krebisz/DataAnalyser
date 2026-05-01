using DataVisualiser.VNext.Contracts;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.Syncfusion;

public sealed class SyncfusionSunburstRenderingContract : ISyncfusionSunburstRenderingContract
{
    private readonly ChartRenderPlanAdapterDispatcher<SyncfusionSunburstRenderSurface> _dispatcher;

    public SyncfusionSunburstRenderingContract(
        ChartRenderPlanAdapterDispatcher<SyncfusionSunburstRenderSurface>? dispatcher = null)
    {
        _dispatcher = dispatcher
            ?? new ChartRenderPlanAdapterDispatcher<SyncfusionSunburstRenderSurface>([new SyncfusionSunburstRenderPlanAdapter()]);
    }

    public async Task<ChartRenderAdapterResult> RenderAsync(SyncfusionSunburstChartRenderRequest request, SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(host);

        var plan = SyncfusionSunburstRenderPlanBuilder.Build(request);
        var consumptionContract = request.ConsumptionContract
            ?? SyncfusionSunburstVNextConsumptionContractBuilder.Build(request, plan);
        plan = ChartRenderPlanConsumptionContractMetadata.Attach(plan, consumptionContract);

        return await _dispatcher.ApplyAsync(
            new SyncfusionSunburstRenderSurface(host.Target, host.IsVisible),
            plan);
    }

    public void Clear(SyncfusionSunburstChartRenderHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        host.Target.SetItems(Array.Empty<SyncfusionSunburstItem>());
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

        return host.Target.HasItems;
    }
}
