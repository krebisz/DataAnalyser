# Chart Controller Decoupling Plan

Goal: Make chart controllers swappable without touching `MainChartsView` or
core orchestration logic.

## Scope
- Charts: Main, Normalized, DiffRatio, Distribution, WeekdayTrend, Transform, BarPie.
- Affects: `DataVisualiser/UI/MainChartsView.xaml.cs` and per-chart controllers.
- Non-goals: redesign UI or chart rendering engines.

## Progress Log (Living)
- 2026-01-16: Inventory complete. Coupling map captured from `DataVisualiser/UI/MainChartsView.xaml.cs` (visibility/toggles, subtype combos, rendering orchestration, tooltip/labels, reset/zoom, chart-type switches).
- 2026-01-16: Added `IChartController` contract and initial `DistributionChartControllerAdapter` skeleton (not wired).
- 2026-01-16: Wired Distribution events to adapter; moved Distribution UI state handling (mode, interval, subtype, chart-type visibility) into adapter. Rendering still delegated from `MainChartsView`.
- 2026-01-16: Completed Distribution migration: rendering, cache, and clear/reset logic now live in adapter; `MainChartsView` delegates to `DistributionChartControllerAdapter`.
- 2026-01-16: Completed WeekdayTrend migration: rendering, cache, UI handlers, and clear/reset logic now live in adapter; `MainChartsView` delegates to `WeekdayTrendChartControllerAdapter`.
- 2026-01-16: Completed Normalized migration: rendering, cache, UI handlers, and clear/reset logic now live in adapter; `MainChartsView` delegates to `NormalizedChartControllerAdapter`.
- 2026-01-16: Completed DiffRatio migration: rendering, cache, UI handlers, and clear/reset logic now live in adapter; `MainChartsView` delegates to `DiffRatioChartControllerAdapter`.
- 2026-01-20: Completed Transform migration: rendering, compute, cache, UI handlers, and clear/reset logic now live in adapter; `MainChartsView` delegates to `TransformDataPanelControllerAdapter`.
- 2026-01-20: Completed Main migration: rendering, display mode handling, and clear/reset logic now live in adapter; `MainChartsView` delegates to `MainChartControllerAdapter`.
- 2026-01-20: Added registry guards, key catalog, registry-based clear/reset paths, and resolver seam; registry remains a mirror for now.
- 2026-01-20: Added BarPie adapter and moved Bar/Pie rendering + controls into adapter; BarPie visibility moved into `ChartState`; `MainChartsView` now uses registry for BarPie render/init and hidden-chart clearing includes BarPie.
- 2026-01-20: BarPie now participates in ViewModel visibility toggles (`ShowBarPie` in update args; `ToggleBarPie`), and `MainChartsView` handles BarPie in visibility/render paths alongside other charts.
- 2026-01-20: Added adapter-level tests for BarPie controller behavior.
- 2026-01-20: Introduced `ChartControllerFactory` seam so `MainChartsView` no longer instantiates adapters directly.
- 2026-01-21: Phase 3.5 reachability evidence captured in `documents/reachability-20260121-074430.json`, `documents/reachability-20260121-074900.json`, `documents/reachability-20260121-075301.json`, `documents/reachability-20260121-085510.json`.

## Technical Decisions (Living)
- Prefer a scaffold-style UI contract (e.g., `IChartPanelScaffold`) to standardize panel wiring while keeping rendering logic unchanged.
- Keep rendering engines and computation orchestration untouched; adapters live in UI layer only.
- Avoid exposing concrete UI controls beyond what is necessary; introduce thin wrappers where needed.
- Defer registry/factory work until all chart adapters are in place; current scope focuses on per-chart logic extraction.
- With all chart adapters in place, begin registry/factory work next.

## Step 1: Inventory Current Coupling
Checklist:
- Identify all direct references to chart controllers in
  `DataVisualiser/UI/MainChartsView.xaml.cs`.
- Categorize usage:
  - UI toggles and visibility
  - Rendering orchestration calls
  - Subtype combo management
  - Tooltip/label updates
  - Reset/Clear/Zoom operations
  - Chart type switching (Distribution/WeekdayTrend)
- Record where each chart-specific behavior lives.

Deliverables:
- A short mapping table: "Responsibility -> Chart -> Current method(s)".

## Step 2: Define a Common Chart Controller Contract
Propose an interface (example):

- `string Key { get; }` (e.g., "DiffRatio")
- `bool RequiresSecondaryData { get; }`
- `Panel Panel { get; }` (or abstract wrapper)
- `ButtonBase ToggleButton { get; }`
- `CartesianChart? Chart { get; }` (or chart wrapper)
- `void Initialize(...)` (view model, services, state)
- `Task RenderAsync(ChartDataContext ctx)`
- `void Clear(ChartState state)`
- `void ResetZoom()`
- Optional: subtype selectors (`PrimaryCombo`, `SecondaryCombo`) via interface

