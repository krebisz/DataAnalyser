using DataVisualiser.Shared.Models;
using DataVisualiser.UI.State;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartPresentationCoordinator
{
    public sealed class Actions(
        Action<string, string> setChartStateTitles,
        Action<string> setMainChartTitle,
        Action<string> setNormalizedChartTitle,
        Action<string> setDiffRatioChartTitle,
        Action<string> updateMainChartLabel,
        Action<string> updateDiffRatioChartLabel,
        Action<string> clearChart)
    {
        public Action<string, string> SetChartStateTitles { get; } = setChartStateTitles ?? throw new ArgumentNullException(nameof(setChartStateTitles));
        public Action<string> SetMainChartTitle { get; } = setMainChartTitle ?? throw new ArgumentNullException(nameof(setMainChartTitle));
        public Action<string> SetNormalizedChartTitle { get; } = setNormalizedChartTitle ?? throw new ArgumentNullException(nameof(setNormalizedChartTitle));
        public Action<string> SetDiffRatioChartTitle { get; } = setDiffRatioChartTitle ?? throw new ArgumentNullException(nameof(setDiffRatioChartTitle));
        public Action<string> UpdateMainChartLabel { get; } = updateMainChartLabel ?? throw new ArgumentNullException(nameof(updateMainChartLabel));
        public Action<string> UpdateDiffRatioChartLabel { get; } = updateDiffRatioChartLabel ?? throw new ArgumentNullException(nameof(updateDiffRatioChartLabel));
        public Action<string> ClearChart { get; } = clearChart ?? throw new ArgumentNullException(nameof(clearChart));
    }

    public void ApplyDefaultTitles(Actions actions)
    {
        ArgumentNullException.ThrowIfNull(actions);

        actions.SetMainChartTitle("Metrics: Total");
        actions.SetNormalizedChartTitle("Metrics: Normalized");
        actions.SetDiffRatioChartTitle("Difference / Ratio");
    }

    public void UpdateTitlesFromSelections(IReadOnlyList<MetricSeriesSelection> selections, bool isDiffRatioDifferenceMode, Actions actions)
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

    public void ClearHiddenCharts(ChartState chartState, Actions actions)
    {
        ArgumentNullException.ThrowIfNull(chartState);
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var key in ChartVisibilityHelper.GetHiddenChartKeys(chartState))
            actions.ClearChart(key);
    }
}
