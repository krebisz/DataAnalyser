> ⚠️ This document is part of the frozen core documentation set. See MASTER_OPERATING_PROTOCOL.md.

📙 UPDATED PROJECT BIBLE (2025-12-11)

A rigorous, architectural, forward-accurate document intended for:
— onboarding,
— refactoring governance,
— architectural consistency,
— future evolution.

Project Bible — DataFileReaderRedux Solution
1. Purpose

This document defines the governing architectural rules for the entire analytical system.
It describes:

How the system works today

How subsystems must evolve

What constraints future code must obey

What conceptual principles underpin the architecture

Every refactor, extension, or computation addition must respect this Bible.

2. System Identity
2.1 Core Identity

The system is a general analytical engine built from two coordinated subsystems:

Subsystem A — DataFileReader
Ingestion + Normalization + Unification

Subsystem B — DataVisualiser
Computation + Rendering + Interaction

Together, they form a pipeline from raw data → structured model → analytical insight → interactive visualization.

2.2 Architectural Values

Generalization over specialization

Progressive modularization

Extensibility at every boundary

Predictable state separated from computation

Rendering completely independent of analytics

Emergence-inspired layering and composability

3. Architectural Philosophy

The system respects design principles that deliberately echo your long-term research themes:

3.1 Unified Representation of Heterogeneous Inputs

Anything ingestible should resolve into:

a known structure,

comparable metric sequences,

metadata that links relationships,

hierarchical or temporal context.

3.2 Hierarchical Layering

Every layer only sees the abstractions below it:

Parsers ↔ Normalizers

Strategies ↔ Computation Engine

Computation ↔ Rendering

Rendering ↔ UI

UI ↔ ViewModel State

3.3 Abstractions Before Implementations

Interfaces define every interaction point—especially:

IHealthFileParser → ingestion

IChartComputationStrategy → computation

Renderer interfaces → visualization

State models → UI control

This constraint ensures the system remains evolvable.

3.4 Emergence-Inspired Mechanics (Phase 6+)

Future system evolution will adopt:

metadata-driven selection of strategies

dynamic evaluation of metric topologies

algorithmic clustering of metric groups

self-organizing analytical workflows

reflective computation graph design

These concepts must be considered in any architecture change from here forward.

4. Subsystem A — DataFileReader (Ingestion Layer)
4.1 Responsibilities

File parsing (CSV/JSON/mixed formats)

Schema detection and interpretation

Time normalization

Value normalization

Metadata extraction

Hierarchical structuring of dataset components

Output construction for DataVisualiser

4.2 Major Components
4.2.1 Parsers

SamsungCsvParser

SamsungJsonParser

LegacyJsonParser

SamsungHealthCsvParser

SamsungHealthParser

IHealthFileParser (contract boundary)

4.2.2 JSON Abstraction Layer

JsonObject

JsonArray

JsonValue

IJson

Tiny but powerful layer enabling introspection of arbitrary schemas.

4.2.3 Hierarchy + Metadata Models

HierarchyObject / HierarchyObjectList

MetaData / MetaDataList

MetaDataComparer

These maintain relationships between metrics, timestamps, and sources.

4.2.4 Normalization Helpers

DataNormalization

TimeNormalizationHelper

MetricTypeParser

AggregationPeriod

These enforce internal consistency.

4.2.5 Services

FileProcessingService

MetricAggregator

These combine all other components into unified outputs.

5. Subsystem B — DataVisualiser (Computation & Visualization Layer)
5.1 Roles

This subsystem transforms normalized datasets into analytical/visual models.

5.2 Computation Strategies

Every analytical behavior is a strategy:

SingleMetricStrategy

NormalizedStrategy

DifferenceStrategy

RatioStrategy

CombinedMetricStrategy

WeeklyDistributionStrategy

All strategies implement or extend IChartComputationStrategy.

Strategies define what to compute—not how it will be displayed.

5.3 Computation Engine

ChartComputationEngine:

delegates to strategies

orchestrates multi-metric logic

constructs ChartComputationResult

manages smoothing, scaling, and pipeline consistency

5.4 Rendering Engine

ChartRenderEngine translates computation models into LiveCharts objects:

series generation

axis construction

colors / palettes

tooltip orchestration

shading overlays

The rendering engine is strictly presentation-focused.

5.5 State Layer

The system uses isolated state representations:

UiState — what the user is doing

MetricState — what metrics exist and are selected

ChartState — what the chart is currently configured to show

This separation prevents UI logic from leaking into computation or rendering.

5.6 ViewModels

The central orchestrator:

MainWindowViewModel

Responsibilities:

binding UI interactions to state changes

selecting strategies

triggering data fetch + compute + render cycles

maintaining consistency across subsystems

6. Cross-System Architecture
6.1 Pipeline
Raw Data
 → Parser
 → Normalizer
 → Unified Model
 → Strategy
 → Computation Engine
 → Render Engine
 → UI


Each arrow is a replaceable boundary.

6.2 Extension Points

Developers may add:

new file formats

new normalizers

new metric models

new computation strategies

new visualization modes

new metadata-driven behaviors (Phase 6+)

new shading or annotation modules

new data sources (SQL/API/local)

7. Evolutionary Direction

This portion governs future growth.

7.1 Short Term

complete generalization of ingestion

unify computation engines

modular multi-metric graphing

rendering expansion (histograms, heatmaps)

7.2 Medium Term

metadata-driven analytical selection

heuristic visualization recommendations

reflective computation graphs

dynamic reshaping of analytical pipelines

7.3 Long Term

The system becomes a self-organizing analytical topology that reflects your long-term research themes:

hierarchical remapping

manifold-like restructuring of metric networks

emergent grouping and re-grouping

autonomous selection of analytical pathways

adaptive “purposeful” workflows

End of Updated Project Bible