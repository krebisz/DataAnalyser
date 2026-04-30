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

Current migration read after Phases 1-5:

```text
VNext authority/provenance spine exists and is testable.
Legacy/UI paths remain active transitional bridges.
Generated governance now references the architectural vocabulary.
Dependency density is classified before refactoring.
Refactoring opportunities are documented but not yet broadly actionable.
Provenance, provider, vocabulary, confidence, and interpretation guardrails now exist.
The next implementation risk is old integration hubs absorbing new responsibilities.
Phase 6 should focus on containment and seam hardening before capability expansion.
```

---

## Produced Artifact Index

This index records the evidence produced or consumed by Phases 1-13. Phase 1 and Phase 2 use generated repository artifacts and script output rather than dedicated audit notes; Phases 3-13 use focused audit notes plus targeted tests where applicable.

| Phase | Artifact Type | Files |
|---|---|---|
| Phase 1 | Generated baseline | `project-tree.txt`; `codebase-index.md`; `dependency-summary.md`; `type-dependency-diagram.md` |
| Phase 2 | Generator/script update | `scripts/Generate-DependencySummary.ps1`; regenerated `dependency-summary.md` |
| Phase 3 | Audit note | `documents/DataVisualiser_Dependency_Density_Audit.md` |
| Phase 4 | Audit note | `documents/DataVisualiser_Refactoring_Opportunity_Audit.md` |
| Phase 5 | Audit note and guardrails | `documents/DataVisualiser_Authority_Provenance_Fidelity_Audit.md`; VNext provenance/fidelity tests |
| Phase 6 | Audit note and guardrails | `documents/DataVisualiser_Integration_Hub_Containment_Audit.md`; `ArchitectureGuardrailTests` |
| Phase 7 | Audit note and guardrails | `documents/DataVisualiser_Contract_Boundary_Qualification_Audit.md`; contract/boundary tests |
| Phase 8 | Audit note and guardrails | `documents/DataVisualiser_Projection_Translation_Containment_Audit.md`; projection/adapter guardrails |
| Phase 9 | Audit note and guardrails | `documents/DataVisualiser_Consumer_Interaction_Containment_Audit.md`; consumer/interaction guardrails |
| Phase 10 | Audit note and guardrails | `documents/DataVisualiser_Surface_Model_Seam_Audit.md`; surface-model guardrails |
| Phase 11 | Audit note and guardrails | `documents/DataVisualiser_Terminal_Delivery_Boundary_Audit.md`; terminal-delivery guardrails |
| Phase 12 | Audit note and guardrails | `documents/DataVisualiser_Evidence_Observability_Audit.md`; evidence observability guardrails |
| Phase 13 | Audit note and guardrails | `documents/DataVisualiser_Governance_Constraints_Audit.md`; governance documentation guardrails |
| Phase 14 | Capability-slice audit and guardrails | `documents/DataVisualiser_Distribution_Capability_Slice_Audit.md`; Distribution target-spine tests |

Historical planning note:

```text
DataVisualiser_Subsystem_Plan.md uses older subsystem phase numbering.
This migration plan is the active sequential execution plan for the current Phase 1-22 migration described here.
When phase numbers conflict, use this migration plan and its progress log as the authoritative current sequence.
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

- [x] Identify repeated structures across chart/render/delivery families.
- [x] Identify exception-driven paths that can become generalized seams.
- [x] Identify contradictory ownership between Core, VNext, UI, rendering, delivery, and evidence.
- [x] Identify hubs that can lose responsibility safely.
- [x] Identify duplicated request/route/qualification/adapter patterns.
- [x] Identify places where target grammar can replace implementation-shaped concepts:
  - [x] `Controller` -> `ConsumerAdapter`
  - [x] `ViewModel` -> `ConsumerState`
  - [x] `Renderer` -> `DeliveryAdapter`
  - [x] `Route` -> `Binding`
  - [x] `Host` -> `RuntimeBoundary` / `DeliverySurface`
  - [x] `Diagnostics` -> `Evidence`, where proof/audit is intended
- [x] Classify each opportunity as:
  - [x] safe now
  - [x] needs tests first
  - [x] defer until another slice proves shared shape
  - [x] reject because difference is real
- [x] Implement only safe-now or test-backed opportunities.

Completion condition:

```text
Refactoring opportunities are recorded and only behavior-preserving, architecture-strengthening changes proceed.
```

Phase 4 evidence:

```text
Audit note:
- documents/DataVisualiser_Refactoring_Opportunity_Audit.md

Classification result:
- safe-now code refactors: none identified
- needs tests first: old hub caps for ChartUpdateCoordinator, ChartRenderingOrchestrator, MetricLoadCoordinator, ChartControllerFactory, EvidenceDiagnosticsBuilder; contract/boundary metadata preservation and mismatch rejection
- defer until another slice proves shared shape: repeated render request/route/qualification/capability/host/surface/adapter/builder patterns
- reject for now: cosmetic target-grammar renames and dense domain/state carrier refactors

Implementation result:
- no behavior code changed in Phase 4
- no refactoring performed without safe-now or test-backed justification
```

---

## 1.6 Phase 5 — Lock Authority / Semantics / Provenance / Fidelity

Goal:

```text
Prevent canonical meaning, provenance, traceability, fidelity, determinism, and reversibility from being lost across boundaries.
```

Tasks:

- [x] Identify current authority/provenance-carrying structures.
- [x] Identify request/result/snapshot types crossing major seams.
- [x] Identify where Envelope-like carriers are already present or needed.
- [x] Check whether semantic context is preserved across those crossings.
- [x] Check whether transformation/projection steps are lossless or explicitly annotated.
- [x] Check whether reversible/traceable paths exist where required.
- [x] Add or strengthen tests proving provenance/metadata preservation.
- [x] Add guardrail tests for loss of provider/vocabulary metadata.
- [x] Ensure confidence remains annotation unless consumed by explicit policy.
- [x] Ensure interpretation does not mutate canonical result truth.

Completion condition:

```text
Key analytical outputs preserve semantic context, provenance, traceability, fidelity, confidence, and reversibility across major handoffs.
```

Phase 5 evidence:

```text
Audit note:
- documents/DataVisualiser_Authority_Provenance_Fidelity_Audit.md

Code inspected:
- DataVisualiser/VNext/Contracts
- DataVisualiser/VNext/Application
- DataVisualiser/VNext/Rendering
- DataVisualiser/Core/Rendering/Adapters
- DataVisualiser.Tests/VNext
- DataVisualiser.Tests/UI/MainHost

Tests strengthened:
- DataVisualiser.Tests/VNext/AnalyticalIntentContractsTests.cs
- DataVisualiser.Tests/VNext/AnalyticalInterpretationBuilderTests.cs

Validation:
- targeted VNext provenance/fidelity guardrails passed 70 tests

Implementation result:
- no production behavior code changed in Phase 5
- existing VNext authority/provenance carriers were confirmed sufficient for this phase
- guardrail tests now cover provider/vocabulary metadata and interpretation confidence immutability
- legacy projection, derived-program reversibility, and lossy density projection remain documented transitional risks for later phases
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

- [x] Inspect each target hub.
- [x] List semantic responsibilities currently held by each.
- [x] List capability/program responsibilities currently held by each.
- [x] List provider/qualification responsibilities currently held by each.
- [x] List evidence/diagnostic responsibilities currently held by each.
- [x] List delivery/rendering responsibilities currently held by each.
- [x] Identify responsibilities that should remain coordination-only.
- [x] Identify responsibilities that should move later, but do not move yet without tests.
- [x] Add tests or static checks preventing new semantic/provider/evidence/capability authority from being added to these hubs.

Completion condition:

```text
Hubs may coordinate, but must not become homes for authority, capability, provider policy, evidence policy, or delivery policy.
```

Phase 6 evidence:

```text
Audit note:
- documents/DataVisualiser_Integration_Hub_Containment_Audit.md

Hubs inspected:
- DataVisualiser/Core/Orchestration/ChartUpdateCoordinator.cs
- DataVisualiser/Core/Orchestration/ChartRenderingOrchestrator.cs
- DataVisualiser/UI/ViewModels/MetricLoadCoordinator.cs
- DataVisualiser/UI/Charts/Presentation/ChartControllerFactory.cs
- chart-family adapters under DataVisualiser/UI/Charts/Presentation

Tests added:
- ChartRenderingOrchestrator_ShouldRemainCoordinationOnly
- MetricLoadCoordinator_ShouldNotAcquireRenderProviderOrInterpretationPolicy
- ChartFamilyAdapters_ShouldNotAcquireProviderBackendOrAnalyticalAuthority

Validation:
- ArchitectureGuardrailTests passed 75 tests

Implementation result:
- no production behavior code changed in Phase 6
- hub responsibilities were classified before moving code
- containment guardrails now cover orchestrator, load coordinator, and chart-family adapters
- MetricLoadCoordinator remains the highest-risk transitional bridge and should be revisited after contract/boundary/projection phases
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

- [x] Verify provider metadata survives render-plan projection.
- [x] Verify vocabulary metadata survives render-plan projection.
- [x] Verify semantic/provenance context survives contract crossing.
- [x] Verify provider/plan/backend mismatches are rejected.
- [x] Verify adapter qualification is enforced before delivery.
- [x] Verify bindings are inspectable and not hidden route policy.
- [x] Verify consumers cannot bypass the contract seam where a VNext route exists.
- [x] Add tests for invalid backend/provider/delivery combinations.
- [x] Add tests for metadata preservation through delivery binding.

Completion condition:

```text
Downstream delivery is contract-bound, boundary-safe, qualification-backed, binding-explicit, and metadata-preserving.
```

Phase 7 evidence:

```text
Audit note:
- documents/DataVisualiser_Contract_Boundary_Qualification_Audit.md

