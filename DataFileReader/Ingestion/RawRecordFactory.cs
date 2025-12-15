using DataFileReader.Normalization;
using System;
using System.Collections.Generic;

namespace DataFileReader.Ingestion
{
    /// <summary>
    /// Factory for creating RawRecord instances.
    /// Provides optional lightweight diagnostics.
    /// </summary>
    public static class RawRecordFactory
    {
        /// <summary>
        /// Optional diagnostic hook invoked on RawRecord creation.
        /// Intended for tracing and validation only.
        /// </summary>
        public static Action<RawRecord>? OnCreated { get; set; }

        public static RawRecord Create(
            string sourceId,
            IReadOnlyDictionary<string, object?> fields,
            DateTimeOffset? rawTimestamp = null,
            string? sourceGroup = null,
            IReadOnlyDictionary<string, string>? metadata = null)
        {
            var record = new RawRecord(
                sourceId: sourceId,
                fields: fields,
                rawTimestamp: rawTimestamp,
                sourceGroup: sourceGroup,
                metadata: metadata
            );

            OnCreated?.Invoke(record);
            NormalizationDiagnostics.OnRawRecordObserved?.Invoke(record);

            return record;
        }

        public static RawRecord Create(
            string sourceId,
            IDictionary<string, object?> fields,
            DateTimeOffset? rawTimestamp = null,
            string? sourceGroup = null,
            IDictionary<string, string>? metadata = null)
        {
            var record = new RawRecord(
                sourceId: sourceId,
                fields: new Dictionary<string, object?>(fields),
                rawTimestamp: rawTimestamp,
                sourceGroup: sourceGroup,
                metadata: metadata is null
                    ? null
                    : new Dictionary<string, string>(metadata)
            );

            OnCreated?.Invoke(record);

            return record;
        }
    }
}
