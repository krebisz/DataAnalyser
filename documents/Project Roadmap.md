# PROJECT ROADMAP
**Status:** Sequencing and Execution Authority  
**Scope:** Phase ordering, execution dependencies, closure criteria, and evolutionary gating  
**Authority:** Subordinate to `Project Bible.md` and `SYSTEM_MAP.md`  
**Operational Execution Source:** `DataVisualiser_Subsystem_Plan.md` for the active hierarchy-reconciliation and VNext activation work (consolidates the former `DataVisualiser_Consolidation_Plan.md` and `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md`)  
**Last Updated:** 2026-04-13  
**Change Note:** Phase 6 is closed except for `6.3` (VNext widening), which remains open until all active chart families route through the VNext reasoning engine. All other sub-phases (`6.1`, `6.2`, `6.4`–`6.7`) closed, all 5 global closure conditions met. Phase 7 entry gate is satisfied — new capabilities may proceed in parallel with VNext family widening.

---

## 1. Purpose

This roadmap defines how the project advances safely over time.

It answers:

- what is truly closed
- what is implemented but not yet safely closed
- what is open now
- what is blocked
- what may proceed next
- what must not proceed yet

It does not define semantic law.  
It defines order, discipline, gating, and closure.

If a conflict exists:
1. `Project Bible.md` wins on architectural law.
2. `SYSTEM_MAP.md` wins on structural direction.
3. This roadmap wins on sequencing and closure discipline.
4. `DataVisualiser_Subsystem_Plan.md` operationalizes the active delivery-side hierarchy reconciliation and VNext activation permitted by this roadmap (consolidates the former consolidation plan and rehaul execution plan).

---

## 2. Binding Roadmap Doctrine

This roadmap is governed by the following rules:

1. Truth comes before insight.
2. Phases exist to protect trust and correctness, not to accelerate delivery.
3. A closure claim without current executable evidence is not closure.
4. Unreachable or unobservable execution is treated as non-existent.
5. Exploratory power and downstream flexibility must be explicitly staged above semantic authority and only after the hierarchy is legible enough to support them safely.
6. Architecture legibility is phase work whenever hierarchy drift, responsibility sprawl, or exception-driven structure threatens safe future extension.
7. Capability preservation is the default: refactor may not silently remove current abilities.
8. Tactical stabilization is permitted, but stabilization is not the same as architectural closure.
9. Multiple chart backends may coexist intentionally, but only to strengthen modular rendering boundaries.
10. No backend is treated as production-safe for a capability family until it has passed explicit qualification for that capability family.
11. Future enhancement ideas are welcome, but they do not enter active work unless intentionally assigned to a phase and layer.
12. For a single-maintainer project, bounded slices with immediate maintenance payoff take priority over broad theoretical cleanup.
13. The roadmap may be executed through small opportunistic slices as long as each slice preserves truth, evidence discipline, and structural direction.

---

## 3. Strategic Reconciliation

The foundational project intent remains unchanged:

- preserve truth
- assign meaning declaratively
- compute deterministically
- make interpretation explicit and reversible
- surface uncertainty without mutating canonical reality
- support human reasoning rather than replace it

What has changed is the roadmap's understanding of what must happen before the project can safely evolve further.

The architecture rehaul established that the project is not yet structurally ready for unconstrained future growth. The most important current realities are:

1. The truthful canonical foundation is real and still authoritative.
2. The system has meaningful implementation progress across migration, visualization, and interaction.
3. Structural weaknesses now threaten safe extension if left untreated:
   - UI composition bottlenecks
   - Core/UI leakage
   - rendering fragmentation without one stable capability contract
   - stale or misaligned closure evidence
   - mutable global runtime behavior
   - concentrated data-access and orchestration responsibilities
   - repeated irreducible operations spread across unrelated locations
   - hierarchy that reflects migration history more than stable responsibility
   - uneven assurance quality
