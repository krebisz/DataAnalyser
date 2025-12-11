<# ------------------------------------------------------------------------
   Create-ProjectZip.ps1
   Prepares a clean zipped archive of the DataFileReaderRedux solution.
   Requirements based on user answers.
------------------------------------------------------------------------ #>

param (
    [string]$SolutionRoot = "C:\Development\POCs\DataFileReaderRedux",
    [string]$SolutionName = "DataFileReaderRedux",
    [switch]$GenerateChecksum
)

# -----------------------------
# 1. Validate root folder
# -----------------------------
if (-not (Test-Path $SolutionRoot)) {
    Write-Error "Solution root path does not exist: $SolutionRoot"
    exit 1
}

# -----------------------------
# 2. Compute timestamp
# -----------------------------
$timestamp = (Get-Date -Format "yyyyMMdd_HHmmss")
$zipName = "${SolutionName}_${timestamp}.zip"
$zipPath = Join-Path $SolutionRoot $zipName
$manifestPath = Join-Path $SolutionRoot "manifest_${timestamp}.txt"

Write-Host "Output ZIP: $zipPath"
Write-Host "Manifest: $manifestPath"


# -----------------------------
# 3. Clean bin/obj folders
# -----------------------------
Write-Host "Cleaning bin/obj folders..."
Get-ChildItem -Path $SolutionRoot -Recurse -Directory `
    | Where-Object { $_.Name -in @("bin", "obj") } `
    | ForEach-Object {
        Write-Host "Deleting: $($_.FullName)"
        Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }


# -----------------------------
# 4. Build inclusion list
# -----------------------------
Write-Host "Collecting files..."

$files = Get-ChildItem -Path $SolutionRoot -Recurse -File `
    | Where-Object {
        # Exclusions
        $_.FullName -notmatch "\\bin\\" -and
        $_.FullName -notmatch "\\obj\\" -and
        $_.FullName -notmatch "\\.vs\\" -and
        $_.FullName -notmatch "\\.cursor\\" -and
        $_.FullName -notmatch "\\.vscode\\" -and
        $_.Extension -notin @(".dgml", ".png", ".md")
    }

Write-Host "Files to include: $($files.Count)"


# -----------------------------
# 5. Create manifest.txt
# -----------------------------
Write-Host "Writing manifest file..."
$files | Select-Object FullName | Out-File -FilePath $manifestPath -Encoding UTF8


# -----------------------------
# 6. Create ZIP archive
# -----------------------------

if (Test-Path $zipPath) {
    Write-Host "Removing existing zip: $zipPath"
    Remove-Item $zipPath -Force
}

Write-Host "Creating zip archive..."

Add-Type -AssemblyName System.IO.Compression.FileSystem

$zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Create')

foreach ($file in $files) {
    $relative = $file.FullName.Substring($SolutionRoot.Length).TrimStart('\')
    Write-Host "Adding: $relative"
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $relative)
}

# Add manifest itself into the zip
[System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $manifestPath, "manifest.txt")

$zip.Dispose()

Write-Host "`nZIP created: $zipPath"


# -----------------------------
# 7. Optional Checksum
# -----------------------------
if ($GenerateChecksum) {
    $checksumFile = "$zipPath.sha256"
    Write-Host "Generating SHA256 checksum..."
    Get-FileHash -Algorithm SHA256 -Path $zipPath | Out-File $checksumFile
    Write-Host "Checksum saved to: $checksumFile"
}

Write-Host "`nDone."
