using DataVisualiser.Core.Orchestration;

namespace DataVisualiser.UI.MainHost;

public sealed class MainChartsViewChartUpdateActions(
    Action<string, bool> setChartVisibility,
    Action updateDistributionChartTypeVisibility,
    Action updateWeekdayTrendChartTypeVisibility,
    Action<ChartDataContext?> handleTransformVisibilityOnlyToggle,
    Func<string, ChartDataContext, Task> renderChartAsync,
    Action<string> clearChart)
{
    public Action<string, bool> SetChartVisibility { get; } = setChartVisibility ?? throw new ArgumentNullException(nameof(setChartVisibility));
    public Action UpdateDistributionChartTypeVisibility { get; } = updateDistributionChartTypeVisibility ?? throw new ArgumentNullException(nameof(updateDistributionChartTypeVisibility));
    public Action UpdateWeekdayTrendChartTypeVisibility { get; } = updateWeekdayTrendChartTypeVisibility ?? throw new ArgumentNullException(nameof(updateWeekdayTrendChartTypeVisibility));
    public Action<ChartDataContext?> HandleTransformVisibilityOnlyToggle { get; } = handleTransformVisibilityOnlyToggle ?? throw new ArgumentNullException(nameof(handleTransformVisibilityOnlyToggle));
    public Func<string, ChartDataContext, Task> RenderChartAsync { get; } = renderChartAsync ?? throw new ArgumentNullException(nameof(renderChartAsync));
    public Action<string> ClearChart { get; } = clearChart ?? throw new ArgumentNullException(nameof(clearChart));
}
