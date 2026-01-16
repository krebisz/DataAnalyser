# SYSTEM MAP
**Status:** Canonical (Structural)  
**Scope:** Conceptual architecture, execution boundaries, and data-flow constraints  
**Authority:** Subordinate only to Project Bible.md

---

## 1. Purpose

This document defines the **structural layout of the system**, including:

- conceptual layers
- execution flow
- semantic authority boundaries
- permitted interactions between subsystems

It answers **where things live**, **what may talk to what**, and **what must never cross boundaries**.

This is not an implementation guide.  
It is a **map of allowed reality**.

---

## 2. High-Level System Overview

The system is organized into **strictly ordered layers**, flowing from truth to interpretation.

[ Raw Data ]
↓
[ Normalization ]
↓
[ Canonical Metric Series (CMS) ]
↓
[ Derived Computation ]
↓
[ Interpretive Overlays ]
↓
[ Presentation / UI ]


Information flows **downward only**.  
Authority never flows upward.

---

## 3. Layer Definitions (Authoritative)

### 3.1 Raw Data Layer

**Responsibilities**
- Ingest raw measurements
- Preserve original values
- Retain temporal and source fidelity

**Constraints**
- No interpretation
- No normalization
- No semantic inference

This layer is immutable.

---

### 3.2 Normalization Layer

**Responsibilities**
- Assign declared semantics
- Resolve metric identity
- Apply deterministic transformations

**Constraints**
- Declarative only
- Ordered stages
- No statistical inference
- No downstream inspection

This is the **semantic authority layer**.

---

### 3.3 Canonical Metric Series (CMS)

**Responsibilities**
- Provide a stable, trusted analytical substrate
- Represent the authoritative time series for each metric

**Constraints**
- Immutable once produced
- Assumed correct by all consumers
- Never conditionally altered

CMS is the **only** valid input to computation.

---

### 3.4 Derived Computation Layer

**Responsibilities**
- Perform declared computations over CMS
- Produce non-canonical results such as:
  - transforms
  - aggregates
  - compositions
  - stacked or comparative values

**Constraints**
- Must declare provenance
- Must not mutate CMS
- Results are non-authoritative by default

Derived results may be ephemeral or persistent, but never implicit.

---

## 4. Interpretive Overlay Layer (Non-Authoritative)

This layer provides **interpretation without mutation**.

It exists to help humans reason about data, not to redefine it.

---

### 4.1 Structural Interpretation

Includes (non-exhaustive):

- trend detection
- trend direction equivalence
- clustering (e.g., scatter plots)
- compositional comparisons
- pivot-based views
- dynamic colouring (hot/cold, increase/decrease)

**Constraints**
- No semantic promotion
- No identity inference
- No back-propagation

---

### 4.2 Confidence & Reliability Overlay

This sub-layer explicitly represents **uncertainty and data quality**.

**Responsibilities**
- Detect statistically atypical readings
- Classify variance, noise, or irregularity
- Annotate (not alter) data points

**Permitted Mechanisms**
- Outlier detection under declared statistical models
- Local-window variance analysis
- Missingness or temporal gap detection

**Constraints**
- Canonical values remain unchanged
- All confidence assessments are annotations
- All actions are explicit and reversible

---

### 4.3 Permitted Actions on Confidence Annotations

- Visual marking (e.g., low-confidence indicators)
- Optional attenuation in trend or smoothing overlays
- Optional exclusion from specific interpretive computations

**Prohibited**
- Deletion of data
- Mutation of CMS
- Silent exclusion
- Influence on normalization or identity

---

## 5. Presentation / UI Layer

**Responsibilities**
- Render outputs from lower layers
- Expose configuration and interpretation choices
- Visualize provenance, confidence, and state

**Constraints**
- Must not infer semantics
- Must not hide uncertainty
- Must not compensate for missing computation

UI is expressive, not authoritative.

---

## 6. Execution Boundaries & Flow Rules

### 6.1 Directionality Rule (Binding)

Execution and data flow must proceed:

Truth → Derivation → Interpretation → Presentation


Reverse flow is forbidden.

---

### 6.2 Boundary Violation Examples (Prohibited)

- UI selecting semantic meaning
- Confidence logic modifying CMS
- Trend logic influencing normalization
- Rendering logic altering computation

Such violations constitute architectural breach.

---

## 7. Orchestration Layer (Structural)

The orchestration layer coordinates **execution**, not **meaning**.

**Responsibilities**
- Strategy selection (explicit, declared)
- Execution routing
- Migration coexistence handling

**Constraints**
- No semantic branching
- No heuristic overrides
- No silent bypass paths

Execution reachability must be observable.

---

## 8. Migration Coexistence Model (Temporal)

During migration phases, the system MAY contain:

- Legacy execution paths
- CMS-based execution paths

**Constraints**
- CMS is the forward path
- Legacy is compatibility-only
- Parity visibility is mandatory
- Unreachable code is treated as non-existent

Migration is a state, not a feature.

---

## 9. Rules-Based Option Gating (Interpretive · Non-Authoritative)

The system MAY include rule engines that:

- constrain UI options
- enable or disable interpretive views
- prevent invalid combinations

**Rules must be**
- declarative
- explainable
- transparent to the user

Rules must never be treated as recommendations or truth.

---

## 10. Language & Signalling Constraints

All layers above CMS must communicate uncertainty clearly.

**Avoid**
- “invalid data”
- “wrong reading”
- “bad value”

**Prefer**
- “statistically atypical”
- “low confidence under selected model”
- “excluded from trend overlay”

Language is part of architecture.

---

## 11. Summary

- Canonical truth is immutable
- Interpretation is powerful but bounded
- Confidence enriches understanding without redefining reality
- Authority flows downward only
- Trust is preserved through explicitness

This map defines what the system **is allowed to be**.

---

**End of SYSTEM MAP**
