# PROJECT OVERVIEW

## 1. Purpose

This document provides a high-level description of the project, its intent, and its current trajectory.

It is **descriptive, not binding**.  
Architectural law resides in **Project Bible.md**.

---

## 2. Project Intent

The project exists to ingest heterogeneous structured data, normalize it into a **deterministic and semantically authoritative metric space**, and enable computation and visualization **without semantic erosion**.

The primary objective is **trustworthy metric meaning**, not visualization, analytics, or UI convenience.

---

## 3. Architectural Direction (Current)

The system explicitly prioritizes:

- Canonical semantic authority over inferred meaning
- Deterministic normalization and identity resolution
- Strict separation between:
  - canonical truth
  - derived, exploratory, or comparative computation
- Parallelism over forced migration
- Additive, non-destructive evolution

Downstream consumers are designed to **trust semantics, not reinterpret them**.

---

## 4. Historical Context & Design Evolution (Additive)

> Contextual section — explanatory only.

### 4.1 Early Structural Exploration

Early iterations explored structural and hierarchical equivalence across heterogeneous inputs (notably JSON), enabling discovery through shape- and relationship-based analysis.

This approach proved valuable for exploratory insight.

### 4.2 Limitations Identified

When applied to persisted metrics and computation, structural inference introduced unacceptable risks:

- structurally similar data representing different semantics
- loss of auditability
- long-term semantic drift

### 4.3 Transition to Canonical Metric Semantics

The system transitioned to a **metric-first deterministic model**:

- explicit time semantics
- scalar value primacy
- canonical metric identity
- declarative normalization stages

This established a **truth layer** that downstream systems can consume safely.

### 4.4 Deferred Structural Analysis

Structural and manifold-style analysis was **deliberately deferred**, not discarded.

It is now treated as a **future, non-authoritative analytical layer**, explicitly prevented from mutating canonical truth without declaration and promotion.

---

## 5. Relationship to Foundational Documents

- **Project Bible.md** — architectural law and invariants
- **SYSTEM_MAP.md** — conceptual layering and boundaries
- **MASTER_OPERATING_PROTOCOL.md** — execution and collaboration discipline
- **Project Roadmap.md** — phase sequencing and intent

In case of conflict, higher-authority documents prevail.

---

## 6. Current Phase Status (Descriptive)

This reflects **actual implementation state**, not aspiration:

- **Phase 1 — Ingestion & Persistence**  
  Complete. Lossless ingestion and unified storage are stable.

- **Phase 2 — Canonical Semantics & Normalization Foundations**  
  Complete. Canonical Metric Series (CMS), identity rules, and normalization contracts are established.

- **Phase 3 — Canonical Identity & CMS Integration**  
  Complete. CMS is produced deterministically alongside legacy outputs.

- **Phase 4 — Consumer Adoption & Visualization Integration**  
  In progress (~60% complete).  
  DataVisualiser now:

  - consumes CMS through an explicit dependency surface
  - supports **parallel CMS and legacy execution paths**
  - migrates strategies incrementally
  - uses parity harnesses to validate equivalence without forcing migration
  - provides **user-defined metric transformations** with preview grids and charting pipeline integration
    - Transform results are explicitly ephemeral and non-canonical
    - Operations include unary (Logarithm, Square Root) and binary (Add, Subtract)
    - Results are never promoted to canonical truth

  Parity is treated as a **phase obligation**, not an implementation detail.

- **Phase 5 — Derived Metrics & Analytical Composition**  
  Planned. No derived metrics are authoritative by default.

- **Future — Structural / Manifold Analysis**  
  Deferred, non-authoritative, and explicitly constrained.

---

## 7. Derived & Dynamic Metrics (Clarification, Non-Binding)

Derived metrics are **explicit semantic entities**, created through declared composition or aggregation of canonical metrics.

Constraints:

- Every derived metric has its **own identity**
- Source identities are never mutated
- Units, dimensions, and provenance are explicit
- Promotion to canonical truth is never implicit

Derived metrics may be ephemeral or persistent, but are always **intentional and reversible**.

---

## 7A. Transform Operations (Phase 4 Implementation)

The system currently supports **ephemeral transform operations** (Phase 4):

- Transform results are **explicitly non-canonical** and **ephemeral**
- Operations include unary (Logarithm, Square Root) and binary (Add, Subtract)
- Results are displayed in preview grids and charted, but **never promoted to canonical truth**
- Transform operations track provenance (source metrics, operation type) but do not create authoritative identities

This implementation aligns with Phase 4 intermediate goals and provides a foundation for Phase 5 derived metrics, while maintaining strict separation from canonical semantic authority.

---

## 8. Summary

Canonical semantics form the foundation.  
Everything else builds on top — **explicitly, reversibly, and without inference**.

---

**End of Project Overview**
