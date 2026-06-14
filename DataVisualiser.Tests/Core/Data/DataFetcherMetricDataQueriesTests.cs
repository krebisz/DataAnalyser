using System.Reflection;
using System.Text;
using Dapper;
using DataVisualiser.Core.Data.Repositories;

namespace DataVisualiser.Tests.Data;

public sealed class DataFetcherMetricDataQueriesTests
{
    [Fact]
    public void SamplingQuery_UsesTimeBucketAggregation_InsteadOfWindowedRowNumber()
    {
        var sql = new StringBuilder();
        var parameters = new DynamicParameters();

        InvokeBuildSamplingQuery(
            sql,
            parameters,
            tableName: "HealthMetrics",
            providerColumn: "Provider",
            targetSamples: 1500,
            baseType: "Weight",
            subtype: "Morning",
            from: new DateTime(2022, 01, 01),
            to: new DateTime(2024, 01, 01));

        var text = sql.ToString();
        Assert.Contains("BucketedData", text);
        Assert.Contains("DATEDIFF_BIG", text);
        Assert.Contains("GROUP BY BucketIndex", text);
        Assert.Contains("AVG(CAST(Value AS decimal", text);
        Assert.DoesNotContain("ROW_NUMBER()", text);
        Assert.DoesNotContain("COUNT(*) OVER", text);
        Assert.Contains("BucketSeconds", parameters.ParameterNames);
        Assert.Contains("SamplingFromDate", parameters.ParameterNames);
    }

    [Fact]
    public void SamplingQuery_ResolvesMetricTypeDisplayNames()
    {
        var sql = new StringBuilder();
        var parameters = new DynamicParameters();

        InvokeBuildSamplingQuery(
            sql,
            parameters,
            tableName: "HealthMetrics",
            providerColumn: "hm.Provider AS Provider",
            targetSamples: 1500,
            baseType: "Heart Rate",
            subtype: "Maximum Heart Rate",
            from: new DateTime(2022, 01, 01),
            to: new DateTime(2024, 01, 01));

        var text = sql.ToString();
        Assert.Contains("metricTypeName.MetricType = hm.MetricType", text);
        Assert.Contains("MetricTypeName", text);
        Assert.Contains("COALESCE(NULLIF(metricTypeName.MetricTypeName, ''), metricTypeName.MetricType) = @BaseType", text);
        Assert.Contains("subtypeName.MetricType = hm.MetricType", text);
        Assert.Contains("subtypeName.MetricSubtype = hm.MetricSubtype", text);
        Assert.Contains("COALESCE(NULLIF(subtypeName.MetricSubtypeName, ''), subtypeName.MetricSubtype) = @Subtype", text);
        Assert.Contains("BaseType", parameters.ParameterNames);
        Assert.Contains("Subtype", parameters.ParameterNames);
    }

    [Fact]
    public void SamplingQuery_RequiresBoundedDateRange()
    {
        var sql = new StringBuilder();
        var parameters = new DynamicParameters();

        var exception = Assert.Throws<TargetInvocationException>(() => InvokeBuildSamplingQuery(
            sql,
            parameters,
            tableName: "HealthMetrics",
            providerColumn: "Provider",
            targetSamples: 1500,
            baseType: "Weight",
            subtype: "Morning",
            from: null,
            to: new DateTime(2024, 01, 01)));

        Assert.IsType<ArgumentException>(exception.InnerException);
    }

    private static void InvokeBuildSamplingQuery(
        StringBuilder sql,
        DynamicParameters parameters,
        string tableName,
        string providerColumn,
        int targetSamples,
        string baseType,
        string? subtype,
        DateTime? from,
        DateTime? to)
    {
        var method = typeof(DataFetcherMetricDataQueries).GetMethod("BuildSamplingQuery", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        method!.Invoke(null, new object?[]
        {
            sql,
            parameters,
            tableName,
            providerColumn,
            targetSamples,
            baseType,
            subtype,
            from,
            to
        });
    }
}
