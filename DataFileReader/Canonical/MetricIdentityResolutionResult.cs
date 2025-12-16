using System.Collections.Generic;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Represents the outcome of canonical metric identity resolution.
    /// This type exists to make failure explicit and diagnosable.
    /// Phase 4: Made public for DataVisualiser integration.
    /// </summary>
    public sealed record MetricIdentityResolutionResult
    {
        public bool Success { get; init; }

        public CanonicalMetricId? MetricId { get; init; }

        public IdentityResolutionFailureReason? FailureReason { get; init; }

        public IReadOnlyList<string>? Diagnostics { get; init; }

        public static MetricIdentityResolutionResult Failed(
            IdentityResolutionFailureReason reason,
            IReadOnlyList<string>? diagnostics = null)
        {
            return new MetricIdentityResolutionResult
            {
                Success = false,
                FailureReason = reason,
                Diagnostics = diagnostics
            };
        }

        public static MetricIdentityResolutionResult Succeeded(
            CanonicalMetricId metricId)
        {
            return new MetricIdentityResolutionResult
            {
                Success = true,
                MetricId = metricId
            };
        }
    }

    /// <summary>
    /// Reasons for identity resolution failure.
    /// Phase 4: Made public for DataVisualiser integration.
    /// </summary>
    public enum IdentityResolutionFailureReason
    {
        Unknown,
        NoMatchingRule,
        MultipleMatchingRules,
        MissingRequiredMetadata,
        InvalidMetadata
    }
}
