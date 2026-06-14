using DataVisualiser.Core.Computation;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Rendering.Engines;
using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed record TransformRenderingPipeline(
    ChartState ChartState,
    ITransformRenderingContract RenderingContract);

internal static class TransformRenderingPipelineFactory
{
    public static TransformRenderingPipeline CreateIsolated()
    {
        var chartState = new ChartState();
        return new TransformRenderingPipeline(chartState, CreateContract(chartState));
    }

    public static ITransformRenderingContract CreateContract(ChartState chartState)
    {
        ArgumentNullException.ThrowIfNull(chartState);

        var coordinator = new ChartUpdateCoordinator(
            new ChartComputationEngine(),
            new ChartRenderEngine(),
            tooltipManager: null,
            chartState.ChartTimestamps,
            MessageBoxUserNotificationService.Instance);

        return new TransformRenderingContract(new TransformChartRenderInvoker(coordinator));
    }
}
