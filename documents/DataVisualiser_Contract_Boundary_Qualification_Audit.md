# DataVisualiser Contract, Boundary, and Qualification Audit

Recorded: 2026-04-28

Phase: 7 - Harden Contract / Boundary / Qualification Seam

## Scope

This audit inspects whether downstream delivery fan-out is contract-bound, provider-qualified, backend-safe, binding-explicit, and metadata-preserving.

Inputs inspected:

- `DataVisualiser/VNext/Contracts/ConsumerDeliveryContract.cs`
- `DataVisualiser/VNext/Contracts/ConsumerProviderContract.cs`
- `DataVisualiser/VNext/Contracts/ConsumerProviderRegistry.cs`
- `DataVisualiser/VNext/Contracts/ConsumerProviderContracts.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlan.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderDeliveryBinding.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanProviderMetadata.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanVocabularyMetadata.cs`
- `DataVisualiser/VNext/Rendering/ChartBackendSelector.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanAdapterQualification.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanAdapterDispatcher.cs`
- `DataVisualiser/VNext/Application/AnalyticalRenderPlanPipeline.cs`
- `DataVisualiser.Tests/VNext/ConsumerProviderRegistryTests.cs`
- `DataVisualiser.Tests/VNext/ChartRenderPlanProjectorTests.cs`
- `DataVisualiser.Tests/VNext/ChartRenderPlanAdapterTests.cs`
- `DataVisualiser.Tests/VNext/AnalyticalRenderPlanPipelineTests.cs`

## Seam Shape

The downstream fan-out path is:

```text
AnalyticalIntent
-> ConsumerDeliveryContract
-> ConsumerProviderRegistry
-> ChartRenderDeliveryBinding
-> ChartRenderPlanProjector
-> ChartRenderPlan metadata
-> ChartRenderPlanAdapterQualificationRules
-> ChartRenderPlanAdapterDispatcher
-> adapter result
```

The important separation is that delivery intent, provider resolution, backend qualification, projection, and adapter dispatch are explicit objects rather than hidden switch logic inside UI or legacy hubs.

## Existing Coverage

Current tests already verify:

- built-in provider resolution for LiveCharts cartesian/faceted delivery
- built-in provider resolution for Syncfusion hierarchy delivery
- export/API-style non-rendering provider resolution
- rejection when no provider supports a requested plan kind
- duplicate provider key rejection
- custom third-party provider support
- provider metadata attachment
- provider metadata absence for unsupported coverage
- delivery binding provider/backend resolution
- delivery binding rejection for provider/backend mismatch
- backend capability differences between LiveCharts and Syncfusion
- backend selector honoring provider metadata
- backend selector rejecting provider/backend conflicts
- adapter qualification requiring matching provider and backend metadata
- dispatcher failure when no adapter is qualified
- pipeline rejection when a non-rendering consumer requests a render plan
- pipeline rejection when delivery has no provider
- pipeline provider/backend metadata attachment on render plans

## Phase 7 Test Addition

Phase 7 adds:

- `RenderDeliveryBinding_ShouldPreserveExistingSemanticMetadataWhenAttached`

This proves that applying a delivery binding adds provider/backend metadata without overwriting existing intent, provenance, consumer, delivery, capability, or composition metadata.

## Findings

Provider, backend, delivery, and adapter qualification are already centralized in the VNext contract/rendering seam.

The seam is currently strong enough for Phase 7 because:

- provider policy lives in `ConsumerProviderRegistry` and `ConsumerProviderContract`
- backend selection and compatibility live in backend capability/selector structures
- delivery binding is inspectable through `ChartRenderDeliveryBinding`
- render-plan metadata is attached before dispatch
- adapters must pass `ChartRenderPlanAdapterQualificationRules`
- Core/UI guardrails prevent `ConsumerProviderRegistry`, `ChartRenderDeliveryBinding`, and `ChartBackendSelector` from moving into legacy hubs

The remaining risk is transitional use of render-plan projection and dispatch inside old rendering contracts and `ChartUpdateCoordinator`. Phase 6 already capped those hubs; later projection/translation phases should continue reducing hidden route policy in transitional paths.
