# Architecture Rehaul Consolidated Execution Plan
Status: Consolidated execution source of truth for structural rehaul  
Scope: Structural, philosophical, evolutionary, and intention-aligned codebase overhaul  
Authority: Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, and `Project Roadmap.md`  
Last Updated: 2026-03-26

---

## 1. Purpose and Authority

This document is the single execution plan for architecture rehaul work.

It does **not** replace foundational anchors. It operationalizes them.
It is the active execution source for **Phase 5 - Architecture Rehaul and Backend Qualification** in `Project Roadmap.md`.

Precedence order:
1. `Project Bible.md` (architectural law)
2. `SYSTEM_MAP.md` (structural boundaries and flow rules)
3. `Project Roadmap.md` (phase sequencing and closure discipline)
4. This document (implementation orchestration for the current rehaul)

Current roadmap position that materially governs this document:
1. Phases 1 and 2 are treated as genuinely closed foundations.
2. Phases 3, 3.5, and 4 are treated as implemented but requiring present-evidence revalidation.
3. Phase 5 is the current critical path.
4. Phase 6 exploratory/confidence expansion is intentionally blocked until this phase is actually closed.

---

## 2. Consolidated Inputs (Absorbed)

The following documents are absorbed here for actionable execution content:
1. `DataVisualiser_Architecture_Consolidation_Report.md`
2. `UNDERDEVELOPED_COMPONENTS_IMPLEMENTATION_ORDER.md`
3. `CHART_CONTROLLER_DECOUPLING_PLAN.md`
4. `CMS_MIGRATION_STATUS_AND_IMPLEMENTATION_PLAN.md` (historical, absorbed)
5. `CMS_TEST_RESULTS_AUDIT.md` (historical, absorbed)
6. `REFACTORING_IMPACT_ASSESSMENT.md` (historical, absorbed)
7. `TransformDataPanelControllerV2.md` (layout intention absorbed into adapter split work)
8. `HierarchyRefId_Implementation_Plan.md` (structural identity execution guidance absorbed)
9. `CANONICAL_MAPPING_LIMITATION.md` (mapping governance and historical risk context absorbed)
10. `CMS_TEST_SUMMARY_TABLE.md` (test-lane discipline and path-assertion requirements absorbed)
11. `PARITY_VALIDATION_EXPLAINED.md` (parity execution policy and closure evidence model absorbed)

These inputs are consolidated under the seven top-level findings below.

An additional documentation-agnostic architecture review was performed on 2026-03-15 and reconciled into the same seven findings. It does not create a competing plan; it sharpens the execution intent already captured here.

---

## 3. Non-Negotiable Constraints (Foundational Alignment)

All actions in this plan must preserve:
1. Canonical semantic authority in normalization/CMS layers.
2. Downward-only authority flow (`Truth -> Derivation -> Interpretation -> Presentation`).
3. Legacy as compatibility reference only, not forward architecture.
4. Explicit, deterministic, and reversible migration behavior.
5. No semantic decisions in UI/service/rendering layers.
6. No silent bypass paths or unreachable migration code.
7. No hardcoded semantic mapping as steady-state architecture.
8. No currently shipped capability may be removed as a side effect of refactor work unless that retirement is explicitly authorized by higher-authority project documents.
9. Refactor execution defaults to additive preservation first: introduce seam/adapter, prove behavior, migrate callers, then remove superseded structure.
10. Architectural cleanup must preserve or improve the project's capacity for declared future extension, especially Phase 6 interpretive, exploratory, and confidence-aware capabilities.
11. Charting libraries are replaceable infrastructure, not architectural authorities.
12. Backend-specific lifecycle, hover, animation, disposal, and visibility quirks must be isolated inside rendering adapters/services, never allowed to shape orchestration or semantic flow.
13. Support for multiple chart vendors is intentional experimentation to validate the rendering boundary; coexistence is not itself a defect.
14. No chart backend may be treated as production-safe for a capability family until it has passed explicit qualification for that capability family.
15. The system remains an instrument for human reasoning, not an authority; architectural changes must not quietly convert interpretive behavior into semantic or operational truth.
16. Interpretation, confidence, and exploratory structure remain explicit overlay layers; the rehaul must preserve their reversibility, provenance, and non-authoritative status.
17. UI, state, and integration layers may reflect uncertainty and backend qualification state, but must never compensate for missing semantic, orchestration, or rendering boundaries.
18. Eventual convergence toward standardized graph parent controllers and shared chart-host affordances is intentional; Phase 5 work must not harden bespoke controller divergence as steady-state architecture.
19. Transform-like chart programmability is a downstream derived/interpretive capability, not a semantic authority; selectable submetrics, operations, and result composition must remain explicit, auditable, and reversible.
20. Support for rendering more than one derived result set on a qualified chart surface is future-facing architectural intent and must not be blocked by single-result assumptions in current seams, contracts, or host models.

---

## 3A. Rendering Backend Objective

This rehaul explicitly treats chart backends as swappable, qualifiable infrastructure.

Desired architectural properties:
1. New third-party chart backends can be introduced without changing computation, orchestration, or view-model semantics.
2. Chart families are expressed as capability-oriented render intents and interaction contracts, not vendor control types.
3. Backend-specific lifecycle behavior is quarantined inside backend adapters, probes, and qualification harnesses.
4. Multiple chart backends may coexist intentionally while the project learns which abstractions are stable.
5. A backend is eligible for production use only for the capability slices it has actually passed in the backend qualification matrix.

---

## 3B. Material Foundational Implications For This Rehaul

After reconciliation with `Project Bible.md`, `Project Overview.md`, `Project Philosphy.md`, and the revitalized `Project Roadmap.md`, the following are now treated as materially important:

1. This rehaul is not generic cleanup. It is the structural bridge between truthful canonical foundations and future interpretive/confidence-aware expansion.
2. The primary architectural risk is no longer raw computation capability; it is unsafe evolution that could erode truth boundaries, rendering boundaries, or closure discipline.
3. Interpretation must remain a lens, not a mutation. Therefore rendering, overlays, and confidence features must stay explicitly downstream of canonical truth and derivation.
4. Multi-backend chart support is not an implementation curiosity. It is a deliberate architectural probe into whether rendering has truly been isolated from semantics and orchestration.
5. Tactical stabilization is acceptable when necessary to preserve current capability, but tactical stabilization must be explicitly bounded and must not be mistaken for architectural closure.
6. Stale closure claims are now treated as structural debt. This plan must therefore prioritize evidence recovery and revalidation, not just new code movement.
7. Future exploratory work is blocked not because the ideas are weak, but because the architecture must first earn the right to extend safely.

