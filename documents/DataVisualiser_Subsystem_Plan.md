# DATAVISUALISER SUBSYSTEM PLAN
**Status:** Active Architectural Execution Plan  
**Scope:** `DataVisualiser` hierarchy repair, boundary clarification, entropy reduction, subsystem consolidation, and VNext activation — **Phase 6 closed April 2026**
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `DataVisualiser-Architectural-Vocabulary.md`, `Project Roadmap.md`, and `Project Overview.md`  
**Supersedes:** `DataVisualiser_Consolidation_Plan.md` and `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md`  
**Architectural Grammar Reference:** `DataVisualiser-Architectural-Vocabulary.md` for promoted concepts, ownership containers, target hierarchy, do-not-confuse distinctions, and migration-risk language  
**Last Updated:** 2026-04-26

**Historical numbering note:** This document preserves older subsystem phase numbering. For the current DataVisualiser migration sequence, use `DataVisualiser_Migration_Plan_and_Guardrails.md` as the active sequential plan and progress log. When phase numbers differ, the migration plan's Phase 1-17 sequence is authoritative for current execution.

---

## 1. Purpose

This document is the single active execution plan for `DataVisualiser` architectural work.

It uses `DataVisualiser-Architectural-Vocabulary.md` as the canonical conceptual grammar for ownership containers, promoted concepts, and do-not-confuse distinctions. This document applies that grammar to active subsystem execution; it does not redefine it.

It consolidates:
- The completed Phase 5 architecture rehaul (historical execution record)
- The completed Phase 6 legibility-first consolidation cycle
- The completed VNext routing activation across active chart families (Phase 6.3)

The primary objectives are:

**Phase 6 (closed):** Make `DataVisualiser` legible, structurally coherent, and stable — achieved through hierarchy repair, irreducible-operation consolidation, VNext proof-of-architecture, and structural consolidation.

**Phase 7 (entry gate satisfied):** Expand the reasoning engine's capabilities — confidence-aware results, interpretive overlays, programmable composition — building each as a reasoning-engine feature that delivery surfaces consume, not as chart-specific additions.

**Phase 6.3 (closed):** VNext-compatible request/program support has been widened to all active chart families, with live VNext routes for fresh family loads and legacy retained as compatibility/fallback where still needed.

The reasoning engine is the center of the system. Charts and future consumers are delivery targets of its output.

The target direction should now be read through the enhanced architecture vocabulary: authority and provenance remain upstream; reasoning grows through capability and composition; contracts and boundaries form the downstream fan-out seam; projection/translation moves across explicit seams without becoming authority; consumers and interactions are first-class but non-authoritative; rendering remains terminal delivery infrastructure.

---

## 1.5 START HERE - Handoff and Execution Defaults

Use this section as the default handoff entry point in a new conversation.

Current state:

- Pre-Phase-7 rendering primer is complete: VNext has render-plan, density-policy, render-buffer, backend-capability, and adapter-dispatch contracts, and the active chart families now consume `ChartRenderPlan` through adapters.
- Current automated lane: 493 DataVisualiser source files, 174 DataVisualiser test files, 737 DataVisualiser tests, and 15 DataFileReader tests.
- Current architectural migration estimate: approximately 65–70% complete, working estimate ~68%, per the accepted architectural vocabulary progress snapshot.
- Phase 6.3 VNext widening is complete - all active chart families have VNext-compatible request/program support and live VNext routes for fresh family loads, with legacy retained as compatibility/fallback
- Phase 7 entry gate is satisfied — exploratory and confidence capabilities remain the Phase 7 objective, and the render-plan delivery primer is now complete across the active chart families and tabs.
- known debt: `MainChartsView` host concentration (~1,440 lines), `SyncfusionChartsView` parallel-host concentration (~859 lines), managed legacy/VNext coexistence

Current defaults:

- closed phases are audit baselines, not active workstreams; do not track new progress against Phase 5, Phase 6, Phase 6.3, or the pre-Phase-7 primer
- scope is `DataVisualiser` only
- posture is `Conservative-Pragmatic`
- one iteration must have one primary objective
- the default objective is now Phase 7 capability expansion; the pre-Phase-7 render-plan delivery primer is complete and VNext family widening (6.3) is closed
- safely coupled slices are allowed only when they already share a real contract / host / route / responsibility pattern
- validation must happen on every significant refactor
- if live behavior changes, halt after automated validation and request targeted manual smoke before continuing
- assume single-maintainer execution with limited time
- prefer slices that can be completed in `1-3` focused sessions
- do not open more than one live-behavior risk front at a time
- reliability, performance tolerance, and legibility outrank theoretical purity
- when a task crosses ownership boundaries, classify it using the architectural vocabulary before choosing an implementation home

Supporting navigation aids:

- `SYSTEM_MAP.md` Appendix A (presentation pipeline spine)
- `SYSTEM_MAP.md` Appendix B (legacy/VNext rendering pipeline comparison and render-plan transition)
- `DataVisualiser-Architectural-Vocabulary.md` for promoted concepts, ownership containers, target hierarchy, do-not-confuse distinctions, and migration-risk language

Authority order:

1. `Project Bible.md`
2. `SYSTEM_MAP.md`
3. `DataVisualiser-Architectural-Vocabulary.md`
4. `Project Roadmap.md`
5. `Project Overview.md`
6. this document

Default validation commands:

1. `dotnet build DataAnalyser.sln -c Debug`
2. `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`

Default smoke rule:

1. no manual smoke for internal-only structural cleanup
2. targeted manual smoke only when live UI / render / export / theme / controller behavior is changed

---

## 2. Architectural North Star

`DataVisualiser` is part of a wider canonical data reasoning platform. Within that wider system, the reasoning engine is the architectural center — it produces analytical programs that delivery surfaces consume. The system should evolve toward:

- authority and provenance remain upstream and explicitly visible
- canonical or downstream-safe data enters through explicit boundaries
- the reasoning engine composes derived results, transforms, comparisons, overlays, and future capabilities as inspectable, replayable analytical programs
- every result carries explicit provenance, confidence context, and reasoning path
- orchestration coordinates execution without becoming a semantic authority
- capability and composition logic belongs in the reasoning container, not in presentation or rendering convenience code
- consumer-agnostic downstream contracts sit between engine output and concrete clients, so no presentation technology becomes the primary owner of analytical output shape
- contract and boundary seams standardize what may flow downstream and prevent consumer drift
- projection and translation components translate across explicit seams without creating semantic meaning or hidden policy
- concrete clients (charts, reports, APIs, future consumers) become consumer families and delivery surfaces rather than architectural authorities
- interaction is a named downstream concern, not a hidden controller/rendering side effect
- rendering and delivery infrastructure translate already-defined program output into backend-safe or consumer-safe behavior
- the VNext reasoning engine (`ReasoningSessionCoordinator`) progressively replaces legacy host-authoritative orchestration with explicit request → snapshot → program → contract → consumer delivery semantics
- presentation should become increasingly terminal and replaceable: engine-owned reasoning and contract-owned delivery should survive consumer or backend substitution without semantic or orchestration redesign
- architectural language should remain stable: capability must not collapse into feature, consumer into presentation, interaction into event wiring, composition into builder plumbing, overlay into rendering, provenance into diagnostics, or authority into orchestration

---

## 3. Binding Constraints

All actions must preserve:

1. Canonical semantic authority in normalization/CMS layers.
2. Downward-only authority flow (`Truth -> Reasoning/Derivation -> Interpretation -> Process -> Contracts -> Consumers -> Terminal Delivery`).
3. Legacy as compatibility reference only, not forward architecture.
4. Explicit, deterministic, and reversible migration behavior.
5. No semantic decisions in UI/service/rendering layers.
6. No silent bypass paths or unreachable migration code.
7. No currently shipped capability may be removed as a side effect of refactor work unless explicitly authorized by higher-authority documents.
8. Charting libraries are replaceable infrastructure, not architectural authorities.
9. Backend-specific lifecycle quirks must be isolated inside rendering adapters, never shaping orchestration or semantic flow.
10. No chart backend may be treated as production-safe for a capability family until it has passed explicit qualification.
11. Eventual convergence toward standardized graph parent controllers and shared chart-host affordances is intentional.
12. Transform-like chart programmability is a downstream derived/interpretive capability, not a semantic authority.
13. Support for rendering more than one derived result set on a qualified chart surface is future-facing intent and must not be blocked by single-result assumptions.
14. VNext path must not change the semantic content of the projected context relative to legacy — it is an execution-path alternative, not a semantic one.
15. Transform/controller decomposition must preserve the path toward reusable programmable chart capability over multiple datasets, multiple operations, and multiple rendered graphs on one qualified chart surface.
16. Consumer-agnostic output contracts outrank terminal presentation convenience: presentation and rendering code must not become the primary home of result composition, reasoning, or execution routing.
17. Presentation/client layers should become thinner over time; if a responsibility can live upstream in engine-owned logic or consumer-agnostic contracts without loss of clarity, that is the preferred direction.
18. Capability is not the same as feature: new user-visible features should be expressed through reusable reasoning capabilities where possible.
19. Composition is not builder plumbing: analytical composition belongs in the reasoning/capability container unless explicitly proven to be a terminal delivery concern.
20. Interaction is not event wiring: interaction semantics must cross through explicit contracts and must not redefine meaning.
21. Overlays are interpretive outputs, not rendering conveniences; they must remain reversible, provenance-visible, and non-authoritative.
22. Boundaries are enforcement seams, not folder names; crossing a boundary must preserve authority direction.
23. Projection and translation are not policy: builders, adapters, resolvers, selectors, and projectors may translate across explicit seams, but must not define meaning or execution authority.
24. Provenance is not diagnostics: provenance belongs to truth/result lineage; diagnostics observe system behavior.
25. Authority is not orchestration: orchestration coordinates execution, but must not define what counts as semantic meaning.

---

## 4. Capability Preservation Baseline

Capability families that must not be lost accidentally:

1. Lossless ingestion and persistence of source data.
2. Declarative normalization, canonical identity, and CMS production.
3. Deterministic computation across currently supported strategy families.
4. Visualization coverage across current chart/view families.
5. UI interaction capabilities already present.
6. Reachability, parity, and evidence-generation capability.
7. Existing compatibility/migration observability.
8. Future-extension readiness for interpretive overlays, confidence annotation, structural exploration, and vocabulary-stable capability growth.
9. Rendering-backend extensibility without reworking semantic/application logic.
10. Eventual transform-style programmability and multi-result-set rendering on qualified chart surfaces.

Capability retirement rule: a capability may be removed only if explicitly retired by higher-authority documents or replaced with a validated equivalent.

---

## 5. Current Shape Snapshot (April 2026)

Current observed shape:

- `493` C# source files (Phase 6.6 audit baseline + VNext widening coordinators, milestone recorder, cleanup consolidation, shared tab-host extraction, route/capability seams, admin workflow extraction, strategy parity validation extraction, tooltip formatting split, shared UI-busy lease, parity-series comparer, workspace load/milestone recorders, binary metric context helper, pre-Phase-7 VNext render-plan foundation, and live render-plan adapter wiring)
- `737` DataVisualiser automated tests passing; `15` DataFileReader tests passing
- VNext-compatible request/program support across all current chart families, with live VNext routes and automatic legacy fallback/compatibility projection where still needed
- VNext render-plan foundation exists and is now live-wired across the active chart families and tabs: `ChartRenderPlan`, neutral render buffers, density policy, time-bucket aggregation, backend capabilities, backend selector, and adapter dispatcher are covered by automated tests and adapter-backed delivery
- evidence/export boundary decomposed into standalone DTOs, diagnostics builder, and export orchestrator

Current major concentration points:

- `UI/MainChartsView.xaml.cs` (~1,440 lines)
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs` (~859 lines)
- `UI/WorkspaceTabHost.xaml` (~22 lines)
- `UI/WorkspaceTabHost.xaml.cs` (~30 lines)
- `UI/Admin/AdminMetricsManagerView.xaml` (~114 lines)
- `UI/ChartTabHost.xaml` (~17 lines)
- `UI/ChartTabHost.xaml.cs` (~33 lines)
- `UI/MetricSelectionPanel.xaml` (~134 lines)
- `UI/MetricSelectionPanel.xaml.cs` (~50 lines)
- `UI/MainHost/Coordination/MetricSelectionPanelEventBinder.cs` (~36 lines)
- `UI/MainHost/Coordination/ChartHostDateRangeCoordinator.cs` (~19 lines)
- `UI/MainHost/Coordination/ChartHostMetricSelectionCoordinator.cs` (~89 lines)
- `UI/MainHost/Coordination/MainChartsViewControllerExtrasCoordinator.cs` (~73 lines)
- `UI/MainHost/Coordination/MainChartsViewRegistryCoordinator.cs` (~26 lines)
- `UI/MainHost/Coordination/MainChartsViewSurfaceCoordinator.cs` (~49 lines)
- `UI/MainHost/Coordination/MainChartsViewCmsToggleCoordinator.cs` (~88 lines)
- `UI/MainHost/Coordination/MainChartsViewStateSyncCoordinator.cs` (~95 lines)
- `UI/MainHost/Coordination/MainChartsViewToggleStateCoordinator.cs` (~45 lines)
- `UI/MainHost/Evidence/EvidenceParityBuilder.cs` (~171 lines)
- `UI/MainHost/Evidence/EvidenceDistributionParityEvaluator.cs` (~149 lines)
- `UI/MainHost/Evidence/EvidenceMultiMetricParityEvaluator.cs` (~147 lines)
- `UI/MainHost/Evidence/EvidenceTransformParityEvaluator.cs` (~59 lines)
- `UI/MainHost/Evidence/EvidenceTransformParityDataResolver.cs` (~52 lines)
- `UI/MainHost/Evidence/EvidenceTransformParityComputer.cs` (~76 lines)
- `UI/MainHost/Evidence/MainChartsEvidenceExportService.cs` (~141 lines, reduced from 1,209)
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` (~257 lines, reduced from ~857)
- `UI/Charts/Presentation/TransformRenderCoordinator.cs` (~100 lines)
- `UI/Charts/Presentation/TransformWorkflowCoordinator.cs` (~50 lines)
- `UI/Charts/Presentation/TransformDataResolutionCoordinator.cs` (~171 lines)
- `UI/Charts/Presentation/TransformOperationExecutionCoordinator.cs` (~106 lines)
- `Core/Rendering/Helpers/ChartTooltipFormattingHelper.cs` (~42 lines, reduced from ~464)
- `Core/Services/BaseDistributionService.cs` (~296 lines)
- `Core/Rendering/Engines/ChartRenderEngine.cs` (~333 lines)
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` (~199 lines)
- `UI/Charts/Presentation/BarPieRenderModelBuilder.cs` (~292 lines)

Current read:

- the VNext reasoning engine is live for all current chart families, proving request -> snapshot -> program -> delivery beyond scaffolding
- VNext main-family route eligibility is centralized in `VNextChartRoutePolicy`, keeping routing and `SupportsOnlyMainChart` diagnostics explicit
- the evidence boundary is clean: DTOs, diagnostics, and export orchestration are separate concerns
- evidence export now carries an explicit `ExportScope`; the Charts tab exports as `Charts`, and the Syncfusion tab uses the same `MainChartsEvidenceExportService` as `Syncfusion` instead of a stub service
- tab switches are recorded as `TabSwitched` session milestones through the shared view-model context
- shared tab-shell layout hardening has been smoke-verified: Charts, Syncfusion, and Admin render through the shared workspace shell, with tab/export milestones available in evidence exports
- runtime-path tracking distinguishes VNext from legacy loads with full signature-chain diagnostics
- VNext render plans are now a live-wired preservation-baseline delivery contract over chart programs: `ChartRenderPlanProjector` can project Cartesian and hierarchy-shaped plans, `RenderDensityPolicy` chooses full-fidelity/aggregated/viewport-refined intent, `TimeBucketRenderAggregationKernel` creates neutral bounded render buffers, and `ChartRenderPlanAdapterDispatcher` routes plans to backend-capability adapters
- the current render-plan foundation explicitly covers current chart program families and includes a hierarchy shape for Syncfusion/Sunburst-style delivery, and live UI rendering now flows through adapter-backed render-plan delivery across the active chart families and tabs
- the chart/rendering delivery seams are materially cleaner than before the Phase 5 rehaul
- `DataFetcher` is now a facade over focused query groups for catalog, metric-data, date/count, and admin concerns
- transform subtype-combo lifecycle is now isolated in `TransformSubtypeSelectionCoordinator`
- transform grid/result/chart presentation is now split across dedicated presentation coordinators
- the remaining transform decomposition work must preserve future seams for dataset resolution, operation planning/execution, result-set production, and chart delivery rather than reinforcing unary/binary-only assumptions
- transform dataset/selection resolution is now isolated in `TransformDataResolutionCoordinator`
- transform operation eligibility and execution are now isolated in `TransformOperationExecutionCoordinator`
- distribution bucket extraction, min/max, tooltip, and averaging computations are now delegated to `DistributionComputationHelper`
- distribution simple-range result assembly is now delegated to `DistributionRangeResultBuilder`
- distribution baseline/range/average series construction is now delegated to `DistributionSeriesBuilder`
- chart tooltip formatting is now delegated to `ChartTooltipFormattingHelper`, with pair, stacked, cumulative, title parsing, value formatting, and overlay filtering split into focused helpers
- chart-series label formatting and materialization are now delegated to `ChartSeriesLabelFormatter` and `ChartSeriesMaterializer`
- chart cumulative-series construction and Y-axis normalization-data preparation are now delegated to `ChartCumulativeSeriesBuilder` and `ChartYAxisDataBuilder`
- parity assembly is now delegated out of `MainChartsEvidenceExportService` into `EvidenceParityBuilder`
- distribution parity evaluation is now delegated out of `EvidenceParityBuilder` into `EvidenceDistributionParityEvaluator`
- multi-series parity input resolution and evaluation are now delegated out of `EvidenceParityBuilder` into `EvidenceMultiMetricParityEvaluator`
- transform parity evaluation is now delegated out of `EvidenceParityBuilder` into `EvidenceTransformParityEvaluator`
- transform parity selection/data resolution is now delegated out of `EvidenceTransformParityEvaluator` into `EvidenceTransformParityDataResolver`
- transform unary/binary parity computation is now delegated out of `EvidenceTransformParityEvaluator` into `EvidenceTransformParityComputer`
- session milestone and tracked-host-message bookkeeping is now delegated out of `MainChartsView` into `MainChartsSessionDiagnosticsRecorder`
- transform execution/toggle milestone construction is now delegated out of `TransformDataPanelControllerAdapter` into `TransformSessionMilestoneRecorder`
- transform operation-tag and compute-button state logic is now delegated out of `TransformDataPanelControllerAdapter` into `TransformOperationStateCoordinator`
- transform subtype-change interaction is now delegated out of `TransformDataPanelControllerAdapter` into `TransformSelectionInteractionCoordinator`
- transform grid/result/render-host handoff is now delegated out of `TransformDataPanelControllerAdapter` into `TransformRenderCoordinator`
- transform execution, refresh, and result-render sequencing are now delegated out of `TransformDataPanelControllerAdapter` into `TransformWorkflowCoordinator`
- transform layout-specific behavior is now expressed through `ITransformLayoutCapabilities`, avoiding concrete `TransformDataPanelControllerV2` checks in presentation coordinators
- CMS strategy eligibility is now delegated out of `StrategyCutOverService` into `StrategyCmsDecisionEvaluator`
- parity validation, strategy-type inference, parity harness selection, and fallback parity validation are now delegated out of `StrategyCutOverService` into `StrategyParityValidationService`
- Bar/Pie model planning, date-range resolution, bucket planning, and series-total loading are now delegated out of `BarPieChartControllerAdapter` into `BarPieRenderModelBuilder`
- UI-surface diagnostics capture is now delegated out of `MainChartsView` into `MainChartsUiSurfaceDiagnosticsReader`
- selection/date/resolution/bar-pie state projection back into the WPF surface is now delegated out of `MainChartsView` into `MainChartsViewStateSyncCoordinator`
- primary/secondary chart toggle enablement and main-chart stacked-availability bookkeeping are now delegated out of `MainChartsView` into `MainChartsViewToggleStateCoordinator`
- CMS checkbox state projection, enablement, and config-change handling are now delegated out of `MainChartsView` into `MainChartsViewCmsToggleCoordinator`
- Syncfusion state projection now reuses the shared `MainChartsViewStateSyncCoordinator`
- the top metric-selection/date/CMS control surface is now shared between the Charts and Syncfusion tabs through `MetricSelectionPanel`, hosted by the chart-specialized `ChartTabHost` shell
- `ChartTabHost` is now a specialization over the generic `WorkspaceTabHost`, which exposes header/body slots without assuming metric controls
- `AdminMetricsManagerView` now also uses `WorkspaceTabHost`, keeping its Admin-specific header controls while sharing the same workspace shell pattern as chart surfaces
- Admin row loading, dirty tracking, save-state calculation, disabled-row filtering, and Admin milestone recording are now delegated out of `AdminMetricsManagerView` into `AdminMetricsManagerCoordinator` behind `IAdminMetricsRepository`
- Admin metric-type changes, hide-disabled toggles, reloads, first dirty-row edits, and save attempts now emit session milestones into the shared chart-state timeline for cross-tab smoke exportability
- metric-selection event forwarding is now centralized through `MetricSelectionPanelEventBinder` instead of duplicated direct event wiring in each chart host
- theme-toggle and reset-zoom actions now emit explicit session milestones, so manual smoke exports can prove those interactions instead of relying on visual confirmation only
- Syncfusion load, data-loaded, render completion/failure, export request/completion/failure, and zoom-reset actions now emit explicit session milestones for tab-specific smoke exportability
- default date-range initialization/reset is now shared between `MainChartsView` and `SyncfusionChartsView` through `ChartHostDateRangeCoordinator`
- metric-type list initialization, metric-type-change reset/reload, and subtype-loaded follow-up behavior are now shared between `MainChartsView` and `SyncfusionChartsView` through `ChartHostMetricSelectionCoordinator`
- `MainChartsView` and `SyncfusionChartsView` now share `UiBusyScopeLease` for disposable UI-busy lifetime handling
- controller-extras interaction for Bar/Pie, Distribution, WeekdayTrend, Transform, Diff/Ratio, and Main is now delegated out of `MainChartsView` into `MainChartsViewControllerExtrasCoordinator`
- registry-wide controller enumeration and startup/zoom-clear fallback resolution are now delegated out of `MainChartsView` into `MainChartsViewRegistryCoordinator`
- chart-surface startup, no-data axis-label suppression, default-title initialization, and distribution-polar tooltip creation are now delegated out of `MainChartsView` into `MainChartsViewSurfaceCoordinator`
- subtype-selection bookkeeping and loaded-vs-refresh branching are now delegated out of `MainChartsView` into `MainChartsViewSelectionCoordinator`
- load validation, load execution, and clear/reset bookkeeping are now delegated out of `MainChartsView` into `MainChartsViewLoadCoordinator`
- Syncfusion load validation, load execution, and clear/reset bookkeeping are now delegated out of `SyncfusionChartsView` into `SyncfusionChartsViewLoadCoordinator`
- distribution axis formatting and debug-summary logging are now delegated out of `BaseDistributionService` into `DistributionAxisCoordinator` and `DistributionDebugSummaryLogger`
- VNext workflow state now carries explicit `WorkflowPlanRequest` plans, and the bridge accepts explicit `ChartProgramRequest` input for non-live program projection
- the remaining architectural noise is concentrated in the outliers listed above
- low-level helper duplication is no longer the dominant problem; mixed host/orchestration/evidence/data-access responsibilities are

---

## 5.5 Architectural Migration Progress Snapshot

This is a concise execution-plan view of the accepted architectural vocabulary progress snapshot.

Current estimate:

```text
Architectural migration: approximately 65–70% complete
Working estimate: ~68%
```

| Area | Approx. completion | Execution-plan interpretation |
|---|---:|---|
| Vocabulary / conceptual model | 90% | Stable promoted concepts and target hierarchy are in place and should be treated as current grammar. |
| VNext reasoning spine | 75% | `ReasoningEngine`, analytical intent, program planning, and session coordination exist and are materially usable. |
| Contract / boundary model | 65% | Consumer/provider contracts and render-plan seams are emerging; enforcement still needs proof through bounded slices. |
| Rendering demotion | 60% | Render-plan delivery exists, but `Core.Rendering` remains structurally large and must stay terminal. |
| Consumer / interaction separation | 55% | Better contracts exist, but UI/presentation remains a major remaining front. |
| Governance / evidence | 75% | Evidence, parity, and diagnostics infrastructure are strong but must remain observational. |
| Legacy coexistence cleanup | 50–60% | Legacy/VNext coexistence remains managed debt while VNext becomes progressively more authoritative. |

Execution meaning:

- the migration is materially underway, not merely documentary
- the project has crossed from presentation/rendering-heavy shape toward reasoning-engine + contract/provider/consumer boundary + terminal delivery
- the remaining work is consolidation, enforcement, and selective relocation
- broad decomposition is not the next default move

Best next assessment move:

```text
Audit whether the provider/consumer boundary enforces the intended architectural seam,
or merely renames delivery routing.
```

Priority audit targets:

- `ConsumerProviderContract`
- `ConsumerProviderContracts`
- `ConsumerProviderRegistry`
- `ChartProgramDeliveryTargetResolver`
- `ChartRenderPlanProviderMetadata`
- `ChartRenderPlanVocabularyMetadata`
- `ChartRenderPlanAdapterQualification`

This section does not redefine the architectural vocabulary. It applies the accepted vocabulary progress snapshot to active subsystem execution.

---

## 6. Completed Work

### 6.1 Phase 5 — Architecture Rehaul (CLOSED)

All 17 steps completed. Historical execution source was `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md` (now absorbed here).

Summary of what was achieved:
- Default test lane stabilized (no live SQL dependency)
- Cross-cutting seams introduced (configuration abstraction, notification abstraction)
- `DataFetcher` placed behind narrow query contracts (`IMetricSelectionDataQueries`)
- `ChartHelper` split into core-safe logic vs UI-specific logic
- Chart/controller structure sanitized and standardized
- Rendering capability contracts established and widened across all chart families
- CMS/Legacy cut-over truthfulness repaired for all active strategy families
- Light/dark theme support introduced through explicit UI/theme seams
- Orchestration refactored to explicit handoff boundaries
- `MainChartsView` reduced to a composition host (two passes)
- Syncfusion-specific fragility isolated
- `DataFileReader` modernization started (first infrastructure slice)
- Hard architectural guardrails added (dependency direction, vendor leakage, notification, theme, export)
- Evidence/export flow rebuilt behind dedicated services/coordinators
- `SQLHelper` split by capability
- Final convergence pass completed; remaining debt made explicit

Phase 5 findings and their closure status:

| Finding | Status |
|---------|--------|
| 1. Composition bottleneck in `MainChartsView` | Materially reduced; residual host gravity remains intentional debt |
| 2. Core/UI boundary collapse | Enforced with architecture tests |
| 3. CMS migration scaffolding structural | CMS paths real for all active families |
| 4. Evidence pipeline misaligned | Rebuilt with repository-visible artifacts |
| 5. Mutable global toggles | Configuration abstraction introduced |
| 6. Data access/orchestration concentrated | Query contracts introduced; `DataFileReader` modernized |
| 7. Uneven assurance depth | Guardrails and conformance tests added |

Documents absorbed from Phase 5:
`DataVisualiser_Architecture_Consolidation_Report.md`, `UNDERDEVELOPED_COMPONENTS_IMPLEMENTATION_ORDER.md`, `CHART_CONTROLLER_DECOUPLING_PLAN.md`, `CMS_MIGRATION_STATUS_AND_IMPLEMENTATION_PLAN.md`, `CMS_TEST_RESULTS_AUDIT.md`, `REFACTORING_IMPACT_ASSESSMENT.md`, `TransformDataPanelControllerV2.md`, `HierarchyRefId_Implementation_Plan.md`, `CANONICAL_MAPPING_LIMITATION.md`, `CMS_TEST_SUMMARY_TABLE.md`, `PARITY_VALIDATION_EXPLAINED.md`

### 6.2 Phase 6 — Banked Consolidation Work

**Phase 6.1 (Irreducible operations):** Closed.
- Frequency binning consolidated into `FrequencyBinningHelper`
- Transform preparation consolidated into `TransformComputationService`
- Chart-context series preparation separated into `ChartDataSeriesPreparationHelper`
- Smoothing, data selection, temporal alignment already centralized in dedicated services
- Time-bucket averaging and bucket-index resolution consolidated into `TimeBucketAggregationHelper`
- `BarPieChartControllerAdapter` and `SyncfusionSunburstChartControllerAdapter` now consume the shared bucket helper
- `TransformDataPanelControllerAdapter` now delegates unary/binary computation to `TransformComputationService`
- Architecture guardrails lock the shared owners so irreducible-operation sprawl does not silently re-enter the outliers

**Phase 6.5 (Physical hierarchy):** Closed.
- 11 single-file directories eliminated
- 21 micro-type files merged into logical owners
- `codebase-index.md` and architecture guardrail tests updated

**Phase 6.2 (Boundary reconciliation):** Closed.
**Phase 6.4 (Outlier decomposition):** Closed.

**Phase 6.3 (Request/delivery standardization):** Closed.
- Evidence boundary decomposed: `MainChartsEvidenceExportService` (1,209 -> 141 lines) split into `EvidenceExportModels` (21 DTOs), `EvidenceDiagnosticsBuilder`, and export orchestration
- `VNextMainChartIntegrationCoordinator` built and tested as the VNext → legacy bridge
- VNext routing activated for the main-chart family in `MetricLoadCoordinator`
- `LoadRuntimeState` on `ChartState` tracks runtime path, signatures, and failure reason
- `EvidenceRuntimePath` and `VNextDiagnosticsSnapshot` emitted in evidence exports
- Smoke-verified with April 2026 exports: VNext signature chain aligned, legacy fallback correct, all 8 parity strategies pass

 - `VNext` workflow planning now supports explicit `ChartProgramRequest` / multi-derived-program shaping behind non-live tests
 - Distribution fresh data loads now route through the VNext reasoning engine via shared `VNextDataResolutionHelper` and `VNextSeriesLoadCoordinator`, with identity program builder, signature-chain tracking, and automatic legacy fallback
 - Per-family runtime tracking via `ChartState.SetFamilyRuntime(ChartProgramKind, LoadRuntimeState)` with dictionary-backed `FamilyLoadRuntimes`; evidence diagnostics iterate the dictionary automatically
 - WeekdayTrend, Transform, and BarPie fresh data loads now route through VNext via shared `VNextSeriesLoadCoordinator`; each family has its own `ChartProgramKind`, `EvidenceRuntimePath`, and `LastXxxLoadRuntime` tracking
 - BarPie VNext integration preserves CMS preference: if CMS is available via VNext snapshot, it is used for bucket aggregation
 - All four non-main chart families (Distribution, WeekdayTrend, Transform, BarPie) have automatic legacy fallback on VNext failure
 - Distribution interaction milestones (frequency shading, interval count, mode, chart type, subtype changes) are now recorded as session milestones

**Phase 6.7 (Pre-Phase 7 structural consolidation):** Closed.
- Dead code removed: `IDistributionResultExtractor` (0 implementations, 0 references)
- Syncfusion namespace mismatch corrected: 4 files declared `SyncfusionViews`, folder is `Syncfusion`; aligned to `DataVisualiser.UI.Syncfusion` across 9 files including XAML
- 6 tiny parity type files (41 lines total) consolidated into `ParityTypes.cs`
- Rendering helper pairs merged: `ChartLabelFormatter` → `ChartSeriesLabelFormatter`, `TransformChartAxisLayout` → `TransformChartAxisCalculator`
- Shared `EvidenceDataResolutionHelper` extracted: unified data-resolution and strategy-resolution patterns duplicated across 3 evidence evaluators
- `UI/MainHost/` (41 files flat) decomposed into 3 sub-namespaces: `Evidence/` (15 files), `Export/` (6 files), `Coordination/` (20 files)
- Architecture guardrail paths and `ParityExportShapeTests` reflection strings updated
- Net file reduction: -7 files; 609 tests passed at closure; later shared-panel/evidence-scope hardening, tab-shell extraction, route-policy extraction, transform layout capability isolation, CMS decision extraction, admin workflow extraction, strategy parity validation extraction, tooltip formatting split, shared UI-busy lease, parity-series comparison, workspace load/milestone recording, binary metric context consolidation, VNext render-plan foundation, and Main chart render-plan adapter wiring bring the current lane to 737 DataVisualiser tests

**Phase 6.6 (Architecture audit and baseline refresh):** Closed.

Audit baseline (April 2026):
- Phase 6 closure baseline: 476 C# source files
- Current pre-Phase-7 render-plan foundation lane: 493 C# source files, 174 test files, 737 DataVisualiser automated tests passing; 15 DataFileReader tests passing
- Architecture guardrails continue to enforce structural contracts, including backend-neutral VNext render-plan boundaries

**1. To what extent have the Phase 6 objectives been met?**

Materially met. The hierarchy is trustworthy: similar responsibilities have one obvious home, repeated operations are centralized, and the remaining debt is explicit rather than hidden. Specifically:
- Irreducible operations (frequency binning, transform computation, series preparation, smoothing, timeline alignment, bucket aggregation) are each owned by a single dedicated helper, locked by architecture guardrails (6.1)
- Truth/derivation/orchestration/delivery boundaries are enforced through rendering contracts, orchestration pipelines, and explicit stage separation (6.2)
- Request/delivery standardization has real proof through VNext-compatible request/program support across all active chart families, live VNext routes where appropriate, automatic legacy fallback, runtime-path tracking, and signature-chain diagnostics in evidence exports (6.3)
- Named outliers have been materially reduced: `MainChartsEvidenceExportService` (1,209->141), `TransformDataPanelControllerAdapter` (857->257), `BaseDistributionService` (612->296), `BarPieChartControllerAdapter` (503->199), `ChartRenderEngine` (452->333), `DataFetcher` decomposed into focused query groups (6.4)
- Physical hierarchy is clean: namespace-folder alignment verified, micro-types consolidated, `UI/MainHost/` decomposed into `Evidence/`, `Export/`, `Coordination/` sub-namespaces (6.5, 6.7)

**2. What gaps remain?**

These are accepted as known debt, not open work:
- `MainChartsView.xaml.cs` (~1,440 lines) remains the largest host concentration point. Its remaining responsibilities are genuinely host-level (wiring coordinators, forwarding events, managing host lifecycle). Further decomposition is possible but increasingly behavior-adjacent rather than structural.
- `SyncfusionChartsView.xaml.cs` (~859 lines) is a parallel host with similar characteristics, lower priority.
- VNext-compatible request/program support covers all active chart families through either the main-family route or per-family fresh-load routes. Legacy remains as compatibility, fallback, and delivery projection while Phase 7 progressively makes VNext authoritative per family.
- Controller adapters share structural patterns but their differences are genuine domain variation. A deeper base class was evaluated and rejected as overengineering (documented in the Phase 6.7 plan).
- 20+ coordinators have thin shared patterns (guard → execute) that do not justify inheritance.

**3. How would remaining objectives be reached in the next cycle?**

- `MainChartsView` host reduction: extract one more coordinator per session when a clear host-level responsibility boundary is identified (behavior-adjacent, requires targeted smoke)
- Phase 7 capability expansion: use the now-stable render-plan delivery baseline to add reasoning-engine features, confidence-aware behavior, and interpretive overlays without reopening delivery uncertainty
- Controller adapter convergence: defer to Phase 8 (UI consolidation) when Phase 7 has established what the standardized graph host surface looks like

**Global Phase 6 Closure Assessment:**

| Condition | Met? | Evidence |
|-----------|------|----------|
| 1. Similar responsibilities have one obvious home | Yes | Irreducible operations locked by guardrails; evidence/export/coordination in dedicated sub-namespaces; rendering contracts per capability family |
| 2. Repeated irreducible operations no longer sprawl | Yes | Frequency binning, transform computation, series preparation, smoothing, bucket aggregation each have one owner |
| 3. Truth/derivation/orchestration/delivery seams are clearer | Yes | VNext reasoning engine live for main family; rendering contracts enforce backend qualification; evidence boundary decomposed |
| 4. Remaining outliers are explicit, bounded, and visible | Yes | `MainChartsView` (host gravity), `SyncfusionChartsView` (parallel host), adapter pattern variation — all documented, all bounded |
| 5. Current capabilities preserved or replaced | Yes | 737 DataVisualiser tests and 15 DataFileReader tests pass; Main, Distribution, WeekdayTrend, Bar/Pie, and Syncfusion render through adapter-backed render-plan delivery; evidence exports include runtime-path tracking and tab-scoped export metadata; no regressions |

**Phase 6 is closed.** All active chart families now route through the VNext reasoning engine for fresh data loads. Phase 7 entry gate is satisfied; the render-plan delivery primer is complete and Phase 7 capability expansion is now the next active step.

**Phase B host spine decomposition (banked slices):**
- Export trigger extraction → `MainChartsViewEvidenceExportCoordinator`
- Data-loaded refresh → `MainChartsViewDataLoadedCoordinator`
- Selection lifecycle stabilization → batched state updates in `MainWindowViewModel` and `SubtypeSelectorManager`
- Request-driven load → immutable `MetricLoadRequest` with `LoadRequestSignature`
- MainHost sprawl tightening → trivial micro-carriers folded into primary owners

---

## 7. Primary Mandate

Phase 6 established a trustworthy hierarchy. The primary mandate is now Phase 7 exploratory capability expansion.

**Phase 7 — Exploratory capability expansion:**
- add interpretive overlays, confidence-aware views, and programmable multi-result composition as reasoning-engine capabilities
- build on the proven VNext request/program/contract/delivery architecture rather than extending the legacy path
- express new work through the enhanced containers: authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence
- each new capability must respect canonical boundaries and not reintroduce exception-driven structure

**Closed historical baselines now carried forward:**
- Phase 6.3 VNext family widening is closed: VNext coverage has been extended to Distribution, WeekdayTrend, Transform, and Bar/Pie, each with its own program kind, projection/runtime tracking, and automatic legacy fallback.
- The pre-Phase-7 rendering primer is closed: chart delivery now has the VNext-owned `ChartProgram -> ChartRenderPlan -> backend adapter` baseline across active chart families and tabs.
- These baselines are preservation constraints for Phase 7 work, not reopened Phase 6 or primer workstreams.

In practice:

1. New capabilities should compose over existing canonical and derived views, not redefine them.
2. Each exploratory feature should have an explicit home in the enhanced module buckets below, using the architectural vocabulary as the concept source.
3. Future VNext changes should preserve the established per-family fallback pattern and proceed only as bounded capability slices.
4. Confidence and interpretive overlays are annotations, not mutations — they must be reversible and non-authoritative.
5. Work should reduce future operator burden.
6. The hierarchy established in Phase 6 should be preserved — new code should strengthen the enhanced containers rather than introducing parallel ones.

---

## 8. Target Module Buckets

These are the target responsibility buckets for evaluating any slice. They apply the canonical architectural vocabulary to active `DataVisualiser` execution.

1. **Authority Container** — canonical intake, identity, provenance, truth-aware context/state/request/result, semantic status.
2. **Reasoning and Capability Container** — reasoning engine, strategies, transforms, comparisons, confidence, overlays, analytical composition, chart/program planning.
3. **Process and Execution Container** — workflows, coordinators, routing, fallback/coexistence, context handoff, execution observability.
4. **Contract and Boundary Container** — program, delivery, interaction, view, multi-result, and consumer-agnostic surface contracts; enforcement of downstream seams.
5. **Projection and Translation Container** — builders, adapters, projectors, selectors, resolvers, formatters, and converters that translate across explicit boundaries.
6. **Consumer and Interaction Field** — chart/export/API/future consumer families, controllers, interaction semantics, view models, consumer state, host coordination.
7. **Terminal Delivery Infrastructure** — renderers, render surfaces, backend adapters, backend qualification, route/host binding, vendor-specific lifecycle behavior.
8. **Governance and Evidence Sidecar** — diagnostics, evidence, parity, reachability, qualification probes, validation, export/audit.

---

Pre-Phase-7 rendering-primer note:
- VNext render plans, neutral render buffers, density policy, and backend-neutral adapter dispatch belong under Terminal Delivery Infrastructure only after they consume explicit contracts.
- The contract/boundary seam is the intended fan-out point: concrete clients should not accept chart-program or UI-specific data directly.
- LiveCharts, Syncfusion, and future plugin clients should converge on `IChartRenderPlanAdapter<TSurface>`-style seams under terminal delivery.
- Long-term direction: as consumer-agnostic delivery contracts become stronger, client adapters and surfaces should become increasingly terminal and replaceable rather than acting as major subsystem centers.


---

## 8.5 Enhanced Architecture Migration Phasing

The enhanced architecture should not be treated as a single refactor phase. It is better understood as a forward-only, multi-cycle migration direction that can be advanced through bounded slices.

This section does not reopen Phase 5, Phase 6, Phase 6.3, or the pre-Phase-7 rendering primer. Those remain closed historical baselines.

The architectural vocabulary document defines the concepts used here; this section applies them as execution phasing.

### 8.5.1 Forward Stage 7A — Authority and Intent Clarification

Primary aim:
- make authority, provenance, and canonical intent explicit enough that downstream layers stop re-deciding meaning or request shape.

Main changes:
- clarify truth-aware context/state/request/result ownership
- define or converge toward a canonical intent model
- prevent UI, rendering, or process code from becoming hidden semantic authorities

### 8.5.2 Forward Stage 7B — Reasoning and Capability Expansion

Primary aim:
- express confidence, overlays, transforms, comparisons, and programmable composition as reasoning-engine capabilities.

Main changes:
- keep capability and composition logic inside the reasoning container
- treat overlays as declared interpretive outputs, not rendering conveniences
- preserve provenance and reversibility for all derived outputs

### 8.5.3 Forward Stage 7C — Contract and Boundary Hardening

Primary aim:
- make contracts the real downstream fan-out seam.

Main changes:
- strengthen program, delivery, interaction, view, and multi-result contracts
- ensure consumers receive downstream-safe output rather than chart-program or UI-specific internal structures
- move boundary enforcement out of terminal presentation code

### 8.5.4 Forward Stage 7D — Projection and Translation Discipline

Primary aim:
- ensure builders, adapters, resolvers, selectors, and projectors translate across explicit boundaries without becoming semantic authorities or hidden policy centers.

Main changes:
- classify projection/translation types by the boundary they cross
- remove or constrain adapters/resolvers that hide execution policy or semantic choice
- keep translation explicit, reversible, and subordinate to contracts

### 8.5.5 Forward Stage 7E — Consumer and Interaction Separation

Primary aim:
- make consumers and interactions first-class but non-authoritative.

Main changes:
- separate interaction semantics from event wiring and controller convenience
- classify charts, exports, APIs, and future clients as consumer families
- keep consumer state local and non-semantic

### 8.5.6 Forward Stage 7F — Terminal Delivery Demotion

Primary aim:
- keep rendering and backend-specific behavior terminal, replaceable, and subordinate.

Main changes:
- push route/host binding, backend adapters, vendor lifecycle, and delivery quirks into terminal infrastructure
- prevent terminal delivery from owning composition, semantic meaning, or execution policy

### 8.5.7 Forward Stage 7G — Governance and Migration Sidecar Isolation

Primary aim:
- keep diagnostics, parity, evidence, reachability, and qualification observational rather than governing.

Main changes:
- isolate migration/evidence logic from the steady-state execution path
- keep sidecars proving behavior without shaping live semantic control

### 8.5.8 Forward Stage 8 — Consumer and UI Consolidation

Primary aim:
- consolidate chart/UI/controller families only after contracts and interaction boundaries are stable.

Main changes:
- standardize parent controllers and host affordances where capability semantics genuinely align
- collapse repeated family-specific micro-frameworks only when the upstream spine and contract seam are already clear

### 8.5.9 Forward Stage 9 — Broad Family Pattern Consolidation

Primary aim:
- reduce repeated request/route/qualification/adapter patterns across chart families and consumers.

Main changes:
- consolidate proven shared seams
- retire transitional adapters where VNext/contract-native paths have become primary
- preserve honest outliers where capability differences are real

## 9. Governing Legibility Cycle

All consolidation work should follow this cycle:

1. **Identify Entropy** — find where the hierarchy stops explaining itself
2. **Classify Container Ownership** — determine whether the responsibility belongs to authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, or governance/evidence
3. **Constrain / Re-home** — move responsibilities toward clearer owners
4. **Consolidate Irreducibles** — give similar operations one obvious home
5. **Standardize Proven Patterns** — standardize only after 2-3 real consumers prove a shape
6. **Retire / Remove / Clean** — remove superseded wrappers and residue
7. **Validate / Re-measure** — full build, tests, targeted smoke where behavior changed

---

## 10. Trackable Phase Plan

The trackable phase plan should be read after the enhanced module buckets and migration phasing above. Section 8 defines the target containers; Section 8.5 defines the multi-cycle migration direction; this section records completed and active execution tracks.

### 10.1 Phase A — Re-Baseline (COMPLETED)

Hierarchy baseline refreshed. Current outlier map remains valid.

### 10.2 Phase B — Host Spine Decomposition (CLOSED)

Multiple slices banked (export, data-loaded, selection stabilization, request-driven load, MainHost tightening, 10 dedicated coordinators extracted). Remaining host gravity in `MainChartsView` is intentional debt — genuinely compositional, not mixed-responsibility.

### 10.3 Phase C — Outlier Service Decomposition (CLOSED)

All named outliers materially reduced:
- `MainChartsEvidenceExportService`: 1,209 -> 141 lines
- `TransformDataPanelControllerAdapter`: 857 -> 257 lines (split across 6 coordinators)
- `BaseDistributionService`: 612 → 296 lines
- `BarPieChartControllerAdapter`: 503 -> 199 lines
- `ChartRenderEngine`: 452 -> 333 lines (dead-delegation residue cleaned)
- `DataFetcher`: decomposed into focused query groups (46-line facade)

The tooltip formatting helper outlier has been split: `ChartTooltipFormattingHelper` is now a 42-line facade over focused pair, stacked, cumulative, title parsing, value formatting, and overlay filtering helpers.

### 10.4 Phase D — Delivery and Rendering Spillover Simplification (CLOSED)

`ChartTooltipFormattingHelper` (464 → 42 lines) now delegates pair formatting, stacked totals, cumulative reconstruction, value extraction, series-title parsing, and overlay filtering to focused helpers. `ChartUpdateCoordinator` and vendor seams remain stable.

### 10.5 Phase E — Architecture Audit and Next-Cycle Gate (CLOSED)

Historical audit gate for the completed Phase 6 cycle. Measure concentration reduction. List what became more legible. Name the next cycle from remaining outliers.

Current baseline snapshot (April 2026):

- host responsibilities in `MainChartsView` are materially thinner, but it remains the largest composition concentration point
- transform workflow is now split across selection, resolution, execution, operation-state, milestone, and render-handoff seams; the remaining debt is mostly workflow composition rather than mixed utility logic
- `BaseDistributionService` is no longer a mixed computation/render monolith; the remaining debt is narrower strategy/render coordination
- live VNext routing is stable enough across active chart families to count as real architectural proof, not scaffolding
- the remaining major outliers are now explicit enough to define the next cycle without guesswork

The audit answered these three questions for Phase 6 closure and now serves as the historical baseline for Phase 7 planning:

1. Given the current state of the code, to what extent have the outlined objectives of the plan been met?
2. What gaps, inefficiencies, risks, or missed opportunities remain?
3. How would the remaining objectives be reached in the next major cycle?

Any next-cycle proposal must follow the governing iteration flow (Section 9) or justify the deviation. If something cannot reasonably be completed in one major cycle, that limitation must be stated explicitly.

Next-cycle proposals should prefer the enhanced implementation order: authority spine and canonical intent first, then primary execution model, reasoning/process separation, contract/boundary seam, projection/translation discipline, consumer/interaction branching, terminal delivery demotion, governance/evidence sidecar isolation, and only then broad family-pattern consolidation.

### 10.6 VNext Activation (CLOSED — Historical Parallel Track)

VNext activation is proven across active chart families. This is a closed preservation baseline, not an active Phase 6 continuation. Preservation rules:
- Preserve the current `Main + Normalized + Diff/Ratio` live slice with evidence and targeted smoke as the bounded family baseline
- Preserve the independent per-family VNext routes for Distribution, WeekdayTrend, Transform, and BarPie with automatic legacy fallback
- Treat future VNext work after the render-plan delivery primer as Phase 7 capability expansion unless it repairs a concrete regression

#### VNext Widening Tracker

| Chart Family | VNext Status | Routing Condition | Evidence | Notes |
|---|---|---|---|---|
| Main | Live | Main visible and no non-main-family chart visible | April 2026 exports + current smoke gate | First vertical slice, fresh coordinator per load |
| Normalized | Live via main-family route | Main + Normalized, with no distribution/weekday/transform/bar-pie visible | April 2026 smoke/export evidence | Rendered from the projected two-series context |
| Difference | Live via main-family route | Main + Diff/Ratio, with no distribution/weekday/transform/bar-pie visible | April 2026 smoke/export evidence | Unified Diff/Ratio surface consumes projected two-series context |
| Ratio | Live via main-family route | Main + Diff/Ratio, with no distribution/weekday/transform/bar-pie visible | April 2026 smoke/export evidence | Unified Diff/Ratio surface consumes projected two-series context |
| Distribution | Live (independent route) | Distribution visible + fresh series load (series differs from main context) | April 2026 smoke/export evidence | Single-series VNext load with identity program; distribution computation stays in legacy services; automatic legacy fallback on failure; runtime path tracked as VNextDistribution |
| WeekdayTrend | Live (independent route) | WeekdayTrend visible + fresh series load (series differs from main context) | April 2026 smoke/export evidence | Single-series VNext load via shared VNextSeriesLoadCoordinator; automatic legacy fallback; runtime path tracked as VNextWeekdayTrend |
| Transform | Live (independent route) | Transform visible + fresh series load (primary or secondary differs from main context) | April 2026 smoke/export evidence | Per-series VNext load in TransformDataResolutionCoordinator; automatic legacy fallback; runtime path tracked as VNextTransform |
| BarPie | Live (independent route) | BarPie visible (all series loaded per-selection) | April 2026 smoke/export evidence | Per-series VNext load in BarPieRenderModelBuilder; CMS preference preserved; automatic legacy fallback; runtime path tracked as VNextBarPie |

---

### 10.7 Pre-Phase-7 Render-Plan Delivery Primer (CLOSED — Preservation Baseline)

Current completed foundation:
- `ChartRenderPlan` models backend-neutral Cartesian, faceted, and hierarchy delivery intent.
- `RenderDataBuffer` and `RenderDataPoint` carry chart-library-agnostic render data.
- `RenderDensityPolicy` chooses full-fidelity, aggregated-overview, or viewport-refined intent.
- `TimeBucketRenderAggregationKernel` creates bounded overview buffers without discarding source identity.
- `ChartBackendCapabilities` and `ChartBackendSelector` describe backend support without importing vendor types.
- `IChartRenderPlanAdapter<TSurface>` and `ChartRenderPlanAdapterDispatcher<TSurface>` define the future adapter seam for LiveCharts, Syncfusion, and plugin renderers.

Closed status:
- automated tests cover the foundation across all current `ChartProgramKind` values and hierarchy-shaped Syncfusion/Sunburst-style plans
- Main, Distribution, WeekdayTrend, Bar/Pie, and Syncfusion now consume `ChartRenderPlan` through adapters while preserving their existing computation and render-engine behavior
- manual smoke and export verification have completed for the active live render-plan slices

Completed live slices:
1. Main chart live render-plan delivery.
2. Distribution, WeekdayTrend, Bar/Pie, and Syncfusion live render-plan delivery.
3. Density/backend diagnostics now flow through the evidence export path.
4. Phase 7 capability work may proceed now that the primer is closed.

## 11. Current Priority Outliers

Read these through the enhanced ownership lens: each outlier should be classified by whether it violates authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, or governance/evidence ownership.

Also audit the new provider/consumer boundary types to verify whether they enforce real boundary behavior rather than merely renaming delivery routing.

- `UI/MainChartsView.xaml.cs` — largest host concentration point
- `Core/Data/Repositories/DataFetcher.cs` — query shaping, normalization, subtype resolution
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` — remaining transform workflow and chart/grid handoff concentration
- `Core/Services/BaseDistributionService.cs` — computation + rendering mixed
- `Core/Rendering/Engines/ChartRenderEngine.cs` — render normalization, axis shaping, tooltip range
- `Core/Rendering/Helpers/ChartHelper.cs` — scattered tooltip/axis/clearing helpers
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` — rendering family concentration


### 11.1 Provider / Consumer Boundary Audit Targets

These are not necessarily outliers. They are high-value verification targets because they sit near the current migration frontier.

- `ConsumerProviderContract`
- `ConsumerProviderContracts`
- `ConsumerProviderRegistry`
- `ChartProgramDeliveryTargetResolver`
- `ChartRenderPlanProviderMetadata`
- `ChartRenderPlanVocabularyMetadata`
- `ChartRenderPlanAdapterQualification`

Audit question:

```text
Do these types enforce the intended contract/boundary seam,
or do they merely rename delivery routing?
```

Preferred outcome:
- contracts constrain what crosses the boundary
- provider selection is explicit and testable
- consumer delivery remains downstream and non-authoritative
- projection/translation remains subordinate to contracts
- terminal delivery stays replaceable

---

## 12. Candidate High-Entropy Operation Clusters

- main-host event wiring and chart-update request flow
- presentation-spine publication and host orchestration handoff
- data-fetch query shaping, table normalization, subtype resolution
- transform panel workflow composition across selection, compute, render, grid sync
- future programmable chart/controller seams for dynamic data generated from multiple datasets and operations, including multiple graphs on one chart surface
- render normalization, axis shaping, tooltip range shaping, chart helper spillover
- client-specific repair logic ownership classification
- authority/context/request/result ownership classification
- contract/boundary fan-out placement
- provider/consumer contract boundary enforcement versus delivery-routing renaming
- consumer/interaction separation from controller/event plumbing
- overlay definition versus overlay delivery ownership
- projection/translation components that hide semantic choice or execution policy
- vocabulary drift where feature, event, builder, rendering, diagnostics, or orchestration language masks promoted concepts

Already banked (do not reopen casually):
- bucket / interval / range determination
- transform preparation and result composition
- chart-context series preparation
- evidence / parity snapshot resolution and export composition

---

## 13. Validation and Smoke-Test Discipline

1. Run full solution build and `DataVisualiser.Tests` for every significant refactor.
2. Internal-only, behavior-preserving changes: no manual smoke required.
3. Live behavior/UI/render/export/theme/controller changes: targeted manual smoke required.
4. Each iteration records: tests run, smoke requirement, scope.

---

## 14. Non-Goals and Guardrails

- do not reduce capability or flexibility
- do not force false uniformity across honest outliers
- do not optimize for file-count alone
- do not reopen stabilized rendering seams casually
- do not let UI become a semantic authority
- do not let presentation convenience harden into architectural authority
- do not let backend quirks leak upward into orchestration
- do not mix unrelated subsystems in one iteration without justification
- do not use broad rewrites where a bounded slice can reveal the architecture more honestly
- do not generalize before 2-3 real slices prove a pattern
- do not confuse capability with feature, consumer with presentation, interaction with event wiring, composition with builder plumbing, overlay with rendering, provenance with diagnostics, or authority with orchestration

---

## 15. Success Criteria

**Phase 6 criteria (met):**
- similar operations have one obvious home
- the folder tree communicates responsibility more clearly than migration history
- true outliers are easier to identify and harder to ignore
- VNext reasoning engine is proven live for the first chart family with full signature-chain diagnostics
- no regression in rendering, parity, export, theme, orchestration, or flexibility behavior

**Phase 7 criteria (active):**
- new exploratory capabilities are reasoning-engine features, not chart-specific additions
- confidence and provenance are integral to new results, not optional decorations
- the reasoning engine's generality increases with each capability — programs become more consumer-agnostic
- no regression in existing chart delivery, evidence, or structural integrity
- Phase 7 advances the enhanced architecture through the forward stages in Section 8.5, using the architectural vocabulary as concept grammar and without reopening Phase 5/6 closure tracks
- the provider/consumer boundary is proven as an enforceable seam rather than a naming layer over delivery routing

---

## Appendix A. Programmable Chart Platform Direction

This appendix captures how the intended standardized programmable chart platform fits the immutable foundation documents. It is interpretive, not authoritative. Absorbed from the former `PROGRAMMABLE_CHART_PLATFORM_FOUNDATIONAL_ALIGNMENT_NOTE.md`.

### A.1 Interpreted Fit With Foundational Law

The intended future capability is:

- standardized graph parent controllers and shared chart-host affordances
- transform-style programmability across chart families
- explicit selection of submetrics and derived operations
- rendering of one or more derived result sets on a qualified chart surface

This fits the foundation documents provided the following remain true:

1. Canonical semantics remain upstream and authoritative.
2. Chart programs do not assign or alter meaning.
3. Result-set composition is derived or interpretive by default, not canonical.
4. Multiple result sets preserve explicit provenance and reversibility.
5. Controller standardization does not turn the UI into a semantic authority.
6. Backend qualification remains required before programmable chart behavior is treated as safe on a given surface.

### A.2 Layer Interpretation

The standardized programmable chart direction spans these layers:

- `truth`: unchanged and protected
- `derived`: explicit operations over canonical inputs
- `interpretive`: overlays, confidence-aware handling, optional comparative views
- `contract/boundary`: program, delivery, interaction, view, and multi-result contracts
- `projection/translation`: adapters, projectors, builders, selectors, and resolvers that cross explicit seams without defining meaning
- `consumer/interaction`: standardized graph parent controllers, shared option/toggle surfaces, consumer-local state
- `terminal delivery`: qualified surfaces that can display or transport one or more result sets safely

It must not collapse back upward into normalization, canonical identity, or semantic law.

### A.3 Architectural Implications

1. Chart-program definition should become a first-class downstream concept, separate from rendering and separate from semantics.
2. Rendering contracts should avoid permanent single-result assumptions.
3. Parent-controller convergence should be capability-driven, not forced by appearance alone.
4. Special-case transform programmability should eventually become a reusable capability, not remain trapped in one controller lineage.
5. Multi-result rendering must expose provenance clearly enough that the user can tell what each result set represents and how it was produced.
6. The first live VNext vertical slice (April 2026) proves request → snapshot → program → delivery for the Main chart family, which is the foundational execution shape that programmable chart composition will eventually build upon.
7. Transform decomposition completed in Phase 6 should preserve eventual controller capability for dynamic data generation from multiple datasets and operations, including rendering multiple graphs in a single qualified chart.

### A.4 Open Questions

The following deserve future design work but are not answered here:

1. What is the formal model for a chart program?
2. Which operations belong to derived computation versus interpretive overlays?
3. How should multi-result provenance be shown in the UI?
4. Which chart capability families truly support programmable multi-result composition versus only a subset of it?
5. How should persistence behave for session-scoped versus saved chart programs?

### A.5 Current Recommendation

The legibility work has earned the right to proceed. Phase 7 is the sanctioned home for programmable composition, confidence-aware reasoning, overlays, and exploratory capabilities. Each capability should be built as a reasoning-engine feature first, then delivered to chart surfaces and future consumers through explicit downstream contracts. Do not implement the whole programmable platform in one pass — proceed through bounded slices that each strengthen the engine's generality and reinforce the enhanced containers: authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence.

---

## Appendix B. Conversation Branch Persistence Schemas

These YAML schemas define the structured data model for preserving context across conversation sessions without replaying full history. Absorbed from the former `Conversation_Branch_Data_Model.md`.

### B.1 Foundation Pack

```yaml
foundation_pack:
  authority_order:
    - Project Bible.md
    - SYSTEM_MAP.md
    - DataVisualiser-Architectural-Vocabulary.md
    - Project Roadmap.md
    - Project Overview.md
    - DataVisualiser_Subsystem_Plan.md
  references:
    - documents/Project Bible.md
    - documents/SYSTEM_MAP.md
    - documents/DataVisualiser-Architectural-Vocabulary.md
    - documents/Project Roadmap.md
    - documents/Project Overview.md
    - documents/DataVisualiser_Subsystem_Plan.md
  stable_truths:
    - canonical truth is upstream
    - downstream interpretation is explicit and reversible
    - delivery surfaces are non-authoritative
    - validation evidence outranks narrative
    - architectural vocabulary is the canonical grammar for promoted concepts and ownership containers
