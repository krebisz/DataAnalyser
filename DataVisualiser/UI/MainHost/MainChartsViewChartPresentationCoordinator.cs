using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartPresentationCoordinator
{
    public void ApplyDefaultTitles(MainChartsViewChartPresentationActions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetMainChartTitle("Metrics: Total");
        actions.SetNormalizedChartTitle("Metrics: Normalized");
        actions.SetDiffRatioChartTitle("Difference / Ratio");
    }

    public void UpdateTitlesFromSelections(
        IReadOnlyList<MetricSeriesSelection> selections,
        bool isDiffRatioDifferenceMode,
        MainChartsViewChartPresentationActions actions)
    {
        ArgumentNullException.ThrowIfNull(selections);
        ArgumentNullException.ThrowIfNull(actions);

        var leftName = selections.Count > 0 ? selections[0].DisplayName ?? string.Empty : string.Empty;
        var rightName = selections.Count > 1 ? selections[1].DisplayName ?? string.Empty : string.Empty;
        var diffOperator = isDiffRatioDifferenceMode ? "-" : "/";

        actions.SetChartStateTitles(leftName, rightName);
        actions.SetMainChartTitle($"{leftName} vs. {rightName}");
        actions.SetNormalizedChartTitle($"{leftName} ~ {rightName}");
        actions.SetDiffRatioChartTitle($"{leftName} {diffOperator} {rightName}");

        var mainLabel = !string.IsNullOrEmpty(rightName) ? $"{leftName} vs {rightName}" : leftName;
        actions.UpdateMainChartLabel(mainLabel);

        var diffRatioLabel = !string.IsNullOrEmpty(rightName) ? $"{leftName} {diffOperator} {rightName}" : leftName;
        actions.UpdateDiffRatioChartLabel(diffRatioLabel);
    }

    public void ClearHiddenCharts(ChartState chartState, MainChartsViewChartPresentationActions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var key in ChartVisibilityHelper.GetHiddenChartKeys(chartState))
            actions.ClearChart(key);
    }
}