Structures inspected:
- ConsumerDeliveryContract
- ConsumerProviderContract
- ConsumerProviderRegistry
- ChartRenderPlan
- ChartRenderDeliveryBinding
- ChartRenderPlanProviderMetadata
- ChartRenderPlanVocabularyMetadata
- ChartBackendSelector
- ChartRenderPlanAdapterQualification
- ChartRenderPlanAdapterQualificationRules
- ChartRenderPlanAdapterDispatcher
- AnalyticalRenderPlanPipeline

Tests added:
- RenderDeliveryBinding_ShouldPreserveExistingSemanticMetadataWhenAttached

Validation:
- VNext contract/boundary/qualification tests passed 67 tests
- ArchitectureGuardrailTests passed 75 tests

Implementation result:
- no production behavior code changed in Phase 7
- delivery binding remains explicit and inspectable
- provider/backend mismatch rejection and adapter qualification are test-backed
- binding metadata now has a guardrail proving semantic/provenance metadata is preserved
```

---

## 1.9 Phase 8 — Preserve Projection as Non-Authoritative Translation

Goal:

```text
Keep projectors, adapters, resolvers, selectors, converters, and formatters from owning semantic decisions.
```

Tasks:

- [x] Identify current projectors/adapters/resolvers/selectors in VNext, Core, UI, and delivery paths.
- [x] Classify each as projection, adaptation, selection, resolution, construction, or policy.
- [x] Check whether any translation role owns semantic policy.
- [x] Check whether any translation role owns provider policy.
- [x] Check whether any translation role owns evidence policy.
- [x] Check whether any translation role mutates provenance or confidence.
- [x] Move no code unless a violation is confirmed and covered by tests.
- [x] Add guardrail tests for translation roles where practical.

Completion condition:

```text
Projection and translation move meaning across boundaries without creating meaning, hiding policy, or discarding provenance.
```

Phase 8 evidence:

```text
Audit note:
- documents/DataVisualiser_Projection_Translation_Containment_Audit.md

Translation roles inspected:
- ChartRenderPlanProjector
- LegacyChartProgramProjector
- ChartBackendSelector
- ChartRenderPlanAdapterDispatcher
- render-plan adapters under Core/Rendering
- route resolvers under Core/Rendering/Contracts
- chart-family adapters/helpers under UI/Charts/Presentation
- formatter/converter helpers under Core/Rendering and UI/Converters

Tests added:
- RenderPlanProjectors_ShouldRemainNonAuthoritativeTranslation
- RenderPlanAdapters_ShouldNotAcquireAuthorityPolicyOrEvidenceOwnership
- AdapterDispatcher_ShouldOnlyQualifyAndDispatchPlans

Validation:
- ArchitectureGuardrailTests passed 78 tests
- projector/adapter validation passed 33 tests

Implementation result:
- no production behavior code changed in Phase 8
- no translation role violation confirmed
- projectors, adapters, and dispatcher are now guarded against acquiring authority, evidence, provider, confidence, or interpretation policy
- UI/controller adapter thinning is intentionally carried into Phase 9
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

- [x] Confirm interaction types relay behavior only.
- [x] Confirm tooltip logic does not own semantic interpretation.
- [x] Confirm timestamp sinks do not own analytical authority.
- [x] Confirm controllers do not select provider policy.
- [x] Confirm ViewModel/state helpers do not own canonical meaning.
- [x] Confirm consumer adapters do not own capability planning.
- [x] Add guardrail tests or structural checks where feasible.

Completion condition:

```text
Consumers receive output and interactions relay behavior without redefining authority, intent, provider policy, or analytical meaning.
```

Phase 9 evidence:

```text
Audit note:
- documents/DataVisualiser_Consumer_Interaction_Containment_Audit.md

Consumer/interaction targets inspected:
- chart controllers under DataVisualiser/UI/Charts/Controllers
- controller adapters under DataVisualiser/UI/Charts/Presentation
- tooltip factories/helpers under DataVisualiser/UI/Charts/Interaction
- timestamp sink contract under DataVisualiser/Core/Rendering/Interaction
- tooltip formatting helpers under DataVisualiser/Core/Rendering/Tooltip
- event binders under DataVisualiser/UI/MainHost/Coordination
- UI state helpers under DataVisualiser/UI/State
- view-model state helpers under DataVisualiser/UI/ViewModels

Tests added:
- ChartControllersAndInteractions_ShouldRemainNonAuthoritativeConsumers
- EventBindersAndUiStateHelpers_ShouldNotOwnAnalyticalOrProviderPolicy
- ViewModelStateHelpers_ShouldNotConstructAnalyticalMeaning

Validation:
- Phase 9 consumer/interaction validation passed 118 tests

Implementation result:
- no production behavior code changed in Phase 9
- consumer/interaction targets were confirmed as event, display, state, and interaction relays
- provider/backend, analytical intent, confidence, interpretation, render-plan projection, adapter dispatch, and evidence export ownership are now guarded out of the targeted consumer/interaction layer
- chart-family adapter risk remains visible through Phase 6, Phase 8, and Phase 9 guardrails
```

---

## 1.11 Phase 10 — Elevate SurfaceModel Seam

Goal:

```text
Make consumer-neutral surface models the bridge before terminal delivery.
```

Tasks:

- [x] Identify current render models, UI models, and delivery models.
- [x] Identify where a consumer-neutral surface model already exists.
- [x] Identify where UI/render/vendor assumptions enter too early.
- [x] Add or strengthen surface-level contracts where needed.
- [x] Ensure surface models do not own semantic authority.
- [x] Ensure surface models do not own vendor lifecycle.
- [x] Ensure surface models preserve envelope/provenance/metadata where required.
- [x] Add tests proving surface output can be consumed without vendor assumptions where practical.

Completion condition:

```text
Surface-level output is consumer-neutral, metadata-preserving, and upstream of terminal delivery.
```

Phase 10 evidence:

```text
Audit note:
- documents/DataVisualiser_Surface_Model_Seam_Audit.md

Model types inspected:
- ChartRenderPlan
- ChartRenderModel
- UiChartRenderModel
- ChartSeriesModel
- ChartFacetModel
- ChartAxisModel
- ChartLegendModel
- ChartOverlayModel
- ChartInteractionModel
- render request/host/surface records under Core/Rendering/Contracts
- render-plan adapters under Core/Rendering
- UI renderers under UI/Charts/Presentation

Tests added:
- ChartRenderPlan_ShouldRemainConsumerNeutralSurfaceModel
- SurfaceModels_ShouldNotAcquireSemanticProviderOrEvidenceAuthority
- UiRenderModels_ShouldStayUpstreamOfConcreteRendererLifecycle

Validation:
- Phase 10 surface-model validation passed 121 tests

Implementation result:
- no production behavior code changed in Phase 10
- ChartRenderPlan is confirmed as the consumer-neutral, metadata-preserving surface seam
- ChartRenderModel remains a legacy render model with WPF color assumptions
- UiChartRenderModel remains a UI presentation model upstream of concrete renderer lifecycle
- terminal delivery structures are carried into Phase 11
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

- [x] Identify vendor-specific delivery boundaries.
- [x] Confirm vendor code does not define analytical meaning.
- [x] Confirm backend code does not define semantic policy.
- [x] Confirm rendering code does not own interpretation.
- [x] Confirm host/lifecycle code is terminal.
- [x] Confirm delivery adapters consume surface/contract output rather than upstream internals.
- [x] Add tests proving upstream contracts do not depend on vendor-specific types.
- [x] Defer broad render-family consolidation until at least two hardened slices reveal a shared shape.

Completion condition:

```text
Delivery remains downstream, replaceable, vendor-contained, and semantically non-authoritative.
```

Phase 11 evidence:

```text
Audit note:
- documents/DataVisualiser_Terminal_Delivery_Boundary_Audit.md

Delivery/vendor boundaries inspected:
- render engines under DataVisualiser/Core/Rendering/Engines
- render adapters under DataVisualiser/Core/Rendering/Adapters
- chart-family render adapters under DataVisualiser/Core/Rendering
- backend capability/selector types under DataVisualiser/VNext/Rendering
- Syncfusion render contracts under DataVisualiser/Core/Rendering/Contracts/Syncfusion
- Syncfusion delivery adapter under DataVisualiser/Core/Rendering/Syncfusion
- LiveCharts renderer under DataVisualiser/UI/Charts/Presentation/LiveCharts
- ECharts placeholder renderer/surface under DataVisualiser/UI/Charts/Presentation/ECharts
- render host/lifecycle helpers under DataVisualiser/UI/Charts/Presentation

Tests added:
- VNextSurfaceAndBackendContracts_ShouldNotImportConcreteVendorOrUiLibraries
- TerminalRenderDelivery_ShouldNotAcquireAnalyticalOrEvidenceAuthority
- RenderingHostLifecycleHelpers_ShouldRemainTerminalWiring

Validation:
- Phase 11 terminal-delivery validation passed 142 tests

Implementation result:
- no production behavior code changed in Phase 11
- upstream VNext surface/backend contracts remain free of concrete vendor/UI imports
- terminal render delivery is guarded against analytical, provider-resolution, interpretation, confidence, and evidence authority
- broad render-family consolidation remains deferred
- Syncfusion view evidence export composition is carried into Phase 12
```

---

## 1.13 Phase 12 — Preserve Evidence as Observational

Goal:

```text
Ensure evidence, diagnostics, parity, reachability, validation, and audit do not control live behavior.
```

Tasks:

- [x] Identify all evidence/diagnostics readers.
- [x] Identify all evidence/export services.
- [x] Identify parity evaluators.
- [x] Identify reachability validators.
- [x] Identify validation flows.
- [x] Identify audit/export flows.
- [x] Confirm evidence reads state but does not mutate live decisions.
- [x] Confirm diagnostics do not select providers.
- [x] Confirm parity does not route execution.
- [x] Confirm reachability does not become live policy.
- [x] Confirm validation does not become hidden runtime authority unless explicitly designed as policy.
- [x] Add guardrail tests or static checks where feasible.

Completion condition:

```text
Evidence proves, records, exports, validates, and audits without controlling runtime behavior.
```

Phase 12 evidence:

```text
Audit note:
- documents/DataVisualiser_Evidence_Observability_Audit.md

