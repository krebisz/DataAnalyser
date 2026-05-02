# DataVisualiser Final Convergence Audit

Phase: 35 - Final Convergence Audit

## Generated Evidence

Generated on 2026-05-02:

- `project-tree.txt`
- `codebase-index.md`
- `dependency-summary.md`
- `type-dependency-diagram.md`

Current generated structural readings:

- codebase index: 1048 declared symbols, including 767 `DataVisualiser` symbols and 192 `DataVisualiser.Tests` symbols
- type dependency diagram: 1013 declared type symbols
- type dependency diagram: 7575 direct textual type-reference edges
- dependency density: 0.7389%
- project dependency shape unchanged: `DataVisualiser` depends on `DataFileReader`; `DataVisualiser.Tests` depends on `DataVisualiser`; `DataFileReader.Tests` depends on `DataFileReader`
- `LegacyMetricViewGateway` production references: 0
- `MetricLoadSnapshotGateway` production references: 3
- main-chart VNext result carries native `ChartProgram` and `MetricLoadSnapshot` before compatibility projection

## Convergence Result

The architecture is substantially converged around the target grammar, but final convergence is not fully closed.

The target spine is used by production paths for capability contracts, consumption contracts, consumer-neutral surface models, render-plan metadata, provider qualification, evidence export, and Operation Chain execution.

Final closure remains blocked by bounded production bridges that still carry compatibility responsibilities:

- `ChartDataContext`
- main-chart `LegacyChartProgramProjector`
- selected-series `VNextDataResolutionHelper` legacy fallback
- service-backed metric loading through `MetricLoadSnapshotGateway`
- strategy cut-over compatibility

These are documented and gated. They are not unbounded architectural authority, but they are also not retired or validation-only.

## Dependency Density Reading

`ChartDataContext` remains a high incoming hub:

- `ChartDataContext`: 166 incoming references
- `ChartProgramKind`: 166 incoming references
- `ChartRenderPlan`: 96 incoming references
- `VNextUiConsumptionContract`: 74 incoming references
- `ConsumerSurfaceModel`: 64 incoming references
- `IAnalyticalCapabilityContract`: 63 incoming references

Reading:

- `ChartDataContext` remains the largest blocker to declaring full convergence.
- VNext contract and surface types are now visible as shared production vocabulary, not isolated tests.
- `IAnalyticalCapabilityContract` confirms the common capability-contract shape added in Phase 33.

## Remaining Bridge Classification

| Bridge | Current classification | Closure condition |
| --- | --- | --- |
| `ChartDataContext` | bounded production compatibility model | retire as primary UI/rendering state carrier only after production consumers receive equivalent VNext-native input or surface output |
| `LegacyChartProgramProjector` | partly retired; still production-bound for main chart integration | remove after `VNextMainChartLoadResult.ProjectedContext` is no longer required; native `Program` and `Snapshot` are now available beside it |
| `VNextDataResolutionHelper` | production selected-series VNext-first bridge with legacy fallback | retire per family after fallback-free parity, smoke, metadata, and provenance evidence |
| `LegacyMetricViewGateway` | retired as a legacy-named production bridge type | completed by replacement with `MetricLoadSnapshotGateway` |
| `MetricLoadSnapshotGateway` with `MetricSelectionServiceSeriesLoader` | production service-backed loader adapter into VNext reasoning | replace when reasoning can load through a native non-service-backed metric loader |
| strategy cut-over / `CreateLegacyStrategy` | production compatibility and evidence parity support | reduce after native strategy input contracts cover live paths and parity remains evidence-only |
| evidence parity builders | validation-only | keep while they provide active audit value |
| terminal rendering fallbacks | terminal delivery compatibility | keep while qualified as terminal fallback and not semantic authority |

## Target Spine Confirmation

Confirmed:

- canonical meaning remains upstream in VNext/CMS-facing contracts
- capability contracts share `IAnalyticalCapabilityContract`
- production families carry `VNextUiConsumptionContract`
- `ConsumerSurfaceModel` exists before terminal delivery
- render plans carry vocabulary metadata, provider metadata, provenance, and surface identifiers
- rendering/vendor concerns remain terminal in VNext boundaries
- evidence export observes parity, reachability, diagnostics, runtime state, and render-plan history without live-routing authority
- non-chart consumer path remains represented by evidence/API/export delivery contracts
- Operation Chain core uses VNext-native derived-dataset surface output and metadata
- Operation Chain UI remains display-only and does not execute or define meaning

## Production Consumer Status

Production chart families have VNext-native consumption metadata and surface output:

- Distribution
- WeekdayTrend
- BarPie
- Transform
- SyncfusionSunburst
- Main
- Normalized
- Difference and Ratio remain automated/test-backed but not manually UI-smokeable because the tab is not wired for them as an interactive path.

## Operation Chain Status

Operation Chain is architecturally aligned but not yet user-value-complete.

Confirmed:

- core executor exists
- provenance/trace/lossiness/reversibility metadata is preserved
- derived dataset surface output is represented through `ConsumerSurfaceModel`
- workbench tab is display-only and currently empty unless a result is supplied

Not complete:

- no interactive workflow is wired into the tab
- no user-authored operation chain is executable from the UI

This is expected before Phase 36+ formal coverage and bounded-generativity work.

## Evidence

Automated validation:

- `DataVisualiser.Tests`: 1037 passed
- `DataFileReader.Tests`: 15 passed
- focused Phase 34 validation: 11 passed
- focused metric gateway retirement validation: 168 passed
- focused main-chart projection reduction validation: 147 passed
- main-chart projection reduction smoke: `documents/reachability-20260502-120706.json`
  - no recent UI errors
  - VNext Main request/snapshot/program/projected-context signatures present and aligned
  - Main and Normalized render-plan history present
  - Diff/Ratio unavailable for manual smoke

Manual smoke evidence:

- `documents/reachability-20260502-062913.json`
- no recent UI errors
- parity summary passed
- parity warnings: 0
- render-plan history includes Distribution, WeekdayTrend, Transform, SyncfusionSunburst, and Main
- selected-series Distribution and Transform renders are present after Phase 34 bypass retirement

## Final Assessment

Phase 35 proves convergence progress, not final closure.

The architecture can now be described through the target grammar for the migrated production paths. It still relies on bounded compatibility bridges for main-chart projection, selected-series fallback, service-backed metric loading, and strategy cut-over.

Next implementation should not begin Phase 36+ formal-algebra work until the project accepts one of these positions:

- treat the remaining bridges as acceptable bounded compatibility for now, and proceed to post-convergence exploratory work with that caveat
- run a dedicated bridge-removal track to eliminate `ProjectedContext`, selected-series fallback, and service-backed metric loader dependencies before declaring final convergence complete
