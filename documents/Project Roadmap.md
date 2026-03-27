# PROJECT ROADMAP
**Status:** Sequencing and Execution Authority  
**Scope:** Phase ordering, execution dependencies, closure criteria, and evolutionary gating  
**Authority:** Subordinate to `Project Bible.md` and `SYSTEM_MAP.md`  
**Operational Execution Source:** `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md` for active architecture rehaul work  
**Last Updated:** 2026-03-26  
**Change Note:** Roadmap updated to reflect completed architecture rehaul closure, explicit residual debt posture, and post-rehaul maintenance discipline

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
4. `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md` operationalizes the active rehaul work permitted by this roadmap.

---

## 2. Binding Roadmap Doctrine

This roadmap is governed by the following rules:

1. Truth comes before insight.
2. Phases exist to protect trust and correctness, not to accelerate delivery.
3. A closure claim without current executable evidence is not closure.
4. Unreachable or unobservable execution is treated as non-existent.
5. Exploratory power must be explicitly staged above canonical truth, never folded into it.
6. Architecture work is phase work when it is required to preserve truth, reversibility, and safe future extension.
7. Capability preservation is the default: refactor may not silently remove current abilities.
8. Tactical stabilization is permitted, but stabilization is not the same as architectural closure.
9. Multiple chart backends may coexist intentionally, but only to strengthen modular rendering boundaries.
10. No backend is treated as production-safe for a capability family until it has passed explicit qualification for that capability family.
11. Future enhancement ideas are welcome, but they do not enter active work unless intentionally assigned to a phase and layer.

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
   - uneven assurance quality
4. Multi-backend chart support is not a cosmetic preference. It is a deliberate architectural probe into whether rendering has actually been isolated from the rest of the application.
5. Standardized programmable chart hosts are intended future product behavior, but that future is unsafe unless controller convergence, rendering contracts, and orchestration seams are made explicit first.

Therefore, architecture rehaul is not a side quest.  
It is now the mandatory bridge between the truthful foundation already built and the exploratory, confidence-aware, multi-backend future the project intends to support.

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

**Historical evidence references (now treated as historical, not sufficient by themselves)**
- `documents/reachability-20260121-160503.json`

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

**Historical evidence references (now treated as historical, not sufficient by themselves)**
- `documents/reachability-20260121-074430.json`
- `documents/reachability-20260121-074900.json`
- `documents/reachability-20260121-075301.json`
- `documents/reachability-20260121-085510.json`

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

**Historical evidence references (now treated as historical, not sufficient by themselves)**
- `documents/reachability-20260121-151124.json`
- `documents/reachability-20260121-114215.json`

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
- `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md` is the operational execution source for this phase

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

### Phase 6 - Exploratory and Confidence Capability Expansion (Intentional)  
**Status:** OPEN / NEXT CRITICAL PATH  
**Entry Gate:** Opened now that Phases 3, 3.5, 4, and 5 are closed under current evidence rules

**Purpose**
Phase 6 increases interpretive power without eroding canonical truth.

This phase remains the sanctioned home for exploratory capability, confidence-aware interpretation, and richer analytical views.
It exists to make the system a stronger instrument for human reasoning without turning it into an authority.
It is also the sanctioned home for standardized programmable chart composition once Phase 5 has earned the structural right to support it safely.

#### Phase 6.1 - Interpretive Visual Overlays (Render-Only)

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

#### Phase 6.2 - Confidence Annotation (Non-Destructive)

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

#### Phase 6.3 - Confidence-Aware Interpretation

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

#### Phase 6.4 - Programmable Chart Composition and Multi-Result Derived Views

**Scope**
- standardize graph parent controllers and shared option/toggle surfaces where capability semantics align
- allow transform-style programming across qualified chart families, including selected submetrics and unary, binary, ternary, or higher-order derived operations
- support rendering more than one derived result set on the same qualified chart surface where the chart family supports it
- preserve provenance and explicit derived identity for each result set

**Constraints**
- programmable chart composition is downstream of CMS and does not redefine semantics
- result sets remain derived or interpretive unless explicitly promoted by a separate declarative mechanism
- backend qualification must already cover the interactions and rendering behavior used by the programmable chart surface

**Closure Condition**
- standardized programmable chart hosts can express the current chart examples they are intended to subsume without semantic erosion
- multi-result derived rendering works on qualified chart families with visible provenance
- transform-style programmability is no longer isolated to a single special-case controller

#### Phase 6.5 - Structural and Relational Exploration

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

#### Phase 6.6 - Rules-Based Option Gating

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

**Global Closure Condition for Phase 6**
1. All exploratory features are explicitly layered above truth.
2. Confidence handling is visible and non-destructive.
3. Interpretive power increases without semantic erosion.
4. Programmable chart composition remains explicit, reversible, and provenance-preserving.
5. New exploratory features do not breach rendering-boundary or backend qualification rules.

---

### Phase 7 - UI, State, and Integration Consolidation  
**Status:** PLANNED / BLOCKED
**Entry Gate:** Phase 6 must advance far enough to justify broad UI/state consolidation without reopening structural uncertainty

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
2. Begin Phase 6 deliberately through bounded exploratory and confidence-aware slices.
3. Keep residual debt explicit and bounded rather than allowing silent structural drift.
4. Revalidate future closure claims with present evidence and repository-visible artifacts.
5. Introduce further generalization only when the codebase has earned it through repeated real slices.
6. Keep future phase entry explicit rather than reopening rehaul work implicitly.

Phase 6 must not be treated as open merely because ideas are ready.  
It opens only when the system is structurally able to absorb them safely.

---

## 8.5 Open Phase Summary

Remaining open phases:
1. `Phase 6 - Exploratory and Confidence Capability Expansion`: `OPEN / NEXT CRITICAL PATH`
2. `Phase 7 - UI, State, and Integration Consolidation`: `PLANNED / BLOCKED`

Major next steps in sequence:
1. `Phase 6.1` - interpretive visual overlays
2. `Phase 6.2` - confidence annotation
3. `Phase 6.3` - confidence-aware interpretation
4. `Phase 6.4` - programmable chart composition and multi-result derived views
5. `Phase 6.5` - structural and relational exploration
6. `Phase 6.6` - rules-based option gating
7. `Phase 7` - UI, state, and integration consolidation after Phase 6 is sufficiently advanced

---

## 9. Summary

- Early phases built truthful foundations.
- Middle phases built CMS-capable behavior, but some closure claims now require revalidation.
- Phase 5 is the completed bridge between what the system already does and what it is intended to become.
- Phase 6 is now the active sanctioned home of exploration, confidence, and richer interpretive power.
- Multi-backend rendering support is not incidental. It is part of the deliberate architectural learning process.
- Closure means present truth, present evidence, and present structural safety.

This roadmap defines how the project grows without lying to itself, and without losing the flexibility it is explicitly trying to earn.

---

**End of Project Roadmap**
