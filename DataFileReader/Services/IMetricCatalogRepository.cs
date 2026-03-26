namespace DataFileReader.Services;

public interface IMetricCatalogRepository
{
    List<string> GetAllMetricTypes();
    List<string?> GetSubtypesForMetricType(string metricType);
    (DateTime MinDate, DateTime MaxDate)? GetDateRangeForMetric(string metricType, string? metricSubtype = null);
}
