# Under-Developed Components - Implementation Order (Running)

Last updated: 2026-01-27

## Ordered list (risk-aware)
1. Shared selection/cache helpers in UI adapters (low risk, high duplication payoff).
2. Chart controller adapter decoupling from ViewModel/control specifics (reduce coupling, improve testability).
3. Transform panel split (UI layout vs transform computation/rendering responsibilities).
4. Chart orchestration/service boundary cleanup (separate data access, strategy selection, rendering coordination).
5. CMS vs legacy data path consolidation (reduce divergence and branching risk).
6. Helper/utilities consolidation (clarify ownership and reduce copy/paste patterns).
7. Configuration defaults and policy centralization (ensure consistency and versioning).

## Notes
- This is a running document; entries will be expanded with scoped steps and risk checks.
- Migration-readiness guardrail: keep UI render-only and non-authoritative; all chart semantics/decisions must remain in .NET.
- Recent groundwork completed (ECharts-ready seam + WPF isolation):
  - Renderer seam is async and routable: `IChartRenderer.ApplyAsync(...)`, `ChartRendererResolver`, `ChartSurfaceFactory`.
  - Controller contract no longer exposes WPF controls directly (`IChartController` now uses `SetVisible/SetTitle/SetToggleEnabled`).
  - WPF escape hatches are explicit and isolated:
    - Panel host: `IWpfChartPanelHost`
    - Cartesian chart host: `IWpfCartesianChartHost`
  - WPF cartesian access in the view is now centralized behind `GetWpfCartesianChart(...)` in `DataVisualiser/UI/MainChartsView.xaml.cs:1105`.
- Near-term guidance:
  - Do: add advanced chart types (treemap/sunburst/chord-like) via renderer routing/resolver.
  - Don't: introduce new WPF control types into controller interfaces.

## 1. Shared selection/cache helpers in UI adapters
- Inventory adapters that maintain local caches or selection logic (e.g., `*ChartControllerAdapter.cs`).
- Normalize cache-key usage and cache ownership (replace ad-hoc caches with `MetricSeriesSelectionCache` where safe).
- Consolidate combo selection handling to shared helpers (build/find/get/resolve patterns).
- Remove duplicated selection comparison logic (use `MetricSeriesSelectionCache.IsSameSelection`).
- Add light regression checks: build, and verify common flows (e.g., switching subtype combos, toggling chart type).

## 2. Chart controller adapter decoupling from ViewModel/control specifics
- Catalogue adapter responsibilities (UI wiring, data selection, rendering orchestration) and mark boundaries.
- Introduce thin adapter interfaces for UI controls (hide concrete control types from adapters).
- Move view-model state mutations behind small service interfaces (e.g., chart visibility, selection updates).
- Replace direct control event handling with command/handler abstractions where possible.
- Add unit tests for adapter behavior using fakes for the new interfaces.

## 3. Transform panel split (UI layout vs transform computation/rendering)
- Identify computation-heavy sections in `TransformDataPanelControllerAdapter` and extract to a service.
- Separate layout measurement logic into a UI-only helper/class.
- Define a transform result model to decouple computation from rendering details.
- Ensure binary/unary operation gating is handled centrally (single source of truth).
- Validate with targeted UI smoke checks (compute action, layout resize, result chart render).

## 4. Chart orchestration/service boundary cleanup
- Map orchestration flow (data load -> strategy creation -> rendering) and mark current coupling points.
- Extract data access concerns into a dedicated data provider layer with explicit contracts.
- Split strategy selection from rendering orchestration into distinct services.
- Introduce a single ChartRenderContext builder to avoid duplicated `ChartDataContext` assembly.
- Add integration-level tests for a representative chart pipeline (main, distribution, weekday trend).

## 5. CMS vs legacy data path consolidation
- Inventory CMS/legacy branching sites and categorize by feature (loading, computation, rendering).
- Define a unified data model or adapter for CMS/legacy inputs.
- Centralize the cut-over logic (single service) and remove scattered feature flags.
- Align test coverage across both paths (parity checks, edge cases).
- Decommission old code paths once parity and stability criteria are met.

## 6. Helper/utilities consolidation
- Group helpers by responsibility (data, rendering, UI) and identify overlaps.
- Create small focused services to replace broad helper classes.
- Move repeated calculations into shared, tested utilities with explicit contracts.
- Deprecate or remove "helper" methods that are unused or now redundant.
- Add documentation notes for new helper ownership and usage rules.

## 7. Configuration defaults and policy centralization
- Inventory defaults across configuration files and identify conflicts/duplicates.
- Define a single configuration policy module (versioned or layered defaults).
- Provide a clear override hierarchy (env, user config, code defaults).
- Update consumers to use the centralized defaults provider.
- Add tests validating default resolution and override precedence.
