# DataVisualiser Architectural Vocabulary and Target Hierarchy

## Purpose

This document condenses the type-dependency discussion into a cleaner reference.

It does four things:

1. reduces the codebase naming down to reusable architectural vocabulary
2. shows the conceptual map that sits beneath the project-specific terms
3. contrasts the current generalized shape with the desired target shape
4. captures the corrected end-state: engine-centered, contract-driven, presentation-subordinate, and rendering-disposable

---


## Formal Interpretation Note

This document translates the project's higher-order axioms into architectural vocabulary and target ownership language.

The vocabulary is intentionally stable, but not mathematically absolute. It represents the current best abstraction of discovered project concepts, including authority, provenance, capability, composition, consumer, interaction, boundary, overlay, evidence, and the related ownership containers.

These concepts should guide implementation, architectural classification, and responsibility placement, but they should not be treated as closed formal theory. As the architecture converges, especially through reasoning-engine unification, programmable composition, confidence handling, consumer-general contracts, and future analytical capability, the vocabulary may be refined without violating its current purpose.

The goal is lawful adaptability: future flexibility must preserve truth, provenance, reversibility, and user authority without collapsing back into ad hoc feature growth or presentation-driven semantics.

---

---

## 1. Vocabulary Reduction

### 1.1 Atomic conceptual terms

These are the stable architectural words that remain after stripping chart-family and scenario-specific qualifiers:

- Adapter
- Args
- Authority
- Boundary
- Builder
- Capabilities
- Capability
- Composition
- Configuration
- Consumer
- Context
- Contract
- Controller
- Coordinator
- Data
- Defaults
- Diagnostics
- Engine
- Evaluator
- Event
- Factory
- Guard
- Helper
- Host
- Interaction
- Invoker
- Kernel
- Loader
- Manager
- Mode
- Model
- Orchestrator
- Overlay
- Pipeline
- Plan
- Policy
- Probe
- Provider
- Provenance
- Qualification
- Registry
- Renderer
- Rendering
- Request
- Resolver
- Result
- Route
- Selector
- Service
- Snapshot
- Stage
- State
- Strategy
- Surface
- Tooltip
- Tracker
- Transition
- Validator
- ViewModel
- Workflow

#### Promoted concept glossary

These terms should be treated as promoted architectural concepts, not generic vocabulary.

- **Authority** — the right to define semantic meaning, trust status, and architectural legitimacy.
- **Provenance** — visible lineage of where data, results, interpretations, and derived outputs came from.
- **Capability** — a declared system ability that can be reasoned about, qualified, composed, and reused.
- **Composition** — the structured combination of inputs, operations, overlays, and results into an analytical output.
- **Consumer** — any downstream client of engine-owned output, including charts, exports, APIs, or future interfaces.
- **Interaction** — user or consumer behavior expressed through contracts without redefining semantic meaning.
- **Boundary** — an architectural seam that controls what may cross between responsibility containers.
- **Overlay** — a downstream interpretive layer applied over authoritative output without mutating truth.

---

#### Do-not-confuse distinctions

These distinctions prevent semantic drift when applying the vocabulary.

- **Capability is not Feature**  
  A feature is user-visible behavior; a capability is an architectural ability that may support many features.

- **Consumer is not Presentation**  
  A consumer may be visual, exported, API-based, automated, or future-facing; presentation is only one terminal form.

- **Interaction is not Event**  
  An event is a technical signal; interaction is the meaningful consumer-side behavior represented by that signal.

- **Composition is not Builder**  
  A builder constructs an object; composition defines how analytical parts are lawfully combined.

- **Boundary is not Layer**  
  A layer groups responsibilities; a boundary governs what may cross between them.

- **Overlay is not Rendering**  
  An overlay is interpretive content; rendering is only one way to deliver it.

- **Provenance is not Diagnostics**  
  Provenance belongs to truth and result lineage; diagnostics observe system behavior.

- **Authority is not Orchestration**  
  Orchestration coordinates execution; authority defines what may count as meaning.

---

### 1.4 Higher-order composite concepts

These combinations are frequent enough to treat as recurring architectural patterns:

- ControllerAdapter
- DataContext
- DataResolution
- DiagnosticsSnapshot
- EventBinder
- EvidenceBuilder
- ExportWriter
- InvocationStage
- OrchestrationPipeline
- ParityEvaluator
- ParityHarness
- PreparationStage
- ProgramPlanner
- QualificationProbe
- ReasoningEngine
- RenderAdapter
- RenderHost
- RenderModel
- RenderPlan
- RenderRequest
- RenderSurface
- RenderingContract
- RenderingQualification
- RenderingRoute
- RouteResolver
- SelectionState
- SessionRecorder
- StateSync
- StrategyFactory
- StrategySelection
- StrategyValidation
- SurfaceCoordinator
- TransitionState
- UpdateCoordinator
- VisibilityController
- WorkflowCoordinator

### 1.5 Non-core specifics to strip away

These are useful implementation qualifiers, but not core architectural concepts:

- Main
- Secondary
- Weekday
- Trend
- Syncfusion
- Sunburst
- Bar
- Pie
- Cartesian
- Diff
- Ratio
- Normalized
- Combined
- Single
- Multi
- Hourly
- Weekly
- Cms
- Admin
- DateRange
- Load
- Toggle
- Zoom
- Theme

### 1.6 Reduced architecture grammar

The diagram collapses into a much smaller conceptual grammar:

- **Authority / Truth**: Authority, Provenance, Context, State, Mode, Request, Result, Snapshot, Model, Plan
- **Flow**: Coordinator, Orchestrator, Pipeline, Stage, Workflow, Transition, Guard
- **Execution**: Strategy, Service, Engine, Provider, Loader, Kernel, Capability, Composition
- **Projection**: Builder, Factory, Adapter, Resolver, Selector, Projector, Contract
- **Rendering**: Renderer, Route, Qualification, Host, Surface, Capabilities
- **Verification**: Validator, Evaluator, Probe, Harness, Diagnostics, Evidence, Parity
- **Presentation / Consumption**: Consumer, Controller, Interaction, ViewModel, Event, Args, Binder, Manager, Overlay, Boundary

### 1.7 Shortest useful reduced list

If the goal is the shortest practical architectural vocabulary, use:

- Authority
- Provenance
- Capability
- Composition
- Consumer
- Context
- State
- Request
- Result
- Model
- Plan
- Workflow
- Coordinator
- Pipeline
- Stage
- Strategy
- Service
- Engine
- Builder
- Factory
- Adapter
- Resolver
- Selector
- Contract
- Renderer
- Route
- Qualification
- Host
- Surface
- Controller
- Interaction
- ViewModel
- Validator
- Diagnostics
- Evidence

### 1.8 Immediate conclusion

The codebase is large, but the architectural language underneath it is much smaller.

That matters because the real refactoring leverage is likely to sit in the repeated concepts, not in the family-specific names.

---

---

## 2. Multi-Parent Concept Map

A strict tree is too rigid for this architecture. The same concept often plays more than one legitimate role.

Examples:

- `StrategySelection` belongs to both flow and execution
- `RouteResolver` belongs to both projection and rendering
- `ControllerAdapter` belongs to both projection and presentation
- `DiagnosticsSnapshot` belongs to both structure and diagnostics
- `RenderPlan` belongs to both structure and rendering

That overlap is not a flaw. It is the point of the concept map.

### Concept map

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

### Why this matters

This lets us ask sharper questions:

- Which concepts are overloaded across too many zones?
- Which zones are doing work that belongs elsewhere?
- Which concept chains are too long?
- Which concept pairs are collapsing into one another?
- Which concepts are missing a true parent abstraction?

Likely high-overlap concepts are:

- Context
- State
- Coordinator
- Strategy
- Adapter
- Resolver
- Render*
- Diagnostics / Evidence
- Controller

---

---

## 3. Structural Diagnosis Lens

The refined vocabulary and target hierarchy become more useful when read through the structural diagnosis of the current codebase.

The most important additional conclusion is this:

The project is not under-architected.  
It is **over-articulated**.

The main issue is not lack of layering, but the coexistence of too many parallel abstractions, repeated per feature family, with several transitional architectures cross-coupling the same concepts.

### 3.1 Architectural multiplicity

A useful way to read the current structure is as several concurrent architectural tracks operating at once:

- a legacy/core computation path
- an orchestration pipeline path
- a UI controller / adapter path
- a VNext reasoning / program / render-plan path

Individually, each path can make sense.  
Together, they create duplicate representations of the same business intent.

This strengthens an earlier conclusion in this document:

- the biggest weakness is not raw code volume
- it is **parallel representations of the same intent and flow**

### 3.2 Centers of truth and intent fragmentation

The structural assessment sharpened an especially important problem:

the same conceptual decision appears to be expressible in multiple places, including:

- strategy selection
- orchestration stage selection
- controller routing
- render qualification / routing
- program planning

That means there is no single abstraction that fully owns:

- what should be produced
- from which data
- through which execution path
- through which delivery path
- under which mode or constraints

This reinforces the need for the hierarchy in Section 7, especially:

- **Truth Authority**
- **Reasoning Core**
- **Application Process**
- **Provider Contract Boundary**

### 3.3 Repetition pressure in feature families

The structural assessment also confirmed that many feature-family-specific structures appear to be repeated variants of the same deeper pattern.

Recurring concepts include:

- request
- host
- route
- qualification
- capabilities
- contract
- adapter
- builder
- resolver
- probe

This does not automatically mean those abstractions are wrong.
It does mean the system is likely paying a structural tax for expressing the same architectural pattern many times under family-specific names.

That observation strengthens the reduction approach in Section 1:
the vocabulary reduction is not cosmetic; it exposes the repeated skeleton underneath the family-specific surface area.

### 3.4 UI as accidental integration shell

Another useful non-conflicting conclusion is that the UI/presentation area has historically carried too much architectural weight.

Instead of acting only as a downstream consumer boundary, it has also acted as an integration shell for:

- coordination
- workflow control
- diagnostics gathering
- state synchronization
- controller routing
- render-surface mediation

This aligns directly with the later clarification in Sections 6 and 7:
presentation cannot be truthfully treated as terminal and replaceable until non-terminal responsibilities are extracted upward.

### 3.5 Coordinator and adapter proliferation

The structural assessment also gives extra support to the overlap analysis in Section 4.

Large numbers of:

- coordinators
- adapters
- resolvers
- builders
- evaluators
- probes

usually indicate one of two things:

- healthy decomposition
- or insufficiently strong core abstractions that force the architecture to compensate with many support types

Here, the likely reading is mixed, but weighted toward the second interpretation often enough to matter.

That reinforces why these concepts were identified earlier as high-overlap and high-risk.

### 3.6 Rendering abstraction pressure

The structural assessment also sharpened a useful distinction:

the problem is not simply that rendering has many types.
It is that rendering has tended to grow into a sub-architecture with its own repeated micro-frameworks.

That supports the later correction in this document:
rendering should not remain a peer architectural center.
It should be demoted beneath stronger upstream concepts:

- reasoning/program output
- provider contracts
- consumer boundaries

#### Partial correction via folder restructure (April 2026)

A structural folder restructure partially corrected a symptom of this pressure.

The previous layout placed rendering-related sub-backends (`ECharts/`, `LiveCharts/`) one level deeper inside `UI/Charts/Presentation/Rendering/`, which gave `Rendering` the appearance of a nested architectural center within the UI boundary.

The restructure:
- flattened `UI/Charts/Presentation/Rendering/` into `UI/Charts/Presentation/`, promoting the root-level rendering types directly into the consumer/interaction field
- relocated the vendor-specific sub-backends (`ECharts/`, `LiveCharts/`) as siblings at `UI/Charts/Presentation/ECharts/` and `UI/Charts/Presentation/LiveCharts/`, making their terminal nature more visible
- moved Syncfusion types from `UI/Syncfusion/` to `UI/Charts/Syncfusion/`, co-locating them with the rest of the consumer/interaction field rather than treating them as a top-level UI concern
- consolidated controller interfaces from `UI/Charts/Interfaces/` into `UI/Charts/Presentation/`, making it clearer that controller interfaces and their adapters belong to the same coherent consumer/interaction container
- extracted tooltip helpers from `Core/Rendering/Helpers/` into `Core/Rendering/Tooltip/`, separating pure rendering support from general helper utilities

This does not yet fully demote rendering to Terminal Delivery Infrastructure as described in Section 8.3.
The `Core.Rendering` layer remains structurally large.
But the folder layout now better reflects the intended ownership shape: vendor backends are visibly terminal, and controller interfaces live within the consumer/interaction field where they belong.

