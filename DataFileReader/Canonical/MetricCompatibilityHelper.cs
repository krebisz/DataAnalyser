using System;
using System.Collections.Generic;
using System.Linq;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Helper for validating semantic compatibility between canonical metrics.
    /// 
    /// Prevents invalid metric combinations (e.g., comparing Mass to Duration)
    /// and ensures computations operate on semantically compatible metrics.
    /// 
    /// Aligns with Project Bible principles:
    /// - Explicit Semantics: Compatibility is determined by explicit dimension rules
    /// - Single Semantic Authority: Dimension is the authoritative compatibility signal
    /// - Determinism: Same inputs always produce same compatibility results
    /// </summary>
    public static class MetricCompatibilityHelper
    {
        /// <summary>
        /// Checks if two canonical metric IDs have compatible dimensions for computation.
        /// 
        /// Current rule: Only metrics with identical dimensions are compatible.
        /// Future: Could allow same-dimension metrics (e.g., Mass + Mass, Duration + Duration)
        /// even if they have different canonical IDs.
        /// </summary>
        /// <param name="canonicalId1">First canonical metric identifier</param>
        /// <param name="canonicalId2">Second canonical metric identifier</param>
        /// <returns>True if metrics are compatible for computation, false otherwise</returns>
        public static bool AreCompatible(string canonicalId1, string canonicalId2)
        {
            if (string.IsNullOrWhiteSpace(canonicalId1) || string.IsNullOrWhiteSpace(canonicalId2))
                return false;

            // For now, only same canonical ID is compatible
            // This is conservative and prevents semantic errors
            // Future: Could relax to same dimension
            return canonicalId1 == canonicalId2;
        }

        /// <summary>
        /// Gets the dimension for a canonical metric ID.
        /// 
        /// Dimensions are the authoritative semantic classification for compatibility.
        /// </summary>
        /// <param name="canonicalMetricId">The canonical metric identifier</param>
        /// <returns>The metric dimension, or Unknown if not found</returns>
        public static MetricDimension GetDimension(string canonicalMetricId)
        {
            if (string.IsNullOrWhiteSpace(canonicalMetricId))
                return MetricDimension.Unknown;

            return canonicalMetricId switch
            {
                "metric.body_weight" => MetricDimension.Mass,
                "metric.sleep" => MetricDimension.Duration,
                _ => MetricDimension.Unknown
            };
        }

        /// <summary>
        /// Validates that multiple metrics can be used together in a computation.
        /// 
        /// Useful for strategies that operate on multiple series (MultiMetric, Combined, etc.)
        /// </summary>
        /// <param name="canonicalIds">Collection of canonical metric identifiers</param>
        /// <returns>True if all metrics are compatible with each other, false otherwise</returns>
        public static bool ValidateCompatibility(IEnumerable<string> canonicalIds)
        {
            if (canonicalIds == null)
                return false;

            var ids = canonicalIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

            // Empty or single metric is always compatible (nothing to compare)
            if (ids.Count < 2)
                return true;

            // For now, all must have the same canonical ID
            // Future: Could allow same dimension
            var firstId = ids[0];
            return ids.Skip(1).All(id => id == firstId);
        }

        /// <summary>
        /// Validates compatibility by dimension rather than exact canonical ID match.
        /// 
        /// More permissive than ValidateCompatibility - allows metrics with same dimension
        /// but different canonical IDs (e.g., different weight measurements).
        /// 
        /// Use this when you want to allow same-dimension metrics to be combined.
        /// </summary>
        /// <param name="canonicalIds">Collection of canonical metric identifiers</param>
        /// <returns>True if all metrics have compatible dimensions, false otherwise</returns>
        public static bool ValidateCompatibilityByDimension(IEnumerable<string> canonicalIds)
        {
            if (canonicalIds == null)
                return false;

            var ids = canonicalIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

            // Empty or single metric is always compatible
            if (ids.Count < 2)
                return true;

            var firstDimension = GetDimension(ids[0]);

            // Unknown dimension is not compatible with anything
            if (firstDimension == MetricDimension.Unknown)
                return false;

            // All metrics must have the same dimension
            return ids.Skip(1).All(id => GetDimension(id) == firstDimension);
        }

        /// <summary>
        /// Gets a human-readable explanation of why metrics are incompatible.
        /// 
        /// Useful for error messages and UI feedback.
        /// </summary>
        /// <param name="canonicalIds">Collection of canonical metric identifiers</param>
        /// <returns>Explanation string, or null if metrics are compatible</returns>
        public static string? GetIncompatibilityReason(IEnumerable<string> canonicalIds)
        {
            if (canonicalIds == null)
                return "No metrics provided";

            var ids = canonicalIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

            if (ids.Count < 2)
                return null; // Compatible (nothing to compare)

            // Check for mismatched canonical IDs
            var uniqueIds = ids.Distinct().ToList();
            if (uniqueIds.Count > 1)
            {
                var displayNames = uniqueIds.Select(id => CanonicalMetricMapping.GetDisplayName(id));
                return $"Metrics have different identities: {string.Join(", ", displayNames)}";
            }

            // Check for dimension mismatches (if using dimension-based validation)
            var dimensions = ids.Select(GetDimension).Distinct().ToList();
            if (dimensions.Count > 1)
            {
                var dimensionNames = dimensions.Select(d => d.ToString());
                return $"Metrics have incompatible dimensions: {string.Join(", ", dimensionNames)}";
            }

            return null; // Compatible
        }
    }
}
