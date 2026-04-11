using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.UI.State;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public enum SecondaryMetricChartRoute
{
    Normalized,
    Difference,
    Ratio
}

public sealed record SecondaryMetricChartRenderRequest(
    ChartDataContext Context,
    ChartState ChartState,
    SecondaryMetricChartRoute Route);

public sealed record SecondaryMetricChartStrategyPlan(
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? SecondaryLabel,
    string OperationType,
    bool IsOperationChart = true);
