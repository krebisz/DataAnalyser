# DataVisualiser Migration Plan and Guardrails

## Purpose

This document is the implementation-facing companion to the rebuilt architectural vocabulary and target architecture document.

It is intended for:

```text
AI agent execution in an IDE
human review
sequential progress tracking
auditable completion
```

Use this document as a to-do plan.

Refactoring is permitted and encouraged where it reduces architectural sprawl, removes contradiction, strengthens generalized seams, or improves extensibility without violating behavior, tests, or authority boundaries.

Do not treat this as permission for broad, cosmetic, or speculative refactoring.

---

## Source Context

This plan assumes the target architecture has already been defined around:

```text
Authority / Semantics / Provenance / Traceability / Envelope
Fidelity / Determinism / Reversibility / Constraint / Governance
Intent / Capability / Composition / Transformation / Interpretation / Confidence / Overlay
Program / Policy / Contract / Boundary / Neutrality / Qualification
Provider / Consumer / Interaction / SurfaceModel / Binding
Projection / Adapter / Resolver / Selector / Registry
Delivery / Backend / RuntimeBoundary / VendorBoundary / Lifecycle
Evidence / Diagnostics / Parity / Reachability / Validation / Audit / Record
```

Current state:

```text
target spine exists
old mesh remains active
status is yellow
migration should proceed by stabilization, seam hardening, then expansion
```

---

# 1. Migration Plan

## 1.1 Execution Rules

```text
Work top-to-bottom.
Complete one phase before starting the next.
Prefer narrow, test-backed changes.
Preserve behavior.
Preserve tests.
Refactor when it reduces sprawl, duplication, contradiction, or exception-driven structure.
Prefer generalized seams over family-specific exceptions when the shared shape is proven.
Do not add new analytical capability until containment, seams, and guardrails are stable.
Do not rename or move code for cosmetic reasons.
Do not stop for minor user confirmations when a logical, behavior-preserving work bundle can be completed before manual testing or milestone review.
```

---

## 1.2 Phase 1 — Establish Current Baseline

Goal:

```text
Capture the current project state before further architectural change.
```

Tasks:

- [x] Regenerate latest `project-tree.txt`.
- [x] Regenerate latest `codebase-index.md`.
- [x] Regenerate latest `dependency-summary.md`.
- [x] Regenerate latest type dependency diagram.
- [x] Record current test count.
- [x] Record current symbol count.
- [x] Record current dependency-density reading.
- [x] Record current known high-risk hubs.
- [x] Record current known legacy/VNext coexistence paths.
- [x] Confirm vocabulary and target-architecture documents are available to the IDE agent.

Completion condition:

```text
Current structural baseline exists and can be compared after each migration slice.
```

Phase 1 baseline evidence:

```text
Recorded: 2026-04-28
Generated artifacts:
- project-tree.txt
- codebase-index.md
- dependency-summary.md
- type-dependency-diagram.md
- documents/Type Dependencies Diagram.md

Test discovery:
- DataVisualiser.Tests: 910 tests
- DataFileReader.Tests: 15 tests
- Total listed tests: 925 tests

Symbol count:
- DataFileReader: 83 symbols
- DataFileReader.Tests: 6 symbols
- DataVisualiser: 714 symbols
- DataVisualiser.Tests: 185 symbols
- Total indexed symbols: 988 symbols
- Top-level symbols: 978
- Nested symbols: 10

Type dependency diagram:
- Supplied diagram edges parsed: 4434
- Declared type symbols: 953
- Direct type-reference edges: 6039
- Dependency-density reading: 0.6656%

Known high-risk hubs from supplied dependency evidence:
- MainChartsView
- SyncfusionChartsView
- ChartUpdateCoordinator
- ChartRenderingOrchestrator
- WeekdayTrendChartControllerAdapter
- BaseDistributionService
- DistributionChartControllerAdapter
- MetricLoadCoordinator
- MainWindowViewModel
- ChartPanelController
- StrategyCutOverService
- MainChartControllerAdapter
- SyncfusionSunburstChartControllerAdapter
- AnalyticalRenderPlanPipeline
- BarPieRenderModelBuilder
- TransformDataResolutionCoordinator

Known high-density data/type carriers from generated dependency evidence:
- ChartDataContext
- MetricData
- ChartState
- MetricSeriesSelection
- MetricSelectionService
- MetricState
- ChartProgramKind
- IChartComputationStrategy
- ChartComputationResult
- EvidenceDiagnosticsBuilder
- ChartRenderPlan

Known legacy/VNext coexistence paths from code inspection:
- DataVisualiser.UI.MainHost.VNextMainChartIntegrationCoordinator
- DataVisualiser.UI.MainHost.VNextSeriesLoadCoordinator
- DataVisualiser.UI.Charts.Presentation.VNextDataResolutionHelper
- DataVisualiser.VNext.Application.LegacyChartProgramProjector
- DataVisualiser.VNext.Application.LegacyMetricViewGateway
- chart-family controller adapters under DataVisualiser.UI.Charts.Presentation
- parity/evidence paths comparing legacy and VNext/CMS execution under DataVisualiser.UI.MainHost.Evidence

Vocabulary / target architecture availability:
- documents/DataVisualiser-Architectural-Vocabulary.md available and read
- documents/DataVisualiser_Migration_Plan_and_Guardrails.md available and read
```