Evidence/diagnostics/export paths inspected:
- DataVisualiser/UI/MainHost/Evidence
- DataVisualiser/UI/MainHost/Export
- DataVisualiser/Core/Validation
- DataVisualiser/Core/Validation/Parity
- DataVisualiser/Core/Strategies/Reachability
- session milestone recorders under UI/Admin, UI/Workspace, UI/Charts/Presentation

Tests added:
- EvidenceAndDiagnostics_ShouldRemainObservationalNotLiveRouting
- EvidenceExport_ShouldKeepFileSystemWritesOnDedicatedExportWriter
- ParityAndReachabilityEvidence_ShouldNotMutateLiveChartState

Validation:
- Phase 12 evidence/diagnostics/parity/reachability validation passed 158 tests

Implementation result:
- no production behavior code changed in Phase 12
- evidence/export remains observational and file writes remain on the dedicated writer seam
- diagnostics and parity evidence are guarded against live rendering/provider/backend routing
- evidence export timing remains permitted as observational telemetry
- runtime validation/parity policy remains explicit and separate from evidence/export
- Syncfusion view evidence export composition remains documented host composition
```

---

## 1.14 Phase 13 — Add Governance Constraints

Goal:

```text
Make architectural growth bounded and auditable.
```

Tasks:

- [x] Define guardrails for new vocabulary.
- [x] Define guardrails for new concepts.
- [x] Define guardrails for new capabilities.
- [x] Define guardrails for new transformations.
- [x] Define guardrails for new consumers.
- [x] Define guardrails for new delivery backends.
- [x] Define guardrails for new evidence paths.
- [x] Ensure future additions map to the reduced grammar.
- [x] Ensure future additions do not centralize UI/render/vendor concerns.
- [x] Ensure future additions support losslessness, traceability, reversibility, neutrality, or evidence.

Completion condition:

```text
New architectural growth is possible, governed, auditable, and aligned with the target grammar.
```

Phase 13 evidence:

```text
Audit note:
- documents/DataVisualiser_Governance_Constraints_Audit.md

Governance constraints defined for:
- new vocabulary
- new concepts
- new capabilities
- new transformations
- new consumers
- new delivery backends
- new evidence paths

Tests added:
- ArchitectureVocabulary_ShouldDefineGovernedGrowthCriteriaAndExtensionPool
- MigrationPlan_ShouldKeepGovernanceBeforeCapabilityExpansion
- GovernanceConstraintsAudit_ShouldMapFutureGrowthToReducedGrammar

Validation:
- ArchitectureGuardrailTests passed 93 tests

Implementation result:
- no production behavior code changed in Phase 13
- future growth is tied to the reduced grammar and governed extension pool
- capability expansion remains gated behind governance and the target spine
- source-boundary guardrails from Phases 6 through 12 remain the enforcement layer for current code
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

- [x] Select one small new or existing capability slice.
- [x] Express it through capability/program structures.
- [x] Keep UI/controller/rendering out of capability ownership.
- [x] Preserve provenance and traceability.
- [x] Preserve transformation reversibility where applicable.
- [x] Preserve confidence as annotation unless policy explicitly consumes it.
- [x] Add tests through the contract/delivery/evidence path.
- [x] Verify no old hub absorbs the new capability.

Completion condition:

```text
At least one capability moves through the target spine without old-hub ownership.
```

Phase 14 evidence:

```text
Audit note:
- documents/DataVisualiser_Distribution_Capability_Slice_Audit.md

Selected capability slice:
- active Distribution capability
- active WeekdayTrend capability
- active Transform capability

Target-spine proof:
- intent/program entry: AnalyticalIntentFactory.Distribution and ChartProgramRequest.Distribution
- capability mapping: CapabilityRequest.FromProgramRequest maps Distribution to AnalyticalCapabilityKind.Distribution and CompositionKind.SingleSeries
- contract/boundary: ConsumerDeliveryContract / ChartProgramDeliveryTargetResolver provides DistributionChart delivery
- qualification: DistributionRenderingContract exposes qualified Cartesian route and tactical polar fallback route
- surface/delivery: DistributionChartControllerAdapter attaches DistributionCapabilityContract to the live render request, and DistributionRenderPlanBuilder emits ChartRenderPlan metadata from that runtime capability/program/delivery contract
- evidence: VNextDataResolutionHelper records EvidenceRuntimePath.VNextDistribution and render-plan diagnostics capture Distribution metadata
- intent/program entry: ChartProgramRequest.WeekdayTrend
- capability mapping: CapabilityRequest.FromProgramRequest maps WeekdayTrend to AnalyticalCapabilityKind.TemporalTrend and CompositionKind.SingleSeries
- contract/boundary: ConsumerDeliveryContract / ChartProgramDeliveryTargetResolver provides WeekdayTrendChart delivery
- qualification: WeekdayTrendRenderingContract exposes the existing WeekdayTrend render-plan path while carrying WeekdayTrendCapabilityContract through the live controller request
- surface/delivery: WeekdayTrendChartControllerAdapter attaches WeekdayTrendCapabilityContract to the live render request, and WeekdayTrendRenderPlanBuilder emits ChartRenderPlan metadata from that runtime capability/program/delivery contract
- intent/program entry: ChartProgramRequest.Transform with explicit SeriesOperationRequest payloads where the UI can derive the selected operation
- capability mapping: CapabilityRequest.FromProgramRequest maps Transform to AnalyticalCapabilityKind.Transform and CompositionKind.DerivedSeries when operation payloads are present
- contract/boundary: ConsumerDeliveryContract / ChartProgramDeliveryTargetResolver provides TransformChart delivery
- qualification: TransformRenderingContract and TransformChartRenderInvoker carry TransformCapabilityContract through the existing render-plan adapter path without moving transform semantics into UI/controller code
- surface/delivery: TransformChartPresentationCoordinator derives operation requests from the selected transform operation and supplies TransformCapabilityContract to the render request

Tests added:
- DistributionCapabilitySlice_ShouldRemainOwnedByTargetSpine
- LoadAsync_ForDistribution_ShouldPreserveProgramKindAndDataLineage
- DistributionRenderPlanBuilder_ShouldPreserveCapabilityContractAndDeliveryMetadata
- RenderAsync_ShouldPassDistributionCapabilityContract_ToRenderingContract
- DistributionRenderPlanBuilder_ShouldUseRuntimeCapabilityContract
- DistributionCapabilityContract_ShouldRejectProgramKindDrift
- WeekdayTrendRenderPlanBuilder_ShouldUseRuntimeCapabilityContract
- WeekdayTrendCapabilityContract_ShouldRejectProgramKindDrift
- OnChartTypeToggleRequested_ShouldToggleMode_AndRenderLastContext verifies WeekdayTrend capability/delivery metadata on the live adapter request
- RenderAsync_ShouldForwardTransformCapabilityContract
- TransformCapabilityContract_ShouldRejectProgramKindDrift
- TransformChartRenderInvoker_ShouldUseAdapterPath_AndCaptureTransformDiagnostics verifies Transform DerivedSeries metadata

Validation:
- Phase 14 Distribution capability validation passed 197 focused Distribution/VNext/architecture tests after the reopened implementation correction
- Phase 14 Distribution/WeekdayTrend/Transform capability validation passed 227 focused contract, adapter, render-plan, and orchestration tests after completing the remaining implementation slices

Implementation result:
- Phase 14 was reopened because the first closure was proof-only and did not satisfy the agreed implementation-slice intent
- production code now carries DistributionCapabilityContract through DistributionChartRenderRequest on the live Distribution controller/rendering path
- production code now carries WeekdayTrendCapabilityContract through WeekdayTrendChartRenderRequest on the live WeekdayTrend controller/rendering path
- production code now carries TransformCapabilityContract through TransformChartRenderRequest and TransformChartRenderInvoker on the live Transform controller/rendering path
- ChartRenderPlanVocabularyMetadata now accepts explicit program/capability/delivery contracts so render-plan metadata can be built from the runtime contract rather than reconstructed constants
- Distribution, WeekdayTrend, and Transform are proven as active capability slices through the target spine
- UI/controller code relays Distribution behavior without owning capability semantics
- UI/controller code relays WeekdayTrend and Transform behavior without owning capability semantics
- transformation reversibility is not applicable to this single-series Distribution slice
- Transform operation traceability is carried through SeriesOperationRequest where the current UI operation can be mapped
- confidence remains annotation-only; no Distribution policy consumes confidence
- confidence remains annotation-only; no WeekdayTrend or Transform policy consumes confidence
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

- [x] Select one non-chart or chart-independent consumer path.
- [x] Use the same contract/boundary/qualification model.
- [x] Use a consumer-neutral surface or equivalent output shape.
- [x] Avoid render-specific assumptions.
- [x] Preserve semantics/provenance/evidence.
- [x] Add tests proving consumer-general behavior.

Completion condition:

```text
At least one non-chart consumer uses the same target seam without chart/render assumptions.
```

Phase 15 evidence:

```text
Selected non-chart consumer path:
- evidence export

Target-spine proof:
- intent/program entry: AnalyticalIntent with ConsumerDeliveryContract.Export
- capability mapping: CapabilityRequest.FromProgramRequest still supplies capability/composition semantics for export consumers
- contract/boundary: ConsumerProviderRegistry resolves ConsumerProviderContracts.EvidenceExport for ConsumerKind.Export without requiring a render plan
- consumer-neutral surface: ConsumerDeliveryEvidence captures program, source, intent, capability, delivery, provenance, and provider metadata without ChartRenderPlan dependency
- delivery: AnalyticalRenderPlanPipeline.ExecuteForConsumerAsync executes non-rendering consumers and returns AnalyticalConsumerDeliveryResult
- evidence export: EvidenceExportConsumerContractBuilder emits ExportConsumers diagnostics from the same contract/provider seam

