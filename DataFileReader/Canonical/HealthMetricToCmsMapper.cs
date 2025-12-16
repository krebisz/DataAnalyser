using DataFileReader.Helper;
using System;
using System.Collections.Generic;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Maps HealthMetric records to Canonical Metric Series.
    /// Phase 4: Made public for DataVisualiser integration.
    /// </summary>
    public sealed class HealthMetricToCmsMapper
    {
        private readonly CanonicalMetricIdentityResolver _identityResolver;

        public HealthMetricToCmsMapper(
            CanonicalMetricIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
        }

        public IReadOnlyList<ICanonicalMetricSeries> Map(
            IReadOnlyList<HealthMetric> records)
        {
            if (records == null) throw new ArgumentNullException(nameof(records));

            var output = new List<ICanonicalMetricSeries>(records.Count);

            foreach (var r in records)
            {
                if (r == null) continue;

                var resolved = _identityResolver.Resolve(
                    r.Provider ?? string.Empty,
                    r.MetricType ?? string.Empty,
                    r.MetricSubtype ?? string.Empty);

                if (!resolved.Success || resolved.MetricId == null)
                    continue;

                // -------------------------
                // Weight (Phase 3)
                // -------------------------
                if (resolved.MetricId.Value == "metric.body_weight")
                {
                    if (r.NormalizedTimestamp == null)
                        continue;

                    var ts = new DateTimeOffset(r.NormalizedTimestamp.Value);

                    output.Add(new CanonicalMetricSeries
                    {
                        MetricId = resolved.MetricId,
                        Time = new TimeSemantics(TimeRepresentation.Point, ts, null),
                        Samples = new[] { new MetricSample(ts, r.Value) },
                        Unit = new MetricUnit(r.Unit ?? "kg", false),
                        Dimension = MetricDimension.Mass,
                        Provenance = new MetricProvenance(
                            r.Provider ?? string.Empty,
                            r.SourceFile ?? string.Empty,
                            "Phase3.Weight.Shadow"),
                        Quality = new MetricQuality(DataCompleteness.Unknown, ValidationStatus.Assumed)
                    });
                }
                // -------------------------
                // Sleep (Phase 3)
                // -------------------------
                else if (resolved.MetricId.Value == "metric.sleep")
                {
                    if (r.NormalizedTimestamp == null)
                        continue;

                    var ts = new DateTimeOffset(r.NormalizedTimestamp.Value);

                    output.Add(new CanonicalMetricSeries
                    {
                        MetricId = resolved.MetricId,
                        Time = new TimeSemantics(TimeRepresentation.Point, ts, null),
                        Samples = new[] { new MetricSample(ts, r.Value) },
                        Unit = new MetricUnit(r.Unit ?? "hours", false),
                        Dimension = MetricDimension.Duration,
                        Provenance = new MetricProvenance(
                            r.Provider ?? string.Empty,
                            r.SourceFile ?? string.Empty,
                            "Phase3.Sleep.Shadow"),
                        Quality = new MetricQuality(DataCompleteness.Unknown, ValidationStatus.Assumed)
                    });
                }
            }

            return output;
        }
    }
}
