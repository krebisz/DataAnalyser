# DataVisualiser Refactor TODOs

## Group 1 - Rendering Defaults (quick wins)
- [x] Centralize tooltip timings and sizing constants (RenderingDefaults)
- [x] Centralize chart height calculation constants and hover offsets (RenderingDefaults)
- [x] Centralize distribution service rendering constants (min height, axis padding/rounding)
- [x] Centralize frequency shading colors and max column widths
- [x] Centralize weekday trend palette and bucket count

## Group 2 - UI Defaults (quick wins)
- [x] Centralize common UI labels (toggle text, axis titles)
- [x] Centralize common UI layout sizes (default widths, margins) where reused

## Group 3 - Computation Defaults (medium)
- [x] Centralize smoothing window size and smoothing strategy
- [x] Centralize missing-value fill strategy and defaults
- [x] Centralize ratio/divide-by-zero and normalization policies

## Group 4 - Data Access Defaults (medium)
- [x] Centralize table name mappings and defaults
- [x] Centralize SQL limiting thresholds and sampling policy
