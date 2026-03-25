using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public sealed record SecondaryMetricChartRenderRequest(
    ChartDataContext Context,
    ChartState ChartState,
    SecondaryMetricChartRoute Route);
