using DataFileReader.Canonical;
using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;

namespace DataFileReader.Normalization.Stages;

/// <summary>
///     Produces Canonical Metric Series from processed RawRecords.
///     This stage extracts metric data, resolves identity, and creates CMS instances.
/// </summary>
public sealed class CmsProductionStage : INormalizationStage
{
    private readonly CanonicalMetricIdentityResolver _identityResolver;
    private readonly List<CanonicalMetricSeries<object>> _producedCms;

    public CmsProductionStage()
    {
        _identityResolver = new CanonicalMetricIdentityResolver();
        _producedCms = new List<CanonicalMetricSeries<object>>();
    }

    /// <summary>
    ///     Gets the CMS instances produced by this stage.
    /// </summary>
    public IReadOnlyList<CanonicalMetricSeries<object>> ProducedCms => _producedCms;

    public IReadOnlyCollection<RawRecord> Process(IReadOnlyCollection<RawRecord> input, NormalizationContext context)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Clear previous results
        _producedCms.Clear();

        // Group records by resolved identity for series construction
        var groupedByIdentity = new Dictionary<string, List<RawRecord>>();

        foreach (var record in input)
        {
            if (record == null)
                continue;

            // Extract provider, metric type, and subtype from fields
            var provider = ExtractStringField(record, "Provider") ?? record.SourceGroup ?? string.Empty;
            var metricType = ExtractStringField(record, "MetricType") ?? record.SourceId ?? string.Empty;
            var metricSubtype = ExtractStringField(record, "MetricSubtype") ?? string.Empty;

            // Resolve canonical identity
            var resolutionResult = _identityResolver.Resolve(provider, metricType, metricSubtype);

            if (!resolutionResult.Success || resolutionResult.MetricId == null)
                continue; // Skip records that don't resolve to a canonical identity

            var identityKey = resolutionResult.MetricId.Value;

            if (!groupedByIdentity.ContainsKey(identityKey))
                groupedByIdentity[identityKey] = new List<RawRecord>();

            groupedByIdentity[identityKey].Add(record);
        }

        // Produce CMS for each resolved identity group
        foreach (var group in groupedByIdentity)
        {
            var identityId = group.Key;
            var records = group.Value;

            // Determine metric identity
            MetricIdentity metricIdentity;
            if (identityId == "metric.body_weight")
                metricIdentity = MetricIdentity.BodyWeight;
            else if (identityId == "metric.sleep")
                metricIdentity = MetricIdentity.Sleep;
            else
                    // Unknown identity - skip for now (Phase 3: Weight only)
                continue;

            // Extract timestamps and values
            var timestamps = new List<DateTimeOffset>();
            var values = new List<object?>();
            var provenance = new Dictionary<string, string>();

            foreach (var record in records.OrderBy(r => ExtractTimestamp(r)))
            {
                var timestamp = ExtractTimestamp(record);
                if (!timestamp.HasValue)
                    continue;

                var value = ExtractValue(record);
                if (value == null && !IsNullableMetric(identityId))
                    continue;

                timestamps.Add(timestamp.Value);
                values.Add(value);

                // Collect provenance from first record
                if (provenance.Count == 0)
                {
                    var provider = ExtractStringField(record, "Provider") ?? record.SourceGroup ?? "Unknown";
                    var sourceFile = ExtractStringField(record, "SourceFile") ?? record.SourceId ?? "Unknown";
                    provenance["SourceProvider"] = provider;
                    provenance["SourceDataset"] = sourceFile;
                    provenance["NormalizationVersion"] = "Phase3.Shadow";
                }
            }

            if (timestamps.Count == 0)
                continue;

            // Create TimeAxis
            var timeZone = TimeZoneInfo.Utc;   // Default to UTC
            var timeAxis = new TimeAxis(false, // Point-based for now
                    TimeSpan.FromDays(1),      // Daily resolution
                    timeZone);

            // Create DimensionSet
            var dimensions = new Dictionary<string, string>();
            if (identityId == "metric.body_weight")
                dimensions["Dimension"] = "Mass";
            else if (identityId == "metric.sleep")
                dimensions["Dimension"] = "Duration";

            var dimensionSet = new DimensionSet(dimensions);

            // Create CMS
            var cms = new CanonicalMetricSeries<object>(metricIdentity, timeAxis, dimensionSet, timestamps, values, provenance);

            _producedCms.Add(cms);
        }

        // Return input unchanged (stages must return RawRecord collection)
        return input;
    }

    private static string? ExtractStringField(RawRecord record, string fieldName)
    {
        if (record.Fields.TryGetValue(fieldName, out var value))
            return value?.ToString();
        return null;
    }

    private static DateTimeOffset? ExtractTimestamp(RawRecord record)
    {
        // Try NormalizedTimestamp first
        if (record.Fields.TryGetValue("NormalizedTimestamp", out var normalizedTs))
        {
            if (normalizedTs is DateTime dt)
                return new DateTimeOffset(dt);
            if (normalizedTs is DateTimeOffset dto)
                return dto;
            if (normalizedTs != null && DateTime.TryParse(normalizedTs.ToString(), out var parsed))
                return new DateTimeOffset(parsed);
        }

        // Fall back to RawTimestamp
        return record.RawTimestamp;
    }

    private static object? ExtractValue(RawRecord record)
    {
        if (record.Fields.TryGetValue("Value", out var value))
            return value;
        return null;
    }

    private static bool IsNullableMetric(string identityId)
    {
        // Some metrics may allow null values
        return false; // For Phase 3, all metrics require values
    }
}