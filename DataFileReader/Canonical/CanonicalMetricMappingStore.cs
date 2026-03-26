using System.Configuration;
using System.Diagnostics;

namespace DataFileReader.Canonical;

internal static class CanonicalMetricMappingStore
{
    internal const string MappingTableName = "CanonicalMetricMappings";
    internal const string AllSubtypeToken = "(all)";

    private static readonly object InitLock = new();
    private static bool _initialized;
    private static Dictionary<string, (string MetricType, string MetricSubtype)> _byCanonicalId = new(StringComparer.OrdinalIgnoreCase);
    private static Dictionary<(string MetricType, string MetricSubtype), string> _byLegacyKey = new();

    public static string? FromLegacyFields(string metricType, string? metricSubtype)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(metricType))
            return null;

        var normalizedType = NormalizeSegment(metricType);
        var normalizedSubtype = NormalizeSubtype(metricSubtype);
        var key = (normalizedType, normalizedSubtype);

        return _byLegacyKey.TryGetValue(key, out var canonicalId) ? canonicalId : null;
    }

    public static (string? MetricType, string? Subtype) ToLegacyFields(string canonicalMetricId)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(canonicalMetricId))
            return (null, null);

        if (_byCanonicalId.TryGetValue(canonicalMetricId.Trim(), out var legacy))
            return (legacy.MetricType, legacy.MetricSubtype == AllSubtypeToken ? null : legacy.MetricSubtype);

        return (null, null);
    }

    internal static void InitializeForTesting(IEnumerable<CanonicalMetricMappingRecord> mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        lock (InitLock)
        {
            ApplyMappings(mappings);
            _initialized = true;
        }
    }

    internal static void ResetForTesting()
    {
        lock (InitLock)
        {
            _initialized = false;
            _byCanonicalId = new Dictionary<string, (string MetricType, string MetricSubtype)>(StringComparer.OrdinalIgnoreCase);
            _byLegacyKey = new Dictionary<(string MetricType, string MetricSubtype), string>();
        }
    }

    private static void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (InitLock)
        {
            if (_initialized)
                return;

            TryInitializeMappings();
            _initialized = true;
        }
    }

    private static void TryInitializeMappings()
    {
        var connectionString = ConfigurationManager.AppSettings["HealthDB"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Debug.WriteLine("[CanonicalMapping] HealthDB connection string not configured.");
            return;
        }

        try
        {
            var mappings = CanonicalMetricMappingSqlDataSource.LoadOrInitializeMappings(connectionString);
            ApplyMappings(mappings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CanonicalMapping] Failed to initialize mappings: {ex.Message}");
        }
    }

    private static void ApplyMappings(IEnumerable<CanonicalMetricMappingRecord> mappings)
    {
        var byCanonicalId = new Dictionary<string, (string MetricType, string MetricSubtype)>(StringComparer.OrdinalIgnoreCase);
        var byLegacyKey = new Dictionary<(string MetricType, string MetricSubtype), string>();

        foreach (var mapping in mappings)
        {
            if (string.IsNullOrWhiteSpace(mapping.MetricType) ||
                string.IsNullOrWhiteSpace(mapping.MetricSubtype) ||
                string.IsNullOrWhiteSpace(mapping.CanonicalMetricId))
                continue;

            var normalizedType = NormalizeSegment(mapping.MetricType);
            var normalizedSubtype = NormalizeSubtype(mapping.MetricSubtype);
            var normalizedCanonicalId = mapping.CanonicalMetricId.Trim().ToLowerInvariant();

            byLegacyKey[(normalizedType, normalizedSubtype)] = normalizedCanonicalId;
            byCanonicalId[normalizedCanonicalId] = (normalizedType, normalizedSubtype);
        }

        _byLegacyKey = byLegacyKey;
        _byCanonicalId = byCanonicalId;
    }

    internal static string NormalizeSubtype(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype))
            return AllSubtypeToken;

        var trimmed = subtype.Trim();
        if (string.Equals(trimmed, AllSubtypeToken, StringComparison.OrdinalIgnoreCase))
            return AllSubtypeToken;

        return NormalizeSegment(trimmed);
    }

    internal static string NormalizeSegment(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.Length == 0)
            return trimmed;

        var lower = trimmed.ToLowerInvariant();
        var chars = new char[lower.Length];

        for (var i = 0; i < lower.Length; i++)
        {
            var c = lower[i];
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                chars[i] = c;
                continue;
            }

            chars[i] = char.IsWhiteSpace(c) || c == '.' ? '_' : '_';
        }

        return new string(chars);
    }
}
