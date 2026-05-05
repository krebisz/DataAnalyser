# DataVisualiser Migration Plan and Guardrails — Agent-Sequential Optimized Checklist Version

## Purpose

Implementation-facing execution document for the next stage of `DataVisualiser` work.

This document is aligned to the `DataVisualiser Architectural Vocabulary — Derivational Foundational Grammar` and exists to turn the target architecture into a near-sequential implementation path for an AI agent and human driver.

Use this document for:

```text
AI IDE-agent execution
human review
sequential implementation tracking
evidence-backed completion
```

This document does **not** govern the wider `DataAnalyser` platform except where upstream context affects `DataVisualiser`.

Tracking rule:

```text
Tasks are intentionally represented as unchecked markdown checkboxes.
Do not remove the checkboxes during future optimization or compaction.
```

---

## 1. Document Authority

```text
Architectural Vocabulary:
  owns concept meaning, derivational grammar, ownership containers,
  Prime Directive, do-not-collapse algebra, target architecture graph,
  expanded responsibility lattice, abstract code-map direction, and
  current-vs-target conceptual responsibility placement.

Migration Plan:
  owns execution order, implementation phases, task scope,
  pressure-reduction slices, guardrails, implementation evidence,
  closure records, and AI-agent working sequence.
```

Conflict rule:

```text
Concept meaning / responsibility placement / blueprint layer ownership -> architectural vocabulary wins.
Execution order / current state / implementation sequencing -> migration plan wins.
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
MainChartsView, SyncfusionChartsView, ChartControllerFactory*,
ChartRenderingOrchestrator, MetricLoadCoordinator, and repeated family
contract/builder/route/qualification patterns remain pressure points.
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
The next work must classify, reduce, contain, or formally govern pressure
before broad new runtime machinery is introduced.
```

---

## 3. Implementation Chain to Preserve

The implementation must preserve the architectural authority direction and keep `Projection / Translation` explicit before terminal delivery.

```text
source / selected metric input
-> authoritative request / snapshot / provenance carriers
-> semantic formation / analytical intent
-> reasoning / capability / construction result
-> program / execution result
-> contract / boundary / qualification
-> consumer-neutral surface model / ChartRenderPlan
-> projection / translation / adapter payload
-> qualified terminal delivery adapter
-> terminal rendering, export, API, report, or other consumer surface
-> observational evidence / diagnostics / audit
```

Current code-facing expressions:

```text
MetricSelectionRequest / MetricSeriesRequest
-> MetricLoadSnapshot / MetricSeriesSnapshot / ProvenanceDescriptor
-> AnalyticalIntent
-> CapabilityRequest / ChartProgramRequest / ChartProgram
-> AnalyticalExecutionResult / AnalyticalResultSet
-> VNextUiConsumptionContract / ConsumerDeliveryContract / ConsumerProviderContract
-> ConsumerSurfaceModel / ChartRenderPlan
-> ChartRenderPlanProjector / ChartRenderDeliveryBinding / adapter qualification
-> ChartRenderAdapterResult / terminal delivery surface
-> evidence / diagnostics / audit export
```

Rules:

```text
Do not replace this chain with a new mega-object.
Do not let Projection / Translation create meaning.
Do not let Terminal Delivery select semantic truth.
Do not let Evidence route live behavior.
```

---

## 4. Target Blueprint Alignment Map

Every implementation slice must name the blueprint layer it touches and whether the work affects what that layer owns, governs, emits, or observes.

