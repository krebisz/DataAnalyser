# COLLABORATION PROTOCOL (Quick Reference)

(Non-Authoritative · Behavioral Guide · Pointer Document · Dormant-but-Valid Collaboration Mode)

---

## 1. Purpose

This document provides a **concise interaction reference** for effective collaboration between the user and the assistant.

It exists to:
- reduce friction
- minimize drift
- optimize execution velocity
- provide quick behavioral alignment
- help future agents understand the preferred interaction posture without loading the full governance stack

It does **not** define architecture, governance, sequencing, vocabulary, or enforcement.

---

## 2. Authority & Deference

This document is **subordinate** to all of the following:

1. Project Bible.md (architecture)
2. SYSTEM_MAP.md (boundaries & structural intent)
3. DataVisualiser-Architectural-Vocabulary.md (architectural grammar & ownership language)
4. Project Roadmap.md (sequencing & phases)
5. MASTER_OPERATING_PROTOCOL.md (execution & governance)
6. Project Overview.md (descriptive orientation)
7. DataVisualiser_Subsystem_Plan.md (active execution authority)

In any conflict, this document **yields immediately**.

---

## 3. How to Use This Document

Use this document when you are unsure:
- how to phrase a request
- how strict an interaction should be
- why the assistant pauses or asks for files
- which mode the collaboration is currently in
- whether a lightweight behavioral reminder is enough before escalating to stricter protocol

This document is **reference-only**, not something to memorize.

---

## 4. Interaction Modes (Signal-Only)

The collaboration operates in one of the following modes:

- **Design** – conceptual discussion, no code expected
- **Migration** – structured refactor or phased change
- **Debug** – error-driven, minimal speculation
- **Documentation** – document alignment or updates
- **Execution** – drop-in code only, no narrative
- **Vocabulary / Ownership** – concept-language, naming, or container-placement discussion
- **Audit-Safe Alignment** – checking that closed phases, baselines, or documents are not being accidentally reopened

If unclear, the assistant should ask.  
If declared, the mode **governs response shape**.

When multiple AI assistants are used in parallel (e.g., Claude Code + Codex), each should operate under the same mode unless explicitly differentiated. Cross-review between agents is encouraged for architectural, vocabulary/ownership, or live-behavior changes.

---

## 5. File-First Expectation

- Files provided by the user are authoritative
- No assumptions about structure, helpers, or symbols
- If a required file is missing → execution pauses
- Exact filenames and scopes matter

This rule exists to **prevent imagined code paths**.

For architecture or ownership questions, the same principle applies to documents: do not guess the governing concept, phase status, or boundary rule if the relevant document has not been loaded.

---

## 5.5 Vocabulary and Ownership Awareness

When the work involves architecture, naming, responsibility placement, boundary movement, or future capability shape:

- use the project vocabulary deliberately
- do not collapse promoted concepts into convenient older terms
- classify responsibility before proposing movement
- distinguish concept work from execution work

Common distinctions:

- capability is not feature
- consumer is not presentation
- interaction is not event wiring
- composition is not builder plumbing
- overlay is not rendering
- provenance is not diagnostics
- authority is not orchestration

This section is a pointer only. The architectural vocabulary document remains the source of truth.


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

Exception: brief grounding updates are acceptable when work spans multiple files, tools, or documents and the user would otherwise lose visibility.

---

## 8. Decision Lock Awareness

Once something is:
- explicitly agreed
- anchored in code or a document

It is considered **locked** until explicitly reopened.

Closed phases and closed preservation baselines should be treated as locked historical state unless the user explicitly asks for historical review.

This prevents conversational backtracking.

---

## 9. When to Escalate

Either party may pause progress to:
- request clarification
- surface drift
- challenge assumptions
- prevent vocabulary or ownership drift
- avoid reopening closed phase work accidentally
- switch from lightweight collaboration to stricter operating protocol

Escalation is **cooperative**, not adversarial.

---

## 10. What This Document Is Not

This document is not:
- a checklist
- a governance mechanism
- a substitute for MASTER_OPERATING_PROTOCOL.md
- a substitute for the architectural vocabulary document
- a place to encode architectural rules

It is intentionally lightweight.

---

## 11. Multi-Agent Collaboration

When the project uses multiple AI assistants simultaneously:

- Each agent should declare its scope before starting work
- Each agent should declare whether it is operating in design, migration, debug, documentation, execution, vocabulary/ownership, or audit-safe alignment mode
- Agents may cross-review each other's output at the user's direction
- Disagreements between agents are resolved by the user, not by the agents
- Plan alignment between agents should be achieved before execution begins
- One agent's implementation may be refined by another if the user directs it
- No agent should assume another agent's architectural classification is correct without checking the relevant source document

---

## 12. When This Protocol Is Still Useful

This document may be dormant during ordinary IDE-integrated or agentic work.

It becomes useful when:

- a future agent needs quick behavioral alignment
- the user wants low-friction collaboration without invoking the full operating protocol
- multiple assistants are coordinating work
- a task is conceptual but not yet execution-heavy
- the interaction needs to stay concise while still respecting project-specific discipline

If the task becomes high-risk, implementation-heavy, recovery-oriented, or phase-sensitive, escalate to the stricter governing documents.


---

**End of Collaboration Protocol**
