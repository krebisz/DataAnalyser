# SYSTEM MAP

## 1. Purpose

This document defines the **conceptual structure of the system**, its major layers, and the relationships between them.

It is authoritative for:

- semantic boundaries
- responsibility separation
- present and future system shape

It is **not** an implementation guide.

---

## 2. High-Level System Overview

The system is designed to ingest heterogeneous data sources, normalize them into a canonical, deterministic metric space, and support computation, visualization, and future analytical extensions.

The architecture is deliberately layered to ensure:

- lossless ingestion
- explicit semantic authority
- reversible evolution

---

## 3. Core Data Flow (Authoritative)

```
External Sources
    ↓
Ingestion (Raw)
    ↓
Normalization (Semantic)
    ↓
Canonical Metric Series (CMS)
    ↓
Computation / Aggregation
    ↓
Presentation / Visualization
```

Each stage has exclusive responsibility for its concern.

---

## 4. Ingestion Layer

### 4.1 Raw Record

- Represents lossless, uninterpreted observations
- Captures raw fields, timestamps, and source metadata
- Makes no semantic claims

RawRecords are immutable and traceable.

---

## 5. Normalization Layer

### 5.1 Purpose

The normalization layer is the **sole authority** for assigning semantic meaning to raw observations.

It is:

- deterministic
- explicit
- rule-driven

---

### 5.2 Normalization Stages

Normalization is composed of ordered stages, including but not limited to:

1. Metric Identity Resolution
2. Unit Normalization (future)
3. Time Normalization (future)
4. Dimensional Qualification (future)

Each stage:

- has a single responsibility
- must not override earlier stages

---

### 5.3 Metric Identity Resolution

Metric Identity Resolution:

- determines what a metric _is_
- assigns canonical metric identity
- does not infer meaning from values

This stage is declarative, stable, and explainable.

---

## 6. Canonical Metric Series (CMS)

CMS represents trusted, semantically-resolved metrics.

Properties:

- identity is canonical and opaque
- time semantics are explicit
- suitable for computation and aggregation

CMS is the **only valid semantic input** for downstream computation.

---

## 6A. CMS Internal vs Consumer Representation (Additive Clarification)

> **Additive clarification — no existing sections modified.**

Two representations of Canonical Metric Series coexist by design.

### 6A.1 Internal CMS (Normalization Authority)

- Produced by the normalization pipeline
- Strongly typed, normalization-scoped structures
- Used exclusively within ingestion and normalization layers
- Never consumed directly by visualization or UI layers

This representation is the **semantic authority**.

---

### 6A.2 Consumer CMS Interface (Downstream Boundary)

- Exposed via a consumer-facing interface (e.g. `ICanonicalMetricSeries`)
- The **only CMS surface visible to downstream systems**
- Decouples normalization internals from consumers
- Enables parallel legacy + CMS adoption without leakage

Conversion between internal CMS and consumer CMS is:

- explicit
- one-way
- non-authoritative

Semantic authority **never leaves normalization**.

This boundary is mandatory.

---

## 7. Computation & Presentation Layer

Downstream layers:

- assume semantic correctness
- do not reinterpret meaning
- operate on:
  - consumer CMS, or
  - legacy-compatible projections (during migration)

No downstream layer may:

- assign identity
- reinterpret semantics
- influence normalization outcomes

---

## 7C. Ephemeral Transformations & Derived Metrics (Additive Clarification)

> **Additive clarification — Phase 4 implementation.**

### 7C.1 Transform Operations

The system supports user-defined transformations over canonical metrics:

- **Unary operations**: Logarithm, Square Root
- **Binary operations**: Add, Subtract
- Operations are applied to metric values, not identities

### 7C.2 Transform Infrastructure

Transform operations are implemented using an expression tree architecture:

- **TransformExpression**: Represents operations as expression trees, supporting nested/chained operations
- **TransformOperation**: Encapsulates operation logic (unary, binary, n-ary)
- **TransformOperationRegistry**: Centralized registry of available operations
- **TransformExpressionEvaluator**: Evaluates expressions over aligned metric data, generates labels, aligns metrics by timestamp
- **TransformExpressionBuilder**: Builds expressions from simple operation strings
- **TransformDataHelper**: Utilities for formatting transform result data
- **TransformResultStrategy**: Integrates transform results into charting pipeline

The infrastructure is provisioned for future expansion to:

- N-metrics (not just 1 or 2)
- Chained operations (e.g., `log(A + B)`, `sqrt(A - B + C)`)
- Complex expression trees

### 7C.3 Transform Results Are Ephemeral

Transform results:

- are **explicitly non-canonical**
- have **no semantic authority**
- are **never promoted to canonical truth**
- exist only for visualization and exploratory analysis

### 7C.4 Transform Pipeline

Transform operations:

1. Accept canonical metric data as input
2. Build expression trees from operation specifications
3. Evaluate expressions over aligned metric data
4. Apply mathematical operations to values
5. Produce ephemeral results with:
   - derived units (preserved from sources where applicable)
   - operation provenance (tracked but not authoritative)
   - preview grids before charting
6. Feed results into charting pipeline as non-authoritative series via `TransformResultStrategy`

### 7C.5 Boundaries

Transform layer:

- **does not** create canonical metric identities
- **does not** influence normalization
- **does not** persist results as authoritative metrics
- **does** provide explicit operation tracking
- **does** maintain separation from canonical truth
- **does** use expression tree architecture for extensibility

This boundary ensures transforms remain **exploratory tools**, not semantic mutations.

---

## 7A. Legacy + CMS Parallelism Boundary (Additive Clarification)

> **Additive clarification — migration-specific.**

During Phase 4:

- Legacy computation paths and CMS-based paths **must coexist**
- CMS adoption is **explicit and opt-in**
- Legacy paths remain authoritative for comparison only

No computation layer may silently switch semantic inputs.

Parallelism exists to:

- validate equivalence
- protect correctness
- prevent forced migration

---

## 7B. Parity Validation Boundary (Additive Clarification)

> **Additive clarification — phase-exit semantics.**

Parity validation is a **boundary artifact**, not an implementation detail.

- Parity harnesses sit **between** legacy and CMS computation
- They do not participate in normalization or presentation
- They exist solely to validate equivalence of outcomes

Parity:

- is strategy-scoped
- is explicitly activated
- must not alter computation paths

A strategy is not considered migrated until parity is proven.

---

## 8. Structural / Manifold Analysis Layer (Additive · Future / Exploratory)

> **Additive section — no existing sections modified.**

### 8.1 Intent

An optional analytical layer intended to:

- explore structural similarity, equivalence, or hierarchy
- support discovery and comparison
- enable higher-order reasoning

This layer provides **insight**, not authority.

---

### 8.2 Relationship to Core Pipeline

This layer:

- operates orthogonally to ingestion and normalization
- consumes normalized or canonical data as input
- does not modify RawRecords, normalization outputs, or CMS

It must not participate in ingestion, normalization, or computation.

---

### 8.3 Non-Authority Constraint (Binding)

Structural / Manifold Analysis MUST NOT:

- assign or alter canonical metric identity
- modify normalization outcomes
- influence computation or rendering implicitly

---

### 8.4 Promotion Boundary

Insights may be promoted only via:

- explicit, declarative rule changes
- reviewable mechanisms
- reversible processes

No automatic back-propagation is permitted.

---

## 9. Architectural Boundary Visibility (Additive Clarification)

> **Additive clarification — enforcement-oriented.**

When introducing or modifying a cross-project dependency, the following must be explicit:

- source project
- dependency direction
- justification
- migration intent (temporary vs permanent)

Silent boundary erosion is prohibited.

---

## 10. Evolutionary Direction

Future evolution must:

- preserve ingestion boundaries
- centralize semantic authority
- isolate analytical creativity from canonical truth

Deferred layers may be declared here without implementation commitment.

---

**End of SYSTEM MAP**
