using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.Distribution;

public sealed class DistributionRenderPlanAdapter : IChartRenderPlanAdapter<DistributionRenderSurface>
{
    private readonly Func<DistributionChartRenderRequest, DistributionChartRenderHost, Task> _renderAsync;

    public DistributionRenderPlanAdapter(Func<DistributionChartRenderRequest, DistributionChartRenderHost, Task> renderAsync)
    {
        _renderAsync = renderAsync ?? throw new ArgumentNullException(nameof(renderAsync));
    }

    public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.LiveChartsWpf;

    public bool CanRender(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return Capabilities.Supports(plan.PlanKind);
    }

    public async ValueTask<ChartRenderAdapterResult> ApplyAsync(
        DistributionRenderSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        await _renderAsync(surface.Request, surface.Host);

        var activeChart = surface.Host.CartesianChart;
        var seriesCount = activeChart.Series.OfType<LiveCharts.Wpf.Series>().Count();
        var pointCount = activeChart.Series.OfType<LiveCharts.Wpf.Series>().Sum(series => series.Values?.Count ?? 0);

        return new ChartRenderAdapterResult(
            ResolveBackendKey(plan),
            plan.Id,
            plan.PlanKind,
            plan.Density.Mode,
            seriesCount,
            0,
            pointCount,
            plan.Metadata);
    }

    private static string ResolveBackendKey(ChartRenderPlan plan)
    {
        if (plan.Metadata.TryGetValue(ChartRenderPlanMetadataKeys.BackendKey, out var backendKey) &&
            !string.IsNullOrWhiteSpace(backendKey))
        {
            return backendKey;
        }

        return ChartBackendCapabilities.LiveChartsWpf.BackendKey;
    }
}
