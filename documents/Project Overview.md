# PROJECT OVERVIEW
**Status:** Descriptive  
**Scope:** System intent, current capabilities, and evolutionary direction  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `DataVisualiser-Architectural-Vocabulary.md`, and `Project Roadmap.md`  
**Execution References:** current implementation/migration plans provide active task order and implementation state; `DataVisualiser_Subsystem_Plan.md` remains supporting subsystem context
**Architectural Grammar Reference:** `DataVisualiser-Architectural-Vocabulary.md` for promoted concepts, ownership containers, target hierarchy, and do-not-confuse distinctions

---

## 1. Purpose

This document provides a descriptive overview of the project:

- what the system currently does
- how it is structured conceptually
- what vocabulary should be used to understand its architectural direction
- what direction it is evolving toward
- what constraints govern that evolution

It does not define architectural law, canonical vocabulary, or sequencing authority.

Current alignment note:

```text
This overview is descriptive.
It should not be used as the active implementation plan or progress log.
The roadmap governs long-term sequencing.
The architectural vocabulary governs target grammar.
The current implementation/migration plan governs local execution state.
```

If a conflict exists, higher-authority and governing reference documents prevail.

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

The current target direction is best understood through the architectural vocabulary and enhanced ownership containers: authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence.

This vocabulary matters because it prevents recurring drift:
- capability is not merely feature delivery
- composition is not builder plumbing
- consumer is not the same as presentation
- interaction is not event wiring
- overlay is not rendering
- provenance is not diagnostics
- authority is not orchestration

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
- has a closed pre-Phase-7 VNext render-plan preservation baseline for backend-neutral render intent, render buffers, density policy, and adapter dispatch across the active chart families/tabs
- has materially implemented target-spine structures, including VNext-native consumption contracts, consumer-neutral surface models, capability contracts, Operation Chain, derived dataset, evidence, and trace structures
- still retains active legacy/UI/state pressure points such as `ChartDataContext`, `MainChartsView`, `ChartControllerFactory*`, and related rendering/state hubs
- now has a formal architectural vocabulary for describing ownership, capability, composition, consumers, interaction, overlays, boundaries, and evidence without relying on ad hoc interpretation

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
- VNext render-plan contracts now provide backend-neutral delivery to LiveCharts, Syncfusion, and plugin renderers through adapter-backed live routes where those surfaces are wired
- exports and evidence paths that observe execution and result state
- a shared metric-selection/date/CMS control surface reused by both the Charts and Syncfusion tabs
- a generic `WorkspaceTabHost` shell with header/body slots, specialized by `ChartTabHost` for chart tabs and reused directly by the Admin manager view with Admin-specific controls

The broader intended model is:

- authority and provenance remain upstream
- consumers express what they want to inspect or compare
- a canonical intent / request shape captures that need before delivery concerns dominate
- reasoning/capability logic composes derived, confidence-aware, and interpretive outputs
- process/execution coordinates the declared work without becoming semantic authority
- contract/boundary seams convert engine-owned output into downstream-safe instructions
- projection/translation components translate across explicit seams without creating semantic meaning
- consumer/interaction layers adapt those contracts without redefining meaning
- terminal delivery infrastructure renders or transports those results through qualified backends
- render density and backend adapter choice are planned before concrete chart-library binding

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
- system-supported conclusions that recommend explicit follow-up computation or view changes based on declared results and confidence context
- future programmable chart composition over selected metric and submetric inputs
- contextual subset selection, filtering, and comparison over declared views
- pivot-oriented inspection (event-relative views)
- dynamic visual cues (colouring, emphasis)

These capabilities are non-authoritative reasoning operations applied on top of canonical truth. They should be expressed as reasoning-engine capabilities, carried through explicit contracts, adapted by consumers, and delivered by terminal infrastructure.

Overlay definition belongs with reasoning/capability or contract intent; overlay delivery belongs in terminal infrastructure.

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

Derived metrics and result sets are created through explicit reasoning-engine composition or transformation of canonical metrics or other declared downstream-safe inputs.

