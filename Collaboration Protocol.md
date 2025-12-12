> ⚠️ This document is part of the frozen core documentation set. See MASTER_OPERATING_PROTOCOL.md.

🤝 Collaboration Protocol
Purpose

This document defines the principles, expectations, and enforcement rules governing collaboration between the user and ChatGPT.

It does not define technical workflows, file-handling mechanics, or workspace initialization steps.
Those are authoritatively defined elsewhere.

This protocol exists to ensure:

clarity

discipline

mutual correction

long-term coherence in complex work

Authority & Scope

This document is principles-only.

The following documents are authoritative for execution and enforcement:

MASTER_OPERATING_PROTOCOL.md — ChatGPT behavior and constraints

Workspace Workflow.md — workspace initialization and refresh procedures

In the event of any conflict, those documents take precedence.

Collaboration Mode

All collaboration is conducted under Strict Mode.

This implies:

no assumptions

no inferred context

no speculative code or reasoning

explicit acknowledgment of missing or unreadable inputs

deterministic, auditable steps

This discipline exists to support clarity, creativity, and emergent understanding in complex work, not to suppress exploration or constrain legitimate experimentation.

Strict Mode is not optional and is not relaxed implicitly.

Generated Artifacts as Shared Ground Truth

Collaboration relies on auto-generated artifacts for structural understanding, including:

project-tree.txt

codebase-index.md

dependency-summary.md

These artifacts are:

authoritative for structure and coupling

preferred over exploratory or speculative requests

regenerated rather than manually edited

Human-authored documents provide intent and meaning, not inventories.

Mutual Responsibility & Correction (IMPORTANT)

Both parties are responsible for maintaining adherence to the established protocols.

Protocol Deviation Handling

If, at any point, the user appears to be acting in conflict with the established protocols, ChatGPT must:

Explicitly flag the deviation

State which protocol or rule is being violated

Pause forward progress

The user will then:

attempt to correct the approach, or

engage in an explicit, ad-hoc resolution to adjust the protocol if needed

Proceeding silently in the presence of a protocol violation is not permitted.

This rule applies equally to:

workflow steps

file-provision rules

Strict Mode constraints

artifact usage

collaboration boundaries

Expectations of ChatGPT

ChatGPT must:

Follow MASTER_OPERATING_PROTOCOL.md without exception

Use provided artifacts and documents as primary inputs

Request missing context explicitly and early

Avoid overreach, assumption, or invention

Challenge ambiguity rather than smoothing it over

Flag protocol violations as defined above

Expectations of the User

The user commits to:

Following the Workspace Workflow

Providing required artifacts and files

Respecting Strict Mode constraints

Accepting protocol flags in good faith

Correcting deviations or resolving them explicitly

The protocol exists to support progress, not to obstruct it.

Relationship to Other Documents

This document works in concert with:

MASTER_OPERATING_PROTOCOL.md — enforcement and AI-side rules

Workspace Workflow.md — procedural steps

Project Bible.md — architectural law

SYSTEM_MAP.md — semantic intent

Generated artifacts — structural truth

No single document stands alone.

Update Policy

This document should change rarely.

Update only when:

collaboration rules change

enforcement expectations evolve

the Strict Mode contract is amended

Do not update for:

tooling changes

workflow refinements

feature development

End of Collaboration Protocol