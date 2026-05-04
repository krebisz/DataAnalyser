# PROJECT ROADMAP
**Status:** Long-Term Sequencing and Evolution Authority  
**Scope:** Phase ordering, execution dependencies, closure criteria, evolutionary gating, and long-term progression before and after target architecture implementation  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, and `DataVisualiser-Architectural-Vocabulary.md`  
**Operational Execution Sources:** current implementation plans and subsystem plans provide execution detail only; they do not redefine this roadmap's long-term sequencing doctrine  
**Architectural Grammar Reference:** `DataVisualiser-Architectural-Vocabulary.md` for promoted concepts, ownership containers, target hierarchy, and do-not-confuse distinctions  
**Last Updated:** 2026-04-30
**Role Realigned:** 2026-05-04
**Historical Phase 19 Closure Note:** VNext/legacy load routing is now delegated from `MetricLoadCoordinator` into `VNextMetricLoadRouter`, closing the remaining Phase 19 routing item.
**Historical Update:** Phase 7 was active at the time of the April 2026 update. Migration Plan Phases 14–22 are complete. The VNext spine is proved end-to-end: a new MovingAverage capability enters through the target seams and is consumed by two independent consumers (chart + API) without touching old hubs or legacy bridges. The UseRenderPlanAdapter dual-path is retired; explicit CapabilityContracts are threaded across all chart families. The accepted architectural migration progress estimate is approximately 70–75% complete, with a working estimate of ~72% (updated from ~68% after Phases 19–22).
**Historical Change Note:** Phase 6 is fully closed — all sub-phases (`6.1`–`6.7`) closed, including `6.3` VNext family widening. All active chart families now have VNext-compatible request/program support and live VNext routes where appropriate, with legacy retained as compatibility/fallback/projection. New work must not be tracked against Phase 5, Phase 6, Phase 6.3, or the pre-Phase-7 rendering primer.

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

It does not define semantic law or canonical architectural vocabulary.  
It defines order, discipline, gating, and closure.

This roadmap continues after the target architecture implementation is substantially complete.

It exists to sequence:

```text
canonical foundation
-> architectural legibility
-> target architecture convergence
-> formal coverage
-> construction algebra
-> bounded generativity
-> scenario hardening
-> governed long-term evolution
```

The active implementation plan may change more frequently than this roadmap. The roadmap should absorb only durable sequencing lessons, not transient implementation detail.

If a conflict exists:
1. `Project Bible.md` wins on architectural law.
2. `SYSTEM_MAP.md` wins on structural direction.
3. `DataVisualiser-Architectural-Vocabulary.md` wins on canonical architectural grammar, promoted concepts, ownership containers, bounded-generativity language, and do-not-confuse distinctions.
4. This roadmap wins on long-term sequencing, evolutionary gates, and closure discipline.
5. Operational implementation plans win only on current task order, local execution state, and immediate evidence requirements.
6. `DataVisualiser_Subsystem_Plan.md` is supporting subsystem context unless a future version is explicitly promoted as an active operational plan.

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

14. Consumer-agnostic contracts outrank terminal presentation convenience: rendering or UI work counts as structural progress only when it strengthens replaceable downstream consumption rather than promoting presentation into an architectural center.
15. Authority and provenance must remain explicit as capability grows; new interpretive power may not blur truth ownership.
16. Capability is not feature delivery: user-visible work should strengthen reusable reasoning capability where possible.
17. Composition belongs upstream of terminal delivery unless explicitly proven otherwise.
18. Interaction is a downstream contract concern, not merely event wiring or controller convenience.
19. Overlays are interpretive outputs, not rendering conveniences; they must remain reversible and non-authoritative.
20. Boundaries are enforcement seams; crossing them must preserve authority direction.
21. Broad family-pattern consolidation should wait until authority, intent, contract, and consumer seams are sufficiently stable.
22. Architectural vocabulary must remain stable during phase work: capability must not collapse into feature, consumer into presentation, interaction into event wiring, composition into builder plumbing, overlay into rendering, provenance into diagnostics, or authority into orchestration.
23. Projection and translation components may translate across explicit boundaries, but must not become semantic authorities or hidden policy centers.
24. Target architecture implementation is not the final product state; it is the structural precondition for safe long-term generativity.
25. Formal coverage must precede construction-algebra expansion: a requirement should be speakable in the architectural grammar before being absorbed into core construction.
26. Construction algebra must grow from inventory and evidence, not from abstract desire alone.
27. Bounded generativity is valid only when semantic authority, provenance, traceability, determinism, reversibility, evidence discipline, and boundary clarity are preserved.
28. Current pressure points should be reduced, contained, or formally governed before broad new abstraction layers are introduced.

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

