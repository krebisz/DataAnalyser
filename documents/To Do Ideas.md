# TODO Enhancements

**Status:** Lightweight idea list  
**Purpose:** Capture possible future enhancements without turning them into a roadmap or formal backlog.

## Compact Legend

| Area | Tags / Questions |
|---|---|
| State | `[seed]` raw idea · `[candidate]` likely useful · `[partial]` partly done · `[done]` completed · `[merge?]` maybe combine · `[drop?]` maybe discard |
| Type | `capability` reasoning/analysis · `interaction` UI/consumer behavior · `delivery` rendering/export/backend · `data` canonical/data/model · `evidence` confidence/provenance/diagnostics |
| Promotion check | What type is it? · Does it preserve provenance/reversibility? · Can it be done as a bounded slice? |

---

## Active / Possible Ideas

### Visual overlays and interaction

- `[partial] [delivery]` Dynamic line colouring based on vertical value, e.g. hot/cold ranges.
- `[seed] [capability+delivery]` Dynamic line colouring based on equivalent trend at a point, e.g. increasing/decreasing.
- `[seed] [interaction+delivery]` Add through-lines on hover for point/median/range references.
- `[seed] [interaction]` Fix tooltip display hover time.
- `[candidate] [capability+evidence]` Detect statistically atypical data points and mark/ignore/attenuate them explicitly.

### Transform and composition

- `[candidate] [capability]` Expand transform graph to ternary operations and more than two submetrics.
- `[candidate] [capability+interaction]` Allow multiple custom transforms / generated graphs in the transform area.
- `[seed] [capability]` Generate dynamic result sets from multiple datasets and operations.
- `[seed] [delivery]` Render multiple derived result sets on one qualified chart surface.

### Time, trend, and pivot analysis

- `[candidate] [interaction]` Rename weekly/hourly/daily trend terminology to remove inconsistency.
- `[candidate] [capability+interaction]` Add daily/hourly option to weekly trend graph with similar Mon–Sun toggles.
- `[candidate] [capability]` Select pivot events and return closest readings, e.g. first heart-rate after exercise.
- `[seed] [capability]` Event-relative before/after inspection windows.

### Distribution and bucketing

- `[candidate] [capability]` Expand distribution chart to use custom modulo bucket intervals.
- `[seed] [capability]` More flexible bucket planning per metric family.
- `[seed] [capability]` Compare distribution shape across time windows.

### Selection and rules

- `[candidate] [interaction]` Rules-based engine governing chart options based on current selections.
- `[seed] [interaction]` Explain why disabled options are disabled.
- `[seed] [interaction]` Save/load chart inspection sessions.

### Data hygiene and naming

- `[candidate] [data]` Format metric/submetric default DB insertion names before saving.
- `[seed] [evidence]` User-facing explanation of confidence model and parameters.

### Future graph / view types

- `[seed] [delivery]` Treemap, e.g. WinDirStat-like magnitude view.
- `[seed] [delivery]` Venn diagram for overlap / set membership.
- `[seed] [delivery]` Sankey / alluvial map for flow or category movement.
- `[seed] [delivery]` Chord diagram for weighted relationships.
- `[seed] [delivery]` Sunburst chart for hierarchical part-whole composition.
- `[partial] [delivery]` Scatter chart for date vs. heart rate / exercise clusters.

---

## Done / Banked

- `[done]` Stack chart to compare summed graphs/values to total, e.g. muscle + fat vs. total weight.
- `[done]` Legends that work as toggling radio buttons for graph display.
- `[done]` Pie charts per resolution, e.g. year/month side-by-side composition.
- `[done]` Metric:submetric selection introduced and expanded toward more chart coverage.
- `[done]` Move original main tab/window contents into separate control.
- `[done]` Decouple main charts controller and subordinate chart flows/controls.
- `[done]` Add average scoring to simple-range distribution graph.
- `[done]` Persist `HealthMetricsCanonical` table in DB across cleans.
- `[done]` Move `disabled`, `metric name`, and `submetric name` from `HealthMetricsCount` to `HealthMetricsCanonical`.

---

## Original Raw List

```markdown
# ENHANCEMENTS

Running list of ideas

---

- Dynamic Line Chart colouring based on vertical value (hot/cold) : PARTIALLY DONE
- Dynamic Lines colouring based on equivalent trends at a point (increasing/decreasing)
- Scatter chart for date vs. heart rate (exercise, etc.) clusters : PARTIALLY DONE
- <!-- Stack chart to compare summed graphs/values to total (e.g.: muscle + fat vs. total weight value) : DONE -->
- <!-- Legends that work as toggling radiobuttons for graph display on charts : DONE -->
- <!-- Pie Chart(s) per resolution (A Pie Chart per year or month to show compositional changes, side by side) : DONE -->
- <!-- Introducing metric:submetric selection present for some charts, to all charts : DONE -->
- <!-- Moving original main tab (window) contents into separate control : DONE -->
- <!-- Decouple main charts controller, and all suboordinate charts, flows and controls : DONE -->
- <!-- Adding "average" scoring to simple range rendering of distribution graph : DONE -->
- Expanding transform graph to ternary operations, with more than two submetrics
- Allowing for more than one graph to be created/generated within transform graph by dynamically adding more custom transforms (new control)
- <!-- Persisting HealthMetricsCanonical table in DB across cleans : DONE -->
- <!-- Moving fields: disabled; metric name; submetric name from HealthMetricsCount to HealthMetricsCanonical in DB : DONE -->
- Modifying metric/submetric name default insertion into DB to undergo some formatting first
- Rename either trends "weekly" to "daily", or "hourly" to "daily" to close the inconsistency
- Add a daily/hourly option to weekly trend graph, with similar toggles that exist for Mon to Sun
- Select "pivot events" upon which a chosen metric will bring back closest readings for (e.g.: First heartrate after every exercise found)
- Fixing tooltip display hover time
- Adding "through" lines when hovering over point-specific info (e.g.: real data point, or on median line when added for simple range in distribution)
- Expand distribution chart to use custom modulo bucket intervals
- Rules based engine governing list of options available to each chart based on current selections made
- Statistical determination of inaccurate data points, their clear indication on the UI, and what to do with them (ignore or mark graphically, or reduce impact on trend assessments)


GRAPH TYPES
-Treemap (Like windir for file sizes on system)
-Venn Diagram
Senke/Alluvial Maps
-Chard Diagram
-Sunburst Chart

```
