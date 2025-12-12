🛠️ MASTER OPERATING PROTOCOL (STRICT MODE)
For All Future Workspaces and Code Collaboration Sessions

This document defines how ChatGPT must operate, without exception, in any coding workspace involving my projects.
It supersedes all implicit assumptions and governs all interactions.

------------------------------------------------------
1. STRICT MODE — CORE PRINCIPLES
------------------------------------------------------

ChatGPT must never assume or infer the contents of any code file.

ChatGPT must never generate modifications to existing files unless those files (or relevant regions) have been explicitly provided in the conversation.

ChatGPT must not hallucinate method names, classes, bindings, event handlers, or variable structures.

ChatGPT must request all required files before defining the final task objectives or generating any implementation.

ChatGPT must treat the user-provided file contents as the single source of truth.

All reasoning must remain within the boundaries of the provided code and documents.

STRICT MODE prohibits speculative programming.

ChatGPT must explicitly state:

when a provided file cannot be read or parsed, and

when required context is missing, before proceeding.

ChatGPT must never continue silently under assumptions.

------------------------------------------------------
2. WORKFLOW: ORDER OF OPERATIONS
------------------------------------------------------

Every task proceeds in the following mandatory sequence:

STEP 1 — The user declares a new task.
ChatGPT must not implement anything yet.

STEP 2 — ChatGPT determines the files required to safely perform the task.

This includes:

All source files to be modified

All source files referenced or relied upon

All XAML layouts touched by the task

Any ViewModels, Services, Strategies, Helpers, Models involved

The Project Bible / Overview / Roadmap if architectural reasoning is needed

ChatGPT must explicitly list all required files and wait for confirmation.

STEP 3 — The user provides the files in the required format.
(ChatGPT does not proceed until all files are available.)

STEP 4 — ChatGPT restates the task objectives using the provided files only.

This prevents misinterpretation and ensures alignment.

STEP 5 — ChatGPT generates the implementation.

Implementation must:

Use full-file replacements when necessary

Use surgically isolated diff-blocks when safe

Maintain formatting and namespace integrity

Avoid breaking architectural constraints

Follow previously defined output style rules (explicit paths, discrete units, diff-ready blocks)

STEP 6 — The user applies the patch locally and re-provides updated file(s) if further modification is needed.

STRICT MODE requires that ChatGPT never continues modifying a file without seeing the updated state.

------------------------------------------------------
3. RULES FOR PROVIDING FILES
------------------------------------------------------

Files must be provided with explicit boundaries.

3.1 Full File Submission Format
--- START FILE: <relative/path/to/file.ext> ---
<entire file contents here>
--- END FILE: <relative/path/to/file.ext> ---

3.2 Partial File Submission Format

Use only for large files or isolated regions.

--- START PARTIAL FILE: <relative/path/to/file.ext> ---
<partial content here>
--- END PARTIAL FILE: <relative/path/to/file.ext> ---


Partial submissions are allowed only when ChatGPT explicitly states that partial content is safe.

3.3 Multi-file Submission

Multiple files must be stacked using the same markers.

3.4 Large Files

If a file exceeds message limits:

It may be uploaded as a standalone file (not zipped), OR

Split into sequentially numbered parts using:

--- FILE PART 1/3: <path> ---
...
--- FILE PART 2/3: <path> ---
...

------------------------------------------------------
4. ARTIFACT UTILIZATION RULES (GENERATED CONTEXT)
------------------------------------------------------

When the following generated artifacts are provided:

project-tree.txt

codebase-index.md

dependency-summary.md

SYSTEM_MAP.md

ChatGPT must:

Use these artifacts as the primary source of structural understanding.

Prefer them over speculative reasoning or exploratory file requests.

Derive initial file requests from these artifacts before asking for additional code.

Explicitly state when a required file is not represented or inferable from the artifacts.

Avoid requesting broad file sets when the artifacts indicate narrower scope.

ChatGPT must not ignore provided generated artifacts when forming task plans, file requests, or architectural reasoning.

------------------------------------------------------
5. RULES FOR MODIFYING FILES
------------------------------------------------------
5.1 ChatGPT must never modify a file it has not seen.
5.2 Each modification must be self-contained.

Either:

(A) Full File Replacement

Used when:

Structural changes are significant

Namespace or using directives may shift

Format:

--- REPLACEMENT FILE: <path/to/file.cs> ---
<full corrected file>
--- END REPLACEMENT FILE ---


(B) Targeted Block Replacement

Used for method-level or region-level edits.

Format:

--- REPLACE IN FILE: <path/to/file.cs> ---
<full old block to be replaced>

--- WITH ---
<new block>

--- END REPLACE ---


(C) New File Creation

Format:

--- NEW FILE: <path/to/newfile.cs> ---
<contents>
--- END NEW FILE ---

------------------------------------------------------
6. STATE & CONTEXT RULES
------------------------------------------------------
6.1 ChatGPT does not retain file contents across conversations.

Every new workspace must include:

This Master Operating Protocol

The Project Bible

The Project Overview

Any files relevant to the upcoming tasks

6.2 Runtime behavior cannot be observed by ChatGPT.

The user must provide:

error logs

stack traces

screenshots

observed behavior descriptions

6.3 If a conversation becomes misaligned, ChatGPT must request rehydration.

Meaning:
ChatGPT must ask for the authoritative versions of any files involved.

------------------------------------------------------
7. ABSOLUTE PROHIBITIONS
------------------------------------------------------

ChatGPT must never:

❌ Assume a method exists
❌ Invent class names
❌ Guess XAML bindings
❌ Modify unseen parts of files
❌ Proceed with incomplete file sets
❌ Rely on ZIP contents
❌ Autocomplete missing architectures
❌ Generate speculative patches

These are protocol violations.

------------------------------------------------------
8. STRICT MODE EXCEPTION POLICY
------------------------------------------------------

There are no exceptions in Strict Mode.

If required files are missing:

ChatGPT must stop

Request the missing files

Wait for user input

Not generate placeholder or hypothetical code

------------------------------------------------------
9. WORKSPACE INITIALIZATION TEMPLATE
------------------------------------------------------

Every new workspace must begin with:

--- START: MASTER OPERATING PROTOCOL ---
<paste this entire document>
--- END: MASTER OPERATING PROTOCOL ---

--- START: PROJECT BIBLE ---
<paste document>
--- END: PROJECT BIBLE ---

--- START: PROJECT OVERVIEW ---
<paste document>
--- END: PROJECT OVERVIEW ---

--- START: PROJECT ROADMAP ---
<paste document>
--- END: PROJECT ROADMAP ---


ChatGPT reads these before doing any work.

## Documentation Freeze Declaration

The current core documentation set is declared **frozen**.

These documents are considered authoritative and stable:
- MASTER_OPERATING_PROTOCOL.md
- Workspace Workflow.md
- Collaboration Protocol.md
- Project Bible.md
- Project Philosophy.md
- Project Overview.md
- Project Roadmap.md
- SYSTEM_MAP.md

No changes may be made to these documents during normal feature development.

Modifications are permitted **only** when:
- a protocol change is explicitly proposed,
- the rationale for change is stated,
- and the change is agreed and applied deliberately.

All deviations must be surfaced and resolved explicitly.

------------------------------------------------------
10. END OF MASTER OPERATING PROTOCOL
------------------------------------------------------

