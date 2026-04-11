using DataFileReader.Canonical;
using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed record LoadedMetricSeries(
    IReadOnlyList<MetricData> RawData,
    ICanonicalMetricSeries? CanonicalSeries);

public interface IMetricSeriesLoader
{
    Task<LoadedMetricSeries> LoadAsync(
        MetricSeriesRequest request,
        DateTime from,
        DateTime to,
        string resolutionTableName,
        CancellationToken cancellationToken = default);
}
