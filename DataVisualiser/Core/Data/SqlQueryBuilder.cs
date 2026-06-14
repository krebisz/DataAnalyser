using System.Text;
using Dapper;
using DataVisualiser.Core.Configuration.Defaults;

namespace DataVisualiser.Core.Data;

/// <summary>
///     Helper class for building parameterized SQL queries with common patterns.
/// </summary>
public static class SqlQueryBuilder
{
    /// <summary>
    ///     Builds a base WHERE clause filter for MetricType.
    /// </summary>
    public static void AddMetricTypeFilter(StringBuilder sql, DynamicParameters parameters, string baseType, string metricTypeColumn = "MetricType")
    {
        if (string.Equals(baseType, "(All)", StringComparison.OrdinalIgnoreCase))
            return;

        sql.Append($" AND {metricTypeColumn} = @BaseType");
        parameters.Add("BaseType", baseType);
    }

    public static void AddMetricTypeOrDisplayNameFilter(StringBuilder sql, DynamicParameters parameters, string baseType, string metricTypeColumn)
    {
        if (string.Equals(baseType, "(All)", StringComparison.OrdinalIgnoreCase))
            return;

        sql.Append($@"
            AND (
                {metricTypeColumn} = @BaseType
                OR EXISTS (
                    SELECT 1
                    FROM {DataAccessDefaults.HealthMetricsCanonicalTable} metricTypeName
                    WHERE metricTypeName.MetricType = {metricTypeColumn}
                      AND (metricTypeName.Disabled IS NULL OR metricTypeName.Disabled = 0)
                      AND COALESCE(NULLIF(metricTypeName.MetricTypeName, ''), metricTypeName.MetricType) = @BaseType
                )
            )");
        parameters.Add("BaseType", baseType);
    }

    /// <summary>
    ///     Conditionally adds a subtype filter to the query.
    /// </summary>
    public static void AddSubtypeFilter(StringBuilder sql, DynamicParameters parameters, string? subtype, string subtypeColumn = "MetricSubtype")
    {
        if (!string.IsNullOrEmpty(subtype))
        {
            sql.Append($" AND {subtypeColumn} = @Subtype");
            parameters.Add("Subtype", subtype);
        }
    }

    public static void AddSubtypeOrDisplayNameFilter(StringBuilder sql, DynamicParameters parameters, string? subtype, string metricTypeColumn, string subtypeColumn)
    {
        if (string.IsNullOrEmpty(subtype))
            return;

        sql.Append($@"
            AND (
                {subtypeColumn} = @Subtype
                OR EXISTS (
                    SELECT 1
                    FROM {DataAccessDefaults.HealthMetricsCanonicalTable} subtypeName
                    WHERE subtypeName.MetricType = {metricTypeColumn}
                      AND subtypeName.MetricSubtype = {subtypeColumn}
                      AND (subtypeName.Disabled IS NULL OR subtypeName.Disabled = 0)
                      AND COALESCE(NULLIF(subtypeName.MetricSubtypeName, ''), subtypeName.MetricSubtype) = @Subtype
                )
            )");
        parameters.Add("Subtype", subtype);
    }

    public static void AddSubtypesOrDisplayNameFilter(StringBuilder sql, DynamicParameters parameters, IReadOnlyCollection<string> subtypes, string metricTypeColumn, string subtypeColumn)
    {
        if (subtypes.Count == 0)
            return;

        sql.Append($@"
            AND (
                {subtypeColumn} IN @Subtypes
                OR EXISTS (
                    SELECT 1
                    FROM {DataAccessDefaults.HealthMetricsCanonicalTable} subtypeName
                    WHERE subtypeName.MetricType = {metricTypeColumn}
                      AND subtypeName.MetricSubtype = {subtypeColumn}
                      AND (subtypeName.Disabled IS NULL OR subtypeName.Disabled = 0)
                      AND COALESCE(NULLIF(subtypeName.MetricSubtypeName, ''), subtypeName.MetricSubtype) IN @Subtypes
                )
            )");
        parameters.Add("Subtypes", subtypes);
    }

    /// <summary>
    ///     Conditionally adds date range filters to the query.
    /// </summary>
    public static void AddDateRangeFilters(StringBuilder sql, DynamicParameters parameters, DateTime? from, DateTime? to, string timestampColumn = "NormalizedTimestamp")
    {
        if (from.HasValue)
        {
            sql.Append($" AND {timestampColumn} >= @FromDate");
            parameters.Add("FromDate", from.Value);
        }

        if (to.HasValue)
        {
            sql.Append($" AND {timestampColumn} <= @ToDate");
            parameters.Add("ToDate", to.Value);
        }
    }

    /// <summary>
    ///     Gets the provider column expression based on table name.
    /// </summary>
    public static string GetProviderColumn(string tableName)
    {
        return tableName == DataAccessDefaults.DefaultTableName ? "Provider" : "NULL AS Provider";
    }

    /// <summary>
    ///     Validates and normalizes table name, returning default if invalid.
    /// </summary>
    public static string NormalizeTableName(string? tableName, string defaultValue = DataAccessDefaults.DefaultTableName)
    {
        return string.IsNullOrWhiteSpace(tableName) ? defaultValue : tableName;
    }
}