| Blueprint layer | Current code expressions | Implementation intent | Closure signal |
|---|---|---|---|
| Canonical Source / Input | metric load gateways, selected metric paths, `MetricLoadSnapshotGateway`, source/snapshot entry points | preserve source/input identity before reasoning | source/input assumptions remain visible, non-render-owned, and not hidden inside UI state |
| Authority Substrate | `MetricSelectionRequest`, `MetricSeriesRequest`, `MetricLoadSnapshot`, `MetricSeriesSnapshot`, `ProvenanceDescriptor`, preservation metadata | keep canonical meaning, provenance, fidelity, and loss declaration upstream | no downstream truth mutation, metadata loss, or provenance-erasing projection |
| Semantic Formation | semantic metric selection, metric identity, meaning qualification, relationship assumptions, `AnalyticalIntentFactory` | separate meaning from labels, charts, UI terms, and delivery-specific names | semantic identity is not owned by UI, renderer, adapter, or vendor code |
| Reasoning / Capability | `CapabilityRequest`, analytical capability contracts, strategy outputs, transformations, Operation Chain | express reusable analytical power independent of feature delivery | capability does not collapse into chart feature behavior or controller flow |
| Construction Algebra | Operation Chain, derived datasets, operation traces, relation candidates, construction/evidence records | formalize only construction concepts justified by inventory and pressure evidence | minimal algebra exists without broad runtime graph/workflow sprawl |
| Execution / Process | `ChartProgram`, `ChartProgramPlanner`, coordinators, orchestrators, resolvers, selectors, registries, explicit policy candidates | coordinate execution without becoming semantic authority | orchestration does not create truth, hidden policy, provider authority, or delivery semantics |
| Contract / Boundary | `VNextUiConsumptionContract`, `ConsumerDeliveryContract`, `ConsumerProviderContract`, compatibility proofs, metadata preservation rules, qualification seams | enforce handoff shape, neutrality, metadata preservation, and qualification | qualified contract/boundary handoff exists before consumer or delivery adaptation |
| Consumer Model | `ConsumerSurfaceModel`, `ChartRenderPlan`, consumer contracts, interaction/explanation payloads, state relay paths | let consumers receive meaning without redefining it | no `ChartDataContext`, render model, or consumer state carrier becomes a new pseudo-core |
| Projection / Translation | `LegacyChartProgramProjector`, `ChartRenderPlanProjector`, `VNextDataResolutionHelper`, `LegacyMetricViewGateway`, target-to-consumer mappings, consumer-to-delivery mappings | translate valid meaning without creating new meaning | projection remains non-authoritative, loss-aware, provenance-preserving, and bridge-bounded |
| Terminal Delivery | renderers, `ChartRenderAdapterResult`, Syncfusion/WPF/LiveCharts/ECharts host surfaces, backend/vendor delivery adapters | keep delivery terminal, replaceable, and vendor-contained | vendor/runtime assumptions remain downstream of contract, consumer, and projection seams |
| Evidence / Diagnostics | parity, validation, reachability, diagnostics, audit records, evidence exports | observe, prove, compare, and record behavior only | evidence does not route live behavior, select providers, or define semantic truth |
| Governance / Control Plane | gap triage, promotion/quarantine decisions, drift review, architecture review records, guardrail checks | control abstraction growth and concept promotion | promoted concepts have evidence, explicit ownership, and no hidden runtime branching |

Alignment rule:

```text
No implementation slice should proceed unless its touched blueprint layer,
ownership zone, derivational vocabulary term, do-not-collapse risk, and closure
signal are named.
```

Derivational trace rule:

```text
Every slice must be traceable from pressure point or requirement to:
  vocabulary term
  promoted concept or collision risk
  blueprint layer
  implementation action
  evidence / closure signal
```

---

## 5. Current Pressure Points

```text
ChartDataContext
LegacyChartProgramProjector / compatibility projected contexts
VNextDataResolutionHelper selected-series fallback
LegacyMetricViewGateway compatibility gateway
service-backed metric loading through MetricLoadSnapshotGateway
strategy cut-over compatibility
MainChartsView / SyncfusionChartsView host gravity
ChartControllerFactory / ChartControllerFactoryContext fan-out
ChartRenderingOrchestrator / ChartUpdateCoordinator concentration
MetricLoadCoordinator VNext/legacy route pressure
ChartRenderPlanProjector / ChartRenderDeliveryBinding / adapter qualification seam
family contract / builder / route / qualification repetition
EvidenceDiagnosticsBuilder expansion risk
```

Blueprint-layer pressure grouping:

