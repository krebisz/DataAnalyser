# Temporary Export Smoke Checklist

Export reachability after each flow and upload the JSON files.

## Flow 1: Metric-Type Switch

- Switch from one metric type to a different one, then export before loading data again.
- Expected:
  - new metric type sticks
  - date range refreshes to the new metric
  - primary subtype list belongs only to the new metric
  - no recent error is recorded

## Flow 2: Add Subtypes + Load

- On one metric type, leave subtype 1 on its displayed default, add a second subtype, load data, and export.
- Then add a third subtype, load again, and export again.
- Expected:
  - add subtype works without manually reselecting subtype 1
  - selected subtype count matches combo count
  - loaded series count matches selected subtype count

## Flow 3: Clear

- Click `Clear`, then export.
- Expected:
  - date range returns to the default window
  - stale loaded context is gone

## Flow 4: Transform From Existing Two-Series Load

- Load two subtypes, open transform, run a unary operation, switch to a binary operation without reloading, run `=`, then export.
- Expected:
  - transform primary/secondary selections are populated
  - selected operation is captured
  - no recent error is recorded

## Flow 5: Transform After Dynamic Second Subtype

- Load one subtype, open transform, add a second subtype without reloading, run a binary operation with `=`, then export.
- Expected:
  - transform secondary state becomes visible
  - binary transform works after dynamic second-subtype addition
  - no recent error is recorded

## Export Fields I Will Check

- `Diagnostics.UiSurface.MetricType`
- `Diagnostics.UiSurface.DateRange`
- `Diagnostics.UiSurface.Subtypes`
- `Diagnostics.UiSurface.Transform`
- `Diagnostics.UiSurface.RecentMessages`
- `Diagnostics.SmokeChecks`
