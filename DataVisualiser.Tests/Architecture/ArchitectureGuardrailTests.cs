using System.IO;
using System.Text;
using DataVisualiser.Tests.Helpers;
using DataVisualiser.Tests.Helpers.Infrastructure;

namespace DataVisualiser.Tests.Architecture;

public sealed class ArchitectureGuardrailTests
{
    [Fact]
    public void MainChartsView_ShouldKeepExportOnDedicatedWriterSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsEvidenceExportService", source);
        Assert.DoesNotContain("File.WriteAllText(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.CreateDirectory(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer.Serialize(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceDiagnosticsBuilder_ShouldNotDependBackOnExportServiceHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Evidence", "EvidenceDiagnosticsBuilder.cs");

        Assert.DoesNotContain("MainChartsEvidenceExportService.IsSameSelection", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CompactMigrationPlan_ShouldCarryForwardStructuralSpineAndConsumptionMigrationRules()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Migration_Plan_and_Guardrails.md");

        Assert.Contains("Compact Continuation Version", source, StringComparison.Ordinal);
        Assert.Contains("Phases 1-23 are completed structural spine migration.", source, StringComparison.Ordinal);
        Assert.Contains("Phases 24-35 are consumption migration and convergence.", source, StringComparison.Ordinal);
        Assert.Contains("Do not treat Phase 23 as full architectural completion.", source, StringComparison.Ordinal);
        Assert.Contains("Do not add new capability as the next priority unless it directly supports consumption migration.", source, StringComparison.Ordinal);
        Assert.Contains("Regenerate structural artifacts for Phase 24+ instead of relying on Phase 3 density numbers.", source, StringComparison.Ordinal);
        Assert.Contains("ChartDataContext remains the primary consumption-model blocker.", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CompactMigrationPlan_ShouldRetainTargetGrammarAndPostConvergenceDeferral()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Migration_Plan_and_Guardrails.md");

        Assert.Contains("Authority / Semantics / Provenance / Traceability / Envelope", source, StringComparison.Ordinal);
        Assert.Contains("Intent / Capability / Composition / Transformation / Interpretation / Confidence / Overlay", source, StringComparison.Ordinal);
        Assert.Contains("Program / Policy / Contract / Boundary / Neutrality / Qualification", source, StringComparison.Ordinal);
        Assert.Contains("Provider / Consumer / Interaction / SurfaceModel / Binding", source, StringComparison.Ordinal);
        Assert.Contains("Delivery / Backend / RuntimeBoundary / VendorBoundary / Lifecycle", source, StringComparison.Ordinal);
        Assert.Contains("Evidence / Diagnostics / Parity / Reachability / Validation / Audit / Record", source, StringComparison.Ordinal);
        Assert.Contains("Phases 36+ become post-convergence formalisation and bounded-generativity alignment.", source, StringComparison.Ordinal);
        Assert.Contains("Do not force post-convergence algebra into the active convergence phases.", source, StringComparison.Ordinal);
        Assert.Contains("Do not treat Phase 26 Operation Chain as full algebra implementation", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MigrationPlan_ShouldKeepChartDataContextAuditBeforeUiConsumptionContract()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Migration_Plan_and_Guardrails.md");

        Assert.True(
            source.IndexOf("## 4.1 Phase 24", StringComparison.Ordinal) <
            source.IndexOf("## 4.2 Phase 25", StringComparison.Ordinal));
        Assert.Contains("Map every site where ChartDataContext carries data", source, StringComparison.Ordinal);
        Assert.Contains("Define what production UI consumes instead of ChartDataContext.", source, StringComparison.Ordinal);
        Assert.Contains("Do not retire ChartDataContext-related bridges until replacement, parity, smoke, metadata, and provenance evidence exist.", source, StringComparison.Ordinal);
        Assert.Contains("A ChartDataContext retirement map exists.", source, StringComparison.Ordinal);
        Assert.Contains("A VNext-native UI consumption contract shape exists and is test-backed before any family migration begins.", source, StringComparison.Ordinal);
    }

    [Fact]
    public void CompactMigrationPlan_ShouldNotRequireArchivedScaffoldAuditsByDefault()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Migration_Plan_and_Guardrails.md");

        Assert.Contains("Early scaffold audit files from Phases 3-14 are historical evidence", source, StringComparison.Ordinal);
        Assert.Contains("Do not require future agents to read the individual scaffold audits by default.", source, StringComparison.Ordinal);
        Assert.Contains("Use archived audits only for historical investigation, not active execution routing.", source, StringComparison.Ordinal);
        Assert.Contains("DataVisualiser_Distribution_Capability_Slice_Audit.md", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartDataContextMigrationAudit_ShouldGateBridgeRetirement()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_ChartDataContext_Migration_Audit.md");

        Assert.Contains("Phase: 24 - Audit ChartDataContext Migration Path", source, StringComparison.Ordinal);
        Assert.Contains("Production files referencing ChartDataContext: 77", source, StringComparison.Ordinal);
        Assert.Contains("Test files referencing ChartDataContext: 52", source, StringComparison.Ordinal);
        Assert.Contains("No production `ChartDataContext` dependency is classified as retirable now.", source, StringComparison.Ordinal);
        Assert.Contains("VNext-native UI consumption contract", source, StringComparison.Ordinal);
        Assert.Contains("Consumer-neutral surface model", source, StringComparison.Ordinal);
        Assert.Contains("Strategy input contract", source, StringComparison.Ordinal);
        Assert.Contains("Evidence/parity snapshot input", source, StringComparison.Ordinal);
        Assert.Contains("LegacyChartProgramProjector", source, StringComparison.Ordinal);
        Assert.Contains("VNextDataResolutionHelper", source, StringComparison.Ordinal);
        Assert.Contains("LegacyMetricViewGateway", source, StringComparison.Ordinal);
        Assert.Contains("VNextMetricLoadRouter", source, StringComparison.Ordinal);
        Assert.Contains("It does not authorize bridge retirement by itself.", source, StringComparison.Ordinal);
    }

    [Fact]
    public void DistributionCapabilitySlice_ShouldRemainOwnedByTargetSpine()
    {
        var capabilitySource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Contracts", "CapabilityRequest.cs");
        var intentFactorySource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Application", "AnalyticalIntentFactory.cs");
        var plannerSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ViewModels", "VNextChartProgramRequestPlanner.cs");
        var renderingContractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "Core",
            "Rendering",
            "Contracts",
            "Distribution",
            "DistributionRenderingContract.cs");
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Charts",
            "Presentation",
            "DistributionChartControllerAdapter.cs");
        var builderSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Charts",
            "Presentation",
            "DistributionRenderInputBuilder.cs");

        Assert.Contains("ChartProgramKind.Distribution => new CapabilityRequest", capabilitySource, StringComparison.Ordinal);
        Assert.Contains("AnalyticalCapabilityKind.Distribution", capabilitySource, StringComparison.Ordinal);
        Assert.Contains("CompositionKind.SingleSeries", capabilitySource, StringComparison.Ordinal);
        Assert.Contains("public AnalyticalIntent Distribution(", intentFactorySource, StringComparison.Ordinal);
        Assert.Contains("ChartProgramRequest.Distribution()", intentFactorySource, StringComparison.Ordinal);
        Assert.Contains("ChartProgramRequest.Distribution()", plannerSource, StringComparison.Ordinal);
        Assert.Contains("VNextDataResolutionHelper.ResolveSeriesDataAsync", builderSource, StringComparison.Ordinal);
        Assert.Contains("ChartProgramKind.Distribution", adapterSource, StringComparison.Ordinal);
        Assert.Contains("EvidenceRuntimePath.VNextDistribution", builderSource, StringComparison.Ordinal);
        Assert.Contains("ChartRenderPlanVocabularyMetadata.AddTo", renderingContractSource, StringComparison.Ordinal);
        Assert.Contains("ChartProgramKind.Distribution", renderingContractSource, StringComparison.Ordinal);
        Assert.Contains("DistributionRenderingQualification.Qualified", renderingContractSource, StringComparison.Ordinal);

