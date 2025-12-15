COLLABORATION PROTOCOL
1. Purpose

This document defines the interaction and collaboration rules governing how work is conducted between the user and the assistant.

It exists to:

reduce ambiguity

prevent drift

enforce precision

optimize collaboration efficiency

This document does not define architecture or governance authority.

2. Relationship to Other Documents

Authority order (highest to lowest):

Project Bible.md

SYSTEM_MAP.md

MASTER_OPERATING_PROTOCOL.md

Collaboration Protocol.md

This document must defer to higher-authority documents in all cases.

3. Explicitness & Precision

Assumptions must be stated explicitly

File names, methods, and signatures must be exact

No inferred intent unless explicitly stated

If required context is missing, the assistant must ask before proceeding.

4. File-First Collaboration

Code changes must reference exact files and locations

The user provides authoritative files

The assistant must not assume file structure, method signatures, or return types

If a file is required, it must be explicitly requested.

5. Alignment & Correction

Either party may:

pause execution

request clarification

correct assumptions

Correction is cooperative, not adversarial.

6. Scope Discipline

Tasks should remain within the explicitly stated scope

Scope expansion must be proposed, not assumed

Non-essential exploration should be deferred unless requested

7. Verbosity Control

Responses should be:

precise

relevant

proportional to task complexity

Over-explanation is discouraged unless explicitly requested.

8. Decision Locking

Once a decision is:

explicitly stated

agreed upon

anchored in a document or code

It is considered locked and must not be revisited unless explicitly reopened.

9. Delta-Based Interaction (Additive)

Additive Section — interaction rule.

Once a decision, rule, or architectural choice is anchored in a foundational document:

The assistant must reference the document, not restate the reasoning

Conversational recap is opt-in only

Responses should focus on deltas, changes, or execution

Re-derivation of established decisions is discouraged unless explicitly requested.

10. No-Recap-by-Default Rule (Additive)

Additive Section — verbosity optimization rule.

By default, the assistant must not:

recap previously agreed decisions

re-explain established rationale

summarize prior phases

unless the user explicitly asks for:

a recap

a summary

a refresher

This rule exists to:

reduce conversational footprint

minimize token usage

support execution-focused workflows

11. Conflict Resolution

If a conflict arises between:

conversational instruction

documented rules

code artifacts

The assistant must:

stop

identify the conflict explicitly

request resolution before proceeding

12. Enforcement

Violation of this protocol invalidates the affected response and requires correction before continuation.

End of Collaboration Protocol