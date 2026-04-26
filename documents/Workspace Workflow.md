# Workspace Workflow
**Updated:** April 2026 — Multi-Agent Aligned, Phase 6 Fully Closed, Phase 7 Entry Gate Satisfied, Pre-Phase-7 Baseline Closed, Architectural Vocabulary Added

This document is part of the core documentation set.
See `MASTER_OPERATING_PROTOCOL.md` for governing authority.

---

## Purpose

This document defines the artifacts, procedures, and per-workspace declarations required to initialize, refresh, or reset an AI assistant workspace for the DataAnalyser solution.

It exists to:
- prevent contextual drift
- enforce grounding discipline
- ensure deterministic rehydration
- protect execution velocity during long-running collaborations
- declare per-workspace execution context, observability, and termination semantics
- prevent closed historical phases from being accidentally re-tracked as active workstreams
- preserve a shared architectural vocabulary for ownership-container and boundary reasoning

It applies to any AI assistant used for this project (Claude Code, Codex, or other agents).

---

## 1. Document Authority Hierarchy

When reconstructing a workspace, documents are interpreted strictly in this order:

1. `Project Bible.md` (architectural law)
2. `SYSTEM_MAP.md` (structural boundaries)
3. `DataVisualiser-Architectural-Vocabulary.md` (canonical architectural grammar and target ownership language)
4. `Project Roadmap.md` (sequencing and phase discipline)
5. `MASTER_OPERATING_PROTOCOL.md` (execution governance)
6. `Project Overview.md` (descriptive)
7. `DataVisualiser_Subsystem_Plan.md` (active execution)
8. `codebase-index.md`
9. `dependency-summary.md`
10. `project-tree.txt`
11. Solution ZIP (reference only)

### Document Purpose Reference

| Document | Role | Why it is separate |
|---|---|---|
| `Project Bible.md` | Architectural law | Immutable authority. Must not carry operational or descriptive weight. |
| `SYSTEM_MAP.md` | Structural boundaries | Defines allowed reality. Includes pipeline spine as appendix. |
| `DataVisualiser-Architectural-Vocabulary.md` | Canonical architectural grammar | Defines promoted concepts, ownership containers, do-not-confuse distinctions, target hierarchy, and migration-risk language. |
| `MASTER_OPERATING_PROTOCOL.md` | Execution governance | Binding behavioral rules for how work proceeds. Distinct from architecture or planning. |
| `Project Roadmap.md` | Sequencing authority | Phase gates, closure criteria, and evolutionary gating. |
| `Project Overview.md` | Descriptive snapshot | What the system is today. Audience: someone orienting, not executing. |
| `Project Philosophy.md` | Non-binding values | Intent and trade-off guidance. Deliberately non-authoritative. |
| `DataVisualiser_Subsystem_Plan.md` | Active execution plan | Single plan for DataVisualiser architectural work, VNext activation, and consolidation. |
| `Collaboration Protocol.md` | Human quick-reference | Lightweight behavioral guide so the user doesn't have to read the MOP for simple questions. |
| `Workspace Workflow.md` | Procedural rehydration | How to set up, refresh, and maintain an AI assistant workspace. (This document.) |
| `TODO Enhancements.md` | Ideas backlog | Running list of future enhancement ideas. Not governed by the authority chain. |

---

## 2. Required Rehydration Bundle

### Mandatory

- `MASTER_OPERATING_PROTOCOL.md`
- `Project Bible.md`
- `Project Overview.md`
- `Project Roadmap.md`
- `SYSTEM_MAP.md`
- `DataVisualiser-Architectural-Vocabulary.md`
- `DataVisualiser_Subsystem_Plan.md`
- `project-tree.txt` (if generated)
- `codebase-index.md` (if generated)
- `dependency-summary.md` (if generated)

The architectural vocabulary document is mandatory because it preserves the shared language for ownership-container, boundary, capability, composition, consumer, interaction, overlay, and provenance reasoning.

### Optional but Recommended

- Solution ZIP / package (reference only)

### 2.1 Artifact Classes and Readiness

Workspace materials are not all the same kind of artifact.

| Artifact Class | Examples | Role |
|---|---|---|
| Foundational documents | Bible, System Map, Roadmap, Overview, Subsystem Plan, Vocabulary, MOP | Authority, structure, sequencing, execution governance, or orientation |
| Procedural documents | Workspace Workflow, Collaboration Protocol | Workspace setup and collaboration mechanics |
| Generated structural lookup artifacts | `project-tree.txt`, `codebase-index.md`, `dependency-summary.md` | Generated structure/navigation aids; authoritative for observed structure, not intent |
| Generated evidence artifacts | reachability exports, parity exports, smoke exports, runtime diagnostics | Audit/proof outputs; not foundational context |
| Source artifacts | live repository, solution ZIP, attached source files | Required for implementation work |

