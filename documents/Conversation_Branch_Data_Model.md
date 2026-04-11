# CONVERSATION BRANCH DATA MODEL

**Purpose:** preserve completeness, focus, and currentness across execution and analysis branches without replaying full history

---

## 1. Core Objects

### 1.1 Foundation Pack

Stable authority and project intent.

```yaml
foundation_pack:
  authority_order:
    - Project Bible.md
    - SYSTEM_MAP.md
    - Project Roadmap.md
    - Project Overview.md
    - subsystem_plan
  references:
    - documents/Project Bible.md
    - documents/SYSTEM_MAP.md
    - documents/Project Roadmap.md
    - documents/Project Overview.md
    - documents/DataVisualiser_Consolidation_Plan.md
    - log.md
  stable_truths:
    - canonical truth is upstream
    - downstream interpretation is explicit and reversible
    - delivery surfaces are non-authoritative
    - validation evidence outranks narrative
```

### 1.2 Branch Context

The active identity of a conversation branch.

```yaml
branch_context:
  branch_name: execution_main
  branch_type: execution   # execution | analysis
  posture: conservative    # conservative | balanced | aggressive
  objective: stabilize DataVisualiser and improve legibility
  scope: DataVisualiser
  risk_tolerance:
    live_behavior_churn: low
    temporary_instability: false
    broad_refactors_allowed: false
    smoke_deferral_allowed: false
  active_constraints:
    - single maintainer
    - 1-3 focused sessions per slice
    - one primary objective per iteration
```

### 1.3 Current State Snapshot

Only what is true now.

```yaml
current_state_snapshot:
  captured_at_utc: 2026-04-05T00:00:00Z
  summary:
    done:
      - host selection-state stabilization
      - export diagnostics enhancement
      - current smoke gate mostly closed
    open:
      - large date-range performance tolerance
      - DataFetcher decomposition
    blocked:
      - none
    next:
      - bounded performance slice
  evidence:
    build_status: green
    test_status: green
    manual_smoke_status: partially export-backed
  known_outliers:
    - DataVisualiser/Core/Data/Repositories/DataFetcher.cs
    - DataVisualiser/UI/MainChartsView.xaml.cs
    - DataVisualiser/UI/MainHost/MainChartsEvidenceExportService.cs
    - DataVisualiser/UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs
```

### 1.4 Iteration Record

Smallest durable unit of sanctioned progress.

```yaml
iteration_record:
  id: phase_b_slice_03
  objective: stabilize host selection batching
  slice:
    files:
      - DataVisualiser/UI/MainChartsView.xaml.cs
      - DataVisualiser/UI/ViewModels/MainWindowViewModel.cs
      - DataVisualiser/UI/Charts/Presentation/SubtypeSelectorManager.cs
  changes:
    - batched selection updates
    - suppressed synthetic combo events
  validation:
    build: passed
    tests: passed
    manual_smoke_required: true
    manual_smoke_result: passed
  promoted_to_state_snapshot: true
```

### 1.5 Reload Packet

Minimum data needed to restart a branch cleanly.

```yaml
reload_packet:
  foundation_pack_ref: foundation_pack
  branch_context_ref: branch_context
  current_state_snapshot_ref: current_state_snapshot
  active_documents:
    - documents/DataVisualiser_Consolidation_Plan.md
    - documents/log.md
  entry_instruction: continue from next open item only
```

### 1.6 Analysis Proposal

Speculative work that must not contaminate execution state until accepted.

```yaml
analysis_proposal:
  proposal_name: aggressive_state_spine_rebuild
  branch_type: analysis
  objective: replace ad hoc host state with atomic selection/load/presentation/workflow state
  assumptions:
    - temporary instability acceptable
    - broader refactor allowed
  expected_payoff:
    - removes recurring state drift class
    - simplifies host/controller behavior
  risks:
    - broad live-behavior impact
    - high smoke burden
  status: speculative
```

---

## 2. Branch Types

### 2.1 Execution Branch

```yaml
rules:
  - only current verified facts
  - only bounded next steps
  - only validated changes count as progress
  - speculative redesign stays out unless accepted
```

### 2.2 Analysis Branch

```yaml
rules:
  - may challenge the current structure
  - may propose high-risk alternatives
  - must stay anchored to foundation docs and current code
  - must mark all outputs as speculative, bounded, or next-cycle
```

---

## 3. Operations

### 3.1 Create Branch

```yaml
create_branch:
  inputs:
    - branch_type
    - objective
    - posture
    - risk_tolerance
    - reload_packet
  output:
    - new branch_context
```

### 3.2 Reload Branch

```yaml
reload_branch:
  inputs:
    - reload_packet
  output:
    - active context restored without replaying whole history
```

### 3.3 Merge Analysis Into Execution

```yaml
merge_rule:
  requirements:
    - proposal is explicit
    - alignment with foundation docs is stated
    - bounded first slice is identified
    - validation and smoke implications are known
  result:
    - accepted proposal becomes execution iteration candidate
```

---

## 4. Compression Rules

To preserve focus:

```yaml
compression_rules:
  - stable truth belongs in foundation_pack only
  - transient reasoning belongs in branch conversation only
  - only accepted conclusions move into current_state_snapshot
  - only completed work moves into iteration_record
  - only current blockers and next steps belong in reload_packet
```

---

## 5. Minimal Starter Forms

### Execution Branch

```yaml
branch_context:
  branch_name: execution_main
  branch_type: execution
  posture: conservative
  objective: stabilize current host and performance behavior
```

### Analysis Branch

```yaml
branch_context:
  branch_name: analysis_aggressive
  branch_type: analysis
  posture: aggressive
  objective: propose faster architectural convergence from the current baseline
```

---

## 6. Recommended Working Set

For any new branch, load only:

```yaml
working_set:
  foundation:
    - documents/Project Bible.md
    - documents/SYSTEM_MAP.md
    - documents/Project Roadmap.md
    - documents/Project Overview.md
  subsystem:
    - documents/DataVisualiser_Consolidation_Plan.md
    - log.md
  branch_specific:
    - current_state_snapshot
    - objective
    - risk_tolerance
```

This is the smallest structured context pack that preserves completeness, focus, and currentness.
