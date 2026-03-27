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
- safely coupled slices are allowed when they already share one contract/host/route pattern and batching them reduces duplicated churn
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
4. once a proving pair establishes a shared seam, broaden subsequent iterations to the next safe converged cluster instead of repeating one-family-at-a-time by default

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

1. `Establish / Refresh Regression Protection`
   - Add or extend focused regression tests around the bounded slice before production mutation.
   - Prefer direct seam tests for the exact consolidation target rather than broad incidental coverage.
   - If adequate focused coverage already exists, rerun that lane first and record it as the pre-mutation regression gate.

2. `Select a bounded subsystem slice`
   - Choose one primary slice per iteration.
   - Preferred slices:
     - analytical kernel
     - chart capability rendering
     - chart hosts/controllers
     - evidence/diagnostics
     - vendor-specific adapters
   - Do not mix unrelated slices unless coupling forces it.
   - Do couple adjacent families or controllers when they already share a real contract/host/route shape and one bounded iteration can simplify that whole converged cluster safely.

3. `Inventory the current shape`
   - Record hotspot files.
   - Record repeated class/module shapes.
   - Record overlapping responsibilities.
   - Define the target ownership model before moving code.

4. `Constrain / Simplify / Re-organize`
   - Tighten boundaries first.
   - Move responsibilities toward clearer owners.
   - Standardize naming and placement only where ownership is clear.
   - Prefer structural contraction before new abstractions.

5. `Consolidate / Coalesce`
   - Merge overlapping helpers, duplicated orchestration shapes, and fragmented micro-types.
   - Reduce the number of competing implementations in the slice.
   - Centralize semantically aligned analytical logic.

6. `Generalize / Abstract`
   - Introduce shared abstractions only after the consolidation pass proves a repeated pattern.
   - Require at least `2-3` real consumers before promoting a shared abstraction.
   - Generalize by responsibility, not by cosmetic similarity.

7. `Retire / Remove / Clean`
   - Delete superseded files, folders, classes, and compatibility glue.
   - Apply standardized renames where the ownership model is stable.
   - Remove dead code continuously during the pass, then perform a closing cleanup sweep.

8. `Validate / Re-measure`
   - Run:
     - full solution build
     - `DataVisualiser.Tests`
     - focused test lanes for the touched subsystem
   - Reassess:
     - hotspot size
     - repeated scaffolding count
     - ownership clarity
     - remaining intentional outliers

9. `Record the new baseline`
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
3. batch the next safe converged cluster when multiple controllers already share the same contract/host/route seam
4. standardize ownership and responsibilities for converged shapes
5. keep `Syncfusion` and other justified outliers explicit
6. remove passive host/controller glue exposed by the new shape

### 7.5 Phase E - Debt Retirement and Folder Realignment

Goal:

- remove exposed debt and align the physical structure with the stabilized ownership model

Tracked steps:

1. identify superseded files, folders, and classes
2. identify micro-types that can be merged safely
3. perform bounded folder and namespace realignment
4. apply standardized renames
5. remove dead code and compatibility residue

Recommended execution shape:

- do not perform Phase E as one broad mass-churn pass unless new evidence shows the physical structure is already unusually stable
- prefer `2-3` bounded iterations:
  - first bounded cleanup pass:
    - remove obvious superseded files, classes, and passive glue
    - merge trivial micro-types that no longer earn a standalone file
    - keep namespace churn minimal
  - second bounded cleanup pass:
    - perform the main folder and namespace realignment around the now-stable ownership model
    - apply standardized renames where the destination structure is clear
  - optional third bounded cleanup pass:
    - remove residual compatibility residue, dead code, and naming leftovers exposed by the first two passes
- the default recommendation is `2` iterations when the cleanup proves orderly, and `3` iterations when residual rename fallout or compatibility residue remains

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

5. `Next broadened Phase D cluster after the proving pair`
   - after a shared seam is proven on `Distribution` / `WeekdayTrend`, broaden the next controller/host pass to the cartesian-metric controller family where the contract and host shape already converge
   - preferred next cluster:
     - `Main`
     - `DiffRatio`
     - `Normalized`
   - preferred first coupled seams in that cluster:
     - constant-route render-host lifecycle delegation
     - subtype-selection and selected-series resolution where `DiffRatio` and `Normalized` are materially aligned

6. defer aggressive folder/namespace realignment until the earlier iterations produce a stable ownership model

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

#### Iteration 2 - Phase B - Analytical Kernel Consolidation

**Date:** `2026-03-27`  
**Primary objective:** constrain the first analytical helper slice by separating interval logic, smoothing logic, and valued-series preparation logic into clearer analytical owners without changing the public helper surface

**Bounded slice executed**

- `Shared/Helpers/MathHelper.cs`
- `Shared/Helpers/FrequencyBinningHelper.cs`
- `Shared/Helpers/StrategyComputationHelper.cs`
- new internal kernel helpers added in the same subsystem:
  - `Shared/Helpers/TemporalIntervalHelper.cs`
  - `Shared/Helpers/TimeSeriesSmoothingHelper.cs`
  - `Shared/Helpers/MetricDataSeriesHelper.cs`

**What was simplified**

- `MathHelper` no longer owns interval normalization/mapping logic and smoothing/interpolation implementation details directly
- `StrategyComputationHelper` no longer repeats valued-series filtering/ordering and timestamp-dictionary preparation logic inline
- `FrequencyBinningHelper` now expresses bin sizing, bin membership, and normalization steps with smaller private helpers rather than one long linear implementation

