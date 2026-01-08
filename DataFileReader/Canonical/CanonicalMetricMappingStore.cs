using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DataFileReader.Canonical;

internal static class CanonicalMetricMappingStore
{
    private const string MappingTableName = "CanonicalMetricMappings";
    private const string AllSubtypeToken = "(all)";

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

    public static(string? MetricType, string? Subtype) ToLegacyFields(string canonicalMetricId)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(canonicalMetricId))
            return (null, null);

        if (_byCanonicalId.TryGetValue(canonicalMetricId.Trim(), out var legacy))
            return (legacy.MetricType, legacy.MetricSubtype == AllSubtypeToken ? null : legacy.MetricSubtype);

        return (null, null);
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
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            EnsureMappingTableExists(connection);
            SeedMappingsIfMissing(connection);

            LoadMappings(connection);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CanonicalMapping] Failed to initialize mappings: {ex.Message}");
        }
    }

    private static void EnsureMappingTableExists(SqlConnection connection)
    {
        var sql = $@"
IF OBJECT_ID(N'dbo.{MappingTableName}', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.{MappingTableName}
    (
        MetricType NVARCHAR(200) NOT NULL,
        MetricSubtype NVARCHAR(200) NOT NULL,
        CanonicalMetricId NVARCHAR(400) NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_{MappingTableName}_IsEnabled DEFAULT(1),
        CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_{MappingTableName}_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_{MappingTableName} PRIMARY KEY (MetricType, MetricSubtype)
    );
END";
        using var cmd = new SqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    private static void SeedMappingsIfMissing(SqlConnection connection)
    {
        var seedSource = "SELECT DISTINCT MetricType, MetricSubtype FROM dbo.HealthMetrics";
        using var seedCmd = new SqlCommand(seedSource, connection);

        using var reader = seedCmd.ExecuteReader();
        var pairs = new List<(string MetricType, string? MetricSubtype)>();
        var metricTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.Read())
        {
            var metricType = reader["MetricType"] as string;
            var metricSubtype = reader["MetricSubtype"] as string;

            if (string.IsNullOrWhiteSpace(metricType))
                continue;

            pairs.Add((metricType, metricSubtype));
            metricTypes.Add(metricType);
        }

        reader.Close();

        foreach (var pair in pairs)
        {
            var normalizedType = NormalizeSegment(pair.MetricType);
            var normalizedSubtype = NormalizeSubtype(pair.MetricSubtype);
            var canonicalId = $"{normalizedType}.{normalizedSubtype}";

            var insertSql = $@"
IF NOT EXISTS (
    SELECT 1 FROM dbo.{MappingTableName}
    WHERE MetricType = @MetricType AND MetricSubtype = @MetricSubtype
)
BEGIN
    INSERT INTO dbo.{MappingTableName} (MetricType, MetricSubtype, CanonicalMetricId)
    VALUES (@MetricType, @MetricSubtype, @CanonicalMetricId);
END";

            using var insertCmd = new SqlCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@MetricType", normalizedType);
            insertCmd.Parameters.AddWithValue("@MetricSubtype", normalizedSubtype);
            insertCmd.Parameters.AddWithValue("@CanonicalMetricId", canonicalId);
            insertCmd.ExecuteNonQuery();
        }

        foreach (var metricType in metricTypes)
        {
            var normalizedType = NormalizeSegment(metricType);
            var normalizedSubtype = AllSubtypeToken;
            var canonicalId = $"{normalizedType}.{normalizedSubtype}";

            var insertSql = $@"
IF NOT EXISTS (
    SELECT 1 FROM dbo.{MappingTableName}
    WHERE MetricType = @MetricType AND MetricSubtype = @MetricSubtype
)
BEGIN
    INSERT INTO dbo.{MappingTableName} (MetricType, MetricSubtype, CanonicalMetricId)
    VALUES (@MetricType, @MetricSubtype, @CanonicalMetricId);
END";

            using var insertCmd = new SqlCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@MetricType", normalizedType);
            insertCmd.Parameters.AddWithValue("@MetricSubtype", normalizedSubtype);
            insertCmd.Parameters.AddWithValue("@CanonicalMetricId", canonicalId);
            insertCmd.ExecuteNonQuery();
        }
    }

    private static void LoadMappings(SqlConnection connection)
    {
        var sql = $"SELECT MetricType, MetricSubtype, CanonicalMetricId FROM dbo.{MappingTableName} WHERE IsEnabled = 1";
        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        var byCanonicalId = new Dictionary<string, (string MetricType, string MetricSubtype)>(StringComparer.OrdinalIgnoreCase);
        var byLegacyKey = new Dictionary<(string MetricType, string MetricSubtype), string>();

        while (reader.Read())
        {
            var metricType = reader["MetricType"] as string;
            var metricSubtype = reader["MetricSubtype"] as string;
            var canonicalId = reader["CanonicalMetricId"] as string;

            if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(metricSubtype) || string.IsNullOrWhiteSpace(canonicalId))
                continue;

            var normalizedType = NormalizeSegment(metricType);
            var normalizedSubtype = NormalizeSubtype(metricSubtype);
            var normalizedCanonicalId = canonicalId.Trim().ToLowerInvariant();

            byLegacyKey[(normalizedType, normalizedSubtype)] = normalizedCanonicalId;
            byCanonicalId[normalizedCanonicalId] = (normalizedType, normalizedSubtype);
        }

        _byLegacyKey = byLegacyKey;
        _byCanonicalId = byCanonicalId;
    }

    private static string NormalizeSubtype(string? subtype)
    {
        if (string.IsNullOrWhiteSpace(subtype))
            return AllSubtypeToken;

        var trimmed = subtype.Trim();
        if (string.Equals(trimmed, AllSubtypeToken, StringComparison.OrdinalIgnoreCase))
            return AllSubtypeToken;

        return NormalizeSegment(trimmed);
    }

    private static string NormalizeSegment(string value)
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

            if (char.IsWhiteSpace(c) || c == '.')
            {
                chars[i] = '_';
                continue;
            }

            chars[i] = '_';
        }

        return new string(chars);
    }
}