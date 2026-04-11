# DATAVISUALISER SUBSYSTEM PLAN
**Status:** Active Architectural Execution Plan  
**Scope:** `DataVisualiser` hierarchy repair, boundary clarification, entropy reduction, subsystem consolidation, and VNext activation  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `Project Roadmap.md`, and `Project Overview.md`  
**Supersedes:** `DataVisualiser_Consolidation_Plan.md` and `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md`  
**Last Updated:** 2026-04-11

---

## 1. Purpose

This document is the single active execution plan for `DataVisualiser` architectural work.

It consolidates:
- The completed Phase 5 architecture rehaul (historical execution record)
- The active Phase 6 legibility-first consolidation cycle
- The first live VNext vertical slice (Phase 6.3)

The primary objective is to make `DataVisualiser`:

- legible to its architect again
- structurally coherent enough that similar work has one obvious home
- explicit enough that true outliers stand out immediately
- stable enough that future growth does not keep re-entering the system as "programming by exception"
- progressively authoritative through the VNext reasoning engine where proven live

The mandate is hierarchy repair and request/delivery standardization, not file-count reduction for its own sake.

---

## 1.5 START HERE - Handoff and Execution Defaults

Use this section as the default handoff entry point in a new conversation.

Current defaults:

- scope is `DataVisualiser` only
- posture is `Conservative-Pragmatic`
- one iteration must have one primary objective
- the default objective is now architectural legibility and VNext activation, not generic consolidation
- safely coupled slices are allowed only when they already share a real contract / host / route / responsibility pattern
- validation must happen on every significant refactor
- if live behavior changes, halt after automated validation and request targeted manual smoke before continuing
- assume single-maintainer execution with limited time
- prefer slices that can be completed in `1-3` focused sessions
- do not open more than one live-behavior risk front at a time
- reliability, performance tolerance, and legibility outrank theoretical purity

Supporting navigation aids:

- `SYSTEM_MAP.md` Appendix A (presentation pipeline spine)

Authority order:

1. `Project Bible.md`
2. `SYSTEM_MAP.md`
3. `Project Roadmap.md`
4. `Project Overview.md`
5. this document

Default validation commands:

1. `dotnet build DataAnalyser.sln -c Debug`
2. `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`

Default smoke rule:

1. no manual smoke for internal-only structural cleanup
2. targeted manual smoke only when live UI / render / export / theme / controller behavior is changed

---

## 2. Architectural North Star

`DataVisualiser` is part of a wider canonical data reasoning platform. Within that wider system, it should evolve toward a clean downstream role:

- canonical or downstream-safe data enters through explicit boundaries
- derived results, transforms, comparisons, overlays, and chart-program-style requests remain explicit and reversible
- orchestration coordinates execution without becoming a semantic authority
- rendering infrastructure translates already-defined intent into backend-safe behavior
- concrete clients become delivery surfaces rather than architectural authorities
- the VNext reasoning engine (`ReasoningSessionCoordinator`) progressively replaces legacy host-authoritative orchestration with explicit request → snapshot → program → delivery semantics

---

## 3. Binding Constraints

All actions must preserve:

1. Canonical semantic authority in normalization/CMS layers.
2. Downward-only authority flow (`Truth -> Derivation -> Interpretation -> Presentation`).
3. Legacy as compatibility reference only, not forward architecture.
4. Explicit, deterministic, and reversible migration behavior.
5. No semantic decisions in UI/service/rendering layers.
6. No silent bypass paths or unreachable migration code.
7. No currently shipped capability may be removed as a side effect of refactor work unless explicitly authorized by higher-authority documents.
8. Charting libraries are replaceable infrastructure, not architectural authorities.
9. Backend-specific lifecycle quirks must be isolated inside rendering adapters, never shaping orchestration or semantic flow.
10. No chart backend may be treated as production-safe for a capability family until it has passed explicit qualification.
11. Eventual convergence toward standardized graph parent controllers and shared chart-host affordances is intentional.
12. Transform-like chart programmability is a downstream derived/interpretive capability, not a semantic authority.
13. Support for rendering more than one derived result set on a qualified chart surface is future-facing intent and must not be blocked by single-result assumptions.
14. VNext path must not change the semantic content of the projected context relative to legacy — it is an execution-path alternative, not a semantic one.

