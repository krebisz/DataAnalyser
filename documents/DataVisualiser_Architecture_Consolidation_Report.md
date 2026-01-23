# DataVisualiser Architecture Consolidation Report
Status: Draft
Scope: Interface extraction, consolidation, and common class candidates for bloat reduction
Authority: Informational (subordinate to Project Bible.md and SYSTEM_MAP.md)

## 1. Context and Intent
This report reviews the DataVisualiser project for interface extraction and consolidation opportunities.
It respects the system boundaries defined in `documents/Project Bible.md` and `documents/SYSTEM_MAP.md`.
The goal is to reduce duplication and complexity without changing semantics or execution flow.

## 2. Interface Extraction Candidates
1) Unified chart controller adapter contract
   - Consolidate the fragmented controller capability interfaces into one explicit contract.
   - Current spread: `DataVisualiser/UI/Controls/IChartController.cs`,
     `DataVisualiser/UI/Controls/IChartSubtypeOptionsController.cs`,
     `DataVisualiser/UI/Controls/IChartCacheController.cs`,
     `DataVisualiser/UI/Controls/IChartSeriesAvailability.cs`,
     `DataVisualiser/UI/Controls/ICartesianChartSurface.cs`,
     `DataVisualiser/UI/Controls/IPolarChartSurface.cs`.

2) Metric series selection controller interface
   - Standardize subtype selection, cache invalidation, and data resolution logic.
   - Current duplication: `DataVisualiser/UI/Controls/NormalizedChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/DiffRatioChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/DistributionChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/WeekdayTrendChartControllerAdapter.cs`.

3) Core rendering abstraction
   - Introduce a core-facing render interface so orchestration depends on a stable contract.
   - Aligns `DataVisualiser/Core/Rendering/Engines/ChartRenderEngine.cs`
     with `DataVisualiser/UI/Rendering/IChartRenderer.cs`
     and `DataVisualiser/UI/Rendering/LiveCharts/LiveChartsChartRenderer.cs`.

4) Distribution service contract
   - Formalize the base distribution service API into `IDistributionService`.
   - Current implicit contract: `DataVisualiser/Core/Services/BaseDistributionService.cs`,
     `DataVisualiser/Core/Services/WeeklyDistributionService.cs`,
     `DataVisualiser/Core/Services/HourlyDistributionService.cs`.

5) Strategy metadata provider
   - Replace switch-based metadata with a catalog interface for CMS usage, parity harness, and log identity.
   - Current switches: `DataVisualiser/Core/Strategies/StrategyCutOverService.cs`,
     `DataVisualiser/Core/Validation/Parity/IStrategyParityHarness.cs`.

## 3. Consolidation Targets
1) Subtype combo + selection logic
   - Shared patterns exist across all chart controller adapters with subtype selection.
   - A shared helper or base adapter would eliminate repeated cache keys, combo population, and selection matching.

2) HasSeries checks
   - Duplicate `HasSeriesInternal` blocks in multiple adapters.
   - Centralize as a shared helper or base controller.

3) Timeline alignment duplication
   - `AlignSeriesToTimeline` appears in both render engine and coordinator.
   - Consolidate into a shared utility to reduce drift and mismatched alignment rules.

4) Rendering helpers vs UI helpers separation
   - `DataVisualiser/Core/Rendering/Helpers/ChartHelper.cs` mixes UI component logic with core rendering helpers.
   - Split into a UI-layer helper to respect layer boundaries.

5) ChartRenderModel naming collision
   - Two distinct `ChartRenderModel` classes exist in Core and UI namespaces.
   - Consider shared interface or distinct naming to prevent confusion.

6) Controller key literals
   - Some adapters hardcode keys instead of using `DataVisualiser/UI/Controls/ChartControllerKeys.cs`.
   - Standardize to reduce string drift.

## 4. Common Class Candidates
1) MetricSeriesSelectionCache
   - Unify cache key creation, selection matching, and MetricSelectionService data loading.
   - Targets: `DataVisualiser/UI/Controls/NormalizedChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/DiffRatioChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/DistributionChartControllerAdapter.cs`,
     `DataVisualiser/UI/Controls/WeekdayTrendChartControllerAdapter.cs`.

2) SeriesAlignmentHelper
   - Centralize series alignment rules used by rendering and coordinator.
   - Targets: `DataVisualiser/Core/Rendering/Engines/ChartRenderEngine.cs`,
     `DataVisualiser/Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`,
     `DataVisualiser/Core/Orchestration/Builders/ChartDataContextBuilder.cs`.

