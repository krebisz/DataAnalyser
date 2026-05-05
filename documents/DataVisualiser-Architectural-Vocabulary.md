# DataVisualiser Architectural Vocabulary — Derivational Foundational Grammar

**Status:** Foundational architectural reference  
**Scope:** derivational concept algebra, vocabulary reduction, concept graph, structural diagnosis, target hierarchy, responsibility lattice, current-vs-target conceptual map, review gates, and bounded-generativity language for `DataVisualiser`  
**Not scope:** work sequencing, progress tracking, validation inventories, generated evidence logs, manual smoke notes, bridge-retirement ledgers, or IDE-agent execution instructions  
**Execution companion:** `documents/DataVisualiser_Migration_Plan_and_Guardrails.md`

---

## 1. Purpose

This document is not merely a glossary.

It defines the architectural grammar of `DataVisualiser` by deriving it from the observed concept space of the codebase and then reducing, folding, promoting, and organizing those concepts into a target ownership model.

It does four things:

| Function | Purpose |
|---|---|
| Vocabulary reduction | Reduce project-specific naming into reusable architectural terms. |
| Concept graph | Show the conceptual map beneath project-specific words. |
| Structural diagnosis | Explain why the current architecture drifts toward mesh, repetition, and presentation gravity. |
| Target derivation | Derive the corrected end-state: engine-centered, contract-driven, consumer-aware, evidence-observed, and terminal-delivery-disposable. |

The derivation flow is:

```text
observed code language
  -> atomized concept space
  -> non-core qualifier stripping
  -> composite pattern detection
  -> reduced grammar
  -> promoted concepts
  -> multi-parent concept graph
  -> overlap / collision analysis
  -> missing parent abstractions
  -> target ownership containers
  -> target responsibility lattice
  -> governed future extension
```

The vocabulary is intentionally stable, but not closed formal theory. It may be extended when new project results require new language, but extension must follow the same derivational discipline.

### 1.1 Companion Boundary and Authority Separation

| Document | Owns |
|---|---|
| Architectural Vocabulary | concept meaning, derivational grammar, ownership placement, boundary language, target conceptual direction |
| Migration / Guardrails document | execution order, current implementation status, task lists, completion criteria, validation evidence, agent instructions |

Conflict rule:

```text
Concept meaning and architectural grammar -> this document wins.
Execution order and current implementation state -> execution companion wins.
```

The companion relationship is a concern-separation rule, not permission to import status language into this document.

---

## 2. Formal Interpretation Note

This document translates the project's higher-order axioms into architectural vocabulary and target ownership language.

The vocabulary is the current best abstraction of discovered project concepts, especially:

```text
authority
provenance
capability
composition
consumer
interaction
boundary
overlay
evidence
```

These concepts guide architectural classification, responsibility placement, and future extension. They should not be treated as mathematically final or implementation-prescriptive.

The goal is lawful adaptability:

```text
future flexibility must preserve truth, provenance, reversibility, user authority, boundary clarity, and delivery replaceability without collapsing back into ad hoc feature growth or presentation-driven semantics.
```

---

## 3. Prime Directive and Conservation Laws

### 3.1 Prime Directive

```text
Preserve coherence while enabling bounded generativity.
```

Growth is valid only when it strengthens the system's ability to reason, compose, transform, explain, and deliver analytical results without promoting UI, rendering, vendor, evidence, diagnostics, or orchestration code into semantic authority.

### 3.2 Conservation Laws

| Law | Meaning |
|---|---|
| Authority conservation | Canonical meaning must not move downstream silently. |
| Provenance conservation | Source lineage and derivation context must survive meaningful crossings. |
| Fidelity conservation | Projection and delivery must not pretend lossless behavior when loss exists. |
| Reversibility discipline | Loss, transformation, and derived paths must be explainable or explicitly constrained. |
| Boundary conservation | A seam must remain enforceable, not merely named. |
| Consumer neutrality | Downstream consumers receive meaning; they do not redefine it. |
| Terminal delivery | Rendering and vendor infrastructure remain replaceable. |
| Evidence sidecar | Evidence observes, proves, and audits; it does not secretly route. |
| Governance restraint | New concepts are promoted only when they improve expressiveness without weakening coherence. |

---


### 3.3 Architectural North Star

`DataVisualiser` is not merely a charting or reporting tool. It is the visual / consumer-facing part of a wider canonical data reasoning platform.

North-star rule:

```text
reasoning engine = architectural center
charts / reports / APIs / exports / diagnostics / future clients = consumers
rendering / vendor surfaces = terminal delivery
```

Target direction:

| Property | Architectural meaning |
|---|---|
| canonical meaning remains upstream | data truth and semantics are not recreated downstream |
| provenance remains explicit | source and derivation lineage must remain inspectable |
| meaning is assigned declaratively | interpretation is not inferred from UI labels or render shapes |
| reasoning grows through capability and composition | analytical power is reusable, not feature-specific |
| contracts govern downstream handoff | fan-out starts at explicit boundary seams |
| projection translates without authority | compatibility mapping does not create truth |
| consumers receive meaning | consumers adapt output but do not define it |
| interaction is downstream | behavior requests do not mutate canonical meaning |
| delivery is terminal | rendering and vendor mechanisms stay replaceable |
| evidence observes | diagnostics and parity prove behavior without routing it |
| governance constrains growth | language and construction are promoted only when they preserve coherence |

Inspectable architecture should always reveal:

```text
where meaning originates
where derivation occurs
where assumptions enter
where confidence is annotated
where consumers receive output
where delivery becomes vendor-specific
where evidence proves behavior
where governance decides promotion or quarantine
```

### 3.4 Foundational Flow of Authority

Default authority flow:

```text
Truth
  -> Reasoning / Derivation
  -> Interpretation
  -> Process / Execution
  -> Contracts / Boundaries
  -> Consumers / Surface Models
  -> Terminal Delivery
```

Authority-flow rules:

```text
Truth must not be silently altered.
Meaning is assigned declaratively, not inferred ad hoc.
Canonical semantics remain authoritative.
Reasoning may derive, compose, transform, and interpret.
Process may coordinate execution but must not create semantic truth.
Contracts govern what may cross boundaries.
Consumers receive meaning.
Delivery realizes already-defined output.
Evidence observes and audits without becoming hidden live policy.
```

Compatibility rule:

```text
Legacy may remain as compatibility, fallback, or projection during migration.
Legacy must not become forward architectural truth.
```

## 4. Vocabulary Reduction

### 4.1 Atomized Concept Space

These are stable architectural words remaining after chart-family and scenario-specific qualifiers are stripped away.

```text
Adapter, Args, Authority, Boundary, Builder, Capabilities, Capability,
Composition, Configuration, Consumer, Context, Contract, Controller,
Coordinator, Data, Defaults, Diagnostics, Engine, Evaluator, Event,
Factory, Guard, Helper, Host, Interaction, Invoker, Kernel, Loader,
Manager, Mode, Model, Orchestrator, Overlay, Pipeline, Plan, Policy,
Probe, Provider, Provenance, Qualification, Registry, Renderer,
Rendering, Request, Resolver, Result, Route, Selector, Service,
Snapshot, Stage, State, Strategy, Surface, Tooltip, Tracker,
Transition, Validator, ViewModel, Workflow
```

### 4.2 Non-Core Qualifiers to Strip Away

These terms may be useful implementation qualifiers, but they are not core architectural concepts:

```text
Main, Secondary, Weekday, Trend, Syncfusion, Sunburst, Bar, Pie,
Cartesian, Diff, Ratio, Normalized, Combined, Single, Multi,
Hourly, Weekly, Cms, Admin, DateRange, Load, Toggle, Zoom, Theme
```

Rule:

```text
A qualifier may describe a family, view, mode, vendor, or scenario.
It must not be mistaken for an architectural ownership container.
```

### 4.3 Higher-Order Composite Patterns

These combinations recur often enough to expose architectural patterns already present in the codebase.

| Pattern family | Recurring compounds |
|---|---|
| Consumer / Presentation | `ChartController`, `ControllerAdapter`, `VisibilityController`, `EventBinder`, `SurfaceCoordinator`, `SelectionState`, `StateSync` |
| Rendering / Delivery | `RenderRequest`, `RenderHost`, `RenderSurface`, `RenderPlan`, `RenderModel`, `RenderAdapter`, `RenderingContract`, `RenderingRoute`, `RenderingQualification` |
| Reasoning / Program | `ReasoningEngine`, `ProgramPlanner`, `StrategyFactory`, `StrategySelection`, `StrategyValidation`, `DataResolution` |
| Flow / Process | `WorkflowCoordinator`, `OrchestrationPipeline`, `PreparationStage`, `InvocationStage`, `UpdateCoordinator`, `TransitionState` |
| Evidence / Verification | `DiagnosticsSnapshot`, `EvidenceBuilder`, `ParityEvaluator`, `ParityHarness`, `QualificationProbe`, `SessionRecorder`, `ExportWriter` |

Interpretation:

```text
These are descriptive compounds, not ideal end-state abstractions.
They expose the repeated skeleton beneath family-specific code.
```

### 4.4 Reduced Architecture Grammar

The observed language collapses into a smaller conceptual grammar:

| Grammar family | Terms |
|---|---|
| Authority / Truth | Authority, Provenance, Context, State, Mode, Request, Result, Snapshot, Model, Plan |
| Flow | Coordinator, Orchestrator, Pipeline, Stage, Workflow, Transition, Guard |
| Execution | Strategy, Service, Engine, Provider, Loader, Kernel, Capability, Composition |
| Projection | Builder, Factory, Adapter, Resolver, Selector, Projector, Contract |
| Rendering | Renderer, Route, Qualification, Host, Surface, Capabilities |
| Verification | Validator, Evaluator, Probe, Harness, Diagnostics, Evidence, Parity |
| Presentation / Consumption | Consumer, Controller, Interaction, ViewModel, Event, Args, Binder, Manager, Overlay, Boundary |

Shortest practical reduced list:

```text
Authority, Provenance, Capability, Composition, Consumer, Context, State,
Request, Result, Model, Plan, Workflow, Coordinator, Pipeline, Stage,
Strategy, Service, Engine, Builder, Factory, Adapter, Resolver, Selector,
Contract, Renderer, Route, Qualification, Host, Surface, Controller,
Interaction, ViewModel, Validator, Diagnostics, Evidence
```

Immediate conclusion:

```text
The codebase is large, but the architectural language underneath it is much smaller.
The real leverage sits in repeated concepts, not family-specific names.
```

---


### 4.5 Exploratory Grammar Pools

The reduced grammar is derived from broader pools of possible compounds. These pools preserve the algebraic search space from which concepts are promoted, folded, or deferred.