---

## 4. Capability Preservation Baseline

Capability families that must not be lost accidentally:

1. Lossless ingestion and persistence of source data.
2. Declarative normalization, canonical identity, and CMS production.
3. Deterministic computation across currently supported strategy families.
4. Visualization coverage across current chart/view families.
5. UI interaction capabilities already present.
6. Reachability, parity, and evidence-generation capability.
7. Existing compatibility/migration observability.
8. Future-extension readiness for interpretive overlays, confidence annotation, and structural exploration.
9. Rendering-backend extensibility without reworking semantic/application logic.
10. Eventual transform-style programmability and multi-result-set rendering on qualified chart surfaces.

Capability retirement rule: a capability may be removed only if explicitly retired by higher-authority documents or replaced with a validated equivalent.

---

## 5. Current Shape Snapshot (April 2026)

Current observed shape:

- `~395` C# files (after Phase 6.5 hierarchy cleanup and file merges)
- `471` automated tests passing
- first live VNext vertical slice active for main-chart loads
- evidence/export boundary decomposed into standalone DTOs, diagnostics builder, and export orchestrator

Current major concentration points:

- `UI/MainChartsView.xaml.cs` (~1,627 lines)
- `Core/Data/Repositories/DataFetcher.cs` (~920 lines)
- `UI/MainHost/MainChartsEvidenceExportService.cs` (~700 lines, reduced from 1,209)
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` (~857 lines)
- `Core/Services/BaseDistributionService.cs` (~612 lines)
- `Core/Rendering/Engines/ChartRenderEngine.cs` (~588 lines)
- `Core/Rendering/Helpers/ChartHelper.cs` (~595 lines)
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` (~503 lines)

Current read:

- the VNext reasoning engine is live for main-chart loads, proving request → snapshot → program → delivery
- the evidence boundary is clean: DTOs, diagnostics, and export orchestration are separate concerns
- runtime-path tracking distinguishes VNext from legacy loads with full signature-chain diagnostics
- the chart/rendering delivery seams are materially cleaner than before the Phase 5 rehaul
- the remaining architectural noise is concentrated in the outliers listed above
- low-level helper duplication is no longer the dominant problem; mixed host/orchestration/evidence/data-access responsibilities are

---

## 6. Completed Work

### 6.1 Phase 5 — Architecture Rehaul (CLOSED)

All 17 steps completed. Historical execution source was `ARCHITECTURE_REHAUL_CONSOLIDATED_EXECUTION_PLAN.md` (now absorbed here).

Summary of what was achieved:
- Default test lane stabilized (no live SQL dependency)
- Cross-cutting seams introduced (configuration abstraction, notification abstraction)
- `DataFetcher` placed behind narrow query contracts (`IMetricSelectionDataQueries`)
- `ChartHelper` split into core-safe logic vs UI-specific logic
- Chart/controller structure sanitized and standardized
- Rendering capability contracts established and widened across all chart families
- CMS/Legacy cut-over truthfulness repaired for all active strategy families
- Light/dark theme support introduced through explicit UI/theme seams
- Orchestration refactored to explicit handoff boundaries
- `MainChartsView` reduced to a composition host (two passes)
- Syncfusion-specific fragility isolated
- `DataFileReader` modernization started (first infrastructure slice)
- Hard architectural guardrails added (dependency direction, vendor leakage, notification, theme, export)
- Evidence/export flow rebuilt behind dedicated services/coordinators
- `SQLHelper` split by capability
- Final convergence pass completed; remaining debt made explicit

Phase 5 findings and their closure status:

| Finding | Status |
|---------|--------|
| 1. Composition bottleneck in `MainChartsView` | Materially reduced; residual host gravity remains intentional debt |
| 2. Core/UI boundary collapse | Enforced with architecture tests |
| 3. CMS migration scaffolding structural | CMS paths real for all active families |
| 4. Evidence pipeline misaligned | Rebuilt with repository-visible artifacts |
| 5. Mutable global toggles | Configuration abstraction introduced |
| 6. Data access/orchestration concentrated | Query contracts introduced; `DataFileReader` modernized |
| 7. Uneven assurance depth | Guardrails and conformance tests added |

Documents absorbed from Phase 5:
`DataVisualiser_Architecture_Consolidation_Report.md`, `UNDERDEVELOPED_COMPONENTS_IMPLEMENTATION_ORDER.md`, `CHART_CONTROLLER_DECOUPLING_PLAN.md`, `CMS_MIGRATION_STATUS_AND_IMPLEMENTATION_PLAN.md`, `CMS_TEST_RESULTS_AUDIT.md`, `REFACTORING_IMPACT_ASSESSMENT.md`, `TransformDataPanelControllerV2.md`, `HierarchyRefId_Implementation_Plan.md`, `CANONICAL_MAPPING_LIMITATION.md`, `CMS_TEST_SUMMARY_TABLE.md`, `PARITY_VALIDATION_EXPLAINED.md`

### 6.2 Phase 6 — Banked Consolidation Work

**Phase 6.1 (Irreducible operations):** Mostly complete.
- Frequency binning consolidated into `FrequencyBinningHelper`
- Transform preparation consolidated into `TransformComputationService`
- Chart-context series preparation separated into `ChartDataSeriesPreparationHelper`
- Smoothing, data selection, temporal alignment already centralized in dedicated services

**Phase 6.5 (Physical hierarchy):** Complete for current cycle.
- 11 single-file directories eliminated
- 21 micro-type files merged into logical owners
- `codebase-index.md` and architecture guardrail tests updated

**Phase 6.2/6.3 (Boundary reconciliation + request/delivery standardization):**
- Evidence boundary decomposed: `MainChartsEvidenceExportService` (1,209 → 700 lines) split into `EvidenceExportModels` (21 DTOs), `EvidenceDiagnosticsBuilder`, and export orchestration
- `VNextMainChartIntegrationCoordinator` built and tested as the VNext → legacy bridge
- First live VNext main-chart slice activated in `MetricLoadCoordinator`
- `LoadRuntimeState` on `ChartState` tracks runtime path, signatures, and failure reason
- `EvidenceRuntimePath` and `VNextDiagnosticsSnapshot` emitted in evidence exports
- Smoke-verified with April 2026 exports: VNext signature chain aligned, legacy fallback correct, all 8 parity strategies pass

**Phase B host spine decomposition (banked slices):**
- Export trigger extraction → `MainChartsViewEvidenceExportCoordinator`
- Data-loaded refresh → `MainChartsViewDataLoadedCoordinator`
- Selection lifecycle stabilization → batched state updates in `MainWindowViewModel` and `SubtypeSelectorManager`
- Request-driven load → immutable `MetricLoadRequest` with `LoadRequestSignature`
- MainHost sprawl tightening → trivial micro-carriers folded into primary owners

---

## 7. Primary Mandate

The primary mandate of the next cycle is:

- make the hierarchy trustworthy enough that it exposes the real remaining entropy
- widen VNext coverage where bounded slices prove the architecture
- do so in a way that a single maintainer can actually sustain

In practice:

1. Similar responsibilities should follow one recognizable structural pattern.
2. Similar low-level operations should have one obvious home.
3. Exceptions should look exceptional rather than normal.
4. Large files should exist only where their size reflects genuine concentration, not accidental drift.
5. VNext should progressively replace legacy orchestration where bounded slices prove equivalent behavior.
6. Work should reduce future operator burden.

---

## 8. Target Module Buckets

