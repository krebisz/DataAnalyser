using DataVisualiser.VNext.Rendering;

namespace DataVisualiser.Core.Rendering.WeekdayTrend;

public sealed class WeekdayTrendRenderPlanAdapter : IChartRenderPlanAdapter<WeekdayTrendRenderSurface>
{
    private readonly Action<WeekdayTrendChartRenderRequest, WeekdayTrendChartRenderHost> _render;

    public WeekdayTrendRenderPlanAdapter(Action<WeekdayTrendChartRenderRequest, WeekdayTrendChartRenderHost> render)
    {
        _render = render ?? throw new ArgumentNullException(nameof(render));
    }

    public ChartBackendCapabilities Capabilities => ChartBackendCapabilities.LiveChartsWpf;

    public bool CanRender(ChartRenderPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return ChartRenderPlanAdapterQualificationRules.CanRender(Capabilities, plan);
    }

    public ValueTask<ChartRenderAdapterResult> ApplyAsync(
        WeekdayTrendRenderSurface surface,
        ChartRenderPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(surface);
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        _render(surface.Request, surface.Host);

        var activeChart = surface.Request.Route == WeekdayTrendRenderingRoute.Polar
            ? surface.Host.PolarChart
            : surface.Host.CartesianChart;
        var seriesCount = activeChart.Series.OfType<LiveCharts.Wpf.Series>().Count();
        var pointCount = activeChart.Series.OfType<LiveCharts.Wpf.Series>().Sum(series => series.Values?.Count ?? 0);

        return ValueTask.FromResult(new ChartRenderAdapterResult(
            ResolveBackendKey(plan),
            plan.Id,
            plan.PlanKind,
            plan.Density.Mode,
            seriesCount,
            0,
            pointCount,
            plan.Metadata));
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