Tests added:
- ExecuteForConsumerAsync_ShouldReturnExportDeliveryEvidenceWithoutRenderPlan
- ExecuteForConsumerAsync_ShouldRejectRenderingConsumer
- ExportAsync_ShouldIncludeNonChartExportConsumerEvidence

Validation:
- Phase 15 focused VNext pipeline and evidence export validation passed 34 tests

Implementation result:
- non-chart export consumer evidence is now represented as a first-class neutral output shape
- evidence export can report export consumer contract/provenance/provider metadata without depending on render-plan construction
- render-plan build paths still reject non-rendering consumers, and non-rendering consumer execution rejects rendering consumers
- no UI rendering behavior was changed

Syncfusion alignment note:
- SyncfusionSunburst participates in export consumer evidence as ConsumerKind.Export / DeliveryTarget EvidenceExport / ProviderKey EvidenceExport
- SyncfusionSunburst remains a hierarchy render delivery as ConsumerKind.HierarchyChart / DeliveryTarget SyncfusionSunburst / ProviderKey SyncfusionSunburst
- the distinction is intentionally preserved so Phase 17 consolidation does not flatten hierarchy-chart delivery into the LiveCharts chart family shape
```

---

## 1.17 Phase 16 — Retire Legacy Bypasses Selectively

Goal:

```text
Remove old bypasses only after target replacements are proven.
```

Tasks:

- [x] List known legacy bypass paths.
- [x] For each path, identify target replacement.
- [x] Confirm parity evidence.
- [x] Confirm smoke evidence.
- [x] Confirm metadata preservation.
- [x] Confirm semantic/provenance preservation.
- [x] Confirm no consumer still depends on the bypass.
- [x] Remove one bypass at a time.

Completion condition:

```text
Legacy coexistence shrinks without behavior loss, metadata loss, or semantic drift.
```

Phase 16 evidence:

```text
Bypass classification:
- retired: ChartRenderPlanVocabularyMetadata overloads that accepted only ChartProgramKind and reconstructed program/capability/delivery metadata internally
- retired: ChartRenderPlanProviderMetadata overload that accepted delivery plus a separate ChartProgramKind, allowing provider metadata to drift from the delivery contract
- checked: SyncfusionSunburst-specific render metadata now uses explicit program/capability/delivery contracts and has no remaining direct provider-resolution bypass in the UI adapter path
- contained: VNext fallback runtime paths remain preserved because they carry runtime/evidence state and still provide operational flexibility
- contained: ChartUpdateCoordinator fallback construction remains preserved for callers not yet carrying explicit contracts, but explicit program/capability/delivery inputs are now available and used by hardened slices
- preserved: legacy computation/parity strategies remain preserved because parity, diagnostics, and manual smoke evidence still rely on them as comparison and fallback paths

Target replacement for retired bypass:
- callers now pass ChartProgramRequest, CapabilityRequest, and ConsumerDeliveryContract into ChartRenderPlanVocabularyMetadata
- BarPie and Syncfusion render-plan builders now construct explicit vocabulary contracts before adding render-plan metadata
- provider metadata now derives its default plan-kind resolution from ConsumerDeliveryContract.ProgramKind instead of accepting a separate program kind
- tests now build explicit program/capability/delivery contracts instead of depending on kind-only metadata inference

Evidence checks:
- parity evidence remains preserved by existing parity export and strategy parity tests
- smoke evidence remains preserved by the completed Phase 14 manual exports for chart slices and Phase 15 export evidence path
- metadata preservation is covered by render-plan vocabulary diagnostics tests and architecture guardrails
- semantic/provenance preservation is covered by explicit contract metadata tests
- no production caller remains dependent on the removed kind-only vocabulary metadata overload

Validation:
- Phase 16 focused metadata/export/rendering/architecture validation passed 176 tests
- Phase 16 second-pass provider metadata/render-plan/architecture validation passed 196 tests
- Syncfusion Phase 15/16 alignment validation passed 186 focused Syncfusion/export/provider/architecture tests

Implementation result:
- duplicate semantic inference bypasses were removed from vocabulary metadata and provider metadata
- flexibility-preserving legacy/fallback paths were not removed
- the target seam is stricter: render metadata now requires explicit program/capability/delivery contracts
- provider metadata can no longer drift by receiving a delivery contract and conflicting program kind
- Syncfusion is explicitly verified before Phase 17 as both a hierarchy render consumer and a non-chart export evidence consumer
```

---

## 1.18 Phase 17 — Consolidate Repeated Family Patterns Last

Goal:

```text
Collapse repeated request/route/qualification/adapter patterns only after multiple slices prove a shared shape.
```

Tasks:

- [x] Compare at least two hardened family slices.
- [x] Identify repeated safe structure.
- [x] Identify real family-specific differences.
- [x] Extract only genuinely shared patterns.
- [x] Preserve explicit differences.
- [x] Preserve behavior.
- [x] Preserve tests.
- [x] Confirm consolidation strengthens the target seam.

Completion condition:

```text
Consolidation strengthens the generalized architecture instead of hiding real differences.
```

Phase 17 evidence:

```text
Slices compared:
- Distribution (DistributionCapabilityContract, DistributionRenderPlanBuilder) — Phase 14 hardened
- WeekdayTrend (WeekdayTrendCapabilityContract, WeekdayTrendRenderPlanBuilder) — Phase 14 hardened
- Transform (TransformCapabilityContract) — Phase 14 hardened
- BarPie (previously inline program/capability/delivery creation) — Phase 17 target
- SyncfusionSunburst (previously inline HierarchyChart creation) — preserved distinct

Repeated safe structure identified:
- Sealed record CapabilityContract with ProgramRequest, Capability, Delivery
- Constructor validates programRequest.Kind == expected family kind
- Constructor validates delivery.ProgramKind == programRequest.Kind
- Static Create() factory using ChartProgramRequest.Xxx() / CapabilityRequest.FromProgramRequest / ConsumerDeliveryContract.Chart
- Optional CapabilityContract field on render request (null fallback to Create())
- RenderPlanBuilder uses capabilityContract.ProgramRequest / .Capability / .Delivery instead of inline creation

Real family-specific differences preserved:
- SyncfusionSunburst uses ConsumerDeliveryContract.HierarchyChart (not Chart); builds hierarchy node trees (not flat series);
  the Phase 15/16 alignment distinction is explicitly preserved — no SyncfusionSunburstCapabilityContract added
- BarPie's builder takes an extra rendererKind parameter for backend key resolution; this remains a delivery-layer concern
  separate from the capability contract (which covers program/capability/delivery only)
- Transform's Create() takes title and operations parameters; remains as-is

Consolidation applied:
- BarPieCapabilityContract added to BarPieRenderingTypes.cs (same sealed record pattern as Distribution/WeekdayTrend/Transform)
- BarPieChartRenderRequest gains optional CapabilityContract field
- BarPieRenderPlanBuilder.Build updated to use capabilityContract when provided, fallback to Create()
- BarPieChartControllerAdapter.RenderBarPieChartAsync now passes BarPieCapabilityContract.Create() on the render request

Tests added:
- BarPieRenderPlanBuilder_ShouldUseRuntimeCapabilityContract
- BarPieCapabilityContract_ShouldRejectProgramKindDrift

Validation:
- 950 tests pass (up from 948; 2 new tests added)
- all existing BarPie tests continue to pass without changes

Implementation result:
- BarPie now carries an explicit capability contract through the render request, matching the Distribution/WeekdayTrend/Transform pattern
- the last inline-creation outlier among the LiveCharts chart families has been resolved
- SyncfusionSunburst hierarchy delivery distinction is explicitly preserved and not collapsed
- no production behavior changed; fallback to Create() ensures backward compatibility for any callers not yet providing the contract
```

---

## 1.19 Phase 18 — Complete CapabilityContract Carriage Across Remaining Families

Goal:

```text
Align SyncfusionSunburst and MainChartControllerAdapter with the Phase 14/17 contract-carriage pattern
so no chart family or top-level adapter remains as an inline-creation outlier.
```

Primary targets:

```text
SyncfusionSunburstChartControllerAdapter
SyncfusionSunburst render plan builder
MainChartControllerAdapter
```

Tasks:

- [x] Compare SyncfusionSunburst against the hardened LiveCharts family pattern.
- [x] Add SyncfusionSunburstCapabilityContract using ConsumerDeliveryContract.HierarchyChart — not Chart.
- [x] Thread the contract through the SyncfusionSunburst render request and render plan builder.
- [x] Update SyncfusionSunburstChartControllerAdapter to pass the contract on the live render request.
- [x] Inspect MainChartControllerAdapter for capability or delivery decisions that should be contract-bound.
- [x] Thread contract carriage through MainChartControllerAdapter where applicable. (resolved in Phase 19 through the ChartRenderingOrchestrator hub path)
- [x] Add tests proving contract carriage and program kind drift rejection for each new contract.
- [x] Explicitly preserve the HierarchyChart delivery distinction — do not flatten it to the LiveCharts Chart family shape.

Completion condition:

```text
All chart families carry explicit CapabilityContracts through their live render paths.
No family or top-level adapter remains as an inline-creation outlier.
SyncfusionSunburst hierarchy delivery distinction is preserved in the contract.
```

Phase 18 evidence:

```text
Slices compared:
- Distribution, WeekdayTrend, Transform, BarPie — Phase 14/17 hardened, all carrying explicit CapabilityContracts
- SyncfusionSunburst — Phase 18 target; previously inline-creating ChartProgramRequest/CapabilityRequest/ConsumerDeliveryContract in the builder
- MainChartControllerAdapter — inspected in Phase 18; at that point it routed through CartesianMetricChartRenderInvoker into ChartRenderingOrchestrator and needed Phase 19 hub restructuring before clean contract carriage could be completed

