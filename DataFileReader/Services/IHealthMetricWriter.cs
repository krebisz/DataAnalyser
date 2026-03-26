using DataFileReader.Helper;

namespace DataFileReader.Services;

public interface IHealthMetricWriter
{
    void InsertHealthMetrics(IReadOnlyList<HealthMetric> metrics);
}
