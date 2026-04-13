using DataVisualiser.Core.Data;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Shared.Models;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class EvidenceMultiMetricParityEvaluatorTests
{
    [Fact]
    public async Task BuildAsync_WithFewerThanThreeSelectedSeries_ReturnsUnavailable()
    {
        var evaluator = CreateEvaluator();
        var metricState = new MetricState();
        metricState.SetSeriesSelections(
        [
            new MetricSeriesSelection("Weight", "body_fat_mass"),
            new MetricSeriesSelection("Weight", "fat_free_mass")
        ]);
        var context = new ChartDataContext
        {
            Data1 = new List<MetricData> { new() { NormalizedTimestamp = new DateTime(2026, 4, 1), Value = 1m } },
            From = new DateTime(2026, 4, 1),
            To = new DateTime(2026, 4, 2)
        };

        var result = await evaluator.BuildAsync(metricState, context);

        Assert.Equal("Unavailable", result.Status);
        Assert.Equal("At least three series required", result.Reason);
    }

    private static EvidenceMultiMetricParityEvaluator CreateEvaluator()
    {
        var service = new MetricSelectionService(new FakeMetricSelectionDataQueries(), "Server=(localdb)\\MSSQLLocalDB;Database=Fake;Trusted_Connection=True;");
        return new EvidenceMultiMetricParityEvaluator(service, () => null);
    }

    private sealed class FakeMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null) => Task.FromResult(0L);
        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null) => Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());
        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName) => Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName) => Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
    }
}