3) ChartLabelFormatter
   - Standardize label formatting for series titles and tooltips.
   - Targets: `DataVisualiser/Core/Rendering/Engines/ChartRenderEngine.cs`,
     `DataVisualiser/Core/Rendering/Helpers/ChartHelper.cs`.

4) StrategyTypeMetadata
   - Create a static registry or service for strategy metadata (CMS eligibility, parity harness).
   - Target: `DataVisualiser/Core/Strategies/StrategyCutOverService.cs`.

## 5. Architectural Guardrails
- Preserve downward-only authority flow: computation and rendering must not reassign semantics.
- Keep UI logic non-authoritative: no semantic inference or canonical mutation.
- Ensure CMS usage and parity checks remain explicit and observable.

## 6. Suggested Starting Point
The lowest risk, high value first step is extracting a MetricSeriesSelectionCache
used by the four subtype-heavy chart adapters. This yields immediate duplication
reduction without affecting core computation semantics.

## 7. Sequential Implementation Plan
This plan sequences work from low-risk refactors to higher-impact consolidations.
Each step should preserve system boundaries and avoid semantic changes.

Step 1: Baseline alignment and scope locking
- Confirm current behavior via existing tests (especially UI + strategy tests).
- Identify adapters and helpers in scope; avoid touching canonical/normalization layers.
- Define a small "no behavior change" checkpoint for each step.

Step 2: Extract MetricSeriesSelectionCache (UI-layer helper)
- Create a reusable helper for:
  - cache key construction
  - selection equivalence checks
  - subtype combo item creation and lookup
  - shared selection resolution from combo or ChartState
- Update adapters to use it:
  - `DataVisualiser/UI/Controls/NormalizedChartControllerAdapter.cs`
  - `DataVisualiser/UI/Controls/DiffRatioChartControllerAdapter.cs`
  - `DataVisualiser/UI/Controls/DistributionChartControllerAdapter.cs`
  - `DataVisualiser/UI/Controls/WeekdayTrendChartControllerAdapter.cs`
- Validate:
  - subtype combos populate as before
  - cache hits/misses are unchanged
  - rendering still triggered correctly after selection changes

Step 3: Consolidate HasSeries and chart surface checks
- Add a small shared helper (or base adapter) for series detection and visibility.
- Replace duplicate `HasSeriesInternal` implementations in adapters.
- Validate:
  - series presence checks still match previous behavior in tooltips and UI toggles

Step 4: Extract SeriesAlignmentHelper (core helper)
- Move duplicated timeline alignment logic into a single helper.
- Replace:
  - `AlignSeriesToTimeline` in `DataVisualiser/Core/Rendering/Engines/ChartRenderEngine.cs`
  - `AlignSeriesToTimeline` in `DataVisualiser/Core/Orchestration/Coordinator/ChartUpdateCoordinator.cs`
- Validate:
  - charts render identical timestamps and series counts
  - stacked/cumulative charts still align correctly

Step 5: Split ChartHelper by layer responsibility
- Create a UI-only helper for WPF control manipulation (tooltips, popups, UI elements).
- Keep core rendering helpers pure of UI dependencies.
- Update references from `DataVisualiser/Core/Rendering/Helpers/ChartHelper.cs`.
- Validate:
  - tooltips, hover overlays, and axis normalization still behave correctly

Step 6: Clarify ChartRenderModel naming
- Introduce clearer names or an interface to distinguish Core vs UI models.
- Update import usage to reduce confusion and avoid accidental coupling.
- Validate:
  - build references are stable and no namespace collisions remain

Step 7: Strategy metadata registry
- Create a `StrategyTypeMetadata` registry to remove switch duplication in
  `DataVisualiser/Core/Strategies/StrategyCutOverService.cs`.
- Include:
  - CMS eligibility flag
  - parity harness mapping
  - log identity/name
- Validate:
  - CMS cut-over decisions are unchanged
  - parity harness selection remains consistent

Step 8: Optional - Introduce `IDistributionService`
- Define a small interface mirroring `BaseDistributionService` usage.
- Update orchestration and adapters to depend on the interface.
- Validate:
  - weekly/hourly distribution rendering unaffected

Step 9: Optional - Unified chart controller adapter interface
- Replace the fragmented controller capability interfaces with a single contract.
- Update `ChartControllerRegistry` to consume the unified contract.
- Validate:
  - chart toggles, caching, and subtype options remain unchanged

Step 10: Final cleanup and documentation
- Remove unused helpers or redundant private methods.
- Update documentation in `documents/CHART_CONTROLLER_DECOUPLING_PLAN.md` if needed.
- Capture any new helper usage patterns or constraints.

End of report.
