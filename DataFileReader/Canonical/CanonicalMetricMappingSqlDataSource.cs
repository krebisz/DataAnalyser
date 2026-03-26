using Microsoft.Data.SqlClient;

namespace DataFileReader.Canonical;

internal static class CanonicalMetricMappingSqlDataSource
{
    public static IReadOnlyList<CanonicalMetricMappingRecord> LoadOrInitializeMappings(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        connection.Open();

        EnsureMappingTableExists(connection);
        SeedMappingsIfMissing(connection);

        return LoadMappings(connection);
    }

    private static void EnsureMappingTableExists(SqlConnection connection)
    {
        var sql = $@"
IF OBJECT_ID(N'dbo.{CanonicalMetricMappingStore.MappingTableName}', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.{CanonicalMetricMappingStore.MappingTableName}
    (
        MetricType NVARCHAR(200) NOT NULL,
        MetricSubtype NVARCHAR(200) NOT NULL,
        CanonicalMetricId NVARCHAR(400) NOT NULL,
        IsEnabled BIT NOT NULL CONSTRAINT DF_{CanonicalMetricMappingStore.MappingTableName}_IsEnabled DEFAULT(1),
        CreatedUtc DATETIME2 NOT NULL CONSTRAINT DF_{CanonicalMetricMappingStore.MappingTableName}_CreatedUtc DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_{CanonicalMetricMappingStore.MappingTableName} PRIMARY KEY (MetricType, MetricSubtype)
    );
END";

        using var cmd = new SqlCommand(sql, connection);
        cmd.ExecuteNonQuery();
    }

    private static void SeedMappingsIfMissing(SqlConnection connection)
    {
        const string seedSource = "SELECT DISTINCT MetricType, MetricSubtype FROM dbo.HealthMetrics";
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
            var normalizedType = CanonicalMetricMappingStore.NormalizeSegment(pair.MetricType);
            var normalizedSubtype = CanonicalMetricMappingStore.NormalizeSubtype(pair.MetricSubtype);
            var canonicalId = $"{normalizedType}.{normalizedSubtype}";

            var insertSql = $@"
IF NOT EXISTS (
    SELECT 1 FROM dbo.{CanonicalMetricMappingStore.MappingTableName}
    WHERE MetricType = @MetricType AND MetricSubtype = @MetricSubtype
)
BEGIN
    INSERT INTO dbo.{CanonicalMetricMappingStore.MappingTableName} (MetricType, MetricSubtype, CanonicalMetricId)
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
            var normalizedType = CanonicalMetricMappingStore.NormalizeSegment(metricType);
            var normalizedSubtype = CanonicalMetricMappingStore.AllSubtypeToken;
            var canonicalId = $"{normalizedType}.{normalizedSubtype}";

            var insertSql = $@"
IF NOT EXISTS (
    SELECT 1 FROM dbo.{CanonicalMetricMappingStore.MappingTableName}
    WHERE MetricType = @MetricType AND MetricSubtype = @MetricSubtype
)
BEGIN
    INSERT INTO dbo.{CanonicalMetricMappingStore.MappingTableName} (MetricType, MetricSubtype, CanonicalMetricId)
    VALUES (@MetricType, @MetricSubtype, @CanonicalMetricId);
END";

            using var insertCmd = new SqlCommand(insertSql, connection);
            insertCmd.Parameters.AddWithValue("@MetricType", normalizedType);
            insertCmd.Parameters.AddWithValue("@MetricSubtype", normalizedSubtype);
            insertCmd.Parameters.AddWithValue("@CanonicalMetricId", canonicalId);
            insertCmd.ExecuteNonQuery();
        }
    }

    private static IReadOnlyList<CanonicalMetricMappingRecord> LoadMappings(SqlConnection connection)
    {
        var sql = $"SELECT MetricType, MetricSubtype, CanonicalMetricId FROM dbo.{CanonicalMetricMappingStore.MappingTableName} WHERE IsEnabled = 1";
        using var cmd = new SqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        var mappings = new List<CanonicalMetricMappingRecord>();

        while (reader.Read())
        {
            var metricType = reader["MetricType"] as string;
            var metricSubtype = reader["MetricSubtype"] as string;
            var canonicalId = reader["CanonicalMetricId"] as string;

            if (string.IsNullOrWhiteSpace(metricType) || string.IsNullOrWhiteSpace(metricSubtype) || string.IsNullOrWhiteSpace(canonicalId))
                continue;

            mappings.Add(new CanonicalMetricMappingRecord(metricType, metricSubtype, canonicalId));
        }

        return mappings;
    }
}
