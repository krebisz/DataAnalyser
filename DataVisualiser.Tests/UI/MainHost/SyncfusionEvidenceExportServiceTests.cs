using System.IO;
using DataVisualiser.Core.Strategies.Abstractions;
using DataVisualiser.Core.Strategies.Reachability;
using DataVisualiser.UI.MainHost;
using DataVisualiser.UI.State;
using DataVisualiser.UI.SyncfusionViews;

namespace DataVisualiser.Tests.UI.MainHost;

public sealed class SyncfusionEvidenceExportServiceTests
{
    [Fact]
    public void Export_ShouldWriteSyncfusionScopedEvidencePayload()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new StubEvidenceStore();
            var service = new SyncfusionEvidenceExportService(
                new ReachabilityExportWriter(),
                new StubPathResolver(tempDir),
                store);

            var result = service.Export(new ChartState(), new MetricState(), new DateTime(2026, 3, 26, 10, 0, 0, DateTimeKind.Utc));

            Assert.True(File.Exists(result.FilePath));
            Assert.Contains(SyncfusionChartsViewCoordinator.ReachabilityExportNotWiredMessage, result.Notes);
            Assert.False(result.HadReachabilityRecords);
            Assert.True(store.Cleared);

            var contents = File.ReadAllText(result.FilePath);
            Assert.Contains("\"ExportScope\": \"Syncfusion\"", contents);
            Assert.Contains("\"ReachabilityStatus\": \"NotApplicable\"", contents);
            Assert.Contains("\"ReachabilityRecords\": []", contents);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
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
            return
            [
                new StrategyReachabilityRecord(
                    StrategyType.MultiMetric,
                    true,
                    true,
                    true,
                    true,
                    true,
                    "Test",
                    true,
                    true,
                    1,
                    1,
                    3,
                    3,
                    new DateTime(2026, 3, 26),
                    new DateTime(2026, 3, 27),
                    DateTime.UtcNow)
            ];
        }

        public void Clear()
        {
            Cleared = true;
        }
    }
}