| Pool | Candidate terms | Use | Open tensions |
|---|---|---|---|
| Authority / Truth | `AuthorityEnvelope`, `ProvenanceEnvelope`, `TruthEnvelope`, `SemanticEnvelope`, `ResultEnvelope`, `RequestEnvelope`, `ProvenanceDescriptor`, `TrustModel`, `SemanticIdentity`, `ResultIdentity`, `CapabilityIdentity`, `DecisionLineage`, `ResultLineage`, `InterpretationLineage`, `ProvenanceLineage`, `SemanticInvariant`, `ProvenanceInvariant` | truth, identity, trust, provenance, reversibility, lineage | Envelope, Lineage, Identity, Invariant |
| Reasoning / Capability | `ReasoningEngine`, `CapabilityRequest`, `CapabilityModel`, `CapabilityPolicy`, `CapabilityContract`, `CapabilityGraph`, `CapabilityKernel`, `CompositionGraph`, `AnalyticalProgram`, `AnalyticalIntent`, `SemanticPolicy`, `InterpretationModel`, `InterpretationKernel`, `ConfidenceModel`, `OverlayLayer`, `SemanticProjection`, `CapabilityProjection` | reasoning, capability, composition, interpretation, confidence, overlays | Strategy vs Capability vs Policy; Operation vs Composition; Kernel scope |
| Program / Analytical Plan | `AnalyticalProgram`, `ProgramRequest`, `ProgramPlan`, `AnalyticalExecutionPlan`, `RenderPlan`, `InteractionPlan`, `DeliveryPlan`, `DensityPlan`, `SurfaceModel`, `ProgramDeliveryBinding`, `DeliveryBinding`, `AnalyticalExecutionBinding`, `ProviderBinding`, `ConsumerBinding` | planned analytical work before consumer-specific delivery | ChartProgram vs AnalyticalProgram; Plan vs Program; Route vs Binding; SurfaceModel placement |
| Contract / Boundary | `CapabilityContract`, `ProviderContract`, `ConsumerContract`, `DeliveryContract`, `InteractionContract`, `EvidenceContract`, `BoundaryContract`, `ContractBoundary`, `ProviderBoundary`, `ConsumerBoundary`, `RuntimeBoundary`, `VendorBoundary`, `NeutralContract`, `QualifiedContract`, `QualificationPolicy`, `CompatibilityRule`, `BoundaryInvariant`, `NeutralityInvariant` | handoff shape, seam enforcement, neutrality, compatibility, qualification | Boundary vs Contract; Qualification vs Selection; Neutrality vs generic abstraction |
| Consumer / Interaction | `ConsumerSurfaceModel`, `ConsumerState`, `ConsumerAdapter`, `ConsumerRequest`, `InteractionRequest`, `InteractionPlan`, `InteractionContract`, `TooltipInteraction`, `SelectionInteraction`, `TimestampInteraction`, `DisplayParticipation`, `ConsumerBinding`, `InteractionBinding` | consumer participation without semantic authority | Consumer vs Presentation; Interaction vs Event; State vs Authority; Tooltip vs Interpretation |
| Projection / Adapter / Resolution | `Projection`, `SemanticProjection`, `CapabilityProjection`, `ConsumerProjection`, `DeliveryProjection`, `LegacyProjection`, `Adapter`, `ConsumerAdapter`, `DeliveryAdapter`, `Resolver`, `Selector`, `Registry`, `Route`, `Binding`, `Qualification` | translation, compatibility, candidate discovery, candidate choice, handoff binding | Projection vs Transformation; Adapter vs Policy; Resolver vs Selector; Registry vs Runtime Policy; Route vs Binding |
| Delivery / Runtime | `Delivery`, `DeliveryAdapter`, `DeliverySurface`, `DeliveryBackend`, `BackendCapability`, `RuntimeBoundary`, `VendorBoundary`, `Lifecycle`, `RenderSurface`, `RenderAdapter`, `Host`, `RuntimeHost`, `BackendQualification`, `AdapterQualification` | terminal realization, vendor/runtime containment, backend capability, lifecycle management | Delivery vs Semantics; Backend vs Provider; RuntimeBoundary vs DomainBoundary; Lifecycle vs Ownership |
| Evidence / Audit | `Evidence`, `Diagnostics`, `Parity`, `Reachability`, `Validation`, `Audit`, `EvidenceRecord`, `AuditRecord`, `EvidenceGraph`, `ProvenanceGraph`, `DecisionLineage`, `ConstructionDecisionRecord`, `PromotionRecord`, `QuarantineRecord`, `ConflictRecord` | proof, inspection, comparison, reachability, validation, durable review | Evidence vs Control; Diagnostics vs Authority; Validation vs Hidden Policy; Audit vs Logging |

Derivation rule:

```text
A new term should enter the grammar by first being located in a pool,
then tested against folds, collisions, parentage, ownership, and target responsibility.
```

## 5. Promotion, Folding, and Do-Not-Collapse Algebra

### 5.1 Promoted Concept Glossary

The promoted concepts are not labels. They are placement rules. A type, module, workflow, refactor, or proposal should be judged by which concept it actually owns.

| Concept | Meaning | Must not collapse into |
|---|---|---|
| Authority | The right to define canonical meaning, truth, trust status, or architectural legitimacy. | orchestration, rendering, UI state |
| Semantics | Meaning of data, result, operation, relationship, or analytical expression. | labels, formatting, diagnostics |
| Provenance | Source lineage and derivation context. | logging, debugging, metadata decoration |
| Traceability | Ability to follow how a result, decision, or construction was produced. | vague audit text |
| Envelope | Carrier that preserves meaning, provenance, and context across seams. | DTO convenience object / transport bag |
| Fidelity | Preservation of meaning across transformation or projection. | visual similarity |
| Determinism | Same valid input and rules produce the same result. | repeatable UI behavior only |
| Reversibility | Ability to recover, reconstruct, or explain prior state or derivation path. | undo UI action |
| Constraint | Explicit rule bounding valid construction or execution. | incidental guard clause |
| Governance | Reviewable control over growth, promotion, quarantine, and drift. | bureaucracy, comments, stale notes |
| Intent | Stated analytical purpose. | button click or UI event |
| Capability | Reusable analytical power that can be reasoned about, qualified, composed, and reused. | feature, chart type, screen behavior |
| Composition | Lawful combination of capabilities, operations, inputs, overlays, or results. | builder plumbing |
| Transformation | Semantically meaningful change from input to output. | formatting conversion |
| Interpretation | Meaning assigned to a result under assumptions. | rendering annotation |
| Confidence | Annotation of certainty, ambiguity, reliability, or quality. | boolean validity / truth mutation |
| Overlay | Interpretive layer over authoritative output. | visual decoration / render adornment |
| Program | Executable or inspectable analytical plan. | controller workflow / presentation path |
| Policy | Explicit decision rule. | hidden branch or fallback |
| Contract | Handoff agreement with required semantics and metadata. | interface only / DTO shape |
| Boundary | Enforcement seam between ownership containers. | folder, namespace, layer label |
| Neutrality | Consumer/vendor independence where required. | generic naming |
| Qualification | Explicit compatibility proof before use. | optimistic selection |
| Provider | Source of qualified capability or delivery support. | semantic authority |
| Consumer | Receiver of already-defined meaning. | UI presentation only |
| Interaction | User or system behavior request against output. | event wiring |
| SurfaceModel | Consumer-neutral output shape before delivery. | render model or replacement mega-object |
| Binding | Explicit connection between contract, provider, consumer, and delivery target. | hidden route policy |
| Projection | Translation from one valid shape to another. | semantic creation / transformation |
| Adapter | Applies already-decided output to a target boundary. | policy owner |
| Resolver | Finds valid candidates under explicit rules. | arbitrary branching / hidden selector |
| Selector | Chooses among qualified options. | authority source |
| Registry | Declares available providers, consumers, backends, or capabilities. | runtime policy engine |
| Delivery | Terminal realization of output. | analytical result / semantics |
| Backend | Concrete mechanism capable of delivery. | consumer-neutral contract / capability |
| RuntimeBoundary | Runtime execution or hosting edge. | domain boundary |
| VendorBoundary | Boundary containing external library/vendor assumptions. | upstream design constraint |
| Lifecycle | Creation, disposal, and runtime management concerns. | semantic ownership |
| Evidence | Observable proof about behavior or correctness. | live control path |
| Diagnostics | Inspection output for understanding state. | authority or policy |
| Parity | Comparison proving equivalence or acceptable divergence. | production selector |
| Reachability | Proof that a path can execute or be observed. | provider selection |
| Validation | Rule-based correctness check. | hidden semantic policy |
| Audit | Durable review record. | log spam |
| Record | Stable evidence or state carrier. | transient debug output |

### 5.2 Folded Atom Notes

| Folded term | Folds into / expressed by |
|---|---|
| Canonical | Semantics / Authority |
| Trust | Provenance / Evidence / Confidence |
| Lineage | Provenance / Traceability |
| Identity | Authority / Semantics / Envelope |
| Invariant | Constraint / Governance / Boundary |
| Layer | Contextual grouping only |
| Graph | Relationship representation only |
| Kernel | Construction detail unless computation-core specific |
| Surface + Model | `SurfaceModel` where consumer-neutral output shape is meant |
| Runtime + Boundary | `RuntimeBoundary` where execution-host seam is meant |
| Vendor + Boundary | `VendorBoundary` where external-library assumptions are contained |

### 5.3 Do-Not-Collapse Algebra

```text
Authority != Orchestration
Provenance != Diagnostics
Envelope != Bag
Capability != Feature
Composition != Builder
Interpretation != Rendering
Confidence != Truth Mutation
Overlay != Render Decoration
Program != Presentation Path
Contract != DTO
Boundary != Folder
Neutrality != Generic Abstraction
Qualification != Selection
Provider != Authority
Consumer != Presentation
Interaction != Event
SurfaceModel != UI Model
Binding != Hidden Route Policy
Projection != Transformation
Adapter != Policy Owner
Resolver != Arbitrary Branching
Selector != Authority Source
Registry != Runtime Policy Engine
Delivery != Semantics
Backend != Capability
RuntimeBoundary != Domain Boundary
VendorBoundary != Upstream Constraint
Lifecycle != Semantic Ownership
Evidence != Control
Diagnostics != Authority
Parity != Production Selector
Reachability != Provider Selection
Validation != Hidden Authority
Audit != Logging Only
```

Use this as a guardrail when naming, refactoring, reviewing, or promoting concepts.

---


### 5.4 Grooming Rules

The following are target-language directions, not mandatory renames. They guide future design and review without forcing current code churn.

| Current / code-shaped language | Preferred target-language direction |
|---|---|
| `ChartProgram` | `AnalyticalProgram`, when the program is analytical rather than chart-specific |
| `Request`, `Result`, `Snapshot` | `Envelope`, when semantic crossing is involved |
| `Route` | `Binding`, when compatibility, consumer, provider, or delivery policy matters |
| `Renderer` | `DeliveryAdapter`, where delivery is broader than rendering |
| `Controller` | `ConsumerAdapter`, where consumer adaptation is the real role |
| `ViewModel` | `ConsumerState`, where state is consumer-side |
| `Host` | `RuntimeBoundary` or `DeliverySurface` |
| `Diagnostics` | `Evidence`, where proof/audit is intended |
| `Manager` | `Policy`, `Coordinator`, `Registry`, or `StateOwner`, depending on actual ownership |
| `Factory` | `Builder`, `Provider`, `Registry`, or `Adapter`, depending on actual responsibility |
| `Selection` | `Qualification`, where compatibility is being tested |
| `Layer` | `Boundary`, where crossing rules matter |
| UI/render model | `SurfaceModel`, where consumer neutrality matters |

Rule:

```text
Use these directions when designing the target architecture.
Do not force renames into current code without architectural justification.
```

### 5.5 Stable Safety Constraints

```text
UI must not own authority.
Rendering must not own meaning.
Evidence must not control live behavior.
Process must not define semantic truth.
Adapters must not become policy owners.
Contexts must not become service locators.
Results must not lose provenance.
Contracts must not smuggle vendor assumptions upstream.
Consumers must not redefine canonical meaning.
Delivery must remain replaceable.
```

### 5.6 Concept Promotion Result

Promoted target concept set:

```text
Authority
Canonical Semantics
Provenance
Traceability
Envelope
Lossless Fidelity
Determinism
Reversibility
Constraint
Governance
Intent
Capability
Composition
Transformation
Interpretation
Confidence
Overlay
Program
Contract
Boundary
Neutrality
Qualification
Provider
Consumer
Interaction
SurfaceModel
Binding
Delivery
Evidence
Audit
```

Working architecture phrase:

```text
Authority-bound, canonically semantic, lossless, and traceable analytical capability
expressed through neutral contracts,
adapted by consumers,
terminalized by replaceable delivery,
and governed by observational evidence.
```

