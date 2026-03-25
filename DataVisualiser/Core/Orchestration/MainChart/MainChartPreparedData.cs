using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed record MainChartPreparedData(
    ChartDataContext WorkingContext,
    IReadOnlyList<IEnumerable<MetricData>> Series,
    IReadOnlyList<string> Labels,
    bool IsStacked,
    bool IsCumulative,
    IReadOnlyList<SeriesResult>? OverlaySeries);