---

## 3C. Programmable Chart Platform Direction

This rehaul must preserve the path toward a standardized programmable chart platform.

Desired architectural properties:
1. Graph parent controllers converge toward a shared host model wherever capability semantics actually align, rather than remaining permanently fragmented by prototype history.
2. Chart capability families can be driven by explicit chart programs that describe selected submetrics, declared operations, and render intent without embedding semantic decisions in controllers.
3. A chart program may yield one or more derived result sets, each with visible provenance, without requiring per-chart-family controller redesign.
4. Transform-style programmability is not confined to a single transform panel long-term; it is an eventual cross-chart capability layered above canonical truth and qualified rendering surfaces.
5. Result-set composition remains derived/interpretive by default unless a higher-authority declarative promotion path is introduced explicitly.
6. Backend qualification remains orthogonal: programmable composition may use only backend/capability slices that have already been qualified for the interactions they need.

---

## 4. Reality Baseline (As Of 2026-03-18)

Roadmap reality framing:
1. Phase 5 is the current critical path.
2. Architecture rehaul is mandatory enabling work, not optional refactoring.
3. Phase 6 and Phase 7 remain blocked on genuine structural closure, not merely on accumulated implementation effort.

Top-level findings (hierarchy for all actions):
1. Composition bottleneck remains in a single UI class.
2. Layer-boundary collapse exists between Core and UI/WPF concerns.
3. CMS migration scaffolding is still structural in key areas.
4. Reachability/parity evidence pipeline is misaligned with declared closure evidence.
5. Runtime behavior is controlled by mutable global static toggles.
6. Data access and orchestration responsibilities are overly concentrated.
7. Assurance depth is uneven versus declared confidence model.

All detailed intentions and work below are structurally placed under these findings.

Independent architecture-only diagnostics, reconciled into the same hierarchy:
1. The current "Core" layer behaves partly as a UI-support layer rather than a framework-agnostic application/domain layer.
2. Visualization technology is fragmented across `LiveCharts`, `LiveChartsCore`, and Syncfusion without one stable rendering contract.
3. Data-access seams are too weak, allowing presentation/test flows to pull concrete database behavior into higher layers.
4. Cross-cutting operational concerns (notifications, failure surfacing, crash handling, export behavior) are distributed through orchestration/UI paths rather than isolated infrastructure boundaries.
5. `DataFileReader` is structurally procedural and static-heavy, not yet a cleanly bounded ingestion pipeline.
6. Test breadth exists, but boundary porosity still leaks infrastructure dependency into tests.

These diagnostics are not new top-level findings. They are execution-relevant clarifications absorbed into Findings 1, 2, 5, 6, and 7 below.

---

## 4A. Capability Preservation Baseline (Non-Regression Scope)

This baseline defines the minimum capability surface the rehaul must preserve, except where a capability has already been explicitly deprecated or retired by higher-authority documents.

Capability families that must not be lost accidentally:
1. Lossless ingestion and persistence of source data.
2. Declarative normalization, canonical identity, and CMS production.
3. Deterministic computation across currently supported strategy families and migration lanes.
4. Visualization coverage across current chart/view families, including time-series, distribution, transform, comparative/compositional, and current Syncfusion-backed views where present.
5. UI interaction capabilities already present, including chart visibility/state management, transform interaction, and current render-time exploratory controls.
6. Reachability, parity, and evidence-generation capability needed to validate migration and architectural safety.
7. Existing compatibility/migration observability, where retained intentionally by roadmap and migration law.
8. Future-extension readiness for declared interpretive overlays, confidence annotation, confidence-aware views, and structural/relational exploration.
9. Rendering-backend extensibility across current and future chart families without reworking semantic/application logic.
10. Visible, non-destructive uncertainty and interpretive-overlay support without canonical erosion.
11. Eventual standardized chart-host/controller convergence across the graph families already present in the prototype.
12. Eventual transform-style programmability and multi-result-set rendering on qualified chart surfaces with preserved provenance.

Capability retirement rule:
1. A capability may be removed only if one of the following is true:
   - it is explicitly retired/deprecated by higher-authority documents, or
   - it is replaced by an equivalent or better capability with preserved intent and validated behavior
2. Incidental deletion caused by refactor simplification is invalid.
3. If a capability is being replaced rather than retired, the document trail must explicitly map:
   - old capability
   - replacement path
   - proof of preserved behavior

---

## 4B. Rendering Backend Lessons (Current Working Assumption)

Current working lessons from the ongoing prototype evolution:
1. Backend-specific render-loop or lifecycle failures must be treated as evidence about rendering-boundary weakness, not merely as chart-specific bugs.
2. Tactical fallbacks that preserve user-visible capability are acceptable for stabilization, but they do not count as backend qualification or architectural closure.
3. Future charting work must separate:
   - chart semantics/capabilities the product needs
   - backend(s) currently qualified to deliver those capabilities safely
4. Rendering backend choice must remain reversible until qualification evidence is strong enough to justify a narrower long-term direction.
5. A single successful backend slice does not prove backend-wide viability.
6. A single backend failure does not justify collapsing the multi-backend objective; it just means qualification must become explicit.

---

## 5. Hierarchical Rehaul Intentions and Ordered Actions

## 5.1 Finding 1: Composition bottleneck remains in a single UI class

### Target State
`MainChartsView` is a thin host/composition boundary only.  
Chart-specific behavior lives in adapters/controllers/services and is swappable through registry/factory contracts.  
View composition does not double as chart workflow ownership.
Graph parent-controller divergence is reduced in a way that preserves later convergence toward standardized chart hosts and shared option/toggle affordances.

### Ordered Actions
1. Complete decoupling plan "next steps":
   - remove remaining direct controller references in `MainChartsView`
   - remove fallback direct control retrieval paths
   - rely exclusively on registry-based controller resolution
2. Move chart event wiring from view-level manual attachment to factory-driven registration where feasible.
3. Split Transform panel responsibilities:
   - UI layout shell responsibility (including collapsible rail intention)
   - transform computation/rendering responsibility
