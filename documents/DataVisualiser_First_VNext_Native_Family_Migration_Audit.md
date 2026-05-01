# DataVisualiser First VNext-Native Family Migration Audit

Recorded: 2026-04-30

Phase: 27 - Migrate First Production Chart Family to VNext-Native Consumption

## Selected Family

Selected family:

```text
Distribution
```

Selection reason:

```text
Distribution had the clearest contract path, strong focused tests, explicit capability contract carriage, existing render-plan diagnostics, and a direct service rendering path that could avoid the legacy ChartDataContext orchestrator.
```

Rejected first-slice alternatives:

```text
WeekdayTrend: clear capability contract, but more chart-mode interaction variants.
Main/Normalized/DiffRatio: broader shared Cartesian surface and secondary-series coupling.
SyncfusionSunburst: hierarchy/vendor-specific path, better left until after one LiveCharts family proves the pattern.
Operation Chain: already new Phase 26 path, not an existing production chart family.
```

## Production Changes

Changed files:

```text
DataVisualiser/Core/Rendering/Contracts/Distribution/DistributionRenderingContract.cs
DataVisualiser/UI/Charts/Presentation/DistributionChartControllerAdapter.cs
```

The Distribution adapter now sets:

```text
UseVNextNativeConsumption: true
```

The Distribution rendering contract now:

```text
builds a VNextUiConsumptionContract from the Distribution render plan
attaches consumption-contract metadata to the render plan
passes the contract-bearing request to the render adapter
uses the direct Distribution service path when UseVNextNativeConsumption is true
keeps the legacy ChartDataContext orchestrator path only for compatibility when the native flag is false
```

## Preserved Metadata

Preserved metadata includes:

```text
program kind
capability kind
composition kind
consumer kind
delivery target
provider key/signature
intent signature
provenance signature
backend key
route
mode
selection display key
surface kind
surface id
consumption contract signature
```

## ChartDataContext Dependency Status

Distribution has not fully removed `ChartDataContext`.

Reduced dependency:

```text
ChartDataContext remains the outer adapter entry model because the shared chart-controller interface is still ChartDataContext-first.
ChartDataContext remains on DistributionChartRenderRequest as compatibility data for the legacy orchestrator path.
The native Distribution render path no longer uses the ChartDataContext orchestrator as its primary render path.
```

This satisfies Phase 27 as a reduction/migration slice, not Phase 28 bridge retirement.

## Evidence

Focused tests:

```text
DistributionRenderingContractTests.DistributionVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
DistributionRenderingContractTests.RenderAsync_WithVNextNativeConsumption_ShouldUseDirectServicePath
DistributionChartControllerAdapterTests.RenderAsync_ShouldPassDistributionCapabilityContract_ToRenderingContract
DistributionChartControllerAdapterTests.RenderAsync_ShouldPassVNextNativeConsumptionContract_ToRenderingContract
```

Focused validation:

```text
Distribution-focused test filter passed 63 tests
```

## Phase 28 Input

Phase 28 may consider retiring the Distribution legacy orchestrator path only after:

```text
full validation passes
manual Distribution smoke test passes
evidence/export behavior is checked
the shared chart-controller interface migration risk is assessed
```

## Phase 28 Supersession Note

Current Phase 28 status:

```text
Superseded by documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md.
The Phase 27 compatibility switch and legacy orchestrator fallback were removed in Phase 28.
The Phase 27 notes above are retained as historical migration evidence, not as a description of the current code shape.
```
