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
