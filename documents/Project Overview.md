# PROJECT OVERVIEW
Status: Descriptive (Non-Binding)
Scope: Intent, Trajectory, and Current State

---

## 1. Purpose

This document provides a high-level description of the project, its intent, and its current trajectory.

It is **descriptive, not binding**.  
Architectural law resides in **Project Bible.md**.  
Execution discipline resides in **MASTER_OPERATING_PROTOCOL.md**.

---

## 2. Project Intent

The project exists to ingest heterogeneous structured data, normalize it into a **deterministic and semantically authoritative metric space**, and enable computation and visualization **without semantic erosion**.

The primary objective is **trustworthy metric meaning**, not visualization, analytics, or UI convenience.

Visualization and analytics are treated as **consumers of truth**, not definers of it.

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

## 3A. Declarative Mapping Policy (Additive - Descriptive)

Additive clarification aligned with Project Bible and SYSTEM_MAP.

Canonical identity mapping is treated as declarative infrastructure:

- Mapping sources are authoritative and dynamic (e.g., mapping tables).
- Hardcoded lists are permitted only as short-lived migration scaffolding.
- Manual upkeep is avoided once a declarative source exists.

This preserves determinism and avoids long-term semantic bottlenecks.

---

## 4. Historical Context & Design Evolution

> Contextual section — explanatory only.

### 4.1 Early Structural Exploration

Early iterations explored structural and hierarchical equivalence across heterogeneous inputs (notably JSON), enabling discovery through shape- and relationship-based analysis.

This approach proved valuable for exploratory insight.

---

### 4.2 Limitations Identified

When applied to persisted metrics and computation, structural inference introduced unacceptable risks:

- structurally similar data representing different semantics
- loss of auditability
- long-term semantic drift

These risks were deemed incompatible with the project’s trust objectives.

---

### 4.3 Transition to Canonical Metric Semantics

The system transitioned to a **metric-first deterministic model**:

- explicit time semantics
- scalar value primacy
- canonical metric identity
- declarative normalization stages

This established a **truth layer** that downstream systems can consume safely.

---

### 4.4 Deferred Structural Analysis

Structural and manifold-style analysis was **deliberately deferred**, not discarded.

It is now treated as a **future, non-authoritative analytical layer**, explicitly prevented from mutating canonical truth without declaration and promotion.

---

## 5. Relationship to Foundational Documents

This document must be interpreted alongside:

- **Project Bible.md** — architectural law and invariants
- **SYSTEM_MAP.md** — conceptual layering and execution boundaries
- **MASTER_OPERATING_PROTOCOL.md** — execution, collaboration, and failure discipline
- **Workspace Workflow.md** — workspace initialization and grounding rules
- **Project Roadmap.md** — sequencing and intent

In case of conflict, higher-authority documents prevail.

---

## 6. Current Phase Status (Descriptive)

This reflects **actual implementation state**, not aspiration.

### Phase 1 — Ingestion & Persistence  
**Complete**

- Lossless ingestion
- Unified persistence
- No semantic inference at ingestion

---

### Phase 2 — Canonical Semantics & Normalization Foundations  
**Complete**

- Canonical Metric Series (CMS)
- Metric identity resolution
- Deterministic normalization contracts

---

### Phase 3 — Canonical Identity & CMS Integration  
**Complete**

- CMS produced alongside legacy outputs
- Identity and semantics stabilized
- Downstream exposure via consumer interfaces

---

### Phase 4 — Consumer Adoption & Visualization Integration  
**In Progress (~85%)**

DataVisualiser currently:

- Consumes CMS through an explicit dependency surface
- Supports **parallel CMS and legacy execution paths**
- Migrates strategies incrementally (90% complete - all 8 strategies have CMS implementations)
- Uses parity harnesses to validate equivalence without forced cut-over
- **Orchestration layer migration in progress** (70% complete - StrategyCutOverService implemented)
- Provides **user-defined metric transformations**:
  - Unary: Logarithm, Square Root
  - Binary: Add, Subtract, Divide (Ratio)