### 3.7 Migration architecture must not become steady-state architecture

A further non-conflicting conclusion is that the migration machinery is at risk of becoming part of the permanent architecture if it is not kept bounded.

The presence of:

- cutover services
- parity harnesses
- evidence builders
- legacy projectors
- VNext coordinators
- runtime diagnostics

is valid during transition.

But these concepts must remain either:

- transitional adapters
- observational mechanisms
- or explicitly promoted primary architecture

The expensive state is to let them remain half-integrated indefinitely.

### 3.8 Canonical intent remains the missing center

The single strongest practical insight from the structural assessment is still this:

the architecture likely needs one canonical application-level intent model that states, in one place:

- what the user wants
- what data is selected
- what transformation/comparison mode applies
- what delivery constraints exist
- what execution path is allowed

That intent should drive downstream:

- strategy selection
- execution flow
- program planning
- delivery planning
- consumer adaptation

This fits cleanly with the hierarchy already developed here.
It is the missing center that would reduce duplicated control logic across process, projection, delivery, and consumer layers.

### 3.9 Practical synthesis

Taken together, the structural assessment does not overturn the refined target hierarchy.

It strengthens it.

It suggests that the refined hierarchy is not merely philosophically cleaner; it is also the right response to the concrete structural pathologies already present:

- too many parallel execution stories
- too many repeated feature-family micro-patterns
- too much presentation-adjacent ownership
- too many support abstractions compensating for blurred centers
- too much transition logic embedded into steady-state structure

In short:

- the vocabulary reduction identifies the repeated conceptual skeleton
- the overlap analysis identifies the unstable concepts
- the terminal-layer clarification demotes presentation correctly
- the structural assessment explains **why** those corrections are necessary


---

---

## 4. Generalized As-Is vs Target

### 4.1 As-is generalized architecture

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

### 4.2 What this implies

The current system is concept-dense and role-overlapped.

It appears dominated by:

- too many flow-control concepts
- too many projection mechanisms
- rendering treated as a large subsystem
- presentation carrying architectural behavior
- diagnostics deeply embedded
- structural carriers like `Context`, `State`, `Request`, and `Result` appearing everywhere

### 4.3 Earlier generalized target shape

The earlier cleaned target looked like this:

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

---

---

## 5. Refined Overlap Analysis

The target architecture is not trying to optimize for generic software neatness.

It is trying to preserve:

- truth discipline
- the reasoning engine as the center
- delivery surfaces as non-authoritative consumers
- a safe path for Phase 7 capability growth

### 5.1 Highest-risk overlap concepts

#### State
Most dangerous.
If semantic, workflow, presentation, and diagnostic state blur together, trust collapses.

Rule:
- semantic state must stay upstream
- presentation state must not become semantic authority
- diagnostic state must remain observational

#### Context
Extremely risky.
A broad context object is one of the easiest ways to smuggle authority across layers.

Rule:
- no universal context
- keep context boundary-local and purpose-specific

#### Coordinator
High risk.
Coordinators are supposed to sequence and delegate, not think.

Rule:
- no semantics
- no backend knowledge
- no hidden policy

#### Rendering / Render*
High risk.
Rendering is important, but it must not become a second semantic or orchestration core.

Rule:
- rendering owns delivery
- it does not own meaning or interpretive policy

#### Strategy
Important, but bounded.
Strategy defines behavior; selection and validation should not be collapsed into the same thing.

#### Controller
Risk lies in convenience leakage.
Controllers must remain downstream request translators, not authorities.

#### Adapter
Necessary, but risky when it hides policy.

#### Resolver
Useful, but often becomes disguised orchestration.

#### Diagnostics / Evidence
Essential, but must remain observational.

### 5.2 Refined risk ranking

1. State
2. Context
3. Rendering / Render*
4. Coordinator
5. Strategy
6. Controller
7. Diagnostics / Evidence
8. Adapter
9. Resolver

### 5.3 Most vulnerable zones

- **Semantic Core** — sacred; cannot absorb interpretive or presentation concerns
- **Application Flow** — sequencing can quietly become meaning
- **Rendering Boundary** — easiest place for delivery logic to grow too large
- **Presentation Boundary** — easiest place for convenience leakage to harden into architecture

### 5.4 Concept pairs to watch closely

- State + Context
- Coordinator + Strategy
- Coordinator + Controller
- Render + Strategy
- Render + Controller
- Diagnostics + Execution
- Adapter + Resolver

### 5.5 Missing parent abstractions

- **Truth Envelope** for Context / State / Request / Result
- **Execution Process** for Workflow / Coordinator / Pipeline / Stage / Transition
- **Projection Layer** for Builder / Adapter / Resolver / Selector / Projector
- **Delivery Boundary** for Renderer / Route / Qualification / Contract / Host / Surface

### 5.6 Boundary rules

- Semantic Core owns truth status and provenance
- Application Flow owns sequencing, not meaning
- Execution owns behavior, not authority routing
- Projection owns translation, not semantic choice
- Rendering owns delivery, not interpretation
- Presentation owns interaction, not orchestration
- Diagnostics owns observation, not live control

---

---

## 6. Critical Architectural Clarification

The most important later clarification is this:

Rendering is not just modular. It is **disposable infrastructure**.

The cleaner rule is:

- **engines define**
- **providers expose**
- **presentation consumes**
- **rendering is swappable**

That means the real task is not merely to make rendering replaceable.

It is to remove non-terminal responsibility from the terminal layer until replacement becomes honest and cheap.

### 6.1 What belongs above the terminal layer

Anything that is really:

- reasoning
- planning
- shaping
- transformation
- capability selection
- interaction semantics
- backend-agnostic delivery contracts
- non-UI-specific view-state logic

must be extracted upward.

### 6.2 Terminal-layer test

A terminal layer should only:

- bind to contracts
- render supplied structures
- relay interaction events upward
- manage local lifecycle quirks
- provide vendor-specific surface behavior

It should not decide:

- what gets shown semantically
- how results are composed
- how execution is routed
- what interaction means
- what result shape must exist

If those concerns still live there, the layer is not terminal.

### 6.3 Updated boundary statement

The earlier rule:

- **Rendering owns delivery, not interpretation**

