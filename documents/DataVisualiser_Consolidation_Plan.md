# DATAVISUALISER CONSOLIDATION PLAN
**Status:** Active Architectural Reorientation Plan  
**Scope:** `DataVisualiser` hierarchy repair, boundary clarification, entropy reduction, and subsystem consolidation  
**Authority:** Subordinate to `Project Bible.md`, `SYSTEM_MAP.md`, `Project Roadmap.md`, and `Project Overview.md`

---

## 1. Purpose

This document replaces the earlier `DataVisualiser` consolidation framing.

The old plan was useful for harvesting repeated seams, pruning micro-types, and reducing local chart/rendering fragmentation.
That work remains valid.

The primary objective has now changed.

This plan exists to make `DataVisualiser`:

- legible to its architect again
- structurally coherent enough that similar work has one obvious home
- explicit enough that true outliers stand out immediately
- stable enough that future growth does not keep re-entering the system as "programming by exception"

The new mandate is not file-count reduction for its own sake.
It is hierarchy repair.

Secondary objectives from the prior plan still matter where they support this:

- reduce repeated chart-capability scaffolding
- consolidate mathematical and analytical logic
- standardize controller/host/rendering patterns where they are truly converged
- retire superseded glue and migration residue
- align folders and namespaces to present responsibility rather than historical accident
- preserve behavior, capability, reversibility, and future extensibility

This document does not replace roadmap authority.
It operationalizes the next `DataVisualiser` architectural cycle only.

---

## 1.5 START HERE - Handoff and Execution Defaults

Use this section as the default handoff entry point in a new conversation.

Current defaults:

- scope is `DataVisualiser` only
- posture is `Moderate`
- one iteration must have one primary objective
- the default objective is now architectural legibility, not generic consolidation
- safely coupled slices are allowed only when they already share a real contract / host / route / responsibility pattern and batching them reduces duplicated churn
- validation must happen on every significant refactor
- if live behavior changes, halt after automated validation and request targeted manual smoke before continuing

Supporting navigation aids:

- `documents/DATAVISUALISER_PIPELINE_SPINE.md`
- `documents/log.md`

Authority order:

1. `Project Bible.md`
2. `SYSTEM_MAP.md`
3. `Project Roadmap.md`
4. `Project Overview.md`
5. this document

Default execution starting point for the current landscape:

1. refresh the hierarchy map around the current named outliers and the documented presentation spine
2. treat the already-banked helper / transform / chart-context slices as closed unless fresh duplication is discovered
3. begin with the mixed boundary or large outlier that now most impedes legibility
4. only broaden once that slice proves a clearer ownership model

Optimized active iteration spine:

1. `Iteration 1 / Phase A`
   - refresh the hierarchy map around the current outliers and the presentation spine
   - confirm the current highest-leverage outlier
   - choose the smallest slice that most improves architectural legibility
2. `Iteration 2-3 / Phase B`
   - decompose the host / presentation spine
   - separate view concerns, chart-update initiation, export hooks, and pipeline composition in `MainChartsView` / `MainHost`
3. `Iteration 4-5 / Phase C`
   - decompose the highest-value service outliers behind that spine
   - prioritize `DataFetcher`, `MainChartsEvidenceExportService`, or `TransformDataPanelControllerAdapter`
4. `Iteration 6 / Phase D`
   - simplify delivery / rendering spillover only after host and service boundaries are clearer
   - touch `ChartUpdateCoordinator`, `ChartRenderEngine`, `ChartHelper`, or vendor seams only where the earlier passes prove a cleaner owner
5. `Iteration 7 / Phase E`
   - audit what became clearer
   - record remaining honest outliers and define the next cycle from them

Default first-slice preference:

1. start with `MainChartsView / MainHost`
2. move next to `DataFetcher`, `MainChartsEvidenceExportService`, or `TransformDataPanelControllerAdapter`
3. touch delivery/rendering internals only after the host/spine and service layers are less ambiguous
4. reopen low-level helper clusters only if current code proves fresh duplication rather than historical residue

