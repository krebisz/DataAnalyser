param(
    [string]$RepoRoot = (Join-Path $PSScriptRoot ".."),
    [int]$MaxRetries = 20,
    [int]$RetryDelayMs = 750,
    [switch]$KillProcesses
)

$ErrorActionPreference = "Stop"

$dataVisualiserDir = Join-Path $RepoRoot "DataVisualiser"
if (-not (Test-Path $dataVisualiserDir)) {
    throw "DataVisualiser directory not found: $dataVisualiserDir"
}

Write-Host "Shutting down dotnet build servers..."
try { dotnet build-server shutdown | Out-Host } catch { }

if ($KillProcesses) {
    Write-Host "Stopping likely locking processes (DataVisualiser, dotnet, MSBuild)..."
    foreach ($name in @("DataVisualiser", "dotnet", "MSBuild")) {
        try {
            Get-Process -Name $name -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        } catch { }
    }
}

$targets = @(
    (Join-Path $dataVisualiserDir "bin"),
    (Join-Path $dataVisualiserDir "obj")
)

foreach ($path in $targets) {
    if (-not (Test-Path $path)) {
        Write-Host "Skipping (missing): $path"
        continue
    }

    Write-Host "Deleting: $path"

    $deleted = $false
    for ($i = 0; $i -lt $MaxRetries; $i++) {
        try {
            Remove-Item -LiteralPath $path -Recurse -Force -ErrorAction Stop
            $deleted = $true
            break
        }
        catch {
            if ($i -eq 0) {
                Write-Warning ("Delete failed, will retry (likely locked): {0}" -f $_.Exception.Message)
            }

            Start-Sleep -Milliseconds $RetryDelayMs
        }
    }

    if ($deleted) {
        Write-Host "Deleted OK."
    }
    else {
        throw "FAILED to delete after $MaxRetries retries: $path"
    }
}

Write-Host "Done."

