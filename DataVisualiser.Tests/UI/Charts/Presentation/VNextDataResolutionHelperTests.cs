using DataFileReader.Canonical;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.Charts.Presentation;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Contracts;

namespace DataVisualiser.Tests.UI.Charts.Presentation;

public sealed class VNextDataResolutionHelperTests
{
    [Fact]
    public async Task ResolveSeriesDataAsync_WithPrimarySelection_ReturnsPrimaryData()
    {
        var primaryData = CreateData(1m);
        var request = CreateRequest(
            selectedSeries: new MetricSeriesSelection("Weight", "fat"),
            primaryData: primaryData,
            primaryMetricType: "Weight",
            primarySubtype: "fat");

        var result = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request);

        Assert.Same(primaryData, result.Data);
        Assert.Null(result.Cms);
    }

    [Fact]
    public async Task ResolveSeriesDataAsync_WithSecondarySelection_ReturnsSecondaryData()
    {
        var primaryData = CreateData(1m);
        var secondaryData = CreateData(2m);
        var request = CreateRequest(
            selectedSeries: new MetricSeriesSelection("Steps", "morning"),
            primaryData: primaryData,
            secondaryData: secondaryData,
            secondaryMetricType: "Steps",
            secondarySubtype: "morning");

        var result = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request);

        Assert.Same(secondaryData, result.Data);
    }

    [Fact]
    public async Task ResolveSeriesDataAsync_WithCachedSelection_ReturnsCachedDataWithoutFallback()
    {
        var cache = new MetricSeriesSelectionCache();
        var selection = new MetricSeriesSelection("Sleep", "deep");
        var cachedData = CreateData(3m);
        cache.SetDataWithCms(MetricSeriesSelectionCache.BuildCacheKey(selection, From, To, TableName), cachedData, null);
        var legacyCalled = false;
        var request = CreateRequest(
            selectedSeries: selection,
            cache: cache,
            legacyLoad: (_, _, _, _) =>
            {
                legacyCalled = true;
                return Task.FromResult<(IReadOnlyList<MetricData>, ICanonicalMetricSeries?)>((CreateData(4m), null));
            });

        var result = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request);

        Assert.Same(cachedData, result.Data);
        Assert.False(legacyCalled);
    }

    [Fact]
    public async Task ResolveSeriesDataAsync_WithVNextFailure_UsesLegacyFallbackAndRecordsRuntime()
    {
        LoadRuntimeState? runtime = null;
        var legacyData = CreateData(5m);
        var selection = new MetricSeriesSelection("Sleep", "deep");
        var request = CreateRequest(
            selectedSeries: selection,
            setRuntime: state => runtime = state,
            legacyLoad: (series, from, to, tableName) =>
            {
                Assert.Same(selection, series);
                Assert.Equal(From, from);
                Assert.Equal(To, to);
                Assert.Equal(TableName, tableName);
                return Task.FromResult<(IReadOnlyList<MetricData>, ICanonicalMetricSeries?)>((legacyData, null));
            });

        var result = await VNextDataResolutionHelper.ResolveSeriesDataAsync(request);

        Assert.Equal(legacyData, result.Data);
        Assert.NotNull(runtime);
        Assert.Equal(EvidenceRuntimePath.Legacy, runtime.RuntimePath);
        Assert.Contains("coordinator unavailable", runtime.FailureReason);
    }

    private const string TableName = "HealthData";
    private static readonly DateTime From = new(2026, 1, 1);
    private static readonly DateTime To = new(2026, 1, 7);

    private static SeriesResolutionRequest CreateRequest(
        MetricSeriesSelection? selectedSeries,
        IReadOnlyList<MetricData>? primaryData = null,
        IReadOnlyList<MetricData>? secondaryData = null,
        MetricSeriesSelectionCache? cache = null,
        string? metricType = null,
        string? primaryMetricType = null,
        string? secondaryMetricType = null,
        string? primarySubtype = null,
        string? secondarySubtype = null,
        Action<LoadRuntimeState>? setRuntime = null,
        Func<MetricSeriesSelection, DateTime, DateTime, string, Task<(IReadOnlyList<MetricData> Data, ICanonicalMetricSeries? Cms)>>? legacyLoad = null)
    {
        return new SeriesResolutionRequest(
            selectedSeries,
            cache ?? new MetricSeriesSelectionCache(),
            TableName,
            new VNextSeriesLoadCoordinator(() => throw new InvalidOperationException("coordinator unavailable")),
            ChartProgramKind.Distribution,
            EvidenceRuntimePath.VNextDistribution,
            setRuntime ?? (_ => { }),
            legacyLoad ?? ((_, _, _, _) => Task.FromResult<(IReadOnlyList<MetricData>, ICanonicalMetricSeries?)>((CreateData(9m), null))),
            From,
            To,
            primaryData ?? CreateData(1m),
            secondaryData,
            null,
            null,
            metricType,
            primaryMetricType,
            secondaryMetricType,
            primarySubtype,
            secondarySubtype);
    }

    private static IReadOnlyList<MetricData> CreateData(decimal value)
    {
        return
        [
            new MetricData { NormalizedTimestamp = From, Value = value }
        ];
    }
}
