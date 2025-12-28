WeekdayTrendStrategy — Migration Freeze Reference

Workspace: DataAnalyser—2025.12.23.2—Dev.CmsMigration
Status: FROZEN
Scope: CMS Migration — Parity Preservation Only

Migration Intent

Migrate WeekdayTrendStrategy from Legacy to CMS without altering behavior.

Legacy output is the sole source of truth

CMS implementation must be behaviorally identical

No refactoring, cleanup, or enhancement permitted

Constraints (Binding)

Week start: Monday

Timestamp basis: Local time

CMS input: ICanonicalMetricSeries only

Legacy path must remain intact and selectable

No inferred semantics

No normalization changes

No UI or rendering changes

Required Outputs (Parity Targets)

For CMS implementation to be accepted, outputs must match Legacy for:

Series count

Series ordering

Timestamp alignment

Value sequences

Labels / identifiers

Handling of missing days

Handling of sparse data

Zero / NaN semantics

Frozen Migration Sequence

Inspect legacy WeekdayTrendStrategy behavior

Identify:

Bucketing logic

Aggregation rules

Output shape

Implement CMS equivalent using canonical data only

Add explicit CMS execution path (legacy preserved)

Implement strategy-scoped parity harness

Validate CMS vs Legacy equivalence

Resolve divergences in CMS only

Declare strategy migrated

Leave legacy code dormant but unchanged

Explicit Non-Goals

Performance improvements

Algorithm changes

Data smoothing changes

Visualization changes

API redesign

Test refactors beyond parity coverage

Exit Criteria

WeekdayTrendStrategy is considered migrated only when:

CMS and Legacy outputs are provably equivalent

Parity harness passes without tolerance exceptions

No undocumented behavior differences exist

This document is authoritative until Phase 4 closure.