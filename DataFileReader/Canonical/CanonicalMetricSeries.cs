namespace DataFileReader.Canonical;

/// <summary>
///     Internal implementation of ICanonicalMetricSeries.
/// </summary>
internal sealed record CanonicalMetricSeries : ICanonicalMetricSeries
{
    public CanonicalMetricId MetricId { get; init; } = default!;

    public TimeSemantics Time { get; init; } = default!;

    public IReadOnlyList<MetricSample> Samples { get; init; } = Array.Empty<MetricSample>();

    public MetricUnit Unit { get; init; } = default!;

    public MetricDimension Dimension { get; init; } = MetricDimension.Unknown;

    public MetricProvenance Provenance { get; init; } = default!;

    public MetricQuality Quality { get; init; } = new(DataCompleteness.Unknown, ValidationStatus.Unknown);
}

// -------------------------
// Supporting value types
// -------------------------