using DataVisualiser.UI.Charts.Presentation.Rendering;
using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.Adapters;

public sealed record UiChartRenderSurface(
    IChartSurface Surface,
    IChartRenderer Renderer,
    ChartRendererKind RendererKind,
    UiChartRenderModel Model);

public sealed class UiChartRenderPlanAdapter : IChartRenderPlanAdapter<UiChartRenderSurface>
{
    public const string BackendKeyMetadataKey = "BackendKey";

    public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.LiveChartsWpf;

    public bool CanRender(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return plan.PlanKind is ChartRenderPlanKind.Cartesian or ChartRenderPlanKind.Faceted;
    }

    public async ValueTask<ChartRenderAdapterResult> ApplyAsync(
        UiChartRenderSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        await surface.Renderer.ApplyAsync(surface.Surface, surface.Model, cancellationToken);

        return new ChartRenderAdapterResult(
            ResolveBackendKey(surface.RendererKind, plan),
            plan.Id,
            plan.PlanKind,
            plan.Density.Mode,
            surface.Model.Series.Count,
            surface.Model.Facets.Count,
            surface.Model.Series.Sum(series => series.Values.Count) +
            surface.Model.Facets.Sum(facet => facet.Series.Sum(series => series.Values.Count)),
            plan.Metadata);
    }

    private static string ResolveBackendKey(ChartRendererKind rendererKind, ChartRenderPlan plan)
    {
        if (plan.Metadata.TryGetValue(BackendKeyMetadataKey, out var backendKey) &&
            !string.IsNullOrWhiteSpace(backendKey))
        {
            return backendKey;
        }

        var renderer = rendererKind switch
        {
            ChartRendererKind.ECharts => "ECharts.Placeholder",
            _ => "LiveChartsWpf"
        };

        return $"{renderer}.{plan.PlanKind}";
    }
}
