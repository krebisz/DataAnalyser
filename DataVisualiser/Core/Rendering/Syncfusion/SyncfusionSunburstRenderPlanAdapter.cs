using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.Syncfusion;

public sealed class SyncfusionSunburstRenderPlanAdapter : IChartRenderPlanAdapter<SyncfusionSunburstRenderSurface>
{
    public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.SyncfusionSunburst;

    public bool CanRender(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return ChartRenderPlanAdapterQualificationRules.CanRender(Capabilities, plan);
    }

    public ValueTask<ChartRenderAdapterResult> ApplyAsync(
        SyncfusionSunburstRenderSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        var items = surface.IsVisible
            ? FlattenHierarchy(plan.HierarchyRoots)
            : Array.Empty<SyncfusionSunburstItem>();

        surface.Target.SetItems(items);

        return ValueTask.FromResult(new ChartRenderAdapterResult(
            SyncfusionSunburstBackendKey.SyncfusionWpfHierarchy,
            plan.Id,
            plan.PlanKind,
            plan.Density.Mode,
            RenderedSeriesCount: CountDistinctSubmetrics(items),
            RenderedHierarchyNodeCount: plan.Density.RenderedPointCount,
            RenderedPointCount: items.Count,
            plan.Metadata));
    }

    private static IReadOnlyList<SyncfusionSunburstItem> FlattenHierarchy(IReadOnlyList<ChartHierarchyNodePlan> roots)
    {
        var items = new List<SyncfusionSunburstItem>();
        foreach (var root in roots)
        {
            foreach (var child in root.Children)
                items.Add(new SyncfusionSunburstItem(root.Label, child.Label, child.Value));
        }

        return items;
    }

    private static int CountDistinctSubmetrics(IReadOnlyList<SyncfusionSunburstItem> items)
    {
        return items
            .Select(item => item.Submetric)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }
}
