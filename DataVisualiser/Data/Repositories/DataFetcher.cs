using Dapper;
using DataVisualiser.Data;
using DataVisualiser.Models;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace DataVisualiser.Data.Repositories
{
    public class DataFetcher
    {
        private readonly string _connectionString;
        public DataFetcher(string connectionString) => _connectionString = connectionString;

        public async Task<IEnumerable<dynamic>> GetCombinedData(string[] tables, DateTime from, DateTime to)
        {
            using var conn = new SqlConnection(_connectionString);

            // Build dynamic join
            var baseTable = tables[0];
            var sql = $"SELECT t0.[datetime]";

            for (int i = 0; i < tables.Length; i++)
                sql += $", t{i}.Value AS {tables[i]}";

            sql += $" FROM {baseTable} t0 ";

            for (int i = 1; i < tables.Length; i++)
                sql += $"LEFT JOIN {tables[i]} t{i} ON t0.[datetime] = t{i}.[datetime] ";

            sql += "WHERE t0.[datetime] BETWEEN @from AND @to ORDER BY t0.[datetime]";

            return await conn.QueryAsync<dynamic>(sql, new { from, to });
        }

        public async Task<IEnumerable<dynamic>> GetMetricTypes()
        {
            using var conn = new SqlConnection(_connectionString);
            // Open connection explicitly to catch connection issues early
            await conn.OpenAsync();

            // Get metric types from HealthMetricsCounts table where RecordCount > 0
            var metricTypes = await conn.QueryAsync<string>(
                @"SELECT DISTINCT MetricType 
                      FROM HealthMetricsCounts 
                      WHERE MetricType IS NOT NULL 
                        AND RecordCount > 0
                      ORDER BY MetricType");
            return metricTypes;
        }

        public async Task<(DateTime MinDate, DateTime MaxDate)?> GetMetricTypeDateRange(string metricType)
        {
            using var conn = new SqlConnection(_connectionString);
            // Set connection timeout to prevent hanging
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    MIN(NormalizedTimestamp) AS MinDate,
                    MAX(NormalizedTimestamp) AS MaxDate
                FROM HealthMetrics
                WHERE MetricType = @MetricType
                    AND NormalizedTimestamp IS NOT NULL";

            var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql, new
            {
                MetricType = metricType
            });

            if (result != null && result.MinDate.HasValue && result.MaxDate.HasValue)
            {
                return (result.MinDate.Value, result.MaxDate.Value);
            }

            return null;
        }

        /// <summary>
        /// Retrieves health metrics data from the database filtered by MetricType and date range
        /// </summary>
        public async Task<IEnumerable<HealthMetricData>> GetHealthMetricsData(string metricType, DateTime from, DateTime to)
        {
            using var conn = new SqlConnection(_connectionString);
            // Set connection timeout to prevent hanging
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    NormalizedTimestamp,
                    Value,
                    Unit,
                    Provider
                FROM HealthMetrics
                WHERE MetricType = @MetricType
                    AND NormalizedTimestamp >= @FromDate
                    AND NormalizedTimestamp <= @ToDate
                    AND Value IS NOT NULL
                ORDER BY NormalizedTimestamp";

            var results = await conn.QueryAsync<HealthMetricData>(sql, new
            {
                MetricType = metricType,
                FromDate = from,
                ToDate = to
            });

            return results;
        }

        /// <summary>
        /// Helper method to parse MetricType and extract base type (first part before any non-alphanumeric character)
        /// </summary>
        private static string GetBaseType(string metricType)
        {
            if (string.IsNullOrWhiteSpace(metricType))
                return string.Empty;

            var parts = Regex.Split(metricType, @"[^a-zA-Z0-9]+")
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .ToList();

            return parts.Count > 0 ? parts[0] : metricType;
        }

        /// <summary>
        /// Gets distinct base metric types from the specified table
        /// </summary>
        /// <param name="tableName">The table name to query (e.g., HealthMetrics, HealthMetricsWeek, etc.)</param>
        public async Task<IEnumerable<string>> GetBaseMetricTypes(string tableName = "HealthMetrics")
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // For resolution tables (Hour, Day, Week, Month, Year), query directly from the table
            // For HealthMetrics, use HealthMetricsCounts for efficiency
            if (tableName == "HealthMetrics")
            {
                var metricTypes = await conn.QueryAsync<string>(
                    @"SELECT DISTINCT MetricType 
                      FROM HealthMetricsCounts 
                      WHERE MetricType IS NOT NULL 
                        AND RecordCount > 0
                      ORDER BY MetricType");
                return metricTypes;
            }
            else
            {
                // Query directly from the resolution table
                var sql = $@"
                    SELECT DISTINCT MetricType 
                    FROM [dbo].[{tableName}]
                    WHERE MetricType IS NOT NULL
                    ORDER BY MetricType";

                var metricTypes = await conn.QueryAsync<string>(sql);
                return metricTypes;
            }
        }

        /// <summary>
        /// Gets distinct subtypes for a given base metric type from the specified table
        /// </summary>
        /// <param name="baseType">The base metric type</param>
        /// <param name="tableName">The table name to query (e.g., HealthMetrics, HealthMetricsWeek, etc.)</param>
        public async Task<IEnumerable<string>> GetSubtypesForBaseType(string baseType, string tableName = "HealthMetrics")
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // For resolution tables (Hour, Day, Week, Month, Year), query directly from the table
            // For HealthMetrics, use HealthMetricsCounts for efficiency
            if (tableName == "HealthMetrics")
            {
                var subtypes = await conn.QueryAsync<string>(
                    @"SELECT DISTINCT MetricSubtype 
                      FROM HealthMetricsCounts 
                      WHERE MetricType = @BaseType
                        AND MetricSubtype IS NOT NULL
                        AND MetricSubtype != ''
                        AND RecordCount > 0
                      ORDER BY MetricSubtype",
                    new { BaseType = baseType });
                return subtypes;
            }
            else
            {
                // Query directly from the resolution table
                var sql = $@"
                    SELECT DISTINCT MetricSubtype 
                    FROM [dbo].[{tableName}]
                    WHERE MetricType = @BaseType
                        AND MetricSubtype IS NOT NULL
                        AND MetricSubtype != ''
                    ORDER BY MetricSubtype";

                var subtypes = await conn.QueryAsync<string>(sql, new { BaseType = baseType });
                return subtypes;
            }
        }

        /// <summary>
        /// Gets all metric types grouped by base type and subtype from HealthMetricsCounts table where RecordCount > 0
        /// Returns a dictionary where key is base type and value is list of subtypes
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetMetricTypesByBaseType()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Get MetricType and MetricSubtype pairs from HealthMetricsCounts
            // Only include records with RecordCount > 0
            var results = await conn.QueryAsync<(string MetricType, string MetricSubtype)>(
                @"SELECT MetricType, MetricSubtype
                      FROM HealthMetricsCounts 
                      WHERE MetricType IS NOT NULL 
                        AND RecordCount > 0");

            var grouped = results
                .GroupBy(r => r.MetricType) // MetricType is already the base type after normalization
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.MetricSubtype)
                          .Where(st => !string.IsNullOrEmpty(st))
                          .Distinct()
                          .OrderBy(st => st)
                          .ToList()
                );

            return grouped;
        }

        /// <summary>
        /// Retrieves health metrics data filtered by base type and optional subtype from the specified table
        /// </summary>
        /// <param name="baseType">The base metric type</param>
        /// <param name="subtype">Optional subtype filter</param>
        /// <param name="from">Optional start date</param>
        /// <param name="to">Optional end date</param>
        /// <param name="tableName">The table name to query (e.g., HealthMetrics, HealthMetricsWeek, etc.)</param>
        public async Task<IEnumerable<HealthMetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype = null, DateTime? from = null, DateTime? to = null, string tableName = "HealthMetrics")
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(baseType))
                return Enumerable.Empty<HealthMetricData>();

            tableName = SqlQueryBuilder.NormalizeTableName(tableName);

            // Validate date range if both provided
            if (from.HasValue && to.HasValue && from.Value > to.Value)
                return Enumerable.Empty<HealthMetricData>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var providerColumn = SqlQueryBuilder.GetProviderColumn(tableName);

            var sql = new StringBuilder($@"
                SELECT 
                    NormalizedTimestamp,
                    Value,
                    Unit,
                    {providerColumn}
                FROM [dbo].[{tableName}]
                WHERE 1=1");

            var parameters = new DynamicParameters();
            SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
            SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
            SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);

            sql.Append(" AND Value IS NOT NULL");
            sql.Append(" ORDER BY NormalizedTimestamp");

            var results = await conn.QueryAsync<HealthMetricData>(sql.ToString(), parameters);

            return results;
        }

        /// <summary>
        /// Gets date range for a base metric type and optional subtype from the specified table
        /// </summary>
        /// <param name="baseType">The base metric type</param>
        /// <param name="subtype">Optional subtype filter</param>
        /// <param name="tableName">The table name to query (e.g., HealthMetrics, HealthMetricsWeek, etc.)</param>
        public async Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype = null, string tableName = "HealthMetrics")
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(baseType))
                return null;

            tableName = SqlQueryBuilder.NormalizeTableName(tableName);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = new StringBuilder($@"
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
            {
                return (result.MinDate.Value, result.MaxDate.Value);
            }

            return null;
        }

        /// <summary>
        /// Gets the record count for a specific MetricType and optional MetricSubtype from the summary table
        /// Returns 0 if the combination doesn't exist
        /// </summary>
        public async Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT RecordCount
                FROM HealthMetricsCounts
                WHERE MetricType = @MetricType
                  AND MetricSubtype = @MetricSubtype";

            var result = await conn.QuerySingleOrDefaultAsync<long?>(sql, new
            {
                MetricType = metricType,
                MetricSubtype = metricSubtype ?? string.Empty
            });

            return result ?? 0;
        }

        /// <summary>
        /// Gets all MetricType/Subtype combinations with their record counts from the summary table
        /// Returns a dictionary where key is (MetricType, MetricSubtype) and value is the count
        /// Note: Empty strings in MetricSubtype are converted to null for consistency
        /// </summary>
        public async Task<Dictionary<(string MetricType, string? MetricSubtype), long>> GetAllRecordCounts()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var results = await conn.QueryAsync<(string MetricType, string MetricSubtype, long RecordCount)>(
                @"SELECT MetricType, MetricSubtype, RecordCount
                  FROM HealthMetricsCounts
                  ORDER BY MetricType, MetricSubtype");

            return results.ToDictionary(
                r => (r.MetricType, string.IsNullOrEmpty(r.MetricSubtype) ? null : r.MetricSubtype),
                r => r.RecordCount
            );
        }

        /// <summary>
        /// Gets record counts grouped by MetricType (summing all subtypes)
        /// Returns a dictionary where key is MetricType and value is the total count
        /// </summary>
        public async Task<Dictionary<string, long>> GetRecordCountsByMetricType()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var results = await conn.QueryAsync<(string MetricType, long TotalCount)>(
                @"SELECT MetricType, SUM(RecordCount) AS TotalCount
                  FROM HealthMetricsCounts
                  GROUP BY MetricType
                  ORDER BY MetricType");

            return results.ToDictionary(
                r => r.MetricType,
                r => r.TotalCount
            );
        }

        /// <summary>
        /// Gets record counts for a specific MetricType, grouped by MetricSubtype
        /// Returns a dictionary where key is MetricSubtype (empty string means no subtype) and value is the count
        /// </summary>
        public async Task<Dictionary<string, long>> GetRecordCountsBySubtype(string metricType)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var results = await conn.QueryAsync<(string MetricSubtype, long RecordCount)>(
                @"SELECT MetricSubtype, RecordCount
                  FROM HealthMetricsCounts
                  WHERE MetricType = @MetricType
                  ORDER BY MetricSubtype",
                new { MetricType = metricType });

            return results.ToDictionary(
                r => r.MetricSubtype,
                r => r.RecordCount
            );
        }
    }
}

