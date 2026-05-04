# DataVisualiser Migration Plan and Guardrails — Recalibrated Lean Version

## Purpose

Implementation-facing execution document for the next stage of work.

Use this document for:

```text
AI IDE-agent execution
human review
sequential implementation tracking
evidence-backed completion
```

This document replaces the previous cluttered phase record with a current-forward plan.

Historical work is preserved only as footnotes.

---

## 1. Document Authority

```text
Architectural Vocabulary:
  owns concept meaning, ownership containers, target grammar, and Prime Directive.

Migration Plan:
  owns execution order, implementation phases, task scope, guardrails, and completion evidence.
```

Conflict rule:

```text
Concept meaning -> architectural vocabulary wins.
Execution order / current state -> migration plan wins.
```

---

## 2. Current State

```text
Target spine is materially represented in code.
Old mesh remains active.
Status is yellow, not green.
Consumer/surface/contract vocabulary is present in code.
Structural density increased after cleanup.
ChartDataContext remains a major UI/rendering-state carrier.
MainChartsView, SyncfusionChartsView, ChartControllerFactory*, ChartRenderingOrchestrator, and repeated family contract/builders remain pressure points.
```

Current structural snapshot:

```text
Generated artifact set: 2026-05-02 15:52-15:53
DataVisualiser symbols: 767
DataVisualiser.Tests symbols: 192
Total indexed symbols: 1048
Declared type symbols: 1013
Direct type-reference edges: 7587
Dependency density: 0.7401%
```

Current conclusion:

```text
The architecture is more semantically expressed, but not structurally lighter.

The next work must classify and reduce/contain pressure before adding broad formal runtime machinery.
```

---

## 3. Current Target Chain to Preserve

```text
authoritative program / capability / delivery metadata
-> VNextUiConsumptionContract
-> ConsumerSurfaceModel / ChartRenderPlan
-> qualified delivery adapter
-> terminal rendering or non-chart surface
-> observational evidence
```

Do not replace this chain with a new mega-object.

---

## 4. Current Pressure Points

```text
ChartDataContext
ProjectedContext / main-chart LegacyChartProgramProjector
VNextDataResolutionHelper selected-series fallback
service-backed metric loading through MetricLoadSnapshotGateway
strategy cut-over compatibility
MainChartsView / SyncfusionChartsView host gravity
ChartControllerFactory* fan-out
ChartRenderingOrchestrator / ChartUpdateCoordinator concentration
MetricLoadCoordinator VNext/legacy route pressure
family contract / builder repetition
EvidenceDiagnosticsBuilder expansion risk
```

Priority rule:

```text
If a proposed task does not classify, reduce, contain, or formally govern one of these pressure points, it is likely premature.
```

---

# Active Sequential Implementation Plan

## Phase 1 — Rebaseline and Density Classification

Goal:

```text
Classify the latest density increase before adding new abstraction.
```

Tasks:

- [ ] Regenerate current structural artifacts.
- [ ] Compare against the previous baseline.
- [ ] Classify density as:
  - [ ] necessary target-grammar density
  - [ ] transitional bridge density
  - [ ] UI/state gravity
  - [ ] delivery/vendor gravity
  - [ ] evidence/diagnostic support density
  - [ ] scaffolding debt
  - [ ] accidental coupling
- [ ] Identify the top incoming hubs.
- [ ] Identify the top outgoing sources.
- [ ] Identify which density is acceptable, bounded, reducible, or unknown.
- [ ] Produce a concise density classification note.

Completion condition:

```text
The current density increase is classified and no Phase 3+ implementation proceeds blindly.
```

Evidence:

```text
latest project-tree.txt
latest codebase-index.md
latest dependency-summary.md
latest type-dependency-diagram.md
DataVisualiser_Density_Rebaseline.md
```

---

## Phase 2 — Construction Inventory Before Algebra

Goal:

```text
Map the existing construction set before creating construction-algebra runtime machinery.
```

Tasks:

- [ ] Inventory current construction expressions:
  - [ ] Operation Chain
  - [ ] VNextUiConsumptionContract
  - [ ] ConsumerSurfaceModel
  - [ ] ChartRenderPlan
  - [ ] IAnalyticalCapabilityContract
  - [ ] evidence traces
  - [ ] projection bridges
  - [ ] strategy cut-over
  - [ ] metric-load gateway paths
