# PROJECT OVERVIEW
**Status:** Descriptive  
**Scope:** System intent, current capabilities, and evolutionary direction  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, and `Project Roadmap.md`  
**Active Subsystem Execution Reference:** `DataVisualiser_Subsystem_Plan.md` for Phase 7 exploratory capability expansion

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

The project exists to provide a trustworthy canonical data reasoning environment for exploring messy, real-world, time-indexed information, with an explicit emphasis on:

- canonical truth
- deterministic computation
- explicit semantics
- reversible interpretation
- visible uncertainty
- downstream flexibility in how results are selected, transformed, composed, and consumed

The system favors clarity and auditability over automation or convenience.
It should not be understood as a reporting tool that only exposes preselected outputs.
It is intended to become a platform that can preserve data faithfully, standardize it, reason over it explicitly, and serve those results to multiple kinds of clients.

**Strategic position:** The system's differentiator is that every derived result has explicit provenance, inspectable reasoning, and visible confidence. The architecture was designed for this from the ground up — canonical truth preservation, deterministic derivation, and evidence infrastructure that can prove its own correctness are foundational, not retrofitted. The reasoning engine is the center of the system; charts and other delivery surfaces are consumers of its output, not the definition of the platform.

It is also a single-maintainer project, so the near-term execution style must favor bounded, high-payoff steps that improve reliability, legibility, and operational tolerance without demanding constant large-scale reorientation.

---

## 3. What the System Is (Today)

At present, the system:

- ingests raw metric data losslessly
- assigns meaning through declarative normalization
- produces Canonical Metric Series (CMS) as the default comparison and interoperability substrate
- computes derived results deterministically
- increasingly exposes raw, normalized, canonical, and derived views through explicit downstream boundaries
- visualizes results through multiple chart types
- validates behavior through parity testing
- supports parallel legacy and CMS execution paths during migration
- increasingly exposes downstream-safe derived and interpretive result shaping through explicit orchestration and rendering seams
- emits tab-scoped reachability/evidence exports from both the Charts and Syncfusion surfaces through the shared evidence service

Canonical semantics form the foundation for all trusted meaning and for all computation that claims canonical comparability.
Charts are currently the dominant delivery surface, but they are not the intended limit of the platform.

---

## 4. Core Capabilities (Current)

### 4.1 Canonical Semantics & CMS

- Canonical Metric Series (CMS) is the default canonical interoperability substrate
- Metric identity is stable and explicit
- Normalization is deterministic and staged
- Downstream systems may consume raw, normalized, canonical, or derived views only through explicit boundaries with visible provenance and no semantic reinterpretation

---

### 4.2 Strategy-Based Computation

The system supports multiple computation strategies, including:

- single metric
- combined metrics
- multi-metric comparisons
- temporal distributions
- transformations, currently supporting unary and binary operations, with intended expansion to ternary and higher-order operations across multiple datasets, operations, and result graphs
- explicit downstream result composition over selected metrics, submetrics, and contextual slices

Strategies may exist in both legacy and CMS forms during migration, with parity validation enforcing correctness.

---

### 4.3 Visualization, Delivery & Interaction

Current delivery surfaces provide:

- time-series charts
- distribution views
- transform previews
- compositional and comparative charts
- dynamic chart visibility and state management
- backend-qualified multi-vendor rendering experiments in support of rendering-boundary isolation
- exports and evidence paths that observe execution and result state
- a shared metric-selection/date/CMS control surface reused by both the Charts and Syncfusion tabs
- a generic `WorkspaceTabHost` shell with header/body slots, specialized by `ChartTabHost` for chart tabs and reused directly by the Admin manager view with Admin-specific controls

The broader intended model is:

- consumers express what they want to inspect or compare
- request and presentation planning shape those needs into explicit downstream instructions
- orchestration coordinates the declared work
- derived and interpretive result sets are produced explicitly
- qualified delivery clients render or transport those results in a uniform way