should be tightened to:

- **Presentation owns visualization only; engine-owned contracts remain authoritative; rendering is fully replaceable.**

---

---

## 7. Reshaped Optimal Concept Hierarchy

Once rendering is demoted properly, the cleaner end-state is:

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
├── 5. Projection and Translation
│   ├── Builder
│   ├── Adapter
│   ├── Resolver
│   ├── Selector
│   ├── Projector
│   └── Formatter / Converter
│
├── 6. Consumer Boundary
│   ├── Controller
│   ├── ViewModel
│   ├── Event / Binder
│   ├── Interaction Relay
│   ├── Consumer State
│   └── Host Coordination
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

### 7.1 Why this is better

This hierarchy fits the project better because it:

- keeps truth authority upstream
- makes the reasoning engine the center
- makes process subordinate to meaning
- introduces the missing middle of provider contracts
- demotes presentation to a consumer role
- demotes rendering to replaceable infrastructure

### 7.2 Main conceptual promotion

The most important promotion is:

- **Provider Contract Boundary**

Without this layer, rendering remains too important.

### 7.3 Main conceptual demotion

The most important demotion is:

- **Rendering Boundary -> Terminal Presentation Infrastructure**

Rendering is not a peer subsystem. It is the last step in delivery.

### 7.4 Cleanest short form

```text
Truth -> Reasoning -> Process -> Contract -> Consumer -> Terminal Presentation
                           \-> Governance / Evidence
```

Or, in words:

- **Truth defines**
- **Reasoning composes**
- **Process coordinates**
- **Contracts carry**
- **Consumers adapt**
- **Presentation displays**
- **Evidence proves**

---

---

## 8. Enhanced Architecture

The first enhanced architecture was useful, but it still read too much like a linear extension of the earlier target model.

With the expanded vocabulary now available, the architecture can be improved more materially.

The key shift is this:

the new concepts are not just extra labels.  
They justify a stronger architecture built around **containers of responsibility** rather than a simple downstream line.

In particular:

- **Authority** and **Provenance** make truth-carrying concerns more explicit
- **Capability** and **Composition** make the reasoning layer richer and less builder-centric
- **Consumer** and **Interaction** make downstream branching a first-class architectural fact
- **Boundary** clarifies that several containers exist mainly to prevent semantic drift
- **Overlay** gives interpretive branching a cleaner home

So the enhanced architecture should not be read as:

> target architecture + extra nouns

It should be read as:

> a better structural decomposition made possible by the richer vocabulary

### 8.1 Enhanced architecture principles

The improved structure is guided by these principles:

1. **Authority remains singular**
   - meaning, trust class, and semantic legitimacy still have one upstream spine

2. **Reasoning becomes capability-oriented**
   - the system should be able to express transforms, compositions, overlays, and future analytical programs as explicit capabilities, not as feature-specific exceptions

3. **Contracts become a true fan-out seam**
   - downstream variation should begin at contracts, not earlier

4. **Consumers become first-class**
   - the architecture should model charts, exports, APIs, and future clients as real consumer families, not as special cases of presentation

5. **Interaction is separated from meaning**
   - interaction should no longer be implicitly buried inside controllers, renderers, or UI-specific event flow

6. **Overlay becomes explicit**
   - interpretive overlays should not remain half-owned by rendering or presentation

7. **Presentation remains terminal**
   - even with richer branching, the terminal layer stays replaceable and non-authoritative

---

### 8.2 Enhanced current architecture

This version of the current shape uses the enriched vocabulary while preserving the same diagnosis.

```text
ENHANCED CURRENT ARCHITECTURE
├── Authority / Truth Carriers
│   ├── Context
│   ├── State
│   ├── Request
│   ├── Result
│   ├── Model
│   ├── Plan
│   └── Snapshot
├── Flow-Control Mesh
│   ├── Coordinator
│   ├── Orchestrator
│   ├── Pipeline
│   ├── Stage
│   ├── Workflow
│   ├── Transition
│   ├── Resolver
│   └── Selector
├── Execution / Strategy Layer
│   ├── Strategy
│   ├── Service
│   ├── Engine
│   ├── Provider
│   ├── Loader
│   ├── Kernel
│   └── Capability
├── Projection / Mediation Layer
│   ├── Builder
│   ├── Factory
│   ├── Adapter
│   ├── Resolver
│   ├── Selector
│   ├── Contract
│   └── Composition
├── Rendering / Delivery Layer
│   ├── Renderer
│   ├── Route
│   ├── Qualification
│   ├── Host
│   ├── Surface
│   ├── Capabilities
│   ├── Interaction
│   └── Overlay
├── Presentation / Consumer Layer
│   ├── Consumer
│   ├── Controller
│   ├── ViewModel
│   ├── Event
│   ├── Args
│   ├── Binder
│   ├── Manager
│   └── Boundary
└── Verification / Evidence Layer
    ├── Diagnostics
    ├── Evidence
    ├── Parity
    ├── Validator
    ├── Evaluator
    ├── Probe
    └── Harness
```

**Reading:**
- the current system still branches too early
- too many mediating abstractions sit between intent and result
- authority and provenance carriers exist, but are not dominant enough
- capability and composition are present, but are not cleanly centered upstream
- consumer and interaction concerns remain too close to orchestration and delivery

---

### 8.3 Enhanced target architecture

The better enhanced architecture is not a single line.

It is:

- an **authority spine**
- a **reasoning and composition container**
- a **process and contract transition**
- a **branching consumer field**
- a **terminal delivery layer**
- and an **observational governance sidecar**

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
├── 6. Terminal Delivery Infrastructure
│   ├── Renderer
│   ├── Render Surface
│   ├── Backend Adapter
│   ├── Capability Qualification
│   ├── Route / Host Binding
│   ├── Overlay Delivery
│   └── Vendor-Specific Behavior
│
└── 7. Governance and Evidence Sidecar
    ├── Diagnostics
    ├── Evidence
    ├── Parity
    ├── Reachability
    ├── Validation
    ├── Export / Audit
    └── Qualification Probes
```

---

### 8.4 Structural flow of the enhanced target

The improved structure should be understood in two ways at once.

#### A. Authority spine

```text
Authority -> Reasoning / Capability -> Process -> Contracts -> Consumers -> Terminal Delivery
                                  \-> Governance / Evidence
