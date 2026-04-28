# DataVisualiser Dependency Density Audit

Recorded: 2026-04-28

## Purpose

This note classifies the current dependency-density evidence before refactoring or capability expansion continues. It is a Phase 3 audit artifact for `DataVisualiser_Migration_Plan_and_Guardrails.md`.

## Evidence

Inputs inspected:

- `documents/Type Dependencies Diagram.md`
- `type-dependency-diagram.md`
- `codebase-index.md`
- `dependency-summary.md`
- `DataVisualiser-Architectural-Vocabulary.md`
- `SYSTEM_MAP.md`
- selected hub files under `DataVisualiser/Core`, `DataVisualiser/UI`, and `DataVisualiser/VNext`

Density readings:

- Supplied typed diagram: 4434 parsed edges.
- Generated textual diagram: 953 declared type symbols, 6039 direct textual type-reference edges, 0.6656% dependency density.
- Generated textual diagram is repeatable baseline evidence, but it is coarser than the supplied typed diagram because it does not distinguish relationship kinds.

## Classification Summary

### Legitimate Steady-State Seams

These dense nodes are expected shared seams or carriers, not automatic refactoring targets:

- `MetricData`, `MetaData`, `ICanonicalMetricSeries`, `CanonicalMetricSeries`
- `ChartDataContext`, `ChartState`, `MetricState`, `MetricSeriesSelection`
- `ChartProgramKind`, `ChartDisplayMode`, `StrategyType`, `DistributionMode`
- `ChartRenderPlan`, `ChartRenderPlanKind`, `ChartRenderPlanMetadataKeys`, `ChartRenderPlanVocabularyMetadata`
- `ChartRenderAdapterResult`, `RenderDensityPlan`, `ChartHierarchyNodePlan`, `ChartSeriesPlan`, `ChartInteractionPlan`
- `IChartComputationStrategy`, `IDistributionService`, `IStrategyCutOverService`, `IUnitResolutionService`

Reasoning: these types carry domain state, program identity, render-plan metadata, strategy contracts, or consumer/provider handoff data. Their density is legitimate when they remain carrier/contract types and do not absorb policy.

### Legitimate Transitional Bridges

These dense nodes are transitional because they bridge old and target architecture paths:

- `MetricLoadCoordinator`
- `VNextMainChartIntegrationCoordinator`
- `VNextSeriesLoadCoordinator`
- `VNextDataResolutionHelper`
- `LegacyChartProgramProjector`
- `LegacyMetricViewGateway`
- chart-family controller adapters that route VNext-compatible data through legacy-compatible delivery surfaces
- evidence/parity flows that compare legacy, CMS, and VNext execution

Reasoning: these paths are expected while legacy and VNext coexist. They should be bounded and monitored, then reduced only after parity, smoke, and metadata-preservation evidence exists.

### Diagram / Export Noise

These density readings are partly inflated by diagram or scraper behavior:

- `Result`, `Context`, `Actions`, `Handlers`, and similarly generic nested names.
- test classes such as `ChartControllerFactoryTests`, `AnalyticalRenderPlanPipelineTests`, and `DistributionChartControllerAdapterTests`.
- repeated family-specific render request, render host, route, capability, and qualification records in the generated textual diagram.
- duplicated node declarations in the supplied Mermaid diagram.

Reasoning: the generated diagram records direct textual references, not compiler-bound semantic dependencies. The supplied diagram preserves relationship kinds but still contains repeated edge/node output. These entries should not drive refactoring without code inspection.

### Accidental Coupling Candidates

These are not confirmed violations, but they are Phase 4 candidates because their density is tied to responsibility concentration:

- `MainChartsView`
- `SyncfusionChartsView`
- `ChartUpdateCoordinator`
- `ChartRenderingOrchestrator`
- `ChartControllerFactory`
- `ChartControllerFactoryContext`
- `BaseDistributionService`
- `MainChartControllerAdapter`
- `DistributionChartControllerAdapter`
- `WeekdayTrendChartControllerAdapter`
- `SyncfusionSunburstChartControllerAdapter`
- `TransformDataPanelControllerAdapter`
- `EvidenceDiagnosticsBuilder`

Reasoning: existing documents already classify the large views as bounded host gravity, and current architecture guardrails restrict hubs from absorbing semantic/provider/evidence authority. These nodes should be examined for responsibility shedding only where tests prove a narrower seam already exists.

### Actual Drift

No actual drift is confirmed by density evidence alone.

Observed risks remain:

- host gravity in `MainChartsView` and `SyncfusionChartsView`
- old orchestration hubs becoming homes for VNext authority or delivery policy
- repeated family-specific request/route/qualification/adapter shapes
- evidence builders expanding beyond observational responsibility
- transitional legacy/VNext paths becoming permanent

These risks require Phase 4 classification before any refactoring.

## Phase 4 Candidate Flags

Safe-next inspection candidates:

- verify `ChartUpdateCoordinator` remains compute/render coordination only
- verify `ChartRenderingOrchestrator` remains orchestration over explicit pipelines
- verify `ChartControllerFactory` and `ChartControllerFactoryContext` remain controller composition only
- verify `MetricLoadCoordinator` remains a bounded legacy/VNext bridge
- compare repeated render-plan family shapes, but do not consolidate until multiple hardened slices prove the shared shape
- inspect `EvidenceDiagnosticsBuilder` only for observational boundaries, not for live behavior control

Rejected as immediate refactoring drivers:

- dense domain carriers
- dense enum/key types
- test-only density
- generic nested names in generated diagrams
- vendor/family-specific route types where the difference is still real

## Conclusion

Dependency density is classified enough to proceed to Phase 4. The next phase should identify refactoring opportunities from the accidental-coupling candidates, but no code should move solely because a node appears dense.
