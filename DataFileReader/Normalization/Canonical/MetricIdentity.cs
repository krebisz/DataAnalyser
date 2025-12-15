using System;

namespace DataFileReader.Normalization.Canonical
{
    /// <summary>
    /// Opaque identifier representing a single canonical metric.
    /// Semantic meaning is resolved during normalization and immutable thereafter.
    /// </summary>
    public sealed class MetricIdentity
    {
        public string Id { get; }

        public MetricIdentity(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Metric identity must be non-empty.", nameof(id));

            Id = id;
        }

        public override string ToString() => Id;
    }
}
