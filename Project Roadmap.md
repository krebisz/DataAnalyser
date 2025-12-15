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

Phase 3 — Execution: Canonical Identity & CMS Integration (Current)

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

Status: ▶ In Progress

Phase 4 — Consumer Adoption & Visualization Integration (Planned)

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

End of Project Roadmap