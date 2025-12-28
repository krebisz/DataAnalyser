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
**Status:** COMPLETE

Migrated strategies:

- SingleMetricStrategy
- CombinedMetricStrategy
- MultiMetricStrategy
- DifferenceStrategy
- RatioStrategy
- NormalizedStrategy

All strategies:
- implemented in CMS
- validated independently
- not yet required to be reachable from UI/services

**Closure Condition:**  
Strategies compile, execute in isolation, and match expected semantics.

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

### Phase 6 — Services & Orchestration  
**Status:** NOT STARTED

Includes:
- Chart coordination services
- Metric selection logic
- Context builders

**Guardrail:**
- Phase 6 MUST NOT begin until Phase 4 is closed
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

## Current Critical Path (Authoritative)

WeeklyDistributionStrategy CMS cut-over  
→ WeekdayTrendStrategy CMS cut-over  
→ Strategy-level parity confirmation  
→ Phase 4 closure  
→ Phase 6 eligibility

---

## Roadmap Integrity Notes

- No architectural changes were introduced during Phase 4
- Recent failures were **execution-discipline failures**, not design failures
- All updates in this roadmap are **status clarifications**, not re-sequencing
- The roadmap remains valid once protocol enforcement is respected

---

**Last Updated:** 2025-01-23  
**Overall Status:** Phase 4 blocked only by weekly / temporal strategy cut-over and reachability proof