Consolidation applied:
- SyncfusionSunburstCapabilityContract added to SyncfusionSunburstRenderingTypes.cs (sealed record pattern matching all other families)
- Constructor validates programRequest.Kind == ChartProgramKind.SyncfusionSunburst
- Constructor validates delivery.ProgramKind == programRequest.Kind
- Static Create() uses ConsumerDeliveryContract.HierarchyChart (not Chart) — hierarchy delivery distinction explicitly preserved
- SyncfusionSunburstChartRenderRequest gains optional CapabilityContract field
- SyncfusionSunburstRenderPlanBuilder.Build updated to use capabilityContract when provided, fallback to Create()
- SyncfusionSunburstChartControllerAdapter.RenderSunburstAsync now passes SyncfusionSunburstCapabilityContract.Create() on the render request

Real differences preserved:
- ConsumerDeliveryContract.HierarchyChart used throughout — not collapsed to Chart
- Hierarchy node tree building unchanged
- MainChartControllerAdapter was deferred during Phase 18 because it was a hub-owned path, then resolved in Phase 19 by threading CartesianMetricCapabilityContract through MainChartControllerAdapter -> CartesianMetricChartRenderInvoker -> ChartRenderingOrchestrator -> MainChartRenderRequest -> MainChartOrchestrationPipeline

Tests added:
- SyncfusionSunburstRenderPlanBuilder_ShouldUseRuntimeCapabilityContract
- SyncfusionSunburstCapabilityContract_ShouldRejectProgramKindDrift

Validation:
- 952 tests passed at Phase 18 closure for the SyncfusionSunburst slice
- MainChartControllerAdapter contract carriage was completed by Phase 19; current validation is 995 DataVisualiser tests and 15 DataFileReader tests passing
- all existing SyncfusionSunburst tests continue to pass without changes
```

---

## 1.20 Phase 19 — Migrate Hub Responsibilities to the Target Spine

Goal:

```text
Move capability, provider, and delivery authority out of integration hubs into the correct spine layers.
```

Primary targets:

```text
MetricLoadCoordinator
ChartUpdateCoordinator
ChartRenderingOrchestrator
ChartControllerFactory
ChartControllerFactoryContext
```

Tasks:

- [x] Inspect ChartControllerFactory and ChartControllerFactoryContext; confirm composition-only with no non-coordination responsibilities.
- [x] Inspect ChartRenderingOrchestrator; confirm routing-only with no render-plan construction or capability authority.
- [x] Inspect ChartUpdateCoordinator; identify BuildChartRenderPlan and all private render-plan construction helpers as non-coordination render-plan authority.
- [x] Extract BuildChartRenderPlan, BuildRenderPlanMetadata, BuildRenderPlanSignature, BuildChartSeriesPrograms, BuildOverlayRenderPlanSeries, ResolveRenderPlanTimeline, ResolveDisplayMode, and AddMetadata from ChartUpdateCoordinator into CartesianMetricRenderPlanBuilder.
- [x] Update ChartUpdateCoordinator to call CartesianMetricRenderPlanBuilder.Build; confirm coordinator is coordination-only for this responsibility.
- [x] Remove dead ShouldUseStackedTotals from ChartUpdateCoordinator.
- [x] Update RenderPlanBuilders_ShouldAttachVocabularyMetadata guardrail to point to CartesianMetricRenderPlanBuilder.
- [x] Inspect MetricLoadCoordinator; confirm VNext/legacy route selection is a transitional bridge without a proven replacement path.
- [x] Thread CartesianMetricCapabilityContract through CartesianMetricChartRenderRequest, CartesianMetricChartRenderInvoker, and the Main/Secondary invocation stages once the builder seam is stable.
- [x] Migrate MetricLoadCoordinator VNext/legacy routing to the target spine once the replacement path is proven.

Completion condition:

```text
Each targeted hub coordinates only.
No hub owns capability, provider, delivery, or evidence authority.
```

Phase 19 evidence:

```text
Hubs inspected:
- ChartControllerFactory / ChartControllerFactoryContext — confirmed composition-only per Phase 6 audit; no non-coordination responsibilities found; no production changes required
- ChartRenderingOrchestrator — confirmed routing-only; delegates to family pipelines; no render-plan construction authority; no production changes required
- MetricLoadCoordinator — inspected; VNext/legacy route selection was a transitional bridge, and the proven replacement is now extracted into VNextMetricLoadRouter so MetricLoadCoordinator remains a load coordinator rather than the owner of VNext route policy and family runtime recording
- ChartUpdateCoordinator — identified BuildChartRenderPlan and all private render-plan construction helpers as non-coordination render-plan authority

Non-coordination responsibility migrated:
- BuildChartRenderPlan, BuildRenderPlanMetadata, BuildRenderPlanSignature, BuildChartSeriesPrograms, BuildOverlayRenderPlanSeries, ResolveRenderPlanTimeline, ResolveDisplayMode, AddMetadata extracted from ChartUpdateCoordinator into CartesianMetricRenderPlanBuilder (new static class in DataVisualiser/Core/Rendering/CartesianMetrics/)
- Dead ShouldUseStackedTotals method removed from ChartUpdateCoordinator (unreferenced)
- ChartUpdateCoordinator now calls CartesianMetricRenderPlanBuilder.Build(...) — coordinator is coordination-only for this responsibility
- RenderPlanBuilders_ShouldAttachVocabularyMetadata guardrail updated to point to CartesianMetricRenderPlanBuilder instead of ChartUpdateCoordinator

CartesianMetricCapabilityContract threaded (2026-04-29):
- CartesianMetricCapabilityContract sealed record added to CartesianMetricChartRenderingContract.cs; validates kind ∈ {Main, Normalized, Difference, Ratio}; static Create(kind) factory; constructor rejects delivery kind mismatch
- CartesianMetricChartRenderRequest gains optional CapabilityContract field
- CartesianMetricChartRenderInvoker.RenderMainAndCaptureContextAsync threads contract to ChartRenderingOrchestrator.RenderPrimaryChartAsync
- ChartRenderingOrchestrator.RenderPrimaryChartAsync gains optional capabilityContract param; passes to MainChartRenderRequest
- MainChartRenderRequest gains optional CapabilityContract field
- MainChartOrchestrationPipeline passes request.CapabilityContract to IMainChartRenderInvocationStage.RenderAsync
- IMainChartRenderInvocationStage.RenderAsync gains optional CartesianMetricCapabilityContract? param
- MainChartRenderInvocationStage passes contract.Delivery as renderDelivery to UpdateChartUsingStrategyAsync (display mode remains dynamically resolved; program request and capability intentionally not passed to preserve stacked/cumulative mode correctness)
- SecondaryMetricChartRenderInvocationStage creates CartesianMetricCapabilityContract.Create(programKind) inline; passes contract.Delivery as renderDelivery
- MainChartControllerAdapter.RenderMainChartAsync passes CartesianMetricCapabilityContract.Create(ChartProgramKind.Main) on the render request
- 6 new contract tests added (Create delivery targets for all 4 kinds; invalid kind rejection; delivery kind mismatch rejection); initial contract-threading validation passed 958 tests

MetricLoadCoordinator routing responsibility migrated:
- VNextMetricLoadRouter now owns VNext main-family route eligibility, visible chart-family request planning, VNext load invocation, VNext timing, LastLoadRuntime updates, family runtime recording, and fallback-required signaling
- MetricLoadCoordinator now delegates VNext routing through VNextMetricLoadRouter.TryLoadAsync and continues to own load sequencing, validation, legacy fallback invocation, callbacks, and UI loading-state cleanup
- MetricLoadCoordinator_ShouldDelegateVNextRoutingToMetricLoadRouter guardrail verifies the coordinator no longer directly owns VNextChartRoutePolicy, VNextChartProgramRequestPlanner, or RecordVNextFamilyRuntimes

Deferred:
- None for Phase 19. MetricLoadCoordinator VNext/legacy routing has been extracted into VNextMetricLoadRouter.

Validation:
- 995 DataVisualiser tests and 15 DataFileReader tests pass; no regressions
```

---

## 1.21 Phase 20 — Thin the Chart-Family Adapter Layer

Goal:

```text
Replace adapter-owned logic with contract/surface/delivery delegation so adapters relay only.
```

Primary targets:

```text
DistributionChartControllerAdapter
WeekdayTrendChartControllerAdapter
BarPieChartControllerAdapter
TransformDataPanelControllerAdapter
SyncfusionSunburstChartControllerAdapter
MainChartControllerAdapter
```

Pre-implementation audit findings:

```text
BarPie — relay-compliant; BarPieRenderModelBuilder already handles data preparation;
  no logic migration needed; minor DI concern remains bounded outside the adapter-relay behavior slice.

SyncfusionSunburst — adapter owns the full render model pipeline
  (selection deduplication, date range resolution, bucket count determination,
  bucket plan generation, series totals loading); mirrors the BarPie pre-builder state.
  Target: extract into SyncfusionSunburstRenderModelBuilder.

Distribution — adapter owns BuildDistributionRenderInputAsync, ResolveSelectedDistributionSeries,
  and BuildDistributionContext; mode definition application and CMS fallback decisions also inline.
  Target: extract into DistributionRenderInputBuilder.

WeekdayTrend — adapter owns ComputeWeekdayTrend (strategy creation + invocation),
  ResolveWeekdayTrendDataAsync (data loading decision), and ChartDataContext assembly.
  Computation invocation should live in an invoker, not the adapter.
  Target: extract into WeekdayTrendComputationInvoker (pattern: TransformChartRenderInvoker).

MainChartControllerAdapter — adapter owns BuildOverlaySeriesAsync (load → match → smooth →
  construct SeriesResult), ResolveOverlaySelection, ResolveContextSeries, IsMatchingSelection.
  This is the full overlay series lifecycle; not relay.
  Target: extract into CartesianMetricOverlaySeriesBuilder.

TransformDataPanelControllerAdapter — adapter is an inline composition root for 8 coordinators;
  actual render/compute delegation IS thin (coordinators own the logic);
  the problem is construction ownership, not relay logic. This remains a bounded DI/composition
  concern after Phase 21; it is not part of the adapter-relay behavior slice.
