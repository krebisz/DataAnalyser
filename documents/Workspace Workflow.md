# Workspace Workflow
**Updated:** April 2026 — Multi-Agent Aligned, Phase 6 Closed (except 6.3), Phase 7 Entry Gate Satisfied

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

It applies to any AI assistant used for this project (Claude Code, Codex, or other agents).

---

## 1. Document Authority Hierarchy

When reconstructing a workspace, documents are interpreted strictly in this order:

1. `Project Bible.md` (architectural law)
2. `SYSTEM_MAP.md` (structural boundaries)
3. `Project Roadmap.md` (sequencing and phase discipline)
4. `MASTER_OPERATING_PROTOCOL.md` (execution governance)
5. `Project Overview.md` (descriptive)
6. `DataVisualiser_Subsystem_Plan.md` (active execution)
7. `codebase-index.md`
8. `dependency-summary.md`
9. `project-tree.txt`
10. Solution ZIP (reference only)

### Document Purpose Reference

| Document | Role | Why it is separate |
|---|---|---|
| `Project Bible.md` | Architectural law | Immutable authority. Must not carry operational or descriptive weight. |
| `SYSTEM_MAP.md` | Structural boundaries | Defines allowed reality. Includes pipeline spine as appendix. |
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
- `DataVisualiser_Subsystem_Plan.md`
- `project-tree.txt` (if generated)
- `codebase-index.md` (if generated)
- `dependency-summary.md` (if generated)

### Optional but Recommended

- Solution ZIP / package (reference only)

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
   - `DataVisualiser_Subsystem_Plan.md` -> execution plan or VNext state change

3. Provide the refresh bundle.

4. Enter: `Refresh workspace with the updated files.`

5. Wait for explicit confirmation before proceeding.

### 3.3 Phase Transition Guardrail

Creating a new workspace is a controlled phase sub-step, not an escape hatch. Before resetting:
- Documentation alignment must be complete
- Parity obligations must be explicitly closed or deferred
- Reset justification must be stated

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

---

## 5. Workspace State Declaration (Mandatory)

After initialization or refresh, the assistant MUST explicitly declare the workspace state before proposing or executing changes.

The declaration MUST include:

- Documents loaded (explicit list)
- Code snapshot status (ZIP present or live codebase access)
- Active project phase
- VNext activation status (active slice scope, legacy fallback posture)
- Parity status
- Test count
- Any missing or unknown artifacts

Example:

```
Workspace State:
- Documents: MASTER_OPERATING_PROTOCOL, Project Bible, SYSTEM_MAP, Roadmap, Execution Plan
- Code snapshot: live codebase access
- Phase: Phase 6 — Architectural Legibility and Concern Reconciliation
- VNext: main-chart slice active, legacy fallback for extended charts
- Parity: all 8 strategies passing (April 2026 exports)
- Tests: 471 passing
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

### 6.2 Cut-Over Locus Declaration

Before beginning any migration or refactor task, the following MUST be explicitly declared:

- File name
- Class name
- Method name
- Call site (who invokes it)

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

A task without reachability proof is considered non-executable.

---

## 7. Active Phase Artifacts (Phase 6)

The following files are actively relevant during Phase 6 (Architectural Legibility and VNext Activation):

**VNext:**
- `VNext/Application/ReasoningSessionCoordinator.cs`
- `VNext/Application/ReasoningEngineFactory.cs`
- `VNext/Application/LegacyChartProgramProjector.cs`
- `VNext/Application/ChartProgramPlanner.cs`
- `VNext/Contracts/ChartProgram.cs`
- `VNext/Contracts/MetricSelectionRequest.cs`
- `VNext/State/ReasoningSessionTransitions.cs`

**Integration Bridge:**
- `UI/MainHost/VNextMainChartIntegrationCoordinator.cs`
- `UI/ViewModels/MetricLoadCoordinator.cs`
- `UI/State/ChartState.cs` (`LoadRuntimeState`)

**Evidence Boundary** (`UI/MainHost/Evidence/`):
- `UI/MainHost/Evidence/EvidenceExportModels.cs`
- `UI/MainHost/Evidence/EvidenceDiagnosticsBuilder.cs`
- `UI/MainHost/Evidence/EvidenceDataResolutionHelper.cs`
- `UI/MainHost/Evidence/MainChartsEvidenceExportService.cs`

**Orchestration:**
- `Core/Orchestration/ChartRenderingOrchestrator.cs`
- `Core/Orchestration/ChartDataContextBuilder.cs`
- `UI/MainHost/Coordination/MainChartsViewChartUpdateCoordinator.cs`

**Current Priority Outliers:**
- `UI/MainChartsView.xaml.cs`
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
   Active Phase: (e.g. Phase 6.3 — VNext Widening, Phase C — DataFetcher Decomposition)
   AI Assistant(s): (e.g. Claude Code, Codex, or both in parallel)

2. Execution Context

   2.1 Execution Locus
       Executable Entry Point(s):
         File:
         Class:
         Method / Runner / Host:
       Non-Executable Components: (explicit list or "None")

   2.2 VNext Routing State
       VNext Active Slice: (e.g. Main chart only / Main + Normalized / None)
       Legacy Fallback: (e.g. Automatic for extended charts / Full legacy)
       Runtime Path Tracking: (e.g. LoadRuntimeState on ChartState)

3. Observability Strategy

   Primary Observability Mode:
     - Inspection-only
     - Temporary instrumentation
     - Parity harness
     - Evidence export with RuntimePath and signature chain
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
   Current Test Count: (e.g. 471 passing)
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
