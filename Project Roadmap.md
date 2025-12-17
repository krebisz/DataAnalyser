PROJECT ROADMAP

1. Purpose

This document outlines the planned evolution of the project, describing major phases, their goals, and the order in which capabilities are introduced.

It is directional, not binding.
Binding rules reside in the Project Bible.

2. Guiding Principles

Roadmap execution adheres to the following constraints:

Semantic correctness over convenience

Explicit contracts before implementation

Non-destructive, additive evolution

Clear separation between truth and exploration

Execution proceeds only after foundations are locked

3. Phase Overview
   Phase 1 — Ingestion & Persistence (Completed)

Goal:
Establish reliable ingestion of heterogeneous data sources and persist raw / lightly normalized records.

Key outcomes:

Multiple parsers (CSV / JSON)

Unified storage via HealthMetric

Lossless data capture

Provider-agnostic ingestion

Status: ✅ Complete

Phase 2 — Canonical Semantics & Normalization Foundations (Completed)

Goal:
Formalize semantic authority and remove ambiguity from metric meaning.

Key outcomes:

Canonical Metric Series (CMS) contract

Explicit canonical metric identity rules

Identity resolution scaffolding

HealthMetric → CMS mapping contract

Derived / dynamic metric identity framework

Structural / manifold analysis explicitly constrained

Contracts anchored in Project Bible and SYSTEM_MAP

Status: ✅ Complete

This phase establishes the semantic “truth layer” of the system.

Phase 3 — Execution: Canonical Identity & CMS Integration (Completed)

Goal:
Introduce real behavior while preserving existing ingestion and storage.

Scope:

Implement concrete identity resolution (metric family by metric family)

Begin with Weight metric family

Implement CMS mapping in shadow mode

No destructive changes to existing flows

No changes to DataVisualiser yet

Deliverables:

Deterministic identity resolution

CMS emitted alongside existing outputs

Diagnostics for identity and mapping failures

Status: ✅ Complete (core goals achieved; additional metric families may be added incrementally).

Phase 4 — Consumer Adoption & Visualization Integration (In Progress)

Goal:
Allow downstream systems (DataVisualiser) to consume CMS safely.

Scope:

Define minimal CMS dependency for DataVisualiser

Parallel support for legacy HealthMetric paths

Explicit opt-in to CMS-based workflows

No forced migration

Outcome:

Safer aggregation

Explicit composition

Reduced semantic ambiguity in UI
Generalized cyclic distribution visualizations (e.g., weekly, hourly, or N-bucket cycles)
Foundation for user-defined metric transformations with preview tables feeding the charting pipeline

Status: ▶ In Progress (~55% complete; CMS infrastructure and two strategies migrated, UI integration and remaining strategy migration ongoing)

Phase 5 — Derived Metrics & Analytical Composition (Planned)

Goal:
Enable explicit creation of new metrics through composition and aggregation.

Scope:

Dynamic / derived metric identity instantiation

Explicit aggregation and transformation pipelines

Support for ephemeral and persistent derived metrics

No implicit promotion to canonical truth

Outcome:

Advanced analysis without semantic erosion

User-driven metric synthesis

Phase 6 — Structural / Manifold Analysis (Future)

Goal:
Reintroduce advanced structural analysis capabilities without compromising semantic authority.

Scope:

Structural similarity detection

Equivalence class exploration

Analytical suggestion systems

Constraints:

Non-authoritative

No automatic promotion

Explicit declaration required for semantic impact

4. Out of Scope (Explicit)

The roadmap does not include:

Heuristic identity inference

Silent aggregation

Implicit semantic promotion

Forced migrations

Architectural shortcuts

5. Evolution Policy

Phases may overlap only when explicitly declared

Earlier phases must not be weakened by later work

Any deviation from this roadmap requires:

explicit justification

document updates

agreement

6. Intermediate Goals (Descriptive, Non-Binding)

The following goals are **descriptive**, not binding; they clarify expected evolution within the existing phases:

- **Generalized cyclic distribution charts**
  - Extend current weekly distribution visualizations into a generic cyclic distribution engine (e.g., weekly, hourly, or N-bucket cycles) without altering underlying semantic contracts.
- **User-defined metric transformations with preview**
  - Allow users to define explicit transformations over canonical metric series (e.g., `A + B - C`, `sqrt(A)`), preview representative “before/after” values in a small grid, and feed the resulting derived series into the existing charting pipeline as ephemeral, non-canonical metrics.

End of Project Roadmap
