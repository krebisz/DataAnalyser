📘 Workspace Workflow.md

(Updated — Automation Wired In, Strict Mode–Compliant, Drift-Hardened)

⚠️ This document is part of the frozen core documentation set.  
See MASTER_OPERATING_PROTOCOL.md for governing authority.

------------------------------------------------------
Workspace Initialization & Rehydration Protocol (Authoritative)
------------------------------------------------------

This document defines the exact artifacts and step-by-step process required to initialize,
refresh, or deliberately reset a ChatGPT workspace for the DataFileReaderRedux solution.

It exists to:
- prevent contextual drift
- enforce grounding discipline
- ensure deterministic rehydration
- protect execution velocity during long-running collaborations

------------------------------------------------------
🔒 SECTION 0 — SACRED WORKSPACE WORKFLOWS (IMMUTABLE)
------------------------------------------------------

⛔ Sacred Workflow A — Create a New Workspace (Authoritative Sequence)

Follow these steps exactly when starting a new ChatGPT workspace.

1. Run structural generators:

- Generate-ProjectTree.ps1 → produces project-tree.txt
- Generate-CodebaseIndex.ps1 → produces codebase-index.md
- Generate-DependencySummary.ps1 → produces dependency-summary.md

2. Verify the following documents are current:

- MASTER_OPERATING_PROTOCOL.md
- Project Bible.md
- Project Overview.md
- Project Roadmap.md
- SYSTEM_MAP.md (if present)

3. Prepare the Rehydration Bundle:

- project-tree.txt
- codebase-index.md
- dependency-summary.md
- MASTER_OPERATING_PROTOCOL.md
- Project Bible.md
- Project Overview.md
- Project Roadmap.md
- SYSTEM_MAP.md (if present)

(Optional) Latest solution ZIP or package

4. Start a new ChatGPT conversation.

(Optional but Recommended) Declare a workspace label:

Workspace: <Project> — <Context> — <Qualifier>

This label defines the identity and scope of the workspace for the duration of the conversation
and is treated as the canonical workspace reference.

5. Upload ALL files from the Rehydration Bundle.

6. Enter the initialization command:

Initialize workspace with the uploaded files.

7. Do NOT proceed with any development tasks until ChatGPT explicitly confirms:

Workspace successfully initialized.

------------------------------------------------------
⛔ Sacred Workflow B — Refresh an Existing Workspace (Authoritative Sequence)
------------------------------------------------------

Follow these steps exactly when updating an existing workspace.

1. Re-run required generators:

- Always run Generate-ProjectTree.ps1
- Run Generate-CodebaseIndex.ps1 if code structure changed
- Run Generate-DependencySummary.ps1 if dependencies may have changed

2. Identify which documents require updating:

- MASTER_OPERATING_PROTOCOL.md → only if collaboration rules change
- Project Bible.md → only if architecture changes
- Project Overview.md → only if system capabilities change
- Project Roadmap.md → only if phase or trajectory changes
- SYSTEM_MAP.md → only if structural intent changes

3. Prepare the Refresh Bundle:

- Updated generated artifacts
- Any updated documentation
- (Optional) Updated solution ZIP

4. Upload the Refresh Bundle to the existing ChatGPT workspace.

5. Enter the refresh command:

Refresh workspace with the updated files.

6. Wait for explicit confirmation:

Workspace successfully refreshed.

Proceed with development only after confirmation.

------------------------------------------------------
⚠️ Notes (Immutable)
------------------------------------------------------

- Generated artifacts are authoritative for structure, not intent.
- Generated files must never be edited by hand.
- These workflows MUST NOT be altered without explicit agreement.
- These workflows override all other procedural descriptions.

------------------------------------------------------
1. Required Rehydration Bundle
------------------------------------------------------

A. Mandatory Files

- project-tree.txt  
- codebase-index.md  
- dependency-summary.md  
- MASTER_OPERATING_PROTOCOL.md  
- Project Bible.md  
- Project Overview.md  
- Project Roadmap.md  
- SYSTEM_MAP.md (if present)

B. Optional but Recommended

- Solution ZIP / Package (reference only)

------------------------------------------------------
2. Workspace State Declaration (Additive · Mandatory)
------------------------------------------------------

> **Additive rule — derived from collaboration drift observations.**

After initialization or refresh, the assistant MUST explicitly declare the workspace state
before proposing or executing changes.

The declaration MUST include:

- Documents loaded (explicit list)
- Code snapshot status (ZIP present or not)
- Active project phase (e.g., Phase 4)
- Parity status (e.g., none / partial / active / closed)
- Any missing or unknown artifacts

Example:

Workspace State:
- Documents: MASTER_OPERATING_PROTOCOL, Project Bible, SYSTEM_MAP, Roadmap
- Code snapshot: present (dated YYYY-MM-DD)
- Phase: Phase 4 — Consumer Adoption
- Parity: SingleMetric complete, CombinedMetric in progress
- Unknowns: DifferenceStrategy CMS status

No execution may proceed until this declaration is acknowledged.

------------------------------------------------------
3. Grounding Gate (Additive · Enforcement Rule)
------------------------------------------------------

Before proposing **any code change**, the assistant MUST pass a grounding gate.

The gate requires:

- Explicit list of files inspected
- Explicit list of symbols verified
- Explicit list of unknown or missing artifacts

If a required file or symbol is missing:
- execution MUST stop
- the file must be requested explicitly

Conceptual, assumed, or “simplified” substitutions are prohibited in active phases.

------------------------------------------------------
4. Change Atomicity & Drop-In Safety (Additive)
------------------------------------------------------

All implementation guidance MUST be one of:

- Full method replacement
- Full class replacement
- Explicit before/after diff block

Prohibited:

- Partial snippets without surrounding context
- “Modify this part” instructions
- Multi-location changes without enumeration

------------------------------------------------------
5. Phase Transition Guardrail (Additive)
------------------------------------------------------

Creating a new workspace is treated as a **controlled phase sub-step**, not an escape hatch.

Before resetting a workspace, the following must be true:

- Documentation alignment complete
- Parity obligations explicitly closed or deferred
- Reset justification stated

------------------------------------------------------
6. Active Phase Artifacts (Reference-Only · Phase 4)
------------------------------------------------------

> **Additive section. No existing rules are modified.**

The following files are actively relevant during Phase 4 (CMS adoption & parity):

DataVisualiser — Strategies:
- SingleMetricLegacyStrategy.cs
- SingleMetricCmsStrategy.cs
- CombinedMetricStrategy.cs
- CombinedMetricCmsStrategy.cs
- MultiMetricStrategy.cs

DataVisualiser — Parity:
- IStrategyParityHarness.cs
- CombinedMetricParityHarness.cs

DataVisualiser — Context & Wiring:
- ChartDataContext.cs
- ChartDataContextBuilder.cs
- MetricSelectionService.cs
- MainWindow.xaml.cs
- MainWindowViewModel.cs

DataFileReader — Canonical:
- ICanonicalMetricSeries.cs
- CanonicalMetricSeries.cs

These files define the **CMS ↔ legacy coexistence boundary**.

------------------------------------------------------
7. Document Authority Hierarchy
------------------------------------------------------

When reconstructing a workspace, documents are interpreted strictly in this order:

1. MASTER_OPERATING_PROTOCOL.md
2. Project Bible.md
3. SYSTEM_MAP.md
4. Project Overview.md
5. Project Roadmap.md
6. codebase-index.md
7. dependency-summary.md
8. project-tree.txt
9. Solution ZIP (reference only)

------------------------------------------------------
End of Workspace Workflow.md
------------------------------------------------------