- [ ] Map each construction to architectural vocabulary terms.
- [ ] Mark each construction as:
  - [ ] stable
  - [ ] transitional
  - [ ] overloaded
  - [ ] under-expressed
  - [ ] candidate for consolidation
  - [ ] candidate for retirement
- [ ] Identify reused terms that hide different concerns.
- [ ] Identify implemented structures not cleanly expressible in the vocabulary.

Completion condition:

```text
The implemented construction set is known before formal algebra work begins.
```

Evidence:

```text
DataVisualiser_Construction_Inventory.md
```

---

## Phase 3 — Coverage Gap Triage

Goal:

```text
Decide which language gaps require implementation, deferral, guardrails, or no action.
```

Inputs:

```text
Requirements-to-language coverage matrix
Construction inventory
Architectural vocabulary
Current density classification
```

Tasks:

- [ ] Triage missing or partial terms:
  - [ ] Confidence
  - [ ] Envelope
  - [ ] Interaction
  - [ ] Policy
  - [ ] Binding
  - [ ] Registry
  - [ ] Lifecycle
  - [ ] Interpretation
  - [ ] Semantics
  - [ ] Determinism
- [ ] For each term, decide:
  - [ ] implement now
  - [ ] express using existing construct
  - [ ] defer
  - [ ] guardrail only
  - [ ] reject as premature
- [ ] Review collapsed concerns:
  - [ ] Contract / Capability
  - [ ] SurfaceModel / Consumer
  - [ ] Projection / Adapter
  - [ ] Evidence / Validation
- [ ] Review ambiguity pairs:
  - [ ] Transformation / Projection
  - [ ] Provider / Backend
  - [ ] Delivery / Consumer
- [ ] Record only decisions that affect implementation.

Completion condition:

```text
No missing vocabulary term becomes runtime code merely because it is named.
```

Evidence:

```text
DataVisualiser_Coverage_Gap_Triage.md
```

---

## Phase 4 — Bridge and Hub Feasibility Matrix

Goal:

```text
Determine which current pressure point should be reduced first.
```

Tasks:

- [ ] Build a bridge-reduction feasibility matrix for:
  - [ ] ProjectedContext
  - [ ] LegacyChartProgramProjector
  - [ ] VNextDataResolutionHelper
  - [ ] MetricLoadSnapshotGateway
  - [ ] strategy cut-over compatibility
- [ ] Build a hub-responsibility review for:
  - [ ] MainChartsView
  - [ ] SyncfusionChartsView
  - [ ] ChartControllerFactory*
  - [ ] ChartRenderingOrchestrator
  - [ ] ChartUpdateCoordinator
  - [ ] MetricLoadCoordinator
  - [ ] EvidenceDiagnosticsBuilder
- [ ] Classify each as:
  - [ ] retire now
  - [ ] reduce now
  - [ ] contain now
  - [ ] document as bounded
  - [ ] defer
- [ ] Select exactly one first implementation slice for the Phase 5–6 loop.

Completion condition:

```text
The next implementation slice is chosen by evidence, not preference.
```

Evidence:

```text
DataVisualiser_Bridge_Hub_Feasibility_Matrix.md
```

---

## Phase 5 — Pressure-Reduction Slice

Goal:

```text
Reduce or contain one named pressure point without adding broad architecture.
```

Rules:

```text
one pressure point only
smallest coherent slice
behavior preserved
tests first or alongside
no broad folder/class reshuffle
no new mega-object
```

Tasks:

- [ ] Define selected pressure point.
- [ ] Define scope.
- [ ] Define non-goals.
- [ ] Define affected files/classes.
- [ ] Add or identify behavior-preserving tests.
- [ ] Implement the smallest safe reduction/containment.
- [ ] Preserve target chain.
- [ ] Re-run relevant tests.
- [ ] Regenerate structural artifacts if coupling changes materially.

Completion condition:

```text
One pressure point is measurably reduced or more tightly bounded.
```

Evidence:

```text
DataVisualiser_Pressure_Reduction_Slice.md
tests
updated structural artifacts if needed
```

---

## Phase 6 — Post-Slice Reassessment

Goal:

```text
Confirm whether the first pressure-reduction slice improved the architecture or merely moved coupling.
```

Tasks:

- [ ] Compare before/after dependency evidence.
- [ ] Confirm no new authority leakage.
- [ ] Confirm no new UI/state pseudo-core.
- [ ] Confirm no new vendor/delivery leakage upstream.
- [ ] Confirm evidence remains observational.
- [ ] Confirm target chain remains intact.
- [ ] Decide whether to:
  - [ ] repeat pressure-reduction
  - [ ] proceed to formal construction baseline
  - [ ] pause for manual architecture review

Completion condition:

```text
The next phase is justified by evidence.
```

Evidence:

```text
DataVisualiser_Post_Slice_Reassessment.md
```

### Phase 5–6 finite iteration rule

```text
Phases 5–6 may repeat for a finite number of pressure-reduction iterations before Phase 7 begins.
```

Recommended limit:

```text
2–3 iterations.
```

Hard limit:

```text
4 iterations without explicit architectural review.
```

Loop rule:

```text
Each Phase 5–6 loop must reduce or contain exactly one named pressure point, then reassess density, coupling, authority leakage, target-chain integrity, and test evidence.
```

Proceed to Phase 7 when:

```text
the next pressure point requires formal construction language rather than local cleanup
```

Stop and review when:

```text
the same pressure class remains unresolved after 3 focused slices
```

Interpretation:

```text
If a pressure class survives 3 focused slices, it is probably not local refactoring debt.
It is likely a deeper boundary issue, an accepted compatibility seam, or a signal that Phase 7 formalisation is needed.
```

---

## Phase 7 — Minimal Construction Algebra Baseline

Goal:

```text
Define only the construction-algebra concepts already justified by inventory and pressure evidence.
```

Allowed scope:

```text
conceptual contracts
records/interfaces only where needed
no broad runtime engine
no generalized workflow runtime
no speculative graph engine
```

Candidate concepts:

```text
Construction
Operation
Relation
InputSet
OutputSet
DerivedSet
IntermediateSet
TransformationTrace
EvidenceTrace
ConflictRecord
PromotionRecord
QuarantineRecord
```

Tasks:

- [ ] Select only justified terms from Phase 2 and Phase 3.
- [ ] Define minimal formal model.
- [ ] Define explicit non-goals.
- [ ] Reuse existing contracts/surfaces where possible.
- [ ] Add guardrail tests where meaningful.
- [ ] Avoid production rewiring unless required by the selected slice.

Completion condition:

```text
A minimal construction-algebra baseline exists without expanding runtime complexity unnecessarily.
```

Evidence:

```text
DataVisualiser_Minimal_Construction_Algebra_Baseline.md
focused tests if code is changed
```

---

## Phase 8 — Operation / Capability Algebra Slice

Goal:

```text
Formalize analytical capability as lawful composable power where current Operation Chain and capability contracts prove the need.
```

Tasks:

- [ ] Map existing operation/capability rules.
- [ ] Define arity, preconditions, postconditions, compatibility, cost, and lossiness only where proven.
- [ ] Align Operation Chain with the minimal construction baseline.
- [ ] Preserve provenance and evidence traceability.
- [ ] Avoid broad workflow-engine expansion.

Completion condition:

```text
Operation/capability rules are explicit enough to govern current derived-dataset work.
```

Evidence:

```text
DataVisualiser_Operation_Capability_Algebra_Slice.md
```

---

## Phase 9 — Typed Relation Slice

Goal:

```text
Make only the necessary relations explicit where hidden relation ambiguity creates risk.
```

Candidate relations:

```text
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

Tasks:

- [ ] Select relation types justified by current implementation pressure.
- [ ] Avoid universal graph machinery unless proven necessary.
- [ ] Add relation rules around pressure points first.
- [ ] Prefer documentation/guardrails before runtime model expansion.

Completion condition:

```text
Important construction relationships are explicit enough to prevent hidden ownership and boundary drift.
```

Evidence:

```text
DataVisualiser_Typed_Relation_Slice.md
```

---

## Phase 10 — Multiplicity / Derived Dataset Slice

Goal:

```text
Formalize N-input, N-operation, N-output derived dataset behavior where Operation Chain proves the need.
```

Tasks:

- [ ] Define input/output/intermediate/derived set semantics.
- [ ] Define arity and cardinality rules.
- [ ] Preserve source provenance per input.
- [ ] Preserve operation trace per step.
- [ ] Preserve lossiness/reversibility metadata.
- [ ] Keep output consumer-neutral before delivery.

Completion condition:

```text
Derived datasets have stable identity, provenance, and trace semantics.
```

Evidence:

```text
DataVisualiser_Derived_Dataset_Multiplicity_Slice.md
```

---

## Phase 11 — Evidence Sufficiency / Promotion Slice

Goal:

```text
Define when generated or derived constructions are promoted, retained, quarantined, or rejected.
```

Tasks:

- [ ] Define evidence sufficiency for current derived outputs.
- [ ] Define promotion/quarantine/rejection states.
- [ ] Keep evidence observational.
- [ ] Do not allow evidence to become live routing.
- [ ] Add audit records only where review value is clear.

Completion condition:

```text
Generated or derived constructions have reviewable evidence status without hidden runtime control.
```

Evidence:

```text
DataVisualiser_Evidence_Sufficiency_Promotion_Slice.md
```

---

## Phase 12 — Semantic / Interpretation / Confidence Slice

Goal:

```text
Support interpretation and confidence without violating canonical semantic authority.
```

Tasks:

- [ ] Distinguish canonical semantics from downstream analytical interpretation.
- [ ] Define confidence as annotation, not correctness.
- [ ] Define assumption records only where current outputs require them.
- [ ] Add contrastive explanation only where useful.
- [ ] Preserve truth layers from downstream mutation.

Completion condition:

```text
Interpretation and confidence are explicit, non-authoritative, and provenance-visible.
```

Evidence:

```text
DataVisualiser_Semantic_Interpretation_Confidence_Slice.md
```

---

## Phase 13 — Analytical Fitness Slice

Goal:

```text
Evaluate whether valid constructions are useful, meaningful, or distortion-prone.
```

Tasks:

- [ ] Define usefulness criteria for one bounded scenario.
- [ ] Define distortion / signal-preservation notes where needed.
- [ ] Avoid generic scoring frameworks until multiple scenarios prove shared shape.
- [ ] Keep validity, usefulness, meaning, explanation, computability, and evidence distinct.

Completion condition:

```text
At least one bounded scenario can distinguish valid execution from useful analytical construction.
```

Evidence:

```text
DataVisualiser_Analytical_Fitness_Slice.md
```

---

## Phase 14 — Computational Planning / Bounded Search Slice

Goal:

```text
Keep generative analytical construction computationally tractable.
```

Tasks:

- [ ] Define bounded-search rules only for a proven scenario.
- [ ] Add cost or pruning rules only where the operation space requires it.
- [ ] Preserve deterministic replay where applicable.
- [ ] Reuse cached/intermediate results only with provenance intact.

Completion condition:

```text
A bounded construction/search scenario is tractable without hiding selection policy.
```

Evidence:

```text
DataVisualiser_Computational_Planning_Slice.md
```

---

## Phase 15 — Generative Multi-Consumer Output Slice

Goal:

```text
Allow generated analytical constructions to serve multiple consumers without creating a new pseudo-core.
```

Tasks:

- [ ] Select one generated output.
- [ ] Serve at least two consumer types if existing seams support it.
- [ ] Preserve consumer-neutral core output.
- [ ] Use consumer-specific projection only downstream.
- [ ] Prevent generated surface models from becoming mega-objects.

Completion condition:

```text
Generated output can serve multiple consumers while preserving one truth discipline.
```

Evidence:

```text
DataVisualiser_Generative_MultiConsumer_Output_Slice.md
```

---

## Phase 16 — Governance / Emergence Review

Goal:

```text
Review whether bounded generativity is improving coherence or creating new entropy.
```

Tasks:

- [ ] Review language growth.
- [ ] Review construction growth.
- [ ] Review pressure-point movement.
- [ ] Review evidence sufficiency.
- [ ] Review whether new abstractions reduce or increase sprawl.
- [ ] Quarantine concepts that lack evidence.
- [ ] Promote only concepts with proven repeated value.

Completion condition:

```text
The system can distinguish healthy emergence from uncontrolled abstraction growth.
```

Evidence:

```text
DataVisualiser_Governance_Emergence_Review.md
```

---

## Phase 17 — Scenario Hardening

Goal:

```text
Harden one bounded product/domain scenario against the formal coverage and construction model.
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