These are the target responsibility buckets for evaluating any slice:

1. **Data Access and Intake Facades** — retrieval contracts, repository facades
2. **Canonical / Context Handoff** — context objects bringing canonical data into `DataVisualiser`
3. **Derived and Transform Kernel** — shared algebraic operations, interval/bucket/range logic, smoothing/alignment
4. **Presentation Planning / Chart Programs** — declared result composition, selected-series shaping, multi-result intent
5. **Orchestration and Coordinators** — context building, execution routing (VNext/legacy), render invocation handoff, evidence initiation
6. **Rendering Capability Families** — capability contracts, backend qualification, route/probe/host logic
7. **UI-Agnostic Controllers and Hosts** — standardized controller behavior, host coordination
8. **Client Adapters and Surfaces** — `LiveCharts`, `Syncfusion`, future consumers
9. **Evidence and Diagnostics** — parity exports, reachability/evidence generation, runtime-path diagnostics

---

## 9. Governing Legibility Cycle

All consolidation work should follow this cycle:

1. **Identify Entropy** — find where the hierarchy stops explaining itself
2. **Constrain / Re-home** — move responsibilities toward clearer owners
3. **Consolidate Irreducibles** — give similar operations one obvious home
4. **Standardize Proven Patterns** — standardize only after 2-3 real consumers prove a shape
5. **Retire / Remove / Clean** — remove superseded wrappers and residue
6. **Validate / Re-measure** — full build, tests, targeted smoke where behavior changed

---

## 10. Trackable Phase Plan

### 10.1 Phase A — Re-Baseline (COMPLETED)

Hierarchy baseline refreshed. Current outlier map remains valid.

### 10.2 Phase B — Host Spine Decomposition (MOSTLY COMPLETED)

Multiple slices banked (export, data-loaded, selection stabilization, request-driven load, MainHost tightening). Remaining host gravity in `MainChartsView` is intentional debt for now.

### 10.3 Phase C — Outlier Service Decomposition (ACTIVE)

Evidence boundary decomposition is complete (`MainChartsEvidenceExportService` split).

Remaining targets in priority order:
1. `Core/Data/Repositories/DataFetcher.cs`
2. `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`

### 10.4 Phase D — Delivery and Rendering Spillover Simplification

Reassess `ChartUpdateCoordinator`, `ChartRenderEngine`, `ChartHelper`, and vendor seams after Phase C. Touch only where earlier phases expose a better owner.

### 10.5 Phase E — Architecture Audit and Next-Cycle Gate

Measure concentration reduction. List what became more legible. Name the next cycle from remaining outliers.

The audit must answer these three questions directly:

1. Given the current state of the code, to what extent have the outlined objectives of the plan been met?
2. What gaps, inefficiencies, risks, or missed opportunities remain?
3. How would you reach the remaining objectives in the next major cycle?

Any next-cycle proposal must follow the governing iteration flow (Section 9) or justify the deviation. If something cannot reasonably be completed in one major cycle, that limitation must be stated explicitly.

### 10.6 VNext Activation (ACTIVE — Parallel Track)

First live VNext main-chart slice is proven and stable. Next VNext slices to consider:
- Extend VNext to handle Normalized, Difference, and Ratio programs (the reasoning engine already supports `BuildNormalizedProgram`, `BuildDifferenceProgram`, `BuildRatioProgram`)
- Widen the routing condition to include more chart combinations
- Each widening must be a bounded slice with automatic legacy fallback

---

## 11. Current Priority Outliers

- `UI/MainChartsView.xaml.cs` — largest host concentration point
- `Core/Data/Repositories/DataFetcher.cs` — query shaping, normalization, subtype resolution
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs` — selection, compute, grid, render mixed
- `Core/Services/BaseDistributionService.cs` — computation + rendering mixed
- `Core/Rendering/Engines/ChartRenderEngine.cs` — render normalization, axis shaping, tooltip range
- `Core/Rendering/Helpers/ChartHelper.cs` — scattered tooltip/axis/clearing helpers
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs` — rendering family concentration

