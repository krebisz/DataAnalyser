WeeklyDistributionStrategy — CMS Migration Execution Instructions

Workspace: DataAnalyser—2025.12.23.2—Dev.CmsMigration
Strategy: WeeklyDistributionStrategy
Mode: Sequential, parity-locked, single-risk steps

Ground Rules (Do Not Reinterpret)

Legacy behavior is authoritative

CMS output must be bit-for-bit equivalent

One step completed and verified before moving on

No work on any other strategy

Tests must not introduce new seams or visibility changes

Structural Invariants (Additive)
Execution Locus Requirement

Every step that declares a success criterion must declare where it executes:

File

Class

Method or explicitly marked Inspection-Only

Steps lacking an execution locus are invalid.

Observability Requirement

A step is verifiable only if its outcome can be observed without inference.
If no safe observability mechanism exists, the step must be marked Inspection-Only or deferred.

Temporary Instrumentation Rule

If a step requires temporary hooks or helpers:

Scope must be explicit

Purpose must be explicit

Removal step must be declared

Step 1 — CMS Strategy Shell

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: constructor only

Do

Create CmsWeeklyDistributionStrategy

Constructor params:

ICanonicalMetricSeries series

DateTime from

DateTime to

string label

Test opportunity

None (structure only)

Observability

Compilation only

Stop when

Code compiles

No logic present

Legacy untouched

User action: Create file, paste skeleton, confirm build succeeds.

Step 2 — Materialize CMS Input

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: internal/private materialization helper

Do

Project CMS samples to in-memory list:

Timestamp (local)

double Value

string Unit

Test opportunity

Conceptual invariant only:

sample order preserved

non-null cardinality preserved

Implementation deferred (no safe public surface yet)

Observability

Inspection-only (code review)

Stop when

Item count equals CMS non-null sample count

Order preserved exactly

User action: Implement projection and perform code inspection.

Step 3 — Apply Legacy Filter Rules

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: internal filter helper (temporary execution hook permitted)

Do

Filter [from, to] inclusive

Exclude null / missing values

Order ascending by timestamp

Test opportunity

Deterministic CMS input

Assertion on filtered count

Prefer to defer formal parity to Step 11

Observability

Temporary internal hook permitted

Inspection-only if no hook is present

Instrumentation lifecycle

Hook introduced: Step 3

Hook removed or subsumed: Step 11

Stop when

Result count matches legacy filtered count

User action: Implement filtering and verify against legacy data.

Step 4 — Weekday Bucketing

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: weekday bucketing helper

Do

Map timestamp → weekday index:

Monday = 0 … Sunday = 6

Populate Dictionary<int, List<double>> DayValues

Test opportunity

Partial parity:

each value in exactly one bucket

sum of bucket counts = filtered count

Observability

Inspection-only or deferred parity

Stop when

Every value appears in exactly one bucket

Bucket sizes match legacy DayValues

User action: Implement bucketing and inspect per-day counts.

Step 5 — Per-Day Statistics

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: per-day stats helper

Do
For weekday indices 0..6:

If bucket empty:

Min = NaN

Max = NaN

Range = NaN

Count = 0

Else:

Compute Min, Max

Range = Max − Min

Count = value count

Test opportunity

Strong parity candidate:

arrays length = 7

NaN placement identical

Observability

Deferred to CMS result snapshot

Stop when

Arrays length = 7

Values equal legacy Mins / Maxs / Ranges / Counts

User action: Implement stats and diff against legacy snapshot.

Step 6 — Global Bounds

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: global bounds helper

Do

GlobalMin = min(non-NaN mins)

GlobalMax = max(non-NaN maxs)

Apply legacy degenerate handling

Test opportunity

Deterministic numeric equality

Observability

Deferred to parity harness

Stop when

Values exactly equal legacy GlobalMin / GlobalMax

User action: Implement bounds and verify exact equality.

Step 7 — Bin Construction

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: bin construction helper

Do

BinSize = FrequencyBinningHelper.CalculateBinSize(GlobalMin, GlobalMax)

Bins = CreateBins(GlobalMin, GlobalMax, BinSize)

Test opportunity

Structural parity:

bin count

boundaries

Observability

CMS result serialization

Stop when

BinSize equals legacy

Bin boundaries identical

Bin count identical

User action: Implement bin logic and compare to legacy bins.

Step 8 — Frequency Calculation

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: frequency calculation helper

Do

For each weekday:

BinValuesAndCountFrequencies(DayValues[day], Bins)

NormalizeFrequencies(FrequenciesPerDay)

Test opportunity

High-value parity:

raw frequencies

normalized frequencies

Observability

CMS result serialization

Stop when

Raw frequencies equal legacy

Normalized frequencies equal legacy

User action: Implement frequencies and verify against legacy maps.

Step 9 — CMS Result Assembly

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: result assembly

Do

Populate CMS WeeklyDistributionResult with all fields.

Test opportunity

Snapshot parity test (preferred)

Observability

JSON diff against legacy snapshot

Stop when

Serialized CMS result matches legacy snapshot exactly

User action: Serialize CMS result and diff against legacy JSON.

Step 10 — Compatibility Result Mapping

Execution locus

File: CmsWeeklyDistributionStrategy.cs

Class: CmsWeeklyDistributionStrategy

Method: compatibility mapping

Do

Build ChartComputationResult

Preserve ordering and units

Test opportunity

Compatibility parity

Observability

Serialized output comparison

Stop when

Serialized result equals legacy output

User action: Map result and compare serialized output.

Step 11 — Parity Harness

Execution locus

Declared parity harness (test or diagnostic runner)

Do

Execute legacy and CMS paths with identical inputs

Compare outputs

Observability

Primary enforcement point

Stop when

Zero diffs

No tolerances used

User action: Run harness and confirm clean parity.

Step 12 — Lock Strategy

Execution locus

Governance / documentation update

Do

Mark strategy as CMS-migrated

Leave legacy code unchanged

Stop when

Parity harness green

Strategy flagged complete

No other strategy touched

User action: Declare migration complete and stop work.

End of instructions.