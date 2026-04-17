# DATAVISUALISER SUBSYSTEM PLAN
**Status:** Active Architectural Execution Plan  
**Scope:** `DataVisualiser` hierarchy repair, boundary clarification, entropy reduction, subsystem consolidation, and VNext activation — **Phase 6 closed April 2026**
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `Project Roadmap.md`, and `Project Overview.md`  
**Supersedes:** `DataVisualiser_Consolidation_Plan.md` and `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md`  
**Last Updated:** 2026-04-13

---

## 1. Purpose

This document is the single active execution plan for `DataVisualiser` architectural work.

It consolidates:
- The completed Phase 5 architecture rehaul (historical execution record)
- The completed Phase 6 legibility-first consolidation cycle
- The completed VNext routing activation across active chart families (Phase 6.3)

The primary objectives are:

**Phase 6 (closed):** Make `DataVisualiser` legible, structurally coherent, and stable — achieved through hierarchy repair, irreducible-operation consolidation, VNext proof-of-architecture, and structural consolidation.

**Phase 7 (entry gate satisfied):** Expand the reasoning engine's capabilities — confidence-aware results, interpretive overlays, programmable composition — building each as a reasoning-engine feature that delivery surfaces consume, not as chart-specific additions.

**Phase 6.3 (closed):** VNext coverage has been widened to all active chart families so the standardized request/delivery path is now authoritative for fresh chart-family loads.

The reasoning engine is the center of the system. Charts and future consumers are delivery targets of its output.

---

## 1.5 START HERE - Handoff and Execution Defaults

Use this section as the default handoff entry point in a new conversation.

Current state:

- Phase 6.3 VNext widening is complete — all active chart families route through the VNext reasoning engine for fresh data loads
- Phase 7 entry gate is satisfied — exploratory and confidence capabilities may proceed
- 471 source files, 688 tests, 56 architecture guardrails
- known debt: `MainChartsView` host concentration (~1,238 lines), adapter pattern variation

Current defaults:

- scope is `DataVisualiser` only
- posture is `Conservative-Pragmatic`
- one iteration must have one primary objective
- the default objective is now Phase 7 exploratory capability expansion, with VNext family widening (6.3) as parallel work
- safely coupled slices are allowed only when they already share a real contract / host / route / responsibility pattern
- validation must happen on every significant refactor
- if live behavior changes, halt after automated validation and request targeted manual smoke before continuing
- assume single-maintainer execution with limited time
- prefer slices that can be completed in `1-3` focused sessions
- do not open more than one live-behavior risk front at a time
- reliability, performance tolerance, and legibility outrank theoretical purity

Supporting navigation aids:

- `SYSTEM_MAP.md` Appendix A (presentation pipeline spine)

Authority order:

1. `Project Bible.md`
2. `SYSTEM_MAP.md`
3. `Project Roadmap.md`
4. `Project Overview.md`
5. this document

Default validation commands:

1. `dotnet build DataAnalyser.sln -c Debug`
2. `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`

Default smoke rule:

1. no manual smoke for internal-only structural cleanup
2. targeted manual smoke only when live UI / render / export / theme / controller behavior is changed

---

## 2. Architectural North Star

`DataVisualiser` is part of a wider canonical data reasoning platform. Within that wider system, the reasoning engine is the architectural center — it produces analytical programs that delivery surfaces consume. The system should evolve toward:

- canonical or downstream-safe data enters through explicit boundaries
- the reasoning engine composes derived results, transforms, comparisons, and overlays as inspectable, replayable analytical programs
- every result carries explicit provenance, confidence context, and reasoning path
- orchestration coordinates execution without becoming a semantic authority
- rendering and delivery infrastructure translate already-defined program output into backend-safe or consumer-safe behavior
- concrete clients (charts, reports, APIs, future consumers) become delivery surfaces rather than architectural authorities
- the VNext reasoning engine (`ReasoningSessionCoordinator`) progressively replaces legacy host-authoritative orchestration with explicit request → snapshot → program → delivery semantics

---

## 3. Binding Constraints

All actions must preserve:

1. Canonical semantic authority in normalization/CMS layers.
2. Downward-only authority flow (`Truth -> Derivation -> Interpretation -> Presentation`).
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
8. Future-extension readiness for interpretive overlays, confidence annotation, and structural exploration.
9. Rendering-backend extensibility without reworking semantic/application logic.
10. Eventual transform-style programmability and multi-result-set rendering on qualified chart surfaces.

Capability retirement rule: a capability may be removed only if explicitly retired by higher-authority documents or replaced with a validated equivalent.

