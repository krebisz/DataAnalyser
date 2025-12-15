using System;
using System.Collections.Generic;
using DataFileReader.Ingestion;
using DataFileReader.Canonical;

namespace DataFileReader.Normalization.Stages
{
    public sealed class MetricIdentityResolutionStage : INormalizationStage
    {
        private readonly CanonicalMetricIdentityResolver _resolver =
            new CanonicalMetricIdentityResolver();

        public IReadOnlyCollection<RawRecord> Process(
            IReadOnlyCollection<RawRecord> input,
            NormalizationContext context)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (var record in input)
            {
                if (record == null)
                    continue;

                var provider = record.SourceGroup;
                var metricType = record.SourceId;
                string? metricSubtype = null;

                var result = _resolver.Resolve(
                    provider,
                    metricType,
                    metricSubtype);

                NormalizationDiagnostics.OnMetricIdentityResolutionEvaluated(
                    record,
                    result);
            }

            return input;
        }
    }
}