**What was consolidated**

- repeated valued-series filtering/order rules were consolidated into `MetricDataSeriesHelper`
- interval selection, interval generation, and interval index mapping were consolidated into `TemporalIntervalHelper`
- smoothing and interpolation logic were consolidated into `TimeSeriesSmoothingHelper`
- the public analytical helper entry points were preserved as stable facades so calling code did not need to change

**What was generalized**

- one shared valued-series preparation path now serves `PrepareOrderedData`, `FilterAndOrderByRange`, `PrepareDataForComputation`, timestamp dictionary creation, and unit selection
- one shared temporal interval path now serves tick-interval selection, separator-step calculation, normalized interval generation, and interval index mapping

**What was removed / retired**

- duplicate valued-series filtering and ordering logic previously embedded in `StrategyComputationHelper`
- mixed interval and smoothing implementation detail previously embedded directly inside `MathHelper`

**Regression protection added before refactor**

- added direct tests for `FrequencyBinningHelper`
- expanded direct tests for `MathHelper`
- expanded direct tests for `StrategyComputationHelper`

**Validation / smoke result**

- pre-refactor regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `380` tests passed, `0` failed, `0` skipped
- post-refactor automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and existing warnings only
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `380` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change remained internal-only and behavior-preserving at the helper/kernel layer

**Remaining intentional debt after Iteration 2**

- `MathHelper` still contains formatting, normalization, and binary/unary numeric operations that remain broader than one pure responsibility
- no folder or namespace realignment was performed yet
- rendering-specific analytical cleanup remains deferred to later bounded slices once helper ownership stabilizes

#### Iteration 3 - Phase C - Chart Capability Unification

**Date:** `2026-03-27`  
**Primary objective:** consolidate the repeated qualification-probe lifecycle scaffold shared by the `Distribution` and `WeekdayTrend` rendering families without changing family-specific contracts, routes, or live rendering behavior

**Bounded slice executed**

- `Core/Rendering/Distribution/DistributionRenderingQualificationProbe.cs`
- `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingQualificationProbe.cs`
- new shared internal support:
  - `Core/Rendering/RenderingQualificationProbeSupport.cs`

**Inventory outcome for the chosen slice**

- both families already had explicit family-specific contracts, routes, route resolvers, qualification records, and probe result types
- the strongest repeated shape was not the route map itself, but the lifecycle probe scaffold:
  - render
  - visibility transition
  - offscreen transition
  - reset view
  - clear
  - failure collection / stage reporting
- the family-specific parts remained real and worth keeping explicit:
  - `Distribution` keeps async rendering, disposal validation, tooltip/tag cleanup checks, and polar fallback behavior
  - `WeekdayTrend` keeps route-specific active-chart resolution and reset-assisted render-state recovery

**What was simplified**

- lifecycle probe stages no longer re-implement the same try/catch and stage-failure scaffolding in both families
- visibility, offscreen, reset, clear, and render-stage orchestration now share one internal support path

**What was consolidated**

- common qualification-probe lifecycle mechanics moved into `RenderingQualificationProbeSupport`
- `Distribution` and `WeekdayTrend` probes now delegate shared stage mechanics while preserving their own route/content semantics

**What was generalized**

- a shared probe support abstraction was promoted because there are now at least two real rendering-family consumers with the same lifecycle qualification pattern
- the abstraction stays below the family contract layer and does not force false uniformity into route selection or backend capability models

**What remained explicit intentionally**

- `DistributionRenderingContract` and `WeekdayTrendRenderingContract`
- family-specific route enums and route resolvers
- family-specific qualification records and probe result types
- `Distribution` disposal/tooltip/tag cleanup verification
- `WeekdayTrend` active-chart resolution and reset-assisted content recovery

**Validation / smoke result**

- focused subsystem lane:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~DistributionRenderingQualificationProbeTests|FullyQualifiedName~WeekdayTrendRenderingQualificationProbeTests"` passed on `2026-03-27` with `5` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `380` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change stayed structural inside rendering-family qualification scaffolding and did not intentionally change live UI/render behavior

**Remaining intentional debt after Iteration 3**

- `Distribution` and `WeekdayTrend` contracts still have different capability matrices and host semantics, which should remain explicit unless a later slice proves deeper shared structure
- the next likely Phase C candidate is contract/qualification metadata shape, not live rendering execution
- broader rendering-family unification beyond `Distribution` and `WeekdayTrend` remains deferred until this smaller shared-support seam proves stable

#### Iteration 4 - Phase D - Controller and Host Standardization

**Date:** `2026-03-27`  
**Primary objective:** standardize the converged subtype-selection and display-name resolution behavior shared by the `Distribution` and `WeekdayTrend` controller adapters without changing their family-specific data-loading or rendering-host behavior

**Regression protection established before mutation**

- added direct controller-adapter coverage for `WeekdayTrendChartControllerAdapter`
- expanded rendering contract metadata consistency tests for `Distribution` and `WeekdayTrend`
- pre-mutation focused regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~WeekdayTrendChartControllerAdapterTests|FullyQualifiedName~DistributionRenderingContractTests|FullyQualifiedName~WeekdayTrendRenderingContractTests"` passed on `2026-03-27` with `21` tests passed, `0` failed, `0` skipped

