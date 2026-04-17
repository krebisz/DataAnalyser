using DataVisualiser.Core.Data.Repositories;
using DataVisualiser.Shared.Models;

namespace DataVisualiser.UI.Admin;

internal sealed class DataFetcherAdminMetricsRepository(DataFetcher dataFetcher) : IAdminMetricsRepository
{
    private readonly DataFetcher _dataFetcher = dataFetcher ?? throw new ArgumentNullException(nameof(dataFetcher));

    public Task<IReadOnlyList<string>> GetMetricTypesAsync()
    {
        return _dataFetcher.GetCountsMetricTypesForAdmin();
    }

    public Task<IReadOnlyList<HealthMetricsCountEntry>> GetCountsAsync(string? metricType)
    {
        return _dataFetcher.GetHealthMetricsCountsForAdmin(metricType);
    }

    public Task<int> UpdateCountsAsync(IEnumerable<HealthMetricsCountEntry> updates)
    {
        return _dataFetcher.UpdateHealthMetricsCountsForAdmin(updates);
    }
}