4. Reduce `MainChartsView` responsibilities into explicit startup modules:
   - infrastructure bootstrap
   - pipeline bootstrap
   - ui binding bootstrap
   - export/evidence command handlers
5. Ensure chart feature composition hangs off stable chart-host abstractions rather than ad-hoc view-owned control knowledge, and do not hard-code the assumption that each chart family needs a permanently unique parent controller shell.
6. Add controller swap integration tests (registry-level, not view-internal reflection paths).

### Closure Criteria
1. `MainChartsView` no longer contains direct chart-controller usage outside composition seams.
2. Transform layout shell and transform compute/render logic are separated.
3. Registry/factory is the only chart controller access path.
4. View-level composition is not responsible for chart-specific workflow logic.
5. Remaining controller divergence is capability-driven or qualification-driven, not prototype-history drift.

---

## 5.2 Finding 2: Layer-boundary collapse exists between Core and UI/WPF concerns

### Target State
Core remains framework-agnostic for orchestration/computation semantics.  
UI/WPF concerns reside in UI layer adapters and rendering adapters.  
Chart-library choice and backend lifecycle behavior are implementation details behind stable rendering capability contracts and qualification harnesses.

### Ordered Actions
1. Define and enforce boundary rule set:
   - Core must not reference `DataVisualiser.UI.*`
   - Core must not depend directly on WPF message dialogs or UI state objects for semantic flow
2. Split mixed helpers:
   - separate `ChartHelper` into core-safe logic vs UI-specific tooltip/control logic
   - move UI element construction out of Core rendering helpers
3. Introduce explicit abstractions for rendering targets, chart surfaces, interactions, and user notifications where Core currently references UI elements.
4. Define rendering contracts by capability family rather than by vendor:
   - time-series/cartesian
   - distribution/range
   - polar/radar-style projection
   - compositional/hierarchical
   - transform/result
   - hover/selection/legend/visibility/reset interactions
   - chart-program/result-set composition handoff sufficient to avoid locking future programmable chart hosts into single-result assumptions
5. Converge chart backend usage behind those capability contracts so `LiveCharts`, `LiveChartsCore`, Syncfusion, and future vendors remain replaceable adapters rather than architectural peers.
6. Add backend qualification harnesses for each backend/capability slice:
   - load
   - update
   - hide/show
   - tab switch/offscreen behavior
   - unload/disposal
   - application close
   - hover/tooltip interaction
7. Refactor orchestrators/coordinators to depend on capability abstractions, not concrete WPF controls or chart-library types.
8. Treat current tactical fallbacks as temporary until the backend qualification matrix confirms which vendor path is safe for which capability family.
9. Add architecture tests to fail builds on prohibited dependency directions and prohibited vendor-type leakage above rendering adapters.

### Closure Criteria
1. Dependency graph matches `SYSTEM_MAP` directionality constraints.
2. Core compilation unit no longer depends on UI namespace types for orchestration semantics.
3. Application/orchestration flow does not depend directly on chart-library concrete types.
4. A backend qualification matrix exists for supported chart capability families.
5. A new backend can be introduced for an existing chart family by implementing the rendering contract without changing semantic/orchestration flow.
6. Boundary tests exist and run in CI/test workflow.
7. Rendering contracts do not assume a single derived result set where future qualified chart families are intended to support multiple concurrent result sets.

---

## 5.3 Finding 3: CMS migration scaffolding is still structural in key areas

### Target State
CMS is forward execution path by implementation, not only by configuration intent.

### Ordered Actions
1. Complete CMS factory wiring where TODO scaffolds remain:
   - `MultiMetric`
   - `Normalized`
   - `Difference`
   - `Ratio`
2. Remove steady-state CMS->legacy conversion dependence in forward computation paths.
3. Evolve strategy creation contracts so CMS-native inputs are first-class and explicit.
4. Keep legacy path only as declared compatibility fallback while parity obligations remain open.
5. Ensure parity harness coverage is explicit per strategy and phase-exit evidence is reproducible.
6. Resolve CMS data flow gaps in distribution and related adapter contexts where availability is inconsistent.
7. Operationalize canonical mapping governance:
   - treat subtype-aware runtime mapping data as the sole bridge authority
   - detect missing/duplicate/conflicting mapping rows before strategy execution
   - fail explicitly on unmapped subtype selections (no silent fallback semantics)
8. Add explicit multi-subtype CMS parity scenarios for combined/multi/distribution strategy families.
9. Implement structural RefVal identity for hierarchy trees:
   - replace legacy parent-path-sum RefVal logic with deterministic bottom-up structural fingerprinting
   - include node kind, semantic name, primitive type category, and child signatures
   - enforce container order-insensitive canonicalization and array order-sensitive canonicalization
10. Persist RefVal as an auditable join key format (stable hash with optional payload traceability).

### Closure Criteria
1. No CMS factory TODO remains for active strategies.
2. Forward path strategies execute CMS-native logic without defaulting to legacy conversion bridges.
3. Legacy path usage is observable, bounded, and justified by explicit migration state.
4. Canonical mapping is subtype-aware, centrally governed, and test-validated for multi-subtype selections.
5. Structural RefVal identity is active, and legacy path-sum identity is removed or explicitly deprecated.

---

## 5.4 Finding 4: Reachability/parity evidence pipeline is misaligned with declared closure evidence

### Target State
Declared roadmap evidence and actual artifacts are synchronized, reproducible, and repository-visible.

### Ordered Actions
1. Standardize evidence export destination to project-root `documents/` (not runtime `bin` directory).
2. Define evidence artifact contract:
   - timestamp
   - commit hash
   - strategy coverage
   - CMS/legacy path usage counts
   - parity summary
3. Add deterministic export command path (scriptable/headless), not UI-only export.
4. Rebuild missing reachability/parity artifacts referenced by roadmap/overview docs.
5. Add documentation sync checks:
   - if a referenced evidence file does not exist, closure claim is invalid
   - no workspace closure while docs and execution reality diverge
6. Define parity execution proof policy:
   - automated unit/integration parity tests are primary phase-exit proof
   - runtime parity checks are supplementary diagnostics only
   - parity artifacts must include layer-level failure classification (structural/temporal/value)
