using System.Text;
using Dapper;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

internal sealed class DataFetcherDateRangeQueries : DataFetcherQueryGroup
{
    public DataFetcherDateRangeQueries(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetMetricTypeDateRange(string metricType)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetMetricTypeDateRange
                SELECT 
                    MIN(NormalizedTimestamp) AS MinDate,
                    MAX(NormalizedTimestamp) AS MaxDate
                FROM {DataAccessDefaults.DefaultTableName}
                WHERE MetricType = @MetricType
                    AND NormalizedTimestamp IS NOT NULL";

        var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql, new { MetricType = metricType });
        return result != null && result.MinDate.HasValue && result.MaxDate.HasValue
            ? (result.MinDate.Value, result.MaxDate.Value)
            : null;
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype = null, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

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
        return result != null && result.MinDate.HasValue && result.MaxDate.HasValue
            ? (result.MinDate.Value, result.MaxDate.Value)
            : null;
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        var subtypeList = subtypes?
                              .Where(subtype => !string.IsNullOrWhiteSpace(subtype) && !string.Equals(subtype, "(All)", StringComparison.OrdinalIgnoreCase))
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList()
                          ?? [];

        using var conn = await OpenConnectionAsync();

        var sql = new StringBuilder($@"
                -- DataFetcher.GetBaseTypeDateRangeFromCounts
                SELECT 
                    MIN(c.EarliestDateTime) AS MinDate,
                    MAX(c.MostRecentDateTime) AS MaxDate
                FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = c.MetricType
                      AND m.MetricSubtype = c.MetricSubtype
                WHERE c.MetricType = @MetricType
                  AND (m.Disabled IS NULL OR m.Disabled = 0)
                  AND c.RecordCount > 0
                  AND c.EarliestDateTime IS NOT NULL
                  AND c.MostRecentDateTime IS NOT NULL");

        var parameters = new DynamicParameters();
        parameters.Add("MetricType", baseType);

        if (subtypeList.Count > 0)
        {
            sql.Append(" AND c.MetricSubtype IN @Subtypes");
            parameters.Add("Subtypes", subtypeList);
        }

        var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql.ToString(), parameters);
        return result != null && result.MinDate.HasValue && result.MaxDate.HasValue
            ? (result.MinDate.Value, result.MaxDate.Value)
            : null;
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName = DataAccessDefaults.DefaultTableName)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        var subtypeList = subtypes?
                              .Where(subtype => !string.IsNullOrWhiteSpace(subtype) && !string.Equals(subtype, "(All)", StringComparison.OrdinalIgnoreCase))
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToList()
                          ?? [];

        using var conn = await OpenConnectionAsync();

        var sql = new StringBuilder($@"
                -- DataFetcher.GetBaseTypeDateRangeForSubtypes
                SELECT 
                    MIN(NormalizedTimestamp) AS MinDate,
                    MAX(NormalizedTimestamp) AS MaxDate
                FROM [dbo].[{tableName}]
                WHERE NormalizedTimestamp IS NOT NULL");

        var parameters = new DynamicParameters();
        SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);

        if (subtypeList.Count > 0)
        {
            sql.Append(" AND MetricSubtype IN @Subtypes");
            parameters.Add("Subtypes", subtypeList);
        }

        var result = await conn.QuerySingleOrDefaultAsync<DateRangeResult>(sql.ToString(), parameters);
        return result != null && result.MinDate.HasValue && result.MaxDate.HasValue
            ? (result.MinDate.Value, result.MaxDate.Value)
            : null;
    }

    public async Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCount
                SELECT SUM(RecordCount)
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                WHERE (@MetricType = '(All)' OR MetricType = @MetricType)
                  AND (@MetricSubtype IS NULL OR @MetricSubtype = '' OR MetricSubtype = @MetricSubtype)";

        var result = await conn.QuerySingleOrDefaultAsync<long?>(sql, new
        {
            MetricType = metricType,
            MetricSubtype = metricSubtype
        });

        return result ?? 0;
    }

    public async Task<Dictionary<(string MetricType, string? MetricSubtype), long>> GetAllRecordCounts()
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetAllRecordCounts
                SELECT MetricType, MetricSubtype, RecordCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                ORDER BY MetricType, MetricSubtype";

        var results = await conn.QueryAsync<(string MetricType, string MetricSubtype, long RecordCount)>(sql);
        return results.ToDictionary(r => (r.MetricType, string.IsNullOrEmpty(r.MetricSubtype) ? null : r.MetricSubtype), r => r.RecordCount);
    }

    public async Task<Dictionary<string, long>> GetRecordCountsByMetricType()
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCountsByMetricType
                SELECT MetricType, SUM(RecordCount) AS TotalCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                GROUP BY MetricType
                ORDER BY MetricType";

        var results = await conn.QueryAsync<(string MetricType, long TotalCount)>(sql);
        return results.ToDictionary(r => r.MetricType, r => r.TotalCount);
    }

    public async Task<Dictionary<string, long>> GetRecordCountsBySubtype(string metricType)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetRecordCountsBySubtype
                SELECT MetricSubtype, RecordCount
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                WHERE MetricType = @MetricType
                ORDER BY MetricSubtype";

        var results = await conn.QueryAsync<(string MetricSubtype, long RecordCount)>(sql, new { MetricType = metricType });
        return results.ToDictionary(r => r.MetricSubtype, r => r.RecordCount);
    }
}
