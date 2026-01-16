# PROJECT BIBLE
**Status:** Canonical  
**Scope:** Architectural Law

---

## 1. Purpose

The Project Bible defines binding architectural law for the system.

It establishes:

- non-negotiable principles
- semantic authority boundaries
- long-term invariants

If a decision conflicts with this document, the decision is invalid.

---

## 2. Core Principles (Binding)

### Lossless Ingestion
Raw data must never be destroyed or silently altered.

### Explicit Semantics
Meaning is assigned declaratively, not inferred.

### Single Semantic Authority
Normalization is the sole arbiter of meaning.

### Determinism
Identical inputs produce identical outputs.

### Reversibility
Evolution must allow rollback.

---

## 3. Semantic Authority

Metric meaning:

- is defined once
- is stable
- is opaque to consumers

No downstream layer may reinterpret or override semantic decisions.

---

## 4. Identity Law

Metric identity:

- must be canonical
- must be globally unique
- must not encode source-specific information

Identity resolution is declarative and explainable.

---

## 5. Normalization Law

Normalization:

- is staged
- is ordered
- is explicit

Each stage:

- has a single responsibility
- must not inspect concerns outside its scope

---

## 6. Canonical Metric Series Law

CMS:

- is the only trusted analytical input
- is required for computation
- is assumed correct by consumers

---

## 7. Prohibited Behaviors

The system must never:

- infer semantic meaning implicitly
- conflate structure with meaning
- allow heuristic logic to alter identity
- permit silent semantic drift

---

## 8. Evolution Constraints

Future changes must:

- preserve declared boundaries
- maintain semantic authority
- avoid architectural shortcuts

Convenience must not override correctness.

---

## 9. Structural / Manifold Analysis Constraint (Additive · Binding)

Additive Section — No existing sections are modified by this addition.

The system MAY introduce future analytical subsystems that:

- analyze structural similarity, equivalence, or hierarchy
- operate on normalized or canonical representations
- support exploratory or comparative analysis

Such subsystems MUST NOT:

- assign or alter canonical metric identity
- modify normalization outcomes
- influence computation or rendering implicitly

Any promotion of insights into canonical semantics MUST:

- be explicit
- be declarative
- be reviewable
- be reversible

No automatic or implicit back-propagation is permitted.

---

## 10. Temporal Execution & Migration Law (Additive · Binding)

Additive Section — Introduced to support phased migration without violating Canonical Law.

### 10.1 Temporal Coexistence

The system MAY temporarily support multiple execution paths (e.g. legacy and CMS-based) **provided that**:

- Canonical law remains authoritative
- No semantic authority is duplicated
- Coexistence is explicitly bounded and observable

Temporal coexistence is a migration state, not a steady-state design.

---

### 10.2 Migration Non-Violation Rule

During migration phases:

- Legacy execution is permitted only as a compatibility reference
- CMS execution is the sole forward path

Legacy paths MUST NOT:

- define new semantics
- diverge behaviorally without parity visibility
- be silently preferred over CMS logic

---

### 10.3 Parity as Architectural Safety Mechanism

Parity testing is a structural safety tool, not a heuristic convenience.

Parity exists to:

- validate behavioral equivalence
- expose divergence explicitly
- protect Canonical assumptions during transition

Parity mechanisms MUST:

- be isolated from production execution
- be deterministic
- never influence live computation

---

### 10.4 Execution Reachability Constraint

A computation path that is:

- unreachable
- unobservable
- or conditionally bypassed

is considered architecturally non-existent, regardless of correctness.

Migration work MUST prove execution reachability.

---

## 11. Service & Rendering Layer Constraint (Additive · Binding)

Additive Section — Clarifies downstream boundaries.

Service and rendering layers:

- consume results
- do not define semantics
- do not select meaning
- do not infer correctness

They MAY:

- switch between explicitly declared strategies
- visualize Canonical outputs
- reflect migration state

They MUST NOT:

- conditionally reinterpret Canonical results
- embed semantic branching logic

---

