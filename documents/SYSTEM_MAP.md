# SYSTEM MAP
**Status:** Canonical (Structural)  
**Scope:** Conceptual architecture, execution boundaries, rendering boundaries, and data-flow constraints  
**Authority:** Subordinate only to `Project Bible.md`

---

## 1. Purpose

This document defines the structural layout of the system, including:

- conceptual layers
- execution flow
- semantic authority boundaries
- rendering boundaries
- permitted interactions between subsystems

It answers where things live, what may talk to what, and what must never cross boundaries.

This is not an implementation guide.  
It is a map of allowed reality.

---

## 2. Structural Views

### 2.1 Authority Stack (Binding)

Semantic and interpretive authority flows downward only:

`Raw Data -> Normalization -> CMS -> Derived Computation -> Interpretive Overlays -> Presentation`

Authority never flows upward.

---

### 2.2 Execution Stack (Structural)

Runtime execution typically passes through these operational zones:

`Presentation/UI -> Orchestration -> Computation/Overlay Selection -> Rendering Infrastructure -> Presentation/UI`

This execution loop does not change authority.
The UI may initiate execution, but it does not gain semantic authority by doing so.

---

### 2.3 Rendering Position (Binding)

Rendering infrastructure sits below derivation and interpretation, and above concrete backend controls.

Its role is to:

- consume already-defined results or overlays
- expose qualified rendering capabilities
- isolate backend lifecycle quirks

It must never define semantics, reinterpret CMS, or compensate for missing orchestration boundaries.

---

## 3. Layer Definitions (Authoritative)

### 3.1 Raw Data Layer

**Responsibilities**
- ingest raw measurements
- preserve original values
- retain temporal and source fidelity

**Constraints**
- no interpretation
- no normalization
- no semantic inference

This layer is immutable.

---

### 3.2 Normalization and Canonical Identity Layer

**Responsibilities**
- assign declared semantics
- resolve metric identity
- apply deterministic normalization stages

**Constraints**
- declarative only
- ordered stages
- no statistical inference
- no downstream inspection

This is the semantic authority layer.

---

### 3.3 Canonical Metric Series (CMS) Layer

**Responsibilities**
- provide the stable, trusted analytical substrate
- represent the authoritative time series for each metric

**Constraints**
- immutable once produced
- assumed correct by all consumers
- never conditionally altered

CMS is the only trusted analytical input to forward computation.

---

### 3.4 Derived Computation Layer

**Responsibilities**
- perform declared computations over CMS
- produce non-canonical results such as:
  - transforms
  - aggregates
  - compositions
  - comparative values
  - stacked values
  - chart-program result sets

**Constraints**
- must declare provenance
- must not mutate CMS
- results are non-authoritative by default
- result identity must remain explicit when persisted or reused

Derived results may be ephemeral or persistent, but never implicit.

---

### 3.5 Interpretive Overlay Layer

This layer provides interpretation without mutation.

It exists to help humans reason about data, not to redefine it.

#### 3.5.1 Structural Interpretation

Includes, non-exhaustively:

- trend detection
- trend comparison
- compositional comparisons
- pivot-based or event-relative views
- dynamic colouring and emphasis
- structural or relational exploration

**Constraints**
- no semantic promotion
- no identity inference
- no back-propagation into normalization, CMS, or derived truth

#### 3.5.2 Confidence and Reliability Overlay

**Responsibilities**
- detect statistically atypical readings
- classify variance, noise, or irregularity
- annotate, not alter, data points or interpretive views

**Permitted mechanisms**
- declared outlier detection models
- local-window variance analysis
- missingness or temporal gap detection

**Constraints**
- canonical values remain unchanged
- all confidence assessments are annotations
- all exclusions or attenuations are explicit and reversible

#### 3.5.3 Rules-Based Option Gating

The system may include declarative rules that:

- constrain UI options
- enable or disable interpretive views
- prevent invalid combinations

Rules must be:

- declarative
- explainable
- transparent to the user

Rules must never be treated as recommendations or semantic truth.

---

### 3.6 Orchestration Layer

The orchestration layer coordinates execution, not meaning.

**Responsibilities**
- strategy selection through explicit declared mechanisms
- execution routing
- chart-program/result composition handoff
- migration coexistence handling
- evidence/export initiation

**Constraints**
- no semantic branching
- no heuristic overrides
- no hidden controller-specific execution shortcuts
- no silent bypass paths

Execution reachability must be observable.

---

### 3.7 Rendering Infrastructure Layer

This layer contains rendering contracts, backend adapters, backend probes, and qualification harnesses.

**Responsibilities**
- define rendering capabilities by chart family and interaction contract
- isolate backend-specific lifecycle, hover, animation, disposal, and visibility behavior
- host backend qualification artifacts and matrices
- translate render intent into backend-specific control behavior

