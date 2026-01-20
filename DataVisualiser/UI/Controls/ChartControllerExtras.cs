using DataVisualiser.Core.Orchestration;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.Controls;

public interface IChartSubtypeOptionsController
{
    void UpdateSubtypeOptions();
}

public interface IChartCacheController
{
    void ClearCache();
}

public interface IChartSeriesAvailability
{
    bool HasSeries(ChartState state);
}

public interface IDistributionChartControllerExtras
{
    void InitializeControls();
    void UpdateChartTypeVisibility();
}

public interface IWeekdayTrendChartControllerExtras
{
    void InitializeControls();
    void UpdateChartTypeVisibility();
}

public interface ITransformPanelControllerExtras
{
    void CompleteSelectionsPendingLoad();
    void ResetSelectionsPendingLoad();
    void HandleVisibilityOnlyToggle(ChartDataContext? context);
    void UpdateTransformSubtypeOptions();
    void UpdateTransformComputeButtonState();
}

public interface IDiffRatioChartControllerExtras
{
    void UpdateOperationButton();
}

public interface IMainChartControllerExtras
{
    void SyncDisplayModeSelection();
}
