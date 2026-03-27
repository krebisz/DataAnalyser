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
