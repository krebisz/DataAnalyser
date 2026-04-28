# DataVisualiser Consumer and Interaction Containment Audit

Recorded: 2026-04-28

Phase: 9 - Thin Consumer / Interaction Layer

## Scope

This audit inspects consumer and interaction paths to confirm they receive output and relay behavior without redefining authority, intent, provider policy, capability planning, or analytical meaning.

Inputs inspected:

- chart controllers under `DataVisualiser/UI/Charts/Controllers`
- controller adapters under `DataVisualiser/UI/Charts/Presentation`
- tooltip factories and helpers under `DataVisualiser/UI/Charts/Interaction`
- timestamp sink contract under `DataVisualiser/Core/Rendering/Interaction`
- tooltip formatting helpers under `DataVisualiser/Core/Rendering/Tooltip`
- event binders under `DataVisualiser/UI/MainHost/Coordination`
- UI state helpers under `DataVisualiser/UI/State`
- view-model state helpers under `DataVisualiser/UI/ViewModels`
- existing adapter, interaction, event-binder, state, and architecture tests

## Classification

### Chart Controllers

Chart controllers are UI event sources and surface controls. They expose toggles, selections, display modes, and user actions.

They should remain non-authoritative consumers. They must not choose provider/backend policy, create analytical intent, or define confidence/interpretation behavior.

### Controller Adapters

Controller adapters translate controller events into chart-family requests and call rendering/data-resolution contracts.

They may:

- adapt UI events
- assemble family-specific requests
- coordinate busy scopes
- update chart-family runtime handoff state

They must not:

- own provider/backend registry policy
- construct analytical intent or capability authority
- own confidence/interpretation policy
- own evidence export policy

Phase 6 and Phase 8 already added broad adapter containment guardrails. Phase 9 carries those findings forward.

### Tooltip and Interaction Helpers

Tooltip and interaction helpers format displayed values, track hover state, draw visual indicators, and relay timestamp information.

They should remain terminal interaction helpers. They must not interpret analytical confidence, mutate canonical results, or select providers/backends.

### Timestamp Sinks

`IChartTimestampSink` is a relay contract from rendering coordination into tooltip state. It carries chart/timestamp updates only.

It should not own analytical authority or source/provenance decisions.

### Event Binders

Event binders attach and detach event handlers. Their responsibility is lifecycle-safe subscription wiring.

They should not contain route, provider, capability, or evidence policy.

### UI State Helpers

UI state helpers track visibility, busy state, display modes, and selected options.

They should not define canonical analytical meaning. Runtime/evidence fields on shared state remain transitional observation and are not a license for state helpers to become authority or provider policy.

## Existing Evidence

Existing tests cover:

- controller factory composition and event binding
- chart controller registry behavior
- controller adapter event handling
- controller adapter lifecycle behavior
- tooltip formatting and participation helpers
- event binder bind/unbind behavior
- busy state and UI state behavior
- architecture guardrails for chart-family adapters and integration hubs

## Phase 9 Guardrail Additions

Phase 9 adds static architecture guardrails:

- `ChartControllersAndInteractions_ShouldRemainNonAuthoritativeConsumers`
- `EventBindersAndUiStateHelpers_ShouldNotOwnAnalyticalOrProviderPolicy`
- `ViewModelStateHelpers_ShouldNotConstructAnalyticalMeaning`

These checks prevent consumer/interaction targets from acquiring provider policy, delivery binding policy, backend selection policy, analytical intent construction, reasoning coordination, confidence policy, interpretation policy, render-plan projection, adapter dispatch, or evidence export ownership.

## Findings

No production code movement is justified in Phase 9.

The consumer and interaction layer is not clean in the sense of being small; it remains broad because the UI still carries many transitional family paths. However, the inspected consumer and interaction targets are currently acting as event, display, state, and interaction relays rather than authority owners.

The main carry-forward risk is still chart-family adapters because they sit between UI events, VNext data resolution, legacy rendering contracts, and runtime diagnostics. That risk is already guarded by Phase 6 and Phase 8 checks and should remain visible when later surface-model and terminal-delivery phases are executed.
