# DataVisualiser Migration Plan and Guardrails — Compact Continuation Version

## Purpose

Implementation-facing companion to the rebuilt architectural vocabulary and target architecture document.

Use this document for:

```text
AI agent execution in an IDE
human review
sequential progress tracking
auditable completion
```

Refactoring is permitted only when it reduces architectural sprawl, contradiction, duplication, or exception-driven structure without violating behavior, tests, or authority boundaries.

Do not treat this as permission for broad, cosmetic, speculative, or folder/class reshuffling refactors.

---

## Source Context

Target architecture is defined around:

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
migration proceeds by stabilization, seam hardening, consumption migration, convergence, then productization
```

Current post-synopsis read after Phases 1-23:

```text
Phases 1-23 proved and hardened the structural spine.
The target architecture is now partially implemented, not merely described.
Capability, contract, render-plan, metadata, delivery-binding, and evidence seams are materially stronger.
MovingAverage and TabularSummary prove that new capability and independent consumers can grow through the spine.
Legacy bridges are classified and bounded, but not all are retired.
The primary remaining convergence blocker is ChartDataContext as the dominant UI consumption model.
LegacyChartProgramProjector, VNextDataResolutionHelper, LegacyMetricViewGateway, and parity/evidence bridge paths remain blocked by that UI consumption model.
The next migration track is consumption migration and convergence: move production UI consumption onto VNext-native contracts, reduce ChartDataContext dependency, retire bridge paths selectively, and consolidate only after repeated family slices prove the same shape.
```

Migration continuation rule:

```text
Phases 1-23 are completed structural spine migration.
Phases 24-35 are consumption migration and convergence.
Phases 36+ are post-convergence formalisation and bounded-generativity alignment.
Do not treat Phase 23 as full architectural completion.
Do not add new capability as the next priority unless it directly supports consumption migration.
```

Scaffold audit consolidation rule:

```text
Early scaffold audit files from Phases 3-14 are historical evidence, not active working documents for Phase 24+.
Their reusable constraints are carried forward in this compact plan where they support consumption migration and convergence.
Do not require future agents to read the individual scaffold audits by default.
Archive the individual scaffold audit files after accepting this consolidated carry-forward state.
Regenerate structural artifacts for Phase 24+ instead of relying on Phase 3 density numbers.
```

Key carry-forward implications:

```text
ChartDataContext remains the primary consumption-model blocker.
ChartRenderPlan remains the strongest current consumer-neutral surface candidate.
LegacyChartProgramProjector, VNextDataResolutionHelper, LegacyMetricViewGateway, and parity/evidence bridge paths remain transitional or validation-adjacent until replacement, parity, smoke, metadata, and provenance evidence exists.
Evidence observes; it must not route live behavior.
UI, adapters, renderers, evidence, and terminal delivery must not own analytical authority.
```

Prime Directive alignment:

```text
The migration must preserve coherence while enabling bounded generativity.
```

Formal concern to carry forward:

```text
R = requirement space
L = formal architectural language / grammar
P = implementation plan
C = implemented construction set
E = documented architectural expressions