Default validation commands:

1. `dotnet build DataAnalyser.sln -c Debug`
2. `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`

Default smoke rule:

1. no manual smoke for internal-only structural cleanup
2. targeted manual smoke only when live UI / render / export / theme / controller behavior is changed

---

## 2. Architectural North Star

`DataVisualiser` should not be treated as a reporting application.
It is part of a wider canonical data reasoning platform.

Within that wider system, `DataVisualiser` should evolve toward a clean downstream role:

- canonical or downstream-safe data enters through explicit boundaries
- derived results, transforms, comparisons, overlays, and chart-program-style requests remain explicit and reversible
- orchestration coordinates execution without becoming a semantic authority
- rendering infrastructure translates already-defined intent into backend-safe behavior
- UI-facing controllers remain non-authoritative and increasingly UI-agnostic
- concrete clients (`LiveCharts`, `Syncfusion`, web, exports, future consumers) become delivery surfaces rather than architectural authorities

This plan is not imposing a rigid runtime chain of responsibility.
It is imposing a cleaner separation of concerns.

The hierarchy should make these distinctions obvious:

- truth and canonical meaning are upstream and must not be redefined here
- derivation and transforms are downstream and explicit
- orchestration coordinates declared work
- rendering infrastructure is replaceable and qualification-bound
- presentation surfaces are expressive, not authoritative

---

## 3. Current Shape Snapshot

Current observed shape:

- `390` C# files
- `18` XAML files
- `50` C# files under `UI/Charts/Presentation`
- `35` C# files under `Core/Orchestration`
- `14` C# files under `UI/MainHost`
- an explicit interactive navigation aid now exists in `documents/DATAVISUALISER_PIPELINE_SPINE.md`

Current major concentration points:

- `UI/MainChartsView.xaml.cs`
- `Core/Data/Repositories/DataFetcher.cs`
- `UI/MainHost/MainChartsEvidenceExportService.cs`
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`
- `Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`
- `Core/Services/BaseDistributionService.cs`
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs`
- `Core/Rendering/Engines/ChartRenderEngine.cs`
- `Core/Rendering/Helpers/ChartHelper.cs`
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs`

Current read:

- the chart / rendering delivery seams are materially cleaner than before
- the `UI/Charts/Presentation` and `Core/Orchestration` areas are more semantically named and easier to navigate
- there is now a clearer named spine from load to chart refresh, but the host/view layer still carries too much imperative gravity
- the project is still architecturally noisy, but the remaining noise is now more concentrated in a smaller set of high-value outliers
- low-level helper duplication is less dominant than mixed host / orchestration / evidence / data-access responsibilities
- several large files still hide mixed responsibilities that blur the intended hierarchy
- some exception-handling and client-specific repair patterns are still localized rather than absorbed into stable layers

The central problem is now legibility:

- the codebase does not yet explain itself clearly enough to show what is normal versus what is truly exceptional

---

## 4. Completed Work Already Banked

The previous cycle already achieved work worth preserving.

Do not reopen these gains casually:

- chart/rendering scaffolding was materially consolidated
- repeated contract / route / probe / host micro-types were reduced in `CartesianMetrics`, `Distribution`, and `WeekdayTrend`
- chart-side support seams were realigned out of an `Infrastructure` catch-all and into clearer owners
- `UI/Charts/Infrastructure` was reduced to its real composition role
- regression-first mutation, validation discipline, and targeted smoke-gate rules were established and exercised
- chart-side presentation concerns were further realigned under `UI/Charts/Presentation`
- a named presentation spine now exists through `ChartPresentationSpine`, `MainChartsViewChartPipelineFactory`, and the staged orchestration pipeline types
- `ChartDataContextBuilder` was reduced toward context assembly and offloaded series preparation
- duplicate transform preparation ownership was retired
- frequency interval and per-bucket counting responsibility was consolidated into `FrequencyBinningHelper`
- `UI/MainHost` micro-carrier wrappers were flattened back into their single owners where the standalone files no longer earned a boundary

Measured gains already banked:

- overall `DataVisualiser` C# file count reduced from `430` to `390`
- `UI/Charts/Infrastructure` reduced from `10` files to `3`
- `Core/Rendering/CartesianMetrics` reduced from `13` files to `5`
- `Core/Rendering/Distribution` reduced from `12` files to `3`
- `Core/Rendering/WeekdayTrend` reduced from `12` files to `3`
- `UI/MainHost` reduced from `22` files to `14`

Historical summary of that cycle lives in `documents/log.md`.
This plan no longer serves as a line-by-line execution ledger for that earlier work.
Later April 2026 presentation/orchestration optimizations are also considered banked unless current code proves they should be reopened.

---

## 5. Primary Mandate

The primary mandate of the next cycle is:

- make the hierarchy trustworthy enough that it exposes the real remaining entropy

In practice, that means:

1. Similar responsibilities should follow one recognizable structural pattern.
2. Similar low-level operations should have one obvious home.
3. Exceptions should look exceptional rather than normal.
4. Large files should exist only where their size reflects a genuine concentration of responsibility, not accidental drift.
5. The tree should reveal intent:
   - what is data-access-facing
   - what is derivation-facing
   - what is orchestration
   - what is rendering infrastructure
   - what is controller / host / client-specific
   - what is evidence / diagnostics

This is the standard against which future simplification should be judged.

---

## 6. Target Module Buckets

These are the target responsibility buckets to use when evaluating any `DataVisualiser` slice.

### 6.1 Data Access and Intake Facades

- data retrieval contracts
- repository facades
- ingestion-adjacent caller boundaries used by `DataVisualiser`

### 6.2 Canonical / Context Handoff

- context objects and handoff shapes that bring canonical or downstream-safe data into `DataVisualiser`
- no semantic reinterpretation here

### 6.3 Derived and Transform Kernel

- shared algebraic operations
- interval / bucket / range logic
- smoothing / alignment / comparison helpers
- transform preparation and derived-result construction

### 6.4 Presentation Planning / Chart Programs

- declared result composition
- selected-series shaping
- multi-result intent
- future programmable chart composition seams

### 6.5 Orchestration and Coordinators

- context building
- execution routing
- render invocation handoff
- evidence initiation

### 6.6 Rendering Capability Families

- capability contracts
- backend qualification
- route / probe / host logic
- backend lifecycle isolation

### 6.7 UI-Agnostic Controllers and Hosts

- standardized controller behavior
- host coordination
- option / toggle / visibility behaviors where capability semantics genuinely align

### 6.8 Client Adapters and Surfaces

- `LiveCharts`
- `Syncfusion`
- future web or export-oriented consumers

### 6.9 Evidence and Diagnostics

- parity exports
- reachability / evidence generation
- architecture diagnostics and state visibility

Not every class will fit perfectly the first time.
That is acceptable.
The point is to make misfits visible.

---

## 7. Governing Legibility Cycle

All later consolidation work should follow this cycle:

1. `Identify Entropy`
2. `Constrain / Re-home`
3. `Consolidate Irreducibles`
4. `Standardize Proven Patterns`
5. `Retire / Remove / Clean`
6. `Validate / Re-measure`

Operational rule:

- name the confusing shape first
- move work toward the clearest owner second
- consolidate similar low-level logic third
- standardize only what the slice actually proves
- delete residue continuously
- validate after every pass

Meaning of each stage:

### 7.1 Identify Entropy

- find where the hierarchy stops explaining itself
- identify similar operations spread across unrelated files
- identify accidental exceptions versus honest outliers

### 7.2 Constrain / Re-home

- move responsibilities toward clearer owners
- separate mixed concerns before introducing new abstractions
- prefer obvious placement over clever structure

### 7.3 Consolidate Irreducibles

- give similar low-level operations one obvious home
- examples include:
  - intervals
  - buckets
  - ranges
  - temporal alignment
  - smoothing
  - selected-series resolution
  - transform preparation

### 7.4 Standardize Proven Patterns

- standardize only after a slice proves a real repeated shape
- require at least `2-3` real consumers before promoting a shared abstraction
- do not generalize superficial similarity

### 7.5 Retire / Remove / Clean

- remove superseded wrappers, glue, or compatibility residue
- perform bounded renames only when the new owner is obvious
- reduce structural noise, not just raw file count

### 7.6 Validate / Re-measure

- full solution build
- `DataVisualiser.Tests`
- focused subsystem lanes
- targeted smoke only where live behavior changed
- re-measure whether the tree is actually easier to read

---

## 8. Per-Iteration Protocol

Every implementation pass should follow this procedure:

1. `Establish / Refresh Regression Protection`
   - add or extend focused tests around the exact bounded slice before production mutation
   - if focused coverage already exists, rerun it first and record it as the pre-mutation regression gate

2. `Select a bounded subsystem slice`
   - choose one primary slice per iteration
   - do not mix unrelated slices unless coupling forces it
   - do couple adjacent slices when the same bounded pass materially improves hierarchy legibility

3. `Inventory the current shape`
   - record hotspot files
   - record repeated operations
   - record naming inconsistencies
   - record unclear ownership seams

4. `Constrain / Re-home`
   - tighten boundaries first
   - move responsibilities toward clearer owners
   - prefer structural clarity before abstraction

5. `Consolidate irreducible operations or duplicated shapes`
   - merge overlapping helpers, pipelines, and micro-types
   - reduce parallel implementations of the same idea

6. `Standardize only what the pass proves`
   - shared abstractions require real repeated consumers
   - avoid flattening honest differences

7. `Retire / Remove / Clean`
   - delete superseded files, classes, and compatibility residue
   - apply bounded renames where ownership is stable

8. `Validate / Re-measure`
   - run:
     - full solution build
     - `DataVisualiser.Tests`
     - focused subsystem lanes
   - reassess:
     - whether similar work now has one obvious home
     - whether the tree better reflects intent
     - whether the true outliers are clearer

9. `Record the new baseline`
   - record:
     - what was simplified
     - what was re-homed
     - what was consolidated
     - what was removed
     - what remains intentionally deferred

Execution rules:

1. one iteration must have one primary objective
2. behavior preservation beats cosmetic cleanup
3. do not abstract before the consolidation pass proves a real pattern
4. if hidden coupling expands the scope materially, stop and re-scope
5. honest outliers may remain outliers if explicitly documented
6. each iteration should leave the slice more obvious than it was before

---

## 9. Trackable Phase Plan

These are the concrete active step groups to follow and track.

### 9.1 Phase A - Re-Baseline and Outcome Gate

Goal:

- confirm the current architecture map against the latest code and choose the highest-leverage legibility slice

Default iteration count:

- `1` bounded iteration at the start of a cycle

Tracked steps:

1. inventory the current large outliers
2. inventory the now-banked seams that should not be reopened casually
3. map the current tree against the presentation spine and target module buckets
4. identify which single outlier most impedes comprehension of the whole system
5. choose the smallest slice that unlocks the clearest architectural payoff

### 9.2 Phase B - Host Spine Decomposition

Goal:

- make the primary interactive host path legible from view event to chart update request to pipeline composition

Default iteration count:

- `1-2` bounded iterations

Tracked steps:

1. separate view-only behavior from chart-update initiation in `MainChartsView`
2. separate export / diagnostics hooks from live chart-host behavior
3. make pipeline composition and coordinator construction explicit in `MainHost`
4. reduce imperative host gravity without changing the spine contract
5. leave `MainChartsView` smaller and more obviously a host rather than a catch-all

### 9.3 Phase C - Outlier Service Decomposition

Goal:

- decompose the highest-value non-host service outliers behind the presentation spine

Default iteration count:

- `1-2` bounded iterations

Tracked steps:

1. choose between `DataFetcher`, `MainChartsEvidenceExportService`, and `TransformDataPanelControllerAdapter` based on current leverage
2. separate query shaping from lookup/catalog concerns if the `DataFetcher` slice is chosen
3. separate parity snapshot building, warning aggregation, and export payload composition if the evidence slice is chosen
4. separate selection workflow, compute orchestration, grid sync, and render invocation if the transform slice is chosen
5. keep each chosen service closer to one dominant responsibility

### 9.4 Phase D - Delivery and Rendering Spillover Simplification

Goal:

- clean up the remaining delivery / rendering spillover only after the host and service seams are clearer

Default iteration count:

- `1` bounded iteration, and only if earlier phases expose a better owner

Tracked steps:

1. reassess `ChartUpdateCoordinator`, `ChartRenderEngine`, `ChartHelper`, and vendor seams after Phases B-C
2. move chart-update repair logic downward only where a clearer owner is now visible
3. keep vendor quirks quarantined below orchestration
4. avoid broad rendering churn unless the host/service work proves it necessary

### 9.5 Phase E - Architecture Audit and Next-Cycle Gate

Goal:

- measure whether the hierarchy now explains itself well enough to expose the next true bottlenecks

Default iteration count:

- `1` closeout iteration

Tracked steps:

1. measure concentration reduction in the dominant host / service outliers
2. list what became more legible in the tree
3. list what remains mixed and why
4. name the next cycle from the remaining outliers rather than from convenience

---

## 9.7 Current Cycle Progress

Current April 2026 re-baseline for the legibility-first cycle:

- the hierarchy baseline is banked
- the helper / kernel consolidation slices are banked
- the initial chart-context boundary slice is banked
- the chart presentation landscape has shifted materially through the introduction of `UI/Charts/Presentation`, `ChartPresentationSpine`, `MainChartsViewChartPipelineFactory`, and staged orchestration pipelines
- a pre-plan `UI/MainHost` sprawl-tightening pass is banked and should not by itself be mistaken for the real host decomposition phase
- the active plan has been re-optimized around host-spine decomposition, service-outlier decomposition, and only then delivery simplification
- the next default entry point should now be chosen from the remaining mixed host / orchestration / evidence / data-access outliers rather than from already-harvested helper clusters

Historical implementation record from the first three slices:

- `Phase A` has been refreshed
- the current outlier map remains materially valid
- the first bounded `Phase B` slice was selected from the high-entropy operation clusters rather than from UI churn

Bounded slice selected:

- distribution interval creation and per-bucket frequency counting
- primary files:
  - `Shared/Helpers/FrequencyBinningHelper.cs`
  - `Core/Services/FrequencyShadingCalculator.cs`
  - `Core/Services/BaseDistributionService.cs`

Reason this slice was chosen first:

- the low-level interval / bucket frequency logic was split between more than one owner
- the duplication was pure and behavior-preserving enough to consolidate safely
- re-homing it improves legibility without forcing a premature controller or UI pass

Current banked result from this slice:

- uniform interval creation and per-bucket frequency counting now have one obvious owner in `FrequencyBinningHelper`
- `FrequencyShadingCalculator` is reduced back toward shading-data orchestration rather than low-level interval ownership
- `BaseDistributionService` tooltip preparation now consumes the same shared low-level helper
- focused regression protection was extended for the re-homed interval and frequency semantics

Validation recorded for this pass:

- focused regression lane: `12` passed, `0` failed
- `dotnet build DataAnalyser.sln -c Debug`: passed with `0` errors, `0` warnings
- `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`: `392` passed, `0` failed, `0` skipped

Manual smoke:

- not required for this pass because the change stayed internal-only and behavior-preserving

### Banked Phase B Work (Slice 2)

The second bounded `Phase B` slice stayed in the transform-preparation / result-composition cluster.

Bounded slice selected:

- duplicate transform preparation and result composition services
- primary files:
  - `Core/Transforms/TransformComputationService.cs`
  - `Core/Transforms/TransformOperationService.cs`
  - `Core/Transforms/TransformComputationResult.cs`
  - `Core/Transforms/TransformOperationResult.cs`

Reason this slice was chosen next:

- the transform layer still had two near-identical services preparing metric data, aligning timestamps, choosing the same expression-vs-legacy path, and shaping equivalent result payloads
- that duplication did not express a real subsystem difference
- retiring the duplicate owner improves legibility without forcing a UI or controller pass

Current banked result from this slice:

- `TransformComputationService` is the single owner for transform preparation and computation execution
- the duplicate `TransformOperationService` and `TransformOperationResult` files were retired
- transform preparation and result composition now have one obvious home instead of parallel service shapes

Validation recorded for this pass:

- focused regression lane: `11` passed, `0` failed
- `dotnet build DataAnalyser.sln -c Debug`: passed with `0` errors, `47` existing warnings
- `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`: `392` passed, `0` failed, `0` skipped

Manual smoke:

- not required for this pass because the change stayed internal-only and behavior-preserving

### Banked Phase C Work

The next bounded slice stayed structural and reconciled a mixed ownership boundary inside chart-context construction.

Bounded slice selected:

- chart data context assembly vs series-derivation preparation
- primary files:
  - `Core/Orchestration/Builders/ChartDataContextBuilder.cs`
  - `Core/Orchestration/Builders/ChartDataSeriesPreparationHelper.cs`
  - `Tests/Orchestration/ChartDataContextBuilderTests.cs`

Reason this slice was chosen:

- `ChartDataContextBuilder` was still owning both orchestration/assembly and pure timeline / alignment / smoothing / derived-series preparation
- that mixed boundary made the builder harder to read as an orchestration component
- the split could be made without changing the public contract or live chart behavior

Current banked result from this slice:

- `ChartDataContextBuilder` is reduced toward context assembly and label / CMS attachment
- the pure timeline, alignment, smoothing, difference, ratio, and normalization preparation path now has a dedicated internal owner
- dead CMS conversion residue was removed from the builder because it was no longer part of its responsibility

Validation recorded for this pass:

- focused regression lane: `4` passed, `0` failed
- `dotnet build DataAnalyser.sln -c Debug`: passed with `0` errors, `0` warnings
- `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`: `393` passed, `0` failed, `0` skipped

Manual smoke:

- not required for this pass because the change stayed structural-only and preserved the existing chart-context contract

### Current recommended re-entry point

Preferred next bounded slice:

- `UI/MainChartsView.xaml.cs`
- `UI/MainHost/MainChartsViewChartPipelineFactory.cs`
- `UI/ChartPresentationSpine.cs`

Why this is now the best next move:

- the project now has a named presentation spine and clearer orchestration staging
- `MainChartsView.xaml.cs` remains the single largest concentration point and still carries event wiring, toggle logic, export hooks, view concerns, and chart-update initiation
- the new spine makes the remaining host/view residue easier to separate without guessing at intent

Fallback next slices if host decomposition proves too broad:

- `Core/Data/Repositories/DataFetcher.cs`
- `UI/MainHost/MainChartsEvidenceExportService.cs`
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`