7. Rendering and chart surfaces are important delivery instruments, but they are still terminal infrastructure. Future architectural cleanup should continue moving composition, shaping, and delivery contracts upward so that presentation becomes increasingly replaceable without semantic or orchestration redesign.
8. The enhanced architecture should be treated as a forward-only migration direction for active and future work: authority and intent first, then reasoning capability, then contract/boundary hardening, then projection/translation discipline, then consumer/interaction separation, then terminal delivery demotion, then governance/evidence isolation and broader consolidation.
9. The architectural vocabulary document provides the canonical grammar for interpreting those containers; this roadmap only sequences work against that grammar.

Post-target-architecture reconciliation:

1. Target architecture convergence is not the end of the roadmap.
2. Once the target spine is materially real, the roadmap shifts from structural migration toward formal coverage, construction algebra, bounded generativity, and scenario hardening.
3. Formal language and implementation must remain mutually intelligible:
   - requirements must map into the architectural grammar without semantic loss
   - implemented constructions must not expose ungoverned language gaps
   - abstractions must not grow faster than evidence
4. The system's long-term direction is not ordinary feature expansion; it is governed analytical generation over explicit truth, provenance, capability, contract, surface, delivery, and evidence seams.
5. Pressure reduction remains valid after convergence when old hubs, bridge seams, or state carriers threaten the target grammar.

Therefore, architecture rehaul was not a side quest, and hierarchy legibility (Phase 6) was not cosmetic cleanup.  
That bridge is now crossed: the hierarchy is trustworthy, true outliers are obvious, and future capability can land without creating new exception-driven structure. The reasoning engine is the composition center going forward.

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


### Audit-Safe Progression Rule

Closed phases are historical baselines, not active workstreams.

Do not track new progress against:
- Phase 5
- Phase 6
- Phase 6.3
- the pre-Phase-7 rendering primer

Forward work should be tracked against Phase 7, Phase 8, later consolidation phases, or the forward sequencing model below.

---

## 4.5 Historical Architectural Migration Progress Status

This section records the accepted high-level progress estimate for sequencing purposes only. The architectural vocabulary document remains the canonical grammar source, and the subsystem plan applies the estimate to active execution.

Historical estimate retained for context:

```text
Architectural migration at this snapshot: approximately 70–75%
Working estimate at this snapshot: ~72% (updated after Phases 19–22)

This estimate is not the current roadmap endpoint.
The roadmap continues beyond target architecture convergence into formal coverage, construction algebra, bounded generativity, and scenario hardening.
```

Sequencing interpretation:

| Area | Approx. completion | Roadmap implication |
|---|---:|---|
| Vocabulary / conceptual model | 90% | Concept language is stable enough to govern Phase 7 work. |
| VNext reasoning spine | 80% | End-to-end spine proved through MovingAverage; Phase 7 may build on proven seams. |
| Contract / boundary model | 75% | CapabilityContracts threaded; adapter dual-path retired; boundary enforcement materially strengthened. |
| Rendering demotion | 60% | Render-plan delivery is real, but rendering must continue moving toward terminal infrastructure. |
| Consumer / interaction separation | 65% | TabularSummaryChart proves non-chart consumer path; UI/presentation remains a main Phase 7/Phase 8 front. |
| Governance / evidence | 75% | Evidence is strong enough to support migration, but must remain observational. |
| Legacy coexistence cleanup | 50–60% | Legacy remains managed compatibility/fallback/projection; integration seams classified and bounded. |

Roadmap consequence:

- do not reopen Phase 5, Phase 6, Phase 6.3, or the pre-Phase-7 render-plan primer
- do not start another broad decomposition campaign by default
- prioritize capability slices that prove contract/boundary enforcement, provider/consumer separation, and reasoning-engine capability growth
- treat provider/consumer boundary verification as an early audit concern
- after target architecture convergence, use coverage, construction inventory, and pressure-point classification before formal runtime expansion
- do not treat increased architectural vocabulary in code as proof that structural risk has decreased


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
- Major migration work has been implemented and revalidated with current April 2026 evidence

