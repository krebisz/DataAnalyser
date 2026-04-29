# DataVisualiser Distribution Capability Slice Audit

Recorded: 2026-04-28

Phase: 14 - Resume Capability Expansion

Selected slice: active Distribution capability

## Scope

This audit records why Distribution was selected as the first Phase 14 capability slice and how the live implementation now carries the target-spine contract through the active Distribution rendering path without adding new user-visible behavior.

Distribution was selected because it is active, UI-reachable, analytically meaningful, and already crosses the main target seams: intent, capability, program, delivery contract, qualification, surface/render plan, terminal delivery, and evidence.

## Target Spine Mapping

Distribution maps to the Phase 14 path as follows:

```text
Intent
-> AnalyticalIntentFactory.Distribution / AnalyticalIntentFactory.Create
Capability
-> CapabilityRequest.FromProgramRequest(ChartProgramKind.Distribution)
Program
-> ChartProgramRequest.Distribution
Contract / Boundary
-> ConsumerDeliveryContract through ChartProgramDeliveryTargetResolver
Qualification
-> DistributionRenderingContract qualification matrix and route capabilities
Provider / Consumer / SurfaceModel
-> ChartRenderPlan metadata and DistributionRenderSurface
Delivery
-> DistributionRenderingContract and DistributionRenderPlanAdapter
Evidence / Audit
-> VNextDataResolutionHelper runtime recording, render-plan diagnostics, parity/evidence export paths
```

## Active UI Path

The active UI path is `DistributionChartControllerAdapter`. It relays interaction, resolves data, builds a distribution render request, attaches a validated `DistributionCapabilityContract`, and delegates delivery to `IDistributionRenderingContract`.

The adapter does not define the Distribution capability, composition kind, consumer kind, provider key, or delivery target. It now passes the upstream VNext capability/program/delivery contract into the render request, and render-plan vocabulary metadata is built from that runtime contract.

## Existing Evidence

Existing tests already cover:

- Distribution inclusion in visible VNext family requests
- Distribution runtime path preservation
- Distribution rendering qualification matrix and route capabilities
- Distribution adapter rendering behavior
- Distribution parity harnesses for weekly and hourly distribution
- architecture guardrails keeping rendering and adapters non-authoritative

## Phase 14 Guardrail Additions

Phase 14 adds targeted tests proving:

- Distribution VNext loading returns a traceable Distribution program signature through the capability/program path
- Distribution render plans preserve target-spine metadata across delivery
- Distribution capability ownership stays in VNext capability/program/contract structures, not in the UI adapter
- the live Distribution controller adapter passes a `DistributionCapabilityContract` into the rendering contract
- the Distribution render-plan builder uses the runtime capability/delivery contract instead of reconstructing delivery metadata from constants

## Findings

The original Phase 14 closure was proof-heavy and did not satisfy the agreed first production implementation slice. The reopened Phase 14 correction adds production contract carriage through the active Distribution path: `DistributionChartControllerAdapter -> DistributionChartRenderRequest -> DistributionRenderingContract -> DistributionRenderPlanBuilder`.

This remains behavior-preserving from the user's perspective, but it is a real implementation change: the live Distribution render path now transports the target-spine capability/program/delivery contract instead of relying on tests and render-plan constants to infer that relationship.

The next capability-expansion slice should only proceed after this one remains green through focused validation.
