# 📘 SYSTEM_MAP.md

(Canonical System Orientation — NOW + INTENT)

---

## 0. Purpose & Scope

This document captures:

- What the system is **right now (NOW)**
- What it is intentionally evolving toward **(INTENT)**

It does **not** enumerate files or symbols.
Generated artifacts are authoritative for structure and dependencies.

Use this document to answer:

> “Where should changes go, and what should remain stable?”

---

## 1. System Identity (NOW)

### System Role

DataFileReaderRedux is a data ingestion, normalization, computation, and visualization system designed to explore, transform, and surface structured insights from heterogeneous data sources.

### Core Characteristics

- Strong separation between ingestion, computation, and visualization
- Explicit strategy-based extensibility
- UI-driven exploratory analysis
- Emphasis on correctness, traceability, and inspectability over automation magic

---

## 2. Core Subsystems (NOW)

### 2.1 Ingestion & Normalization

#### 2.1.1 Canonical Ingestion Pipeline

The system enforces a **strict, multi-stage ingestion pipeline** whose purpose is to isolate *structural parsing* from *semantic interpretation*.

The canonical pipeline is:

```
Source
 → Parser (structure only)
 → Raw Record (lossless, neutral)
 → Normalization Pipeline (semantic authority)
 → Canonical Metric Series
```

Each stage has **explicit responsibilities and prohibitions**.
Violations of these boundaries are considered architectural defects.

---

#### 2.1.2 Parser Boundary — Structural Responsibility Only

**Parsers are responsible solely for structural extraction.**

Parsers MAY:
- read files, databases, or APIs
- traverse schemas or record layouts
- extract raw fields and values
- preserve source metadata and references
- emit raw, lossless records

Parsers MUST NOT:
- assign metric meaning or identity
- normalize units or values
- interpret timestamps or timezones
- apply aggregation logic
- embed domain semantics

Parsers answer only the question:

> “What data exists here, and where did it come from?”

All semantic interpretation is explicitly forbidden at this layer.

---

#### 2.1.3 Raw Record Boundary — Semantic Quarantine

Raw records represent **uninterpreted data** extracted from sources.

Properties:
- lossless
- domain-neutral
- unnormalized
- fully traceable to the source

Raw records are a **transitional abstraction**, used to decouple parsing from meaning assignment.

They exist to preserve ambiguity until interpretation is performed deliberately and centrally.

---

#### 2.1.4 Normalization Pipeline — Semantic Authority

All **semantic meaning** is assigned within the normalization pipeline.

This pipeline is the **single authoritative location** for:

- metric identity resolution
- timestamp and interval normalization
- timezone handling
- unit normalization and conversion
- value coercion
- dimensional context assignment
- data quality annotation

Semantic interpretation MUST NOT occur:
- in parsers
- in helpers
- in computation strategies
- in rendering logic
- in UI or ViewModels

Normalization is deterministic, inspectable, and order-explicit.

---

#### 2.1.5 Metric Identity Resolution — Single Point of Truth

Metric identity is resolved **exactly once** during normalization.

After normalization:
- every metric has a canonical identity
- identity does not change downstream
- no component may reinterpret or infer identity

Metric identity resolution may use:
- raw field names
- source metadata
- domain configuration
- explicit mapping rules

It must not rely on:
- string heuristics embedded in downstream layers
- duplicated logic across components

---

#### 2.1.6 Time Normalization — Canonical Time Guarantee

All timestamps and intervals are normalized within the ingestion pipeline.

After normalization:
- downstream layers assume time is canonical
- no strategy or renderer performs time correction
- aggregation boundaries are explicit and consistent

Time normalization includes:
- instant vs interval resolution
- timezone normalization
- temporal resolution
- aggregation semantics

Incorrect or ambiguous time handling is treated as an ingestion-layer defect.

---

#### 2.1.7 Canonical Metric Series — Downstream Contract

The output of ingestion is a **canonical metric series**.

This representation guarantees:
- resolved metric identity
- canonical time axis
- normalized values and units
- explicit dimensions and context
- preserved provenance

All downstream components operate exclusively on this representation.

Once data crosses this boundary:
- computation trusts ingestion
- rendering ignores origin
- UI treats metrics as abstract analytical signals

---

#### 2.1.8 Stability Expectations

**NOW**
- Parsers may currently violate some boundaries (legacy tolerance)
- Normalization logic may be partially distributed

**INTENT**
- Enforce boundaries strictly
- Centralize all semantic logic
- Enable domain-agnostic ingestion
- Support future reflective and emergent behaviors without parser intelligence

---

### 2.2 Computation & Analytics

**Responsibility:**
- Transform normalized data into metrics, aggregates, and derived values
- Encapsulate analytical logic

**Stability Expectation:**
- Medium stability
- Strategies are expected to grow

**INTENT:**
- Encourage composable strategies
- Allow multiple analytical “views” over the same data

---

### 2.3 Visualization & UI

**Responsibility:**
- Render computed results
- Enable interactive exploration
- Coordinate user intent → computation → rendering

**Stability Expectation:**
- Lower stability
- Iterative refinement expected

**INTENT:**
- Richer metadata surfacing
- More consistent interaction semantics across charts
- Gradual convergence toward reusable visualization primitives

---

## 3. Execution Paths That Matter (NOW)

Only the important ones are listed.

### 3.1 Data Load → Visualization

High-level flow:

```
User Input
 → Ingestion
   → Normalization
     → Computation Strategy
       → Visualization Render
```

**INTENT:**
This path must remain inspectable at each stage.
Hidden or implicit transitions are considered architectural smells.

---

## 4. Extension Points (NOW)

### Accepted Extension Mechanisms

- Strategy-based computation
- Pluggable ingestion logic
- UI composition via ViewModels

### Discouraged Extension Patterns

- Ad-hoc logic inside UI layers
- Cross-subsystem shortcuts
- Implicit coupling via shared state

**INTENT:**
New functionality should preferentially appear as:
- new strategies
- new adapters
- new ViewModel-level coordination

—not as deep modifications to existing engines.

---

## 5. Fragility & Care Zones (NOW)

The following areas require caution:

- UI ↔ computation boundaries
- Cross-strategy coordination
- State propagation across ViewModels

These areas are not forbidden, but changes should be:
- narrow in scope
- explicitly justified
- validated via inspection

**INTENT:**
Over time, reduce fragility by clarifying contracts, not by centralizing logic.

---

## 6. Terminology (Canonical Meanings)

| Term | Meaning in this System |
|-----|------------------------|
| Ingestion | Raw data acquisition |
| Normalization | Deterministic transformation into internal form |
| Strategy | Replaceable computation logic |
| Metric | Computed analytical output |
| State | UI-visible, user-relevant system condition |

---

## 7. Relationship to Generated Artifacts

Authoritative factual sources:

- project-tree.txt — repository structure
- codebase-index.md — symbols, namespaces, roles
- dependency-summary.md — structural coupling

This document provides interpretation, not inventory.

---

## 8. Update Policy (Important)

Update this document only when:
- subsystem responsibilities change
- a new core execution path is introduced
- architectural intent shifts

Do not update for:
- refactors
- renames
- implementation details

---

**End of SYSTEM_MAP.md**

