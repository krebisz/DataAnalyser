# PROJECT OVERVIEW
**Status:** Descriptive  
**Scope:** System intent, current capabilities, and evolutionary direction  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `Project Roadmap.md`, and `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md`

---

## 1. Purpose

This document provides a descriptive overview of the project:

- what the system currently does
- how it is structured conceptually
- what direction it is evolving toward
- what constraints govern that evolution

It does not define architectural law or sequencing authority.

If a conflict exists, higher-authority documents prevail.

---

## 2. Project Intent (High-Level)

The project exists to provide a trustworthy analytical environment for exploring time-indexed metrics, with an explicit emphasis on:

- canonical truth
- deterministic computation
- explicit semantics
- reversible interpretation
- visible uncertainty

The system favors clarity and auditability over automation or convenience.

---

## 3. What the System Is (Today)

At present, the system:

- ingests raw metric data losslessly
- assigns meaning through declarative normalization
- produces Canonical Metric Series (CMS)
- computes derived results deterministically
- visualizes results through multiple chart types
- validates behavior through parity testing
- supports parallel legacy and CMS execution paths during migration

Canonical semantics form the foundation for all computation.

---

## 4. Core Capabilities (Current)

### 4.1 Canonical Semantics & CMS

- Canonical Metric Series (CMS) is the sole analytical truth
- Metric identity is stable and explicit
- Normalization is deterministic and staged
- Downstream systems consume CMS without reinterpretation

---

### 4.2 Strategy-Based Computation

The system supports multiple computation strategies, including:

- single metric
- combined metrics
- multi-metric comparisons
- temporal distributions
- transformations, currently including unary and binary flows with broader programmable composition intended

Strategies may exist in both legacy and CMS forms during migration, with parity validation enforcing correctness.

---

### 4.3 Visualization & Interaction

The UI layer provides:

- time-series charts
- distribution views
- transform previews
- compositional and comparative charts
- dynamic chart visibility and state management
- backend-qualified multi-vendor rendering experiments in support of rendering-boundary isolation

UI components are controller-based, but full standardization is still an active architectural direction rather than finished reality.
The intended direction is convergence toward standardized graph hosts with shared option/toggle affordances and programmable derived-result composition on qualified chart surfaces.

---

## 5. Interpretive & Exploratory Capabilities (Evolving)

Beyond raw computation, the system increasingly supports interpretive exploration, including:

- trend identification and comparison
- compositional analysis (part vs whole)
- transform-based derived views
- future programmable chart composition over selected metric and submetric inputs
- pivot-oriented inspection (event-relative views)
- dynamic visual cues (colouring, emphasis)

These capabilities are non-authoritative overlays applied on top of canonical truth.

They exist to aid understanding, not to redefine meaning.

---

## 6. Confidence & Reliability (Explicit Direction)

The system supports, or is explicitly staged to support, representation of data confidence and reliability, including:

- statistical identification of atypical readings
- visual marking of low-confidence points
- optional attenuation or exclusion from interpretive overlays
- preservation of raw and canonical values

Confidence assessments:

- are annotations, not mutations
- are explicitly declared
- are reversible
- never influence normalization or identity

This allows uncertainty to be visible without compromising trust.

---

## 7. Derived & Dynamic Metrics

Derived metrics are created through explicit composition or transformation of canonical metrics.

Characteristics:

- each derived metric has its own identity
- provenance is always preserved
- derived metrics are non-canonical by default
- promotion to canonical truth is explicit and declarative
- future chart programs may compose and render more than one derived result set at a time on qualified chart families

Derived results may be ephemeral (session-scoped) or persisted, depending on intent.

---

## 8. Current Phase Status (Descriptive)

This section reflects observed implementation state, not aspiration.
Sequencing authority remains the roadmap; where older descriptive claims drifted, the roadmap wins.

### Phase 1 - Ingestion & Persistence  
**Complete**

- Lossless ingestion
- Unified persistence
- No semantic inference at ingestion

---

### Phase 2 - Canonical Semantics & Normalization  
**Complete**

- Canonical Metric Series established
- Deterministic normalization
- Stable identity resolution

---

### Phase 3 - Strategy Migration  
**Implemented / Revalidation Required**

- CMS strategies are broadly implemented
- Parallel legacy execution remains as migration compatibility
- Historical evidence exists, but closure requires current-date revalidation under the roadmap evidence rules

---

### Phase 3.5 - Orchestration Layer Assessment  
**Implemented / Revalidation Required**

- Unified cut-over behavior exists
- CMS is preserved through orchestration
- Reachability evidence must be regenerated through the approved evidence path before closure is treated as current

---

### Phase 4 - Consumer Adoption & Visualization Integration  
**Implemented / Revalidation Required**

- CMS is materially adopted by consumer-facing visualization paths
- UI migration is advanced but not accepted as finally closed without present evidence
- Parity remains a phase obligation rather than a historical claim

---

### Phase 5 - Architecture Rehaul & Backend Qualification  
**Open / Current Critical Path**

- Structural repair, boundary reduction, and backend qualification are the active focus
- Current work is intended to preserve the path toward standardized programmable chart hosts
- Rendering contracts, orchestration seams, and evidence discipline must close before exploratory expansion opens

---

### Phase 6 - Exploratory & Confidence Capability Expansion  
**Planned / Blocked**

- Interpretive overlays, confidence-aware views, structural exploration, and programmable multi-result chart composition are intentionally staged here
- This phase remains blocked until earlier phases are genuinely closed with current evidence

---

### Phase 7 - UI, State, & Integration Consolidation  
**Planned / Blocked**

- Standardized graph hosts, shared option/toggle surfaces, and predictable integration behavior are intended here
- This phase exists to consolidate the UI after the architecture is strong enough to support that convergence safely

---

## 9. Evolutionary Direction (Non-Exhaustive)

The project is intentionally open-ended.

Future directions may include:

- richer interpretive overlays
- expanded transform capabilities
- standardized programmable chart hosts across the current chart families
- multi-result derived chart composition on qualified rendering surfaces
- compositional and relational analysis
- confidence-aware visualizations
- rules-based option gating
- advanced exploratory views

These directions represent intent, not immediate commitment.

All future work must respect canonical boundaries and phase discipline.

Exploratory and confidence-related capabilities are no longer treated as informal future ideas.
They are explicitly staged and gated under Phase 6 of the roadmap, with declared scope, constraints, and closure conditions to prevent semantic erosion while supporting intentional exploration.

---

## 10. What This Project Is Not

To avoid ambiguity, the system is not:

- an automated decision engine
- a semantic inference system
- a self-correcting data authority
- a recommendation engine
- an AI-driven reinterpretation layer

Human judgement remains central.

The system exists to support reasoning, not replace it.

---

## 11. Summary

- Canonical truth is stable and immutable
- Interpretation is powerful but bounded
- Confidence is explicit, not implicit
- Exploration is supported without semantic erosion
- Standardized programmable charting is intended, but still phase-gated
- Evolution is intentional and evidence-bound

This overview describes the system as it exists today and the direction it is deliberately moving toward.

---

**End of Project Overview**
