using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Admin;

internal interface IAdminMetricsRepository
{
    Task<IReadOnlyList<string>> GetMetricTypesAsync();
    Task<IReadOnlyList<HealthMetricsCountEntry>> GetCountsAsync(string? metricType);
    Task<int> UpdateCountsAsync(IEnumerable<HealthMetricsCountEntry> updates);
}
