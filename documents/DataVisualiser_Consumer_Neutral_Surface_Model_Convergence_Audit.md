# DataVisualiser Consumer-Neutral Surface Model Convergence Audit

Phase: 30 - Elevate Consumer-Neutral Surface Model

Date: 2026-05-01

Purpose:

```text
Compare the migrated VNext-native consumption shapes and make the pre-delivery surface seam explicit across chart and non-chart consumers.
```

## Compared Inputs

```text
Distribution
WeekdayTrend
BarPie
Transform
SyncfusionSunburst
Main
Normalized
Difference/Ratio
Operation Chain
```

## Common Surface Shape

```text
ConsumerSurfaceModel is the required consumer-neutral surface reference before terminal delivery.
It carries:
- surface kind
- surface identity
- whether the surface requires a render plan
- optional render-plan kind for chart render-plan surfaces
- metadata copied from the surface source
```

## Chart Render-Plan Surface

```text
Chart render-plan surface output uses ConsumerSurfaceModelKind.ChartRenderPlan.
It preserves the render plan identity as SurfaceId.
It declares RenderPlanKind so provider qualification checks both delivery compatibility and surface shape compatibility.
It carries render-plan metadata forward under the VNextUiConsumptionContract Surface.* metadata namespace.
It does not carry concrete UI controls, WPF chart instances, LiveCharts types, Syncfusion types, or controller references.
```

## Derived Dataset Surface

```text
Derived dataset surface output uses ConsumerSurfaceModelKind.DerivedDataset.
It preserves derived dataset identities, operation signatures, and output count.
It does not require a render plan.
It is used by Operation Chain through VNextUiConsumptionContract before workbench delivery.
It keeps operation execution, trace, and evidence upstream of UI display.
```

## Family-Specific Extensions

```text
Chart families keep family-specific data in render-plan metadata, capability contracts, overlays, interactions, and delivery metadata.
Operation Chain keeps step trace, lossiness, reversibility, operation signatures, and derived dataset IDs in OperationChainResult and surface metadata.
No family-specific extension becomes a new shared mega-object.
```

## Qualification Rule

```text
VNextUiConsumptionContract now rejects providers that support the requested consumer/program delivery but do not support the surface render-plan kind.
This keeps qualification upstream of terminal delivery and prevents a provider from receiving an incompatible surface shape.
```

## Boundary Result

```text
No new mega-object replaces ChartDataContext.
SurfaceModel remains a consumer-neutral pre-delivery seam.
Consumers receive meaning through contract, surface, provenance, provider, delivery, and metadata fields.
Consumers do not define canonical meaning.
Evidence can observe the surface contract but does not control live behavior.
```

## Test Evidence

```text
VNextUiConsumptionContractTests.FromRenderPlan_ShouldPreserveProgramDeliveryProviderProvenanceAndSurfaceMetadata
VNextUiConsumptionContractTests.FromRenderPlan_ShouldRejectProviderPlanKindDrift
VNextUiConsumptionContractTests.SurfaceModel_ShouldExposeCommonShapeForChartAndDerivedDatasetConsumers
OperationChainExecutorTests.ExecuteAsync_ShouldPreserveProvenanceTraceEvidenceAndConsumptionContractMetadata
ArchitectureGuardrailTests.ConsumerNeutralSurfaceModel_ShouldRemainRequiredPreDeliverySeam
```
