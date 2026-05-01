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
| Transform | Pending | Existing capability contract and render-plan path | Candidate after chart-family migration pattern is stable. |
| SyncfusionSunburst | Pending | Existing hierarchy render-plan path | Vendor/hierarchy-specific; defer until more LiveCharts families prove the common shape. |
| Main | Pending | Shared Cartesian primary chart path | Broader blast radius; defer until smaller families prove convergence. |
| Normalized | Pending | Shared Cartesian secondary chart path | Shares broader secondary-metric rendering path. |
| Difference/Ratio | Pending | Shared Cartesian secondary chart path; latent UI usage caveat | User noted Diff/Ratio is not currently wired as an active UI capability. |

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

## Current Deferrals

```text
No shared abstraction has been extracted yet.
Phase 30 is the earliest point for comparing common surface shape across migrated families.
Phase 29 should continue one family at a time after selecting the next remaining family.
```
