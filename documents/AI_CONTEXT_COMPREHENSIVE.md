# Comprehensive AI Context: DataVisualiser System

**Status**: Living Document  
**Purpose**: Consolidated reference for AI assistants working on this codebase  
**Last Updated**: 2025-01-XX  
**Scope**: Architecture, refactoring history, current state, migration status, risks, and priorities

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture & Boundaries](#architecture--boundaries)
3. [Migration Status & Phases](#migration-status--phases)
4. [Refactoring History](#refactoring-history)
5. [Current State & Metrics](#current-state--metrics)
6. [Known Limitations & Risks](#known-limitations--risks)
7. [Remaining Work & Priorities](#remaining-work--priorities)
8. [Key Components Reference](#key-components-reference)

---

## System Overview

### Purpose
DataVisualiser ingests heterogeneous health data sources, normalizes them into a canonical metric space (CMS), and provides computation, visualization, and analytical capabilities.

### Core Data Flow
```
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
```

### Key Principles
- **Semantic Authority**: Normalization layer is sole authority for metric identity
- **Lossless Ingestion**: Raw data preserved and traceable
- **Reversible Evolution**: Migration supports parallel legacy + CMS paths
- **Boundary Enforcement**: Clear separation between layers

---

## Architecture & Boundaries

### Layer Structure

#### 1. Ingestion Layer
- **RawRecord**: Lossless, uninterpreted observations
- Captures raw fields, timestamps, source metadata
- Immutable and traceable

#### 2. Normalization Layer
- **Sole authority** for semantic meaning
- Deterministic, explicit, rule-driven
- **Metric Identity Resolution**: Assigns canonical metric identity
- Produces **Internal CMS** (normalization-scoped, strongly typed)

#### 3. Canonical Metric Series (CMS)
- **Consumer CMS Interface** (`ICanonicalMetricSeries`): Only CMS surface visible to downstream
- Trusted, semantically-resolved metrics
- **Only valid semantic input** for downstream computation
- Conversion between internal and consumer CMS is explicit, one-way, non-authoritative

#### 4. Computation & Presentation Layer
- Assumes semantic correctness
- Operates on consumer CMS or legacy-compatible projections
- **MUST NOT** assign identity, reinterpret semantics, or influence normalization

#### 5. Orchestration Layer (Critical Migration Component)
**Purpose**: Coordinates strategies within unified pipeline

**Key Components**:
- **ChartDataContextBuilder**: Builds unified context (CRITICAL GAP: converts CMS to legacy)
- **ChartUpdateCoordinator**: Coordinates chart updates (CRITICAL GAP: expects legacy format)
- **MetricSelectionService**: Loads metric data (CRITICAL GAP: uses legacy loading)
- **StrategyCutOverService**: Single decision point for legacy vs CMS (REQUIRED: must be created)

**Migration Gap**: Phase 3 migrated strategies without assessing orchestration layer. Result: CMS converted to legacy before strategies receive it.

#### 6. Transform Pipeline
- **Ephemeral transformations**: User-defined operations over canonical metrics
- **Operations**: Unary (Log, Sqrt), Binary (Add, Subtract, Divide)
- **Infrastructure**: TransformExpression, TransformOperationRegistry, TransformExpressionEvaluator, TransformResultStrategy
- **Boundary**: Does not create canonical identities, does not influence normalization

#### 7. UI / Presentation Layer
**Chart Panel Architecture**:
- **ChartPanelController**: Reusable base component (standardized structure)
- **Chart-Specific Controllers**: MainChartController (migrated), others pending
- **IChartRenderingContext**: Interface decoupling controllers from MainWindow
- **ChartDiffRatio**: Unified chart (Difference/Ratio with toggle)

**Status**: 1/6 charts migrated, 5 remaining

---

## Migration Status & Phases

### Phase Overview

| Phase | Status | Key Points |
|-------|--------|------------|
| **Phase 1: Baseline Stabilization** | ✅ COMPLETE | Legacy system frozen, baseline documented |
| **Phase 2: CMS Core Infrastructure** | ✅ COMPLETE | CMS pipeline established, outputs verified |
| **Phase 3: Strategy Migration** | ⚠️ PARTIALLY COMPLETE | Strategies migrated in isolation; orchestration gap identified |
| **Phase 3.5: Orchestration Assessment** | 🔴 NOT STARTED | **BLOCKING** - Orchestration layer migration required |
| **Phase 4: Parity Validation** | ⚠️ PARTIALLY COMPLETE | Core strategies pass; weekly/temporal pending |
| **Phase 4A: Core Strategy Parity** | ✅ COMPLETE | All core strategy parity tests pass |
| **Phase 4B: Transform Pipeline Parity** | ✅ COMPLETE | Enhanced with "Divide" operation |
| **Phase 4C: Weekly/Temporal Migration** | ⚠️ PARTIALLY COMPLETE | Strategies exist; service/UI cut-over not completed |
| **Phase 5: Optional E2E Parity** | ⚠️ OPTIONAL | Deferred |
| **Phase 6: Services & Orchestration** | 🔴 NOT STARTED | Depends on Phase 3.5 |
| **Phase 7: UI/State/Integration** | 🟡 IN PROGRESS | Chart panel consolidation started |

### Critical Path

**Phase 3.5 - Orchestration Layer Assessment** (BLOCKING)
→ SingleMetricStrategy end-to-end migration (reference)
→ Unified cut-over mechanism (`StrategyCutOverService`)
→ Orchestration layer handles CMS directly
→ **Then**: WeeklyDistributionStrategy CMS cut-over
→ **Then**: WeekdayTrendStrategy CMS cut-over
→ **Then**: Strategy-level parity confirmation in pipeline context
→ **Then**: Phase 4 closure
→ **Then**: Phase 6 eligibility

### Phase 3.5 Requirements (To Start)

1. **ChartRenderingOrchestrator**: Replace direct strategy instantiation with `IStrategyCutOverService.CreateStrategy()`
2. **WeeklyDistributionService**: Remove manual cut-over logic, delegate to `StrategyCutOverService`
3. **Main Chart Rendering**: Extract to use `StrategyCutOverService` for single/multi/combined metric strategies

**Risk Assessment**: Medium risk - parameter mapping differences, behavioral changes, complex logic translation

---

## Refactoring History

### Completed Unifications

1. **Timeline Generation** - Unified across 7+ time-series strategies
2. **Smoothing** - Unified across 7+ time-series strategies
3. **Unit Resolution** - Unified across 13 strategies (all strategies)
4. **Data Alignment** - Unified in `StrategyComputationHelper` (2 strategies)
5. **Chart Diff/Ratio Operations** - Unified ChartDiff and ChartRatio into ChartDiffRatio with operation toggle
6. **Transform Operations** - Added "Divide" operation to TransformOperationRegistry
7. **Chart Panel UI Structure** - Created reusable `ChartPanelController` component

### Services Created

- **ITimelineService** / **TimelineService**: Unified timeline generation with caching
- **ISmoothingService** / **SmoothingService**: Unified smoothing algorithms
- **IUnitResolutionService** / **UnitResolutionService**: Unified unit resolution
- **IDataPreparationService** / **DataPreparationService**: Unified data preparation and filtering
- **IStrategyCutOverService** / **StrategyCutOverService**: Unified cut-over mechanism

### UI Components Created

- **ChartPanelController**: Reusable base component for chart panels
- **MainChartController**: Chart-specific controller (migrated)
- **IChartRenderingContext**: Interface for chart data/state access
- **ChartRenderingContextAdapter**: Adapter bridging MainWindow to interface

### Metrics

- **Strategies Refactored**: 13 strategies
- **Services Created**: 5 new services
- **UI Components Created**: 4 new components
- **Charts Consolidated**: 2 charts unified (ChartDiff + ChartRatio → ChartDiffRatio)
- **Charts Migrated**: 1 chart migrated to new UI structure
- **Transform Operations**: 3 binary operations (Add, Subtract, Divide), 2 unary (Log, Sqrt)
- **Duplication Eliminated**: ~50+ duplicate code blocks + ~100+ lines of duplicate chart panel UI
- **Lines of Code Reduced**: ~200+ lines of duplicate code removed + ~50+ lines of duplicate chart panel structure

---

## Current State & Metrics

### Build Status
✅ **Compiles successfully** with 0 errors

### Test Status
- **Parity Tests**: 13/14 passing (1 known failure with mismatched counts in CombinedMetricParityTests)
- **Unit Tests**: Strategy tests pass in isolation
- **Integration Tests**: Not yet created

### Code Organization
- **Namespaces**: Well-organized with `Services/ChartRendering`, `Services/Transform`, `Services/WeeklyDistribution`
- **Abstractions**: Clear interface/implementation separation
- **Dependencies**: Proper dependency injection patterns

### Known Issues

#### 1. Canonical Mapping Limitation
- **Issue**: `CanonicalMetricMapping` doesn't distinguish between metric subtypes
- **Impact**: Multi-subtype scenarios map to same canonical ID
- **Workaround**: Single-subtype scenarios work correctly
- **Future**: Requires architectural changes (extend canonical IDs or add subtype filtering)

#### 2. Orchestration Layer Gap
- **Issue**: CMS converted to legacy before strategies receive it
- **Impact**: Strategies never actually receive CMS data in production
- **Solution**: Phase 3.5 - Migrate orchestration layer

#### 3. Duplicate ChartRenderingOrchestrator Files
- **Issue**: Two files with same class name exist
- **Impact**: Confusion, potential build conflicts
- **Recommendation**: Consolidate into `Services/ChartRendering/ChartRenderingOrchestrator.cs`

---

## Known Limitations & Risks

### Architectural Limitations

1. **Multi-Subtype CMS Support**: Blocked by canonical mapping limitation
2. **Orchestration Layer**: Not migrated, blocking Phase 4 completion
3. **TransformResultStrategy**: Not in StrategyType enum (by design - result renderer, not computation strategy)

### Implementation Risks

#### Phase 3.5 Implementation Risks

**Medium Risk**:
- **Parameter Mapping**: `StrategyCreationParameters` may not match current direct instantiation
- **Behavioral Differences**: `ShouldUseCms()` logic may differ from manual checks
- **Complex Logic Translation**: Main chart rendering (~180 lines) has many edge cases

**Low Risk**:
- **TransformResultStrategy**: No change needed (correctly uses transform pipeline)
- **Normalized Chart**: Already migrated to use `StrategyCutOverService`

**Mitigation**:
- Start with WeeklyDistributionService (simpler, isolated)
- Test thoroughly - verify `ShouldUseCms()` matches current behavior
- Enable parity validation to catch differences early
- Incremental approach - migrate one chart at a time

### Backward Compatibility

✅ **All changes backward compatible** (optional constructor parameters, default implementations)
✅ **No breaking changes** to existing functionality
⚠️ **Consideration**: Services use default implementations if not injected

---

## Remaining Work & Priorities

### 🔴 High Priority - Phase 3.5 (Orchestration Assessment)

**1. ChartRenderingOrchestrator Strategy Instantiation**
- **Issue**: Directly instantiates strategies (`new NormalizedStrategy`, etc.)
- **Refactor**: Use `IStrategyCutOverService.CreateStrategy()` instead
- **Impact**: Unifies cut-over logic, enables Phase 3.5 orchestration

**2. WeeklyDistributionService Manual Cut-Over**
- **Issue**: Manual cut-over logic (`if (useCmsStrategy && cmsSeries != null)`)
- **Refactor**: Delegate to `IStrategyCutOverService.CreateStrategy(StrategyType.WeeklyDistribution, ...)`
- **Impact**: Removes duplicate cut-over logic

**3. Main Chart Rendering Extraction**
- **Issue**: Complex logic in `MainWindow.RenderMainChart` (~180 lines)
- **Refactor**: Extract to use `StrategyCutOverService` for single/multi/combined metric strategies
- **Impact**: Completes orchestrator abstraction

### 🔴 High Priority - Phase 7 (UI/State/Integration)

**4. Migrate Remaining Chart Panels**
- **Issue**: 5 chart panels still use duplicate StackPanel/Border structure
- **Refactor**: Create chart-specific controllers inheriting from `ChartPanelController`
- **Impact**: Eliminates ~250+ lines of duplicate UI code
- **Status**: ChartMain migration completed as proof of concept

**5. Complete ChartDiffRatio Migration**
- **Issue**: Verify all code paths use `TransformResultStrategy`
- **Refactor**: Ensure all rendering paths use unified transform pipeline
- **Impact**: Fully unifies Diff/Ratio operations

### 🟡 Medium Priority - Phase 3 (Strategy Migration)

**6. Strategy Factory Pattern**
- **Issue**: Large switch statements in `StrategyCutOverService`
- **Refactor**: Extract to `IStrategyFactory` implementations
- **Impact**: Easier to extend, better testability

**7. Primary Chart Rendering Service**
- **Issue**: Complex multi-series logic in `MainWindow.RenderMainChart`
- **Refactor**: Extract to `PrimaryChartRenderingService`
- **Impact**: Reduces MainWindow complexity

### 🟢 Lower Priority - Phase 4 (Parity Validation)

**8. Parity Validation Enhancement**
- **Issue**: Simplified validation in `StrategyCutOverService`
- **Refactor**: Use `IStrategyParityHarness` implementations
- **Impact**: Enables proper parity validation

**9. Weekly Distribution Interval Rendering**
- **Issue**: Large method (~200+ lines) with complex rendering logic
- **Refactor**: Extract to `WeeklyIntervalRenderer` class
- **Impact**: Improves testability and maintainability

### Summary Priority Order

1. **Phase 3.5**: #1, #2, #3 (unify cut-over usage) - **BLOCKING**
2. **Phase 7**: #4, #5 (complete UI consolidation)
3. **Phase 3**: #6, #7 (strategy factory pattern, main chart abstraction)
4. **Phase 4**: #8, #9 (parity validation, code organization)

---

## Key Components Reference

### Services

#### StrategyCutOverService
- **Location**: `DataVisualiser/Services/Implementations/StrategyCutOverService.cs`
- **Purpose**: Unified cut-over mechanism for all strategies
- **Methods**: `CreateStrategy()`, `ShouldUseCms()`, `ValidateParity()`
- **Status**: Implemented, supports all strategy types

#### ChartRenderingOrchestrator
- **Location**: `DataVisualiser/Services/ChartRendering/ChartRenderingOrchestrator.cs`
- **Purpose**: Orchestrates chart rendering operations
- **Status**: Uses `StrategyCutOverService` for Normalized chart; needs migration for others

#### WeeklyDistributionService
- **Location**: `DataVisualiser/Services/WeeklyDistributionService.cs`
- **Purpose**: Weekly distribution chart computation and rendering
- **Status**: Has manual cut-over logic; needs migration to `StrategyCutOverService`

### Strategies

#### Core Strategies (Migrated)
- `SingleMetricStrategy` / `SingleMetricCmsStrategy` / `SingleMetricLegacyStrategy`
- `CombinedMetricStrategy` / `CombinedMetricCmsStrategy` / `CombinedMetricLegacyStrategy`
- `MultiMetricStrategy` / `MultiMetricCmsStrategy` / `MultiMetricLegacyStrategy`
- `DifferenceStrategy` / `DifferenceCmsStrategy` / `DifferenceLegacyStrategy`
- `RatioStrategy` / `RatioCmsStrategy` / `RatioLegacyStrategy`
- `NormalizedStrategy` / `NormalizedCmsStrategy` / `NormalizedLegacyStrategy`

#### Temporal Strategies (Partially Migrated)
- `WeeklyDistributionStrategy` / `WeeklyDistributionCmsStrategy` / `WeeklyDistributionLegacyStrategy`
- `WeekdayTrendStrategy` / `WeekdayTrendCmsStrategy` / `WeekdayTrendLegacyStrategy`

#### Result Strategies
- `TransformResultStrategy`: Renders transform operation results (not a computation strategy)

### UI Components

#### ChartPanelController
- **Location**: `DataVisualiser/UI/ChartPanelController.xaml` / `.xaml.cs`
- **Purpose**: Reusable base component for chart panels
- **Features**: Title, visibility toggle, behavioral controls, chart content injection
- **Status**: Implemented and tested

#### MainChartController
- **Location**: `DataVisualiser/UI/MainChartController.xaml` / `.xaml.cs`
- **Purpose**: Main chart panel controller
- **Status**: Migrated to use `ChartPanelController`

#### IChartRenderingContext
- **Location**: `DataVisualiser/UI/IChartRenderingContext.cs`
- **Purpose**: Interface for chart data and state access
- **Implementation**: `ChartRenderingContextAdapter` bridges MainWindow/ViewModel

### Transform Infrastructure

#### TransformOperationRegistry
- **Location**: `DataVisualiser/Models/TransformOperationRegistry.cs`
- **Purpose**: Registry of available transform operations
- **Operations**: Add, Subtract, Divide (binary); Log, Sqrt (unary)
- **Status**: Enhanced with "Divide" operation

#### TransformResultStrategy
- **Location**: `DataVisualiser/Charts/Strategies/TransformResultStrategy.cs`
- **Purpose**: Renders transform operation results
- **Status**: Uses unified services (ITimelineService, ISmoothingService, IUnitResolutionService)

### State Management

#### ChartState
- **Location**: `DataVisualiser/State/ChartState.cs`
- **Purpose**: Tracks chart visibility and state
- **Properties**: `IsMainVisible`, `IsNormalizedVisible`, `IsDiffRatioVisible`, `IsDiffRatioDifferenceMode`, etc.

---

## Critical Notes for AI Assistants

### Do's

✅ **DO** use `StrategyCutOverService` for all strategy creation
✅ **DO** use unified services (ITimelineService, ISmoothingService, IUnitResolutionService) in new code
✅ **DO** follow existing patterns when migrating chart panels to `ChartPanelController`
✅ **DO** test parity when making changes to computation logic
✅ **DO** preserve backward compatibility (optional parameters, default implementations)
✅ **DO** update this document when making significant changes

### Don'ts

❌ **DON'T** directly instantiate strategies (use `StrategyCutOverService`)
❌ **DON'T** convert CMS to legacy in orchestration layer
❌ **DON'T** create new chart panels without using `ChartPanelController`
❌ **DON'T** modify normalization layer from computation/presentation layers
❌ **DON'T** treat non-executable components as executable
❌ **DON'T** bypass parity validation during migration

### Migration Patterns

#### Strategy Migration Pattern
1. Create CMS strategy class (if not exists)
2. Create Legacy strategy class (if not exists)
3. Create StrategyFactory implementation
4. Register factory in `StrategyCutOverService`
5. Update orchestration to use `StrategyCutOverService.CreateStrategy()`
6. Enable parity validation
7. Test end-to-end in unified pipeline

#### Chart Panel Migration Pattern
1. Create chart-specific controller inheriting from `ChartPanelController`
2. Move chart-specific UI elements to controller
3. Create behavioral controls (if needed)
4. Wire up events and bindings
5. Replace old StackPanel structure in MainWindow.xaml
6. Update MainWindow.xaml.cs references

---

## Document Maintenance

**When to Update**:
- Phase status changes
- New components/services created
- Significant refactoring completed
- New limitations or risks identified
- Priority changes

**How to Update**:
- Add new sections as needed
- Update status tables
- Add to "Remaining Work" section
- Update "Key Components Reference"
- Maintain chronological order in "Refactoring History"

---

**End of Document**

