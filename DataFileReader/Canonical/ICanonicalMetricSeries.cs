namespace DataFileReader.Canonical;

/// <summary>
///     Canonical Metric Series (CMS)
///     Consumer-facing, semantically authoritative representation of a metric.
///     This interface defines the ONLY semantic surface exposed upward.
///     Invariants (documented, not enforced here):
///     - Metric identity is canonical and opaque
///     - Time semantics are normalized
///     - Samples are ordered
///     - Units and dimensions are explicit
///     - No inference is required by consumers
/// </summary>
public interface ICanonicalMetricSeries
{
    CanonicalMetricId MetricId { get; }

    TimeSemantics Time { get; }

    IReadOnlyList<MetricSample> Samples { get; }

    MetricUnit Unit { get; }

    MetricDimension Dimension { get; }

    MetricProvenance Provenance { get; }

    MetricQuality Quality { get; }
}