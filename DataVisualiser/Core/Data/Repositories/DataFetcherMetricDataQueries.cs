using System.Text;
using Dapper;
using DataVisualiser.Core.Configuration.Defaults;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.Core.Data.Repositories;

internal sealed class DataFetcherMetricDataQueries : DataFetcherQueryGroup
{
    public DataFetcherMetricDataQueries(string connectionString)
        : base(connectionString)
    {
    }

    public async Task<IEnumerable<dynamic>> GetCombinedData(string[] tables, DateTime from, DateTime to)
    {
        if (tables == null || tables.Length == 0)
            throw new ArgumentException("At least one table must be provided.", nameof(tables));

        if (from > to)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));

        using var conn = await OpenConnectionAsync();

        var baseTable = tables[0];
        var sql = $"-- DataFetcher.GetCombinedData{Environment.NewLine}SELECT t0.[datetime]";

        for (var i = 0; i < tables.Length; i++)
            sql += $", t{i}.Value AS {tables[i]}";

        sql += $" FROM {baseTable} t0 ";

        for (var i = 1; i < tables.Length; i++)
            sql += $"LEFT JOIN {tables[i]} t{i} ON t0.[datetime] = t{i}.[datetime] ";

        sql += "WHERE t0.[datetime] BETWEEN @from AND @to ORDER BY t0.[datetime]";

        return await conn.QueryAsync<dynamic>(sql, new { from, to });
    }

    public async Task<IEnumerable<MetricData>> GetHealthMetricsData(string metricType, DateTime from, DateTime to)
    {
        if (string.IsNullOrWhiteSpace(metricType))
            throw new ArgumentException("Metric type cannot be null or empty.", nameof(metricType));

        if (from > to)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));

        using var conn = await OpenConnectionAsync();

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

        var results = await conn.QueryAsync<MetricData>(sql, new
        {
            MetricType = metricType,
            FromDate = from,
            ToDate = to
        });

        return NormalizeMetricTimestamps(results);
    }

    public async Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype = null, DateTime? from = null, DateTime? to = null, string tableName = DataAccessDefaults.DefaultTableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
    {
        ValidateArguments(baseType, from, to);

        tableName = SqlQueryBuilder.NormalizeTableName(tableName);

        using var conn = await OpenConnectionAsync();

        var providerColumn = SqlQueryBuilder.GetProviderColumn(tableName);
        var sql = new StringBuilder();
        var parameters = new DynamicParameters();
        var mode = ResolveQueryMode(samplingMode, targetSamples, maxRecords);

        switch (mode)
        {
            case QueryMode.Sampled:
                BuildSamplingQuery(sql, parameters, tableName, providerColumn, targetSamples!.Value, baseType, subtype, from, to);
                break;
            case QueryMode.Limited:
                BuildLimitedQuery(sql, tableName, providerColumn, maxRecords!.Value);
                ApplyCommonFilters(sql, parameters, baseType, subtype, from, to);
                break;
            default:
                BuildUnboundedQuery(sql, tableName, providerColumn);
                ApplyCommonFilters(sql, parameters, baseType, subtype, from, to);
                break;
        }

        var results = await conn.QueryAsync<MetricData>(sql.ToString(), parameters);
        return NormalizeMetricTimestamps(results);
    }

    private static IEnumerable<MetricData> NormalizeMetricTimestamps(IEnumerable<MetricData> source)
    {
        return source.Select(data => new MetricData
        {
            NormalizedTimestamp = NormalizeToLocal(data.NormalizedTimestamp),
            Value = data.Value,
            Unit = data.Unit,
            Provider = data.Provider
        });
    }

    private static DateTime NormalizeToLocal(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp.ToLocalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(timestamp, DateTimeKind.Local),
            _ => timestamp
        };
    }

    private static QueryMode ResolveQueryMode(SamplingMode samplingMode, int? targetSamples, int? maxRecords)
    {
        if (samplingMode == SamplingMode.UniformOverTime && targetSamples.HasValue && targetSamples.Value > 0)
            return QueryMode.Sampled;

        if (maxRecords.HasValue && maxRecords.Value > 0)
            return QueryMode.Limited;

        return QueryMode.Unbounded;
    }

    private static void BuildSamplingQuery(StringBuilder sql, DynamicParameters parameters, string tableName, string providerColumn, int targetSamples, string baseType, string? subtype, DateTime? from, DateTime? to)
    {
        if (!from.HasValue || !to.HasValue)
            throw new ArgumentException("Sampling requires both from and to dates.");

        var bucketSeconds = CalculateSamplingBucketSeconds(from.Value, to.Value, targetSamples);

        sql.Append($@"
        WITH BucketedData AS (
            SELECT
                DATEDIFF_BIG(second, @SamplingFromDate, NormalizedTimestamp) / @BucketSeconds AS BucketIndex,
                NormalizedTimestamp,
                Value,
                Unit,
                {providerColumn}
            FROM [dbo].[{tableName}]
            WHERE 1=1");

        SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
        SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
        SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);
        sql.Append(" AND Value IS NOT NULL");

        sql.Append($@"
        )
        SELECT
            MIN(NormalizedTimestamp) AS NormalizedTimestamp,
            CAST(AVG(CAST(Value AS decimal(38,10))) AS decimal(18,6)) AS Value,
            MAX(Unit) AS Unit,
            MAX(Provider) AS Provider
        FROM BucketedData
        GROUP BY BucketIndex
        ORDER BY MIN(NormalizedTimestamp)");

        parameters.Add("@SamplingFromDate", from.Value);
        parameters.Add("@BucketSeconds", bucketSeconds);
    }

    private static long CalculateSamplingBucketSeconds(DateTime from, DateTime to, int targetSamples)
    {
        if (targetSamples <= 0)
            return 1;

        var totalSeconds = Math.Max(1.0, (to - from).TotalSeconds);
        return Math.Max(1L, (long)Math.Ceiling(totalSeconds / targetSamples));
    }

    private static void BuildLimitedQuery(StringBuilder sql, string tableName, string providerColumn, int maxRecords)
    {
        sql.Append($@"
        SELECT TOP {maxRecords}
            NormalizedTimestamp,
            Value,
            Unit,
            {providerColumn}
        FROM [dbo].[{tableName}]
        WHERE 1=1");
    }

    private static void BuildUnboundedQuery(StringBuilder sql, string tableName, string providerColumn)
    {
        sql.Append($@"
        SELECT
            NormalizedTimestamp,
            Value,
            Unit,
            {providerColumn}
        FROM [dbo].[{tableName}]
        WHERE 1=1");
    }

    private static void ApplyCommonFilters(StringBuilder sql, DynamicParameters parameters, string baseType, string? subtype, DateTime? from, DateTime? to)
    {
        SqlQueryBuilder.AddMetricTypeFilter(sql, parameters, baseType);
        SqlQueryBuilder.AddSubtypeFilter(sql, parameters, subtype);
        SqlQueryBuilder.AddDateRangeFilters(sql, parameters, from, to);
        sql.Append(" AND Value IS NOT NULL ORDER BY NormalizedTimestamp");
    }

    private static void ValidateArguments(string baseType, DateTime? from, DateTime? to)
    {
        if (string.IsNullOrWhiteSpace(baseType))
            throw new ArgumentException("Base metric type cannot be null or empty.", nameof(baseType));

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("'from' date must be earlier than or equal to 'to' date.", nameof(from));
    }

    private enum QueryMode
    {
        Sampled,
        Limited,
        Unbounded
    }
}
