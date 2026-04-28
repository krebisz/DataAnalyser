# DataVisualiser Integration Hub Containment Audit

Recorded: 2026-04-28

Phase: 6 - Cap Integration Hubs

## Scope

This audit inspects whether old integration hubs are acting as coordinators or absorbing target-architecture responsibilities that should belong to authority, capability, provider, evidence, delivery, or rendering seams.

Inputs inspected:

- `DataVisualiser/Core/Orchestration/ChartUpdateCoordinator.cs`
- `DataVisualiser/Core/Orchestration/ChartRenderingOrchestrator.cs`
- `DataVisualiser/UI/ViewModels/MetricLoadCoordinator.cs`
- `DataVisualiser/UI/Charts/Presentation/ChartControllerFactory.cs`
- chart-family adapters under `DataVisualiser/UI/Charts/Presentation`
- existing architecture guardrails in `DataVisualiser.Tests/Architecture/ArchitectureGuardrailTests.cs`
- existing orchestration, view-model, and presentation tests covering these hubs

## Hub Responsibility Map

### ChartControllerFactory

Current responsibilities:

- composition of chart-family adapters
- construction of rendering contracts and invokers
- event binding from controllers to adapters
- registration of adapters in `ChartControllerRegistry`

Responsibilities it should keep:

- composition only
- controller-to-adapter wiring
- factory result assembly

Responsibilities it should not acquire:

- provider selection policy
- backend selection policy
- evidence/export policy
- analytical intent or confidence authority
- chart computation semantics

Current assessment:

`ChartControllerFactory` is dense but currently composition-oriented. It creates adapters and rendering contracts, but provider/backend authority remains outside this factory. Existing guardrails already prevent provider and evidence policy from being added to this file.

### ChartControllerFactoryContext

Current responsibilities:

- constructor-time dependency carrier for chart-family composition
- delegates access to existing coordinators, services, controllers, tooltip providers, renderers, and surfaces

Responsibilities it should keep:

- composition context only
- dependency transport for the factory

Responsibilities it should not acquire:

- provider/evidence bags
- runtime policy state
- analytical result or provenance authority

Current assessment:

The context is an envelope-like composition carrier, not a policy object. Existing architecture tests already guard it from accumulating provider or evidence authority.

### ChartUpdateCoordinator

Current responsibilities:

- compute a chart strategy
- build `ChartRenderModel`
- optionally build/apply a VNext `ChartRenderPlan`
- render through the legacy render engine or render-plan adapter dispatcher
- synchronize chart timestamps and tooltips
- normalize Y-axis after rendering
- expose last render-plan adapter diagnostics for callers

Responsibilities it should keep:

- chart update coordination
- render invocation
- render-model-to-render-plan bridging while legacy coexistence remains active
- post-render chart surface synchronization

Responsibilities it should not acquire:

- analytical intent construction
- confidence or interpretation policy
- evidence export policy
- provider/backend registry policy
- capability selection authority

Current assessment:

`ChartUpdateCoordinator` remains a high-density transitional bridge. It still contains render-plan construction from legacy `ChartRenderModel`, but it delegates cumulative series, Y-axis preparation, vocabulary metadata, and adapter qualification. Existing architecture tests already prevent it from acquiring several semantic/provider/evidence authority types.

### ChartRenderingOrchestrator

Current responsibilities:

- route chart rendering across main, normalized, diff/ratio, and distribution families
- construct family orchestration pipelines
- call family-specific render methods based on visibility and route state
- capture render-plan adapter diagnostics from `ChartUpdateCoordinator`

Responsibilities it should keep:

- render orchestration and routing
- family pipeline construction
- render diagnostics handoff to `ChartState`

Responsibilities it should not acquire:

- provider/backend selection policy
- evidence export or diagnostics construction
- analytical intent, confidence, or interpretation authority
- render-plan projection or adapter dispatch ownership

Current assessment:

`ChartRenderingOrchestrator` is coordination-oriented. Its main risk is becoming the natural place to add new family policy because it already sees all major chart families. Phase 6 guardrails should explicitly keep provider policy, analytical authority, and evidence construction out of this class.

### MetricLoadCoordinator

Current responsibilities:

- load metric types, subtypes, date ranges, and metric data
- validate load prerequisites
- choose VNext main-family path or legacy fallback path
- build legacy `ChartDataContext`
- record load runtimes and timing evidence in `ChartState`
- preserve VNext family runtime state for downstream chart families

Responsibilities it should keep for now:

- load coordination
- validation coordination
- VNext/legacy coexistence routing while migration is active
- runtime evidence recording as an observational bridge

Responsibilities it should not acquire:

- provider/backend delivery policy
- chart render-plan projection or adapter dispatch
- confidence/interpretation policy
- direct chart rendering responsibilities

Current assessment:

`MetricLoadCoordinator` is the most important transitional bridge in Phase 6. It currently owns coexistence routing and runtime bookkeeping. That is acceptable for now because it is part of migration containment, but it should not absorb render delivery, provider selection, or interpretation responsibilities.

### Chart-Family Adapters

Current responsibilities:

- translate controller events into chart-family requests
- call chart-family rendering contracts or VNext data-resolution helpers
- update family-specific runtime/render diagnostics on `ChartState`
- coordinate busy scopes and view-model state around UI interactions

Responsibilities they should keep:

- UI event adaptation
- chart-family request assembly
- family-specific interaction wiring
- family runtime handoff to shared state

Responsibilities they should not acquire:

- provider/backend registry policy
- analytical intent authority
- confidence or interpretation policy
- evidence export ownership
- render-plan adapter dispatch ownership

Current assessment:

Adapters remain family-specific presentation bridges. Several use VNext data-resolution helpers and record runtime/render diagnostics. That is acceptable as transitional UI integration, but provider policy and analytical authority should remain outside adapters.

## Existing Guardrails

Existing tests already protect several containment boundaries:

- `ChartUpdateCoordinator_ShouldNotAcquireSemanticProviderOrEvidenceAuthority`
- `ChartControllerFactoryContext_ShouldRemainCompositionContextNotProviderOrEvidenceBag`
- `ChartUpdateCoordinator_ShouldDelegateCumulativeAndYAxisPreparationToDedicatedHelpers`
- `CoreAndUi_ShouldNotOwnProviderDeliveryPolicy`
- `CoreOrchestrationAndMainHost_ShouldNotReferenceSyncfusionOrLiveChartsCore`
- render-plan metadata preservation tests

## Phase 6 Guardrail Additions

Phase 6 adds static guardrails to prevent:

- `ChartRenderingOrchestrator` from acquiring provider/backend/evidence/analytical authority
- `MetricLoadCoordinator` from acquiring render delivery, provider/backend, confidence, or interpretation policy
- chart-family adapters from acquiring provider/backend policy, analytical authority, evidence export ownership, or render-plan adapter dispatch ownership

## Findings

Phase 6 confirms the old hubs are still active, but they are mostly acting as coordination and transitional bridge points rather than new homes for target-architecture authority.

The highest-risk hub is `MetricLoadCoordinator`, because it owns VNext/legacy route selection and runtime evidence recording during coexistence. The next highest-risk hub is `ChartRenderingOrchestrator`, because it sees all major chart families and could easily absorb new family policy.

No production move is justified yet. The appropriate Phase 6 work is to document responsibilities and add guardrails that stop these hubs from accumulating new authority while later phases harden contract, boundary, qualification, projection, and delivery seams.
