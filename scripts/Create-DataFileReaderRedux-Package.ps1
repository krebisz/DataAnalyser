<#
.SYNOPSIS
    Generates a clean, zipped project package for the DataFileReaderRedux solution.

.DESCRIPTION
    - Cleans all bin/obj directories
    - Excludes .dgml, .png, .md
    - Recursively processes all subdirectories
    - Creates manifest.txt inside the zip
    - Adds timestamp to filename
    - Overwrites existing zip
    - Optional SHA256 checksum generation
#>

# ---------------------------
# User Configuration
# ---------------------------
$SolutionPath = "C:\Development\POCs\DataFileReaderRedux"
$OutputPath   = "C:\Development\POCs\DataFileReaderRedux"
$ZipBaseName  = "DataFileReaderRedux"
$Timestamp    = Get-Date -Format "yyyyMMdd_HHmmss"
$ZipName      = "$ZipBaseName-$Timestamp.zip"
$ZipFullPath  = Join-Path $OutputPath $ZipName

$GenerateChecksum = $true   # set to $false if not needed

# Exclusion patterns
$ExcludeExtensions = @("*.dgml", "*.png", "*.md")
$ExcludeDirectories = @("bin", "obj", ".vs", ".idea", "packages", "logs")

# ---------------------------
# Validate
# ---------------------------
if (-not (Test-Path $SolutionPath)) {
    Write-Host "ERROR: Solution path not found: $SolutionPath" -ForegroundColor Red
    exit 1
}

Set-Location $SolutionPath

# ---------------------------
# Clean bin/obj folders
# ---------------------------
Write-Host "Cleaning bin/obj folders..."
Get-ChildItem -Directory -Recurse | Where-Object { $_.Name -in $ExcludeDirectories } |
    ForEach-Object {
        Write-Host "Removing $($_.FullName)"
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }

# ---------------------------
# Gather files to include
# ---------------------------
Write-Host "Collecting project files..."

$AllFiles = Get-ChildItem -File -Recurse

# Apply extension filters
$FilesToInclude = $AllFiles | Where-Object {
    $ext = "*$($_.Extension)"
    -not ($ExcludeExtensions -contains $ext)
}

# ---------------------------
# Build manifest.txt
# ---------------------------
$ManifestPath = Join-Path $SolutionPath "manifest.txt"

Write-Host "Generating manifest.txt..."
$FilesToInclude.FullName | Set-Content -Path $ManifestPath

# Add manifest.txt to list
$FilesToZip = $FilesToInclude + (Get-Item $ManifestPath)

# ---------------------------
# Create the ZIP archive
# ---------------------------
Write-Host "Creating ZIP package: $ZipFullPath"

if (Test-Path $ZipFullPath) {
    Write-Host "Overwriting existing zip..."
    Remove-Item $ZipFullPath
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open($ZipFullPath, 'Create')

foreach ($file in $FilesToZip) {
    $relativePath = Resolve-Path $file.FullName | ForEach-Object {
        $_.Path.Substring($SolutionPath.Length).TrimStart('\')
    }

    Write-Host " → Adding $relativePath"
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $relativePath)
}

$zip.Dispose()

# ---------------------------
# Compute checksum (optional)
# ---------------------------
if ($GenerateChecksum) {
    Write-Host "Generating SHA256 checksum..."
    $Checksum = (Get-FileHash -Path $ZipFullPath -Algorithm SHA256).Hash
    $ChecksumFile = "$ZipFullPath.sha256.txt"
    $Checksum | Out-File $ChecksumFile
    Write-Host "Checksum written to: $ChecksumFile"
}

# ---------------------------
# Completed
# ---------------------------
Write-Host ""
Write-Host "Packaging complete!"
Write-Host "Created: $ZipFullPath" -ForegroundColor Green
