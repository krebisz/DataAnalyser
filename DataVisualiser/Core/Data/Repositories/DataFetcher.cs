using System.Text;
using Dapper;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Core.Data.QueryBuilders;
using DataVisualiser.Shared.Models;
using Microsoft.Data.SqlClient;
using DataVisualiser.Core.Data;


namespace DataVisualiser.Core.Data.Repositories;

public class DataFetcher
{
    private readonly string _connectionString;

    public DataFetcher(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));

        _connectionString = connectionString;
    }

    public async Task<IEnumerable<dynamic>> GetCombinedData(string[] tables, DateTime from, DateTime to)
    {
        if (tables == null || tables.Length == 0)
            throw new ArgumentException("At least one table must be provided.", nameof(tables));

        if (from > to)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));

        using var conn = new SqlConnection(_connectionString);

        var baseTable = tables[0];
        var sql = $"-- DataFetcher.GetCombinedData{Environment.NewLine}SELECT t0.[datetime]";

        for (var i = 0; i < tables.Length; i++)
            sql += $", t{i}.Value AS {tables[i]}";

        sql += $" FROM {baseTable} t0 ";

        for (var i = 1; i < tables.Length; i++)
            sql += $"LEFT JOIN {tables[i]} t{i} ON t0.[datetime] = t{i}.[datetime] ";

        sql += "WHERE t0.[datetime] BETWEEN @from AND @to ORDER BY t0.[datetime]";

        return await conn.QueryAsync<dynamic>(sql,
                new
                {
                        from,
                        to
                });
    }

    public async Task<IEnumerable<string>> GetMetricTypes()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypes
                SELECT DISTINCT MetricType 
                FROM {DataAccessDefaults.HealthMetricsCountsTable} 
                WHERE MetricType IS NOT NULL 
                  AND RecordCount > 0
                ORDER BY MetricType";

        var metricTypes = await conn.QueryAsync<string>(sql);
        return metricTypes;
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetMetricTypeDateRange(string metricType)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypeDateRange
                SELECT 
                    MIN(NormalizedTimestamp) AS MinDate,
                    MAX(NormalizedTimestamp) AS MaxDate
                FROM {DataAccessDefaults.DefaultTableName}
                WHERE MetricType = @MetricType
                    AND NormalizedTimestamp IS NOT NULL";

        var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql,
                new
                {
                        MetricType = metricType
                });

        if (result != null && result.MinDate.HasValue && result.MaxDate.HasValue)
            return (result.MinDate.Value, result.MaxDate.Value);

        return null;
    }

    /// <summary>
    ///     Retrieves health metrics data from the database filtered by MetricType and date range.
    /// </summary>
    public async Task<IEnumerable<MetricData>> GetHealthMetricsData(string metricType, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        if (from > to)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetHealthMetricsData
                SELECT 
                    NormalizedTimestamp,
                    Value,
                    Unit,
                    Provider
                FROM {DataAccessDefaults.DefaultTableName}
                WHERE MetricType = @MetricType
                    AND NormalizedTimestamp >= @FromDate
                    AND NormalizedTimestamp <= @ToDate
                    AND Value IS NOT NULL
                ORDER BY NormalizedTimestamp";

        var results = await conn.QueryAsync<MetricData>(sql,
                new
                {
                        MetricType = metricType,
                        FromDate = from,
                        ToDate = to
                });

        return results;
    }

    /// <summary>
    ///     Gets distinct base metric types from the specified table.
    /// </summary>
    public async Task<IEnumerable<string>> GetBaseMetricTypes(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetBaseMetricTypes (HealthMetrics)
                    SELECT DISTINCT MetricType 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} 
                    WHERE MetricType IS NOT NULL 
                      AND RecordCount > 0
                    ORDER BY MetricType";

            var metricTypes = await conn.QueryAsync<string>(sql);
            return metricTypes;
        }
        else
        {
            var sql = $@"
                    -- DataFetcher.GetBaseMetricTypes ({tableName})
                    SELECT DISTINCT MetricType 
                    FROM [dbo].[{tableName}]
                    WHERE MetricType IS NOT NULL
                    ORDER BY MetricType";

            var metricTypes = await conn.QueryAsync<string>(sql);
            return metricTypes;
        }
    }

    /// <summary>
    ///     Gets distinct subtypes for a given base metric type from the specified table.
    /// </summary>
    public async Task<IEnumerable<string>> GetSubtypesForBaseType(string baseType, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        if (string.Equals(baseType, "(All)", StringComparison.OrdinalIgnoreCase))
            return await GetAllSubtypes(tableName);

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetSubtypesForBaseType (HealthMetrics)
                    SELECT DISTINCT MetricSubtype 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} 
                    WHERE MetricType = @BaseType
                      AND MetricSubtype IS NOT NULL
                      AND MetricSubtype != ''
                      AND RecordCount > 0
                    ORDER BY MetricSubtype";

            var subtypes = await conn.QueryAsync<string>(sql,
                    new
                    {
                            BaseType = baseType
                    });
            return subtypes;
        }
        else
        {
            var sql = $@"
                    -- DataFetcher.GetSubtypesForBaseType ({tableName})
                    SELECT DISTINCT MetricSubtype 
                    FROM [dbo].[{tableName}]
                    WHERE MetricType = @BaseType
                      AND MetricSubtype IS NOT NULL
                      AND MetricSubtype != ''
                    ORDER BY MetricSubtype";

            var subtypes = await conn.QueryAsync<string>(sql,
                    new
                    {
                            BaseType = baseType
                    });
            return subtypes;
        }
    }

    /// <summary>
    ///     Gets distinct subtypes across all metric types from the specified table.
    /// </summary>
    public async Task<IEnumerable<string>> GetAllSubtypes(string tableName = DataAccessDefaults.DefaultTableName)
    {
        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        if (tableName == DataAccessDefaults.DefaultTableName)
        {
            var sql = $@"
                    -- DataFetcher.GetAllSubtypes (HealthMetrics)
                    SELECT DISTINCT MetricSubtype 
                    FROM {DataAccessDefaults.HealthMetricsCountsTable} 
                    WHERE MetricSubtype IS NOT NULL
                      AND MetricSubtype != ''
                      AND RecordCount > 0
                    ORDER BY MetricSubtype";

            var subtypes = await conn.QueryAsync<string>(sql);
            return subtypes;
        }

        var fallbackSql = $@"
                -- DataFetcher.GetAllSubtypes ({tableName})
                SELECT DISTINCT MetricSubtype 
                FROM [dbo].[{tableName}]
                WHERE MetricSubtype IS NOT NULL
                  AND MetricSubtype != ''
                ORDER BY MetricSubtype";

        var fallbackSubtypes = await conn.QueryAsync<string>(fallbackSql);
        return fallbackSubtypes;
    }

    /// <summary>
    ///     Gets all metric types grouped by base type and subtype from HealthMetricsCounts
    ///     where RecordCount > 0.
    /// </summary>
    public async Task<Dictionary<string, List<string>>> GetMetricTypesByBaseType()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypesByBaseType
                SELECT MetricType, MetricSubtype
                FROM {DataAccessDefaults.HealthMetricsCountsTable} 
                WHERE MetricType IS NOT NULL 
                  AND RecordCount > 0";

        var results = await conn.QueryAsync<(string MetricType, string MetricSubtype)>(sql);

        var grouped = results.GroupBy(r => r.MetricType).ToDictionary(g => g.Key, g => g.Select(r => r.MetricSubtype).Where(st => !string.IsNullOrEmpty(st)).Distinct().OrderBy(st => st).ToList());

        return grouped;
    }

    /// <summary>
    ///     Retrieves health metrics data filtered by base type and optional subtype from the specified table.
    /// </summary>
    /// <param name="maxRecords">
    ///     Maximum number of records to return. If null, returns all records. Used for performance
    ///     optimization with large datasets.
    /// </param>
    public async Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(
        string baseType,
        string? subtype = null,
        DateTime? from = null,
        DateTime? to = null,
        string tableName = DataAccessDefaults.DefaultTableName,
        int? maxRecords = null,
        SamplingMode samplingMode = SamplingMode.None,
        int? targetSamples = null)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var providerColumn = SqlQueryBuilder.GetProviderColumn(tableName);
        var sql = new StringBuilder();
        var parameters = new DynamicParameters();

        // ---------- SAMPLING PATH ----------
        if (samplingMode == SamplingMode.UniformOverTime
            && targetSamples.HasValue
            && targetSamples.Value > 0)
        {
            sql.Append($@"
            -- DataFetcher.GetHealthMetricsDataByBaseType (UniformOverTime sampling)
            WITH OrderedData AS (
                SELECT
                    NormalizedTimestamp,
                    Value,
                    Unit,
                    {providerColumn},
                    ROW_NUMBER() OVER (ORDER BY NormalizedTimestamp) AS rn,
                    COUNT(*) OVER () AS total_count
                FROM [dbo].[{tableName}]
                WHERE 1=1");

            SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
            SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
            SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);

            sql.Append(@"
                    AND Value IS NOT NULL
            )
            SELECT
                NormalizedTimestamp,
                Value,
                Unit,
                " + providerColumn + @"
            FROM OrderedData
            WHERE rn = 1
               OR rn = total_count
               OR rn % CEILING(1.0 * total_count / @TargetSamples) = 1
            ORDER BY NormalizedTimestamp");

            parameters.Add("@TargetSamples", targetSamples.Value);
        }
        // ---------- LEGACY LIMITING PATH ----------
        else if (maxRecords.HasValue && maxRecords.Value > 0)
        {
            sql.Append($@"
            -- DataFetcher.GetHealthMetricsDataByBaseType (TOP {maxRecords.Value})
            SELECT TOP {maxRecords.Value}
                NormalizedTimestamp,
                Value,
                Unit,
                {providerColumn}
            FROM [dbo].[{tableName}]
            WHERE 1=1");

            SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
            SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
            SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);

            sql.Append(@"
                AND Value IS NOT NULL
            ORDER BY NormalizedTimestamp");
        }
        // ---------- FULL DATA PATH ----------
        else
        {
            sql.Append($@"
            -- DataFetcher.GetHealthMetricsDataByBaseType (Unbounded)
            SELECT
                NormalizedTimestamp,
                Value,
                Unit,
                {providerColumn}
            FROM [dbo].[{tableName}]
            WHERE 1=1");

            SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
            SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
            SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);

            sql.Append(@"
                AND Value IS NOT NULL
            ORDER BY NormalizedTimestamp");
        }

        return await conn.QueryAsync<MetricData>(sql.ToString(), parameters);
    }


    /// <summary>
    ///     Gets date range for a base metric type and optional subtype from the specified table.
    /// </summary>
    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype = null, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = new StringBuilder($@"
                -- DataFetcher.GetBaseTypeDateRange
                SELECT 
                    MIN(NormalizedTimestamp) AS MinDate,
                    MAX(NormalizedTimestamp) AS MaxDate
                FROM [dbo].[{tableName}]
                WHERE NormalizedTimestamp IS NOT NULL");

        var parameters = new DynamicParameters();
        SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
        SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);

        var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql.ToString(), parameters);

        if (result != null && result.MinDate.HasValue && result.MaxDate.HasValue)
            return (result.MinDate.Value, result.MaxDate.Value);

        return null;
    }

    /// <summary>
    ///     Gets the record count for a specific MetricType and optional MetricSubtype.
    ///     Returns 0 if the combination doesn't exist.
    /// </summary>
    public async Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCount
                SELECT RecordCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                WHERE MetricType = @MetricType
                  AND MetricSubtype = @MetricSubtype";

        var result = await conn.QuerySingleOrDefaultAsync<long?>(sql,
                new
                {
                        MetricType = metricType,
                        MetricSubtype = metricSubtype ?? string.Empty
                });

        return result ?? 0;
    }

    /// <summary>
    ///     Gets all MetricType/Subtype combinations with their record counts.
    /// </summary>
    public async Task<Dictionary<(string MetricType, string? MetricSubtype), long>> GetAllRecordCounts()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetAllRecordCounts
                SELECT MetricType, MetricSubtype, RecordCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                ORDER BY MetricType, MetricSubtype";

        var results = await conn.QueryAsync<(string MetricType, string MetricSubtype, long RecordCount)>(sql);

        return results.ToDictionary(r => (r.MetricType, string.IsNullOrEmpty(r.MetricSubtype) ? null : r.MetricSubtype), r => r.RecordCount);
    }

    /// <summary>
    ///     Gets record counts grouped by MetricType (summing all subtypes).
    /// </summary>
    public async Task<Dictionary<string, long>> GetRecordCountsByMetricType()
    {
        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCountsByMetricType
                SELECT MetricType, SUM(RecordCount) AS TotalCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                GROUP BY MetricType
                ORDER BY MetricType";

        var results = await conn.QueryAsync<(string MetricType, long TotalCount)>(sql);

        return results.ToDictionary(r => r.MetricType, r => r.TotalCount);
    }

    /// <summary>
    ///     Gets record counts for a specific MetricType, grouped by MetricSubtype.
    /// </summary>
    public async Task<Dictionary<string, long>> GetRecordCountsBySubtype(string metricType)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCountsBySubtype
                SELECT MetricSubtype, RecordCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                WHERE MetricType = @MetricType
                ORDER BY MetricSubtype";

        var results = await conn.QueryAsync<(string MetricSubtype, long RecordCount)>(sql,
                new
                {
                        MetricType = metricType
                });

        return results.ToDictionary(r => r.MetricSubtype, r => r.RecordCount);
    }
}