```text
Canonical Source / Authority pressure:
  MetricLoadSnapshotGateway
  source/snapshot assumptions embedded in UI or delivery paths
  provenance loss through compatibility projection

Semantic Formation / Reasoning pressure:
  strategy cut-over compatibility
  Operation Chain capability pressure
  semantic metric selection carried by UI-facing paths

Consumer Model / Terminal Delivery pressure:
  ChartDataContext
  MainChartsView
  SyncfusionChartsView
  chart-family adapters

Projection / Translation pressure:
  LegacyChartProgramProjector
  compatibility projected contexts
  VNextDataResolutionHelper selected-series fallback
  LegacyMetricViewGateway
  ChartRenderPlanProjector

Execution / Process pressure:
  ChartControllerFactory / ChartControllerFactoryContext
  ChartRenderingOrchestrator
  ChartUpdateCoordinator
  MetricLoadCoordinator
  MetricLoadSnapshotGateway

Contract / Boundary pressure:
  family contract repetition
  builder / route / qualification repetition
  strategy cut-over compatibility
  ChartRenderDeliveryBinding / adapter qualification seam

Delivery / Vendor pressure:
  Syncfusion / WPF / LiveCharts / ECharts delivery surfaces
  backend/vendor assumptions leaking into upstream contracts

Evidence / Diagnostics pressure:
  EvidenceDiagnosticsBuilder expansion risk
  parity / reachability / validation support growth
```

Pressure interpretation rule:

```text
A pressure point is not automatically a violation.
It becomes implementation priority when it hides authority, collapses distinct
vocabulary terms, bypasses a boundary, mutates projection into meaning,
creates a pseudo-core, or prevents evidence-visible closure.
```

---

# Execution Tracking Dashboard

Use this as the visible implementation checklist. Only one stage and one slice should be active at a time.

- [x] Stage 0 — Prepare working baseline
- [x] Stage 1 — Rebaseline and classify density
- [x] Stage 2 — Inventory construction and coverage
- [x] Stage 3 — Rank pressure points by architectural leverage
- [x] Stage 4 — Execute one pressure-reduction slice
- [x] Stage 5 — Reassess the slice
- [x] Stage 6 — Define minimal construction algebra baseline
- [x] Stage 7 — Formalize operation / capability algebra slice
- [x] Stage 8 — Formalize multiplicity / derived dataset slice
- [x] Stage 9 — Formalize typed relation slice
- [x] Stage 10 — Define evidence sufficiency / promotion slice
- [x] Stage 11 — Define semantic / interpretation / confidence slice
- [x] Stage 12 — Define analytical fitness slice
- [x] Stage 13 — Define computational planning / bounded search slice
- [x] Stage 14 — Validate generative multi-consumer output slice
- [x] Stage 15 — Perform governance / emergence review
- [x] Stage 16 — Harden one bounded scenario

Active-slice tracker:

- [ ] Not started
- [ ] Active
- [x] Complete
- [ ] Blocked

| Field | Value |
|---|---|
| Active stage | Stage 16 complete; plan implementation pass complete |
| Active pressure point / construction concern | Operation Chain Workbench scenario hardening |
| Blueprint layer(s) | Reasoning / Capability; Construction Algebra; Contract / Boundary; Consumer Model; Evidence / Diagnostics; Governance / Control Plane |
| Ownership zone(s) | Governs / Emits / Observes |
| Slice charter file | Inline only |
| Required evidence | `DataVisualiser.Tests` 1051 passed; `DataFileReader.Tests` 15 passed |
| Stop condition | Scenario requires broader platform concerns outside DataVisualiser |

---

# Agent Execution Protocol

## A. One Active Slice Rule

```text
Only one implementation slice may be active at a time.
A slice may touch multiple files, but it must reduce, contain, or formalize one named pressure point or one justified construction concern.
```

## B. Slice Charter Required Before Code Change

Before changing code, the AI agent must complete the slice-charter checklist:

- [ ] Pressure / requirement named
- [ ] Vocabulary term identified
- [ ] Do-not-collapse risk identified
- [ ] Blueprint layer named
- [ ] Ownership zone named
- [ ] Intended action declared
- [ ] Non-goals declared
- [ ] Expected tests/evidence declared
- [ ] Stop condition declared

The table below defines the required content for each item:

| Field | Required content |
|---|---|
| Pressure / requirement | named source pressure or construction need |
| Vocabulary term | target vocabulary term from the architecture document |
| Do-not-collapse risk | e.g. Projection != Transformation, Evidence != Control |
| Blueprint layer | touched layer(s) |
| Ownership zone | Owns / Governs / Emits / Observes |
| Intended action | reduce / contain / formalize / defer / guardrail |
| Non-goals | what the slice must not attempt |
| Expected tests/evidence | tests, dependency check, parity/smoke, provenance check |
| Stop condition | exact point where the agent must stop and report |

## C. Agent Stop Conditions

The AI agent must stop and report rather than improvise if:

```text
the slice starts requiring a second pressure point
projection begins creating meaning
evidence starts influencing live routing
a UI/vendor type must move upstream
ChartDataContext or any successor starts absorbing unrelated responsibilities
construction algebra requires broad runtime machinery
test failures indicate behavior change beyond the slice charter
```

---

# Optimized Sequential Implementation Plan

## Stage 0 — Prepare the Working Baseline

Goal:

```text
Establish the exact code/document state the next implementation slice will operate against.
```

Tasks:

- [x] Confirm the active branch, solution build state, and test baseline.
- [x] Confirm the current architecture document and this migration plan are the active pair.
- [x] Regenerate or locate:
   - [x] project tree
   - [x] codebase index
   - [x] dependency summary
   - [x] type dependency diagram
- [x] Record the baseline artifact names and timestamps.
- [x] Do not change production code in this stage.

Completion condition:

```text
The agent and human driver know exactly which code/document/artifact baseline is active.
```

Evidence:

```text
DataVisualiser_Working_Baseline.md
```

Stop condition:

```text
Stop if the codebase, generated artifacts, or active documents do not match the expected workspace.
```

---

## Stage 1 — Rebaseline and Density Classification

Goal:

```text
Classify density before choosing any implementation slice.
```

Tasks:

- [ ] Compare latest dependency evidence against the previous known baseline.
- [ ] Classify density by:
   - [ ] blueprint layer
   - [ ] ownership zone
   - [ ] derivational vocabulary term
   - [ ] compound pattern
   - [ ] promoted concept or collision risk
- [ ] Classify each significant density cluster as:
   - [ ] necessary target-grammar density
   - [ ] transitional bridge density
   - [ ] UI/state gravity
   - [ ] delivery/vendor gravity
   - [ ] evidence/diagnostic support density
   - [ ] scaffolding debt
   - [ ] accidental coupling
   - [ ] unknown
- [ ] Identify top incoming hubs.
- [ ] Identify top outgoing sources.
- [ ] Mark each density cluster as acceptable, bounded, reducible, unknown, or violating.

Completion condition:

```text
Density increase is classified, not merely counted.
```

Evidence:

```text
DataVisualiser_Density_Rebaseline.md
```

Stop condition:

```text
Stop if density cannot be classified without source inspection.
Do not proceed by raw edge count alone.
```

---

## Stage 2 — Construction and Coverage Inventory

Goal:

```text
Know what already exists before adding formal algebra or new abstractions.
```

Tasks:

- [ ] Inventory current construction expressions:
   - [ ] Operation Chain
   - [ ] VNextUiConsumptionContract
   - [ ] ConsumerSurfaceModel
   - [ ] ChartRenderPlan
   - [ ] ChartRenderPlanProjector
   - [ ] ChartRenderDeliveryBinding
   - [ ] ChartRenderPlanAdapterQualification
   - [ ] provider metadata / vocabulary metadata carriers
   - [ ] IAnalyticalCapabilityContract
   - [ ] evidence traces
   - [ ] projection bridges
   - [ ] LegacyMetricViewGateway compatibility path
   - [ ] strategy cut-over
   - [ ] metric-load gateway paths
- [ ] Map each construction to:
   - [ ] vocabulary term
   - [ ] do-not-collapse risk
   - [ ] blueprint layer
   - [ ] Owns / Governs / Emits / Observes zone
- [ ] Classify each construction as:
   - [ ] stable
   - [ ] transitional
   - [ ] overloaded
   - [ ] under-expressed
   - [ ] candidate for consolidation
   - [ ] candidate for retirement
- [ ] Triage coverage terms:
   - [ ] Confidence
   - [ ] Envelope
   - [ ] Interaction
   - [ ] Policy
   - [ ] Binding
   - [ ] Registry
   - [ ] Lifecycle
   - [ ] Interpretation
   - [ ] Semantics
   - [ ] Provenance
   - [ ] Traceability
   - [ ] Fidelity
   - [ ] Reversibility
   - [ ] Determinism
   - [ ] Constraint
   - [ ] Neutrality
   - [ ] Qualification
   - [ ] SurfaceModel
- [ ] For each gap, decide:
   - [ ] implement now
   - [ ] express using existing construct
   - [ ] defer
   - [ ] guardrail only
   - [ ] reject as premature