```

This preserves the strict downward flow of meaning.

#### B. Branching structure

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
   +---------> Consumer Family A -> Terminal Delivery A
   |
   +---------> Consumer Family B -> Terminal Delivery B
   |
   +---------> Consumer Family C -> Terminal Delivery C

Governance / Evidence observes the spine and branch behavior from the side.
```

This is what materially changes the architecture.

The added concepts do not merely rename parts of the old target.
They allow a more accurate shape:

- **Authority** clarifies the upstream source of legitimacy
- **Capability** and **Composition** turn the reasoning container into a generative core
- **Consumer** makes branching explicit instead of accidental
- **Interaction** moves out of vague UI space into a real architectural field
- **Boundary** makes the contract seam more defensible
- **Overlay** becomes a defined downstream interpretive construct rather than a rendering convenience

---

### 8.5 Why these new containers matter

#### 1. Authority Container
This is stronger than a generic semantic core.

It explicitly holds:
- truth legitimacy
- provenance
- identity
- canonical trust status

Without this container, `Context`, `State`, `Request`, and `Result` remain too generic and risk turning into transport bags.

#### 2. Reasoning and Capability Container
This is stronger than a plain execution layer.

It gives a home to:
- strategies
- transforms
- comparative logic
- compositions
- overlays as declared outputs
- analytical program planning

Without this container, programmable growth risks falling back into feature-specific orchestration.

#### 3. Process and Execution Container
This prevents process concerns from polluting either meaning or consumer delivery.

It keeps:
- sequencing
- delegation
- routing
- fallback
- runtime observability

separate from truth and separate from terminal interaction.

#### 4. Contract and Boundary Container
This is the most important structural improvement.

It is not just a passive handoff point.
It is the container that:
- standardizes what may flow downstream
- protects upstream meaning from consumer drift
- enables lawful branching
- allows multiple consumer families to exist without architectural duplication

This is the real turning point in the whole architecture.

#### 5. Consumer and Interaction Field
This is richer than a simple presentation boundary.

It makes explicit that the system may have:
- multiple consumers
- multiple interaction styles
- multiple host behaviors
- multiple consumer states

without letting any of them become semantic authorities.

#### 6. Terminal Delivery Infrastructure
This keeps rendering, backend adaptation, and vendor-specific behavior where they belong: terminal, replaceable, and subordinate.

#### 7. Governance and Evidence Sidecar
This remains outside the main authority path.

Its role is:
- observation
- validation
- qualification
- proof
- audit

not live semantic control.

---

### 8.6 What has materially changed from the earlier target

The earlier target architecture mainly corrected direction.

This enhanced architecture improves **structure**.

#### Earlier target:
- mostly linear
- clearer than the current architecture
- but still read as a cleaned-up stack

#### Enhanced target:
- spine + containers + lawful branching
- stronger separation between authority, capability, contracts, consumers, and delivery
- clearer motivation for the new concepts
- better fit for multi-consumer, programmable, overlay-capable growth

So the enhanced architecture is not just more detailed.
It is a better **organizational geometry**.

---

### 8.7 Best short form

```text
Authority -> Reasoning / Capability -> Process -> Contracts -> Consumers / Interaction -> Terminal Delivery
                                   \-> Governance / Evidence
```

Or, in words:

- **Authority defines**
- **Provenance preserves**
- **Capability enables**
- **Composition shapes**
- **Process coordinates**
- **Contracts constrain and carry**
- **Consumers interact**
- **Delivery displays**
- **Evidence proves**


---

---

## 9. Visual Contrast: Current vs Target Architecture Shapes

Two visual views are useful here:

1. the **chain of responsibility / main spine**
2. the **non-linear relation / flow shape**

The first shows authority and ownership order.  
The second preserves the legitimate branching and interaction structure.

### 9.1 Chain of responsibility / main spine

This view answers:

> Where should meaning and responsibility flow, in order?

#### Current architecture — implied main spine

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

**Reading:**
- the spine starts too high in the presentation-adjacent area
- computation and semantics are not clearly dominant
- rendering sits too centrally in the main path
- diagnostics often feel attached to the main path instead of side-observing it
- too many support abstractions sit inline

So the current chain is too mediation-heavy.

#### Target architecture — desired main spine

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
Terminal Presentation Infrastructure
```

**Side-observer:**

```text
Governance / Evidence / Diagnostics
        ^
        |
(observes truth, reasoning, process, and delivery,
 but does not define live meaning)
```

**Reading:**
- truth and reasoning become the real start of authority
- process coordinates, but does not define semantics
- contracts become the handoff boundary
- presentation becomes terminal and replaceable
- diagnostics move off the main spine

So the target chain is authority-clean.

#### Short contrast

```text
Current:
UI -> Coordination -> Mediation -> Compute/Render -> Surface

Target:
Truth -> Reasoning -> Process -> Contract -> Consumer -> Presentation
```

---

### 9.2 Non-linear relation / flow shape

This view answers:

> How do the parts relate, branch, and interact?

This is where the target keeps legitimate branching instead of becoming falsely linear.

#### Current architecture — non-linear relation shape

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

**Reading:**
- many-to-many relations
- coordination is central
- UI is entangled with orchestration
- rendering is not terminal enough
- state/context/request/result are shared too broadly
- diagnostics touch many layers, sometimes too directly

This is the braided mesh shape.

#### Target architecture — non-linear relation shape

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
| (canonical data, |     | (programs,     |     | (workflow, routing,  |
| provenance,      |     | strategy,      |     | coordination,        |
| trust, semantics)|     | transforms)    |     | execution control)   |
+------------------+     +-------+--------+     +----------+-----------+
                                 |                         |
                                 +------------+------------+
                                              |
                                              v
                               +--------------+---------------+
                               | Provider Contract Boundary   |
                               | (result, delivery, view,    |
                               | interaction, multi-result)  |
                               +------+------------+---------+
                                      |            |
                         +------------+            +-------------+
                         |                                        |
                         v                                        v
              +----------+-----------+               +------------+----------+
              | Consumer Boundary A  |               | Consumer Boundary B   |
              | (chart consumer)     |               | (export / API / other)|
              +----------+-----------+               +------------+----------+
                         |                                        |
                         v                                        v
              +----------+-----------+               +------------+----------+
              | Terminal Presentation|               | Terminal Presentation |
              | / Backend / Surface  |               | / Delivery Surface    |
              +----------------------+               +-----------------------+
```

