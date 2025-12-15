using DataFileReader.Canonical;
using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;
using System;

namespace DataFileReader.Normalization
{
    /// <summary>
    /// Optional diagnostics wiring for normalization components.
    /// Disabled by default.
    /// </summary>
    public static class NormalizationDiagnostics
    {
        public static Action<RawRecord>? OnRawRecordObserved { get; set; }
        public static Action<CanonicalMetricSeries<object>>? OnMetricSeriesProduced { get; set; }

        /// <summary>
        /// Records the outcome of canonical metric identity resolution.
        /// Informational only; does not alter normalization behavior.
        /// </summary>
        internal static void OnMetricIdentityResolutionEvaluated(
            RawRecord record,
            MetricIdentityResolutionResult result)
        {
            // Intentionally minimal.
            // This exists to make identity resolution observable
            // without promoting or mutating any data.

            // You may later route this to logging, persistence,
            // or in-memory inspection.
        }
    }
}