Readiness levels:

| Readiness | Required Materials | Allows |
|---|---|---|
| Alignment-ready | Foundational documents | architectural alignment and phase grounding |
| Navigation-ready | Alignment-ready + generated structural lookup artifacts | file/symbol orientation |
| Audit-ready | Navigation-ready + relevant generated evidence artifacts | closure, parity, reachability, or regression review |
| Implementation-ready | Navigation-ready + live repo / solution ZIP / relevant source files | code changes |

---

## 3. Workspace Lifecycle

### 3.1 Create a New Workspace

1. Run structural generators (if available):
   - `Generate-ProjectTree.ps1` -> `project-tree.txt`
   - `Generate-CodebaseIndex.ps1` -> `codebase-index.md`
   - `Generate-DependencySummary.ps1` -> `dependency-summary.md`

2. Verify all mandatory documents are current.

3. Start a new conversation with the AI assistant.

4. (Recommended) Declare a workspace label: `Workspace: <Project> - <Context> - <Qualifier>`

5. Provide ALL files from the rehydration bundle.

6. Enter: `Initialize workspace with the uploaded files.`

7. Do NOT proceed until the assistant confirms initialization and declares workspace state (Section 5).

### 3.2 Refresh an Existing Workspace

1. Re-run generators if structure, code, or dependencies changed.

2. Identify which documents need updating:
   - `MASTER_OPERATING_PROTOCOL.md` -> collaboration rules change
   - `Project Bible.md` -> architecture change
   - `Project Overview.md` -> capabilities change
   - `Project Roadmap.md` -> phase or trajectory change
   - `SYSTEM_MAP.md` -> structural intent change
   - `DataVisualiser-Architectural-Vocabulary.md` -> promoted concepts, ownership containers, target hierarchy, or do-not-confuse distinctions change
   - `DataVisualiser_Subsystem_Plan.md` -> execution plan, Phase 7 forward-stage state, ownership-container alignment, or VNext preservation-baseline state change

3. Provide the refresh bundle.

4. Enter: `Refresh workspace with the updated files.`

5. Wait for explicit confirmation before proceeding.

### 3.3 Phase Transition Guardrail

Creating a new workspace is a controlled phase sub-step, not an escape hatch. Before resetting:
- Documentation alignment must be complete
- Parity obligations must be explicitly closed or deferred
- Reset justification must be stated
- Closed Phase 5, Phase 6, Phase 6.3, and pre-Phase-7 primer work must remain historical baselines unless the task is explicitly historical review

### 3.4 Systemic Failure Re-Entry

If a task triggers systemic failure (per `MASTER_OPERATING_PROTOCOL`):
- Workspace is considered tainted
- Execution halts
- Only rehydration, inspection, or protocol repair may occur
- No forward progress resumes until a clean state is acknowledged

---

## 4. Immutable Workflow Notes

- Generated artifacts are authoritative for structure, not intent.
- Generated files must never be edited by hand.
- These workflows MUST NOT be altered without explicit agreement.
- These workflows override all other procedural descriptions.
- Closed historical phases are not reopened by workspace initialization, refresh, or reset.
- Architectural vocabulary is a conceptual grammar source, not an execution plan or phase authority.

---

## 5. Workspace State Declaration (Mandatory)

After initialization or refresh, the assistant MUST explicitly declare the workspace state before proposing or executing changes.

The declaration MUST include:

- Documents loaded (explicit list)
- Code snapshot status (ZIP present or live codebase access)
- Active project phase / forward-stage posture
- VNext status (closed preservation baseline, active slice scope if any, legacy fallback posture)
- Parity status
- Test count
- Architectural vocabulary status, if boundary/ownership reasoning is involved
- Ownership-container focus, if the task crosses architectural or structural boundaries
- Any missing or unknown artifacts

Example:

```
Workspace State:
- Documents: MASTER_OPERATING_PROTOCOL, Project Bible, SYSTEM_MAP, Architectural Vocabulary, Roadmap, Execution Plan
- Code snapshot: live codebase access
- Phase: Phase 6 closed, Phase 7 entry gate satisfied
- Forward posture: Phase 7 capability expansion; closed Phase 5/6/6.3 and pre-Phase-7 primer remain preservation baselines
- VNext: all chart families have VNext-compatible request/program support and live VNext routes where appropriate; legacy remains compatibility/fallback/projection
- Ownership-container focus: authority/provenance, reasoning/capability, process/execution, contract/boundary, consumer/interaction, terminal delivery, governance/evidence as applicable
- Parity: all 8 strategies passing (April 2026 exports)
- Tests: 737 DataVisualiser + 15 DataFileReader passing
- Unknowns: none
```

