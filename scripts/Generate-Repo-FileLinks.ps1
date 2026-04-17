param(
  [string] $Ref = "HEAD",
  [string] $OutTxt = "links.txt",
  [string] $RemoteName = "origin",
  [string[]] $ExcludeDirectories = @(
    ".git", ".vs", "bin", "obj", "packages", "node_modules", "TestResults",
    ".cursor", ".dotnet-cli", ".dotnet-sdk", "artifacts"
  ),
  [string[]] $ExcludeFiles = @(
    "codebase-index.md", "dependency-summary.md", "project-tree.txt", "links.txt"
  ),
  [string[]] $ExcludeExtensions = @(".log", ".tmp", ".cache")
)

$ErrorActionPreference = "Stop"

function Get-GitOutput {
  param([Parameter(Mandatory=$true)][string[]] $Args)
  $result = & git @Args 2>$null
  if ($LASTEXITCODE -ne 0) { return $null }
  return $result
}

function Normalize-RepoPath {
  param([string] $PathValue)
  if ([string]::IsNullOrWhiteSpace($PathValue)) { return "" }
  return ($PathValue -replace "\\","/").TrimStart("./").Trim()
}

function Resolve-RemoteInfo {
  param([string] $Remote)

  $origin = Get-GitOutput -Args @("remote", "get-url", $Remote)
  if (-not $origin) { throw "No '$Remote' remote found." }

  $origin = $origin.Trim()
  if ($origin -match 'github\.com[:/](.+?)/([^/\.]+?)(?:\.git)?$') {
    return [pscustomobject]@{
      BaseRawUrl = "https://raw.githubusercontent.com/$($Matches[1])/$($Matches[2])"
    }
  }

  throw "Remote '$Remote' is not a GitHub remote."
}

function Get-RelativePathCompat {
  param(
    [Parameter(Mandatory=$true)][string]$BasePath,
    [Parameter(Mandatory=$true)][string]$TargetPath
  )

  try {
    return [System.IO.Path]::GetRelativePath($BasePath, $TargetPath)
  }
  catch {
    $baseResolved = [System.IO.Path]::GetFullPath($BasePath)
    $targetResolved = [System.IO.Path]::GetFullPath($TargetPath)

    if (-not $baseResolved.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
      $baseResolved += [System.IO.Path]::DirectorySeparatorChar
    }

    $baseUri = New-Object System.Uri($baseResolved)
    $targetUri = New-Object System.Uri($targetResolved)
    $relativeUri = $baseUri.MakeRelativeUri($targetUri)
    $relative = [System.Uri]::UnescapeDataString($relativeUri.ToString())
    return ($relative -replace '/', '\')
  }
}

function Should-ExcludeItem {
  param(
    [string] $RelativePath,
    [bool] $IsContainer,
    [string[]] $ExcludedDirs,
    [string[]] $ExcludedFiles,
    [string[]] $ExcludedExts
  )

  $normalized = Normalize-RepoPath $RelativePath
  $parts = $normalized -split '/'

  if ($IsContainer) {
    foreach ($part in $parts) {
      if ($ExcludedDirs -contains $part) { return $true }
    }
    return $false
  }

  foreach ($part in $parts) {
    if ($ExcludedDirs -contains $part) { return $true }
  }

  $name = [System.IO.Path]::GetFileName($normalized)
  if ($ExcludedFiles -contains $name) { return $true }

  $ext = [System.IO.Path]::GetExtension($name).ToLowerInvariant()
  if ($ExcludedExts -contains $ext) { return $true }

  return $false
}

function Get-RepoFiles {
  param(
    [string] $Root,
    [string[]] $ExcludedDirs,
    [string[]] $ExcludedFiles,
    [string[]] $ExcludedExts
  )

  $results = New-Object System.Collections.Generic.List[string]
  $queue = New-Object System.Collections.Queue
  $queue.Enqueue($Root)

  while ($queue.Count -gt 0) {
    $currentDir = [string]$queue.Dequeue()

    try {
      $children = Get-ChildItem -LiteralPath $currentDir -Force -ErrorAction Stop
    }
    catch {
      continue
    }

    foreach ($child in $children) {
      $relative = Get-RelativePathCompat -BasePath $Root -TargetPath $child.FullName
      $isContainer = [bool]$child.PSIsContainer

      if (Should-ExcludeItem -RelativePath $relative -IsContainer $isContainer -ExcludedDirs $ExcludedDirs -ExcludedFiles $ExcludedFiles -ExcludedExts $ExcludedExts) {
        continue
      }

      if ($isContainer) {
        $attributes = $child.Attributes
        $isReparsePoint = (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)
        if (-not $isReparsePoint) {
          $queue.Enqueue($child.FullName)
        }
      }
      else {
        $results.Add((Normalize-RepoPath $relative))
      }
    }
  }

  return @($results | Sort-Object -Unique)
}

$root = Get-GitOutput -Args @("rev-parse", "--show-toplevel")
if (-not $root) { throw "Run this script inside a cloned repository." }
$root = $root.Trim()

$remoteInfo = Resolve-RemoteInfo -Remote $RemoteName

$sha = Get-GitOutput -Args @("rev-parse", $Ref)
if (-not $sha) { throw "Failed to resolve ref '$Ref'." }
$sha = $sha.Trim()

$files = Get-RepoFiles -Root $root -ExcludedDirs $ExcludeDirectories -ExcludedFiles $ExcludeFiles -ExcludedExts $ExcludeExtensions
if (-not $files -or $files.Count -eq 0) { throw "No files matched." }

$urls = foreach ($f in $files) {
  "$($remoteInfo.BaseRawUrl)/$sha/$f"
}

$outPath = Join-Path $root $OutTxt
$urls | Set-Content -Encoding UTF8 -LiteralPath $outPath

Write-Host "Generated $outPath"
Write-Host "BaseRawUrl: $($remoteInfo.BaseRawUrl)"
Write-Host "Ref: $sha"
Write-Host "Files: $($urls.Count)"
