using System.Collections.Generic;

namespace DataFileReader.Canonical
{
    /// <summary>
    /// Represents the outcome of canonical metric identity resolution.
    /// This type exists to make failure explicit and diagnosable.
    /// </summary>
    internal sealed record MetricIdentityResolutionResult
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

    internal enum IdentityResolutionFailureReason
    {
        Unknown,
        NoMatchingRule,
        MultipleMatchingRules,
        MissingRequiredMetadata,
        InvalidMetadata
    }
}
