using Dapper;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

internal sealed class DataFetcherMetricCatalogQueries : DataFetcherQueryGroup
{
    public DataFetcherMetricCatalogQueries(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<string>> GetMetricTypes()
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypes
                SELECT DISTINCT c.MetricType 
                FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = c.MetricType
                      AND m.MetricSubtype = c.MetricSubtype
                WHERE c.MetricType IS NOT NULL 
                  AND (m.Disabled IS NULL OR m.Disabled = 0)
                  AND c.RecordCount > 0
                ORDER BY c.MetricType";

        return await conn.QueryAsync<string>(sql);
    }

    public async Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetBaseMetricTypeOptions (HealthMetrics)
                    SELECT c.MetricType, MAX(m.MetricTypeName) AS MetricTypeName
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricType IS NOT NULL
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.RecordCount > 0
                    GROUP BY c.MetricType
                    ORDER BY c.MetricType";

            var rows = await conn.QueryAsync<(string MetricType, string? MetricTypeName)>(sql);
            return rows.Select(row => new MetricNameOption(row.MetricType, string.IsNullOrWhiteSpace(row.MetricTypeName) ? row.MetricType : row.MetricTypeName));
        }

        var tableSql = $@"
                -- DataFetcher.GetBaseMetricTypeOptions ({tableName})
                SELECT t.MetricType, MAX(m.MetricTypeName) AS MetricTypeName
                FROM [dbo].[{tableName}] t
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = t.MetricType
                WHERE t.MetricType IS NOT NULL
                GROUP BY t.MetricType
                ORDER BY t.MetricType";

        var tableRows = await conn.QueryAsync<(string MetricType, string? MetricTypeName)>(tableSql);
        return tableRows.Select(row => new MetricNameOption(row.MetricType, string.IsNullOrWhiteSpace(row.MetricTypeName) ? row.MetricType : row.MetricTypeName));
    }

    public async Task<IEnumerable<string>> GetBaseMetricTypes(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetBaseMetricTypes (HealthMetrics)
                    SELECT DISTINCT c.MetricType 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricType IS NOT NULL 
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.RecordCount > 0
                    ORDER BY c.MetricType";

            return await conn.QueryAsync<string>(sql);
        }

        var fallbackSql = $@"
                -- DataFetcher.GetBaseMetricTypes ({tableName})
                SELECT DISTINCT MetricType 
                FROM [dbo].[{tableName}]
                WHERE MetricType IS NOT NULL
                ORDER BY MetricType";

        return await conn.QueryAsync<string>(fallbackSql);
    }

    public async Task<IEnumerable<string>> GetSubtypesForBaseType(string baseType, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        if (string.Equals(baseType, "(All)", StringComparison.OrdinalIgnoreCase))
            return await GetAllSubtypes(tableName);

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetSubtypesForBaseType (HealthMetrics)
                    SELECT DISTINCT c.MetricSubtype 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricType = @BaseType
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.MetricSubtype IS NOT NULL
                      AND c.MetricSubtype != ''
                      AND c.RecordCount > 0
                    ORDER BY c.MetricSubtype";

            return await conn.QueryAsync<string>(sql, new { BaseType = baseType });
        }

        var fallbackSql = $@"
                -- DataFetcher.GetSubtypesForBaseType ({tableName})
                SELECT DISTINCT MetricSubtype 
                FROM [dbo].[{tableName}]
                WHERE MetricType = @BaseType
                  AND MetricSubtype IS NOT NULL
                  AND MetricSubtype != ''
                ORDER BY MetricSubtype";

        return await conn.QueryAsync<string>(fallbackSql, new { BaseType = baseType });
    }

    public async Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        if (string.Equals(baseType, "(All)", StringComparison.OrdinalIgnoreCase))
            return await GetAllSubtypeOptions(tableName);

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetSubtypeOptionsForBaseType (HealthMetrics)
                    SELECT DISTINCT c.MetricSubtype, ISNULL(m.MetricSubtypeName, c.MetricSubtype) AS MetricSubtypeName
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricType = @MetricType
                      AND c.MetricSubtype IS NOT NULL
                      AND c.MetricSubtype != ''
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.RecordCount > 0
                    ORDER BY c.MetricSubtype";

            var rows = await conn.QueryAsync<(string MetricSubtype, string? MetricSubtypeName)>(sql, new { MetricType = baseType });
            return rows.Select(row => new MetricNameOption(row.MetricSubtype, string.IsNullOrWhiteSpace(row.MetricSubtypeName) ? row.MetricSubtype : row.MetricSubtypeName));
        }

        var tableSql = $@"
                -- DataFetcher.GetSubtypeOptionsForBaseType ({tableName})
                SELECT t.MetricSubtype, MAX(m.MetricSubtypeName) AS MetricSubtypeName
                FROM [dbo].[{tableName}] t
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = t.MetricType
                      AND m.MetricSubtype = t.MetricSubtype
                WHERE t.MetricType = @MetricType
                  AND t.MetricSubtype IS NOT NULL
                  AND t.MetricSubtype != ''
                GROUP BY t.MetricSubtype
                ORDER BY t.MetricSubtype";

        var tableRows = await conn.QueryAsync<(string MetricSubtype, string? MetricSubtypeName)>(tableSql, new { MetricType = baseType });
        return tableRows.Select(row => new MetricNameOption(row.MetricSubtype, string.IsNullOrWhiteSpace(row.MetricSubtypeName) ? row.MetricSubtype : row.MetricSubtypeName));
    }

    public async Task<IEnumerable<string>> GetAllSubtypes(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetAllSubtypes (HealthMetrics)
                    SELECT DISTINCT c.MetricSubtype 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricSubtype IS NOT NULL
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.MetricSubtype != ''
                      AND c.RecordCount > 0
                    ORDER BY c.MetricSubtype";

            return await conn.QueryAsync<string>(sql);
        }

        var fallbackSql = $@"
                -- DataFetcher.GetAllSubtypes ({tableName})
                SELECT DISTINCT MetricSubtype 
                FROM [dbo].[{tableName}]
                WHERE MetricSubtype IS NOT NULL
                  AND MetricSubtype != ''
                ORDER BY MetricSubtype";

        return await conn.QueryAsync<string>(fallbackSql);
    }

    public async Task<IEnumerable<MetricNameOption>> GetAllSubtypeOptions(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetAllSubtypeOptions (HealthMetrics)
                    SELECT DISTINCT c.MetricSubtype, ISNULL(m.MetricSubtypeName, c.MetricSubtype) AS MetricSubtypeName
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                    LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                           ON m.MetricType = c.MetricType
                          AND m.MetricSubtype = c.MetricSubtype
                    WHERE c.MetricSubtype IS NOT NULL
                      AND (m.Disabled IS NULL OR m.Disabled = 0)
                      AND c.MetricSubtype != ''
                      AND c.RecordCount > 0
                    ORDER BY c.MetricSubtype";

            var rows = await conn.QueryAsync<(string MetricSubtype, string? MetricSubtypeName)>(sql);
            return rows.Select(row => new MetricNameOption(row.MetricSubtype, string.IsNullOrWhiteSpace(row.MetricSubtypeName) ? row.MetricSubtype : row.MetricSubtypeName));
        }

        var fallbackSql = $@"
                -- DataFetcher.GetAllSubtypeOptions ({tableName})
                SELECT t.MetricSubtype, MAX(m.MetricSubtypeName) AS MetricSubtypeName
                FROM [dbo].[{tableName}] t
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = t.MetricType
                      AND m.MetricSubtype = t.MetricSubtype
                WHERE t.MetricSubtype IS NOT NULL
                  AND t.MetricSubtype != ''
                GROUP BY t.MetricSubtype
                ORDER BY t.MetricSubtype";

        var fallbackRows = await conn.QueryAsync<(string MetricSubtype, string? MetricSubtypeName)>(fallbackSql);
        return fallbackRows.Select(row => new MetricNameOption(row.MetricSubtype, string.IsNullOrWhiteSpace(row.MetricSubtypeName) ? row.MetricSubtype : row.MetricSubtypeName));
    }

    public async Task<Dictionary<string, List<string>>> GetMetricTypesByBaseType()
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypesByBaseType
                SELECT c.MetricType, c.MetricSubtype
                FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = c.MetricType
                      AND m.MetricSubtype = c.MetricSubtype
                WHERE c.MetricType IS NOT NULL 
                  AND (m.Disabled IS NULL OR m.Disabled = 0)
                  AND c.RecordCount > 0";

        var results = await conn.QueryAsync<(string MetricType, string MetricSubtype)>(sql);
        return results
            .GroupBy(r => r.MetricType)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.MetricSubtype).Where(st => !string.IsNullOrEmpty(st)).Distinct().OrderBy(st => st).ToList());
    }
}
