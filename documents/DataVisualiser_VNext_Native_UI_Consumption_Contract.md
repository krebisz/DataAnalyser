# DataVisualiser VNext Native UI Consumption Contract

Recorded: 2026-04-30

Phase: 25 - Define VNext-Native UI Consumption Contract

## Purpose

This document defines the first production contract shape that UI-facing consumption can move toward instead of using `ChartDataContext` as the semantic carrier.

The contract is implemented by:

```text
DataVisualiser/VNext/Contracts/VNextUiConsumptionContract.cs
DataVisualiser/VNext/Contracts/ConsumerSurfaceModel.cs
```

## Contract Role

`VNextUiConsumptionContract` is:

```text
consumer-facing
metadata-preserving
surface-ready
non-authoritative
delivery-neutral
provenance-preserving
interaction-aware without owning interaction policy
```

It is not a rendering backend, controller adapter, ViewModel, or chart-family implementation.

## Preserved Metadata

The contract preserves:

```text
program kind
capability kind
composition kind
consumer kind
delivery target
render-plan requirement flag
provider key
provider display name
provider signature
source signature
intent signature
provenance signature
overlay signatures
interaction signatures
delivery metadata
provider metadata
surface metadata
explicit contract metadata
```

## Surface Model

`ConsumerSurfaceModel` is the consumer-neutral surface reference.

It can represent:

```text
no surface
chart render plan
tabular data
evidence
derived dataset
other surface
```

Existing `ChartRenderPlan` output participates by being wrapped as a `ConsumerSurfaceModelKind.ChartRenderPlan` reference. The contract stores the surface identity and metadata, not a concrete UI control, vendor backend, or controller.

Non-chart consumers can use `ConsumerSurfaceModel.None` or another non-render-plan surface kind without requiring a render plan.

## Drift Rules

The contract rejects:

```text
delivery program kind drift
provider/delivery support drift
render-plan surface attached to non-render-plan delivery
missing source, intent, or provenance signatures
render-plan program kind drift
```

## Migration Use

Phase 25 does not retire `ChartDataContext`.

It supplies the replacement consumption shape required before Phase 26 and before any chart-family migration can safely make UI consumption VNext-native.

Bridge retirement remains gated by:

```text
family-specific migration
parity evidence
smoke evidence
metadata preservation evidence
provenance preservation evidence
manual integrity checks where UI behavior is affected
```
