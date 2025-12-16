namespace DataFileReader.Canonical
{
    /// <summary>
    /// Centralized mapping utilities for canonical metric identities.
    /// 
    /// Provides conversion between canonical metric IDs and legacy representations,
    /// display names, and other metadata mappings.
    /// 
    /// Phase 4: This centralizes mapping logic that was previously duplicated
    /// across components, enabling reuse and maintainability.
    /// </summary>
    public static class CanonicalMetricMapping
    {
        /// <summary>
        /// Maps canonical metric ID to legacy MetricType/MetricSubtype for database queries.
        /// 
        /// Phase 4: This is a temporary mapping. In future phases, CMS will be stored directly
        /// and this mapping may become obsolete or used only for migration purposes.
        /// </summary>
        /// <param name="canonicalMetricId">The canonical metric identifier (e.g., "metric.body_weight")</param>
        /// <returns>Tuple of (MetricType, MetricSubtype) or (null, null) if not found</returns>
        public static (string? MetricType, string? Subtype) ToLegacyFields(string canonicalMetricId)
        {
            if (string.IsNullOrWhiteSpace(canonicalMetricId))
                return (null, null);

            return canonicalMetricId switch
            {
                "metric.body_weight" => ("weight", null),
                "metric.sleep" => ("com.samsung.shealth.sleep", null),
                _ => (null, null)
            };
        }

        /// <summary>
        /// Gets human-readable display name for canonical metric ID.
        /// 
        /// Useful for UI display where canonical IDs are too technical.
        /// </summary>
        /// <param name="canonicalMetricId">The canonical metric identifier</param>
        /// <returns>Human-readable display name</returns>
        public static string GetDisplayName(string canonicalMetricId)
        {
            if (string.IsNullOrWhiteSpace(canonicalMetricId))
                return "Unknown Metric";

            return canonicalMetricId switch
            {
                "metric.body_weight" => "Body Weight",
                "metric.sleep" => "Sleep",
                _ => canonicalMetricId
                    .Replace("metric.", "", StringComparison.OrdinalIgnoreCase)
                    .Replace("_", " ")
            };
        }

        /// <summary>
        /// Gets the canonical metric ID from legacy MetricType/MetricSubtype.
        /// 
        /// Reverse mapping of ToLegacyFields(). Useful for migration scenarios.
        /// </summary>
        /// <param name="metricType">Legacy metric type</param>
        /// <param name="metricSubtype">Optional legacy metric subtype</param>
        /// <returns>Canonical metric ID or null if not found</returns>
        public static string? FromLegacyFields(string metricType, string? metricSubtype = null)
        {
            if (string.IsNullOrWhiteSpace(metricType))
                return null;

            // Normalize for comparison
            var normalizedType = metricType.Trim();

            return normalizedType switch
            {
                "weight" => "metric.body_weight",
                "com.samsung.shealth.sleep" => "metric.sleep",
                _ => null
            };
        }
    }
}
