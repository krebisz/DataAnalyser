<#
.SYNOPSIS
Copies the latest project package zip into the ChatGPT workspace,
moves previous one to archives, and prepares the workspace.
#>

param(
    [string]$ProjectRoot = "C:\Development\POCs\DataFileReaderRedux",
    [string]$WorkspaceRoot = "$env:USERPROFILE\ChatGPT-Workspaces\DataVisualiser",
    [string]$BaseName = "DataFileReaderRedux"
)

Write-Host "Refreshing ChatGPT workspace..."

# Find latest package zip
$package = Get-ChildItem -Path $ProjectRoot -Filter "$BaseName*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $package) {
    Write-Host "No package zip found. Aborting."
    exit 1
}

$latestDir = Join-Path $WorkspaceRoot "latest"
$archiveDir = Join-Path $WorkspaceRoot "archives"

# Move old zips to archive
$existing = Get-ChildItem -Path $latestDir -Filter "$BaseName*.zip" -ErrorAction SilentlyContinue
foreach ($old in $existing) {
    Move-Item -Path $old.FullName -Destination $archiveDir -Force
    Write-Host "Archived old package: $($old.Name)"
}

# Copy new zip into latest
Copy-Item -Path $package.FullName -Destination $latestDir -Force

Write-Host ""
Write-Host "Workspace updated."
Write-Host "Latest package is now available at:"
Write-Host "  $latestDir"
Write-Host ""
Write-Host "Drag the zip in 'latest' into ChatGPT to refresh project context."
