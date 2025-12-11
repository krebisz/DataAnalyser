<#
.SYNOPSIS
Finds the latest DataFileReaderRedux package zip and opens its folder
so it can be uploaded into ChatGPT with one drag/drop.
#>

param(
    [string]$ProjectRoot = "C:\Development\POCs\DataFileReaderRedux",
    [string]$BaseName = "DataFileReaderRedux"
)

Write-Host "Searching for latest $BaseName package zip..."

$files = Get-ChildItem -Path $ProjectRoot -Filter "$BaseName*.zip" |
    Sort-Object LastWriteTime -Descending

if (-not $files -or $files.Count -eq 0) {
    Write-Host "No package files found."
    exit 1
}

$latest = $files[0]

Write-Host "Latest package found:"
Write-Host "  $($latest.FullName)"

# Copy full path to clipboard for convenience
$latest.FullName | Set-Clipboard
Write-Host ""
Write-Host "Full path copied to clipboard."

# Open File Explorer at location
Start-Process "explorer.exe" -ArgumentList "/select,`"$($latest.FullName)`""

Write-Host ""
Write-Host "File Explorer opened to the zip. Drag it directly into ChatGPT."