**Why closure is not currently accepted as final**
- This concern has been satisfied by current April 2026 reachability/parity exports and current green default test lanes
- Active closure scope excludes the retired live `Difference/Ratio` chart surface
- `Syncfusion` no longer uses a stub reachability export; it emits through the shared evidence service with explicit `ExportScope = "Syncfusion"`, while backend qualification remains separately scoped

**Historical evidence references**
- January 2026 reachability artifacts have been retired from the repository. Current closure is based on April 2026 evidence and green default test lanes.

**Closure Condition**
1. All active strategies are CMS-capable.
2. All active strategies are reachable through live orchestration.
3. Path used is observable and test/assertion backed.
4. Current evidence artifacts exist in the repository-visible evidence path.
5. Closure is based on current April 2026 evidence artifacts in `documents/`, not historical January 2026 artifacts.

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
4. Closure scope explicitly excludes the retired live `Difference/Ratio` chart surface; Syncfusion evidence export is wired through the shared export path, but Syncfusion backend qualification remains explicit.

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
4. Closure scope excludes retired live consumer surfaces and keeps Syncfusion backend qualification explicit rather than treating exportability as rendering parity.

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
3. Keep Syncfusion evidence export and backend qualification claims separate: shared exportability is implemented, but rendering qualification remains capability-specific.
4. Do not generalize further just to reduce file count; only generalize patterns proven in multiple real slices.
5. Revalidate future closure claims with present evidence rather than relying on historical refactor intent alone.
6. Keep `DataFileReader` active callers on capability facades and do not reintroduce direct `SQLHelper` usage in those caller layers.

---

### Phase 6 - Architectural Legibility and Concern Reconciliation  
**Status:** CLOSED (April 2026)  
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

**Current Progress (April 2026) — CLOSED**
- First live VNext vertical slice: `MetricLoadCoordinator` routes the main chart family (`Main`, `Normalized`, `Diff/Ratio`) through `VNextMainChartIntegrationCoordinator` → `ReasoningSessionCoordinator` → `LegacyChartProgramProjector`
- VNext main-family route eligibility is centralized in `VNextChartRoutePolicy`, keeping route activation and the `SupportsOnlyMainChart` diagnostics flag explicit
- VNext widening complete: all remaining chart families (`Distribution`, `WeekdayTrend`, `Transform`, `BarPie`) now route fresh data loads through unified `VNextDataResolutionHelper` → `VNextSeriesLoadCoordinator`, with identity program builders, dictionary-backed per-family runtime tracking (`ChartState.FamilyLoadRuntimes`), and automatic legacy fallback on failure
- Fresh `ReasoningSessionCoordinator` per load attempt — no shared session state across loads
- `MainChartDisplayMode` propagated through VNext path — Regular/Summed/Stacked behavior preserved
- Evidence boundary decomposed: `MainChartsEvidenceExportService` split into `EvidenceExportModels` (21 standalone DTOs), `EvidenceDiagnosticsBuilder` (diagnostics assembly), and export orchestration
- Evidence export now carries `ExportScope`; the Charts and Syncfusion tabs both emit through `MainChartsEvidenceExportService` with tab-specific scope values
- Tab switches are recorded as `TabSwitched` session milestones through the shared view-model context
- `LoadRuntimeState` on `ChartState` tracks runtime path per family, request/snapshot/program/projected-context signatures, and failure reason
- `EvidenceRuntimePath` and `VNextDiagnosticsSnapshot` emitted in evidence exports with signature-chain alignment flags for all chart families
- Distribution interaction milestones recorded for frequency shading, interval count, mode, chart type, and subtype changes
- `MainChartsView` now delegates session diagnostics, UI-surface diagnostics, subtype-selection bookkeeping, load/clear bookkeeping, resolution reset, zoom reset, startup, evidence export, data-loaded flow, chart update, and chart presentation through dedicated host seams
- `MainChartsView` and `SyncfusionChartsView` now share the top metric-selection/date/CMS surface through `MetricSelectionPanel`, hosted by the chart-specialized `ChartTabHost` shell
- `ChartTabHost` now composes the generic `WorkspaceTabHost`, which exposes reusable header/body slots without metric-specific assumptions
- `AdminMetricsManagerView` now also uses `WorkspaceTabHost`, preserving Admin-specific controls while aligning with the shared workspace shell pattern
- `AdminMetricsManagerView` now delegates row loading, dirty tracking, filtering, save state, and milestone recording through `AdminMetricsManagerCoordinator` behind `IAdminMetricsRepository`
- Admin metric-type changes, hide-disabled toggles, reloads, first dirty-row edits, and save attempts now emit shared session milestones for cross-tab smoke exportability
- metric-selection event forwarding is centralized through `MetricSelectionPanelEventBinder` instead of duplicated host-local event wiring
- theme-toggle and reset-zoom actions now emit explicit session milestones for smoke-test exportability
- Syncfusion load, data-loaded, render completion/failure, export request/completion/failure, and zoom-reset actions now emit explicit session milestones for tab-specific smoke exportability
- metric-type list initialization, metric-type-change reset/reload, and subtype-loaded follow-up behavior are now shared between `MainChartsView` and `SyncfusionChartsView` through `ChartHostMetricSelectionCoordinator`
- `MainChartsView` host/controller-extras interaction, registry-wide controller resolution, and chart-surface startup/no-data presentation are now delegated through dedicated coordinators
- `TransformDataPanelControllerAdapter` now delegates subtype-selection interaction, operation state, execution, milestone recording, data resolution, and render/grid handoff through dedicated coordinators; V2-specific layout behavior is expressed through `ITransformLayoutCapabilities`
- `StrategyCutOverService` now delegates CMS eligibility decisions to `StrategyCmsDecisionEvaluator` and parity validation to `StrategyParityValidationService`, leaving strategy creation and reachability recording in the cut-over service
- `ChartTooltipFormattingHelper` is now a facade over focused pair, stacked, cumulative, value-formatting, title-parsing, and overlay-filter helpers
- `MainChartsView` and `SyncfusionChartsView` share `UiBusyScopeLease` for disposable UI-busy lifetime handling
- `BaseDistributionService` now delegates pure computation, simple-range assembly, series construction, axis shaping, and debug-summary formatting through dedicated helpers
- Smoke-verified with April 2026 exports: VNext path produces aligned signatures across all chart families, legacy fallback produces correct state, all 8 parity strategies pass
- 737 DataVisualiser tests and 15 DataFileReader tests pass in the current default full-solution lane
- Shared `WorkspaceTabHost` / `ChartTabHost` layout hardening has been smoke-verified: Charts, Syncfusion, and Admin render through the shared workspace shell, with tab/export milestones available in evidence exports