**Reading:**
- one authoritative upstream center
- one contract fan-out point
- lawful downstream branching
- multiple consumers are first-class
- presentation is terminal
- diagnostics observe from the side

This is a rooted spine with disciplined branching.

---

### 9.3 Geometric contrast

#### Current geometry

```text
multi-center mesh
```

- branching happens early
- mediation happens everywhere
- ownership is blurred
- presentation-adjacent layers carry too much weight

#### Target geometry

```text
authoritative spine + downstream branching + side observers
```

- authority is linear
- branching is delayed
- contracts are the fan-out boundary
- presentation is lightweight and replaceable

---

### 9.4 Short summary

The cleanest way to use these diagrams is:

- **spine view** for authority and ownership
- **non-linear view** for legitimate branching and interaction shape

So:

- **spine view** asks: who should own meaning?
- **non-linear view** asks: how should the system lawfully branch and relate?


---

---

## 10. Supplemental Pattern Vocabulary

The earlier vocabulary-reduction draft contained a few useful emphases that remain compatible with this refined document and are worth preserving explicitly.

### 10.1 Composite pattern reminder

Beyond the atomic vocabulary, the following composite patterns remain useful as shorthand when reading or discussing the codebase:

- `ChartController`
- `ControllerAdapter`
- `RenderRequest`
- `RenderHost`
- `RenderSurface`
- `RenderPlan`
- `RenderModel`
- `RenderAdapter`
- `RenderingContract`
- `RenderingRoute`
- `RenderingQualification`
- `QualificationProbe`
- `RouteResolver`
- `StrategyFactory`
- `StrategySelection`
- `StrategyValidation`
- `DataContext`
- `DataResolution`
- `SelectionState`
- `SessionRecorder`
- `DiagnosticsSnapshot`
- `EvidenceBuilder`
- `ParityEvaluator`
- `ParityHarness`
- `WorkflowCoordinator`
- `OrchestrationPipeline`
- `PreparationStage`
- `InvocationStage`
- `UpdateCoordinator`
- `StateSync`
- `VisibilityController`
- `EventBinder`
- `SurfaceCoordinator`
- `ExportWriter`
- `ReasoningEngine`
- `ProgramPlanner`
- `TransitionState`

These should not be mistaken for ideal end-state abstractions.
They are useful because they expose the recurring compound patterns already present in the codebase.

### 10.2 Earlier grammar emphasis worth retaining

The earliest reduction also highlighted a simpler seven-part grammar that is still useful as a compact reading aid:

- **Flow / Control**
- **Construction / Projection**
- **Runtime / State**
- **Rendering**
- **Quality / Verification**
- **UI Boundary**
- **Domain / Service**

This is not the final target hierarchy.
It remains valuable as a diagnostic shorthand for reading the existing architecture before applying the more corrected engine-centered model from Sections 6 to 8.

### 10.3 Why retain this older lens

The refined document intentionally corrected and tightened the target direction.

However, the original reduction remains useful in one respect:
it captures the architecture as a repeated **concept grammar** before stronger judgments are imposed about what should be promoted, demoted, or re-owned.

So the older lens should be treated as:

- a good **descriptive compression** of the current language
- but not the final **normative hierarchy**

That distinction helps preserve both:
- descriptive honesty about the current codebase
- and conceptual clarity about the desired end-state


---

---

## 11. Ranked Analysis Roadmap

The current document points toward several valid next analytical moves. To avoid branching too early, the following sequence is recommended.

### 11.1 Recommended order

1. **Target Ownership Matrix**  
   Define the intended primary home, secondary zones, and allowed / forbidden responsibilities for the key concepts.

2. **As-Is Ownership Matrix**  
   Map where those same concepts currently live, what roles they presently play, and where they appear overloaded.

3. **Canonical Intent Model**  
   Define the missing upstream intent abstraction that should unify user request, selected data, transformation/comparison mode, execution constraints, and delivery constraints.

4. **Concept-Collision Map**  
   Identify the concept pairs that most often collapse into one another improperly, such as:
   - State + Context
   - Coordinator + Strategy
   - Coordinator + Controller
   - Render + Strategy
   - Adapter + Resolver

5. **Hotspot-to-Zone Mapping**  
   Map concrete subsystem hotspots into the target hierarchy to see which are correctly placed, overloaded, transitional, or misowned.

### 11.2 Why this order

This sequence moves from:

- ideal ownership
- to current ownership
- to the likely missing center
- to concept-boundary stress points
- to concrete code leverage points

That progression keeps the analysis disciplined and prevents early drift into tactical refactoring before the architectural picture is fully pinned down.

### 11.3 Primary recommendation

If only one next step is taken immediately, it should be the **Target Ownership Matrix**.

It is the shortest path from conceptual architecture to enforceable structural rules.


---

---

## 12. Summarized Implementation Sequence

The enhanced architecture suggests the following high-level implementation order.

This is optimized for architectural leverage rather than raw speed.

### 12.1 Recommended sequence

1. **Authority spine + canonical intent model**  
   Make authority, provenance, and intent explicit so `Context`, `State`, `Request`, and `Result` stop carrying blurred ownership.

2. **One primary execution model**  
   Choose the reasoning/program path as the main story and progressively demote competing execution paths into adapters, fallback routes, or observers.

3. **Reasoning vs process separation**  
   Keep strategy, capability, composition, transform, and overlay definition in the reasoning container; keep workflow, routing, fallback, and sequencing in the process container.

4. **Contract / boundary seam**  
   Establish program, delivery, interaction, and multi-result contracts as the main fan-out seam between upstream meaning and downstream consumption.

5. **Consumer / interaction branching**  
   Model charts, exports, APIs, and future clients as real consumer families fed from the same contract seam, with interaction treated as a named boundary concern.

6. **Terminal rendering demotion**  
   Push backend routing, qualification, host binding, and vendor-specific behavior fully into terminal delivery infrastructure.

7. **Diagnostics / migration sidecar isolation**  
   Keep parity, evidence, reachability, qualification, and migration machinery observational and structurally outside the main authority path.

8. **Collapse repeated family micro-frameworks**  
   Only after the spine, contracts, and consumer seams are real, reduce repeated family-specific request/route/qualification/adapter patterns into fewer shared structures.

