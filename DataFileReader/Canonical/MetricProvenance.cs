namespace DataFileReader.Canonical;

/// <summary>
///     Minimal provenance for traceability and diagnostics.
/// </summary>
public sealed record MetricProvenance(string SourceProvider, string SourceDataset, string NormalizationVersion);