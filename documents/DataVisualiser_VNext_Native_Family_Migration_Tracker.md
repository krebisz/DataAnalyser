# DataVisualiser VNext-Native Family Migration Tracker

Phase: 29 - Repeat Production Family Migration Slice-by-Slice

Date: 2026-05-01

Purpose:

```text
Track production chart-family migration through the VNext-native consumption contract path one family at a time.
```

## Family Status

| Family | Status | Evidence | Notes |
| --- | --- | --- | --- |
| Distribution | Migrated and bridge retired | `documents/DataVisualiser_First_VNext_Native_Family_Migration_Audit.md`; `documents/DataVisualiser_First_Family_Legacy_Bridge_Retirement.md`; post-retirement smoke export `documents/reachability-20260501-114754.json` | First proven family. Legacy orchestrator fallback retired in Phase 28. |
| WeekdayTrend | Migrated and smoke confirmed | `WeekdayTrendVNextConsumptionContractBuilder`; focused WeekdayTrend tests; architecture guardrail; smoke export `documents/reachability-20260501-154628.json` | Second family slice. No equivalent legacy orchestrator bridge was found in its rendering contract, so bridge retirement is not a separate code removal for this family. |
| BarPie | Migrated and smoke confirmed | `BarPieVNextConsumptionContractBuilder`; focused BarPie tests; architecture guardrail; smoke export `documents/reachability-20260501-170058.json` | Third family slice. No equivalent legacy orchestrator bridge was found in its rendering contract, so bridge retirement is not a separate code removal for this family. |
| Transform | Migrated and smoke confirmed | `TransformVNextConsumptionContractBuilder`; shared Cartesian consumption-contract factory; focused Transform tests; architecture guardrail; smoke export `documents/reachability-20260501-173257.json` | Fourth family slice. Existing render-invoker behavior is preserved; VNext consumption metadata is attached through the shared Cartesian update path after render-plan construction. |
| SyncfusionSunburst | Migrated and smoke confirmed | `SyncfusionSunburstVNextConsumptionContractBuilder`; focused SyncfusionSunburst tests; architecture guardrail; smoke export `documents/reachability-20260501-175856.json` | Fifth family slice. Existing hierarchy render-plan path is preserved; VNext consumption metadata is attached before Syncfusion delivery. |
| Main | Migrated and smoke confirmed | `CartesianMetricVNextConsumptionContractBuilder`; shared Cartesian consumption-contract factory; focused Main/Cartesian tests; architecture guardrail; smoke export `documents/reachability-20260501-180931.json` | Sixth family slice. Dynamic main display-mode resolution is preserved by building the VNext consumption contract from the concrete render plan. |
| Normalized | Migrated and smoke confirmed | `CartesianMetricVNextConsumptionContractBuilder`; shared secondary Cartesian consumption-contract factory; focused Normalized tests; architecture guardrail; smoke export `documents/reachability-20260501-182337.json` | Seventh family slice. Migrated together with Difference/Ratio because both share the same secondary Cartesian invocation stage. |
| Difference/Ratio | Migrated; UI smoke not available | `CartesianMetricVNextConsumptionContractBuilder`; shared secondary Cartesian consumption-contract factory; focused Difference/Ratio tests; architecture guardrail | Migrated with Normalized in the same secondary Cartesian slice at user request. Diff/Ratio is not wired to the current UI, so manual smoke is not available; the latent render path carries VNext consumption metadata when invoked. |

## WeekdayTrend Slice

Selected family:

```text
WeekdayTrend
```

Selection reason:

```text
WeekdayTrend had a dedicated rendering contract, existing capability contract, render-plan builder, VNext family loading evidence, and focused tests.
Its rendering contract already avoided the Distribution-style legacy orchestrator fallback, making it a lower-risk second family for consumption-contract convergence.
```

Production changes:

```text
DataVisualiser/Core/Rendering/Contracts/WeekdayTrend/WeekdayTrendRenderingContract.cs
```

Contract changes:

```text
WeekdayTrendChartRenderRequest now carries an optional VNextUiConsumptionContract.
WeekdayTrendRenderingContract builds a VNextUiConsumptionContract when one is not supplied.
WeekdayTrendRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId to the render plan before delivery.
WeekdayTrend rendering behavior still delegates to WeekdayTrendChartUpdateCoordinator.
```

Focused tests:

```text
WeekdayTrendRenderingContractTests.WeekdayTrendVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
WeekdayTrendRenderingContractTests.Render_ShouldAttachVNextConsumptionMetadata
WeekdayTrendChartControllerAdapterTests.OnChartTypeToggleRequested_ShouldToggleMode_AndRenderLastContext
ArchitectureGuardrailTests.WeekdayTrendFamilyMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-154628.json
WeekdayTrend visible: true
WeekdayTrend parity completed and passed
latest WeekdayTrend render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan history included Cartesian, Polar, and Scatter WeekdayTrend plans
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
```