```

Tasks:

- [x] Audit all six adapters; classify non-relay responsibilities per adapter.
- [x] BarPie: confirm relay-compliant; no logic migration required; document as complete.
- [x] SyncfusionSunburst: extract full model pipeline (BuildRenderModelFromSelectionsAsync,
      LoadSeriesTotalsAsync, bucket plan generation) into SyncfusionSunburstRenderModelBuilder;
      adapter becomes thin relay calling builder then contract.
- [x] Distribution: extract BuildDistributionRenderInputAsync, ResolveSelectedDistributionSeries,
      BuildDistributionContext, and related helpers into DistributionRenderInputBuilder;
      adapter becomes thin relay.
- [x] WeekdayTrend: extract ComputeWeekdayTrend, ResolveWeekdayTrendDataAsync, and
      ChartDataContext assembly into WeekdayTrendComputationInvoker;
      adapter becomes thin relay.
- [x] CartesianMetric/Main: extract BuildOverlaySeriesAsync, ResolveOverlaySelection,
      ResolveContextSeries, IsMatchingSelection into CartesianMetricOverlaySeriesBuilder;
      adapter becomes thin relay.
- [x] TransformDataPanelControllerAdapter: classify coordinator construction as a bounded DI/composition concern;
      adapter is already a thin relay for render/compute delegation, so no behavior migration is required for Phase 20.
- [x] Add contract tests covering each new builder and invoker.
- [x] Update guardrail tests to assert thinned adapter bodies contain no direct service calls
      or computation invocations (e.g. LoadMetricDataAsync, CreateStrategy);
      adapters may call builders and contracts — that is the correct relay pattern.

Completion condition:

```text
Adapter code relays behavior through contract/surface/delivery seams.
No adapter owns capability, provider, rendering, or semantic decisions.
BarPie is the reference pattern: thin adapter + dedicated builder + contract.
```

Phase 20 evidence:

```text
Pre-implementation audit:
- BarPie — confirmed relay-compliant; BarPieRenderModelBuilder already owns data preparation; no logic migration needed
- SyncfusionSunburst — adapter owned full render model pipeline; extracted into SyncfusionSunburstRenderModelBuilder
- Distribution — adapter owned BuildDistributionRenderInputAsync, ResolveSelectedDistributionSeries, BuildDistributionContext; extracted into DistributionRenderInputBuilder
- WeekdayTrend — adapter owned ComputeWeekdayTrend and ResolveWeekdayTrendDataAsync; extracted into WeekdayTrendComputationInvoker
- MainChartControllerAdapter — adapter owned BuildOverlaySeriesAsync and overlay lifecycle; extracted into CartesianMetricOverlaySeriesBuilder
- TransformDataPanelControllerAdapter — adapter is inline composition root for coordinators; relay delegation already thin; construction refactor remains a bounded DI/composition concern after Phase 21, not an adapter-owned semantic/rendering behavior concern

Extracted builders and invokers:
- SyncfusionSunburstRenderModelBuilder (DataVisualiser/UI/Charts/Presentation/)
- CartesianMetricOverlaySeriesBuilder (DataVisualiser/UI/Charts/Presentation/)
- DistributionRenderInputBuilder (DataVisualiser/UI/Charts/Presentation/)
- WeekdayTrendComputationInvoker (DataVisualiser/UI/Charts/Presentation/)

Tests:
- Phase20BuilderInvokerTests: 16 builder/invoker unit tests
- ArchitectureGuardrailTests: 4 new guardrails (SyncfusionSunburstAdapter_ShouldDelegateModelBuildingToBuilder, DistributionAdapter_ShouldDelegateDataPreparationToBuilder, WeekdayTrendAdapter_ShouldDelegateComputationToInvoker, MainChartAdapter_ShouldDelegateOverlayBuildingToBuilder)

Validation:
- 975 tests passed at Phase 20 closure; current validation is 995 DataVisualiser tests and 15 DataFileReader tests passing
```

---

## 1.22 Phase 21 — Classify and Bound Integration Seams

Goal:

```text
Audit every named "bridge" from the original retirement list; document each one's true role,
its retirement condition, and whether that condition is currently met.
Retire what is genuinely retirable now. Bound what is not.
```

Audit findings (completed 2026-04-29):

```text
VNextMainChartIntegrationCoordinator
  Role: IS the VNext spine for main chart loading. Not a bridge — it is the target.
  Retirement condition: Never retire. This is the authoritative chart-load coordinator.

VNextSeriesLoadCoordinator
  Role: IS the VNext spine for series-level loading. Same conclusion.
  Retirement condition: Never retire. Used by all builder/invoker classes introduced in Phase 20.

VNextDataResolutionHelper
  Role: Active decision helper orchestrating VNext-vs-legacy loading at the series level.
        Contains explicit fallback logic; no dedicated unit test coverage.
  Retirement condition: Retirable once ChartDataContext is no longer the primary UI model
        and legacy fallback loading paths are confirmed dead. Still blocked after Phase 22.

LegacyChartProgramProjector
  Role: Projects VNext ChartProgram output back to legacy ChartDataContext for UI consumption.
        Cannot be removed while ChartDataContext is the primary UI model.
  Retirement condition: Retirable once UI consumes VNext contracts natively (Phase 22+).

LegacyMetricViewGateway
  Role: Wraps legacy IMetricSeriesLoader for use by the VNext reasoning engine.
        Thin adapter; no fallback code; minimal risk.
  Retirement condition: Retirable once IMetricSeriesLoader is replaced by a VNext-native
        series provider. Blocked on Phase 22+.

ChartUpdateCoordinator — UseRenderPlanAdapter=false branch
  Role: Legacy render path (lines 125–128) that calls _chartRenderEngine.Render() directly,
        bypassing the render plan adapter. All production call sites now pass
        UseRenderPlanAdapter=true after Phases 17–19.
  Retirement condition: MET — verify no call site passes false, then remove the else branch.
        This is the one concrete retirement available now.

Parity/evidence comparison paths
  Role: Active validation harnesses for VNext cut-over.
        ChartComputationParityHarness, StrategyParityValidationService, etc.
  Retirement condition: Retirable after cut-over is complete and VNext is the sole path.
        Blocked on Phase 22+.
```

Root cause note:

```text
Phase 21 was originally written as if Phase 22 were already done. The actual blocking
dependency was never addressed in Phases 1-20: the legacy ChartDataContext model remains
the primary data structure the UI consumes. As long as ChartDataContext flows through the
UI, the format-gap projectors (LegacyChartProgramProjector, VNextMainChartIntegrationCoordinator)
must exist. These are integration seams, not bridges to delete. True retirement requires
the UI to speak VNext contracts natively end-to-end. Phase 22 proves that new capabilities
can enter through the target spine without legacy bridges; it does not by itself retire the
legacy UI model or the compatibility seams around it.
```

Tasks:

- [x] Audit all named bridges; classify each as retirable, bounded, or permanent seam.
- [x] Document each bridge's retirement condition.
- [x] Retire the UseRenderPlanAdapter=false branch in ChartUpdateCoordinator:
      verify no call site passes UseRenderPlanAdapter=false in production, remove the else
      branch, update tests.
- [x] Bound all remaining bridges with explicit named retirement conditions above.

Completion condition:

```text
Every named bridge is classified. The one retirable item (UseRenderPlanAdapter legacy branch)
is removed. All others carry an explicit, named retirement condition tied to Phase 22+.
```

Phase 21 evidence:

```text
Bridge classification results:
- VNextMainChartIntegrationCoordinator — permanent seam (IS the VNext spine); never retire
- VNextSeriesLoadCoordinator — permanent seam (IS the VNext spine); never retire
- VNextDataResolutionHelper — bounded; retirable once ChartDataContext is retired (Phase 22+)
- LegacyChartProgramProjector — bounded; retirable once UI speaks VNext natively (Phase 22+)
- LegacyMetricViewGateway — bounded; retirable once IMetricSeriesLoader replaced (Phase 22+)
- Parity/evidence paths — bounded; retirable after VNext-only cut-over (Phase 22+)
- ChartUpdateCoordinator UseRenderPlanAdapter=false branch — RETIRED (retirement condition met)

Retired:
- UseRenderPlanAdapter property removed from ChartUpdateRequest
- ChartUpdateCoordinator if/else dual-path collapsed to single unconditional adapter path
- UseRenderPlanAdapter=true removed from MainChartRenderInvocationStage, SecondaryMetricChartRenderInvocationStage, and TransformChartRenderInvoker
- ChartUpdateCoordinatorTests updated (3 tests no longer set UseRenderPlanAdapter=true)

Validation:
- 975 tests passed at Phase 21 closure; current validation is 995 DataVisualiser tests and 15 DataFileReader tests passing
```

---

## 1.23 Phase 22 — Prove the Spine End-to-End with a New Capability and Independent Consumers

Goal:

```text
Demonstrate the architecture can grow through its own seams without touching old hubs or legacy bridges.
```

Tasks:

- [x] Define one small new analytical capability not currently in the system.
- [x] Express it through intent/capability/program structures from scratch without modifying existing hubs.
- [x] Deliver it through contract/qualification/surface to at least one chart consumer.
- [x] Deliver it through the same contract/qualification/surface to at least one genuinely independent non-chart consumer.
- [x] Use a non-LiveCharts backend for at least part of the delivery path to prove replaceability in practice.
- [x] Confirm no old hub absorbs the new capability.
- [x] Confirm no legacy bridge is required.
- [x] Add evidence through the full spine from intent to delivery to audit record.
- [x] Confirm all completion criteria in Section 3 are satisfied by the result.

Completion condition:

```text
A new capability enters through the target spine and is consumed by two independent consumers
without touching old hubs or legacy bridges.
All Section 3 completion criteria are satisfied.
```

Phase 22 evidence:

```text
New capability: MovingAverage (rolling mean; not previously in the system)

