namespace DataFileReader.Canonical;

/// <summary>
///     Canonical, opaque metric identifier.
///     Consumers must not infer meaning from structure.
/// </summary>
public sealed record CanonicalMetricId(string Value);