# File: scripts/Generate-RawLinks.ps1
<#
Default: double-click friendly.
- No parameters needed.
- Outputs links.md in repo root (commit-pinned raw URLs for every tracked file).
- Pauses automatically when launched from Explorer (so the console window doesn’t vanish).
#>

[CmdletBinding()]
param(
  [string]$RepoUrl,              # Optional; auto from 'origin' if omitted
  [string]$Ref = "HEAD",         # Optional; default HEAD
  [string]$OutMarkdown = "links.md",
  [string]$IncludeExtensions     # Optional; e.g. ".md,.cs,.csproj"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Fail([string]$msg) { Write-Error $msg; throw $msg }

function Get-ParentProcessName {
  try {
    $pidObj = Get-CimInstance Win32_Process -Filter "ProcessId=$PID"
    if (-not $pidObj) { return $null }
    $ppid = $pidObj.ParentProcessId
    $parent = Get-CimInstance Win32_Process -Filter "ProcessId=$ppid"
    return ($parent.Name)
  } catch { return $null }
}

$launchedByExplorer = ($null -ne (Get-ParentProcessName) -and (Get-ParentProcessName).ToLower() -eq "explorer.exe")

try {
  # Ensure git + repo
  $gitVersion = (& git --version) 2>$null
  if (-not $gitVersion) { Fail "Git is required. Install Git and run inside a cloned repository." }

  $repoRoot = (& git rev-parse --show-toplevel) 2>$null
  if (-not $repoRoot) { Fail "Run this script inside a cloned repository (git rev-parse failed)." }
  Set-Location $repoRoot

  # Resolve RepoUrl from origin when omitted
  if (-not $RepoUrl) {
    $origin = (& git remote get-url origin).Trim()
    if (-not $origin) { Fail "No RepoUrl and no 'origin' remote found. Provide -RepoUrl." }
    if ($origin -match "github\.com[:/](?<owner>[^/]+)/(?<repo>[^/\.]+)(?:\.git)?$") {
      $RepoUrl = "https://github.com/$($matches.owner)/$($matches.repo)"
    } else {
      Fail "Origin remote is not a GitHub URL. Provide -RepoUrl (e.g., https://github.com/owner/repo)."
    }
  }

  # Extract owner/repo
  if ($RepoUrl -notmatch "github\.com/([^/]+)/([^/]+)") { Fail "Invalid -RepoUrl. Expect https://github.com/<owner>/<repo>" }
  $Owner = $matches[1]
  $Repo  = ($matches[2] -replace '\.git$', '')

  # Resolve exact commit SHA
  $Sha = (& git rev-parse $Ref).Trim()
  if (-not $Sha) { Fail "Failed to resolve ref '$Ref' to a commit SHA." }

  # Enumerate tracked files
  $files = (& git ls-files) -split "`n" | Where-Object { $_ -ne "" }

  # Optional extension filter
  $extSet = @()
  if ($IncludeExtensions) {
    $extSet = $IncludeExtensions.Split(",") | ForEach-Object { $_.Trim().ToLower() } | Where-Object { $_ }
    if ($extSet.Count -gt 0) {
      $files = $files | Where-Object {
        [string]$e = [System.IO.Path]::GetExtension($_).ToLower()
        $extSet -contains $e
      }
    }
  }

  if (-not $files -or $files.Count -eq 0) {
    Fail ("No files found." + ($(if ($extSet.Count -gt 0) { " Check -IncludeExtensions." } else { "" })))
  }

  # Emit Markdown
  $outPath = Join-Path -Path $repoRoot -ChildPath $OutMarkdown
  "# Raw links for $Owner/$Repo @ $Sha`n" | Out-File -Encoding utf8 $outPath
  "| Path | Raw URL |`n|---|---|`n" | Out-File -Encoding utf8 -Append $outPath

  foreach ($f in $files) {
    $raw = "https://raw.githubusercontent.com/$Owner/$Repo/$Sha/$f"
    "| $f | $raw |" | Out-File -Encoding utf8 -Append $outPath
  }

  Write-Host "Wrote: $outPath"
  Write-Host "Files: $($files.Count) | Commit: $Sha | Repo: $Owner/$Repo"

} catch {
  Write-Host ""
  Write-Host "ERROR: $($_.Exception.Message)"
  if ($launchedByExplorer) {
    Write-Host ""
    Read-Host "Press ENTER to close"
  }
  exit 1
}

if ($launchedByExplorer) {
  Write-Host ""
  Read-Host "Done. Press ENTER to close"
}
