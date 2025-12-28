MASTER OPERATING PROTOCOL
1. Purpose

This document defines the operational rules governing how work is conducted within the project.

It is authoritative for:

execution discipline

interaction constraints

enforcement of collaboration and documentation rules

This document does not define architecture.
Architectural law resides in the Project Bible.

2. Authority & Precedence

In descending order of authority:

Project Bible.md — architectural law

SYSTEM_MAP.md — conceptual structure and boundaries

MASTER_OPERATING_PROTOCOL.md — execution and governance rules

Collaboration Protocol.md — collaboration quick-reference

Project Roadmap.md — sequencing and planning

In the event of conflict, higher-authority documents prevail.

3. Scope of This Protocol

This protocol governs:

how work proceeds

how changes are proposed

how documents evolve

how ambiguity is resolved

how collaboration is enforced

It does not:

prescribe implementation details

override architectural constraints

4. Explicitness Requirement

All significant decisions, assumptions, or changes must be explicit.

Implicit behavior, silent assumptions, inferred intent, or “obvious” steps are not acceptable.

5. Failure & Recovery Protocol (Additive)

This section defines mandatory behavior when execution failure, misalignment, or systemic breakdown is detected.

5.1 Failure Classification

A failure must be classified into one or more of the following:

Grounding Failure — missing, stale, or assumed code / documents

Execution Locus Failure — no clear file / method where work occurs

Drop-In Violation — partial or ambiguous implementation guidance

Protocol Drift — deviation from declared workflow or phase rules

Parity Integrity Failure — mismatch between legacy and CMS behavior

Context Saturation Failure — operator burden exceeds protocol safeguards

Unclassified failure is itself a protocol violation.

5.2 Mandatory Failure Declaration

Upon detection of failure:

Forward progress must stop

Failure type(s) must be named explicitly

The last known stable state must be identified

The workspace enters RECOVERY MODE

Proceeding without declaration is prohibited.

5.3 Recovery Mode Rules

While in RECOVERY MODE:

No new features or extensions may be introduced

Only stabilization, alignment, or rollback actions are permitted

All guidance must be:

file-explicit

method-explicit

drop-in safe

Narrative, speculative, or exploratory guidance is disallowed.

5.4 Re-Grounding Requirement

Before exiting RECOVERY MODE, the assistant must provide:

Explicit list of files re-verified

Explicit statement of current system truth

Explicit identification of remaining unknowns

Assumptions reset to UNKNOWN unless re-verified.

5.5 Recovery Exit Conditions

RECOVERY MODE may only exit when all are true:

Compilation succeeds

All tests pass

Parity status explicitly declared

Canonical documents match execution reality

If any condition cannot be met, the workspace remains OPEN / UNSYNCED.

5.6 Failure Retrospective Obligation

Before workspace termination after failure:

A concise failure summary must be produced

Root causes must be mapped to protocol sections

At least one preventative protocol adjustment must be proposed

Failure without retrospective is incomplete work.

6. Drift Prevention

When conceptual, architectural, or procedural drift is detected:

forward progress must pause

the drift must be identified explicitly

resolution must occur before continuation

Proceeding with known drift is a protocol violation.

7. Change Proposal Discipline

All non-trivial changes must be:

proposed explicitly

scoped clearly

justified briefly

Silence does not imply consent.

8. Use of Foundational Documents

Foundational documents encode long-term intent and constraints.

They must be treated with:

conservatism

respect for historical context

preference for additive evolution

9. Correction & Challenge

Either party may:

challenge assumptions

request clarification

halt progress due to misalignment

Correction is cooperative, not adversarial.

10. Document Evolution Rules

Changes to documents must respect their role and authority.

Higher-authority documents require higher discipline.

10A. Foundational Document Evolution Rule (Additive)
10A.1 Non-Destructive Default

Existing content is authoritative

Changes are additive unless explicitly approved otherwise

Removal, reinterpretation, or compression is prohibited by default

10A.2 Refactor-by-Proposal Requirement

If refactoring is proposed:

Provide an additive extension first

Provide a refactored version separately

Justify the refactor explicitly