---

## 1.3 Phase 2 — Fix Generated Governance References

Goal:

```text
Ensure generated dependency/governance output references the architectural vocabulary document.
```

Tasks:

- [x] Find the generator/source script that creates `dependency-summary.md`.
- [x] Add `DataVisualiser-Architectural-Vocabulary.md` to boundary-evaluation references.
- [x] Regenerate `dependency-summary.md`.
- [x] Confirm generated output references:
  - [x] `Project Bible.md`
  - [x] `SYSTEM_MAP.md`
  - [x] `DataVisualiser-Architectural-Vocabulary.md`

Completion condition:

```text
Generated governance output includes the vocabulary document without manual patching.
```

Phase 2 evidence:

```text
Generator updated:
- scripts/Generate-DependencySummary.ps1

Generated output confirmed:
- dependency-summary.md references Project Bible.md
- dependency-summary.md references SYSTEM_MAP.md
- dependency-summary.md references DataVisualiser-Architectural-Vocabulary.md
```

---

## 1.4 Phase 3 — Classify Dependency Density

Goal:

```text
Separate legitimate architecture density from accidental coupling.
```

Classify dense dependencies as:

```text
legitimate steady-state seam
legitimate transitional bridge
diagram/export noise
accidental coupling
actual drift
```

Tasks:

- [x] Inspect latest type dependency diagram.
- [x] Identify dense Authority / Reasoning / Contract / Delivery / Evidence seams.
- [x] Identify transitional bridge clusters.
- [x] Identify diagram/export noise.
- [x] Identify accidental coupling clusters.
- [x] Identify any actual drift.
- [x] Document findings in a short audit note.
- [x] Flag refactoring candidates only after classification.

Completion condition:

```text
Density is classified before refactoring or capability expansion proceeds.
```

Phase 3 evidence:

```text
Audit note:
- documents/DataVisualiser_Dependency_Density_Audit.md

Inputs inspected:
- documents/Type Dependencies Diagram.md
- type-dependency-diagram.md
- codebase-index.md
- dependency-summary.md
- DataVisualiser-Architectural-Vocabulary.md
- SYSTEM_MAP.md
- selected hub files under DataVisualiser/Core, DataVisualiser/UI, and DataVisualiser/VNext

Classification result:
- legitimate steady-state seams: domain/state carriers, strategy contracts, render-plan contracts, metadata keys, and vocabulary/program identifiers
- legitimate transitional bridges: MetricLoadCoordinator, VNext main/series load coordinators, VNext data resolution, legacy projectors/gateways, chart-family adapter bridges, parity/evidence comparison paths
- diagram/export noise: generic nested names, test-only density, repeated family request/route/capability records, and repeated Mermaid node/edge output
- accidental coupling candidates: MainChartsView, SyncfusionChartsView, ChartUpdateCoordinator, ChartRenderingOrchestrator, ChartControllerFactory, ChartControllerFactoryContext, BaseDistributionService, chart-family controller adapters, TransformDataPanelControllerAdapter, EvidenceDiagnosticsBuilder
- actual drift: none confirmed by density evidence alone
```

---

## 1.5 Phase 4 — Identify Refactoring Opportunities

Goal:

```text
Find safe refactoring opportunities that reduce sprawl and strengthen the generalized target architecture.
```

Refactoring is desirable when it:

