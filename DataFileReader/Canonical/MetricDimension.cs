namespace DataFileReader.Canonical;

/// <summary>
///     Dimensional classification of the metric.
/// </summary>
public enum MetricDimension
{
    Unknown,
    Count,
    Rate,
    Duration,
    Energy,
    Distance,
    Mass,
    Percentage
}