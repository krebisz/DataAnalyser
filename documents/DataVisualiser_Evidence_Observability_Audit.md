# DataVisualiser Evidence Observability Audit

Recorded: 2026-04-28

Phase: 12 - Preserve Evidence as Observational

## Scope

This audit identifies evidence, diagnostics, parity, reachability, validation, and export flows and checks that they prove, record, export, validate, and audit without controlling live rendering behavior.

Inputs inspected:

- `DataVisualiser/UI/MainHost/Evidence`
- `DataVisualiser/UI/MainHost/Export`
- `DataVisualiser/Core/Validation`
- `DataVisualiser/Core/Validation/Parity`
- `DataVisualiser/Core/Strategies/Reachability`
- session milestone recorders under UI/Admin, UI/Workspace, and UI/Charts/Presentation
- evidence, parity, reachability, validation, and architecture tests

## Evidence and Diagnostics Readers

Evidence and diagnostics readers include:

- `EvidenceDiagnosticsBuilder`
- `MainChartsUiSurfaceDiagnosticsReader`
- `EvidenceDataResolutionHelper`
- session milestone recorders
- render-plan diagnostics readers through `ChartState`

These readers inspect current state, compute signatures, summarize render-plan metadata, and produce diagnostics snapshots. They do not invoke live rendering, adapter dispatch, provider selection, or backend binding.

`EvidenceDiagnosticsBuilder` does produce interpretations such as `ReloadLikelyRequired`, but these are exported diagnostics, not runtime commands.

## Evidence and Export Services

Evidence/export services include:

- `MainChartsEvidenceExportService`
- `MainChartsViewEvidenceExportCoordinator`
- `ReachabilityExportWriter`
- `ReachabilityExportPathResolver`
- `StrategyReachabilityEvidenceStore`

The export service builds a payload from chart state, metric state, reachability records, parity snapshots, performance timings, and diagnostics. File system writes are isolated in `ReachabilityExportWriter`.

`MainChartsEvidenceExportService` records evidence-export performance timings and clears the reachability evidence store after export. Those are evidence lifecycle operations, not live chart-routing decisions.

## Parity Evaluators

Parity evaluators include:

- `EvidenceParityBuilder`
- `EvidenceDistributionParityEvaluator`
- `EvidenceMultiMetricParityEvaluator`
- `EvidenceTransformParityEvaluator`
- `EvidenceStrategyParityExecutor`
- parity harnesses under `Core/Validation/Parity`
- `StrategyParityValidationService`

Evidence parity paths execute comparison logic and return snapshots. They do not update live render state or select providers/backends.

`Core.Validation.ParityValidationService` is a separate runtime parity-policy service. It is not an evidence export path and is not treated as observational evidence. If it becomes active in production routing, it should remain explicitly policy-owned and tested as runtime policy, not hidden inside evidence.

## Reachability Validators

Reachability structures include:

- `StrategyReachabilityStoreProbe`
- `StrategyReachabilityRecord`
- `StrategyCmsDecisionEvaluator`
- `StrategyCmsDecision`
- `NullStrategyReachabilityProbe`
- `StrategyReachabilityEvidenceStore`

Reachability decision evaluation is part of strategy routing, while the store/export path is observational. The evidence store snapshots and clears recorded reachability records; it does not decide live rendering behavior.

## Validation Flows

Validation flows include:

- `DataLoadValidator`
- `MetricDataValidationHelper`
- parity validators under `Core/Validation`

Data-load validation is explicit runtime gating for invalid user input or missing data. That is acceptable because it is validation policy, not hidden evidence policy.

Parity validation has two roles:

- evidence parity compares paths for export diagnostics
- runtime parity-policy validation must remain explicit and outside evidence/export code

## Phase 12 Guardrail Additions

Phase 12 adds static architecture guardrails:

- `EvidenceAndDiagnostics_ShouldRemainObservationalNotLiveRouting`
- `EvidenceExport_ShouldKeepFileSystemWritesOnDedicatedExportWriter`
- `ParityAndReachabilityEvidence_ShouldNotMutateLiveChartState`

These checks prevent evidence paths from invoking live rendering, selecting providers/backends, mutating live chart context/runtime state, or scattering file-system export writes outside the dedicated writer seam.

## Findings

Evidence is currently observational in the main export/diagnostics/parity paths.

No production refactor is justified in Phase 12. The main carry-forward distinction is that reachability and parity have both runtime-policy and evidence-observation roles. The migration should keep those roles explicit:

- runtime decision policy belongs in strategy/load policy structures
- evidence export belongs in evidence/export structures
- evidence diagnostics must not become a hidden route selector

The Syncfusion view evidence export composition noted in Phase 11 is acceptable as current host composition, because file writes and payload construction remain delegated to evidence/export services. It should still be watched if evidence export expands.