- [ ] Review collapsed / ambiguous pairs:
   - [ ] Contract / Capability
   - [ ] SurfaceModel / Consumer
   - [ ] Projection / Adapter
   - [ ] Evidence / Validation
   - [ ] Transformation / Projection
   - [ ] Provider / Backend
   - [ ] Delivery / Consumer
   - [ ] Semantics / Interpretation
   - [ ] Evidence / Audit

Completion condition:

```text
The existing construction set and coverage gaps are known before formal construction work begins.
```

Evidence:

```text
DataVisualiser_Construction_and_Coverage_Inventory.md
```

Stop condition:

```text
Stop if a named vocabulary gap is being treated as automatic permission to create runtime code.
```

---

## Stage 3 — Rank Pressure Points by Architectural Leverage

Goal:

```text
Select the next implementation slice by evidence and architectural leverage, not convenience.
```

Tasks:

- [ ] Build one combined pressure matrix containing:
   - [ ] bridge-reduction candidates
   - [ ] hub-responsibility candidates
   - [ ] contract/boundary candidates
   - [ ] consumer pseudo-core candidates
   - [ ] projection/translation candidates
   - [ ] evidence/diagnostic candidates
- [ ] Score each candidate by:
   - [ ] severity
   - [ ] feasibility
   - [ ] testability
   - [ ] blast radius
   - [ ] architectural leverage
   - [ ] rollback simplicity
   - [ ] dependency on other slices
- [ ] Apply leverage priority order:
   - [ ] authority leakage
   - [ ] contract / boundary collapse
   - [ ] consumer pseudo-core risk
   - [ ] projection creating meaning
   - [ ] execution becoming policy or authority
   - [ ] delivery / vendor leakage upstream
   - [ ] evidence becoming live routing
   - [ ] general density reduction
- [ ] Select exactly one slice.
- [ ] Record why higher-ranked candidates were not selected, if applicable.
- [ ] Produce the slice charter.

Completion condition:

```text
One implementation slice is selected with explicit architectural reason.
```

Evidence:

```text
DataVisualiser_Pressure_Leverage_Matrix.md
DataVisualiser_Selected_Slice_Charter.md
```

Stop condition:

```text
Stop if no candidate has enough evidence to justify implementation.
```

---