**Bounded slice executed**

- `UI/Charts/Adapters/DistributionChartControllerAdapter.cs`
- `UI/Charts/Adapters/WeekdayTrendChartControllerAdapter.cs`
- new shared controller-adapter support:
  - `UI/Charts/Infrastructure/MetricSeriesSelectionAdapterHelper.cs`
- focused direct test expansion:
  - `DataVisualiser.Tests/Controls/WeekdayTrendChartControllerAdapterTests.cs`
  - `DataVisualiser.Tests/Controls/DistributionChartControllerAdapterTests.cs`
  - `DataVisualiser.Tests/UI/Rendering/DistributionRenderingContractTests.cs`
  - `DataVisualiser.Tests/UI/Rendering/WeekdayTrendRenderingContractTests.cs`

**Inventory outcome for the chosen slice**

- both adapters already converged on the same subtype-combo responsibilities:
  - populate subtype combo from selected series
  - resolve current selection against chart state
  - derive fallback selection from the current chart context
  - derive display name from selected primary/secondary series
- the family-specific parts remained distinct and intentionally explicit:
  - `Distribution` keeps CMS-aware data loading and distribution-mode settings
  - `WeekdayTrend` keeps strategy-driven result computation and chart-mode cycling

**What was simplified**

- repeated subtype-combo population and default-selection logic no longer lives separately in both adapters
- repeated selected-series resolution and display-name derivation logic no longer lives separately in both adapters

**What was consolidated**

- common adapter-side series-selection logic moved into `MetricSeriesSelectionAdapterHelper`
- `DistributionChartControllerAdapter` and `WeekdayTrendChartControllerAdapter` now use the same controller-side selection contract for this converged seam

**What was generalized**

- a shared controller-adapter support helper was promoted because there are now at least two real host/controller consumers with the same subtype-selection and display-name resolution shape
- the generalization stays above family-specific rendering/data-loading behavior and below the broader host/controller layer

**What remained explicit intentionally**

- family-specific render host creation and route resolution
- family-specific data-loading and CMS selection logic
- `WeekdayTrend` strategy creation and result computation
- `Distribution` mode/settings handling and polar-mode behavior

**Validation / smoke result**

- focused subsystem lane after the refactor:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~WeekdayTrendChartControllerAdapterTests|FullyQualifiedName~DistributionChartControllerAdapterTests|FullyQualifiedName~DistributionRenderingContractTests|FullyQualifiedName~WeekdayTrendRenderingContractTests"` passed on `2026-03-27` with `24` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `387` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change remained behavior-preserving controller-adapter standardization backed by focused adapter tests rather than an intentional live UI behavior change

**Remaining intentional debt after Iteration 4**

- broader controller/host convergence still exists outside this slice, especially in full render orchestration and chart-panel event wiring
- no physical folder/file reduction has been attempted yet; this remains deferred to later bounded debt-retirement work once more controller/host seams are stabilized
- `Syncfusion` remains explicitly out of scope for the early controller/host standardization program

#### Iteration 5 - Phase D - Controller and Host Standardization

**Date:** `2026-03-27`  
**Primary objective:** standardize the converged render-host lifecycle delegation shared by the `Distribution` and `WeekdayTrend` controller adapters without changing their family-specific route semantics, render-host creation, or live chart behavior

**Regression protection established before mutation**

- expanded direct controller-adapter coverage to lock down `Clear`, `ResetZoom`, and `HasSeries` delegation through the current render host / resolved route seam
- pre-mutation focused regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~WeekdayTrendChartControllerAdapterTests|FullyQualifiedName~DistributionChartControllerAdapterTests"` passed on `2026-03-27` with `9` tests passed, `0` failed, `0` skipped

**Bounded slice executed**

- `UI/Charts/Adapters/DistributionChartControllerAdapter.cs`
- `UI/Charts/Adapters/WeekdayTrendChartControllerAdapter.cs`
- new shared controller-adapter support:
  - `UI/Charts/Infrastructure/RenderingHostLifecycleAdapterHelper.cs`
  - `UI/Charts/Infrastructure/RenderingHostTarget.cs`
- focused direct test expansion:
  - `DataVisualiser.Tests/Controls/DistributionChartControllerAdapterTests.cs`
  - `DataVisualiser.Tests/Controls/WeekdayTrendChartControllerAdapterTests.cs`

**Inventory outcome for the chosen slice**

- both adapters already converged on the same lifecycle-delegation shape:
  - resolve the active host
  - resolve the route associated with that host or current chart mode
  - delegate `Clear`
  - delegate `ResetZoom`
  - delegate `HasSeries`
- the family-specific parts remained distinct and intentionally explicit:
  - `Distribution` keeps family-specific route resolution tied to distribution mode and polar/cartesian behavior
  - `WeekdayTrend` keeps family-specific host selection and chart-type visibility behavior

**What was simplified**

- repeated route/host lookup no longer lives separately inside both adapters for the lifecycle seam
- `Clear`, `ResetZoom`, and `HasSeries` no longer re-implement nearly identical host-target delegation logic in both adapters

**What was consolidated**

- common render-host lifecycle delegation moved into `RenderingHostLifecycleAdapterHelper`
- a small shared `RenderingHostTarget` value shape now carries the resolved host and route through the common lifecycle path
- `DistributionChartControllerAdapter` and `WeekdayTrendChartControllerAdapter` now use the same lifecycle-delegation contract for this converged seam