## 6. Multi-Parent Concept Map

A strict tree is too rigid for this architecture. The same concept often plays more than one legitimate role.

Examples:

| Compound | Legitimate parents |
|---|---|
| `StrategySelection` | Flow + Execution |
| `RouteResolver` | Projection + Rendering |
| `ControllerAdapter` | Projection + Presentation |
| `DiagnosticsSnapshot` | Structure + Verification |
| `RenderPlan` | Structure + Rendering |

That overlap is not a flaw. It is the point of the concept map.

```text
System Architecture Concept Map
├── Structure
│   ├── Context
│   ├── State
│   ├── Mode
│   ├── Request
│   ├── Result
│   ├── Model
│   ├── Plan
│   └── Snapshot
│
├── Flow
│   ├── Workflow
│   ├── Orchestrator
│   ├── Pipeline
│   ├── Stage
│   ├── Coordinator
│   ├── Transition
│   ├── Guard
│   └── Tracker
│
├── Execution
│   ├── Strategy
│   ├── Service
│   ├── Engine
│   ├── Provider
│   ├── Loader
│   └── Kernel
│
├── Projection
│   ├── Builder
│   ├── Factory
│   ├── Adapter
│   ├── Resolver
│   ├── Selector
│   ├── Projector
│   ├── Formatter
│   └── Converter
│
├── Rendering
│   ├── Renderer
│   ├── Contract
│   ├── Route
│   ├── Qualification
│   ├── Host
│   ├── Surface
│   ├── Capabilities
│   ├── RenderPlan
│   ├── RenderModel
│   └── RenderRequest
│
├── Verification
│   ├── Validator
│   ├── Evaluator
│   ├── Probe
│   ├── Harness
│   ├── Diagnostics
│   ├── Evidence
│   ├── Parity
│   ├── Recorder
│   └── Writer
│
└── Presentation
    ├── Controller
    ├── ViewModel
    ├── Event
    ├── Args
    ├── Binder
    ├── Manager
    ├── Host
    ├── Surface
    └── State
```

This map asks:

```text
Which concepts are overloaded across too many zones?
Which zones are doing work that belongs elsewhere?
Which concept chains are too long?
Which concept pairs are collapsing into one another?
Which concepts are missing a true parent abstraction?
```

Likely high-overlap concepts:

```text
Context, State, Coordinator, Strategy, Adapter, Resolver, Render*, Diagnostics/Evidence, Controller
```

---


### 6.1 Concept Graph Rules

```text
Atomic concepts may have multiple parents.
Compound concepts inherit meaning from both their atoms and their parent context.
A concept may be central in one context and supporting in another.
```

Use the concept graph to discover:

```text
where concepts can live
how concepts combine
which compounds appear architecturally useful
which relationships should be carried into target design
```

Do not use the concept graph to decide:

```text
final ownership
namespace placement
code movement
dependency rules
construction sequence
vocabulary pruning
```

### 6.2 Atomic Parentage Map

The parent list is non-unique and non-final. It preserves relationship options before construction placement narrows them.

| Atomic concept | Possible parents | Meaning under parent |
|---|---|---|
| Authority | Governance, Boundary, Contract, Evidence | semantic legitimacy, decision legitimacy, rule source |
| Canonical | Authority, Semantics, Contract, Evidence | single accepted form or meaning |
| Semantics | Authority, Interpretation, Contract, Consumer | meaning carried through the system |
| Provenance | Authority, Envelope, Evidence, Audit | source lineage and derivation trail |
| Traceability | Provenance, Evidence, Audit, Reversibility | ability to follow decisions/results backward |
| Envelope | Authority, Contract, Boundary, Evidence | meaning-preserving carrier across seams |
| Fidelity | Lossless, Delivery, Projection, Evidence | preservation of meaning through transformation or projection |
| Determinism | Governance, Evidence, Capability, Audit | repeatable behavior under same valid inputs and rules |
| Reversibility | Provenance, Transformation, Audit, Evidence | recovery, reconstruction, or explanation of prior state/path |
| Constraint | Governance, Boundary, Policy, Contract | explicit limit on valid construction or execution |
| Intent | Capability, Program, Contract, Consumer | declared analytical purpose |
| Capability | Reasoning, Contract, Provider, Composition | reusable analytical power |
| Composition | Capability, Program, Graph, Transformation | lawful combination of operations/results |
| Transformation | Capability, Program, Provenance, Reversibility | semantically meaningful change from input to output |
| Interpretation | Semantics, Confidence, Overlay, Consumer | meaning under assumptions |
| Confidence | Interpretation, Evidence, Provenance, Trust | reliability/ambiguity annotation without truth mutation |
| Overlay | Interpretation, Consumer, Surface, Evidence | interpretive layer over a result |
| Program | Capability, Intent, Execution, Delivery | executable or inspectable analytical plan |
| Policy | Governance, Selection, Qualification, Boundary | explicit decision rule |
| Contract | Boundary, Provider, Consumer, Delivery | handoff agreement preserving semantics and metadata |
| Boundary | Contract, Runtime, Vendor, Governance | enforcement seam between ownership containers |
| Neutrality | Contract, SurfaceModel, Consumer, Delivery | independence from concrete consumer/vendor assumptions |
| Qualification | Provider, Backend, Adapter, Delivery | explicit compatibility proof before use |
| Provider | Capability, Contract, Registry, Qualification | source of qualified capability or support |
| Consumer | Contract, SurfaceModel, Interaction, Delivery | receiver of already-defined meaning |
| Interaction | Consumer, Contract, SurfaceModel, State | behavior request against output |
| Surface / Model | Consumer, Delivery, Projection, Binding, Envelope | structured consumer-facing output shape |
| Binding | Contract, Provider, Consumer, Delivery | explicit connection between qualified parties |
| Projection | Boundary, Adapter, SurfaceModel, Legacy | non-authoritative shape translation |
| Adapter | Projection, Delivery, Consumer, Backend | application of already-decided output to a boundary |
| Resolver | Registry, Qualification, Provider, Backend | finds valid candidates under explicit rules |
| Selector | Policy, Qualification, Resolver, Consumer | chooses among qualified candidates |
| Registry | Provider, Capability, Backend, Delivery | declaration of available candidates |
| Delivery | SurfaceModel, Backend, Adapter, Runtime | terminal realization of output |
| Backend | Delivery, Qualification, Provider, Runtime | concrete mechanism capable of delivery |
| Runtime / Vendor / Lifecycle | Delivery, Boundary, Backend, Host, State | hosting edge, vendor containment, creation/disposal discipline |
| Evidence / Diagnostics | Governance, Audit, Validation, Provenance, Runtime | observable proof and inspection, not authority |
| Parity / Reachability / Validation | Evidence, Compatibility, Contract, Boundary | equivalence, path proof, and rule checks |
| Audit / Record | Evidence, Governance, Provenance, State | durable review and stable carrier |

### 6.3 Relationship Patterns

| Pattern | Governs / composes / observes / terminalizes |
|---|---|
| Authority pattern | semantics, provenance, envelopes, contracts, evidence interpretation |
| Capability pattern | intent, operations, transformations, programs, overlays, interpretation |
| Boundary pattern | contracts, projection, qualification, provider/consumer handoff, runtime/vendor crossing |
| Evidence pattern | authority, provenance, contracts, qualification, delivery, compatibility language |
| Delivery pattern | surface models, bindings, vendor adapters, runtime boundaries, lifecycle handling |

### 6.4 Compound Strength Notes

Loose classification only. This is not pruning.

| Strength | Meaning |
|---|---|
| Core | likely central to final target architecture |
| Supporting | useful but probably not central |
| Candidate | promising; requires testing in target design |
| Transitional | useful for current transitional-state description |
| Deferred | interesting but not needed now |

| Compound | Strength |
|---|---|
| `CanonicalSemantics` | Core |
| `ProvenanceEnvelope` | Core |
| `SemanticEnvelope` | Candidate |
| `CapabilityContract` | Core |
| `ProviderContract` | Core |
| `ConsumerContract` | Core |
| `DeliveryContract` | Core |
| `InteractionContract` | Supporting |
| `EvidenceContract` | Candidate |
| `SurfaceModel` | Core |
| `DeliveryBinding` | Core |
| `ProviderBinding` | Supporting |
| `ConsumerBinding` | Supporting |
| `RuntimeBoundary` | Supporting |
| `VendorBoundary` | Supporting |
| `QualificationPolicy` | Supporting |
| `BackendCapability` | Supporting |
| `AdapterQualification` | Supporting |
| `DecisionLineage` / `InterpretationLineage` | Supporting |
| `EvidenceRecord` / `AuditRecord` | Supporting |
| `CompositionGraph` / `CapabilityGraph` / `EvidenceGraph` / `ProvenanceGraph` | Candidate |
| `SemanticInvariant` / `BoundaryInvariant` / `NeutralityInvariant` | Candidate |

### 6.5 Reduced Grammar from the Concept Graph

Reduced atomic grammar:

```text
Authority, Semantics, Provenance, Traceability, Envelope, Fidelity,
Determinism, Reversibility, Constraint, Governance,
Intent, Capability, Composition, Transformation, Interpretation, Confidence,
Overlay, Program, Policy,
Contract, Boundary, Neutrality, Qualification, Provider, Consumer,
Interaction, Surface, Model, Binding,
Projection, Adapter, Resolver, Selector, Registry,
Delivery, Backend, Runtime, Vendor, Lifecycle,
Evidence, Diagnostics, Parity, Reachability, Validation, Audit, Record
```

Reduced compound grammar:

```text
CanonicalSemantics, SemanticAuthority, ProvenanceEnvelope, SemanticEnvelope,
ResultEnvelope, RequestEnvelope, CapabilityContract, ProviderContract,
ConsumerContract, DeliveryContract, InteractionContract, EvidenceContract,
ContractBoundary, ProviderBoundary, ConsumerBoundary, RuntimeBoundary,
VendorBoundary, NeutralityInvariant, BoundaryInvariant, SemanticInvariant,
CapabilityPolicy, QualificationPolicy, ProviderBinding, ConsumerBinding,
DeliveryBinding, AnalyticalExecutionBinding, ProgramDeliveryBinding,
SurfaceModel, ConsumerSurfaceModel, DeliverySurface, DeliveryAdapter,
ConsumerAdapter, BackendCapability, BackendQualification, AdapterQualification,
EvidenceRecord, AuditRecord, DecisionLineage, ResultLineage,
InterpretationLineage, ProvenanceLineage, CompositionGraph, CapabilityGraph,
EvidenceGraph, ProvenanceGraph
```

## 7. Structural Diagnosis Lens

The project is not under-architected.

It is over-articulated.

The issue is not lack of layering. The issue is coexistence of too many parallel abstractions, repeated per feature family, with transitional architectures cross-coupling the same concepts.

### 7.1 Diagnosis Matrix

| Symptom | Reading | Architectural consequence |
|---|---|---|
| Several concurrent tracks | legacy/core computation, orchestration pipeline, UI controller/adapter, VNext reasoning/program/render-plan | duplicate representations of the same business intent |
| Intent fragmentation | strategy selection, orchestration selection, controller routing, render qualification, program planning | no single abstraction fully owns what should be produced and under which constraints |
| Feature-family repetition | request, host, route, qualification, contract, adapter, builder, resolver, probe repeat across families | system pays structural tax for repeated skeletons |
| UI as integration shell | presentation carries coordination, workflow, diagnostics, state sync, routing, render mediation | UI cannot be terminal until non-terminal responsibilities move upward |
| Coordinator/adapter proliferation | many coordinators, adapters, resolvers, builders, evaluators, probes | decomposition may be compensating for weak core abstractions |
| Rendering sub-architecture | rendering has repeated micro-frameworks | rendering has grown too peer-like and must be demoted |
| Migration machinery persistence | cutover, parity, diagnostics, legacy projectors, runtime evidence | transitional tools risk becoming steady-state architecture |
| Missing canonical intent | user request, selected data, transform mode, execution constraints, delivery constraints are split | duplicated control logic persists across process, projection, delivery, and consumer layers |

