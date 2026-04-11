using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed record MainChartPreparedData(
    ChartDataContext WorkingContext,
    IReadOnlyList<IEnumerable<MetricData>> Series,
    IReadOnlyList<string> Labels,
    bool IsStacked,
    bool IsCumulative,
    IReadOnlyList<SeriesResult>? OverlaySeries);

public sealed record MainChartRenderRequest(
    ChartDataContext Context,
    IReadOnlyList<MetricSeriesSelection>? SelectedSeries = null,
    string? ResolutionTableName = null,
    bool IsStacked = false,
    bool IsCumulative = false,
    IReadOnlyList<SeriesResult>? OverlaySeries = null,
    IReadOnlyList<IEnumerable<MetricData>>? AdditionalSeries = null,
    IReadOnlyList<string>? AdditionalLabels = null);

public sealed record MainChartStrategyPlan(
    StrategyType StrategyType,
    ChartDataContext WorkingContext,
    IChartComputationStrategy Strategy,
    string PrimaryLabel,
    string? SecondaryLabel,
    bool IsStacked,
    bool IsCumulative,
    IReadOnlyList<SeriesResult>? OverlaySeries);