---

## 12. Candidate High-Entropy Operation Clusters

- main-host event wiring and chart-update request flow
- presentation-spine publication and host orchestration handoff
- data-fetch query shaping, table normalization, subtype resolution
- transform panel workflow composition across selection, compute, render, grid sync
- render normalization, axis shaping, tooltip range shaping, chart helper spillover
- client-specific repair logic ownership classification

Already banked (do not reopen casually):
- bucket / interval / range determination
- transform preparation and result composition
- chart-context series preparation
- evidence / parity snapshot resolution and export composition

---

## 13. Validation and Smoke-Test Discipline

1. Run full solution build and `DataVisualiser.Tests` for every significant refactor.
2. Internal-only, behavior-preserving changes: no manual smoke required.
3. Live behavior/UI/render/export/theme/controller changes: targeted manual smoke required.
4. Each iteration records: tests run, smoke requirement, scope.

---

## 14. Non-Goals and Guardrails

- do not reduce capability or flexibility
- do not force false uniformity across honest outliers
- do not optimize for file-count alone
- do not reopen stabilized rendering seams casually
- do not let UI become a semantic authority
- do not let backend quirks leak upward into orchestration
- do not mix unrelated subsystems in one iteration without justification
- do not use broad rewrites where a bounded slice can reveal the architecture more honestly
- do not generalize before 2-3 real slices prove a pattern

---

## 15. Success Criteria

This cycle is successful when:

- similar operations have one obvious home
- the folder tree communicates responsibility more clearly than migration history
- true outliers are easier to identify and harder to ignore
- exceptions look exceptional rather than normal
- VNext reasoning engine is proven live for the first chart family with full signature-chain diagnostics
- downstream delivery layers are clearer and less authority-confused
- no regression occurs in rendering, parity, export, theme, orchestration, or flexibility behavior
- remaining intentional debt is explicit enough to define the next cycle cleanly

---

## Appendix A. Programmable Chart Platform Direction

This appendix captures how the intended standardized programmable chart platform fits the immutable foundation documents. It is interpretive, not authoritative. Absorbed from the former `PROGRAMMABLE_CHART_PLATFORM_FOUNDATIONAL_ALIGNMENT_NOTE.md`.

### A.1 Interpreted Fit With Foundational Law

The intended future capability is:

- standardized graph parent controllers and shared chart-host affordances
- transform-style programmability across chart families
- explicit selection of submetrics and derived operations
- rendering of one or more derived result sets on a qualified chart surface

This fits the foundation documents provided the following remain true:

1. Canonical semantics remain upstream and authoritative.
2. Chart programs do not assign or alter meaning.
3. Result-set composition is derived or interpretive by default, not canonical.
4. Multiple result sets preserve explicit provenance and reversibility.
5. Controller standardization does not turn the UI into a semantic authority.
6. Backend qualification remains required before programmable chart behavior is treated as safe on a given surface.

### A.2 Layer Interpretation

The standardized programmable chart direction spans these layers:

- `truth`: unchanged and protected
- `derived`: explicit operations over canonical inputs
- `interpretive`: overlays, confidence-aware handling, optional comparative views
- `UI`: standardized graph parent controllers, shared option/toggle surfaces
- `rendering infrastructure`: qualified surfaces that can display one or more result sets safely

It must not collapse back upward into normalization, canonical identity, or semantic law.

### A.3 Architectural Implications

1. Chart-program definition should become a first-class downstream concept, separate from rendering and separate from semantics.
2. Rendering contracts should avoid permanent single-result assumptions.
3. Parent-controller convergence should be capability-driven, not forced by appearance alone.
4. Special-case transform programmability should eventually become a reusable capability, not remain trapped in one controller lineage.
5. Multi-result rendering must expose provenance clearly enough that the user can tell what each result set represents and how it was produced.
6. The first live VNext vertical slice (April 2026) proves request → snapshot → program → delivery for the Main chart family, which is the foundational execution shape that programmable chart composition will eventually build upon.

