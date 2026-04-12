using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.Charts.Interfaces;
using System.Windows.Controls;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformOperationStateCoordinator
{
    public void UpdateComputeButtonState(
        ITransformDataPanelController controller,
        ChartDataContext? context,
        bool isSelectionPendingLoad,
        Func<ChartDataContext, TransformSelectionResolution> selectionResolver,
        TransformOperationExecutionCoordinator executionCoordinator)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(selectionResolver);
        ArgumentNullException.ThrowIfNull(executionCoordinator);

        if (isSelectionPendingLoad || context == null)
        {
            controller.TransformComputeButton.IsEnabled = false;
            return;
        }

        var operationTag = GetSelectedOperationTag(controller);
        if (string.IsNullOrWhiteSpace(operationTag))
        {
            controller.TransformComputeButton.IsEnabled = TransformDataResolutionCoordinator.CanRenderPrimarySelection(context);
            return;
        }

        var selection = selectionResolver(context);
        controller.TransformComputeButton.IsEnabled = executionCoordinator.CanExecute(context, selection, operationTag);
    }

    public string? GetSelectedOperationTag(ITransformDataPanelController controller)
    {
        ArgumentNullException.ThrowIfNull(controller);
        return controller.TransformOperationCombo.SelectedItem is ComboBoxItem item ? item.Tag?.ToString() : null;
    }
}