**Current evidence basis (April 2026):**
- current repository-visible reachability/parity exports generated through the approved evidence path
- legacy-path export proving active strategy parity
- VNext-path exports proving CMS-enabled and CMS-disabled signature-chain alignment
- current green default test lanes

Generated evidence artifacts are closure proof, not foundational doctrine. Specific export filenames may change as evidence generation improves or snapshots are regenerated.

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
- Phase 6 audit baseline was 476 C# source files, 171 test files, 696 DataVisualiser automated tests, 15 DataFileReader tests, and 56 architecture guardrails; the current pre-Phase-7 render-plan foundation lane is 493 C# source files, 174 test files, 737 DataVisualiser tests, and 15 DataFileReader tests
- All sub-phases (`6.1`–`6.7`) closed, including `6.3` VNext widening — all active chart families have VNext-compatible request/program support and live VNext routes where appropriate, with legacy retained as compatibility/fallback/projection
- All 5 global closure conditions assessed and met (full record in `DataVisualiser_Subsystem_Plan.md` Phase 6.6 section)
- Known debt carried to Phase 7: `MainChartsView.xaml.cs` (~1,440 lines, genuinely host-level), `SyncfusionChartsView.xaml.cs` (~859 lines, parallel host), managed legacy/VNext coexistence, controller adapter pattern variation accepted as domain variation

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
- Net -7 files; 609 tests passed at closure; later shared-panel/evidence-scope hardening, tab-shell extraction, route-policy extraction, transform layout capability isolation, CMS decision extraction, admin workflow extraction, strategy parity validation extraction, tooltip formatting split, shared UI-busy lease, parity-series comparison, workspace load/milestone recording, workspace load coordination, binary metric context consolidation, VNext render-plan foundation, Main chart render-plan adapter wiring, and the later BarPie, Distribution, WeekdayTrend, Syncfusion, and cleanup passes bring the current lane to 737 DataVisualiser tests

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
**Status:** OPEN / ACTIVE  
**Entry Gate:** Phase 6 must restore enough hierarchy legibility that new power can be added without amplifying entropy — **satisfied** (Phase 6 closed April 2026); Migration Plan Phases 14–22 complete; VNext spine proved end-to-end

