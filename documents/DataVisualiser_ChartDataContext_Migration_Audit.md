# DataVisualiser ChartDataContext Migration Audit

Recorded: 2026-04-30

Phase: 24 - Audit ChartDataContext Migration Path

## Scope

This audit maps current `ChartDataContext` usage before any retirement or replacement work. It treats `ChartDataContext` as the dominant remaining UI consumption model and classifies where it is still used by VNext, Core, UI, rendering, evidence, parity, tests, and legacy bridge paths.

No production code is retired in this phase.

## Reference Counts

Fresh reference search:

```text
Production files referencing ChartDataContext: 77
Test files referencing ChartDataContext: 52
Phase 24 audit artifact existed before this phase: no
```

Production references by area:

| Area | File count | Primary role |
|---|---:|---|
| `DataVisualiser/VNext` | 1 | legacy projection bridge |
| `DataVisualiser/Core/Orchestration` | 10 | context construction, legacy orchestration, render requests |
| `DataVisualiser/Core/Rendering` | 3 | rendering request compatibility |
| `DataVisualiser/Core/Services` | 1 | distribution strategy compatibility |
| `DataVisualiser/Core/Strategies` | 11 | CMS/legacy cutover and strategy factory inputs |
| `DataVisualiser/Core/Transforms` | 1 | transform label compatibility |
| `DataVisualiser/UI/Charts/Presentation` | 26 | primary chart-family consumption model |
| `DataVisualiser/UI/Charts/Syncfusion` | 2 | Syncfusion host consumption model |
| `DataVisualiser/UI/MainHost/Evidence` | 8 | evidence/parity snapshot input |
| `DataVisualiser/UI/MainHost/Coordination` | 6 | UI host routing and toggle coordination |
| `DataVisualiser/UI` other | 8 | event, state, and rendering-context adapter plumbing |

Test references by area:

| Area | File count | Primary role |
|---|---:|---|
| Architecture | 1 | static guardrails |
| VNext | 3 | bridge/projection/runtime tests |
| Orchestration | 6 | legacy orchestration behavior |
| Strategies / Services | 5 | strategy cutover/factory behavior |
| Core.Rendering | 5 | rendering request compatibility |
| UI.Charts.Presentation | 15 | chart-family adapter behavior |
| UI.MainHost | 14 | host/evidence/coordination behavior |
| UI.Syncfusion | 1 | Syncfusion host behavior |
| ViewModels / Workspace / Other | 3 | state/runtime diagnostics behavior |

## Production Inventory and Classification

### Definition and Construction

Files:

- `DataVisualiser/Core/Orchestration/ChartDataContext.cs`
- `DataVisualiser/Core/Orchestration/ChartDataContextBuilder.cs`
- `DataVisualiser/VNext/Application/LegacyChartProgramProjector.cs`
- `DataVisualiser/Core/Services/BaseDistributionService.cs`
- `DataVisualiser/UI/Charts/Presentation/BinaryMetricChartContextHelper.cs`
- `DataVisualiser/UI/Charts/Presentation/DistributionRenderInputBuilder.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformDataResolutionCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/WeekdayTrendComputationInvoker.cs`

Classification:

- construction
- legacy compatibility
- VNext bridge
- candidate for VNext-native replacement
- must remain temporarily

Target replacement condition:

Replace construction sites only after Phase 25 defines a VNext-native UI consumption contract that can carry raw/canonical series, program/capability metadata, delivery target, provenance, source/load signature, and interaction metadata without becoming chart-specific. `LegacyChartProgramProjector` must remain until production chart families no longer need `ChartDataContext` as their primary consumption shape.

### UI Host and State Consumption

Files:

- `DataVisualiser/UI/MainChartsView.xaml.cs`
- `DataVisualiser/UI/Charts/Syncfusion/SyncfusionChartsView.xaml.cs`
- `DataVisualiser/UI/ChartRenderingContextAdapter.cs`
- `DataVisualiser/UI/IChartRenderingContext.cs`
- `DataVisualiser/UI/Charts/Controllers/ChartPanelController.xaml.cs`
- `DataVisualiser/UI/Events/DataLoadedEventArgs.cs`
- `DataVisualiser/UI/State/ChartState.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewChartUpdateCoordinator.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewCmsToggleCoordinator.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewControllerExtrasCoordinator.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewDataLoadedCoordinator.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewToggleStateCoordinator.cs`
- `DataVisualiser/UI/MainHost/Coordination/MainChartsViewToggleStateEvaluator.cs`
- `DataVisualiser/UI/Charts/Syncfusion/SyncfusionChartsViewCoordinator.cs`

Classification:

- UI binding
- read-only consumption
- mutation through `ChartState.LastContext`
- fallback routing
- candidate for VNext-native replacement
- must remain temporarily

Target replacement condition:

These references should move behind a VNext-native UI consumption shape after Phase 25. Host state should eventually store a consumer-neutral loaded surface or UI consumption snapshot rather than a mutable `ChartDataContext`. Existing fallbacks and view restoration logic must stay until one production chart family proves the replacement path with parity, smoke, metadata, and provenance evidence.

### Chart-Family Adapter Consumption

Files:

- `DataVisualiser/UI/Charts/Presentation/IChartController.cs`
- `DataVisualiser/UI/Charts/Presentation/ChartControllerAdapterBase.cs`
- `DataVisualiser/UI/Charts/Presentation/MainChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/NormalizedChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/DiffRatioChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/DistributionChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/WeekdayTrendChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/BarPieChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/SyncfusionSunburstChartControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformDataPanelControllerAdapter.cs`
- `DataVisualiser/UI/Charts/Presentation/ITransformPanelControllerExtras.cs`
- `DataVisualiser/UI/Charts/Presentation/MetricSeriesSelectionAdapterHelper.cs`
- `DataVisualiser/UI/Charts/Presentation/ChartContextSelectionGuard.cs`
- `DataVisualiser/UI/Charts/Presentation/CartesianMetricOverlaySeriesBuilder.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformChartPresentationCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformExecutionModels.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformGridPresentationCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformOperationExecutionCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformOperationStateCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformRenderCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformWorkflowCoordinator.cs`

Classification:

- UI/controller/adapter primary consumed model
- read-only consumption
- construction of family-specific working contexts
- VNext bridge through family data resolution
- candidate for VNext-native replacement
- must remain temporarily

Target replacement condition:

This is the main Phase 27+ migration surface. The adapter interface cannot drop `ChartDataContext` until Phase 25 defines the replacement contract and Phase 27 proves one production chart family can render from it. Family helpers that currently build derived `ChartDataContext` instances should become translators from the new consumption contract into family render requests or `ChartRenderPlan`/surface-model inputs.

### VNext Bridge and Routing

Files:

- `DataVisualiser/UI/MainHost/VNextMainChartIntegrationCoordinator.cs`
- `DataVisualiser/UI/MainHost/VNextSeriesLoadCoordinator.cs`
- `DataVisualiser/UI/ViewModels/VNextMetricLoadRouter.cs`
- `DataVisualiser/UI/Charts/Presentation/VNextDataResolutionHelper.cs`

Classification:

- VNext bridge
- legacy compatibility
- fallback routing
- evidence/runtime recording
- candidate for VNext-native replacement
- must remain temporarily

Target replacement condition:

`VNextMainChartIntegrationCoordinator` and `VNextSeriesLoadCoordinator` should stop projecting VNext programs through `LegacyChartProgramProjector` only after the selected chart family consumes VNext-native output directly. `VNextDataResolutionHelper` remains a transitional helper until family-specific selected-series reloads can return a native consumption contract or typed series payload with metadata and runtime evidence.

### Core Orchestration and Rendering Compatibility

Files:

- `DataVisualiser/Core/Orchestration/ChartRenderingOrchestrator.cs`
- `DataVisualiser/Core/Orchestration/PrimaryChartRenderRequest.cs`
- `DataVisualiser/Core/Orchestration/SecondaryCharts/SecondaryMetricChartRenderRequest.cs`
- `DataVisualiser/Core/Orchestration/DistributionCharts/DistributionChartOrchestrationRequest.cs`
- `DataVisualiser/Core/Orchestration/TransformChartRenderInvoker.cs`
- `DataVisualiser/Core/Orchestration/MainChart/MainChartPreparationStage.cs`
- `DataVisualiser/Core/Orchestration/MainChart/MainChartPreparedData.cs`
- `DataVisualiser/Core/Rendering/Contracts/CartesianMetrics/CartesianMetricChartRenderingContract.cs`
- `DataVisualiser/Core/Rendering/Contracts/Distribution/DistributionRenderingContract.cs`
- `DataVisualiser/Core/Rendering/Contracts/Transform/TransformRenderingTypes.cs`

Classification:

- legacy compatibility
- read-only consumption
- construction of working contexts
- render request compatibility
- needs consumer contract shape
- needs surface model shape
- must remain temporarily

Target replacement condition:

Core orchestration references should shrink after UI families consume VNext-native contracts and terminal rendering requests no longer need `ChartDataContext` as a payload. Rendering contracts should receive metadata-preserving render/surface requests, not broad chart contexts. Existing request records may remain until the selected Phase 27 family proves equivalent render behavior.

### Strategy, CMS Cutover, Reachability, and Transform Helpers

Files:

- `DataVisualiser/Core/Orchestration/StrategySelectionService.cs`
- `DataVisualiser/Core/Strategies/Abstractions/IStrategyCutOverService.cs`
- `DataVisualiser/Core/Strategies/Abstractions/IStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/StrategyFactoryBase.cs`
- `DataVisualiser/Core/Strategies/Factories/DifferenceStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/RatioStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/NormalizedStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/MultiMetricStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/Factories/WeekdayTrendStrategyFactory.cs`
- `DataVisualiser/Core/Strategies/StrategyCutOverService.cs`
- `DataVisualiser/Core/Strategies/Reachability/StrategyCmsDecisionEvaluator.cs`
- `DataVisualiser/Core/Strategies/Reachability/StrategyReachabilityRecord.cs`
- `DataVisualiser/Core/Transforms/TransformExpressionEvaluator.cs`

Classification:

- legacy compatibility
- CMS/legacy policy input
- evidence/runtime recording
- candidate for strategy input contract
- must remain temporarily

Target replacement condition:

Strategy cutover and reachability should eventually consume an explicit strategy input/capability execution context instead of `ChartDataContext`. This cannot be done safely before Phase 25 because strategy input still needs access to canonical series, aligned raw data, metric identity, time range, source signatures, and fallback diagnostics.

### Evidence and Parity

Files:

- `DataVisualiser/UI/MainHost/Evidence/EvidenceDataResolutionHelper.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceDiagnosticsBuilder.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceDistributionParityEvaluator.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceMultiMetricParityEvaluator.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceParityBuilder.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceStrategyParityExecutor.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceTransformParityDataResolver.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceTransformParityEvaluator.cs`

Classification:

- evidence/runtime recording
- parity/evidence comparison
- read-only consumption
- construction of parity contexts
- test/parity/evidence only in intent but production evidence path in location
- must remain temporarily

Target replacement condition:

Evidence and parity paths should not lead the replacement. They should follow production consumption migration so they can compare old and new behavior. Keep these references until the selected migrated family has replacement evidence snapshots and parity exports that do not depend on `ChartDataContext` as the primary semantic model.

## Bridge Dependency Map

### `LegacyChartProgramProjector`

Production dependencies:

- `DataVisualiser/UI/MainHost/VNextMainChartIntegrationCoordinator.cs`
- `DataVisualiser/UI/MainHost/VNextSeriesLoadCoordinator.cs`
- `DataVisualiser/VNext/Application/LegacyChartProgramProjector.cs`

Test dependencies:

- `DataVisualiser.Tests/VNext/LegacyChartProgramProjectorTests.cs`
- `DataVisualiser.Tests/ViewModels/MetricLoadCoordinatorTests.cs`
- architecture guardrails

Classification:

- VNext-to-legacy projection bridge
- must remain temporarily

Retirement condition:

Retire only after main-family and selected-series family loads return a VNext-native UI consumption contract or surface model directly, with matching parity, smoke, metadata, and provenance evidence.

### `VNextDataResolutionHelper`

Production dependencies:

- `DataVisualiser/UI/Charts/Presentation/VNextDataResolutionHelper.cs`
- `DataVisualiser/UI/Charts/Presentation/DistributionRenderInputBuilder.cs`
- `DataVisualiser/UI/Charts/Presentation/TransformDataResolutionCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/WeekdayTrendComputationInvoker.cs`

Classification:

- selected-series VNext bridge
- fallback routing helper
- evidence/runtime recording helper
- must remain temporarily

Retirement condition:

Retire after the selected family can request VNext-native selected-series payloads through the Phase 25 contract without reconstructing `ChartDataContext` and without losing runtime evidence.

### `LegacyMetricViewGateway`

Production dependencies:

- `DataVisualiser/VNext/Application/ReasoningEngine.cs`
- `DataVisualiser/VNext/Application/ReasoningEngineFactory.cs`
- `DataVisualiser/VNext/Application/LegacyMetricViewGateway.cs`

Test dependencies:

- VNext engine, pipeline, MovingAverage, and helper tests

Classification:

- upstream data-load compatibility gateway
- not primarily a UI consumption blocker
- must remain temporarily

Retirement condition:

This gateway should not be retired as part of the first UI consumption migration. It can be revisited after Phase 35 when VNext loading has a non-legacy data gateway with equivalent provenance and canonical-series semantics.

### `VNextMetricLoadRouter`

Production dependencies:

- `DataVisualiser/UI/ViewModels/MetricLoadCoordinator.cs`
- `DataVisualiser/UI/ViewModels/VNextMetricLoadRouter.cs`

Classification:

- bounded VNext/legacy load router
- fallback routing
- evidence/runtime recording
- must remain temporarily

Retirement condition:

Retire only after the main-family load path and at least one production family no longer depend on `ChartDataContext` projection and after fallback behavior has replacement tests and smoke evidence.

## Target Replacement Classes

### 1. VNext-native UI consumption contract

Needed for:

- UI host/state
- chart-family adapters
- selected-series family reloads
- Phase 25 contract definition

Minimum content:

- program kind
- capability kind
- composition kind
- delivery target
- consumer kind
- provider key/signature
- intent/provenance signature
- source/load signature
- raw/canonical series payload
- time range
- display labels
- overlay/interaction metadata where applicable

### 2. Consumer-neutral surface model

Needed for:

- chart-family rendering requests
- non-chart consumers
- Phase 30 convergence

Likely base:

- `ChartRenderPlan` or a wrapper/successor that preserves render-plan metadata while avoiding chart-only semantics.

### 3. Strategy input contract

Needed for:

- `StrategyCutOverService`
- strategy factories
- reachability decision/evidence

Minimum content:

- canonical series
- raw series where still needed
- strategy type/capability
- metric identity
- time range
- source/provenance signatures
- CMS availability and fallback state

### 4. Evidence/parity snapshot input

Needed for:

- parity evaluators
- diagnostics builder
- export service

Minimum content:

- immutable snapshot of loaded data and metadata
- runtime path
- provider/backend/capability metadata
- source signatures
- enough legacy compatibility data to compare until legacy paths are retired

## Retirable Now

No production `ChartDataContext` dependency is classified as retirable now.

Reason:

```text
The Phase 25 replacement contract does not exist yet.
The first production family has not been migrated.
Parity/evidence paths still need the old shape to compare legacy and VNext behavior.
Fallback behavior still depends on bridge paths.
```

## Phase 25 Input

Phase 25 should define the VNext-native UI consumption contract before any family migration begins. The first implementation slice should prefer one active production family with bounded behavior and strong existing tests. The selected family must prove:

- it can consume the new contract without `ChartDataContext` as primary semantic model
- it still preserves metadata/provenance/runtime evidence
- existing UI behavior still works
- parity/evidence remains meaningful
- fallback behavior remains explicit where still needed

## Completion Statement

Phase 24 is complete when this audit is accepted as the retirement map. It does not authorize bridge retirement by itself.
