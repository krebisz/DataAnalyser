namespace DataFileReader.Canonical;

/// <summary>
///     A single scalar observation aligned to the declared time semantics.
/// </summary>
public sealed record MetricSample(DateTimeOffset Timestamp, decimal? Value);