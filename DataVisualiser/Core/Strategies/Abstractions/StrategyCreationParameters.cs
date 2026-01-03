using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Strategies.Abstractions;

/// <summary>
///     Parameters for strategy creation.
/// </summary>
public class StrategyCreationParameters
{
    public IEnumerable<HealthMetricData>? LegacyData1 { get; init; }
    public IEnumerable<HealthMetricData>? LegacyData2 { get; init; }
    public IReadOnlyList<IEnumerable<HealthMetricData>>? LegacySeries { get; init; }
    public string Label1 { get; init; } = string.Empty;
    public string Label2 { get; init; } = string.Empty;
    public IReadOnlyList<string>? Labels { get; init; }
    public DateTime From { get; init; }
    public DateTime To { get; init; }
    public string? Unit { get; init; }
    public NormalizationMode? NormalizationMode { get; init; }
}