Expected smoke posture for the next implementation slice:

- likely required if the `MainChartsView` / `MainHost` path is mutated, because that work touches live controller / host / chart-update behavior

### Banked MainHost sprawl-tightening pre-pass

Bounded slice selected:

- trivial `UI/MainHost` micro-carriers that were single-use wrappers around one owner
- primary files:
  - `UI/MainHost/MainChartsViewStartupCoordinator.cs`
  - `UI/MainHost/MainChartsViewResolutionResetCoordinator.cs`
  - `UI/MainHost/MainChartsViewChartPresentationCoordinator.cs`
  - `UI/MainHost/MainChartsViewChartUpdateCoordinator.cs`
  - `UI/MainHost/MainChartsViewEventBinder.cs`
  - `UI/MainHost/MainChartsViewChartPipelineFactory.cs`
  - `UI/MainHost/ReachabilityExportWriter.cs`

Current banked result from this pass:

- action, handler, context, result, and tiny export carrier types were folded into their single owners
- `UI/MainHost` file count was reduced without collapsing the meaningful coordinator / factory / export structure
- the next real host decomposition slice is now slightly cleaner and easier to read

Validation recorded for this pass:

- focused regression lane: `20` passed, `0` failed
- `dotnet build DataAnalyser.sln -c Debug`: passed with `0` errors, existing warnings only
- `dotnet test DataVisualiser.Tests\\DataVisualiser.Tests.csproj -c Debug -m:1`: `393` passed, `0` failed, `0` skipped

