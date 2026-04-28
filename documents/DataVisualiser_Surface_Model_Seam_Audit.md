# DataVisualiser Surface Model Seam Audit

Recorded: 2026-04-28

Phase: 10 - Elevate SurfaceModel Seam

## Scope

This audit classifies current render, UI, delivery, and surface models and checks whether a consumer-neutral model exists before terminal delivery.

Inputs inspected:

- `DataVisualiser/VNext/Rendering/ChartRenderPlan.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanProjector.cs`
- `DataVisualiser/Core/Rendering/Contracts/ChartRenderModel.cs`
- render request/host/surface records under `DataVisualiser/Core/Rendering/Contracts`
- render-plan adapters under `DataVisualiser/Core/Rendering`
- UI presentation models under `DataVisualiser/UI/Charts/Presentation`
- UI renderers under `DataVisualiser/UI/Charts/Presentation/LiveCharts` and `DataVisualiser/UI/Charts/Presentation/ECharts`
- existing VNext, rendering, presentation, and architecture tests

## Model Classification

### Consumer-Neutral Surface Model

`ChartRenderPlan` is the current neutral surface model.

It carries:

- program kind
- render-plan kind
- display mode
- source signature
- series plans
- hierarchy plans
- render density
- interaction capabilities
- metadata
- overlay series

It does not reference WPF, LiveCharts, Syncfusion, WebView, or UI presentation types. This makes it the strongest current bridge before terminal delivery.

### Legacy Render Model

`ChartRenderModel` is the current legacy render model.

It carries chart-ready primary/secondary series, timestamps, labels, colors, display metadata, operation flags, multi-series data, and overlay series. It is not fully consumer-neutral because it uses WPF `Color` and exists close to the LiveCharts render engine path.

This is acceptable as a legacy bridge, but it should not become the target surface seam.

### UI Presentation Models

`UiChartRenderModel`, `ChartSeriesModel`, `ChartFacetModel`, `ChartAxisModel`, `ChartLegendModel`, `ChartOverlayModel`, and `ChartInteractionModel` are UI presentation models.

They are upstream of concrete renderer lifecycle and currently avoid concrete chart controls. They can be consumed by renderer implementations such as LiveCharts and the ECharts placeholder renderer.

### Delivery Models and Surfaces

Delivery models include render-plan adapter surfaces and chart-family render request/host/surface records.

Examples:

- `LiveChartsRenderSurface`
- `UiChartRenderSurface`
- `DistributionRenderSurface`
- `WeekdayTrendRenderSurface`
- `SyncfusionSunburstRenderSurface`
- chart-family render request and host records

These are downstream delivery structures and are allowed to contain concrete renderer or host details. They should remain terminal and replaceable rather than becoming analytical or semantic authority.

## Existing Evidence

Existing tests already cover:

- VNext render-plan projection without vendor types
- render-plan metadata preservation through adapters
- adapter qualification before delivery
- UI renderers consuming `UiChartRenderModel`
- chart panel surface behavior
- surface diagnostics reading
- controller adapter and rendering contract behavior

## Phase 10 Guardrail Additions

Phase 10 adds static architecture guardrails:

- `ChartRenderPlan_ShouldRemainConsumerNeutralSurfaceModel`
- `SurfaceModels_ShouldNotAcquireSemanticProviderOrEvidenceAuthority`
- `UiRenderModels_ShouldStayUpstreamOfConcreteRendererLifecycle`

These checks keep `ChartRenderPlan` vendor-neutral, prevent surface models from acquiring authority/provider/evidence responsibilities, and keep UI render models upstream of concrete renderer lifecycle types.

## Findings

The consumer-neutral surface seam already exists in `ChartRenderPlan`.

No production refactor is justified in Phase 10. The current target should be to keep `ChartRenderPlan` neutral and metadata-preserving while later phases continue demoting terminal delivery.

Known risks carried forward:

- `ChartRenderModel` is legacy and not fully neutral because it contains WPF color assumptions.
- chart-family render surfaces and hosts are concrete delivery structures and should remain terminal.
- UI presentation models are useful but should not replace `ChartRenderPlan` as the architectural surface seam.