### 12.2 Short form

```text
Authority -> Intent -> Execution -> Process -> Contracts -> Consumers -> Terminal Delivery
                                                \-> Governance / Evidence
```

### 12.3 Why this order

This sequence:

- establishes the right upstream centers first
- delays branching until the correct boundary
- prevents UI/rendering cleanup from becoming cosmetic only
- makes later consolidation cheaper and more honest


---

---

## 13. Implementation Status Snapshot

This section records the current implementation state of the sequence above so future agents can continue without re-litigating completed architectural work.

### 13.1 Current status by sequence step

1. **Authority spine + canonical intent model - substantially implemented / still maturing**  
   The system now has explicit VNext request, snapshot, analytical intent, capability, composition, consumer-delivery, provider, render-plan, runtime provenance, and evidence vocabulary. The remaining work is to keep reducing legacy `ChartDataContext` authority and make canonical intent the normal live entry point instead of a projected compatibility path.

2. **One primary execution model - substantially implemented for current chart families**  
   The VNext reasoning/program path is live across the available chart-family routes, including Main, Normalized, Distribution, WeekdayTrend, Bar/Pie, and Transform operations; Diff/Ratio has VNext strategy support but lacks a current frontend controller surface. Legacy execution remains as fallback/compatibility rather than the preferred architectural model.

3. **Reasoning vs process separation - mostly implemented**  
   Reasoning concerns now live primarily in VNext contracts, kernel, analytical intent, operation execution, program planning, provider contracts, and render-plan projection. Process concerns remain in coordinators, route policies, runtime-state recording, and tab/controller adapters.

4. **Contract / boundary seam - substantially implemented**  
   `AnalyticalIntent`, `CapabilityRequest`, `ConsumerDeliveryContract`, `ConsumerProviderContract`, `ChartProgram`, `ChartRenderPlan`, provider metadata, and vocabulary diagnostics now form the main seam between upstream meaning and downstream consumers. The remaining work is to keep moving live consumers onto this seam and remove family-specific bypasses where they no longer provide value.

5. **Consumer / interaction branching - actively maturing**  
   Main, Syncfusion, Admin, evidence export, API response, chart-controller paths, and future plugin providers are now modeled more clearly as consumers/providers rather than semantic authorities. Provider registry and adapter qualification now allow chart libraries to be selected by contract metadata rather than hard-coded tab/vendor assumptions.

6. **Terminal rendering demotion - in progress, with validated provider boundary**  
   Rendering has been pushed toward disposable backend adaptation through render plans, backend capabilities, provider-aware adapter dispatchers, provider contracts, provider metadata, render-plan diagnostics, and vocabulary coverage. Recent manual smoke verified provider-aware rendering for LiveCharts and Syncfusion without missing provider/vocabulary coverage.

7. **Diagnostics / migration sidecar isolation - substantially implemented**  
   Evidence export, reachability diagnostics, runtime-path state, family runtime state, render-plan diagnostics, provider metadata coverage, vocabulary coverage, and session milestones now observe the system rather than define live behavior. Evidence now reports missing vocabulary/provider coverage so migration gaps are provable instead of inferred manually.

8. **Collapse repeated family micro-frameworks - partially implemented / lower priority now**  
   Safe consolidation has happened around parity comparison, workspace milestones, load coordination, binary chart context mechanics, qualification probe support, VNext request planning, and provider metadata stamping. Broader family-framework collapse should be guided by the contract/provider seam, not by cosmetic file-count reduction.

### 13.2 Completed material changes

- VNext contracts now include explicit analytical intent, capability, composition, consumer delivery, interaction, provider, and render-plan vocabulary.
- Consumer/provider contracts and a provider registry now describe built-in LiveCharts, Syncfusion, evidence export, API response, and custom third-party provider seams.
- Provider metadata is stamped into render-plan projection and vocabulary metadata through a shared helper.
- Live render adapters now qualify render plans by both render-plan kind and provider metadata where provider metadata is present.
- Render-plan vocabulary metadata is attached across chart families and preserved by LiveCharts, Syncfusion, orchestration adapters, and evidence diagnostics.
- Evidence export reports runtime path, family runtime state, render-plan diagnostics, vocabulary coverage, provider coverage, and missing vocabulary/provider gaps.
- Live loading uses VNext across the available chart-family routes, including Main, Normalized, Distribution, WeekdayTrend, Bar/Pie, and Transform operations, with legacy fallback preserved.
- Main, Syncfusion, and Admin tabs now share more workspace-host and evidence/session behavior.
- Large-data rendering preparation introduced render-budget planning and vocabulary for viewport/detail strategies, but full adaptive zoom/detail behavior remains future work.

#### Structural folder restructure (April 2026)

A folder restructure was applied to bring the physical layout closer to the intended ownership shape described in this document. The following changes were completed with all 894 automated tests passing:

- `UI/Syncfusion/` relocated to `UI/Charts/Syncfusion/` — Syncfusion types (view, coordinator, helpers, items) are now co-located within the consumer/interaction field rather than treated as a top-level UI concern.
- `UI/Charts/Interfaces/` merged into `UI/Charts/Presentation/` — controller interfaces and their adapters now live in one coherent consumer/interaction container; the `Interfaces` sub-namespace is eliminated.
- `UI/Charts/Presentation/Rendering/` flattened into `UI/Charts/Presentation/` — rendering orchestration types (`ChartRendererResolver`, `IChartRenderer`, `IChartSurface`, `UiChartRenderModel`, etc.) are promoted directly into the consumer/interaction field, removing the illusion of a rendering sub-center.
- Vendor-specific sub-backends promoted from `Presentation/Rendering/ECharts/` and `Presentation/Rendering/LiveCharts/` to `Presentation/ECharts/` and `Presentation/LiveCharts/` — backend siblings are now visibly terminal within the consumer field rather than nested inside a rendering sub-hierarchy.
- `Core/Rendering/Tooltip/` extracted from `Core/Rendering/Helpers/` — tooltip formatting helpers are isolated into their own sub-namespace, separating dedicated tooltip support from general rendering helpers.
- `UI/Defaults/` dissolved into `UI/` — `UiDefaults` is now a flat member of the `DataVisualiser.UI` namespace, removing a spurious sub-container for a single utility class.
- Test project: `Tests/Controls/` renamed to `Tests/UI/Charts/Presentation/` — mirrors the restructured production namespace.
- Test project: `Tests/UI/Rendering/` relocated to `Tests/Core/Rendering/` — test files now mirror their production counterparts under `Core.Rendering`.
- Test project: `Tests/Helpers/Infrastructure/` extracted from `Tests/Helpers/` — builder, stub, and test-helper infrastructure types are isolated from general helper utilities.

