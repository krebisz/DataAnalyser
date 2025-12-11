<#
.SYNOPSIS
    Generates a directory tree snapshot as project-tree.txt.

.DESCRIPTION
    Produces a clean folder tree for the solution, excluding:
      - bin
      - obj
    Output file is always named project-tree.txt and written
    to the solution root.

.PARAMETER RootPath
    Optional. Path to the root of the solution.
    Defaults to the folder where the script resides.

.EXAMPLE
    ./Generate-ProjectTree.ps1
    ./Generate-ProjectTree.ps1 -RootPath "C:\Development\POCs\DataFileReaderRedux"
#>

param(
    [string]$RootPath = $PSScriptRoot
)

# Resolve full path
$RootPath = (Resolve-Path $RootPath).Path

Write-Host "Generating project tree for: $RootPath"

# Output file
$outputFile = Join-Path $RootPath "project-tree.txt"

# Remove old file if exists
if (Test-Path $outputFile) {
    Remove-Item $outputFile -Force
}

# Build directory tree using Get-ChildItem + indentation
function Write-Tree {
    param(
        [string]$Path,
        [int]$Indent = 0
    )

    $prefix = ("|   " * $Indent)

    foreach ($item in Get-ChildItem $Path | Sort-Object Name) {

        # Skip bin/obj explicitly
        if ($item.Name -in @("bin", "obj")) {
            continue
        }

        # Write file or folder
        if ($item.PSIsContainer) {
            Add-Content -Path $outputFile -Value "$prefix+---$($item.Name)"
            Write-Tree -Path $item.FullName -Indent ($Indent + 1)
        }
        else {
            Add-Content -Path $outputFile -Value "$prefix|   $($item.Name)"
        }
    }
}

# Header for clarity
Add-Content -Path $outputFile -Value "Folder PATH listing for: $RootPath"
Add-Content -Path $outputFile -Value ""

# Start
Write-Tree -Path $RootPath -Indent 0

Write-Host "project-tree.txt created at: $outputFile"
