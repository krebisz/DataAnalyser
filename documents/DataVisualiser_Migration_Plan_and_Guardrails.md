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
LoadedChartDataSnapshot now carries loaded-state diagnostics/milestone facts
beside ChartDataContext, reducing but not eliminating ChartDataContext gravity.
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
- [x] Stage 17 — Protect loaded-state split
- [x] Stage 18 — Generalize series resolution
- [x] Stage 19 — Cut over Distribution family input
- [x] Stage 20 — Cut over Weekday Trend family input
- [x] Stage 21 — Align Transform input with Operation Chain concepts
- [x] Stage 22 — Cut over Main / Cartesian rendering inputs
- [ ] Stage 23 — Retire ChartDataContext as shared state carrier (paused after first slice)
- [ ] Stage 24 — Retire legacy projection / compatibility paths
- [ ] Stage 25 — Simplify orchestration and composition hubs
- [ ] Pivot A — Bounded behavior expansion through target architecture
- [ ] Stage 26 — Productize bounded Operation Chain / Transform convergence
- [ ] Stage 27 — Complete terminal delivery and evidence separation
- [ ] Stage 28 — Final bridge retirement pass
- [ ] Stage 29 — Final vocabulary / architecture closure review

Active-slice tracker:

- [ ] Not started
- [x] Active
- [ ] Complete
- [ ] Blocked

| Field | Value |
|---|---|
| Active stage | Pivot A active |
| Active pressure point / construction concern | Operation Chain now begins behavior expansion by loading independent metric/submetric inputs into grids through the target metric snapshot path |
| Blueprint layer(s) | Canonical Source / Input; Reasoning / Capability; Transformation / Projection; Consumer Surface; Evidence / Diagnostics |
| Ownership zone(s) | Emits / Governs / Transforms / Delivers / Observes |
| Slice charter file | Inline only |
| Required evidence | Pivot A active: Operation Chain input grids implemented through `MetricSelectionRequest` / `MetricLoadSnapshotGateway`; input date range refresh follows selected metric/submetric inputs; focused Operation Chain / architecture tests 114 passed; `DataVisualiser.Tests` 1074 passed; `DataFileReader.Tests` 15 passed; manual smoke pending |
| Stop condition | Behavior expansion requires bypassing canonical meaning, provenance, capability contracts, consumer-neutral output, or the existing target-spine boundaries |

## Current Pivot Note — 2026-05-08

```text
Pause the bridge-removal/refit track after the completed Stage 23 first slice.
Begin one bounded behavior-expansion slice that delivers practical user value
through the target architecture. Do not reopen broad theory, do not create a
new mega-object, and do not add UI-first behavior. After the behavior slice is
implemented, tested, and manually smoked, realign Stages 23-29 against the new
code state before resuming closure work.
```

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

- [x] Select one scenario.
- [x] Define scenario boundaries.
- [x] Validate formal coverage against the expanded responsibility lattice.
- [x] Validate construction rules.
- [x] Validate evidence sufficiency.
- [x] Validate consumer output.
- [x] Validate governance review.

Completion condition:

```text
One scenario proves bounded generativity without weakening architectural coherence.
```

Evidence:

```text
Inline only: Operation Chain Workbench scenario tests, governance tests, full test suite.
```

Stop condition:

```text
Stop if the scenario requires broader platform concerns that belong to DataAnalyser rather than DataVisualiser.
```

---

## Architecture / Vocabulary Migration Closure Track

Purpose:

```text
Complete the DataVisualiser architectural and vocabulary migration by moving
live behavior onto the target spine, retiring compatibility bridges only after
replacement evidence, and preserving functionality, flexibility, and higher-level
generalisation.
```

Track rule:

```text
Bridge shrinkage is preferred over bridge removal.
Run old and new shapes in parallel until parity, tests, and smoke evidence
prove that exact bridge pieces can be retired.
The track is complete only when residual bridges are either removed or explicitly
classified as terminal adapters rather than architectural migration debt.
```

Generalisation rule:

```text
Shared meaning stays upstream in VNext contracts and architectural vocabulary.
Shared loaded facts stay in LoadedChartDataSnapshot.
Shared data access should become a reusable series-resolution request/result.
Family render inputs must remain thin, terminal, and non-semantic.
No new universal object may replace ChartDataContext.
```

Priority order:

```text
1. Protect loaded-state split.
2. Generalize series resolution.
3. Cut over Distribution family input.
4. Cut over Weekday Trend family input.
5. Align Transform input with Operation Chain concepts.
6. Cut over Main / Cartesian rendering inputs.
7. Retire ChartDataContext as shared state carrier.
8. Retire legacy projection / compatibility paths.
9. Simplify orchestration and composition hubs.
10. Productize bounded Operation Chain / Transform convergence.
11. Complete terminal delivery and evidence separation.
12. Final bridge retirement pass.
13. Final vocabulary / architecture closure review.
```

Manual smoke rule:

```text
Manual smoke tests are required when a slice changes live UI/render/reload/export
behavior. Contract-only, diagnostic-only, or read-model-only slices may close on
automated tests unless the implementation touches live chart rendering paths.
```

---

## Stage 17 — Protect Loaded-State Split

Goal:

```text
Keep ChartDataContext as a bounded legacy render bridge while preserving
LoadedChartDataSnapshot as the shared loaded-state read model for diagnostics,
milestones, and evidence.
```

Tasks:

- [x] Confirm LastLoadedData is updated when LastContext is assigned or cleared.
- [x] Move remaining non-render loaded-state reads away from LastContext where safe.
- [x] Add guardrails that prevent LoadedChartDataSnapshot from gaining render payloads, vendor concerns, or semantic authority.
- [x] Preserve ChartDataContext for existing live renderers.
- [x] Preserve diagnostic export contents and signatures.

Completion condition:

```text
Loaded-state evidence reads no longer depend directly on the broad render bridge,
and ChartDataContext remains a compatibility bridge rather than the only source
of loaded-state truth.
```

Evidence:

```text
Inline only: focused tests, full test suite, and export/manual smoke only if live export behavior changes.
```

Stop condition:

```text
Stop if LoadedChartDataSnapshot starts absorbing render data, family-specific payloads,
provider policy, or canonical semantic meaning.
```

---

## Stage 18 — Generalize Series Resolution

Goal:

```text
Separate reusable series resolution from ChartDataContext without fragmenting
resolution behavior across chart families.
```

Tasks:

- [x] Introduce a small series-resolution request/result shape if current code proves it useful.
- [x] Preserve VNext-first and legacy-fallback behavior.
- [x] Preserve cache behavior and runtime evidence.
- [x] Adapt VNextDataResolutionHelper or successor to consume the new request shape.
- [x] Keep selection, date range, table name, provenance, and fallback reason visible.
- [x] Do not move UI controls, combo boxes, or vendor types into the resolution shape.

Completion condition:

```text
Distribution, Weekday Trend, and Transform can share one resolution seam without
requiring ChartDataContext as the resolution input.
```

Evidence:

```text
Focused resolution-helper tests and existing family tests.
`DataVisualiser.Tests` 1058 passed; `DataFileReader.Tests` 15 passed.
```

Stop condition:

```text
Stop if resolution starts selecting analytical meaning, rendering provider, chart family policy,
or UI state through hidden branching.
```

---

## Stage 19 — Distribution Family Input Cutover

Goal:

```text
Move Distribution toward thin family-specific render input while preserving the
shared upstream meaning and reusable resolution seam.
```

Tasks:

- [x] Use the generalized series-resolution seam in DistributionRenderInputBuilder.
- [x] Keep DistributionRenderInput terminal and non-semantic.
- [x] Preserve Distribution capability contract, consumption contract, render-plan metadata, and qualification behavior.
- [x] Preserve Cartesian/Polar mode behavior and distribution settings.
- [x] Avoid moving Distribution semantics into the adapter.
- [x] Run manual smoke tests if live render behavior changes.

Completion condition:

```text
Distribution rendering no longer depends on ChartDataContext for data resolution,
while runtime behavior, evidence, and render-plan contracts remain intact.
```

Evidence:

```text
Distribution tests, architecture guardrails, full tests, and manual smoke/export logs if live rendering changed.
Automated evidence: `DataVisualiser.Tests` 1059 passed; `DataFileReader.Tests` 15 passed.
Manual smoke/export evidence: `documents/reachability-20260506-162251.json` verified.
```

Stop condition:

```text
Stop if DistributionRenderInput starts becoming a new semantic model or if
Distribution loses VNext/legacy fallback flexibility.
```

---

## Stage 20 — Weekday Trend Family Input Cutover

Goal:

```text
Move Weekday Trend onto the shared resolution seam after Distribution proves the pattern.
```

Tasks:

- [x] Replace direct ChartDataContext resolution pressure in WeekdayTrendComputationInvoker where safe.
- [x] Preserve Cartesian, Polar, and Scatter route behavior.
- [x] Preserve weekday toggles, average toggles, and average-window behavior.
- [x] Preserve Weekday Trend capability contract and consumption contract.
- [x] Keep computation concerns separate from rendering concerns.
- [x] Run manual smoke tests if live render behavior changes.

