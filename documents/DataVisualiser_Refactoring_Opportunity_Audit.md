# DataVisualiser Refactoring Opportunity Audit

Recorded: 2026-04-28

Phase: 4 - Identify Refactoring Opportunities

## Purpose

This note records Phase 4 refactoring-opportunity classification from `DataVisualiser_Migration_Plan_and_Guardrails.md`. It follows the Phase 3 dependency-density audit and intentionally separates safe refactoring from density-driven churn.

## Evidence Inspected

- `documents/DataVisualiser_Dependency_Density_Audit.md`
- `documents/Type Dependencies Diagram.md`
- `type-dependency-diagram.md`
- `DataVisualiser.Tests/Architecture/ArchitectureGuardrailTests.cs`
- `DataVisualiser/UI/Charts/Presentation/ChartControllerFactory.cs`
- `DataVisualiser/Core/Orchestration/ChartUpdateCoordinator.cs`
- `DataVisualiser/Core/Orchestration/ChartRenderingOrchestrator.cs`
- `DataVisualiser/UI/ViewModels/MetricLoadCoordinator.cs`
- `DataVisualiser/UI/MainHost/Evidence/EvidenceDiagnosticsBuilder.cs`
- `DataVisualiser/Core/Services/BaseDistributionService.cs`
- render contract and render-plan family types under `DataVisualiser/Core/Rendering/Contracts`

## Repeated Structures Across Chart / Render / Delivery Families

Repeated family structures exist across Bar/Pie, Distribution, WeekdayTrend, Transform, Cartesian, and Syncfusion delivery paths:

- backend key or backend qualification
- rendering route
- rendering qualification
- rendering capabilities
- render request
- render host
- render surface
- render-plan builder
- render-plan adapter
- rendering qualification probe
- rendering contract

Classification: defer until another slice proves shared shape.

Reasoning: the shape is clearly repeated, but the family differences are still meaningful. Distribution has Cartesian and polar fallback behavior, WeekdayTrend has Cartesian/polar/scatter behavior, Bar/Pie has column and faceted pie behavior, Transform remains result-Cartesian, Syncfusion is vendor-specific, and Cartesian metrics still cover main/normalized/diff-ratio routes. Consolidating these now would violate the plan's instruction to consolidate repeated family patterns last.

## Exception-Driven Paths That May Become Generalized Seams

Potential generalized seams:

- `ChartRenderPlan` plus `ChartRenderPlanAdapterDispatcher<TSurface>` as the common delivery handoff.
- `ChartRenderPlanVocabularyMetadata` and `ChartRenderPlanProviderMetadata` as common metadata preservation seams.
- `ConsumerProviderRegistry` and consumer/provider contracts as downstream qualification inputs.
- render-plan adapter qualification rules as the common compatibility gate.

Classification: needs tests first.

Reasoning: this is central to later Phase 7 contract/boundary/qualification hardening. It should be strengthened with metadata preservation, mismatch rejection, and bypass-prevention tests before moving more code behind it.

## Contradictory Ownership Candidates

No confirmed contradictory ownership was found that justifies immediate movement.

Watch points:

- `ChartUpdateCoordinator` still bridges computation, render-model construction, render-plan projection, and LiveCharts terminal delivery.
- `ChartRenderingOrchestrator` still coordinates old orchestration pipelines and render-plan evidence capture.
- `MetricLoadCoordinator` still owns the live VNext/legacy routing bridge.
- `EvidenceDiagnosticsBuilder` interprets state for diagnostics, but current guardrails keep it observational and prevent live rendering or execution calls.

Classification: needs tests first for `ChartUpdateCoordinator`, `ChartRenderingOrchestrator`, and `MetricLoadCoordinator`; safe to retain for `EvidenceDiagnosticsBuilder` while observational guardrails hold.

Reasoning: existing architecture guardrails already prohibit the most dangerous authority absorption. Moving behavior now would be higher risk than adding stronger hub-boundary tests in Phase 6.