Manual smoke:

- not required for this pass because the change stayed structural-only and behavior-preserving

---

## 10. Current Priority Outliers

These are the current named outliers most likely to matter in the next cycle:

- `UI/MainChartsView.xaml.cs`
- `Core/Data/Repositories/DataFetcher.cs`
- `UI/MainHost/MainChartsEvidenceExportService.cs`
- `UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`
- `Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`
- `Core/Services/BaseDistributionService.cs`
- `UI/Syncfusion/SyncfusionChartsView.xaml.cs`
- `Core/Rendering/Engines/ChartRenderEngine.cs`
- `Core/Rendering/Helpers/ChartHelper.cs`
- `UI/Charts/Presentation/BarPieChartControllerAdapter.cs`

These are not automatically "wrong."
They are the places where the hierarchy most likely still needs to explain itself better.

---

## 11. Candidate High-Entropy Operation Clusters

Use this list as the first place to look for irreducible-operation duplication:

- main-host event wiring and chart-update request flow
- presentation-spine publication and host orchestration handoff
- evidence / parity snapshot resolution and export composition
- data-fetch query shaping, table normalization, subtype resolution, and sampling / count retrieval
- transform panel workflow composition across selection, compute, render, and grid sync
- render normalization, axis shaping, tooltip range shaping, and chart helper spillover
- client-specific repair logic that may really belong in orchestration, planning, or client adapters