## 12. Canonical Boundary Enforcement Rule (Additive · Binding)

Additive Section — Prevents silent law erosion.

Any code that:

- constructs Canonical objects
- mutates Canonical series
- selects Canonical strategy

MUST reside within explicitly designated Canonical boundaries.

Violation constitutes architectural breach.

---

## 13. Declarative Mapping & Non-Static Semantics Rule (Additive · Binding)

Additive Section — Prevents static, manual semantic bottlenecks.

Canonical identity and semantic mappings MUST be:

- declarative
- centralized
- dynamically resolved at runtime
- stored in authoritative data sources (e.g., mapping tables)

The system MUST NOT rely on hardcoded or manually curated mappings as a steady-state mechanism.

Temporary static mappings are permitted only as short-lived migration scaffolding and MUST:

- be explicitly marked as temporary
- be replaced by declarative sources as soon as feasible
- not become a hidden dependency

This rule applies to all legacy-to-canonical identity bridges.

---

## 14. Confidence & Reliability Law (Additive · Binding)

Additive Section — Supports explicit treatment of data quality **without violating Canonical Law**.

### 14.1 Core Rule: Confidence Is Not Semantics

The system MAY compute confidence / reliability assessments over ingested, normalized, or canonical values.

Such assessments:

- do not alter meaning
- do not alter identity
- do not mutate Canonical values
- do not rewrite history

Confidence is an annotation layer, not a semantic layer.

---

### 14.2 Permitted Confidence Mechanisms

The system MAY implement statistical and rule-based detection of atypical readings, including:

- outlier detection under declared statistical models
- noise / variance classification
- local-window anomaly detection
- detection of missingness or suspicious gaps

These mechanisms must be:

- explicitly declared (model + parameters)
- deterministic under identical inputs and configuration
- auditable and explainable

---

### 14.3 Non-Destructive Action Policy (Mandatory)

Permitted downstream actions:

- visual marking (e.g., “low confidence”)
- optional exclusion from specific computations
- optional attenuation / weighting in trend assessments

Prohibited actions:

- deleting raw records
- mutating Canonical values
- silently excluding points
- promoting confidence outcomes into normalization or identity

All exclusions or attenuation MUST be:

- explicit
- reversible
- visible at the point of interpretation

---

### 14.4 Canonical Immunity Constraint

Confidence mechanisms MUST NOT influence:

- metric identity resolution
- canonical mapping
- normalization rule selection
- CMS construction

Confidence operates **after** meaning assignment.

---

### 14.5 Separation of Truth vs Overlay

The system MUST preserve separation between:

- **Truth layers**: Raw → Normalization → CMS
- **Derived layers**: transforms, aggregates, compositions
- **Interpretive overlays**: trends, clusters, pivots, confidence annotations

Interpretive overlays are:

- non-authoritative
- non-promotable without declaration
- incapable of mutating truth layers

---

### 14.6 Language Constraint (Binding)

User-facing language MUST NOT imply ground-truth invalidation.

Avoid:
- “invalid”
- “wrong”
- “incorrect data”

Prefer:
- “statistically atypical under selected model”
- “low confidence”
- “flagged for review”

---

### 14.7 Provenance Requirement

Any confidence-influenced computation MUST expose:

- statistical model used
- parameters / thresholds
- whether data was marked, attenuated, or excluded
- result classification (raw / smoothed / robust / weighted)

---

## Appendix A. Architectural Rationale (Non-Binding)

Earlier designs relied on inferred meaning, leading to drift and ambiguity.

Declarative semantics guarantee:

- stability
- auditability
- long-term trust

---

## Appendix B. Derived & Dynamic Metric Identity (Non-Binding)

Derived metrics:

- have their own identity
- declare units, dimensions, provenance
- never mutate source identities

Promotion is explicit, reviewable, and reversible.

---

## Appendix C. Contract-First Semantic Compression (Non-Binding)

Architectural intent is captured in text-first contracts.

Once anchored, contracts are authoritative and prevent semantic drift.

---

**End of Project Bible**