**Constraints**
- chart vendors are replaceable infrastructure, not architectural authorities
- rendering contracts must not carry semantic meaning
- rendering infrastructure must not reach upward to decide computation
- backend-specific quirks must be quarantined here, not spread into orchestration or UI state
- unqualified backend slices must not be treated as production-safe

---

### 3.8 Presentation / UI Layer

The presentation layer renders outputs from lower layers and exposes explicit user controls.

**Responsibilities**
- host graph parent controllers and chart surfaces
- expose configuration, interpretation, and visibility choices
- visualize provenance, uncertainty, qualification state, and result-set state
- converge toward standardized graph hosts where capability semantics genuinely align

**Constraints**
- must not infer semantics
- must not hide uncertainty
- must not compensate for missing computation or rendering boundaries
- controller standardization must not turn the UI into a semantic authority

UI is expressive, not authoritative.

---

## 4. Chart Programs and Programmable Composition

The system may evolve toward standardized programmable chart composition, but that capability remains structurally downstream of CMS.

### 4.1 Chart Program Definition

A chart program is an explicit downstream construct that may include:

- selected metrics or submetrics
- declared unary, binary, ternary, or higher-order operations
- render intent for one or more derived result sets
- optional interpretive overlays

### 4.2 Structural Placement

Chart programs sit across:

- derived computation, for declared result construction
- interpretive overlays, for non-authoritative overlay behavior
- orchestration, for explicit execution handoff
- rendering infrastructure, for backend-safe display
- presentation, for user-facing composition and control

### 4.3 Binding Constraints

Chart programs:

- do not assign meaning
- do not alter CMS
- do not promote results into canonical truth implicitly
- must preserve provenance for each result set
- may use only qualified rendering capability slices for the interactions they need

Programmability is a downstream capability, not a semantic one.

---

## 5. Execution Boundaries and Flow Rules

### 5.1 Directionality Rule (Binding)

Authority and permitted dependency direction follow:

`Truth -> Derivation -> Interpretation -> Orchestration/Rendering Consumption -> Presentation`

Upward semantic influence is forbidden.

### 5.2 Allowed Consumption

- orchestration may consume declared strategies, derived results, overlay intent, and rendering contracts
- rendering infrastructure may consume derived results and overlays
- presentation may consume any downstream-safe outputs and state needed to display them

### 5.3 Prohibited Boundary Violations

Examples include:

- UI selecting semantic meaning
- confidence logic modifying CMS
- trend logic influencing normalization
- rendering logic altering computation
- backend lifecycle quirks driving orchestration design
- controller-specific assumptions redefining a chart family's architectural contract

Such violations constitute architectural breach.

---

## 6. Migration and Evidence Boundaries

### 6.1 Migration Coexistence Model

During migration phases, the system may contain:

- legacy execution paths
- CMS-based execution paths

**Constraints**
- CMS is the forward path
- legacy is compatibility-only
- parity visibility is mandatory
- unreachable code is treated as non-existent

Migration is a state, not a feature.

### 6.2 Reachability and Evidence

Evidence generation is structurally downstream of execution.

It may observe:

- which strategy ran
- which path was used
- parity outcomes
- backend qualification outcomes

It must not influence live semantic decisions.

---

## 7. Rendering and Backend Qualification Rules

### 7.1 Capability-Oriented Rendering

Rendering must be defined by capability family rather than vendor control type.

Examples include:

- time-series/cartesian
- distribution/range
- polar/radar-style projection
- compositional/hierarchical
- transform/result
- hover/selection/legend/visibility/reset interactions

### 7.2 Backend Qualification Requirement

No backend may be treated as production-safe for a capability family until it has passed explicit qualification for that slice.

Qualification may include:

- initial render
- repeated updates
- hide/show behavior
- tab switch or offscreen behavior
- unload/disposal
- application close
- hover and tooltip interaction

### 7.3 Tactical Stabilization Rule

Tactical fallbacks may preserve capability temporarily, but do not count as architectural closure or backend qualification.

---

## 8. Language and Signalling Constraints

All layers above CMS must communicate uncertainty clearly.

**Avoid**
- "invalid data"
- "wrong reading"
- "bad value"

**Prefer**
- "statistically atypical"
- "low confidence under selected model"
- "excluded from trend overlay"
- "qualified only for selected capability slice"

Language is part of architecture.

---

## 9. Summary

- canonical truth is immutable
- derivation is explicit and provenance-preserving
- interpretation is powerful but bounded
- orchestration coordinates execution but does not define meaning
- rendering infrastructure is replaceable and qualification-bound
- UI is expressive but non-authoritative
- programmable chart composition is allowed only as a downstream, reversible, provenance-visible capability

This map defines what the system is allowed to be.

---

**End of SYSTEM MAP**
