📘 Workspace Workflow.md

(Updated — Automation Wired In, Strict Mode–Compliant)

⚠️ This document is part of the frozen core documentation set. See MASTER_OPERATING_PROTOCOL.md.

Workspace Initialization & Rehydration Protocol (Authoritative)

This document defines the exact artifacts and step-by-step process required to initialize or refresh a ChatGPT workspace for the DataFileReaderRedux solution.

It ensures architectural consistency, prevents context drift, and allows deterministic rehydration across sessions.

🔒 SECTION 0 — SACRED WORKSPACE WORKFLOWS (IMMUTABLE)
⛔ Sacred Workflow A — Create a New Workspace (Authoritative Sequence)

Follow these steps exactly when starting a new ChatGPT workspace:

Run structural generators:

Generate-ProjectTree.ps1 → produces project-tree.txt

Generate-CodebaseIndex.ps1 → produces codebase-index.md

Generate-DependencySummary.ps1 → produces dependency-summary.md

Verify the following documents are current:

MASTER_OPERATING_PROTOCOL.md

Project Bible.md

Project Overview.md

Project Roadmap.md

SYSTEM_MAP.md (if present)

Prepare the Rehydration Bundle:

project-tree.txt

codebase-index.md

dependency-summary.md

MASTER_OPERATING_PROTOCOL.md

Project Bible.md

Project Overview.md

Project Roadmap.md

SYSTEM_MAP.md (if present)

(Optional) Latest solution ZIP or package

Start a new ChatGPT conversation.

(Optional but Recommended) Declare a workspace label.

At the start of the conversation, the user may declare a workspace identifier in the form:

Workspace: <Project> — <Context> — <Qualifier>


This label defines the identity and scope of the workspace for the duration of the conversation and is treated as the canonical workspace reference.

Upload ALL files from the Rehydration Bundle.

Enter the initialization command:

Initialize workspace with the uploaded files.


Do NOT proceed with any development tasks until ChatGPT explicitly confirms:

Workspace successfully initialized.

⛔ Sacred Workflow B — Refresh an Existing Workspace (Authoritative Sequence)

Follow these steps exactly when updating an existing workspace:

Re-run required generators:

Always run Generate-ProjectTree.ps1

Run Generate-CodebaseIndex.ps1 if code structure changed

Run Generate-DependencySummary.ps1 if dependencies may have changed

Identify which documents require updating:

Update MASTER_OPERATING_PROTOCOL.md only if collaboration rules change

Update Project Bible.md only if architecture changes

Update Project Overview.md only if system capabilities change

Update Project Roadmap.md only if phase or trajectory changes

Update SYSTEM_MAP.md only if structural intent changes

Prepare the Refresh Bundle:

Updated generated artifacts (project-tree.txt, codebase-index.md, dependency-summary.md)

Any updated documentation

(Optional) Updated solution ZIP

Upload the Refresh Bundle to the existing ChatGPT workspace.

Enter the refresh command:

Refresh workspace with the updated files.


Wait for explicit confirmation:

Workspace successfully refreshed.


Proceed with development only after confirmation.

⚠️ Notes (Immutable)

Generated artifacts are authoritative for structure, but not for intent.

Generated files must never be edited by hand.

These workflows MUST NOT be altered without explicit agreement.

These workflows override all other procedural descriptions.

1. Required Rehydration Bundle

Before starting a new ChatGPT workspace, prepare and upload the following files.

A. Mandatory Files

project-tree.txt
Structural listing of the repository.

codebase-index.md
Auto-generated index of namespaces, public symbols, and file roles.

dependency-summary.md
Auto-generated structural dependency overview.

MASTER_OPERATING_PROTOCOL.md
Defines strict operational constraints for ChatGPT.

Project Bible.md
Architectural authority.

Project Overview.md
High-level system summary.

Project Roadmap.md
Phase and evolution guide.

SYSTEM_MAP.md (if present)
Human-authored semantic map bridging intent and structure.

B. Optional but Recommended

Solution ZIP / Package
For deep reference only (ChatGPT cannot extract contents automatically).

2. When to Regenerate Each File

Regenerate project-tree.txt when:

Files or folders change

Regenerate codebase-index.md when:

Public classes, interfaces, or namespaces change

Files are added, removed, or renamed

Regenerate dependency-summary.md when:

New dependencies are introduced

Layering or coupling may have shifted

Update SYSTEM_MAP.md when:

Subsystem boundaries change

New execution paths are introduced

New extension mechanisms are added

3. New Workspace Checklist

Run all required generators

Verify architectural documents

Start a new ChatGPT conversation

(Optional) Declare workspace label

Upload the full Rehydration Bundle

Issue: Initialize workspace with the uploaded files.

Wait for confirmation

4. Workspace Refresh Checklist

Re-run applicable generators

Update documentation only if required

Upload updated artifacts

Issue: Refresh workspace with the updated files.

Wait for confirmation

5. Document Authority Hierarchy

When reconstructing the workspace, ChatGPT interprets documents strictly in this order:

MASTER_OPERATING_PROTOCOL.md (operational truth)

Project Bible.md (architectural truth)

SYSTEM_MAP.md (semantic intent)

Project Overview.md (system summary)

Project Roadmap.md (evolutionary intent)

codebase-index.md (symbolic structure)

dependency-summary.md (structural coupling)

project-tree.txt (raw structure)

Solution ZIP (reference only)

End of Workspace Workflow.md