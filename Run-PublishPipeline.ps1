param(
    [switch]$NewWorkspace,
    [switch]$Refresh
)

$root = "C:\Development\POCs\DataFileReaderRedux"

Write-Host "=== DataFileReaderRedux Workspace Pipeline ===" -ForegroundColor Cyan
Write-Host ""

if (-not ($NewWorkspace -or $Refresh)) {
    Write-Host "You must specify either -NewWorkspace or -Refresh" -ForegroundColor Red
    exit 1
}

# 1. Create package
Write-Host "[1/3] Creating new project package..." -ForegroundColor Yellow
& "$root\Create-DataFileReaderRedux-Package.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Package creation failed. Aborting." -ForegroundColor Red
    exit 1
}

# Locate the newest package
$latestZip = Get-ChildItem -Path $root -Filter "DataFileReaderRedux_*.zip" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

Write-Host "Latest ZIP package found: $($latestZip.FullName)"

# 2. Decide workflow
if ($NewWorkspace) {
    Write-Host "[2/3] Opening folder for NEW workspace initialization..." -ForegroundColor Green
    explorer.exe $root
    Write-Host ""
    Write-Host ">>> Drag & drop '$($latestZip.Name)' into your NEW ChatGPT conversation."
    Write-Host ">>> This creates a clean workspace with full context."
}
elseif ($Refresh) {
    Write-Host "[2/3] Running refresh workflow..." -ForegroundColor Green
    & "$root\Refresh-WorkspaceWithLatestPackage.ps1"
    Write-Host ""
    Write-Host ">>> After this, upload '$($latestZip.Name)' to your EXISTING workspace in ChatGPT."
}

Write-Host ""
Write-Host "[3/3] Pipeline complete!" -ForegroundColor Cyan
