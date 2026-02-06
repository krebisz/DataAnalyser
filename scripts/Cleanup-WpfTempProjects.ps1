param(
    [string]$ProjectDir = (Join-Path $PSScriptRoot "..\\DataVisualiser"),
    [int]$MaxRetriesPerFile = 20,
    [int]$RetryDelayMs = 750
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $ProjectDir)) {
    throw "Project directory not found: $ProjectDir"
}

Write-Host "Shutting down dotnet build servers..."
try { dotnet build-server shutdown | Out-Host } catch { }

$wpftmpPattern = "*_wpftmp.csproj"
$testPattern = "csproj_*test*.csproj"

$wpftmpFiles = Get-ChildItem -Path $ProjectDir -File -Filter $wpftmpPattern -ErrorAction SilentlyContinue

# These should never ship. They were created during local diagnostics to reproduce the locking behavior.
$extraTestProjects = Get-ChildItem -Path $ProjectDir -File -Filter $testPattern -ErrorAction SilentlyContinue

$files = @()
if ($wpftmpFiles) { $files += $wpftmpFiles }
if ($extraTestProjects) { $files += $extraTestProjects }

if (-not $files -or $files.Count -eq 0) {
    Write-Host "No '$wpftmpPattern' or '$testPattern' files found under $ProjectDir"
    exit 0
}

Write-Host ("Found {0} temp/extra project file(s) to delete..." -f $files.Count)

foreach ($file in $files) {
    $deleted = $false
    for ($i = 0; $i -lt $MaxRetriesPerFile; $i++) {
        try {
            Remove-Item -Path $file.FullName -Force -ErrorAction Stop
            $deleted = $true
            break
        }
        catch {
            Start-Sleep -Milliseconds $RetryDelayMs
        }
    }

    if ($deleted) {
        Write-Host ("Deleted: {0}" -f $file.Name)
    }
    else {
        Write-Warning ("FAILED to delete (still locked?): {0}" -f $file.FullName)
    }
}