4. Multi-backend chart support is not a cosmetic preference. It is a deliberate architectural probe into whether rendering has actually been isolated from the rest of the application.
5. Standardized programmable chart hosts are intended future product behavior, but that future is unsafe unless controller convergence, rendering contracts, and orchestration seams are made explicit first.
6. Transform is the first real programmable-chart prototype; Phase 6 work must preserve a path to multi-dataset, multi-operation, multi-graph result delivery instead of hardening unary/binary special cases.

Therefore, architecture rehaul was not a side quest, and its successor work is not cosmetic cleanup.  
The next bridge is hierarchy legibility: the system must become coherent enough that true outliers are obvious and future capability can land without creating new exception-driven structure.

That said, the project is maintained by one person with limited time.
So the roadmap must remain ambitious in direction but conservative in execution:

- solve the most expensive current pain first
- prefer reliability and performance tolerance over elegance-only restructuring
- avoid multi-front refactors that create more coordination burden than value
- defer broad architectural inversions until a bounded slice proves they are the shortest path to relief

---

## 4. Status Model

Use these status labels literally:

- `CLOSED`: closure conditions are currently satisfied with present evidence
- `IMPLEMENTED / REVALIDATION REQUIRED`: implementation progress exists, but closure evidence is stale, incomplete, or structurally misaligned
- `OPEN / CURRENT CRITICAL PATH`: active phase that governs what may proceed next
- `PLANNED / BLOCKED`: defined future work that must not proceed until its gates are satisfied
- `DEFERRED`: intentionally not on the critical path

---

## 5. Phase Overview (Authoritative)

### Phase 1 - Baseline Stabilization  
**Status:** CLOSED

**Objective**
- Freeze and understand legacy behavior as a compatibility reference

**Closure Condition**
- Legacy behavior is stable and reproducible enough to serve as a bounded migration baseline

---

### Phase 2 - Canonical Semantics and Normalization  
**Status:** CLOSED

**Objective**
- Establish a single semantic authority and deterministic canonical foundation

**Closure Condition**
- Normalization and CMS production are deterministic, explainable, and authoritative

---

### Phase 3 - CMS Strategy Capability  
**Status:** CLOSED

**Objective**
- Make CMS-based computation available across active strategy families

**What is believed true**
- CMS-capable strategies exist across the active strategy families
- Major migration work has been implemented and revalidated with current March 2026 evidence

**Why closure is not currently accepted as final**
- This concern has been satisfied by current March 2026 reachability/parity exports and current green default test lanes
- Active closure scope excludes the retired live `Difference/Ratio` chart surface
- `Syncfusion` remains outside CMS reachability closure because its export is still explicitly `NotApplicable`

**Historical evidence references**
- January 2026 reachability artifacts have been retired from the repository. Current closure is based on April 2026 evidence and green default test lanes.

**Closure Condition**
1. All active strategies are CMS-capable.
2. All active strategies are reachable through live orchestration.
3. Path used is observable and test/assertion backed.
4. Current evidence artifacts exist in the repository-visible evidence path.
5. Closure is based on current March 2026 evidence artifacts in `documents/`, not historical January 2026 artifacts.

---

### Phase 3.5 - Execution Reachability and Observability  
**Status:** CLOSED

**Objective**
- Prove that CMS paths are actually exercised in live pipelines and not merely implemented in isolation

**Historical evidence references**
- January 2026 reachability artifacts have been retired from the repository. Current closure is based on April 2026 evidence and green default test lanes.

**Closure Condition**
1. CMS execution is observable for all active strategy families.
2. Bypass paths are either eliminated or explicitly justified.
3. Reachability evidence is current, reproducible, and generated through the approved evidence path.
4. Closure scope explicitly excludes the retired live `Difference/Ratio` chart surface and the still-unwired Syncfusion reachability export.

---

### Phase 4 - Consumer Adoption and Parity Closure  
**Status:** CLOSED

**Objective**
- Prove that CMS behavior is preserved through consumer-facing pipelines and parity obligations remain satisfied

**Historical evidence references**
- January 2026 reachability artifacts have been retired from the repository. Current closure is based on April 2026 evidence and green default test lanes.

#### Phase 4A - Core Strategy Parity  
**Status:** CLOSED

