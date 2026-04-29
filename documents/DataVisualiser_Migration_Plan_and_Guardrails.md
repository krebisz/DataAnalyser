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
This migration plan is the active sequential execution plan for the Phase 1-17 migration described here.
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

Target-spine proof:
- intent/program entry: AnalyticalIntentFactory.Distribution and ChartProgramRequest.Distribution
- capability mapping: CapabilityRequest.FromProgramRequest maps Distribution to AnalyticalCapabilityKind.Distribution and CompositionKind.SingleSeries
- contract/boundary: ConsumerDeliveryContract / ChartProgramDeliveryTargetResolver provides DistributionChart delivery
- qualification: DistributionRenderingContract exposes qualified Cartesian route and tactical polar fallback route
- surface/delivery: DistributionChartControllerAdapter attaches DistributionCapabilityContract to the live render request, and DistributionRenderPlanBuilder emits ChartRenderPlan metadata from that runtime capability/program/delivery contract
- evidence: VNextDataResolutionHelper records EvidenceRuntimePath.VNextDistribution and render-plan diagnostics capture Distribution metadata

Tests added:
- DistributionCapabilitySlice_ShouldRemainOwnedByTargetSpine
- LoadAsync_ForDistribution_ShouldPreserveProgramKindAndDataLineage
- DistributionRenderPlanBuilder_ShouldPreserveCapabilityContractAndDeliveryMetadata
- RenderAsync_ShouldPassDistributionCapabilityContract_ToRenderingContract
- DistributionRenderPlanBuilder_ShouldUseRuntimeCapabilityContract
- DistributionCapabilityContract_ShouldRejectProgramKindDrift

Validation:
- Phase 14 Distribution capability validation passed 197 focused Distribution/VNext/architecture tests after the reopened implementation correction

Implementation result:
- Phase 14 was reopened because the first closure was proof-only and did not satisfy the agreed implementation-slice intent
- production code now carries DistributionCapabilityContract through DistributionChartRenderRequest on the live Distribution controller/rendering path
- ChartRenderPlanVocabularyMetadata now accepts explicit program/capability/delivery contracts so render-plan metadata can be built from the runtime contract rather than reconstructed constants
- Distribution is proven as the first active capability slice through the target spine
- UI/controller code relays Distribution behavior without owning capability semantics
- transformation reversibility is not applicable to this single-series Distribution slice
- confidence remains annotation-only; no Distribution policy consumes confidence
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