### 7.2 Practical Synthesis

```text
vocabulary reduction identifies the repeated conceptual skeleton
multi-parent analysis identifies unstable concepts
structural diagnosis explains why correction is necessary
terminal-layer clarification demotes presentation correctly
target architecture delays branching until contracts can carry meaning safely
```

---

## 8. Generalized As-Is vs Target

### 8.1 As-Is Generalized Architecture

```text
As-Is Architecture
├── Structural Layer
│   ├── Context
│   ├── State
│   ├── Mode
│   ├── Model
│   ├── Request
│   ├── Result
│   ├── Snapshot
│   └── Configuration
├── Flow-Control Layer
│   ├── Coordinator
│   ├── Orchestrator
│   ├── Pipeline
│   ├── Stage
│   ├── Workflow
│   ├── Transition
│   ├── Selector
│   ├── Resolver
│   ├── Guard
│   └── Tracker
├── Behavioral / Execution Layer
│   ├── Strategy
│   ├── Factory
│   ├── Service
│   ├── Provider
│   ├── Engine
│   ├── Kernel
│   ├── Invoker
│   ├── Loader
│   └── Registry
├── Construction / Projection Layer
│   ├── Builder
│   ├── Factory
│   ├── Adapter
│   ├── Resolver
│   ├── Selector
│   ├── Helper
│   ├── Converter
│   ├── Formatter
│   ├── Parser
│   └── Projector
├── Rendering Layer
│   ├── Renderer
│   ├── Rendering
│   ├── Contract
│   ├── Qualification
│   ├── Route
│   ├── Capabilities
│   ├── Host
│   ├── Surface
│   ├── Request
│   ├── Model
│   ├── Plan
│   └── Tooltip
├── Presentation Boundary
│   ├── Controller
│   ├── Adapter
│   ├── ViewModel
│   ├── Event
│   ├── Args
│   ├── Binder
│   ├── Manager
│   ├── Host
│   ├── Surface
│   └── State
├── Diagnostics / Verification Layer
│   ├── Validator
│   ├── Evaluator
│   ├── Probe
│   ├── Harness
│   ├── Diagnostics
│   ├── Evidence
│   ├── Recorder
│   ├── Writer
│   └── Snapshot
└── Cross-Cutting Utility Layer
    ├── Defaults
    ├── Helper
    ├── Tracker
    ├── Manager
    ├── Provider
    ├── Registry
    └── Diagnostics
```

Reading:

```text
concept-dense
role-overlapped
rendering-heavy
presentation-heavy
diagnostics-adjacent
structural carriers appear everywhere
```

### 8.2 Earlier Generalized Target Shape

```text
Target Architecture
├── Semantic Core
├── Application Flow
├── Execution Layer
├── Construction / Projection
├── Rendering Boundary
├── Presentation Boundary
├── Diagnostics / Verification
└── Cross-Cutting Utilities
```

This was useful, but still gave too much dignity to rendering.

### 8.3 Corrected Target Direction

```text
Truth -> Reasoning -> Process -> Contract -> Consumer -> Terminal Delivery
                          \-> Governance / Evidence as observer
```

Or:

```text
Truth defines.
Reasoning composes.
Process coordinates.
Contracts carry.
Consumers adapt.
Delivery displays.
Evidence proves.
```

---

## 9. Refined Overlap Analysis

The target architecture does not optimize for generic software neatness. It preserves truth discipline, the reasoning engine as center, delivery as non-authoritative, and future capability growth as bounded.

### 9.1 Highest-Risk Concepts

| Rank | Concept | Risk | Rule |
|---:|---|---|---|
| 1 | State | Semantic, workflow, presentation, and diagnostic state can blur. | Semantic state upstream; presentation state cannot become authority; diagnostic state observational. |
| 2 | Context | Broad context objects smuggle authority across seams. | No universal context; keep context boundary-local and purpose-specific. |
| 3 | Rendering / Render* | Rendering can become second semantic or orchestration core. | Rendering owns delivery only. |
| 4 | Coordinator | Coordinators can start thinking instead of sequencing. | No semantics, no backend knowledge, no hidden policy. |
| 5 | Strategy | Behavior, selection, and validation can collapse. | Keep behavior separate from selection and validation. |
| 6 | Controller | Convenience leakage hardens into architecture. | Translate downstream requests; do not own authority. |
| 7 | Diagnostics / Evidence | Observation can become live control. | Evidence observes; it does not route. |
| 8 | Adapter | Adapters can hide policy. | Apply already-decided output to a boundary. |
| 9 | Resolver | Resolvers can become disguised orchestration. | Find valid candidates under explicit rules. |

### 9.2 Vulnerable Zones

| Zone | Failure mode |
|---|---|
| Semantic Core / Authority | absorbs interpretation or presentation concerns |
| Application Process | sequencing quietly becomes meaning |
| Rendering / Delivery | delivery logic grows too large and too central |
| Consumer / Presentation | convenience becomes architectural ownership |

### 9.3 Concept-Collision Watchlist

```text
State + Context
Coordinator + Strategy
Coordinator + Controller
Render + Strategy
Render + Controller
Diagnostics + Execution
Adapter + Resolver
Contract + Capability
SurfaceModel + Consumer
Projection + Adapter
Evidence + Validation
Transformation + Projection
Provider + Backend
Delivery + Consumer
Semantics + Interpretation
Evidence + Audit
```

### 9.4 Missing Parent Abstractions

| Missing parent | Concepts it absorbs / clarifies |
|---|---|
| Truth Envelope | Context, State, Request, Result |
| Execution Process | Workflow, Coordinator, Pipeline, Stage, Transition |
| Projection Layer | Builder, Adapter, Resolver, Selector, Projector |
| Delivery Boundary | Renderer, Route, Qualification, Contract, Host, Surface |
| Canonical Intent Model | user request, selected data, transform/comparison mode, execution constraints, delivery constraints |

---

## 10. Critical Architectural Clarification: Rendering Is Disposable Infrastructure

Rendering is not just modular.

It is disposable infrastructure.

Clean rule:

```text
engines define
providers expose
contracts carry
consumers adapt
projection translates
rendering is swappable
```

### 10.1 What Belongs Above Terminal Delivery

Anything that is really:

```text
reasoning
planning
shaping
transformation
capability selection
interaction semantics
backend-agnostic delivery contracts
non-UI-specific view-state logic
```

must be extracted upward.

### 10.2 Terminal-Layer Test

A terminal layer should only:

```text
bind to contracts
render supplied structures
relay interaction events upward
manage local lifecycle quirks
provide vendor-specific surface behavior
```

It must not decide:

```text
what gets shown semantically
how results are composed
how execution is routed
what interaction means
what result shape must exist
```

If those concerns live there, the layer is not terminal.

Updated boundary statement:

```text
Presentation owns visualization only.
Engine-owned contracts remain authoritative.
Rendering is fully replaceable.
```

---

## 11. Reshaped Optimal Concept Hierarchy

```text
Optimal Concept Hierarchy
├── 1. Truth Authority
│   ├── Ingestion
│   ├── Identity
│   ├── Normalization
│   ├── Canonical Semantics
│   ├── Provenance
│   ├── Trust Classification
│   └── Canonical Contracts
│
├── 2. Reasoning Core
│   ├── Analytical Program
│   ├── Strategy
│   ├── Transform / Composition Capability
│   ├── Confidence Annotation
│   ├── Interpretation Rules
│   ├── Program Planning
│   ├── Execution Engine
│   └── Derived Result Identity
│
├── 3. Application Process
│   ├── Request
│   ├── Context Handoff
│   ├── Workflow
│   ├── Coordinator
│   ├── Pipeline / Stage
│   ├── Routing
│   ├── Fallback / Coexistence Control
│   └── Execution Observability
│
├── 4. Provider Contract Boundary
│   ├── Result Contract
│   ├── Program Contract
│   ├── Delivery Contract
│   ├── Interaction Contract
│   ├── Multi-Result Contract
│   ├── View Contract
│   └── Consumer-Agnostic Surface Model
│
├── 5. Consumer Boundary
│   ├── Controller
│   ├── ViewModel
│   ├── Event / Binder
│   ├── Interaction Relay
│   ├── Consumer State
│   └── Host Coordination
│
├── 6. Projection and Translation
│   ├── Builder
│   ├── Adapter
│   ├── Resolver
│   ├── Selector
│   ├── Projector
│   └── Formatter / Converter
│
├── 7. Terminal Presentation Infrastructure
│   ├── Renderer
│   ├── Render Surface
│   ├── Backend Adapter
│   ├── Capability Qualification
│   ├── Route / Host Binding
│   ├── Lifecycle Handling
│   └── Vendor-Specific Behavior
│
└── 8. Governance and Evidence
    ├── Diagnostics
    ├── Evidence
    ├── Parity
    ├── Reachability
    ├── Qualification Probes
    ├── Validation
    └── Export / Audit
```

Why this hierarchy is better:

| Correction | Effect |
|---|---|
| Truth authority upstream | stops `Context`, `State`, `Request`, and `Result` from becoming transport bags |
| Reasoning core centered | prevents feature-specific orchestration from owning capability growth |
| Process subordinate to meaning | keeps sequencing from becoming truth |
| Provider contract boundary promoted | creates the real fan-out seam |
| Consumer boundary made explicit | allows charts, exports, APIs, and future clients without presentation authority |
| Projection and translation contained | keeps shape adaptation non-authoritative and prevents compatibility mapping from creating meaning |
| Rendering demoted | makes replacement honest and cheap |
| Evidence sidecar isolated | proves behavior without controlling it |

---

## 12. Enhanced Target Architecture

The target is not a single line.

It is:

```text
authority spine
+ reasoning and composition container
+ process and contract transition
+ branching consumer field
+ non-authoritative projection / translation seam
+ terminal delivery layer
+ observational governance sidecar
```

### 12.1 Enhanced Target Containers

```text
ENHANCED TARGET ARCHITECTURE
├── 1. Authority Container
│   ├── Authority
│   ├── Provenance
│   ├── Canonical Semantics
│   ├── Identity
│   ├── Truth-Aware Context
│   ├── Truth-Aware State
│   ├── Truth-Aware Request
│   ├── Truth-Aware Result
│   └── Truth Envelope / Canonical Contracts
│
├── 2. Reasoning and Capability Container
│   ├── Engine
│   ├── Strategy
│   ├── Capability
│   ├── Composition
│   ├── Transform / Comparison
│   ├── Program Planning
│   ├── Derived Result Identity
│   ├── Interpretation Rules
│   └── Overlay Definition
│
├── 3. Process and Execution Container
│   ├── Workflow
│   ├── Coordinator
│   ├── Pipeline / Stage
│   ├── Routing
│   ├── Context Handoff
│   ├── Fallback / Coexistence
│   ├── Execution Observability
│   └── Process Boundaries
│
├── 4. Contract and Boundary Container
│   ├── Program Contract
│   ├── Delivery Contract
│   ├── Interaction Contract
│   ├── View Contract
│   ├── Multi-Result Contract
│   ├── Consumer-Agnostic Surface Model
│   └── Boundary Enforcement
│
├── 5. Consumer and Interaction Field
│   ├── Consumer
│   ├── Consumer Family
│   ├── Controller
│   ├── Interaction
│   ├── ViewModel
│   ├── Event / Binder
│   ├── Consumer State
│   └── Host Coordination
│
├── 6. Projection and Translation Container
│   ├── Projection
│   ├── Adapter
│   ├── Legacy Mapping
│   ├── Consumer Projection
│   ├── Delivery Projection
│   ├── Format Translation
│   └── Loss Declaration
│
├── 7. Terminal Delivery Infrastructure
│   ├── Renderer
│   ├── Render Surface
│   ├── Backend Adapter
│   ├── Capability Qualification
│   ├── Route / Host Binding
│   ├── Overlay Delivery
│   └── Vendor-Specific Behavior
│
└── 8. Governance and Evidence Sidecar
    ├── Diagnostics
    ├── Evidence
    ├── Parity
    ├── Reachability
    ├── Validation
    ├── Export / Audit
    └── Qualification Probes
```