**Closure Condition**
- Parity tests pass deterministically for active strategy families with current evidence and current test lanes

#### Phase 4B - Transform Pipeline Parity  
**Status:** CLOSED

**Closure Condition**
- Transform outputs match required reference behavior across supported expressions with current evidence

#### Phase 4C - Temporal and Distribution Consumer Adoption  
**Status:** CLOSED

**Closure Condition**
- Temporal and distribution strategies remain reachable, parity-backed, and consumer-safe in live pipeline context

#### Phase 4D - Optional End-to-End Regression Protection  
**Status:** DEFERRED

**Objective**
- Add broader regression protection against orchestration and integration regressions

**Note**
- This idea remains valid, but its practical assurance role is now absorbed into the broader Phase 5 assurance and guardrail work

**Global Closure Condition for Phase 4**
1. Consumer-facing CMS paths are current, reachable, and parity-validated.
2. Historical closure claims have been refreshed with present evidence.
3. No active parity claim depends on missing or stale artifacts.
4. Closure scope excludes retired live consumer surfaces and keeps the current Syncfusion export limitation explicit rather than pretending it is parity-complete.

---

### Phase 5 - Architecture Rehaul and Backend Qualification  
**Status:** CLOSED

**Entry Context**
- The project has enough truthful and functional implementation progress to expose its next bottlenecks clearly.
- The next risk is no longer "can the system compute?" but "can it evolve safely without contaminating truth, orchestration, or rendering boundaries?"

**Purpose**
Phase 5 exists to make the system structurally safe for its intended future:

- exploratory and confidence-aware capability expansion
- continued CMS-first evolution
- safe UI growth
- support for multiple chart vendors and future rendering tools without semantic contamination
- eventual standardized programmable chart hosts and multi-result derived chart composition

**Execution Source**
- `DataVisualiser_Subsystem_Plan.md` Section 6.1 contains the historical completion record for this phase

#### Phase 5.1 - Closure Repair and Evidence Recovery

**Scope**
- repair evidence export path
- regenerate required reachability and parity artifacts
- eliminate stale closure claims
- synchronize roadmap, overview, and actual evidence reality

**Closure Condition**
- roadmap and overview claims are backed by present, reproducible evidence artifacts

#### Phase 5.2 - Composition and Boundary Reduction

**Scope**
- reduce `MainChartsView` as an orchestration bottleneck
- remove Core/UI and WPF leakage from higher layers
- split mixed helpers and misplaced rendering concerns
- avoid cementing prototype-era parent-controller divergence that later standardized chart hosts must undo

**Closure Condition**
- Core and orchestration semantics no longer depend on UI/view-level authority or vendor control knowledge

#### Phase 5.3 - Rendering Capability Contracts and Backend Qualification

**Scope**
- define rendering contracts by capability family rather than by vendor
- isolate vendor lifecycle, hover, animation, disposal, and visibility quirks behind backend adapters
- intentionally qualify current and future chart vendors against explicit capability slices
- treat multi-backend coexistence as qualification work, not architectural failure
- ensure the rendering boundary does not hard-code single-result assumptions that would block future programmable chart composition

**Required outputs**
- rendering capability contracts
- backend qualification matrix
- backend qualification probes / harnesses
- bounded classification of any tactical fallback still in use

**Closure Condition**
1. At least one end-to-end chart family renders through a backend-agnostic capability contract.
2. Supported backends are qualified per capability slice rather than assumed globally safe.
3. A new backend can be introduced for an existing chart family without changing semantic or orchestration logic.
4. Tactical backend fallbacks are either retired or explicitly bounded as temporary.

#### Phase 5.4 - Configuration, Data Access, and Orchestration Hardening

**Scope**
- remove mutable global runtime toggles from production flow
- put concrete data-fetch behavior behind narrow contracts
- separate context building, data retrieval, and render invocation
- move side effects such as notifications behind explicit seams
- separate chart-program/result composition from render execution so programmable charting can evolve without controller-specific orchestration

