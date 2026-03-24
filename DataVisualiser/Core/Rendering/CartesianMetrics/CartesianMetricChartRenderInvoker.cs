using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed class CartesianMetricChartRenderInvoker : ICartesianMetricChartRenderInvoker
{
    private readonly Func<ChartRenderingOrchestrator?> _getChartRenderingOrchestrator;

    public CartesianMetricChartRenderInvoker(Func<ChartRenderingOrchestrator?> getChartRenderingOrchestrator)
    {
        _getChartRenderingOrchestrator = getChartRenderingOrchestrator ?? throw new ArgumentNullException(nameof(getChartRenderingOrchestrator));
    }

    public Task RenderAsync(CartesianMetricChartRenderRequest request, CartesianMetricChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        var orchestrator = _getChartRenderingOrchestrator();
        if (orchestrator == null)
            return Task.CompletedTask;

        return request.Route switch
        {
            CartesianMetricChartRoute.Main => RenderMainAsync(orchestrator, request, host),
            CartesianMetricChartRoute.Normalized => orchestrator.RenderNormalizedChartAsync(request.Context, host.Chart, host.ChartState),
            CartesianMetricChartRoute.DiffRatio => orchestrator.RenderDiffRatioChartAsync(request.Context, host.Chart, host.ChartState),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Route), request.Route, "Unknown cartesian metric chart rendering route.")
        };
    }

    private static Task RenderMainAsync(
        ChartRenderingOrchestrator orchestrator,
        CartesianMetricChartRenderRequest request,
        CartesianMetricChartRenderHost host)
    {
        var context = request.Context;
        if (context.Data1 == null)
            return Task.CompletedTask;

        return orchestrator.RenderPrimaryChartAsync(
            context,
            host.Chart,
            context.Data1,
            context.Data2,
            context.DisplayName1 ?? string.Empty,
            context.DisplayName2 ?? string.Empty,
            context.From,
            context.To,
            context.MetricType,
            request.SelectedSeries,
            request.ResolutionTableName,
            request.IsStacked,
            request.IsCumulative,
            request.OverlaySeries);
    }
}
