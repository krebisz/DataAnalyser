# PROJECT OVERVIEW

## 1. Purpose

This document provides a high-level description of the project, its goals, and its intended direction.

It is descriptive, not binding.

---

## 2. Project Intent

The project aims to ingest heterogeneous personal and health-related data sources, normalize them into a deterministic and trustworthy metric space, and enable both computation and advanced analytical exploration.

---

## 3. Current Direction

The current architecture emphasizes:
- explicit semantic authority
- deterministic normalization
- canonical metric identity
- strict separation between truth and exploration

This direction prioritizes correctness and long-term maintainability over convenience.

---

## 4. Historical Evolution & Direction (Additive)

> **Additive Section — Contextual and explanatory only.**

### 4.1 Early Structural & Hierarchical Approach

Earlier designs explored generalized structural equivalence mechanisms, where data structures were analyzed hierarchically and assigned computed identifiers based on shape and relationships. This enabled powerful comparison across heterogeneous formats, particularly JSON-based inputs.

### 4.2 Limitations Encountered

While effective for discovery, the approach revealed several limitations when used as a foundation for metric computation:
- structurally similar data could represent fundamentally different semantics
- inferred equivalence led to ambiguity and drift
- reproducibility and auditability suffered over time

### 4.3 Transition to Metric-First Determinism

To address these issues, the system transitioned to a metric-first model:
- time (or interval) as the primary axis
- scalar values as the fundamental payload
- meaning resolved explicitly through declarative normalization stages

This ensured stability and trust while still allowing exploration elsewhere.

### 4.4 Future Analytical Extensions

The earlier structural insights were not discarded, but deliberately deferred. They now inform the planned Structural / Manifold Analysis Layer, which operates orthogonally to canonical semantics and does not alter truth without explicit promotion.

---

## 5. Relationship to Other Documents

- Binding rules reside in the Project Bible
- Conceptual layering is defined in SYSTEM_MAP
- Governance rules reside in MASTER_OPERATING_PROTOCOL

---

**End of Project Overview**


---

## Appendix B. Derived & Dynamic Metric Identity (Non-Binding)

> **Additive Appendix — does not modify any binding law above.**

Derived or dynamic metrics are first-class semantic entities created through **explicit composition, aggregation, or transformation** of existing canonical metrics.

Key constraints:
- Derived metrics MUST have their own canonical identity.
- Derived metrics MUST declare dimension, unit, and provenance.
- Source identities are never altered or overridden.
- No derived metric may be instantiated implicitly or heuristically.

Derived identities may be ephemeral (session- or query-scoped) or persistent. In all cases, identity creation must be **explicit, reviewable, and reversible**.

Analytical or structural systems may suggest derived identities but MUST NOT promote them without explicit declaration.


---

## Structural / Derived Metric Layer (Explicit Extension)

This layer formalizes **derived and dynamic metrics** created through explicit aggregation or transformation of canonical metrics.

Characteristics:
- Operates strictly on Canonical Metric Series (CMS)
- Produces new metric identities without altering source identities
- Supports both ephemeral (session-scoped) and persistent derived metrics

Constraints:
- No implicit promotion of derived metrics into canonical space
- All derived identities must be explicitly declared and versioned
- Structural or manifold analysis systems may suggest derivations but may not instantiate them autonomously

This layer exists above normalization and canonical identity resolution and below visualization.

