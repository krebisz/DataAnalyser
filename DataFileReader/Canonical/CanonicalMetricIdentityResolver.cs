namespace DataFileReader.Canonical;

/// <summary>
///     Canonical Metric Identity Resolver.
///     Declarative, deterministic, non-heuristic.
///     Phase 4: Made public for DataVisualiser integration.
/// </summary>
public sealed class CanonicalMetricIdentityResolver
{
    /// <summary>
    ///     Resolves a canonical metric identity from descriptive metadata.
    /// </summary>
    public MetricIdentityResolutionResult Resolve(string provider, string metricType, string metricSubtype)
    {
        // ----------------------------
        // Validation (explicit)
        // ----------------------------

        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(metricType))
            return MetricIdentityResolutionResult.Failed(IdentityResolutionFailureReason.MissingRequiredMetadata,
                    new[]
                    {
                            $"Provider='{provider ?? "<null>"}'",
                            $"MetricType='{metricType ?? "<null>"}'"
                    });

        // Normalize comparison inputs (not inference)
        var normalizedProvider = provider.Trim();
        var normalizedMetricType = metricType.Trim();

        // ----------------------------
        // Canonical Mapping Rule
        // ----------------------------

        var canonicalId = CanonicalMetricMapping.FromLegacyFields(metricType, metricSubtype);
        if (!string.IsNullOrWhiteSpace(canonicalId))
            return MetricIdentityResolutionResult.Succeeded(new CanonicalMetricId(canonicalId));
        // ----------------------------
        // Explicit non-match
        // ----------------------------

        return MetricIdentityResolutionResult.Failed(IdentityResolutionFailureReason.NoMatchingRule,
                new[]
                {
                        $"Provider='{provider}'",
                        $"MetricType='{metricType}'",
                        $"MetricSubtype='{metricSubtype ?? "<null>"}'"
                });
    }
}