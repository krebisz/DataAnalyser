using System;
using System.Collections.Generic;

namespace DataFileReader.Normalization.Canonical
{
    /// <summary>
    /// CanonicalMetricSeries is the normalized, trusted analytical representation
    /// consumed by computation layers.
    /// </summary>
    public sealed class CanonicalMetricSeries<TValue>
    {
        /// <summary>
        /// Optional diagnostic hook invoked after successful construction.
        /// Intended for tracing and validation only.
        /// </summary>
        public static Action<CanonicalMetricSeries<TValue>>? OnCreated { get; set; }

        public MetricIdentity Metric { get; }
        public TimeAxis TimeAxis { get; }
        public DimensionSet Dimensions { get; }
        public IReadOnlyList<DateTimeOffset> Timestamps { get; }
        public IReadOnlyList<TValue?> Values { get; }
        public IReadOnlyDictionary<string, string> Provenance { get; }

        public CanonicalMetricSeries(
            MetricIdentity metric,
            TimeAxis timeAxis,
            DimensionSet dimensions,
            IReadOnlyList<DateTimeOffset> timestamps,
            IReadOnlyList<TValue?> values,
            IReadOnlyDictionary<string, string> provenance)
        {
            Metric = metric ?? throw new ArgumentNullException(nameof(metric));
            TimeAxis = timeAxis ?? throw new ArgumentNullException(nameof(timeAxis));
            Dimensions = dimensions ?? throw new ArgumentNullException(nameof(dimensions));
            Timestamps = timestamps ?? throw new ArgumentNullException(nameof(timestamps));
            Values = values ?? throw new ArgumentNullException(nameof(values));
            Provenance = provenance ?? throw new ArgumentNullException(nameof(provenance));

            ValidateStructure();

            OnCreated?.Invoke(this);
        }

        private void ValidateStructure()
        {
            if (Timestamps.Count != Values.Count)
                throw new ArgumentException(
                    "Timestamps and values must have the same length.");

            if (Timestamps.Count == 0)
                throw new ArgumentException(
                    "CanonicalMetricSeries must contain at least one data point.");
        }
    }
}