**Purpose**
Phase 7 increases interpretive power without eroding truth or reintroducing exception-driven structure.

This phase remains the sanctioned home for exploratory capability, confidence-aware interpretation, and richer analytical views.
It is also the sanctioned home for standardized programmable chart composition once Phase 6 has earned the structural right to support it safely.

**Architectural orientation:** Exploratory capabilities should be built as reasoning-engine features that delivery surfaces consume, not as chart-specific features. The VNext reasoning engine is the composition surface; charts, reports, APIs, and future consumers are delivery targets. Each Phase 7 capability should strengthen the reasoning engine's generality rather than binding new power to a specific rendering backend.

Phase 7 should advance through forward stages aligned to the enhanced containers:
1. authority/intent clarification
2. reasoning/capability expansion
3. process/execution separation
4. contract/boundary hardening
5. projection/translation discipline
6. consumer/interaction separation
7. terminal delivery demotion
8. governance/evidence sidecar isolation

These are forward stages inside active/future work, not reopened Phase 6 or pre-Phase-7 tracks.

---

### Phase 7 Forward Sequencing Model

This section does not reopen any closed phase.

It defines the forward-looking sequencing model for Phase 7, Phase 8, and later consolidation work.
The architectural vocabulary document defines the concepts used here; this section defines only their sequencing.

The enhanced architecture migration should proceed in this order:

1. **Authority and Intent Clarification**  
   Make authority, provenance, and canonical intent explicit before adding more downstream power.

2. **Reasoning and Capability Expansion**  
   Express confidence, overlays, transforms, comparisons, and programmable composition as reasoning-engine capabilities.

3. **Process and Execution Separation**  
   Keep workflow, routing, fallback, and sequencing separate from semantic meaning and result composition.

4. **Contract and Boundary Hardening**  
   Make program, delivery, interaction, view, provider, consumer, and multi-result contracts the real downstream fan-out seam. Early Phase 7 work should verify that provider/consumer boundary types enforce real seams rather than merely renaming delivery routing.

5. **Projection and Translation Discipline**  
   Ensure builders, adapters, resolvers, selectors, and projectors translate across explicit boundaries without becoming semantic authorities or hidden policy centers.

6. **Consumer and Interaction Separation**  
   Treat charts, exports, APIs, and future clients as consumer families; keep interaction non-authoritative and contract-mediated.

7. **Terminal Delivery Demotion**  
   Keep rendering, backend adapters, route/host binding, and vendor lifecycle terminal, replaceable, and subordinate.

8. **Governance and Evidence Sidecar Isolation**  
   Keep diagnostics, parity, reachability, qualification, and evidence observational rather than governing live behavior.

9. **Broad Family Pattern Consolidation**  
   Collapse repeated request/route/qualification/adapter patterns only after the upstream spine and contract seam are stable.

**Closed pre-Phase-7 preservation baseline (April 2026):**
- VNext now contains backend-neutral render-plan contracts: `ChartRenderPlan`, `ChartSeriesPlan`, `ChartHierarchyNodePlan`, `RenderDataBuffer`, `RenderDensityPlan`, and `ChartInteractionPlan`, with live delivery now wired across the active chart families and tabs
- `RenderDensityPolicy` and `TimeBucketRenderAggregationKernel` establish the planned large-range rendering direction: preserve full source identity while producing bounded render buffers for overview or viewport-refined delivery.
- `ChartBackendCapabilities`, `ChartBackendSelector`, `IChartRenderPlanAdapter<TSurface>`, and `ChartRenderPlanAdapterDispatcher<TSurface>` define the adapter seam for LiveCharts, Syncfusion, and plugin renderers.
- Main, Distribution, WeekdayTrend, BarPie, and Syncfusion now consume `ChartRenderPlan` through adapters while preserving existing behavior. This work was the pre-Phase-7 primer and remains closed; Phase 7 capability expansion may now proceed against the stabilized delivery baseline.


The following Phase 7 capability areas are active/future work only; they do not reopen completed Phase 6 or primer tracks.

#### Forward Stage 7A - Authority, Intent, and Provenance Clarification

**Scope**
- define or converge toward a canonical intent model
- make provenance, truth-aware request/result shape, and authority boundaries explicit enough that downstream consumers do not re-decide meaning

**Closure Condition**
- new capability slices can state their intent, provenance, truth status, and downstream contract shape before rendering or UI work begins

