# DataVisualiser Remaining Legacy Bypass Retirement Audit

Phase: 34 - Retire Remaining Legacy Bypasses

## Scope

This audit covers remaining legacy coexistence paths after the production family migrations and capability-contract consolidation.

Phase 34 does not authorize broad removal of `ChartDataContext`, strategy cut-over, parity, evidence, or terminal rendering fallback paths. Each bypass must have a named replacement, dependency proof, parity or smoke evidence, metadata preservation, and semantic/provenance preservation before retirement.

## Remaining Bypass Map

| Path | Current role | Replacement target | Dependency status | Phase 34 decision |
| --- | --- | --- | --- | --- |
| `LegacyChartProgramProjector` in `VNextMainChartIntegrationCoordinator` | Projects VNext `ChartProgram` back to `ChartDataContext` for main-chart production consumption | Consumer-neutral surface / VNext-native UI consumption model for main chart delivery | Production-bound by `VNextMainChartLoadResult.ProjectedContext` and main-chart tests | Keep. Main-chart ChartDataContext projection remains bounded until production consumers no longer require `ProjectedContext`. |
| `LegacyChartProgramProjector` in `VNextSeriesLoadCoordinator` | Projected a single-series `ChartProgram` into `ChartDataContext` only to return `Data1` | Direct `ChartProgram` series materialization | No production consumer requires the intermediate `ChartDataContext`; result contract already returns series data and CMS series separately | Retired now. |
| `VNextDataResolutionHelper` | Shared VNext-first load helper with legacy load fallback for selected chart-family data resolution | Family-specific VNext-native series resolution without legacy fallback, or consumer-neutral surface input | Production-bound by Distribution, WeekdayTrend, and Transform selected-series paths | Keep. Retire per family only after fallback-free smoke, parity, metadata, and provenance evidence exists. |
| `LegacyMetricViewGateway` | Adapter from legacy metric-selection service to VNext `MetricLoadSnapshot` | Native VNext metric loader / repository boundary | Production-bound through `ReasoningEngineFactory` and tests for reasoning/session loading | Keep. Rename or retire only when the loader boundary is no longer legacy service-backed. |
| `StrategyCutOverService` and `CreateLegacyStrategy` | CMS/legacy strategy compatibility and parity comparison | Native strategy input contracts and post-migration parity harnesses | Production-bound by orchestration paths and evidence parity executor | Keep. Parity/evidence usage remains validation-only where invoked by evidence export. |
| Evidence parity builders/evaluators | Observational validation of CMS/legacy parity | Convergence evidence and later validation-only parity suite | Evidence-bound, not live-routing authority | Keep as validation-only while it provides active audit value. |
| Distribution polar fallback route | Terminal rendering fallback/projection route | Qualified rendering backend or dedicated polar backend | Production-capable terminal fallback, not semantic authority | Keep as terminal tactical fallback. |

## Retired Bypass: VNextSeriesLoadCoordinator Single-Series Projector Detour

Before Phase 34, `VNextSeriesLoadCoordinator` executed VNext reasoning, received a `ChartProgram`, projected that program to `ChartDataContext`, and returned `projectedContext.Data1`.

The coordinator now materializes the primary `ChartProgram` series directly into `MetricData` and returns it through the existing `VNextSeriesLoadResult.Data` contract.

Preserved behavior:

- request, snapshot, and program source signatures are unchanged
- program kind is unchanged
- CMS series comes from the original load snapshot
- `NaN` values continue to become null metric values
- timestamp/value pairing still follows the program timeline and primary raw values

Replacement evidence:

- Target replacement: direct `ChartProgram` series materialization
- Production dependency proof: `VNextSeriesLoadResult` already carries `Data`, `CmsSeries`, and provenance signatures without exposing `ChartDataContext`
- Parity/smoke evidence: family smoke evidence already covers Distribution, WeekdayTrend, BarPie, Transform, SyncfusionSunburst, Main, Normalized, and automated Difference/Ratio paths; this change preserves the result contract used by selected-series family loading
- Metadata preservation: `CmsSeries`, `ProgramKind`, `RequestSignature`, `SnapshotSignature`, and `ProgramSourceSignature` are preserved
- Semantic/provenance preservation: `ChartProgram` remains produced by VNext reasoning and retains source signature before delivery

Guardrail evidence:

- `ArchitectureGuardrailTests.RemainingLegacyBypassRetirement_ShouldRemoveSeriesLoadProjectorBypassOnly`
- `VNextSeriesLoadCoordinatorTests`

## Validation-Only Paths

The evidence parity paths remain validation-only. They may construct legacy and CMS strategies to compare output, but they do not control live render selection or define canonical meaning.

Named retirement condition:

Evidence parity paths may be reduced only after Phase 35 convergence proves that remaining production consumers operate through the target grammar and after replacement validation artifacts exist for the same scenarios.

## Still Production-Bound Paths

Main-chart ChartDataContext projection, `VNextDataResolutionHelper`, `LegacyMetricViewGateway`, and strategy cut-over remain production-bound or compatibility-bound.

They must not be removed in a bulk Phase 34 change.

Named retirement conditions:

- main-chart consumers no longer require `ProjectedContext`
- selected-series chart-family resolution can run VNext-native without legacy fallback
- the reasoning engine can load through a non-legacy metric gateway
- strategy consumers can run through explicit native strategy input contracts, with parity retained only as evidence

