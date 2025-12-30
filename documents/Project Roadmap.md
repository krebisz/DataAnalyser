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
**Status:** PARTIALLY COMPLETE (ORCHESTRATION GAP IDENTIFIED)

Migrated strategies:

- SingleMetricStrategy
- CombinedMetricStrategy
- MultiMetricStrategy
- DifferenceStrategy
- RatioStrategy
- NormalizedStrategy

**What Was Completed:**
- Strategies implemented in CMS
- Strategies validated independently (unit tests)
- Strategies pass parity tests in isolation

**Critical Gap Identified:**
- Strategies were migrated **without orchestration layer assessment**
- Orchestration layer (`ChartDataContextBuilder`, `ChartUpdateCoordinator`) was never migrated
- Strategies never tested in unified pipeline context
- CMS-to-legacy conversion happens before strategies receive data
- **Result**: Strategies receive legacy format even when CMS is available

**Original Closure Condition (Flawed):**  
Strategies compile, execute in isolation, and match expected semantics.

**Corrected Closure Condition:**  
Strategies compile, execute in isolation, AND work correctly in unified pipeline with migrated orchestration layer.

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
**Status:** NOT STARTED (BLOCKING PHASE 4)

**Purpose:**  
Assess and migrate the orchestration layer that coordinates strategies in the unified pipeline.

**Why This Phase Exists:**
Phase 3 migrated strategies in isolation, but the orchestration layer was never assessed. When cut-over was attempted (weekly distribution), it exposed that:
- `ChartDataContextBuilder` converts CMS to legacy before strategies receive it
- `ChartUpdateCoordinator` expects legacy format
- `MetricSelectionService` uses legacy data loading
- Strategies never actually receive CMS data in production pipeline

**Required Work:**
1. Map data flow: UI → Service → Strategy
2. Identify all CMS-to-legacy conversion points
3. Design unified cut-over mechanism (`StrategyCutOverService`)
4. Migrate orchestration to handle CMS directly
5. Test one strategy end-to-end (SingleMetricStrategy as reference)
6. Validate unified pipeline handles CMS correctly

**Includes:**
- `ChartDataContextBuilder` - Remove CMS-to-legacy conversion
- `ChartUpdateCoordinator` - Handle CMS data directly
- `MetricSelectionService` - CMS data loading coordination
- `MainWindow.SelectComputationStrategy` - Unified cut-over logic
- `StrategyCutOverService` - Single decision point for all strategies

**Closure Condition:**  
- Orchestration layer handles CMS directly (no conversion)
- SingleMetricStrategy works end-to-end in unified pipeline
- Unified cut-over mechanism established and tested
- One strategy fully migrated and validated in production context

**Guardrail:**
- Phase 3.5 MUST complete before Phase 4 can proceed
- No strategy cut-over until orchestration is migrated
- Test strategies in unified pipeline, not just isolation

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

### Phase 7 — UI / State / Integration  
**Status:** NOT STARTED

Includes:
- ViewModel tests
- State container validation
- Repository / persistence validation

**Guardrail:**
- UI integration is explicitly downstream of semantic correctness
- UI must not compensate for incomplete migration

---

## Current Critical Path (Authoritative - CORRECTED)

**Phase 3.5 - Orchestration Layer Assessment** (BLOCKING)
→ SingleMetricStrategy end-to-end migration (reference implementation)
→ Unified cut-over mechanism (`StrategyCutOverService`)
→ Orchestration layer handles CMS directly
→ **Then**: WeeklyDistributionStrategy CMS cut-over  
→ **Then**: WeekdayTrendStrategy CMS cut-over  
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

**Last Updated:** 2025-01-04  
**Overall Status:** Phase 3.5 (Orchestration Assessment) is blocking. Phase 4 cannot proceed until orchestration layer is migrated and strategies tested in unified pipeline context.