---

## 5. Current Shape Snapshot (April 2026)

Current observed shape:

- `471` C# source files (Phase 6.6 audit baseline + VNext widening coordinators, milestone recorder, cleanup consolidation, shared tab-host extraction, route/capability seams, admin workflow extraction, strategy parity validation extraction, tooltip formatting split, and shared UI-busy lease)
- `688` automated tests passing
- VNext routing active for all current chart families, with automatic legacy fallback
- evidence/export boundary decomposed into standalone DTOs, diagnostics builder, and export orchestrator

Current major concentration points:

- `UI/MainChartsView.xaml.cs` (~1,238 lines)
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs` (~719 lines)
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
- `UI/MainHost/Evidence/MainChartsEvidenceExportService.cs` (~131 lines, reduced from 1,209)
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` (~220 lines, reduced from ~857)
- `UI/Charts/Presentation/TransformRenderCoordinator.cs` (~100 lines)
- `UI/Charts/Presentation/TransformWorkflowCoordinator.cs` (~50 lines)
- `UI/Charts/Presentation/TransformDataResolutionCoordinator.cs` (~171 lines)
- `UI/Charts/Presentation/TransformOperationExecutionCoordinator.cs` (~106 lines)
- `Core/Rendering/Helpers/ChartTooltipFormattingHelper.cs` (~42 lines, reduced from ~464)
- `Core/Services/BaseDistributionService.cs` (~296 lines)
- `Core/Rendering/Engines/ChartRenderEngine.cs` (~452 lines)
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` (~197 lines)
- `UI/Charts/Presentation/BarPieRenderModelBuilder.cs` (~259 lines)

Current read:

- the VNext reasoning engine is live for all current chart families, proving request -> snapshot -> program -> delivery beyond scaffolding
- VNext main-family route eligibility is centralized in `VNextChartRoutePolicy`, keeping routing and `SupportsOnlyMainChart` diagnostics explicit
- the evidence boundary is clean: DTOs, diagnostics, and export orchestration are separate concerns
- evidence export now carries an explicit `ExportScope`; the Charts tab exports as `Charts`, and the Syncfusion tab uses the same `MainChartsEvidenceExportService` as `Syncfusion` instead of a stub service
- tab switches are recorded as `TabSwitched` session milestones through the shared view-model context
- targeted manual smoke is still required for the latest shared tab-shell layout hardening: confirm both chart-tab layouts still render and operate correctly, and confirm the Admin tab renders and behaves correctly inside the generic workspace shell
- runtime-path tracking distinguishes VNext from legacy loads with full signature-chain diagnostics
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
- Evidence boundary decomposed: `MainChartsEvidenceExportService` (1,209 → 131 lines) split into `EvidenceExportModels` (21 DTOs), `EvidenceDiagnosticsBuilder`, and export orchestration
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
- Net file reduction: -7 files; 609 tests passed at closure; later shared-panel/evidence-scope hardening, tab-shell extraction, route-policy extraction, transform layout capability isolation, CMS decision extraction, admin workflow extraction, strategy parity validation extraction, tooltip formatting split, and shared UI-busy lease bring the current lane to 688 tests

**Phase 6.6 (Architecture audit and baseline refresh):** Closed.

Audit baseline (April 2026):
- 451 C# source files, ~36,900 lines of code
- 170 test files, 688 automated tests passing
- 56 architecture guardrail tests enforcing structural contracts

**1. To what extent have the Phase 6 objectives been met?**

Materially met. The hierarchy is trustworthy: similar responsibilities have one obvious home, repeated operations are centralized, and the remaining debt is explicit rather than hidden. Specifically:
- Irreducible operations (frequency binning, transform computation, series preparation, smoothing, timeline alignment, bucket aggregation) are each owned by a single dedicated helper, locked by architecture guardrails (6.1)
- Truth/derivation/orchestration/delivery boundaries are enforced through rendering contracts, orchestration pipelines, and explicit stage separation (6.2)
- Request/delivery standardization has real proof through live VNext routing across all active chart families, automatic legacy fallback, runtime-path tracking, and signature-chain diagnostics in evidence exports (6.3)
- Named outliers have been materially reduced: `MainChartsEvidenceExportService` (1,209→131), `TransformDataPanelControllerAdapter` (857→257), `BaseDistributionService` (612→296), `BarPieChartControllerAdapter` (503→197), `ChartRenderEngine` (452→333), `DataFetcher` decomposed into focused query groups (6.4)
- Physical hierarchy is clean: namespace-folder alignment verified, micro-types consolidated, `UI/MainHost/` decomposed into `Evidence/`, `Export/`, `Coordination/` sub-namespaces (6.5, 6.7)

**2. What gaps remain?**

These are accepted as known debt, not open work:
- `MainChartsView.xaml.cs` (~1,238 lines) remains the largest host concentration point. Its remaining responsibilities are genuinely host-level (wiring coordinators, forwarding events, managing host lifecycle). Further decomposition is possible but increasingly behavior-adjacent rather than structural.
- `SyncfusionChartsView.xaml.cs` (~719 lines) is a parallel host with similar characteristics, lower priority.
- VNext covers all active chart families through either the main-family route or per-family fresh-load routes. Future VNext work is capability expansion, not Phase 6 closure work.
- Controller adapters share structural patterns but their differences are genuine domain variation. A deeper base class was evaluated and rejected as overengineering (documented in the Phase 6.7 plan).
- 20+ coordinators have thin shared patterns (guard → execute) that do not justify inheritance.

**3. How would remaining objectives be reached in the next cycle?**

- `MainChartsView` host reduction: extract one more coordinator per session when a clear host-level responsibility boundary is identified (behavior-adjacent, requires targeted smoke)
- VNext widening: extend `VNextMainChartIntegrationCoordinator` to cover additional chart families one at a time, each with its own smoke verification
- Controller adapter convergence: defer to Phase 8 (UI consolidation) when Phase 7 has established what the standardized graph host surface looks like

**Global Phase 6 Closure Assessment:**

| Condition | Met? | Evidence |
|-----------|------|----------|
| 1. Similar responsibilities have one obvious home | Yes | Irreducible operations locked by guardrails; evidence/export/coordination in dedicated sub-namespaces; rendering contracts per capability family |
| 2. Repeated irreducible operations no longer sprawl | Yes | Frequency binning, transform computation, series preparation, smoothing, bucket aggregation each have one owner |
| 3. Truth/derivation/orchestration/delivery seams are clearer | Yes | VNext reasoning engine live for main family; rendering contracts enforce backend qualification; evidence boundary decomposed |
| 4. Remaining outliers are explicit, bounded, and visible | Yes | `MainChartsView` (host gravity), `SyncfusionChartsView` (parallel host), adapter pattern variation — all documented, all bounded |
| 5. Current capabilities preserved or replaced | Yes | 688 tests pass; all chart families render; evidence exports include runtime-path tracking and tab-scoped export metadata; no regressions |

**Phase 6 is closed.** All active chart families now route through the VNext reasoning engine for fresh data loads. Phase 7 entry gate is satisfied — new capabilities may proceed.

**Phase B host spine decomposition (banked slices):**
- Export trigger extraction → `MainChartsViewEvidenceExportCoordinator`
- Data-loaded refresh → `MainChartsViewDataLoadedCoordinator`
- Selection lifecycle stabilization → batched state updates in `MainWindowViewModel` and `SubtypeSelectorManager`
- Request-driven load → immutable `MetricLoadRequest` with `LoadRequestSignature`
- MainHost sprawl tightening → trivial micro-carriers folded into primary owners

---

## 7. Primary Mandate

Phase 6 established a trustworthy hierarchy. The primary mandate is now twofold:

**Phase 7 — Exploratory capability expansion:**
- add interpretive overlays, confidence-aware views, and programmable multi-result composition
- build on the proven VNext request/delivery architecture rather than extending the legacy path
- each new capability must respect canonical boundaries and not reintroduce exception-driven structure

**Phase 6.3 continuation - VNext family widening (closed):**
- VNext coverage has been extended to Distribution, WeekdayTrend, Transform, and Bar/Pie one family at a time
- each widened family has its own program kind, projection/runtime tracking, and automatic legacy fallback
- the architectural pattern is proven; future work is capability expansion, not Phase 6 discovery

In practice:

1. New capabilities should compose over existing canonical and derived views, not redefine them.
2. Each exploratory feature should have an explicit home in the module buckets below.
3. Future VNext changes should preserve the established per-family fallback pattern and proceed only as bounded capability slices.
4. Confidence and interpretive overlays are annotations, not mutations — they must be reversible and non-authoritative.
5. Work should reduce future operator burden.
6. The hierarchy established in Phase 6 should be preserved — new code should follow the existing structural patterns rather than introducing parallel ones.

---

## 8. Target Module Buckets

These are the target responsibility buckets for evaluating any slice:

1. **Data Access and Intake Facades** — retrieval contracts, repository facades
2. **Canonical / Context Handoff** — context objects bringing canonical data into `DataVisualiser`
3. **Derived and Transform Kernel** — shared algebraic operations, interval/bucket/range logic, smoothing/alignment
4. **Analytical Programs / Presentation Planning** — declared result composition, selected-series shaping, multi-result intent, confidence context; chart programs are the chart-oriented specialization of this broader model
5. **Orchestration and Coordinators** — context building, execution routing (VNext/legacy), render invocation handoff, evidence initiation
6. **Rendering Capability Families** — capability contracts, backend qualification, route/probe/host logic
7. **UI-Agnostic Controllers and Hosts** — standardized controller behavior, host coordination
8. **Client Adapters and Surfaces** — `LiveCharts`, `Syncfusion`, future consumers
9. **Evidence and Diagnostics** — parity exports, reachability/evidence generation, runtime-path diagnostics

---

## 9. Governing Legibility Cycle

All consolidation work should follow this cycle:

1. **Identify Entropy** — find where the hierarchy stops explaining itself
2. **Constrain / Re-home** — move responsibilities toward clearer owners
3. **Consolidate Irreducibles** — give similar operations one obvious home
4. **Standardize Proven Patterns** — standardize only after 2-3 real consumers prove a shape
5. **Retire / Remove / Clean** — remove superseded wrappers and residue
6. **Validate / Re-measure** — full build, tests, targeted smoke where behavior changed

---

## 10. Trackable Phase Plan

### 10.1 Phase A — Re-Baseline (COMPLETED)

Hierarchy baseline refreshed. Current outlier map remains valid.

### 10.2 Phase B — Host Spine Decomposition (CLOSED)

Multiple slices banked (export, data-loaded, selection stabilization, request-driven load, MainHost tightening, 10 dedicated coordinators extracted). Remaining host gravity in `MainChartsView` is intentional debt — genuinely compositional, not mixed-responsibility.

### 10.3 Phase C — Outlier Service Decomposition (CLOSED)

All named outliers materially reduced:
- `MainChartsEvidenceExportService`: 1,209 → 131 lines
- `TransformDataPanelControllerAdapter`: 857 → 257 lines (split across 6 coordinators)
- `BaseDistributionService`: 612 → 296 lines
- `BarPieChartControllerAdapter`: 503 → 197 lines
- `ChartRenderEngine`: 452 → 333 lines (dead-delegation residue cleaned)
- `DataFetcher`: decomposed into focused query groups (46-line facade)

The tooltip formatting helper outlier has been split: `ChartTooltipFormattingHelper` is now a 42-line facade over focused pair, stacked, cumulative, title parsing, value formatting, and overlay filtering helpers.

### 10.4 Phase D — Delivery and Rendering Spillover Simplification (CLOSED)

`ChartTooltipFormattingHelper` (464 → 42 lines) now delegates pair formatting, stacked totals, cumulative reconstruction, value extraction, series-title parsing, and overlay filtering to focused helpers. `ChartUpdateCoordinator` and vendor seams remain stable.

### 10.5 Phase E — Architecture Audit and Next-Cycle Gate

Measure concentration reduction. List what became more legible. Name the next cycle from remaining outliers.

Current baseline snapshot (April 2026):

- host responsibilities in `MainChartsView` are materially thinner, but it remains the largest composition concentration point
- transform workflow is now split across selection, resolution, execution, operation-state, milestone, and render-handoff seams; the remaining debt is mostly workflow composition rather than mixed utility logic
- `BaseDistributionService` is no longer a mixed computation/render monolith; the remaining debt is narrower strategy/render coordination
- live VNext routing is stable enough across active chart families to count as real architectural proof, not scaffolding
- the remaining major outliers are now explicit enough to define the next cycle without guesswork

The audit must answer these three questions directly:

1. Given the current state of the code, to what extent have the outlined objectives of the plan been met?
2. What gaps, inefficiencies, risks, or missed opportunities remain?
3. How would you reach the remaining objectives in the next major cycle?

Any next-cycle proposal must follow the governing iteration flow (Section 9) or justify the deviation. If something cannot reasonably be completed in one major cycle, that limitation must be stated explicitly.

### 10.6 VNext Activation (CLOSED — Parallel Track)

VNext activation is proven across active chart families. Preservation rules:
- Preserve the current `Main + Normalized + Diff/Ratio` live slice with evidence and targeted smoke as the bounded family baseline
- Preserve the independent per-family VNext routes for Distribution, WeekdayTrend, Transform, and BarPie with automatic legacy fallback
- Treat future VNext work as Phase 7 capability expansion unless it repairs a concrete regression

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

## 11. Current Priority Outliers

- `UI/MainChartsView.xaml.cs` — largest host concentration point
- `Core/Data/Repositories/DataFetcher.cs` — query shaping, normalization, subtype resolution
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` — remaining transform workflow and chart/grid handoff concentration
- `Core/Services/BaseDistributionService.cs` — computation + rendering mixed
- `Core/Rendering/Engines/ChartRenderEngine.cs` — render normalization, axis shaping, tooltip range
- `Core/Rendering/Helpers/ChartHelper.cs` — scattered tooltip/axis/clearing helpers
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` — rendering family concentration

---

## 12. Candidate High-Entropy Operation Clusters

- main-host event wiring and chart-update request flow
- presentation-spine publication and host orchestration handoff
- data-fetch query shaping, table normalization, subtype resolution
- transform panel workflow composition across selection, compute, render, grid sync
- future programmable chart/controller seams for dynamic data generated from multiple datasets and operations, including multiple graphs on one chart surface
- render normalization, axis shaping, tooltip range shaping, chart helper spillover
- client-specific repair logic ownership classification

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
- do not let backend quirks leak upward into orchestration
- do not mix unrelated subsystems in one iteration without justification
- do not use broad rewrites where a bounded slice can reveal the architecture more honestly
- do not generalize before 2-3 real slices prove a pattern

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
- VNext family widening (6.3) proceeds in parallel without blocking capability expansion

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
- `UI`: standardized graph parent controllers, shared option/toggle surfaces
- `rendering infrastructure`: qualified surfaces that can display one or more result sets safely

It must not collapse back upward into normalization, canonical identity, or semantic law.

### A.3 Architectural Implications

1. Chart-program definition should become a first-class downstream concept, separate from rendering and separate from semantics.
2. Rendering contracts should avoid permanent single-result assumptions.
3. Parent-controller convergence should be capability-driven, not forced by appearance alone.
4. Special-case transform programmability should eventually become a reusable capability, not remain trapped in one controller lineage.
5. Multi-result rendering must expose provenance clearly enough that the user can tell what each result set represents and how it was produced.
6. The first live VNext vertical slice (April 2026) proves request → snapshot → program → delivery for the Main chart family, which is the foundational execution shape that programmable chart composition will eventually build upon.
7. Transform decomposition in Phase 6 should preserve eventual controller capability for dynamic data generation from multiple datasets and operations, including rendering multiple graphs in a single qualified chart.

### A.4 Open Questions

The following deserve future design work but are not answered here:

1. What is the formal model for a chart program?
2. Which operations belong to derived computation versus interpretive overlays?
3. How should multi-result provenance be shown in the UI?
4. Which chart capability families truly support programmable multi-result composition versus only a subset of it?
5. How should persistence behave for session-scoped versus saved chart programs?

### A.5 Current Recommendation

The legibility work has earned the right to proceed. Phase 7 is the sanctioned home for programmable composition, confidence-aware reasoning, and exploratory capabilities. Each capability should be built as a reasoning-engine feature first, then delivered to chart surfaces and future consumers through explicit downstream contracts. Do not implement the whole programmable platform in one pass — proceed through bounded slices that each strengthen the engine's generality.

---

## Appendix B. Conversation Branch Persistence Schemas

These YAML schemas define the structured data model for preserving context across conversation sessions without replaying full history. Absorbed from the former `Conversation_Branch_Data_Model.md`.

### B.1 Foundation Pack

```yaml
foundation_pack:
  authority_order:
    - Project Bible.md
    - SYSTEM_MAP.md
    - Project Roadmap.md
    - Project Overview.md
    - DataVisualiser_Subsystem_Plan.md
  references:
    - documents/Project Bible.md
    - documents/SYSTEM_MAP.md
    - documents/Project Roadmap.md
    - documents/Project Overview.md
    - documents/DataVisualiser_Subsystem_Plan.md
  stable_truths:
    - canonical truth is upstream
    - downstream interpretation is explicit and reversible
    - delivery surfaces are non-authoritative
    - validation evidence outranks narrative
```

### B.2 Branch Context

```yaml
branch_context:
  branch_name: <string>
  branch_type: execution | analysis
  posture: conservative | balanced | aggressive
  objective: <string>
  scope: <string>
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
  - stable truth belongs in foundation_pack only
  - transient reasoning belongs in branch conversation only
  - only accepted conclusions move into current_state_snapshot
  - only completed work moves into iteration_record
  - only current blockers and next steps belong in reload_packet
```

---

**End of DataVisualiser Subsystem Plan**
