using System;
using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;

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
    }
}
