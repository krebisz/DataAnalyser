# PROJECT ROADMAP

## 1. Purpose

This document outlines the planned evolution of the project, describing major phases, their goals, and the order in which capabilities are introduced.

It is **directional, not binding**.  
Binding rules reside in the **Project Bible**.

---

## 2. Guiding Principles

Roadmap execution adheres to the following constraints:

- Semantic correctness over convenience
- Explicit contracts before implementation
- Non-destructive, additive evolution
- Clear separation between truth and exploration
- Execution proceeds only after foundations are locked

---

## 3. Phase Overview

### Phase 1 — Ingestion & Persistence (Completed)

**Goal**  
Establish reliable ingestion of heterogeneous data sources and persist raw / lightly normalized records.

**Key outcomes**

- Multiple parsers (CSV / JSON)
- Unified storage via `HealthMetric`
- Lossless data capture
- Provider-agnostic ingestion

**Status**: ✅ Complete

---

### Phase 2 — Canonical Semantics & Normalization Foundations (Completed)

**Goal**  
Formalize semantic authority and remove ambiguity from metric meaning.

**Key outcomes**

- Canonical Metric Series (CMS) contract
- Explicit canonical metric identity rules
- Identity resolution scaffolding
- HealthMetric → CMS mapping contract
- Derived / dynamic metric identity framework
- Structural / manifold analysis explicitly constrained
- Contracts anchored in Project Bible and SYSTEM_MAP

**Status**: ✅ Complete

This phase establishes the semantic **truth layer** of the system.

---

### Phase 3 — Execution: Canonical Identity & CMS Integration (Completed)

**Goal**  
Introduce real behavior while preserving existing ingestion and storage.

**Scope**

- Implement concrete identity resolution (metric family by metric family)
- Begin with Weight metric family
- Implement CMS mapping in shadow mode
- No destructive changes to existing flows
- No changes to DataVisualiser yet

**Deliverables**

- Deterministic identity resolution
- CMS emitted alongside existing outputs
- Diagnostics for identity and mapping failures

**Status**: ✅ Complete  
(Core goals achieved; additional metric families may be added incrementally.)

---

### Phase 4 — Consumer Adoption & Visualization Integration (In Progress)

**Goal**  
Allow downstream systems (DataVisualiser) to consume CMS safely.

**Scope**

- Define minimal CMS dependency for DataVisualiser
- Parallel support for legacy `HealthMetric` paths
- Explicit opt-in to CMS-based workflows
- No forced migration

**Outcome**

- Safer aggregation
- Explicit composition
- Reduced semantic ambiguity in UI
- Generalized cyclic distribution visualizations (e.g., weekly, hourly, or N-bucket cycles)
- Foundation for user-defined metric transformations with preview tables feeding the charting pipeline

**Status**: ▶ In Progress (~60% complete)

CMS infrastructure is in place; multiple strategies are migrated or in progress; UI integration and remaining strategy migrations continue. User-defined metric transformations with preview are implemented.

---

### Phase 4A — Workspace Realignment & Parity Closure

_(Additive · Non-Destructive)_

**Goal**  
Allow controlled workspace reset and consolidation **without loss of semantic, architectural, or executional continuity**.

**Scope**

- Rehydrate workspace from frozen foundational documents
- Preserve CMS + legacy parallelism guarantees
- Complete remaining strategy migrations under parity harness protection
- Explicitly close Phase 4 parity obligations before Phase 5 discussion

**Constraints**

- No architectural reinterpretation
- No semantic contract changes
- No implicit parity assumptions
- All parity activation must be explicit and reversible

**Outcome**

- Clean workspace with preserved intent
- Reduced cognitive load
- Phase 4 completion without drift

---

### Phase 5 — Derived Metrics & Analytical Composition (Planned)

**Goal**  
Enable explicit creation of new metrics through composition and aggregation.

**Scope**

- Dynamic / derived metric identity instantiation
- Explicit aggregation and transformation pipelines
- Support for ephemeral and persistent derived metrics
- No implicit promotion to canonical truth

**Outcome**

- Advanced analysis without semantic erosion
- User-driven metric synthesis

---

### Phase 6 — Structural / Manifold Analysis (Future)

**Goal**  
Reintroduce advanced structural analysis capabilities without compromising semantic authority.

**Scope**

- Structural similarity detection
- Equivalence class exploration
- Analytical suggestion systems

**Constraints**

- Non-authoritative
- No automatic promotion
- Explicit declaration required for semantic impact

---

## 4. Out of Scope (Explicit)

The roadmap does **not** include:

- Heuristic identity inference
- Silent aggregation
- Implicit semantic promotion
- Forced migrations
- Architectural shortcuts

---

## 5. Evolution Policy

- Phases may overlap **only when explicitly declared**
- Earlier phases must not be weakened by later work

Any deviation from this roadmap requires:

- explicit justification
- document updates
- agreement

---

## 6. Intermediate Goals (Descriptive, Non-Binding)

The following goals clarify expected evolution **within existing phases**:

- **Generalized cyclic distribution charts**

  - Extend weekly distribution visualizations into a generic cyclic engine (weekly, hourly, or N-bucket) without altering semantic contracts.

- **User-defined metric transformations with preview** ✅ **Complete**
  - Allow explicit transformations over CMS (e.g. `A + B - C`, `sqrt(A)`), preview before/after values in a grid, and feed results into the charting pipeline as ephemeral, non-canonical metrics.
  - **Status**: Implemented. Transform panel supports unary operations (Logarithm, Square Root) and binary operations (Add, Subtract) on metric data. Results are explicitly marked as ephemeral/non-canonical and displayed in preview grids before charting. Transform results are never promoted to canonical truth.

---

**End of Project Roadmap**