```text
removes duplicated family-specific structure
collapses exception-driven paths into generalized seams
reduces contradictory ownership
reduces integration-hub responsibility
strengthens contract / boundary / qualification flow
strengthens Authority / Envelope / Provenance preservation
strengthens SurfaceModel / Delivery separation
keeps delivery terminal and replaceable
keeps evidence observational
```

Refactoring is not desirable when it:

```text
renames without architectural gain
moves code cosmetically
hides real family-specific differences
collapses unproven patterns too early
centralizes UI, rendering, vendor, process, or evidence authority
breaks parity or behavior
```

Tasks:

- [ ] Identify repeated structures across chart/render/delivery families.
- [ ] Identify exception-driven paths that can become generalized seams.
- [ ] Identify contradictory ownership between Core, VNext, UI, rendering, delivery, and evidence.
- [ ] Identify hubs that can lose responsibility safely.
- [ ] Identify duplicated request/route/qualification/adapter patterns.
- [ ] Identify places where target grammar can replace implementation-shaped concepts:
  - [ ] `Controller` -> `ConsumerAdapter`
  - [ ] `ViewModel` -> `ConsumerState`
  - [ ] `Renderer` -> `DeliveryAdapter`
  - [ ] `Route` -> `Binding`
  - [ ] `Host` -> `RuntimeBoundary` / `DeliverySurface`
  - [ ] `Diagnostics` -> `Evidence`, where proof/audit is intended
- [ ] Classify each opportunity as:
  - [ ] safe now
  - [ ] needs tests first
  - [ ] defer until another slice proves shared shape
  - [ ] reject because difference is real
- [ ] Implement only safe-now or test-backed opportunities.

Completion condition:

```text
Refactoring opportunities are recorded and only behavior-preserving, architecture-strengthening changes proceed.
```

---

## 1.6 Phase 5 — Lock Authority / Semantics / Provenance / Fidelity

Goal:

```text
Prevent canonical meaning, provenance, traceability, fidelity, determinism, and reversibility from being lost across boundaries.
```

Tasks:

- [ ] Identify current authority/provenance-carrying structures.
- [ ] Identify request/result/snapshot types crossing major seams.
- [ ] Identify where Envelope-like carriers are already present or needed.
- [ ] Check whether semantic context is preserved across those crossings.
- [ ] Check whether transformation/projection steps are lossless or explicitly annotated.
- [ ] Check whether reversible/traceable paths exist where required.
- [ ] Add or strengthen tests proving provenance/metadata preservation.
- [ ] Add guardrail tests for loss of provider/vocabulary metadata.
- [ ] Ensure confidence remains annotation unless consumed by explicit policy.
- [ ] Ensure interpretation does not mutate canonical result truth.

Completion condition:

```text
Key analytical outputs preserve semantic context, provenance, traceability, fidelity, confidence, and reversibility across major handoffs.
```

---

## 1.7 Phase 6 — Cap Integration Hubs

Goal:

```text
Stop old hubs from absorbing new target architecture responsibilities.
```

Primary targets:

```text
ChartControllerFactory
ChartControllerFactoryContext
ChartUpdateCoordinator
ChartRenderingOrchestrator
MetricLoadCoordinator
chart-family adapters
```

Tasks:

- [ ] Inspect each target hub.
- [ ] List semantic responsibilities currently held by each.
- [ ] List capability/program responsibilities currently held by each.
- [ ] List provider/qualification responsibilities currently held by each.
- [ ] List evidence/diagnostic responsibilities currently held by each.
- [ ] List delivery/rendering responsibilities currently held by each.
- [ ] Identify responsibilities that should remain coordination-only.
- [ ] Identify responsibilities that should move later, but do not move yet without tests.
- [ ] Add tests or static checks preventing new semantic/provider/evidence/capability authority from being added to these hubs.

Completion condition:

```text
Hubs may coordinate, but must not become homes for authority, capability, provider policy, evidence policy, or delivery policy.
```

---

## 1.8 Phase 7 — Harden Contract / Boundary / Qualification Seam

Goal:

```text
Make contracts, boundaries, qualifications, and bindings the required downstream fan-out seam.
```

Primary structures:

```text
ConsumerDeliveryContract
ConsumerProviderContract
ConsumerProviderRegistry
ChartRenderPlan
ChartRenderDeliveryBinding
ChartRenderPlanProviderMetadata
ChartRenderPlanVocabularyMetadata
ChartBackendSelector
ChartRenderPlanAdapterQualification
ChartRenderPlanAdapterQualificationRules
ChartRenderPlanAdapterDispatcher
```