## Stage 4 — Execute One Pressure-Reduction Slice

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
no silent authority movement
```

Tasks:

- [ ] Define affected files/classes.
- [ ] Define exact non-goals.
- [ ] Add or identify behavior-preserving tests.
- [ ] Implement the smallest safe reduction or containment.
- [ ] Preserve the target chain, including explicit Projection / Translation before Terminal Delivery.
- [ ] Confirm projection remains non-authoritative and preserves or declares metadata/provenance loss.
- [ ] Confirm evidence remains observational.
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

Stop condition:

```text
Stop if the slice requires a second pressure point, a broad abstraction, or unplanned behavior change.
```

---

## Stage 5 — Post-Slice Reassessment

Goal:

```text
Confirm whether the slice improved architecture or merely moved coupling.
```

Tasks:

- [x] Compare before/after dependency evidence.
- [x] Confirm no new authority leakage.
- [x] Confirm no new UI/state pseudo-core.
- [x] Confirm no new vendor/delivery leakage upstream.
- [x] Confirm target chain remains intact.
- [x] Confirm Projection / Translation and Terminal Delivery remain distinct.
- [x] Confirm no projection, adapter, resolver, or selector became hidden policy or semantic authority.
- [x] Confirm evidence remains observational.
- [x] Confirm the blueprint layer responsibility was improved, reduced, or more clearly bounded.
- [x] Decide one outcome:
    - [ ] repeat Stage 3–5 for another pressure slice
    - [x] proceed to Stage 6 formal construction baseline
    - [ ] pause for manual architecture review

Completion condition:

```text
The next move is justified by evidence.
```

Evidence:

```text
DataVisualiser_Post_Slice_Reassessment.md
```

Iteration rule:

```text
Repeat Stages 3–5 for 2–3 pressure-reduction slices where useful.
Do not exceed 4 slices without explicit architecture review.
```

Proceed to Stage 6 when:

```text
the remaining pressure requires formal construction language rather than local cleanup
```

Stop condition:

```text
Stop if the same pressure class remains unresolved after 3 focused slices.
```

---

## Stage 6 — Minimal Construction Algebra Baseline

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

- [x] Select only justified terms from Stage 2 and Stage 5 evidence.
- [x] Record which grammar pool and promoted concept justified each selected term.
- [x] Confirm each term belongs in Construction Algebra rather than Execution, Contract, Consumer, Projection, Delivery, or Evidence.
- [x] Define minimal contracts/records/interfaces only where necessary.
- [x] Define non-goals.
- [x] Reuse existing contracts/surfaces where possible.
- [x] Add guardrail tests where meaningful.
- [x] Avoid production rewiring unless required by the selected slice.

Completion condition:

```text
A minimal construction-algebra baseline exists without unnecessary runtime complexity.
```

Evidence:

```text
DataVisualiser_Minimal_Construction_Algebra_Baseline.md
focused tests if code changes
```

Stop condition:

```text
Stop if the baseline starts becoming a workflow engine, graph engine, or replacement pseudo-core.
```

---

## Stage 7 — Operation / Capability Algebra Slice

Goal:

```text
Formalize analytical capability as lawful composable power where Operation Chain and capability contracts prove the need.
```

Tasks:

- [x] Map existing operation/capability rules.
- [x] Define arity, preconditions, postconditions, compatibility, cost, and lossiness only where proven.
- [x] Align Operation Chain with the minimal construction baseline and Reasoning / Capability layer.
- [x] Preserve provenance, traceability, reversibility/lossiness, and evidence visibility.
- [x] Avoid broad workflow-engine expansion.

Completion condition:

```text
Operation/capability rules are explicit enough to govern current derived-dataset work.
```

Evidence:

```text
DataVisualiser_Operation_Capability_Algebra_Slice.md
```

Stop condition:

```text
Stop if capability logic starts becoming chart feature behavior or controller flow.
```

---

## Stage 8 — Multiplicity / Derived Dataset Slice

Goal:

```text
Formalize N-input, N-operation, N-output derived dataset behavior where Operation Chain proves the need.
```

Tasks:

- [x] Define input/output/intermediate/derived set semantics.
- [x] Define arity and cardinality rules.
- [x] Preserve source provenance per input.
- [x] Preserve operation trace per step.
- [x] Preserve lossiness/reversibility metadata.
- [x] Keep output consumer-neutral before delivery.

Completion condition:

```text
Derived datasets have stable identity, provenance, and trace semantics.
```

Evidence:

```text
DataVisualiser_Derived_Dataset_Multiplicity_Slice.md
```

Stop condition:

```text
Stop if derived output skips consumer-neutral contracts or enters delivery-specific form too early.
```

---

## Stage 9 — Typed Relation Slice

Goal:

```text
Make necessary relations explicit where hidden relation ambiguity creates risk.
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

- [x] Select relation types justified by current pressure and derived-dataset work.
- [x] Keep `ProjectionRelation` and transformation/derivation relations distinct.
- [x] Add relation rules around pressure points first.
- [x] Prefer documentation/guardrails before runtime model expansion.
- [x] Avoid universal graph machinery unless proven necessary.

Completion condition:

```text
Important construction relationships are explicit enough to prevent hidden ownership and boundary drift.
```

Evidence:

```text
DataVisualiser_Typed_Relation_Slice.md
```

Stop condition:

```text
Stop if relation modeling becomes general-purpose graph infrastructure without proven pressure.
```

---

## Stage 10 — Evidence Sufficiency / Promotion Slice

Goal:

```text
Define when generated or derived constructions are promoted, retained, quarantined, or rejected.
```

Tasks:

- [x] Define evidence sufficiency for current derived outputs as Evidence / Diagnostics output, not live routing policy.
- [x] Define promotion/quarantine/rejection states.
- [x] Add audit records only where review value is clear.
- [x] Keep evidence observational.
- [x] Prohibit evidence from provider selection or live routing.

Completion condition:

```text
Generated or derived constructions have reviewable evidence status without hidden runtime control.
```

Evidence:

```text
DataVisualiser_Evidence_Sufficiency_Promotion_Slice.md
```

Stop condition:

```text
Stop if evidence begins influencing live execution decisions outside explicit named policy.
```

---

## Stage 11 — Semantic / Interpretation / Confidence Slice

Goal:

```text
Support interpretation and confidence without violating canonical semantic authority.
```

Tasks:

- [ ] Distinguish Authority Substrate semantics, Semantic Formation meaning, and downstream analytical interpretation.
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

Stop condition:

```text
Stop if interpretation or confidence mutates truth, validity, or canonical semantic identity.
```

---

## Stage 12 — Analytical Fitness Slice

Goal:

```text
Evaluate whether valid constructions are useful, meaningful, or distortion-prone.
```

Tasks:

- [ ] Select one bounded scenario.
- [ ] Define usefulness criteria in the Reasoning / Capability layer.
- [ ] Define distortion / signal-preservation notes where needed.
- [ ] Keep validity, usefulness, meaning, explanation, computability, and evidence distinct.
- [ ] Avoid generic scoring frameworks until multiple scenarios prove shared shape.

Completion condition:

```text
At least one bounded scenario can distinguish valid execution from useful analytical construction.
```

Evidence:

```text
DataVisualiser_Analytical_Fitness_Slice.md
```

Stop condition:

```text
Stop if usefulness scoring becomes generic policy without scenario proof.
```

---

## Stage 13 — Computational Planning / Bounded Search Slice

Goal:

```text
Keep generative analytical construction computationally tractable.
```

Tasks:

- [ ] Define bounded-search rules only for a proven scenario.
- [ ] Keep search and pruning rules in explicit Process / Execution policy.
- [ ] Add cost or pruning rules only where the operation space requires it.
- [ ] Preserve deterministic replay where applicable.
- [ ] Reuse cached/intermediate results only with provenance intact.

Completion condition:

```text
A bounded construction/search scenario is tractable without hidden selection policy.
```

Evidence:

```text
DataVisualiser_Computational_Planning_Slice.md
```

Stop condition:

```text
Stop if bounded search becomes hidden resolver/selector policy.
```

---

## Stage 14 — Generative Multi-Consumer Output Slice

Goal:

```text
Allow generated analytical constructions to serve multiple consumers without creating a new pseudo-core.
```

Tasks:

- [ ] Select one generated output.
- [ ] Serve at least two consumer types if existing seams support it.
- [ ] Preserve consumer-neutral core output through Contract / Boundary and Consumer Model layers.
- [ ] Use consumer-specific projection only downstream.
- [ ] Prevent generated surface models from replacing `ChartDataContext` as a new mega-object.

Completion condition:

```text
Generated output can serve multiple consumers while preserving one truth discipline.
```

Evidence:

```text
DataVisualiser_Generative_MultiConsumer_Output_Slice.md
```

Stop condition:

```text
Stop if multi-consumer support starts centralizing unrelated concerns into one universal model.
```

---

## Stage 15 — Governance / Emergence Review

Goal:

```text
Review whether bounded generativity is improving coherence or creating new entropy.
```

Tasks:

- [ ] Review language growth against the Governance / Control Plane and blueprint layers.
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

Stop condition:

```text
Stop if governance decisions cannot be traced to evidence and ownership rules.
```

---

## Stage 16 — Scenario Hardening

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
- [ ] Validate formal coverage against the expanded responsibility lattice.
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

Stop condition:

```text
Stop if the scenario requires broader platform concerns that belong to DataAnalyser rather than DataVisualiser.
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
Reasoning / Capability may derive, compose, transform, and interpret under explicit provenance and assumptions.
Process, Contract, Consumer, Projection, Delivery, and Evidence layers may carry, qualify, project, deliver, or observe meaning.
They may not redefine canonical meaning, identity, normalization, or source truth.
```

## Guardrail 4 — Reduce Before Expanding

```text
The next implementation slice should reduce or contain a current pressure point before adding new formal layers.
```

## Guardrail 5 — Architectural-Leverage First

```text
Pressure reduction must be ranked by architectural leverage before implementation begins.
Prefer slices that remove authority leakage, boundary collapse, consumer pseudo-core risk, projection-as-meaning, execution-as-policy, vendor leakage, or evidence-as-routing before general density cleanup.
Feasibility matters, but an easy cleanup must not displace a higher-risk architectural correction without a recorded reason.
```

## Guardrail 6 — Finite Local Cleanup

```text
Do not stay indefinitely in pressure-reduction mode.
Repeat Stages 3–5 for 2–3 slices where useful.
Do not exceed 4 slices without explicit architectural review.
If local slices stop reducing risk, proceed to formalisation or accept the seam as bounded.
```