Required future pressure:
- improve R -> L coverage
- reduce harmful many-to-one language collapse
- identify missing formal language where requirements cannot be expressed without semantic loss
- evolve from architectural grammar toward construction algebra
```

Interpretation:

```text
Phases 24-35 remain convergence work.
Phases 36+ become post-convergence formalisation and bounded-generativity alignment.
Do not force post-convergence algebra into the active convergence phases.
Do not treat Phase 26 Operation Chain as full algebra implementation; treat it as the first bounded pressure test.
```

---

# 1. Execution Rules

```text
Work top-to-bottom.
Complete one phase before starting the next.
Prefer narrow, test-backed changes.
Preserve behavior.
Preserve tests.
Refactor only when it reduces sprawl, duplication, contradiction, or exception-driven structure.
Prefer generalized seams only after shared shape is proven.
Do not rename or move code cosmetically.
Do not retire ChartDataContext-related bridges until replacement, parity, smoke, metadata, and provenance evidence exist.
Update progress only when tests, generated evidence, or code inspection support completion.
```

---

# 2. Completed Structural Spine Migration Summary — Phases 1-23

This section intentionally compresses completed phase detail. Historical specifics remain in the referenced audit notes, tests, generated artifacts, and progress log.

## Phase 1 — Establish Current Baseline

```text
Status: Complete
Core outcome: Baseline artifacts, counts, density, high-risk hubs, high-density carriers, and legacy/VNext coexistence paths were captured before migration work proceeded.
```

## Phase 2 — Fix Generated Governance References

```text
Status: Complete
Core outcome: dependency-summary generation was corrected so generated governance output references the architectural vocabulary alongside existing boundary authorities.
```

## Phase 3 — Classify Dependency Density

```text
Status: Complete
Core outcome: Dense dependencies were classified before refactoring; no actual drift was confirmed from density evidence alone.
```

## Phase 4 — Identify Refactoring Opportunities

```text
Status: Complete
Core outcome: Refactoring candidates were classified; no speculative or cosmetic code movement was allowed without test-backed justification.
```

## Phase 5 — Lock Authority / Semantics / Provenance / Fidelity

```text
Status: Complete
Core outcome: Provenance, metadata, confidence immutability, and semantic-fidelity guardrails were established across key VNext handoffs.
```

## Phase 6 — Cap Integration Hubs

```text
Status: Complete
Core outcome: Old integration hubs were bounded so they may coordinate but must not absorb authority, capability, provider, evidence, or delivery ownership.
```

## Phase 7 — Harden Contract / Boundary / Qualification Seam

```text
Status: Complete
Core outcome: Contract, boundary, qualification, binding, and metadata-preservation seams were hardened as the required downstream handoff.
```

## Phase 8 — Preserve Projection as Non-Authoritative Translation

```text
Status: Complete
Core outcome: Projectors, adapters, dispatchers, resolvers, and related translation roles were guarded against owning authority, policy, evidence, confidence, or interpretation.
```

## Phase 9 — Thin Consumer / Interaction Layer

```text
Status: Complete
Core outcome: Controllers, adapters, events, tooltips, timestamp sinks, ViewModel/state helpers, and interaction flows were bounded as non-authoritative relays.
```

## Phase 10 — Elevate SurfaceModel Seam

```text
Status: Complete
Core outcome: ChartRenderPlan was confirmed as the consumer-neutral, metadata-preserving surface seam before terminal delivery.
```

## Phase 11 — Demote Terminal Delivery

```text
Status: Complete
Core outcome: Rendering, backend, vendor, host, and lifecycle code were constrained as downstream, replaceable, semantically non-authoritative delivery concerns.
```

## Phase 12 — Preserve Evidence as Observational

```text
Status: Complete
Core outcome: Evidence, diagnostics, parity, reachability, validation, and audit paths were guarded so they observe and prove rather than control live behavior.
```

## Phase 13 — Add Governance Constraints

```text
Status: Complete
Core outcome: Future vocabulary, concepts, capabilities, transformations, consumers, backends, and evidence paths were tied to governed architectural growth rules.
```

## Phase 14 — Resume Capability Expansion

```text
Status: Complete
Core outcome: Distribution, WeekdayTrend, and Transform were carried through the target spine with explicit capability contracts and runtime metadata preservation.
```

## Phase 15 — Validate Non-Chart Consumer Path

```text
Status: Complete
Core outcome: Evidence export proved a non-chart consumer can use the same intent/capability/delivery/provider seam without render-plan assumptions.
```

## Phase 16 — Retire Legacy Bypasses Selectively

```text
Status: Complete
Core outcome: Proven metadata inference bypasses were removed; flexible fallback, parity, and runtime bridge paths were preserved where still needed.
```

## Phase 17 — Consolidate Repeated Family Patterns Last

```text
Status: Complete
Core outcome: BarPie adopted the proven explicit capability-contract pattern while preserving real family differences and leaving Syncfusion hierarchy delivery distinct.
```

## Phase 18 — Complete CapabilityContract Carriage Across Remaining Families

```text
Status: Complete
Core outcome: SyncfusionSunburst gained explicit capability-contract carriage while preserving HierarchyChart delivery semantics; MainChart carriage was deferred to the Phase 19 hub path.
```

## Phase 19 — Migrate Hub Responsibilities to the Target Spine

```text
Status: Complete
Core outcome: Render-plan construction and VNext route policy were moved out of hubs into dedicated target-spine builders/routers; hubs remain coordination-focused.
```

## Phase 20 — Thin the Chart-Family Adapter Layer

```text
Status: Complete
Core outcome: Chart-family adapters were thinned by extracting model-building, overlay-building, distribution input, and weekday computation work into focused builders/invokers.
```

## Phase 21 — Classify and Bound Integration Seams

```text
Status: Complete
Core outcome: Named bridges were classified as permanent, bounded, or retirable; the obsolete UseRenderPlanAdapter=false legacy branch was removed.
```

## Phase 22 — Prove Spine End-to-End with New Capability and Independent Consumers

```text
Status: Complete
Core outcome: MovingAverage proved new capability growth through the spine with independent chart and API consumers, no old hub changes, and no legacy bridge requirement.
```

## Phase 23 — Bound Composition Roots and Remaining DI Concentration

```text
Status: Complete
Core outcome: Transform adapter coordinator construction was moved into a composition factory; remaining local DI defaults were classified and bounded.
```

---

# 3. Produced Artifact Index

## Completed Phases 1-23

| Phase | Artifact Type | Files / Evidence |
|---|---|---|
| 1 | Generated baseline | `project-tree.txt`; `codebase-index.md`; `dependency-summary.md`; `type-dependency-diagram.md`; `documents/Type Dependencies Diagram.md` |
| 2 | Generator/script update | `scripts/Generate-DependencySummary.ps1`; regenerated `dependency-summary.md` |
| 3 | Audit note | `documents/DataVisualiser_Dependency_Density_Audit.md` |
| 4 | Audit note | `documents/DataVisualiser_Refactoring_Opportunity_Audit.md` |
| 5 | Audit note / guardrails | `documents/DataVisualiser_Authority_Provenance_Fidelity_Audit.md`; VNext provenance/fidelity tests |
| 6 | Audit note / guardrails | `documents/DataVisualiser_Integration_Hub_Containment_Audit.md`; `ArchitectureGuardrailTests` |
| 7 | Audit note / guardrails | `documents/DataVisualiser_Contract_Boundary_Qualification_Audit.md`; contract/boundary tests |
| 8 | Audit note / guardrails | `documents/DataVisualiser_Projection_Translation_Containment_Audit.md`; projection/adapter guardrails |
| 9 | Audit note / guardrails | `documents/DataVisualiser_Consumer_Interaction_Containment_Audit.md`; consumer/interaction guardrails |
| 10 | Audit note / guardrails | `documents/DataVisualiser_Surface_Model_Seam_Audit.md`; surface-model guardrails |
| 11 | Audit note / guardrails | `documents/DataVisualiser_Terminal_Delivery_Boundary_Audit.md`; terminal-delivery guardrails |
| 12 | Audit note / guardrails | `documents/DataVisualiser_Evidence_Observability_Audit.md`; evidence observability guardrails |
| 13 | Audit note / guardrails | `documents/DataVisualiser_Governance_Constraints_Audit.md`; governance guardrails |
| 14 | Capability-slice audit / implementation | `documents/DataVisualiser_Distribution_Capability_Slice_Audit.md`; Distribution/WeekdayTrend/Transform contract-carriage tests |
| 15 | Non-chart consumer proof | `ConsumerDeliveryEvidence`; `AnalyticalRenderPlanPipeline.ExecuteForConsumerAsync`; evidence export tests |
| 16 | Bypass retirement | explicit metadata contract requirements; provider/vocabulary metadata tests |
| 17 | Family-pattern consolidation | `BarPieCapabilityContract`; BarPie render-plan tests |
| 18 | Remaining family contract carriage | `SyncfusionSunburstCapabilityContract`; Syncfusion hierarchy-contract tests |
| 19 | Hub responsibility migration | `CartesianMetricRenderPlanBuilder`; `CartesianMetricCapabilityContract`; `VNextMetricLoadRouter`; hub guardrails |
| 20 | Adapter thinning | `SyncfusionSunburstRenderModelBuilder`; `CartesianMetricOverlaySeriesBuilder`; `DistributionRenderInputBuilder`; `WeekdayTrendComputationInvoker`; builder/invoker tests |
| 21 | Seam classification / branch retirement | `UseRenderPlanAdapter` retired; bridge classification captured |
| 22 | End-to-end spine proof | MovingAverage; TabularSummary backend/provider; API consumer; end-to-end tests |
| 23 | Composition-root cleanup | `TransformDataPanelControllerAdapterCompositionFactory`; composition guardrail |

## Planned Continuation Phases 24-47

| Phase | Artifact Type | Expected Files |
|---|---|---|
| Phase 24 | Audit note | `documents/DataVisualiser_ChartDataContext_Migration_Audit.md` |
| Phase 25 | Contract specification / guardrails | `documents/DataVisualiser_VNext_Native_UI_Consumption_Contract.md`; UI consumption contract tests |
| Phase 26 | Feature proving slice | `documents/DataVisualiser_Operation_Chain_Workbench_MVP_Audit.md`; operation-chain execution/provenance/evidence tests |
| Phase 27 | First production-family migration audit | `documents/DataVisualiser_First_VNext_Native_Family_Migration_Audit.md`; family migration tests |
| Phase 28 | Bridge retirement note | `documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md`; parity / metadata / UI behavior tests |
| Phase 29 | Multi-family migration tracker | `documents/DataVisualiser_VNext_Native_Family_Migration_Tracker.md`; per-family migration tests |
| Phase 30 | Surface-model convergence audit | `documents/DataVisualiser_Consumer_Neutral_Surface_Model_Convergence_Audit.md`; surface contract tests |
| Phase 31 | UI / interaction / state thinning audit | `documents/DataVisualiser_UI_Interaction_State_Consolidation_Audit.md`; UI guardrail tests |
| Phase 32 | Delivery demotion audit | `documents/DataVisualiser_Rendering_Vendor_Delivery_Demotion_Audit.md`; vendor-boundary tests |
| Phase 33 | Capability / contract consolidation audit | `documents/DataVisualiser_Capability_Contract_Consolidation_Audit.md`; shared-shape tests |
| Phase 34 | Final legacy bypass retirement note | `documents/DataVisualiser_Remaining_Legacy_Bypass_Retirement_Audit.md`; regression / parity tests |
| Phase 35 | Final convergence audit | `documents/DataVisualiser_Final_Convergence_Audit.md`; regenerated baseline artifacts |
| Phase 36 | Formal coverage | Requirements-to-language coverage matrix |
| Phase 37 | Construction algebra | Construction algebra baseline |
| Phase 38 | Operation / capability algebra | Operation, capability, arity, precondition, postcondition, and compatibility artifacts |
| Phase 39 | Relation system | Typed relation model and graph-projection artifacts |
| Phase 40 | Multiplicity / derived dataset model | Input/output/intermediate/derived set artifacts |
| Phase 41 | Evidence sufficiency / promotion rules | Evidence sufficiency, promotion, quarantine, and governance artifacts |
| Phase 42 | Semantic / interpretation model | Assumption, semantic plurality, interpretation, confidence, and explanation artifacts |
| Phase 43 | Analytical fitness | Usefulness, distortion, signal-preservation, and analytical-fitness artifacts |
| Phase 44 | Computational planning | Bounded search, pruning, cost, caching, and execution-planning artifacts |
| Phase 45 | Generative multi-consumer output | Generated chart/table/report/API/export/evidence output artifacts |
| Phase 46 | Governance / emergence review | Emergence review, guardrail scorecard, and agent/governance benchmark artifacts |
| Phase 47 | Scenario hardening | Domain/provider/product scenario hardening artifacts |

## Consolidated Scaffold Audit Handling

The early scaffold audit files are retained as historical evidence only. Their reusable constraints have been carried forward into the active migration phases and guardrails where coherent.

Recommended archive location:

```text
documents/archive/structural-spine-audits-2026-04-28/
```

Files covered by this consolidation:

```text
DataVisualiser_Dependency_Density_Audit.md
DataVisualiser_Refactoring_Opportunity_Audit.md
DataVisualiser_Authority_Provenance_Fidelity_Audit.md
DataVisualiser_Integration_Hub_Containment_Audit.md
DataVisualiser_Contract_Boundary_Qualification_Audit.md
DataVisualiser_Projection_Translation_Containment_Audit.md
DataVisualiser_Consumer_Interaction_Containment_Audit.md
DataVisualiser_Surface_Model_Seam_Audit.md
DataVisualiser_Terminal_Delivery_Boundary_Audit.md
DataVisualiser_Evidence_Observability_Audit.md
DataVisualiser_Governance_Constraints_Audit.md
DataVisualiser_Distribution_Capability_Slice_Audit.md
```

Archive rule:

```text
Keep this compact plan as the active plan.
Keep the consolidated scaffold-audit carry-forward constraints embedded here.
Do not require IDE agents to read archived scaffold audits by default.
Use archived audits only for historical investigation, not active execution routing.
```

---

# 4. Active Migration Plan — Phases 24-35

## 4.1 Phase 24 — Audit ChartDataContext Migration Path

Goal:

```text
Map every site where ChartDataContext carries data between VNext, Core, UI, rendering, evidence, parity, and legacy bridge paths.
```

Context:

```text
ChartDataContext is the primary remaining consumption-model blocker.
LegacyChartProgramProjector, VNextDataResolutionHelper, LegacyMetricViewGateway, and parity/evidence bridge paths cannot be retired cleanly while UI consumption remains ChartDataContext-first.
```

Scaffold audit carry-forward classification detail:

```text
For each ChartDataContext reference, identify whether the use is:
construction
read-only consumption
mutation
UI binding
VNext bridge
legacy compatibility
fallback routing
evidence/runtime recording
parity/evidence comparison
test-only
candidate for VNext-native replacement
```

Tasks:

- [x] Find all `ChartDataContext` references in production code.
- [x] Find all `ChartDataContext` references in tests and parity/evidence paths.
- [x] Classify each reference as:
  - [x] retirable now
  - [x] needs VNext-native equivalent
  - [x] needs consumer contract shape
  - [x] needs surface model shape
  - [x] test/parity/evidence only
  - [x] must remain temporarily
- [x] Identify every `LegacyChartProgramProjector` dependency.
- [x] Identify every `VNextDataResolutionHelper` dependency.
- [x] Identify every `LegacyMetricViewGateway` dependency.
- [x] Identify all parity/evidence paths still dependent on `ChartDataContext`.
- [x] Identify all UI/controller/adapter paths where `ChartDataContext` is still the primary consumed model.
- [x] Document target replacement shape for each dependency class.
- [x] Do not retire anything in this phase unless it is already proven dead and covered by tests.

Completion condition:

```text
A ChartDataContext retirement map exists.
Every ChartDataContext dependency has a classification and target replacement condition.
No bridge retirement proceeds without this map.
```

Planned evidence:

```text
documents/DataVisualiser_ChartDataContext_Migration_Audit.md
fresh reference search output
focused guardrail tests if new static checks are added
```

Phase 24 evidence:

```text
Audit note:
- documents/DataVisualiser_ChartDataContext_Migration_Audit.md

