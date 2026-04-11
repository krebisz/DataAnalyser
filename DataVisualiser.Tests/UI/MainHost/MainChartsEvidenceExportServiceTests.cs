using System.IO;
using System.Text.Json;
using DataVisualiser.Core.Configuration;
using DataVisualiser.Core.Data;
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
            Assert.Contains("\"Diagnostics\"", contents);
            Assert.Contains("\"LoadedContext\"", contents);
            Assert.Contains("\"MainChartPipeline\"", contents);
            Assert.Contains("\"Reachability\"", contents);
            Assert.Contains("\"UiSurface\"", contents);
            Assert.Contains("\"SmokeChecks\"", contents);
            Assert.Contains("\"Transition\"", contents);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeSelectionAndContextDiagnostics()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState
            {
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    Data2 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }],
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    SecondaryMetricType = "Weight",
                    PrimarySubtype = "morning",
                    SecondarySubtype = "evening",
                    LoadRequestSignature = "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening|Weight:weekly_avg",
                    ActualSeriesCount = 2,
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                }
            };
            var metricState = new MetricState
            {
                SelectedMetricType = "Weight",
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 1, 2),
                ResolutionTableName = "HealthMetrics"
            };
            metricState.SetSeriesSelections(
            [
                new MetricSeriesSelection("Weight", "morning", "Weight", "Morning"),
                new MetricSeriesSelection("Weight", "evening", "Weight", "Evening"),
                new MetricSeriesSelection("Weight", "weekly_avg", "Weight", "Weekly Avg")
            ]);

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");

            Assert.Equal(3, diagnostics.GetProperty("Selection").GetProperty("SelectedSeriesCount").GetInt32());
            Assert.True(diagnostics.GetProperty("LoadedContext").GetProperty("ReusableForCurrentSelection").GetBoolean());
            Assert.True(diagnostics.GetProperty("Transition").GetProperty("LoadedRequestMatchesCurrentSelection").GetBoolean());
            Assert.Equal(1, diagnostics.GetProperty("MainChartPipeline").GetProperty("ExpectedAdditionalSeriesLoad").GetInt32());
            Assert.Equal("SeriesCountMismatch", diagnostics.GetProperty("Transition").GetProperty("State").GetString());
            Assert.True(diagnostics.GetProperty("Transition").GetProperty("ReloadLikelyRequired").GetBoolean());

            var orderedSeries = diagnostics.GetProperty("Selection").GetProperty("OrderedSeries");
            Assert.Equal(3, orderedSeries.GetArrayLength());
            Assert.True(orderedSeries[2].GetProperty("RequiresOnDemandResolution").GetBoolean());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeUiSurfaceDiagnosticsAndSmokeChecks()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var uiSnapshot = new UiSurfaceDiagnosticsSnapshot
            {
                MetricType = new MetricTypeUiDiagnosticsSnapshot
                {
                    SelectedValue = "Weight",
                    SelectedDisplay = "Weight",
                    OptionCount = 4
                },
                DateRange = new DateRangeUiDiagnosticsSnapshot
                {
                    SelectedFromDate = new DateTime(2026, 3, 6),
                    SelectedToDate = new DateTime(2026, 4, 5),
                    ExpectedDefaultFromDateUtc = new DateTime(2026, 3, 6),
                    ExpectedDefaultToDateUtc = new DateTime(2026, 4, 5),
                    MatchesExpectedDefaultWindow = true
                },
                Subtypes = new SubtypeUiDiagnosticsSnapshot
                {
                    ActiveComboCount = 2,
                    PrimarySelectionMaterialized = true,
                    AllCombosBoundToSelectedMetricType = true,
                    OrderedCombos =
                    [
                        new SubtypeComboDiagnosticsSnapshot
                        {
                            Index = 0,
                            BoundMetricType = "Weight",
                            SelectedValue = "morning",
                            SelectedDisplay = "Morning",
                            OptionCount = 2,
                            OptionValues = ["morning", "evening"]
                        },
                        new SubtypeComboDiagnosticsSnapshot
                        {
                            Index = 1,
                            BoundMetricType = "Weight",
                            SelectedValue = "evening",
                            SelectedDisplay = "Evening",
                            OptionCount = 2,
                            OptionValues = ["morning", "evening"]
                        }
                    ]
                },
                Transform = new TransformUiDiagnosticsSnapshot
                {
                    PanelVisible = true,
                    SecondaryPanelVisible = true,
                    ComputeEnabled = true,
                    SelectedOperation = "Add",
                    SelectedPrimarySubtype = "morning",
                    SelectedSecondarySubtype = "evening",
                    PrimaryOptionCount = 2,
                    SecondaryOptionCount = 2
                },
                RecentMessages =
                [
                    new HostMessageDiagnosticsSnapshot
                    {
                        TimestampUtc = new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc),
                        Severity = "Error",
                        Title = "Error",
                        Message = "Synthetic export diagnostic"
                    }
                ]
            };

            var service = CreateService(tempDir, uiSnapshotFactory: () => uiSnapshot);
            var chartState = new ChartState
            {
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    Data2 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }],
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    SecondaryMetricType = "Weight",
                    PrimarySubtype = "morning",
                    SecondarySubtype = "evening",
                    LoadRequestSignature = "Weight::HealthMetrics::2026-03-06T00:00:00.0000000->2026-04-05T00:00:00.0000000::Weight:morning|Weight:evening",
                    ActualSeriesCount = 2,
                    From = new DateTime(2026, 3, 6),
                    To = new DateTime(2026, 4, 5)
                }
            };
            var metricState = new MetricState
            {
                SelectedMetricType = "Weight",
                FromDate = new DateTime(2026, 3, 6),
                ToDate = new DateTime(2026, 4, 5),
                ResolutionTableName = "HealthMetrics"
            };
            metricState.SetSeriesSelections(
            [
                new MetricSeriesSelection("Weight", "morning", "Weight", "Morning"),
                new MetricSeriesSelection("Weight", "evening", "Weight", "Evening")
            ]);

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");
            var uiSurface = diagnostics.GetProperty("UiSurface");
            var smokeChecks = diagnostics.GetProperty("SmokeChecks");
            var transition = diagnostics.GetProperty("Transition");

            Assert.Equal("Weight", uiSurface.GetProperty("MetricType").GetProperty("SelectedValue").GetString());
            Assert.Equal(2, uiSurface.GetProperty("Subtypes").GetProperty("ActiveComboCount").GetInt32());
            Assert.True(uiSurface.GetProperty("Subtypes").GetProperty("PrimaryOptionsMatchSelectedMetric").GetBoolean());
            Assert.True(smokeChecks.GetProperty("SubtypeComboCountMatchesSelectedSeries").GetBoolean());
            Assert.True(smokeChecks.GetProperty("LoadedSeriesCountMatchesSelection").GetBoolean());
            Assert.True(smokeChecks.GetProperty("RecentErrorsPresent").GetBoolean());
            Assert.Equal(1, smokeChecks.GetProperty("RecentErrorCount").GetInt32());
            Assert.True(transition.GetProperty("LoadedRequestMatchesCurrentSelection").GetBoolean());
            Assert.Equal("HostErrorObserved", transition.GetProperty("State").GetString());
            Assert.Equal(2, transition.GetProperty("ExpectedSeriesCount").GetInt32());
            Assert.Equal(2, transition.GetProperty("LoadedContextSeriesCount").GetInt32());
            Assert.False(transition.GetProperty("ReloadLikelyRequired").GetBoolean());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldFlagStoredContextLaggingWhenReachabilityShowsMoreSeries()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var store = new StubEvidenceStore(
            [
                new StrategyReachabilityRecord(
                    StrategyType.MultiMetric,
                    false,
                    false,
                    false,
                    false,
                    false,
                    "Rendered with two series",
                    true,
                    false,
                    10,
                    0,
                    0,
                    2,
                    new DateTime(2026, 4, 1),
                    new DateTime(2026, 4, 5),
                    DateTime.UtcNow)
            ]);

            var service = CreateService(tempDir, store);
            var chartState = new ChartState
            {
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    MetricType = "SkinTemperature",
                    PrimaryMetricType = "SkinTemperature",
                    PrimarySubtype = "left",
                    ActualSeriesCount = 1,
                    From = new DateTime(2026, 4, 1),
                    To = new DateTime(2026, 4, 5)
                }
            };
            var metricState = new MetricState
            {
                SelectedMetricType = "SkinTemperature",
                FromDate = new DateTime(2026, 4, 1),
                ToDate = new DateTime(2026, 4, 5),
                ResolutionTableName = "HealthMetrics"
            };
            metricState.SetSeriesSelections(
            [
                new MetricSeriesSelection("SkinTemperature", "left"),
                new MetricSeriesSelection("SkinTemperature", "right")
            ]);

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 5, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var transition = document.RootElement.GetProperty("Diagnostics").GetProperty("Transition");

            Assert.Equal("StaleContextAfterRender", transition.GetProperty("State").GetString());
            Assert.True(transition.GetProperty("RenderEvidenceExceedsStoredContext").GetBoolean());
            Assert.True(transition.GetProperty("ReloadLikelyRequired").GetBoolean());
            Assert.Equal(2, transition.GetProperty("LatestReachabilitySeriesCount").GetInt32());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldEmitVNextRuntimeDiagnosticsFromLastLoadRuntime()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState
            {
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    Data2 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }],
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    SecondaryMetricType = "Weight",
                    PrimarySubtype = "morning",
                    SecondarySubtype = "evening",
                    LoadRequestSignature = "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening",
                    ActualSeriesCount = 2,
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                },
                LastLoadRuntime = new LoadRuntimeState(
                    EvidenceRuntimePath.VNextMain,
                    "req-sig",
                    "req-sig",
                    DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                    "req-sig",
                    "req-sig",
                    null,
                    true)
            };
            var metricState = new MetricState
            {
                SelectedMetricType = "Weight",
                FromDate = new DateTime(2024, 1, 1),
                ToDate = new DateTime(2024, 1, 2),
                ResolutionTableName = "HealthMetrics"
            };
            metricState.SetSeriesSelections(
            [
                new MetricSeriesSelection("Weight", "morning"),
                new MetricSeriesSelection("Weight", "evening")
            ]);

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 11, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");
            var vnext = diagnostics.GetProperty("VNext");

            Assert.Equal("VNextMain", diagnostics.GetProperty("RuntimePath").GetString());
            Assert.Equal("req-sig", vnext.GetProperty("RequestSignature").GetString());
            Assert.Equal("req-sig", vnext.GetProperty("SnapshotSignature").GetString());
            Assert.Equal("Main", vnext.GetProperty("ProgramKind").GetString());
            Assert.True(vnext.GetProperty("RequestMatchesSnapshot").GetBoolean());
            Assert.True(vnext.GetProperty("SnapshotMatchesProgramSource").GetBoolean());
            Assert.True(vnext.GetProperty("ProgramSourceMatchesProjectedContext").GetBoolean());
            Assert.True(vnext.GetProperty("SupportsOnlyMainChart").GetBoolean());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static MainChartsEvidenceExportService CreateService(
        string targetDirectory,
        StubEvidenceStore? store = null,
        Func<UiSurfaceDiagnosticsSnapshot>? uiSnapshotFactory = null)
    {
        var metricService = new MetricSelectionService(new StubMetricSelectionDataQueries(), "TestConnection");
        return new MainChartsEvidenceExportService(
            new ReachabilityExportWriter(),
            new StubPathResolver(targetDirectory),
            store ?? new StubEvidenceStore(),
            metricService,
            () => null,
            () => null,
            () => "Bar",
            uiSnapshotFactory);
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
        private readonly IReadOnlyList<StrategyReachabilityRecord> _records;

        public StubEvidenceStore()
            : this(Array.Empty<StrategyReachabilityRecord>())
        {
        }

        public StubEvidenceStore(IReadOnlyList<StrategyReachabilityRecord> records)
        {
            _records = records;
        }

        public bool Cleared { get; private set; }

        public IReadOnlyList<StrategyReachabilityRecord> Snapshot()
        {
            return _records;
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
            IEnumerable<MetricNameOption> options = string.Equals(baseType, "Weight", StringComparison.OrdinalIgnoreCase)
                ?
                [
                    new MetricNameOption("morning", "Morning"),
                    new MetricNameOption("evening", "Evening")
                ]
                : Array.Empty<MetricNameOption>();

            return Task.FromResult(options);
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
