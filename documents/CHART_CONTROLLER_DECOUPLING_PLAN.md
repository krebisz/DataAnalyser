# Chart Controller Decoupling Plan

Goal: Make chart controllers swappable without touching `MainChartsView` or
core orchestration logic.

## Scope
- Charts: Main, Normalized, DiffRatio, Distribution, WeekdayTrend, Transform.
- Affects: `DataVisualiser/UI/MainChartsView.xaml.cs` and per-chart controllers.
- Non-goals: redesign UI or chart rendering engines.

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
1) Distribution (self-contained chart type switching)
2) WeekdayTrend
3) Normalized
4) DiffRatio (most coupled)
5) Transform panel
6) Main chart

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