**Closure Condition**
- runtime behavior is scoped, testable, contract-driven, and no longer dependent on ad-hoc repository construction or global mutable state

#### Phase 5.5 - Assurance and Architectural Guardrails

**Scope**
- strengthen default test lanes
- add architecture-conformance checks
- separate unit and integration concerns properly
- enforce current-date evidence discipline
- prevent future vendor leakage above rendering adapters

**Closure Condition**
- the system can no longer silently drift back into the same structural failures without tests or guardrails catching it

**Global Closure Condition for Phase 5**
1. Capability preservation baseline is satisfied.
2. Evidence and documentation are synchronized.
3. Rendering capability contracts and backend qualification matrix exist.
4. Core/UI and orchestration/rendering boundaries are materially improved and test-enforced.
5. Current shipped capabilities are preserved or replaced with validated equivalents.
6. Residual tactical debt is explicit rather than accidental.

**Post-Closure Maintenance Discipline**
1. Preserve the qualified seams introduced by the rehaul unless a new bounded step deliberately replaces them.
2. Treat remaining large concentration points as explicit intentional debt, not as hidden unfinished migration work.
3. Keep the current Syncfusion export limitation honest until true Syncfusion reachability evidence is deliberately implemented.
4. Do not generalize further just to reduce file count; only generalize patterns proven in multiple real slices.
5. Revalidate future closure claims with present evidence rather than relying on historical refactor intent alone.
6. Keep `DataFileReader` active callers on capability facades and do not reintroduce direct `SQLHelper` usage in those caller layers.

---

### Phase 6 - Architectural Legibility and Concern Reconciliation  
**Status:** CLOSED (April 2026) — except 6.3 (VNext widening open)  
**Entry Gate:** Opened now that Phases 3, 3.5, 4, and 5 are closed under current evidence rules

**Purpose**
Phase 6 restores a coherent, trustworthy hierarchy before further capability expansion.

This phase exists because the current risk is no longer a missing feature.
It is a muddled architecture in which similar operations live in too many places, local exceptions accumulate into structure, and the project stops explaining itself to its own architect.

The goal is to make the system legible enough that:

- similar responsibilities have one obvious home
- truth, derivation, orchestration, delivery, and consumer boundaries are easier to inspect
- remaining large outliers are explicit rather than hidden in background noise
- future exploratory and programmable capability can land without multiplying exceptions

**Default bounded execution shape**
1. begin with one mapping iteration that refreshes the hierarchy around the current outliers
2. perform one or two irreducible-operation consolidation iterations before broad UI or folder churn
3. reconcile one mixed truth / derivation / delivery boundary
4. standardize one real downstream delivery seam only if earlier slices prove it
5. decompose one highest-value remaining outlier and realign the physical tree around the stabilized ownership model
6. close the cycle with an architecture audit and a fresh outlier baseline

#### Phase 6.1 - Irreducible Operation Consolidation

**Status:** CLOSED (April 2026)

**Scope**
- consolidate repeated low-level operations such as interval resolution, bucketing, binning, smoothing, alignment, selection, and other algebraic or structural helpers into one obvious home per concern
- remove parallel variations that differ only because of local history or controller-specific exception handling

**Constraints**
- no capability regression
- no silent semantic change
- provenance and declared semantics must remain intact

**Closure Condition**
- repeated irreducible operations no longer sprawl across unrelated layers, and any retained duplicates are explicitly justified

#### Phase 6.2 - Truth / Derivation / Delivery Boundary Reconciliation

**Status:** CLOSED (April 2026)

**Scope**
- clarify where raw, normalized, canonical, derived, overlay, request, and delivery concerns live
- keep access to lower-level views explicit, provenance-visible, and non-promoted
- remove places where consumer, controller, or rendering code smuggles semantic or transform responsibility upward

**Constraints**
- no new false hierarchy
- no semantic policing through UI or delivery layers
- no silent promotion of non-canonical views into canonical truth

**Closure Condition**
- the project's main bands are easier to explain, inspect, and defend in code

#### Phase 6.3 - Request, Consumer, and Delivery Standardization

