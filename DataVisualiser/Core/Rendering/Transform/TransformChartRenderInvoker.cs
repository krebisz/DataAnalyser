using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.Core.Rendering.Transform;

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

        var context = request.Context;
        return _chartUpdateCoordinator.UpdateChartUsingStrategyAsync(
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
            context.DisplaySecondarySubtype);
    }
}
