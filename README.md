DataFileReaderRedux Workspace Packaging & Refresh Workflow

This document defines the repeatable, reliable workflow for preparing and uploading the project to ChatGPT as a clean project workspace.

🎯 Goal

Provide ChatGPT with a clean, complete, zipped snapshot of the latest project source — without bin/obj folders, without unwanted file types, and without manually curating anything.

This ensures:

ChatGPT works with the latest codebase

No loss of context or reliance on stale history

Straightforward refresh when the project evolves

🚀 Workflow Overview

There are two workflows:

Workflow A — Starting a New ChatGPT Workspace

Use this when:

You start a new conversation thread, OR

The previous workspace drifted out of sync

Steps

Open PowerShell in the solution directory

cd C:\Development\POCs\DataFileReaderRedux


Generate a clean project ZIP package

.\Create-DataFileReaderRedux-Package.ps1


Drag & drop the generated ZIP file into the new ChatGPT chat

ChatGPT will extract the structure

It will create the new Workspace context

You can now proceed with engineering tasks

Workflow B — Updating an Existing Workspace

Use this when:

ChatGPT is already working on the project

You have made code changes locally

You want to “sync” your changes with the existing workspace

Steps

Generate a fresh ZIP

.\Create-DataFileReaderRedux-Package.ps1


Use the refresh script

.\Refresh-WorkspaceWithLatestPackage.ps1


Upload the ZIP into ChatGPT

ChatGPT updates its workspace state

No need to re-establish context

You can continue seamlessly

📦 Script Summary
1. Create-ProjectZip.ps1

Recursively scans the project

Removes bin, obj, and excluded file types

Generates:

Clean ZIP

Manifest file

Optional checksum

2. Create-DataFileReaderRedux-Package.ps1

Wrapper around Create-ProjectZip.ps1

Enforces naming conventions

Applies timestamp: yyyyMMdd_HHmmss

Saves the ZIP in the solution root

3. Refresh-WorkspaceWithLatestPackage.ps1

Opens the solution folder

Guides the user to upload the ZIP to ChatGPT

In future versions can automate ChatGPT upload via desktop tools

🧠 Why Two Workflows?

ChatGPT “Workspaces” behave like lightweight virtual project environments.
They persist per conversation thread.

Therefore:

When starting fresh → initialize a new workspace
When continuing in the same environment → refresh the existing workspace

This keeps context clean, avoids stale information, and prevents drift.

📁 Output Example

After generating, you will see files like:

DataFileReaderRedux_20250214_162233.zip
DataFileReaderRedux_20250214_162233_manifest.txt
DataFileReaderRedux_20250214_162233.sha256

✔ Best Practices

Always refresh before asking ChatGPT to work on code

Always start a new workspace for major architectural changes

Never paste partial files manually unless necessary

Prefer uploading full ZIP snapshots for consistency

🧩 FAQ
Q: Do I need to run “Initialize Workspace” every time?

No.
Only when creating a new conversation thread.

Q: Why does “Refresh Workspace” exist separately?

It ensures ChatGPT does not lose context or switch into “new project mode” inadvertently.

Q: Can ChatGPT parse XAML correctly?

Yes — as uploaded files in a workspace.
The parsing failures occur only when XAML is pasted inline in chat messages.