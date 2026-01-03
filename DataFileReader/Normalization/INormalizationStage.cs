using DataFileReader.Ingestion;

namespace DataFileReader.Normalization;

/// <summary>
///     Represents a single stage in the normalization pipeline.
///     Stages may enrich, transform, or filter records.
/// </summary>
public interface INormalizationStage
{
    IReadOnlyCollection<RawRecord> Process(IReadOnlyCollection<RawRecord> input, NormalizationContext context);
}