No execution may proceed until this declaration is acknowledged.

---

## 6. Execution Enforcement Rules

### 6.1 Grounding Gate

Before proposing any code change, the assistant MUST pass a grounding gate:

- Explicit list of files inspected
- Explicit list of symbols verified
- Explicit list of unknown or missing artifacts

If a required file or symbol is missing, execution MUST stop and the file must be requested explicitly. Conceptual, assumed, or "simplified" substitutions are prohibited.

If a task touches a closed phase or closed primer baseline, the assistant MUST distinguish historical preservation from active work before proposing changes.

If a task crosses ownership containers or uses promoted concepts such as authority, capability, composition, consumer, interaction, boundary, overlay, or provenance, the architectural vocabulary must be treated as active context.

### 6.2 Cut-Over Locus Declaration

Before beginning any migration or refactor task, the following MUST be explicitly declared:

- File name
- Class name
- Method name
- Call site (who invokes it)
- Ownership-container classification if the change crosses a structural boundary
- Whether the change depends on promoted vocabulary concepts or do-not-confuse distinctions

No implementation may proceed without this declaration being acknowledged.

### 6.3 Change Atomicity

All implementation guidance MUST be one of:
- Full method replacement
- Full class replacement
- Explicit before/after diff block

Prohibited:
- Partial snippets without surrounding context
- "Modify this part" instructions
- Multi-location changes without enumeration

### 6.4 Reachability Proof Requirement

For any new or migrated logic, the assistant MUST declare:

- How execution can be observed
- Where a breakpoint/log/test confirms reachability

Examples:
- Unit test assertion
- Parity harness invocation
- Evidence export artifact with `RuntimePath` and signature chain
- Service-layer call confirmed via breakpoint
- Contract-boundary or consumer-delivery path confirmed by test, evidence, or targeted smoke

A task without reachability proof is considered non-executable.

---

## 7. Active / Preservation-Baseline Artifacts (Phase 7 / VNext Infrastructure)

The following files are relevant for Phase 7 capability expansion and the closed VNext/render-plan preservation baseline:

**VNext:**
- `VNext/Application/ReasoningSessionCoordinator.cs`
- `VNext/Application/ReasoningEngineFactory.cs`
- `VNext/Application/LegacyChartProgramProjector.cs`
- `VNext/Application/ChartProgramPlanner.cs`
- `VNext/Contracts/ChartProgram.cs`
- `VNext/Contracts/ChartProgramKind.cs`
- `VNext/Contracts/MetricSelectionRequest.cs`
- `VNext/State/ReasoningSessionTransitions.cs`

**Render-Plan / Terminal Delivery Baseline:**
- `VNext/Contracts/ChartRenderPlan.cs`
- `VNext/Contracts/ChartSeriesPlan.cs`
- `VNext/Contracts/ChartHierarchyNodePlan.cs`
- `VNext/Contracts/RenderDataBuffer.cs`
- `VNext/Contracts/RenderDensityPlan.cs`
- `VNext/Contracts/ChartInteractionPlan.cs`
- `VNext/Rendering/RenderDensityPolicy.cs`
- `VNext/Rendering/TimeBucketRenderAggregationKernel.cs`
- `VNext/Rendering/ChartBackendCapabilities.cs`
- `VNext/Rendering/ChartBackendSelector.cs`
- `VNext/Rendering/IChartRenderPlanAdapter.cs`
- `VNext/Rendering/ChartRenderPlanAdapterDispatcher.cs`

**Integration Bridge:**
- `UI/MainHost/VNextMainChartIntegrationCoordinator.cs` (Main/Normalized/Diff/Ratio)
- `UI/ViewModels/VNextChartRoutePolicy.cs` (Main-family VNext route eligibility)
- `UI/Charts/Presentation/VNextDataResolutionHelper.cs` (shared resolve + fallback for all families)
- `UI/MainHost/VNextSeriesLoadCoordinator.cs` (WeekdayTrend, Transform, BarPie — shared)
- `UI/ViewModels/MetricLoadCoordinator.cs`
- `UI/State/ChartState.cs` (`LoadRuntimeState`, per-family runtime tracking)

**Evidence Boundary** (`UI/MainHost/Evidence/`):
- `UI/MainHost/Evidence/EvidenceExportModels.cs`
- `UI/MainHost/Evidence/EvidenceDiagnosticsBuilder.cs`
- `UI/MainHost/Evidence/EvidenceDataResolutionHelper.cs`
- `UI/MainHost/Evidence/MainChartsEvidenceExportService.cs`

**Strategy Migration:**
- `Core/Strategies/StrategyCutOverService.cs`
- `Core/Strategies/Reachability/StrategyCmsDecisionEvaluator.cs`
- `Core/Strategies/StrategyParityValidationService.cs`

