# File: Generate-Repo-FileLinks.ps1
param(
  [string] $Ref = "HEAD",
  [string] $OutTxt = "links.txt",
  [string] $IncludeExtensions
)

$ErrorActionPreference = "Stop"

# Ensure Git and repo root
$git = (& git --version) 2>$null
if (-not $git) { Write-Error "Git is required. Run inside a cloned repository."; exit 1 }

$root = (& git rev-parse --show-toplevel) 2>$null
if (-not $root) { Write-Error "Run this script inside a cloned repository."; exit 1 }
Set-Location $root

# Resolve origin → owner/repo (HTTPS or SSH)
$origin = (& git remote get-url origin) 2>$null
if (-not $origin) { Write-Error "No 'origin' remote found."; exit 1 }
$origin = $origin.Trim()
if ($origin -match 'github\.com[:/](.+?)/([^/\.]+)(?:\.git)?$') {
  $Owner = $Matches[1]
  $Repo  = $Matches[2]
} else {
  Write-Error ("Origin remote is not a GitHub URL: " + $origin); exit 1
}

# Resolve exact commit SHA
$Sha = (& git rev-parse $Ref) 2>$null
if (-not $Sha) { Write-Error ("Failed to resolve ref '" + $Ref + "'."); exit 1 }
$Sha = $Sha.Trim()

# Enumerate tracked files
$files = (& git ls-files) -split "`n" | Where-Object { $_ } | Sort-Object

# Optional extension filter
if ($IncludeExtensions) {
  $exts = $IncludeExtensions.Split(",") | ForEach-Object { $_.Trim().ToLower() } | Where-Object { $_ }
  if ($exts.Count -gt 0) {
    $files = $files | Where-Object { $exts -contains ([System.IO.Path]::GetExtension($_).ToLower()) }
  }
}
if (-not $files -or $files.Count -eq 0) { Write-Error "No files matched. (Check -IncludeExtensions if used.)"; exit 1 }

# Build raw URLs
$urls = foreach ($f in $files) {
  "https://raw.githubusercontent.com/$Owner/$Repo/$Sha/$f"
}

# Write links.txt
$urls | Set-Content -Encoding UTF8 $OutTxt