## Hubs That Can Lose Responsibility Safely

Safe-now code movement: none identified.

Safe-next guardrail candidates:

- add static tests preventing `ChartUpdateCoordinator` from gaining provider, evidence, reasoning, or policy authority beyond existing forbidden tokens.
- add static tests preventing `ChartRenderingOrchestrator` from gaining semantic/provider/evidence authority.
- add static tests preventing `MetricLoadCoordinator` from becoming capability/program/provider owner outside its bounded VNext/legacy bridge role.
- add tests proving `ChartControllerFactory` remains controller composition and event binding only.
- add tests proving `EvidenceDiagnosticsBuilder` remains observational and does not select provider/backend/delivery policy.

Classification: needs tests first.

Reasoning: these are old hubs and should be capped before responsibility is moved. This aligns with Phase 6.

## Duplicated Request / Route / Qualification / Adapter Patterns

Duplicated patterns:

- `*RenderingRoute`
- `*RenderingQualification`
- `*RenderingCapabilities`
- `*BackendQualification`
- `*ChartRenderRequest`
- `*ChartRenderHost`
- `*RenderSurface`
- `*RenderPlanBuilder`
- `*RenderPlanAdapter`
- `*RenderingContract`

Classification: defer until another slice proves shared shape.

Reasoning: the shared skeleton is visible, but Phase 17 explicitly places repeated family pattern consolidation last. Current differences are still real enough to preserve.

## Target Grammar Mapping

Implementation-shaped concepts that should guide later naming/ownership work:

- `Controller` -> `ConsumerAdapter`: valid conceptual mapping for chart controller adapters, but defer renaming because it would be cosmetic without an ownership move.
- `ViewModel` -> `ConsumerState`: valid conceptual mapping for UI state/view model boundaries, but defer renaming because current WPF conventions still make `ViewModel` operationally clear.
- `Renderer` -> `DeliveryAdapter`: valid for render-plan adapters and vendor delivery code; apply only when delivery adapters become the dominant seam.
- `Route` -> `Binding`: valid direction for route policy that becomes explicit delivery binding; defer until contract/boundary hardening proves the replacement shape.
- `Host` -> `RuntimeBoundary` / `DeliverySurface`: valid for vendor or WPF delivery hosts; defer because existing host names still describe concrete UI infrastructure.
- `Diagnostics` -> `Evidence`: apply only where proof/audit is intended; retain `Diagnostics` where the object is a runtime snapshot rather than durable evidence.

Classification: defer until ownership changes make names truthful.

Reasoning: renaming alone would be cosmetic and would increase churn without architectural gain.

## Opportunity Classification

| Opportunity | Classification | Reason |
|---|---|---|
| Consolidate render-plan family builders | Defer until another slice proves shared shape | Repeated skeleton exists, but family differences are real and Phase 17 reserves consolidation for last. |
| Move more rendering policy out of `ChartUpdateCoordinator` | Needs tests first | It is behavior-adjacent and tied to live chart rendering. |
| Cap `ChartRenderingOrchestrator` authority growth | Needs tests first | It coordinates explicit pipelines; guardrails should prevent new authority before movement. |
| Cap `MetricLoadCoordinator` VNext bridge scope | Needs tests first | Transitional bridge should stay bounded before legacy bypass retirement. |
| Extract controller factory construction helpers | Reject for now | It would mostly reorganize composition code without reducing architectural risk. |
| Rename implementation-shaped concepts to target grammar | Reject for now | Renames would be cosmetic unless paired with proven ownership changes. |
| Keep `EvidenceDiagnosticsBuilder` observational | Safe now as guardrail posture, no code movement | Existing tests already prevent live execution/render calls. |
| Treat domain/state carriers as refactor targets | Reject | Density is legitimate steady-state seam usage. |

## Conclusion

Phase 4 identifies real refactoring opportunities, but no behavior code should move in this slice. The safe next work is to harden guardrails around old hubs and contract/boundary metadata preservation in later phases before extracting or consolidating behavior.
