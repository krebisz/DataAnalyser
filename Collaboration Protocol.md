📙 Collaboration Protocol

A durable guide for ChatGPT ↔ Developer workflows across sessions and workspaces

1. Purpose

This document ensures:

Continuity across new ChatGPT workspaces

Repeatable onboarding

Stable expectations for code analysis

Minimal management overhead for you

It standardizes how YOU and ChatGPT interact with the solution.

2. The Single Source of Truth

The following files represent canonical knowledge and must always be included in every ZIP upload:

Project Bible.md — detailed technical architecture

Project Overview.md — high-level orientation

Project Roadmap.md — sequenced future trajectory

Project Philosophy.md (this file)

Collaboration Protocol.md (this file)

These collectively act as the "workspace bootstrap kit."

3. Workspace Initialization (New Workspace)

Every new workspace should start with:

Upload the latest zipped DataFileReaderRedux package

Provide the full set of project documents listed above

Instruct ChatGPT:
“Use these documents as the canonical reference for all future context.”

When needed, also provide a clarifying statement:

"If any ambiguity exists, defer to the code first, documents second."

4. Workspace Refresh Workflow (Existing Workspace)

When revisiting an ongoing workspace:

Upload a new ZIP containing the updated codebase

Upload only documents that have changed since the last session

Tell ChatGPT:
“Refresh your internal model using this updated package.”

This refreshes context while avoiding drift.

5. Rules for Document & Code Evolution
5.1 Document Integrity Rules

Project Bible and Overview should evolve deliberately, not reactively

The Roadmap should remain flexible but structured

Philosophy should rarely change (only major shifts)

Collaboration Protocol should change only when workflows break

5.2 Code Interpretation Rules

ChatGPT always applies the following priority:

Current Code

Project Bible

Roadmap

Project Overview

Project Philosophy

This prevents hallucinations and misalignment.

6. Request-Oriented Behavior Expectations

When you request something from ChatGPT, the assistant should:

Re-check the Bible, Overview, Roadmap

Infer context, relationships, and architecture from code

Ask only essential clarifying questions

Produce coherent, integrated output consistent with the overall architecture

Avoid fragmentary or “guessy” interpretations

Maintain stability of naming, structure, and intent

7. Your Role (Developer Responsibilities)

You only need to:

Supply updated ZIPs

Keep essential documents aligned

Decide when major architectural shifts occur

Validate that output remains aligned with your intent

Everything else is handled within the workspace.

8. Boundaries and Expectations

ChatGPT will not:

Create features inconsistent with the architecture

Drift away from the project philosophy

Make unilateral design decisions

Replace existing patterns without a rational improvement trajectory

You will not need to:

Re-explain previous phases

Manage context manually

Address internal inconsistencies caused by assistant drift

9. Roadmap-Driven Collaboration

All future work should tie back to:

the Roadmap phase you are in

or to an explicitly chosen deviation

No new feature should ever appear in isolation.

10. Long-Term Compatibility

This protocol is designed so that:

future versions of ChatGPT

future features such as persistent workspaces

or alternative assistants

can all “plug in” using the same initialization process.

This ensures the project never becomes dependent on one session or model.