#### Phase 7.1 - Interpretive Visual Overlays (Reasoning-Declared / Delivery-Rendered)

**Scope**
- dynamic line colouring
- through-lines such as average, median, or reference markers
- enhanced hover and tooltip signalling
- legend-driven visibility toggles

**Constraints**
- overlay definition belongs in reasoning/capability or contract space
- overlay delivery belongs in terminal rendering
- no effect on computation, CMS, canonical identity, or truth authority
- backend used for overlay delivery must already be qualified for the relevant chart capability slice

**Closure Condition**
- overlays are declared, visually correct, reversible, provenance-visible, non-authoritative, and backend-safe for the capability slice in which they are used

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

#### Phase 7.4 - Programmable Composition and Multi-Result Derived Views

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
- standardized programmable consumer hosts can express the current chart examples they are intended to subsume without semantic erosion
- multi-result derived delivery works on qualified consumer/rendering families with visible provenance
- transform-style programmability is no longer isolated to a single special-case controller
- composition is expressed through reasoning/program/contract structures rather than terminal rendering or UI-only logic

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

#### Phase 7.6 - Rules-Based Option and Interaction Gating

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


#### Phase 7.7 - Contract and Consumer Boundary Hardening

**Scope**
- formalize program, delivery, interaction, view, provider, consumer, and multi-result contracts
- classify charts, exports, APIs, and future clients as consumer families
- verify that provider/consumer contracts enforce boundary behavior rather than merely routing delivery
- prevent consumer-specific delivery shapes from leaking back into reasoning or process layers

**Constraints**
- contracts constrain and carry; they do not create semantic meaning
- consumers adapt and interact; they do not define truth, composition, or execution policy
- terminal delivery remains replaceable

**Closure Condition**
- at least two consumer families can consume reasoning-engine output through explicit contracts without changing semantic or execution logic
- provider/consumer boundary contracts are proven as enforceable seams, not just delivery-routing names

**Global Closure Condition for Phase 7**
1. All exploratory features are explicitly layered above truth.
2. Confidence handling is visible and non-destructive.
3. Interpretive power increases without semantic erosion.
4. Programmable chart composition remains explicit, reversible, and provenance-preserving.
5. New exploratory features do not breach rendering-boundary or backend qualification rules.
6. New capabilities strengthen the enhanced containers rather than creating parallel feature-specific architecture.
7. Contract and consumer boundaries become clearer with each capability slice.

---

### Phase 8 - Consumer, Interaction, UI, State, and Integration Consolidation  
**Status:** PLANNED / BLOCKED
**Entry Gate:** Phase 7 must advance far enough to justify broad consumer/UI/state consolidation without reopening authority, intent, contract, or interaction uncertainty

**Objective**
- ensure consumers and UI accurately reflect truth, interpretation, uncertainty, qualified rendering behavior, and standardized programmable capability without compensating for architectural weakness
- establish the first non-chart consumer of reasoning-engine output, proving that the engine's analytical programs are genuinely consumer-agnostic and not implicitly chart-shaped
- separate interaction semantics from controller/event plumbing wherever repeated patterns prove the shape

**Closure Condition**
1. Qualified chart families share standardized parent controller/host behavior wherever capability semantics genuinely align.
2. UI state reflects truth, uncertainty, and programmable chart composition without distortion.
3. Integration behavior is predictable across supported chart families and qualified backends.
4. The UI no longer acts as a repair layer for missing architectural boundaries.
5. At least one non-chart consumer successfully consumes reasoning-engine output through the same downstream contracts as chart consumers.

6. Where practical, Phase 8 should continue demoting chart/UI surfaces from major subsystem status toward thin, replaceable consumer layers over reasoning-engine and consumer-agnostic delivery contracts.
7. Interaction semantics are explicit, contract-mediated, and not hidden inside event wiring or controller convenience.

---

### Phase 9 - Post-Target Architecture Recalibration  
**Status:** PLANNED / CONTINUING GATE

**Entry Gate**
- Target architecture convergence is materially implemented.
- Remaining bridges, hubs, state carriers, and density pressure are named.
- The architectural vocabulary remains the governing grammar.

**Objective**
- Rebaseline the system after target architecture convergence.
- Classify whether remaining density is necessary target grammar, transitional bridge density, UI/state gravity, delivery/vendor gravity, evidence support density, scaffolding debt, or accidental coupling.
- Prevent formal post-architecture work from layering new abstractions on top of unresolved pressure.