Completion condition:

```text
Weekday Trend consumes resolved series input through the shared seam while preserving
all existing chart behavior and contract metadata.
```

Evidence:

```text
Weekday Trend tests, architecture guardrails, full tests, and manual smoke/export logs if live rendering changed.
Automated evidence: `DataVisualiser.Tests` 1060 passed; `DataFileReader.Tests` 15 passed.
Manual smoke/export evidence: `documents/reachability-20260506-170749.json` verified.
```

Stop condition:

```text
Stop if Weekday Trend cutover requires broader main-chart or platform-level behavior.
```

---

## Stage 21 — Transform / Operation Chain Alignment

Goal:

```text
Align Transform input and operation behavior with Operation Chain concepts without
prematurely replacing the existing Transform UI or reducing latent capability.
```

Tasks:

- [x] Inspect TransformDataResolver, TransformOperationExecutor, and Operation Chain contracts together.
- [x] Identify one small compatibility-preserving alignment slice.
- [x] Reuse operation rules, derived dataset rules, confidence, fitness, or planning concepts only where directly useful.
- [x] Preserve existing Transform tab behavior.
- [x] Do not introduce formal algebra beyond the already proven Operation Chain shapes.
- [x] Run manual smoke tests if live Transform behavior changes.

Completion condition:

```text
Transform becomes more compatible with Operation Chain concepts without losing current UI behavior or flexibility.
```

Evidence:

```text
Transform tests, Operation Chain tests, full tests, and manual smoke/export logs if live Transform behavior changed.
Automated evidence: `DataVisualiser.Tests` 1064 passed; `DataFileReader.Tests` 15 passed.
Manual smoke/export evidence: `documents/reachability-20260506-172616.json` and `documents/reachability-20260506-173634.json` verified.
```

Stop condition:

```text
Stop if the slice attempts to redesign Transform, wire a full Operation Chain UI,
or formalize broad algebra beyond the bounded scenario.
```

---

## Stage 22 — Main / Cartesian Rendering Input Cutover

Goal:

```text
Move Main / Cartesian rendering inputs onto explicit target-shaped request/input
models after smaller families prove the pattern.
```

Tasks:

- [x] Review remaining ChartDataContext use in MainChart, Cartesian, normalized, diff/ratio, and orchestration paths.
- [x] Define one explicit Main / Cartesian render input shape if existing request models are insufficient.
- [x] Preserve main chart, stacked mode, overlays, normalized view, and diff/ratio behavior where currently reachable.
- [x] Preserve ChartRenderPlan, VNextUiConsumptionContract, capability contract, and metadata handoff.
- [x] Keep UI controls and vendor types out of upstream render input models.
- [x] Run manual smoke tests for main chart, stacked overlays, normalized chart, distribution/weekday regression, export, reload, and selection changes.

Completion condition:

```text
Main / Cartesian rendering can consume target-shaped input without depending on
ChartDataContext as its shared state source.
```

Evidence:

```text
Focused Main / Cartesian tests, full tests, and manual smoke/export log `reachability-20260506-181329.json`; Diff/Ratio is covered by automated tests only because it is not UI-reachable.
```

Stop condition:

```text
Stop if the cutover requires a broad MainChartsView rewrite or changes user-visible
chart semantics outside the selected slice.
```

---

## Stage 23 — Retire ChartDataContext as Shared State Carrier

Goal:

```text
Remove ChartDataContext from shared application state once live render families
consume explicit inputs and LoadedChartDataSnapshot owns loaded-state facts.
```

Tasks:

- [ ] Replace ChartState.LastContext with explicit render/session handoff where all consumers have migrated.
- [ ] Keep ChartDataContext only as a local terminal adapter shape if still required by a legacy renderer.
- [ ] Remove diagnostics, evidence, milestone, and selection dependencies on ChartDataContext.
- [ ] Remove nullable LastContext fallbacks that create empty ChartDataContext instances for control flow.
- [ ] Add guardrails preventing ChartDataContext from re-entering UI state, evidence, or VNext contracts.
- [ ] Run full manual smoke tests before removing shared-state access.

Completion condition:

```text
ChartDataContext is no longer a shared UI/application state carrier.
```

Evidence:

```text
Full tests, architecture guardrails, manual smoke/export logs, and dependency search showing no shared-state use.
```

Stop condition:

```text
Stop if removal would reduce functionality, remove latent-but-wanted behavior,
or force ChartDataContext to be replaced by another mega-object.
```

---

## Pivot A — Bounded Behavior Expansion Through Target Architecture

Goal:

```text
Use the stabilized target-spine work to add one small, practical behavior slice
that creates user value while strengthening canonical meaning, capability,
composition, consumer-neutral output, and evidence boundaries.
```

Tasks:

- [x] Select one bounded behavior slice, preferably Operation Chain / Transform / computational workflow adjacent.
- [x] Define the behavior upstream of UI/rendering through existing canonical input, capability, composition, and output concepts.
- [x] Preserve provenance, traceability, compatibility qualification, and diagnostic export.
- [x] Keep UI changes terminal and thin; consumers receive meaning but do not define it.
- [x] Add focused automated tests for capability/composition/output behavior and any affected UI coordination.
- [ ] Run manual smoke/export tests if live UI behavior changes.
- [ ] Reassess and realign Stages 23-29 after the behavior slice lands.

Completion condition:

```text
One bounded behavior slice is usable, tested, manually smoked if needed, and
implemented through the target architecture rather than beside it.
```

Evidence:

```text
Focused behavior tests, full test suites, export/manual smoke logs if UI changed,
and a post-slice realignment of remaining closure stages.
```

Stop condition:

```text
Stop if the behavior requires UI-first semantics, new ChartDataContext coupling,
vendor-specific capability definitions, broad formal algebra, or a new shared
mega-object.
```

---

## Stage 24 — Retire Legacy Projection / Compatibility Paths

Goal:

```text
Retire compatibility projection paths once their live consumers use target-spine
contracts, render inputs, and consumer-neutral surfaces directly.
```

Tasks:

- [ ] Inventory active use of LegacyChartProgramProjector, compatibility projected contexts, LegacyMetricViewGateway, and VNextDataResolutionHelper bridge behavior.
- [ ] Remove or terminally contain one proven obsolete projection path at a time.
- [ ] Preserve provenance, source signatures, metadata, fidelity/loss notes, and fallback behavior where still needed.
- [ ] Prove replacement through tests before removal.
- [ ] Add guardrails that prevent projection from creating analytical meaning.

Completion condition:

```text
Legacy projection paths are removed or explicitly classified as terminal adapters,
not architecture-migration bridges.
```

Evidence:

```text
Focused bridge-retirement tests, dependency search, full tests, and manual smoke/export logs where live behavior changed.
```

Stop condition:

```text
Stop if projection removal would erase provenance, remove fallback flexibility,
or reintroduce legacy truth through another route.
```

---

## Stage 25 — Simplify Orchestration and Composition Hubs

Goal:

```text
Reduce residual orchestration/factory/coordinator sprawl after live paths have
explicit inputs and bridge retirement evidence.
```

Tasks:

- [ ] Inspect MetricLoadCoordinator, ChartRenderingOrchestrator, ChartUpdateCoordinator, ChartControllerFactory, MainChartsView, and SyncfusionChartsView.
- [ ] Remove or split only responsibilities made obsolete by completed cutovers.
- [ ] Keep orchestration as process coordination, not semantic authority, provider policy, or evidence control.
- [ ] Preserve composition-root behavior and controller lifecycle behavior.
- [ ] Avoid cosmetic refactoring and broad file churn.

Completion condition:

```text
Major orchestration hubs no longer coordinate both target-spine and retired legacy responsibilities.
```

Evidence:

```text
Architecture guardrails, focused orchestration/controller tests, full tests, and manual smoke where live behavior changed.
```

Stop condition:

```text
Stop if simplification requires behavior redesign, broad UI rewrites, or unrelated cleanup.
```

---

## Stage 26 — Productize Bounded Operation Chain / Transform Convergence

Goal:

```text
Turn the proven Operation Chain / Transform alignment into concrete user value
without destabilizing existing Transform behavior or introducing premature formal algebra.
```

Tasks:

- [ ] Select one small existing Transform capability or Operation Chain capability that can be exposed or reused safely.
- [ ] Keep capability, composition, construction, confidence, fitness, and planning upstream of UI/rendering.
- [ ] Preserve current Transform tab behavior.
- [ ] Add user-facing behavior only if it can be tested and manually smoked.
- [ ] Keep Diff/Ratio latent unless deliberately wired and smoke-tested.

Completion condition:

```text
At least one bounded Operation Chain / Transform convergence slice produces practical user value through the target spine.
```

Evidence:

```text
Operation Chain tests, Transform tests, full tests, and manual smoke/export logs if UI behavior changes.
```