Tasks:

- [ ] Verify provider metadata survives render-plan projection.
- [ ] Verify vocabulary metadata survives render-plan projection.
- [ ] Verify semantic/provenance context survives contract crossing.
- [ ] Verify provider/plan/backend mismatches are rejected.
- [ ] Verify adapter qualification is enforced before delivery.
- [ ] Verify bindings are inspectable and not hidden route policy.
- [ ] Verify consumers cannot bypass the contract seam where a VNext route exists.
- [ ] Add tests for invalid backend/provider/delivery combinations.
- [ ] Add tests for metadata preservation through delivery binding.

Completion condition:

```text
Downstream delivery is contract-bound, boundary-safe, qualification-backed, binding-explicit, and metadata-preserving.
```

---

## 1.9 Phase 8 — Preserve Projection as Non-Authoritative Translation

Goal:

```text
Keep projectors, adapters, resolvers, selectors, converters, and formatters from owning semantic decisions.
```

Tasks:

- [ ] Identify current projectors/adapters/resolvers/selectors in VNext, Core, UI, and delivery paths.
- [ ] Classify each as projection, adaptation, selection, resolution, construction, or policy.
- [ ] Check whether any translation role owns semantic policy.
- [ ] Check whether any translation role owns provider policy.
- [ ] Check whether any translation role owns evidence policy.
- [ ] Check whether any translation role mutates provenance or confidence.
- [ ] Move no code unless a violation is confirmed and covered by tests.
- [ ] Add guardrail tests for translation roles where practical.

Completion condition:

```text
Projection and translation move meaning across boundaries without creating meaning, hiding policy, or discarding provenance.
```

---

## 1.10 Phase 9 — Thin Consumer / Interaction Layer

Goal:

```text
Keep consumers and interactions non-authoritative.
```

Targets:

```text
chart controllers
controller adapters
tooltip factories
timestamp sinks
event binders
interaction factories
interaction helpers
UI state helpers
```

Tasks:

- [ ] Confirm interaction types relay behavior only.
- [ ] Confirm tooltip logic does not own semantic interpretation.
- [ ] Confirm timestamp sinks do not own analytical authority.
- [ ] Confirm controllers do not select provider policy.
- [ ] Confirm ViewModel/state helpers do not own canonical meaning.
- [ ] Confirm consumer adapters do not own capability planning.
- [ ] Add guardrail tests or structural checks where feasible.

Completion condition:

```text
Consumers receive output and interactions relay behavior without redefining authority, intent, provider policy, or analytical meaning.
```

---

## 1.11 Phase 10 — Elevate SurfaceModel Seam

Goal:

```text
Make consumer-neutral surface models the bridge before terminal delivery.
```

Tasks:

- [ ] Identify current render models, UI models, and delivery models.
- [ ] Identify where a consumer-neutral surface model already exists.
- [ ] Identify where UI/render/vendor assumptions enter too early.
- [ ] Add or strengthen surface-level contracts where needed.
- [ ] Ensure surface models do not own semantic authority.
- [ ] Ensure surface models do not own vendor lifecycle.
- [ ] Ensure surface models preserve envelope/provenance/metadata where required.
- [ ] Add tests proving surface output can be consumed without vendor assumptions where practical.

Completion condition:

```text
Surface-level output is consumer-neutral, metadata-preserving, and upstream of terminal delivery.
```

---

## 1.12 Phase 11 — Demote Terminal Delivery

Goal:

```text
Keep rendering, backends, vendors, hosts, and lifecycle terminal and replaceable.
```

Targets:

```text
render engines
render adapters
render surfaces
backend selectors
Syncfusion-specific types
LiveCharts-specific types
ECharts-specific types
render hosts
vendor lifecycle helpers
```

Tasks:

- [ ] Identify vendor-specific delivery boundaries.
- [ ] Confirm vendor code does not define analytical meaning.
- [ ] Confirm backend code does not define semantic policy.
- [ ] Confirm rendering code does not own interpretation.
- [ ] Confirm host/lifecycle code is terminal.
- [ ] Confirm delivery adapters consume surface/contract output rather than upstream internals.
- [ ] Add tests proving upstream contracts do not depend on vendor-specific types.
- [ ] Defer broad render-family consolidation until at least two hardened slices reveal a shared shape.