7. Attach CMS execution-lane metadata to each evidence artifact:
   - global CMS toggle state
   - per-strategy toggle state
   - actual path used (CMS vs legacy) per strategy run
8. Maintain a compact CMS test summary matrix per run to prevent false closure from toggle misconfiguration.

### Closure Criteria
1. All referenced evidence files exist and are current.
2. Evidence generation is repeatable without UI interaction.
3. Roadmap and overview status statements match current artifacts.
4. Parity closure evidence is produced by automated tests plus reproducible artifacts, not runtime-only observation.
5. CMS evidence includes explicit toggle states and path-used assertions for each tested strategy.

---

## 5.5 Finding 5: Runtime behavior is controlled by mutable global static toggles

### Target State
Runtime behavior is deterministic, scoped, testable, and explicitly versioned.

### Ordered Actions
1. Replace mutable static cut-over toggles with injected runtime configuration service/options model.
2. Introduce explicit configuration profile snapshot in exported evidence artifacts.
3. Support user/session overrides through controlled state boundary, not global statics.
4. Ensure tests do not share mutable global state across cases.
5. Centralize default and override precedence policy (config governance workstream).

### Closure Criteria
1. No production path relies on mutable static strategy toggles.
2. Runtime configuration used in execution is explicit and externally inspectable.
3. Tests are isolated from cross-test global toggle bleed.

---

## 5.6 Finding 6: Data access and orchestration responsibilities are overly concentrated

### Target State
Data access, strategy selection, and rendering orchestration are separated with focused contracts.
`DataFileReader` is a bounded ingestion subsystem rather than a static helper cluster.

### Ordered Actions
1. Decompose monolithic repository/service responsibilities:
   - split read concerns by domain capability (selection, admin, counts, ranges, series data)
2. Decompose `DataFileReader` into explicit ingestion pipeline stages:
   - input discovery
   - normalization
   - canonical mapping
   - persistence
   - maintenance/repair operations
3. Use injected repositories/services instead of repeated ad-hoc instantiation.
4. Remove presentation-layer dependence on concrete repository construction and direct count/fetch probing.
5. Replace silent catch-and-continue with typed failure propagation and explicit logging.
6. Move user-notification and other operational side effects behind infrastructure boundaries rather than orchestration/services.
7. Complete unresolved orchestration TODO paths (including weekly trend placeholder path).
8. Align orchestration flow to a single context builder and explicit handoff boundaries.

### Closure Criteria
1. Large multi-concern data access paths are split into explicit contracts.
2. Silent failures are removed from critical load/render paths.
3. Orchestration flow is fully reachable and observable across strategy types.
4. `DataFileReader` no longer relies on large static utility hubs as the primary ingestion architecture.
5. Presentation-path execution and tests do not require live database behavior unless explicitly classified as integration work.

---

## 5.7 Finding 7: Assurance depth is uneven versus declared confidence model

### Target State
Assurance reflects stated trust model: deterministic parity, boundary guarantees, and phase-closure evidence.
Breadth of tests is secondary to seam quality and environment independence.

### Ordered Actions
1. Expand `DataFileReader` test coverage for normalization, canonical mapping, and identity behavior.
2. Add architecture conformance tests (dependency direction + boundary rules).
3. Add integration tests for:
   - strategy cut-over reachability
   - evidence export contract
   - parity closure assertions
4. Reduce reliance on reflection against private nested UI types by extracting testable contracts/models.
5. Convert presentation-layer tests that currently require concrete database behavior into isolated contract tests, and keep live-database coverage in explicit integration lanes only.
6. Create and maintain explicit traceability matrix:
   - finding -> action -> test/evidence -> closure sign-off
7. Introduce mandatory CMS validation matrix:
   - run migrated strategies with global CMS ON
   - verify expected behavior under per-strategy ON/OFF toggles
   - assert actual execution path in test output/artifacts
8. Add parity harness layer assertions for each migrated strategy:
   - structural parity
   - temporal parity
   - value parity with explicit tolerance policy
9. Add stale-claim guardrails:
   - no historical pass/fail claim remains without current-date executable evidence
   - failing claims must be revalidated or removed during document updates

### Closure Criteria
1. Coverage expands in ingestion/normalization/canonical layers and critical orchestration seams.
2. Architectural boundaries are test-enforced.
3. Presentation-layer tests run without accidental external SQL dependency unless explicitly marked integration.
4. Phase closure claims are backed by executable tests and present artifacts.
5. CMS validation matrix fails fast if global CMS is disabled in CMS test lanes.
6. Parity harness layer checks are present for all migrated strategies.
7. Stale status claims are eliminated or replaced with current evidence.

---

## 6. Unified Execution Order (Cross-Workstream)

Execution order for minimal risk and maximum structural leverage:
1. Evidence pipeline correction and documentation/evidence sync guardrails.
2. CMS/Legacy cut-over truthfulness and missing strategy-path completion.
3. Composition bottleneck reduction in `MainChartsView`.
4. Core/UI boundary enforcement, helper splits, rendering contract convergence, and backend qualification.
5. Configuration centralization (remove global mutable toggles and shared runtime state assumptions).
6. Data access/orchestration decomposition, `DataFileReader` pipeline decomposition, and error-path hardening.
7. Assurance expansion, architecture conformance testing, and environment-independent seam validation.
8. Only after the above is materially closed may Phase 6 exploratory/confidence work be treated as open roadmap work.

This ordering is aligned with existing risk-aware implementation order documents and decoupling plans.

---

## 7. Evidence, Governance, and Change Control

For each action batch:
1. Record impacted files.
2. Record reachability/parity evidence files produced.
3. Record tests added/updated.
4. Record which foundational constraints were checked.
5. Record which capability families were touched (`preserved`, `extended`, or `explicitly retired`).
6. Record pre/post validation evidence for touched capabilities (automated test, smoke test, artifact, or explicit gap).
7. Record backend qualification evidence added, changed, or invalidated for affected chart capability slices.
8. Record intended layer classification for the work (`truth`, `derived`, `interpretive`, `UI`, `rendering infrastructure`, or `governance/evidence`).
9. Record whether the change is tactical stabilization, structural closure work, or future capability enablement.
10. Update this document only after code + evidence are in sync.

No status "complete" may be declared for any workstream without evidence artifacts and test confirmation.
No deletion or retirement may be declared complete without an explicit capability mapping or higher-authority retirement basis.

