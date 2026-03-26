namespace DataFileReader.Services;

public interface IMetricAggregationWriter
{
    void InsertDay(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate);
    void InsertWeek(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate);
    void InsertMonth(string metricType, string? metricSubtype, DateTime fromDate, DateTime toDate);
}