Completion condition:

```text
Delivery remains downstream, replaceable, vendor-contained, and semantically non-authoritative.
```

---

## 1.13 Phase 12 — Preserve Evidence as Observational

Goal:

```text
Ensure evidence, diagnostics, parity, reachability, validation, and audit do not control live behavior.
```

Tasks:

- [ ] Identify all evidence/diagnostics readers.
- [ ] Identify all evidence/export services.
- [ ] Identify parity evaluators.
- [ ] Identify reachability validators.
- [ ] Identify validation flows.
- [ ] Identify audit/export flows.
- [ ] Confirm evidence reads state but does not mutate live decisions.
- [ ] Confirm diagnostics do not select providers.
- [ ] Confirm parity does not route execution.
- [ ] Confirm reachability does not become live policy.
- [ ] Confirm validation does not become hidden runtime authority unless explicitly designed as policy.
- [ ] Add guardrail tests or static checks where feasible.

Completion condition:

```text
Evidence proves, records, exports, validates, and audits without controlling runtime behavior.
```

---

## 1.14 Phase 13 — Add Governance Constraints

Goal:

```text
Make architectural growth bounded and auditable.
```

Tasks:

- [ ] Define guardrails for new vocabulary.
- [ ] Define guardrails for new concepts.
- [ ] Define guardrails for new capabilities.
- [ ] Define guardrails for new transformations.
- [ ] Define guardrails for new consumers.
- [ ] Define guardrails for new delivery backends.
- [ ] Define guardrails for new evidence paths.
- [ ] Ensure future additions map to the reduced grammar.
- [ ] Ensure future additions do not centralize UI/render/vendor concerns.
- [ ] Ensure future additions support losslessness, traceability, reversibility, neutrality, or evidence.

Completion condition:

```text
New architectural growth is possible, governed, auditable, and aligned with the target grammar.
```

---

## 1.15 Phase 14 — Resume Capability Expansion

Goal:

```text
Add new analytical behavior only through the target spine.
```

Allowed path:

```text
Intent
-> Capability
-> Composition / Transformation
-> Interpretation / Confidence / Overlay
-> Program
-> Contract / Boundary
-> Qualification
-> Provider / Consumer / SurfaceModel
-> Delivery
-> Evidence / Audit
```

Tasks:

- [ ] Select one small new or existing capability slice.
- [ ] Express it through capability/program structures.
- [ ] Keep UI/controller/rendering out of capability ownership.
- [ ] Preserve provenance and traceability.
- [ ] Preserve transformation reversibility where applicable.
- [ ] Preserve confidence as annotation unless policy explicitly consumes it.
- [ ] Add tests through the contract/delivery/evidence path.
- [ ] Verify no old hub absorbs the new capability.

Completion condition:

```text
At least one capability moves through the target spine without old-hub ownership.
```

---

## 1.16 Phase 15 — Validate Non-Chart Consumer Path

Goal:

```text
Prove the architecture is not chart-only.
```

Candidate consumers:

```text
evidence export
report export
API-style response
plugin-style consumer
diagnostic consumer
```

Tasks:

- [ ] Select one non-chart or chart-independent consumer path.
- [ ] Use the same contract/boundary/qualification model.
- [ ] Use a consumer-neutral surface or equivalent output shape.
- [ ] Avoid render-specific assumptions.
- [ ] Preserve semantics/provenance/evidence.
- [ ] Add tests proving consumer-general behavior.

Completion condition:

```text
At least one non-chart consumer uses the same target seam without chart/render assumptions.
```

---

## 1.17 Phase 16 — Retire Legacy Bypasses Selectively

Goal:

```text
Remove old bypasses only after target replacements are proven.
```

Tasks:

- [ ] List known legacy bypass paths.
- [ ] For each path, identify target replacement.
- [ ] Confirm parity evidence.
- [ ] Confirm smoke evidence.
- [ ] Confirm metadata preservation.
- [ ] Confirm semantic/provenance preservation.
- [ ] Confirm no consumer still depends on the bypass.
- [ ] Remove one bypass at a time.

Completion condition:

```text
Legacy coexistence shrinks without behavior loss, metadata loss, or semantic drift.
```

---

## 1.18 Phase 17 — Consolidate Repeated Family Patterns Last