---

## 8. Supersession Scope (Execution Docs Only)

This document supersedes prior scattered execution guidance for architecture rehaul orchestration only.

Foundational anchors remain untouched and authoritative.

If any absorbed source conflicts with foundational documents, foundational documents win and conflicting absorbed guidance is invalid.

Retired as superfluous after absorption:
1. `DataVisualiser_Architecture_Consolidation_Report.md`
2. `UNDERDEVELOPED_COMPONENTS_IMPLEMENTATION_ORDER.md`
3. `CHART_CONTROLLER_DECOUPLING_PLAN.md`
4. `CMS_MIGRATION_STATUS_AND_IMPLEMENTATION_PLAN.md`
5. `CMS_TEST_RESULTS_AUDIT.md`
6. `REFACTORING_IMPACT_ASSESSMENT.md`
7. `TransformDataPanelControllerV2.md`
8. `HierarchyRefId_Implementation_Plan.md`
9. `CANONICAL_MAPPING_LIMITATION.md`
10. `CMS_TEST_SUMMARY_TABLE.md`
11. `PARITY_VALIDATION_EXPLAINED.md`

---

## 9. Immediate Next Action Set

1. Continue `Step 17` and finish the final convergence pass.
2. Keep the remaining work behavior-preserving: remove superseded intermediate glue first, then only promote abstractions already proven in multiple real slices.
3. Close the rehaul only after the final build, default test lanes, and broad manual smoke pass are clean.
4. Revalidate roadmap-adjacent closure claims with present evidence before treating any pre-Phase-5 work as truly closed.
5. Ensure new rendering/orchestration seams do not hard-code single-result or permanently bespoke-controller assumptions that would block the standardized programmable chart direction.

### Remaining Step Summary
1. `Step 17`: run the final convergence pass, promote only proven shared abstractions, remove superseded structure, and close residual technical debt.

---

## 10. Incremental Implementation To-Do Sequence

Purpose: provide a sequential, low-risk implementation order that an AI agent or human can execute in safe chunks with a clear validation gate after each step.

Execution rules:
1. Complete one step fully before starting the next.
2. Do not mix architectural refactor with unrelated feature work in the same step.
3. Each step must end with a build pass and the listed focused test/smoke check.
4. If hidden coupling is discovered that materially expands scope, stop, record it, and revise this sequence before continuing.
5. Before each step, list the current capabilities touched by that step.
6. For each touched capability, assign exactly one status: `preserve`, `extend`, or `explicitly retire`.
7. `Explicitly retire` is only valid if supported by higher-authority project documents and made visible to the user before implementation.
8. Code deletion is permitted only after the replacement path has passed the step's validation gate.
9. Before each step, declare the intended layer classification for the work.
10. If a step is only tactical stabilization, record that explicitly and do not count it as architectural closure for the affected workstream.
11. Prefer contraction, simplification, and generalization of existing behavior-preserving structure before introducing new architectural surfaces when that reduces qualification risk.
12. Keep each step reversible as a single bounded change set wherever practical so rollback and fault isolation remain straightforward.

### Step 1. Stabilize the default test lane
Status: completed

Agent work:
1. Remove accidental live-database dependency from default `DataVisualiser.Tests` execution.
2. Reclassify any truly external-resource tests into an explicit integration lane, or replace their dependencies with fakes where practical.
3. Make the default local test lane safe to run repeatedly during refactor work.

Primary code focus:
1. `DataVisualiser.Tests`
2. adapter/controller tests that currently transitively trigger concrete SQL access

Validation gate:
1. `dotnet test DataVisualiser.Tests/DataVisualiser.Tests.csproj -c Debug --no-build`

Done when:
1. The default `DataVisualiser.Tests` lane does not require live SQL Server access.

### Step 2. Introduce cross-cutting seams before deeper refactors
Status: completed

Agent work:
1. Add an injected runtime configuration abstraction to wrap current mutable static strategy/config toggles.
2. Add an injected user-notification abstraction to replace direct `MessageBox` use in core/orchestration paths.
3. Preserve existing behavior by backing the new abstractions with compatibility adapters first.

Primary code focus:
1. `CmsConfiguration`
2. orchestration/services that currently surface UI dialogs directly

Validation gate:
1. solution build
2. targeted unit tests for configuration and notification-backed flows

Done when:
1. newly touched orchestration/core code no longer depends directly on global static toggles or direct UI dialog calls

### Step 3. Put `DataFetcher` behind narrow query contracts
Status: completed

Agent work:
1. Define small interfaces for counts, subtype lookup, and series retrieval.
2. Keep `DataFetcher` as the initial concrete implementation behind those interfaces.
3. Refactor `MetricSelectionService` and the first dependent adapter path to consume the interfaces through injection.

Primary code focus:
1. `MetricSelectionService`
2. `DataFetcher`
3. transform and selection-related adapter flows

Validation gate:
1. build
2. unit tests for the refactored services/adapters using fakes instead of SQL

Done when:
1. presentation-path logic no longer constructs `DataFetcher` directly in the first migrated slice

### Step 4. Split `ChartHelper` into pure logic and UI logic
Status: completed

Agent work:
1. Extract chart-independent formatting/math/selection logic into core-safe helpers.
2. Move WPF control construction, tooltip handling, and axis mutation helpers into UI/rendering-specific helpers.
3. Update callers incrementally without broad renaming churn.

Primary code focus:
1. `ChartHelper`
2. direct dependents in rendering/orchestration

Validation gate:
1. build
2. affected helper/orchestration tests
3. quick manual hover/tooltip smoke test

Done when:
1. the extracted core-safe helper logic is usable without WPF or chart-control references

### Step 5. Sanitize chart/controller structure before new rendering abstractions
Status: completed

Agent work:
1. Inventory existing chart families, controllers, adapters, helpers, and host patterns to identify repeated responsibilities and accidental divergence.
2. Standardize chart host and surface access patterns for the current chart families where capability semantics already align.
3. Remove ad-hoc control discovery and visual-tree traversal where explicit ownership or host access can replace it safely.
4. Collapse duplicated adapter/controller plumbing into shared helpers or base patterns where doing so preserves behavior and reduces architectural noise.
5. Normalize naming and placement of chart-layer types so controller, adapter, host, rendering, and helper responsibilities are structurally obvious.
6. Isolate vendor-specific behavior more clearly so shared chart code stops carrying backend-specific quirks unnecessarily.
7. Delete superseded compatibility code only after the replacement path has passed validation.

