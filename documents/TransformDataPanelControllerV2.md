# TransformDataPanelControllerV2 — Grid-based Collapsible Left Rail with Horizontal Scroll (AI-Ready Task)

## Goal

Create a **new WPF UserControl** (do not modify the existing working one) that replaces the current horizontal `StackPanel`-based “data grids + chart” layout with a **Grid-based shell** supporting:

1. A **collapsible left rail** that can host **N grid panels** (initially Grid1 + Grid2).
2. The left rail includes a **horizontal scrollbar** so additional panels can overflow sideways.
3. When collapsed, the rail becomes a **thin vertical bar/handle** that remains clickable.
4. When expanded, the main content (Grid3 + chart) should **naturally gain remaining width** (reflow is a “nice-to-have”, but should work reasonably by default).

**No third-party controls.**


## Non-goals / Constraints

- Do **not** modify the existing working control; create a **new version** (new XAML + code-behind).
- MVVM is not required, but the structure must not block MVVM later.
- Skip animations for now (optional later).
- Prefer a single horizontal scroll mechanism for the rail:
  - rail ScrollViewer handles horizontal overflow of *panels*
  - DataGrid internal horizontal scrolling should be disabled in-rail to avoid “double horizontal scrollbars”.


## New Control: Naming / Files

Create new control alongside existing one:

- `Controls/TransformDataPanelControllerV2.xaml`
- `Controls/TransformDataPanelControllerV2.xaml.cs`

Namespace (consistent with existing):
- `DataVisualiser.UI.Controls`

Class name:
- `TransformDataPanelControllerV2`


## Layout Requirements

### High-level shell

Use a **Grid** with 3 columns:

- Column 0: `LeftRailColumn` (expanded width configurable; collapsible to handle width)
- Column 1: splitter column (optional `GridSplitter`, width ~5)
- Column 2: main content column (`*`)

### Left rail internal layout

Left rail is itself a Grid with 2 columns:

- Column 0: rail content (scrollable)
- Column 1: always-visible handle (thin ToggleButton), fixed width ~12–16

Inside rail content column:

- A `ScrollViewer` with:
  - `HorizontalScrollBarVisibility="Auto"`
  - `VerticalScrollBarVisibility="Disabled"`
  - `CanContentScroll="False"` (pixel scrolling is fine horizontally)

ScrollViewer content: a horizontal host panel for the rail items:

- Recommended:
  - `ItemsControl` with a horizontal `StackPanel` as its `ItemsPanelTemplate`
- Acceptable for v1:
  - a plain `StackPanel Orientation="Horizontal"` (if dynamic items are not required yet)

### Main content

Main content (column 2) contains:

- TransformGrid3 panel
- Chart container (TransformChartContainerControl)
- Any related panels

Use `Grid` or `DockPanel` inside main content. Avoid a horizontal `StackPanel` that fights resizing.


## Collapse/Expand Behavior (Functional Spec)

State: `IsLeftRailCollapsed` (private bool + ToggleButton is fine)

### Collapsed state

- Rail content (`ScrollViewer`) -> `Visibility.Collapsed`
- `LeftRailColumn.Width` -> fixed handle width (e.g., 12)
- Toggle content indicates expand (e.g., `⮞`)

### Expanded state

- Rail content -> `Visibility.Visible`
- `LeftRailColumn.Width` -> fixed expanded width OR user-resizable width (recommended default: fixed like 650)
- Toggle content indicates collapse (e.g., `⮜`)

### Default widths

- Expanded left rail width: start with a **fixed width** (e.g., 650–900), because rail content may become very wide with N panels.
- Allow resizing via `GridSplitter` between rail and main content.


## DataGrid Scrollbar Policy

To avoid two horizontal scrollbars (rail + each DataGrid):

- Set each rail DataGrid:
  - `HorizontalScrollBarVisibility="Disabled"`
  - keep vertical scrolling `Auto`

This ensures the rail scrollbar is the only horizontal scrolling surface for the left panel set.

If some grids genuinely need horizontal scrolling later, revisit this; for v2 keep it simple.


## Implementation Plan (AI-friendly To-Do Steps)

### Step 1 — Add new control files
- Create `TransformDataPanelControllerV2.xaml` + `.xaml.cs`
- Copy required namespaces from existing control XAML.
- Keep naming conventions for elements consistent with the project.

### Step 2 — Build the Grid-based shell
In XAML, implement:
- Root Grid with 3 columns (`LeftRailColumn`, splitter column, main column).
- Left rail Grid with (rail content column + handle column).
- Optional `GridSplitter` in middle column.
- Main content container in column 2.

### Step 3 — Implement rail scroll container
- Add `ScrollViewer` in rail content column:
  - HorizontalScrollBarVisibility Auto
  - VerticalScrollBarVisibility Disabled
- Add rail host panel inside ScrollViewer:
  - For immediate implementation: `StackPanel Orientation="Horizontal"`
  - For future scalability: `ItemsControl` with horizontal StackPanel ItemsPanelTemplate

### Step 4 — Move/replicate existing Grid1 + Grid2 panels into rail host panel
- Recreate `TransformGrid1Panel` and `TransformGrid2PanelControl` inside the rail host panel.
- Preserve existing DataGrid column definitions.
- Apply DataGrid `HorizontalScrollBarVisibility="Disabled"` to both.

### Step 5 — Recreate Grid3 + chart in main column
- Recreate `TransformGrid3PanelControl` and `TransformChartContainerControl` in main column.
- Keep existing XAML element names if code-behind expects them.
- Ensure main column uses a layout that can stretch horizontally (`Grid` recommended).

### Step 6 — Add the rail toggle handle and wire events
- Add a `ToggleButton` in the rail handle column (always visible).
- Wire `Checked` and `Unchecked` events in code-behind to collapse/expand.

### Step 7 — Implement collapse/expand logic in code-behind
- Create constants:
  - `CollapsedHandleWidth = 12`
  - `DefaultExpandedRailWidth = 700` (or use `ChartUiDefaults` if appropriate)
- Implement:
  - `CollapseLeftRail()`
  - `ExpandLeftRail()`
- Toggle events call those methods.

### Step 8 — Make splitter optional and safe
- Include GridSplitter by default.
- If rail is collapsed, optionally disable splitter; not required for v2 (keep simple).

### Step 9 — Verify UI behavior
Test:
- Expanded: rail shows Grid1+Grid2, rail can scroll horizontally if the panel set exceeds rail width.
- Collapsed: only thin handle remains; main content occupies most width.
- Resizing window: main content width grows/shrinks; rail stays fixed width unless resized via splitter.

### Step 10 — Integrate into app
- Add this new control to wherever the old one was used (temporary swap or feature flag).
- Confirm bindings/events for compute logic still work (or stub if this iteration is layout-only).


## Acceptance Criteria

- New control compiles and renders.
- Left rail can host at least Grid1 + Grid2 and supports horizontal scrolling for overflow.
- Rail collapses into a thin, clickable bar and expands back.
- Main content area gains horizontal space when rail collapses (obvious change; perfect resizing not required).
- No third-party dependencies added.


## Notes / Hints for the AI Agent

- Avoid `StackPanel Orientation="Horizontal"` at the top-level for overall layout; use `Grid`.
- Use fixed expanded width for the rail to prevent `Auto` width expanding to full content width when many panels exist.
- Avoid nested horizontal scrollbars: disable DataGrid horizontal scrolling within rail panels for v2.