Goal:

```text
Collapse repeated request/route/qualification/adapter patterns only after multiple slices prove a shared shape.
```

Tasks:

- [ ] Compare at least two hardened family slices.
- [ ] Identify repeated safe structure.
- [ ] Identify real family-specific differences.
- [ ] Extract only genuinely shared patterns.
- [ ] Preserve explicit differences.
- [ ] Preserve behavior.
- [ ] Preserve tests.
- [ ] Confirm consolidation strengthens the target seam.

Completion condition:

```text
Consolidation strengthens the generalized architecture instead of hiding real differences.
```

---

# 2. Guardrails

## 2.1 Refactoring Guardrails

- [ ] Refactoring must reduce sprawl, contradiction, duplication, or exception-driven architecture.
- [ ] Refactoring must strengthen the generalized target architecture.
- [ ] Refactoring must preserve behavior.
- [ ] Refactoring must preserve tests.
- [ ] Refactoring must not hide real family-specific differences.
- [ ] Refactoring must not centralize UI, rendering, vendor, process, or evidence authority.
- [ ] Refactoring must not replace explicit seams with hidden orchestration.
- [ ] Refactoring must be auditable through tests, parity, or dependency evidence.

---

## 2.3 Authority Guardrails

- [ ] UI must not define canonical meaning.
- [ ] Rendering must not define analytical meaning.
- [ ] Process must not define semantic truth.
- [ ] Evidence must not create live semantic authority.
- [ ] Provider code must not become semantic authority.
- [ ] Consumer code must not redefine canonical meaning.

---

## 2.3 Provenance / Traceability Guardrails

- [ ] Results must preserve source lineage.
- [ ] Interpretations must preserve derivation context.
- [ ] Transformations must record traceability.
- [ ] Projections must not discard provenance silently.
- [ ] Delivery must not remove required metadata.
- [ ] Evidence must expose lineage where relevant.

---

## 2.4 Fidelity / Reversibility Guardrails

- [ ] Transformations must be lossless or explicitly annotated as lossy.
- [ ] Derived outputs must preserve recovery path where required.
- [ ] Projections must preserve semantic fidelity.
- [ ] Delivery adaptation must not alter canonical meaning.
- [ ] Any irreversible step must be explicit and justified.

---

## 2.5 Capability Guardrails

- [ ] New analytical behavior must enter through capability/program structures.
- [ ] Controllers must not become capability owners.
- [ ] Renderers must not become capability owners.
- [ ] Coordinators must not absorb capability planning.
- [ ] Capability output must remain contract-bound.

---

## 2.6 Contract / Boundary Guardrails

- [ ] Contracts must be explicit at major handoffs.
- [ ] Boundaries must prevent vendor assumptions from moving upstream.
- [ ] Boundaries must prevent UI assumptions from moving upstream.
- [ ] Boundaries must preserve metadata.
- [ ] Boundary bypasses must be identified and retired only after replacement proof.

---

## 2.7 Qualification Guardrails

- [ ] Provider compatibility must be qualified.
- [ ] Backend compatibility must be qualified.
- [ ] Adapter compatibility must be qualified.
- [ ] Delivery compatibility must be qualified.
- [ ] Qualification must not become hidden selection.
- [ ] Selection must be justified when compatibility matters.

---

## 2.8 Consumer / Interaction Guardrails

- [ ] Consumers receive meaning; they do not define it.
- [ ] Interaction relays behavior; it does not redefine intent.
- [ ] Tooltip logic must not own interpretation.
- [ ] Timestamp sinks must not own analytical meaning.
- [ ] ViewModels must not become semantic authorities.
- [ ] Controllers must not own provider policy.

---

## 2.9 Delivery Guardrails

- [ ] Delivery remains terminal.
- [ ] Vendor concerns remain terminal.
- [ ] Backend concerns remain terminal.
- [ ] Rendering remains replaceable.
- [ ] Host/lifecycle concerns remain downstream.
- [ ] Delivery must not mutate canonical semantics.

---

## 2.10 Evidence / Audit Guardrails

- [ ] Evidence observes only.
- [ ] Diagnostics do not control live routing.
- [ ] Parity does not control live execution.
- [ ] Reachability does not select providers.
- [ ] Validation does not become hidden authority.
- [ ] Audit records must be durable and reviewable.

---

