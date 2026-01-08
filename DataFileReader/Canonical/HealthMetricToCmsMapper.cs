using DataFileReader.Helper;
using DataFileReader.Normalization.Canonical;

namespace DataFileReader.Canonical;

/// <summary>
///     Maps HealthMetric records to Canonical Metric Series.
///     Phase 4: Made public for DataVisualiser integration.
/// </summary>
public sealed class HealthMetricToCmsMapper
{
    private readonly CanonicalMetricIdentityResolver _identityResolver;

    public HealthMetricToCmsMapper(CanonicalMetricIdentityResolver identityResolver)
    {
        _identityResolver = identityResolver;
    }

    public IReadOnlyList<ICanonicalMetricSeries> Map(IReadOnlyList<HealthMetric> records)
    {
        if (records == null)
            throw new ArgumentNullException(nameof(records));

        // Group records by canonical metric ID to create one series per metric
        var groupedRecords = new Dictionary<string, List<(HealthMetric Record, CanonicalMetricId MetricId, string? Unit, MetricDimension Dimension, string ProvenanceVersion)>>();

        foreach (var r in records)
        {
            if (r == null || r.NormalizedTimestamp == null)
                continue;

            var resolved = _identityResolver.Resolve(r.Provider ?? string.Empty, r.MetricType ?? string.Empty, r.MetricSubtype ?? string.Empty);

            if (!resolved.Success || resolved.MetricId == null)
                continue;

            var metricIdValue = resolved.MetricId.Value;
            string? unit = null;
            var dimension = MetricDimension.Unknown;
            string provenanceVersion;

            // Determine unit, dimension, and provenance based on metric type
            if (metricIdValue == MetricIdentity.BodyWeight.Id)
            {
                unit = r.Unit ?? "kg";
                dimension = MetricDimension.Mass;
                provenanceVersion = "Phase3.Weight.Shadow";
            }
            else if (metricIdValue == MetricIdentity.Sleep.Id)
            {
                unit = r.Unit ?? "hours";
                dimension = MetricDimension.Duration;
                provenanceVersion = "Phase3.Sleep.Shadow";
            }
            else
            {
                unit = r.Unit ?? string.Empty;
                dimension = MetricDimension.Unknown;
                provenanceVersion = "Phase4.Generic.Mapping";
            }

            if (!groupedRecords.TryGetValue(metricIdValue, out var group))
            {
                group = new List<(HealthMetric, CanonicalMetricId, string?, MetricDimension, string)>();
                groupedRecords[metricIdValue] = group;
            }

            group.Add((r, resolved.MetricId, unit, dimension, provenanceVersion));
        }

        // Create one series per metric ID with all samples
        var output = new List<ICanonicalMetricSeries>(groupedRecords.Count);

        foreach (var (metricIdValue, group) in groupedRecords)
        {
            if (group.Count == 0)
                continue;

            // Get metadata from first record (assuming all records in group have same metadata)
            var firstRecord = group[0];
            var samples = new List<MetricSample>(group.Count);

            // Collect all samples, ordered by timestamp
            foreach (var (record, _, _, _, _) in group)
                if (record.NormalizedTimestamp.HasValue)
                {
                    var ts = new DateTimeOffset(record.NormalizedTimestamp.Value);
                    samples.Add(new MetricSample(ts, record.Value));
                }

            if (samples.Count == 0)
                continue;

            // Sort samples by timestamp
            samples.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

            // Determine time semantics from first and last sample
            var firstTimestamp = samples[0].Timestamp;
            var lastTimestamp = samples[samples.Count - 1].Timestamp;

            // Use first record's provider and source file for provenance
            var firstHealthMetric = group[0].Record;

            output.Add(new CanonicalMetricSeries
            {
                    MetricId = firstRecord.MetricId,
                    Time = new TimeSemantics(TimeRepresentation.Point, firstTimestamp, lastTimestamp),
                    Samples = samples,
                    Unit = new MetricUnit(firstRecord.Unit ?? string.Empty, false),
                    Dimension = firstRecord.Dimension,
                    Provenance = new MetricProvenance(firstHealthMetric.Provider ?? string.Empty, firstHealthMetric.SourceFile ?? string.Empty, firstRecord.ProvenanceVersion),
                    Quality = new MetricQuality(DataCompleteness.Unknown, ValidationStatus.Assumed)
            });
        }

        return output;
    }
}