        Assert.DoesNotContain("AnalyticalCapabilityKind.Distribution", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("CompositionKind.SingleSeries", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("CapabilityRequest", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("AnalyticalIntentFactory", adapterSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartUpdateCoordinator_ShouldNotAcquireSemanticProviderOrEvidenceAuthority()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs");
        var forbiddenTokens = new[]
        {
            "EvidenceDiagnosticsBuilder",
            "MainChartsEvidenceExportService",
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartControllerFactoryContext_ShouldRemainCompositionContextNotProviderOrEvidenceBag()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "ChartControllerFactory.cs");
        var forbiddenTokens = new[]
        {
            "EvidenceDiagnosticsBuilder",
            "MainChartsEvidenceExportService",
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartRenderingOrchestrator_ShouldRemainCoordinationOnly()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Orchestration", "ChartRenderingOrchestrator.cs");
        var forbiddenTokens = new[]
        {
            "EvidenceDiagnosticsBuilder",
            "MainChartsEvidenceExportService",
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "ChartRenderPlanProjector",
            "ChartRenderPlanAdapterDispatcher",
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void MetricLoadCoordinator_ShouldNotAcquireRenderProviderOrInterpretationPolicy()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ViewModels", "MetricLoadCoordinator.cs");
        var forbiddenTokens = new[]
        {
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "ChartRenderPlanProjector",
            "ChartRenderPlanAdapterDispatcher",
            "LiveChartsRenderPlanAdapter",
            "SyncfusionSunburstRenderPlanAdapter",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void MetricLoadCoordinator_ShouldDelegateVNextRoutingToMetricLoadRouter()
    {
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ViewModels", "MetricLoadCoordinator.cs");
        var routerSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ViewModels", "VNextMetricLoadRouter.cs");

        Assert.Contains("VNextMetricLoadRouter", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("TryLoadAsync", coordinatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VNextChartRoutePolicy.ShouldUseMainFamilyPath", coordinatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VNextChartProgramRequestPlanner.BuildVisibleChartFamilyRequests", coordinatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RecordVNextFamilyRuntimes", coordinatorSource, StringComparison.Ordinal);

        Assert.Contains("VNextChartRoutePolicy.ShouldUseMainFamilyPath", routerSource, StringComparison.Ordinal);
        Assert.Contains("VNextChartProgramRequestPlanner.BuildVisibleChartFamilyRequests", routerSource, StringComparison.Ordinal);
        Assert.Contains("RecordVNextFamilyRuntimes", routerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartFamilyAdapters_ShouldNotAcquireProviderBackendOrAnalyticalAuthority()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "Charts", "Presentation")],
            [
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector",
                "ChartRenderPlanAdapterDispatcher",
                "AnalyticalIntentFactory",
                "ReasoningSessionCoordinator",
                "ConfidenceAnnotationEvaluator",
                "InterpretiveOverlayPlanner",
                "MainChartsEvidenceExportService"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void RenderPlanProjectors_ShouldRemainNonAuthoritativeTranslation()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "VNext", "Rendering", "ChartRenderPlanProjector.cs"),
            Path.Combine("DataVisualiser", "VNext", "Application", "LegacyChartProgramProjector.cs")
        };
        var forbiddenTokens = new[]
        {
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner",
            "MainChartsEvidenceExportService",
            "EvidenceDiagnosticsBuilder"
        };

        foreach (var file in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            foreach (var token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void RenderPlanAdapters_ShouldNotAcquireAuthorityPolicyOrEvidenceOwnership()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Rendering", "Adapters"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Distribution"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Syncfusion"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "WeekdayTrend")
            ],
            [
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector",
                "AnalyticalIntentFactory",
                "ReasoningSessionCoordinator",
                "ConfidenceAnnotationEvaluator",
                "InterpretiveOverlayPlanner",
                "MainChartsEvidenceExportService",
                "EvidenceDiagnosticsBuilder"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void AdapterDispatcher_ShouldOnlyQualifyAndDispatchPlans()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Rendering", "ChartRenderPlanAdapter.cs");
        var forbiddenTokens = new[]
        {
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner",
            "MainChartsEvidenceExportService",
            "EvidenceDiagnosticsBuilder"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartControllersAndInteractions_ShouldRemainNonAuthoritativeConsumers()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "UI", "Charts", "Controllers"),
                Path.Combine("DataVisualiser", "UI", "Charts", "Interaction"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Interaction"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Tooltip")
            ],
            [
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector",
                "AnalyticalIntentFactory",
                "ReasoningSessionCoordinator",
                "ConfidenceAnnotationEvaluator",
                "InterpretiveOverlayPlanner",
                "ChartRenderPlanProjector",
                "ChartRenderPlanAdapterDispatcher",
                "EvidenceDiagnosticsBuilder",
                "MainChartsEvidenceExportService"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void EventBindersAndUiStateHelpers_ShouldNotOwnAnalyticalOrProviderPolicy()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "UI", "MainHost", "Coordination"),
                Path.Combine("DataVisualiser", "UI", "State")
            ],
            [
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector",
                "AnalyticalIntentFactory",
                "ReasoningSessionCoordinator",
                "ConfidenceAnnotationEvaluator",
                "InterpretiveOverlayPlanner",
                "ChartRenderPlanProjector",
                "ChartRenderPlanAdapterDispatcher"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void ViewModelStateHelpers_ShouldNotConstructAnalyticalMeaning()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "UI", "ViewModels", "ChartVisibilityController.cs"),
            Path.Combine("DataVisualiser", "UI", "ViewModels", "BusyStateTracker.cs"),
            Path.Combine("DataVisualiser", "UI", "ViewModels", "MainWindowViewModel.StateSetters.cs")
        };
        var forbiddenTokens = new[]
        {
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ChartProgramRequest",
            "CapabilityRequest",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner",
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector"
        };

        foreach (var file in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            foreach (var token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ChartRenderPlan_ShouldRemainConsumerNeutralSurfaceModel()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Rendering", "ChartRenderPlan.cs");
        var forbiddenTokens = new[]
        {
            "using System.Windows",
            "using LiveCharts",
            "using Syncfusion",
            "DataVisualiser.UI",
            "CartesianChart",
            "PieChart",
            "Sunburst",
            "WebView"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void SurfaceModels_ShouldNotAcquireSemanticProviderOrEvidenceAuthority()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "VNext", "Rendering", "ChartRenderPlan.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "ChartRenderModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "UiChartRenderModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartSeriesModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartFacetModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartAxisModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartLegendModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartOverlayModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartInteractionModel.cs")
        };
        var forbiddenTokens = new[]
        {
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner",
            "EvidenceDiagnosticsBuilder",
            "MainChartsEvidenceExportService"
        };

        foreach (var file in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            foreach (var token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void UiRenderModels_ShouldStayUpstreamOfConcreteRendererLifecycle()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "UiChartRenderModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartSeriesModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartFacetModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartAxisModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartLegendModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartOverlayModel.cs"),
            Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ChartInteractionModel.cs")
        };
        var forbiddenTokens = new[]
        {
            "CartesianChart",
            "PieChart",
            "WebView",
            "AxisSection",
            "SeriesCollection",
            "ChartValues",
            "Sunburst"
        };

        foreach (var file in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            foreach (var token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void VNextSurfaceAndBackendContracts_ShouldNotImportConcreteVendorOrUiLibraries()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "VNext", "Contracts"),
                Path.Combine("DataVisualiser", "VNext", "Rendering")
            ],
            [
                "using LiveCharts",
                "using Syncfusion",
                "using System.Windows",
                "DataVisualiser.UI"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void TerminalRenderDelivery_ShouldNotAcquireAnalyticalOrEvidenceAuthority()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Rendering", "Engines"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Adapters"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Distribution"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Syncfusion"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "WeekdayTrend"),
                Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "LiveCharts"),
                Path.Combine("DataVisualiser", "UI", "Charts", "Presentation", "ECharts")
            ],
            [
                "AnalyticalIntentFactory",
                "ReasoningSessionCoordinator",
                "ConfidenceAnnotationEvaluator",
                "InterpretiveOverlayPlanner",
                "EvidenceDiagnosticsBuilder",
                "MainChartsEvidenceExportService",
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void RenderingHostLifecycleHelpers_ShouldRemainTerminalWiring()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "Charts",
            "Presentation",
            "RenderingHostLifecycleAdapterHelper.cs");
        var forbiddenTokens = new[]
        {
            "AnalyticalIntentFactory",
            "ReasoningSessionCoordinator",
            "ConfidenceAnnotationEvaluator",
            "InterpretiveOverlayPlanner",
            "EvidenceDiagnosticsBuilder",
            "MainChartsEvidenceExportService",
            "ConsumerProviderRegistry",
            "ChartRenderDeliveryBinding",
            "ChartBackendSelector",
            "ChartRenderPlanProjector",
            "ChartRenderPlanAdapterDispatcher"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceAndDiagnostics_ShouldRemainObservationalNotLiveRouting()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "MainHost", "Evidence")],
            [
                "UpdateChartUsingStrategyAsync(",
                "RenderAsync(",
                ".ApplyAsync(",
                "LoadProgramAsync(",
                "ChartRenderDeliveryBinding.Resolve",
                "ConsumerProviderRegistry",
                "ChartBackendSelector",
                "SetRenderPlanDiagnostics",
                "LastContext =",
                "LastLoadRuntime =",
                "IsMainVisible =",
                "IsNormalizedVisible =",
                "IsDiffRatioVisible =",
                "IsDistributionVisible ="
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void EvidenceExport_ShouldKeepFileSystemWritesOnDedicatedExportWriter()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "MainHost", "Evidence")],
            [
                "File.WriteAllText(",
                "Directory.CreateDirectory(",
                "JsonSerializer.Serialize("
            ]);

        AssertNoMatches(offenders);

        var writerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser",
            "UI",
            "MainHost",
            "Export",
            "ReachabilityExportWriter.cs");
        Assert.Contains("Directory.CreateDirectory", writerSource, StringComparison.Ordinal);
        Assert.Contains("File.WriteAllText", writerSource, StringComparison.Ordinal);
        Assert.Contains("JsonSerializer.Serialize", writerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ParityAndReachabilityEvidence_ShouldNotMutateLiveChartState()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "UI", "MainHost", "Evidence"),
                Path.Combine("DataVisualiser", "Core", "Validation", "Parity"),
                Path.Combine("DataVisualiser", "Core", "Strategies", "Reachability")
            ],
            [
                "UpdateChartUsingStrategyAsync(",
                "RenderAsync(",
                ".ApplyAsync(",
                "SetRenderPlanDiagnostics",
                "LastContext =",
                "LastLoadRuntime =",
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void EvidenceDiagnosticsBuilder_ShouldRemainObservationalAndNotDriveLiveRendering()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Evidence", "EvidenceDiagnosticsBuilder.cs");
        var forbiddenTokens = new[]
        {
            "UpdateChartUsingStrategyAsync(",
            "RenderAsync(",
            "ApplyAsync(",
            "LoadProgramAsync(",
            "ExecuteAsync(",
            "ChartRenderDeliveryBinding.Resolve",
            "ConsumerProviderRegistry.BuiltIn.Resolve"
        };

        foreach (var token in forbiddenTokens)
            Assert.DoesNotContain(token, source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsEvidenceExportService_ShouldDelegateParityAssemblyToDedicatedBuilder()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Evidence", "MainChartsEvidenceExportService.cs");

        Assert.Contains("EvidenceParityBuilder", source, StringComparison.Ordinal);
        Assert.Contains("_parityBuilder.BuildAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildTransformParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildMultiMetricParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteParitySafe(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceParityBuilder_ShouldDelegateTransformParityToDedicatedEvaluator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Evidence", "EvidenceParityBuilder.cs");

        Assert.Contains("EvidenceDistributionParityEvaluator", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceMultiMetricParityEvaluator", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityEvaluator", source, StringComparison.Ordinal);
        Assert.Contains("_distributionParityEvaluator.BuildAsync", source, StringComparison.Ordinal);
        Assert.Contains("_multiMetricParityEvaluator.BuildAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformParityEvaluator.BuildAsync", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceParitySummaryBuilder.BuildSummary", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceParitySummaryBuilder.BuildWarnings", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceStrategyParityExecutor.ExecuteSafe", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeBinaryTransformParity(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeUnaryTransformParity(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<DistributionParitySnapshot> BuildDistributionParitySnapshotAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<SimpleParitySnapshot> BuildMultiMetricParitySnapshotAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static ParitySummarySnapshot BuildParitySummary(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static IReadOnlyList<string> BuildParityWarnings(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private ParityResultSnapshot ExecuteParitySafe(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidenceTransformParityEvaluator_ShouldDelegateResolutionAndComputationToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Evidence", "EvidenceTransformParityEvaluator.cs");

        Assert.Contains("EvidenceTransformParityDataResolver", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityComputer", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityDataResolver.ResolveSelections", source, StringComparison.Ordinal);
        Assert.Contains("_dataResolver.ResolveAsync", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityComputer.IsUnaryTransform", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityComputer.ComputeUnary", source, StringComparison.Ordinal);
        Assert.Contains("EvidenceTransformParityComputer.ComputeBinary", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static bool IsUnaryTransform(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<IReadOnlyList<MetricData>?> ResolveTransformParityDataAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeUnaryTransformParity", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static (ParityResultSnapshot Result, int LegacySamples, int NewSamples, bool ExpressionAvailable) ComputeBinaryTransformParity", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldNotReacquireParityAssemblyOrTransformParityHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.DoesNotContain("BuildParitySummary(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildTransformParitySnapshotAsync(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformExpressionBuilder", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformExpressionEvaluator", source, StringComparison.Ordinal);
        Assert.DoesNotContain("UnaryOperators", source, StringComparison.Ordinal);
        Assert.DoesNotContain("BinaryOperators", source, StringComparison.Ordinal);
        Assert.DoesNotContain("JsonSerializer", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldRemainComposedThroughKnownHostSeams()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewThemeCoordinator", source);
        Assert.Contains("MainChartsEvidenceExportService", source);
        Assert.Contains("MainChartsViewEvidenceExportCoordinator", source);
        Assert.Contains("MainChartsViewDataLoadedCoordinator", source);
        Assert.Contains("MainChartsViewChartUpdateCoordinator", source);
        Assert.Contains("MainChartsViewChartPresentationCoordinator", source);
        Assert.Contains("MainChartsViewChartPipelineFactory", source);
    }

    [Fact]
    public void MainChartsView_ShouldKeepThemeOnDedicatedCoordinatorSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewThemeCoordinator", source);
        Assert.DoesNotContain("ThemeChanged +=", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeChanged -=", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_OnDataLoaded_ShouldDelegateThroughDedicatedCoordinatorSeam()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var onDataLoadedBody = ExtractMethodBody(source, "private async void OnDataLoaded");

        Assert.Contains("_dataLoadedCoordinator.HandleAsync", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("CompleteTransformSelectionsPendingLoad();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateSubtypeOptions(ChartControllerKeys.Normalized);", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateTransformSubtypeOptions();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateTransformComputeButtonState();", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await RenderChartAsync(ChartControllerKeys.BarPie, ctx);", onDataLoadedBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await RenderChartsFromLastContext();", onDataLoadedBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_UpdateSelectedSubtypes_ShouldRefreshTransformAlongsidePeerCharts()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void UpdateSelectedSubtypesInViewModel");

        Assert.Contains("_selectionCoordinator.UpdateSelectedSubtypes", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateTransformSubtypeOptions();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain(".GroupBy(series => series.DisplayKey", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_HasLoadedData_ShouldUseSharedSelectionCompatibilityGuard()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private bool HasLoadedData");

        Assert.Contains("ChartContextSelectionGuard.IsCompatibleWithCurrentSelection", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_OnMetricTypeSelectionChanged_ShouldClearSubtypeControlsBeforeLoadingNewSubtypes()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void OnMetricTypeSelectionChanged");
        var actionFactoryBody = ExtractMethodBody(source, "private ChartHostMetricSelectionCoordinator.MetricTypeSelectionChangedActions CreateMetricTypeSelectionChangedActions");

        Assert.Contains("_metricSelectionCoordinator.HandleMetricTypeSelectionChanged", methodBody, StringComparison.Ordinal);
        Assert.Contains("UpdateChartTitlesFromSelections();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ClearAllCharts();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("_viewModel.LoadSubtypesCommand.Execute(null);", methodBody, StringComparison.Ordinal);

        Assert.True(
            actionFactoryBody.IndexOf("ClearAllCharts();", StringComparison.Ordinal) <
            actionFactoryBody.IndexOf("_viewModel.SetSelectedMetricType", StringComparison.Ordinal),
            "Metric-type changes should clear the loaded chart context before new subtype state is committed.");

        Assert.True(
            actionFactoryBody.IndexOf("_viewModel.SetSelectedMetricType", StringComparison.Ordinal) <
            actionFactoryBody.IndexOf("_selectorManager.ClearAllSubtypeControls()", StringComparison.Ordinal),
            "Metric type should be committed before subtype controls are cleared so selection sync cannot snap the combo back.");
    }

    [Fact]
    public void MainChartsView_OnSubtypesLoaded_ShouldNotPreserveOnlyTheLastDynamicComboOnMetricTypeChange()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void OnSubtypesLoaded");
        var actionFactoryBody = ExtractMethodBody(source, "private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions");

        Assert.DoesNotContain("UpdateLastDynamicComboItems", methodBody, StringComparison.Ordinal);
        Assert.Contains("_metricSelectionCoordinator.HandleSubtypesLoaded", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("_selectorManager.SuppressSelectionChanged()", methodBody, StringComparison.Ordinal);
        Assert.Contains("_selectorManager.SuppressSelectionChanged", actionFactoryBody, StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch", actionFactoryBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_OnAnySubtypeSelectionChanged_ShouldNotClearLoadedChartsJustBecauseSelectionChanged()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private async void OnAnySubtypeSelectionChanged");

        Assert.Contains("UpdateSelectedSubtypesInViewModel();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ClearAllCharts();", methodBody, StringComparison.Ordinal);
        Assert.Contains("_selectionCoordinator.HandleSubtypeSelectionChangedAsync", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("if (HasLoadedData())", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await RenderChartsFromLastContext();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("await LoadDateRangeForSelectedMetrics();", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ToggleEnablement_ShouldUseLoadedContextCapabilities_NotSelectionCompatibility()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var primaryMethodBody = ExtractMethodBody(source, "private void UpdatePrimaryDataRequiredButtonStates");
        var secondaryMethodBody = ExtractMethodBody(source, "private void UpdateSecondaryDataRequiredButtonStates");

        Assert.Contains("MainChartsViewToggleStateCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_toggleStateCoordinator.UpdatePrimaryChartToggles", primaryMethodBody, StringComparison.Ordinal);
        Assert.Contains("_toggleStateCoordinator.UpdateSecondaryChartToggles", secondaryMethodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("HasLoadedData()", primaryMethodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("HasLoadedData()", secondaryMethodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateCmsToggleFlowThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewCmsToggleCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_cmsToggleCoordinator.SyncStates", ExtractMethodBody(source, "private void SyncCmsToggleStates"), StringComparison.Ordinal);
        Assert.Contains("_cmsToggleCoordinator.HandleCmsToggleChangedAsync", ExtractMethodBody(source, "private async void OnCmsToggleChanged"), StringComparison.Ordinal);
        Assert.Contains("_cmsToggleCoordinator.HandleStrategyToggleChangedAsync", ExtractMethodBody(source, "private async void OnCmsStrategyToggled"), StringComparison.Ordinal);
        Assert.DoesNotContain("CmsConfiguration.UseCmsData =", ExtractMethodBody(source, "private async void OnCmsToggleChanged"), StringComparison.Ordinal);
        Assert.DoesNotContain("CmsConfiguration.UseCmsForSingleMetric =", ExtractMethodBody(source, "private async void OnCmsStrategyToggled"), StringComparison.Ordinal);
    }

    [Fact]
    public void Hosts_ShouldReuseSharedDateRangeCoordinator()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.Contains("ChartHostDateRangeCoordinator", mainSource, StringComparison.Ordinal);
        Assert.Contains("_dateRangeCoordinator.ApplyDefaultRange", ExtractMethodBody(mainSource, "private void InitializeDateRange"), StringComparison.Ordinal);
        Assert.Contains("_dateRangeCoordinator.ApplyDefaultRange", ExtractMethodBody(mainSource, "private void ResetDateRangeToDefault"), StringComparison.Ordinal);

        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        Assert.Contains("ChartHostDateRangeCoordinator", syncfusionSource, StringComparison.Ordinal);
        Assert.Contains("_dateRangeCoordinator.ApplyDefaultRange", ExtractMethodBody(syncfusionSource, "private void InitializeDateRange"), StringComparison.Ordinal);
        Assert.Contains("_dateRangeCoordinator.ApplyDefaultRange", ExtractMethodBody(syncfusionSource, "private void ResetDateRangeToDefault"), StringComparison.Ordinal);
    }

    [Fact]
    public void ChartAdapters_ShouldUseSharedTimeBucketAggregationHelper()
    {
        var barPieBuilderSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "BarPieRenderModelBuilder.cs");
        var syncfusionBuilderSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "SyncfusionSunburstRenderModelBuilder.cs");

        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", barPieBuilderSource, StringComparison.Ordinal);
        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", syncfusionBuilderSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static double?[] BuildBucketTotals", barPieBuilderSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static double?[] BuildBucketTotals", syncfusionBuilderSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static int ResolveBucketIndex", barPieBuilderSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static int ResolveBucketIndex", syncfusionBuilderSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BarPieAdapter_ShouldDelegateModelPlanningToDedicatedBuilder()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "Charts", "Presentation", "BarPieChartControllerAdapter.cs");
        var builderSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "Charts", "Presentation", "BarPieRenderModelBuilder.cs");

        Assert.Contains("BarPieRenderModelBuilder", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_renderModelBuilder.BuildAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<UiChartRenderModel> BuildBarPieRenderModelAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private bool TryResolveBarPieDateRange", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private int ResolveBarPieBucketCount", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static BarPieBucketPlan BuildBarPieBucketPlan", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<IReadOnlyList<BarPieSeriesTotals>> LoadBarPieSeriesTotalsAsync", adapterSource, StringComparison.Ordinal);

        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", builderSource, StringComparison.Ordinal);
        Assert.Contains("LoadSeriesTotalsAsync", builderSource, StringComparison.Ordinal);
        Assert.Contains("BuildBucketPlan", builderSource, StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionSunburstAdapter_ShouldDelegateModelBuildingToBuilder()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "SyncfusionSunburstChartControllerAdapter.cs");
        var builderSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "SyncfusionSunburstRenderModelBuilder.cs");

        Assert.Contains("SyncfusionSunburstRenderModelBuilder", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_renderModelBuilder.BuildAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadMetricDataAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadSeriesTotalsAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildBucketPlan", adapterSource, StringComparison.Ordinal);

        Assert.Contains("LoadSeriesTotalsAsync", builderSource, StringComparison.Ordinal);
        Assert.Contains("BuildBucketPlan", builderSource, StringComparison.Ordinal);
        Assert.Contains("TimeBucketAggregationHelper.BuildAverageTotals", builderSource, StringComparison.Ordinal);
    }

    [Fact]
    public void DistributionAdapter_ShouldDelegateDataPreparationToBuilder()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "DistributionChartControllerAdapter.cs");
        var builderSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "DistributionRenderInputBuilder.cs");

        Assert.Contains("DistributionRenderInputBuilder", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_renderInputBuilder.BuildAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadMetricDataAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VNextDataResolutionHelper.ResolveSeriesDataAsync", adapterSource, StringComparison.Ordinal);

        Assert.Contains("VNextDataResolutionHelper.ResolveSeriesDataAsync", builderSource, StringComparison.Ordinal);
        Assert.Contains("LoadMetricDataWithCmsAsync", builderSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WeekdayTrendAdapter_ShouldDelegateComputationToInvoker()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "WeekdayTrendChartControllerAdapter.cs");
        var invokerSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "WeekdayTrendComputationInvoker.cs");

        Assert.Contains("WeekdayTrendComputationInvoker", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_computationInvoker.ComputeAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateStrategy", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadMetricDataAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VNextDataResolutionHelper", adapterSource, StringComparison.Ordinal);

        Assert.Contains("CreateStrategy", invokerSource, StringComparison.Ordinal);
        Assert.Contains("VNextDataResolutionHelper.ResolveSeriesDataAsync", invokerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartAdapter_ShouldDelegateOverlayBuildingToBuilder()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "MainChartControllerAdapter.cs");
        var builderSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "CartesianMetricOverlaySeriesBuilder.cs");

        Assert.Contains("CartesianMetricOverlaySeriesBuilder", adapterSource, StringComparison.Ordinal);
        Assert.Contains("_overlayBuilder.BuildAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LoadMetricDataAsync", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SmoothingService", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BuildOverlaySeriesAsync", adapterSource, StringComparison.Ordinal);

        Assert.Contains("LoadMetricDataAsync", builderSource, StringComparison.Ordinal);
        Assert.Contains("SmoothingService", builderSource, StringComparison.Ordinal);
        Assert.Contains("IsMatchingSelection", builderSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformExecutionCoordinator_ShouldDelegateTransformComputationToSharedService()
    {
        var workflowSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformWorkflowCoordinator.cs");
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformOperationExecutionCoordinator.cs");

        Assert.Contains("_transformOperationExecutionCoordinator.Execute", workflowSource, StringComparison.Ordinal);
        Assert.Contains("_transformComputationService.ComputeUnaryTransform", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("_transformComputationService.ComputeBinaryTransform", coordinatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("PrepareMetricData", workflowSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeUnaryTransform(", workflowSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ComputeBinaryTransform(", workflowSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformAdapter_ShouldDelegateSelectionResolutionAndExecutionToDedicatedCoordinators()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformDataPanelControllerAdapter.cs");
        var presentationDirectory = SourceTreeTestHelper.GetRepositoryPath("DataVisualiser", "UI", "Charts", "Presentation");
        var presentationSources = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(presentationDirectory, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText));

        Assert.Contains("TransformDataResolutionCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformOperationExecutionCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformOperationStateCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformRenderCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformSelectionInteractionCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("TransformSessionMilestoneRecorder", source, StringComparison.Ordinal);
        Assert.Contains("TransformWorkflowCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_transformDataResolutionCoordinator.ResolveSelections", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationStateCoordinator.UpdateComputeButtonState", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationStateCoordinator.GetSelectedOperationTag", source, StringComparison.Ordinal);
        Assert.Contains("_transformRenderCoordinator.PopulateInputGrids", source, StringComparison.Ordinal);
        Assert.Contains("_transformSessionMilestoneRecorder.RecordToggle", source, StringComparison.Ordinal);
        Assert.Contains("_transformWorkflowCoordinator.ExecuteOperationAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformWorkflowCoordinator.RenderPrimarySelectionAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformWorkflowCoordinator.RefreshFromSelectionAsync", source, StringComparison.Ordinal);
        Assert.Contains("TransformSubtypeSelectionCoordinator.ApplySubtypeOptions", source, StringComparison.Ordinal);
        Assert.Contains("TransformSubtypeSelectionCoordinator.ResetSelectionControls", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformGridPresentationCoordinator.PopulateInputGrids", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformGridPresentationCoordinator.PopulateResultGrid", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformChartPresentationCoordinator.RenderResultsAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private bool CanUpdateTransformSubtypeOptions()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ResetTransformSelectionControls()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void UpdatePrimaryTransformSubtype(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void UpdateSecondaryTransformSubtype(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void PopulateTransformResultGrid(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task RenderTransformChart(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private async Task<IReadOnlyList<MetricData>?> ResolveTransformDataAsync", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private MetricSeriesSelection? ResolveSelectedTransformPrimarySeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private MetricSeriesSelection? ResolveSelectedTransformSecondarySeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static ChartDataContext BuildTransformContext", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private TransformChartRenderHost CreateRenderHost", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ResetTransformAuxiliaryVisuals", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordTransformMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordTransformToggleMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private bool TryGetSelectedOperation", source, StringComparison.Ordinal);
        Assert.Contains("ITransformLayoutCapabilities", presentationSources, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformDataPanelControllerV2", presentationSources, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformAdapter_ShouldDelegateCoordinatorConstructionToCompositionFactory()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformDataPanelControllerAdapter.cs");
        var factorySource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformDataPanelControllerAdapterCompositionFactory.cs");

        Assert.Contains("TransformDataPanelControllerAdapterCompositionFactory.Create", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformDataResolutionCoordinator", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformOperationExecutionCoordinator", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformOperationStateCoordinator", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformRenderCoordinator", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformSelectionInteractionCoordinator", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformSessionMilestoneRecorder", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new TransformWorkflowCoordinator", adapterSource, StringComparison.Ordinal);

        Assert.Contains("new TransformDataResolutionCoordinator", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformOperationExecutionCoordinator", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformOperationStateCoordinator", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformRenderCoordinator", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformSelectionInteractionCoordinator", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformSessionMilestoneRecorder", factorySource, StringComparison.Ordinal);
        Assert.Contains("new TransformWorkflowCoordinator", factorySource, StringComparison.Ordinal);
        Assert.DoesNotContain("ConsumerProviderRegistry", factorySource, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartBackendSelector", factorySource, StringComparison.Ordinal);
        Assert.DoesNotContain("AnalyticalIntentFactory", factorySource, StringComparison.Ordinal);
    }

    [Fact]
    public void StrategyCutOverService_ShouldDelegateCmsDecisionLogicToEvaluator()
    {
        var serviceSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Strategies", "StrategyCutOverService.cs");
        var evaluatorSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Strategies", "Reachability", "StrategyCmsDecisionEvaluator.cs");

        Assert.Contains("StrategyCmsDecisionEvaluator", serviceSource, StringComparison.Ordinal);
        Assert.Contains("_cmsDecisionEvaluator.Evaluate", serviceSource, StringComparison.Ordinal);
        Assert.Contains("CreateMultiMetricDecision", evaluatorSource, StringComparison.Ordinal);
        Assert.Contains("SupportsRealCmsStrategy", evaluatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private StrategyCmsDecision EvaluateCmsDecision", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static bool HasSufficientCmsSamples", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static bool SupportsRealCmsStrategy", serviceSource, StringComparison.Ordinal);
    }

    [Fact]
    public void StrategyCutOverService_ShouldDelegateParityValidationToDedicatedService()
    {
        var serviceSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Strategies", "StrategyCutOverService.cs");
        var validationSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "Core", "Strategies", "StrategyParityValidationService.cs");

        Assert.Contains("StrategyParityValidationService", serviceSource, StringComparison.Ordinal);
        Assert.Contains("_parityValidationService.ValidateParity", serviceSource, StringComparison.Ordinal);
        Assert.Contains("GetParityHarness", validationSource, StringComparison.Ordinal);
        Assert.Contains("PerformBasicValidation", validationSource, StringComparison.Ordinal);
        Assert.Contains("ConvertParityResult", validationSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private StrategyType? DetermineStrategyType", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private IStrategyParityHarness? GetParityHarness", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private ParityResult PerformBasicValidation", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private ParityResult ConvertParityResult", serviceSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformWorkflowCoordinator_ShouldOwnExecutionRefreshAndResultRenderSequencing()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformWorkflowCoordinator.cs");

        Assert.Contains("_transformDataResolutionCoordinator.ResolveAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformOperationExecutionCoordinator.Execute", source, StringComparison.Ordinal);
        Assert.Contains("_transformRenderCoordinator.RenderResultsAsync", source, StringComparison.Ordinal);
        Assert.Contains("_transformSessionMilestoneRecorder.RecordExecution", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BaseDistributionService_ShouldDelegateAxisAndSummaryFormattingToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Services", "BaseDistributionService.cs");

        Assert.Contains("DistributionAxisCoordinator.ConfigureYAxis", source, StringComparison.Ordinal);
        Assert.Contains("DistributionAxisCoordinator.ConfigureXAxis", source, StringComparison.Ordinal);
        Assert.Contains("DistributionDebugSummaryLogger.LogSummary", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected static void ConfigureYAxis(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void ConfigureXAxis(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void LogSummary(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformOperationStateCoordinator_ShouldOwnComputeButtonExecutionEligibility()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Presentation", "TransformOperationStateCoordinator.cs");

        Assert.Contains("TransformDataResolutionCoordinator.CanRenderPrimarySelection", source, StringComparison.Ordinal);
        Assert.Contains("executionCoordinator.CanExecute", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Hosts_ShouldBatchProgrammaticMetricAndSubtypeSelectionMutations()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.Contains("ChartHostMetricSelectionCoordinator", mainSource, StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch", ExtractMethodBody(mainSource, "private ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions CreateMetricTypesLoadedActions"), StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch", ExtractMethodBody(mainSource, "private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ValidateAndPrepareLoad", ExtractMethodBody(mainSource, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        var mainAddSubtypeBody = ExtractMethodBody(mainSource, "private async void AddSubtypeComboBox");
        Assert.Contains("_selectorManager.SuppressSelectionChanged()", mainAddSubtypeBody, StringComparison.Ordinal);
        Assert.Contains("RenderChartsFromLastContext", mainAddSubtypeBody, StringComparison.Ordinal);

        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        Assert.Contains("ChartHostMetricSelectionCoordinator", syncfusionSource, StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch", ExtractMethodBody(syncfusionSource, "private ChartHostMetricSelectionCoordinator.MetricTypesLoadedActions CreateMetricTypesLoadedActions"), StringComparison.Ordinal);
        Assert.Contains("_viewModel.BeginSelectionStateBatch", ExtractMethodBody(syncfusionSource, "private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ValidateAndPrepareLoad", ExtractMethodBody(syncfusionSource, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        var syncfusionAddSubtypeBody = ExtractMethodBody(syncfusionSource, "private async void AddSubtypeComboBox");
        Assert.Contains("_selectorManager.SuppressSelectionChanged()", syncfusionAddSubtypeBody, StringComparison.Ordinal);
        Assert.Contains("RenderChartAsync", syncfusionAddSubtypeBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateLoadValidationExecutionAndClearFlowThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewLoadCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ValidateAndPrepareLoad", ExtractMethodBody(source, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ExecuteLoadAsync", ExtractMethodBody(source, "private async Task LoadMetricData"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ClearSelection", ExtractMethodBody(source, "private void OnClear"), StringComparison.Ordinal);
        Assert.DoesNotContain("ChartPresentationSpine.LoadMetricDataIntoLastContextAsync", ExtractMethodBody(source, "private async Task LoadMetricData"), StringComparison.Ordinal);
        Assert.DoesNotContain("_evidenceExportCoordinator.ClearEvidence", ExtractMethodBody(source, "private void OnClear"), StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateStateProjectionToDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewStateSyncCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_stateSyncCoordinator.Apply", ExtractMethodBody(source, "private void ApplySelectionStateToUi"), StringComparison.Ordinal);
        Assert.Contains("CreateStateSyncActions", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyResolutionFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyDateRangeFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyMetricTypeFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplySubtypeSelectionsFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyBucketCountFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static void SetComboSelectionByValue", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionChartsView_ShouldReuseSharedStateProjectionCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("MainChartsViewStateSyncCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_stateSyncCoordinator.Apply", ExtractMethodBody(source, "private void ApplySelectionStateToUi"), StringComparison.Ordinal);
        Assert.Contains("CreateStateSyncActions", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyResolutionFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyDateRangeFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplyMetricTypeFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void ApplySubtypeSelectionsFromState()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static void SetComboSelectionByValue", source, StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionChartsView_ShouldDelegateLoadValidationExecutionAndClearFlowThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("SyncfusionChartsViewLoadCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ValidateAndPrepareLoad", ExtractMethodBody(source, "private Task<bool> LoadDataAndValidate"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ExecuteLoadAsync", ExtractMethodBody(source, "private async Task LoadMetricData"), StringComparison.Ordinal);
        Assert.Contains("_loadCoordinator.ClearSelection", ExtractMethodBody(source, "private void OnClear"), StringComparison.Ordinal);
        Assert.DoesNotContain("try", ExtractMethodBody(source, "private async Task LoadMetricData"), StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionChartsView_ShouldRenderSubtypeChangesFromCurrentSelection_NotOnlyLoadedContext()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        var subtypeChangedBody = ExtractMethodBody(source, "private async void OnAnySubtypeSelectionChanged");
        var addSubtypeBody = ExtractMethodBody(source, "private async void AddSubtypeComboBox");

        Assert.Contains("HasRenderableSelection()", subtypeChangedBody, StringComparison.Ordinal);
        Assert.Contains("LastContext ?? new ChartDataContext()", subtypeChangedBody, StringComparison.Ordinal);
        Assert.Contains("HasRenderableSelection()", addSubtypeBody, StringComparison.Ordinal);
        Assert.Contains("LastContext ?? new ChartDataContext()", addSubtypeBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ShouldRenderAfterSubtypeSelectionChange(_isApplyingSelectionSync, HasRenderableContext()", subtypeChangedBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldRestoreVisibleChartsFromSharedContextWhenLoaded()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var onLoadedBody = ExtractMethodBody(source, "private async void OnLoaded");
        var restoreBody = ExtractMethodBody(source, "private async Task RestoreChartsFromSharedLastContextAsync");
        var completeRestoreBody = ExtractMethodBody(source, "private async Task CompleteTabSwitchRestoreAsync");
        var visibilityBody = ExtractMethodBody(source, "private async void OnViewVisibilityChanged");

        var subtypesLoadedBody = ExtractMethodBody(source, "private void OnSubtypesLoaded");
        var subtypesLoadedActionsBody = ExtractMethodBody(source, "private ChartHostMetricSelectionCoordinator.SubtypesLoadedActions CreateSubtypesLoadedActions");

        Assert.Contains("RestoreChartsFromSharedLastContextAsync", onLoadedBody, StringComparison.Ordinal);
        Assert.Contains("ShouldRestoreChartsWhenViewLoads(_isInitializing, ctx)", restoreBody, StringComparison.Ordinal);
        // Deferred path: metric types not loaded OR saved metric type differs from what TablesCombo
        // currently shows (e.g. data was loaded under a different type in the Syncfusion tab).
        // In both cases set the flag and reload subtypes before completing the restore.
        Assert.Contains("TablesCombo.Items.Count == 0", restoreBody, StringComparison.Ordinal);
        Assert.Contains("needsSubtypeReload", restoreBody, StringComparison.Ordinal);
        Assert.Contains("_pendingTabSwitchRestore = true", restoreBody, StringComparison.Ordinal);
        Assert.Contains("LoadSubtypesCommand", restoreBody, StringComparison.Ordinal);
        Assert.Contains("CompleteTabSwitchRestoreAsync", restoreBody, StringComparison.Ordinal);
        // Direct path: full restore lives in CompleteTabSwitchRestoreAsync
        Assert.Contains("_dataLoadedCoordinator.HandleAsync", completeRestoreBody, StringComparison.Ordinal);
        Assert.Contains("RenderChartsFromLastContext()", completeRestoreBody, StringComparison.Ordinal);
        // The pending flag must be checked before the followUp branches in OnSubtypesLoaded.
        // With HasLoadedData true the coordinator returns ApplySelectionState; without the early
        // check that branch overwrites the combos with defaulted state before CompleteTabSwitchRestoreAsync runs.
        var pendingFlagIndex = subtypesLoadedBody.IndexOf("_pendingTabSwitchRestore", StringComparison.Ordinal);
        var applySelectionBranchIndex = subtypesLoadedBody.IndexOf("SubtypesFollowUp.ApplySelectionState", StringComparison.Ordinal);
        Assert.True(pendingFlagIndex >= 0, "_pendingTabSwitchRestore check missing from OnSubtypesLoaded");
        Assert.True(pendingFlagIndex < applySelectionBranchIndex, "_pendingTabSwitchRestore must be checked before ApplySelectionState branch");
        // UpdateSelectedSubtypesInViewModel must be suppressed during a pending restore so
        // HandleSubtypesLoaded cannot overwrite the saved selections before CompleteTabSwitchRestoreAsync applies them.
        Assert.Contains("_pendingTabSwitchRestore", subtypesLoadedActionsBody, StringComparison.Ordinal);
        Assert.Contains("UpdateSelectedSubtypesInViewModel", subtypesLoadedActionsBody, StringComparison.Ordinal);
        // OnLoaded only fires once (first visual-tree entry). Tab-switch back does NOT re-fire Loaded.
        // OnViewVisibilityChanged handles subsequent tab switches: bind/unbind the event binder and
        // trigger restore when the view becomes visible again after initialization completes.
        Assert.Contains("_viewModelEventBinder?.Bind()", visibilityBody, StringComparison.Ordinal);
        Assert.Contains("_viewModelEventBinder?.Unbind()", visibilityBody, StringComparison.Ordinal);
        Assert.Contains("RestoreChartsFromSharedLastContextAsync", visibilityBody, StringComparison.Ordinal);
        Assert.Contains("!_isInitializing", visibilityBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsViewAndMainHost_ShouldNotReintroduceNamedControlFallbacks()
    {
        var mainChartsViewSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.DoesNotContain("FindName(", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VisualTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LogicalTreeHelper", mainChartsViewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GetType().GetField", mainChartsViewSource, StringComparison.Ordinal);

        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "UI", "MainHost")],
            ["FindName(", "VisualTreeHelper", "LogicalTreeHelper", "GetType().GetField"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreValidation_DataLoadHelpers_ShouldNotReferenceUiEventsNamespace()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Validation", "DataLoad", "MetricDataValidationHelper.cs");

        Assert.DoesNotContain("DataVisualiser.UI.Events", source, StringComparison.Ordinal);
    }

    [Fact]
    public void DataFetcher_ShouldRemainFacadeOverFocusedQueryGroups()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Data", "Repositories", "DataFetcher.cs");

        Assert.Contains("DataFetcherMetricCatalogQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherMetricDataQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherDateRangeQueries", source, StringComparison.Ordinal);
        Assert.Contains("DataFetcherAdminQueries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SELECT ", source, StringComparison.Ordinal);
        Assert.DoesNotContain("MERGE ", source, StringComparison.Ordinal);
        Assert.DoesNotContain("QueryAsync<", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ExecuteAsync(", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BaseDistributionService_ShouldDelegatePureDistributionComputationToSharedHelper()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Services", "BaseDistributionService.cs");

        Assert.Contains("DistributionComputationHelper.GetBucketValues", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateGlobalMinMax", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateTooltipData", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateSimpleRangeTooltipData", source, StringComparison.Ordinal);
        Assert.Contains("DistributionComputationHelper.CalculateBucketAverages", source, StringComparison.Ordinal);
        Assert.Contains("DistributionRangeResultBuilder.Build", source, StringComparison.Ordinal);
        Assert.Contains("DistributionSeriesBuilder.AddBaselineAndRangeSeries", source, StringComparison.Ordinal);
        Assert.Contains("DistributionSeriesBuilder.AddAverageSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void AddBaselineAndRangeSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected void AddAverageSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected StackedColumnSeries CreateBaselineSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("protected StackedColumnSeries CreateRangeSeries", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartHelper_ShouldDelegateTooltipFormattingToDedicatedHelper()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Helpers", "ChartHelper.cs");
        var tooltipSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Tooltip", "ChartTooltipFormattingHelper.cs");

        Assert.Contains("ChartTooltipFormattingHelper.GetChartValuesAtIndex", source, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipFormattingHelper.GetChartValuesFormattedAtIndex", source, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipFormattingHelper.ParseSeriesTitle", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildStackedValuesFormattedString", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildCumulativeTooltipFromSeries", source, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipPairFormatter.Build", tooltipSource, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipStackedFormatter.Build", tooltipSource, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipCumulativeFormatter", tooltipSource, StringComparison.Ordinal);
        Assert.Contains("ChartTooltipSeriesTitleParser.Parse", tooltipSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildStackedValuesFormattedString", tooltipSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string BuildCumulativeTooltipFromSeries", tooltipSource, StringComparison.Ordinal);
    }

    [Fact]
    public void MainAndSyncfusionHosts_ShouldShareUiBusyScopeLease()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("new UiBusyScopeLease(EndUiBusyScope)", mainSource, StringComparison.Ordinal);
        Assert.Contains("new UiBusyScopeLease(EndUiBusyScope)", syncfusionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private sealed class UiBusyScope", mainSource, StringComparison.Ordinal);
        Assert.DoesNotContain("private sealed class UiBusyScope", syncfusionSource, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartRenderEngine_ShouldDelegateSeriesFormattingAndMaterializationToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Engines", "ChartRenderEngine.cs");

        Assert.Contains("ChartSeriesMaterializer.CreateAndPopulateSeries", source, StringComparison.Ordinal);
        Assert.Contains("ChartSeriesMaterializer.ResolveStackedSeriesValues", source, StringComparison.Ordinal);
        Assert.Contains("ChartSeriesLabelFormatter.FormatSeriesLabel", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static LineSeries CreateAndPopulateSeries", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static IList<double> ResolveStackedSeriesValues", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static string FormatMetricLabel", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartUpdateCoordinator_ShouldDelegateCumulativeAndYAxisPreparationToDedicatedHelpers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs");

        Assert.Contains("ChartCumulativeSeriesBuilder.Build", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.BuildSyntheticRawData", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.CollectSmoothedValues", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.EnsureOverlayExtremes", source, StringComparison.Ordinal);
        Assert.Contains("ChartYAxisDataBuilder.EnsureStackedBaseline", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private(List<SeriesResult>? RenderSeries, List<SeriesResult>? OriginalSeries) BuildCumulativeSeriesFromMulti", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private List<double> BuildStackedSmoothedValues", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static void EnsureOverlayExtremes", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateSessionDiagnosticsBookkeepingToDedicatedRecorder()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsSessionDiagnosticsRecorder", source, StringComparison.Ordinal);
        Assert.Contains("_sessionDiagnosticsRecorder.TrackHostMessage", source, StringComparison.Ordinal);
        Assert.Contains("_sessionDiagnosticsRecorder.RecordSessionMilestone", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void TrackHostMessage", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void RecordSessionMilestone", source, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateUiSurfaceDiagnosticsReadingToDedicatedReader()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private UiSurfaceDiagnosticsSnapshot CaptureEvidenceExportUiSurfaceDiagnostics");

        Assert.Contains("MainChartsUiSurfaceDiagnosticsReader", source, StringComparison.Ordinal);
        Assert.Contains("_uiSurfaceDiagnosticsReader.Capture", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("TransformUiDiagnosticsSnapshot", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("SubtypeComboDiagnosticsSnapshot", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateZoomResetThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var methodBody = ExtractMethodBody(source, "private void ResetRegisteredChartsZoom");

        Assert.Contains("MainChartsViewZoomResetCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_zoomResetCoordinator.ResetRegisteredCharts", methodBody, StringComparison.Ordinal);
        Assert.Contains("ZoomResetRequested", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("controller.ResetZoom();", methodBody, StringComparison.Ordinal);
        Assert.DoesNotContain("ResolveController(key).ResetZoom();", methodBody, StringComparison.Ordinal);
    }

    [Fact]
    public void ChartHosts_ShouldRecordThemeAndZoomSmokeMilestones()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");
        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("ThemeToggled", ExtractMethodBody(mainSource, "private void OnToggleTheme"), StringComparison.Ordinal);
        Assert.Contains("ZoomResetRequested", ExtractMethodBody(mainSource, "private void ResetRegisteredChartsZoom"), StringComparison.Ordinal);
        Assert.Contains("ThemeToggled", ExtractMethodBody(syncfusionSource, "private void OnToggleTheme"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionZoomResetRequested", ExtractMethodBody(syncfusionSource, "private void OnResetZoom"), StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionHost_ShouldRecordLoadRenderAndExportSmokeMilestones()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("SyncfusionLoadRequested", ExtractMethodBody(source, "private async void OnLoadData"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionLoadValidationFailed", ExtractMethodBody(source, "private async void OnLoadData"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionDataLoaded", ExtractMethodBody(source, "private async void OnDataLoaded"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionRenderCompleted", ExtractMethodBody(source, "private async Task RenderChartAsync"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionRenderFailed", ExtractMethodBody(source, "private async Task RenderChartAsync"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionExportRequested", ExtractMethodBody(source, "private async void OnExportReachability"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionExportCompleted", ExtractMethodBody(source, "private async void OnExportReachability"), StringComparison.Ordinal);
        Assert.Contains("SyncfusionExportFailed", ExtractMethodBody(source, "private async void OnExportReachability"), StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateControllerExtrasInteractionThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewControllerExtrasCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.InitializeBarPieControls", ExtractMethodBody(source, "private void InitializeBarPieControlsFromRegistry"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.InitializeDistributionControls", ExtractMethodBody(source, "private void InitializeDistributionControlsFromRegistry"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.InitializeWeekdayTrendControls", ExtractMethodBody(source, "private void InitializeWeekdayTrendControls"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.CompleteTransformSelectionsPendingLoad", ExtractMethodBody(source, "private void CompleteTransformSelectionsPendingLoad"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.UpdateTransformComputeButtonState", ExtractMethodBody(source, "private void UpdateTransformComputeButtonState"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.UpdateDiffRatioOperationButton", ExtractMethodBody(source, "private void UpdateDiffRatioOperationButton"), StringComparison.Ordinal);
        Assert.Contains("_controllerExtrasCoordinator.SyncMainDisplayModeSelection", ExtractMethodBody(source, "private void SyncMainDisplayModeSelection"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IBarPieChartControllerExtras ", ExtractMethodBody(source, "private void InitializeBarPieControlsFromRegistry"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IDistributionChartControllerExtras ", ExtractMethodBody(source, "private void InitializeDistributionControlsFromRegistry"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IWeekdayTrendChartControllerExtras ", ExtractMethodBody(source, "private void InitializeWeekdayTrendControls"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IDistributionChartControllerExtras ", ExtractMethodBody(source, "private void UpdateDistributionChartTypeVisibility"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IWeekdayTrendChartControllerExtras ", ExtractMethodBody(source, "private void UpdateWeekdayTrendChartTypeVisibility"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is ITransformPanelControllerExtras ", ExtractMethodBody(source, "private void CompleteTransformSelectionsPendingLoad"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is ITransformPanelControllerExtras ", ExtractMethodBody(source, "private void ResetTransformSelectionsPendingLoad"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is ITransformPanelControllerExtras ", ExtractMethodBody(source, "private void HandleTransformVisibilityOnlyToggle"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is ITransformPanelControllerExtras ", ExtractMethodBody(source, "private void UpdateTransformSubtypeOptions"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is ITransformPanelControllerExtras ", ExtractMethodBody(source, "private void UpdateTransformComputeButtonState"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IDiffRatioChartControllerExtras ", ExtractMethodBody(source, "private void UpdateDiffRatioOperationButton"), StringComparison.Ordinal);
        Assert.DoesNotContain(" is IMainChartControllerExtras ", ExtractMethodBody(source, "private void SyncMainDisplayModeSelection"), StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateRegistryWideControllerOperationsThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewRegistryCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_registryCoordinator.ClearRegisteredCharts", ExtractMethodBody(source, "private void ClearRegisteredCharts"), StringComparison.Ordinal);
        Assert.Contains("_registryCoordinator.ResolveControllers", ExtractMethodBody(source, "private void ResetRegisteredChartsZoom"), StringComparison.Ordinal);
        Assert.DoesNotContain("foreach (var key in ChartControllerKeys.All)", ExtractMethodBody(source, "private void ClearRegisteredCharts"), StringComparison.Ordinal);
        Assert.DoesNotContain("foreach (var controller in _chartControllerRegistry.All())", ExtractMethodBody(source, "private void ClearRegisteredCharts"), StringComparison.Ordinal);
        Assert.DoesNotContain("ChartControllerKeys.All.Select(ResolveController)", ExtractMethodBody(source, "private void ResetRegisteredChartsZoom"), StringComparison.Ordinal);
    }

    [Fact]
    public void MainChartsView_ShouldDelegateSurfaceStartupAndNoDataPresentationThroughDedicatedCoordinator()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainChartsView.xaml.cs");

        Assert.Contains("MainChartsViewSurfaceCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_surfaceCoordinator.InitializeSurfaces", ExtractMethodBody(source, "private void InitializeCharts"), StringComparison.Ordinal);
        Assert.DoesNotContain("private void InitializeDistributionPolarTooltip()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void InitializeDistributionChartBehavior(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void DisableAxisLabelsWhenNoData()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private static void DisableAxisLabels(", source, StringComparison.Ordinal);
        Assert.DoesNotContain("private void SetDefaultChartTitles()", source, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextBridge_ShouldAcceptExplicitProgramRequests()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "MainHost", "VNextMainChartIntegrationCoordinator.cs");

        Assert.Contains("LoadProgramAsync(", source, StringComparison.Ordinal);
        Assert.Contains("ChartProgramRequest", source, StringComparison.Ordinal);
        Assert.Contains("AnalyticalIntentFactory", source, StringComparison.Ordinal);
        Assert.Contains("_intentFactory.Create", source, StringComparison.Ordinal);
        Assert.Contains("coordinator.ExecuteAsync(intent", source, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextWorkflowState_ShouldUseExplicitWorkflowPlanRequest()
    {
        var stateSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "VNext", "State", "WorkflowState.cs");
        var transitionSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "VNext", "State", "ReasoningSessionTransitions.cs");

        Assert.Contains("WorkflowPlanRequest", stateSource, StringComparison.Ordinal);
        Assert.Contains("WorkflowPlanRequest", transitionSource, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextRenderPlans_ShouldRemainBackendNeutral()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "VNext", "Rendering")],
            ["using LiveCharts", "using Syncfusion", "using System.Windows"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void VNextContracts_ShouldRemainUiAndBackendNeutral()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "VNext", "Contracts")],
            ["using LiveCharts", "using Syncfusion", "using System.Windows", "DataVisualiser.UI"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void VNextUiConsumptionContract_ShouldRemainVendorAndUiNeutral()
    {
        var sources = new[]
        {
            SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Contracts", "VNextUiConsumptionContract.cs"),
            SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Contracts", "ConsumerSurfaceModel.cs")
        };

        foreach (var source in sources)
        {
            Assert.DoesNotContain("using LiveCharts", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using Syncfusion", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using System.Windows", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DataVisualiser.UI", source, StringComparison.Ordinal);
            Assert.DoesNotContain("IChartController", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ChartPanelController", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void OperationChainWorkbench_ShouldKeepExecutionOutsideUiSurface()
    {
        var viewSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "OperationChain", "OperationChainWorkbenchView.xaml.cs");
        var mainWindowSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "MainWindow.xaml");

        Assert.Contains("OperationChainWorkbenchView", mainWindowSource, StringComparison.Ordinal);
        Assert.Contains("DisplayResult(OperationChainResult result)", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("OperationChainExecutor", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartDataContext", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LiveCharts", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Syncfusion", viewSource, StringComparison.Ordinal);
        Assert.DoesNotContain("IReasoningEngine", viewSource, StringComparison.Ordinal);
    }

    [Fact]
    public void OperationChainCore_ShouldStayUiAndVendorNeutral()
    {
        var sources = new[]
        {
            SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Application", "OperationChainExecutor.cs"),
            SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "VNext", "Contracts", "OperationChainContracts.cs")
        };

        foreach (var source in sources)
        {
            Assert.DoesNotContain("using LiveCharts", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using Syncfusion", source, StringComparison.Ordinal);
            Assert.DoesNotContain("using System.Windows", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DataVisualiser.UI", source, StringComparison.Ordinal);
            Assert.DoesNotContain("ChartDataContext", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void DistributionFamilyBridgeRetirement_ShouldRemoveLegacyOrchestratorFallback()
    {
        var adapterSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "UI", "Charts", "Presentation", "DistributionChartControllerAdapter.cs");
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "Distribution", "DistributionRenderingContract.cs");
        var retirementSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_First_Family_Legacy_Bridge_Retirement.md");

        Assert.DoesNotContain("UseVNextNativeConsumption", adapterSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartRenderingOrchestrator", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("RenderDistributionChartAsync(request.RenderingContext", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("if (!request.UseVNextNativeConsumption)", contractSource, StringComparison.Ordinal);
        Assert.Contains("DistributionVNextConsumptionContractBuilder.Build", contractSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", contractSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", contractSource, StringComparison.Ordinal);
        Assert.Contains("Distribution", retirementSource, StringComparison.Ordinal);
        Assert.Contains("Retired bridge path", retirementSource, StringComparison.Ordinal);
    }

    [Fact]
    public void WeekdayTrendFamilyMigration_ShouldUseVNextConsumptionContractMetadata()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "WeekdayTrend", "WeekdayTrendRenderingContract.cs");
        var trackerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_VNext_Native_Family_Migration_Tracker.md");

        Assert.Contains("WeekdayTrendVNextConsumptionContractBuilder.Build", contractSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", contractSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", contractSource, StringComparison.Ordinal);
        Assert.Contains("ConsumptionContractSignature", contractSource, StringComparison.Ordinal);
        Assert.Contains("WeekdayTrend", trackerSource, StringComparison.Ordinal);
        Assert.Contains("Migrated and smoke confirmed", trackerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BarPieFamilyMigration_ShouldUseVNextConsumptionContractMetadata()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "BarPie", "BarPieRenderingContract.cs");
        var typesSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "BarPie", "BarPieRenderingTypes.cs");
        var trackerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_VNext_Native_Family_Migration_Tracker.md");

        Assert.Contains("BarPieVNextConsumptionContractBuilder.Build", contractSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", typesSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", typesSource, StringComparison.Ordinal);
        Assert.Contains("ConsumptionContractSignature", typesSource, StringComparison.Ordinal);
        Assert.Contains("BarPie", trackerSource, StringComparison.Ordinal);
        Assert.Contains("Migrated and smoke confirmed", trackerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TransformFamilyMigration_ShouldUseVNextConsumptionContractMetadata()
    {
        var typesSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "Transform", "TransformRenderingTypes.cs");
        var invokerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "TransformChartRenderInvoker.cs");
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs");
        var trackerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_VNext_Native_Family_Migration_Tracker.md");

        Assert.Contains("TransformVNextConsumptionContractBuilder.Build", invokerSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", typesSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", typesSource, StringComparison.Ordinal);
        Assert.Contains("RenderConsumptionContractFactory", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("ChartRenderPlanConsumptionContractMetadata.Attach", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("Transform", trackerSource, StringComparison.Ordinal);
        Assert.Contains("Migrated and smoke confirmed", trackerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionSunburstFamilyMigration_ShouldUseVNextConsumptionContractMetadata()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "Syncfusion", "SyncfusionSunburstRenderingContract.cs");
        var typesSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "Syncfusion", "SyncfusionSunburstRenderingTypes.cs");
        var trackerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_VNext_Native_Family_Migration_Tracker.md");

        Assert.Contains("SyncfusionSunburstVNextConsumptionContractBuilder.Build", contractSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", typesSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", typesSource, StringComparison.Ordinal);
        Assert.Contains("ChartRenderPlanConsumptionContractMetadata.Attach", contractSource, StringComparison.Ordinal);
        Assert.Contains("SyncfusionSunburst", trackerSource, StringComparison.Ordinal);
        Assert.Contains("Migrated and smoke confirmed", trackerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void MainFamilyMigration_ShouldUseVNextConsumptionContractMetadata()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Rendering", "Contracts", "CartesianMetrics", "CartesianMetricChartRenderingContract.cs");
        var invocationSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "MainChart", "MainChartRenderInvocationStage.cs");
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs");
        var trackerSource = SourceTreeTestHelper.ReadRepositoryFile(
            "documents", "DataVisualiser_VNext_Native_Family_Migration_Tracker.md");

        Assert.Contains("CartesianMetricVNextConsumptionContractBuilder", contractSource, StringComparison.Ordinal);
        Assert.Contains("VNextUiConsumptionContract", contractSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerSurfaceModel.FromRenderPlan", contractSource, StringComparison.Ordinal);
        Assert.Contains("RenderConsumptionContractFactory", invocationSource, StringComparison.Ordinal);
        Assert.Contains("ChartRenderPlanConsumptionContractMetadata.Attach", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("Main", trackerSource, StringComparison.Ordinal);
        Assert.Contains("Migrated and smoke confirmed", trackerSource, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextAnalyticalContracts_ShouldStaySplitByConcept()
    {
        var requiredFiles = new[]
        {
            "AnalyticalIntent.cs",
            "AnalyticalIntentSet.cs",
            "AnalyticalExecutionResult.cs",
            "AnalyticalResultSet.cs",
            "ConsumerDeliveryContract.cs",
            "CapabilityRequest.cs",
            "ProvenanceDescriptor.cs"
        };

        foreach (var file in requiredFiles)
        {
            var path = SourceTreeTestHelper.GetRepositoryPath("DataVisualiser", "VNext", "Contracts", file);
            Assert.True(File.Exists(path), $"Expected split contract file '{file}' to exist.");
        }

        var analyticalIntentSource = SourceTreeTestHelper.ReadRepositoryFile(
            "DataVisualiser", "VNext", "Contracts", "AnalyticalIntent.cs");
        Assert.DoesNotContain("public sealed record AnalyticalIntentSet", analyticalIntentSource, StringComparison.Ordinal);
        Assert.DoesNotContain("public sealed record CapabilityRequest", analyticalIntentSource, StringComparison.Ordinal);
        Assert.DoesNotContain("public sealed record ConsumerDeliveryContract", analyticalIntentSource, StringComparison.Ordinal);
        Assert.DoesNotContain("public sealed record ProvenanceDescriptor", analyticalIntentSource, StringComparison.Ordinal);
    }

    [Fact]
    public void VNextApplication_ShouldRemainUiAndBackendNeutral()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "VNext", "Application")],
            ["using LiveCharts", "using Syncfusion", "using System.Windows", "DataVisualiser.UI"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreAndUi_ShouldNotOwnProviderDeliveryPolicy()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core"),
                Path.Combine("DataVisualiser", "UI")
            ],
            [
                "ConsumerProviderRegistry",
                "ChartRenderDeliveryBinding",
                "ChartBackendSelector"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreRenderingContracts_ShouldNotImportConcreteUiInteractionTypes()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts")],
            ["DataVisualiser.UI.Charts.Interaction"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreSyncfusionRendering_ShouldDependOnNeutralSunburstTarget()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "Syncfusion"),
                Path.Combine("DataVisualiser", "Core", "Rendering", "Syncfusion")
            ],
            [
                "DataVisualiser.UI.Charts.Syncfusion",
                "DataVisualiser.UI.Charts.Presentation",
                "ISyncfusionSunburstChartController",
                "new SunburstItem"
            ]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreOrchestrationAndMainHost_ShouldNotReferenceSyncfusionOrLiveChartsCore()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [
                Path.Combine("DataVisualiser", "Core", "Orchestration"),
                Path.Combine("DataVisualiser", "UI", "MainHost")
            ],
            ["Syncfusion", "LiveChartsCore"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void CoreOrchestration_ShouldNotOwnThemeOrExportLogic()
    {
        var offenders = SourceTreeTestHelper.FindForbiddenTokenMatches(
            [Path.Combine("DataVisualiser", "Core", "Orchestration")],
            ["AppThemeService", "ThemeChanged", "ReachabilityExportWriter", "JsonSerializer", "File.WriteAllText(", "Directory.CreateDirectory(", "reachability-", "documents\\"]);

        AssertNoMatches(offenders);
    }

    [Fact]
    public void GuardedRuntimeFiles_ShouldNotUseSilentMessageBoxSingletonFallbacks()
    {
        var guardedFiles = new[]
        {
            Path.Combine("DataVisualiser", "Core", "Orchestration", "ChartUpdateCoordinator.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "SecondaryCharts", "SecondaryMetricChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Orchestration", "DistributionCharts", "DistributionChartOrchestrationPipeline.cs"),
            Path.Combine("DataVisualiser", "Core", "Services", "BaseDistributionService.cs")
        };

        foreach (var relativeFile in guardedFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(relativeFile);
            Assert.DoesNotContain("MessageBoxUserNotificationService.Instance", source, StringComparison.Ordinal);
        }

        var compositionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainHost", "Coordination", "MainChartsViewChartPipelineFactory.cs");
        Assert.Contains("MessageBoxUserNotificationService.Instance", compositionSource);
    }

    [Fact]
    public void ExecutionPlan_ShouldKeepLateGeneralizationGuardrailsDocumented()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("documents", "DataVisualiser_Subsystem_Plan.md");

        Assert.Contains("2-3", source, StringComparison.Ordinal);
        Assert.Contains("do not generalize before", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BothViews_ShouldUseSharedMetricSelectionPanel()
    {
        var mainSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml.cs");
        Assert.Contains("ChartTabHost.SelectionSurface", mainSource, StringComparison.Ordinal);
        Assert.Contains("SelectionPanel.MetricTypeCombo", mainSource, StringComparison.Ordinal);
        Assert.Contains("MetricSelectionPanelEventBinder", mainSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectionPanel.LoadDataRequested +=", mainSource, StringComparison.Ordinal);

        var syncfusionSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        Assert.Contains("ChartTabHost.SelectionSurface", syncfusionSource, StringComparison.Ordinal);
        Assert.Contains("SelectionPanel.MetricTypeCombo", syncfusionSource, StringComparison.Ordinal);
        Assert.Contains("MetricSelectionPanelEventBinder", syncfusionSource, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectionPanel.LoadDataRequested +=", syncfusionSource, StringComparison.Ordinal);

        var mainXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "MainChartsView.xaml");
        Assert.Contains("ChartTabHost", mainXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MetricSelectionPanel", mainXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<DockPanel", mainXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<ScrollViewer", mainXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"TablesCombo\"", mainXaml, StringComparison.Ordinal);

        var syncfusionXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml");
        Assert.Contains("ChartTabHost", syncfusionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MetricSelectionPanel", syncfusionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<DockPanel", syncfusionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<ScrollViewer", syncfusionXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("x:Name=\"TablesCombo\"", syncfusionXaml, StringComparison.Ordinal);
    }

    [Fact]
    public void WorkspaceTabHost_ShouldRemainGenericForFutureAdminAdoption()
    {
        var workspaceSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "WorkspaceTabHost.xaml");
        var chartSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "ChartTabHost.xaml");

        Assert.Contains("HeaderContent", workspaceSource, StringComparison.Ordinal);
        Assert.Contains("BodyContent", workspaceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("MetricSelectionPanel", workspaceSource, StringComparison.Ordinal);
        Assert.Contains("MetricSelectionPanel", chartSource, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminTab_ShouldNotUseMetricChartHostSpecializationYet()
    {
        var mainWindowXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "MainWindow.xaml");
        var adminXaml = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Admin", "AdminMetricsManagerView.xaml");
        var adminSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Admin", "AdminMetricsManagerView.xaml.cs");
        var coordinatorSource = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Admin", "AdminMetricsManagerCoordinator.cs");

        Assert.Contains("admin:AdminMetricsManagerView", mainWindowXaml, StringComparison.Ordinal);
        Assert.Contains("MetricTypeCombo", adminXaml, StringComparison.Ordinal);
        Assert.Contains("HideDisabledCheckBox", adminXaml, StringComparison.Ordinal);
        Assert.Contains("ReloadButton", adminXaml, StringComparison.Ordinal);
        Assert.Contains("SaveButton", adminXaml, StringComparison.Ordinal);
        Assert.Contains("WorkspaceTabHost", adminXaml, StringComparison.Ordinal);
        Assert.Contains("WorkspaceHeaderPanel", adminXaml, StringComparison.Ordinal);
        Assert.Contains("WorkspaceTabHost.HeaderContent", adminXaml, StringComparison.Ordinal);
        Assert.Contains("WorkspaceTabHost.BodyContent", adminXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ThemeTopBarBackgroundBrush", adminXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartTabHost", adminXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("MetricSelectionPanel", adminXaml, StringComparison.Ordinal);
        Assert.Contains("AdminMetricsManagerCoordinator", adminSource, StringComparison.Ordinal);
        Assert.Contains("DataFetcherAdminMetricsRepository", adminSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GetHealthMetricsCountsForAdmin", adminSource, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateHealthMetricsCountsForAdmin", adminSource, StringComparison.Ordinal);
        Assert.Contains("AdminSessionMilestoneRecorder", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordMetricTypeChanged", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordHideDisabledToggled", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordReloadRequested", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordReloadCompleted", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordGridEdited", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordSaveRequested", coordinatorSource, StringComparison.Ordinal);
        Assert.Contains("RecordSaveCompleted", coordinatorSource, StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionChartsView_ShouldBindAndUnbindViewModelEventsWithVisibility()
    {
        // Root cause of the Syncfusion→Charts tab-switch bug: SyncfusionChartsView was permanently
        // subscribed to MetricTypesLoaded/SubtypesLoaded. When Charts tab triggered LoadSubtypesCommand,
        // Syncfusion's OnSubtypesLoaded fired and called UpdateSelectedSubtypesInViewModel with
        // defaulted combo state, overwriting MetricState.SelectedSeries before CompleteTabSwitchRestoreAsync
        // could apply the saved selections. Fix: use MainChartsViewEventBinder driven by IsVisibleChanged,
        // matching the bind/unbind contract already used by MainChartsView via Loaded/Unloaded.
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        var wireBody = ExtractMethodBody(source, "private void WireViewModelEvents");
        var visibilityBody = ExtractMethodBody(source, "private async void OnViewVisibilityChanged");

        // WireViewModelEvents must create the binder, not subscribe raw events
        Assert.Contains("MainChartsViewEventBinder", wireBody, StringComparison.Ordinal);
        Assert.Contains("_viewModelEventBinder.Bind()", wireBody, StringComparison.Ordinal);
        Assert.DoesNotContain("+= OnMetricTypesLoaded", wireBody, StringComparison.Ordinal);
        Assert.DoesNotContain("+= OnSubtypesLoaded", wireBody, StringComparison.Ordinal);
        Assert.DoesNotContain("+= OnDataLoaded", wireBody, StringComparison.Ordinal);

        // OnViewVisibilityChanged must bind when visible and unbind when hidden
        Assert.Contains("_viewModelEventBinder?.Bind()", visibilityBody, StringComparison.Ordinal);
        Assert.Contains("_viewModelEventBinder?.Unbind()", visibilityBody, StringComparison.Ordinal);

        // Bind must come before Unbind (true branch first, false branch second)
        var bindIndex = visibilityBody.IndexOf("_viewModelEventBinder?.Bind()", StringComparison.Ordinal);
        var unbindIndex = visibilityBody.IndexOf("_viewModelEventBinder?.Unbind()", StringComparison.Ordinal);
        Assert.True(bindIndex < unbindIndex, "Bind() must appear before Unbind() in OnViewVisibilityChanged");
    }

    [Fact]
    public void SyncfusionChartsView_ShouldSuppressSelectionHandlersDuringResolutionReset()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");

        Assert.Contains("_isChangingResolution = true", ExtractMethodBody(source, "private void ResetForResolutionChange"), StringComparison.Ordinal);
        Assert.Contains("_isChangingResolution", ExtractMethodBody(source, "private void OnMetricTypeSelectionChanged"), StringComparison.Ordinal);
        Assert.Contains("_isChangingResolution", ExtractMethodBody(source, "private async void OnAnySubtypeSelectionChanged"), StringComparison.Ordinal);
        Assert.Contains("_isChangingResolution", ExtractMethodBody(source, "private void OnSelectionStateChanged"), StringComparison.Ordinal);
        Assert.Contains("_isChangingResolution", ExtractMethodBody(source, "private async Task RenderChartAsync"), StringComparison.Ordinal);
    }

    [Fact]
    public void SyncfusionChartsView_RenderFailures_ShouldNotEscapeAsyncVoidEventHandlers()
    {
        var source = SourceTreeTestHelper.ReadRepositoryFile("DataVisualiser", "UI", "Charts", "Syncfusion", "SyncfusionChartsView.xaml.cs");
        var renderBody = ExtractMethodBody(source, "private async Task RenderChartAsync");
        var catchIndex = renderBody.IndexOf("catch (Exception ex)", StringComparison.Ordinal);

        Assert.True(catchIndex >= 0, "RenderChartAsync should catch render exceptions.");
        Assert.DoesNotContain("throw;", renderBody[catchIndex..], StringComparison.Ordinal);
        Assert.Contains("SyncfusionRenderFailed", renderBody, StringComparison.Ordinal);
    }

    [Fact]
    public void RenderPlanAdapters_ShouldPreservePlanVocabularyMetadata()
    {
        var adapterFiles = new[]
        {
            Path.Combine("DataVisualiser", "Core", "Rendering", "Adapters", "LiveChartsRenderPlanAdapter.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Adapters", "UiChartRenderPlanAdapter.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Distribution", "DistributionRenderPlanAdapter.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "WeekdayTrend", "WeekdayTrendRenderPlanAdapter.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Syncfusion", "SyncfusionSunburstRenderPlanAdapter.cs")
        };

        foreach (var file in adapterFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            Assert.Contains("plan.Metadata", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void RenderPlanBuilders_ShouldAttachVocabularyMetadata()
    {
        var builderFiles = new[]
        {
            Path.Combine("DataVisualiser", "Core", "Rendering", "CartesianMetrics", "CartesianMetricRenderPlanBuilder.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "BarPie", "BarPieRenderingTypes.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "Distribution", "DistributionRenderingContract.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "Syncfusion", "SyncfusionSunburstRenderingTypes.cs"),
            Path.Combine("DataVisualiser", "Core", "Rendering", "Contracts", "WeekdayTrend", "WeekdayTrendRenderingContract.cs")
        };

        foreach (var file in builderFiles)
        {
            var source = SourceTreeTestHelper.ReadRepositoryFile(file);
            Assert.Contains("ChartRenderPlanVocabularyMetadata.AddTo", source, StringComparison.Ordinal);
        }
    }

    // Phase 22: MovingAverage capability enters through the VNext spine only

    [Fact]
    public void MovingAverageCapability_ShouldNotReferenceOldHubs()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            Path.Combine("DataVisualiser", "VNext", "Rendering", "MovingAverage", "MovingAverageCapabilityContract.cs"));

        Assert.DoesNotContain("ChartDataContext", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartUpdateCoordinator", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LegacyChartProgramProjector", contractSource, StringComparison.Ordinal);
        Assert.DoesNotContain("VNextMainChartIntegrationCoordinator", contractSource, StringComparison.Ordinal);
    }

    [Fact]
    public void MovingAverageCapability_ShouldExpressItselfThroughVNextSpine()
    {
        var contractSource = SourceTreeTestHelper.ReadRepositoryFile(
            Path.Combine("DataVisualiser", "VNext", "Rendering", "MovingAverage", "MovingAverageCapabilityContract.cs"));

        Assert.Contains("ChartProgramKind.MovingAverage", contractSource, StringComparison.Ordinal);
        Assert.Contains("CapabilityRequest", contractSource, StringComparison.Ordinal);
        Assert.Contains("ConsumerDeliveryContract", contractSource, StringComparison.Ordinal);
        Assert.Contains("ChartProgramDeliveryTargetResolver", contractSource, StringComparison.Ordinal);
    }

    [Fact]
    public void TabularSummaryBackend_ShouldBeIndependentOfLiveCharts()
    {
        var backendSource = SourceTreeTestHelper.ReadRepositoryFile(
            Path.Combine("DataVisualiser", "VNext", "Rendering", "ChartBackendCapabilities.cs"));

        Assert.Contains("TabularSummaryChart", backendSource, StringComparison.Ordinal);
        Assert.DoesNotContain("LiveCharts", backendSource.Substring(
            backendSource.IndexOf("TabularSummary", StringComparison.Ordinal)),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TabularSummaryConsumer_ShouldSupportOnlyMovingAverageProgramKind()
    {
        var consumerSource = SourceTreeTestHelper.ReadRepositoryFile(
            Path.Combine("DataVisualiser", "VNext", "Contracts", "ConsumerProviderContracts.cs"));

        var tabularStart = consumerSource.IndexOf("TabularSummaryChart", StringComparison.Ordinal);
        Assert.True(tabularStart >= 0, "TabularSummaryChart consumer not found.");

        var tabularBlock = consumerSource.Substring(tabularStart,
            consumerSource.IndexOf("EvidenceExport", StringComparison.Ordinal) - tabularStart);

        Assert.Contains("ChartProgramKind.MovingAverage", tabularBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartProgramKind.Main", tabularBlock, StringComparison.Ordinal);
        Assert.DoesNotContain("ChartProgramKind.Normalized", tabularBlock, StringComparison.Ordinal);
    }

    private static void AssertNoMatches(IReadOnlyList<string> offenders)
    {
        if (offenders.Count == 0)
            return;

        var builder = new StringBuilder();
        builder.AppendLine("Forbidden architectural references were found:");
        foreach (var offender in offenders)
            builder.AppendLine(offender);

        Assert.True(offenders.Count == 0, builder.ToString());
    }

    private static string ExtractMethodBody(string source, string signature)
    {
        var start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.True(start >= 0, $"Method signature '{signature}' was not found.");

        var braceStart = source.IndexOf('{', start);
        Assert.True(braceStart >= 0, $"Opening brace for '{signature}' was not found.");

        var depth = 0;
        for (var i = braceStart; i < source.Length; i++)
        {
            if (source[i] == '{')
                depth++;
            else if (source[i] == '}')
                depth--;

            if (depth == 0)
                return source.Substring(braceStart, i - braceStart + 1);
        }

        throw new InvalidOperationException($"Closing brace for '{signature}' was not found.");
    }
}
