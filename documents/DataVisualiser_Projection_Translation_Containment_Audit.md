# DataVisualiser Projection and Translation Containment Audit

Recorded: 2026-04-28

Phase: 8 - Preserve Projection as Non-Authoritative Translation

## Scope

This audit classifies current projectors, adapters, resolvers, selectors, converters, and formatters that move data across VNext, Core, UI, and delivery paths. The goal is to confirm that translation roles move meaning across boundaries without creating authority, hiding provider policy, owning evidence policy, or mutating provenance/confidence.

Inputs inspected:

- `DataVisualiser/VNext/Rendering/ChartRenderPlanProjector.cs`
- `DataVisualiser/VNext/Application/LegacyChartProgramProjector.cs`
- `DataVisualiser/VNext/Rendering/ChartBackendSelector.cs`
- `DataVisualiser/VNext/Rendering/ChartRenderPlanAdapter.cs`
- render-plan adapters under `DataVisualiser/Core/Rendering`
- route resolvers under `DataVisualiser/Core/Rendering/Contracts`
- chart-family adapters and helpers under `DataVisualiser/UI/Charts/Presentation`
- formatter/converter helpers under `DataVisualiser/Core/Rendering` and `DataVisualiser/UI/Converters`
- existing VNext, rendering, and architecture tests

## Classification

### Projection

- `ChartRenderPlanProjector` projects canonical `ChartProgram` / `AnalyticalExecutionResult` values into render plans.
- `LegacyChartProgramProjector` projects VNext programs into legacy `ChartDataContext` for coexistence.
- `ChartRenderPlanVocabularyMetadata` projects vocabulary metadata into render-plan metadata dictionaries.
- `ChartRenderPlanProviderMetadata` attaches provider metadata from delivery/provider contracts.

Assessment:

These roles translate existing program, intent, provenance, and delivery context. They should not choose analytical meaning, confidence policy, evidence policy, or backend route policy.

### Adaptation

- `ChartRenderPlanAdapterDispatcher<TSurface>` selects a qualified adapter and delegates application.
- `IChartRenderPlanAdapter<TSurface>` implementations adapt neutral render plans to concrete rendering surfaces.
- chart-family controller adapters translate UI events into family requests.
- `MetricSeriesSelectionAdapterHelper` and related UI helpers adapt UI selections into request-friendly shapes.

Assessment:

Adapters may translate and apply already-decided output. They should not own provider registry policy, analytical intent construction, confidence/interpretation policy, or evidence export ownership.

### Selection

- `ChartBackendSelector` selects from backend candidates using plan kind and provider metadata.
- `ChartBackendCandidateSet` applies provider-qualified backend selection.
- strategy-selection stages select legacy rendering strategies for already-prepared requests.

Assessment:

Selection is acceptable where explicit candidates and metadata are inputs. Selection should remain inspectable and should not be hidden inside UI event handlers or rendering adapters.

### Resolution

- `ChartProgramDeliveryTargetResolver` resolves delivery targets from program kinds.
- `BarPieRenderingRouteResolver`, `DistributionRenderingRouteResolver`, and `WeekdayTrendRenderingRouteResolver` resolve small family route values from already-existing UI state.
- `ChartRendererResolver` resolves a UI renderer implementation.
- `VNextDataResolutionHelper` resolves VNext series data for transitional UI paths.

Assessment:

Resolvers are acceptable where they map existing state to an explicit route or target. They should not become provider/backend registries or semantic policy engines.

### Mapping / Conversion / Formatting

- `TransformSeriesOperationRequestMapper` maps UI transform choices into operation requests.
- UI converters map display values and bindings.
- tooltip and chart-label formatters format already-resolved values for presentation.
- `ParityResultAdapter` maps validation output into parity records.

Assessment:

Mapping and formatting roles are non-authoritative by design. They should not mutate provenance, confidence, or canonical result values.

## Existing Evidence

Existing tests already cover:

- render-plan projection preserving source identity and series shape
- density projection being explicit and annotated
- hierarchy projection without vendor types
- legacy projection preserving load request signature
- backend selector rejection for provider metadata conflicts
- adapter qualification rejection for mismatched provider/backend metadata
- dispatcher rejection when no adapter qualifies
- contract/binding metadata preservation

## Phase 8 Guardrail Additions

Phase 8 adds static architecture guardrails:

- `RenderPlanProjectors_ShouldRemainNonAuthoritativeTranslation`
- `RenderPlanAdapters_ShouldNotAcquireAuthorityPolicyOrEvidenceOwnership`
- `AdapterDispatcher_ShouldOnlyQualifyAndDispatchPlans`

These checks prevent projectors and render-plan adapters from acquiring delivery binding ownership, backend selection ownership, evidence export ownership, confidence/interpretation policy, or reasoning authority.

## Findings

No production code movement is justified in Phase 8. Current projection and adapter roles are mostly non-authoritative, and the known transitional risks are already documented:

- `LegacyChartProgramProjector` is intentionally lossy and transitional.
- `ChartRenderPlanProjector` attaches provider metadata through a metadata helper but does not own delivery binding or backend selection.
- `ChartRenderPlanAdapterDispatcher` qualifies and dispatches; it does not resolve providers or construct semantic context.
- Core render adapters apply render plans and preserve metadata; they do not own provider registry or evidence export policy.

The next migration pressure remains containment: later phases should continue thinning UI consumers and elevating neutral surface models, but Phase 8 does not require production refactoring.
