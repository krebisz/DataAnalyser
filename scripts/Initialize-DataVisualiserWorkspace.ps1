<#
.SYNOPSIS
Creates a clean ChatGPT workspace directory for the DataVisualiser project.
This workspace is what you will upload to ChatGPT whenever context resets.
#>

param(
    [string]$WorkspaceRoot = "$env:USERPROFILE\ChatGPT-Workspaces",
    [string]$WorkspaceName = "DataVisualiser"
)

# Resolve the workspace path
$workspacePath = Join-Path $WorkspaceRoot $WorkspaceName

Write-Host "Initializing workspace at: $workspacePath"

# Create workspace if needed
if (-not (Test-Path $workspacePath)) {
    New-Item -ItemType Directory -Path $workspacePath | Out-Null
    Write-Host "Workspace created."
} else {
    Write-Host "Workspace already exists."
}

# Create structure for future use
$dirs = @(
    "latest",
    "archives",
    "notes",
    "snapshots"
)

foreach ($d in $dirs) {
    $full = Join-Path $workspacePath $d
    if (-not (Test-Path $full)) {
        New-Item -ItemType Directory -Path $full | Out-Null
        Write-Host "Created directory: $d"
    }
}

Write-Host "Workspace initialization complete."
Write-Host ""
Write-Host "Upload workflow:"
Write-Host "  1. Place the latest exported zip in workspace\latest"
Write-Host "  2. ChatGPT will treat that as the authoritative context"
Write-Host "  3. Older zips move into workspace\archives"