Primary code focus:
1. chart controllers/adapters/hosts
2. shared chart helpers and chart-surface access patterns
3. backend-specific code currently leaking into shared chart paths

Validation gate:
1. build
2. targeted adapter/controller tests for the touched chart families
3. manual smoke for load/render, toggle visibility, reset zoom, clear, and transform compute on touched flows

Done when:
1. current chart families use more consistent host/access patterns where appropriate
2. duplicated plumbing is reduced without introducing new user-facing behavior
3. vendor-specific quirks are more isolated from shared chart structure
4. the codebase is materially cleaner to receive rendering capability contracts

### Step 6. Establish rendering capability contracts and backend qualification on a narrow vertical slice
Status: completed
Completion note:
1. Proven and widened beyond the first slice for `Distribution`, `Weekday Trend`, `Bar/Pie`, `Main`, `Normalized`, `Diff/Ratio`, and `Transform`.

Agent work:
1. Introduce rendering contracts by capability for one chart family, including render, reset, visibility, and interaction lifecycle.
2. Apply the first slice to the distribution chart path only.
3. Shape the first contract so it does not hard-code a permanent single-result assumption that would later block programmable multi-result chart composition.
4. Keep adapters for `LiveCharts`, `LiveChartsCore`, and Syncfusion behind that contract rather than attempting a full rendering rewrite.
5. Build a backend qualification harness for that slice covering:
   - initial render
   - repeated updates
   - tab switch/offscreen behavior
   - hide/show
   - unload/disposal
   - application close
   - hover/tooltip behavior
6. Record qualification outcome in a backend capability matrix instead of assuming equal support across vendors.

Primary code focus:
1. distribution chart controller/adapter path
2. rendering interfaces and first concrete adapters
3. backend qualification harness/tests
4. backend capability matrix documentation/artifacts

Validation gate:
1. build
2. distribution-chart tests
3. manual render/reset/switch smoke test for the distribution chart path
4. backend qualification checklist completed for the chosen slice

Done when:
1. one end-to-end chart flow renders through the new contract without leaking chart-library concrete types upward
2. backend qualification evidence exists for that chart family/capability slice
3. any tactical fallback still in place is explicitly classified as temporary and bounded
4. the first rendering contract does not accidentally foreclose future chart-program/result-set composition above the rendering boundary

### Step 7. Repair CMS/Legacy cut-over truthfulness and complete missing dual-path strategies
Status: completed

Agent work:
1. Make cut-over reporting truthful for strategy families that do not yet have a real CMS implementation.
2. Remove false-positive `UseCms` outcomes or false-positive reachability claims for unsupported families until real CMS execution exists.
3. Complete real CMS strategy paths for:
   - `MultiMetric`
   - `Normalized`
   - `Difference`
   - `Ratio`
4. Update orchestration/context contracts so the repaired CMS paths receive first-class CMS inputs instead of relying on legacy-derived substitutes.
5. Remove or replace chart-path short-circuits that bypass cut-over for repaired families, especially `Diff/Ratio`.
6. Add focused reachability, no-silent-fallback, and parity coverage for each repaired family.
7. Regenerate the CMS/Legacy capability truth table after the repaired paths are in place.

Primary code focus:
1. `StrategyCutOverService`
2. strategy factories for `MultiMetric`, `Normalized`, `Difference`, and `Ratio`
3. `ChartDataContextBuilder`
4. `ChartRenderingOrchestrator`
5. tests covering reachability and parity for the repaired families

Validation gate:
1. full solution build
2. focused strategy cut-over tests
3. focused parity tests for repaired families
4. manual smoke for `Main`, `Normalized`, and `Diff/Ratio`

Done when:
1. no strategy family reports CMS execution without a real CMS computation path
2. `MultiMetric`, `Normalized`, `Difference`, and `Ratio` each have a real CMS path or are explicitly reported as legacy-only
3. `Diff/Ratio` no longer bypasses the cut-over truth model in live chart flow
4. reachability/parity evidence for the repaired families is executable and current

### Step 8. Introduce reversible light/dark theme support through explicit UI/theme seams
Status: completed

Completion note:
1. Theme state/service, top-level toggle, and shared light/dark resource dictionaries are implemented.
2. Shared shell/control theming is implemented across `Main`, `Syncfusion`, and `Admin`.
3. Shared legend, combo-box, tooltip/popup, and major chart-host theming is implemented.
4. Chart-local label/text seams that blocked readability have been normalized, including weekday/distribution polar labels, transform grids, and Syncfusion sunburst labels/tooltips.
5. Shared grid header/body theming is implemented with distinct readable header/body treatment.
6. Runtime theme switching works through live resource references on the touched interactive/chart-local surfaces.

Agent work:
1. Add a small application theme state and a top-level theme toggle in the existing header/options area.
2. Move currently hard-coded shared UI colors out of controls/code-behind and into explicit theme resources.
3. Introduce a theme service or equivalent coordinator so theme changes are applied centrally rather than through scattered ad-hoc brush mutation.
4. Separate generic application chrome colors from chart-specific palette/tooltip/legend colors so later chart theming remains controllable without disturbing semantic/rendering seams.
5. Move chart tooltip, popup, legend, axis, and host-surface colors behind theme-aware brush providers where those values are still hard-coded in code paths.
6. Preserve current light-theme behavior as the default baseline while adding dark mode as an alternate qualified UI state.
7. Keep the change behavior-preserving outside appearance; do not mix theme work with unrelated chart/controller logic changes.
8. Remove superseded color definitions only after the new shared theme resources are live and validated.

Primary code focus:
1. top-level selection/header UI and theme toggle placement
2. shared resource dictionaries / theme resource organization
3. chart tooltip/legend/popup/host color providers
4. any controls still owning hard-coded color values

Validation gate:
1. full solution build
2. targeted tests for any new theme coordinator/resource selection logic
3. manual smoke in both `Light` and `Dark`:
   - application startup
   - `Main`
   - `Normalized`
   - `Distribution` cartesian and polar
   - `Weekday Trend` cartesian and polar
   - `Transform`
   - `Syncfusion`
   - `Admin`
   - dropdown open and closed states
   - tooltip readability
   - legend visibility/toggle where applicable
   - reset zoom
   - theme toggle back and forth without restart