Composition here means lawful analytical assembly of inputs, operations, overlays, and results; it is not merely object construction or UI shaping.

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
- Closure is now supported by current April 2026 reachability/parity evidence under the approved export path

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
- Current April 2026 exports and green default test lanes provide the present evidence required for closure
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

- All sub-phases (6.1-6.7) closed, including 6.3 VNext widening - all active chart families have VNext-compatible request/program support and live VNext routes where appropriate, with legacy retained as compatibility/fallback/projection
- All 5 global closure conditions met: similar responsibilities have obvious homes, irreducible operations are centralized, truth/derivation/orchestration/delivery seams are defended, outliers are explicit and bounded, all capabilities are preserved
- Named outliers materially reduced: `MainChartsEvidenceExportService` (1,209->141), `TransformDataPanelControllerAdapter` (857->257), `BaseDistributionService` (612->296), `BarPieChartControllerAdapter` (503->199), `ChartRenderEngine` (452->333), `DataFetcher` decomposed into focused query groups
- Evidence/export boundary decomposed into `UI/MainHost/Evidence/`, `UI/MainHost/Export/`, and `UI/MainHost/Coordination/` sub-namespaces
- Strategy migration responsibilities are split between `StrategyCmsDecisionEvaluator` for CMS eligibility and `StrategyParityValidationService` for parity validation
- Admin workspace behavior now routes row loading, dirty tracking, filtering, save state, and milestone recording through `AdminMetricsManagerCoordinator`
- Tooltip formatting is split into focused helpers behind the stable `ChartTooltipFormattingHelper` facade
- Main and Syncfusion hosts now share the disposable UI-busy lease helper
- VNext-compatible request/program support exists for all chart families: Main/Normalized/Diff/Ratio via `VNextMainChartIntegrationCoordinator`; Distribution/WeekdayTrend/Transform/BarPie via `VNextSeriesLoadCoordinator` with per-family identity programs; legacy remains the compatibility/fallback/projection path while Phase 7 progressively deepens VNext authority
- Runtime-path tracking (`LoadRuntimeState`) and VNext signature-chain diagnostics emitted in evidence exports for all chart families
- Evidence exports now include `ExportScope`; Charts and Syncfusion exports share the same export path, and tab switches are recorded as session milestones
- Recent shared seams added before Phase 7: `ParitySeriesComparer`, `WorkspaceLoadCoordinator`, `WorkspaceSessionMilestoneRecorder`, and `BinaryMetricChartContextHelper`
- Pre-Phase-7 enabling infrastructure is complete and closed as a preservation baseline: `ChartRenderPlan`, neutral render buffers, `RenderDensityPolicy`, time-bucket render aggregation, backend capability descriptors, backend selection, and adapter dispatch now exist under VNext and are wired into live chart rendering across the active chart families/tabs
- Known debt carried to Phase 7: `MainChartsView` host concentration (~1,440 lines, genuinely host-level), `SyncfusionChartsView` parallel-host concentration (~859 lines), managed legacy/VNext coexistence, adapter pattern variation (accepted as domain variation)
- 493 source files, 737 DataVisualiser tests, 15 DataFileReader tests (Phase 6 closure baseline; current lane is ~505 source files and 996 DataVisualiser tests after Phases 19–23)

---

### Phase 7 - Exploratory & Confidence Capability Expansion  
**Historical / Superseded as Current Active Framing**

- Interpretive overlays, confidence-aware views, structural exploration, and programmable multi-result composition remain intended
- Migration Plan Phases 14–22 were completed during the earlier target-spine expansion period; the VNext spine was proved end-to-end through a MovingAverage capability, TabularSummary backend, and independent chart + API consumers
- The earlier active framing of Phase 7 as exploratory capability expansion has been superseded by post-target recalibration work
- Later family or tab exceptions must be handled explicitly and must not silently reopen closed Phase 5/6/6.3 or pre-Phase-7 primer work
- Future exploratory capability work should still align to the enhanced containers: authority/intent, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence
- the architectural vocabulary document provides the canonical grammar for interpreting these containers and promoted concepts
- Known debt inherited from Phase 6 and later convergence work remains documented and bounded
- New progress should be tracked against the current migration/implementation plan or later roadmap phases, not against closed Phase 5/6/6.3 or the closed pre-Phase-7 primer

