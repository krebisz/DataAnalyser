using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Abstractions;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed record MainChartStrategyPlan(
    StrategyType StrategyType,
    ChartDataContext WorkingContext,
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? SecondaryLabel,
    bool IsStacked,
    bool IsCumulative,
    IReadOnlyList<SeriesResult>? OverlaySeries);