Done when:
1. the app can switch between `Light` and `Dark` from the top-level UI
2. shared UI colors are no longer primarily embedded in individual controls/code-behind
3. chart-adjacent UI visuals use explicit theme-aware resources/providers rather than scattered literal brush values
4. theme switching does not require behavior changes or restart
5. runtime theme switching is driven by live resource references rather than one-time brush snapshots on interactive/chart-local surfaces

Recommended execution batches:
1. Batch 1: completed
   - theme state/service
   - top-level toggle
   - shared theme dictionaries
   - validation: build + startup/toggle smoke
2. Batch 2: completed
   - shared shell/header/panel/control colors moved into theme resources
   - `Main`, `Syncfusion`, and `Admin` top-level surfaces themed
   - validation: build + light/dark UI shell smoke
3. Batch 3: completed
   - shared legends, combo boxes, common tooltips/popups, chart-host surfaces, and grid theming
   - live theme switching fixed for those shared interactive surfaces
   - validation: build + tooltip/legend/dropdown/grid smoke in both themes
4. Residual theme work:
   - future visual tweaks stay local and must not reopen a broad theme refactor batch
   - preserve dynamic color assignment where functionally required, but improve placement opportunistically when safe

### Step 9. Refactor orchestration to explicit handoff boundaries
Status: completed

Agent work:
1. Separate context building, strategy selection, chart-program/result composition, data retrieval, render invocation, and render-result/reporting into explicit handoff stages.
2. Keep this step focused on orchestration flow only; do not mix in general `MainChartsView` code-behind extraction here.
3. Remove direct UI state/control assumptions from the first orchestration slice migrated.
4. Route failure handling through the new notification/logging seams.
5. Make runtime path-used reporting and render-result metadata attach at orchestration boundaries rather than ad-hoc view code paths.

Primary code focus:
1. `ChartRenderingOrchestrator`
2. `ChartUpdateCoordinator`
3. associated context builders/services

Validation gate:
1. build
2. orchestration tests
3. focused reachability/parity/export tests for the migrated slice where those seams move
4. manual chart update smoke test on the migrated slice

Done when:
1. the migrated orchestration path does not directly require WPF controls, `MessageBox`, or concrete repository construction
2. orchestration handoff stages are explicit enough that later `MainChartsView` extraction is mostly shell work rather than workflow surgery

### Step 10. Reduce `MainChartsView` to a composition host, pass 1
Status: completed

Agent work:
1. Extract startup/bootstrap responsibilities from `MainChartsView`.
2. Extract event registration and workflow setup into dedicated bootstrap/registration helpers.
3. Extract reachability/parity/export command wiring out of `MainChartsView` and leave the view owning only command triggering/binding.
4. Extract theme-toggle handling out of `MainChartsView` if it still owns non-trivial theme coordination logic.
5. Extract view-only coordination seams such as chart-title updates, clear-hidden-chart coordination, and other host responsibilities that do not belong in deeper orchestration.
6. Keep view behavior unchanged while shrinking code-behind responsibility, and do not re-entrench chart-family-specific parent host divergence that later standardization must undo.

Primary code focus:
1. `MainChartsView`
2. controller registration/bootstrap code
3. export/theme/bootstrap helper seams

Validation gate:
1. build
2. controller registry tests
3. targeted tests for extracted bootstrap/export helpers where practical
4. manual application startup and chart-load smoke test

Done when:
1. `MainChartsView` primarily wires composition and bindings rather than owning workflow logic
2. export/theme/bootstrap/event wiring is no longer concentrated in one code-behind class

### Step 11. Reduce `MainChartsView` to a composition host, pass 2
Status: completed

Agent work:
1. Remove remaining direct controller access and fallback control lookups.
2. Use registry/factory resolution as the sole controller access path.
3. Ensure transform-panel interactions also resolve through explicit controller/adapter contracts rather than remaining a permanently special-case programmability island.
4. Remove any remaining ad-hoc view-owned controller branching that survived pass 1.

Primary code focus:
1. `MainChartsView`
2. controller registry/factory
3. transform-panel integration points

Validation gate:
1. build
2. controller swap/registry tests
3. targeted integration tests for extracted host/coordinator seams where practical
4. manual chart toggle/load/transform smoke test

Done when:
1. `MainChartsView` has no remaining chart-specific fallback control knowledge outside composition seams
2. transform interactions no longer depend on direct special-case control retrieval from the view shell

### Step 12. Isolate Syncfusion-specific fragility
Status: completed

Agent work:
1. Move reflection-heavy tooltip/hit-testing workarounds behind a dedicated Syncfusion-specific behavior/service layer.
2. Keep the controller focused on host responsibilities only.
3. Do not generalize Syncfusion quirks into shared rendering abstractions.
4. Treat theme/readability work on Syncfusion as already complete; this step is only for behavioral/workaround isolation.
5. Reconcile Syncfusion tab/view lifecycle refresh behavior with the chart-host rules already proven in the main views without leaking those special cases into shared code.
6. Make Syncfusion reachability/export status explicit: either add a bounded export seam or document the exclusion clearly.

Primary code focus:
1. Syncfusion sunburst controller/adapter path
2. Syncfusion-specific tooltip and hit-testing support code
3. Syncfusion tab/view lifecycle refresh handling
4. any Syncfusion-only export/reachability stubs or placeholders

Validation gate:
1. build
2. Syncfusion adapter/controller tests
3. manual sunburst hover and selection smoke test

Done when:
1. Syncfusion version/workaround logic is isolated from shared chart orchestration logic
2. Syncfusion lifecycle quirks are handled in Syncfusion-owned seams rather than in shared host code
3. Syncfusion export/reachability status is explicit rather than placeholder-driven

### Step 13. Start `DataFileReader` modernization with the safest infrastructure slice
Status: completed

Agent work:
1. Introduce injected options/connection-string access instead of ad-hoc configuration reads in the first migrated slice.
2. Replace obsolete `System.Data.SqlClient` usage with `Microsoft.Data.SqlClient` in the canonical-mapping path first.
3. Preserve behavior before attempting broader `SQLHelper` decomposition.
4. Remove direct concrete repository/query construction from the first active presentation/orchestration slice that is already structurally ready to migrate.