No refactor becomes authoritative without approval.

10A.3 Information Preservation Guarantee

If information would be removed:

it must be identified

it must be re-homed first

Loss without re-homing is a protocol violation.

10B. Mandatory Grounding & Assumption Prohibition
10B.1 Grounding Requirement

Before proposing:

architectural changes

refactors

new components

new workflows

abstractions

the assistant must explicitly list:

current system state

files inspected

symbols verified

frozen vs evolvable boundaries

10B.2 Assumption Prohibition

If information is not present, it is UNKNOWN.

The assistant must not:

infer intent

assume structure

extrapolate architecture

rely on pattern expectation

10B.3 Grounding Failure Stop Condition

If grounding is missing or challenged:

progress halts

grounding is corrected

Only then may work continue.

10C. Execution Discipline Extensions
10C.1 Workspace Initialization Discipline

Workspace state must be declared:

documents loaded

code snapshot or commit reference

active phase

parity status

Work may not proceed until workspace is explicitly locked.

Missing artifacts are a hard stop.

10C.2 Assumption Suppression Gate

Before implementation guidance:

list files inspected

list symbols verified

list unknowns

Absent files or symbols → stop and request upload.

Conceptual or assumed code is prohibited during active phases.

10C.3 Change Atomicity & Drop-In Safety

All guidance must be:

full method replacement, or

full class replacement, or

explicit before/after diff

Prohibited:

partial snippets

vague “modify this”

unnumbered multi-location changes

10C.4 Strategy Migration Protocol (Phase 4)

A strategy is not migrated unless:

legacy path preserved

CMS constructor added

conversion isolated

parity harness wired (disabled)

activation flag present

equivalence demonstrated

10C.5 Parity as Phase-Exit Artifact

One parity harness per strategy

Explicit activation switch

Diagnostic + strict modes

Phase 4 cannot close without parity sign-off.

10C.6 Document Impact Declaration

All document updates must declare:

why this document

additive vs structural

All updates must be output in full, never fragmented.

10C.7 Velocity Protection (“Proceed Mode”)

When Proceed Mode is active:

output actions, code, or decisions only

no recap

no narrative

no justification unless requested

10C.8 Architectural Boundary Visibility

New dependencies require declaration of:

source project

dependency direction

justification

migration intent

Silent boundary erosion is prohibited.

10C.9 Phase Transition Guardrails

Before phase transition or workspace reset:

documentation aligned

parity obligations closed

reset explicitly justified

Workspace reset is a controlled phase step, not an escape hatch.

10C.10 Interaction Mode Declaration

Interaction mode must be explicit or inferred:

Design

Migration

Debug

Documentation

Execution

Response shape must conform to mode.

10C.11 Test Cycle Discipline (Additive)

When implementing or migrating tests, the following cycle is mandatory:

Identify next test group

Declare required reference files

Upload references

Implement tests

Resolve mismatches

Lock behavior and update specifications

Deviation from this cycle requires explicit justification.

10D. Execution Feasibility & Observability Invariants (Additive)
10D.1 Execution Locus Requirement

Any step with a success criterion is invalid unless it explicitly declares:

where it executes (file / class / method or inspection-only)

Steps lacking an execution locus may not proceed.

10D.2 Observability Requirement

Correctness without observability is treated as non-existence.

Any success criterion must be paired with a declared observability mechanism.

Heuristic verification is prohibited unless explicitly sanctioned.

10D.3 Temporary Instrumentation Rule

Temporary hooks or instrumentation are permitted only if they declare:

scope

purpose

explicit removal step

Undeclared or permanent instrumentation is a protocol violation.

10D.4 No-Closure Rule

A workspace may not be closed while canonical documents are out of sync with execution reality.

If such divergence exists, the workspace enters an OPEN / UNSYNCED state.

10D.5 Response Attenuation Mandate

As operator signal tightens (fatigue, terseness, correction cadence):

verbosity must strictly decrease

protocol adherence supersedes pattern inference

11. Enforcement

Violation of this protocol invalidates the affected work and requires correction before continuation.

12. End of MASTER OPERATING PROTOCOL