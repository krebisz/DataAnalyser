using Dapper;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

internal sealed class DataFetcherAdminQueries : DataFetcherQueryGroup
{
    public DataFetcherAdminQueries(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IReadOnlyList<string>> GetCountsMetricTypesForAdmin()
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetCountsMetricTypesForAdmin
                SELECT DISTINCT MetricType
                FROM {DataAccessDefaults.HealthMetricsCountsTable}
                WHERE MetricType IS NOT NULL AND MetricType != ''
                ORDER BY MetricType";

        var rows = await conn.QueryAsync<string>(sql);
        return rows.ToList();
    }

    public async Task<IReadOnlyList<HealthMetricsCountEntry>> GetHealthMetricsCountsForAdmin(string? metricType = null)
    {
        using var conn = await OpenConnectionAsync();

        var sql = $@"
                -- DataFetcher.GetHealthMetricsCountsForAdmin
                SELECT
                    c.MetricType,
                    c.MetricSubtype,
                    ISNULL(m.MetricTypeName, c.MetricType) AS MetricTypeName,
                    ISNULL(m.MetricSubtypeName, c.MetricSubtype) AS MetricSubtypeName,
                    CAST(ISNULL(m.Disabled, 0) AS bit) AS Disabled,
                    c.RecordCount,
                    c.EarliestDateTime,
                    c.MostRecentDateTime
                FROM {DataAccessDefaults.HealthMetricsCountsTable} c
                LEFT JOIN {DataAccessDefaults.HealthMetricsCanonicalTable} m
                       ON m.MetricType = c.MetricType
                      AND m.MetricSubtype = c.MetricSubtype
                WHERE (@MetricType IS NULL OR c.MetricType = @MetricType)
                ORDER BY c.MetricType, c.MetricSubtype";

        var rows = await conn.QueryAsync<HealthMetricsCountEntry>(sql, new { MetricType = metricType });
        return rows.ToList();
    }

    public async Task<int> UpdateHealthMetricsCountsForAdmin(IEnumerable<HealthMetricsCountEntry> updates)
    {
        if (updates is null)
            throw new ArgumentNullException(nameof(updates));

        var updateList = updates.ToList();
        if (updateList.Count == 0)
            return 0;

        using var conn = await OpenConnectionAsync();
        using var tx = conn.BeginTransaction();

        try
        {
            var sql = $@"
                -- DataFetcher.UpdateHealthMetricsCountsForAdmin
                MERGE {DataAccessDefaults.HealthMetricsCanonicalTable} AS target
                USING (VALUES (@MetricType, @MetricSubtype, @MetricTypeName, @MetricSubtypeName, @Disabled))
                      AS source (MetricType, MetricSubtype, MetricTypeName, MetricSubtypeName, Disabled)
                ON target.MetricType = source.MetricType
                   AND target.MetricSubtype = source.MetricSubtype
                WHEN MATCHED THEN
                    UPDATE SET
                        MetricTypeName = source.MetricTypeName,
                        MetricSubtypeName = source.MetricSubtypeName,
                        Disabled = source.Disabled,
                        LastUpdated = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (MetricType, MetricSubtype, MetricTypeName, MetricSubtypeName, Disabled, CreatedDate, LastUpdated)
                    VALUES (source.MetricType, source.MetricSubtype, source.MetricTypeName, source.MetricSubtypeName, source.Disabled, GETDATE(), GETDATE());";

            var affected = await conn.ExecuteAsync(sql, updateList, tx);
            tx.Commit();
            return affected;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