### 12.2 Authority Spine

```text
Authority -> Reasoning / Capability -> Process -> Contracts -> Consumers -> Projection / Translation -> Terminal Delivery
                                  \-> Governance / Evidence
```

### 12.3 Branching Structure

```text
Authority
   |
   v
Reasoning / Capability / Composition
   |
   v
Process / Execution
   |
   v
Contracts / Boundaries
   |
   +---------> Consumer Family A -> Projection / Translation A -> Terminal Delivery A
   |
   +---------> Consumer Family B -> Projection / Translation B -> Terminal Delivery B
   |
   +---------> Consumer Family C -> Projection / Translation C -> Terminal Delivery C

Governance / Evidence observes the spine and branch behavior from the side.
```

### 12.4 Why the Containers Matter

| Container | Why it matters |
|---|---|
| Authority | prevents truth and identity from becoming generic request/result bags |
| Reasoning / Capability | gives programmable growth a home that is not feature orchestration |
| Process / Execution | coordinates without polluting meaning or delivery |
| Contract / Boundary | protects upstream meaning and enables lawful fan-out |
| Consumer / Interaction | models multiple consumers and interaction styles without semantic authority |
| Projection / Translation | translates valid shapes without creating meaning, mutating confidence, or hiding compatibility loss |
| Terminal Delivery | keeps vendor/runtime behavior replaceable and subordinate |
| Governance / Evidence | observes, validates, qualifies, proves, and audits without live semantic control |

Material change from earlier targets:

```text
earlier target = corrected direction
enhanced target = spine + containers + lawful branching + non-authoritative translation seams
```

---


### 12.5 Target Architectural Code-Map Vocabulary

Use this vocabulary to map conceptual grammar to code placement without letting current code names become authority.

| Architectural concern | Preferred code-map expression |
|---|---|
| Authority / Semantics / Provenance | contracts, descriptors, signatures, immutable semantic carriers, provenance envelopes |
| Reasoning / Capability | capability contracts, operation models, analytical programs, composition graphs, transformation kernels |
| Program / Execution Plan | analytical program, program request, execution plan, render/surface plan where downstream |
| Contract / Boundary | request/result/envelope contracts, qualification seams, explicit metadata preservation rules |
| Provider / Qualification | provider registry, provider metadata, qualification policy, resolver/selector with explicit rules |
| Consumer / SurfaceModel | consumer-neutral surface models, consumer contracts, consumer adapters, consumer state relays |
| Interaction | interaction requests, interaction contracts, tooltip/selection/timestamp participation contracts |
| Projection / Adapter | projectors for shape translation, adapters for boundary application, no authority creation |
| Delivery / Runtime / Vendor | delivery adapters, backend capabilities, runtime boundaries, vendor-contained surfaces |
| Evidence / Audit | evidence records, diagnostics exports, parity records, reachability records, audit trails |

Code-map rule:

```text
A type name may lag behind target vocabulary during compatibility work.
Actual ownership is judged by responsibility, metadata preservation, authority direction, and boundary behavior.
```

### 12.6 Compact Target Architecture Graph

This graph is the target architectural shape produced from the vocabulary reduction and concept relationship graph. It is not a literal namespace tree.

```text
Authority
   ├── CanonicalSemantics
   ├── Provenance
   ├── Traceability
   ├── Fidelity
   ├── Determinism
   └── Reversibility
        │
        v
Envelopes
   ├── RequestEnvelope
   ├── ResultEnvelope
   ├── ProgramEnvelope
   ├── SurfaceEnvelope
   ├── DeliveryEnvelope
   └── EvidenceEnvelope
        │
        v
Reasoning
   ├── Intent
   ├── Capability
   ├── Composition
   ├── Transformation
   ├── Interpretation
   ├── Confidence
   └── Overlay
        │
        v
Program
   ├── AnalyticalProgram
   ├── ProgramPlan
   ├── AnalyticalExecutionPlan
   └── AnalyticalExecutionBinding
        │
        v
Contracts / Boundaries
   ├── SemanticContract
   ├── CapabilityContract
   ├── ProviderContract
   ├── ConsumerContract
   ├── SurfaceContract
   ├── DeliveryContract
   └── EvidenceContract
        │
        v
Qualification
   ├── ProviderQualification
   ├── ConsumerQualification
   ├── BackendQualification
   ├── AdapterQualification
   ├── SurfaceQualification
   └── DeliveryQualification
        │
        v
Providers / Projection
   ├── ProviderRegistry
   ├── ProviderBinding
   ├── SemanticProjection
   ├── CapabilityProjection
   ├── ConsumerProjection
   ├── SurfaceProjection
   └── DeliveryProjection
        │
        v
Consumers / Interaction
   ├── ConsumerBinding
   ├── ConsumerSurface
   ├── ConsumerState
   ├── ConsumerAdapter
   ├── InteractionRequest
   ├── InteractionBinding
   └── InteractionFlow
        │
        v
Surfaces / Delivery
   ├── SurfaceModel
   ├── SurfaceBinding
   ├── DeliveryBinding
   ├── DeliveryAdapter
   ├── DeliverySurface
   ├── BackendAdapter
   ├── RuntimeBoundary
   └── VendorBoundary

Evidence observes all seams.
Audit records reviewable lineage.
Governance constrains growth.
Delivery remains terminal.
```

Projection placement rule:

```text
Projection / Translation is boundary-local and non-authoritative.
It may appear as consumer projection, delivery projection, or compatibility projection.
Its placement in a diagram identifies a crossing point, not a new source of meaning.
```

## 13. Expanded Target Responsibility Lattice

The enhanced target architecture defines the container geometry. The responsibility lattice defines the internal contract of each layer.

| Layer | Owns | Governs | Emits |
|---|---|---|---|
| Canonical Source / Input | SourceIdentity, InputIdentity, RawInputReference, IntakeContext | SourceAcceptance, InputEligibility, RawPreservation | CanonicalInput, SourceRecord, InputEnvelope |
| Authority Substrate | SemanticAuthority, Provenance, Traceability, Fidelity, Reversibility, Constraint, Envelope | CanonicalTruth, PreservationRules, LossDeclaration | AuthorityEnvelope, ProvenanceRecord, ConstraintSet |
| Semantic Formation | SemanticIdentity, DomainConcept, MetricIdentity, TemporalIdentity, RelationshipIdentity, AssumptionRecord, MeaningQualification | ConceptRecognition, SemanticTyping, RelationshipClassification, AmbiguityRegistration | SemanticModel, QualifiedMeaning, AssumptionSet, ConceptRelation |
| Reasoning / Capability | Intent, Capability, Operation, Transformation, Composition, Interpretation, Confidence, Overlay, AnalyticalFitness | CapabilityEligibility, OperationCompatibility, TransformationValidity, InterpretationDiscipline | CapabilityResult, AnalyticalResult, ConfidenceAnnotation, OverlayPlan |
| Construction Algebra | Construction, InputSet, IntermediateSet, DerivedSet, OutputSet, CompositionGraph, TypedRelation, TransformationTrace, EvidenceTrace, ConflictRecord | Arity, Cardinality, CompositionLaw, RelationTyping, EvidenceSufficiency | ConstructionGraph, DerivedDataset, OperationTrace, RelationGraph, ConstructionDecisionRecord |
| Execution / Process | Program, ExecutionPlan, Orchestration, Resolver, Selector, Registry, Policy, RuntimeState, Lifecycle | ExecutionSequencing, CandidateResolution, PolicyApplication, StateTransition, LifecycleBoundaries | ExecutedProgram, ResolutionRecord, SelectionRecord, RuntimeManifest, ExecutionManifest |
| Contract / Boundary | Contract, Boundary, Qualification, Neutrality, Binding, CompatibilityProof, MetadataPreservationRule | HandoffShape, RequiredMetadata, CompatibilityCriteria, BoundaryLegality | QualifiedContract, BoundaryCrossingRecord, BindingRecord, ConsumerHandoffShape |
| Consumer Model | SurfaceModel, ConsumerContract, ConsumerRequest, InteractionRequest, ExplanationRequest, SelectionRequest, ConsumerStateRelay | ConsumerNeutralOutputShape, InteractionConstraints, ExplanationParticipation, StateRelayBoundaries | ConsumerSurfaceModel, ConsumerInstruction, InteractionInstruction, ExplanationPayload |
| Projection / Translation | Projection, Adapter, LegacyMapping, TargetToConsumerMapping, ConsumerToDeliveryMapping, FormatTranslation, LossDeclaration | ShapeTranslation, LegacyCompatibility, MetadataPreservation, BoundaryTranslation | ProjectedModel, AdapterPayload, TranslationRecord, ProjectionLossRecord |
| Terminal Delivery | Renderer, Backend, VendorBoundary, RuntimeBoundary, HostLifecycle, DisplaySurface, ExportSurface, ApiSurface, ReportSurface | Rendering, BackendAdaptation, VendorLifecycle, RuntimeDeliveryMechanics, SurfaceRealization | RenderedOutput, ExportedOutput, ApiResponse, ReportOutput, DeliveryRecord |
| Evidence / Diagnostics Sidecar | Evidence, Diagnostics, Validation, Parity, Reachability, Audit | Observability, CorrectnessChecks, DivergenceChecks, AuditDurability | EvidenceReport, ValidationResult, ParityResult, ReachabilityResult, AuditRecord |
| Governance / Control Plane | GovernanceRules, Promotion/Quarantine, DriftDetection, EvidenceSufficiencyCriteria, ArchitecturalFitnessReview | ConceptPromotion, CapabilityPromotion, PolicyIntroduction, BoundaryHardening, ConstructionGeneralisation | GovernanceDecision, PromotionRecord, QuarantineRecord, DriftSignal, CoherenceRisk |

Lattice rule:

```text
The lattice controls responsibility placement, authority direction, boundary discipline, and conceptual containment.
It does not mandate folders, class names, interfaces, or runtime order.
```

---


### 13.1 Target Architecture Spine and Planes

The expanded target is best read as three simultaneous structures:

```text
simple target graph
  -> expresses authority direction

expanded responsibility lattice
  -> expresses governed conceptual containment

current project code map
  -> expresses present implementation pressure against the target
```

Target spine:

```text
Canonical Source / Input
  -> Authority / Provenance / Envelope
  -> Semantic Formation
  -> Reasoning / Capability
  -> Construction Algebra
  -> Execution / Process
  -> Contract / Boundary
  -> Consumer Model
  -> Projection / Translation
  -> Terminal Delivery
```

Cross-cutting governance plane:

```text
Cross-Cutting Governance / Control Plane
├── GovernanceRules
├── Promotion / Quarantine
├── DriftDetection
├── EvidenceSufficiencyCriteria
└── ArchitecturalFitnessReview
```

Evidence / diagnostics sidecar:

```text
Evidence / Diagnostics Sidecar
├── Evidence
├── Diagnostics
├── Validation
├── Parity
├── Reachability
└── Audit
```

Plane rules:

```text
Governance evaluates and constrains growth.
Governance may introduce named policy only when explicitly promoted into Process / Execution.
Evidence may observe every executable or inspectable seam.
Evidence must not own semantic truth, provider selection, live routing, or hidden policy.
```

