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
            new ChartUpdateRequest
            {
                PrimaryLabel = request.PrimaryLabel,
                MinHeight = request.MinHeight,
                MetricType = context.PrimaryMetricType ?? context.MetricType,
                PrimarySubtype = context.PrimarySubtype,
                SecondarySubtype = context.SecondarySubtype,
                OperationType = request.OperationType,
                IsOperationChart = request.IsOperationChart,
                SecondaryMetricType = context.SecondaryMetricType,
                DisplayPrimaryMetricType = context.DisplayPrimaryMetricType,
                DisplaySecondaryMetricType = context.DisplaySecondaryMetricType,
                DisplayPrimarySubtype = context.DisplayPrimarySubtype,
                DisplaySecondarySubtype = context.DisplaySecondarySubtype,
                RenderProgramKind = ChartProgramKind.Transform,
                RenderProgramRequest = request.CapabilityContract?.ProgramRequest,
                RenderCapability = request.CapabilityContract?.Capability,
                RenderDelivery = request.CapabilityContract?.Delivery,
                RenderConsumptionContractFactory = plan => TransformVNextConsumptionContractBuilder.Build(request, plan)
            });

        if (_chartUpdateCoordinator.LastRenderPlanAdapterResult != null)
            host.ChartState.SetRenderPlanDiagnostics(
                ChartProgramKind.Transform,
                _chartUpdateCoordinator.LastRenderPlanAdapterResult);
    }
}
