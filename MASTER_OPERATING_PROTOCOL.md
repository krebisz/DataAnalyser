# MASTER OPERATING PROTOCOL

------------------------------------------------------

## 1. Purpose

This document defines the **operational rules governing how work is conducted** within the project.

It is authoritative for:
- execution discipline
- interaction constraints
- enforcement of collaboration and documentation rules

This document does **not** define architecture. Architectural law resides in the Project Bible.

------------------------------------------------------

## 2. Authority & Precedence

In descending order of authority:

1. Project Bible.md — architectural law
2. SYSTEM_MAP.md — conceptual structure and boundaries
3. MASTER_OPERATING_PROTOCOL.md — execution and governance rules
4. Collaboration Protocol.md — relationship-specific collaboration rules
5. Project Roadmap.md — sequencing and planning

In the event of conflict, higher-authority documents prevail.

------------------------------------------------------

## 3. Scope of This Protocol

This protocol governs:
- how changes are proposed
- how documents are evolved
- how alignment is maintained
- how ambiguity is resolved

It does not:
- prescribe implementation details
- override architectural constraints

------------------------------------------------------

## 4. Explicitness Requirement

All significant decisions, assumptions, or changes must be made explicit.

Implicit behavior, silent assumptions, or inferred intent are not acceptable.

------------------------------------------------------

## 5. Drift Prevention

When conceptual, architectural, or procedural drift is detected:
- forward progress must pause
- the drift must be identified explicitly
- resolution must occur before continuation

Proceeding in the presence of known drift is a protocol violation.

------------------------------------------------------

## 6. Change Proposal Discipline

All non-trivial changes must be:
- proposed explicitly
- scoped clearly
- justified briefly

Approval must be deliberate. Silence does not imply consent.

------------------------------------------------------

## 7. Use of Foundational Documents

Foundational documents encode long-term intent and constraints.

They must be treated with:
- conservatism
- respect for historical context
- preference for additive evolution

------------------------------------------------------

## 8. Correction & Challenge

Either party may:
- challenge assumptions
- request clarification
- halt progress due to misalignment

Challenge is cooperative, not adversarial.

------------------------------------------------------

## 9. Document Evolution Rules

Changes to documents must respect their role and authority.

Higher-authority documents require higher discipline.

------------------------------------------------------

## 9A. Foundational Document Evolution Rule (Additive)

### 9A.0 Definition

Foundational documents include (but are not limited to):
- MASTER_OPERATING_PROTOCOL.md
- Project Bible.md
- SYSTEM_MAP.md
- Project Overview.md
- Project Philosophy.md

These documents encode architectural law, semantic boundaries, and long-term intent.

---

### 9A.1 Non-Destructive Default

When proposing changes to any foundational document, the assistant must:
- treat the existing document as authoritative
- preserve all existing sections verbatim by default
- apply changes only as **additive extensions** unless explicitly instructed otherwise

Removal, condensation, reinterpretation, or restructuring of existing content is **prohibited by default**.

---

### 9A.2 Refactor-by-Proposal Requirement

If the assistant determines that refactoring a foundational document would improve clarity, coherence, or long-term maintainability, it must follow this mandatory sequence:

1. **First**, provide a newly **extended (additive)** version of the document, preserving all original content.
2. **Second**, separately propose a **refactored version** of the document.
3. Provide a **brief justification** for each proposed refactor.

No refactored version may be treated as authoritative unless explicitly approved.

---

### 9A.3 Information Preservation Guarantee

If any refactor proposal would remove or collapse information that is:
- relevant to system understanding, and
- not already present in another workspace-initializing document

The assistant must:
- explicitly identify that information
- propose where it must be re-homed (and added) **before** removal

Loss of relevant information without re-homing is a protocol violation.

---

### 9A.4 Explicit Agreement Requirement

Destructive or restructuring changes to foundational documents may occur **only** when:
- the changes are explicitly proposed
- the impact is clearly stated
- and the user gives deliberate, explicit approval

Absent such approval, additive evolution is the only permitted mode.

------------------------------------------------------

## 10. Enforcement

Violation of this protocol invalidates the affected work and requires correction before continuation.

------------------------------------------------------

## 11. End of MASTER OPERATING PROTOCOL

------------------------------------------------------


---

## Addendum: Foundational Document Governance Reference

This collaboration operates under the **Foundational Document Evolution Rule** defined in `MASTER_OPERATING_PROTOCOL.md` (Section 9A).

Accordingly:
- Foundational documents (e.g., Project Bible, SYSTEM_MAP, MASTER_OPERATING_PROTOCOL) must evolve **additively by default**.
- Any proposal to refactor, condense, or remove content must follow the explicit **refactor-by-proposal** process.
- No destructive or restructuring changes are considered authoritative without **explicit approval**.

This addendum does not supersede the MASTER OPERATING PROTOCOL; it exists solely to ensure alignment during collaboration.

