# SYSTEM MAP

Status: Canonical
Scope: Conceptual Structure & Boundary Law

---

## 1. Purpose

This document defines the conceptual structure of the system, its major layers, and the relationships between them.

It is authoritative for:

- semantic boundaries
- responsibility separation
- present and future system shape

It is **not** an implementation guide.

---

## 2. High-Level System Overview

The system is designed to ingest heterogeneous data sources, normalize them into a canonical, deterministic metric space, and support computation, visualization, and future analytical extensions.

The architecture is deliberately layered to ensure:

- lossless ingestion
- explicit semantic authority
- reversible evolution

---

## 3. Core Data Flow (Authoritative)

External Sources  
↓  
Ingestion (Raw)  
↓  
Normalization (Semantic)  
↓  
Canonical Metric Series (CMS)  
↓  
Computation / Aggregation  
↓  
Presentation / Visualization

Each stage has **exclusive responsibility** for its concern.

---

## 4. Ingestion Layer

### 4.1 Raw Record

Represents lossless, uninterpreted observations.

- Captures raw fields, timestamps, and source metadata
- Makes no semantic claims
- Immutable and traceable

---

## 5. Normalization Layer

### 5.1 Purpose

The normalization layer is the **sole authority** for assigning semantic meaning to raw observations.

It is:

- deterministic
- explicit
- rule-driven

### 5.2 Normalization Stages

Normalization is composed of ordered stages, including but not limited to:

- Metric Identity Resolution
- Unit Normalization (future)
- Time Normalization (future)
- Dimensional Qualification (future)

Each stage:

- has a single responsibility
- must not override earlier stages

### 5.3 Metric Identity Resolution

Metric Identity Resolution:

- determines what a metric _is_
- assigns canonical metric identity
- does not infer meaning from values

This stage is declarative, stable, and explainable.

---

## 5.4 Declarative Mapping Source (Additive · Binding)

Additive clarification - prevents static semantic bottlenecks.

Metric identity resolution MUST source legacy-to-canonical mappings from a declarative, authoritative store (e.g., a mapping table).

Hardcoded or manually curated mappings are permitted only as temporary migration scaffolding and MUST be retired once a declarative source exists.

---

## 6. Canonical Metric Series (CMS)

CMS represents trusted, semantically-resolved metrics.

Properties:

- identity is canonical and opaque
- time semantics are explicit
- suitable for computation and aggregation

CMS is the **only valid semantic input** for downstream computation.

---

## 6A. CMS Internal vs Consumer Representation (Additive · Binding)

Additive clarification — no existing sections modified.

Two representations of Canonical Metric Series coexist by design.

### 6A.1 Internal CMS (Normalization Authority)

- Produced by the normalization pipeline
- Strongly typed, normalization-scoped structures
- Used exclusively within ingestion and normalization layers
- Never consumed directly by visualization or UI layers

This representation is the **semantic authority**.

### 6A.2 Consumer CMS Interface (Downstream Boundary)

- Exposed via a consumer-facing interface (e.g. `ICanonicalMetricSeries`)
- The **only CMS surface** visible to downstream systems
- Decouples normalization internals from consumers
- Enables parallel legacy + CMS adoption without leakage

Conversion between internal CMS and consumer CMS is:

- explicit
- one-way
- non-authoritative

Semantic authority **never leaves normalization**.

This boundary is mandatory.

---

## 7. Computation & Presentation Layer

Downstream layers:

- assume semantic correctness
- do not reinterpret meaning
- operate on:
  - consumer CMS, or
  - legacy-compatible projections (during migration)

They MUST NOT:

- assign identity
- reinterpret semantics
- influence normalization outcomes

---

## 7D. Orchestration Layer (Additive · Critical Migration Component)

**Additive clarification** — Identified as critical gap during migration.

### 7D.1 Purpose

The orchestration layer coordinates strategies within the unified pipeline:

- Data flow: UI → Service → Strategy
- Format conversion: CMS ↔ Legacy (during migration)
- Strategy selection: Legacy vs CMS cut-over
- Context building: Unified timeline, alignment, smoothing

### 7D.2 Key Components

**ChartDataContextBuilder**:

- Builds unified context from metric data
- **STATUS**: ✅ COMPLETE - Preserves CMS in context, doesn't convert to legacy
- Stores CMS directly in context for strategies to use
- Legacy data still available for derived calculations (diff/ratio/norm)

**ChartUpdateCoordinator**:

- Coordinates chart updates across multiple charts
- **STATUS**: ✅ COMPLETE - Handles strategies generically (works with both CMS and legacy)
- Uses ChartComputationEngine which accepts any IChartComputationStrategy

**MetricSelectionService**:

- Loads metric data from database
- **STATUS**: ✅ COMPLETE - Coordinates CMS and legacy data loading
- Provides both CMS and legacy data to context builder

**StrategyCutOverService**:

- **STATUS**: ✅ COMPLETE - Implemented and registered for all 9 strategy types
- Single decision point for legacy vs CMS
- Unified cut-over mechanism for all strategies
- Parity validation support included
- Factory-based strategy creation (uses StrategyFactoryBase pattern)

### 7D.3 Migration Gap Status

**Problem**: Phase 3 migrated strategies in isolation without assessing orchestration layer.

**Result**: When cut-over was attempted (weekly distribution), it exposed:

- CMS converted to legacy before strategies receive it
- Strategies never actually receive CMS data in production
- Orchestration layer cannot coordinate CMS and legacy together
- "Migrated" strategies work in isolation but fail in unified pipeline

**Solution**: Phase 3.5 - Orchestration Layer Assessment

**Status**: 70% COMPLETE - Major infrastructure in place, reachability verification pending

**Completed**:
- �o. StrategyCutOverService implemented for all 9 strategy types
- �o. ChartRenderingOrchestrator uses unified cut-over mechanism
- �o. ChartDataContextBuilder preserves CMS (doesn't convert)
- �o. WeeklyDistributionService migrated to use StrategyCutOverService
- �o. ChartUpdateCoordinator handles strategies generically

**Remaining**:
- �o. StrategySelectionService cleanup resolved
- �?� Verify all code paths use StrategyCutOverService

### 7D.4 Boundaries

The orchestration layer:

- MUST NOT convert CMS to legacy (defeats migration purpose)
- MUST provide unified cut-over mechanism (single decision point)
- MUST coordinate CMS and legacy during migration (parallel paths)
- MUST validate parity at cut-over point (safety mechanism)

---

## 7A. Legacy + CMS Parallelism Boundary (Additive · Binding)

Additive clarification — migration-specific.

During migration phases:

- Legacy computation paths and CMS-based paths may coexist
- CMS adoption is explicit and opt-in
- Legacy paths are authoritative **for comparison only**

No computation layer may silently switch semantic inputs.

Parallelism exists to:

- validate equivalence
- protect correctness
- prevent forced migration

---

## 7B. Parity Validation Boundary (Additive · Binding)

Additive clarification — phase-exit semantics.

Parity validation is a **boundary artifact**, not an implementation detail.

- Parity harnesses sit _between_ legacy and CMS computation
- They do not participate in normalization or presentation
- They exist solely to validate equivalence of outcomes

Parity:

- is strategy-scoped
- is explicitly activated
- must not alter computation paths

A strategy is **not considered migrated** until parity is proven.

---

## 7C. Ephemeral Transformations & Derived Metrics (Additive)

Additive clarification — implementation-level but boundary-enforced.

### 7C.1 Transform Operations

The system supports user-defined transformations over canonical metrics:

- Unary operations (e.g. log, sqrt)
- Binary operations (e.g. add, subtract)

Operations apply to **values**, not identities.

### 7C.2 Transform Infrastructure

Transform operations are implemented using expression-tree infrastructure, including:

- TransformExpression
- TransformOperation
- TransformOperationRegistry (supports Add, Subtract, Divide binary operations; Log, Sqrt unary operations) - **Updated**: Divide operation added for Ratio support
- TransformExpressionEvaluator
- TransformExpressionBuilder
- TransformDataHelper
- TransformResultStrategy

**Current Operations** (as of 2025-01-XX):

- **Unary**: Log, Sqrt
- **Binary**: Add, Subtract, Divide (Ratio)

**Note**: TransformDataHelper functionality merged into TransformExpressionEvaluator as part of code consolidation.

Provisioned for future expansion to:

- N-metric expressions
- chained and nested operations

### 7C.3 Transform Results Are Ephemeral

Transform results:

- are explicitly non-canonical
- have no semantic authority
- are never promoted implicitly
- exist only for visualization and exploratory analysis

### 7C.4 Transform Pipeline

Transform operations:

- accept canonical metric data
- build expression trees
- evaluate expressions over aligned data
- apply mathematical operations
- produce ephemeral results with provenance
- feed results into charting pipeline

### 7C.5 Boundaries

The transform layer:

- does not create canonical identities
- does not influence normalization
- does not persist authoritative metrics

---

## 8. Structural / Manifold Analysis Layer (Future / Exploratory)

Additive section — no existing sections modified.

### 8.1 Intent

An optional analytical layer intended to:

- explore structural similarity, equivalence, or hierarchy
- support discovery and comparison
- enable higher-order reasoning

This layer provides **insight, not authority**.

### 8.2 Relationship to Core Pipeline

This layer:

- operates orthogonally to ingestion and normalization
- consumes normalized or canonical data
- does not modify RawRecords or CMS

### 8.3 Non-Authority Constraint (Binding)

Structural analysis MUST NOT:

- assign or alter canonical metric identity
- modify normalization outcomes
- influence computation or rendering implicitly

### 8.4 Promotion Boundary

Insights may be promoted only via:

- explicit, declarative rule changes
- reviewable mechanisms
- reversible processes

No automatic back-propagation is permitted.

---

## 9. Component Executability & Observability Classification (Additive · Binding)

Additive clarification — critical to migration correctness.

All components MUST be classified as exactly one of the following:

### 9.1 Executable Components

- Have a defined instantiation and execution path
- Are reachable from an external driver (UI, service, test)
- May host temporary instrumentation or parity harnesses

### 9.2 Non-Executable Components

- Strategies, helpers, mappers, transformers
- Have no direct execution locus
- Must not be assumed runnable
- Require an external execution surface for observation

**Treating a non-executable component as executable is a protocol violation.**

---

## 10. Architectural Boundary Visibility (Additive · Binding)

When introducing or modifying a cross-project dependency, the following MUST be explicit:

- source project
- dependency direction
- justification
- migration intent (temporary vs permanent)

Silent boundary erosion is prohibited.

---

## 11. Execution Reachability Invariant (Additive · Cross-Document)

A computation path that is:

- unreachable
- unobservable
- or conditionally bypassed

is considered **architecturally non-existent**, regardless of correctness.

Migration work MUST prove execution reachability.

---

## 7E. UI / Presentation Layer (Additive · Binding)

**Additive clarification** — UI consolidation and standardization work.

### 7E.1 Purpose

The UI/Presentation layer provides visualization and user interaction for computed metrics and transforms.

### 7E.2 Chart Panel Architecture

**ChartPanelController** (Reusable Component):

- Base UserControl providing standardized chart panel structure
- Encapsulates: header (title, toggle), behavioral controls, chart content
- Supports dependency injection of rendering context via `IChartRenderingContext`
- Enables consistent UI structure across all chart panels

**Chart-Specific Controllers**:

- `MainChartController` - Main metrics chart
- `NormalizedChartController` - Normalized chart
- `DiffRatioChartController` - Difference/ratio chart
- `WeekdayTrendChartController` - Weekday trend chart
- `DistributionChartController` - Weekly/hourly distribution chart
- `TransformDataPanelController` - Transform results panel

**Benefits**:

- Eliminates duplicate UI code (~50+ lines per chart panel)
- Standardizes chart panel structure
- Enables easier maintenance and future additions

### 7E.3 Chart Consolidation

**ChartDiffRatio** (Unified Chart):

- Consolidates previously separate ChartDiff and ChartRatio charts
- Uses operation toggle (Difference "-" vs Ratio "/")
- Leverages `TransformResultStrategy` with "Subtract" or "Divide" operations
- Unified with transform pipeline infrastructure

### 7E.4 Rendering Context

**IChartRenderingContext**:

- Interface providing access to chart data and state
- Decouples chart controllers from MainWindow implementation
- Enables testability and reusability

**ChartRenderingContextAdapter**:

- Adapter bridging MainWindow/ViewModel to `IChartRenderingContext`
- Provides access to `ChartDataContext` and `ChartState`

### 7E.5 Migration Status

**Completed**:

- ChartPanelController component created (reusable base component)
- All chart controllers migrated to controller-based panels (Main, Normalized, DiffRatio, WeekdayTrend, Distribution, Transform)
- ChartDiffRatio unified (ChartDiff + ChartRatio ��' single chart with operation toggle)
- IChartRenderingContext interface created (decouples controllers from MainWindow)
- ChartRenderingContextAdapter created (bridges MainWindow/ViewModel to interface)

**Remaining** (validation only):

- Repository / persistence validation

**Impact**: ~50+ lines of duplicate UI code eliminated per migrated chart, chart panels consolidated.

### 7E.6 Boundaries

The UI layer:

- MUST NOT perform computation (delegates to strategies)
- MUST NOT assign semantic meaning (consumes canonical/computed data)
- MUST provide clear separation between presentation and computation
- MUST support both legacy and CMS data paths during migration

---

## 12. Evolutionary Direction

Future evolution must:

- preserve ingestion boundaries
- centralize semantic authority
- isolate analytical creativity from canonical truth

Deferred layers may be declared here without implementation commitment.

---

## 13. Structural Invariant (Additive · Cross-Document)

A system component may not be treated as executable unless:

- an explicit execution locus is declared
- an observability mechanism exists

Correctness without observability is treated as non-existence.

---

End of SYSTEM MAP





