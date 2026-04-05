using System.IO;
using System.Text.Json;
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
            var uiSnapshot = new MainChartsEvidenceExportService.UiSurfaceDiagnosticsSnapshot
            {
                MetricType = new MainChartsEvidenceExportService.MetricTypeUiDiagnosticsSnapshot
                {
                    SelectedValue = "Weight",
                    SelectedDisplay = "Weight",
                    OptionCount = 4
                },
                DateRange = new MainChartsEvidenceExportService.DateRangeUiDiagnosticsSnapshot
                {
                    SelectedFromDate = new DateTime(2026, 3, 6),
                    SelectedToDate = new DateTime(2026, 4, 5),
                    ExpectedDefaultFromDateUtc = new DateTime(2026, 3, 6),
                    ExpectedDefaultToDateUtc = new DateTime(2026, 4, 5),
                    MatchesExpectedDefaultWindow = true
                },
                Subtypes = new MainChartsEvidenceExportService.SubtypeUiDiagnosticsSnapshot
                {
                    ActiveComboCount = 2,
                    PrimarySelectionMaterialized = true,
                    AllCombosBoundToSelectedMetricType = true,
                    OrderedCombos =
                    [
                        new MainChartsEvidenceExportService.SubtypeComboDiagnosticsSnapshot
                        {
                            Index = 0,
                            BoundMetricType = "Weight",
                            SelectedValue = "morning",
                            SelectedDisplay = "Morning",
                            OptionCount = 2,
                            OptionValues = ["morning", "evening"]
                        },
                        new MainChartsEvidenceExportService.SubtypeComboDiagnosticsSnapshot
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
                Transform = new MainChartsEvidenceExportService.TransformUiDiagnosticsSnapshot
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
                    new MainChartsEvidenceExportService.HostMessageDiagnosticsSnapshot
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

            Assert.Equal("StoredContextLagging", transition.GetProperty("State").GetString());
            Assert.True(transition.GetProperty("RenderEvidenceExceedsStoredContext").GetBoolean());
            Assert.True(transition.GetProperty("ReloadLikelyRequired").GetBoolean());
            Assert.Equal(2, transition.GetProperty("LatestReachabilitySeriesCount").GetInt32());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static MainChartsEvidenceExportService CreateService(
        string targetDirectory,
        StubEvidenceStore? store = null,
        Func<MainChartsEvidenceExportService.UiSurfaceDiagnosticsSnapshot>? uiSnapshotFactory = null)
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