**Admin Workspace:**
- `UI/Admin/AdminMetricsManagerCoordinator.cs`
- `UI/Admin/IAdminMetricsRepository.cs`
- `UI/Admin/DataFetcherAdminMetricsRepository.cs`

**Orchestration:**
- `Core/Orchestration/ChartRenderingOrchestrator.cs`
- `Core/Orchestration/ChartDataContextBuilder.cs`
- `UI/MainHost/Coordination/MainChartsViewChartUpdateCoordinator.cs`
- `UI/MainHost/Coordination/UiBusyScopeLease.cs`

**Tooltip Rendering Helpers:**
- `Core/Rendering/Helpers/ChartTooltipFormattingHelper.cs`
- `Core/Rendering/Helpers/ChartTooltipPairFormatter.cs`
- `Core/Rendering/Helpers/ChartTooltipStackedFormatter.cs`
- `Core/Rendering/Helpers/ChartTooltipCumulativeFormatter.cs`

**Current Priority Outliers / Debt Concentrations:**
- `UI/MainChartsView.xaml.cs`
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs`
- managed legacy / VNext coexistence
- `Core/Data/Repositories/DataFetcher.cs`
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`

---

## Appendix A. Per-Workspace Context Declaration Template

Copy and fill in this template when starting a workspace that requires explicit execution context tracking. This declaration is disposable — discard it when the workspace is formally closed or terminated.

```
WORKSPACE CONTEXT DECLARATION

1. Workspace Identity

   Workspace Name:
   Date / Time Initialized:
   Primary Objective:
   Active Phase / Forward Stage: (e.g. Phase 7 capability expansion, Forward Stage 7A — Authority and Intent Clarification, Phase 8 consumer/UI consolidation)
   Architectural Vocabulary Loaded: Yes / No / Not required
   AI Assistant(s): (e.g. Claude Code, Codex, or both in parallel)

2. Execution Context

   2.1 Execution Locus
       Executable Entry Point(s):
         File:
         Class:
         Method / Runner / Host:
       Non-Executable Components: (explicit list or "None")

   2.2 VNext / Preservation Baseline State
       VNext Active Slice: (e.g. Phase 7 capability slice / None)
       Closed Baseline: (e.g. all active chart families have VNext-compatible request/program support and render-plan delivery)
       Legacy Fallback: (e.g. automatic compatibility/fallback/projection where still needed)
       Runtime Path Tracking: (e.g. LoadRuntimeState on ChartState)

   2.3 Ownership Container Focus
       Primary Container:
         - authority/provenance
         - reasoning/capability
         - process/execution
         - contract/boundary
         - projection/translation
         - consumer/interaction
         - terminal delivery
         - governance/evidence
       Secondary Container(s):
       Promoted Concept(s) in Play:
       Do-Not-Confuse Distinction(s) Relevant:

3. Observability Strategy

   Primary Observability Mode:
     - Inspection-only
     - Temporary instrumentation
     - Parity harness
     - Evidence export with RuntimePath and signature chain
     - Provider/consumer boundary seam proof
     - Mixed (declare per step)

   Constraints / Notes:

4. Temporary Instrumentation Policy

   Instrumentation Allowed: Yes / No
   Permitted Scope:
   Mandatory Removal Phase / Step:

5. Legacy Parity Anchor

   Legacy Strategy / Path:
   Dataset / Scenario:
   Snapshot / Hash / Output Reference:
   VNext Signature Chain Reference:
     (e.g. RequestSignature, SnapshotSignature, ProgramSourceSignature, ProjectedContextSignature)

6. Test & Verification Posture

   Testing Enabled: Yes / Deferred / Parity-only
   Test Project(s) in Scope:
   Current Test Count: (e.g. 737 DataVisualiser + 15 DataFileReader passing)
   Manual Smoke Required: Yes / No / Conditional on live behavior change

7. Termination Semantics

   Permitted Termination Modes:
     - Clean closure
     - Pause (resumable)
     - Terminate without closure (unsynced)
   Default on Failure:

8. Signal & Interaction Contract

   Preferred Interaction Mode:
     - Procedural
     - Minimal / execute-mode
     - Exploratory
     - Audit-safe alignment
   Verbosity Attenuation Expected: Yes / No
   Multi-Agent Coordination: (e.g. Claude Code + Codex in parallel with cross-review)

9. Acknowledgement

   This declaration instantiates contextual commitments for this workspace only.
   It does not modify or override foundational documents.

   Declared by:
   Acknowledged by:

10. Status

    Workspace State: ACTIVE / PAUSED / UNSYNCED / TERMINATED
    Last Updated:
```

---

**End of Workspace Workflow**