**What was generalized**

- a shared controller/host support helper was promoted because there are now at least two real adapter consumers with the same route-plus-host lifecycle delegation shape
- the generalization stays narrow and does not force uniformity into family-specific render-host creation, route resolution rules, or chart-mode policy

**What remained explicit intentionally**

- family-specific render-host creation
- family-specific route resolution and chart-mode semantics
- family-specific data-loading, strategy computation, and mode/settings behavior
- chart-type visibility and any UI-facing toggling policy

**Validation / smoke result**

- focused subsystem lane after the refactor:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~WeekdayTrendChartControllerAdapterTests|FullyQualifiedName~DistributionChartControllerAdapterTests"` passed on `2026-03-27` with `9` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `389` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change remained behavior-preserving controller/host standardization backed by focused adapter tests rather than an intentional live UI behavior change

**Remaining intentional debt after Iteration 5**

- broader controller/host convergence still exists outside this lifecycle seam, especially full render orchestration and chart-panel event wiring
- no physical folder/file reduction has been attempted yet; this remains deferred to later bounded debt-retirement work once controller/host ownership stabilizes further
- `Syncfusion` remains explicitly out of scope for the early controller/host standardization program

#### Iteration 6 - Phase D - Controller and Host Standardization

**Date:** `2026-03-27`  
**Primary objective:** broaden the controller/host standardization pass from the proving pair to the safe cartesian-metric controller cluster by standardizing the fixed-route render-host lifecycle delegation shared by `Main`, `DiffRatio`, and `Normalized`

**Regression protection established before mutation**

- added direct adapter coverage for the fixed-route lifecycle seam across `Main`, `DiffRatio`, and `Normalized`
- pre-mutation focused regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~CartesianMetricControllerAdapterLifecycleTests"` passed on `2026-03-27` with `3` tests passed, `0` failed, `0` skipped

**Bounded slice executed**

- `UI/Charts/Adapters/MainChartControllerAdapter.cs`
- `UI/Charts/Adapters/DiffRatioChartControllerAdapter.cs`
- `UI/Charts/Adapters/NormalizedChartControllerAdapter.cs`
- shared lifecycle support refinement:
  - `UI/Charts/Infrastructure/RenderingHostLifecycleAdapterHelper.cs`
- focused direct test addition:
  - `DataVisualiser.Tests/Controls/CartesianMetricControllerAdapterLifecycleTests.cs`

**Inventory outcome for the chosen slice**

- all three adapters already converged on the same controller-side lifecycle shape:
  - one fixed cartesian-metric route per adapter
  - one cartesian render host type per adapter
  - direct delegation of `Clear`
  - direct delegation of `ResetZoom`
  - direct delegation of `HasSeries`
- the family-specific parts remained distinct and intentionally explicit:
  - `Main` keeps stacked/overlay display-mode behavior and overlay series construction
  - `DiffRatio` keeps operation-toggle behavior, tooltip label updates, and primary/secondary comparison context rebuilding
  - `Normalized` keeps normalization-mode behavior and primary/secondary normalization context rebuilding

**What was simplified**

- repeated fixed-route lifecycle delegation no longer lives separately in the three cartesian-metric adapters
- the route constant used by each adapter is now defined once and reused across the lifecycle seam and render-request construction

**What was consolidated**

- `RenderingHostLifecycleAdapterHelper` now supports fixed-route target creation, clear, reset, and has-content delegation
- `MainChartControllerAdapter`, `DiffRatioChartControllerAdapter`, and `NormalizedChartControllerAdapter` now share one constant-route lifecycle delegation pattern

**What was generalized**

- the lifecycle helper was broadened because there are now multiple controller families using both dynamic-route and fixed-route render-host delegation shapes
- the generalization stays below family-specific chart behavior and above host creation so the cartesian-metric cluster can share one structural path without flattening its distinct behaviors

**What remained explicit intentionally**

- family-specific render-host creation details
- `Main` overlay selection and stacked rendering behavior
- `DiffRatio` comparison-operation behavior and tooltip integration
- `Normalized` normalization-mode selection and context shaping

**Validation / smoke result**

- focused subsystem lane after the refactor:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~CartesianMetricControllerAdapterLifecycleTests"` passed on `2026-03-27` with `3` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `392` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change remained behavior-preserving controller/host standardization backed by direct adapter tests rather than an intentional live UI behavior change

**Remaining intentional debt after Iteration 6**

- the next safe coupled seam inside the cartesian-metric cluster is subtype-selection and selected-series resolution, especially the tighter `DiffRatio` / `Normalized` pairing
- broader chart-panel event wiring and render orchestration still remain outside this bounded slice
- no physical folder/file reduction has been attempted yet; this remains deferred to later bounded debt-retirement work once more controller/host seams are stabilized

#### Iteration 7 - Phase D - Controller and Host Standardization

**Date:** `2026-03-27`  
**Primary objective:** standardize the paired subtype-selection, selected-series resolution, and context-rebuild seam shared by `DiffRatio` and `Normalized` while preserving their distinct chart behavior and semantics

**Regression protection established before mutation**

- added direct adapter coverage for paired subtype population and context rebuilding in `DiffRatio` and `Normalized`
- pre-mutation focused regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~DiffRatioNormalizedSelectionAdapterTests"` passed on `2026-03-27` with `4` tests passed, `0` failed, `0` skipped