UI components are controller-based, but full standardization is still an active architectural direction rather than finished reality.
The intended direction is convergence toward standardized graph hosts, shared option/toggle affordances, and programmable derived-result composition on qualified chart surfaces, without making chart controllers the semantic center of the system.
That direction explicitly includes future controller capability for dynamic data generation from multiple datasets and operations, with more than one graph rendered on a single qualified chart where the chart family supports it.
The recent architecture rehaul established explicit orchestration, rendering, theme, and evidence seams, while leaving a small number of large concentration points as intentional debt rather than hidden unfinished migration work.
Current evolution therefore prioritizes sustainable control over aggressive expansion: fewer recurring failures, better tolerance of real-world dataset sizes, clearer ownership, and only then broader capability growth.

---

## 5. Interpretive & Exploratory Capabilities (Evolving)

Beyond raw computation, the system increasingly supports interpretive exploration, including:

- trend identification and comparison
- compositional analysis (part vs whole)
- transform-based derived views
- future programmable chart composition over selected metric and submetric inputs
- contextual subset selection, filtering, and comparison over declared views
- pivot-oriented inspection (event-relative views)
- dynamic visual cues (colouring, emphasis)

These capabilities are non-authoritative reasoning operations applied on top of canonical truth. They are expressed as analytical programs through the reasoning engine and delivered to consumers through explicit downstream contracts.

They exist to aid understanding, not to redefine meaning.

---

## 6. Confidence & Reliability (Foundational)

Confidence is a core property of every result the system produces, not a decorative layer added after the fact. The architecture is designed so that no derived result exists without an explicit trust context — the provenance chain, the data quality, and the reasoning path are all visible.

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

## 7. Derived & Dynamic Results

Derived metrics and result sets are created through explicit composition or transformation of canonical metrics or other declared downstream-safe inputs.

Characteristics:

- each derived metric has its own identity
- provenance is always preserved
- derived metrics are non-canonical by default
- promotion to canonical truth is explicit and declarative
- future chart programs may compose and render more than one derived result set at a time on qualified chart families
- future transform-style chart controllers are intended to generate dynamic result sets from multiple datasets and operations, not remain limited to one unary/binary result at a time
- future non-chart consumers should be able to consume those same explicit result sets through uniform downstream contracts

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
**Closed**

- CMS strategies are implemented across the active strategy families
- Parallel legacy execution remains as bounded compatibility rather than open migration uncertainty
- Closure is now supported by current March 2026 reachability/parity evidence under the approved export path

---

### Phase 3.5 - Orchestration Layer Assessment  
**Closed**

- Unified cut-over behavior exists
- CMS is preserved through orchestration
- Reachability evidence has been regenerated through the approved evidence path for the active live surfaces

---

### Phase 4 - Consumer Adoption & Visualization Integration  
**Closed**

- CMS is materially adopted by the active consumer-facing visualization paths
- Current March 2026 exports and green default test lanes provide the present evidence required for closure
- Parity is now treated as a currently satisfied obligation for the active live surfaces rather than a historical claim

---

### Phase 5 - Architecture Rehaul & Backend Qualification  
**Closed / Maintenance Discipline Active**

- Structural repair, boundary reduction, and backend qualification have been completed as an explicit rehaul phase
- Qualified seams now exist for orchestration, rendering contracts, theme ownership, and evidence/export flow
- A small set of residual concentration points remains explicit intentional debt rather than hidden unfinished migration work
- Future change is expected to preserve the path toward standardized programmable chart hosts and broader downstream consumers rather than reopen prototype-era boundary leakage

---

### Phase 6 - Architectural Legibility & Concern Reconciliation  
**Closed**

