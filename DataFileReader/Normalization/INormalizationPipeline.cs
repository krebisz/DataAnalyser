using System.Collections.Generic;
using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;

namespace DataFileReader.Normalization
{
    /// <summary>
    /// Defines the contract for transforming RawRecords into
    /// canonical metric series.
    /// </summary>
    public interface INormalizationPipeline
    {
        IReadOnlyList<CanonicalMetricSeries<object>> Normalize(
            IReadOnlyCollection<RawRecord> rawRecords);
    }
}
