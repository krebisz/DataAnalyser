using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Core.Orchestration;

public sealed class TransformChartRenderInvoker : ITransformChartRenderInvoker
{
    private readonly ChartUpdateCoordinator _chartUpdateCoordinator;

    public TransformChartRenderInvoker(ChartUpdateCoordinator chartUpdateCoordinator)
    {
        _chartUpdateCoordinator = chartUpdateCoordinator ?? throw new ArgumentNullException(nameof(chartUpdateCoordinator));
    }

    public Task RenderAsync(TransformChartRenderRequest request, TransformChartRenderHost host)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        if (host == null)
            throw new ArgumentNullException(nameof(host));

        return RenderAndCaptureDiagnosticsAsync(request, host, request.Context);
    }

    private async Task RenderAndCaptureDiagnosticsAsync(
        TransformChartRenderRequest request,
        TransformChartRenderHost host,
        ChartDataContext context)
    {
        await _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
            host.Chart,
            request.Strategy,
            request.PrimaryLabel,
            null,
            request.MinHeight,
            context.PrimaryMetricType ?? context.MetricType,
            context.PrimarySubtype,
            context.SecondarySubtype,
            request.OperationType,
            request.IsOperationChart,
            context.SecondaryMetricType,
            context.DisplayPrimaryMetricType,
            context.DisplaySecondaryMetricType,
            context.DisplayPrimarySubtype,
            context.DisplaySecondarySubtype,
            useRenderPlanAdapter: true,
            renderProgramKind: ChartProgramKind.Transform);

        if (_chartUpdateCoordinator.LastRenderPlanAdapterResult != null)
            host.ChartState.SetRenderPlanDiagnostics(
                ChartProgramKind.Transform,
                _chartUpdateCoordinator.LastRenderPlanAdapterResult);
    }
}