Tasks:

- [ ] Select one scenario.
- [ ] Define scenario boundaries.
- [ ] Validate formal coverage.
- [ ] Validate construction rules.
- [ ] Validate evidence sufficiency.
- [ ] Validate consumer output.
- [ ] Validate governance review.

Completion condition:

```text
One scenario proves bounded generativity without weakening architectural coherence.
```

Evidence:

```text
DataVisualiser_Scenario_Hardening.md
```

---

# Standing Guardrails

## Guardrail 1 — No Premature Algebra

```text
Do not implement construction algebra runtime machinery before density, construction inventory, and coverage-gap triage are complete.
```

## Guardrail 2 — No Replacement Mega-Object

```text
VNextUiConsumptionContract, ConsumerSurfaceModel, ChartRenderPlan, and future construction algebra must not replace ChartDataContext as a new pseudo-core.
```

## Guardrail 3 — Preserve Authority Direction

```text
Canonical meaning remains upstream.
Downstream analytical authority may derive and interpret.
Downstream layers may not redefine canonical meaning, identity, normalization, or CMS truth.
```

## Guardrail 4 — Reduce Before Expanding

```text
The next implementation slice should reduce or contain a current pressure point before adding new formal layers.
```

## Guardrail 4.1 — Finite Local Cleanup

```text
Do not stay indefinitely in pressure-reduction mode.
Repeat Phases 5–6 for 2–3 iterations where useful.
Do not exceed 4 iterations without explicit architectural review.
If local slices stop reducing risk, proceed to formalisation or accept the seam as bounded.
```

## Guardrail 5 — Evidence Remains Observational

```text
Evidence, diagnostics, parity, reachability, validation, and audit may prove behavior.
They must not become hidden runtime routing or provider-selection mechanisms.
```

## Guardrail 6 — Bridge Retirement Discipline

```text
Replacement first.
Parity/smoke/metadata/provenance evidence second.
One bridge retirement at a time.
No bulk bridge removal.
```

## Guardrail 7 — Concept Coverage Discipline

```text
Do not implement a missing language term merely because it is missing.
A new formal type requires owner, boundary, input/output shape, evidence path, and explicit non-goals.
```

---

# Completion Definition

The next-stage migration is complete when:

```text
current density increase is classified
existing construction set is inventoried
coverage gaps are triaged
at least one current pressure point is reduced or contained
Phases 5–6 are completed within the finite iteration limit or escalated to architectural review
minimal construction algebra exists only where justified
Operation Chain pressure has informed the formal model
bounded generativity is supported without new pseudo-core creation
evidence remains observational
canonical authority remains upstream
```

No closure claim is valid unless supported by:

```text
tests
dependency review
parity/smoke evidence where applicable
metadata/provenance preservation
updated documentation
guardrail checks
```

---

# Historical Footnotes

## Footnote 1 — Completed Structural Spine Work

Previous Phases 1-23 completed baseline establishment, density classification, authority/provenance/fidelity locking, hub containment, boundary/contract hardening, projection containment, consumer/interaction thinning, surface-model elevation, terminal-delivery demotion, evidence observability, governance constraints, capability expansion, non-chart consumer proof, selective bypass retirement, family-pattern consolidation, hub responsibility migration, adapter thinning, seam classification, end-to-end spine proof, and composition-root cleanup.

Historical details remain in prior audit files and generated artifacts.

## Footnote 2 — Completed Consumption / Convergence Work

Previous Phases 24-35 completed ChartDataContext audit, VNext-native UI consumption contract, Operation Chain MVP, production-family migration, bridge-retirement patterns, family migration tracking, surface-model convergence, UI/delivery thinning, capability/contract consolidation, remaining legacy-bypass retirement, and final convergence audit.

Closure was accepted under a bounded-compatibility clause, not as full elimination of all residual bridges.

## Footnote 3 — Completed Requirements-to-Language Coverage

Previous Phase 36 produced requirements-to-language coverage and recorded formal gaps, collapsed concerns, and ambiguity pressure.

The result is an input to this recalibrated plan, not permission for immediate broad runtime algebra.

## Footnote 4 — Historical Evidence Use

Historical audit files are source evidence only.

They should not clutter current execution.

Use them only when investigating prior decisions, not as active phase instructions.
