# DataVisualiser Distribution Capability Slice Audit

Recorded: 2026-04-28

Phase: 14 - Resume Capability Expansion

Selected slice: active Distribution capability

## Scope

This audit records why Distribution was selected as the first Phase 14 capability slice and how the existing implementation maps to the target spine without adding new user-visible behavior.

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

The active UI path is `DistributionChartControllerAdapter`. It relays interaction, resolves data, builds a distribution render request, and delegates delivery to `IDistributionRenderingContract`.

The adapter does not define the Distribution capability, composition kind, consumer kind, provider key, or delivery target. Those are defined upstream through VNext contracts and render-plan vocabulary metadata.

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

## Findings

No production behavior change is required for this slice. The current implementation already supports Distribution as an active capability path. The Phase 14 work should therefore focus on tightening proof around the existing path rather than enabling new capability behavior.

The next capability-expansion slice should only proceed after this one remains green through focused validation.