- All sub-phases (6.1–6.7) closed, including 6.3 VNext widening — all active chart families route through the VNext reasoning engine with automatic legacy fallback
- All 5 global closure conditions met: similar responsibilities have obvious homes, irreducible operations are centralized, truth/derivation/orchestration/delivery seams are defended, outliers are explicit and bounded, all capabilities are preserved
- Named outliers materially reduced: `MainChartsEvidenceExportService` (1,209→131), `TransformDataPanelControllerAdapter` (857→257), `BaseDistributionService` (612→296), `BarPieChartControllerAdapter` (503→197), `ChartRenderEngine` (452→333), `DataFetcher` decomposed into focused query groups
- Evidence/export boundary decomposed into `UI/MainHost/Evidence/`, `UI/MainHost/Export/`, and `UI/MainHost/Coordination/` sub-namespaces
- VNext active for all chart families: Main/Normalized/Diff/Ratio via `VNextMainChartIntegrationCoordinator`; Distribution/WeekdayTrend/Transform/BarPie via `VNextSeriesLoadCoordinator` with per-family identity programs
- Runtime-path tracking (`LoadRuntimeState`) and VNext signature-chain diagnostics emitted in evidence exports for all chart families
- Evidence exports now include `ExportScope`; Charts and Syncfusion exports share the same export path, and tab switches are recorded as session milestones
- Known debt carried to Phase 7: `MainChartsView` host concentration (~1,242 lines, genuinely host-level), `SyncfusionChartsView` parallel-host concentration (~719 lines), adapter pattern variation (accepted as domain variation)
- 455 source files, 657 automated tests, 53 architecture guardrails

---

### Phase 7 - Exploratory & Confidence Capability Expansion  
**Planned / Entry Gate Satisfied**

- Interpretive overlays, confidence-aware views, structural exploration, and programmable multi-result composition remain intended
- Phase 6 closure satisfies the entry gate: the hierarchy is now legible enough that new power can be added without amplifying entropy
- Known debt inherited from Phase 6 is documented and bounded

---

### Phase 8 - UI, State, & Integration Consolidation  
**Planned / Blocked**

- Standardized graph hosts, shared option/toggle surfaces, and predictable integration behavior are intended here
- This phase exists to consolidate the UI after Phase 7 has advanced enough to support that convergence safely

---

## 9. Evolutionary Direction (Non-Exhaustive)

The project is intentionally open-ended.

Future directions may include:

Reasoning-engine-first evolution:
- generalize the reasoning engine beyond chart-program output toward composable analytical programs
- make analytical programs inspectable, modifiable, and replayable
- establish non-chart consumers as first-class delivery targets of reasoning-engine output
- make confidence and provenance integral to every result, not optional annotations

Capability expansion:
- richer interpretive overlays
- expanded transform capabilities
- standardized programmable chart hosts across the current chart families
- multi-result derived chart composition on qualified rendering surfaces
- broader downstream consumer support beyond current chart clients
- more explicit request/result composition over selected canonical and derived views
- compositional and relational analysis
- confidence-aware visualizations
- rules-based option gating
- advanced exploratory views

These directions represent intent, not immediate commitment.

All future work must respect canonical boundaries and phase discipline.

Exploratory and confidence-related capabilities are no longer treated as informal future ideas.
They remain explicitly staged and gated, but they now sit behind the current legibility-first reconciliation phase so that future power is added onto a coherent structure rather than a muddled one.

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
- The reasoning engine is the center of the system; delivery surfaces are consumers of its output
- Confidence is foundational — every result carries explicit provenance, reasoning, and trust context
- Interpretation is powerful but bounded
- Exploration is supported without semantic erosion
- Charts are the current dominant client, not the final definition of the platform
- The broader direction is a reasoning environment that can serve multiple qualified consumers uniformly
- The architecture rehaul and legibility reconciliation are complete; the next critical work is exploratory capability expansion and VNext family widening
- Evolution is intentional and evidence-bound

This overview describes the system as it exists today and the direction it is deliberately moving toward.

---

**End of Project Overview**