### A.4 Open Questions

The following deserve future design work but are not answered here:

1. What is the formal model for a chart program?
2. Which operations belong to derived computation versus interpretive overlays?
3. How should multi-result provenance be shown in the UI?
4. Which chart capability families truly support programmable multi-result composition versus only a subset of it?
5. How should persistence behave for session-scoped versus saved chart programs?

### A.5 Current Recommendation

Stage the actual capability under later gated phases after legibility work has earned it. Do not prematurely implement the whole programmable chart platform through new exception-driven structures.

---

## Appendix B. Conversation Branch Persistence Schemas

These YAML schemas define the structured data model for preserving context across conversation sessions without replaying full history. Absorbed from the former `Conversation_Branch_Data_Model.md`.

### B.1 Foundation Pack

```yaml
foundation_pack:
  authority_order:
    - Project Bible.md
    - SYSTEM_MAP.md
    - Project Roadmap.md
    - Project Overview.md
    - DataVisualiser_Subsystem_Plan.md
  references:
    - documents/Project Bible.md
    - documents/SYSTEM_MAP.md
    - documents/Project Roadmap.md
    - documents/Project Overview.md
    - documents/DataVisualiser_Subsystem_Plan.md
  stable_truths:
    - canonical truth is upstream
    - downstream interpretation is explicit and reversible
    - delivery surfaces are non-authoritative
    - validation evidence outranks narrative
```

### B.2 Branch Context

```yaml
branch_context:
  branch_name: <string>
  branch_type: execution | analysis
  posture: conservative | balanced | aggressive
  objective: <string>
  scope: <string>
  risk_tolerance:
    live_behavior_churn: low | medium | high
    temporary_instability: true | false
    broad_refactors_allowed: true | false
    smoke_deferral_allowed: true | false
  active_constraints:
    - <list of constraints>
```

### B.3 Current State Snapshot

```yaml
current_state_snapshot:
  captured_at_utc: <datetime>
  summary:
    done: [<list>]
    open: [<list>]
    blocked: [<list>]
    next: [<list>]
  evidence:
    build_status: <string>
    test_status: <string>
    manual_smoke_status: <string>
  known_outliers: [<file paths>]
```

### B.4 Iteration Record

```yaml
iteration_record:
  id: <string>
  objective: <string>
  slice:
    files: [<file paths>]
  changes: [<list>]
  validation:
    build: passed | failed
    tests: passed | failed
    manual_smoke_required: true | false
    manual_smoke_result: passed | failed | not_required
  promoted_to_state_snapshot: true | false
```

### B.5 Reload Packet

```yaml
reload_packet:
  foundation_pack_ref: foundation_pack
  branch_context_ref: branch_context
  current_state_snapshot_ref: current_state_snapshot
  active_documents: [<file paths>]
  entry_instruction: <string>
```

### B.6 Analysis Proposal

```yaml
analysis_proposal:
  proposal_name: <string>
  branch_type: analysis
  objective: <string>
  assumptions: [<list>]
  expected_payoff: [<list>]
  risks: [<list>]
  status: speculative | accepted | rejected
```

### B.7 Branch Types

**Execution branch rules:**
- only current verified facts
- only bounded next steps
- only validated changes count as progress
- speculative redesign stays out unless accepted

**Analysis branch rules:**
- may challenge the current structure
- may propose high-risk alternatives
- must stay anchored to foundation docs and current code
- must mark all outputs as speculative, bounded, or next-cycle

### B.8 Compression Rules

```yaml
compression_rules:
  - stable truth belongs in foundation_pack only
  - transient reasoning belongs in branch conversation only
  - only accepted conclusions move into current_state_snapshot
  - only completed work moves into iteration_record
  - only current blockers and next steps belong in reload_packet
```

---

**End of DataVisualiser Subsystem Plan**
