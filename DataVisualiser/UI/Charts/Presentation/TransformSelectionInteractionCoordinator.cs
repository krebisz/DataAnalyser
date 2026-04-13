using System.Windows.Controls;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Charts.Presentation;

internal sealed class TransformSelectionInteractionCoordinator
{
    public async Task HandleSelectionChangedAsync(
        bool isInitializing,
        bool isUpdatingTransformSubtypeCombos,
        ComboBox selectionCombo,
        Action<MetricSeriesSelection?> applySelection,
        Action updateComputeButtonState,
        Func<Task> refreshTransformGridsFromSelectionAsync)
    {
        ArgumentNullException.ThrowIfNull(selectionCombo);
        ArgumentNullException.ThrowIfNull(applySelection);
        ArgumentNullException.ThrowIfNull(updateComputeButtonState);
        ArgumentNullException.ThrowIfNull(refreshTransformGridsFromSelectionAsync);

        if (isInitializing || isUpdatingTransformSubtypeCombos)
            return;

        var selection = MetricSeriesSelectionCache.GetSeriesSelectionFromCombo(selectionCombo);
        applySelection(selection);
        updateComputeButtonState();
        await refreshTransformGridsFromSelectionAsync();
    }
}
