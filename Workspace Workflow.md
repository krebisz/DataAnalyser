📘 Workspace Workflow.md (Updated, Strict Mode–Compliant)

(Fully integrated with MASTER_OPERATING_PROTOCOL.md)

Workspace Initialization & Rehydration Protocol (Authoritative)

This document defines the exact artifacts and step-by-step process required to initialize or refresh a ChatGPT workspace for the DataFileReaderRedux solution.
It ensures architectural consistency, prevents context drift, and allows deterministic rehydration across sessions.

🔒 SECTION 0 — SACRED WORKSPACE WORKFLOWS (IMMUTABLE)
⛔ Sacred Workflow A — Create a New Workspace (Authoritative Sequence)

Follow these steps exactly when starting a new ChatGPT workspace:

Run Generate-ProjectTree.ps1
→ Produces the latest project-tree.txt.

Verify the following documents are current:

MASTER_OPERATING_PROTOCOL.md

Project Bible.md

Project Overview.md

Project Roadmap.md

Prepare the Rehydration Bundle:

project-tree.txt

MASTER_OPERATING_PROTOCOL.md

Project Bible.md

Project Overview.md

Project Roadmap.md

(Optional) Latest solution ZIP or package

Start a new ChatGPT conversation.

Upload ALL files from the Rehydration Bundle.

Enter the initialization command:
“Initialize workspace with the uploaded files.”

Do NOT proceed with any development tasks until ChatGPT confirms:
“Workspace successfully initialized.”

⛔ Sacred Workflow B — Refresh an Existing Workspace (Authoritative Sequence)

Follow these steps exactly when updating a workspace:

Run Generate-ProjectTree.ps1
→ Creates an updated project-tree.txt.

Identify which documents require updating:

Update Bible only if architecture changed.

Update Overview only if system capabilities changed.

Update Roadmap only if the phase or trajectory changed.

Update Master Protocol only if collaboration rules change.

Prepare the Refresh Bundle:

Updated project-tree.txt

Any updated documentation (MASTER_OPERATING_PROTOCOL.md, Bible, Overview, Roadmap)

(Optional) Updated solution ZIP

Upload the Refresh Bundle to the existing ChatGPT workspace.

Enter the refresh command:
“Refresh workspace with the updated files.”

Wait for explicit confirmation:
“Workspace successfully refreshed.”

Proceed with development only after confirmation.

⚠️ Notes (Immutable)

These workflows MUST NOT be altered without explicit agreement.

These workflows override all other procedural descriptions.

Any deviation must be formally documented and merged into this section.

-------------------------------------------
1. Required Rehydration Bundle
-------------------------------------------

Before starting a new ChatGPT workspace, prepare and upload the following files:

A. Mandatory Files
1. project-tree.txt

Generated using Generate-ProjectTree.ps1.
Must reflect current and complete solution structure.

2. MASTER_OPERATING_PROTOCOL.md

Defines strict behavioral constraints for ChatGPT.
Must be included in every workspace initialization.

3. Project Bible.md

The authoritative architecture and subsystem definition.
Updated only when architectural boundaries change.

4. Project Overview.md

High-level system-purpose and data flow summary.

5. Project Roadmap.md

Defines planned phase evolution.

B. Optional but Recommended
Solution ZIP / Package

Created via Create-ProjectZip.ps1 or Create-DataFileReaderRedux-Package.ps1.
Useful for deep structural review (though ChatGPT cannot extract file contents automatically).

-------------------------------------------
2. When to Regenerate Each File
-------------------------------------------
A. Regenerate project-tree.txt when:

Any file is added, deleted, renamed, or moved

Folder structure changes

A refactor touches multiple directories

(Most frequently updated artifact.)

B. Update MASTER_OPERATING_PROTOCOL.md when:

ChatGPT operational rules change

A new workflow mode is introduced

File-ingestion or modification rules evolve

C. Update Project Bible.md when:

Architectural boundaries change

New engines, strategies, or subsystems are added

Core abstractions evolve

D. Update Project Overview.md when:

The system gains new visible capabilities

Data flow or subsystem roles shift

E. Update Project Roadmap.md when:

A new phase begins

A phase definition changes

Long-term direction changes

-------------------------------------------
3. New Workspace Checklist
-------------------------------------------

Use this exact sequence when starting a new workspace:

Run Generate-ProjectTree.ps1 → produce fresh project-tree.txt.

Ensure the four architectural documents are current:

MASTER_OPERATING_PROTOCOL.md

Project Bible.md

Project Overview.md

Project Roadmap.md

(Optional) Generate or locate the current ZIP package.

Start a new ChatGPT conversation.

Upload all files.

Issue the command:
“Initialize workspace with the uploaded files.”

ChatGPT will reconstruct:

subsystem architecture

ingestion pipeline

computation & rendering boundaries

strategy layer mechanics

UI/state structure

naming conventions

long-term development phase alignment

The workspace is now fully rehydrated.

-------------------------------------------
4. Workspace Refresh Checklist
-------------------------------------------

Use this when continuing development after making changes:

Regenerate project-tree.txt.

Update:

MASTER_OPERATING_PROTOCOL.md (if rules changed)

Bible (if architecture changed)

Overview (if visible capabilities changed)

Roadmap (if phase evolved)

Upload the changed files.

Instruct ChatGPT:
“Refresh workspace with the updated files.”

Proceed only after confirmation.

-------------------------------------------
5. Document Authority Hierarchy
-------------------------------------------

When reconstructing the workspace, ChatGPT interprets documents strictly in the following order:

MASTER_OPERATING_PROTOCOL.md (operational truth)

Project Bible.md (architectural truth)

Project Overview.md (high-level system truth)

Project Roadmap.md (evolutionary truth)

project-tree.txt (structural truth)

Solution ZIP (implementation truth)

This hierarchy ensures deterministic, conflict-free behavior.

End of Workspace Workflow.md