### 13.3 Current next practical work

- Finish removing remaining bypasses around live consumer/provider delivery before Phase 7 capability expansion unless explicitly deferred.
- Keep `MetricLoadCoordinator` and chart-family adapters as process/consumer coordinators, not semantic authorities.
- Continue extracting chart-family request planning and runtime mapping into shared VNext planning seams; provider selection now has an initial shared contract/qualification surface.
- Preserve legacy execution as a compatibility/fallback adapter until VNext parity and smoke evidence are strong enough to retire each path safely.
- Avoid broad folder or family-framework consolidation unless it directly strengthens the contract/provider seam.

#### Folder restructure complete (April 2026)

The structural folder restructure described in §3.6 and §13.2 is complete. The physical layout now more closely reflects the intended ownership containers. No further folder-level reorganization is planned as a priority; the next leverage is in behavioral/ownership enforcement rather than file location.

### 13.4 Manual validation state

Recent automated validation has covered the current structural, contract, provider, metadata, and evidence work, including full solution test runs. Manual smoke has verified Main, Distribution, WeekdayTrend, Bar/Pie, Syncfusion render/export, and complete provider/vocabulary coverage after provider-aware adapter qualification. New manual smoke is only required after further live behavior changes, especially when changing runtime routing, render delivery, provider selection, chart-controller interaction behavior, or adaptive viewport/detail behavior.


---

---
## 14. Mini Ownership Rule Table

This is not a full ownership matrix. It is a compact rule table for the enhanced architecture.

| Container | Owns | Must Not Own |
|---|---|---|
| Authority Container | truth, provenance, identity, semantic status | interaction, rendering, consumer convenience |
| Reasoning and Capability Container | strategy, capability, composition, derived outputs, overlay definition | UI state, backend behavior, vendor adaptation |
| Process and Execution Container | workflow, sequencing, routing, fallback, observability | semantic meaning, result identity, presentation behavior |
| Contract and Boundary Container | safe handoff, downstream constraints, consumer-agnostic output shape | backend quirks, UI-specific assumptions |
| Consumer and Interaction Field | consumer adaptation, interaction meaning, local consumer state | truth authority, result composition, execution policy |
| Terminal Delivery Infrastructure | rendering, host binding, backend adaptation, vendor lifecycle | semantic interpretation, analytical composition |
| Governance and Evidence Sidecar | validation, parity, reachability, audit, qualification evidence | live semantic control, execution authority |

---

---

## 15. Migration Risk Note

Do not begin by collapsing family-specific micro-frameworks.

That cleanup should happen only after the following are materially clear:

1. authority spine
2. canonical intent model
3. primary execution model
4. contract / boundary seam
5. consumer branching model

Otherwise, consolidation risks becoming cosmetic and may simply move the same ambiguity into fewer files.

---

## 16. Final Takeaway

The codebase should not ultimately evolve toward a chart-centered or rendering-centered architecture.

It should evolve toward:

- truth-centered authority
- reasoning-centered composition
- contract-driven delivery
- consumer-thin interaction
- replaceable terminal presentation
- observational governance and evidence

That is the clean conceptual destination.


---


---

## Architectural Migration Progress Snapshot

This section is an approximate, non-binding progress assessment based on the latest project structure, codebase index, dependency summary, and the target architecture described in this document.

### Current estimated completion

```text
Architectural migration: approximately 65–70% complete
Working estimate: ~68%
```

### Breakdown

| Area | Approx. completion | Reason |
|---|---:|---|
| Vocabulary / conceptual model | 90% | Stable promoted concepts and target hierarchy exist. |
| VNext reasoning spine | 75% | `ReasoningEngine`, analytical intent, program planning, and session coordination exist. |
| Contract / boundary model | 65% | Consumer/provider contracts are emerging, but boundary enforcement is not fully proven. |
| Rendering demotion | 63% | Render-plan delivery exists; folder restructure removed the `Presentation/Rendering/` sub-hierarchy, but `Core.Rendering` remains structurally large. |
| Consumer / interaction separation | 60% | Controller interfaces and adapters now live in one coherent `UI.Charts.Presentation` container; Syncfusion types relocated into the consumer field; further behavioral enforcement remains. |
| Governance / evidence | 75% | Evidence, parity, and diagnostics infrastructure are strong, but must remain observational. |
| Legacy coexistence cleanup | 50–60% | Older mesh structures still coexist with VNext and family-specific delivery patterns. |

### Interpretation

The migration is now materially underway rather than merely documentary.

The hardest conceptual shift has happened:

```text
from:
presentation / rendering-heavy architecture

toward:
reasoning-engine + contract/provider/consumer boundary + terminal delivery
```

The remaining work is mostly consolidation, enforcement, and selective relocation rather than another broad decomposition campaign.

### Why the estimate is not higher

The old architectural mesh still carries significant weight, especially around:

- `UI.MainHost.Coordination`
- `Core.Rendering`
- family-specific rendering contracts, builders, resolvers, and qualification structures

These areas may still contain responsibilities that belong higher in the target hierarchy.

Note: `UI.Charts.Presentation` was previously listed here as a problem area. The April 2026 folder restructure partially corrected it by merging controller interfaces into the presentation container, flattening the rendering sub-hierarchy, and relocating vendor backends to visibly terminal positions. The physical layout is now more aligned. Behavioral enforcement — ensuring that nothing in this layer acts as a semantic authority — remains in progress.

### Why the estimate is not lower

The core migration shape now exists in code:

- reasoning-engine structures exist
- analytical intent and program planning exist
- render-plan delivery exists
- consumer/provider contracts are emerging
- evidence and parity infrastructure are well established

That means the project has crossed from conceptual planning into structural implementation.

### Best next assessment move

The next useful review is a narrow ownership audit of the provider/consumer boundary and nearby delivery types.

Primary question:

```text
Do these types enforce the intended boundary,
or do they merely rename delivery routing?
```

This should be answered before starting another broad refactor.