**Bounded slice executed**

- `UI/Charts/Adapters/DiffRatioChartControllerAdapter.cs`
- `UI/Charts/Adapters/NormalizedChartControllerAdapter.cs`
- shared selection/context support refinement:
  - `UI/Charts/Infrastructure/MetricSeriesSelectionAdapterHelper.cs`
- focused direct test addition:
  - `DataVisualiser.Tests/Controls/DiffRatioNormalizedSelectionAdapterTests.cs`

**Inventory outcome for the chosen slice**

- both adapters already converged on the same paired-selection shape:
  - primary and secondary subtype combo population
  - secondary-panel visibility and enablement rules
  - state-backed fallback selection
  - display-name resolution from the current chart context
  - cache-backed single-series resolution when the current context does not already supply the selected series
- the family-specific parts remained distinct and intentionally explicit:
  - `DiffRatio` keeps operation-toggle semantics, comparison label generation, and permissive fallback behavior when the secondary path collapses back to the primary series
  - `Normalized` keeps normalization-mode behavior and stricter secondary-series semantics

**What was simplified**

- repeated primary/secondary subtype combo coordination no longer lives separately in both adapters
- repeated primary/secondary selection fallback logic no longer lives separately in both adapters
- repeated display-name resolution and cache-backed selected-series resolution scaffolding no longer lives separately in both adapters

**What was consolidated**

- `MetricSeriesSelectionAdapterHelper` now owns paired subtype combo population, generalized selected-series fallback construction, and shared cache-backed series resolution mechanics
- `DiffRatioChartControllerAdapter` and `NormalizedChartControllerAdapter` now delegate the shared paired-selection seam through one infrastructure path

**What was generalized**

- the existing selection helper was broadened because the `DiffRatio` / `Normalized` pair now proves a second real selection-management shape beyond the earlier single-selection adapter seam
- the generalization stays at the controller-support level and uses policy delegates so the two adapters can keep their legitimate fallback differences

**What remained explicit intentionally**

- `DiffRatio` comparison-operation behavior and panel-title semantics
- `Normalized` normalization-mode behavior and panel-title semantics
- family-specific fallback rules where secondary-series behavior differs

**Validation / smoke result**

- focused subsystem lane after the refactor:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~DiffRatioNormalizedSelectionAdapterTests"` passed on `2026-03-27` with `4` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `396` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - required for this iteration because the touched seam sits in live controller flow and selected-series context rebuilding
  - halt further mutation until targeted smoke completes
  - targeted smoke scope:
    - `DiffRatio`: with two selected series, switch primary and secondary subtype combos and verify chart title, rendered series pairing, and rerender behavior stay correct
    - `Normalized`: with two selected series, switch primary and secondary subtype combos and verify chart title, rendered pairing, and rerender behavior stay correct
    - `DiffRatio` and `Normalized`: with only one selected series, verify the secondary subtype panel collapses and the chart remains stable
- manual smoke follow-up:
  - `DiffRatio` smoke is not currently executable because there is no reachable front-end implementation using that controller/chart path
  - reviewed `documents/reachability-20260327-124731.json` on `2026-03-27` as supporting evidence for the reachable chart/controller smoke surface
  - `Normalized` smoke outcome was accepted based on the reviewed evidence and user verification that the reachable controller/chart behavior looks correct
  - confirmed expected behavior: the `Normalized` chart intentionally does not render when only one sub metric type is selected

**Remaining intentional debt after Iteration 7**

- the next likely cartesian-metric controller seam is adjacent chart-panel event wiring and rerender orchestration around the now-shared selection path
- broader render orchestration outside the cartesian-metric cluster still remains unconsolidated
- no physical folder/file reduction has been attempted yet; this remains deferred to later bounded debt-retirement work once the remaining live controller seams are stabilized

#### Iteration 8 - Phase D - Controller and Host Standardization

**Date:** `2026-03-27`  
**Primary objective:** standardize the subtype-change event wiring and rerender-orchestration seam shared by `DiffRatio` and `Normalized` while preserving their distinct operation and normalization behavior

**Regression protection established before mutation**

- expanded direct adapter coverage for subtype-change event handling and the rerender-if-visible gate in `DiffRatio` and `Normalized`
- pre-mutation focused regression gate:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~DiffRatioNormalizedSelectionAdapterTests"` passed on `2026-03-27` with `8` tests passed, `0` failed, `0` skipped

**Bounded slice executed**

- `UI/Charts/Adapters/DiffRatioChartControllerAdapter.cs`
- `UI/Charts/Adapters/NormalizedChartControllerAdapter.cs`
- shared event/rerender support refinement:
  - `UI/Charts/Infrastructure/MetricSeriesSelectionAdapterHelper.cs`
- focused direct test expansion:
  - `DataVisualiser.Tests/Controls/DiffRatioNormalizedSelectionAdapterTests.cs`

**Inventory outcome for the chosen slice**

- both adapters already converged on the same controller event shape around the shared selection path:
  - ignore subtype changes during initialization
  - ignore subtype changes while subtype combos are being repopulated
  - read the selected series from the combo
  - store the updated chart-state selection
  - rerender only when the chart is visible and the last chart context is available
