using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.Charts.Interfaces;

public interface ITransformPanelControllerExtras
{
    void CompleteSelectionsPendingLoad();
    void ResetSelectionsPendingLoad();
    void HandleVisibilityOnlyToggle(ChartDataContext? context);
    void UpdateTransformSubtypeOptions();
    void UpdateTransformComputeButtonState();
}