Stop condition:

```text
Stop if productization becomes a broad Operation Chain UI redesign or formal algebra expansion.
```

---

## Stage 27 — Terminal Delivery and Evidence Separation

Goal:

```text
Complete separation between terminal delivery, consumer surfaces, and observational evidence.
```

Tasks:

- [ ] Verify delivery/vendor concerns remain terminal and replaceable.
- [ ] Verify evidence/export observes behavior without routing live behavior.
- [ ] Verify VNextUiConsumptionContract and ConsumerSurfaceModel remain UI/vendor neutral.
- [ ] Verify render-plan metadata and consumption-contract metadata are preserved.
- [ ] Add or update guardrails where separation is not enforceable by current tests.

Completion condition:

```text
Delivery, consumer, projection, and evidence boundaries are explicit and enforced.
```

Evidence:

```text
Architecture guardrails, export evidence, full tests, and manual smoke/export logs.
```

Stop condition:

```text
Stop if evidence starts controlling runtime routing or delivery starts defining semantic truth.
```

---

## Stage 28 — Final Bridge Retirement Pass

Goal:

```text
Remove remaining bridge code that is demonstrably obsolete after cutover, while
preserving terminal adapters and intentional fallback paths.
```

Tasks:

- [ ] Inventory remaining bridge-labeled or compatibility-labeled paths.
- [ ] Classify each as remove, terminal adapter, intentional fallback, or defer with reason.
- [ ] Remove only paths with replacement evidence.
- [ ] Preserve fallback where capability/flexibility would otherwise be reduced.
- [ ] Run full tests and required manual smoke tests after each removal bundle.

Completion condition:

```text
No active migration bridge remains without an explicit retained-purpose classification.
```

Evidence:

```text
Dependency search, architecture guardrails, full tests, and manual smoke/export logs where live behavior changed.
```

Stop condition:

```text
Stop if bridge retirement becomes bulk deletion or reduces capability/flexibility.
```

---

## Stage 29 — Final Vocabulary / Architecture Closure Review

Goal:

```text
Decide whether DataVisualiser has reached target architecture status, and record
remaining intentional exceptions if any.
```

Tasks:

- [ ] Verify every live path maps to the implementation chain.
- [ ] Verify every remaining exception is terminal, intentional, and documented inline in code/tests or this plan.
- [ ] Verify no mega-object replaced ChartDataContext.
- [ ] Verify canonical authority remains upstream.
- [ ] Verify capability, composition, construction, confidence, fitness, planning, contracts, consumer surfaces, projection, delivery, and evidence remain distinct.
- [ ] Verify full automated tests pass.
- [ ] Verify required manual smoke/export tests pass.
- [ ] Mark architecture status green only if no active migration bridge remains.

Completion condition:

```text
The DataVisualiser architectural and vocabulary migration is complete, or the
remaining gap list is explicit, bounded, and no longer part of migration debt.
```

Evidence:

```text
Full tests, architecture guardrails, dependency search, manual smoke/export logs, and final checklist updates in this plan.
```

Stop condition:

```text
Stop if any remaining bridge, legacy path, or vocabulary collapse cannot be
classified as removed, terminal adapter, intentional fallback, or explicit future product work.
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

The DataVisualiser architectural and vocabulary migration is complete when:

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
bridge shrinkage proceeds without capability or flexibility loss
LoadedChartDataSnapshot remains a loaded-state read model, not a ChartDataContext replacement
shared series resolution exists before family-specific input cutovers expand
family render inputs remain thin terminal shapes rather than semantic owners
bridge retirement occurs only after replacement, parity, tests, and smoke evidence where applicable
Distribution, Weekday Trend, Transform, Main / Cartesian, normalized, and reachable diff/ratio paths are either target-spine consumers or explicitly terminal adapters
ChartDataContext is no longer a shared UI/application state carrier
legacy projection / compatibility paths are removed or explicitly terminal and non-authoritative
orchestration hubs no longer coordinate both retired legacy behavior and target-spine behavior
Operation Chain / Transform convergence has produced at least one bounded practical user-value slice or is explicitly classified as future product work
terminal delivery remains replaceable and downstream
evidence/export remains observational
no active migration bridge remains unclassified
architecture status is green only after Stage 29 closure evidence passes
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
manual smoke/export logs for every live UI/render behavior changed during the closure track
final dependency search for ChartDataContext, LegacyChartProgramProjector, LegacyMetricViewGateway, compatibility projected contexts, and bridge-labeled paths
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
