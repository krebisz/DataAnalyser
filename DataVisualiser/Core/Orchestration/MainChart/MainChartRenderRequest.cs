using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration.MainChart;

public sealed record MainChartRenderRequest(
    ChartDataContext Context,
    IReadOnlyList<MetricSeriesSelection>? SelectedSeries = null,
    string? ResolutionTableName = null,
    bool IsStacked = false,
    bool IsCumulative = false,
    IReadOnlyList<SeriesResult>? OverlaySeries = null,
    IReadOnlyList<IEnumerable<MetricData>>? AdditionalSeries = null,
    IReadOnlyList<string>? AdditionalLabels = null);
