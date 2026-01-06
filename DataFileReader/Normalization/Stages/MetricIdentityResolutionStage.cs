using DataFileReader.Canonical;
using DataFileReader.Ingestion;

namespace DataFileReader.Normalization.Stages;

public sealed class MetricIdentityResolutionStage : INormalizationStage
{
    private readonly CanonicalMetricIdentityResolver _resolver = new();

    public IReadOnlyCollection<RawRecord> Process(IReadOnlyCollection<RawRecord> input, NormalizationContext context)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        foreach (var record in input)
        {
            if (record == null)
                continue;

            var provider = record.SourceGroup ?? string.Empty;
            var metricType = record.SourceId ?? string.Empty;
            var metricSubtype = string.Empty;

            var result = _resolver.Resolve(provider, metricType, metricSubtype);

            NormalizationDiagnostics.OnMetricIdentityResolutionEvaluated(record, result);
        }

        return input;
    }
}