### 13.2 Abstract Code Map Direction

The following package / namespace families are non-binding target expressions for implementation planning. They express responsibility placement, not mandatory folder structure.

```text
DataVisualiser.Core.Input
DataVisualiser.Core.Authority
DataVisualiser.Core.Semantics
DataVisualiser.Core.Capabilities
DataVisualiser.Core.Construction
DataVisualiser.Runtime.Execution
DataVisualiser.Contracts
DataVisualiser.Consumers
DataVisualiser.Projection
DataVisualiser.Delivery
DataVisualiser.Evidence
DataVisualiser.Governance
```

Code-map direction rule:

```text
These names are responsibility targets.
They should inform implementation-plan structure.
They must not be treated as compulsory namespaces, class names, or project splits unless later implementation analysis justifies that move.
```

## 14. Visual Contrast: Current vs Target Geometry

### 14.1 Current Main Spine

```text
CURRENT MAIN SPINE

User / Host
   |
   v
Controller / UI Coordination
   |
   v
Coordinator / Orchestrator Layer
   |
   v
Strategy / Resolver / Adapter Layer
   |
   v
Computation / Rendering Preparation
   |
   v
Rendering / Surface / Host
   |
   v
Presentation Output
   |
   v
Diagnostics / Evidence / Parity
```

Reading:

```text
spine starts too high in presentation
computation and semantics are not dominant enough
rendering sits too centrally
support abstractions sit inline
evidence feels attached instead of side-observing
```

### 14.2 Target Main Spine

```text
TARGET MAIN SPINE

Truth Authority
   |
   v
Reasoning Core
   |
   v
Application Process
   |
   v
Provider Contract Boundary
   |
   v
Consumer Boundary
   |
   v
Projection / Translation
   |
   v
Terminal Presentation Infrastructure

Governance / Evidence / Diagnostics observes from the side.
```

Short contrast:

```text
Current: UI -> Coordination -> Mediation -> Compute/Render -> Surface
Target:  Truth -> Reasoning -> Process -> Contract -> Consumer -> Projection -> Presentation
```

### 14.3 Current Non-Linear Shape

```text
CURRENT NON-LINEAR SHAPE

                      +----------------------+
                      |   Diagnostics /      |
                      |   Evidence / Parity  |
                      +----------+-----------+
                                 ^
                                 |
+----------------+      +--------+--------+      +----------------+
| UI / Views /   |<---->| Controllers /   |<---->| Hosts / Binders|
| Chart Surfaces |      | Presentation    |      | / UI Helpers   |
+--------+-------+      +--------+--------+      +--------+-------+
         ^                       |                        |
         |                       v                        |
         |              +--------+--------+               |
         |              | Coordinators /  |<--------------+
         |              | Orchestrators   |
         |              +----+-----+------+
         |                   |     |
         |                   |     v
         |                   |  +--+------------------+
         |                   |  | Strategy / Factory /|
         |                   |  | Resolver / Selector |
         |                   |  +--+------------------+
         |                   |     |
         |                   v     v
         |              +----+-----+------+
         |              | State / Context /|
         |              | Request / Result |
         |              +----+-----+------+
         |                   |     |
         |                   v     v
         |              +----+-----+------+
         +--------------| Computation /   |
                        | Program / Render|
                        | Preparation     |
                        +----+-----+------+
                             |
                             v
                        +----+-----------+
                        | Rendering /    |
                        | Qualification /|
                        | Route / Surface|
                        +----------------+
```

Current geometry:

```text
multi-center mesh
branching happens early
mediation happens everywhere
ownership is blurred
presentation-adjacent layers carry too much weight
```

### 14.4 Target Non-Linear Shape

```text
TARGET NON-LINEAR SHAPE

                      +----------------------+
                      | Governance / Evidence|
                      | Diagnostics / Parity |
                      +----------^-----------+
                                 |
                                 |
+------------------+     +-------+--------+     +----------------------+
| Truth Authority  +---->| Reasoning Core +---->| Application Process  |
| canonical data,  |     | programs,      |     | workflow, routing,   |
| provenance,      |     | strategy,      |     | coordination,        |
| trust, semantics |     | transforms     |     | execution control    |
+------------------+     +-------+--------+     +----------+-----------+
                                 |                         |
                                 +------------+------------+
                                              |
                                              v
                               +--------------+---------------+
                               | Provider Contract Boundary   |
                               | result, delivery, view,      |
                               | interaction, multi-result    |
                               +------+------------+---------+
                                      |            |
                         +------------+            +-------------+
                         |                                        |
                         v                                        v
              +----------+-----------+               +------------+----------+
              | Consumer Boundary A  |               | Consumer Boundary B   |
              | chart consumer       |               | export / API / other  |
              +----------+-----------+               +------------+----------+
                         |                                        |
                         v                                        v
              +----------+-----------+               +------------+----------+
              | Projection /         |               | Projection /          |
              | Translation A        |               | Translation B         |
              +----------+-----------+               +------------+----------+
                         |                                        |
                         v                                        v
              +----------+-----------+               +------------+----------+
              | Terminal Presentation|               | Terminal Presentation |
              | / Backend / Surface  |               | / Delivery Surface    |
              +----------------------+               +-----------------------+
```

Target geometry:

```text
authoritative spine + downstream branching + side observers
authority is linear
branching is delayed
contracts are the fan-out boundary
projection is non-authoritative shape translation
presentation is lightweight and replaceable
```

---


### 14.5 Renewed Current Project Code Map

This map records the current code shape against the target architecture. It is descriptive, not normative.

```text
CURRENT PROJECT CODE MAP

DataVisualiser
├── Core
│   ├── domain/state carriers
│   │   ├── MetricData
│   │   ├── ChartState
│   │   ├── MetricState
│   │   ├── MetricSeriesSelection
│   │   └── MetricSelectionService
│   │
│   ├── computation / strategies
│   │   ├── IChartComputationStrategy
│   │   ├── ChartComputationResult
│   │   ├── legacy and CMS strategy families
│   │   ├── parity-aware strategy support
│   │   └── focused helpers for binning, smoothing, series preparation, timeline alignment, bucket aggregation, and transform computation
│   │
│   ├── rendering contracts / render models / adapters
│   │   ├── rendering contracts used by active chart families
│   │   ├── ChartRenderModel / UiChartRenderModel / chart-series and chart-axis models
│   │   ├── render engines and render-plan adapters
│   │   ├── Syncfusion contracts and delivery adapter
│   │   ├── LiveCharts presentation rendering
│   │   └── ECharts placeholder surface / future delivery surface
│   │
│   ├── validation / parity / reachability
│   │   ├── parity comparison support
│   │   ├── reachability validation
│   │   └── runtime validation flows kept separate from evidence/export control
│   │
│   └── orchestration and transitional coordination
│       ├── ChartUpdateCoordinator
│       ├── ChartRenderingOrchestrator
│       └── bounded coordination-only hubs guarded against semantic/provider/evidence authority
│
├── VNext
│   ├── contracts / authority carriers
│   │   ├── MetricSelectionRequest
│   │   ├── MetricSeriesRequest
│   │   ├── MetricLoadSnapshot
│   │   ├── MetricSeriesSnapshot
│   │   ├── ProvenanceDescriptor
│   │   ├── AnalyticalIntent
│   │   ├── AnalyticalExecutionResult
│   │   └── AnalyticalResultSet
│   │
│   ├── reasoning / capability / program
│   │   ├── AnalyticalIntentFactory
│   │   ├── CapabilityRequest
│   │   ├── CompositionKind
│   │   ├── ChartProgramRequest
│   │   ├── ChartProgramKind
│   │   ├── ChartProgram
│   │   ├── ChartProgramPlanner
│   │   ├── confidence annotation structures
│   │   ├── overlay planning structures
│   │   └── analytical interpretation structures
│   │
│   ├── application / compatibility projection
│   │   ├── LegacyChartProgramProjector
│   │   ├── LegacyMetricViewGateway
│   │   └── compatibility projection into ChartDataContext where old UI consumption remains active
│   │
│   └── rendering / contract-bound delivery spine
│       ├── ChartRenderPlan
│       ├── ChartRenderPlanProjector
│       ├── ChartRenderDeliveryBinding
│       ├── ChartRenderPlanProviderMetadata
│       ├── ChartRenderPlanVocabularyMetadata
│       ├── ChartBackendSelector
│       ├── ChartRenderPlanAdapterQualification
│       ├── ChartRenderPlanAdapterQualificationRules
│       ├── ChartRenderPlanAdapterDispatcher
│       └── ChartRenderAdapterResult
│
├── UI
│   ├── MainHost / ViewModels / Admin shell
│   │   ├── VNext integration coordinators
│   │   ├── MetricLoadCoordinator
│   │   ├── evidence and export composition
│   │   └── shared workspace/session behavior
│   │
│   ├── Charts / Presentation / Controllers / Interaction
│   │   ├── ChartControllerFactory
│   │   ├── ChartControllerFactoryContext
│   │   ├── chart-family controller adapters
│   │   ├── VNextDataResolutionHelper
│   │   ├── tooltip factories and timestamp sinks
│   │   └── selection / tooltip / display participation helpers
│   │
│   └── Delivery surfaces
│       ├── SyncfusionChartsView
│       ├── LiveCharts presentation rendering
│       ├── ECharts placeholder surface
│       └── terminal vendor-specific host / surface behavior
│
├── DataVisualiser.Tests
│   ├── VNext contract / provenance / interpretation tests
│   ├── render-plan / adapter / delivery qualification tests
│   ├── architecture guardrail tests
│   ├── consumer / interaction containment tests
│   └── evidence / diagnostics / parity / reachability tests
│
└── Current transitional pressure points
    ├── ChartDataContext remains the dominant UI consumption model
    ├── LegacyChartProgramProjector remains necessary while ChartDataContext remains active
    ├── VNextDataResolutionHelper remains a bounded bridge into production UI consumption
    ├── LegacyMetricViewGateway remains a compatibility gateway
    ├── MainChartsView and SyncfusionChartsView remain host-level concentration points
    ├── MetricLoadCoordinator remains a high-risk transitional bridge
    ├── ChartControllerFactory / ChartControllerFactoryContext remain bounded integration hubs
    └── chart-family adapters remain active consumer/delivery bridges until VNext-native consumption replaces the old model
```

Current target-spine flow:

```text
MetricSelectionRequest / MetricSeriesRequest
  -> AnalyticalIntent
  -> MetricLoadSnapshot / MetricSeriesSnapshot
  -> ChartProgram
  -> AnalyticalExecutionResult / AnalyticalResultSet
  -> ChartRenderPlan
  -> ChartRenderAdapterResult
  -> evidence / diagnostics / audit export
```

Current compatibility flow:

```text
ChartProgram
  -> LegacyChartProgramProjector
  -> ChartDataContext
  -> existing UI / chart-family adapters
```

Current capability-carriage flow for active family slices:

```text
AnalyticalIntentFactory
  -> CapabilityRequest
  -> ChartProgramRequest
  -> family-specific capability contract
  -> family-specific chart render request
  -> rendering contract
  -> render-plan builder
  -> ChartRenderPlanVocabularyMetadata
  -> delivery adapter / evidence
```

Current-state reading:

```text
The target spine is materially present and test-backed.
Production UI consumption is still partially governed by the older ChartDataContext-centered model.
The current architecture is therefore a managed coexistence state:
target-spine authority upstream, compatibility projection through old consumption seams downstream.
```

### 14.6 Current-vs-Target Comparison Rules