**Scope**
- standardize request and presentation-planning seams where real convergence exists
- make orchestration handoff and delivery contracts more uniform
- reduce "programming by exception" in controller and consumer flows

**Constraints**
- do not force false abstractions
- keep consumer-specific behavior explicit where it is genuinely different
- preserve qualified rendering and evidence boundaries

**Current Progress (April 2026)**
- First live VNext vertical slice widened: `MetricLoadCoordinator` now routes the main chart family (`Main`, `Normalized`, `Diff/Ratio`) through `VNextMainChartIntegrationCoordinator` -> `ReasoningSessionCoordinator` -> `LegacyChartProgramProjector`
- Automatic legacy fallback remains for `Distribution`, `WeekdayTrend`, `Transform`, `BarPie`, and any VNext load failure
- Fresh `ReasoningSessionCoordinator` per load attempt — no shared session state across loads
- `MainChartDisplayMode` propagated through VNext path — Regular/Summed/Stacked behavior preserved
- Evidence boundary decomposed: `MainChartsEvidenceExportService` split into `EvidenceExportModels` (21 standalone DTOs), `EvidenceDiagnosticsBuilder` (diagnostics assembly), and export orchestration
- `LoadRuntimeState` on `ChartState` tracks runtime path (`Legacy`/`VNextMain`), request/snapshot/program/projected-context signatures, and failure reason
- `EvidenceRuntimePath` and `VNextDiagnosticsSnapshot` emitted in evidence exports with signature-chain alignment flags
- `MainChartsView` now delegates session diagnostics, UI-surface diagnostics, subtype-selection bookkeeping, load/clear bookkeeping, resolution reset, zoom reset, startup, evidence export, data-loaded flow, chart update, and chart presentation through dedicated host seams
- metric-type list initialization, metric-type-change reset/reload, and subtype-loaded follow-up behavior are now shared between `MainChartsView` and `SyncfusionChartsView` through `ChartHostMetricSelectionCoordinator`
- `MainChartsView` host/controller-extras interaction, registry-wide controller resolution, and chart-surface startup/no-data presentation are now delegated through dedicated coordinators
- `TransformDataPanelControllerAdapter` now delegates subtype-selection interaction, operation state, execution, milestone recording, data resolution, and render/grid handoff through dedicated coordinators
- `BaseDistributionService` now delegates pure computation, simple-range assembly, series construction, axis shaping, and debug-summary formatting through dedicated helpers
- Smoke-verified with April 2026 exports: VNext path produces aligned signatures, legacy fallback produces correct state, all 8 parity strategies pass
- 609 automated tests pass in the current default full-solution lane

**Current evidence artifacts (April 2026):**
- `documents/reachability-20260411-093257.json` — legacy path, 3-series multi-metric, all 8 parity strategies passing
- `documents/reachability-20260411-093604.json` — VNext path, 2-series, CMS enabled, signature chain aligned
- `documents/reachability-20260411-093813.json` — VNext path, 2-series, CMS disabled, signature chain aligned

**Closure Condition**
- consumers ask for explicit result shapes through consistent seams rather than through special-case controller logic

#### Phase 6.4 - Outlier Decomposition

**Status:** CLOSED (April 2026)

**Scope**
- decompose large mixed-responsibility concentration points such as `MainChartsView`, `TransformDataPanelControllerAdapter`, `MainChartsEvidenceExportService`, `DataFetcher`, `BaseDistributionService`, and similar verified outliers
- split only where the resulting ownership is clearer than the current concentration
- preserve future programmable-chart/controller seams where the current outlier is an early prototype of broader chart capability

**Constraints**
- no decomposition for its own sake
- no churn that hides the same ambiguity behind more files
- validation discipline must stay intact

**Closure Condition**
- the remaining large outliers are reduced materially or explicitly bounded as intentional next-cycle debt
- transform/controller outlier work leaves the system more ready for reusable multi-input, multi-operation, multi-result chart programming

#### Phase 6.5 - Physical Hierarchy and Naming Realignment

