# PROJECT BIBLE

## 1. Purpose

The Project Bible defines binding architectural law for the system.

It establishes:
- non-negotiable principles
- semantic authority boundaries
- long-term invariants

If a decision conflicts with this document, the decision is invalid.

---

## 2. Core Principles (Binding)

1. Lossless Ingestion — raw data must never be destroyed or silently altered
2. Explicit Semantics — meaning is assigned declaratively, not inferred
3. Single Semantic Authority — normalization is the sole arbiter of meaning
4. Determinism — identical inputs produce identical outputs
5. Reversibility — evolution must allow rollback

---

## 3. Semantic Authority

Metric meaning:
- is defined once
- is stable
- is opaque to consumers

No downstream layer may reinterpret or override semantic decisions.

---

## 4. Identity Law

Metric identity:
- must be canonical
- must be globally unique
- must not encode source-specific information

Identity resolution is declarative and explainable.

---

## 5. Normalization Law

Normalization:
- is staged
- is ordered
- is explicit

Each stage:
- has a single responsibility
- must not inspect concerns outside its scope

---

## 6. Canonical Metric Series Law

CMS:
- is the only trusted analytical input
- is required for computation
- is assumed correct by consumers

---

## 7. Prohibited Behaviors

The system must never:
- infer semantic meaning implicitly
- conflate structure with meaning
- allow heuristic logic to alter identity
- permit silent semantic drift

---

## 8. Evolution Constraints

Future changes must:
- preserve declared boundaries
- maintain semantic authority
- avoid architectural shortcuts

Convenience must not override correctness.

---

## 9. Structural / Manifold Analysis Constraint (Additive · Binding)

> **Additive Section — No existing sections are modified by this addition.**

The system MAY introduce future analytical subsystems that:
- analyze structural similarity, equivalence, or hierarchy
- operate on normalized or canonical representations
- support exploratory or comparative analysis

Such subsystems MUST NOT:
- assign or alter canonical metric identity
- modify normalization outcomes
- influence computation or rendering implicitly

Any promotion of insights into canonical semantics MUST:
- be explicit
- be declarative
- be reviewable
- be reversible

No automatic or implicit back-propagation is permitted.

---

## Appendix A. Architectural Rationale (Non-Binding)

> **This appendix is explanatory only. It does not introduce binding rules.**

### A.1 Rationale for Explicit Semantics

Earlier iterations of the system relied on inferred meaning derived from structure, naming, or statistical behavior. While powerful for exploration, this led to ambiguity and irreproducible results. Declarative semantics were introduced to ensure stability, auditability, and long-term trust.

### A.2 Rationale for Canonical Identity

Allowing multiple representations or implicit equivalence classes for metrics caused semantic drift over time. Canonical identity enforces a single source of truth while still allowing exploratory analysis elsewhere.

### A.3 Common Failure Modes Observed Historically

- Structural similarity incorrectly assumed to imply semantic equivalence
- Heuristic logic leaking into normalization
- Silent reinterpretation of metrics by downstream consumers

These failures motivated the strict separation of law (this document) from exploration and analysis.

---

**End of Project Bible**