Notes:
- Avoid leaking concrete UI controls where possible; introduce wrappers if
  needed (e.g., `IChartViewSurface`).

Deliverables:
- `IChartController` draft in `DataVisualiser/UI/Controls` or a new namespace.

## Step 3: Extract Chart-Specific Logic
For each chart:
- Move chart-specific handlers and state updates into a dedicated controller.
- Keep `MainChartsView` as a coordinator only.
Examples:
- `DiffRatioChartControllerAdapter` handles:
  - Toggle and operation toggle events
  - Subtype combo population
  - Title/label updates
  - Render and clear methods
- `DistributionChartControllerAdapter` handles:
  - Mode and interval changes
  - Polar/cartesian switching

Deliverables:
- One adapter per chart implementing `IChartController`.

## Step 4: Add a Controller Registry/Factory
Create a registry:
- `IChartControllerRegistry` with `Get(string key)` and `All()` methods.
- Wiring via DI or local factory in `MainChartsView`.
- Support a config-based enable/disable list.

Deliverables:
- Registry implementation, tests for registration.

## Step 5: Refactor MainChartsView to Use Registry
Changes:
- Replace direct field usage (`DiffRatioChartController.*`) with
  registry lookups and interface calls.
- Loop over controllers for common actions:
  - Clear, ResetZoom, Toggle states
  - Global updates (tooltip, titles)

Deliverables:
- `MainChartsView` depends on the interface/registry only.

## Step 6: Update ViewModels and State
Changes:
- Use chart keys for visibility and updates.
- Decouple state setters from concrete controller references.
Update tests to use the new interfaces or chart keys.

Deliverables:
- Updated ViewModel logic and tests.

## Step 7: Tests and Rollout
Tests:
- Unit tests per controller adapter (render call, toggle state).
- Integration test: "swap DiffRatio controller with stub and app still runs."
- Smoke tests for chart visibility toggles.

Rollout:
- Introduce adapters side-by-side with current logic, then migrate chart by chart.
- Remove placeholder/hard-coded references after migration.

## Suggested Order of Migration
1) Distribution (self-contained chart type switching) (done)
2) WeekdayTrend (done)
3) Normalized (done)
4) DiffRatio (most coupled) (done)
5) Transform panel (done)
6) Main chart (done)

## Completed Migration Checklist (Step 1: Distribution)
- Define `DistributionChartControllerAdapter` responsibilities (toggle, mode, subtype, interval, polar/cartesian switching). (done)
- Add adapter class in `DataVisualiser/UI/Controls` (or `DataVisualiser/UI/Adapters`) with explicit dependencies (view model, update coordinator, rendering services). (done)
- Move `UpdateDistributionSubtypeOptions`, `ApplyDistributionModeDefinition`, `ApplyDistributionSettingsToUi`, and chart-type visibility logic into adapter methods. (done)
- Route Distribution UI events in `MainChartsView` to adapter (no direct control access). (done)
- Keep render calls in adapter: `RenderDistributionChart`, `HandleDistributionDisplayModeChanged`, `HandleDistributionIntervalCountChanged`. (done)
- Leave rendering engines unchanged; only UI orchestration shifts. (on track)

## Completed Migration Checklist (Step 2: WeekdayTrend)
- Define `WeekdayTrendChartControllerAdapter` responsibilities (toggle, mode, subtype, day/average controls, chart-type switching). (done)
- Move render/data resolution (`RenderWeekdayTrendAsync`, cache, selection helpers) into adapter. (done)
- Route WeekdayTrend UI events in `MainChartsView` to adapter (no direct control access). (done)
- Keep rendering engines unchanged; only UI orchestration shifts. (on track)

## Completed Migration Checklist (Step 3: Normalized)
- Define `NormalizedChartControllerAdapter` responsibilities (toggle, subtype combos, normalization mode). (done)
- Move render/data resolution and title updates into adapter. (done)
- Route Normalized UI events in `MainChartsView` to adapter. (done)
- Keep rendering engines unchanged; only UI orchestration shifts. (on track)

## Completed Migration Checklist (Step 4: DiffRatio)
- Define `DiffRatioChartControllerAdapter` responsibilities (toggle, operation toggle, subtype combos). (done)
- Move render/data resolution and title updates into adapter. (done)
- Route DiffRatio UI events in `MainChartsView` to adapter. (done)
- Keep rendering engines unchanged; only UI orchestration shifts. (on track)

## Completed Migration Checklist (Step 5: Transform)
- Define `TransformDataPanelControllerAdapter` responsibilities (toggle, subtype combos, compute). (done)
- Move transform render/data resolution and UI state into adapter. (done)
- Route Transform UI events in `MainChartsView` to adapter. (done)
- Keep rendering engines unchanged; only UI orchestration shifts. (on track)

