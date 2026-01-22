namespace DataFileReader.Helper;

/// <summary>
///     Represents a standardized health metric record
/// </summary>
public class HealthMetric
{
    public string Provider { get; set; } = string.Empty;
    public string MetricType { get; set; } = string.Empty;
    public string MetricSubtype { get; set; } = string.Empty;
    public string SourceFile { get; set; } = string.Empty;
    public DateTime? NormalizedTimestamp { get; set; }
    public string RawTimestamp { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalFields { get; set; } = new();
}