**Status:** CLOSED (April 2026)

**Scope**
- align folders, namespaces, and names with stabilized responsibility boundaries
- remove leftover boilerplate micro-types only when ownership is already clear
- make the tree reveal architectural intent rather than migration history

**Constraints**
- do not optimize for file count alone
- do not rename broadly without structural payoff
- keep current capabilities reachable and understandable

**Closure Condition**
- the physical hierarchy is materially clearer and exposes true anomalies faster

#### Phase 6.6 - Architecture Audit and Baseline Refresh

**Status:** CLOSED (April 2026)

**Scope**
- measure the new hierarchy
- record remaining outliers and repeated debt patterns
- refresh current execution maps, success criteria, and architectural evidence

**Audit Record (April 2026)**
- 448 C# source files (~36,900 lines), 153 test files, 609 automated tests, 48 architecture guardrails
- Sub-phases `6.1`, `6.2`, `6.4`, `6.5`, `6.7` closed; `6.3` has live proof through VNext main-chart-family route with legacy fallback and export-backed signature-chain diagnostics
- All 5 global closure conditions assessed and met (full record in `DataVisualiser_Subsystem_Plan.md` Phase 6.6 section)
- Known debt carried to Phase 7: `MainChartsView.xaml.cs` (~1,401 lines, genuinely host-level), `SyncfusionChartsView.xaml.cs` (~775 lines, parallel host), VNext covers Main/Normalized/Diff/Ratio only, controller adapter pattern variation accepted as domain variation

**Closure Condition**
- the next cycle starts from an auditable baseline rather than from accumulated guesswork — **satisfied**

#### Phase 6.7 - Pre-Phase 7 Structural Consolidation

**Status:** CLOSED (April 2026)

**Scope**
- eliminate dead code, namespace mismatches, and scattered micro-types
- extract shared patterns duplicated across evidence evaluators
- decompose the largest flat folder (`UI/MainHost/`, 41 files) into logical sub-namespaces

**Completed Work**
- `IDistributionResultExtractor` deleted (0 implementations, 0 references)
- Syncfusion namespace corrected: `SyncfusionViews` → `Syncfusion` across 9 files
- 6 parity type files consolidated into `ParityTypes.cs`
- Rendering helpers merged: `ChartLabelFormatter` → `ChartSeriesLabelFormatter`, `TransformChartAxisLayout` → `TransformChartAxisCalculator`
- `EvidenceDataResolutionHelper` extracted: shared data-resolution and strategy cut-over resolution
- `UI/MainHost/` decomposed into `Evidence/` (15 files), `Export/` (6 files), `Coordination/` (20 files)
- Net -7 files; 609 tests pass; no behavior changes

**Closure Condition**
- structural sprawl is materially reduced and the codebase is primed for Phase 7 capability expansion

**Global Closure Condition for Phase 6**
1. Similar responsibilities have one obvious home often enough that the hierarchy becomes trustworthy again.
2. Repeated irreducible operations no longer sprawl silently across unrelated layers.
3. Truth, derivation, orchestration, delivery, and consumer seams are materially clearer and easier to defend.
4. Remaining outliers are explicit, bounded, and visible.
5. Current capabilities are preserved or replaced with validated equivalents.

---

### Phase 7 - Exploratory and Confidence Capability Expansion (Intentional)  
**Status:** PLANNED / ENTRY GATE SATISFIED  
**Entry Gate:** Phase 6 must restore enough hierarchy legibility that new power can be added without amplifying entropy — **satisfied** (Phase 6 closed April 2026)

**Purpose**
Phase 7 increases interpretive power without eroding truth or reintroducing exception-driven structure.

This phase remains the sanctioned home for exploratory capability, confidence-aware interpretation, and richer analytical views.
It is also the sanctioned home for standardized programmable chart composition once Phase 6 has earned the structural right to support it safely.

#### Phase 7.1 - Interpretive Visual Overlays (Render-Only)

**Scope**
- dynamic line colouring
- through-lines such as average, median, or reference markers
- enhanced hover and tooltip signalling
- legend-driven visibility toggles

