using System;
using System.Collections.Generic;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Canonical Metric Series (CMS)
    ///
    /// Consumer-facing, semantically authoritative representation of a metric.
    /// This interface defines the ONLY semantic surface exposed upward.
    ///
    /// Invariants (documented, not enforced here):
    /// - Metric identity is canonical and opaque
    /// - Time semantics are normalized
    /// - Samples are ordered
    /// - Units and dimensions are explicit
    /// - No inference is required by consumers
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

    namespace DataFileReader.Canonical
    {
        internal sealed record CanonicalMetricSeries : ICanonicalMetricSeries
        {
            public CanonicalMetricId MetricId { get; init; } = default!;

            public TimeSemantics Time { get; init; } = default!;

            public IReadOnlyList<MetricSample> Samples { get; init; } = Array.Empty<MetricSample>();

            public MetricUnit Unit { get; init; } = default!;

            public MetricDimension Dimension { get; init; } = MetricDimension.Unknown;

            public MetricProvenance Provenance { get; init; } = default!;

            public MetricQuality Quality { get; init; } =
                new MetricQuality(DataCompleteness.Unknown, ValidationStatus.Unknown);
        }
    }

    // -------------------------
    // Supporting value types
    // -------------------------

    /// <summary>
    /// Canonical, opaque metric identifier.
    /// Consumers must not infer meaning from structure.
    /// </summary>
    public sealed record CanonicalMetricId(string Value);

    /// <summary>
    /// Declares how time is represented for the series.
    /// </summary>
    public sealed record TimeSemantics(
        TimeRepresentation Representation,
        DateTimeOffset Start,
        DateTimeOffset? End
    );

    public enum TimeRepresentation
    {
        Point,
        Interval
    }

    /// <summary>
    /// A single scalar observation aligned to the declared time semantics.
    /// </summary>
    public sealed record MetricSample(
        DateTimeOffset Timestamp,
        decimal? Value
    );

    /// <summary>
    /// Quantitative unit metadata.
    /// </summary>
    public sealed record MetricUnit(
        string Symbol,
        bool IsCanonical
    );

    /// <summary>
    /// Dimensional classification of the metric.
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

    /// <summary>
    /// Minimal provenance for traceability and diagnostics.
    /// </summary>
    public sealed record MetricProvenance(
        string SourceProvider,
        string SourceDataset,
        string NormalizationVersion
    );

    /// <summary>
    /// Quality and validation signals.
    /// Informational only; not corrective.
    /// </summary>
    public sealed record MetricQuality(
        DataCompleteness Completeness,
        ValidationStatus ValidationStatus,
        IReadOnlyList<string>? Notes = null
    );

    public enum DataCompleteness
    {
        Unknown,
        Complete,
        Partial,
        Sparse
    }

    public enum ValidationStatus
    {
        Unknown,
        Validated,
        Assumed,
        Derived
    }
}
