using DataVisualiser.Core.Computation.Results;
using DataVisualiser.Core.Rendering.CartesianMetrics;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Orchestration;

public sealed record PrimaryChartRenderRequest
{
    public required ChartDataContext Context { get; init; }
    public required IEnumerable<MetricData> Data1 { get; init; }
    public IEnumerable<MetricData>? Data2 { get; init; }
    public required string DisplayName1 { get; init; }
    public required string DisplayName2 { get; init; }
    public required DateTime From { get; init; }
    public required DateTime To { get; init; }
    public string? MetricType { get; init; }
    public IReadOnlyList<MetricSeriesSelection>? SelectedSeries { get; init; }
    public string? ResolutionTableName { get; init; }
    public bool IsStacked { get; init; }
    public bool IsCumulative { get; init; }
    public IReadOnlyList<SeriesResult>? OverlaySeries { get; init; }
    public CartesianMetricCapabilityContract? CapabilityContract { get; init; }
}
