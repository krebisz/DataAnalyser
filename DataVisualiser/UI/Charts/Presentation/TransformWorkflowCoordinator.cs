using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformWorkflowCoordinator(
    TransformDataResolutionCoordinator transformDataResolutionCoordinator,
    TransformOperationExecutionCoordinator transformOperationExecutionCoordinator,
    TransformRenderCoordinator transformRenderCoordinator,
    TransformSessionMilestoneRecorder transformSessionMilestoneRecorder)
{
    private readonly TransformDataResolutionCoordinator _transformDataResolutionCoordinator = transformDataResolutionCoordinator ?? throw new ArgumentNullException(nameof(transformDataResolutionCoordinator));
    private readonly TransformOperationExecutionCoordinator _transformOperationExecutionCoordinator = transformOperationExecutionCoordinator ?? throw new ArgumentNullException(nameof(transformOperationExecutionCoordinator));
    private readonly TransformRenderCoordinator _transformRenderCoordinator = transformRenderCoordinator ?? throw new ArgumentNullException(nameof(transformRenderCoordinator));
    private readonly TransformSessionMilestoneRecorder _transformSessionMilestoneRecorder = transformSessionMilestoneRecorder ?? throw new ArgumentNullException(nameof(transformSessionMilestoneRecorder));

    internal async Task ExecuteOperationAsync(ChartDataContext context, bool isSelectionPendingLoad, string? operationTag)
    {
        ArgumentNullException.ThrowIfNull(context);

        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(context, isSelectionPendingLoad);
        var execution = _transformOperationExecutionCoordinator.Execute(resolution, operationTag);
        if (execution == null)
            return;

        await RenderResultsAsync(execution, resolution);
    }

    internal async Task RenderPrimarySelectionAsync(ChartDataContext context, bool isSelectionPendingLoad)
    {
        ArgumentNullException.ThrowIfNull(context);

        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(context, isSelectionPendingLoad);
        var execution = _transformOperationExecutionCoordinator.Execute(resolution, null);
        if (execution == null)
            return;

        await RenderResultsAsync(execution, resolution);
    }

    internal async Task RefreshFromSelectionAsync(
        ChartDataContext? context,
        bool isSelectionPendingLoad,
        Action<TransformResolutionResult> populateTransformGrids,
        Action updateTransformComputeButtonState)
    {
        ArgumentNullException.ThrowIfNull(populateTransformGrids);
        ArgumentNullException.ThrowIfNull(updateTransformComputeButtonState);

        if (context == null)
            return;

        var resolution = await _transformDataResolutionCoordinator.ResolveAsync(context, isSelectionPendingLoad);
        populateTransformGrids(resolution);
        updateTransformComputeButtonState();
    }

    private async Task RenderResultsAsync(TransformExecutionResult execution, TransformResolutionResult resolution)
    {
        await _transformRenderCoordinator.RenderResultsAsync(execution, resolution);
        _transformSessionMilestoneRecorder.RecordExecution(execution, resolution);
    }
}