| Target concern | Current project expression | Current reading | Watch risk |
|---|---|---|---|
| Authority / provenance | `MetricSelectionRequest`, `MetricSeriesRequest`, snapshots, `ProvenanceDescriptor`, `AnalyticalIntent`, `AnalyticalExecutionResult`, `AnalyticalResultSet` | materially present in VNext | reduced provenance through legacy projection |
| Envelope | intent, execution result, result set, render plan, adapter result | envelope-like carriers exist at main seams | naming may lag vocabulary |
| Reasoning / capability | `CapabilityRequest`, `ChartProgramRequest`, `ChartProgram`, `ChartProgramPlanner`, confidence, overlay, interpretation structures | present and growing through capability slices | feature-specific growth bypassing capability path |
| Program | `ChartProgram` and chart-family program kinds | implemented but still chart-shaped | `AnalyticalProgram` remains target language, not current dominant type name |
| Contract / boundary | consumer/provider/delivery contracts, render requests, render plans, delivery bindings | materially strengthened | bypass through old UI paths |
| Qualification | backend selector, adapter qualification, rendering-contract route capability matrices | present and guarded | selection becoming hidden policy |
| Projection | render-plan projectors and `LegacyChartProgramProjector` | present and classified non-authoritative | projection treated as semantic creation |
| Consumer / interaction | controllers, adapters, tooltip/selection/timestamp helpers, consumer state relays | mostly relay behavior | consumer layer regaining provider or analytical authority |
| SurfaceModel | `ChartRenderPlan`, render models, UI render models | seam exists; `ChartRenderPlan` is strongest current expression | old render/UI models becoming pseudo-core |
| Delivery / vendor | rendering contracts, delivery adapters, Syncfusion/LiveCharts/ECharts surfaces | terminal and vendor-contained in guarded paths | vendor assumptions leaking upstream |
| Evidence / audit | evidence diagnostics, parity, reachability, validation, export paths | strong and broad | evidence controlling live behavior |
| Governance | architecture guardrails, audit notes, external planning record, vocabulary document | active as test/document discipline | governance turning into stale documentation |
| Legacy coexistence | `ChartDataContext`, legacy projector, VNext data helper, legacy gateway, parity lanes | bounded but still real | permanent mesh if consumer transition stalls |

Bridge rule:

```text
The current code map may contain transitional bridges.
A bridge is acceptable only while it is named, bounded, evidence-visible, and not promoted as target architecture.
```

## 15. Ownership Containers and Rule Table

| Container | Owns | Must not own |
|---|---|---|
| Authority / Provenance | truth, provenance, identity, semantic status, traceability, envelopes | interaction, rendering, consumer convenience, vendor lifecycle |
| Reasoning / Capability | strategy, capability, composition, transforms, derived outputs, interpretation, overlay definition | UI state, backend behavior, vendor adaptation |
| Process / Execution | workflow, sequencing, routing, fallback, runtime state, observability | semantic meaning, result identity, presentation behavior |
| Contract / Boundary | safe handoff, qualification, downstream constraints, consumer-agnostic output shape | backend quirks, UI-specific assumptions, semantic mutation |
| Projection / Translation | shape translation, mapping, adaptation, format conversion | semantic creation, policy ownership, confidence mutation |
| Consumer / Interaction | consumer adaptation, interaction meaning, local consumer state, explanation participation | truth authority, result composition, execution policy |
| Terminal Delivery | rendering, host binding, backend adaptation, vendor lifecycle | semantic interpretation, analytical composition, provider-independent truth |
| Governance / Evidence | validation, parity, reachability, audit, qualification evidence, promotion/quarantine records | live semantic control, execution authority, hidden routing |


### 15.1 Boundary Rules

| Rule | Meaning |
|---|---|
| Authority direction | Canonical meaning flows downstream. Downstream layers may annotate, project, bind, or deliver meaning; they must not redefine it. |
| Provenance preservation | Every meaningful transformation or projection must preserve provenance, document loss, or explicitly declare acceptable loss. |
| Consumer neutrality | Before terminal delivery, output should be represented in a consumer-neutral shape wherever practical. |
| Qualification before delivery | Provider, backend, adapter, and delivery compatibility must be explicit before delivery occurs. |
| Projection containment | Projection may support compatibility but must remain non-authoritative and must not silently discard metadata or provenance. |
| Evidence separation | Evidence, diagnostics, parity, reachability, validation, and audit may observe and prove behavior; they must not control live execution unless deliberately promoted into named policy. |
| No replacement mega-object | No new concept, surface model, contract, or construction algebra may become a universal pseudo-core that re-centralizes unrelated concerns. |

---

## 16. Formal Coverage Model

Use the following relation when evaluating whether requirements, language, construction, and documentation remain aligned:

```text
R = requirement space
L = formal architectural language / grammar
A = intended architectural expression
C = implemented construction set
E = documented architectural expressions
```

Required pressure:

```text
improve R -> L coverage
reduce harmful many-to-one language collapse
identify missing formal language where requirements cannot be expressed without semantic loss
separate vocabulary growth from construction growth
evolve from architectural grammar toward construction algebra only when supported by evidence
```

Coverage statuses:

| Status | Meaning |
|---|---|
| covered | language has stable conceptual expression |
| partially covered | concept exists but is not cleanly separated |
| collapsed | multiple concerns share one expression and require watching |
| ambiguous | ownership or placement is not canonical |
| missing | required concept lacks mature expression |
| deferred | valid concept not yet exercised by a current scenario |

Key rule:

```text
A requirement is not safely absorbed because code exists.
It must also be speakable in the grammar without semantic loss.
```

---


### 16.1 Coverage Pressure Areas

The coverage model may expose language pressure without immediately demanding construction.

| Pressure area | Clarification required |
|---|---|
| Confidence | formal place for reliability, ambiguity, uncertainty, and quality without becoming correctness |
| Envelope | formal place for outer contract guarantees, not merely payload shape |
| Interpretation | separation between semantic explanation and labels/display names |
| Policy | explicit decision rules rather than hidden branches |
| Binding | explicit connection between contract, consumer, provider, and delivery |
| Registry | declaration of available candidates without becoming hidden runtime policy |
| Lifecycle | runtime-state boundaries without becoming semantic ownership |
| Interaction | behavior requests against output without analytical authority |

### 16.2 Collapsed Concern Watchlist

Some collapses may be acceptable temporarily when repeated use proves a shared shape. They remain risks if they hide distinctions.

```text
Contract and Capability may share a carrier, but capability describes analytical power while contract governs handoff.
SurfaceModel and Consumer may be named together, but surface is the neutral output shape while consumer is the receiver.
Projection and Adapter may sit near each other, but projection translates shape while adapter applies output to a boundary.
Evidence and Validation may use the same supporting infrastructure, but evidence observes while validation checks rules.
```

### 16.3 Ambiguity Watchlist

```text
Transformation vs Projection: analytical change with lossiness/reversibility semantics is not non-authoritative shape translation.
Provider vs Backend: source/provider of capability/support is not the runtime mechanism that delivers it.
Delivery vs Consumer: output mode or transport is not the actor/surface receiving it.
Semantics vs Interpretation: canonical meaning is not explanatory framing under assumptions.
Evidence vs Audit: observable proof is not the same as durable review record.
```

### 16.4 Construction Algebra Carry-Forward

Coverage findings feed construction algebra as conceptual input. They do not automatically create runtime types.

```text
Formalize only missing or collapsed concepts needed to preserve coherence.
Do not implement Confidence, Envelope, Policy, Registry, Lifecycle, or Interaction merely because they are named gaps.
Decide whether each gap is a true language failure, a tolerated abstraction, or a deferred scenario requirement.
Use Operation Chain and future derived-dataset work as pressure tests for arity, sequence, provenance, lossiness, reversibility, evidence sufficiency, and consumer projection.
```

## 17. Construction and Bounded Generativity Vocabulary

This section defines future-facing language. It is foundational, not an execution checklist.

| Construction band | Conceptual purpose | Key concepts |
|---|---|---|
| Construction Algebra | first formal construction layer above vocabulary and grammar | Construction, Operation, Relation, InputSet, OutputSet, DerivedSet, IntermediateSet, CompositionGraph, TransformationTrace, EvidenceTrace |
| Operation / Capability Algebra | capability as lawful composable analytical power | CapabilityPurpose, CapabilityPrecondition, CapabilityPostcondition, CapabilityArity, CapabilityCost, CapabilityCompatibility, CapabilityFitness |
| Typed Relation System | explicit, auditable, projectable relations | OwnershipRelation, DependencyRelation, DerivationRelation, ProjectionRelation, QualificationRelation, InterpretationRelation, EvidenceRelation, BoundaryRelation |
| Multiplicity / Derived Dataset | N-input, N-operation, N-output analytical construction | Multiplicity, Arity, Cardinality, Sequence, Chain, ManyToOne, OneToMany, ManyToMany, DerivedDatasetIdentity |
| Evidence Sufficiency / Promotion | promotion, retention, quarantine, rejection | EvidenceSufficiencyRule, PromotionRule, QuarantineRule, RejectionRule, GovernanceReviewRecord |
| Semantic / Interpretation / Assumption | semantic plurality without false certainty | AssumptionRecord, InterpretationContext, InterpretationModel, ConfidenceModel, ConflictRecord, ContrastiveExplanation |
| Analytical Fitness | valid vs useful vs meaningful | AnalyticalFitness, UsefulnessScore, SignalPreservation, DistortionProfile, InterpretationPotential |
| Bounded Search | tractable generative construction | BoundedSearchPolicy, OperationCostModel, CapabilityPruningRule, IncrementalRecomputation, DeterministicReplayRecord |
| Multi-Consumer Output | one truth, many downstream projections | GeneratedSurfaceModel, GeneratedChartConsumer, GeneratedReportConsumer, GeneratedApiConsumer, ConsumerProjectionRule |
| Governance / Emergence | distinguish healthy emergence from entropy | EmergenceReview, LanguageGrowthRecord, GovernanceDecision, CoherenceRisk, DriftSignal, BoundaryPressure |
| Scenario Hardening | bounded product/domain scenarios against the formal model | analytical workbench, dashboard runtime, personal analytics, evidence platform, AI-assisted governance |

Construction rule:

```text
Do not create runtime machinery merely because the language names a concept.
Formalize only what preserves coherence under real construction pressure.
```

---


### 17.1 Core Construction Laws

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

### 17.2 Construction Band Details

| Band | Additional concepts / laws to carry forward | Alignment focus |
|---|---|---|
| Construction Algebra | `ConflictRecord`, `PromotionRecord`, `QuarantineRecord`; arity, provenance, traceability, lossiness, qualification, boundary crossing | grammar -> algebra; classification -> construction; validity -> governed generativity |
| Operation / Capability Algebra | `OperationAlgebra`, `CapabilityPromotionRule`; precondition, postcondition, cost, compatibility, fitness | operation rules; capability rules; input/output discipline; lossiness and reversibility declaration |
| Typed Relation System | `ContradictionRelation`, `BoundaryRelation`; relation-specific evidence | edges, not only nodes; graph projection by purpose |
| Multiplicity / Derived Dataset | `DerivedDatasetIdentity`; intermediate-state visibility | Operation Chain pressure test; consumer-neutral derived outputs |
| Evidence Sufficiency / Promotion | `EvidenceTrace`, `AuditRecord`, `ConstructionDecisionRecord` | evidence-backed promotion; non-controlling evidence; reviewable governance |
| Semantic / Interpretation / Assumption | `WhyThisOutput`, `WhyThisDifference`, `WhyThisMatters` | semantic ambiguity; plural interpretation; trace vs explanation |
| Analytical Fitness | `InsightHypothesis`, `ConfidenceImpact`, `FitnessEvidence` | validity != usefulness; computability != meaning; analytical relevance over mere execution |
| Bounded Search | `AnalyticalManifest`, `DeterministicReplayRecord` | qualified search space; bounded depth; reproducibility; tractability |
| Multi-Consumer Output | `GeneratedTableConsumer`, `GeneratedChartConsumer`, `GeneratedExportConsumer`, `GeneratedEvidenceConsumer` | one truth, many projections; no replacement mega-object |
| Governance / Emergence | `PromotionJudgement`, `QuarantineJudgement`, `ConceptualRegression` | controlled language growth; evidence-sensitive evolution; drift detection |
| Scenario Hardening | Operation Chain Workbench, analytical workbench, dashboard runtime, personal analytics, evidence platform, compatibility framework, AI-assisted governance | scenario proof; formal coverage proof; construction algebra proof |

