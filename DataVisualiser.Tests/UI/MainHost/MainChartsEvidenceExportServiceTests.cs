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
using DataVisualiser.UI.MainHost.Evidence;
using DataVisualiser.UI.MainHost.Export;
using DataVisualiser.UI.State;
using DataVisualiser.VNext.Rendering;

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
            Assert.Contains("\"RenderPlans\"", contents);
            Assert.Contains("\"SessionMilestones\"", contents);
            Assert.Contains("\"PerformanceTimings\"", contents);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludePerformanceTimings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            chartState.RecordPerformanceTiming("MetricLoad", "Total", 123, EvidenceRuntimePath.VNextMain, "test");

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var timings = document.RootElement.GetProperty("PerformanceTimings");

            Assert.True(timings.GetArrayLength() >= 1);
            Assert.Contains(timings.EnumerateArray(), timing =>
                timing.GetProperty("Scope").GetString() == "MetricLoad" &&
                timing.GetProperty("Operation").GetString() == "Total" &&
                timing.GetProperty("DurationMs").GetInt64() == 123 &&
                timing.GetProperty("RuntimePath").GetString() == "VNextMain");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeRenderPlanBackendAndDensityDiagnostics()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf",
                    "Main:sig-1",
                    ChartRenderPlanKind.Cartesian,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 2,
                    RenderedHierarchyNodeCount: 0,
                    RenderedPointCount: 4,
                    new Dictionary<string, string>
                    {
                        ["Adapter"] = "LiveChartsRenderPlanAdapter",
                        ["ProgramKind"] = "Main"
                    }));

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var renderPlan = document.RootElement
                .GetProperty("Diagnostics")
                .GetProperty("RenderPlans")
                .GetProperty("Main");

            Assert.Equal("LiveChartsWpf", renderPlan.GetProperty("BackendKey").GetString());
            Assert.Equal("Main:sig-1", renderPlan.GetProperty("PlanId").GetString());
            Assert.Equal("Cartesian", renderPlan.GetProperty("PlanKind").GetString());
            Assert.Equal("FullFidelity", renderPlan.GetProperty("DensityMode").GetString());
            Assert.Equal(2, renderPlan.GetProperty("RenderedSeriesCount").GetInt32());
            Assert.Equal(4, renderPlan.GetProperty("RenderedPointCount").GetInt32());
            Assert.Equal("LiveChartsRenderPlanAdapter", renderPlan.GetProperty("Metadata").GetProperty("Adapter").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldSummarizeRenderPlanVocabularyMetadata()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf",
                    "Main:sig-1",
                    ChartRenderPlanKind.Cartesian,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 2,
                    RenderedHierarchyNodeCount: 0,
                    RenderedPointCount: 4,
                    new Dictionary<string, string>
                    {
                        [ChartRenderPlanMetadataKeys.IntentSignature] = "intent-main",
                        [ChartRenderPlanMetadataKeys.ProvenanceSignature] = "VNext:Requested:req-main",
                        [ChartRenderPlanMetadataKeys.ConsumerKind] = "Chart",
                        [ChartRenderPlanMetadataKeys.DeliveryTarget] = "MainChart",
                        [ChartRenderPlanMetadataKeys.CapabilityKind] = "Identity",
                        [ChartRenderPlanMetadataKeys.CompositionKind] = "MultiSeries",
                        [ChartRenderPlanMetadataKeys.OverlayCount] = "1",
                        [ChartRenderPlanMetadataKeys.InteractionCount] = "2"
                    }));
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Distribution,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf",
                    "Distribution:sig-1",
                    ChartRenderPlanKind.Cartesian,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 1,
                    RenderedHierarchyNodeCount: 0,
                    RenderedPointCount: 3,
                    new Dictionary<string, string>
                    {
                        ["ProgramKind"] = "Distribution"
                    }));

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 26, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var vocabulary = document.RootElement
                .GetProperty("Diagnostics")
                .GetProperty("RenderPlanVocabulary");

            Assert.Equal(2, vocabulary.GetProperty("RenderPlanCount").GetInt32());
            Assert.Equal(1, vocabulary.GetProperty("PlansWithIntentSignature").GetInt32());
            Assert.Equal(1, vocabulary.GetProperty("PlansWithProvenanceSignature").GetInt32());
            Assert.Contains(vocabulary.GetProperty("ConsumerKinds").EnumerateArray(), item => item.GetString() == "Chart");
            Assert.Contains(vocabulary.GetProperty("DeliveryTargets").EnumerateArray(), item => item.GetString() == "MainChart");
            Assert.Contains(vocabulary.GetProperty("CapabilityKinds").EnumerateArray(), item => item.GetString() == "Identity");
            Assert.Contains(vocabulary.GetProperty("CompositionKinds").EnumerateArray(), item => item.GetString() == "MultiSeries");
            Assert.Equal(1, vocabulary.GetProperty("OverlayCountTotal").GetInt32());
            Assert.Equal(2, vocabulary.GetProperty("InteractionCountTotal").GetInt32());
            Assert.Contains(vocabulary.GetProperty("MissingVocabularyPlanKinds").EnumerateArray(), item => item.GetString() == "Distribution");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldReportNoMissingVocabulary_WhenAllRenderPlansCarryVocabularyMetadata()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            foreach (var kind in new[]
                     {
                         DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.Normalized,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.Transform,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.Distribution,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.WeekdayTrend,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.BarPie,
                         DataVisualiser.VNext.Contracts.ChartProgramKind.SyncfusionSunburst
                     })
            {
                var metadata = new Dictionary<string, string>
                {
                    ["ProgramKind"] = kind.ToString()
                };
                ChartRenderPlanVocabularyMetadata.AddTo(metadata, kind, $"source:{kind}");
                chartState.SetRenderPlanDiagnostics(
                    kind,
                    new ChartRenderAdapterResult(
                        "TestBackend",
                        $"{kind}:plan",
                        kind == DataVisualiser.VNext.Contracts.ChartProgramKind.SyncfusionSunburst
                            ? ChartRenderPlanKind.Hierarchy
                            : ChartRenderPlanKind.Cartesian,
                        ChartRenderDensityMode.FullFidelity,
                        RenderedSeriesCount: 1,
                        RenderedHierarchyNodeCount: 0,
                        RenderedPointCount: 1,
                        metadata));
            }

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 26, 13, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var vocabulary = document.RootElement
                .GetProperty("Diagnostics")
                .GetProperty("RenderPlanVocabulary");

            Assert.Equal(7, vocabulary.GetProperty("RenderPlanCount").GetInt32());
            Assert.Equal(7, vocabulary.GetProperty("PlansWithIntentSignature").GetInt32());
            Assert.Equal(7, vocabulary.GetProperty("PlansWithProvenanceSignature").GetInt32());
            Assert.Empty(vocabulary.GetProperty("MissingVocabularyPlanKinds").EnumerateArray());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeRenderPlanHistoryForRepeatedChartFamilyRoutes()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.BarPie,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf.Column",
                    "BarPie:bar",
                    ChartRenderPlanKind.Cartesian,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 3,
                    RenderedHierarchyNodeCount: 0,
                    RenderedPointCount: 9,
                    new Dictionary<string, string>
                    {
                        ["ProgramKind"] = "BarPie",
                        ["Route"] = "Column"
                    }));
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.BarPie,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf.PieFacet",
                    "BarPie:pie",
                    ChartRenderPlanKind.Faceted,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 0,
                    RenderedHierarchyNodeCount: 2,
                    RenderedPointCount: 6,
                    new Dictionary<string, string>
                    {
                        ["ProgramKind"] = "BarPie",
                        ["Route"] = "PieFacet"
                    }));

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 21, 10, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");
            var latest = diagnostics.GetProperty("RenderPlans").GetProperty("BarPie");
            var history = diagnostics.GetProperty("RenderPlanHistory").EnumerateArray().ToList();

            Assert.Equal("PieFacet", latest.GetProperty("Metadata").GetProperty("Route").GetString());
            Assert.Equal(2, history.Count);
            Assert.Contains(history, entry =>
                entry.GetProperty("ProgramKind").GetString() == "BarPie" &&
                entry.GetProperty("Metadata").GetProperty("Route").GetString() == "Column");
            Assert.Contains(history, entry =>
                entry.GetProperty("ProgramKind").GetString() == "BarPie" &&
                entry.GetProperty("Metadata").GetProperty("Route").GetString() == "PieFacet");
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

    [Fact]
    public async Task ExportAsync_ShouldNotRequireReloadForNormalizedVNextFamilyLoad()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            const string requestSignature = "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening";
            var chartState = new ChartState
            {
                IsMainVisible = true,
                IsNormalizedVisible = true,
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    Data2 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }],
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    SecondaryMetricType = "Weight",
                    PrimarySubtype = "morning",
                    SecondarySubtype = "evening",
                    LoadRequestSignature = requestSignature,
                    ActualSeriesCount = 2,
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                },
                LastLoadRuntime = new LoadRuntimeState(
                    EvidenceRuntimePath.VNextMain,
                    requestSignature,
                    requestSignature,
                    DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                    requestSignature,
                    requestSignature,
                    null,
                    false)
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

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 12, 12, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");
            var transition = diagnostics.GetProperty("Transition");
            var vnext = diagnostics.GetProperty("VNext");

            Assert.Equal("VNextMain", diagnostics.GetProperty("RuntimePath").GetString());
            Assert.Equal("ContextAligned", transition.GetProperty("State").GetString());
            Assert.False(transition.GetProperty("ReloadLikelyRequired").GetBoolean());
            Assert.False(vnext.GetProperty("SupportsOnlyMainChart").GetBoolean());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExportAsync_ShouldEmitRenderedNormalizedFamilyRuntimeAfterVNextMainLoad()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            const string requestSignature = "Weight::HealthMetrics::2024-01-01T00:00:00.0000000->2024-01-02T00:00:00.0000000::Weight:morning|Weight:evening";
            var chartState = new ChartState
            {
                IsMainVisible = true,
                IsNormalizedVisible = true,
                LastContext = new ChartDataContext
                {
                    Data1 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 1m }],
                    Data2 = [new MetricData { NormalizedTimestamp = DateTime.Today, Value = 2m }],
                    MetricType = "Weight",
                    PrimaryMetricType = "Weight",
                    SecondaryMetricType = "Weight",
                    PrimarySubtype = "morning",
                    SecondarySubtype = "evening",
                    LoadRequestSignature = requestSignature,
                    ActualSeriesCount = 2,
                    From = new DateTime(2024, 1, 1),
                    To = new DateTime(2024, 1, 2)
                },
                LastLoadRuntime = new LoadRuntimeState(
                    EvidenceRuntimePath.VNextMain,
                    requestSignature,
                    requestSignature,
                    DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
                    requestSignature,
                    requestSignature,
                    null,
                    true)
            };
            chartState.SetFamilyRuntime(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Normalized,
                new LoadRuntimeState(
                    EvidenceRuntimePath.VNextNormalized,
                    requestSignature,
                    requestSignature,
                    DataVisualiser.VNext.Contracts.ChartProgramKind.Normalized,
                    requestSignature,
                    "normalized-context",
                    null,
                    false));
            chartState.SetRenderPlanDiagnostics(
                DataVisualiser.VNext.Contracts.ChartProgramKind.Normalized,
                new ChartRenderAdapterResult(
                    "LiveChartsWpf",
                    "Normalized:sig-1",
                    ChartRenderPlanKind.Cartesian,
                    ChartRenderDensityMode.FullFidelity,
                    RenderedSeriesCount: 2,
                    RenderedHierarchyNodeCount: 0,
                    RenderedPointCount: 4,
                    new Dictionary<string, string>
                    {
                        ["Adapter"] = "LiveChartsRenderPlanAdapter",
                        ["ProgramKind"] = "Normalized"
                    }));
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

            var result = await service.ExportAsync(chartState, metricState, new DateTime(2026, 4, 26, 12, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var diagnostics = document.RootElement.GetProperty("Diagnostics");
            var vnext = diagnostics.GetProperty("VNext");
            var normalized = diagnostics.GetProperty("VNextFamilies").GetProperty("Normalized");

            Assert.False(vnext.GetProperty("SupportsOnlyMainChart").GetBoolean());
            Assert.Equal("Normalized", normalized.GetProperty("ProgramKind").GetString());
            Assert.Equal("normalized-context", normalized.GetProperty("ProjectedContextSignature").GetString());
            Assert.False(normalized.GetProperty("SupportsOnlyMainChart").GetBoolean());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildTransitionDiagnostics_ShouldNotRequireReloadForExtendedChartsWhenRenderEvidenceExists()
    {
        var selectedSeries = new List<MetricSeriesSelection>
        {
            new("Weight", "body_fat_mass")
        };

        var runtime = new LoadRuntimeState(
            EvidenceRuntimePath.VNextMain,
            "Weight::HealthMetrics::2023-08-15T12:05:48.2933333->2023-11-23T00:00:00.0000000::Weight:body_fat_mass",
            "Weight::HealthMetrics::2023-08-15T12:05:48.2933333->2023-11-23T00:00:00.0000000::Weight:body_fat_mass",
            DataVisualiser.VNext.Contracts.ChartProgramKind.Main,
            "Weight::HealthMetrics::2023-08-15T12:05:48.2933333->2023-11-23T00:00:00.0000000::Weight:body_fat_mass",
            "Weight::HealthMetrics::2023-08-15T12:05:48.2933333->2023-11-23T00:00:00.0000000::Weight:body_fat_mass",
            null,
            true);

        var context = new ChartDataContext
        {
            Data1 = [new MetricData { NormalizedTimestamp = DateTime.UtcNow, Value = 1m }],
            ActualSeriesCount = 1,
            PrimaryMetricType = "Weight",
            PrimarySubtype = "body_fat_mass",
            LoadRequestSignature = runtime.RequestSignature
        };

        var transition = EvidenceDiagnosticsBuilder.BuildTransitionDiagnostics(
            new MetricState { SelectedMetricType = "Weight" },
            selectedSeries,
            context,
            reusableContext: true,
            latestRecord: null,
            expectedSeriesCount: 1,
            recentErrorCount: 0,
            runtime,
            visibleChartsRequireExtendedContext: true,
            hasExtendedChartRenderEvidence: true);

        Assert.Equal("ContextAligned", transition.State);
        Assert.False(transition.ReloadLikelyRequired);
    }

    [Fact]
    public async Task ExportAsync_ShouldIncludeSessionMilestonesAcrossScenarioFlow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "DataVisualiser.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var service = CreateService(tempDir);
            var chartState = new ChartState();
            chartState.RecordSessionMilestone(new SessionMilestoneSnapshot
            {
                TimestampUtc = new DateTime(2026, 4, 11, 18, 0, 0, DateTimeKind.Utc),
                Kind = "TransformPrimaryProjectionRendered",
                Outcome = "Success",
                MetricType = "Weight",
                SelectedSeriesCount = 1,
                SelectedDisplayKeys = ["Weight:body_fat_mass"],
                RuntimePath = EvidenceRuntimePath.Legacy,
                LoadedSeriesCount = 1,
                ContextSignature = "ctx-1",
                OperationArity = 1,
                PrimarySeriesDisplayKey = "Weight:body_fat_mass",
                ResultPointCount = 12,
                Note = "Primary data projected without an explicit transform operation."
            });
            chartState.RecordSessionMilestone(new SessionMilestoneSnapshot
            {
                TimestampUtc = new DateTime(2026, 4, 11, 18, 5, 0, DateTimeKind.Utc),
                Kind = "TransformOperationRendered",
                Outcome = "Success",
                MetricType = "Weight",
                SelectedSeriesCount = 2,
                SelectedDisplayKeys = ["Weight:body_fat_mass", "Weight:skeletal_muscle_mass"],
                RuntimePath = EvidenceRuntimePath.Legacy,
                LoadedSeriesCount = 2,
                ContextSignature = "ctx-2",
                Operation = "Add",
                OperationArity = 2,
                PrimarySeriesDisplayKey = "Weight:body_fat_mass",
                SecondarySeriesDisplayKey = "Weight:skeletal_muscle_mass",
                ResultPointCount = 12
            });

            var result = await service.ExportAsync(chartState, new MetricState(), new DateTime(2026, 4, 11, 20, 0, 0, DateTimeKind.Utc));
            using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
            var milestones = document.RootElement.GetProperty("SessionMilestones");

            Assert.Equal(2, milestones.GetArrayLength());
            Assert.Equal("TransformPrimaryProjectionRendered", milestones[0].GetProperty("Kind").GetString());
            Assert.Equal(1, milestones[0].GetProperty("SelectedSeriesCount").GetInt32());
            Assert.Equal("TransformOperationRendered", milestones[1].GetProperty("Kind").GetString());
            Assert.Equal("Add", milestones[1].GetProperty("Operation").GetString());
            Assert.Equal(2, milestones[1].GetProperty("OperationArity").GetInt32());
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
