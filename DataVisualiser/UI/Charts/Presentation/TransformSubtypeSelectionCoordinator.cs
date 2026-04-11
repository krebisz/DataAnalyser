using System.Windows;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Interfaces;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Charts.Presentation;

internal static class TransformSubtypeSelectionCoordinator
{
    public static bool CanUpdateSubtypeOptions(ITransformDataPanelController controller, bool isTransformSelectionPendingLoad)
    {
        if (controller.TransformPrimarySubtypeCombo == null || controller.TransformSecondarySubtypeCombo == null)
            return false;

        return !isTransformSelectionPendingLoad;
    }

    public static void ApplySubtypeOptions(ITransformDataPanelController controller, ChartState chartState, IReadOnlyList<MetricSeriesSelection> selectedSeries, Action<bool> setBinaryTransformOperationsEnabled)
    {
        controller.TransformPrimarySubtypeCombo.Items.Clear();
        controller.TransformSecondarySubtypeCombo.Items.Clear();

        if (selectedSeries.Count == 0)
        {
            HandleNoSelectedSeries(controller, chartState);
            return;
        }

        ChartSubtypeComboHelper.PopulateCombo(controller.TransformPrimarySubtypeCombo, selectedSeries);
        ChartSubtypeComboHelper.PopulateCombo(controller.TransformSecondarySubtypeCombo, selectedSeries);

        var primaryCurrent = chartState.SelectedTransformPrimarySeries;
        var primarySelection = ChartSubtypeComboHelper.ResolveSelection(selectedSeries, primaryCurrent) ?? selectedSeries[0];
        ChartSubtypeComboHelper.SelectComboItem(controller.TransformPrimarySubtypeCombo, primarySelection);
        chartState.SelectedTransformPrimarySeries = primarySelection;

        if (selectedSeries.Count > 1)
        {
            controller.TransformSecondarySubtypePanel.Visibility = Visibility.Visible;
            controller.TransformSecondarySubtypeCombo.IsEnabled = true;
            setBinaryTransformOperationsEnabled(true);

            var secondaryCurrent = chartState.SelectedTransformSecondarySeries;
            var secondarySelection = secondaryCurrent != null && selectedSeries.Any(series => string.Equals(series.DisplayKey, secondaryCurrent.DisplayKey, StringComparison.OrdinalIgnoreCase))
                ? secondaryCurrent
                : selectedSeries[1];

            ChartSubtypeComboHelper.SelectComboItem(controller.TransformSecondarySubtypeCombo, secondarySelection);
            chartState.SelectedTransformSecondarySeries = secondarySelection;
            return;
        }

        controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
        controller.TransformSecondarySubtypeCombo.IsEnabled = false;
        controller.TransformSecondarySubtypeCombo.SelectedItem = null;
        chartState.SelectedTransformSecondarySeries = null;
        setBinaryTransformOperationsEnabled(false);
    }

    public static void ResetSelectionControls(ITransformDataPanelController controller)
    {
        controller.TransformPrimarySubtypeCombo.Items.Clear();
        controller.TransformSecondarySubtypeCombo.Items.Clear();
        controller.TransformPrimarySubtypeCombo.IsEnabled = false;
        controller.TransformSecondarySubtypeCombo.IsEnabled = false;
        controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;
        controller.TransformPrimarySubtypeCombo.SelectedItem = null;
        controller.TransformSecondarySubtypeCombo.SelectedItem = null;
        controller.TransformOperationCombo.SelectedItem = null;
        controller.TransformComputeButton.IsEnabled = false;
    }

    private static void HandleNoSelectedSeries(ITransformDataPanelController controller, ChartState chartState)
    {
        controller.TransformPrimarySubtypeCombo.IsEnabled = false;
        controller.TransformSecondarySubtypeCombo.IsEnabled = false;
        controller.TransformSecondarySubtypePanel.Visibility = Visibility.Collapsed;

        chartState.SelectedTransformPrimarySeries = null;
        chartState.SelectedTransformSecondarySeries = null;

        controller.TransformPrimarySubtypeCombo.SelectedItem = null;
        controller.TransformSecondarySubtypeCombo.SelectedItem = null;
    }
}
