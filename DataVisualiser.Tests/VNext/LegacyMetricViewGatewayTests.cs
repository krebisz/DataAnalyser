using DataVisualiser.Shared.Models;
using DataVisualiser.VNext.Application;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.VNext;

public sealed class LegacyMetricViewGatewayTests
{
    [Fact]
    public async Task LoadAsync_ShouldMaterializeAllRequestedSeries()
    {
        var gateway = new LegacyMetricViewGateway(new StubMetricSeriesLoader());
        var request = new MetricSelectionRequest(
            "Weight",
            [new MetricSeriesRequest("Weight", "morning"), new MetricSeriesRequest("Weight", "evening")],
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 2),
            "HealthMetrics");

        var snapshot = await gateway.LoadAsync(request);

        Assert.Equal(request.Signature, snapshot.Signature);
        Assert.Equal(2, snapshot.SeriesCount);
        Assert.All(snapshot.Series, series => Assert.Single(series.RawData));
    }

    private sealed class StubMetricSeriesLoader : IMetricSeriesLoader
    {
        public Task<LoadedMetricSeries> LoadAsync(
            MetricSeriesRequest request,
            DateTime from,
            DateTime to,
            string resolutionTableName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new LoadedMetricSeries(
                [new MetricData { NormalizedTimestamp = from, Value = request.QuerySubtype == "evening" ? 2m : 1m }],
                null));
        }
    }
}