- the family-specific parts remained distinct and intentionally explicit:
  - `DiffRatio` keeps operation-toggle behavior and comparison title/tooltip semantics
  - `Normalized` keeps normalization-mode behavior and the stricter mode-change rerender guard that requires both primary and secondary data

**What was simplified**

- repeated primary/secondary subtype-change event handling no longer lives separately in both adapters
- repeated visible-and-last-context rerender gating no longer lives separately in both adapters

**What was consolidated**

- `MetricSeriesSelectionAdapterHelper` now owns shared subtype-change event handling and shared rerender-if-visible orchestration
- `DiffRatioChartControllerAdapter` and `NormalizedChartControllerAdapter` now delegate the repeated event/rerender seam through one controller-support path

**What was generalized**

- the selection helper was broadened again because the `DiffRatio` / `Normalized` pair now proves a shared live-controller event shape in addition to the already shared selection/context shape
- the generalization stays narrow and policy-based so mode-specific and chart-specific behavior remains explicit in each adapter

**What remained explicit intentionally**

- `DiffRatio` operation-toggle behavior and title/tooltip updates
- `Normalized` normalization-mode behavior and its stricter mode-change rerender precondition
- family-specific chart rendering semantics after the rerender gate passes

**Validation / smoke result**

- focused subsystem lane after the refactor:
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1 --filter "FullyQualifiedName~DiffRatioNormalizedSelectionAdapterTests"` passed on `2026-03-27` with `8` tests passed, `0` failed, `0` skipped
- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - required for this iteration because the touched seam sits directly in live controller event flow and rerender behavior
  - halt further mutation until targeted smoke completes
  - targeted smoke scope:
    - `Normalized`: with two selected series, change both subtype combos and verify rerender and panel-title behavior stay correct
    - `Normalized`: with the chart hidden or without a reusable context, verify subtype changes update selection state without unstable rerender behavior
    - `DiffRatio`: same controller-flow checks only if that chart path is reachable from the front end
- manual smoke follow-up:
  - `DiffRatio` smoke remains non-executable while no reachable front-end implementation uses that chart/controller path

**Remaining intentional debt after Iteration 8**

- the next likely bounded step before Phase E is any remaining cartesian-metric controller glue that still meaningfully duplicates orchestration without changing semantics
- if no further safe, high-value controller seam remains after smoke, the next move should be reassessment for Phase E readiness rather than forcing one more abstraction
- no physical folder/file reduction has been attempted yet; this remains deferred to bounded debt-retirement work once the remaining controller seams are considered stable

#### Iteration 9 - Phase E - Debt Retirement and Folder Realignment

**Date:** `2026-03-27`  
**Primary objective:** begin the first bounded physical cleanup pass by removing trivial standalone infrastructure files that no longer earn their own file boundary after the controller/host seams stabilized

**Bounded slice executed**

- `UI/Charts/Infrastructure/ChartControllerFactory.cs`
- `UI/Charts/Infrastructure/RenderingHostLifecycleAdapterHelper.cs`
- `UI/Charts/Infrastructure/SubtypeSelectorManager.cs`
- retired standalone files:
  - `UI/Charts/Infrastructure/ChartControllerFactoryContext.cs`
  - `UI/Charts/Infrastructure/ChartControllerFactoryResult.cs`
  - `UI/Charts/Infrastructure/SyncfusionChartControllerFactoryResult.cs`
  - `UI/Charts/Infrastructure/RenderingHostTarget.cs`
  - `UI/Charts/Infrastructure/SubtypeControlPair.cs`

**Inventory outcome for the chosen slice**

- the retired types were all tiny, tightly coupled support shapes with no independent behavioral responsibility:
  - controller-factory context/result records existed only to support `ChartControllerFactory`
  - `RenderingHostTarget` existed only to support `RenderingHostLifecycleAdapterHelper`
  - `SubtypeControlPair` existed only to support `SubtypeSelectorManager`
- keeping those as separate files was increasing physical sprawl without clarifying subsystem ownership

**What was simplified**

- the controller-factory support cluster is now physically co-located in one file
- the render-host lifecycle helper now owns its tiny support value shape directly
- the subtype-selector manager now owns its tiny label/combo pair type directly

**What was consolidated**

- five low-value standalone support files were merged back into their owning infrastructure files
- no public type names or namespaces changed; this was a physical-structure cleanup rather than a behavioral refactor

**What was removed / retired**

- standalone factory context/result wrapper files
- standalone render-host target wrapper file
- standalone subtype-control pair wrapper file

**Validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change stayed internal to physical file consolidation and did not intentionally change live behavior

**Remaining intentional debt after Iteration 9**

- larger Phase E work remains: broader folder/namespace realignment and additional micro-type merging where the owning boundaries are now stable
- the current `DataVisualiser` C# file count is `425`; this pass reduced that count by `5` files from the pre-iteration state
- the next Phase E pass should target the next bounded cluster of low-value file fragmentation rather than broad mass-churn

#### Iteration 10 - Phase E - Debt Retirement and Folder Realignment

**Date:** `2026-03-27`  
**Primary objective:** continue the bounded physical cleanup pass by collapsing the tiny cartesian-metric metadata/request/host cluster into its owning rendering files

**Bounded slice executed**

- `Core/Rendering/CartesianMetrics/CartesianMetricChartRenderingContract.cs`
- `Core/Rendering/CartesianMetrics/CartesianMetricChartRenderingQualificationProbe.cs`
- retired standalone files:
  - `Core/Rendering/CartesianMetrics/CartesianMetricBackendKey.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricBackendQualification.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricRenderingCapabilities.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricRenderingQualification.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricChartRoute.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricChartRenderHost.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricChartRenderRequest.cs`
  - `Core/Rendering/CartesianMetrics/CartesianMetricChartRenderingQualificationProbeResult.cs`

**Inventory outcome for the chosen slice**

- the retired types were all tiny, tightly coupled support shapes with no independent behavior outside the cartesian-metric rendering contract/probe seam
- keeping each of those shapes in its own file was inflating physical sprawl without improving ownership clarity, because they already moved in lockstep with the two owning files

**What was simplified**

- the cartesian-metric rendering contract now physically owns its supporting route, request, qualification, backend, capability, and host types
- the cartesian-metric qualification probe now physically owns its result shape

**What was consolidated**

- eight low-value standalone support files were merged into the two rendering files that actually own the seam
- public type names and namespaces stayed unchanged; this was physical pruning only

**What was removed / retired**

- standalone cartesian-metric backend-key, backend-qualification, capability, and rendering-qualification files
- standalone cartesian-metric route, render-host, and render-request files
- standalone cartesian-metric qualification-probe result file

**Validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and `0` warnings
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change stayed internal to physical file consolidation and did not intentionally change live behavior

**Remaining intentional debt after Iteration 10**

- the larger Phase E realignment pass still remains: bounded folder/namespace cleanup and broader deletion of superseded glue where ownership is already stable
- the current `DataVisualiser` C# file count is `417`; this pass reduced that count by `8` files from the pre-iteration state
- the next Phase E pass should shift from pure micro-type pruning toward bounded folder/namespace cleanup in the now-stabilized chart/rendering areas

#### Iteration 11 - Phase E - Debt Retirement and Folder Realignment

**Date:** `2026-03-27`  
**Primary objective:** execute the main bounded chart-side ownership realignment pass by shrinking `UI/Charts/Infrastructure` to true composition concerns only

**Bounded slice executed**

- `UI/Charts/Infrastructure`
- `UI/Charts/Adapters`
- `UI/Charts/Helpers`
- `UI/Charts/Rendering`
- retired standalone file:
  - `UI/Charts/Infrastructure/ChartSeriesHelper.cs`

**Inventory outcome for the chosen slice**

- `UI/Charts/Infrastructure` had become a catch-all for unrelated responsibilities:
  - controller factory / registry / keys
  - adapter-only lifecycle helpers
  - metric-series selection helpers and caches
  - subtype-selection UI management
  - legend/rendering support
- those seams were already behaviorally stable from earlier iterations, so the remaining debt was mainly physical ownership drift rather than unresolved abstraction work

**What was simplified**

- adapter-only support now lives with adapters:
  - `MetricSeriesSelectionAdapterHelper`
  - `RenderingHostLifecycleAdapterHelper`
- chart selection support now lives with chart helpers:
  - `MetricSeriesSelectionCache`
  - `SubtypeSelectorManager`
- legend/rendering support now lives with chart rendering:
  - `LegendToggleManager`
  - `PieFacetLegendToggleManager`
- `ChartSeriesHelper` was folded into `ChartSurfaceHelper` because it no longer earned a standalone file

**What was consolidated**

- the `Infrastructure` folder is now reduced to its actual composition role:
  - `ChartControllerFactory`
  - `ChartControllerKeys`
  - `ChartControllerRegistry`
- chart-side support types now align physically and by namespace with the owners that use them most directly

**What was removed / retired**

- standalone `ChartSeriesHelper.cs`

**Validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and existing warnings only
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change stayed internal to file/folder/namespace ownership and did not intentionally change live behavior

**Remaining intentional debt after Iteration 11**

- one final bounded Phase E residue pass may still be worthwhile to remove or merge any remaining low-value glue exposed by the new ownership layout
- the current `DataVisualiser` C# file count is `416`; this pass reduced that count by `1` file from the pre-iteration state
- the main structural payoff of Phase E has now been achieved: future cleanup should be judged against whether it materially improves the layout beyond this new baseline rather than continuing churn by principle

#### Iteration 12 - Phase E - Debt Retirement and Folder Realignment

**Date:** `2026-03-27`  
**Primary objective:** finish the final bounded Phase E residue pass by collapsing the remaining low-value micro-type clusters in the converged `Distribution` and `WeekdayTrend` rendering families

**Bounded slice executed**

- `Core/Rendering/Distribution/DistributionRenderingContract.cs`
- `Core/Rendering/Distribution/DistributionRenderingQualificationProbe.cs`
- `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingContract.cs`
- `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingQualificationProbe.cs`
- retained interfaces:
  - `Core/Rendering/Distribution/IDistributionRenderingContract.cs`
  - `Core/Rendering/WeekdayTrend/IWeekdayTrendRenderingContract.cs`
- retired standalone files:
  - `Core/Rendering/Distribution/DistributionBackendKey.cs`
  - `Core/Rendering/Distribution/DistributionBackendQualification.cs`
  - `Core/Rendering/Distribution/DistributionChartRenderHost.cs`
  - `Core/Rendering/Distribution/DistributionChartRenderRequest.cs`
  - `Core/Rendering/Distribution/DistributionRenderingCapabilities.cs`
  - `Core/Rendering/Distribution/DistributionRenderingQualification.cs`
  - `Core/Rendering/Distribution/DistributionRenderingQualificationProbeResult.cs`
  - `Core/Rendering/Distribution/DistributionRenderingRoute.cs`
  - `Core/Rendering/Distribution/DistributionRenderingRouteResolver.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendBackendKey.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendBackendQualification.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendChartRenderHost.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendChartRenderRequest.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingCapabilities.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingQualification.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingQualificationProbeResult.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingRoute.cs`
  - `Core/Rendering/WeekdayTrend/WeekdayTrendRenderingRouteResolver.cs`

**Inventory outcome for the chosen slice**

- both rendering families still carried many tiny family-local files for route, qualification, capability, request, host, backend-key, backend-qualification, and probe-result types
- those types had no independent behavioral responsibility outside the contract/probe seams that owned them, so they were still inflating file sprawl after the earlier controller/host convergence work had stabilized

**What was simplified**

- each family is now reduced to the files that matter structurally:
  - interface
  - rendering contract
  - qualification probe
- the contract files now physically own their route/request/host/qualification/capability/backend support shapes
- the qualification probe files now physically own their result types

**What was consolidated**

- `Distribution` and `WeekdayTrend` now follow the same compact physical pattern already established in `CartesianMetrics`
- public type names and namespaces remained unchanged; this was physical consolidation only

**What was removed / retired**

- eighteen standalone rendering-family support files across `Distribution` and `WeekdayTrend`

**Validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and existing warnings only
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this iteration because the change stayed internal to physical file consolidation and did not intentionally change live behavior

**Remaining intentional debt after Iteration 12**

- the bounded Phase E pruning and realignment work is complete enough to move to evaluation rather than force another cleanup pass by habit
- the current `DataVisualiser` C# file count is `398`; this pass reduced that count by `18` files from the pre-iteration state
- the next major step in the plan is `Phase F - Final Simplification and Metrics Pass`

#### Iteration 13 - Phase F - Final Simplification and Metrics Pass

**Date:** `2026-03-27`  
**Primary objective:** close the cycle with an auditable post-refactor baseline rather than more structural churn

**Measured outcome**

- overall `DataVisualiser` C# file count:
  - pre-Phase E baseline: `430`
  - current baseline: `398`
  - net reduction across the cleanup cycle: `32` files
- repeated scaffolding / physical sprawl reduction in the most targeted areas:
  - `UI/Charts/Infrastructure`: `10` files -> `3`
  - `Core/Rendering/CartesianMetrics`: `13` files -> `5`
  - `Core/Rendering/Distribution`: `12` files -> `3`
  - `Core/Rendering/WeekdayTrend`: `12` files -> `3`

**Success-criteria checkpoint**

- fewer large mixed-responsibility files:
  - achieved in the targeted chart/rendering seams through helper consolidation, controller seam harvesting, and Phase E physical pruning
- fewer repeated scaffolding types across chart families:
  - achieved across `CartesianMetrics`, `Distribution`, `WeekdayTrend`, and chart-side infrastructure
- clearer subsystem ownership boundaries:
  - achieved by shrinking `Infrastructure` to true composition concerns and aligning selection, lifecycle, and legend support with their owners
- lower cognitive load in `Core/Rendering`, `UI/Charts`, and helper-heavy areas:
  - materially improved in the targeted seams, though not eliminated across the whole project
- no regression in rendering, parity, export, theme, orchestration, or flexibility behavior:
  - supported by green build/tests and targeted smoke gates where live controller flow was touched earlier in the cycle
- intentional outliers remain explicit rather than accidental:
  - achieved; remaining large files are now identifiable as honest next-cycle targets rather than unexamined spillover

**Remaining intentional outliers for the next major cycle**

- `UI/MainChartsView.xaml.cs` (`1207` lines):
  - main host composition, selection workflow, event wiring, and orchestration still coexist in one file
- `UI/Charts/Adapters/TransformDataPanelControllerAdapter.cs` (`688` lines):
  - transform workflow, compute gating, pending-load handling, and chart update behavior remain intentionally coupled
- `UI/MainHost/MainChartsEvidenceExportService.cs` (`661` lines):
  - evidence export / parity snapshot composition is large but outside the core rendering-seam consolidation focus
- `Core/Data/Repositories/DataFetcher.cs` (`768` lines):
  - broad data-access responsibilities remain outside this chart-structure cycle
- `Core/Services/BaseDistributionService.cs` (`490` lines):
  - shared distribution computation and rendering-prep logic remains large but was not the main structural risk after the rendering seams stabilized
- `UI/Charts/Adapters/BarPieChartControllerAdapter.cs` (`423` lines):
  - async renderer/surface/facet orchestration remains an honest outlier that was not safe to flatten during this cycle

**Validation / smoke result**

- required automated validation:
  - `dotnet build DataAnalyser.sln -c Debug` passed on `2026-03-27` with `0` errors and existing warnings only
  - `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1` passed on `2026-03-27` with `400` tests passed, `0` failed, `0` skipped
- manual smoke requirement:
  - not required for this closeout iteration because it recorded metrics / completion state and did not intentionally change live behavior

**Cycle closeout position**

- the current cycle is complete enough to hand to external review without needing another cleanup pass by default
- the next major cycle should start from the named intentional outliers above rather than reopening the now-stabilized chart/rendering seams

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