**Constraints**
- render-only
- no effect on computation or CMS
- backend used for these overlays must already be qualified for the relevant chart capability slice

**Closure Condition**
- overlays are visually correct, reversible, provenance-neutral, and backend-safe for the capability slice in which they are used

#### Phase 7.2 - Confidence Annotation (Non-Destructive)

**Scope**
- statistical identification of atypical points
- confidence markers on charts and data views
- declared model and parameter visibility

**Constraints**
- annotation only
- canonical values unchanged
- confidence does not become semantics

**Closure Condition**
- confidence annotations are visible, explainable, reversible, and do not mutate truth layers

#### Phase 7.3 - Confidence-Aware Interpretation

**Scope**
- optional exclusion of low-confidence points from interpretive overlays
- optional attenuation or weighting in non-authoritative trend views
- explicit user control over confidence handling

**Constraints**
- applies only to interpretive layers
- never affects normalization, identity, or CMS construction
- provenance must remain visible

**Closure Condition**
- confidence-aware views are opt-in, transparent, reversible, and semantically non-invasive

#### Phase 7.4 - Programmable Chart Composition and Multi-Result Derived Views

**Scope**
- standardize graph parent controllers and shared option/toggle surfaces where capability semantics align
- allow transform-style programming across qualified chart families, including selected submetrics and unary, binary, ternary, or higher-order derived operations
- support rendering more than one derived result set on the same qualified chart surface where the chart family supports it
- support dynamic result generation from multiple datasets and operations, including multiple graphs within a single qualified chart
- preserve provenance and explicit derived identity for each result set

**Constraints**
- programmable chart composition is downstream of CMS and does not redefine semantics
- result sets remain derived or interpretive unless explicitly promoted by a separate declarative mechanism
- backend qualification must already cover the interactions and rendering behavior used by the programmable chart surface

**Closure Condition**
- standardized programmable chart hosts can express the current chart examples they are intended to subsume without semantic erosion
- multi-result derived rendering works on qualified chart families with visible provenance
- transform-style programmability is no longer isolated to a single special-case controller

#### Phase 7.5 - Structural and Relational Exploration

**Scope**
- scatter plots and clustering views
- compositional and hierarchical views
- pivot-relative inspection
- multi-resolution comparative views

**Constraints**
- non-authoritative
- no implicit promotion of structure into meaning
- rendering backend choice must remain behind qualified capability contracts

**Closure Condition**
- structural views are powerful, bounded, explicitly interpretive, and semantically non-promotable by default

#### Phase 7.6 - Rules-Based Option Gating

**Scope**
- declarative rules controlling UI option availability
- prevention of invalid or misleading combinations
- context-aware UI constraints

**Constraints**
- rules are constraints, not recommendations
- rules must be explainable
- rules must not encode semantic truth

**Closure Condition**
- rule effects are transparent, reversible, and clearly separate from semantics

**Global Closure Condition for Phase 7**
1. All exploratory features are explicitly layered above truth.
2. Confidence handling is visible and non-destructive.
3. Interpretive power increases without semantic erosion.
4. Programmable chart composition remains explicit, reversible, and provenance-preserving.
5. New exploratory features do not breach rendering-boundary or backend qualification rules.

---

### Phase 8 - UI, State, and Integration Consolidation  
**Status:** PLANNED / BLOCKED
**Entry Gate:** Phase 7 must advance far enough to justify broad UI/state consolidation without reopening structural uncertainty

**Objective**
- ensure the UI accurately reflects truth, interpretation, uncertainty, qualified rendering behavior, and standardized programmable chart capabilities without compensating for architectural weakness

**Closure Condition**
1. Qualified chart families share standardized parent controller/host behavior wherever capability semantics genuinely align.
2. UI state reflects truth, uncertainty, and programmable chart composition without distortion.
3. Integration behavior is predictable across supported chart families and qualified backends.
4. The UI no longer acts as a repair layer for missing architectural boundaries.

## 6. Rendering Backend Introduction Policy (Explicit)

