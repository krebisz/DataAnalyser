# DATAVISUALISER CONSOLIDATION PLAN
**Status:** Planning / Execution Guide  
**Scope:** `DataVisualiser` post-rehaul streamlining, standardization, technical-debt retirement, and structural consolidation  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `Project Roadmap.md`, and `Project Overview.md`

---

## 1. Purpose

This document consolidates the current post-rehaul `DataVisualiser` refactor discussion into one execution-oriented plan.

It exists to:

- identify the main streamlining and unification opportunities
- identify current technical-debt retirement opportunities
- identify structural and naming standardization opportunities
- provide one coherent working plan for future implementation
- provide one repeatable iteration cycle for executing that plan safely

This document does not replace roadmap authority.
It operationalizes the next consolidation program for `DataVisualiser` only.

---

## 1.5 START HERE - Handoff and Execution Defaults

Use this section as the default handoff entry point in a new conversation.

Current defaults:

- scope is `DataVisualiser` only
- posture is `Moderate`
- one iteration must have one primary objective
- validation must happen on every significant refactor
- if live behavior changes, halt after automated validation and request targeted manual smoke before continuing

Authority order:

1. `Project Bible.md`
2. `SYSTEM_MAP.md`
3. `Project Roadmap.md`
4. `Project Overview.md`
5. this document

Default execution starting point:

1. start at `Phase A - Subsystem Inventory and Target Module Map`
2. once Phase A produces a stable slice map, begin with the first `Analytical Kernel Consolidation` slice
3. do not begin with broad folder churn or mass renames

Default validation commands:

1. `dotnet build DataAnalyser.sln -c Debug`
2. `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`

Default smoke rule:

1. no manual smoke for internal-only structural cleanup
2. targeted manual smoke only when live UI/render/export/theme/controller behavior is changed

---

## 2. Current Shape Snapshot

Current observed shape:

- `423` C# files
- `18` XAML files

Current major concentration points:

- `UI/MainChartsView.xaml.cs`
- `Core/Data/Repositories/DataFetcher.cs`
- `UI/MainHost/MainChartsEvidenceExportService.cs`
- `UI/Charts/Adapters/TransformDataPanelControllerAdapter.cs`
- `Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`
- `Shared/Helpers/MathHelper.cs`
- `Core/Services/BaseDistributionService.cs`
- `Core/Rendering/Engines/ChartRenderEngine.cs`
- `Core/Rendering/Helpers/ChartHelper.cs`
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs`

Current major concentration zones:

- `Core/Rendering`
- `Core/Strategies`
- `Core/Orchestration`
- `UI/Charts`
- `UI/MainHost`

Current read:

- the architecture is materially safer than before the rehaul
- the project is still sprawling
- many repeated shapes now exist explicitly, but remain fragmented across many files
- mathematical and algorithmic logic remains too dispersed
- folder and namespace layout still reflects migration history more than the present capability model

---

## 3. Consolidation Opportunity Map

### 3.1 Architecture Streamlining and Unification

Main opportunities:

- reduce repeated chart-capability scaffolding across rendering families
- reduce fragmentation across controller, adapter, contract, route, qualification, and probe shapes
- reduce mixed host/orchestration responsibilities that still remain concentrated in a few files
- converge mathematical and analytical logic into clearer subsystem ownership
- reduce the number of distinct structural patterns used to solve similar chart-family problems

### 3.2 Technical Debt Retirement

Main opportunities:

- remove superseded glue and compatibility remnants exposed by the rehaul
- reduce helper dumping-ground behavior in large multi-purpose classes
- remove dead or passive code made unnecessary by subsystem consolidation
- merge micro-types that do not earn their own long-term file or boundary
- remove folder and naming residue left over from migration-era staging

### 3.3 Standardization and Structural Alignment

Main opportunities:

- align folders and namespaces to the current capability model
- standardize subsystem ownership by responsibility rather than by historical implementation path
- standardize controller/host patterns where behavior has already converged
- standardize rendering-family support structure where multiple real families already match
- reduce cognitive load without reducing capability or flexibility

---

## 4. General Outline

Recommended posture:

- `Moderate`

Working principles:

- preserve behavior first
- simplify by subsystem, not by mass rename
- unify only proven repeated patterns
- defer cosmetic folder churn until ownership is clear
- keep honest outliers explicit
- do not optimize for file-count alone

Recommended workstreams:

1. `Subsystem Inventory and Target Module Map`
2. `Analytical Kernel Consolidation`
3. `Chart Capability Unification`
4. `Controller and Host Standardization`
5. `Debt Retirement and Folder Realignment`
6. `Final Simplification and Metrics Pass`

Recommended target module buckets:

- `Analytical Kernel`
- `Chart Capability Rendering`
- `Chart Hosts and Controllers`
- `Evidence and Diagnostics`
- `Vendor-Specific Adapters`

---

## 5. Governing Consolidation Cycle

All later consolidation work should follow this cycle:

1. `Constrain / Simplify / Re-organize`
2. `Consolidate / Coalesce`
3. `Generalize / Abstract`
4. `Retire / Remove / Clean`
5. `Validate / Re-measure`

Operational rule:

- simplify first
- merge second
- abstract third
- delete continuously and then sweep
- validate after every pass

Meaning of each stage:

### 5.1 Constrain / Simplify / Re-organize

- tighten ownership boundaries
- reduce mixed responsibilities
- move code toward clearer subsystem owners
- standardize names and placement only where target ownership is already clear

### 5.2 Consolidate / Coalesce

- merge overlapping classes, helpers, and pipelines
- reduce duplicate shapes across chart families
- centralize semantically aligned analytical logic

### 5.3 Generalize / Abstract

- only after consolidation exposes a real repeated pattern
- require at least `2-3` real slices or consumers before promoting a shared abstraction
- generalize by logical responsibility, not superficial similarity

### 5.4 Retire / Remove / Clean

- remove superseded files, folders, classes, and compatibility glue
- perform standardized renames where ownership is stable
- remove dead code both opportunistically and in a final sweep

### 5.5 Validate / Re-measure

- full solution build
- `DataVisualiser.Tests`
- focused tests for the touched subsystem
- targeted manual smoke only where live behavior changed
- reassess remaining hotspots before the next iteration

---

## 6. WORKING SECTION - Implementation Iteration Protocol

This section is the operational working guide for implementation.
Use it for each actual consolidation pass.

### 6.1 Per-Iteration Procedure

1. `Select a bounded subsystem slice`
   - Choose one primary slice per iteration.
   - Preferred slices:
     - analytical kernel
     - chart capability rendering
     - chart hosts/controllers
     - evidence/diagnostics
     - vendor-specific adapters
   - Do not mix unrelated slices unless coupling forces it.

2. `Inventory the current shape`
   - Record hotspot files.
   - Record repeated class/module shapes.
   - Record overlapping responsibilities.
   - Define the target ownership model before moving code.

3. `Constrain / Simplify / Re-organize`
   - Tighten boundaries first.
   - Move responsibilities toward clearer owners.
   - Standardize naming and placement only where ownership is clear.
   - Prefer structural contraction before new abstractions.

4. `Consolidate / Coalesce`
   - Merge overlapping helpers, duplicated orchestration shapes, and fragmented micro-types.
   - Reduce the number of competing implementations in the slice.
   - Centralize semantically aligned analytical logic.

5. `Generalize / Abstract`
   - Introduce shared abstractions only after the consolidation pass proves a repeated pattern.
   - Require at least `2-3` real consumers before promoting a shared abstraction.
   - Generalize by responsibility, not by cosmetic similarity.

6. `Retire / Remove / Clean`
   - Delete superseded files, folders, classes, and compatibility glue.
   - Apply standardized renames where the ownership model is stable.
   - Remove dead code continuously during the pass, then perform a closing cleanup sweep.

7. `Validate / Re-measure`
   - Run:
     - full solution build
     - `DataVisualiser.Tests`
     - focused test lanes for the touched subsystem
   - Reassess:
     - hotspot size
     - repeated scaffolding count
     - ownership clarity
     - remaining intentional outliers

8. `Record the new baseline`
   - Update this document after each completed iteration.
   - Record:
     - what was simplified
     - what was merged
     - what was generalized
     - what was removed
     - what remains intentionally deferred

### 6.2 Execution Rules

1. One iteration must have one primary objective.
2. Do not abstract before the consolidation pass proves a real repeated pattern.
3. Do not remove old code until the replacement path passes validation.
4. If hidden coupling expands the scope materially, stop and re-scope instead of broadening the iteration implicitly.
5. Honest outliers may remain outliers if explicitly documented.
6. Each iteration must end with a smaller, clearer subsystem than it began with.

### 6.3 Typical Iteration Outputs

1. updated hotspot list for the slice
2. target ownership map for the slice
3. rename/move plan if needed
4. proved abstraction candidates
5. removal candidates
6. validation results
7. remaining intentional debt

---

## 7. Trackable Implementation Steps

These are the concrete step groups to follow and track.

### 7.1 Phase A - Subsystem Inventory and Target Module Map

Goal:

- define the stable ownership model before broad consolidation begins

Tracked steps:

1. inventory the current major concentration points
2. map current files into the target module buckets
3. identify the top consolidation candidates by subsystem
4. identify justified outliers that should remain explicit
5. define the first subsystem slice to attack

### 7.2 Phase B - Analytical Kernel Consolidation

Goal:

- coalesce mathematical, alignment, transform, binning, and analytical helper logic

Tracked steps:

1. identify overlapping analytical helpers
2. separate pure analytical logic from rendering-specific helper logic
3. move analytical logic toward a coherent kernel
4. merge duplicate or near-duplicate analytical helpers
5. remove superseded helper residue

### 7.3 Phase C - Chart Capability Unification

Goal:

- reduce repeated scaffolding across chart capability families

Tracked steps:

1. compare `CartesianMetrics`, `Distribution`, `WeekdayTrend`, `BarPie`, and `Transform`
2. identify genuinely repeated contract/route/qualification/probe skeletons
3. simplify each family first
4. promote proven shared support only where multiple families already match
5. remove superseded per-family duplication

### 7.4 Phase D - Controller and Host Standardization

Goal:

- reduce fragmentation in controller, adapter, and host ownership models

Tracked steps:

1. inventory controller/adapter/host shapes now in use
2. identify which ones are already behaviorally converged
3. standardize ownership and responsibilities for converged shapes
4. keep `Syncfusion` and other justified outliers explicit
5. remove passive host/controller glue exposed by the new shape

### 7.5 Phase E - Debt Retirement and Folder Realignment

Goal:

- remove exposed debt and align the physical structure with the stabilized ownership model

Tracked steps:

1. identify superseded files, folders, and classes
2. identify micro-types that can be merged safely
3. perform bounded folder and namespace realignment
4. apply standardized renames
5. remove dead code and compatibility residue

### 7.6 Phase F - Final Simplification and Metrics Pass

Goal:

- measure the improvement and record the remaining intentional outliers

Tracked steps:

1. measure hotspot reduction
2. measure repeated scaffolding reduction
3. identify remaining large mixed-responsibility files
4. record intentional outliers explicitly
5. close the consolidation program with the new baseline

### 7.7 Immediate Recommended Start Sequence

Unless new evidence materially changes the codebase shape, use this sequence first:

1. `Iteration 1 - Phase A`
   - refresh the hotspot inventory from the current repo
   - map the top concentration files into the target module buckets
   - explicitly choose the first bounded subsystem slice

2. `Iteration 2 - Phase B, first bounded slice`
   - begin with pure or mostly pure analytical/helper logic before rendering-heavy logic
   - preferred first targets:
     - `Shared/Helpers/MathHelper.cs`
     - `Shared/Helpers/FrequencyBinningHelper.cs`
     - `Shared/Helpers/StrategyComputationHelper.cs`
     - adjacent analytical helpers that can move toward a coherent kernel without changing live UI behavior

3. `Iteration 3 - Phase C, first rendering-family simplification slice`
   - compare and simplify the repeated route/qualification/probe/contract scaffolding in the most closely aligned chart families first
   - preferred first comparison:
     - `Distribution`
     - `WeekdayTrend`

4. `Iteration 4 - Phase D, first host/controller simplification slice`
   - standardize one already-converged controller/adapter/host shape
   - do not start with `Syncfusion`

5. defer aggressive folder/namespace realignment until the earlier iterations produce a stable ownership model

### 7.8 Iteration Baseline Log

#### Iteration 1 - Phase A - Subsystem Inventory and Target Module Map

**Date:** `2026-03-27`  
**Primary objective:** refresh the current `DataVisualiser` inventory, map the current concentration points into the target module buckets, and choose the first bounded consolidation slice

**Refreshed inventory**

- `423` C# files
- `18` XAML files
- highest file-count concentration zones:
  - `UI/Charts` (`99` files)
  - `Core/Rendering` (`94` files)
  - `Core/Strategies` (`38` files)
  - `Core/Orchestration` (`34` files)
  - `UI/MainHost` (`22` files)
  - `Shared/Helpers` (`4` files)
- refreshed hotspot files by line count:
  - `UI/MainChartsView.xaml.cs` (`1207`)
  - `Core/Data/Repositories/DataFetcher.cs` (`768`)
  - `UI/Charts/Adapters/TransformDataPanelControllerAdapter.cs` (`688`)
  - `UI/MainHost/MainChartsEvidenceExportService.cs` (`661`)
  - `UI/Syncfusion/SyncfusionChartsView.xaml.cs` (`642`)
  - `Shared/Helpers/MathHelper.cs` (`631`)
  - `Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs` (`605`)
  - `UI/Charts/Controllers/SyncfusionSunburstChartController.xaml.cs` (`542`)
  - `Core/Services/BaseDistributionService.cs` (`490`)
  - `Core/Rendering/Engines/ChartRenderEngine.cs` (`481`)
  - `Core/Rendering/Helpers/ChartHelper.cs` (`466`)

**Target module bucket map**

- `Analytical Kernel`
  - `Shared/Helpers/MathHelper.cs`
  - `Shared/Helpers/FrequencyBinningHelper.cs`
  - `Shared/Helpers/StrategyComputationHelper.cs`
  - `Shared/Helpers/CmsConversionHelper.cs`
  - analytical portions of `Core/Services/BaseDistributionService.cs`
- `Chart Capability Rendering`
  - `Core/Rendering/*`
  - rendering-facing portions of `Core/Services/BaseDistributionService.cs`
  - chart-family rendering engines such as `Core/Rendering/Engines/ChartRenderEngine.cs`
- `Chart Hosts and Controllers`
  - `UI/MainChartsView.xaml.cs`
  - `UI/Charts/Adapters/*`
  - `UI/Charts/Controllers/*`
  - `Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`
  - `Core/Orchestration/ChartRenderingOrchestrator.cs`
- `Evidence and Diagnostics`
  - `UI/MainHost/MainChartsEvidenceExportService.cs`
  - evidence/export and qualification-support seams that are not part of live rendering ownership
- `Vendor-Specific Adapters`
  - `UI/Syncfusion/*`
  - backend-specific chart renderer implementations such as `UI/Charts/Rendering/LiveCharts/*`

**Top consolidation candidates by subsystem**

- `Analytical Kernel`
  - `Shared/Helpers/MathHelper.cs` is the primary hotspot and the strongest first consolidation candidate
  - `FrequencyBinningHelper.cs` and `StrategyComputationHelper.cs` are adjacent helper candidates with mostly analytical ownership
- `Chart Capability Rendering`
  - `Core/Rendering/Engines/ChartRenderEngine.cs`
  - `Core/Rendering/Helpers/ChartHelper.cs`
  - `Core/Rendering/Engines/WeekdayTrendRenderingService.cs`
- `Chart Hosts and Controllers`
  - `UI/MainChartsView.xaml.cs`
  - `UI/Charts/Adapters/TransformDataPanelControllerAdapter.cs`
  - `UI/Charts/Adapters/BarPieChartControllerAdapter.cs`
  - `UI/Charts/Adapters/DistributionChartControllerAdapter.cs`
- `Evidence and Diagnostics`
  - `UI/MainHost/MainChartsEvidenceExportService.cs`
- `Vendor-Specific Adapters`
  - `UI/Syncfusion/SyncfusionChartsView.xaml.cs`
  - `UI/Charts/Controllers/SyncfusionSunburstChartController.xaml.cs`
  - `UI/Charts/Rendering/LiveCharts/LiveChartsChartRenderer.cs`

**Justified outliers / explicit deferrals**

- `UI/Syncfusion/*` remains an explicit outlier and should not be the first controller/rendering standardization target
- `Core/Data/Repositories/DataFetcher.cs` is a major concentration point but does not belong to the first `DataVisualiser` consolidation slice; treat it as a separate data-access concentration problem rather than force-fitting it into the initial target buckets
- `UI/MainHost/MainChartsEvidenceExportService.cs` remains explicit evidence/export ownership and should not be folded into generic rendering consolidation

**Chosen first bounded subsystem slice**

- `Iteration 2` should begin with `Phase B - Analytical Kernel Consolidation`
- bounded first slice:
  - `Shared/Helpers/MathHelper.cs`
  - `Shared/Helpers/FrequencyBinningHelper.cs`
  - `Shared/Helpers/StrategyComputationHelper.cs`
  - inspect `Shared/Helpers/CmsConversionHelper.cs` for adjacency, but do not widen the slice unless consolidation proves necessary
- rationale:
  - these files are small in number, already colocated, and mostly analytical rather than UI-bound
  - this slice improves ownership clarity without broad folder churn
  - expected behavior impact is low because the work can begin with internal analytical/helper consolidation rather than live rendering or controller changes

**Iteration 1 validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug`
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`
- executed result:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and existing warnings only
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `363` tests passed, `0` failed, `0` skipped
- focused subsystem lane: not applicable for this iteration because the change is documentation-only
- manual smoke requirement: not required unless later iterations change live UI, rendering, export, theme, or controller behavior

---

## 8. Validation and Smoke-Test Discipline

Validation rules for every significant refactor:

1. Run:
   - full solution build
   - `DataVisualiser.Tests`
   - focused test lanes relevant to the touched subsystem

2. If a change is internal-only and behavior-preserving:
   - manual smoke is not required by default

3. If a change affects live behavior, UI composition, rendering, controller flow, themes, exports, or data-loading behavior:
   - halt after automated validation
   - request manual smoke tests before continuing further mutation

4. Manual smoke must be:
   - targeted
   - minimal
   - specific to the changed subsystem
   - widened only if regressions are discovered

5. Heavy smoke or data-import smoke is required only when the touched subsystem actually affects those flows.

6. Each iteration should record:
   - tests run
   - whether manual smoke was required
   - what smoke scope was requested

---

## 9. Non-Goals and Guardrails

- do not reduce capability or flexibility
- do not force false uniformity across honest outliers
- do not optimize for file-count alone
- do not reopen completed rehaul seams casually
- do not abstract accidental complexity
- do not fold interpretive logic into canonical logic
- do not mix unrelated subsystems in one iteration without explicit justification

---

## 10. Success Criteria

- fewer large mixed-responsibility files
- fewer repeated scaffolding types across chart families
- clearer subsystem ownership boundaries
- lower cognitive load in `Core/Rendering`, `UI/Charts`, and helper-heavy areas
- no regression in rendering, parity, export, theme, orchestration, or flexibility behavior
- intentional outliers remain explicit rather than accidental

---

**End of DataVisualiser Consolidation Plan**