## Guardrail 7 — Evidence Remains Observational

```text
Evidence, diagnostics, parity, reachability, validation, and audit may prove behavior.
They must not become hidden runtime routing or provider-selection mechanisms.
```

## Guardrail 8 — Bridge Retirement Discipline

```text
Replacement first.
Parity/smoke/metadata/provenance evidence second.
One bridge retirement at a time.
No bulk bridge removal.
```

## Guardrail 9 — Concept Coverage Discipline

```text
Do not implement a missing language term merely because it is missing.
A new formal type requires owner, boundary, input/output shape, evidence path, and explicit non-goals.
```

## Guardrail 10 — Blueprint Layer Discipline

```text
Every implementation slice must name the blueprint layer it touches.
Every new or changed type must have a clear responsibility zone: Owns, Governs, Emits, or Observes.
Every slice must name its derivational vocabulary term and the do-not-collapse risk it avoids.
No type may silently span Authority, Capability, Contract, Consumer, Projection, Delivery, and Evidence concerns.
A cross-layer type is allowed only as an explicit envelope, contract, manifest, trace, or record with non-goals stated.
```

## Guardrail 11 — Projection / Translation Non-Authority

```text
Projection, adapters, resolvers, selectors, and compatibility bridges may translate, bind, qualify, or apply already-defined output.
They must not create analytical meaning, mutate confidence, discard provenance silently, select providers through hidden policy, or become the new route by which legacy truth re-enters the target spine.
Any projection slice must declare source shape, target shape, preserved metadata, known loss, and retirement/containment intent.
```

## Guardrail 12 — Derivational Traceability

```text
A change is not architecture-aligned merely because it uses target vocabulary.
It must be traceable from observed pressure or requirement to vocabulary term, promoted concept, collision risk, ownership container, blueprint layer, implementation action, and closure evidence.
```

---

# Completion Definition

The next-stage migration is complete when:

```text
working baseline is confirmed
current density increase is classified
existing construction set is inventoried
coverage gaps are triaged
at least one current pressure point is reduced or contained
the selected pressure-reduction slice was ranked by architectural leverage before implementation
Stages 3–5 are completed within the finite iteration limit or escalated to architectural review
minimal construction algebra exists only where justified
Operation Chain pressure has informed the formal model
bounded generativity is supported without new pseudo-core creation
evidence remains observational
canonical authority remains upstream
implementation slices map cleanly to the expanded target architecture blueprint
Projection / Translation remains explicit, non-authoritative, and loss/provenance-aware
implementation decisions preserve derivational traceability from vocabulary to code slice
```

No closure claim is valid unless supported by:

```text
tests
dependency review
parity/smoke evidence where applicable
metadata/provenance preservation
blueprint-layer alignment check
derivational traceability check
projection / translation non-authority check
architectural-leverage ranking record
updated documentation
guardrail checks
```

---

# Historical Footnotes

## Footnote 1 — Completed Structural Spine Work

Previous phases completed baseline establishment, density classification, authority/provenance/fidelity locking, hub containment, boundary/contract hardening, projection containment, consumer/interaction thinning, surface-model elevation, terminal-delivery demotion, evidence observability, governance constraints, capability expansion, non-chart consumer proof, selective bypass retirement, family-pattern consolidation, hub responsibility migration, adapter thinning, seam classification, end-to-end spine proof, and composition-root cleanup.

Historical details remain in prior audit files and generated artifacts.

## Footnote 2 — Completed Consumption / Convergence Work

Previous convergence work completed ChartDataContext audit, VNext-native UI consumption contract, Operation Chain MVP, production-family migration, bridge-retirement patterns, family migration tracking, surface-model convergence, UI/delivery thinning, capability/contract consolidation, remaining legacy-bypass retirement, and final convergence audit.

Closure was accepted under a bounded-compatibility clause, not as full elimination of all residual bridges.

## Footnote 3 — Completed Requirements-to-Language Coverage

Previous requirements-to-language coverage produced formal gaps, collapsed concerns, and ambiguity pressure.

The result is an input to this recalibrated plan, not permission for immediate broad runtime algebra.

## Footnote 4 — Historical Evidence Use

Historical audit files are source evidence only.

They should not clutter current execution.

Use them only when investigating prior decisions, not as active implementation instructions.