Spine additions (no old hubs modified):
- ChartProgramKind.MovingAverage added to enum
- AnalyticalCapabilityKind.Smoothing added to enum
- SeriesOperationKind.MovingAverage added to enum
- OperationKernel: rolling mean implementation
- ChartProgramPlanner: MovingAverage case added
- ChartBackendCapabilities.TabularSummary: new backend descriptor (BackendKey = "TabularSummaryChart", Cartesian only, no LiveCharts dependency)
- ConsumerProviderContracts.TabularSummaryChart: new chart consumer/provider contract (supports MovingAverage only)
- ConsumerProviderRegistry: TabularSummaryChart built-in consumer added
- MovingAverageCapabilityContract: sealed record with ProgramRequest/Capability/Delivery; static Create factory; validates kind and delivery alignment

Independent consumers proved:
- Chart consumer: MovingAverageChart with MovingAverageCapabilityContract delivered through the independent TabularSummaryChart provider/backend (no LiveCharts dependency)
- Non-chart consumer: ApiResponse through ConsumerDeliveryContract.Api without render-plan delivery

Tests:
- Phase22MovingAverageEndToEndTests: 14 tests covering full spine from intent to delivery to audit record
- ArchitectureGuardrailTests: 4 new guardrails covering MovingAverage capability seam correctness

