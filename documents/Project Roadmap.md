# PROJECT ROADMAP
**Status:** Sequencing & Execution Authority  
**Scope:** Phase ordering, execution dependencies, and closure criteria  
**Authority:** Subordinate to Project Bible.md and SYSTEM_MAP.md  
**Last Updated:** 2026-01-21  
**Change Note:** Phase 6 decomposed into explicit exploratory & confidence sub-phases

---

## 1. Purpose

This roadmap defines **how the project advances safely over time**.

It answers:

- what is complete
- what is in progress
- what is blocked
- what may proceed
- what must not proceed yet

It does **not** define architecture or semantics.  
It defines **order, discipline, and exit conditions**.

---

## 2. Roadmap Philosophy (Binding for This Document)

- Phases exist to **protect correctness**, not to accelerate delivery
- A phase is not complete until its **closure conditions are provably met**
- Unreachable code is treated as **non-existent**
- Exploratory capability must be **explicitly staged**, not silently accumulated
- The roadmap is **evolutionary**, but never vague

---

## 3. Phase Overview (Authoritative)

---

### Phase 1 — Baseline Stabilization  
**Status:** COMPLETE

**Objective**
- Freeze and understand legacy behavior

**Closure Condition**
- Legacy behavior is stable and reproducible

---

### Phase 2 — Canonical Semantics & Normalization  
**Status:** COMPLETE

**Objective**
- Establish a single semantic authority

**Closure Condition**
- CMS produces deterministic, independently verifiable outputs

---

### Phase 3 — Strategy Migration  
**Status:** COMPLETE  
**Blocked By:** None

**Objective**
- Migrate computation strategies to CMS-based execution

**Closure Condition**
- All strategies are CMS-capable
- All strategies are reachable via orchestration
- Parity coverage exists for all strategies
- Evidence: `documents/reachability-20260121-160503.json`

---

### Phase 3.5 — Orchestration Layer Assessment (CRITICAL)  
**Status:** COMPLETE

**Objective**
- Ensure CMS strategies are actually executed in live pipelines

**Closure Condition**
- CMS execution is observable and provable for all strategies
- Evidence: `documents/reachability-20260121-074430.json`, `documents/reachability-20260121-074900.json`, `documents/reachability-20260121-075301.json`, `documents/reachability-20260121-085510.json`

---

### Phase 4 — Parity Validation (CRITICAL)  
**Status:** COMPLETE  
**Blocked By:** None

**Objective**
- Prove CMS behavior is numerically, structurally, and semantically identical to legacy behavior

**Evidence**
- `documents/reachability-20260121-151124.json`

---

#### Phase 4A — Core Strategy Parity  
**Status:** COMPLETE

**Closure Condition**
- All parity tests pass deterministically
- Difference/Ratio parity is N/A (strategies deprecated)

---

#### Phase 4B — Transform Pipeline Parity  
**Status:** COMPLETE

**Closure Condition**
- Transform results match legacy output for all supported expressions

---

#### Phase 4C — Weekly / Temporal Strategy Migration  
**Status:** COMPLETE

**Closure Condition**
- CMS strategies reachable via services
- Parity verified in pipeline context
- Legacy path preserved behind explicit flag
- Evidence: `documents/reachability-20260121-114215.json`

---

### Phase 5 — Optional End-to-End Parity  
**Status:** OPTIONAL / DEFERRED

**Objective**
- Regression protection against orchestration regressions

---

## 4. Phase 6 — Exploratory & Confidence Capability Expansion (INTENTIONAL)

**Status:** PLANNED / UNOPENED  
**Entry Gate:** Phases 3, 3.5, and 4 fully closed

**Purpose**
Phase 6 introduces **explicit, non-authoritative exploratory capabilities** and **confidence-aware interpretation**, without violating Canonical Law.

This phase is intentionally decomposed to prevent semantic leakage and uncontrolled scope growth.

---

### Phase 6.1 — Interpretive Visual Overlays (Render-Only)

**Scope**
- Dynamic line colouring (hot/cold, increasing/decreasing)
- Through-lines (median, average, reference markers)
- Enhanced hover and tooltip signalling
- Legend-driven visibility toggles

**Constraints**
- Render-only
- No impact on computation
- No influence on derived values

**Closure Condition**
- Overlays are visually correct, reversible, and provenance-neutral

---

### Phase 6.2 — Confidence Annotation (Non-Destructive)

**Scope**
- Statistical identification of atypical data points
- Confidence markers on charts and data views
- Declaration of statistical model and parameters

**Constraints**
- Canonical values remain unchanged
- Confidence is annotation only
- Language avoids claims of correctness or error

**Closure Condition**
- Confidence annotations are visible, explainable, and reversible

---

### Phase 6.3 — Confidence-Aware Interpretation

**Scope**
- Optional exclusion of low-confidence points from overlays
- Optional attenuation / weighting in trend or smoothing views
- Explicit user control over confidence handling

**Constraints**
- Applies only to interpretive overlays
- Never affects CMS or normalization
- Must expose provenance

**Closure Condition**
- Confidence-aware views are opt-in and fully transparent

---

### Phase 6.4 — Structural & Relational Exploration

**Scope**
- Scatter plots and clustering views
- Compositional (stacked) analysis
- Pivot-based inspection (event-relative views)
- Multi-resolution comparative views

**Constraints**
- Non-authoritative
- No inferred identity or meaning
- No automatic promotion of structure to semantics

**Closure Condition**
- Structural views are clearly interpretive and bounded

---

### Phase 6.5 — Rules-Based Option Gating

**Scope**
- Declarative rules controlling UI option availability
- Prevention of invalid or misleading combinations
- Context-aware UI constraints

**Constraints**
- Rules are constraints, not recommendations
- Rules must be explainable
- Rules must not encode semantic truth

**Closure Condition**
- Rule effects are transparent and reversible

---

### Phase 6 Completion Criteria (Global)

Phase 6 is considered complete when:

- All exploratory features are explicitly layered
- Canonical truth remains untouched
- Confidence handling is visible and non-destructive
- Interpretive power increases without semantic erosion

---

### Phase 7 — UI / State / Integration  
**Status:** IMPLEMENTATION COMPLETE, VALIDATION PENDING

**Objective**
- Ensure UI accurately reflects computation, interpretation, and uncertainty

**Closure Condition**
- UI reflects truth and uncertainty without compensation or distortion

---

## 5. Handling Evolving Requirements (Explicit Policy)

Enhancement ideas are:

- welcome
- expected
- non-exhaustive

However:

- They do not auto-enter active phases
- They must be assigned intentionally to a Phase 6 sub-phase (or later)
- They must declare scope, layer, and reversibility

This policy protects both delivery discipline and long-term trust.

---

## 6. Current Critical Path (Authoritative)

1. Open Phase 6 deliberately

---

## 7. Summary

- Early phases protect truth
- Mid phases protect correctness
- Phase 6 protects interpretation
- Evolution is deliberate, not accidental
- Closure means closure

This roadmap defines **how the project grows without lying to itself**.

---

**End of Project Roadmap**