Any new chart vendor or rendering tool must enter the project through Phase 5.3 discipline.

Required declaration before adoption:
1. capability family targeted
2. render and interaction requirements
3. lifecycle risks being qualified
4. probe or harness used to validate the backend
5. rollback path if the backend fails qualification

Prohibited:
1. introducing vendor-specific types into orchestration or semantic layers
2. assuming one successful chart proves backend-wide safety
3. treating a tactical workaround as backend qualification

This policy exists because backend plurality is a deliberate architectural learning tool in this project.

---

## 7. Handling Evolving Requirements (Explicit Policy)

Enhancement ideas are:

- welcome
- expected
- non-exhaustive

However, they do not auto-enter active work.

Every new idea must declare:
1. intended phase
2. intended layer (`truth`, `derived`, `interpretive`, `UI`, `rendering infrastructure`, or `governance/evidence`)
3. reversibility expectation
4. whether a rendering backend qualification step is required

This policy protects both long-term trust and long-term extensibility.

---

## 8. Current Critical Path (Authoritative)

1. Preserve the qualified seams produced by the completed Phase 5 rehaul.
2. Execute Phase 6 through bounded hierarchy-reconciliation slices that make the architecture more legible and the remaining outliers more obvious.
3. Keep residual debt explicit and bounded rather than allowing silent structural drift.
4. Revalidate future closure claims with present evidence and repository-visible artifacts.
5. Do not open exploratory expansion until the hierarchy is coherent enough to absorb it safely.
6. Introduce further generalization only when the codebase has earned it through repeated real slices.
7. Keep future phase entry explicit rather than reopening rehaul work implicitly.

Phase 7 must not be treated as open merely because ideas are ready.  
It opens only when the system is structurally able to absorb them safely.

---

## 8.5 Open Phase Summary

Remaining open phases:
1. `Phase 6 - Architectural Legibility and Concern Reconciliation`: `CLOSED (except 6.3 — VNext widening open)`
2. `Phase 7 - Exploratory and Confidence Capability Expansion`: `PLANNED / ENTRY GATE SATISFIED`
3. `Phase 8 - UI, State, and Integration Consolidation`: `PLANNED / BLOCKED`

Phase 6 sub-phase status:
- `Phase 6.1` - irreducible operation consolidation: **CLOSED**
- `Phase 6.2` - truth / derivation / delivery boundary reconciliation: **CLOSED**
- `Phase 6.3` - request, consumer, and delivery standardization: **OPEN** (VNext widening ongoing)
- `Phase 6.4` - outlier decomposition: **CLOSED**
- `Phase 6.5` - physical hierarchy and naming realignment: **CLOSED**
- `Phase 6.6` - architecture audit and baseline refresh: **CLOSED**
- `Phase 6.7` - pre-Phase 7 structural consolidation: **CLOSED**

**Phase 6 is closed except for 6.3.** All 5 global closure conditions met. 6.3 remains open until all active chart families (Distribution, WeekdayTrend, Transform, Bar/Pie) route through the VNext reasoning engine.

Major next steps in sequence:
1. `Phase 6.3` - widen VNext to remaining chart families (may proceed in parallel with Phase 7)
2. `Phase 7` - exploratory and confidence capability expansion (entry gate satisfied)
3. `Phase 8` - UI, state, and integration consolidation after Phase 7 is sufficiently advanced

---

## 9. Summary

- Early phases built truthful foundations.
- Middle phases built CMS-capable behavior, but some closure claims now require revalidation.
- Phase 5 is the completed bridge between what the system already does and what it is intended to become.
- Phase 6 is now the active sanctioned home of hierarchy repair, legibility, and concern reconciliation.
- Phase 7 remains the sanctioned home of exploration, confidence, and richer interpretive power, but only after Phase 6 earns that right.
- Multi-backend rendering support is not incidental. It is part of the deliberate architectural learning process.
- Closure means present truth, present evidence, and present structural safety.

This roadmap defines how the project grows without lying to itself, and without losing the flexibility it is explicitly trying to earn.

---

**End of Project Roadmap**

