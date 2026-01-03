using System.Text;
using Dapper;

namespace DataVisualiser.Data;

/// <summary>
///     Helper class for building parameterized SQL queries with common patterns.
/// </summary>
public static class SqlQueryBuilder
{
    /// <summary>
    ///     Builds a base WHERE clause filter for MetricType.
    /// </summary>
    public static void AddMetricTypeFilter(StringBuilder sql, DynamicParameters parameters, string baseType)
    {
        sql.Append(" AND MetricType = @BaseType");
        parameters.Add("BaseType", baseType);
    }

    /// <summary>
    ///     Conditionally adds a subtype filter to the query.
    /// </summary>
    public static void AddSubtypeFilter(StringBuilder sql, DynamicParameters parameters, string? subtype)
    {
        if (!string.IsNullOrEmpty(subtype))
        {
            sql.Append(" AND MetricSubtype = @Subtype");
            parameters.Add("Subtype", subtype);
        }
    }

    /// <summary>
    ///     Conditionally adds date range filters to the query.
    /// </summary>
    public static void AddDateRangeFilters(StringBuilder sql, DynamicParameters parameters, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            sql.Append(" AND NormalizedTimestamp >= @FromDate");
            parameters.Add("FromDate", from.Value);
        }

        if (to.HasValue)
        {
            sql.Append(" AND NormalizedTimestamp <= @ToDate");
            parameters.Add("ToDate", to.Value);
        }
    }

    /// <summary>
    ///     Gets the provider column expression based on table name.
    /// </summary>
    public static string GetProviderColumn(string tableName)
    {
        return tableName == "HealthMetrics" ? "Provider" : "NULL AS Provider";
    }

    /// <summary>
    ///     Validates and normalizes table name, returning default if invalid.
    /// </summary>
    public static string NormalizeTableName(string? tableName, string defaultValue = "HealthMetrics")
    {
        return string.IsNullOrWhiteSpace(tableName) ? defaultValue : tableName;
    }
}