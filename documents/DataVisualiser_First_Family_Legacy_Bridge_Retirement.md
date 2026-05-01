# DataVisualiser First Family Legacy Bridge Retirement

Phase: 28 - Retire Corresponding Legacy Bridge for First Migrated Family

Date: 2026-05-01

Selected family:

```text
Distribution
```

Retired bridge path:

```text
DistributionRenderingContract -> ChartRenderingOrchestrator.RenderDistributionChartAsync
```

Retirement summary:

```text
The Distribution rendering contract no longer accepts or stores a ChartRenderingOrchestrator provider.
The Cartesian Distribution render path no longer branches back to the legacy orchestrator.
The Distribution native-consumption switch was removed because the native path is now the only contract path.
Distribution rendering continues to build a VNextUiConsumptionContract and attach surface metadata before delivery.
```

Production files changed:

```text
DataVisualiser/Core/Rendering/Contracts/Distribution/DistributionRenderingContract.cs
DataVisualiser/UI/Charts/Presentation/ChartControllerFactory.cs
DataVisualiser/UI/Charts/Presentation/DistributionChartControllerAdapter.cs
```

Behavior preserved:

```text
Distribution still renders through the Distribution rendering contract.
Cartesian rendering still uses the selected Distribution service directly.
Polar fallback still uses DistributionPolarRenderingService.
Capability, delivery, provider, vocabulary, provenance, and surface metadata remain attached to the render result.
Other chart-family bridge paths were not changed.
```

Evidence used before retirement:

```text
Phase 27 focused validation:
- Distribution-focused test filter passed 63 tests
- Architecture/evidence export focused filter passed 128 tests
- DataVisualiser.Tests passed 1012 tests
- DataFileReader.Tests passed 15 tests

Manual smoke evidence:
- documents/reachability-20260501-083930.json
- Distribution visible
- weekly and hourly Distribution parity passed
- Distribution mode/settings/chart-type milestones recorded
- latest Distribution render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
- no recent UI smoke-check errors recorded
```

Post-retirement manual smoke evidence:

```text
documents/reachability-20260501-114754.json
Distribution visible: true
Distribution runtime path milestones: VNextDistribution
Distribution mode/settings/subtype/chart-type milestones recorded
Distribution parity completed with weekly and hourly parity passed
latest Distribution render plan used LiveChartsWpf.Cartesian
latest Distribution render plan carried ConsumptionContractSignature, SurfaceKind, and SurfaceId
render-plan vocabulary reported no missing vocabulary or provider plan kinds
no recent UI smoke-check errors were recorded
```

Bridge-retirement proof:

```text
DistributionRenderingContract has no ChartRenderingOrchestrator dependency.
DistributionRenderingContract no longer calls RenderDistributionChartAsync on a legacy orchestrator.
DistributionChartControllerAdapter no longer passes a compatibility switch.
Distribution VNext consumption metadata is unconditional for this family.
```

Remaining intentional debt:

```text
ChartRenderingOrchestrator still contains legacy Distribution methods because older orchestration tests and non-family-specific orchestration behavior still cover that broader type.
ChartDataContext still exists in the Distribution render request as compatibility input inherited from the current UI/controller boundary.
Removing that broader context dependency belongs to later surface-model/UI-consumption phases, not this bridge retirement.
```
