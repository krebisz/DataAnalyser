PROJECT BIBLE (Markdown)

(Recommended filename: Project-Bible.md)

Project Bible — DataFileReaderRedux Solution

A technical, architectural guide to the unified analytical system.

1. Purpose of This Document

This Bible defines the current architecture, design principles, and future direction of the entire DataFileReaderRedux solution. It serves as the authoritative reference for:

Understanding how the system works

Extending or modifying components

Maintaining architectural integrity during future evolution

Onboarding new development workspaces or collaborators

It combines current state (as of the latest uploaded solution) with the intended architectural trajectory.

2. System Identity
2.1 Core Identity

The system is a General Analytical Engine, composed of two coordinated subsystems:

Subsystem A — Data Ingestion & Unification (DataFileReader project)

A modular, extensible system for:

Reading structured/semi-structured data

Normalizing heterogeneous sources into unified, comparable internal models

Interpreting domain-specific semantics (health metrics today; arbitrary domains tomorrow)

Subsystem B — Visualization & Computation (DataVisualiser project)

A flexible analytical front-end capable of:

Parameterized computations

Visual transformations

Dynamic analytical comparisons

Strategy-driven data interpretation

Multi-dimensional data exploration

The architecture emphasizes domain-agnostic capability, even if the current active use case centers on health metrics.

3. Architectural Philosophy

This solution is built intentionally around principles derived from modular, emergent, and adaptive system design:

3.1 Generalization Over Specificity

Whenever practical, components transition from domain-specific to generalized, reusable abstractions, reducing:

Complexity

Duplication

Bloat

Fragmentation

3.2 Progressive Modularization

Subsystems evolve by identifying common conceptual cores and extracting them into:

Strategies

Helpers

Engines

Computation pipelines

3.3 Extensibility by Construction

Every component should be replaceable or extendable without restructuring the rest of the system.

3.4 Separation of Concerns

Clear delineation exists between:

Data ingestion

Data normalization

Computation

Rendering

UI orchestration

Domain-specific logic (constrained to thin layers)

3.5 Emergence-Inspired Architecture

The system is not an intelligent agent, but it embraces:

hierarchical layering

composability

abstract representations

uniform treatment of objects (data or conceptual)

future potential for emergent analytical behaviors

4. Subsystem A — DataFileReader (Ingestion & Unification Layer)

This subsystem transforms heterogeneous input data into normalized, structured internal forms.

4.1 Key Responsibilities

Parse raw files (CSV, JSON, Samsung formats, etc.)

Normalize metric values

Build consistent representations across time and sources

Provide generalized data structures usable by any domain

Offer an extensible parser architecture

4.2 Core Components
4.2.1 Parsers

SamsungCsvParser

SamsungJsonParser

LegacyJsonParser

IHealthFileParser (interface)

These share a conceptual interface but will eventually generalize into domain-agnostic parsing pipelines.

4.2.2 JSON Abstraction Layer

(JsonArray, JsonObject, JsonValue, IJson)
A minimal structural abstraction over JSON, enabling:

Dynamic introspection

Consistent object modeling

Robust handling of unknown schemas

4.2.3 Metadata & Hierarchy Models

MetaData, MetaDataList, HierarchyObject, HierarchyObjectList

Provide:

Contextual descriptions of data

Internal organization

Mapping between hierarchical datasets

Comparers for normalization

4.2.4 Normalization Pipeline

Helpers include:

DataNormalization

TimeNormalizationHelper

MetricTypeParser

AggregationPeriod

These convert diverse time-series or categorical datasets into consistent, comparable sequences.

4.2.5 Services Layer

Key services include:

FileProcessingService

MetricAggregator

They orchestrate parsing + normalization + output construction.

5. Subsystem B — DataVisualiser (Computation & Visualization Layer)

This is the user-facing analysis platform integrating computation strategies, rendering, reactive UI state, and comparative analytics.

5.1 Architectural Summary

This subsystem turns normalized data into:

Computed analytical representations

Visual charts

Comparative metric views

Weekly distribution analyses

Multi-series charts

Dynamic, strategy-driven transformations

It separates computation from visualization and visualization from UI orchestration.

5.2 Major Architectural Areas
5.2.1 Strategies Layer

Core computational strategies:

SingleMetricStrategy

NormalizedStrategy

DifferenceStrategy

RatioStrategy

CombinedMetricStrategy

WeeklyDistributionStrategy

All implement or extend:

IChartComputationStrategy

Strategies output abstract computation models independent of UI representation.

5.2.2 Computation Engine

ChartComputationEngine orchestrates:

Delegating to strategies

Producing ChartComputationResult

Managing multi-series computations

Handling edge cases and data smoothing

5.2.3 Rendering Engine

ChartRenderEngine translates computation results into:

LiveCharts series

Axis models

Color palettes

Tooltip integration (ChartTooltipManager, etc.)

This allows complete decoupling between “what is computed” vs. “how it is visualized”.

5.2.4 Data Access

DataFetcher retrieves query results via:

SQL reading

Domain-agnostic data queries

General metric acquisition

5.2.5 State Layer

UiState, MetricState, ChartState represent:

Current metric selections

Chart type / mode

User interactions

Selections, filters, and ranges

These unify interactions across the entire UI.

5.2.6 ViewModels

The key orchestrator is:

MainWindowViewModel

This binds UI actions to:

state changes

strategy changes

data refresh

chart rendering

5.2.7 Front-End (XAML)

MainWindow.xaml defines:

Chart areas

Metric selectors

Strategy dropdowns

Computation mode selectors

Powerful but straightforward, and fully supported via ViewModel binding.

6. Cross-System Architecture
6.1 Data Flow
Raw File → Parser → Normalizer → Unified Data Model → Strategy → Computation Engine → Render Engine → UI

6.2 Extension Points

Add new parsers

Add new strategies

Add new computation modes

Add new rendering styles

Add new data domains

7. Long-Term Architectural Direction

Fully domain-agnostic ingestion

Generalized computation graph

Pluggable data sources (not just files)

Incremental reflection toward emergent analytical capability

Reuse of DataVisualiser patterns in larger intelligent architectures

8. Versioning & Workspace Workflow Alignment

This Bible is designed to remain stable across:

Workspace refreshes

Context rebuilding

Future code evolution

It contains everything needed for ChatGPT or a developer to reorient themselves to the project.

END OF PROJECT BIBLE