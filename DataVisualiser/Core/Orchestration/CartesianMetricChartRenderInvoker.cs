using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

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
        var data1 = request.Context.Data1;
        if (data1 == null)
            return Task.CompletedTask;

        return RenderMainAndCaptureContextAsync(orchestrator, request, host, data1);
    }

    private static async Task RenderMainAndCaptureContextAsync(
        ChartRenderingOrchestrator orchestrator,
        CartesianMetricChartRenderRequest request,
        CartesianMetricChartRenderHost host,
        IEnumerable<MetricData> data1)
    {
        var context = request.Context;
        var renderedContext = await orchestrator.RenderPrimaryChartAsync(
            host.Chart,
            new PrimaryChartRenderRequest
            {
                Context = context,
                Data1 = data1,
                Data2 = context.Data2,
                DisplayName1 = context.DisplayName1 ?? string.Empty,
                DisplayName2 = context.DisplayName2 ?? string.Empty,
                From = context.From,
                To = context.To,
                MetricType = context.MetricType,
                SelectedSeries = request.SelectedSeries,
                ResolutionTableName = request.ResolutionTableName,
                IsStacked = request.IsStacked,
                IsCumulative = request.IsCumulative,
                OverlaySeries = request.OverlaySeries,
                CapabilityContract = request.CapabilityContract
            });

        if (renderedContext != null)
            host.ChartState.LastContext = renderedContext;

        if (orchestrator.LastRenderPlanAdapterResult != null)
            host.ChartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                orchestrator.LastRenderPlanAdapterResult);
    }
}