Fresh reference counts:
- production files referencing ChartDataContext: 77
- test files referencing ChartDataContext: 52

Bridge paths classified:
- LegacyChartProgramProjector
- VNextDataResolutionHelper
- LegacyMetricViewGateway
- VNextMetricLoadRouter

Target replacement classes identified:
- VNext-native UI consumption contract
- consumer-neutral surface model
- strategy input contract
- evidence/parity snapshot input

Tests added/updated:
- ChartDataContextMigrationAudit_ShouldGateBridgeRetirement
- CompactMigrationPlan_ShouldCarryForwardStructuralSpineAndConsumptionMigrationRules
- CompactMigrationPlan_ShouldRetainTargetGrammarAndPostConvergenceDeferral
- MigrationPlan_ShouldKeepChartDataContextAuditBeforeUiConsumptionContract
- CompactMigrationPlan_ShouldNotRequireArchivedScaffoldAuditsByDefault

Validation:
- ArchitectureGuardrailTests passed 107 tests

Implementation result:
- no production behavior code changed in Phase 24
- no ChartDataContext dependency was classified as retirable now
- bridge retirement is gated on Phase 25 replacement contract plus parity, smoke, metadata, and provenance evidence
- Phase 25 can start with contract definition, not bridge removal
```

## 4.2 Phase 25 — Define VNext-Native UI Consumption Contract

Goal:

```text
Define what production UI consumes instead of ChartDataContext.
```

Contract requirements:

```text
consumer-facing
metadata-preserving
surface-ready
non-authoritative
delivery-neutral
provenance-preserving
interaction-aware without owning interaction policy
```

Minimum metadata preservation requirement:

```text
program kind
capability kind
composition kind
delivery target
consumer kind
provider key/signature
intent/provenance signature
source/load signature
overlay and interaction metadata where applicable
```

Candidate inputs:

```text
ConsumerDeliveryContract
ConsumerDeliveryEvidence
ChartProgramRequest / Program shape
ChartRenderPlan or successor SurfaceModel shape
Provider metadata
Vocabulary metadata
InteractionRequest / Interaction contract
Delivery binding
```

Tasks:

- [x] Define the VNext-native UI consumption shape.
- [x] Define how the shape preserves program, capability, delivery, provider, vocabulary, provenance, and evidence metadata.
- [x] Define how the shape supports chart consumers without becoming chart-only.
- [x] Define how the shape supports non-chart consumers without render-plan assumptions.
- [x] Define how interaction/toggle/tooltip concerns attach without owning semantic meaning.
- [x] Define how existing `ChartRenderPlan` participates or is wrapped as a consumer-neutral surface.
- [x] Define compatibility rules for existing chart-family adapters.
- [x] Add contract tests for metadata preservation and invalid drift.
- [x] Add guardrails preventing UI consumption contracts from importing concrete vendor/UI rendering types.

Completion condition:

```text
A VNext-native UI consumption contract shape exists and is test-backed before any family migration begins.
```

Planned evidence:

```text
documents/DataVisualiser_VNext_Native_UI_Consumption_Contract.md
contract metadata preservation tests
vendor/UI import guardrails
```

Phase 25 evidence:

```text
Production contracts added:
- DataVisualiser/VNext/Contracts/VNextUiConsumptionContract.cs
- DataVisualiser/VNext/Contracts/ConsumerSurfaceModel.cs

Contract document:
- documents/DataVisualiser_VNext_Native_UI_Consumption_Contract.md

Metadata and drift tests:
- VNextUiConsumptionContractTests.FromRenderPlan_ShouldPreserveProgramDeliveryProviderProvenanceAndSurfaceMetadata
- VNextUiConsumptionContractTests.FromIntent_ShouldSupportNonChartConsumersWithoutRenderPlan
- VNextUiConsumptionContractTests.Constructor_ShouldRejectProviderDeliveryDrift
- VNextUiConsumptionContractTests.Constructor_ShouldRejectRenderSurfaceForNonRenderDelivery
- VNextUiConsumptionContractTests.FromRenderPlan_ShouldRejectProgramDrift

Architecture guardrail:
- VNextUiConsumptionContract_ShouldRemainVendorAndUiNeutral

Validation:
- VNextUiConsumptionContractTests passed 5 tests
- ArchitectureGuardrailTests passed 108 tests

Implementation result:
- `ChartDataContext` was not retired in Phase 25
- UI/controller call sites were not rewired in Phase 25
- the replacement contract now exists for Phase 26+ consumption paths and family migrations
```

## 4.3 Phase 26 — Operation Chain Workbench MVP

Goal:

```text
Create a new tab/workbench that chains N operations over multiple input data series to produce derived datasets through the VNext-native UI consumption contract.
```

Tasks:

- [x] Define `OperationChainRequest`.
- [x] Define `OperationChainStep`.
- [x] Define `OperationChainProgram`.
- [x] Define `OperationChainExecutionPlan`.
- [x] Define `OperationChainExecutor`.
- [x] Define `OperationChainResult`.
- [x] Define `DerivedDataset`.
- [x] Define `DerivedDatasetSurfaceModel` or equivalent consumer-neutral output shape.
- [x] Define `OperationChainEvidence`.
- [x] Define `OperationChainTrace`.
- [x] Support at least two input series.
- [x] Support an ordered list of operations.
- [x] Support at least one derived dataset output.
- [x] Support at least three initial operations from the existing operation kernel / transform capabilities.
- [x] Deliver output through the Phase 25 VNext-native UI consumption contract.
- [x] Display result in a simple tab surface.
- [x] Prefer table/export first and chart rendering second unless the existing contract path makes chart display equally safe.
- [x] Preserve source provenance for every input series.
- [x] Preserve operation traceability for every chain step.
- [x] Preserve transformation reversibility/lossiness metadata where applicable.
- [x] Emit evidence/export metadata for the derived dataset.
- [x] Avoid `ChartDataContext` as the primary semantic model.
- [x] Avoid controller/ViewModel-owned operation execution.
- [x] Avoid vendor-specific rendering dependency in the operation-chain core.
- [x] Add tests for operation-chain execution.
- [x] Add tests for metadata preservation.
- [x] Add tests for provenance preservation.
- [x] Add tests for operation trace/evidence output.
- [x] Add guardrails preventing the new tab from owning analytical authority.

Completion condition:

```text
The new tab can create at least one derived dataset from multiple source series through a VNext-native, metadata-preserving consumption path without making ChartDataContext the primary semantic model.
```

Planned evidence:

```text
documents/DataVisualiser_Operation_Chain_Workbench_MVP_Audit.md
operation-chain execution tests
metadata/provenance/evidence tests
UI consumption contract tests
```

Phase 26 evidence:

```text
Production contracts/core added:
- DataVisualiser/VNext/Contracts/OperationChainContracts.cs
- DataVisualiser/VNext/Application/OperationChainExecutor.cs

UI surface added:
- DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml
- DataVisualiser/UI/OperationChain/OperationChainWorkbenchView.xaml.cs
- DataVisualiser/MainWindow.xaml Operation Chain tab registration

Audit note:
- documents/DataVisualiser_Operation_Chain_Workbench_MVP_Audit.md

Focused tests:
- OperationChainExecutorTests.ExecuteAsync_ShouldChainMultipleOperationsAcrossWorkingDatasets
- OperationChainExecutorTests.ExecuteAsync_ShouldPreserveProvenanceTraceEvidenceAndConsumptionContractMetadata
- OperationChainExecutorTests.ExecuteAsync_ShouldRejectLoadedSnapshotsWithoutTwoInputSeries

Architecture guardrails:
- OperationChainWorkbench_ShouldKeepExecutionOutsideUiSurface
- OperationChainCore_ShouldStayUiAndVendorNeutral

Validation:
- OperationChainExecutorTests passed 3 tests
- ArchitectureGuardrailTests passed 110 tests

