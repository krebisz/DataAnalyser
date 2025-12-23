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

5. Drift Prevention

When conceptual, architectural, or procedural drift is detected:

forward progress must pause

the drift must be identified explicitly

resolution must occur before continuation

Proceeding with known drift is a protocol violation.

6. Change Proposal Discipline

All non-trivial changes must be:

proposed explicitly

scoped clearly

justified briefly

Silence does not imply consent.

7. Use of Foundational Documents

Foundational documents encode long-term intent and constraints.

They must be treated with:

conservatism

respect for historical context

preference for additive evolution

8. Correction & Challenge

Either party may:

challenge assumptions

request clarification

halt progress due to misalignment

Correction is cooperative, not adversarial.

9. Document Evolution Rules

Changes to documents must respect their role and authority.

Higher-authority documents require higher discipline.

9A. Foundational Document Evolution Rule (Additive)
9A.1 Non-Destructive Default

Existing content is authoritative

Changes are additive unless explicitly approved otherwise

Removal, reinterpretation, or compression is prohibited by default

9A.2 Refactor-by-Proposal Requirement

If refactoring is proposed:

Provide an additive extension first

Provide a refactored version separately

Justify the refactor explicitly

No refactor becomes authoritative without approval.

9A.3 Information Preservation Guarantee

If information would be removed:

it must be identified

it must be re-homed first

Loss without re-homing is a protocol violation.

9B. Mandatory Grounding & Assumption Prohibition
9B.1 Grounding Requirement

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

9B.2 Assumption Prohibition

If information is not present, it is UNKNOWN.

The assistant must not:

infer intent

assume structure

extrapolate architecture

rely on pattern expectation

9B.3 Grounding Failure Stop Condition

If grounding is missing or challenged:

progress halts

grounding is corrected

only then may work continue

9C. Execution Discipline Extensions
9C.1 Workspace Initialization Discipline

Workspace state must be declared:

documents loaded

code snapshot or commit reference

active phase

parity status

Work may not proceed until workspace is explicitly locked

Missing artifacts are a hard stop

9C.2 Assumption Suppression Gate

Before implementation guidance:

list files inspected

list symbols verified

list unknowns

Absent files or symbols → stop and request upload.

Conceptual or assumed code is prohibited during active phases.

9C.3 Change Atomicity & Drop-In Safety

All guidance must be:

full method replacement, or

full class replacement, or

explicit before/after diff

Prohibited:

partial snippets

vague “modify this”

unenumerated multi-location changes

9C.4 Strategy Migration Protocol (Phase 4)

A strategy is not migrated unless:

legacy path preserved

CMS constructor added

conversion isolated

parity harness wired (disabled)

activation flag present

equivalence demonstrated

9C.5 Parity as Phase-Exit Artifact

One parity harness per strategy

Explicit activation switch

Diagnostic + strict modes

Phase 4 cannot close without parity sign-off

9C.6 Document Impact Declaration

All document updates must declare:

why this document

additive vs structural

All updates must be output in full, never fragmented.

9C.7 Velocity Protection (“Proceed Mode”)

When Proceed Mode is active:

output actions, code, or decisions only

no recap

no narrative

no justification unless requested

9C.8 Architectural Boundary Visibility

New dependencies require declaration of:

source project

dependency direction

justification

migration intent

Silent boundary erosion is prohibited.

9C.9 Phase Transition Guardrails

Before phase transition or workspace reset:

documentation aligned

parity obligations closed

reset explicitly justified

Workspace reset is a controlled phase step, not an escape hatch.

9C.10 Interaction Mode Declaration

Interaction mode must be explicit or inferred:

Design

Migration

Debug

Documentation

Execution

Response shape must conform to mode.

9C.11 Test Cycle Discipline (Additive)

When implementing or migrating tests, the following cycle is mandatory:

Identify next test group

Declare required reference files

Upload references

Implement tests

Resolve mismatches

Lock behavior and update specifications

Deviation from this cycle requires explicit justification.

10. Enforcement

Violation of this protocol invalidates the affected work and requires correction before continuation.

11. End of MASTER OPERATING PROTOCOL