# PROJECT ROADMAP

DataVisualiser

Status: Descriptive / Sequencing Authority  
Scope: Phase ordering, execution dependencies, and closure criteria  
Authority: Subordinate to Project Bible.md and MASTER_OPERATING_PROTOCOL.md

---

## Purpose

This roadmap defines **phased execution milestones** for the migration, parity validation, and stabilization of the DataVisualiser system.

It is a **status and sequencing document**, not an architectural authority.

It answers:

- what is complete
- what is blocked
- what is allowed to proceed
- what must not proceed yet

---

## Phase Overview

### Phase 1 — Baseline Stabilization

**Status:** COMPLETE

- Legacy system understood and frozen
- Core computation paths identified
- Baseline behavior documented
- Initial CMS scaffolding completed

**Closure Condition:**  
Legacy behavior stable and reproducible.

---

### Phase 2 — CMS Core Infrastructure

**Status:** COMPLETE

- CMS computation pipeline established
- Shared models stabilized
- Helper and normalization logic implemented
- CMS outputs verified for standalone correctness

**Closure Condition:**  
CMS produces deterministic, independently verifiable outputs.

---

### Phase 3 — Strategy Migration

**Status:** MOSTLY COMPLETE (90%) - ORCHESTRATION GAP ADDRESSED

Migrated strategies:

- SingleMetricStrategy (unified - handles both CMS and legacy)
- CombinedMetricStrategy (unified - handles both CMS and legacy)
- MultiMetricStrategy
- DifferenceStrategy
- RatioStrategy
- NormalizedStrategy
- WeeklyDistributionStrategy
- WeekdayTrendStrategy

**What Was Completed:**

- All 8 strategies implemented in CMS
- Strategies validated independently (unit tests)
- Strategies pass parity tests in isolation
- **Strategy consolidation**: SingleMetric and CombinedMetric unified (single class with dual constructors)
- **Factory pattern consolidation**: StrategyFactoryBase created, all 8 factories refactored
- **Code abstraction**: Common patterns extracted to shared helpers

**Orchestration Gap Status:**

