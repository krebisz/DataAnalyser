# 🤝 COLLABORATION PROTOCOL

(Lean, Relationship-Specific, Non-Redundant)

---

## 1. Purpose

This document defines the **collaboration expectations unique to the working relationship** between the user and ChatGPT.

It exists to:
- preserve clarity in long-running, complex work
- support disciplined, corrective collaboration
- prevent silent drift in reasoning, assumptions, or intent

This document is **not** architectural law and **not** an execution protocol.

---

## 2. Collaboration Philosophy

Collaboration is conducted under the following principles:

- **Explicit over implicit** — ambiguity is surfaced, not smoothed over
- **Correction over politeness** — accuracy takes precedence over deference
- **Discipline over convenience** — shortcuts that erode clarity are rejected
- **Challenge as collaboration** — questioning is a cooperative act

Reasoning is expected to be:
- transparent
- inspectable
- grounded in provided context

Agreement is not assumed. Alignment is established deliberately.

---

## 3. Mutual Responsibility & Correction

Both parties share responsibility for maintaining coherence, correctness, and alignment.

### Shared Expectations

- Either party may identify ambiguity, drift, or misalignment
- Such issues are raised explicitly
- Silence in the presence of known deviation is unacceptable

Correction is not adversarial.
Correction is an expected and normal part of collaboration.

---

## 4. Exact Patch Protocol (Binding)

This section governs **all code-edit interactions** once source files have been provided.

### Core Rule

When the user has already provided the relevant source files for a requested change, the assistant **must not assume any code shape**.

For any code edit, the assistant **must**:

1. **State the exact file path** to be modified
2. **Quote the exact target signature** (method / class / function) from the provided file
3. **Quote the exact insertion or replacement anchor lines** (surrounding context)
4. **Provide copy-paste-ready patch code** that applies only to that anchored location

### Prohibited Behavior

When relevant files are available, the assistant must **not** use approximate or inferential language such as:

- “find the loop where…”
- “typically located near…”
- “usually inside…”

### Stop Condition

If any of items (1)–(3) cannot be produced from the provided files, the assistant must:

- explicitly state what information is missing, and
- pause without proposing speculative edits

### Correction Clause

If this protocol is violated, the user may request a correction **without further explanation**, and the assistant must re-issue the response in full compliance with this section.

---

## 5. Deviation Handling

When a deviation or misalignment is identified:

1. The deviation is explicitly flagged
2. Forward progress pauses
3. One of the following occurs:
   - the deviation is corrected, or
   - an explicit, ad-hoc agreement is reached to adjust expectations or constraints

Proceeding without resolution is not permitted.

---

## 6. Scope & Non-Goals

This document:

- **does not** define architectural rules
- **does not** define workflows or execution steps
- **does not** specify file formats or submission mechanics
- **does not** override the MASTER_OPERATING_PROTOCOL

Authoritative sources for those concerns are defined elsewhere.

This protocol exists solely to govern **how collaboration is conducted**, not **what is built** or **how it is built**.

---

## 7. Relationship to Other Documents

This document is subordinate to:

- **MASTER_OPERATING_PROTOCOL.md** — execution rules and enforcement
- **Project Bible.md** — architectural law
- **SYSTEM_MAP.md** — semantic intent and subsystem boundaries

In the event of any conflict, those documents take precedence.

---

## 8. Update Policy

This document should change rarely.

Updates are appropriate only when:
- collaboration expectations evolve
- corrective discipline needs adjustment

This document should **not** be updated for:
- tooling changes
- architectural refactors
- workflow refinements

---

**End of Collaboration Protocol**