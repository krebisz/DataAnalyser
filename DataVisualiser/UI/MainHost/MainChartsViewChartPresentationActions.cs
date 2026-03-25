using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartPresentationActions(
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
