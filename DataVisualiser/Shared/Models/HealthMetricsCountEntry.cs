namespace DataVisualiser.Shared.Models;

public sealed class HealthMetricsCountEntry
{
    public string MetricType { get; init; } = string.Empty;
    public string MetricSubtype { get; init; } = string.Empty;

    public string MetricTypeName { get; set; } = string.Empty;
    public string MetricSubtypeName { get; set; } = string.Empty;

    public bool Disabled { get; set; }

    public long RecordCount { get; init; }
    public DateTime? EarliestDateTime { get; init; }
    public DateTime? MostRecentDateTime { get; init; }
}

