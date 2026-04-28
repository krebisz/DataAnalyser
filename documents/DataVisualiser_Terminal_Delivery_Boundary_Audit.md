# DataVisualiser Terminal Delivery Boundary Audit

Recorded: 2026-04-28

Phase: 11 - Demote Terminal Delivery

## Scope

This audit identifies terminal rendering, backend, vendor, host, and lifecycle boundaries and checks that they stay downstream, replaceable, and semantically non-authoritative.

Inputs inspected:

- render engines under `DataVisualiser/Core/Rendering/Engines`
- render adapters under `DataVisualiser/Core/Rendering/Adapters`
- chart-family render adapters under `DataVisualiser/Core/Rendering`
- backend capability/selector types under `DataVisualiser/VNext/Rendering`
- Syncfusion render contracts under `DataVisualiser/Core/Rendering/Contracts/Syncfusion`
- Syncfusion delivery adapter under `DataVisualiser/Core/Rendering/Syncfusion`
- LiveCharts renderer under `DataVisualiser/UI/Charts/Presentation/LiveCharts`
- ECharts placeholder renderer/surface under `DataVisualiser/UI/Charts/Presentation/ECharts`
- render host/lifecycle helpers under `DataVisualiser/UI/Charts/Presentation`
- existing VNext, rendering, UI, and architecture tests

## Boundary Classification

### Upstream Rendering Contracts

`ChartRenderPlan`, `ChartBackendCapabilities`, `ChartBackendCandidateSet`, and `ChartBackendSelector` are upstream of terminal delivery. They describe render shape and backend capability, but they should not import concrete vendor libraries or UI frameworks.

`ChartBackendCapabilities` currently names built-in backends by key, but it does not depend on vendor assemblies or UI types. That is acceptable because the names are contract metadata, not terminal implementation dependencies.

### Terminal Render Engines

`ChartRenderEngine`, frequency renderers, shading renderers, and distribution render services are terminal or legacy terminal rendering structures.

They may contain concrete rendering framework details. They should not construct analytical intent, own confidence/interpretation policy, select providers, or write evidence.

### Render Adapters

Render-plan adapters consume `ChartRenderPlan` or downstream surface models and apply them to concrete surfaces.

Examples:

- `LiveChartsRenderPlanAdapter`
- `UiChartRenderPlanAdapter`
- `DistributionRenderPlanAdapter`
- `WeekdayTrendRenderPlanAdapter`
- `SyncfusionSunburstRenderPlanAdapter`

They should remain delivery adapters and preserve metadata in adapter results. They should not resolve providers or own analytical authority.

### Vendor-Specific Delivery

Vendor-specific delivery currently exists in:

- LiveCharts renderer/engine paths
- Syncfusion sunburst controller/view/render adapter paths
- ECharts placeholder renderer/surface paths

These are valid terminal boundaries. Vendor code can own vendor lifecycle, host controls, and final rendering, but it should not define upstream analytical meaning.

The Syncfusion view currently composes the evidence export service. That is a view-level composition concern and is intentionally carried into Phase 12, where evidence paths are audited directly.

### Host and Lifecycle Helpers

`RenderingHostLifecycleAdapterHelper` and renderer surfaces are terminal wiring helpers. They should remain generic lifecycle/action wrappers and not become policy engines.

## Existing Evidence

Existing tests already cover:

- VNext render-plan projection without vendor types
- render-plan adapter qualification and dispatch
- provider/backend mismatch rejection
- LiveCharts renderer behavior
- chart renderer resolver behavior
- Syncfusion sunburst adapter behavior
- Syncfusion view/coordinator behavior
- surface-model neutrality from Phase 10

## Phase 11 Guardrail Additions

Phase 11 adds static architecture guardrails:

- `VNextSurfaceAndBackendContracts_ShouldNotImportConcreteVendorOrUiLibraries`
- `TerminalRenderDelivery_ShouldNotAcquireAnalyticalOrEvidenceAuthority`
- `RenderingHostLifecycleHelpers_ShouldRemainTerminalWiring`

These checks prevent upstream VNext surface/backend contracts from importing concrete vendor/UI libraries and prevent terminal rendering/delivery code from acquiring analytical, provider-resolution, interpretation, confidence, or evidence ownership.

## Findings

Terminal delivery is currently downstream and mostly replaceable.

No production refactor is justified in Phase 11. The right move is to preserve current boundaries with tests and defer broad render-family consolidation until repeated hardened slices prove a shared shape.

Known carry-forward risks:

- legacy LiveCharts rendering is still active and concrete
- Syncfusion view composition includes evidence export wiring that must be assessed in Phase 12
- ECharts is currently a placeholder seam rather than a full backend
- render-family consolidation should stay deferred until more than one hardened path proves the same terminal shape
