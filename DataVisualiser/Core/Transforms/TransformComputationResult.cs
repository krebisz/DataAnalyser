using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Transforms;

/// <summary>
///     Result of a transform computation operation.
/// </summary>
public sealed class TransformComputationResult
{
    public List<HealthMetricData>                DataList        { get; init; } = new();
    public List<double>                          ComputedResults { get; init; } = new();
    public string                                Operation       { get; init; } = string.Empty;
    public List<IReadOnlyList<HealthMetricData>> MetricsList     { get; init; } = new();
    public bool                                  IsSuccess       { get; init; }

    public static TransformComputationResult Empty => new()
    {
            IsSuccess = false
    };
}