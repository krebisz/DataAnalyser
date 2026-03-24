using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Rendering.CartesianMetrics;

public sealed record CartesianMetricChartRenderRequest(
    CartesianMetricChartRoute Route,
    ChartDataContext Context,
    IReadOnlyList<MetricSeriesSelection>? SelectedSeries = null,
    string? ResolutionTableName = null,
    bool IsStacked = false,
    bool IsCumulative = false,
    IReadOnlyList<SeriesResult>? OverlaySeries = null);