## BarPie Slice

Selected family:

```text
BarPie
```

Selection reason:

```text
BarPie had a dedicated rendering contract, existing capability contract, render-plan builder, VNext family loading evidence, and focused lifecycle tests.
Its rendering contract already avoided the Distribution-style legacy orchestrator fallback, making it a low-risk third family for consumption-contract convergence.
```

Production changes:

```text
DataVisualiser/Core/Rendering/Contracts/BarPie/BarPieRenderingContract.cs
DataVisualiser/Core/Rendering/Contracts/BarPie/BarPieRenderingTypes.cs
```

Contract changes:

```text
BarPieChartRenderRequest now carries an optional VNextUiConsumptionContract.
BarPieRenderingContract builds a VNextUiConsumptionContract when one is not supplied.
BarPieRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId to the render plan before delivery.
BarPie rendering behavior still delegates to the existing UiChartRenderPlanAdapter and renderer surface.
```

Focused tests:

```text
BarPieRenderingContractTests.BarPieVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
BarPieRenderingContractTests.RenderAsync_ShouldPreserveVocabularyMetadata
ArchitectureGuardrailTests.BarPieFamilyMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-170058.json
BarPie visible: true
BarPie bucket count: 4
latest BarPie render plan used LiveChartsWpf.Column
render-plan history included Column and PieFacet BarPie plans
latest BarPie render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
parity summary was unavailable because no chart context / primary series was available for non-BarPie parity lanes
```

## Transform Slice

Selected family:

```text
Transform
```

Selection reason:

```text
Transform had an existing capability contract and already flowed through the shared Cartesian render-plan adapter path.
It was the next lowest-risk family after Distribution, WeekdayTrend, and BarPie because it could preserve the existing render invoker while adding VNext consumption metadata at the shared render-plan handoff.
```

Production changes:

```text
DataVisualiser/Core/Orchestration/ChartUpdateRequest.cs
DataVisualiser/Core/Orchestration/ChartUpdateCoordinator.cs
DataVisualiser/Core/Orchestration/TransformChartRenderInvoker.cs
DataVisualiser/Core/Rendering/Contracts/Transform/TransformRenderingTypes.cs
DataVisualiser/VNext/Contracts/ChartRenderPlanConsumptionContractMetadata.cs
```

Contract changes:

```text
TransformVNextConsumptionContractBuilder builds a VNextUiConsumptionContract from the Transform capability contract and concrete render plan.
ChartUpdateRequest now accepts an optional render-plan consumption-contract factory.
ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId when such a factory is supplied.
TransformChartRenderInvoker supplies the Transform factory while preserving the existing Transform rendering behavior.
```

Focused tests:

```text
TransformRenderingContractTests.TransformVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
ChartUpdateCoordinatorTests.RenderTransformChartAsync_ShouldCaptureRenderPlanDiagnostics
ArchitectureGuardrailTests.TransformFamilyMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-173257.json
Transform panel visible: true
unary Transform operation rendered successfully: Log, 141 points
binary Transform operation rendered successfully: Add, 139 points
latest Transform render plan used LiveChartsWpf
render-plan history included unary and binary Transform plans
latest Transform render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
parity summary was unavailable because the final exported selection no longer had a reusable chart context
```

## SyncfusionSunburst Slice

Selected family:

```text
SyncfusionSunburst
```

Selection reason:

```text
SyncfusionSunburst had a dedicated rendering contract, existing hierarchy render-plan builder, and focused tests.
After four LiveCharts-backed family slices, it is the smallest remaining non-Cartesian family and can prove the same consumption-contract metadata shape across hierarchy delivery without touching shared Cartesian paths.
```

Production changes:

```text
DataVisualiser/Core/Rendering/Contracts/Syncfusion/SyncfusionSunburstRenderingContract.cs
DataVisualiser/Core/Rendering/Contracts/Syncfusion/SyncfusionSunburstRenderingTypes.cs
```

Contract changes:

```text
SyncfusionSunburstChartRenderRequest now carries an optional VNextUiConsumptionContract.
SyncfusionSunburstRenderingContract builds a VNextUiConsumptionContract when one is not supplied.
SyncfusionSunburstRenderingContract attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId to the hierarchy render plan before delivery.
SyncfusionSunburst rendering behavior still delegates to SyncfusionSunburstRenderPlanAdapter.
```

Focused tests:

```text
SyncfusionSunburstRenderingContractTests.SyncfusionSunburstVNextConsumptionContractBuilder_ShouldWrapRenderPlanAndPreserveMetadata
SyncfusionSunburstRenderingContractTests.RenderAsync_ShouldAttachVNextConsumptionMetadata
ArchitectureGuardrailTests.SyncfusionSunburstFamilyMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-175856.json
Syncfusion export scope completed
SyncfusionSunburst rendered successfully multiple times
latest SyncfusionSunburst render plan used SyncfusionWpf.Hierarchy
latest SyncfusionSunburst render plan included 3 rendered series, 16 hierarchy nodes, and 12 rendered points
latest SyncfusionSunburst render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
parity summary completed and passed
```

## Main Slice

Selected family:

```text
Main
```

Selection reason:

```text
Main is the primary shared Cartesian path and the next remaining active production family after the dedicated family contracts were migrated and smoke confirmed.
It has broader blast radius than previous slices, so the migration keeps dynamic display-mode resolution intact and attaches consumption metadata only after concrete render-plan construction.
```

Production changes:

```text
DataVisualiser/Core/Rendering/Contracts/CartesianMetrics/CartesianMetricChartRenderingContract.cs
DataVisualiser/Core/Orchestration/MainChart/MainChartRenderInvocationStage.cs
```

Contract changes:

```text
CartesianMetricVNextConsumptionContractBuilder builds a VNextUiConsumptionContract from the concrete CartesianMetric render plan.
MainChartRenderInvocationStage supplies a render-plan consumption-contract factory to the shared ChartUpdateCoordinator path.
ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before LiveCharts delivery.
Main chart stacked/cumulative display-mode resolution remains render-plan driven rather than fixed by a static capability contract.
```

Focused tests:

```text
CartesianMetricChartRenderingContractTests.CartesianMetricVNextConsumptionContractBuilder_ShouldWrapMainRenderPlanAndPreserveMetadata
CartesianMetricChartRenderingContractTests.CartesianMetricVNextConsumptionContractBuilder_ShouldRejectNonCartesianMetricProgramKind
ChartUpdateCoordinatorTests.UpdateChartUsingStrategyAsync_WithConsumptionContractFactory_ShouldAttachVNextMetadata
ArchitectureGuardrailTests.MainFamilyMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-180931.json
Main chart visible: true
Main chart rendered successfully in regular and stacked modes
latest Main render plan used LiveChartsWpf
latest Main render plan reported stacked mode with one overlay
latest Main render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
parity summary was unavailable because the final exported selection no longer had a reusable chart context
```

## Secondary Cartesian Slice

Selected family:

```text
Normalized and Difference/Ratio
```

Selection reason:

```text
Normalized is the remaining active secondary Cartesian chart family.
Difference/Ratio shares the same secondary render path, and the user requested it be migrated at the same time.
Diff/Ratio UI usage remains caveated, but its latent render path now carries the same VNext consumption metadata when invoked.
```

Production changes:

```text
DataVisualiser/Core/Orchestration/SecondaryCharts/SecondaryMetricChartRenderInvocationStage.cs
```

Contract changes:

```text
SecondaryMetricChartRenderInvocationStage supplies a render-plan consumption-contract factory for Normalized, Difference, and Ratio routes.
ChartUpdateCoordinator attaches ConsumptionContractSignature, SurfaceKind, and SurfaceId before LiveCharts delivery.
Diff/Ratio active UI usage remains caveated, but the shared latent render path now follows the same contract metadata handoff.
```

Focused tests:

```text
ChartRenderingOrchestratorTests.RenderNormalizedChartAsync_ShouldUseNormalizedCutOver_AndRenderTrackedSeries
ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldUseRatioCutOver_WhenRatioModeSelected
ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldUseDifferenceCutOver_AndCaptureRenderPlan_WhenDifferenceModeSelected
ChartRenderingOrchestratorTests.RenderDiffRatioChartAsync_ShouldCaptureDifferenceAndRatioRenderPlans_WhenModeChanges
ArchitectureGuardrailTests.SecondaryCartesianFamiliesMigration_ShouldUseVNextConsumptionContractMetadata
```

Smoke evidence:

```text
documents/reachability-20260501-182337.json confirmed Normalized rendered through LiveChartsWpf with ConsumptionContractSignature, SurfaceKind, and SurfaceId metadata.
The export reported IsNormalizedVisible true, NormalizedParity passed, OverallPassed true, no recent UI smoke-check errors, and no missing render-plan vocabulary or provider plan kinds.
Diff/Ratio manual UI smoke was not available because Diff/Ratio is not wired to the current UI; focused automated tests cover Difference and Ratio render-path metadata when invoked.
```

## Current Deferrals

```text
No shared abstraction has been extracted yet.
Phase 30 is the earliest point for comparing common surface shape across migrated families.
Phase 29 should continue one family at a time after selecting the next remaining family.
```
