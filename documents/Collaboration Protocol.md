# COLLABORATION PROTOCOL (Quick Reference)

(Non-Authoritative · Behavioral Guide · Pointer Document)

---

## 1. Purpose

This document provides a **concise interaction reference** for effective collaboration between the user and the assistant.

It exists to:
- reduce friction
- minimize drift
- optimize execution velocity
- provide quick behavioral alignment

It does **not** define architecture, governance, or enforcement.

---

## 2. Authority & Deference

This document is **subordinate** to all of the following:

1. Project Bible.md (architecture)
2. SYSTEM_MAP.md (boundaries & intent)
3. MASTER_OPERATING_PROTOCOL.md (execution & governance)

In any conflict, this document **yields immediately**.

---

## 3. How to Use This Document

Use this document when you are unsure:
- how to phrase a request
- how strict an interaction should be
- why the assistant pauses or asks for files
- which mode the collaboration is currently in

This document is **reference-only**, not something to memorize.

---

## 4. Interaction Modes (Signal-Only)

The collaboration operates in one of the following modes:

- **Design** – conceptual discussion, no code expected
- **Migration** – structured refactor or phased change
- **Debug** – error-driven, minimal speculation
- **Documentation** – document alignment or updates
- **Execution** – drop-in code only, no narrative

If unclear, the assistant should ask.  
If declared, the mode **governs response shape**.

---

## 5. File-First Expectation

- Files provided by the user are authoritative
- No assumptions about structure, helpers, or symbols
- If a required file is missing → execution pauses
- Exact filenames and scopes matter

This rule exists to **prevent imagined code paths**.

---

## 6. Drop-In Safety Preference

When providing implementation guidance, expect one of:

- Full method replacement
- Full class replacement
- Explicit before/after diff

Avoid:
- partial snippets
- “modify this part” instructions
- multi-location changes without enumeration

---

## 7. Verbosity Control (Default-Minimal)

Unless requested otherwise, responses should be:

- concise
- action-oriented
- proportional to the task

No recap, summary, or justification unless explicitly requested.

---

## 8. Decision Lock Awareness

Once something is:
- explicitly agreed
- anchored in code or a document

It is considered **locked** until explicitly reopened.

This prevents conversational backtracking.

---

## 9. When to Escalate

Either party may pause progress to:
- request clarification
- surface drift
- challenge assumptions

Escalation is **cooperative**, not adversarial.

---

## 10. What This Document Is Not

This document is not:
- a checklist
- a governance mechanism
- a substitute for MASTER_OPERATING_PROTOCOL.md
- a place to encode architectural rules

It is intentionally lightweight.

---

**End of Collaboration Protocol**