---

### Phase 8 - Consumer, Interaction, UI, State, & Integration Consolidation  
**Planned / Blocked**

- Standardized graph hosts, shared option/toggle surfaces, consumer families, and predictable integration behavior are intended here
- This phase exists to consolidate consumer/UI/interaction concerns after Phase 7 has advanced enough to support that convergence safely
- At least one non-chart consumer should eventually prove that reasoning-engine output is genuinely consumer-agnostic rather than implicitly chart-shaped

---

## 8.5 Forward Architecture Direction (Descriptive)

The enhanced architecture direction is forward-only. It does not reopen closed Phase 5, Phase 6, Phase 6.3, or the closed pre-Phase-7 render-plan primer.

Current post-target direction adds an explicit recalibration gate before broad new capability or formal runtime expansion:

```text
rebaseline current density
inventory existing constructions
triage coverage gaps
reduce or contain pressure points where useful
then introduce minimal construction algebra only where justified
```

Future work should broadly progress through:

1. **Authority and Intent Clarification** — make authority, provenance, and canonical intent explicit.
2. **Reasoning and Capability Expansion** — express confidence, overlays, transforms, comparisons, and programmable composition as reasoning-engine capabilities.
3. **Process and Execution Separation** — keep workflow and routing separate from semantic meaning and result composition.
4. **Contract and Boundary Hardening** — make program, delivery, interaction, view, and multi-result contracts the downstream fan-out seam.
5. **Projection and Translation Discipline** — ensure builders, adapters, resolvers, selectors, and projectors translate across boundaries without becoming semantic authorities.
6. **Consumer and Interaction Separation** — treat charts, exports, APIs, and future clients as consumer families.
7. **Terminal Delivery Demotion** — keep rendering, backend adapters, route/host binding, and vendor lifecycle terminal and replaceable.
8. **Governance and Evidence Sidecar Isolation** — keep diagnostics, parity, reachability, qualification, and evidence observational.
9. **Later Broad Family Pattern Consolidation** — collapse repeated request/route/qualification/adapter patterns only after the upstream spine and contract seams are stable.

The architectural vocabulary provides the shared language for this direction; the overview only summarizes it descriptively.


---

## 8.6 Architectural Migration Progress Snapshot

The previous high-level architectural migration estimate was:

```text
Architectural migration: approximately 70–75% complete
Working estimate: ~72% (updated after Phases 19–22)
```

This estimate is retained as historical context only.

Current descriptive reading:

```text
The target architecture is materially real.
The old mesh has not disappeared.
The project is in post-target recalibration territory.
Current status is better described as substantial convergence with remaining pressure points than as a simple percentage.
```

| Area | Approx. completion | Descriptive meaning |
|---|---:|---|
| Vocabulary / conceptual model | 90% | The project now has stable architectural language for future work. |
| VNext reasoning spine | 80% | End-to-end spine proved through MovingAverage; reasoning-engine-centered direction is materially validated. |
| Contract / boundary model | 75% | CapabilityContracts threaded across all chart families; adapter dual-path retired; boundary enforcement materially strengthened. |
| Rendering demotion | 60% | Rendering is moving toward terminal delivery, though rendering infrastructure remains large. |
| Consumer / interaction separation | 65% | TabularSummaryChart proves non-chart consumer path; UI/presentation remains structurally heavy. |
| Governance / evidence | 75% | Evidence and parity infrastructure are strong, but must remain observational. |
| Legacy coexistence cleanup | 50–60% | Legacy remains managed compatibility/fallback/projection; integration seams classified and bounded. |

In plain terms:

- the migration is real and visible in code
- the reasoning engine, contracts, render-plan delivery, and evidence structures now exist as working architectural pieces
- the remaining work is mostly consolidation, enforcement, and selective relocation
- the main risk is not direction, but whether new boundary/provider/consumer constructs enforce real seams rather than simply renaming delivery routing

This progress estimate should not be used to reopen closed Phase 5, Phase 6, Phase 6.3, or pre-Phase-7 primer work.

---

## 9. Evolutionary Direction (Non-Exhaustive)

The project is intentionally open-ended.

Future directions may include:

Reasoning-engine-first evolution:
- generalize the reasoning engine beyond chart-program output toward composable analytical programs
- make analytical programs inspectable, modifiable, and replayable
- express new user-visible power as reusable capabilities where possible
- establish non-chart consumers as first-class delivery targets of reasoning-engine output
- keep interaction semantics contract-mediated rather than hidden in event/controller convenience
- make confidence and provenance integral to every result, not optional annotations

Capability expansion:
- richer interpretive overlays
- expanded transform capabilities
- standardized programmable chart hosts across the current chart families
- multi-result derived chart composition on qualified rendering surfaces
- broader downstream consumer support beyond current chart clients
- render-plan delivery with density-aware buffers for large ranges and backend-neutral adapters for LiveCharts, Syncfusion, and future plugins
- more explicit request/result composition over selected canonical and derived views
- compositional and relational analysis
- confidence-aware visualizations
- rules-based option gating
- advanced exploratory views

These directions represent intent, not immediate commitment.

Post-target direction also includes:

- requirements-to-language coverage
- construction inventory
- construction algebra
- operation / capability algebra
- typed relations
- multiplicity / derived datasets
- evidence sufficiency
- semantic interpretation and confidence
- analytical fitness
- bounded search
- multi-consumer output
- governance and emergence review
- scenario hardening

All future work must respect canonical boundaries, architectural vocabulary, enhanced container ownership, and phase discipline.

Exploratory and confidence-related capabilities are no longer treated as informal future ideas.
They remain explicitly staged and gated, but they now sit behind the closed legibility-first reconciliation phase so that future power is added onto a coherent structure rather than a muddled one.

---

## 10. What This Project Is Not

To avoid ambiguity, the system is not:

- an automated decision engine
- a semantic inference system
- a self-correcting data authority
- a recommendation engine
- an AI-driven reinterpretation layer
- a chart-first system where UI or rendering vocabulary defines the architecture

Human judgement remains central.

The system exists to support reasoning, not replace it.

---

## 11. Summary

- Canonical truth is stable and immutable
- The reasoning engine is the center of the system; delivery surfaces are consumers of its output
- The architectural vocabulary document now supplies the canonical grammar for promoted concepts, ownership containers, target hierarchy, and do-not-confuse distinctions
- Enhanced architecture containers now describe the forward shape: authority/provenance, reasoning/capability, process/execution, contract/boundary, projection/translation, consumer/interaction, terminal delivery, and governance/evidence
- Confidence is foundational — every result carries explicit provenance, reasoning, and trust context
- Interpretation is powerful but bounded
- Exploration is supported without semantic erosion
- Charts are the current dominant client, not the final definition of the platform
- The broader direction is a reasoning environment that can serve multiple qualified consumers uniformly
- Earlier migration estimates are retained as historical context only; the current descriptive state is substantial target-architecture convergence with remaining pressure points
- The architecture rehaul, legibility reconciliation, pre-Phase-7 render-plan delivery primer, and later target-spine convergence work are complete enough to shift the current focus toward post-target recalibration, pressure classification, and bounded formalisation
- Evolution is intentional and evidence-bound
- Long-term evolution now points toward bounded generativity: analytical construction growth governed by semantic authority, provenance, evidence, and boundary clarity

This overview describes the system as it exists today and the direction it is deliberately moving toward. It should not be used to reopen or re-track progress against closed historical phases, nor should it replace the architectural vocabulary source for precise concept ownership.

---

**End of Project Overview**