Already partially banked and not default reopen targets:

- bucket / interval / range determination
- transform preparation and result composition
- chart-context series preparation

The goal is not to create one giant helper.
The goal is to ensure each concern has one obvious home.

---

## 12. Validation and Smoke-Test Discipline

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

## 13. Non-Goals and Guardrails

- do not reduce capability or flexibility
- do not force false uniformity across honest outliers
- do not optimize for file-count alone
- do not reopen stabilized rendering seams casually
- do not let UI become a semantic authority
- do not let backend quirks leak upward into orchestration or meaning
- do not replace one catch-all helper with another catch-all helper
- do not conflate presentation planning with canonical semantics
- do not mix unrelated subsystems in one iteration without explicit justification
- do not use broad rewrites where a bounded slice can reveal the architecture more honestly

---

## 14. Success Criteria

This cycle should be considered successful when:

- similar operations have one obvious home
- the folder tree communicates responsibility more clearly than migration history
- repeated boilerplate variations are materially reduced where they hide the architecture
- true outliers are easier to identify and harder to ignore
- exceptions look exceptional rather than normal
- downstream delivery layers are clearer and less authority-confused
- no regression occurs in rendering, parity, export, theme, orchestration, or flexibility behavior
- remaining intentional debt is explicit enough to define the next cycle cleanly

The deepest test is simple:

- when the architect looks at the tree, can they tell what the truly disparate pieces are without first having to decode a maze of similar-but-different structures?

If not, the consolidation is not complete.

---

**End of DataVisualiser Consolidation Plan**
