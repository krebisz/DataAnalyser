using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Transforms;

/// <summary>
///     Result of a transform operation.
/// </summary>
public sealed class TransformOperationResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public List<HealthMetricData>? DataList { get; init; }
    public List<double>? ComputedResults { get; init; }
    public List<IReadOnlyList<HealthMetricData>>? MetricsList { get; init; }
    public string? Operation { get; init; }
}
