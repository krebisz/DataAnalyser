namespace DataFileReader.Canonical;

/// <summary>
///     Centralized mapping utilities for canonical metric identities.
///     Provides conversion between canonical metric IDs and legacy representations,
///     display names, and other metadata mappings.
/// </summary>
public static class CanonicalMetricMapping
{
    /// <summary>
    ///     Maps canonical metric ID to legacy MetricType/MetricSubtype for database queries.
    ///     Mapping is sourced from the CanonicalMetricMappings table.
    /// </summary>
    public static(string? MetricType, string? Subtype) ToLegacyFields(string canonicalMetricId)
    {
        return CanonicalMetricMappingStore.ToLegacyFields(canonicalMetricId);
    }

    /// <summary>
    ///     Gets human-readable display name for canonical metric ID.
    /// </summary>
    public static string GetDisplayName(string canonicalMetricId)
    {
        if (string.IsNullOrWhiteSpace(canonicalMetricId))
            return "Unknown Metric";

        return canonicalMetricId.Replace("_", " ");
    }

    /// <summary>
    ///     Gets the canonical metric ID from legacy MetricType/MetricSubtype.
    ///     Mapping is sourced from the CanonicalMetricMappings table.
    /// </summary>
    public static string? FromLegacyFields(string metricType, string? metricSubtype = null)
    {
        return CanonicalMetricMappingStore.FromLegacyFields(metricType, metricSubtype);
    }
}