using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Orchestration.SecondaryCharts;

public sealed record SecondaryMetricChartStrategyPlan(
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? SecondaryLabel,
    string OperationType,
    bool IsOperationChart = true);
