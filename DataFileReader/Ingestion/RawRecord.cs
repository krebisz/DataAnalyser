namespace DataFileReader.Ingestion
{
    /// <summary>
    /// RawRecord represents a single, lossless, uninterpreted data observation
    /// extracted from a source.
    ///
    /// It contains NO semantic meaning, NO normalization, and NO inference.
    /// Interpretation is explicitly deferred to the normalization pipeline.
    /// </summary>
    public sealed class RawRecord
    {
        /// <summary>
        /// Identifier of the source that emitted this record
        /// (e.g. file path, device name, export type).
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        /// Optional logical grouping within the source
        /// (e.g. CSV name, section, category).
        /// </summary>
        public string? SourceGroup { get; }

        /// <summary>
        /// Raw field/value pairs exactly as extracted.
        /// Values are untyped beyond object and must not be interpreted here.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Fields { get; }

        /// <summary>
        /// Raw timestamp information as provided by the source, if any.
        /// This value is NOT normalized or interpreted.
        /// </summary>
        public DateTimeOffset? RawTimestamp { get; }

        /// <summary>
        /// Arbitrary provenance metadata (e.g. row number, original header names).
        /// </summary>
        public IReadOnlyDictionary<string, string>? Metadata { get; }

        public RawRecord(
            string sourceId,
            IReadOnlyDictionary<string, object?> fields,
            DateTimeOffset? rawTimestamp = null,
            string? sourceGroup = null,
            IReadOnlyDictionary<string, string>? metadata = null)
        {
            SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
            RawTimestamp = rawTimestamp;
            SourceGroup = sourceGroup;
            Metadata = metadata;
        }
    }
}
