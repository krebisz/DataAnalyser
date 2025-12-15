using System;
using System.Collections.Generic;
using DataFileReader.Ingestion;

namespace DataFileReader.Normalization.Stages
{
    /// <summary>
    /// Resolves canonical metric identities from RawRecords.
    ///
    /// This stage:
    /// - does NOT normalize values
    /// - does NOT normalize time
    /// - does NOT emit CMS
    /// - does NOT mutate RawRecords
    ///
    /// It is responsible only for determining
    /// \"what metric this record represents\".
    /// </summary>
    public sealed class MetricIdentityResolutionStage : INormalizationStage
    {
        public IReadOnlyCollection<RawRecord> Process(
            IReadOnlyCollection<RawRecord> input,
            NormalizationContext context)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // NOTE:
            // Identity resolution is intentionally NOT implemented yet.
            // This stage exists to:
            //  - define execution order
            //  - define responsibility boundaries
            //  - provide a hook point for future rules

            foreach (var record in input)
            {
                // Placeholder for future identity resolution:
                //
                // Examples (NOT IMPLEMENTED):
                // - Inspect record.Fields keys
                // - Inspect record.SourceId / SourceGroup
                // - Consult mapping rules
                // - Emit identity evidence into context
                //
                // Ambiguity must be preserved explicitly.
            }

            // Pass records through unchanged.
            return input;
        }
    }
}