Implementation result:
- operation-chain core executes ordered VNext operation-kernel steps
- derived outputs are delivered through VNextUiConsumptionContract
- UI tab is display-only and table/export first
- manual smoke testing is warranted before Phase 27 because a visible tab was added
- manual smoke confirmed: Operation Chain tab visible; empty surface renders; Charts, Syncfusion, and Admin tab switching still works
```

## 4.4 Phase 27 — Migrate First Production Chart Family to VNext-Native Consumption

Goal:

```text
Move one production chart family away from ChartDataContext-dependent consumption.
```

Selection rule:

```text
Choose the family with the clearest contract path, strongest tests, lowest UI coupling, and least ambiguous parity surface.
Do not choose based only on easiest code movement.
Use Phase 26 findings if the operation-chain MVP exposes better contract/surface requirements.
```

Tasks:

- [x] Select one chart family and document selection reason.
- [x] Capture current behavior with focused tests before changing production code.
- [x] Route the selected family through the VNext-native UI consumption contract.
- [x] Preserve capability contract carriage.
- [x] Preserve provider metadata.
- [x] Preserve vocabulary metadata.
- [x] Preserve provenance / traceability metadata.
- [x] Preserve UI behavior and interaction behavior.
- [x] Preserve evidence/export behavior.
- [x] Confirm no old hub absorbs the new migration responsibility.
- [x] Confirm `ChartDataContext` dependency is removed or reduced for the selected family.
- [x] Run focused family tests and full test suite.

Completion condition:

```text
One production chart family consumes VNext-native contract output instead of relying on ChartDataContext as its primary model.
Behavior, metadata, evidence, and parity remain preserved.
```

Planned evidence:

```text
documents/DataVisualiser_First_VNext_Native_Family_Migration_Audit.md
family-specific migration tests
metadata preservation tests
UI behavior tests
```

Phase 27 evidence:

```text
Selected family:
- Distribution

Audit note:
- documents/DataVisualiser_First_VNext_Native_Family_Migration_Audit.md

Production changes:
- DistributionChartControllerAdapter now sends UseVNextNativeConsumption: true
- DistributionRenderingContract builds and attaches VNextUiConsumptionContract metadata
- DistributionRenderingContract uses direct service rendering when the native flag is true
- Legacy ChartDataContext orchestrator path remains compatibility-only when the native flag is false

Focused tests:
- DistributionRenderingContractTests.DistributionVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
- DistributionRenderingContractTests.RenderAsync_WithVNextNativeConsumption_ShouldUseDirectServicePath
- DistributionChartControllerAdapterTests.RenderAsync_ShouldPassDistributionCapabilityContract_ToRenderingContract
- DistributionChartControllerAdapterTests.RenderAsync_ShouldPassVNextNativeConsumptionContract_ToRenderingContract

Focused validation:
- Distribution-focused test filter passed 63 tests
- Architecture/evidence export focused filter passed 128 tests
- DataVisualiser.Tests passed 1012 tests
- DataFileReader.Tests passed 15 tests

Manual smoke evidence:
- 2026-05-01 export `documents/reachability-20260501-083930.json` confirmed Distribution visible
- Distribution mode/settings/chart-type changes were recorded as session milestones
- Distribution parity completed with weekly and hourly parity passed
- latest Distribution render plan carried `ConsumptionContractSignature`, `SurfaceKind`, and `SurfaceId`
- no recent UI smoke-check errors were recorded
```

## 4.5 Phase 28 — Retire Corresponding Legacy Bridge for First Migrated Family

Goal:

```text
Remove or disable the legacy bridge path for the first migrated family only after the VNext-native path is proven.
```

Tasks:

- [x] Identify the selected family's remaining legacy bridge path.
- [x] Confirm VNext-native replacement is used in production path.
- [x] Confirm parity evidence.
- [x] Confirm smoke evidence or UI behavior evidence.
- [x] Confirm metadata preservation.
- [x] Confirm semantic/provenance preservation.
- [x] Confirm evidence/export path still works.
- [x] Remove or disable only the selected family's retired bridge path.
- [x] Leave other family bridges untouched unless separately proven.
- [x] Run full validation.

Completion condition:

```text
The first migrated family no longer depends on its corresponding legacy bridge path.
No behavior, metadata, parity, or evidence regression occurs.
```

Planned evidence:

```text
documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md
family bridge-retirement tests
parity / smoke / metadata evidence
```

Phase 28 evidence:

```text
Retired bridge path:
- DistributionRenderingContract -> ChartRenderingOrchestrator.RenderDistributionChartAsync

Production changes:
- DistributionRenderingContract no longer accepts/stores a ChartRenderingOrchestrator provider
- DistributionRenderingContract no longer branches to RenderDistributionChartAsync for Cartesian Distribution rendering
- DistributionChartControllerAdapter no longer passes a native-consumption compatibility switch
- ChartControllerFactory no longer supplies the orchestrator provider to DistributionRenderingContract

Evidence note:
- documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md

Focused tests:
- DistributionRenderingContractTests.RenderAsync_AfterBridgeRetirement_ShouldUseDirectServicePath
- ArchitectureGuardrailTests.DistributionFamilyBridgeRetirement_ShouldRemoveLegacyOrchestratorFallback
- DistributionChartControllerAdapterTests.RenderAsync_ShouldPassVNextConsumptionContract_ToRenderingContract

Focused validation:
- Distribution-focused test filter passed 64 tests
- ArchitectureGuardrailTests passed 111 tests

Full validation:
- DataVisualiser.Tests passed 1012 tests
- DataFileReader.Tests passed 15 tests

Manual evidence used:
- documents/reachability-20260501-083930.json confirmed Distribution render/parity/metadata before retirement
- documents/reachability-20260501-114754.json confirmed Distribution render/parity/metadata after bridge retirement
```

## 4.6 Phase 29 — Repeat Production Family Migration Slice-by-Slice

Goal:

```text
Migrate remaining production chart families through the VNext-native consumption path one family at a time.
```

Tasks:

- [ ] Use the Phase 27/28 pattern for each remaining family.
- [ ] Migrate one family at a time.
- [ ] Retire only that family's proven legacy bridge.
- [ ] Preserve capability contract carriage.
- [ ] Preserve metadata and provenance.
- [ ] Preserve UI behavior.
- [ ] Preserve evidence/export behavior.
- [ ] Update the migration tracker after each family.
- [ ] After two families are migrated, identify common shape candidates.
- [ ] Do not extract shared abstractions until common shape is proven by at least two migrated families.

Completion condition:

```text
All production chart families either consume VNext-native contract output or have explicit documented reasons for temporary deferral.
```

Planned evidence:

```text
documents/DataVisualiser_VNext_Native_Family_Migration_Tracker.md
per-family migration tests
per-family bridge retirement evidence
```

Phase 29 current slice evidence:

```text
Active family slice:
- WeekdayTrend

Tracker:
- documents/DataVisualiser_VNext_Native_Family_Migration_Tracker.md

Production changes:
- WeekdayTrendChartRenderRequest now carries optional VNextUiConsumptionContract
- WeekdayTrendRenderingContract builds and attaches VNextUiConsumptionContract metadata
- WeekdayTrendRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before delivery
- existing WeekdayTrendChartUpdateCoordinator rendering behavior is preserved

Bridge status:
- no Distribution-style legacy orchestrator fallback was found in the WeekdayTrend rendering contract
- no bridge retirement code removal was required for this family slice

Focused tests:
- WeekdayTrendRenderingContractTests.WeekdayTrendVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
- WeekdayTrendRenderingContractTests.Render_ShouldAttachVNextConsumptionMetadata
- WeekdayTrendChartControllerAdapterTests.OnChartTypeToggleRequested_ShouldToggleMode_AndRenderLastContext
- ArchitectureGuardrailTests.WeekdayTrendFamilyMigration_ShouldUseVNextConsumptionContractMetadata

Focused validation:
- WeekdayTrend-focused test filter passed 35 tests
- ArchitectureGuardrailTests passed 112 tests

Full validation:
- DataVisualiser.Tests passed 1015 tests
- DataFileReader.Tests passed 15 tests

Pending before next family:
- WeekdayTrend manual smoke confirmed by documents/reachability-20260501-154628.json
- BarPie manual smoke test because a production chart-family render path now attaches VNext consumption metadata
```

Phase 29 current slice evidence:

```text
Active family slice:
- BarPie

Tracker:
- documents/DataVisualiser_VNext_Native_Family_Migration_Tracker.md

Production changes:
- BarPieChartRenderRequest now carries optional VNextUiConsumptionContract
- BarPieRenderingContract builds and attaches VNextUiConsumptionContract metadata
- BarPieRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before delivery
- existing UiChartRenderPlanAdapter and renderer-surface behavior is preserved

Bridge status:
- no Distribution-style legacy orchestrator fallback was found in the BarPie rendering contract
- no bridge retirement code removal was required for this family slice

Focused tests:
- BarPieRenderingContractTests.BarPieVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
- BarPieRenderingContractTests.RenderAsync_ShouldPreserveVocabularyMetadata
- ArchitectureGuardrailTests.BarPieFamilyMigration_ShouldUseVNextConsumptionContractMetadata

Focused validation:
- BarPie-focused test filter passed 28 tests
- Transform-focused test filter passed 7 tests
- SyncfusionSunburst-focused test filter passed 5 tests
- Main-focused test filter passed 12 tests
- Secondary Cartesian focused test filter passed 5 tests
- ArchitectureGuardrailTests passed 117 tests