- Treats transform results as:
  - ephemeral
  - non-canonical
  - non-promotable without declaration
- Integrates transform results into:
  - preview grids
  - charting pipeline
- Applies performance optimizations:
  - configurable SQL result limiting
  - elimination of redundant materialization
  - computation skipping for hidden charts

Parity is treated as a **phase obligation**, not an implementation detail.

**Recent Progress:**
- Phase 4A (Core Strategy Parity): Complete - 3 parity test suites passing
- Phase 4B (Transform Pipeline Parity): Complete - "Divide" operation added
- Phase 4C (Weekly/Temporal Migration): 75% complete - strategies exist, service cut-over completed, UI integration pending

---

### Phase 5 — Derived Metrics & Analytical Composition  
**Planned**

- Derived metrics are not authoritative by default
- Promotion to canonical truth requires explicit declaration

---

### Future — Structural / Manifold Analysis  
**Deferred**

- Non-authoritative
- Explicitly constrained
- Insight-only unless promoted declaratively

---

## 7. Derived & Dynamic Metrics (Clarification)

Derived metrics are **explicit semantic entities**, created through declared composition or aggregation of canonical metrics.

Constraints:

- Each derived metric has its **own identity**
- Source identities are never mutated
- Units, dimensions, and provenance are explicit
- Promotion to canonical truth is never implicit

Derived metrics may be ephemeral or persistent, but are always **intentional and reversible**.

---

## 7A. Transform Operations (Phase 4 Implementation)

The system currently supports **ephemeral transform operations**:

- Results are explicitly non-canonical
- Operations include unary and binary forms
- Results are visualized but not promoted
- Provenance is tracked (sources + operation)

### Infrastructure Architecture (Descriptive)

- Expression Tree Model:
  - `TransformExpression`
  - `TransformOperand`
  - `TransformOperation`
- Operation Registry:
  - `TransformOperationRegistry`
- Evaluation Engine:
  - `TransformExpressionEvaluator`
- Builder:
  - `TransformExpressionBuilder`
- Data Processing:
  - `TransformExpressionEvaluator` (includes merged TransformDataHelper functionality)
- Strategy Integration:
  - `TransformResultStrategy`

### Expansion Readiness

Provisioned to support:

- N-metric expressions
- Chained operations
- Complex expression trees

This aligns with Phase 4 goals while preserving Canonical boundaries.

---

## 8. Migration & Parity Status (Additive · Descriptive)

Additive clarification based on recent execution experience.

- CMS strategies may exist without being reachable
- Migration is only considered real once:
  - execution reachability is proven
  - parity is observable
- Partial migrations are expected and acceptable
- Non-reachable logic is treated as **non-existent**

**Current Migration Status (2025-01-XX):**

- **Strategy Migration (Phase 3)**: 90% complete
  - All 8 strategies have CMS implementations and factory support
  - StrategyCutOverService implemented for unified cut-over
  - Minor cleanup remaining (1 direct instantiation in StrategySelectionService)
  
- **Orchestration Assessment (Phase 3.5)**: 70% complete
  - StrategyCutOverService implemented and registered for all strategy types
  - ChartRenderingOrchestrator uses unified cut-over mechanism
  - ChartDataContextBuilder preserves CMS (doesn't convert to legacy)
  - WeeklyDistributionService migrated to use StrategyCutOverService
  - Minor cleanup remaining (StrategySelectionService)

- **File Reorganization & Code Abstraction**: 100% complete
  - All files reorganized per architectural layers
  - Strategies unified (SingleMetric, CombinedMetric)
  - Factory pattern consolidated (StrategyFactoryBase)
  - Common patterns extracted to shared helpers (StrategyComputationHelper, CmsConversionHelper, ChartHelper)
  - ~450+ lines of duplicate code eliminated

This document reflects **state**, not guarantee.

---

## 9. Summary

Canonical semantics form the foundation.  
Everything else builds on top — **explicitly, reversibly, and without inference**.

The system favors **trust, auditability, and long-term coherence** over speed or convenience.

---

End of Project Overview
