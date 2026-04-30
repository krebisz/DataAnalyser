using DataVisualiser.Core.Rendering.Transform;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Transforms;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.ViewModels;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformDataPanelControllerAdapterCompositionFactory
{
    public static TransformDataPanelControllerAdapterComposition Create(
        ITransformDataPanelController controller,
        MainWindowViewModel viewModel,
        MetricSelectionService metricSelectionService,
        ITransformRenderingContract transformRenderingContract,
        TransformComputationService? transformComputationService = null,
        VNextSeriesLoadCoordinator? vnextCoordinator = null)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(metricSelectionService);
        ArgumentNullException.ThrowIfNull(transformRenderingContract);

        var selectionCache = new MetricSeriesSelectionCache();
        var computationService = transformComputationService ?? new TransformComputationService();
        var dataResolutionCoordinator = new TransformDataResolutionCoordinator(
            controller,
            viewModel,
            metricSelectionService,
            selectionCache,
            vnextCoordinator);
        var operationExecutionCoordinator = new TransformOperationExecutionCoordinator(computationService);
        var operationStateCoordinator = new TransformOperationStateCoordinator();
        var renderCoordinator = new TransformRenderCoordinator(
            controller,
            viewModel.ChartState,
            transformRenderingContract);
        var selectionInteractionCoordinator = new TransformSelectionInteractionCoordinator();
        var sessionMilestoneRecorder = new TransformSessionMilestoneRecorder(viewModel);
        var workflowCoordinator = new TransformWorkflowCoordinator(
            dataResolutionCoordinator,
            operationExecutionCoordinator,
            renderCoordinator,
            sessionMilestoneRecorder);

        return new TransformDataPanelControllerAdapterComposition(
            selectionCache,
            dataResolutionCoordinator,
            operationExecutionCoordinator,
            operationStateCoordinator,
            renderCoordinator,
            selectionInteractionCoordinator,
            sessionMilestoneRecorder,
            workflowCoordinator);
    }
}

internal sealed record TransformDataPanelControllerAdapterComposition(
    MetricSeriesSelectionCache SelectionCache,
    TransformDataResolutionCoordinator DataResolutionCoordinator,
    TransformOperationExecutionCoordinator OperationExecutionCoordinator,
    TransformOperationStateCoordinator OperationStateCoordinator,
    TransformRenderCoordinator RenderCoordinator,
    TransformSelectionInteractionCoordinator SelectionInteractionCoordinator,
    TransformSessionMilestoneRecorder SessionMilestoneRecorder,
    TransformWorkflowCoordinator WorkflowCoordinator);
