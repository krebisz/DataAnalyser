Creating a New Workspace (From Scratch)

This is the “cold start” process you use when beginning a brand-new ChatGPT thread devoted to the DataVisualiser project.

1. Create a clean project package

Run your unified script or the sequence:

Create-ProjectZip.ps1

Create-DataFileReaderRedux-Package.ps1

This generates a timestamped .zip containing only the code, excluding bin/obj, dgml, png, md, etc.

2. Start a fresh ChatGPT conversation

Ensure it is completely new — a clean thread.

3. Upload the project package

Drag-and-drop the newly created .zip into the chat.

4. Upload the “manual context files”

One by one, upload:

Project Bible

Project Overview

Directory tree snapshot

CodeMap DGML (optional but useful)

Any other supporting materials that differ from the last session

5. Issue your “workspace initialization command”

Copy/paste something like:

“Initialize workspace for DataVisualiser using the uploaded ZIP and documents.”

This tells me to:

unpack the zip internally

understand the structure

map the codebase

align with the bible, overview, tree, etc.

synchronize all future reasoning to this state

6. Begin work

Now the environment is ready for:

refactoring

architecture discussions

bug fixing

phase planning

code generation

anything else you need

📌 Updated Workflow: Refreshing an Existing Workspace

Use this when:

The ChatGPT thread is already dedicated to DataVisualiser

You’ve made code changes locally

You want me to realign with the latest state

1. Generate a new project package

Run the unified script again to produce the latest .zip.

2. Upload only what has changed

Upload:

The new .zip

Any updated documentation (Bible, Overview, DGML, etc.) if they changed

If a doc hasn't changed, do not send it again.

3. Issue your “workspace refresh command”

Copy/paste something like:

“Refresh the workspace with the new project package. Replace prior code context with this version.”

This tells me to:

wipe previous code

ingest the new zip

re-sync reasoning

keep all conceptual history unless you say otherwise

4. Continue working seamlessly

No need to re-explain anything — I resume from the updated project state.

📌 Summary of the Differences
Action	New Workspace	Refresh Workspace
New ChatGPT thread?	Yes	No
Upload project zip?	Yes	Yes
Upload bible/overview/tree?	Yes	Only if changed
Command to issue	Initialize workspace	Refresh workspace
AI resets understanding?	Full reset	Only code replaced
📌 Why This Works Reliably

Because:

You maintain strict control over what I ingest

The zipped code enables full-project awareness with zero ambiguity

Manual documents keep high-level reasoning clean without clutter

Two workflows (create vs. refresh) are simple, repeatable, and maintain context integrity

No over-engineering required — no repo access, no custom extensions, no brittle automation

This is the ideal compromise between power, simplicity, and your limited time availability.



























==========================================================================================================================================================================


DataFileReaderRedux – ChatGPT Workspace Management Guide

A streamlined, repeatable process for creating and refreshing ChatGPT workspaces for the DataVisualiser + DataFileReader projects.

Overview

This repository includes automation scripts designed to package your solution into a clean ZIP archive and feed it into a ChatGPT workspace for development assistance.

You follow two workflows depending on the situation:

Creating a New Workspace

Refreshing an Existing Workspace

Both workflows ensure ChatGPT receives:

The full source code (excluding build artifacts and unnecessary files)

A clean, normalized ZIP file

A manifest of included files

An optional SHA256 checksum

Any manually supplied updated documents (Project Bible, Project Overview, DGML, directory tree snapshot)

Scripts Provided
Script	Purpose
Create-ProjectZip.ps1	Cleans bin/obj folders, excludes unwanted files, generates timestamped ZIP + manifest.txt
Create-DataFileReaderRedux-Package.ps1	Wraps the ZIP and manifest into a workspace-ready package
Refresh-WorkspaceWithLatestPackage.ps1	Helps re-upload and update an existing ChatGPT workspace
(optional) Unified automation script	Executes packaging + refresh steps in one go
What’s Not Included in the ZIP

You chose to upload some files manually when they change. These files should NOT be included automatically:

Project Bible

Project Overview

DGML / Code Maps

Directory tree snapshots (project-tree.txt)

You will attach these manually only when they have changed, keeping ChatGPT’s context clean and intentional.

Workflow 1 — Creating a Completely New Workspace

Use this when:

Starting a brand-new ChatGPT thread

Context has drifted too far

You want to guarantee a clean slate

Steps

Ensure your project folder is up-to-date.

Run the ZIP creation script:

Create-ProjectZip.ps1


This will:

Clean bin/obj

Exclude unwanted file types

Generate a timestamped ZIP of your solution

Produce a manifest of included files

(Optional) Generate a checksum

Run the package script:

Create-DataFileReaderRedux-Package.ps1


Open a new ChatGPT conversation.

Drag-and-drop the newly generated ZIP package into the new workspace.

Manually upload the following if they changed:

Project Bible

Project Overview

DGML file(s)

Directory tree snapshot

Tell ChatGPT:

“This is a new workspace for the DataFileReaderRedux / DataVisualiser project. Please ingest the codebase and await instructions.”

Workflow 2 — Refreshing an Existing Workspace

Use this when:

Only the code changed, not the conceptual documents

You want ChatGPT to stay in the same conversation and keep momentum

Steps

Run the ZIP creation script again:

Create-ProjectZip.ps1


Run the package script:

Create-DataFileReaderRedux-Package.ps1


Use the refresh script to guide the re-upload process:

Refresh-WorkspaceWithLatestPackage.ps1


Drag-and-drop the new ZIP into the existing ChatGPT workspace.

If any conceptual files have changed, manually upload the most recent:

Project Bible

Project Overview

DGML

Directory tree snapshot

Tell ChatGPT:

“This is an updated project package. Please refresh your understanding of the codebase.”

How ChatGPT Uses These Files

ChatGPT cannot access remote Git repositories directly, so this ZIP workflow ensures:

The full codebase is always available

Massive historical context does not accumulate

The workspace remains coherent

You no longer need to paste individual files manually

Complex projects like DataVisualiser Phase 6 finally become manageable.

Best Practices

Use a new workspace only when necessary.
Staying in the same workspace preserves momentum.

Refresh frequently.
Especially after restructuring or major refactors.

Only upload conceptual documents when changed.
This keeps noise low.

Keep script outputs versioned.
It makes diffing and tracking easier.

Regenerate project-tree.txt after large structural changes.

Optional Future Enhancements

If time ever allows, the automation can be upgraded to:

A single unified script (Package-And-Refresh.ps1)

Automatic directory snapshot generation

Automatic DGML extraction

Automatic semantic diff reports

Local embedding index for offline search

Per-workspace version tracking

But for now, your request for “not an over-engineered solution” is respected.

End of README

If you'd like, I can also generate:

📌 A matching USAGE.md
📌 Comments added to each script
📌 A companion PowerShell module wrapping all scripts under one namespace

Just say the word.