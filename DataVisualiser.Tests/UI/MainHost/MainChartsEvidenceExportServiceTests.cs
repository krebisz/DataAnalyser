using System.IO;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Data;
using DataVisualiser.Core.Data.Abstractions;
using DataVisualiser.Core.Orchestration;
using DataVisualiser.Core.Services;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.Shared.Models;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class MainChartsEvidenceExportServiceTests
{
    [Fact]
    public async Task ExportAsync_ShouldWriteHeadlessEvidencePayload()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new StubEvidenceStore();
            var service = CreateService(tempDir, store);
            var result = await service.ExportAsync(new ChartState(), new MetricState(), new DateTime(2026, 3, 26, 10, 0, 0, DateTimeKind.Utc));

            Assert.True(File.Exists(result.FilePath));
            Assert.False(result.HadReachabilityRecords);
            Assert.NotEmpty(result.Notes);
            Assert.True(store.Cleared);

            var contents = File.ReadAllText(result.FilePath);
            Assert.Contains("\"ParityWarnings\"", contents);
            Assert.Contains("\"DistributionParity\"", contents);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static MainChartsEvidenceExportService CreateService(string targetDirectory, StubEvidenceStore? store = null)
    {
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        return new MainChartsEvidenceExportService(
            new ReachabilityExportWriter(),
            new StubPathResolver(targetDirectory),
            store ?? new StubEvidenceStore(),
            metricService,
            () => null,
            () => null,
            () => "Bar");
    }

    private sealed class StubPathResolver : IReachabilityExportPathResolver
    {
        private readonly string _targetDirectory;

        public StubPathResolver(string targetDirectory)
        {
            _targetDirectory = targetDirectory;
        }

        public string ResolveDocumentsDirectory()
        {
            return _targetDirectory;
        }
    }

    private sealed class StubEvidenceStore : IReachabilityEvidenceStore
    {
        public bool Cleared { get; private set; }

        public IReadOnlyList<StrategyReachabilityRecord> Snapshot()
        {
            return Array.Empty<StrategyReachabilityRecord>();
        }

        public void Clear()
        {
            Cleared = true;
        }
    }

    private sealed class StubMetricSelectionDataQueries : IMetricSelectionDataQueries
    {
        public Task<long> GetRecordCount(string metricType, string? metricSubtype = null)
        {
            return Task.FromResult(0L);
        }

        public Task<IEnumerable<MetricData>> GetHealthMetricsDataByBaseType(string baseType, string? subtype, DateTime? from, DateTime? to, string tableName, int? maxRecords = null, SamplingMode samplingMode = SamplingMode.None, int? targetSamples = null)
        {
            return Task.FromResult<IEnumerable<MetricData>>(Array.Empty<MetricData>());
        }

        public Task<IEnumerable<MetricNameOption>> GetBaseMetricTypeOptions(string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<IEnumerable<MetricNameOption>> GetSubtypeOptionsForBaseType(string baseType, string tableName)
        {
            return Task.FromResult<IEnumerable<MetricNameOption>>(Array.Empty<MetricNameOption>());
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRange(string baseType, string? subtype, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeFromCounts(string baseType, IReadOnlyCollection<string>? subtypes = null)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }

        public Task<(DateTime MinDate, DateTime MaxDate)?> GetBaseTypeDateRangeForSubtypes(string baseType, IReadOnlyCollection<string>? subtypes, string tableName)
        {
            return Task.FromResult<(DateTime MinDate, DateTime MaxDate)?>(null);
        }
    }
}
