using DataFileReader.Helper;

namespace DataFileReader.Services;

public sealed class SqlHealthMetricWriter : IHealthMetricWriter
{
    public void InsertHealthMetrics(IReadOnlyList<HealthMetric> metrics)
    {
        if (metrics == null || metrics.Count == 0)
            return;

        SQLHelper.InsertHealthMetrics(metrics.ToList());
    }
}