Validation:
- 995 DataVisualiser tests and 15 DataFileReader tests pass after Phase 19 closure and runtime-fix realignment; no regressions
- no old hubs touched; no legacy bridges required
```

---

## 1.24 Phase 23 — Bound Composition Roots and Remaining DI Concentration

Goal:

```text
Move construction ownership out of adapters where it creates avoidable concentration,
without changing behavior, semantic ownership, rendering ownership, or capability ownership.
```

Primary targets:

```text
TransformDataPanelControllerAdapter
BarPieChartControllerAdapter / BarPieRenderModelBuilder DI seam, if inspection confirms it is the same concern
Any other chart-family adapter that still constructs focused collaborators inline
```

Context:

```text
Phase 20 deliberately separated adapter relay behavior from construction ownership.
The Phase 18-22 audit confirmed TransformDataPanelControllerAdapter still acts as an
inline composition root for its focused coordinators. This is a bounded DI/composition
cleanup concern, not a failed Phase 20/21 behavior migration.
```

Tasks:

- [ ] Audit remaining chart-family adapters and builders for inline collaborator construction.
- [ ] Classify each inline construction site as acceptable local default, bounded DI concern, or removable composition ownership.
- [ ] Extract TransformDataPanelControllerAdapter coordinator construction into a factory/composition helper.
- [ ] Keep Transform render, compute, selection, milestone, and grid delegation behavior unchanged.
- [ ] Confirm the factory/helper does not own capability, provider, rendering, semantic, or evidence authority.
- [ ] If BarPie has the same DI concern, extract or explicitly bound it with a named retirement condition.
- [ ] Add tests or guardrails proving adapters delegate to factories/helpers and do not regain construction sprawl.
- [ ] Run full DataVisualiser and DataFileReader tests.
- [ ] Update Section 2/3 checklist scope notes if Phase 23 adds new completion evidence.

Completion condition:

```text
Known post-Phase-22 DI/composition concentration is either removed or explicitly bounded.
TransformDataPanelControllerAdapter no longer acts as an inline composition root for its
coordinator graph. Behavior and evidence output remain unchanged.
```

Phase 23 evidence:

```text
Not started.
```

---

# 2. Guardrails

Status note:

```text
The guardrail checklist below is checked for the completed Phase 1-22 migration scope.
Phase 23 has its own open tasks above. Checked items remain ongoing
constraints for future phases; they are not permission to bypass these rules later.
```

## 2.1 Refactoring Guardrails

- [x] Refactoring must reduce sprawl, contradiction, duplication, or exception-driven architecture.
- [x] Refactoring must strengthen the generalized target architecture.
- [x] Refactoring must preserve behavior.
- [x] Refactoring must preserve tests.
- [x] Refactoring must not hide real family-specific differences.
- [x] Refactoring must not centralize UI, rendering, vendor, process, or evidence authority.
- [x] Refactoring must not replace explicit seams with hidden orchestration.
- [x] Refactoring must be auditable through tests, parity, or dependency evidence.

---

## 2.3 Authority Guardrails

- [x] UI must not define canonical meaning.
- [x] Rendering must not define analytical meaning.
- [x] Process must not define semantic truth.
- [x] Evidence must not create live semantic authority.
- [x] Provider code must not become semantic authority.
- [x] Consumer code must not redefine canonical meaning.

---

## 2.3 Provenance / Traceability Guardrails

- [x] Results must preserve source lineage.
- [x] Interpretations must preserve derivation context.
- [x] Transformations must record traceability.
- [x] Projections must not discard provenance silently.
- [x] Delivery must not remove required metadata.
- [x] Evidence must expose lineage where relevant.

---

## 2.4 Fidelity / Reversibility Guardrails

- [x] Transformations must be lossless or explicitly annotated as lossy.
- [x] Derived outputs must preserve recovery path where required.
- [x] Projections must preserve semantic fidelity.
- [x] Delivery adaptation must not alter canonical meaning.
- [x] Any irreversible step must be explicit and justified.

---

## 2.5 Capability Guardrails

- [x] New analytical behavior must enter through capability/program structures.
- [x] Controllers must not become capability owners.
- [x] Renderers must not become capability owners.
- [x] Coordinators must not absorb capability planning.
- [x] Capability output must remain contract-bound.

---

## 2.6 Contract / Boundary Guardrails

- [x] Contracts must be explicit at major handoffs.
- [x] Boundaries must prevent vendor assumptions from moving upstream.
- [x] Boundaries must prevent UI assumptions from moving upstream.
- [x] Boundaries must preserve metadata.
- [x] Boundary bypasses must be identified and retired only after replacement proof.

---

## 2.7 Qualification Guardrails

- [x] Provider compatibility must be qualified.
- [x] Backend compatibility must be qualified.
- [x] Adapter compatibility must be qualified.
- [x] Delivery compatibility must be qualified.
- [x] Qualification must not become hidden selection.
- [x] Selection must be justified when compatibility matters.

---

## 2.8 Consumer / Interaction Guardrails

- [x] Consumers receive meaning; they do not define it.
- [x] Interaction relays behavior; it does not redefine intent.
- [x] Tooltip logic must not own interpretation.
- [x] Timestamp sinks must not own analytical meaning.
- [x] ViewModels must not become semantic authorities.
- [x] Controllers must not own provider policy.

---

## 2.9 Delivery Guardrails

- [x] Delivery remains terminal.
- [x] Vendor concerns remain terminal.
- [x] Backend concerns remain terminal.
- [x] Rendering remains replaceable.
- [x] Host/lifecycle concerns remain downstream.
- [x] Delivery must not mutate canonical semantics.

---

## 2.10 Evidence / Audit Guardrails

- [x] Evidence observes only.
- [x] Diagnostics do not control live routing.
- [x] Parity does not control live execution.
- [x] Reachability does not select providers.
- [x] Validation does not become hidden authority.
- [x] Audit records must be durable and reviewable.

---

## 2.11 Governance Guardrails

- [x] New vocabulary must improve architectural clarity.
- [x] New concepts must align with project goals.
- [x] New abstractions must not centralize UI/render/vendor concerns.
- [x] New capabilities must use the target spine.
- [x] New consumers must use contracts/boundaries.
- [x] New delivery paths must remain replaceable.

---

# 3. Completion Criteria

Status note:

```text
The completion checklist below is checked for the completed Phase 1-22 migration scope.
Phase 23 is the active next open phase and must provide new evidence before these
criteria are extended beyond the Phase 1-22 scope.
```

## 3.1 Structural Completion

- [x] Authority, semantics, provenance, and traceability are explicit upstream concerns.
- [x] Envelopes or equivalent carriers preserve semantic context across major seams.
- [x] Reasoning owns capability, composition, transformation, interpretation, confidence, and overlay.
- [x] Program structures bridge reasoning into downstream contracts.
- [x] Contracts/boundaries/qualification/bindings are the required downstream handoff.
- [x] Qualification governs provider/backend/adapter/delivery compatibility.
- [x] SurfaceModel or equivalent consumer-neutral, metadata-preserving shape exists before terminal delivery.
- [x] Delivery is terminal and replaceable.
- [x] Evidence is observational and auditable.
- [x] Governance constraints exist for future growth.

---

## 3.2 Behavioral Completion

- [x] Existing behavior is preserved.
- [x] Existing tests pass.
- [x] New guardrail tests pass.
- [x] Parity is preserved for migrated paths.
- [x] Smoke tests pass for affected delivery paths.
- [x] Metadata preservation tests pass.
- [x] Invalid provider/backend/adapter combinations are rejected.
- [x] Evidence does not affect live execution.

---

## 3.3 Migration Completion

- [x] Refactoring opportunities have been classified and acted on where safe.
- [x] Sprawl, duplication, contradiction, and exception-driven paths are reduced where proven.
- [x] Major old hubs no longer absorb new target responsibilities.
- [x] New capabilities enter through the target spine.
- [x] At least one non-chart consumer path is proven.
- [x] Legacy bypasses are reduced or explicitly bounded.
- [x] Repeated family patterns are consolidated only where proven safe.
- [x] Current architecture can be described through the target grammar.
- [x] Documentation reflects validated state, not intended future state.

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
| 2026-04-28 | Phase 4 | Classified refactoring opportunities and avoided unproven code movement. | `documents/DataVisualiser_Refactoring_Opportunity_Audit.md`; safe-now code refactors: none; candidates deferred to tests/guardrails or later family-pattern consolidation. | Complete |
| 2026-04-28 | Phase 5 | Audited authority/provenance/fidelity handoffs and added preservation guardrails. | `documents/DataVisualiser_Authority_Provenance_Fidelity_Audit.md`; `AnalyticalIntentContractsTests`; `AnalyticalInterpretationBuilderTests`; targeted VNext provenance/fidelity validation passed 70 tests. | Complete |
| 2026-04-28 | Plan refinement | Added a current migration read summarizing Phases 1-5 and orienting Phase 6 toward containment. | `documents/DataVisualiser_Migration_Plan_and_Guardrails.md`; documentation-only change. | Complete |
| 2026-04-28 | Phase 6 | Classified integration hub responsibilities and added containment guardrails. | `documents/DataVisualiser_Integration_Hub_Containment_Audit.md`; `ArchitectureGuardrailTests`; architecture validation passed 75 tests. | Complete |
| 2026-04-28 | Phase 7 | Audited contract/boundary/qualification seam and added delivery-binding metadata preservation guardrail. | `documents/DataVisualiser_Contract_Boundary_Qualification_Audit.md`; `ConsumerProviderRegistryTests`; VNext seam validation passed 67 tests; architecture validation passed 75 tests. | Complete |
| 2026-04-28 | Phase 8 | Classified projection/translation roles and added non-authority containment guardrails. | `documents/DataVisualiser_Projection_Translation_Containment_Audit.md`; `ArchitectureGuardrailTests`; projector/adapter validation passed 33 tests; architecture validation passed 78 tests. | Complete |
| 2026-04-28 | Phase 9 | Audited consumer/interaction layer and added non-authority guardrails. | `documents/DataVisualiser_Consumer_Interaction_Containment_Audit.md`; `ArchitectureGuardrailTests`; consumer/interaction validation passed 118 tests. | Complete |
| 2026-04-28 | Phase 10 | Audited surface-model seam and added neutrality/lifecycle guardrails. | `documents/DataVisualiser_Surface_Model_Seam_Audit.md`; `ArchitectureGuardrailTests`; surface-model validation passed 121 tests. | Complete |
| 2026-04-28 | Phase 11 | Audited terminal delivery/vendor boundaries and added downstream containment guardrails. | `documents/DataVisualiser_Terminal_Delivery_Boundary_Audit.md`; `ArchitectureGuardrailTests`; terminal-delivery validation passed 142 tests. | Complete |
| 2026-04-28 | Phase 12 | Audited evidence/diagnostics/parity/reachability/validation flows and added observational guardrails. | `documents/DataVisualiser_Evidence_Observability_Audit.md`; `ArchitectureGuardrailTests`; evidence/parity/reachability validation passed 158 tests. | Complete |
| 2026-04-28 | Phase 13 | Defined governance constraints for future vocabulary, concepts, capabilities, transformations, consumers, backends, and evidence paths. | `documents/DataVisualiser_Governance_Constraints_Audit.md`; `ArchitectureGuardrailTests`; architecture validation passed 93 tests. | Complete |
| 2026-04-28 | Phase 14 | Selected active Distribution as the first capability slice and proved it through the target spine. | `documents/DataVisualiser_Distribution_Capability_Slice_Audit.md`; `ArchitectureGuardrailTests`; Distribution/VNext/rendering/parity validation passed 172 tests. | Complete |
| 2026-04-29 | Phase 14 correction | Reopened the Distribution capability slice to add actual production contract carriage through the live controller/rendering path. | `DistributionCapabilityContract`; `DistributionChartRenderRequest`; `ChartRenderPlanVocabularyMetadata`; focused Distribution/VNext/architecture validation passed 197 tests. | Complete |
| 2026-04-29 | Phase 14 completion | Implemented the remaining Phase 14 production contract-carriage slices for WeekdayTrend and Transform, keeping capability ownership in the target spine. | `WeekdayTrendCapabilityContract`; `TransformCapabilityContract`; `WeekdayTrendChartRenderRequest`; `TransformChartRenderRequest`; focused Distribution/WeekdayTrend/Transform/render-plan/orchestration validation passed 227 tests. | Complete |
| 2026-04-29 | Phase 15 | Added a non-chart evidence export consumer path through the same intent/capability/delivery/provider seam. | `ConsumerDeliveryEvidence`; `AnalyticalRenderPlanPipeline.ExecuteForConsumerAsync`; `EvidenceExportConsumerContractBuilder`; focused VNext/evidence export validation passed 34 tests. | Complete |
| 2026-04-29 | Phase 16 | Retired duplicate metadata bypasses while preserving flexible legacy/fallback paths. | Removed kind-only `ChartRenderPlanVocabularyMetadata` overloads and delivery-plus-program-kind `ChartRenderPlanProviderMetadata` overload; BarPie/Syncfusion/tests now pass explicit program/capability/delivery contracts; focused metadata/export/rendering/architecture validation passed 176 tests and provider metadata/render-plan/architecture validation passed 196 tests. | Complete |
| 2026-04-29 | Phase 15/16 Syncfusion alignment | Verified SyncfusionSunburst before Phase 17 as both a hierarchy render consumer and a non-chart export evidence consumer. | `ExportAsync_ShouldIncludeNonChartExportConsumerEvidence`; focused Syncfusion/export/provider/architecture validation passed 186 tests. | Complete |
| 2026-04-29 | Phase 17 | Added BarPieCapabilityContract to complete the explicit capability-contract pattern across all LiveCharts families; preserved SyncfusionSunburst hierarchy delivery distinction. | `BarPieCapabilityContract`; `BarPieChartRenderRequest` optional contract field; `BarPieRenderPlanBuilder` updated; `BarPieChartControllerAdapter` passes contract; `BarPieRenderPlanBuilder_ShouldUseRuntimeCapabilityContract`; `BarPieCapabilityContract_ShouldRejectProgramKindDrift`; 950 tests pass. | Complete |
| 2026-04-29 | Phase 18 | Added SyncfusionSunburstCapabilityContract to complete contract carriage across remaining chart families; preserved HierarchyChart delivery distinction; MainChartControllerAdapter carriage was inspected here and completed through the Phase 19 hub path. | `SyncfusionSunburstCapabilityContract`; `SyncfusionSunburstChartRenderRequest` optional contract field; `SyncfusionSunburstRenderPlanBuilder` updated; `SyncfusionSunburstChartControllerAdapter` passes contract; `SyncfusionSunburstRenderPlanBuilder_ShouldUseRuntimeCapabilityContract`; `SyncfusionSunburstCapabilityContract_ShouldRejectProgramKindDrift`; 952 tests passed at Phase 18 closure. | Complete |
| 2026-04-29 | Phase 19 (builder extraction) | Extracted CartesianMetricRenderPlanBuilder from ChartUpdateCoordinator; render-plan construction authority now lives in a dedicated builder; coordinator is coordination-only for this responsibility; guardrail updated. | `CartesianMetricRenderPlanBuilder`; `ChartUpdateCoordinator` calls builder; dead `ShouldUseStackedTotals` removed; `RenderPlanBuilders_ShouldAttachVocabularyMetadata` guardrail updated to new builder path; 952 tests pass. | Complete |
| 2026-04-29 | Phase 19 (contract threading) | Threaded CartesianMetricCapabilityContract through the full Main and Secondary invocation chains; delivery authority is now explicit at every render boundary. | `CartesianMetricCapabilityContract`; `CartesianMetricChartRenderRequest`; `CartesianMetricChartRenderInvoker`; `ChartRenderingOrchestrator.RenderPrimaryChartAsync`; `MainChartRenderRequest`; `MainChartOrchestrationPipeline`; `IMainChartRenderInvocationStage`; `MainChartRenderInvocationStage`; `SecondaryMetricChartRenderInvocationStage`; `MainChartControllerAdapter`; 6 new contract tests; 958 tests pass. | Complete |
| 2026-04-30 | Phase 19 (routing closure) | Closed the remaining MetricLoadCoordinator routing item by extracting VNext main-family routing into VNextMetricLoadRouter. | `VNextMetricLoadRouter`; `MetricLoadCoordinator` delegates through `TryLoadAsync`; `MetricLoadCoordinator_ShouldDelegateVNextRoutingToMetricLoadRouter`; 995 DataVisualiser tests and 15 DataFileReader tests pass. | Complete |
| 2026-04-29 | Phase 20 | Thinned chart-family adapter layer: extracted SyncfusionSunburstRenderModelBuilder, CartesianMetricOverlaySeriesBuilder, DistributionRenderInputBuilder, WeekdayTrendComputationInvoker; adapters are now thin relays; 16 builder/invoker tests + 4 new guardrails; 975 tests pass. | `SyncfusionSunburstRenderModelBuilder`; `CartesianMetricOverlaySeriesBuilder`; `DistributionRenderInputBuilder`; `WeekdayTrendComputationInvoker`; `Phase20BuilderInvokerTests`; updated `ArchitectureGuardrailTests`; 975 tests pass. | Complete |
| 2026-04-29 | Phase 21 | Classified integration seams; retired UseRenderPlanAdapter legacy dual-path; adapter path is now always-on in ChartUpdateCoordinator; three call sites and three tests cleaned up. | `ChartUpdateRequest` (UseRenderPlanAdapter removed); `ChartUpdateCoordinator` (single path); `MainChartRenderInvocationStage`; `SecondaryMetricChartRenderInvocationStage`; `TransformChartRenderInvoker`; `ChartUpdateCoordinatorTests`; 975 tests pass. | Complete |
| 2026-04-29 | Phase 22 | Proved the spine end-to-end with MovingAverage: new capability, new TabularSummary chart backend/provider, and independent API consumer — no old hubs touched, no legacy bridges required; chart + API consumers proved independently. | `ChartProgramKind.MovingAverage`; `AnalyticalCapabilityKind.Smoothing`; `SeriesOperationKind.MovingAverage`; `OperationKernel` rolling mean; `ChartProgramPlanner` case; `ChartBackendCapabilities.TabularSummary`; `ConsumerProviderContracts.TabularSummaryChart`; `MovingAverageCapabilityContract`; `Phase22MovingAverageEndToEndTests` (14 tests); 4 architecture guardrails; 989 tests passed at phase closure; current lane is 995 DataVisualiser tests. | Complete |

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