- **ADDRESSED**: Phase 3.5 work has significantly progressed (70% complete)
- StrategyCutOverService implemented for all 8 strategy types
- ChartRenderingOrchestrator uses unified cut-over mechanism
- ChartDataContextBuilder preserves CMS (doesn't convert to legacy)
- WeeklyDistributionService migrated to use StrategyCutOverService

**Remaining Work:**

- Minor cleanup: StrategySelectionService still has 1 direct instantiation
- Verify all code paths use StrategyCutOverService

**Original Closure Condition (Flawed):**  
Strategies compile, execute in isolation, and match expected semantics.

**Corrected Closure Condition:**  
Strategies compile, execute in isolation, AND work correctly in unified pipeline with migrated orchestration layer.

**Current Status:**  
90% complete - orchestration infrastructure in place, minor cleanup remaining.

---

### Phase 4 — Parity Validation (CRITICAL)

**Objective:**  
Ensure CMS behavior is **numerically, structurally, and semantically identical** to Legacy behavior for all migrated features.

Parity is a **safety mechanism**, not a wiring exercise.

---

#### Phase 4A — Core Strategy Parity

**Status:** COMPLETE

- SingleMetricParityTests
- CombinedMetricParityTests
- MultiMetricParityTests
- Helper parity:
  - Math
  - Alignment
  - Smoothing
  - Normalization

**Closure Condition:**  
All core strategy parity tests pass deterministically.

---

#### Phase 4B — Transform Pipeline Parity

**Status:** COMPLETE

- TransformExpressionEvaluator parity
- TransformExpressionBuilder parity
- TransformOperationRegistry parity
- TransformDataHelper parity
- TransformResultStrategy parity

Transform semantics and ordering are now identical to Legacy behavior.

**Closure Condition:**  
Transform results match legacy output for all supported expressions.

---

#### Phase 4C — Weekly / Temporal Strategy Migration

**Status:** PARTIALLY COMPLETE (BLOCKING PHASE 4 CLOSURE)

Legacy-dependent strategies:

- WeeklyDistributionStrategy
- WeekdayTrendStrategy

**Updated factual status (additive clarification):**

- CMS versions of strategies exist
- Unit tests and parity harnesses exist
- Strategy logic correctness largely verified
- **Service/UI cut-over NOT completed**
- **Execution reachability NOT universally guaranteed**
- **Legacy remains authoritative at orchestration level**

This phase exposed **systemic execution-locus ambiguity**, now addressed by protocol updates.

**Remaining Required Steps (Authoritative):**

1. Declare **single cut-over locus** per strategy (file + method)
2. Wire CMS strategy at service boundary **only**
3. Preserve legacy path behind explicit flag
4. Prove reachability (observable execution)
5. Execute parity harness at orchestration level
6. Confirm identical ExtendedResult outputs

**Closure Condition:**  
CMS strategy reachable via service, parity verified, legacy path preserved.

Phase 4 **cannot be closed** until this sub-phase is complete.

---

### Phase 5 — Optional End-to-End Parity

**Status:** OPTIONAL / DEFERRED

- Single orchestration-level parity test
- Guards against wiring or ordering regressions
- Intended as regression protection

**Clarification (Additive):**

- Phase 5 is **not a substitute** for Phase 4C
- Phase 5 must not be used to mask unresolved reachability issues

---

### Phase 3.5 — Orchestration Layer Assessment (CRITICAL GAP)

**Status:** SIGNIFICANT PROGRESS (70%) - MAJOR INFRASTRUCTURE IN PLACE

**Purpose:**  
Assess and migrate the orchestration layer that coordinates strategies in the unified pipeline.

**Why This Phase Exists:**
Phase 3 migrated strategies in isolation, but the orchestration layer was never assessed. When cut-over was attempted (weekly distribution), it exposed that:

- `ChartDataContextBuilder` converts CMS to legacy before strategies receive it
- `ChartUpdateCoordinator` expects legacy format
- `MetricSelectionService` uses legacy data loading
- Strategies never actually receive CMS data in production pipeline

**Completed Work:**

1. ✅ **StrategyCutOverService** - Implemented and registered for all 8 strategy types
   - Unified cut-over mechanism established
   - Parity validation support included
   - Factory-based strategy creation
2. ✅ **ChartRenderingOrchestrator** - Migrated to use StrategyCutOverService
   - Primary chart (SingleMetric, CombinedMetric, MultiMetric) uses unified cut-over
   - Normalized chart uses unified cut-over
   - DiffRatio chart uses unified cut-over (via TransformResultStrategy)
3. ✅ **ChartDataContextBuilder** - Preserves CMS (doesn't convert to legacy)
   - CMS stored in context for strategies to use directly
   - Legacy data still available for derived calculations
4. ✅ **WeeklyDistributionService** - Migrated to use StrategyCutOverService
5. ✅ **ChartUpdateCoordinator** - Handles strategies generically (works with both CMS and legacy)

**Remaining Work:**

1. ⏳ **StrategySelectionService** - Still has 1 direct instantiation (`new MultiMetricStrategy`)
   - Minor cleanup: Replace with StrategyCutOverService call
2. ⏳ **Verification** - Verify all code paths use StrategyCutOverService
   - Search for any remaining direct strategy instantiations

**Includes:**

- ✅ `StrategyCutOverService` - Single decision point for all strategies (COMPLETE)
- ✅ `ChartDataContextBuilder` - Preserves CMS, doesn't convert (COMPLETE)
- ✅ `ChartUpdateCoordinator` - Handles CMS data directly (COMPLETE)
- ✅ `ChartRenderingOrchestrator` - Uses unified cut-over (COMPLETE)
- ⏳ `StrategySelectionService` - Minor cleanup needed (IN PROGRESS)

**Closure Condition:**

- ✅ Orchestration layer handles CMS directly (no conversion) - ACHIEVED
- ✅ Unified cut-over mechanism established and tested - ACHIEVED
- ⏳ All code paths use unified cut-over - MINOR CLEANUP REMAINING
- ✅ Strategies work in unified pipeline context - ACHIEVED

**Guardrail:**

- Phase 3.5 MUST complete before Phase 4 can proceed
- No strategy cut-over until orchestration is migrated
- Test strategies in unified pipeline, not just isolation

**Current Status:**  
70% complete - major infrastructure in place, minor cleanup remaining.

---

### Phase 6 — Services & Orchestration

**Status:** NOT STARTED (DEPENDS ON PHASE 3.5)

**Note:** Phase 3.5 addresses the critical orchestration gap. Phase 6 will handle remaining service-level concerns after orchestration is established.

Includes:

- Chart coordination services (advanced features)
- Metric selection logic (extensions)
- Context builders (optimizations)

**Guardrail:**

- Phase 6 MUST NOT begin until Phase 4 is closed
- Phase 3.5 must complete first (orchestration foundation)
- No service refactors permitted while parity incomplete

---

### Refactoring Plan — File Reorganization, Consolidation & Code Abstraction

**Status:** COMPLETE (100%)

**Purpose:**  
Reorganize codebase structure, consolidate duplicate implementations, and extract common patterns to improve maintainability and reduce code duplication.

**Completed Work:**

1. **File Reorganization (100%)**:

   - All files moved to new directory structure per architectural layers
   - Namespaces updated to match new structure
   - Clear separation: Core, Shared, UI, Validation layers

2. **File Consolidation (100%)**:

   - Strategy consolidation: SingleMetric and CombinedMetric unified (single class with dual constructors)
   - Factory pattern consolidation: StrategyFactoryBase created, all 8 factories refactored
   - Helper merging: TransformDataHelper merged into TransformExpressionEvaluator
   - ~150+ lines of duplicate code eliminated

3. **Code Abstraction (100%)**:
   - StrategyComputationHelper: PrepareOrderedData(), FilterAndOrderByRange() methods
   - CmsConversionHelper: Consistent CMS-to-HealthMetricData conversion
   - ChartHelper: ClearChart() method for consistent chart clearing
   - All strategies and rendering engines updated to use helpers
   - **Distribution pipeline base classes**: BaseDistributionService, BucketDistributionStrategy, CmsBucketDistributionStrategy, BucketDistributionTooltip
   - HourlyDistributionService and WeeklyDistributionService inherit from BaseDistributionService
   - HourlyDistributionTooltip and WeeklyDistributionTooltip inherit from BucketDistributionTooltip
   - ~30 lines of duplicate code eliminated (helpers)
   - ~750+ lines of duplicate code eliminated (distribution pipeline refactoring)

**Impact**:

- ~450+ lines of duplicate code eliminated (refactoring plan + previous work)
- ~750+ additional lines eliminated (distribution pipeline base classes)
- **Total: ~1200+ lines of duplicate code eliminated**
- Improved code organization and maintainability
- Established patterns for future work
- Foundation for easier future migrations
- **Distribution pipeline fully abstracted**: Services, strategies (legacy & CMS), and tooltips all use base classes

**Closure Condition:**  
All files reorganized, strategies consolidated, factories unified, common patterns extracted.

---

### Phase 7 — UI / State / Integration

**Status:** IN PROGRESS (25%)

**Completed Work**:

- ChartPanelController component created (reusable chart panel structure)
- MainChartController migrated (1/6 charts to new structure)
- ChartDiffRatio unified (ChartDiff + ChartRatio → single chart with operation toggle)
- TransformOperationRegistry enhanced (added "Divide" operation)
- IChartRenderingContext interface created (decouples chart controllers from MainWindow)
- ChartRenderingContextAdapter created (bridges MainWindow/ViewModel to IChartRenderingContext)

**Remaining Work**:

- Migrate 5 remaining chart panels to ChartPanelController:
  - ChartNorm (Normalized chart)
  - ChartDiffRatio (Diff/Ratio chart - unified but not migrated to controller)
  - ChartWeekdayTrend (Weekday trend chart)
  - ChartWeekly (Weekly distribution chart)
  - TransformPanel (Transform results panel)
- ViewModel tests
- State container validation
- Repository / persistence validation

**Impact**:

- ~50+ lines of duplicate UI code eliminated per migrated chart
- Foundation established for standardizing all chart panels
- Clear path to eliminate ~250+ more lines of duplicate UI code
- 1/6 charts migrated, 5 remaining

**Guardrail:**

- UI integration is explicitly downstream of semantic correctness
- UI must not compensate for incomplete migration
- UI consolidation work is isolated and doesn't affect computation layers

---

## Current Critical Path (Authoritative - UPDATED)

**Phase 3.5 - Orchestration Layer Assessment** (70% COMPLETE)
→ ✅ Unified cut-over mechanism (`StrategyCutOverService`) - COMPLETE
→ ✅ Orchestration layer handles CMS directly - COMPLETE
→ ✅ ChartRenderingOrchestrator uses unified cut-over - COMPLETE
→ ✅ WeeklyDistributionStrategy CMS cut-over - COMPLETE
→ ⏳ StrategySelectionService cleanup (minor) - IN PROGRESS
→ ⏳ Verify all code paths use StrategyCutOverService - IN PROGRESS
→ **Then**: Complete Phase 3.5 (minor cleanup)
→ **Then**: WeekdayTrendStrategy CMS cut-over (if needed)
→ **Then**: Strategy-level parity confirmation in pipeline context
→ **Then**: Phase 4 closure  
→ **Then**: Phase 6 eligibility

---

## Roadmap Integrity Notes

**Critical Correction (2025-01-04):**

- Phase 3 completion was premature - strategies migrated without orchestration assessment
- Orchestration layer gap identified when weekly distribution cut-over attempted
- Phase 3.5 added to address orchestration layer migration
- Recent failures were **orchestration-layer failures**, not just execution-discipline failures
- Strategies work in isolation but fail in unified pipeline due to orchestration gap

**Root Cause:**

- Strategies migrated assuming orchestration would "just work" once all strategies done
- Reality: Orchestration layer was never assessed or migrated
- CMS converted to legacy before strategies receive it
- Strategies never actually receive CMS data in production pipeline

**Corrected Approach:**

- Phase 3.5: Assess and migrate orchestration layer first
- Test strategies in unified pipeline context, not just isolation
- Establish unified cut-over mechanism before completing strategy migrations
- Then proceed with Phase 4 (parity in pipeline context)

---

**Last Updated:** 2025-01-XX  
**Overall Status:**

- Phase 3.5 (Orchestration Assessment): 70% complete - major infrastructure in place, minor cleanup remaining
- Phase 3 (Strategy Migration): 90% complete - all strategies have CMS implementations, factory pattern established
- Phase 4: 85% complete - Phase 4A and 4B complete, Phase 4C at 75%
- Phase 7 (UI/State/Integration): 25% complete - 1/6 charts migrated, foundation established
- **Refactoring Plan**: 100% complete - file reorganization, consolidation, and code abstraction all complete

**Recent Achievements:**

- Complete file reorganization per architectural layers
- Strategy consolidation (SingleMetric, CombinedMetric unified)
- Factory pattern consolidation (StrategyFactoryBase)
- Code abstraction (common patterns extracted to shared helpers)
- ~450+ lines of duplicate code eliminated
- StrategyCutOverService implemented for all 8 strategy types
- ChartRenderingOrchestrator migrated to use unified cut-over
- ChartDataContextBuilder preserves CMS (doesn't convert)
