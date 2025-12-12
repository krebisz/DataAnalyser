> ⚠️ This document is part of the frozen core documentation set. See MASTER_OPERATING_PROTOCOL.md.

📘 SYSTEM_MAP.md

(Canonical System Orientation — NOW + INTENT)

0. Purpose & Scope

This document captures:

What the system is right now (NOW)

What it is intentionally evolving toward (INTENT)

It does not enumerate files or symbols.
Generated artifacts are authoritative for structure and dependencies.

Use this document to answer:

“Where should changes go, and what should remain stable?”

1. System Identity (NOW)
System Role

DataFileReaderRedux is a data ingestion, normalization, computation, and visualization system designed to explore, transform, and surface structured insights from heterogeneous data sources.

Core Characteristics

Strong separation between ingestion, computation, and visualization

Explicit strategy-based extensibility

UI-driven exploratory analysis

Emphasis on correctness, traceability, and inspectability over automation magic

2. Core Subsystems (NOW)
2.1 Ingestion & Normalization

Responsibility:

Read raw data from external sources

Normalize into internal, consistent representations

Stability Expectation:

High stability

Changes should be additive, not disruptive

INTENT:

Support more heterogeneous inputs

Preserve deterministic normalization rules

2.2 Computation & Analytics

Responsibility:

Transform normalized data into metrics, aggregates, and derived values

Encapsulate analytical logic

Stability Expectation:

Medium stability

Strategies are expected to grow

INTENT:

Encourage composable strategies

Allow multiple analytical “views” over the same data

2.3 Visualization & UI

Responsibility:

Render computed results

Enable interactive exploration

Coordinate user intent → computation → rendering

Stability Expectation:

Lower stability

Iterative refinement expected

INTENT:

Richer metadata surfacing

More consistent interaction semantics across charts

Gradual convergence toward reusable visualization primitives

3. Execution Paths That Matter (NOW)

Only the important ones are listed.

3.1 Data Load → Visualization

High-level flow:

User Input
 → Ingestion
   → Normalization
     → Computation Strategy
       → Visualization Render


INTENT:
This path must remain inspectable at each stage.
Hidden or implicit transitions are considered architectural smells.

4. Extension Points (NOW)
Accepted Extension Mechanisms

Strategy-based computation

Pluggable ingestion logic

UI composition via ViewModels

Discouraged Extension Patterns

Ad-hoc logic inside UI layers

Cross-subsystem shortcuts

Implicit coupling via shared state

INTENT:
New functionality should preferentially appear as:

new strategies

new adapters

new ViewModel-level coordination
—not as deep modifications to existing engines.

5. Fragility & Care Zones (NOW)

The following areas require caution:

UI ↔ computation boundaries

Cross-strategy coordination

State propagation across ViewModels

These areas are not forbidden, but changes should be:

narrow in scope

explicitly justified

validated via inspection

INTENT:
Over time, reduce fragility by clarifying contracts, not by centralizing logic.

6. Terminology (Canonical Meanings)
Term	Meaning in this System
Ingestion	Raw data acquisition
Normalization	Deterministic transformation into internal form
Strategy	Replaceable computation logic
Metric	Computed analytical output
State	UI-visible, user-relevant system condition
7. Relationship to Generated Artifacts

Authoritative factual sources:

project-tree.txt — repository structure

codebase-index.md — symbols, namespaces, roles

dependency-summary.md — structural coupling

This document provides interpretation, not inventory.

8. Update Policy (Important)

Update this document only when:

subsystem responsibilities change

a new core execution path is introduced

architectural intent shifts

Do not update for:

refactors

renames

implementation details

End of SYSTEM_MAP.md