## 2.11 Governance Guardrails

- [ ] New vocabulary must improve architectural clarity.
- [ ] New concepts must align with project goals.
- [ ] New abstractions must not centralize UI/render/vendor concerns.
- [ ] New capabilities must use the target spine.
- [ ] New consumers must use contracts/boundaries.
- [ ] New delivery paths must remain replaceable.

---

# 3. Completion Criteria

## 3.1 Structural Completion

- [ ] Authority, semantics, provenance, and traceability are explicit upstream concerns.
- [ ] Envelopes or equivalent carriers preserve semantic context across major seams.
- [ ] Reasoning owns capability, composition, transformation, interpretation, confidence, and overlay.
- [ ] Program structures bridge reasoning into downstream contracts.
- [ ] Contracts/boundaries/qualification/bindings are the required downstream handoff.
- [ ] Qualification governs provider/backend/adapter/delivery compatibility.
- [ ] SurfaceModel or equivalent consumer-neutral, metadata-preserving shape exists before terminal delivery.
- [ ] Delivery is terminal and replaceable.
- [ ] Evidence is observational and auditable.
- [ ] Governance constraints exist for future growth.

---

## 3.2 Behavioral Completion

- [ ] Existing behavior is preserved.
- [ ] Existing tests pass.
- [ ] New guardrail tests pass.
- [ ] Parity is preserved for migrated paths.
- [ ] Smoke tests pass for affected delivery paths.
- [ ] Metadata preservation tests pass.
- [ ] Invalid provider/backend/adapter combinations are rejected.
- [ ] Evidence does not affect live execution.

---

## 3.3 Migration Completion

- [ ] Refactoring opportunities have been classified and acted on where safe.
- [ ] Sprawl, duplication, contradiction, and exception-driven paths are reduced where proven.
- [ ] Major old hubs no longer absorb new target responsibilities.
- [ ] New capabilities enter through the target spine.
- [ ] At least one non-chart consumer path is proven.
- [ ] Legacy bypasses are reduced or explicitly bounded.
- [ ] Repeated family patterns are consolidated only where proven safe.
- [ ] Current architecture can be described through the target grammar.
- [ ] Documentation reflects validated state, not intended future state.

---

## 3.4 Closure Definition

The migration can be considered complete when:

```text
canonical meaning is upstream
provenance is traceable
capabilities are composable
contracts govern handoff
qualification governs compatibility
consumers remain non-authoritative
delivery remains replaceable
evidence remains observational
governance controls future growth
```

No closure claim is valid unless supported by:

```text
tests
parity evidence
metadata preservation
dependency review
guardrail checks
updated documentation
```

---

# 4. Progress Log

Use this section during implementation.

| Date | Phase | Change | Evidence / Test | Status |
|---|---|---|---|---|
| 2026-04-28 | Phase 1 | Regenerated baseline artifacts and recorded structural baseline. | `project-tree.txt`; `codebase-index.md`; `dependency-summary.md`; `type-dependency-diagram.md`; supplied dependency diagram parsed at 4434 edges; test discovery listed 925 tests; indexed 988 symbols; density reading 0.6656%; validation passed 925 tests. | Complete |
| 2026-04-28 | Phase 2 | Updated dependency summary generator to include the architectural vocabulary in boundary-evaluation references. | `scripts/Generate-DependencySummary.ps1`; regenerated `dependency-summary.md`; confirmed `Project Bible.md`, `SYSTEM_MAP.md`, and `DataVisualiser-Architectural-Vocabulary.md` references; validation passed 925 tests. | Complete |
| 2026-04-28 | Phase 3 | Classified dependency density before refactoring. | `documents/DataVisualiser_Dependency_Density_Audit.md`; supplied diagram parsed at 4434 edges; generated diagram recorded 6039 textual edges and 0.6656% density; no actual drift confirmed by density alone. | Complete |

---

# 5. Agent Instruction Summary

For IDE agents:

```text
Read the vocabulary and target architecture document first.
Use this file as the sequential execution plan.
Do not skip phases.
Do not refactor broadly or cosmetically.
Do pursue targeted refactoring that reduces sprawl, contradiction, duplication, or exception-driven structure.
Do not add new capability before containment.
Do not move code without behavior-preserving tests.
Prefer small auditable changes.
Prefer generalized seams where shared structure is proven.
Update progress only when tests or evidence support completion.
```
