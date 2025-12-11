📘 UPDATED PROJECT OVERVIEW (2025-12-11)

(Developer-oriented, concise, accurate, forward-looking)

1. What This System Is

DataFileReaderRedux is a dual-subsystem analytical platform designed to:

ingest, normalize, and unify structured/semi-structured datasets,

compute analytical transformations, and

visualize multi-dimensional results interactively.

The system is intentionally domain-agnostic.
Health data is simply the initial exemplar.

2. Subsystem Summary
2.1 DataFileReader — Ingestion + Normalization Layer

Responsibilities:

Reading multiple file formats (CSV, JSON, Samsung formats, legacy formats).

Providing a unified internal structure for downstream computation.

Normalizing timestamps, values, metadata.

Constructing internal models and hierarchies.

Offering extensible parser contracts.

Output of this subsystem: clean, consistent, comparable metric data.

2.2 DataVisualiser — Computation + Visualization Layer

Responsibilities:

Selecting computation strategies.

Transforming ingested data into analytical models.

Rendering charts using LiveCharts.

Orchestrating UI state and user workflow.

Supporting week-level, difference, ratio, smoothing, normalized, and multi-metric strategies.

Output of this subsystem: interactive visual analytics.

3. Data Flow Summary
File → Parser → Normalizer → Unified Metric Model
      → Strategy → Computation Engine → Render Engine → UI


Each step is modular, replaceable, well-bounded.

4. Architectural Highlights

Strict separation of concerns across ingestion, computation, rendering.

Strategy-driven computation layer (Difference, Ratio, Weekly Distribution, etc.).

Rendering engine independent from computation engine.

State-driven UI (MetricState, UiState, ChartState).

Composable helpers & engines (computation, shading, frequency binning, etc.).

Domain-agnostic conceptual core—the system generalizes naturally.

5. Current Capabilities

Multi-series charts

Smoothing + normalization modes

Difference and ratio views

Week-based distributions

SQL-based metric fetching

Dynamic strategy selection

Smart color palette + shading

Tooltip orchestration

Robust metric selection workflow

6. Intended Near-Term Direction

According to the Roadmap:

Generalize ingestion beyond health metrics

Standardize metadata models

Unify computation pipelines (multi-metric first-class support)

Expand chart types

Introduce reflective / adaptive behaviors (Phase 6)

Gradually merge your emergent-architecture research into analytical workflows

The platform is evolving toward a self-organizing analytical engine.

End of Updated Overview
📙 UPDATED PROJECT BIBLE (2025-12-11)

A rigorous, architectural, forward-accurate document intended for:
— onboarding,
— refactoring governance,
— architectural consistency,
— future evolution.