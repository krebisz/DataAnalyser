Architectural Flow Overview (High-Level Orientation)

This document provides a collapse-first architectural view of the solution.
It is intended to orient an engineer or AI system that has access to the full codebase but needs a correct mental model without manually understanding hundreds of files.

The system is best understood as a pipeline, not as a hierarchy.

Core Mental Model (TL;DR)

Data → Strategy → Computation → Render Model → Render → Tooltip / State Sync → UI

Every major component in the solution exists to serve one stage of this flow.
The perceived complexity comes from many files implementing the same stage in different contexts, not from deep architectural branching.

1. Data Acquisition & Normalization Layer

“Where raw facts enter the system”

Responsibility

Fetch metric data from storage

Normalize timestamps, values, and resolutions

Align datasets for comparison and aggregation

Characteristics

Service-style classes

I/O-bound

Stateful (DB connections, resolution selection, subtype filtering)

Correctly not over-abstracted

Mental Collapse Point

All data access can be treated as:

“Produces normalized HealthMetricData sequences.”

You generally do not need to understand individual fetchers unless debugging data correctness.

2. Strategy Layer (Computation Policy)

“What computation should be done”

Responsibility

Encapsulate business meaning:

weekly aggregation

combined metrics

CMS strategies

unary/binary metric operations

Decide how data is combined or interpreted

Characteristics

Deterministic

Stateless

No UI or rendering knowledge

Each strategy implements a Compute() protocol

Mental Collapse Point

All strategy implementations can be reduced to:

“Given metric data → produce ChartComputationResult.”

Differences between strategies are policy, not architecture.

This is one of the most important collapse points.

3. Computation Result Layer

“Canonical intermediate representation”

Responsibility

Represent the outcome of computation

Carry:

raw values

smoothed values

min/max ranges

timestamps

metadata

Characteristics

Pure data containers

No logic

Shared across many pipelines

Mental Collapse Point

Treat this as:

“The universal intermediate form passed through the system.”

Most downstream logic assumes this shape.

4. Transform & Expression Layer

“Optional secondary math on results”

Responsibility

Apply unary and binary transforms

Support expression-based operations

Maintain legacy fallbacks

Characteristics

Interpreter-like

Stateless

Deterministic

Operates on computation results, not raw data

Mental Collapse Point

This layer can be thought of as:

“Optional post-processing math that outputs another computation result.”

It is a side pipeline, not a mandatory flow step.

5. Render-Model Construction Layer

“Turn computation into renderable intent”

Responsibility

Translate computation results into:

series definitions

axis ranges

stacking/baselines

interval bins

frequency shading instructions

Characteristics

Non-UI

Policy-heavy

Bridges computation → rendering

Mental Collapse Point

This layer answers one question:

“How should this data appear visually?”

This is a major conceptual boundary despite being spread across many files.

6. Rendering Engines

“Draw the chart”

Responsibility

Convert render models into LiveCharts primitives

Handle:

line charts

stacked columns

interval shading

weekly distributions

Characteristics

Stateless

Deterministic

Rendering-only concerns

No business logic

Mental Collapse Point

All rendering logic reduces to:

“RenderModel → chart primitives.”

Renderers differ by presentation, not responsibility.

7. Orchestration / Coordination Layer

“Glue the pipeline together”

Responsibility

Invoke strategies

Build render models

Call renderers

Normalize axes

Sync tooltips

Track timestamps

Characteristics

Pipeline orchestration

Side-effect heavy

Sensitive to refactors

Must remain concrete

Mental Collapse Point

This layer exists to:

“Execute the end-to-end pipeline safely.”

This is where control flow lives.

8. UI State & ViewModel Layer

“User intent and lifecycle”

Responsibility

Manage user selections

Trigger updates

Track loading and error state

Coordinate multiple charts

Characteristics

Transactional

Highly stateful

Integration-heavy

Resists functional decomposition

Mental Collapse Point

ViewModels should be treated as:

“State machines and event routers.”

They are not computation logic.

The Most Useful Collapse Points for AI Assistance

When reasoning about the solution, it is safe (and recommended) to collapse at these boundaries:

All strategies → “Compute() produces ChartComputationResult”

All renderers → “RenderModel → chart primitives”

All data fetchers → “HealthMetricData providers”

All coordinators → “Pipeline orchestration”

ViewModels → “State + intent routing”

This reduces hundreds of files into ~5 conceptual modules without restructuring the codebase.

Why Conceptual Collapsing Matters More Than Restructuring

The codebase contains many sub-folders with conceptually similar responsibilities.
Physically restructuring hundreds of files would be high risk and low ROI.

Conceptual collapsing allows:

correct reasoning

safer AI assistance

targeted refactoring

boundary-respecting changes

…without moving a single file.

One-Sentence Architectural Summary

The system fetches and normalizes metric data, applies computation strategies to produce canonical results, optionally transforms them, converts results into render models, renders charts via specialized engines, and coordinates everything through orchestrators driven by ViewModel state.

This document is intended to be authoritative for orientation purposes.