### 17.3 Governance Questions for Generated Construction

```text
Does this new construction preserve upstream authority?
Does it introduce new language because existing language was insufficient, or merely because construction became complex?
Does it clarify relations, or create another overloaded abstraction?
Does it preserve provenance, traceability, fidelity, and boundary clarity?
Does it allow generativity without turning generated output into unreviewed authority?
Does it make future interpretation clearer, or more ambiguous?
```

## 18. Review Gates

### 18.1 Ownership Gate

```text
Which ownership container owns this?
Which container must not own it?
Does this move authority downstream?
Does this hide policy in orchestration, rendering, UI, or evidence?
```

### 18.2 Boundary Gate

```text
What contract governs the handoff?
What provenance and metadata must be preserved?
What qualification rule applies?
Is the crossing inspectable?
```

### 18.3 Consumer-Neutrality Gate

```text
Is there a consumer-neutral shape before terminal delivery?
Does any UI or vendor type leak upstream?
Does the consumer receive meaning without redefining it?
```

### 18.4 Evidence Gate

```text
What evidence proves correctness or acceptable divergence?
Is evidence observational, or has it accidentally become live policy?
Is the audit durable and reviewable?
```

### 18.5 Generalization and Hub-Absorption Gate

```text
Has the shared shape been proven by more than one real construction?
Which real family, consumer, or delivery differences must remain visible?
Does this abstraction hide provider, backend, evidence, capability, or semantic policy?
Which existing hub is explicitly prevented from absorbing the responsibility?
Does the change replace one pseudo-core with another?
Does the change preserve the separation between validity, usefulness, traceability, explanation, and meaning?
```

Generalization is not justified merely because code repeats. It is justified only when repeated structure is real, boundary-safe, provenance-preserving, and does not flatten meaningful differences.

---


### 18.6 Historical Evidence Abstraction Gate

When an audit, compatibility note, progress artifact, or construction document contributes to this vocabulary, extract only the stable conceptual rule:

```text
What ownership distinction was clarified?
What boundary became visible?
What concept was overloaded or missing?
What pressure point should remain conceptually visible?
What rule prevents future semantic drift?
```

Do not import:

```text
artifact locations
task sequences
status claims
validation-count inventories
manual validation notes
agent instructions
file-by-file validation evidence
```

Historical evidence can inform the grammar, but the grammar must remain construction-independent.

### 18.7 Prime Directive Coverage Gate

Before post-convergence growth, answer:

```text
Which requirement does this construction satisfy?
Which formal language term expresses it?
Does the construction collapse distinct concerns into one overloaded term?
Does the current language fail to express it without semantic loss?
Which operation, relation, arity, boundary, evidence, or promotion rule applies?
Does this improve bounded generativity without weakening coherence?
```

## 19. Supplemental Pattern Vocabulary

### 19.1 Composite Pattern Reminder

```text
ChartController
ControllerAdapter
RenderRequest
RenderHost
RenderSurface
RenderPlan
RenderModel
RenderAdapter
RenderingContract
RenderingRoute
RenderingQualification
QualificationProbe
RouteResolver
StrategyFactory
StrategySelection
StrategyValidation
DataContext
DataResolution
SelectionState
SessionRecorder
DiagnosticsSnapshot
EvidenceBuilder
ParityEvaluator
ParityHarness
WorkflowCoordinator
OrchestrationPipeline
PreparationStage
InvocationStage
UpdateCoordinator
StateSync
VisibilityController
EventBinder
SurfaceCoordinator
ExportWriter
ReasoningEngine
ProgramPlanner
TransitionState
```

These expose recurring compound patterns already present in the codebase. They are not automatically ideal abstractions.

### 19.2 Earlier Seven-Part Diagnostic Grammar

```text
Flow / Control
Construction / Projection
Runtime / State
Rendering
Quality / Verification
UI Boundary
Domain / Service
```

This is a descriptive compression of current language, not the final target hierarchy.

---

## 20. Analysis Roadmap

Recommended conceptual analysis order:

| Order | Analysis | Purpose |
|---:|---|---|
| 1 | Target Ownership Matrix | define primary home, secondary zones, and allowed/forbidden responsibilities |
| 2 | As-Is Ownership Matrix | map where concepts currently live and where they are overloaded |
| 3 | Canonical Intent Model | unify user request, selected data, transformation/comparison mode, execution constraints, delivery constraints |
| 4 | Concept-Collision Map | identify recurring improper collapses |
| 5 | Hotspot-to-Zone Mapping | place concrete subsystem hotspots into target ownership zones |

Primary recommendation:

```text
If only one next conceptual analysis is performed, create the Target Ownership Matrix.
It is the shortest path from grammar to enforceable structural rules.
```

---

## 21. Conceptual Architecture Closure Order

This is not execution sequencing. It is the order in which architectural ownership should become clear and enforceable.

```text
Authority -> Intent -> Execution -> Process -> Contracts -> Consumers -> Projection -> Terminal Delivery
                                                \-> Governance / Evidence as observer
```

| Order | Closure concern | Meaning |
|---:|---|---|
| 1 | Authority spine + canonical intent | make authority, provenance, and intent explicit so `Context`, `State`, `Request`, and `Result` stop carrying blurred ownership |
| 2 | One primary execution model | make reasoning/program path the main story; demote competing paths into adapters, fallback, or observers |
| 3 | Reasoning vs process separation | keep strategy/capability/composition in reasoning; keep workflow/routing/fallback in process |
| 4 | Contract / boundary seam | preserve program, delivery, provider, render-plan, interaction, and multi-result contracts as fan-out seam |
| 5 | Consumer / interaction branching | model charts, exports, APIs, and future clients as real consumer families |
| 6 | Projection / translation containment | keep compatibility, consumer, and delivery projection non-authoritative and metadata-preserving |
| 7 | Terminal rendering demotion | push backend routing, qualification, host binding, and vendor behavior into terminal delivery |
| 8 | Evidence sidecar isolation | keep parity, evidence, reachability, qualification, and audit observational |
| 9 | Family-pattern consolidation | collapse repeated family micro-patterns only after the spine and contract seam are clear |

Risk note:

```text
Do not begin by collapsing family-specific micro-frameworks.
Without a clear authority spine, canonical intent, primary execution model, contract seam, consumer branch model, and non-authoritative projection seam, consolidation is cosmetic.
```

---


### 21.1 Conceptual Maturation Anchors

These anchors describe conceptual maturation only. They are not task sequences, progress labels, or execution checkpoints.

| Anchor | Conceptual role | Lesson |
|---|---|---|
| Structural spine | baseline clarity, density classification, authority separation, seam hardening, delivery demotion, evidence separation, capability-growth discipline | before capability growth can be trusted, the system must know where meaning lives and where it may travel |
| Consumption and convergence | consumer transition, surface-model proof, bridge containment, consumer/interaction separation, terminal rendering confirmation | a target spine becomes architecture only when production consumers use it without reintroducing hidden legacy authority |
| Operation Chain pressure test | bounded N-input / N-operation / derived-output analysis | derived analytical power requires identity, arity, provenance, reversibility/lossiness, traceability, evidence, and consumer projection |
| Requirements-to-language coverage | check whether requirements can be expressed without semantic loss | a requirement is not safely absorbed until it is implementable and speakable in the grammar |
| Construction algebra and bounded generativity | move from vocabulary to lawful construction | generativity becomes safe only when construction has laws, evidence, constraints, and promotion rules |

Conceptual maturation sequence:

```text
stabilization
  -> seam hardening
  -> consumer transition
  -> convergence
  -> formalisation
  -> productization / scenario hardening
```

## 22. Retained Conceptual Pressure Points

```text
ChartDataContext remains a known consumption-model pressure point.
ChartRenderPlan and ConsumerSurfaceModel are current expressions of the consumer-neutral surface seam.
VNextUiConsumptionContract is the current code-facing expression of the UI consumption contract concept.
LegacyChartProgramProjector, VNextDataResolutionHelper, service-backed metric loading, strategy cut-over, and parity/evidence paths are bounded compatibility and projection pressure.
Legacy may remain as compatibility / fallback / projection, but not as forward architectural truth.
Operation Chain is the first bounded pressure test for N-input / N-operation / derived-output construction language.
Construction algebra should begin from inventory and evidence, not broad runtime machinery.
Confidence and Envelope are known formal pressure points.
Evidence/Validation and Contract/Capability are collapsed-concern risks.
Interaction remains valid even where active interaction workflow is deferred.
Dense hubs are inspection candidates, not automatic violations.
Integration hubs may coordinate but must not absorb authority, capability, provider policy, evidence policy, or delivery policy.
Mapping, conversion, formatting, and tooltip participation must not mutate provenance, confidence, or canonical result values.
Generated constructions must not be promoted without evidence sufficiency and governance review.
```

---

## 23. What Must Not Live in This Document

```text
work sequencing
status tracking
validation inventories
manual verification notes
file-by-file evidence records
bridge-retirement tracking
archive listings
IDE-agent instructions
```

Historical evidence may inform the grammar, but this document should retain only the stable conceptual rule extracted from that evidence.

---


### 23.1 Derivational Preservation Guardrail

The derivational spine of this document must be preserved during future compaction.

Do not silently delete or flatten:

```text
exploratory grammar pools
do-not-collapse algebra
grooming rules
concept promotion result
concept relationship graph
atomic parentage map
relationship patterns
compound strength notes
reduced atomized grammar
reduced compound grammar
target architectural code-map vocabulary
target architecture graph
current project code map
current-vs-target comparison rules
```

If compaction is required, move this material to a named appendix and cross-reference it explicitly.

## 24. Deferred Wider-Platform Coverage Questions

These belong to the broader `DataAnalyser` / canonical data reasoning platform and should not be forced prematurely into the current `DataVisualiser` grammar.

| Area | Current coverage | Deferred conceptual gap |
|---|---:|---|
| Raw data ingestion | weak / implied | source intake, source trust, schema discovery, raw preservation, ingestion fidelity |
| Canonical data modeling | partial | object identity, normalization, semantic typing, temporal identity, stable conceptual representation |
| Cross-source reconciliation | partial | relation/conflict language between sources, measurements, derived entities, contradictory records |
| DataFileReader relationship | weak | upstream ingestion/unification relation without collapsing both documents |
| Ontology / semantic domain model | partial | domain concept formation, metric identity, entity identity, semantic evolution, controlled concept promotion |
| Storage / persistence authority | weak | snapshots, replay, versioned truth, persisted provenance, persistence boundaries |
| AI-assisted reasoning / governance | partial | AI as consumer, planner, reviewer, governance participant, or bounded construction assistant without hidden authority |
| User / workbench / product experience | partial | product-level interaction, analytical workbench concepts, user-facing reasoning workflows |

Deferred coverage rule:

```text
Do not force these into DataVisualiser prematurely.
Do not ignore them when the broader platform requires them.
Promote them only when they preserve authority, provenance, reasoning, contract, consumer, delivery, and evidence boundaries.
```

---

## 25. Final Takeaway

The codebase should not evolve toward a chart-centered or rendering-centered architecture.

It should evolve toward:

```text
truth-centered authority
reasoning-centered composition
contract-driven delivery
consumer-thin interaction
replaceable terminal presentation
observational governance and evidence
```

That is the conceptual destination.