**Closure Condition**
1. Current structural density is classified.
2. Current construction set is inventoried.
3. Remaining pressure points are classified as reducible, containable, bounded, deferred, or requiring formal language.
4. The next phase is justified by evidence, not by architectural ambition alone.

---

### Phase 10 - Formal Coverage and Construction-Algebra Foundation  
**Status:** PLANNED / BLOCKED BY PHASE 9

**Objective**
- Test whether requirements, implemented constructions, and planned future capabilities can be expressed through the architectural grammar without semantic loss.
- Establish only the minimum construction algebra justified by current evidence.

**Scope**
- requirements-to-language coverage
- construction inventory
- collapsed concern triage
- ambiguity triage
- minimal construction algebra
- operation / capability rules where proven
- typed relations where hidden relation ambiguity creates risk

**Closure Condition**
1. Coverage gaps are classified as implement now, express through existing construct, defer, guardrail only, or reject as premature.
2. Construction algebra is minimal, justified, and does not become a new pseudo-core.
3. New formal constructs have clear owner, boundary, input/output shape, evidence path, and non-goals.

---

### Phase 11 - Bounded Generativity and Derived Analytical Construction  
**Status:** PLANNED / BLOCKED BY PHASE 10

**Objective**
- Enable governed generation of derived analytical constructions without weakening truth, provenance, determinism, reversibility, or boundary clarity.

**Scope**
- operation / capability algebra
- multiplicity / derived dataset model
- evidence sufficiency / promotion rules
- semantic interpretation and confidence
- analytical fitness
- bounded search / computational planning

**Closure Condition**
1. Derived datasets have stable identity, provenance, traceability, and lossiness/reversibility metadata.
2. Evidence is sufficient to promote, retain, quarantine, or reject generated constructions.
3. Validity, usefulness, meaning, explanation, computability, and evidence remain distinct.
4. Generative capability remains bounded and reviewable.

---

### Phase 12 - Multi-Consumer Productization and Scenario Hardening  
**Status:** PLANNED / BLOCKED BY PHASE 11

**Objective**
- Prove that bounded generativity can serve real scenarios and multiple consumers without collapsing back into chart-specific architecture or a replacement mega-object.

**Candidate Scenarios**
- Operation Chain Workbench
- analytical workbench
- dashboard runtime
- personal analytics system
- evidence platform
- legacy migration framework
- AI-assisted architecture governance platform

**Closure Condition**
1. At least one bounded scenario proves formal coverage, construction rules, evidence sufficiency, and consumer output.
2. At least two consumer types can receive generated analytical output through explicit contracts.
3. Scenario hardening improves coherence rather than adding unmanaged feature sprawl.

---

### Phase 13 - Governed Evolution and Emergence Review  
**Status:** LONG-TERM / RECURRING

**Objective**
- Keep the project capable of growth without losing architectural meaning.

**Scope**
- language growth review
- construction growth review
- scenario promotion / quarantine
- drift detection
- evidence review
- AI-assisted extension governance
- new project seeding from proven architectural patterns

**Closure Condition**
- This phase does not close permanently.
- It repeats when a new scenario, new language family, new construction model, or new consumer class threatens to exceed the current grammar or evidence model.

---

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
2. intended container (`authority/provenance`, `reasoning/capability`, `process/execution`, `contract/boundary`, `projection/translation`, `consumer/interaction`, `terminal delivery`, or `governance/evidence`)
3. reversibility expectation
4. whether a rendering backend qualification step is required

This policy protects both long-term trust and long-term extensibility.

---


## 8. Current Critical Path (Authoritative)

1. Preserve the qualified seams produced by the completed rehaul, legibility, and target-spine convergence work.
2. Treat the reasoning engine as the composition surface; new capabilities should strengthen its generality rather than bind power to specific delivery surfaces.
3. Keep residual debt explicit and bounded rather than allowing silent structural drift.
4. Revalidate future closure claims with present evidence and repository-visible artifacts.
5. Introduce further generalization only when the codebase has earned it through repeated real slices.
6. After target architecture convergence, rebaseline the system before formal runtime expansion.
7. Classify current density, construction inventory, collapsed concerns, and ambiguity pressure before construction algebra is implemented.
8. Reduce or contain current pressure points where local cleanup is still productive.
9. Move into formal coverage and construction algebra only when pressure evidence proves local cleanup is insufficient or the missing formal model is necessary.
10. Advance bounded generativity through explicit truth, provenance, capability, contract, surface, delivery, and evidence seams.
11. Harden real scenarios only after the formal model has enough evidence to constrain them.

