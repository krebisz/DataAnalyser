# DataVisualiser Rendering / Vendor / Delivery Demotion Audit

Phase: 32 - Demote Rendering / Backend / Vendor Fully

Date: 2026-05-02

Purpose:

```text
Confirm rendering, backend, vendor, host, and lifecycle concerns are terminal and replaceable for migrated production paths.
```

## Upstream Contract Boundary

```text
VNext contracts and application code remain free of concrete WPF, LiveCharts, Syncfusion, WebView, and UI controller dependencies.
ConsumerDeliveryContract, ConsumerProviderContract, VNextUiConsumptionContract, and ConsumerSurfaceModel describe delivery and compatibility without owning rendering lifecycle.
ConsumerSurfaceModel exposes RenderPlanKind only as neutral surface qualification data.
```

## Terminal Rendering Boundary

```text
LiveCharts and Syncfusion references are confined to rendering, orchestration delivery, UI presentation, and terminal interaction surfaces.
Terminal adapters consume ChartRenderPlan / surface output.
Terminal adapters do not define analytical intent, operation-chain execution, provenance authority, evidence export behavior, or canonical meaning.
```

## Delivery Qualification

```text
ConsumerProviderContract.Supports qualifies by consumer kind, program kind, and optional render-plan kind.
VNextUiConsumptionContract rejects providers that support the delivery target but not the surface render-plan kind.
ChartRenderPlanAdapterDispatcher qualifies render plans against adapter support before applying a backend adapter.
```

## Replaceability Evidence

```text
LiveChartsWpf is the default cartesian/faceted chart provider.
SyncfusionSunburst is a separate hierarchy provider for the SyncfusionSunburst family.
TabularSummaryChart is a non-default chart provider used by the MovingAverage proof path.
EvidenceExport and ApiResponse are non-chart providers that do not require render plans.
Operation Chain output is delivered through ConsumerDeliveryContract.Export and ConsumerProviderContracts.EvidenceExport without vendor-specific assumptions.
```

## Explicit Deferrals

```text
ChartRenderingOrchestrator and ChartUpdateCoordinator still accept LiveCharts CartesianChart because current WPF delivery is terminal and production-visible.
This is acceptable for Phase 32 because those classes sit at the orchestration/delivery edge, not inside VNext contracts or application semantics.
ChartRenderModel and selected rendering contracts remain terminal-adjacent and still carry WPF-facing shape such as System.Windows.Media or control-level request parameters.
This is bounded as rendering contract/presentation delivery state, not VNext semantic authority.
Some core rendering contracts still reference UI presentation/state interfaces for current production delivery.
Some core rendering contracts still reference LiveCharts because current WPF delivery request objects carry terminal chart surfaces.
This is bounded as a Phase 33+ consolidation target and is not treated as a VNext upstream contract leak.
No attempt is made here to replace the WPF host or introduce a second live chart backend.
```

## Test Evidence

```text
ArchitectureGuardrailTests.RenderingVendorDelivery_ShouldRemainTerminalAndReplaceable
VNextUiConsumptionContractTests.FromRenderPlan_ShouldRejectProviderPlanKindDrift
ChartRenderPlanAdapterTests.Dispatcher_ShouldFailWhenProviderMetadataSelectsDifferentAdapter
Phase22MovingAverageEndToEndTests.MovingAverage_ChartConsumer_ShouldResolveTabularSummaryProviderWithoutExplicitBackend
OperationChainExecutorTests.ExecuteAsync_ShouldPreserveProvenanceTraceEvidenceAndConsumptionContractMetadata
```
