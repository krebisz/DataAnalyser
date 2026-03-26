using System.Configuration;
using Microsoft.Data.SqlClient;

namespace DataFileReader.Services;

public sealed class SqlMetricCatalogRepository : IMetricCatalogRepository
{
    private readonly string _connectionString;

    public SqlMetricCatalogRepository(string? connectionString = null)
    {
        _connectionString = connectionString
                            ?? ConfigurationManager.AppSettings["HealthDB"]
                            ?? throw new InvalidOperationException("HealthDB connection string is not configured.");
    }

    public List<string> GetAllMetricTypes()
    {
        var metricTypes = new List<string>();

        using var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();

        const string sql = @"
                    SELECT DISTINCT MetricType 
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType IS NOT NULL
                    ORDER BY MetricType";

        using var sqlCommand = new SqlCommand(sql, sqlConnection);
        using var reader = sqlCommand.ExecuteReader();
        while (reader.Read())
        {
            var metricType = reader["MetricType"]?.ToString();
            if (!string.IsNullOrEmpty(metricType))
                metricTypes.Add(metricType);
        }

        return metricTypes;
    }

    public List<string?> GetSubtypesForMetricType(string metricType)
    {
        var subtypes = new List<string?>();

        using var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();

        const string sql = @"
                    SELECT DISTINCT MetricSubtype,
                        CASE WHEN MetricSubtype IS NULL OR MetricSubtype = '' THEN 0 ELSE 1 END AS SortOrder
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType = @MetricType
                    ORDER BY 
                        SortOrder,
                        MetricSubtype";

        using var sqlCommand = new SqlCommand(sql, sqlConnection);
        sqlCommand.Parameters.Add(new SqlParameter("@MetricType", metricType));
        using var reader = sqlCommand.ExecuteReader();

        while (reader.Read())
        {
            var subtype = reader["MetricSubtype"]?.ToString();
            subtypes.Add(string.IsNullOrEmpty(subtype) ? null : subtype);
        }

        return subtypes;
    }

    public (DateTime MinDate, DateTime MaxDate)? GetDateRangeForMetric(string metricType, string? metricSubtype = null)
    {
        using var sqlConnection = new SqlConnection(_connectionString);
        sqlConnection.Open();

        var sql = new System.Text.StringBuilder(@"
                    SELECT 
                        MIN(NormalizedTimestamp) AS MinDate,
                        MAX(NormalizedTimestamp) AS MaxDate
                    FROM [dbo].[HealthMetrics]
                    WHERE MetricType = @MetricType
                        AND NormalizedTimestamp IS NOT NULL");

        var parameters = new List<SqlParameter>
        {
            new("@MetricType", metricType)
        };

        if (!string.IsNullOrEmpty(metricSubtype))
        {
            sql.Append(" AND ISNULL(MetricSubtype, '') = @MetricSubtype");
            parameters.Add(new SqlParameter("@MetricSubtype", metricSubtype));
        }
        else
        {
            sql.Append(" AND (MetricSubtype IS NULL OR MetricSubtype = '')");
        }

        using var sqlCommand = new SqlCommand(sql.ToString(), sqlConnection);
        sqlCommand.Parameters.AddRange(parameters.ToArray());
        using var reader = sqlCommand.ExecuteReader();

        if (!reader.Read())
            return null;

        if (reader["MinDate"] == DBNull.Value || reader["MaxDate"] == DBNull.Value)
            return null;

        return (Convert.ToDateTime(reader["MinDate"]), Convert.ToDateTime(reader["MaxDate"]));
    }
}