Current long-term direction:

```text
target architecture convergence
-> post-target recalibration
-> formal coverage / construction algebra
-> bounded generativity
-> multi-consumer scenario hardening
-> governed evolution
```

---

## 8.5 Open Phase Summary

Historical phase status:
1. `Phase 6 - Architectural Legibility and Concern Reconciliation`: `CLOSED`
2. `Phase 7 - Exploratory and Confidence Capability Expansion`: historically `OPEN / ACTIVE`; later target-architecture convergence work has superseded this as the sole active framing
3. `Phase 8 - Consumer, Interaction, UI, State, and Integration Consolidation`: `PLANNED / BLOCKED` until capability and contract seams are mature enough

Long-term roadmap phases:
1. `Phase 9 - Post-Target Architecture Recalibration`: planned / continuing gate
2. `Phase 10 - Formal Coverage and Construction-Algebra Foundation`: planned / blocked by Phase 9
3. `Phase 11 - Bounded Generativity and Derived Analytical Construction`: planned / blocked by Phase 10
4. `Phase 12 - Multi-Consumer Productization and Scenario Hardening`: planned / blocked by Phase 11
5. `Phase 13 - Governed Evolution and Emergence Review`: long-term / recurring

Phase 6 sub-phase status:
- `Phase 6.1` - irreducible operation consolidation: **CLOSED**
- `Phase 6.2` - truth / derivation / delivery boundary reconciliation: **CLOSED**
- `Phase 6.3` - request, consumer, and delivery standardization: **CLOSED** (all active chart families have VNext-compatible request/program support and live VNext routes, with legacy compatibility/fallback retained)
- `Phase 6.4` - outlier decomposition: **CLOSED**
- `Phase 6.5` - physical hierarchy and naming realignment: **CLOSED**
- `Phase 6.6` - architecture audit and baseline refresh: **CLOSED**
- `Phase 6.7` - pre-Phase 7 structural consolidation: **CLOSED**

**Phase 6 is fully closed.** All 5 global closure conditions met. All sub-phases closed including 6.3 — all active chart families now have VNext-compatible request/program support and live VNext routes where appropriate, with legacy retained as compatibility/fallback/projection.

Major next steps in sequence:
1. `Phase 7` - exploratory and confidence capability expansion on the closed render-plan preservation baseline
2. Preserve the live render-plan delivery baseline with targeted smoke only if a future change touches the delivery path or introduces a regression
3. `Phase 8` - consumer, interaction, UI, state, and integration consolidation after Phase 7 is sufficiently advanced
4. Later broad family-pattern consolidation only after authority, intent, contract, projection/translation, and consumer seams are stable

---

## 9. Summary

Current roadmap interpretation:

```text
The roadmap is no longer only a migration-completion document.
It is the long-term sequencing document for the project after target architecture convergence.
```

- Early phases built truthful foundations.
- Middle phases built CMS-capable behavior, but some closure claims now require revalidation.
- Phase 5 is the completed bridge between what the system already does and what it is intended to become.
- Phase 6 established a trustworthy hierarchy; it is fully closed. All active chart families have VNext-compatible request/program support and managed legacy fallback/compatibility.
- Phase 7 is now active; the VNext spine is proved end-to-end through MovingAverage, and Phases 14–22 are complete.
- Phase 7 should advance according to the forward sequencing model, without reopening any closed historical phase.
- The accepted architectural migration estimate is approximately 70–75% complete, with a working estimate of ~72% (updated after Phases 19–22).
- The architectural vocabulary document supplies the concept grammar; this roadmap supplies the sequencing and closure discipline.
- The reasoning engine is the center of the system. New capabilities should be built as reasoning-engine features that delivery surfaces consume.
- Multi-backend rendering support is not incidental. It is part of the deliberate architectural learning process.
- Target architecture convergence is a major milestone, not the end-state.
- After convergence, the roadmap continues through recalibration, formal coverage, construction algebra, bounded generativity, scenario hardening, and governed evolution.
- Closure means present truth, present evidence, and present structural safety.

This roadmap defines how the project grows without lying to itself, and without losing the flexibility it is explicitly trying to earn.

---

**End of Project Roadmap**
