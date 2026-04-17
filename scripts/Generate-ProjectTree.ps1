[CmdletBinding()]
param (
    [string]$Root,
    [string]$Output,
    [switch]$IncludeInfrastructure,
    [switch]$IncludeGenerated,
    [string[]]$ExcludeDirectories = @('.git', '.vs', 'bin', 'obj', 'packages', 'node_modules', 'TestResults'),
    [string[]]$ExcludeInfrastructureDirectories = @('.dotnet-cli', '.dotnet-sdk', 'artifacts', '.cursor'),
    [string[]]$ExcludeFilePatterns = @('*.user', '*.suo', '*.userosscache', '*.cache', '*.lock.json', '*.tmp', '*.log', 'msbuild_pp_*.xml')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-RepositoryRoot {
    param([string]$ExplicitRoot)

    if ($ExplicitRoot) {
        return (Resolve-Path -LiteralPath $ExplicitRoot).Path
    }

    $candidates = @()
    if ($PSScriptRoot) {
        $candidates += $PSScriptRoot
        $candidates += (Split-Path -Parent $PSScriptRoot)
    }
    $candidates += (Get-Location).Path

    foreach ($candidate in ($candidates | Where-Object { $_ } | Select-Object -Unique)) {
        $current = Resolve-Path -LiteralPath $candidate -ErrorAction SilentlyContinue
        if (-not $current) { continue }
        $dir = [System.IO.DirectoryInfo]$current.Path
        while ($dir) {
            $hasSolution = @(Get-ChildItem -LiteralPath $dir.FullName -Filter *.sln -File -ErrorAction SilentlyContinue).Count -gt 0
            $hasProjects = (Test-Path (Join-Path $dir.FullName 'DataVisualiser')) -or (Test-Path (Join-Path $dir.FullName 'DataFileReader'))
            if ($hasSolution -or $hasProjects) {
                return $dir.FullName
            }
            $dir = $dir.Parent
        }
    }

    throw 'Unable to resolve repository root. Pass -Root explicitly.'
}

function Resolve-OutputPath {
    param([string]$ResolvedRoot, [string]$ExplicitOutput)

    if ([string]::IsNullOrWhiteSpace($ExplicitOutput)) {
        return (Join-Path $ResolvedRoot 'project-tree.txt')
    }

    if ([System.IO.Path]::IsPathRooted($ExplicitOutput)) {
        return $ExplicitOutput
    }

    return (Join-Path $ResolvedRoot $ExplicitOutput)
}

function Should-SkipName {
    param(
        [string]$Name,
        [bool]$IsContainer,
        [string[]]$ExcludedDirectories,
        [string[]]$InfrastructureDirectories,
        [string[]]$ExcludedFilePatterns,
        [bool]$IncludeInfrastructure,
        [bool]$IncludeGenerated
    )

    if ($IsContainer) {
        if ($ExcludedDirectories -contains $Name) {
            return $true
        }

        if ((-not $IncludeInfrastructure) -and ($InfrastructureDirectories -contains $Name)) {
            return $true
        }

        return $false
    }

    foreach ($pattern in $ExcludedFilePatterns) {
        if ($Name -like $pattern) {
            return $true
        }
    }

    if (-not $IncludeGenerated) {
        if ($Name -eq 'codebase-index.md' -or
            $Name -eq 'dependency-summary.md' -or
            $Name -eq 'project-tree.txt') {
            return $true
        }
    }

    return $false
}

function Get-ChildItemsSafe {
    param([string]$Path)

    try {
        return @(Get-ChildItem -LiteralPath $Path -Force -ErrorAction Stop | Sort-Object @{ Expression = { if ($_.PSIsContainer) { 0 } else { 1 } } }, Name)
    }
    catch {
        Write-Warning ("Skipping inaccessible path: {0}" -f $Path)
        return @()
    }
}

function Add-TreeLines {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$CurrentPath,
        [string]$Indent,
        [string[]]$ExcludedDirectories,
        [string[]]$InfrastructureDirectories,
        [string[]]$ExcludedFilePatterns,
        [bool]$IncludeInfrastructure,
        [bool]$IncludeGenerated
    )

    $children = Get-ChildItemsSafe -Path $CurrentPath
    foreach ($child in $children) {
        if (Should-SkipName -Name $child.Name `
                            -IsContainer ([bool]$child.PSIsContainer) `
                            -ExcludedDirectories $ExcludedDirectories `
                            -InfrastructureDirectories $InfrastructureDirectories `
                            -ExcludedFilePatterns $ExcludedFilePatterns `
                            -IncludeInfrastructure $IncludeInfrastructure `
                            -IncludeGenerated $IncludeGenerated) {
            continue
        }

        if ($child.PSIsContainer) {
            $Lines.Add(("{0}{1}/" -f $Indent, $child.Name))

            $attributes = $child.Attributes
            $isReparsePoint = (($attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0)
            if (-not $isReparsePoint) {
                Add-TreeLines -Lines $Lines `
                              -CurrentPath $child.FullName `
                              -Indent ($Indent + '  ') `
                              -ExcludedDirectories $ExcludedDirectories `
                              -InfrastructureDirectories $InfrastructureDirectories `
                              -ExcludedFilePatterns $ExcludedFilePatterns `
                              -IncludeInfrastructure $IncludeInfrastructure `
                              -IncludeGenerated $IncludeGenerated
            }
        }
        else {
            $Lines.Add(("{0}{1}" -f $Indent, $child.Name))
        }
    }
}

$resolvedRoot = Resolve-RepositoryRoot -ExplicitRoot $Root
$outputPath = Resolve-OutputPath -ResolvedRoot $resolvedRoot -ExplicitOutput $Output
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add('Project Tree Snapshot')
$lines.Add(("Generated: {0}" -f $timestamp))
$lines.Add(("Root: {0}" -f $resolvedRoot))
$lines.Add('')
$lines.Add(((Split-Path -Leaf $resolvedRoot) + '/'))

Add-TreeLines -Lines $lines `
              -CurrentPath $resolvedRoot `
              -Indent '  ' `
              -ExcludedDirectories $ExcludeDirectories `
              -InfrastructureDirectories $ExcludeInfrastructureDirectories `
              -ExcludedFilePatterns $ExcludeFilePatterns `
              -IncludeInfrastructure ([bool]$IncludeInfrastructure) `
              -IncludeGenerated ([bool]$IncludeGenerated)

$parent = Split-Path -Parent $outputPath
if ($parent -and -not (Test-Path -LiteralPath $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$lines | Set-Content -LiteralPath $outputPath -Encoding UTF8
Write-Host ("Generated {0}" -f $outputPath)
