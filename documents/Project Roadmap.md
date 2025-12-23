# PROJECT ROADMAP  
**DataVisualiser**

---

## Purpose

This roadmap defines **phased execution milestones** for the migration, parity validation, and stabilization of the DataVisualiser system.

It is a **status and sequencing document**, not an architectural authority.

---

## Phase Overview

### Phase 1 — Baseline Stabilization  
**Status:** COMPLETE

- Legacy system understood and frozen
- Core computation paths identified
- Baseline behavior documented
- Initial CMS scaffolding completed

---

### Phase 2 — CMS Core Infrastructure  
**Status:** COMPLETE

- CMS computation pipeline established
- Shared models stabilized
- Helper and normalization logic implemented
- CMS outputs verified for standalone correctness

---

### Phase 3 — Strategy Migration  
**Status:** COMPLETE

- SingleMetricStrategy
- CombinedMetricStrategy
- MultiMetricStrategy
- DifferenceStrategy
- RatioStrategy
- NormalizedStrategy

All strategies implemented in CMS and validated independently.

---

### Phase 4 — Parity Validation (CRITICAL)

**Objective:**  
Ensure CMS behavior is **numerically, structurally, and semantically identical** to Legacy behavior for all migrated features.

---

#### Phase 4A — Core Strategy Parity  
**Status:** COMPLETE

- SingleMetricParityTests
- CombinedMetricParityTests
- MultiMetricParityTests
- Helper parity (Math, alignment, smoothing, normalization)

---

#### Phase 4B — Transform Pipeline Parity  
**Status:** COMPLETE

- TransformExpressionEvaluator parity
- TransformExpressionBuilder parity
- TransformOperationRegistry parity
- TransformDataHelper parity
- TransformResultStrategy parity

All transform semantics and ordering now match Legacy behavior.

---

#### Phase 4C — Weekly / Temporal Strategy Migration  
**Status:** PENDING (BLOCKING PHASE 4 CLOSURE)

Legacy-dependent strategies not yet migrated to CMS:

- WeeklyDistributionStrategy
- WeekdayTrendStrategy

**Required next steps:**
1. Migrate both strategies into CMS
2. Validate basic runtime behavior
3. Implement parity-aligned unit tests
4. Confirm no divergence from Legacy outputs

Phase 4 **cannot be closed** until this sub-phase is complete.

---

### Phase 5 — Optional End-to-End Parity  
**Status:** OPTIONAL / DEFERRED

- Single orchestration-level parity test
- Guards against wiring or ordering regressions
- Not required for Phase 4 sign-off

---

### Phase 6 — Services & Orchestration  
**Status:** NOT STARTED

- Chart coordination services
- Metric selection logic
- Context builders

Deferred until Phase 4 closure.

---

### Phase 7 — UI / State / Integration  
**Status:** NOT STARTED

- ViewModel tests
- State container validation
- Repository / persistence validation

Out of scope until computational parity is complete.

---

## Current Critical Path
WeeklyDistributionStrategy migration
→ WeekdayTrendStrategy migration
→ Corresponding CMS tests
→ Phase 4 closure


---

## Roadmap Integrity Notes

- No architectural changes were introduced during Phase 4
- All updates in this roadmap are **status corrections only**
- Sequence and intent remain unchanged

---

**Last Updated:** 2025-01-23  
**Overall Status:** Phase 4 near-complete; blocked only on weekly / temporal strategy migration