Full validation:
- DataVisualiser.Tests passed 1027 tests
- DataFileReader.Tests passed 15 tests

Smoke evidence:
- BarPie manual smoke confirmed by documents/reachability-20260501-170058.json
- Transform selected as the next Phase 29 family because it already has a capability contract and uses the shared Cartesian render-plan adapter path
- TransformVNextConsumptionContractBuilder builds a VNextUiConsumptionContract from the Transform capability contract and concrete render plan
- ChartUpdateRequest now accepts an optional render-plan consumption-contract factory for shared Cartesian paths
- ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId when a render-plan consumption-contract factory is supplied
- TransformChartRenderInvoker supplies the Transform consumption-contract factory while preserving existing rendering behavior
- TransformRenderingContractTests.TransformVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata added
- ChartUpdateCoordinatorTests.RenderTransformChartAsync_ShouldCaptureRenderPlanDiagnostics extended for Transform consumption metadata
- ArchitectureGuardrailTests.TransformFamilyMigration_ShouldUseVNextConsumptionContractMetadata added

Pending before next family:
- Transform manual smoke confirmed by documents/reachability-20260501-173257.json
- SyncfusionSunburst selected as the next Phase 29 family because four LiveCharts-backed slices have already proven the common shape and SyncfusionSunburst is the smallest remaining dedicated hierarchy contract
- SyncfusionSunburstChartRenderRequest now carries optional VNextUiConsumptionContract
- SyncfusionSunburstRenderingContract builds and attaches VNextUiConsumptionContract metadata
- SyncfusionSunburstRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before hierarchy delivery
- existing SyncfusionSunburstRenderPlanAdapter rendering behavior is preserved
- SyncfusionSunburstRenderingContractTests.SyncfusionSunburstVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata added
- SyncfusionSunburstRenderingContractTests.RenderAsync_ShouldAttachVNextConsumptionMetadata added
- ArchitectureGuardrailTests.SyncfusionSunburstFamilyMigration_ShouldUseVNextConsumptionContractMetadata added
- SyncfusionSunburst manual smoke confirmed by documents/reachability-20260501-175856.json
- Main selected as the next Phase 29 family because it is the remaining active primary shared Cartesian path
- CartesianMetricVNextConsumptionContractBuilder builds a VNextUiConsumptionContract from the concrete CartesianMetric render plan
- MainChartRenderInvocationStage supplies a render-plan consumption-contract factory to the shared ChartUpdateCoordinator path
- ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before LiveCharts delivery
- Main chart stacked/cumulative display-mode resolution remains render-plan driven rather than fixed by a static capability contract
- CartesianMetricChartRenderingContractTests.CartesianMetricVNextConsumptionContractBuilder_ShouldWrapMainRenderPlanAndPreserveMetadata added
- CartesianMetricChartRenderingContractTests.CartesianMetricVNextConsumptionContractBuilder_ShouldRejectNonCartesianMetricProgramKind added
- ChartUpdateCoordinatorTests.UpdateChartUsingStrategyAsync_WithConsumptionContractFactory_ShouldAttachVNextMetadata added
- ArchitectureGuardrailTests.MainFamilyMigration_ShouldUseVNextConsumptionContractMetadata added
- Main manual smoke confirmed by documents/reachability-20260501-180931.json
- Normalized and Difference/Ratio selected as the next Phase 29 family slice because they share the same secondary Cartesian chart path and the user requested they move together
- SecondaryMetricChartRenderInvocationStage supplies a render-plan consumption-contract factory for Normalized, Difference, and Ratio routes
- ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before LiveCharts delivery
- Diff/Ratio active UI usage remains caveated, but the latent render path now carries VNext consumption metadata when invoked
- ChartRenderingOrchestratorTests.RenderNormalizedChartAsync_ShouldUseNormalizedCutOver_AndRenderTrackedSeries extended for Normalized consumption metadata
- ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldUseRatioCutOver_WhenRatioModeSelected extended for Ratio consumption metadata
- ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldUseDifferenceCutOver_AndCaptureRenderPlan_WhenDifferenceModeSelected extended for Difference consumption metadata
- ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldCaptureDifferenceAndRatioRenderPlans_WhenModeChanges extended for Difference/Ratio consumption metadata
- ArchitectureGuardrailTests.SecondaryCartesianFamiliesMigration_ShouldUseVNextConsumptionContractMetadata added
- Normalized manual smoke confirmed by documents/reachability-20260501-182337.json
- documents/reachability-20260501-182337.json reported IsNormalizedVisible true, NormalizedParity passed, OverallPassed true, no recent UI smoke-check errors, and no missing render-plan vocabulary or provider plan kinds
- Diff/Ratio manual UI smoke is not available because Diff/Ratio is not wired to the current UI; Difference and Ratio render-path metadata is covered by focused automated tests
```

## 4.7 Phase 30 — Elevate Consumer-Neutral Surface Model

Goal:

```text
Make the surface output shape explicit and consumer-neutral before terminal delivery.
```

Tasks:

- [ ] Compare the VNext-native consumption shapes produced by migrated families.
- [ ] Include Phase 26 operation-chain output in the comparison.
- [ ] Identify common consumer-neutral surface elements.
- [ ] Identify family-specific extensions that must remain explicit.
- [ ] Define or refine `SurfaceModel` / equivalent output shape.
- [ ] Ensure the surface shape preserves program, capability, provider, vocabulary, provenance, and delivery metadata.
- [ ] Ensure the surface shape does not import concrete vendor/UI rendering types.
- [ ] Ensure delivery adapters consume surface output rather than upstream internals.
- [ ] Add tests proving surface output can support at least one chart consumer and one non-chart consumer.
- [ ] Add tests proving surface output can support derived datasets from Operation Chain.
- [ ] Update guardrails to treat the surface model as the required pre-delivery seam where applicable.

Completion condition:

```text
A consumer-neutral, metadata-preserving surface seam exists before terminal delivery and is used by migrated production paths.
```

Planned evidence:

```text
documents/DataVisualiser_Consumer_Neutral_Surface_Model_Convergence_Audit.md
surface contract tests
chart + non-chart surface consumption tests
derived-dataset surface tests
```

## 4.8 Phase 31 — Thin UI / Interaction / State Layer

Goal:

```text
Move UI toward receiving already-authoritative, already-qualified output.
```

Tasks:

- [ ] Identify UI/state responsibilities still carrying semantic or provider policy.
- [ ] Identify interaction responsibilities that should become contract-mediated.
- [ ] Identify ViewModel/state dependencies that still require `ChartDataContext`.
- [ ] Confirm Operation Chain UI does not own operation execution.
- [ ] Move or remove only responsibilities already replaced by VNext-native consumption.
- [ ] Keep UI as display/state/interaction relay.
- [ ] Keep interaction behavior non-authoritative.
- [ ] Keep tooltip behavior explanatory, not semantic.
- [ ] Add guardrails preventing reintroduction of authority/provider/evidence policy into UI/state.
- [ ] Run focused UI/interaction/state tests and full validation.

Completion condition:

```text
UI, interaction, and state layers consume target outputs and relay user behavior without owning semantic, provider, evidence, or delivery authority.
```

Planned evidence:

```text
documents/DataVisualiser_UI_Interaction_State_Consolidation_Audit.md
UI/state/interaction guardrail tests
operation-chain UI guardrails
```

## 4.9 Phase 32 — Demote Rendering / Backend / Vendor Fully

Goal:

```text
Make rendering, backend, vendor, host, and lifecycle concerns terminal and replaceable.
```

Tasks:

- [ ] Confirm upstream contracts do not depend on concrete vendor-specific types.
- [ ] Confirm rendering code does not own interpretation.
- [ ] Confirm backend code does not define semantic policy.
- [ ] Confirm vendor code does not define analytical meaning.
- [ ] Confirm host/lifecycle code is terminal.
- [ ] Confirm delivery qualification is explicit and tested.
- [ ] Confirm at least one migrated path can switch or target a non-default backend where applicable.
- [ ] Confirm Operation Chain output can be delivered without vendor-specific assumptions.
- [ ] Remove or bound any remaining vendor assumptions leaking upstream.
- [ ] Run vendor-boundary and delivery tests.

Completion condition:

```text
Delivery is downstream, replaceable, vendor-contained, and semantically non-authoritative for migrated production paths.
```

Planned evidence:

```text
documents/DataVisualiser_Rendering_Vendor_Delivery_Demotion_Audit.md
vendor-boundary tests
delivery qualification tests
```

## 4.10 Phase 33 — Consolidate Capability / Contract Families

Goal:

```text
Prevent capability contracts and render-family contracts from becoming parallel family micro-frameworks.
```

Tasks:

- [ ] Compare all migrated family capability contracts.
- [ ] Compare all migrated family delivery contracts.
- [ ] Compare all migrated family render/surface requests.
- [ ] Compare Operation Chain contracts against existing Transform / MovingAverage / derived-series patterns.
- [ ] Identify repeated safe structure.
- [ ] Identify real family-specific differences.
- [ ] Extract only genuinely shared patterns.
- [ ] Preserve explicit family differences.
- [ ] Remove duplicated exception-driven paths.
- [ ] Add tests proving shared shape does not flatten real family distinctions.
- [ ] Run full validation.

Completion condition:

```text
Capability and contract families are generalized where proven and explicit where genuinely different.
```

Planned evidence:

```text
documents/DataVisualiser_Capability_Contract_Consolidation_Audit.md
shared-shape tests
family-difference preservation tests
operation-chain contract comparison
```

## 4.11 Phase 34 — Retire Remaining Legacy Bypasses

Goal:

```text
Remove remaining legacy coexistence only after production paths have moved.
```

Tasks:

- [ ] List all remaining legacy bypasses.
- [ ] Confirm target replacement for each bypass.
- [ ] Confirm no production consumer still depends on each bypass.
- [ ] Confirm parity evidence.
- [ ] Confirm smoke/UI behavior evidence.
- [ ] Confirm metadata preservation.
- [ ] Confirm semantic/provenance preservation.
- [ ] Retire one bypass at a time.
- [ ] Keep parity/evidence paths only where they still provide active validation value.
- [ ] Update documentation and progress log after each retirement.

Completion condition:

```text
Legacy coexistence is reduced to zero or to explicitly documented validation-only paths with named retirement conditions.
```

Planned evidence:

```text
documents/DataVisualiser_Remaining_Legacy_Bypass_Retirement_Audit.md
bridge-retirement tests
updated parity/evidence validation
```

## 4.12 Phase 35 — Final Convergence Audit

Goal:

```text
Prove the target architecture, not merely implement it.
```

Tasks:

- [ ] Regenerate latest `project-tree.txt`.
- [ ] Regenerate latest `codebase-index.md`.
- [ ] Regenerate latest `dependency-summary.md`.
- [ ] Regenerate latest `type-dependency-diagram.md`.
- [ ] Reclassify dependency density.
- [ ] Confirm no unbounded `ChartDataContext` production bridge remains.
- [ ] Confirm UI consumes VNext-native contract/surface output.
- [ ] Confirm Operation Chain consumes VNext-native contract/surface output.
- [ ] Confirm legacy projectors are retired or validation-only.
- [ ] Confirm rendering/vendor concerns are terminal.
- [ ] Confirm evidence remains observational.
- [ ] Confirm non-chart consumer path still works.
- [ ] Confirm capability contracts are generalized where proven.
- [ ] Confirm all tests pass.
- [ ] Confirm parity/smoke/metadata/provenance evidence.
- [ ] Update final completion criteria and progress log.

Completion condition:

```text
The production architecture can be described through the target grammar without relying on unbounded legacy bridges.
The target spine is used by production consumers, not merely proven by isolated capability slices.
```

Planned evidence:

```text
documents/DataVisualiser_Final_Convergence_Audit.md
fresh generated baseline artifacts
full test validation
parity / smoke / metadata / provenance evidence
```

---

# 5. Post-Architecture Formalisation and Bounded-Generativity Phases

These phases remain intentionally high-level.

They are not the active migration track.

They exist to align post-convergence work with the Prime Directive:

```text
preserve coherence while enabling governed analytical generation
```

They should be refined after Phase 35, using the Prime Directive coverage note, the final convergence audit, and the first Operation Chain evidence.

---

## Phase 36 — Requirements-to-Language Coverage Matrix

Goal:

```text
Determine whether requirements, planned work, and implemented constructions map cleanly into the formal architectural language.
```

Purpose:

```text
R -> L coverage
```

Expected outputs:

```text
RequirementsToLanguageCoverageMatrix
CoverageStatus
AmbiguityRisk
MissingLanguageRecord
CollapsedConcernRecord
FormalExpressionGap
```

Coverage statuses:

```text
covered
partially covered
collapsed
ambiguous
missing
deferred
```

Alignment focus:

```text
one-to-one clarity
onto coverage
semantic loss detection
language gap discovery
```

---

## Phase 37 — Construction Algebra Baseline

Goal:

```text
Define the first formal construction layer above vocabulary and grammar.
```

Expected outputs:

```text
Construction
Operation
Relation
InputSet
OutputSet
DerivedSet
IntermediateSet
CompositionGraph
TransformationTrace
EvidenceTrace
ConflictRecord
PromotionRecord
QuarantineRecord
```

Core laws to establish:

```text
arity law
composition law
transformation law
provenance law
traceability law
lossiness / reversibility law
qualification law
consumer projection law
boundary crossing law
evidence sufficiency law
promotion / quarantine law
```

Alignment focus:

```text
grammar -> algebra
classification -> construction
validity -> governed generativity
```

---

## Phase 38 — Operation / Capability Algebra

Goal:

```text
Formalize analytical capability as lawful composable power, not feature growth.
```

Expected outputs:

```text
OperationAlgebra
CapabilityPurpose
CapabilityPrecondition
CapabilityPostcondition
CapabilityArity
CapabilityCost
CapabilityCompatibility
CapabilityFitness
CapabilityPromotionRule
```

Alignment focus:

```text
operation rules
capability rules
composition compatibility
input/output discipline
lossiness and reversibility declaration
```

---

## Phase 39 — Typed Relation System

Goal:

```text
Make relations between constructions explicit, typed, auditable, and projectable.
```

Expected outputs:

```text
TypedRelation
OwnershipRelation
DependencyRelation
DerivationRelation
ProjectionRelation
QualificationRelation
InterpretationRelation
EvidenceRelation
ContradictionRelation
BoundaryRelation
```

Alignment focus:

```text
edges, not only nodes
ownership clarity
boundary crossing clarity
relation-specific evidence
graph projection by purpose
```

---

## Phase 40 — Multiplicity / Derived Dataset Model

Goal:

```text
Formalize N-input, N-operation, N-output analytical construction.
```

Expected outputs:

```text
Multiplicity
Arity
Cardinality
Sequence
Chain
InputSet
OutputSet
DerivedSet
IntermediateSet
ManyToOne
OneToMany
ManyToMany
DerivedDatasetIdentity
```

Alignment focus:

```text
Operation Chain pressure test
derived dataset formalisation
intermediate-state visibility
consumer-neutral derived outputs
```

---

## Phase 41 — Evidence Sufficiency / Promotion Rules

Goal:

```text
Define when a generated construction has enough evidence to be promoted, retained, quarantined, or rejected.
```

Expected outputs:

```text
EvidenceSufficiencyRule
PromotionRule
QuarantineRule
RejectionRule
EvidenceTrace
AuditRecord
GovernanceReviewRecord
ConstructionDecisionRecord
```

Alignment focus:

```text
evidence-backed promotion
controlled emergence
non-controlling evidence
reviewable governance
```

---

## Phase 42 — Semantic / Interpretation / Assumption Model

Goal:

```text
Support semantic plurality without false certainty.
```

Expected outputs:

```text
SemanticAuthority
AssumptionRecord
InterpretationContext
InterpretationModel
ConfidenceModel
ConflictRecord
ContrastiveExplanation
WhyThisOutput
WhyThisDifference
WhyThisMatters
```

Alignment focus:

```text
semantic ambiguity
plural interpretation
confidence and assumptions
trace vs explanation
meaning without UI/render ownership
```

---

## Phase 43 — Analytical Fitness / Usefulness Evaluation

Goal:

```text
Evaluate whether valid analytical constructions are useful, meaningful, or distortion-prone.
```

Expected outputs:

```text
AnalyticalFitness
UsefulnessScore
InsightHypothesis
SignalPreservation
DistortionProfile
InterpretationPotential
ConfidenceImpact
FitnessEvidence
```

Alignment focus:

```text
validity != usefulness
computability != meaning
traceability != explanation
analytical relevance over mere execution
```

---

## Phase 44 — Computational Planning / Bounded Search

Goal:

```text
Keep generative analytical construction computationally tractable.
```

Expected outputs:

```text
BoundedSearchPolicy
OperationCostModel
CapabilityPruningRule
ExecutionPlanner
IncrementalRecomputation
IntermediateCache
WorkflowCache
ExecutionManifest
DeterministicReplayRecord
```

Alignment focus:

```text
qualified search space
bounded depth
cost-aware execution
reproducibility
tractability under constraints
```

---

## Phase 45 — Generative Multi-Consumer Output

Goal:

```text
Allow generated analytical constructions to produce multiple consumer outputs without centralizing a new mega-model.
```

Expected outputs:

```text
GeneratedSurfaceModel
GeneratedTableConsumer
GeneratedChartConsumer
GeneratedReportConsumer
GeneratedApiConsumer
GeneratedExportConsumer
GeneratedEvidenceConsumer
ConsumerProjectionRule
```

Alignment focus:

```text
one truth, many projections
consumer-neutral core
consumer-specific projection
delivery-specific adaptation
no ChartDataContext replacement mega-object
```

---

## Phase 46 — Governance / Emergence Review

Goal:

```text
Create reviewable governance for generated constructions, language growth, and architecture evolution.
```

Expected outputs:

```text
EmergenceReview
LanguageGrowthRecord
GuardrailScorecard
AgentTaskPack
MigrationBenchmarkScenario
BeforeAfterDependencySnapshot
RefactoringEvidenceReport
GovernanceDecisionLog
```

Alignment focus:

```text
controlled language growth
evidence-backed evolution
agent governance
architecture drift detection
```

---

## Phase 47 — Scenario Hardening

Goal:

```text
Harden one or more bounded product/domain scenarios against the formal coverage and construction algebra model.
```

Candidate scenarios:

```text
Operation Chain Workbench
analytical workbench
dashboard runtime
personal analytics system
evidence platform
legacy migration framework
AI-assisted architecture governance platform
```

Alignment focus:

```text
scenario proof
formal coverage proof
construction algebra proof
evidence sufficiency proof
bounded generativity proof
```


---

# 6. Guardrails

Status note:

```text
Completed Phase 1-23 guardrails remain active constraints.
Unchecked consumption-migration guardrails must be satisfied before convergence can be claimed.
```

## 6.1 Consumption Migration Guardrails

- [ ] Do not replace `ChartDataContext` with another chart-specific pseudo-core.
- [ ] VNext-native UI consumption contracts must preserve program, capability, delivery, provider, vocabulary, provenance, and evidence metadata.
- [ ] UI consumption contracts must not import concrete vendor/rendering types.
- [ ] Production UI paths must move slice-by-slice, not through a broad rewrite.
- [ ] Legacy bridge retirement must follow proven replacement, parity, smoke, metadata, and provenance evidence.
- [ ] Family migration must preserve real family differences.
- [ ] Generalization must wait until at least two migrated families prove the same shape.
- [ ] `LegacyChartProgramProjector`, `VNextDataResolutionHelper`, and `LegacyMetricViewGateway` must not be removed until their named retirement conditions are met.
- [ ] Operation Chain must use the VNext-native UI consumption contract defined in Phase 25.
- [ ] Operation Chain must not use `ChartDataContext` as its primary semantic model.
- [ ] Operation Chain UI must not own analytical operation execution.
- [ ] Operation Chain must preserve source provenance, operation traceability, and evidence metadata.
- [ ] Operation Chain must not introduce vendor-specific rendering dependencies into its core execution path.

## 6.2 Refactoring Guardrails

- [x] Refactoring must reduce sprawl, contradiction, duplication, or exception-driven architecture.
- [x] Refactoring must strengthen the generalized target architecture.
- [x] Refactoring must preserve behavior.
- [x] Refactoring must preserve tests.
- [x] Refactoring must not hide real family-specific differences.
- [x] Refactoring must not centralize UI, rendering, vendor, process, or evidence authority.
- [x] Refactoring must not replace explicit seams with hidden orchestration.
- [x] Refactoring must be auditable through tests, parity, or dependency evidence.

## 6.3 Authority Guardrails

- [x] UI must not define canonical meaning.
- [x] Rendering must not define analytical meaning.
- [x] Process must not define semantic truth.
- [x] Evidence must not create live semantic authority.
- [x] Provider code must not become semantic authority.
- [x] Consumer code must not redefine canonical meaning.

## 6.4 Provenance / Traceability Guardrails

- [x] Results must preserve source lineage.
- [x] Interpretations must preserve derivation context.
- [x] Transformations must record traceability.
- [x] Projections must not discard provenance silently.
- [x] Delivery must not remove required metadata.
- [x] Evidence must expose lineage where relevant.

## 6.5 Fidelity / Reversibility Guardrails

- [x] Transformations must be lossless or explicitly annotated as lossy.
- [x] Derived outputs must preserve recovery path where required.
- [x] Projections must preserve semantic fidelity.
- [x] Delivery adaptation must not alter canonical meaning.
- [x] Any irreversible step must be explicit and justified.

## 6.6 Capability Guardrails

- [x] New analytical behavior must enter through capability/program structures.
- [x] Controllers must not become capability owners.
- [x] Renderers must not become capability owners.
- [x] Coordinators must not absorb capability planning.
- [x] Capability output must remain contract-bound.

## 6.7 Contract / Boundary Guardrails

- [x] Contracts must be explicit at major handoffs.
- [x] Boundaries must prevent vendor assumptions from moving upstream.
- [x] Boundaries must prevent UI assumptions from moving upstream.
- [x] Boundaries must preserve metadata.
- [x] Boundary bypasses must be identified and retired only after replacement proof.

## 6.8 Qualification Guardrails

- [x] Provider compatibility must be qualified.
- [x] Backend compatibility must be qualified.
- [x] Adapter compatibility must be qualified.
- [x] Delivery compatibility must be qualified.
- [x] Qualification must not become hidden selection.
- [x] Selection must be justified when compatibility matters.

## 6.9 Consumer / Interaction Guardrails

- [x] Consumers receive meaning; they do not define it.
- [x] Interaction relays behavior; it does not redefine intent.
- [x] Tooltip logic must not own interpretation.
- [x] Timestamp sinks must not own analytical meaning.
- [x] ViewModels must not become semantic authorities.
- [x] Controllers must not own provider policy.

## 6.10 Delivery Guardrails

- [x] Delivery remains terminal.
- [x] Vendor concerns remain terminal.
- [x] Backend concerns remain terminal.
- [x] Rendering remains replaceable.
- [x] Host/lifecycle concerns remain downstream.
- [x] Delivery must not mutate canonical semantics.

## 6.11 Evidence / Audit Guardrails

- [x] Evidence observes only.
- [x] Diagnostics do not control live routing.
- [x] Parity does not control live execution.
- [x] Reachability does not select providers.
- [x] Validation does not become hidden authority.
- [x] Audit records must be durable and reviewable.

## 6.12 Governance Guardrails

- [x] New vocabulary must improve architectural clarity.
- [x] New concepts must align with project goals.
- [x] New abstractions must not centralize UI/render/vendor concerns.
- [x] New capabilities must use the target spine.
- [x] New consumers must use contracts/boundaries.
- [x] New delivery paths must remain replaceable.

## 6.13 Scaffold Audit Carry-Forward Guardrails

- [ ] Do not remove parity/evidence comparison paths before replacement, parity, smoke, metadata, and provenance evidence exists.
- [ ] Do not let evidence or diagnostics control live behavior.
- [ ] Do not let vendor/rendering types leak upstream into VNext-native consumption contracts.
- [ ] Do not let old hubs absorb authority, capability, provider policy, evidence policy, or delivery policy.
- [ ] Projectors translate; they do not create authority.
- [ ] Adapters apply already-decided output; they do not own provider policy or analytical intent.
- [ ] Selectors and resolvers must remain explicit and inspectable.
- [ ] Mapping, conversion, and formatting must not mutate provenance, confidence, or canonical result values.
- [ ] `ChartRenderModel` must not replace the architectural surface seam unless concrete rendering assumptions have been removed.
- [ ] ECharts remains a placeholder seam unless later hardened through explicit qualification.

## 6.14 Governance Growth Review Gate

Before new growth, answer:

```text
Which promoted grammar concept owns the change?
Which boundary or contract does it cross?
What provenance, traceability, fidelity, determinism, or reversibility is preserved?
What qualification or compatibility rule applies?
Which consumer-neutral shape exists before terminal delivery?
What evidence or audit path proves the behavior?
Which old hub is explicitly prevented from absorbing the responsibility?
```

## 6.15 Prime Directive Coverage Gate

Before post-convergence growth, answer:

```text
Which requirement does this construction satisfy?
Which formal language term(s) express it?
Does the construction collapse distinct concerns into one overloaded term?
Does the current language fail to express the construction without semantic loss?
Which operation, relation, arity, boundary, evidence, or promotion rule applies?
Does this construction improve bounded generativity without weakening coherence?
```

Rule:

```text
Do not expand the language merely because a term is interesting.
Do not collapse requirements merely because one term can cover them.
Do not generalize until repeated constructions prove shared shape.
Do not promote generated constructions without evidence.
Do not treat validity as usefulness.
Do not treat traceability as explanation.
Do not treat computability as meaning.
```

---

# 7. Completion Criteria

Status note:

```text
Structural, behavioral, and migration completion criteria are checked for Phase 1-23 scope only.
Consumption migration / convergence criteria remain open.
```

## 7.1 Structural Completion — Phase 1-23 Scope

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

## 7.2 Behavioral Completion — Phase 1-23 Scope

- [x] Existing behavior is preserved.
- [x] Existing tests pass.
- [x] New guardrail tests pass.
- [x] Parity is preserved for migrated paths.
- [x] Smoke tests pass for affected delivery paths.
- [x] Metadata preservation tests pass.
- [x] Invalid provider/backend/adapter combinations are rejected.
- [x] Evidence does not affect live execution.

## 7.3 Migration Completion — Phase 1-23 Scope

- [x] Refactoring opportunities have been classified and acted on where safe.
- [x] Sprawl, duplication, contradiction, and exception-driven paths are reduced where proven.
- [x] Major old hubs no longer absorb new target responsibilities.
- [x] New capabilities enter through the target spine.
- [x] At least one non-chart consumer path is proven.
- [x] Legacy bypasses are reduced or explicitly bounded.
- [x] Repeated family patterns are consolidated only where proven safe.
- [x] Current architecture can be described through the target grammar.
- [x] Documentation reflects validated state, not intended future state.

## 7.4 Consumption Migration / Convergence Completion

- [ ] `ChartDataContext` dependencies are classified and mapped to target replacements.
- [ ] Production UI consumes VNext-native contract/surface output for migrated paths.
- [ ] `LegacyChartProgramProjector` usage is retired or validation-only with named retirement condition.
- [ ] `VNextDataResolutionHelper` bridge logic is retired or validation-only with named retirement condition.
- [ ] `LegacyMetricViewGateway` is retired or explicitly bounded by provider replacement condition.
- [ ] Operation Chain Workbench consumes VNext-native contract/surface output without `ChartDataContext` as primary semantic model.
- [ ] At least one production chart family has fully migrated away from `ChartDataContext`-primary consumption.
- [ ] All production chart families are migrated or explicitly deferred with named blocker.
- [ ] Consumer-neutral surface output is the required pre-delivery seam for migrated paths.
- [ ] UI, interaction, and state layers remain non-authoritative after migration.
- [ ] Rendering, backend, vendor, host, and lifecycle concerns remain terminal.
- [ ] Remaining legacy bypasses are retired or validation-only.
- [ ] Fresh generated artifacts confirm no unclassified density or drift remains.
- [ ] Full test, parity, smoke, metadata, and provenance evidence supports convergence.

## 7.5 Prime Directive / Post-Convergence Readiness

- [ ] Requirements-to-language coverage can be evaluated after convergence.
- [ ] Operation Chain evidence can be used as the first formal pressure test.
- [ ] Missing language can be recorded without blocking Phases 24-35.
- [ ] Construction algebra work is deferred until convergence evidence exists.
- [ ] Post-convergence phases are aligned to bounded generativity rather than ordinary feature/product expansion.
- [ ] No post-convergence phase assumes that validity, usefulness, meaning, explanation, computability, and evidence are the same thing.

## 7.6 Closure Definition

The full migration can be considered complete when:

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

# 8. Compact Progress Log

| Date | Phase | Compressed Change | Evidence / Test | Status |
|---|---|---|---|---|
| 2026-04-28 | 1 | Regenerated baseline artifacts and recorded structural baseline. | 925 tests; 988 symbols; density 0.6656%; generated artifacts. | Complete |
| 2026-04-28 | 2 | Added architectural vocabulary to generated dependency governance references. | dependency-summary.md regenerated; references confirmed. | Complete |
| 2026-04-28 | 3 | Classified dependency density before refactoring. | Dependency density audit; no actual drift confirmed by density alone. | Complete |
| 2026-04-28 | 4 | Classified refactoring opportunities; avoided unproven movement. | Refactoring opportunity audit. | Complete |
| 2026-04-28 | 5 | Locked authority/provenance/fidelity guardrails. | Authority/provenance audit; 70 focused tests. | Complete |
| 2026-04-28 | 6 | Capped integration hubs. | Integration hub audit; 75 architecture tests. | Complete |
| 2026-04-28 | 7 | Hardened contract/boundary/qualification seam. | Contract/boundary audit; 67 VNext seam tests; 75 architecture tests. | Complete |
| 2026-04-28 | 8 | Guarded projection/translation as non-authoritative. | Projection audit; 33 projector/adapter tests; 78 architecture tests. | Complete |
| 2026-04-28 | 9 | Guarded consumer/interaction layer as non-authoritative. | Consumer/interaction audit; 118 tests. | Complete |
| 2026-04-28 | 10 | Elevated surface-model seam. | Surface-model audit; 121 tests. | Complete |
| 2026-04-28 | 11 | Demoted terminal delivery/vendor boundaries. | Terminal-delivery audit; 142 tests. | Complete |
| 2026-04-28 | 12 | Preserved evidence as observational. | Evidence audit; 158 tests. | Complete |
| 2026-04-28 | 13 | Added governance constraints. | Governance audit; 93 architecture tests. | Complete |
| 2026-04-28/29 | 14 | Implemented target-spine contract carriage for Distribution, WeekdayTrend, Transform. | Capability-slice audit; 227 focused tests. | Complete |
| 2026-04-29 | 15 | Added non-chart evidence export consumer path. | ConsumerDeliveryEvidence; ExecuteForConsumerAsync; 34 tests. | Complete |
| 2026-04-29 | 16 | Retired duplicate metadata bypasses; preserved flexible fallback paths. | Explicit metadata contracts; 176/196/186 focused validations. | Complete |
| 2026-04-29 | 17 | Added BarPieCapabilityContract; preserved Syncfusion hierarchy distinction. | BarPie tests; 950 tests. | Complete |
| 2026-04-29 | 18 | Added SyncfusionSunburstCapabilityContract; preserved HierarchyChart delivery. | Syncfusion tests; 952 tests. | Complete |
| 2026-04-29/30 | 19 | Extracted CartesianMetricRenderPlanBuilder; threaded CartesianMetricCapabilityContract; extracted VNextMetricLoadRouter. | Builder/contract/router tests; 996 DataVisualiser + 15 DataFileReader tests. | Complete |
| 2026-04-29 | 20 | Thinned chart-family adapters via builders/invokers. | Phase20BuilderInvokerTests; adapter guardrails; 975 tests at closure. | Complete |
| 2026-04-29 | 21 | Classified bridges; retired UseRenderPlanAdapter=false branch. | Single adapter render path; 975 tests at closure. | Complete |
| 2026-04-29 | 22 | Proved new MovingAverage capability through chart and API consumers without old hubs. | MovingAverage; TabularSummary; API consumer; end-to-end tests. | Complete |
| 2026-04-30 | 23 | Extracted Transform adapter composition factory and bounded DI concentration. | Composition factory guardrail; 996 DataVisualiser + 15 DataFileReader tests. | Complete |
| 2026-04-30 | 24 | Audited ChartDataContext migration path and bridge-retirement gates. | `documents/DataVisualiser_ChartDataContext_Migration_Audit.md`; ArchitectureGuardrailTests 107 passed. | Complete |
| 2026-04-30 | 25 | Added VNext-native UI consumption contract and neutral surface model. | `VNextUiConsumptionContractTests` 5 passed; ArchitectureGuardrailTests 108 passed. | Complete |
| 2026-04-30 | 26 | Added Operation Chain core executor and display-only workbench tab. | OperationChainExecutorTests 3 passed; ArchitectureGuardrailTests 110 passed; manual smoke confirmed. | Complete |
| 2026-04-30/2026-05-01 | 27 | Migrated Distribution to VNext-native consumption contract path. | Distribution 63 passed; architecture/evidence 128 passed; DataVisualiser 1012 passed; DataFileReader 15 passed; manual smoke export `documents/reachability-20260501-083930.json` confirmed Distribution render/parity/metadata. | Complete |
| 2026-05-01 | 28 | Retired Distribution legacy bridge path. | `documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md`; Distribution 64 passed; ArchitectureGuardrailTests 111 passed; DataVisualiser 1012 passed; DataFileReader 15 passed; post-retirement smoke export confirmed. | Complete |
| 2026-05-01 | 29 | Migrated WeekdayTrend and BarPie through VNext-native consumption contract paths. | `documents/DataVisualiser_VNext_Native_Family_Migration_Tracker.md`; WeekdayTrend smoke confirmed; BarPie 28 passed; ArchitectureGuardrailTests 113 passed; DataVisualiser 1017 passed; DataFileReader 15 passed; BarPie smoke export confirmed. | In progress |
|  | 30 | Elevate consumer-neutral surface model. | Surface convergence audit. | Planned |
|  | 31 | Thin UI / interaction / state layer. | UI/state/interaction guardrails. | Planned |
|  | 32 | Demote rendering / backend / vendor fully. | Delivery demotion audit. | Planned |
|  | 33 | Consolidate capability / contract families. | Consolidation audit. | Planned |
|  | 34 | Retire remaining legacy bypasses. | Remaining bypass retirement audit. | Planned |
|  | 35 | Final convergence audit. | Fresh generated artifacts; full evidence set. | Planned |
|  | 36-47 | Post-convergence formalisation and bounded-generativity phases. | Coverage matrix, construction algebra, typed relations, evidence sufficiency, analytical fitness, bounded search, and scenario-hardening artifacts. | Planned |

---

# 9. Agent Instruction Summary

For IDE agents:

```text
Read the vocabulary and target architecture document first.
Use this file as the sequential execution plan.
Use this file as the active carry-forward summary for scaffold audit constraints; do not require archived scaffold audits by default.
Do not skip phases.
Phases 1-23 are completed structural spine work.
Phases 24-35 are consumption migration and convergence.
Phases 36+ are post-convergence formalisation / bounded-generativity alignment.
Start from the earliest incomplete planned phase.
Do not refactor broadly or cosmetically.
Do pursue targeted refactoring that reduces sprawl, contradiction, duplication, or exception-driven structure.
Do not add new capability before consumption migration blockers are understood, except the planned Phase 26 Operation Chain proving slice after Phase 25.
Do not enact Phase 36+ formal-algebra work before Phase 35 convergence evidence exists.
Do not move code without behavior-preserving tests.
Prefer small auditable changes.
Prefer generalized seams where shared structure is proven by at least two migrated families.
Do not retire ChartDataContext-related bridges until replacement, parity, smoke, metadata, and provenance evidence exist.
Continue through logical work bundles without pausing for minor confirmation.
Pause for manual testing, milestone review, architecture-affecting ambiguity, or behavior-risk decisions.
Update progress only when tests or evidence support completion.
```
