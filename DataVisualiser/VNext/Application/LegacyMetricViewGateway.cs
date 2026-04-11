using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.VNext.Application;

public sealed class LegacyMetricViewGateway
{
    private readonly IMetricSeriesLoader _loader;

    public LegacyMetricViewGateway(IMetricSeriesLoader loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public async Task<MetricLoadSnapshot> LoadAsync(
        MetricSelectionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tasks = request.Series
            .Select(async series =>
            {
                var loaded = await _loader.LoadAsync(series, request.From, request.To, request.ResolutionTableName, cancellationToken);
                return new MetricSeriesSnapshot(series, loaded.RawData, loaded.CanonicalSeries);
            })
            .ToArray();

        var snapshots = await Task.WhenAll(tasks);
        return new MetricLoadSnapshot(request, snapshots, DateTime.UtcNow);
    }
}
