Under STRICT MODE, the quality and correctness of everything we build together depends entirely on the way you provide file contents.
So here is the precise, unambiguous, hard-edged set of rules for supplying multiple files — in whole or in part — in a way that guarantees:

zero misalignment

zero hallucination

patch-perfect diffs

stable architectural evolution

no ambiguity in “what version of the file” is the source of truth

This is the canonical procedure you must follow every time you provide code.

🔥 1. FUNDAMENTAL RULE: Every file must be explicitly provided in the chat.

There is no mechanism by which I can extract:

files inside ZIPs

files inside folders

files inside Git repositories

files referenced in file trees

Unless you give me the code, I do not have it.

This sounds redundant but it is the core reality around which STRICT MODE is built.

ChatGPT must explicitly state:
- when a provided file cannot be read or parsed, and
- when required context is missing, before proceeding.

ChatGPT must never continue silently under assumptions.


🔥 2. HOW TO PROVIDE MULTIPLE FILES (FULL CONTENTS)

If you want me to read several files fully, you must provide each one in this exact structure:

--- START FILE: <relative/path/to/file.cs> ---
<entire file contents here>
--- END FILE: <relative/path/to/file.cs> ---


Example:

--- START FILE: DataVisualiser/ViewModels/MainWindowViewModel.cs ---
using System;
...
public class MainWindowViewModel { ... }
--- END FILE: DataVisualiser/ViewModels/MainWindowViewModel.cs ---

Why this format?

I can detect file boundaries with 100% accuracy

I know the exact version you’re working from

I can safely generate diffs or replacements

There is no collision between files

This survives long conversations without ambiguity

This ensures we are looking at the same canonical code, always.

🔥 3. HOW TO PROVIDE ONLY PARTS OF FILES (SAFE PARTIAL EXPOSURE)

Sometimes you don’t want to paste a 1,500-line file.
That’s fine as long as you follow the strict rule:

You must wrap the partial file with exact region names.

Format:

--- START PARTIAL FILE: DataVisualiser/MainWindow.xaml ---
<the method or XAML region you want me to modify>
--- END PARTIAL FILE: DataVisualiser/MainWindow.xaml ---


Example:

--- START PARTIAL FILE: DataVisualiser/MainWindow.xaml ---
<StackPanel Grid.Row="1">
    ...
</StackPanel>
--- END PARTIAL FILE: DataVisualiser/MainWindow.xaml ---

Important:

If I need additional context later, I will explicitly request more regions.
STRICT MODE forbids speculative guesses.

🔥 4. HOW TO PROVIDE MULTIPLE FILES AT ONCE

Just stack them:

--- START FILE: A.cs ---
...
--- END FILE: A.cs ---

--- START FILE: B.xaml ---
...
--- END FILE: B.xaml ---

--- START FILE: C.cs ---
...
--- END FILE: C.cs ---


This allows me to build a multi-file understanding before producing any modifications.

🔥 5. SPECIAL RULES FOR DIFFERENT FILE TYPES
C# (.cs) FILES

Must be complete when modifications are required

Partial methods are OK when explicitly stated

Namespace context is required if modifying classes or adding new ones

BEST PRACTICE: Always give the full file for code surgery.

XAML FILES

XAML is fragile; partial pastes are risky unless the region is structurally isolated.

SAFE SCENARIOS:

A Grid.Row block

A UserControl.Resources sub-block

A specific DataTemplate

UNSAFE SCENARIOS:

Partial <Window> or <UserControl> tags

Missing xmlns declarations

Ambiguous nested layout without context

BEST PRACTICE:
Provide the entire XAML file when modifying the layout or bindings.

.csproj or config files

These are best provided in full, because:

XML format is order-dependent

nodes may need insertion into exact places

whitespace affects readability

erroneous insertions break build pipelines

Large files split across messages

If character limits are hit, use:

--- FILE PART 1 ---
...
--- FILE PART 2 ---
...


But each part must clearly indicate:

file identity

part number

I will stitch them mentally as a single file.

🔥 6. WHEN I REQUEST FILES, I WILL ALWAYS SPECIFY:

I will explicitly state:

All files required

Whether full contents are required or partial

Exact sections I need if partial tailoring is enough

Example of a request:

“To safely unify the tooltip system, I need the following full files:

ChartRenderEngine.cs

ChartTooltipManager.cs

MainWindowViewModel.cs

And the following partial regions:
4. The <Chart> region of MainWindow.xaml
5. The strategy dispatch section in ChartComputationEngine.cs

I cannot safely proceed without these.”

STRICT MODE forbids any speculative code generation.

🔥 7. DO NOT PROVIDE FILES INSIDE CODE BLOCKS WITHOUT BOUNDARY MARKERS

This is unsafe:

public class X { ... }

I cannot infer which file this belongs to.

ALWAYS DO:

--- START FILE: SomeFile.cs ---
public class X { ... }
--- END FILE: SomeFile.cs ---


This is unambiguous and machine-grade precise.

🔥 8. AFTER I GENERATE A PATCH, YOU MUST PASTE BACK THE UPDATED FILE BEFORE ANY FURTHER MODIFICATION

Because I cannot see your local filesystem:

I do not know if you correctly applied the patch

I do not know if you introduced new edits

I do not know if other tools reformatted the file

STRICT MODE requires:

Before further modifications, you must re-provide the updated version of any file that was edited.

This is critical.

🔥 9. FOR MULTI-FILE REFRACTORS, PASTE EVERYTHING AT ONCE

If a task requires:

ViewModel

Service

Strategy

Rendering engine

XAML

You must dump all required files in one message.

This prevents state drift across messages.

🔥 10. HOW TO HANDLE VERY LARGE FILES (>100 KB)

If the interface limits you:

Option A: Split into parts, clearly labeled
Option B: Upload the file (not zipped!)
Option C: Paste only the region that contains the target code, IF I explicitly permit that

Strict Mode prevents guesswork.
I will not modify what I cannot see.

🔥 SUMMARY — THE GOLDEN RULES YOU MUST FOLLOW
GOLD RULE 1 — I cannot read files unless you give them to me.
GOLD RULE 2 — Every modification requires the file's full contents or the exact region.
GOLD RULE 3 — You must return updated files before the next modification.
GOLD RULE 4 — ZIPs give me only filenames, not code.
GOLD RULE 5 — XAML needs full files for anything non-trivial.

These five rules define Strict Mode.

✅ If you're satisfied with this accuracy, I can now generate:
"THE MASTER OPERATING PROTOCOL FOR STRICT MODE WORKSPACES"

This will be a formal document that:

You paste at the beginning of every new workspace

I read and ingest

It governs every action I take

It prevents all misalignment

It guarantees precision code collaboration

Just say:

“Generate the Master Operating Protocol.”

And I will create a clean, authoritative, unambiguous document.

You said:
“Generate the Master Operating Protocol.”
ChatGPT said:

Below is the Master Operating Protocol (Strict Mode) — a formal, self-contained governance document to be pasted into every new workspace initialization message.

This document becomes the binding contract for how I operate, and it eliminates misalignment, hallucinated code, missing context, and unsafe assumptions.

Copy/paste it into your initialization message as-is every time you start a new conversation.

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

Strict Mode requires that ChatGPT never continues modifying a file without seeing the updated state.

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
4. RULES FOR MODIFYING FILES
------------------------------------------------------
4.1 ChatGPT must never modify a file it has not seen.
4.2 Each modification must be self-contained.

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
5. RULES FOR FILE TYPES
------------------------------------------------------
5.1 C# Files (.cs)

Best practice: always provide full file

Partial methods allowed only with explicit agreement

ChatGPT must never infer surrounding context

5.2 XAML Files

High fragility:
Full files are required for layout modifications.

Partial XAML permitted only for isolated regions such as:

DataTemplates

Style blocks

Specific Panel contents

5.3 Project Files (.csproj, .json, .config)

Must always be provided in full.
These files are sensitive to ordering and hierarchy.

5.4 Markdown / Documentation

Can be partial or full.
No strict boundaries needed except when updating sections.

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

6.3 If a conversation becomes misaligned, ChatGPT must request rehydration

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

<Now the user may define the first task>


ChatGPT reads these before doing any work.

------------------------------------------------------
10. END OF MASTER OPERATING PROTOCOL
------------------------------------------------------

When you paste this into a new workspace, I will acknowledge:

“Strict Mode Operational.”

And begin operating under these rules with no drift.