using System;
using System.Collections.Generic;
using System.Linq;
using DataFileReader.Ingestion;
using DataFileReader.Normalization.Canonical;

namespace DataFileReader.Normalization
{
    /// <summary>
    /// Minimal normalization pipeline implementation.
    /// Executes configured stages and produces canonical metric series.
    ///
    /// This implementation performs NO semantic normalization.
    /// It exists only to structure control flow.
    /// </summary>
    public sealed class DefaultNormalizationPipeline : INormalizationPipeline
    {
        private readonly IReadOnlyList<INormalizationStage> _stages;
        private readonly NormalizationContext _context;

        public DefaultNormalizationPipeline(
            IReadOnlyList<INormalizationStage> stages,
            NormalizationContext context)
        {
            _stages = stages ?? throw new ArgumentNullException(nameof(stages));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IReadOnlyList<CanonicalMetricSeries<object>> Normalize(
            IReadOnlyCollection<RawRecord> rawRecords)
        {
            if (rawRecords == null)
                throw new ArgumentNullException(nameof(rawRecords));

            IReadOnlyCollection<RawRecord> current = rawRecords;

            foreach (var stage in _stages)
            {
                current = stage.Process(current, _context);
            }

            // No CMS production yet.
            // This is intentional: semantic normalization is not implemented.
            return Array.Empty<CanonicalMetricSeries<object>>();
        }
    }
}