## Completed Migration Checklist (Step 6: Main)
- Define `MainChartControllerAdapter` responsibilities (toggle, display mode, render/clear/reset). (done)
- Move main chart render orchestration into adapter. (done)
- Route main chart UI events in `MainChartsView` to adapter. (done)

## Next Steps
- Remove remaining direct controller references in `MainChartsView` (adapter construction aside) and rely solely on registry.
- Remove fallback direct control access in `GetCartesianChart`/`GetPolarChart` once registry is always available.
- Add/adapt adapter-level tests for BarPie similar to other chart adapters. (done)

## Step-by-Step Plan to Mitigate Registry/Factory Risks
1) Snapshot current behavior: list all chart keys, visibility toggles, and render entry points in `MainChartsView`. Capture any special cases (visibility-only toggles, secondary-data gating).
2) Introduce registry types without wiring changes: add `IChartControllerRegistry` and a simple `ChartControllerRegistry` that can register and return adapters. Do not modify existing call sites yet.
3) Build a single adapter factory method in `MainChartsView` (or a dedicated factory) that constructs all adapters in the current order, then registers them. Keep existing fields intact; registry is a mirror only.
4) Add validation at startup: after registration, verify expected keys are present and log/throw if any are missing. This prevents silent no-ops.
5) Add guard checks in the registry:
   - `Get` throws with a clear message when a key is missing.
   - `Register` rejects duplicates (case-insensitive) and null adapters.
   - `All()` returns a stable order (registration order).
6) Introduce seam: a small `IChartKeyCatalog` (or static key list) to centralize expected keys and avoid string drift.
7) Migrate non-critical paths first: switch passive operations to registry loops (e.g., `ClearAllCharts`, `ResetZoom`, `ClearChartsOnStartup`). Validate behavior remains unchanged.
8) Add a compatibility seam: a thin adapter resolver in `MainChartsView` (e.g., `GetController("Main")`) so call sites can be swapped one-by-one while keeping old fields as fallback.
9) Migrate visibility wiring: route panel visibility updates through registry lookups by key. Keep existing conditional logic intact to preserve visibility-only toggle behavior.
10) Add explicit guards for visibility-only toggles: ensure the transform panel toggle remains a visibility-only path and does not trigger full renders.
11) Migrate render orchestration last: replace direct adapter calls with registry lookups, keeping existing gating checks (secondary data, chart visibility) unchanged.
12) Add data-gating guards in loops: for adapters requiring secondary data, skip render when missing and clear if needed (match current behavior).
13) Add a temporary compatibility shim: keep old fields during migration and assert both code paths are equivalent for a short period (log when they diverge).
14) Remove duplicated paths: once behavior is verified, delete old field references and rely solely on registry.
15) Add minimal tests: registry registration/lookup, presence of all keys, and a smoke test that toggling a chart still renders the same path.

## Registry Snapshot (Before Wiring Changes)
Keys: `Main`, `Norm`, `DiffRatio`, `Distribution`, `WeeklyTrend`, `Transform`, `BarPie`.
Visibility toggles:
- `Main` -> `_viewModel.ToggleMain`, `MainChartController.Panel.IsChartVisible`
- `Norm` -> `_viewModel.ToggleNormalized`
- `DiffRatio` -> `_viewModel.ToggleDiffRatio`
- `Distribution` -> `_viewModel.ToggleDistribution` (+ polar/cartesian toggle)
- `WeeklyTrend` -> `_viewModel.ToggleWeeklyTrend` (+ chart type toggle)
- `Transform` -> `_viewModel.ToggleTransformPanel` (visibility-only toggle path)
 - `BarPie` -> adapter-managed toggle + `ChartState.IsBarPieVisible`
Render entry points:
- `Main` -> `_mainAdapter.RenderAsync`
- `Norm` -> `_normalizedAdapter.RenderAsync` (requires secondary data)
- `DiffRatio` -> `_diffRatioAdapter.RenderAsync` (requires secondary data)
- `Distribution` -> `_distributionAdapter.RenderAsync`
- `WeeklyTrend` -> `_weekdayTrendAdapter.RenderAsync`
- `Transform` -> `_transformAdapter.RenderAsync` (grids only unless compute)
 - `BarPie` -> `_barPieAdapter.RenderAsync`
Special cases:
- Transform visibility-only toggle must not trigger full re-render.
- Secondary-data gating clears `Norm`/`DiffRatio` when secondary data disappears.
- BarPie visibility toggles now follow the same ViewModel update path.

## Risks and Mitigations
- Risk: Too many UI control dependencies.
  - Mitigation: Introduce lightweight wrappers.
- Risk: Rendering orchestration still coupled.
  - Mitigation: Push per-chart render into adapter.
- Risk: Regression in visibility toggles.
  - Mitigation: Add tests and log for each toggle event.

## Success Criteria
- `MainChartsView` does not reference chart controllers directly.
- Controllers can be swapped/enabled via registry/config.
- No change required to core render engines to add/remove a chart.