```

### B.2 Branch Context

```yaml
branch_context:
  branch_name: <string>
  branch_type: execution | analysis
  posture: conservative | balanced | aggressive
  objective: <string>
  scope: <string>
  ownership_container_focus: [<list>]
  promoted_concepts_in_play: [<list>]
  risk_tolerance:
    live_behavior_churn: low | medium | high
    temporary_instability: true | false
    broad_refactors_allowed: true | false
    smoke_deferral_allowed: true | false
  active_constraints:
    - <list of constraints>
```

### B.3 Current State Snapshot

```yaml
current_state_snapshot:
  captured_at_utc: <datetime>
  summary:
    done: [<list>]
    open: [<list>]
    blocked: [<list>]
    next: [<list>]
  evidence:
    build_status: <string>
    test_status: <string>
    manual_smoke_status: <string>
  known_outliers: [<file paths>]
```

### B.4 Iteration Record

```yaml
iteration_record:
  id: <string>
  objective: <string>
  slice:
    files: [<file paths>]
  changes: [<list>]
  validation:
    build: passed | failed
    tests: passed | failed
    manual_smoke_required: true | false
    manual_smoke_result: passed | failed | not_required
  promoted_to_state_snapshot: true | false
```

### B.5 Reload Packet

```yaml
reload_packet:
  foundation_pack_ref: foundation_pack
  branch_context_ref: branch_context
  current_state_snapshot_ref: current_state_snapshot
  active_documents: [<file paths>]
  architectural_vocabulary_loaded: true | false
  entry_instruction: <string>
```

### B.6 Analysis Proposal

```yaml
analysis_proposal:
  proposal_name: <string>
  branch_type: analysis
  objective: <string>
  assumptions: [<list>]
  expected_payoff: [<list>]
  risks: [<list>]
  status: speculative | accepted | rejected
```

### B.7 Branch Types

**Execution branch rules:**
- only current verified facts
- only bounded next steps
- only validated changes count as progress
- speculative redesign stays out unless accepted

**Analysis branch rules:**
- may challenge the current structure
- may propose high-risk alternatives
- must stay anchored to foundation docs and current code
- must mark all outputs as speculative, bounded, or next-cycle

### B.8 Compression Rules

```yaml
compression_rules:
  - stable architectural vocabulary belongs in foundation_pack only
  - stable truth belongs in foundation_pack only
  - transient reasoning belongs in branch conversation only
  - only accepted conclusions move into current_state_snapshot
  - only completed work moves into iteration_record
  - only current blockers and next steps belong in reload_packet
```

---

**End of DataVisualiser Subsystem Plan**