Primary code focus:
1. canonical mapping store
2. connection/config access for the first migrated database path
3. first active `DataFetcher`/query-construction hotspots already isolated enough to migrate safely

Validation gate:
1. build
2. `dotnet test DataFileReader.Tests/DataFileReader.Tests.csproj -c Debug --no-build`

Done when:
1. the first `DataFileReader` slice is no longer using obsolete SQL client APIs or implicit config access
2. at least one active higher-layer path no longer constructs its concrete data query/repository dependency directly

### Step 14. Add hard architectural guardrails
Status: completed

Agent work:
1. Add dependency-direction tests for Core->UI and orchestration->chart-library leakage.
2. Add unit-vs-integration lane separation rules for tests.
3. Add a small architecture scan that fails fast on prohibited reference patterns.
4. Add guardrails preventing new chart-vendor usage above rendering adapters unless the backend/capability qualification path exists.
5. Add guardrails for the new theme/export seams so UI resource concerns and export/reporting concerns do not drift back into Core/orchestration accidentally.
6. Add guardrails against new direct controller lookup paths in `MainChartsView` and related host views.
7. Add guardrails around notification usage so orchestration/services do not silently fall back to UI-owned defaults without explicit composition choice.
7. Add guardrails for late-stage generalization:
   prove a pattern in at least `2-3` real slices before promoting it into a shared abstraction.
8. Add guardrails requiring generalization by logical component/layer rather than by file-count reduction:
   orchestration stages, host coordination, rendering contracts, theme resources, and export/evidence seams.
9. Add guardrails against forced sameness:
   thin adapters, pure renderers, and simple state holders must not be wrapped in orchestration abstractions unless they actually own multi-stage workflow logic.

Primary code focus:
1. test projects
2. architecture conformance test utilities

Validation gate:
1. full solution build
2. default unit-test lanes for `DataVisualiser.Tests` and `DataFileReader.Tests`

Done when:
1. the next refactor step cannot silently reintroduce the same boundary violations
2. new vendor adoption cannot bypass the rendering-boundary and qualification rules accidentally
3. future generalization can happen safely without speculative framework-first abstraction
4. notification, theme, and export seams are protected against regression-by-drift

### Step 15. Rebuild evidence/export flow after the code seams are in place

Status: completed

Agent work:
1. Preserve the export metadata already added, but move evidence generation behind a scriptable headless path.
2. Move artifact destination to project-root `documents/`.
3. Keep runtime configuration snapshots and path-used metadata attached to exported artifacts.
4. Ensure evidence generation no longer depends on UI-only command paths.
5. Provide a deterministic way to clear/reset the runtime evidence store between runs so exported artifacts are easier to interpret.
6. Extract parity snapshot construction and reachability payload assembly out of `MainChartsView` into dedicated evidence services/coordinators.
7. Reconcile any remaining chart-family-specific export gaps explicitly instead of leaving placeholder UI messages.

Primary code focus:
1. evidence export path
2. artifact contract
3. headless/scriptable execution path
4. parity snapshot builders currently embedded in UI host code

Validation gate:
1. build
2. targeted evidence export test/integration check
3. confirm artifacts land in project-root `documents/`

Done when:
1. closure evidence can be regenerated consistently without manual UI interaction
2. evidence artifacts land in the repository-visible location with current configuration/path-used metadata intact
3. `MainChartsView` is no longer the primary owner of parity/evidence assembly

### Step 16. Split `SQLHelper` by capability without changing callers all at once

Status: completed

Agent work:
1. Extract separate services/modules for schema maintenance, metric reads, persistence, and count/canonical operations.
2. Leave a temporary facade in place if needed to avoid a large caller migration in one pass.
3. Migrate callers by capability slice, not by mass rename.
4. Reconcile remaining direct `DataFetcher` or query-service creation in UI/core paths onto the new capability-based seams as they become available.

Primary code focus:
1. `SQLHelper`
2. `DataFileReader` callers grouped by capability
3. remaining direct `DataFetcher` construction hotspots in active application paths

Validation gate:
1. build
2. `DataFileReader.Tests`
3. targeted ingestion smoke run if available

Done when:
1. `SQLHelper` is no longer the dominant multi-concern entry point for ingestion/persistence/database maintenance
2. the known direct-construction hotspots are either migrated or explicitly deferred with rationale

### Step 17. Run a final convergence pass before declaring architectural closure

Status: in progress

Agent work:
1. remove compatibility shims that were only meant to support intermediate migration
2. tighten naming and folder placement after the new seams settle
3. update this document with completed steps, qualified backend/capability support, residual risks, and remaining intentional technical debt
4. promote only the now-proven repeated patterns into shared abstractions:
   orchestration stage seams
   host/composition coordination seams
   rendering-contract support seams
   theme/resource seams
   evidence/export seams
5. standardize graph parent-controller and host patterns where the behavior has already converged, without erasing justified chart-specific differences.
6. replace residual duplicate local implementations with shared layer-owned components only after the active slices confirm equivalent behavior.
7. keep dynamic color assignment where functionally required, but relocate it to cleaner theme/render seams if that can now be done without expanding scope.
8. explicitly leave any remaining non-generalized outliers documented as intentional debt rather than implicitly unfinished work.
9. reduce or explicitly justify the remaining large concentration points that still hold mixed responsibility after Steps 12-16.

Primary code focus:
1. temporary shims/adapters
2. final naming/placement cleanup
3. late-stage shared abstractions by layer/component
4. documentation and evidence links
5. remaining oversized types that still concentrate mixed responsibility
   Current explicitly tracked residual concentrations:
   `MainChartsView.xaml.cs`
   `ChartHelper.cs`
   `ChartRenderEngine.cs`
   `TransformDataPanelControllerAdapter.cs`

Validation gate:
1. full solution build
2. default test lanes
3. manual smoke pass across main chart, transform, and Syncfusion flows
4. confirm newly shared abstractions are exercised by more than one real consumer path

Done when:
1. the transitional scaffolding is explicitly bounded and the remaining architecture debt is intentional rather than accidental
2. cross-cutting generalization has happened only where the codebase has already earned it
3. residual large concentration points have been either reduced or explicitly justified

---

End of document.
