using DataFileReader.Helper;

namespace DataFileReader.Services;

public sealed class SqlMetricAggregationWriter : IMetricAggregationWriter
{
    public void InsertDay(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
    {
        SQLHelper.InsertHealthMetricsDay(metricType, metricSubtype, fromDate, toDate);
    }

    public void InsertWeek(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
    {
        SQLHelper.InsertHealthMetricsWeek(metricType, metricSubtype, fromDate, toDate);
    }

    public void InsertMonth(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate)
    {
        SQLHelper.InsertHealthMetricsMonth(metricType, metricSubtype, fromDate, toDate);
    }
}
