namespace DataFileReader.Canonical;

/// <summary>
///     Quality and validation signals.
///     Informational only; not corrective.
/// </summary>
public sealed record MetricQuality(DataCompleteness Completeness, ValidationStatus ValidationStatus, IReadOnlyList<string>? Notes = null);